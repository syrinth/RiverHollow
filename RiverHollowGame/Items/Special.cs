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
        public Special(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Special");
        }

        public override bool AddToInventoryTrigger()
        {
            base.AddToInventoryTrigger();
            if (GetBoolByIDKey("DungeonKey"))
            {
                DungeonManager.AddDungeonKey();
                return true;
            }
            else if (GetBoolByIDKey("Increase"))
            {
                PlayerManager.ObtainedItem = new MapItem(this)
                {
                    Position = PlayerManager.PlayerActor.Position + new Point(0, -(Constants.TILE_SIZE - 9))
                };

                GUIManager.OpenTextWindow("Found_Increaser", Name(), GetStringByIDKey("Increase"));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
