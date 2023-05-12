using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Xml.Serialization;
using static RiverHollow.Game_Managers.SaveManager;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;
using System;

namespace RiverHollow.Misc
{
    public class RHTask
    {
        public int ID { get; }
        public string StartTaskDialogue => "Task_" + ID + "_Start";
        public string EndTaskDialogue => "Task_" + ID + "_End";

        public string Name {get; private set;}
        public string Description { get; private set; }

        private TaskTypeEnum _eTaskType;
        public TaskStateEnum TaskState { get; private set; } = TaskStateEnum.Waiting;

        public TalkingActor StartNPC { get; private set; }
        public TalkingActor GoalNPC { get; private set; }

        private Dictionary<TaskTriggerEnum, string> _diAssignationTriggers;
        private List<KeyValuePair<int, bool>> _liTasksToTrigger;

        private readonly int _iCutsceneID;
        public int NeededCount { get; private set; }
        public int TargetsAccomplished { get; private set; }

        private readonly int _iBuildingEndID = -1;
        private Mob _questMonster;
        private Item _targetItem;
        private int _iTargetObjectID = -1;
        private readonly int _iTargetWorldObjNum = -1;
        public bool ReadyForHandIn { get; private set; } = false;
        

        bool _bFinishOnCompletion;
        int _iActivateID;
        bool _bHiddenGoal;
        #region Rewards
        private int _iUnlockObjectID = -1;
        private int _iRewardMoney;
        private int _iFriendPoints;
        public string FriendTarget { get; }

        public List<Item> LiRewardItems { get; }
        #endregion
        #region Spawn Mobs
        Mob _spawnMob;
        string _sSpawnMap;
        string _sLocName;
        #endregion

        #region Reqs
        int _iDay => DataManager.GetIntByIDKey(ID, "Day", DataType.Task);
        int _iSeason => DataManager.GetIntByIDKey(ID, "Season", DataType.Task);
        #endregion

        private RHTask()
        {
            _iCutsceneID = -1;
            _bFinishOnCompletion = false;
            _iActivateID = -1;
            ID = -1;
            FriendTarget = string.Empty;
            StartNPC = null;
            GoalNPC = null;
            _targetItem = null;
            _questMonster = null;
            NeededCount = -1;
            TargetsAccomplished = -1;
            ReadyForHandIn = false;

            LiRewardItems = new List<Item>();

            _liTasksToTrigger = new List<KeyValuePair<int, bool>>();
            _diAssignationTriggers = new Dictionary<TaskTriggerEnum, string>();
        }

        public RHTask(string name, TaskTypeEnum type, string desc, int target, Mob m, Item i, Villager giver = null) : this()
        {
            _eTaskType = type;
            GoalNPC = giver;
            NeededCount = target;
            _questMonster = m;
            _targetItem = i;
            TargetsAccomplished = 0;
            ReadyForHandIn = false;
        }

        public RHTask(int id, Dictionary<string, string> stringData) : this()
        {
            ID = id;

            
            Name = DataManager.GetTextData(ID, "Name", DataType.Task);
            Description = DataManager.GetTextData(ID, "Task", DataType.Task);

            TargetsAccomplished = 0;
            LiRewardItems = new List<Item>();

            _eTaskType = Util.ParseEnum<TaskTypeEnum>(stringData["Type"]);

            if (stringData.ContainsKey("StartNPC"))
            {
                StartNPC = TownManager.DIVillagers[int.Parse(stringData["StartNPC"])];
            }

            if (stringData.ContainsKey("GoalNPC"))
            {
                GoalNPC = TownManager.DIVillagers[int.Parse(stringData["GoalNPC"])];
            }
            else
            {
                GoalNPC = StartNPC;
            }

            if (stringData.ContainsKey("AssignTrigger"))
            {
                string[] triggers = Util.FindParams(stringData["AssignTrigger"]);
                for (int i = 0; i < triggers.Length; i++)
                {
                    string[] args = Util.FindArguments(triggers[i]);
                    _diAssignationTriggers[Util.ParseEnum<TaskTriggerEnum>(args[0])] = args.Length == 1 ? string.Empty : args[1];
                }
            }

            if (stringData.ContainsKey("TriggerTask"))
            {
                string[] tasks = Util.FindParams(stringData["TriggerTask"]);
                for (int i = 0; i < tasks.Length; i++)
                {
                    string[] split = Util.FindArguments(tasks[i]);
                    if (split.Length == 1)
                    {
                        _liTasksToTrigger.Add(new KeyValuePair<int, bool>(int.Parse(split[0]), false));
                    }
                    else
                    {
                        _liTasksToTrigger.Add(new KeyValuePair<int, bool>(int.Parse(split[0]), true));
                    }
                }
            }

            if (stringData.ContainsKey("GoalItem"))
            {
                _targetItem = DataManager.GetItem(int.Parse(stringData["GoalItem"]));
            }

            if (stringData.ContainsKey("Count"))
            {
                NeededCount = int.Parse(stringData["Count"]);
            }

            if (stringData.ContainsKey("ItemRewardID"))
            {
                string[] items = Util.FindParams(stringData["ItemRewardID"]);
                if (items.Length > 0)
                {
                    foreach (string itemInfo in items)
                    {
                        string[] parse = Util.FindParams(itemInfo);
                        Item it = DataManager.GetItem(int.Parse(parse[0]), parse.Length > 1 ? int.Parse(parse[1]) : 1);
                        if (parse.Length == 3) { it.ApplyUniqueData(parse[2]); }
                        LiRewardItems.Add(it);
                    }
                }
            }

            if (stringData.ContainsKey("Friendship"))
            {
                string[] parse = Util.FindArguments(stringData["Friendship"]);
                if (parse.Length > 1)
                {
                    FriendTarget = parse[0];
                    _iFriendPoints = int.Parse(parse[1]);
                }
            }

            if (stringData.ContainsKey("SpawnMob"))
            {
                string[] parse = Util.FindArguments(stringData["SpawnMob"]);
                if (parse.Length > 1)
                {
                    _spawnMob = DataManager.CreateMob(int.Parse(parse[0]));
                    _sSpawnMap = parse[1];
                    _sLocName = parse[2];
                }
            }

            _iBuildingEndID = Util.AssignValue("EndBuildingID", stringData);

            _iRewardMoney = Util.AssignValue("Money", stringData, 0);
            _iUnlockObjectID = Util.AssignValue("UnlockBuildingID", stringData);

            if (stringData.ContainsKey("TargetObjectID"))
            {
                string[] split = Util.FindParams(stringData["TargetObjectID"]);
                _iTargetObjectID = int.Parse(split[0]);
                _iTargetWorldObjNum = split.Length == 2 ? int.Parse(split[1]) : 1;
            }

            Util.AssignValue(ref _bFinishOnCompletion, "Immediate", stringData);
            _iActivateID = Util.AssignValue("Activate", stringData);
            _iCutsceneID = Util.AssignValue("CutsceneID", stringData);
            Util.AssignValue(ref _bHiddenGoal, "HideGoal", stringData);
        }

        public bool ReadyForAssignation()
        {
            if (_diAssignationTriggers.Count == 0) { return false; }

            int checksum = 0;
            foreach (TaskTriggerEnum trigger in Enum.GetValues(typeof(TaskTriggerEnum)))
            {
                if (!_diAssignationTriggers.ContainsKey(trigger)) { continue; }

                switch (trigger)
                {
                    case TaskTriggerEnum.Building:
                        break;
                    case TaskTriggerEnum.FriendLevel:
                        break;
                    case TaskTriggerEnum.GameStart:
                        checksum++;
                        break;
                    case TaskTriggerEnum.Task:
                        if (TaskManager.TaskCompleted(int.Parse(_diAssignationTriggers[trigger])))
                        {
                            checksum++;
                        }
                        break;
                }
            }

            return checksum == _diAssignationTriggers.Count;
        }

        public void AssignTaskToNPC()
        {
            if ((TaskState == TaskStateEnum.Waiting || TaskState == TaskStateEnum.Assigned) && ReadyForAssignation())
            {
                TaskState = TaskStateEnum.Assigned;
                StartNPC.AssignTask(this);
            }
        }

        /// <summary>
        /// Adds a Task to the Task Log.
        /// 
        /// First we guard against adding any Task that has been Finished. It should never
        /// happen, but just to be sure.
        /// 
        /// Upon adding a Task to the Task Log, we should see if the Task is complete/nearly complete.
        /// 
        /// Some Tasks may involve the defeat of a Task specific monster. If such a monster exists, spawn it now.
        /// </summary>
        public void AddTaskToLog()
        {
            if (TaskState == TaskStateEnum.Talking)
            {
                TaskState = TaskStateEnum.TaskLog;
                switch (_eTaskType)
                {
                    case TaskTypeEnum.Fetch:
                        foreach (Item i in InventoryManager.PlayerInventory) { if (i != null) { AttemptProgress(i); } }
                        break;
                    case TaskTypeEnum.Build:
                        foreach (int k in TownManager.GetTownObjects().Keys) { AttemptProgressBuild(k); }
                        break;
                    case TaskTypeEnum.Population:
                        AttemptProgress();
                        break;
                }
                
                SpawnTaskMobs();

                GUIManager.NewAlertIcon("New Task");
            }
        }

        public bool TaskProgressEnterBuilding(int i)
        {
            bool rv = false;

            if (_iBuildingEndID == i)
            {
                TurnInTask();
            }

            return rv;
        }
        public bool AttemptProgress(Villager a)
        {
            bool rv = false;

            if(_eTaskType == TaskTypeEnum.Talk && GoalNPC == a) {
                rv = true;
                ReadyForHandIn = true;
            }

            return rv;
        }
        public bool AttemptProgress(Mob m)
        {
            bool rv = false;

            if (_questMonster != null && _questMonster.ID == m.ID)
            {
                rv = true;
                IncrementProgress(1);
            }

            return rv;
        }
        public bool AttemptProgress(Item i)
        {
            bool rv = false;

            if (_targetItem != null && _targetItem.ID == i.ID)
            {
                rv = true;
                IncrementProgress(i.Number);
            }

            return rv;
        }
        public bool AttemptProgressBuild(int i)
        {
            bool rv = false;

            if (_eTaskType == TaskTypeEnum.Build && i == _iTargetObjectID)
            {
                rv = true;
                SetReadyForHandIn(TownManager.GetNumberTownObjects(_iTargetObjectID) == _iTargetWorldObjNum);
            }

            return rv;
        }

        public bool AttemptProgress()
        {
            bool rv = false;

            if (_eTaskType == TaskTypeEnum.Population)
            {
                rv = true;
                SetReadyForHandIn(TownManager.GetPopulation() >= NeededCount);
            }

            return rv;
        }

        public void IncrementProgress(int num)
        {
            if (TargetsAccomplished < NeededCount)
            {
                TargetsAccomplished += num;
                if (TargetsAccomplished >= NeededCount)
                {
                    TargetsAccomplished = NeededCount;
                    SetReadyForHandIn(true);
                }
            }
        }
        public bool RemoveProgress(Item i)
        {
            bool rv = false;
            if (i != null && _targetItem != null && _targetItem.ID == ((Item)i).ID)
            {
                if (TargetsAccomplished > 0)
                {
                    TargetsAccomplished--;
                    rv = true;
                }

                if(TargetsAccomplished < NeededCount)
                {
                    SetReadyForHandIn(false);
                }
            }
            return rv;
        }

        public void SpawnTaskMobs()
        {
            if (_spawnMob != null)
            {
                RHMap map = MapManager.Maps[_sSpawnMap];
                map.AddMobByPosition(_spawnMob, map.DictionaryCharacterLayer[_sLocName]);
            }
        }

        public void TaskIsTalking()
        {
            if (TaskState == TaskStateEnum.Assigned)
            {
                TaskState = TaskStateEnum.Talking;
            }
        }

        private void SetReadyForHandIn(bool value)
        {
            ReadyForHandIn = value;

            if (GoalNPC != null && _iBuildingEndID == -1)
            {
                GoalNPC.ModifyTaskGoalValue(value ? 1 : -1);
            }
            if (value && _bFinishOnCompletion)
            {
                TurnInTask();
            }
        }

        public void TurnInTask()
        {
            TaskState = TaskStateEnum.Completed;

            if (_targetItem != null && _targetItem.ID > -1)
            {
                InventoryManager.RemoveItemsFromInventory(_targetItem.ID, NeededCount);
            }

            //If a cutscene will play defer the actual End of the Task until the Cutscene ends
            if (_iCutsceneID != -1)
            {
                CutsceneManager.TriggerCutscene(_iCutsceneID, this);
            }
            else
            {
                EndTask();
            }
        }

        public void EndTask()
        {
            if (TaskState == TaskStateEnum.Completed)
            {
                for (int i = 0; i < _liTasksToTrigger.Count; i++)
                {
                    if (!_liTasksToTrigger[i].Value){ TaskManager.AssignTaskByID(_liTasksToTrigger[i].Key); }
                    else { TaskManager.AddDelayedTask(_liTasksToTrigger[i].Key); }
                }

                foreach (Item i in LiRewardItems)
                {
                    InventoryManager.AddToInventory(i);
                }

                PlayerManager.AddMoney(_iRewardMoney);

                if (FriendTarget.Equals("Giver"))
                {
                    GoalNPC.FriendshipPoints += _iFriendPoints;
                }

                if (_iUnlockObjectID != -1)
                {
                    PlayerManager.AddToCraftingDictionary(_iUnlockObjectID);
                }

                if (_iActivateID > -1)
                {
                    TownManager.DIVillagers[_iActivateID].Activate(true);
                }

                if (DataManager.TaskData[ID].ContainsKey("SendToTown"))
                {
                    ((Villager)GoalNPC).QueueSendToTown();
                }

                if (DataManager.TaskData[ID].ContainsKey("UnlockMagic"))
                {
                    PlayerManager.MagicUnlocked = true;
                    GameManager.GoToHUDScreen();
                }

                TaskManager.TaskLog.Remove(this);
                GUIManager.NewAlertIcon("Task Complete");
            }
        }

        public string GetProgressString()
        {
            string rv = string.Empty;

            if (_bHiddenGoal) { rv = "???"; }
            else if (ReadyForHandIn) {
                if (GoalNPC != null)
                {
                    rv = "Speak to " + GoalNPC.Name();
                    if (_iBuildingEndID != -1)
                    {
                        string name = DataManager.GetTextData(_iBuildingEndID, "Name", DataType.WorldObject);
                        rv += " at the " + name;
                    }
                }
            }
            else
            {
                switch (_eTaskType)
                {
                    case TaskTypeEnum.Population:
                        rv = " Population: " + TownManager.GetPopulation() + "/" + NeededCount;
                        break;
                    case TaskTypeEnum.Fetch:
                        rv = _targetItem.Name() + " Found: " + TargetsAccomplished + "/" + NeededCount;
                        break;
                    case TaskTypeEnum.GroupSlay:
                        rv = _questMonster.Name() + " Defeated: " + TargetsAccomplished + "/" + NeededCount;
                        break;
                    case TaskTypeEnum.Slay:
                        rv = _questMonster.Name() + " Defeated: " + TargetsAccomplished + "/" + NeededCount;
                        break;
                    case TaskTypeEnum.Build:
                        string objName = DataManager.GetTextData(_iTargetObjectID, "Name", DataType.WorldObject);

                        if (_iTargetWorldObjNum > 1) { rv = "Build " + _iTargetWorldObjNum.ToString() + objName + "s"; }
                        else { rv = "Build " + objName; }
                        break;
                    case TaskTypeEnum.Talk:
                        rv = "Speak to " + GoalNPC.Name();
                        break;
                }
            }

            return rv;
        }

        public struct TaskData
        {
            [XmlElement(ElementName = "TaskType")]
            public int questType;

            [XmlElement(ElementName = "TaskState")]
            public int taskState;

            [XmlElement(ElementName = "RewardText")]
            public string rewardText;

            [XmlElement(ElementName = "GoalNPC")]
            public int goalNPC;

            [XmlElement(ElementName = "TaskItem")]
            public int itemID;

            [XmlElement(ElementName = "TaskMonster")]
            public int monsterID;

            [XmlElement(ElementName = "TaskBuilding")]
            public int targetWorldObjectID;

            [XmlElement(ElementName = "TargetGoal")]
            public int targetGoal;

            [XmlElement(ElementName = "Accomplished")]
            public int accomplished;

            [XmlElement(ElementName = "ReadyForHandIn")]
            public bool readyForHandIn;

            [XmlElement(ElementName = "HiddenGoal")]
            public bool hiddenGoal;

            [XmlElement(ElementName = "RewardMoney")]
            public int rewardMoney;

            [XmlArray(ElementName = "RewardItems")]
            public List<ItemData> Items;

            [XmlElement(ElementName = "RewardUnlock")]
            public int unlockBuildingID;
        }

        public TaskData SaveData()
        {
            TaskData qData = new TaskData
            {
                questType = (int)_eTaskType,
                goalNPC = GoalNPC != null ? GoalNPC.ID : -1,
                itemID = _targetItem != null ? _targetItem.ID : -1,
                monsterID = _questMonster != null ? _questMonster.ID : -1,
                targetWorldObjectID = _iTargetObjectID,
                unlockBuildingID = _iUnlockObjectID,
                targetGoal = NeededCount,
                hiddenGoal = _bHiddenGoal,
                accomplished = TargetsAccomplished,
                readyForHandIn = ReadyForHandIn,
                taskState = (int)TaskState,
                Items = new List<ItemData>()
            };
            foreach (Item i in LiRewardItems)
            {
                qData.Items.Add(Item.SaveData(i));
            }

            return qData;
        }
        public void LoadData(TaskData qData)
        {
            if(ID == -1)
            {
                _eTaskType = (TaskTypeEnum)qData.questType;
                GoalNPC = qData.goalNPC != -1 ? TownManager.DIVillagers[qData.goalNPC] : null;
                _targetItem = qData.itemID != -1 ? DataManager.GetItem(qData.itemID) : null;
                _questMonster = qData.monsterID != -1 ? DataManager.CreateMob(qData.monsterID) : null;
                _iTargetObjectID = qData.targetWorldObjectID;
                _iUnlockObjectID = qData.unlockBuildingID;
                NeededCount = qData.targetGoal;
                _bHiddenGoal = qData.hiddenGoal;
                
                foreach (ItemData i in qData.Items)
                {
                    Item newItem = DataManager.GetItem(i.itemID, i.num);

                    if (newItem != null) { newItem.ApplyUniqueData(i.strData); }
                    LiRewardItems.Add(newItem);
                }
            }

            TargetsAccomplished = qData.accomplished;
            ReadyForHandIn = qData.readyForHandIn;
            TaskState = (TaskStateEnum)qData.taskState;

            if (ReadyForHandIn && TaskState != TaskStateEnum.Completed)
            {
                SetReadyForHandIn(true);
            }
        }
    }
}
