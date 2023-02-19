using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Special : Item
    {
        public Special(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Special");
        }

        public bool HasSpecialTag(string specialKey)
        {
            return GetBoolByIDKey(specialKey);
        }

        public override bool AddToInventoryTrigger()
        {
            if (HasSpecialTag("DungeonKey"))
            {
                DungeonManager.AddDungeonKey();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
