using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Adventure.Items
{
    public class InventoryItem : Item
    {
        private Parabola _movement;
        protected bool _doesItStack;
        public bool DoesItStack { get => _doesItStack; }

        protected int _num;
        public int Number { get => _num; set => _num = value; }

        public InventoryItem(ObjectManager.ItemIDs ID, Vector2 sourcePos, Texture2D texture, string name, string description, int number, bool stacks) : this(ID, sourcePos, texture, name, description, number, stacks, null)
        { }

        public InventoryItem(ObjectManager.ItemIDs ID, Vector2 sourcePos, Texture2D texture, string name, string description, int number, bool stacks, List<KeyValuePair<ObjectManager.ItemIDs, int>> reagents) : base(ID, sourcePos, name, texture, description, reagents)
        {
            _doesItStack = stacks;
            _num = number;
        }

        //Copy Constructor
        public InventoryItem(InventoryItem item) : base(item.ItemID, item._sourcePos, item.Name, item.Texture, item._description)
        {
            _num = item.Number;
            _doesItStack = item.DoesItStack;
        }
        public void Update()
        {
            if (_movement != null) {
                if (!_movement.Finished)
                {
                    _position = _movement.MoveTo();
                    _movement.Update();
                }
                else{
                    _movement = null;
                }
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

        public bool Finished()
        {
            bool rv = true;

            if(_movement != null && !_movement.Finished)
            {
                rv = false;
            }
            return rv;
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
                if( rv < minExclude || rv > maxExclude)
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
                if(_pos.Y >= _finalY)
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
