using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Xml.Serialization;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;


namespace RiverHollow.Misc
{
    public class Task
    {
        public int TaskID { get; private set; }
        private TaskTypeEnum _eTaskType;
        private string _sName;
        public string Name => _sName;
        private string _sDescription;
        public string Description => _sDescription;
        public Villager GoalNPC { get; private set; }

        int _iCutsceneID;
        public int RequiredItemAmount { get; private set; }
        public int TargetsAccomplished { get; private set; }

        private Monster _questMob;
        private Item _targetItem;
        private int _iTargetBuildingID = -1;
        public bool ReadyForHandIn { get; private set; } = false;
        public bool Finished { get; private set; }

        bool _bFinishOnCompletion;
        int _iActivateID;
        bool _bHiddenGoal;
        #region Rewards
        private int _iUnlockBuildingID = -1;
        private int _iRewardMoney = 0;
        private int _iFriendPoints = 0;
        public string FriendTarget { get; }

        public List<Item> LiRewardItems { get; }
        #endregion
        #region Spawn Mobs
        Monster _spawnMob;
        string _sSpawnMap;
        string _sLocName;
        #endregion
        #region Reqs
        int _iDay;
        int _iSeason;
        #endregion

        public Task()
        {
            _iCutsceneID = -1;
            _bFinishOnCompletion = false;
            _iActivateID = -1;
            TaskID = -1;
            _iSeason = -1;
            _iDay = -1;
            _sName = string.Empty;
            _sDescription = string.Empty;
            FriendTarget = string.Empty;
            GoalNPC = null;
            _targetItem = null;
            _questMob = null;
            RequiredItemAmount = -1;
            TargetsAccomplished = -1;
            ReadyForHandIn = false;
            Finished = false;

            LiRewardItems = new List<Item>();
        }
        public Task(string name, TaskTypeEnum type, string desc, int target, Monster m, Item i, Villager giver = null) : this()
        {
            _sName = name;
            _eTaskType = type;
            _sDescription = desc;
            GoalNPC = giver;
            RequiredItemAmount = target;
            _questMob = m;
            _targetItem = i;
            TargetsAccomplished = 0;
            ReadyForHandIn = false;
        }

        public Task(int id, Dictionary<string, string> stringData) : this()
        {
            TaskID = id;
            TargetsAccomplished = 0;
            LiRewardItems = new List<Item>();

            DataManager.GetTextData("Task", TaskID, ref _sName, "Name");
            DataManager.GetTextData("Task", TaskID, ref _sDescription, "Description");

            _eTaskType = Util.ParseEnum<TaskTypeEnum>(stringData["Type"]);

            if (stringData.ContainsKey("GoalItem"))
            {
                string[] info = stringData["GoalItem"].Split('-');
                _targetItem = DataManager.GetItem(int.Parse(info[0]));
                RequiredItemAmount = int.Parse(info[1]);
            }

            if (stringData.ContainsKey("ItemReward"))
            {
                string[] items = Util.FindParams(stringData["ItemReward"]);
                if (items.Length > 1)
                {
                    foreach (string itemInfo in items)
                    {
                        string[] parse = itemInfo.Split('-');
                        Item it = DataManager.GetItem(int.Parse(parse[0]), parse.Length > 1 ? int.Parse(parse[1]) : 1);
                        if (parse.Length == 3) { it.ApplyUniqueData(parse[2]); }
                        LiRewardItems.Add(it);
                    }
                }
            }

            if (stringData.ContainsKey("Friendship"))
            {
                string[] parse = stringData["Friendship"].Split('-');
                if (parse.Length > 1)
                {
                    FriendTarget = parse[0];
                    _iFriendPoints = int.Parse(parse[1]);
                }
            }

            if (stringData.ContainsKey("SpawnMob"))
            {
                string[] parse = stringData["SpawnMob"].Split('-');
                if (parse.Length > 1)
                {
                    _spawnMob = DataManager.GetMonsterByIndex(int.Parse(parse[0]));
                    _sSpawnMap = parse[1];
                    _sLocName = parse[2];
                }
            }

            if (stringData.ContainsKey("GoalNPC")) { GoalNPC = DataManager.DIVillagers[int.Parse(stringData["GoalNPC"])]; }

            Util.AssignValue(ref _iRewardMoney, "Money", stringData);
            Util.AssignValue(ref _iTargetBuildingID, "BuildingID", stringData);
            Util.AssignValue(ref _iUnlockBuildingID, "BuildingRewardID", stringData);

            Util.AssignValue(ref _iDay, "Day", stringData);
            Util.AssignValue(ref _iSeason, "Season", stringData);

            Util.AssignValue(ref _bFinishOnCompletion, "Immediate", stringData);
            Util.AssignValue(ref _iActivateID, "Activate", stringData);
            Util.AssignValue(ref _iCutsceneID, "Cutscene", stringData);
            Util.AssignValue(ref _bHiddenGoal, "HideGoal", stringData);
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
        public bool AttemptProgress(Monster m)
        {
            bool rv = false;

            if (_questMob != null && _questMob.ID == m.ID)
            {
                rv = true;
                IncrementProgress(1);
            }

            return rv;
        }
        public bool AttemptProgress(Item i)
        {
            bool rv = false;

            if (_targetItem != null && _targetItem.ItemID == i.ItemID)
            {
                rv = true;
                IncrementProgress(i.Number);
            }

            return rv;
        }
        public bool AttemptBuildingProgress(int i)
        {
            bool rv = false;

            if (_eTaskType == TaskTypeEnum.Build && i == _iTargetBuildingID)
            {
                rv = true;
                ReadyForHandIn = true;
            }

            return rv;
        }

        public void IncrementProgress(int num)
        {
            if (TargetsAccomplished < RequiredItemAmount)
            {
                TargetsAccomplished += num;
                if (TargetsAccomplished >= RequiredItemAmount)
                {
                    TargetsAccomplished = RequiredItemAmount;
                    ReadyForHandIn = true;
                    if (_bFinishOnCompletion)
                    {
                        string questCompleteText = string.Empty;
                        FinishTask(ref questCompleteText);
                    }
                }
            }
        }
        public bool RemoveProgress(Item i)
        {
            bool rv = false;
            if (i != null && _targetItem != null && _targetItem.ItemID == ((Item)i).ItemID)
            {
                if (TargetsAccomplished > 0)
                {
                    TargetsAccomplished--;
                    rv = true;
                }
            }
            return rv;
        }

        public void SpawnTaskMobs()
        {
            if (_spawnMob != null)
            {
                RHMap map = MapManager.Maps[_sSpawnMap];
                map.AddMonsterByPosition(_spawnMob, map.DictionaryCharacterLayer[_sLocName]);
            }
        }
        public void FinishTask(ref string questCompleteText)
        {
            Finished = true;

            if (GoalNPC != null)
            {
                questCompleteText = "Task_" + TaskID + "_End";
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

            if(_iUnlockBuildingID != -1)
            {
                PlayerManager.DIBuildInfo[_iUnlockBuildingID].Unlock();
            }

            if (_iActivateID > -1)
            {
                DataManager.DIVillagers[_iActivateID].Activate(true);
            }

            PlayerManager.TaskLog.Remove(this);
            GUIManager.NewTaskIcon(true);

            if (_iCutsceneID != -1)
            {
                CutsceneManager.TriggerCutscene(_iCutsceneID);
            }
        }

        public bool CanBeGiven() {
            bool rv = true;

            if (_iSeason > -1)
            {
                if (_iSeason != GameCalendar.CurrentSeason)
                {
                    rv = false;
                }
            }
            if (_iDay > -1)
            {
                if (_iDay == GameCalendar.CurrentDay)
                {
                    rv = false;
                }
            }

            return rv;
        }

        public string GetProgressString()
        {
            string rv = string.Empty;

            if (_bHiddenGoal) { rv = "???"; }
            else if (ReadyForHandIn) { rv = "Speak to " + GoalNPC.Name; }
            else
            {
                switch (_eTaskType)
                {
                    case TaskTypeEnum.Fetch:
                        rv = _targetItem.Name + " Found: " + TargetsAccomplished + "/" + RequiredItemAmount;
                        break;
                    case TaskTypeEnum.GroupSlay:
                        rv = _questMob.Name + " Defeated: " + TargetsAccomplished + "/" + RequiredItemAmount;
                        break;
                    case TaskTypeEnum.Slay:
                        rv = _questMob.Name + " Defeated: " + TargetsAccomplished + "/" + RequiredItemAmount;
                        break;
                    case TaskTypeEnum.Build:
                        rv = "Build " + PlayerManager.DIBuildInfo[_iTargetBuildingID].Name;
                        break;
                    case TaskTypeEnum.Talk:
                        rv = "Speak to " + GoalNPC.Name;
                        break;
                }
            }

            return rv;
        }

        public struct TaskData
        {
            [XmlElement(ElementName = "TaskType")]
            public TaskTypeEnum questType;

            [XmlElement(ElementName = "TaskID")]
            public int taskID;

            [XmlElement(ElementName = "Name")]
            public string name;

            [XmlElement(ElementName = "Description")]
            public string description;

            [XmlElement(ElementName = "RewardText")]
            public string rewardText;

            [XmlElement(ElementName = "GoalNPC")]
            public int goalNPC;

            [XmlElement(ElementName = "TaskItem")]
            public int itemID;

            [XmlElement(ElementName = "TaskMob")]
            public int mobID;

            [XmlElement(ElementName = "TaskBuilding")]
            public int targetBuildingID;

            [XmlElement(ElementName = "TargetGoal")]
            public int targetGoal;

            [XmlElement(ElementName = "Accomplished")]
            public int accomplished;

            [XmlElement(ElementName = "ReadyForHandIn")]
            public bool readyForHandIn;

            [XmlElement(ElementName = "Finished")]
            public bool finished;

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
                questType = _eTaskType,
                taskID = TaskID,
                name = _sName,
                description = _sDescription,
                goalNPC = GoalNPC != null ? GoalNPC.ID : -1,
                itemID = _targetItem != null ? _targetItem.ItemID : -1,
                mobID = _questMob != null ? _questMob.ID : -1,
                targetBuildingID = _iTargetBuildingID,
                unlockBuildingID = _iUnlockBuildingID,
                targetGoal = RequiredItemAmount, 
                hiddenGoal = _bHiddenGoal,
                accomplished = TargetsAccomplished, 
                readyForHandIn = ReadyForHandIn,
                finished = Finished
            };

            qData.Items = new List<ItemData>();
            foreach(Item i in LiRewardItems)
            {
                qData.Items.Add(Item.SaveData(i));
            }

            return qData;
        }
        public void LoadData(TaskData qData)
        {
            _eTaskType = qData.questType;
            TaskID = qData.taskID;
            _sName = qData.name;
            _sDescription = qData.description;
            GoalNPC = qData.goalNPC != -1 ? DataManager.DIVillagers[qData.goalNPC] : null;
            _targetItem = qData.itemID != -1 ? DataManager.GetItem(qData.itemID) : null;
            _questMob = qData.mobID != -1 ? DataManager.GetMonsterByIndex(qData.mobID) : null;
            _iTargetBuildingID = qData.targetBuildingID;
            _iUnlockBuildingID = qData.unlockBuildingID;
            RequiredItemAmount = qData.targetGoal;
            _bHiddenGoal = qData.hiddenGoal;
            TargetsAccomplished = qData.accomplished;
            ReadyForHandIn = qData.readyForHandIn;
            Finished = qData.finished;

            foreach (ItemData i in qData.Items)
            {
                Item newItem = DataManager.GetItem(i.itemID, i.num);

                if (newItem != null) { newItem.ApplyUniqueData(i.strData); }
                LiRewardItems.Add(newItem);
            }
        }
    }
}
