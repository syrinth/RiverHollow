using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public static class ItemList
    {
        private static GameContentManager _gcManager = GameContentManager.GetInstance();
        #region ItemIDs
        public enum ItemIDs
        {
            NOTHING, ARCANE_ESSENCE, COPPER_ORE, COPPER_BAR, IRON_ORE, IRON_BAR, LUMBER
        }

        private static Dictionary<ItemIDs, Texture2D> _texturePairings;
        #endregion

        public static InventoryItem GetItem(ItemIDs id)
        {
            string name = "";
            string description = "";
            switch (id)
            {
                case ItemIDs.ARCANE_ESSENCE:
                    name = "Arcane Essence";
                    description = "arcane_essence";
                    return new InventoryItem(id, _texturePairings[ItemIDs.ARCANE_ESSENCE], name, description, 1, true);
            }
            return null;
        }

        public static void LoadContent()
        {
            _texturePairings = new Dictionary<ItemIDs, Texture2D>();
            _texturePairings.Add(ItemIDs.ARCANE_ESSENCE, _gcManager.GetTexture(@"Textures\arcane_essence"));

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
