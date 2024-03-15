using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class MapItem
    {
        public Point Position;
        public Rectangle CollisionBox { get => new Rectangle(Position, Constants.TileSize); }
        private readonly VectorBuffer _vbMovement;

        public Item WrappedItem { get; }

        public ItemPickupState PickupState = ItemPickupState.Auto;

        private Parabola _movement;

        public MapItem(Item i)
        {
            WrappedItem = i;
            _vbMovement = new VectorBuffer();
        }
        public void Update(GameTime gTime)
        {
            if (_movement != null)
            {
                if (!_movement.Finished)
                {
                    Position = _movement.MoveTo().ToPoint();
                    _movement.Update(gTime);
                }
                else
                {
                    _movement = null;
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, float layerDepth = 0)
        {
            if(layerDepth == 0)
            {
                layerDepth = Position.Y + Constants.TILE_SIZE + (Position.X / 100);
            }
            WrappedItem.Draw(spriteBatch, CollisionBox, layerDepth == 0, layerDepth);
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
            WrappedItem.DrawShadow(false);
            Position += _vbMovement.AddMovement(vector);
        }
        public void Pop(Point pos)
        {
            Position = pos;
            _movement = new Parabola(Position.ToVector2());
        }

        public bool FinishedMoving()
        {
            bool rv = true;

            if (_movement != null && !_movement.Finished)
            {
                rv = false;
            }
            else
            {
                WrappedItem.DrawShadow(true);
            }
            return rv;
        }
    }

    public class Parabola
    {
        readonly double _dCurveDegree = -0.1;
        readonly double _dCurveWidth = 2.8;

        Vector2 _vPosition;
        Vector2 _vStartPosition;
        readonly VectorBuffer _vbMovement;

        public bool Finished { get; private set; } = false;

        readonly RHTimer _timer;
        double _dPauseTimer = 0.1;
        int _iXOffset = 0;
        readonly bool _bGoLeft = false;
        readonly bool _bBounce = true;

        Parabola subBounce;
        public Parabola(Vector2 pos)
        {
            _vbMovement = new VectorBuffer();
            RHRandom r = RHRandom.Instance();

            float widthChange = r.Next(0, 5);
            widthChange *= 0.1f;
            if (r.Next(0, 1) == 1) { widthChange *= -1; }
            _dCurveWidth += widthChange;
            if (r.Next(0, 1) == 1)
            {
                _bGoLeft = true;
                _dCurveWidth *= -1;
            }

            double timerChange = r.Next(0, 9);
            timerChange *= 0.01;
            if (r.Next(0, 1) == 1) { timerChange *= -1; }
            _timer = new RHTimer(Constants.ITEM_BOUNCE_SPEED + timerChange);

            _vStartPosition = pos;
            _vPosition = pos;
        }

        public Parabola(Vector2 pos, double curveDegree, double curveWidth, bool goingLeft)
        {
            _timer = new RHTimer(Constants.ITEM_BOUNCE_SPEED);
            _bBounce = false;
            _vStartPosition = pos;
            _vPosition = pos;
            _dCurveDegree = curveDegree;
            _dCurveWidth = curveWidth;
            _bGoLeft = goingLeft;

            _vbMovement = new VectorBuffer();
        }

        public void Update(GameTime gTime)
        {
            if (_timer.TickDown(gTime))
            {
                if (_bBounce || (_bBounce && subBounce != null))
                {
                    if (subBounce == null)
                    {
                        subBounce = new Parabola(_vPosition, _dCurveDegree + 0.04, _dCurveWidth + (_bGoLeft ? 1.5 : -1.5), _bGoLeft);
                    }
                    else if (!subBounce.Finished)
                    {
                        subBounce.Update(gTime);
                    }
                    else { Finished = true; }
                }
                else
                {
                    if (_dPauseTimer > 0)
                    {
                        _dPauseTimer -= gTime.ElapsedGameTime.TotalSeconds;
                    }
                    else
                    {
                        Finished = true;
                    }
                }
            }
            else
            {
                _iXOffset += _bGoLeft ? -2 : 2;

                float yOffset = (float)((_dCurveDegree * (_iXOffset * _iXOffset)) + (_dCurveWidth * _iXOffset));

                _vPosition.X = _vStartPosition.X + _iXOffset;
                _vPosition.Y = _vStartPosition.Y - yOffset;
            }
        }

        public Vector2 MoveTo()
        {
            if (subBounce != null)
            {
                return subBounce.MoveTo();
            }
            else
            {
                return _vPosition;
            }
        }
    }
}
