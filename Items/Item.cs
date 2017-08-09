using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class Item
    {
        protected ItemList.ItemIDs _itemID;
        protected string _textureName;
        protected string _description;

        public Item(ItemList.ItemIDs ID, string texName, string description)
        {
            _itemID = ID;
            _textureName = texName;
            _description = description;
        }
    }
}
