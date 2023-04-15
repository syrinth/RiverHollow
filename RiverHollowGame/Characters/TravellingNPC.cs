using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public abstract class TravellingNPC : TalkingActor
    {
        static readonly string[] Triggers = { "TotalMoneyEarnedReq", "RequiredPopulation" , "ArrivalDelay", "RequiredVillager", "ItemID", "RequiredObjectID", "FirstArrival" };

        protected Dictionary<int, int> _diRequiredObjectIDs;

        protected int _iNextArrival = -1;
        protected int ArrivalPeriod => GetIntByIDKey("ArrivalPeriod");
        protected int TotalMoneyEarnedNeeded => GetIntByIDKey("TotalMoneyEarnedReq");
        protected int RequiredPopulation => GetIntByIDKey("RequiredPopulation");
        protected int RequiredVillagerID => GetIntByIDKey("RequiredVillager");
        protected int RequiredSpecialItem => GetIntByIDKey("ItemID");

        public virtual RelationShipStatusEnum RelationshipState { get; set; }
        public bool Introduced => RelationshipState != RelationShipStatusEnum.None;

        public TravellingNPC(int index, Dictionary<string, string> stringData) : base(index, stringData)
        {
            _diRequiredObjectIDs = new Dictionary<int, int>();

            _sPortrait = Util.GetPortraitLocation(DataManager.PORTRAIT_FOLDER, "Villager", stringData["Key"]);

            if (stringData.ContainsKey("RequiredObjectID"))
            {
                string[] args = Util.FindParams(stringData["RequiredObjectID"]);
                foreach (string i in args)
                {
                    string[] split = i.Split('-');
                    _diRequiredObjectIDs[int.Parse(split[0])] = int.Parse(split[1]);
                }
            }

            OnTheMap = !stringData.ContainsKey("Inactive");
            _iNextArrival = GetIntByIDKey("ArrivalDelay");
        }

        public override void OpenShop()
        {
            GUIManager.OpenMainObject(new HUDShopWindow(CurrentMap.TheShop.GetUnlockedMerchandise()));
        }

        protected bool CheckArrivalTriggers()
        {
            //If they have no triggers. Do not pass go
            if (new List<string>(Triggers).Find(x => GetBoolByIDKey(x)) == null)
            {
                return false;
            }

            foreach (var kvp in _diRequiredObjectIDs)
            {
                if (TownManager.GetNumberTownObjects(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }

            if (RequiredPopulation != -1)
            {
                if (TownManager.GetPopulation() < RequiredPopulation)
                {
                    return false;
                }
            }
            if (RequiredVillagerID != -1)
            {
                if (!TownManager.DIVillagers[RequiredVillagerID].LivesInTown)
                {
                    return false;
                }
            }

            if (RequiredSpecialItem != -1)
            {
                if (!PlayerManager.AlreadyBoughtUniqueItem(RequiredSpecialItem))
                {
                    return false;
                }
            }
            if (TotalMoneyEarnedNeeded != -1 && TotalMoneyEarnedNeeded > PlayerManager.TotalMoneyEarned)
            {
                return false;
            }

            //This clause ensures that the NPC is validated immediately on the day of, or greater if they should have been added due to an update
            //But is invalid if they are currently counting down to the next arrival.
            if (GameCalendar.GetTotalDays() < GetIntByIDKey("FirstArrival") && _iNextArrival == -1)
            {
                return false;
            }
            else if (_iNextArrival > 0)
            {
                return --_iNextArrival == 0;
            }

            return true;
        }

        /// <summary>
        /// Removes the NPC from the current map and moves them back
        /// to their home map and back to their spawn point.
        /// </summary>
        public virtual void MoveToSpawn() { }
    }
}
