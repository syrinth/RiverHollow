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
        protected int _iTotalMoneyEarnedReq = -1;

        protected int _iArrivalPeriod = -1;
        protected int _iNextArrival = -1;

        protected Dictionary<int, int> _diRequiredObjectIDs;
        protected int _iRequiredPopulation = -1;
        protected int _iRequiredVillagerID = -1;

        public virtual RelationShipStatusEnum RelationshipState { get; set; }
        public bool Introduced => RelationshipState != RelationShipStatusEnum.None;
        protected bool _bArrivedOnce = false;

        public TravellingNPC(int id) : base(id) { }

        protected virtual void ImportBasics(Dictionary<string, string> stringData, bool loadanimations = true)
        {
            Util.AssignValue(ref _bHover, "Hover", stringData);

            _diDialogue = DataManager.GetNPCDialogue(stringData["Key"]);

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
            Util.AssignValue(ref _iRequiredPopulation, "RequiredPopulation", stringData);
            Util.AssignValue(ref _iRequiredVillagerID, "RequiredVillager", stringData);

            if (loadanimations)
            {
                List<AnimationData> liAnimationData = Util.LoadWorldAnimations(stringData);

                LoadSpriteAnimations(ref _sprBody, liAnimationData, DataManager.NPC_FOLDER + "NPC_" + stringData["Key"]);
                PlayAnimationVerb(VerbEnum.Idle);
            }

            _bOnTheMap = !stringData.ContainsKey("Inactive");


            int arrivalDelay = -1;
            Util.AssignValue(ref arrivalDelay, "FirstArrival", stringData);
            Util.AssignValue(ref _iArrivalPeriod, "ArrivalPeriod", stringData);

            _iNextArrival = arrivalDelay;
        }

        public virtual void RollOver() { }

        public override void OpenShop()
        {
            GUIManager.OpenMainObject(new HUDShopWindow(CurrentMap.TheShop.GetUnlockedMerchandise()));
        }

        protected bool TownRequirementsMet()
        {
            foreach (KeyValuePair<int, int> kvp in _diRequiredObjectIDs)
            {
                if (PlayerManager.GetNumberTownObjects(kvp.Key) < kvp.Value)
                {
                    return false;
                }
            }

            if(_iRequiredPopulation != -1)
            {
                int livesintown = 0;
                foreach(Villager v in DataManager.DIVillagers.Values)
                {
                    if (v.LivesInTown) { livesintown++; }
                }

                if(livesintown < _iRequiredPopulation)
                {
                    return false;
                }
            }
            if (_iRequiredVillagerID != -1)
            {
                if (!DataManager.DIVillagers[_iRequiredVillagerID].LivesInTown)
                {
                    return false;
                }
            }

            //If there is a Money Earned Requirement and we have not reached it, fail the test
            if (_iTotalMoneyEarnedReq != -1 && _iTotalMoneyEarnedReq < PlayerManager.TotalMoneyEarned)
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
