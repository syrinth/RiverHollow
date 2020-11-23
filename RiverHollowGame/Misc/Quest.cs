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
    public class Quest
    {
        public int QuestID { get; private set; }
        public QuestTypeEnum QuestType { get; }
        private string _name;
        public string Name => _name;
        private string _sDescription;
        public string Description => _sDescription;
        public Villager GoalNPC { get; private set; }

        int _iCutsceneID;
        public int RequiredItemAmount { get; private set; }
        public int TargetsAccomplished { get; private set; }

        private Monster _questMob;
        private Item _targetItem;
        public bool ReadyForHandIn { get; private set; }
        public bool Finished { get; private set; }

        bool _bImmediate;
        int _iActivateID;
        bool _bHideGoal;
        #region Rewards
        public int Friendship { get; }
        public string FriendTarget { get; }
        public int RewardMoney { get; }
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

        public Quest()
        {
            _iCutsceneID = -1;
            _bImmediate = false;
            _iActivateID = -1;
            QuestID = -1;
            _iSeason = -1;
            _iDay = -1;
            _name = string.Empty;
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
        public Quest(string name, QuestTypeEnum type, string desc, int target, Monster m, Item i, Villager giver = null) : this()
        {
            _name = name;
            QuestType = type;
            _sDescription = desc;
            GoalNPC = giver;
            RequiredItemAmount = target;
            _questMob = m;
            _targetItem = i;
            TargetsAccomplished = 0;
            ReadyForHandIn = false;
        }

        public Quest(int id, Dictionary<string, string> stringData) : this()
        {
            QuestID = id;
            TargetsAccomplished = 0;
            LiRewardItems = new List<Item>();

            DataManager.GetTextData("Quest", QuestID, ref _name, "Name");
            DataManager.GetTextData("Quest", QuestID, ref _sDescription, "Description");

            _name = Util.ProcessText(_name);
            _sDescription = Util.ProcessText(_sDescription);

            QuestType = Util.ParseEnum<QuestTypeEnum>(stringData["Type"]);

            if (stringData.ContainsKey("GoalItem"))
            {
                string[] info = stringData["GoalItem"].Split('-');
                _targetItem = DataManager.GetItem(int.Parse(info[0]));
                RequiredItemAmount = int.Parse(info[1]);
            }

            if (stringData.ContainsKey("ItemReward"))
            {
                string[] items = stringData["ItemReward"].Split(' ');
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
                    Friendship = int.Parse(parse[1]);
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

            if (stringData.ContainsKey("GoalNPC")) { GoalNPC = DataManager.DiNPC[int.Parse(stringData["GoalNPC"])]; }
            if (stringData.ContainsKey("Money")) { RewardMoney = int.Parse(stringData["Money"]); }
            if (stringData.ContainsKey("Day")) { _iDay = int.Parse(stringData["Day"]); }
            if (stringData.ContainsKey("Season")) { _iSeason = int.Parse(stringData["Season"]); }
            if (stringData.ContainsKey("Immediate")) { _bImmediate = true; }
            if (stringData.ContainsKey("Activate")) { _iActivateID = int.Parse(stringData["Activate"]); }
            if (stringData.ContainsKey("Cutscene")) { _iCutsceneID = int.Parse(stringData["Cutscene"]); }
            if (stringData.ContainsKey("HideGoal")) { _bHideGoal = true; }
        }

        public bool AttemptProgress(Villager a)
        {
            bool rv = false;

            if(QuestType == QuestTypeEnum.Talk && GoalNPC == a) {
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

        public void IncrementProgress(int num)
        {
            if (TargetsAccomplished < RequiredItemAmount)
            {
                TargetsAccomplished += num;
                if (TargetsAccomplished >= RequiredItemAmount)
                {
                    TargetsAccomplished = RequiredItemAmount;
                    ReadyForHandIn = true;
                    if (_bImmediate)
                    {
                        string questCompleteText = string.Empty;
                        FinishQuest(ref questCompleteText);
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

        public void SpawnQuestMobs()
        {
            if (_spawnMob != null)
            {
                RHMap map = MapManager.Maps[_sSpawnMap];
                map.AddMonsterByPosition(_spawnMob, map.DictionaryCharacterLayer[_sLocName]);
            }
        }
        public void FinishQuest(ref string questCompleteText)
        {
            Finished = true;

            if (GoalNPC != null)
            {
                questCompleteText = "Quest" + QuestID + "End";
            }
            //text = HandInTo.GetDialogEntry("Quest"+_iQuestID+"End");
            foreach (Item i in LiRewardItems)
            {
                InventoryManager.AddToInventory(i);
            }
            PlayerManager.AddMoney(RewardMoney);

            if (FriendTarget.Equals("Giver"))
            {
                GoalNPC.FriendshipPoints += Friendship;
            }

            if (_iActivateID > -1)
            {
                DataManager.DiNPC[_iActivateID].Activate(true);
            }

            PlayerManager.QuestLog.Remove(this);
            GUIManager.NewQuestIcon(true);

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
            if (_bHideGoal) {
                rv = "???";
            }
            else
            {
                switch (QuestType)
                {
                    case QuestTypeEnum.Fetch:
                        rv = _targetItem.Name + " Found: " + TargetsAccomplished + "/" + RequiredItemAmount;
                        break;
                    case QuestTypeEnum.GroupSlay:
                        rv = _questMob.Name + " Defeated: " + TargetsAccomplished + "/" + RequiredItemAmount;
                        break;
                    case QuestTypeEnum.Slay:
                        rv = _questMob.Name + " Defeated: " + TargetsAccomplished + "/" + RequiredItemAmount;
                        break;
                    case QuestTypeEnum.Talk:
                        rv = "Speak to " + GoalNPC.Name;
                        break;
                }
            }

            return rv;
        }

        public struct QuestData
        {
            [XmlElement(ElementName = "QuestID")]
            public int questID;

            [XmlElement(ElementName = "Name")]
            public string name;

            [XmlElement(ElementName = "Description")]
            public string description;

            [XmlElement(ElementName = "RewardText")]
            public string rewardText;

            [XmlElement(ElementName = "GoalNPC")]
            public int goalNPC;

            [XmlElement(ElementName = "QuestItem")]
            public int itemID;

            [XmlElement(ElementName = "QuestMob")]
            public int mobID;

            [XmlElement(ElementName = "TargetGoal")]
            public int targetGoal;

            [XmlElement(ElementName = "Accomplished")]
            public int accomplished;

            [XmlElement(ElementName = "ReadyForHandIn")]
            public bool readyForHandIn;

            [XmlElement(ElementName = "Finished")]
            public bool finished;

            [XmlElement(ElementName = "RewardMoney")]
            public int rewardMoney;

            [XmlArray(ElementName = "RewardItems")]
            public List<ItemData> Items;
        }

        public QuestData SaveData()
        {
            QuestData qData = new QuestData
            {
                questID = QuestID,
                name = _name,
                description = _sDescription,
                goalNPC = GoalNPC != null ? GoalNPC.ID : -1,
                itemID = _targetItem != null  ? _targetItem.ItemID : -1,
                mobID = _questMob != null ? _questMob.ID : -1,
                targetGoal = RequiredItemAmount, 
                accomplished = TargetsAccomplished, 
                readyForHandIn = ReadyForHandIn,
                finished = Finished
                //rewardMoney = _re
            };

            qData.Items = new List<ItemData>();
            foreach(Item i in LiRewardItems)
            {
                qData.Items.Add(Item.SaveData(i));
            }

            return qData;
        }
        public void LoadData(QuestData qData)
        {
            if(qData.questID != -1 )
            {
                Finished = qData.finished;
            }
            else
            {
                QuestID = qData.questID;
                _name = qData.name;
                _sDescription = qData.description;
                GoalNPC = qData.goalNPC != -1 ? DataManager.DiNPC[qData.goalNPC] : null;
                _targetItem = qData.itemID != -1 ? DataManager.GetItem(qData.itemID) : null;
                _questMob = qData.mobID != -1 ? DataManager.GetMonsterByIndex(qData.mobID) : null;
                RequiredItemAmount = qData.targetGoal;
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
}
