using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

namespace RiverHollow.Items
{
    public class Seed : Item
    {
        public int PlantID => GetIntByIDKey("ObjectID");
        public Seed(int id, int num) : base(id, num)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Seeds");
        }

        public override bool HasUse() { return true; }
        public override bool ItemBeingUsed()
        {
            GameManager.EnterTownModeEdit();

            if (MapManager.CurrentMap == MapManager.TownMap || DataManager.GetBoolByIDKey(PlantID, "NoWater", Enums.DataType.WorldObject))
            {
                if (Number > 1) { Remove(1); }
                else { InventoryManager.RemoveItemFromInventory(this); }

                GameManager.PickUpWorldObject(DataManager.CreateWorldObjectByID(PlantID));

                return true;
            }

            return false;
        }

        public bool InSeason() {
            string mySeason = DataManager.GetItemDictionaryKey(ID, "Season");
            return mySeason.Equals(GameCalendar.GetCurrentSeason());
        }
    }
}
