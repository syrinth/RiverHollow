using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public abstract class TravellingNPC : TalkingActor
    {
        protected int _iNextArrival = -1;
        protected int ArrivalPeriod => DataManager.GetIntByIDKey(ID, "ArrivalPeriod", DataType.NPC);
        protected int TotalMoneyEarnedNeeded => DataManager.GetIntByIDKey(ID, "TotalMoneyEarnedReq", DataType.NPC);

        protected Dictionary<int, int> _diRequiredObjectIDs;
        protected int RequiredPopulation => DataManager.GetIntByIDKey(ID, "RequiredPopulation", DataType.NPC);
        protected int RequiredVillagerID => DataManager.GetIntByIDKey(ID, "RequiredVillager", DataType.NPC);

        public virtual RelationShipStatusEnum RelationshipState { get; set; }
        public bool Introduced => RelationshipState != RelationShipStatusEnum.None;
        protected bool _bArrivedOnce = false;

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

            int arrivalDelay = Util.AssignValue("FirstArrival", stringData);
            _iNextArrival = arrivalDelay;
        }

        public override void OpenShop()
        {
            GUIManager.OpenMainObject(new HUDShopWindow(CurrentMap.TheShop.GetUnlockedMerchandise()));
        }

        protected bool TownRequirementsMet()
        {
            foreach (KeyValuePair<int, int> kvp in _diRequiredObjectIDs)
            {
                if (TownManager.GetNumberTownObjects(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }

            if(RequiredPopulation != -1)
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

            //If there is a Money Earned Requirement and we have not reached it, fail the test
            if (TotalMoneyEarnedNeeded != -1 && TotalMoneyEarnedNeeded < PlayerManager.TotalMoneyEarned)
            {
                return false;
            }

            return true;
        }

        #region Travel Methods
        /// <summary>
        /// Counts down the days until the Villager's first arrival, or
        /// time to the next arrival if they do not live in town.
        /// </summary>
        /// <returns></returns>
        public virtual bool HandleTravelTiming()
        {
            bool rv = false;

            if (_iNextArrival > 0)
            {
                rv = TravelTimingHelper(ref _iNextArrival);
            }

            return rv;
        }

        /// <summary>
        /// Given a timer, subtract one from the elapsed time and, if it has become 0
        /// reset the time to next arrival.
        /// </summary>
        /// <param name="arrivalPeriod"></param>
        /// <returns></returns>
        private bool TravelTimingHelper(ref int arrivalPeriod)
        {
            bool rv = --arrivalPeriod == 0;
            if (rv)
            {
                _iNextArrival = 0;
            }

            return rv;
        }

        /// <summary>
        /// Removes the NPC from the current map and moves them back
        /// to their home map and back to their spawn point.
        /// </summary>
        public virtual void MoveToSpawn() { }
        #endregion
    }
}
