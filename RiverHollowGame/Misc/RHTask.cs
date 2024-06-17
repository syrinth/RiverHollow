using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Xml.Serialization;
using RiverHollow.Items;
using System;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class RHTask
    {
        public int ID { get; }
        public string StartTaskDialogue => "Task_" + ID + "_Start";
        public string EndTaskDialogue => "Task_" + ID + "_End";
        public string Name => DataManager.GetTextData(ID, "Name", DataType.Task);
        public string Description => DataManager.GetTextData(ID, "Description", DataType.Task);

        private readonly Dictionary<string, string> _diLookupInfo;

        private TaskTypeEnum _eTaskType;
        public TaskStateEnum TaskState { get; private set; } = TaskStateEnum.Waiting;

        private readonly Dictionary<TaskTriggerEnum, string> _diAssignationTriggers;
        private readonly List<KeyValuePair<int, bool>> _liTasksToTrigger;

        #region Lookups
        bool FinishOnCompletion => GetBoolByIDKey("Immediate");
        bool HiddenGoal => GetBoolByIDKey("HideGoal");
        int CutsceneID => GetIntByIDKey("CutsceneID");
        int EndBuildingID => GetIntByIDKey("EndBuildingID");

        #region Rewards
        public int RewardMoney => GetIntByIDKey("Money", 0);
        int UnlockObjectID => GetIntByIDKey("UnlockBuildingID");
        #endregion
        #endregion

        public int TargetsAccomplished { get; private set; }
        public bool ReadyForHandIn { get; private set; } = false;
        public bool HasEndBuilding => EndBuildingID != -1;

        private RHTask()
        {
            ID = -1;
            TargetsAccomplished = -1;
            ReadyForHandIn = false;

            _liTasksToTrigger = new List<KeyValuePair<int, bool>>();
            _diAssignationTriggers = new Dictionary<TaskTriggerEnum, string>();
        }

        public RHTask(string name, TaskTypeEnum type, string desc, int target, Mob m, Item i, Villager giver = null) : this()
        {
            _diLookupInfo = new Dictionary<string, string>();
            _eTaskType = type;
            TargetsAccomplished = 0;
            ReadyForHandIn = false;
        }

        public RHTask(int id, Dictionary<string, string> stringData) : this()
        {
            ID = id;

            TargetsAccomplished = 0;

            _eTaskType = Util.ParseEnum<TaskTypeEnum>(stringData["Type"]);

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
                    case TaskTriggerEnum.Letter:
                        if (int.TryParse(_diAssignationTriggers[trigger], out int letterID) && TownManager.MailboxMessageRead(letterID))
                        {
                            checksum++;
                        }
                        break;
                    case TaskTriggerEnum.Task:
                        if (int.TryParse(_diAssignationTriggers[trigger], out int taskID) && TaskManager.TaskCompleted(taskID))
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
                StartNPC().AssignTask(this);
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
        public void AddTaskToLog(bool force)
        {
            if (force || TaskState == TaskStateEnum.Talking)
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
                    case TaskTypeEnum.TownState:
                        AttemptProgress();
                        break;
                }

                SpawnTaskMobs();

                GUIManager.NewInfoAlertIcon(Constants.STR_ALERT_TASK);
            }
        }

        public bool TaskProgressEnterBuilding(int i)
        {
            bool rv = false;

            if (EndBuildingID == i)
            {
                TurnInTask();
            }

            return rv;
        }
        public bool AttemptProgress(Villager a)
        {
            bool rv = false;

            if (_eTaskType == TaskTypeEnum.Talk && GoalNPC() == a)
            {
                rv = true;
                ReadyForHandIn = true;
            }

            return rv;
        }
        public bool AttemptProgress(Mob m)
        {
            bool rv = false;

            if (_eTaskType == TaskTypeEnum.TownState && GetBoolByIDKey("MonstersKilled"))
            {
                var count = GetIntByIDKey("MonstersKilled");
                SetReadyForHandIn(TownManager.TotalDefeatedMobs >= count);
            }
            else if (_eTaskType == TaskTypeEnum.Slay)
            {
                var mob = GoalMob();
                if (mob.Item1 == m.ID)
                {
                    rv = true;
                }
            }

            return rv;
        }
        public bool AttemptProgress(Item i)
        {
            bool rv = false;

            var targetItem = GoalItem();
            if (targetItem != null && targetItem.ID == i.ID)
            {
                rv = true;
                CheckItems();
            }

            return rv;
        }
        public bool AttemptProgressBuild(int i)
        {
            bool rv = false;

            var targetObject = GoalObject();
            if (_eTaskType == TaskTypeEnum.Build && i == targetObject.Item1)
            {
                rv = true;
                SetReadyForHandIn(TownManager.GetNumberTownObjects(targetObject.Item1) == targetObject.Item2);
            }

            return rv;
        }

        public bool AttemptProgress()
        {
            bool rv = false;

            if (_eTaskType == TaskTypeEnum.TownState)
            {
                rv = true;
                if (GetBoolByIDKey("Population"))
                {
                    var count = GetIntByIDKey("Population");
                    SetReadyForHandIn(TownManager.GetPopulation() >= count);
                }

                if (GetBoolByIDKey("TownScore"))
                {
                    var count = GetIntByIDKey("TownScore");
                    SetReadyForHandIn(TownManager.GetTownScore() >= count);
                }
            }

            return rv;
        }

        public void CheckItems()
        {
            if (_eTaskType == TaskTypeEnum.Fetch)
            {
                var targetItem = GoalItem();
                if (targetItem != null)
                {
                    var currentNumber = InventoryManager.GetNumberInPlayerInventory(targetItem.ID);
                    if (currentNumber >= targetItem.Number)
                    {
                        TargetsAccomplished = targetItem.Number;
                        SetReadyForHandIn(true);
                    }
                    else
                    {
                        TargetsAccomplished = currentNumber;
                        SetReadyForHandIn(false);
                    }
                }
            }
        }

        public void AttemptProgressCraft(Item i)
        {
            if (_eTaskType == TaskTypeEnum.Craft)
            {
                var goal = GoalItem();
                if(goal != null && i.ID == goal.ID)
                {
                    if(TargetsAccomplished + i.Number > goal.Number)
                    {
                        TargetsAccomplished = goal.Number;
                    }
                    else
                    {
                        TargetsAccomplished += i.Number;
                    }

                    if(TargetsAccomplished == goal.Number)
                    {
                        SetReadyForHandIn(true);
                    }
                }
            }
        }

        public void SpawnTaskMobs()
        {
            if (GetBoolByIDKey("SpawnMob"))
            {
                string[] parse = Util.FindArguments(GetStringByIDKey("SpawnMob"));
                if (parse.Length > 1 && int.TryParse(parse[0], out int mobID))
                {
                    var mob = DataManager.CreateActor<Mob>(mobID);
                    RHMap map = MapManager.Maps[parse[1]];
                    map.AddMobByPosition(mob, map.GetCharacterObject(parse[2]).Location);
                }
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
            if (ReadyForHandIn != value)
            {
                ReadyForHandIn = value;

                var goalNPC = GoalNPC();
                if (goalNPC != null && EndBuildingID == -1)
                {
                    goalNPC.ModifyTaskGoalValue(value ? 1 : -1);
                }
                if (value && FinishOnCompletion)
                {
                    TurnInTask();
                }
            }
        }

        public void TurnInTask()
        {
            TaskState = TaskStateEnum.Completed;

            if (_eTaskType == TaskTypeEnum.Fetch && GoalItem() is Item item)
            {
                InventoryManager.RemoveItemsFromInventory(item.ID, item.Number);
            }

            //If a cutscene will play defer the actual End of the Task until the Cutscene ends
            if (CutsceneID != -1)
            {
                CutsceneManager.TriggerCutscene(CutsceneID, this);
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
                    if (!_liTasksToTrigger[i].Value) { TaskManager.AssignTaskByID(_liTasksToTrigger[i].Key); }
                    else { TaskManager.AddDelayedTask(_liTasksToTrigger[i].Key); }
                }

                var goalNPC = GoalNPC();
                foreach (Item i in ItemRewardList())
                {
                    if (InventoryManager.HasSpaceInInventory(i.ID, i.Number))
                    {
                        InventoryManager.AddToInventory(i);
                    }
                    else
                    {
                        goalNPC.AssignItemToNPC(i.ID, i.Number);
                    }
                }

                goalNPC.CheckInventoryAlert();

                PlayerManager.AddMoney(RewardMoney);

                if (GetBoolByIDKey("Friendship"))
                {
                    string[] parse = Util.FindArguments(GetStringByIDKey("Friendship"));
                    if (parse.Length > 1)
                    {
                        if (int.TryParse(parse[0], out int targetID))
                        {
                            var villager = TownManager.Villagers[targetID];
                            if (!int.TryParse(parse[1], out int pointValue))
                            {
                                pointValue = 0;
                            }
                            villager.FriendshipPoints += pointValue;
                        }
                    }
                }

                if (UnlockObjectID != -1)
                {
                    PlayerManager.AddToCraftingDictionary(UnlockObjectID);
                }

                if (GetBoolByIDKey("SendToTown") && goalNPC is Villager goalVillager)
                {
                    goalVillager.ReadySmokeBomb();
                }

                if (GetBoolByIDKey("UnlockMagic"))
                {
                    PlayerManager.MagicUnlocked = true;
                    GameManager.GoToHUDScreen();
                }

                if (GetBoolByIDKey("UnlockCodex"))
                {
                    PlayerManager.CodexUnlocked = true;
                    GameManager.GoToHUDScreen();
                    GUIManager.NewInfoAlertIcon(Constants.STR_ALERT_CODEX);
                }

                ActivateNPCs();

                TaskManager.TaskLog.Remove(this);
                GUIManager.NewInfoAlertIcon(Constants.STR_ALERT_FINISHED);
            }
        }
        public TalkingActor StartNPC()
        {
            TalkingActor rv = null;
            if (GetBoolByIDKey("StartNPC"))
            {
                rv = TownManager.Villagers[GetIntByIDKey("StartNPC")];
            }

            return rv;
        }
        public TalkingActor GoalNPC()
        {
            TalkingActor rv;
            if (GetBoolByIDKey("StartNPC"))
            {
                rv = TownManager.Villagers[GetIntByIDKey("StartNPC")];
            }
            else
            {
                rv = StartNPC();
            }

            return rv;
        }
        public Item GoalItem()
        {
            Item rv = null;
            if (GetBoolByIDKey("GoalItem"))
            {
                string[] split = Util.FindArguments(GetStringByIDKey("GoalItem"));
                if (int.TryParse(split[0], out int id))
                {
                    if (!int.TryParse(split[1], out int number))
                    {
                        number = 1;
                    }

                    rv = DataManager.GetItem(id, number);
                }
            }

            return rv;
        }
        public Tuple<int, int> GoalObject()
        {
            Tuple<int, int> rv = default;
            if (GetBoolByIDKey("TargetObjectID"))
            {
                string[] split = Util.FindArguments(GetStringByIDKey("TargetObjectID"));
                if (int.TryParse(split[0], out int id))
                {
                    int number;
                    if (split.Length == 1 || !int.TryParse(split[1], out number))
                    {
                        number = 1;
                    }

                    rv = new Tuple<int, int>(id, number);
                }
            }

            return rv;
        }
        public Tuple<int, int> GoalMob()
        {
            Tuple<int, int> rv = new Tuple<int, int>(-1, -1);
            if (GetBoolByIDKey("TargetMobID"))
            {
                string[] split = Util.FindArguments(GetStringByIDKey("TargetMobID"));
                if (int.TryParse(split[0], out int id))
                {
                    int number;
                    if (!int.TryParse(split[1], out number))
                    {
                        number = 1;
                    }

                    rv = new Tuple<int, int>(id, number);
                }
            }

            return rv;
        }

        public List<Item> ItemRewardList()
        {
            List<Item> rv = new List<Item>();
            if (GetBoolByIDKey("ItemRewardID"))
            {
                string[] items = Util.FindParams(GetStringByIDKey("ItemRewardID"));
                if (items.Length > 0)
                {
                    foreach (string itemInfo in items)
                    {
                        string[] parse = Util.FindArguments(itemInfo);
                        Item it = DataManager.GetItem(int.Parse(parse[0]), parse.Length > 1 ? int.Parse(parse[1]) : 1);
                        if (parse.Length == 3) { it.ApplyUniqueData(parse[2]); }
                        rv.Add(it);
                    }
                }
            }

            return rv;
        }

        private void ActivateNPCs()
        {
            if (DataManager.GetBoolByIDKey(ID, "ActivateNPC", DataType.Task))
            {
                var npc = DataManager.GetIntByIDKey(ID, "ActivateNPC", DataType.Task);
                if (TownManager.Villagers.ContainsKey(npc))
                {
                    TownManager.Villagers[npc].Activate(true);
                }
            }
        }

        public string GetProgressString()
        {
            string rv = string.Empty;

            var goalNPC = GoalNPC();
            if (HiddenGoal)
            {
                rv = "???";
            }
            else if (ReadyForHandIn)
            {
                if (goalNPC != null)
                {
                    if (EndBuildingID != -1)
                    {
                        var article = DataManager.GetBoolByIDKey(EndBuildingID, "SkipArticle", DataType.WorldObject) ? "" : "the ";
                        string name = DataManager.GetTextData(EndBuildingID, "Name", DataType.WorldObject);

                        rv = string.Format("Enter {0}{1}", article, name);
                    }
                    else
                    {
                        rv = string.Format("Speak to {0}", goalNPC.Name);
                    }
                }
            }
            else
            {
                var goalItem = GoalItem();
                switch (_eTaskType)
                {
                    case TaskTypeEnum.TownState:
                        if (DataManager.GetBoolByIDKey(ID, "Population", DataType.Task))
                        {
                            var count = DataManager.GetIntByIDKey(ID, "Population", DataType.Task);
                            rv = " Population: " + TownManager.GetPopulation() + "/" + count;
                        }
                        else if (DataManager.GetBoolByIDKey(ID, "TownScore", DataType.Task))
                        {
                            var count = DataManager.GetIntByIDKey(ID, "TownScore", DataType.Task);
                            rv = " Town Score: " + TownManager.GetTownScore() + "/" + count;
                        }
                        else if (DataManager.GetBoolByIDKey(ID, "MonstersKilled", DataType.Task))
                        {
                            var count = DataManager.GetIntByIDKey(ID, "MonstersKilled", DataType.Task);
                            rv = " Monsters Defeated: " + TownManager.TotalDefeatedMobs + "/" + count;
                        }

                        break;
                    case TaskTypeEnum.Fetch:
                        rv = string.Format(@"{0} {1}/{2}", goalItem.Name(), TargetsAccomplished, goalItem.Number);
                        break;
                    case TaskTypeEnum.Craft:
                        rv = string.Format(@"Craft {0} {1}/{2}", goalItem.Name(), TargetsAccomplished, goalItem.Number);
                        break;
                    case TaskTypeEnum.Slay:
                        var mob = GoalMob();
                        rv = DataManager.GetTextData(mob.Item1, "Name", DataType.Actor) + " Defeated: " + TargetsAccomplished + "/" + mob.Item2;
                        break;
                    case TaskTypeEnum.Build:
                        var goalObj = GoalObject();
                        string objName = DataManager.GetTextData(goalObj.Item1, "Name", DataType.WorldObject);

                        if (goalObj.Item2 > 1) { rv = string.Format(@"Build {0} {1}s", goalObj.Item2.ToString(), objName); }
                        else if (!DataManager.GetBoolByIDKey(goalObj.Item1, "SkipArticle", DataType.WorldObject))
                        {
                            var vowels = new List<string>() { "a", "e", "i", "o", "u" };
                            var article = "a";
                            foreach (var vowel in vowels)
                            {
                                if (objName.StartsWith(vowel))
                                {
                                    article = "an";
                                }
                            }
                            rv = string.Format("Build {0} {1}", article, objName);
                        }
                        else
                        {
                            rv = string.Format("Build {0}", objName);
                        }
                        break;
                    case TaskTypeEnum.Talk:
                        rv = "Speak to " + goalNPC.Name;
                        break;
                }
            }

            return rv;
        }

        #region Lookup Handlers
        public bool GetBoolByIDKey(string key)
        {
            bool rv = false;
            if (ID == -1)
            {
                rv = _diLookupInfo.ContainsKey(key);
            }
            else
            {
                rv = DataManager.GetBoolByIDKey(ID, key, DataType.Task);
            }

            return rv;
        }
        public int GetIntByIDKey(string key, int defaultValue = -1)
        {
            int rv = defaultValue;
            if (ID == -1)
            {
                if (_diLookupInfo.ContainsKey(key))
                {
                    int.TryParse(_diLookupInfo[key], out rv);
                }
            }
            else
            {
                rv = DataManager.GetIntByIDKey(ID, key, DataType.Task, defaultValue);
            }

            return rv;
        }
        public float GetFloatByIDKey(string key, float defaultValue = -1)
        {
            float rv = defaultValue;
            if (ID == -1)
            {
                if (_diLookupInfo.ContainsKey(key))
                {
                    float.TryParse(_diLookupInfo[key], out rv);
                }
            }
            else
            {
                rv = DataManager.GetFloatByIDKey(ID, key, DataType.Task, defaultValue);
            }

            return rv;
        }
        public string GetStringByIDKey(string key)
        {
            string rv = string.Empty;
            if (ID == -1)
            {
                if (_diLookupInfo.ContainsKey(key))
                {
                    rv = _diLookupInfo[key];
                }
            }
            else
            {
                rv = DataManager.GetStringByIDKey(ID, key, DataType.Task);
            }

            return rv;
        }
        public virtual TEnum GetEnumByIDKey<TEnum>(string key) where TEnum : struct
        {
            TEnum rv = default;
            if (ID == -1)
            {
                if (_diLookupInfo.ContainsKey(key))
                {
                    rv = Util.ParseEnum<TEnum>(_diLookupInfo[key]);
                }
            }
            else
            {
                rv = DataManager.GetEnumByIDKey<TEnum>(ID, key, DataType.Task);
            }

            return rv;
        }
        #endregion

        public struct TaskData
        {
            [XmlElement(ElementName = "TaskType")]
            public int questType;

            [XmlElement(ElementName = "TaskState")]
            public int taskState;

            [XmlElement(ElementName = "Accomplished")]
            public int accomplished;

            [XmlElement(ElementName = "ReadyForHandIn")]
            public bool readyForHandIn;
        }

        public TaskData SaveData()
        {
            TaskData qData = new TaskData
            {
                questType = (int)_eTaskType,
                taskState = (int)TaskState,
                accomplished = TargetsAccomplished,
                readyForHandIn = ReadyForHandIn
            };

            return qData;
        }
        public void LoadData(TaskData qData)
        {
            if (ID == -1)
            {
                _eTaskType = (TaskTypeEnum)qData.questType;
            }

            TaskState = (TaskStateEnum)qData.taskState;
            TargetsAccomplished = qData.accomplished;
            ReadyForHandIn = qData.readyForHandIn;

            if (ReadyForHandIn && TaskState != TaskStateEnum.Completed)
            {
                if (EndBuildingID == -1)
                {
                    GoalNPC()?.ModifyTaskGoalValue(1);
                }
            }

            if (TaskState == TaskStateEnum.Completed)
            {
                ActivateNPCs();
            }
        }
    }
}
