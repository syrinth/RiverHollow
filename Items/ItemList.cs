using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public static class ItemList
    {
        #region ItemIDs
        public enum ItemIDs
        {
            ARCANE_ESSENCE, COPPER_ORE, COPPER_BAR, IRON_ORE, IRON_BAR, LUMBER
        }
        #endregion

        public static Item GetItem(ItemIDs id, Vector2 position)
        {
            string texturename = "";
            string description = "";
            switch (id)
            {
                case ItemIDs.ARCANE_ESSENCE:
                    texturename = "arcane_essence";
                    description = "arcane_essence";
                    return (position != Vector2.Zero) ? new Item(id, texturename, description) : new WorldItem(id, texturename, description, position);


            }
            return null;
        }
    }
}
