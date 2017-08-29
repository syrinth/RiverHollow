using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Items
{
    public class Item
    {
        public enum ItemType {Resource, Weapon, Tool, Container };

        protected ItemType _itemType;
        public ItemType Type { get => _itemType; }
        protected int _itemID;
        public int ItemID { get => _itemID; }

        protected string _name;
        public string Name { get => _name; }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected int _textureIndex;

        protected Vector2 _sourcePos;

        protected Vector2 _position;
        public Vector2 Position { get => _position; set => _position = value; }

        public virtual Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height); }

        protected bool _onTheMap;
        public bool OnTheMap { get => _onTheMap; set => _onTheMap = value; }

        protected bool _pickup = true;
        public bool Pickup { get => _onTheMap; }

        protected string _description;

        public Item() { }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_onTheMap) {
                spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, 32, 32), new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, 32, 32), Color.White);
            }
        }
    }
}
