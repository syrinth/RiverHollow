
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Adventure.Items
{
    public class WorldItem : Item
    {
        Vector2 _position;

        public WorldItem(ItemList.ItemIDs ID, string texture, string description, Vector2 position) : base(ID, texture, description)
        {
            _position = position;
        }
    }
}
