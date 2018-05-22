using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.WorldObjects;
using System.Collections.Generic;

namespace RiverHollow.Misc
{
    public class Quest
    {
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

        private int _targetGoal;
        public int TargetGoal { get => _targetGoal; }
        private int _accomplished;
        public int Accomplished { get => _accomplished; }

        private Monster _questMob;
        private Item _questItem;

        private bool _finished;
        public bool Finished { get => _finished; }

        //private int _rewardMoney;
        //public int RewardMoney { get => _rewardMoney; }
        private List<Item> _liRewardItems;
        public List<Item> LiRewardItems => _liRewardItems;

        public Quest(string name, QuestType type, string desc, int target, Monster m, Item i, NPC giver = null)
        {
            _name = name;
            _goalType = type;
            _description = desc;
            _questGiver = giver;
            _targetGoal = target;
            _questMob = m;
            _questItem = i;
            _accomplished = 0;
            _finished = false;
        }

        public Quest(string stringData)
        {
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
                    _targetGoal = int.Parse(info[1]);
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
            if (_accomplished < _targetGoal)
            {
                _accomplished += num;
                if (_accomplished >= _targetGoal)
                {
                    _accomplished = _targetGoal;
                    _finished = true;
                }
            }
        }
        public bool RemoveProgress(Item i)
        {
            bool rv = false;
            if (_questItem != null && _questItem.ItemID == ((Item)i).ItemID)
            {
                if (_accomplished > 0)
                {
                    _accomplished--;
                    rv = true;
                }
            }
            return rv;
        }

        //protected int ImportBasics(string[] stringData)
        //{
        //    int i = 0;
        //    _name = stringData[i++];
        //    _goalType = (QuestGoalType)Enum.Parse(typeof(QuestGoalType), stringData[i++]);
        //    _name = stringData[i++];
        //    _description = stringData[i++];
        //    _textureIndex = int.Parse(stringData[i++]);
        //    _sellPrice = int.Parse(stringData[i++]);

        //    _itemID = id;//(ObjectManager.ItemIDs)Enum.Parse(typeof(ObjectManager.ItemIDs), itemValue[i++]);

        //    return i;
        //}
    }
}
