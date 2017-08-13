using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class Item
    {
        private ItemList.ItemIDs _itemID;
        public ItemList.ItemIDs ItemID { get => _itemID; }

        private string _name;
        public string Name { get => _name; }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }
        protected string _description;

        public Item(ItemList.ItemIDs ID, string name, Texture2D tex, string description)
        {
            _itemID = ID;
            _name = name;
            _texture = tex;
            _description = description;
        }

        public ItemList.ItemIDs GetItemID()
        {
            return _itemID;
        }
    }
}
