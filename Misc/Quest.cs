using RiverHollow.Characters;
using RiverHollow.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Misc
{
    public class Quest
    {
        public enum QuestGoalType { GroupSlay, Slay, Fetch }
        private QuestGoalType _goalType;
        private string _name;
        public string Name { get => _name; }
        private string _description;
        public string Description { get => _description; }
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
        //private List<int> _rewardItems;

        public Quest(string name, QuestGoalType type, string desc, NPC giver, int target, Monster m, Item i)
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

        public bool AttemptProgress(Object o)
        {
            bool progress = false;
            int num = 0;
            if (o.GetType().Equals(typeof(Monster)))
            {
                if (_questMob != null && _questMob.ID == ((Monster)o).ID)
                {
                    num = 1;
                    progress = true;
                }
            }
            else if (o.GetType().Equals(typeof(Item)))
            {
                if (_questItem != null && _questItem.ItemID == ((Item)o).ItemID)
                {
                    num = ((Item)o).Number;
                    progress = true;
                }
            }
            if (progress)
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

            return progress;
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
