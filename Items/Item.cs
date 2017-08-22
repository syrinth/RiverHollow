using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
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

        protected Vector2 _position;
        public Vector2 Position { get => _position; set => _position = value; }

        public virtual Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height); }

        protected bool _onTheMap;
        public bool OnTheMap { get => _onTheMap; set => _onTheMap = value; }

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

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_onTheMap) {
                spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, _texture.Width, _texture.Height), Color.White);
            }
        }
    }
}
