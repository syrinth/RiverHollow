using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Adventure.Items
{
    public class Item
    {
        public enum ItemType {Resource, Weapon, Tool, Container, Food };

        #region properties
        protected ItemType _itemType;
        public ItemType Type { get => _itemType; }
        protected int _itemID;
        public int ItemID { get => _itemID; }

        protected string _name;
        public string Name { get => _name; }

        protected int _textureIndex;
        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected Vector2 _sourcePos;

        protected Vector2 _position;
        public Vector2 Position { get => _position; set => _position = value; }

        public virtual Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height); }
        public Rectangle SourceRectangle { get => new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, 32, 32); }

        protected bool _onTheMap;
        public bool OnTheMap { get => _onTheMap; set => _onTheMap = value; }

        protected bool _pickup = true;
        public bool Pickup { get => _onTheMap; }

        protected string _description;

        protected int _columnTextureSize = 32;
        protected int _rowTextureSize = 32;
        private Parabola _movement;
        protected bool _doesItStack;
        public bool DoesItStack { get => _doesItStack; }

        protected int _num;
        public int Number { get => _num; set => _num = value; }

        protected int _sellPrice;
        public int SellPrice { get => _sellPrice; }
        #endregion
        public Item() { }

        public Item(int id, string[] itemValue, int num)
        {
            if (itemValue.Length == 6)
            {
                ImportBasics(itemValue, id, num);

                _doesItStack = true;
                _texture = GameContentManager.GetTexture(@"Textures\items");

                CalculateSourcePos();
            }
        }

        protected void CalculateSourcePos()
        {
            int textureRows = (_texture.Height / _rowTextureSize);
            int textureColumns = (_texture.Width / _columnTextureSize);

            if (textureRows == 0) textureRows = 1;
            if (textureColumns == 0) textureColumns = 1;

            int targetRow = _textureIndex / textureColumns;
            int targetCol = _textureIndex % textureColumns;

            _sourcePos = new Vector2(0 + 32 * targetCol, 0 + 32 * targetRow);
        }
        protected int ImportBasics(string[] itemValue, int id, int num)
        {
            _num = num;

            int i = 0;
            _itemType = (ItemType)Enum.Parse(typeof(ItemType), itemValue[i++]);
            _name = itemValue[i++];
            _description = itemValue[i++];
            _textureIndex = int.Parse(itemValue[i++]);
            i++; //holding out for an enum
            _sellPrice = int.Parse(itemValue[i++]);

            _itemID = id;//(ObjectManager.ItemIDs)Enum.Parse(typeof(ObjectManager.ItemIDs), itemValue[i++]);

            return i;
        }
        //Copy Constructor
        public Item(Item item)
        {
            _itemID = item.ItemID;
            _sourcePos = item._sourcePos;
            _name = item.Name;
            _texture = item.Texture;
            _description = item._description;
            _num = item.Number;
            _doesItStack = item.DoesItStack;
        }

        public void Update()
        {
            if (_movement != null)
            {
                if (!_movement.Finished)
                {
                    _position = _movement.MoveTo();
                    _movement.Update();
                }
                else
                {
                    _movement = null;
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_onTheMap)
            {
                spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, 32, 32), SourceRectangle, Color.White);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            spriteBatch.Draw(_texture, drawBox, new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, 32, 32), Color.White);
        }

        public void Pop(Vector2 pos)
        {

            _position = pos;
            _onTheMap = true;
            // new Vector2(1, -5), 10
            _movement = new Parabola(_position, RandomVelocityVector(), RandNumber(8, 32, 0, 0));
        }

        public bool FinishedMoving()
        {
            bool rv = true;

            if (_movement != null && !_movement.Finished)
            {
                rv = false;
            }
            return rv;
        }

        public void Remove(int x)
        {
            if (x >= _num)
            {
                _num -= x;
            }
        }

        public Vector2 RandomVelocityVector()
        {
            return new Vector2(RandNumber(-3, 3, -1, 1), RandNumber(-5, -2, 0, 0));
        }

        public int RandNumber(int minValue, int maxValue, int minExclude, int maxExclude)
        {
            int rv = 0;
            Random r = new Random();
            bool found = false;
            while (!found)
            {
                rv = r.Next(minValue, maxValue);
                if (rv < minExclude || rv > maxExclude)
                {
                    found = true;
                }
            }

            return rv;
        }

        private class Parabola
        {
            private Vector2 _pos;
            private Vector2 _vel;
            public Vector2 Velocity { get => _vel; }
            private int _finalY;
            private bool _finished = false;
            public bool Finished { get => _finished; }
            public Parabola(Vector2 pos, Vector2 velocity, int Y)
            {
                _pos = pos;
                _vel = velocity;
                _finalY = (int)_pos.Y + Y;
            }

            public void Update()
            {
                _vel.Y += 0.2f;
                if (_pos.Y >= _finalY)
                {
                    _vel.Y = 0;
                    _finished = true;
                }
            }

            public Vector2 MoveTo()
            {
                return _pos += _vel;
            }
        }
    }
}
