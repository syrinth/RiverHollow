using RiverHollow.Actors;
using RiverHollow.Game_Managers;
using RiverHollow.Tile_Engine;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using System.Xml.Serialization;

using static RiverHollow.Game_Managers.GameManager;
namespace RiverHollow.Misc
{
    public class Quest
    {
        int _iQuestID;
        public int QuestID => _iQuestID;
        public enum QuestType { GroupSlay, Slay, Fetch }
        private QuestType _goalType;
        private string _name;
        public string Name => _name;
        private string _sDescription;
        public string Description => _sDescription;

        private Villager _npcHandInTo;
        public Villager HandInTo => _npcHandInTo;

        private int _iTargetGoal;
        public int TargetGoal => _iTargetGoal;
        private int _iAccomplished;
        public int Accomplished => _iAccomplished;

        private Monster _questMob;
        private Item _questItem;

        bool _bReadyForHandIn;
        public bool ReadyForHandIn => _bReadyForHandIn;

        bool _bFinished;
        public bool Finished => _bFinished;

        bool _bImmediate;
        int _iActivateID;

        #region Rewards
        int _iFriendship;
        public int Friendship => _iFriendship;
        string _sFriendTarget;
        public string FriendTarget => _sFriendTarget;
        int _iRewardMoney;
        public int RewardMoney { get => _iRewardMoney; }
        List<Item> _liRewardItems;
        public List<Item> LiRewardItems => _liRewardItems;
        #endregion
        #region Spawn Mobs
        Mob _spawnMob;
        string _sSpawnMap;
        string _sLocName;
        #endregion
        #region Reqs
        int _iDay;
        int _iSeason;
        #endregion

        public Quest()
        {
            _bImmediate = false;
            _iActivateID = -1;
            _iQuestID = -1;
            _iSeason = -1;
            _iDay = -1;
            _name = string.Empty;
            _sDescription = string.Empty;
            _sFriendTarget = string.Empty;
            _npcHandInTo = null;
            _questItem = null;
            _questMob = null;
            _iTargetGoal = -1;
            _iAccomplished = -1;
            _bReadyForHandIn = false;
            _bFinished = false;

            _liRewardItems = new List<Item>();
        }
        public Quest(string name, QuestType type, string desc, int target, Monster m, Item i, Villager giver = null) : this()
        {
            _name = name;
            _goalType = type;
            _sDescription = desc;
            _npcHandInTo = giver;
            _iTargetGoal = target;
            _questMob = m;
            _questItem = i;
            _iAccomplished = 0;
            _bReadyForHandIn = false;
        }

        public Quest(string stringData, int id) : this()
        {
            _iQuestID = id;
            _iAccomplished = 0;
            _liRewardItems = new List<Item>();
            string[] splitParams = stringData.Split('/');
            int i = 0;
            GameContentManager.GetQuestText(_iQuestID, ref _name, ref _sDescription);

            string[] split = Util.FindTags(splitParams[i++]);
            foreach (string s in split)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("HandTo"))
                {
                    _npcHandInTo = ObjectManager.DiNPC[int.Parse(tagType[1])];
                }
                else if (tagType[0].Equals("Type"))
                {
                    _goalType = Util.ParseEnum<QuestType>(tagType[1]);
                }
                else if (tagType[0].Equals("GoalItem"))
                {
                    string[] info = tagType[1].Split('-');
                    _questItem = ObjectManager.GetItem(int.Parse(info[0]));
                    _iTargetGoal = int.Parse(info[1]);
                }
                else if (tagType[0].Equals("Item"))
                {
                    string[] parse = tagType[1].Split('-');
                    if (parse.Length > 1)
                    {
                        Item it = ObjectManager.GetItem(int.Parse(parse[0]), int.Parse(parse[1]));
                        if (parse.Length == 3) { it.ApplyUniqueData(parse[2]); }
                        _liRewardItems.Add(it);
                    }
                }
                else if (tagType[0].Equals("Friendship"))
                {
                    string[] parse = tagType[1].Split('-');
                    if (parse.Length > 1)
                    {
                        _sFriendTarget = parse[0];
                        _iFriendship = int.Parse(parse[1]);
                    }
                }
                else if (tagType[0].Equals("SpawnMob"))
                {
                    string[] parse = tagType[1].Split('-');
                    if (parse.Length > 1)
                    {
                        _spawnMob = ObjectManager.GetMobByIndex(int.Parse(parse[0]));
                        _sSpawnMap = parse[1];
                        _sLocName = parse[2];
                    }
                }
                else if (tagType[0].Equals("Money"))
                {
                    _iRewardMoney = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Day"))
                {
                    _iDay = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Season"))
                {
                    _iSeason = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Immediate"))
                {
                    _bImmediate = true;
                }
                else if (tagType[0].Equals("Activate"))
                {
                    _iActivateID = int.Parse(tagType[1]);
                }
            }        
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

            if (_questItem != null && _questItem.ItemID == i.ItemID)
            {
                rv = true;
                IncrementProgress(i.Number);
            }

            return rv;
        }

        public void IncrementProgress(int num)
        {
            if (_iAccomplished < _iTargetGoal)
            {
                _iAccomplished += num;
                if (_iAccomplished >= _iTargetGoal)
                {
                    _iAccomplished = _iTargetGoal;
                    _bReadyForHandIn = true;
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
            if (i != null && _questItem != null && _questItem.ItemID == ((Item)i).ItemID)
            {
                if (_iAccomplished > 0)
                {
                    _iAccomplished--;
                    rv = true;
                }
            }
            return rv;
        }

        public void SpawnMobs()
        {
            if (_spawnMob != null)
            {
                RHMap map = MapManager.Maps[_sSpawnMap];
                map.AddMob(_spawnMob, map.DictionaryCharacterLayer[_sLocName]);
            }
        }
        public void FinishQuest(ref string text)
        {
            _bFinished = true;

            if (HandInTo != null)
            {
                HandInTo.Talk("Quest" + _iQuestID + "End");
            }
            //text = HandInTo.GetDialogEntry("Quest"+_iQuestID+"End");
            foreach (Item i in LiRewardItems)
            {
                InventoryManager.AddToInventory(i);
            }
            PlayerManager.AddMoney(_iRewardMoney);

            if (_sFriendTarget.Equals("Giver"))
            {
                _npcHandInTo.FriendshipPoints += _iFriendship;
            }

            if (_iActivateID > -1)
            {
                ObjectManager.DiNPC[_iActivateID].Activate(true);
            }

            PlayerManager.QuestLog.Remove(this);

            CutsceneManager.CheckForTriggedCutscene();
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
            if(_questMob != null) { rv += _questMob.Name + " Defeated: "; }
            else if (_questItem != null) { rv += _questItem.Name + " Found: "; }

            rv += _iAccomplished + "/" + _iTargetGoal;

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

            [XmlElement(ElementName = "QuestGiver")]
            public int questGiver;

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
                questID = _iQuestID,
                name = _name,
                description = _sDescription,
                questGiver = _npcHandInTo != null ? _npcHandInTo.ID : -1,
                itemID = _questItem != null  ? _questItem.ItemID : -1,
                mobID = _questMob != null ? _questMob.ID : -1,
                targetGoal = _iTargetGoal, 
                accomplished = _iAccomplished, 
                readyForHandIn = _bReadyForHandIn,
                finished = _bFinished
                //rewardMoney = _re
            };

            qData.Items = new List<ItemData>();
            foreach(Item i in _liRewardItems)
            {
                qData.Items.Add(Item.SaveData(i));
            }

            return qData;
        }
        public void LoadData(QuestData qData)
        {
            if(qData.questID != -1 )
            {
                _bFinished = qData.finished;
            }
            else
            {
                _iQuestID = qData.questID;
                _name = qData.name;
                _sDescription = qData.description;
                _npcHandInTo = qData.questGiver != -1 ? ObjectManager.DiNPC[qData.questGiver] : null;
                _questItem = qData.itemID != -1 ? ObjectManager.GetItem(qData.itemID) : null;
                _questMob = qData.mobID != -1 ? ObjectManager.GetMonsterByIndex(qData.mobID) : null;
                _iTargetGoal = qData.targetGoal;
                _iAccomplished = qData.accomplished;
                _bReadyForHandIn = qData.readyForHandIn;
                _bFinished = qData.finished;

                foreach (ItemData i in qData.Items)
                {
                    Item newItem = ObjectManager.GetItem(i.itemID, i.num);

                    if (newItem != null) { newItem.ApplyUniqueData(i.strData); }
                    _liRewardItems.Add(newItem);
                }
            }
        }
    }
}
