using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Items
{
    public class Item
    {
        protected ObjectManager.ItemIDs _itemID;
        public ObjectManager.ItemIDs ItemID { get => _itemID; }

        protected List<KeyValuePair<ObjectManager.ItemIDs, int>> _reagents;
        public List<KeyValuePair<ObjectManager.ItemIDs, int>> Reagents { get => _reagents; }

        protected string _name;
        public string Name { get => _name; }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected Vector2 _sourcePos;

        protected Vector2 _position;
        public Vector2 Position { get => _position; set => _position = value; }

        public virtual Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height); }

        protected bool _onTheMap;
        public bool OnTheMap { get => _onTheMap; set => _onTheMap = value; }

        protected bool _pickup = true;
        public bool Pickup { get => _onTheMap; }

        protected string _description;

        public Item(ObjectManager.ItemIDs ID, Vector2 sourcePos, string name, Texture2D tex, string description) : this(ID, sourcePos, name, tex, description, null)
        {

        }

        public Item(ObjectManager.ItemIDs ID, Vector2 sourcePos, string name, Texture2D tex, string description, List<KeyValuePair<ObjectManager.ItemIDs, int>> reagents)
        {
            _itemID = ID;
            _name = name;
            _sourcePos = sourcePos;
            _texture = tex;
            _description = description;
            _reagents = reagents;
        }

        public ObjectManager.ItemIDs GetItemID()
        {
            return _itemID;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_onTheMap) {
                spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, 32, 32), new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, 32, 32), Color.White);
            }
        }
    }
}
