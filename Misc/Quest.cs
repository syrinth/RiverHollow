using RiverHollow.Characters;
using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using static RiverHollow.Characters.NPCs.Merchandise;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Misc
{
    public class Quest
    {
        int _iQuestID;
        public enum QuestType { GroupSlay, Slay, Fetch }
        private QuestType _goalType;
        private string _name;
        public string Name { get => _name; }
        private string _description;
        public string Description { get => _description; }
        private string _sRewardText;
        public string RewardText => _sRewardText;

        private NPC _questGiver;
        public NPC QuestGiver { get => _questGiver; }

        private int _iTargetGoal;
        public int TargetGoal { get => _iTargetGoal; }
        private int _iAccomplished;
        public int Accomplished { get => _iAccomplished; }

        private Monster _questMob;
        private Item _questItem;

        bool _bReadyForHandIn;
        public bool ReadyForHandIn => _bReadyForHandIn;

        bool _bFinished;
        public bool Finished => _bFinished;

        //private int _rewardMoney;
        //public int RewardMoney { get => _rewardMoney; }
        private List<Item> _liRewardItems;
        public List<Item> LiRewardItems => _liRewardItems;

        public Quest()
        {
            _iQuestID = -1;
            _name = string.Empty;
            _description = string.Empty;
            _sRewardText = string.Empty;
            _questGiver = null;
            _questItem = null;
            _questMob = null;
            _iTargetGoal = -1;
            _iAccomplished = -1;
            _bReadyForHandIn = false;
            _bFinished = false;

            _liRewardItems = new List<Item>();
        }
        public Quest(string name, QuestType type, string desc, int target, Monster m, Item i, NPC giver = null) : this()
        {
            _name = name;
            _goalType = type;
            _description = desc;
            _questGiver = giver;
            _iTargetGoal = target;
            _questMob = m;
            _questItem = i;
            _iAccomplished = 0;
            _bReadyForHandIn = false;
        }

        public Quest(string stringData, int id) : this()
        {
            _iQuestID = id;
            _liRewardItems = new List<Item>();
            string[] splitParams = stringData.Split('/');
            int i = 0;
            _name = splitParams[i++];
            _description = splitParams[i++];
            i++;   //reqtoProc
            _questGiver = CharacterManager.DiNPC[int.Parse(splitParams[i++])];
            _goalType = Util.ParseEnum<QuestType>(splitParams[i++]);
            string[] req = splitParams[i++].Split('|');
            if (_goalType == QuestType.Fetch)
            {
                foreach (string s in req)
                {
                    string[] info = s.Split(' ');
                    _questItem = ObjectManager.GetItem(int.Parse(info[0]));
                    _iTargetGoal = int.Parse(info[1]);
                }
            }
            string[] rewards = splitParams[i++].Split('|');
            foreach(string s in rewards)
            {
                string[] info = s.Split(' ');
                Item it = ObjectManager.GetItem(int.Parse(info[0]), int.Parse(info[1]));
                if(info.Length == 3) { it.ApplyUniqueData(info[2]); }
                _liRewardItems.Add(it);
            }
            _sRewardText = splitParams[i++];
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
                }
            }
        }
        public bool RemoveProgress(Item i)
        {
            bool rv = false;
            if (_questItem != null && _questItem.ItemID == ((Item)i).ItemID)
            {
                if (_iAccomplished > 0)
                {
                    _iAccomplished--;
                    rv = true;
                }
            }
            return rv;
        }

        public void FinishQuest(ref string text)
        {
            _bFinished = true;

            text = QuestGiver.GetDialogEntry(RewardText);
            foreach (Item i in LiRewardItems)
            {
                InventoryManager.AddItemToInventory(i);
            }
            PlayerManager.QuestLog.Remove(this);
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
                description = _description,
                rewardText = _sRewardText,
                questGiver = _questGiver != null ? _questGiver.ID : -1,
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
                qData.Items.Add(i.SaveData());
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
                _description = qData.description;
                _sRewardText = qData.rewardText;
                _questGiver = qData.questGiver != -1 ? CharacterManager.DiNPC[qData.questGiver] : null;
                _questItem = qData.itemID != -1 ? ObjectManager.GetItem(qData.itemID) : null;
                _questMob = qData.mobID != -1 ? CharacterManager.GetMonsterByIndex(qData.mobID) : null;
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
