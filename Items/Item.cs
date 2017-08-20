using Adventure.Game_Managers;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class Item
    {
        protected ObjectManager.ItemIDs _itemID;
        public ObjectManager.ItemIDs ItemID { get => _itemID; }

        protected string _name;
        public string Name { get => _name; }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }
        protected string _description;

        public Item(ObjectManager.ItemIDs ID, string name, Texture2D tex, string description)
        {
            _itemID = ID;
            _name = name;
            _texture = tex;
            _description = description;
        }

        public ObjectManager.ItemIDs GetItemID()
        {
            return _itemID;
        }
    }
}
