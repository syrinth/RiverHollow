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
            NOTHING, ARCANE_ESSENCE, COPPER_ORE, COPPER_BAR, IRON_ORE, IRON_BAR, LUMBER
        }
        #endregion

        public static InventoryItem GetItem(ItemIDs id)
        {
            string name = "";
            string texturename = "";
            string description = "";
            switch (id)
            {
                case ItemIDs.ARCANE_ESSENCE:
                    name = "Arcane Essence";
                    texturename = "arcane_essence";
                    description = "arcane_essence";
                    return new InventoryItem(id, texturename, name, description, true);
            }
            return null;
        }
        // new WorldItem(id, texturename, description, position)
        /*public static InventoryItem GetInventoryItem(ItemIDs id, Vector2 position)
        {
            string texturename = "";
            string description = "";
            switch (id)
            {
                case ItemIDs.ARCANE_ESSENCE:
                    texturename = "arcane_essence";
                    description = "arcane_essence";
                    return new InventoryItem(id, texturename, description);
            }
            return null;
        }*/
    }
}
