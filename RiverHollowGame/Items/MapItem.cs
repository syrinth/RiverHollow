using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class MapItem
    {
        public Point _pInitial;
        public Point Position;
        public Rectangle CollisionBox { get => new Rectangle(Position, Constants.BASIC_TILE); }
        private Vector2 _velocity;
        private bool _bBounced = false;
        private readonly VectorBuffer _vbMovement;

        public Item WrappedItem { get; }

        public ItemPickupState PickupState = ItemPickupState.Auto;
        private ItemMovementState _eMovementState = ItemMovementState.None;

        public MapItem(Item i)
        {
            WrappedItem = i;
            _vbMovement = new VectorBuffer();
        }
        public void Update(GameTime gTime)
        {
            if (_eMovementState != ItemMovementState.None)
            {
                if (_eMovementState == ItemMovementState.Bounce)
                {
                    _velocity += new Vector2(0, Constants.ITEM_POP_DECAY);
                    var movement = _vbMovement.AddMovement(_velocity);

                    if ((Position + movement).Y >= _pInitial.Y)
                    {
                        if (!_bBounced)
                        {
                            _bBounced = true;
                            _velocity = new Vector2(0, Constants.ITEM_POP_BOUNCE);
                        }
                        else {
                            _velocity = Vector2.Zero;
                            _eMovementState = ItemMovementState.None;
                        }
                    }
                    else
                    {
                        Position += movement;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, float layerDepth = 0)
        {
            if (layerDepth == 0 || _eMovementState == ItemMovementState.None)
            {
                layerDepth = Util.DetermineLayerDepth(_pInitial, Constants.TILE_SIZE);
            }
            WrappedItem.Draw(spriteBatch, CollisionBox, layerDepth);
        }

        public void SelectObject(bool val)
        {
            if (val)
            {
                WrappedItem.SetColor(Color.Green);
            }
            else
            {
                WrappedItem.SetColor(Color.White);
            }
        }
        public void MoveItem(Vector2 vector)
        {
            if (_eMovementState != ItemMovementState.Bounce)
            {
                _eMovementState = ItemMovementState.Magnet;
                WrappedItem.DrawShadow(false);
                Position += _vbMovement.AddMovement(vector);
            }
        }
        public void Pop(Point pos)
        {
            Position = pos;
            _pInitial = pos;

            _eMovementState = ItemMovementState.Bounce;
            float popHeight = Constants.ITEM_POP_VELOCITY - (RHRandom.Instance().Next(0, Constants.ITEM_POP_VARIANCE) / 10f);
            _velocity = new Vector2(0, popHeight);
        }

        public bool FinishedMoving()
        {
            bool rv = true;

            if (_eMovementState == ItemMovementState.None) { WrappedItem.DrawShadow(true); }
            else if (!_bBounced) { rv = false; }

            return rv;
        }
    }
}
