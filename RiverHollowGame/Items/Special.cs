using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Special : Item
    {
        public Special(int id) : base(id, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Special");
        }

        public override bool AddToInventoryTrigger()
        {
            base.AddToInventoryTrigger();
            TownManager.AddToArchive(ID);

            if (GetBoolByIDKey("DungeonKey"))
            {
                DungeonManager.AddDungeonKey();
                return true;
            }
            else if (GetBoolByIDKey("Increase"))
            {
                StrikeAPose();
                return true;
            }
            else
            {
                PlayerManager.AddUniqueItemToList(ID);
                return false;
            }
        }
    }
}
