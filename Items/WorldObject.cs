using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public class WorldObject
    {
        protected float _hp;
        public float HP { get => _hp; }

        protected bool _breakable;
        public bool Breakable { get => _breakable;}

        protected bool _choppable;
        public bool Choppable { get => _choppable; }

        protected Vector2 _position;
        public Vector2 Position { get => _position; }

        protected Rectangle _collisionBox;

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected int _width;
        protected int _height;

        protected int _lvltoDmg;
        public int LvlToDmg { get => _lvltoDmg; }

        protected ObjectManager.ObjectIDs _id;
        public ObjectManager.ObjectIDs ID { get => _id; }

        public WorldObject(ObjectManager.ObjectIDs id, float hp, bool breakIt, bool chopIt, Vector2 pos, Texture2D tex, int lvl, int width, int height)
        {
            _id = id;
            _hp = hp;
            _breakable = breakIt;
            _choppable = chopIt;
            _position = pos;
            _width = width;
            _height = height;
            _texture = tex;
            _lvltoDmg = lvl;

            _collisionBox = new Rectangle((int)Position.X, (int)Position.Y, _width, _height);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X, (int)Position.Y, _width, _height), new Rectangle(0, 0, _width, _height), Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Position.Y + Texture.Height + (Position.X / 100));
        }

        public bool DealDamage(float dmg)
        {
            bool rv = false;
            _hp -= dmg;
            rv = _hp <= 0;
            if (rv)
            {
                MapManager.RemoveWorldObject(this);
                MapManager.DropWorldItems(DropManager.DropItemsFromWorldObject(ID), Position);
                //PlayerManager.Player.AddItemToFirstAvailableInventory(ObjectManager.ItemIDs.Stone);
            }
            return rv;
        }

        public virtual bool IntersectsWith(Rectangle r)
        {
            return _collisionBox.Intersects(r);
        }

        public virtual bool Contains(Point m)
        {
            return _collisionBox.Contains(m);
        }
    }
}
