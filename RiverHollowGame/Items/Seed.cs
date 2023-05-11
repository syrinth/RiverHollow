using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Items
{
    public class Seed : Item
    {
        int _iObjectID;
        public Seed(int id, Dictionary<string, string> stringData, int num) : base(id, stringData, num)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Seeds");
            _iObjectID = int.Parse(stringData["ObjectID"]);
        }

        public override bool HasUse() { return true; }
        public override bool ItemBeingUsed()
        {
            if (GameManager.TownModeEdit())
            {
                GameManager.PickUpWorldObject(DataManager.CreateWorldObjectByID(_iObjectID));

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
