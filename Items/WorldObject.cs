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
        private float _hp;
        public float HP { get => _hp; }

        private bool _breakable;
        public bool Breakable { get => _breakable;}

        private bool _choppable;
        public bool Choppable { get => _choppable; }

        private Vector2 _position;
        public Vector2 Position { get => _position; }

        private int _colWidth;
        public int CollisionWidth { get => _colWidth; }

        private int _colHeight;
        public int CollisionHeight { get => _colHeight; }

        private Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        private int _lvltoDmg;
        public int LvlToDmg { get => _lvltoDmg; }

        private ObjectManager.ObjectIDs _id;
        public ObjectManager.ObjectIDs ID { get => _id; }

        public Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, CollisionWidth, CollisionHeight); }

        public WorldObject(ObjectManager.ObjectIDs id, float hp, bool breakIt, bool chopIt, Vector2 pos, Texture2D tex, int lvl, int width, int height)
        {
            _id = id;
            _hp = hp;
            _breakable = breakIt;
            _choppable = chopIt;
            _position = pos;
            _texture = tex;
            _lvltoDmg = lvl;
            _colWidth = width;
            _colHeight = height;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, CollisionBox, Color.White);
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
    }
}
