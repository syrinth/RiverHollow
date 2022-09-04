using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Seed : Item
    {
        public Seed(int id, Dictionary<string, string> stringData, int num) : base(id, stringData, num) { }

        public override bool HasUse() { return true; }

        public override bool ItemBeingUsed()
        {
            bool rv = false;
            WorldObject obj = MapManager.CurrentMap.TargetTile.WorldObject;
            if (obj != null && obj.Type == ObjectTypeEnum.Garden && InSeason())
            {
                Garden garden = (Garden)obj;
                if (garden.GetPlant() == null)
                {
                    Plant p = (Plant)DataManager.CreateWorldObjectByID(int.Parse(DataManager.GetItemDictionaryKey(_iItemID, "ObjectID")));
                    garden.SetPlant(p);
                    Remove(1);
                    rv = true;
                }
            }

            return rv;
        }

        public bool InSeason() {
            string mySeason = DataManager.GetItemDictionaryKey(_iItemID, "Season");
            return mySeason.Equals(GameCalendar.GetSeason(GameCalendar.CurrentSeason));
        }
    }
}
