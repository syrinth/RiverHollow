using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Items
{
    public class InventoryItem : Item
    {
        private Parabola _movement;
        protected bool _doesItStack;
        public bool DoesItStack { get => _doesItStack; }

        protected int _num;
        public int Number { get => _num; set => _num = value; }

        public InventoryItem(ObjectManager.ItemIDs ID, Texture2D texture, string name, string description, int number, bool stacks) : base(ID, name, texture, description)
        {
            _doesItStack = stacks;
            _num = number;
        }

        //Copy Constructor
        public InventoryItem(InventoryItem item) : base(item.ItemID, item.Name, item.Texture, item._description)
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
            spriteBatch.Draw(_texture, drawBox, Color.White);
        }

        public void Pop(Vector2 pos, Vector2 vel, int y)
        {
            _position = pos;
            _onTheMap = true;
            _movement = new Parabola(_position, vel, (int)(_position.Y+y));
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
                _finalY = Y;
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
