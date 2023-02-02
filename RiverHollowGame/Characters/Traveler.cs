using Newtonsoft.Json.Linq;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Traveler : TalkingActor
    {
        public int BuildingID()
        {
            return DataManager.GetIntByIDKey(ID, "Building", DataType.NPC);
        }

        private int NPC()
        {
            return DataManager.GetIntByIDKey(ID, "NPC", DataType.NPC);
        }

        private int Value()
        {
            return DataManager.GetIntByIDKey(ID, "Value", DataType.NPC);
        }

        public bool Rare()
        {
            return DataManager.GetBoolByIDKey(ID, "Rare", DataType.NPC);
        }

        public TravelerGroupEnum Group => DataManager.GetEnumByIDKey<TravelerGroupEnum>(ID, "Subtype", DataType.NPC);

        public Traveler(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            ActorType = WorldActorTypeEnum.Traveler;
            _bCanWander = true;
        }

        public bool Validate()
        {
            return (BuildingID() == -1 || TownManager.TownObjectBuilt(BuildingID())) &&
                        (NPC() == -1 || TownManager.DIVillagers[NPC()].LivesInTown);
        }

        public int CalculateValue()
        {
            Building b = TownManager.GetBuildingByID(BuildingID());
            return (int)(Value() * ( 1 + b.GetShopValueModifier()));
        }
    }
}
