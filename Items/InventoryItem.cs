using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class InventoryItem : Item
    {
        public InventoryItem(ItemList.ItemIDs ID, string texture, string description) : base(ID, texture, description)
        {
        }
    }
}
