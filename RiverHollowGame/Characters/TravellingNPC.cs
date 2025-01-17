using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public abstract class TravellingNPC : TalkingActor
    {
        static readonly string[] ArrivalReqs = { "GoodsSoldValue", "GoodsSold", "MobsDefeated", "PetCount", "PlantsGrown", "RequiredPopulation" , "ArrivalDay", "RequiredVillager", "ItemID", "RequiredObjectID", "TownScore" };

        protected Dictionary<int, int> _diRequiredObjectIDs;

        protected int MobsDefeated => GetIntByIDKey("MobsDefeated");
        protected int PetCount => GetIntByIDKey("PetCount");
        protected int PlantsGrown => GetIntByIDKey("PlantsGrown");
        protected int GoodsSoldValue => GetIntByIDKey("GoodsSoldValue");
        protected string GoodsSold => GetStringByIDKey("GoodsSold");
        protected int RequiredPopulation => GetIntByIDKey("RequiredPopulation");
        protected int RequiredVillagerID => GetIntByIDKey("RequiredVillager");
        protected int RequiredObjectID => GetIntByIDKey("RequiredObjectID");
        protected int RequiredSpecialItem => GetIntByIDKey("ItemID");
        protected int TownScore => GetIntByIDKey("TownScore");
        protected string ArrivalDay => GetStringByIDKey("ArrivalDay");

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

        protected bool CheckValidate()
        {
            //If they have no triggers. Do not pass go
            if (new List<string>(ArrivalReqs).Find(x => GetBoolByIDKey(x)) == null)
            {
                return false;
            }

            return Validate();
        }

        public bool Validate()
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
                if (!TownManager.Villagers[RequiredVillagerID].LivesInTown)
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

            if (MobsDefeated != -1 && MobsDefeated > TownManager.TotalDefeatedMobs)
            {               
                return false;
            }

            if (PetCount != -1 && PetCount > PlayerManager.Pets.Count)
            {
                return false;
            }

            if (PlantsGrown != -1 && PlantsGrown > TownManager.PlantsGrown)
            {
                return false;
            }

            if (GoodsSoldValue != -1 && GoodsSoldValue > TownManager.ValueGoodsSold)
            {
                return false;
            }

            int score = 0;
            AffinityEnum affinity = AffinityEnum.None;
            if (TownScore != -1 && TownScore > score)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(ArrivalDay))
            {
                var date = Util.FindArguments(ArrivalDay);
                if (Util.ParseEnum<SeasonEnum>(date[0]) != GameCalendar.CurrentSeason || !int.TryParse(date[1], out int day) || day != GameCalendar.CurrentDay)
                {
                    return false;
                }
            }

            if (!string.IsNullOrEmpty(GoodsSold))
            {
                var goodsSoldParams = Util.FindParams(GoodsSold);
                foreach(var good in goodsSoldParams)
                {
                    var data = Util.FindArguments(good);
                    ResourceTypeEnum e = Util.ParseEnum<ResourceTypeEnum>(data[0]);
                    if (int.TryParse(data[1], out int number))
                    {
                        if(!TownManager.CheckSoldGoods(e, number))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        return false;
                    }
                }
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
