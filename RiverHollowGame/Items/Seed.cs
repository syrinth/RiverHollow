using RiverHollow.Game_Managers;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Seed : Item
    {
        public Seed(int id, Dictionary<string, string> stringData, int num) : base(id, stringData, num)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Seeds");
        }

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
                    Plant p = (Plant)DataManager.CreateWorldObjectByID(int.Parse(DataManager.GetItemDictionaryKey(ID, "ObjectID")));
                    garden.SetPlant(p);
                    Remove(1);
                    rv = true;
                }
            }

            return rv;
        }

        public bool InSeason() {
            string mySeason = DataManager.GetItemDictionaryKey(ID, "Season");
            return mySeason.Equals(GameCalendar.GetSeason(GameCalendar.CurrentSeason));
        }
    }
}
