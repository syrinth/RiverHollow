using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class Item
    {
        private ItemList.ItemIDs _itemID;
        public ItemList.ItemIDs ItemID { get => _itemID; }

        private string _name;
        public string Name { get => _name; }

        protected string _textureName;
        protected string _description;

        public Item(ItemList.ItemIDs ID, string name, string texName, string description)
        {
            _itemID = ID;
            _name = name;
            _textureName = texName;
            _description = description;
        }

        public ItemList.ItemIDs GetItemID()
        {
            return _itemID;
        }
    }
}
