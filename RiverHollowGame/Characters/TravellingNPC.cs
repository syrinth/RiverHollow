using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public abstract class TravellingNPC : TalkingActor
    {
        static readonly string[] ArrivalReqs = { "TotalMoneyEarnedReq", "RequiredPopulation" , "ArrivalDelay", "RequiredVillager", "ItemID", "RequiredObjectID" };

        protected Dictionary<int, int> _diRequiredObjectIDs;

        protected int TotalMoneyEarnedNeeded => GetIntByIDKey("TotalMoneyEarnedReq");
        protected int RequiredPopulation => GetIntByIDKey("RequiredPopulation");
        protected int RequiredVillagerID => GetIntByIDKey("RequiredVillager");
        protected int RequiredObjectID => GetIntByIDKey("RequiredObjectID");
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
                    string[] split = Util.FindArguments(i);
                    _diRequiredObjectIDs[int.Parse(split[0])] = split.Length > 1 ? int.Parse(split[1]) : 1;
                }
            }

            OnTheMap = !stringData.ContainsKey("Inactive");
        }

        public override void OpenShop()
        {
            GUIManager.OpenMainObject(new HUDShopWindow(CurrentMap.TheShop.GetUnlockedMerchandise()));
        }

        protected bool CheckArrivalTriggers()
        {
            //If they have no triggers. Do not pass go
            if (new List<string>(ArrivalReqs).Find(x => GetBoolByIDKey(x)) == null)
            {
                return false;
            }

            return CheckTriggers();
        }

        public bool CheckTriggers()
        {
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

            return true;
        }

        /// <summary>
        /// Removes the NPC from the current map and moves them back
        /// to their home map and back to their spawn point.
        /// </summary>
        public virtual void MoveToSpawn() { }
    }
}
