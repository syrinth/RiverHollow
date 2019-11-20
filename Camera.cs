using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Actors;
using RiverHollow.Misc;

namespace RiverHollow
{
    public static class Camera
    {
        static bool _bMoving = false;
        static Vector2 _vObserver;
        static WorldActor _actObserver;

        public static Matrix _transform;
        public static Viewport _view;
        public static Vector2 _center;

        public static void SetViewport(Viewport view)
        {
            //MAR
            //_view = view;
        }

        public static void Update(GameTime gTime)
        {
            Vector2 target = _actObserver.CharCenter.ToVector2() * Scale;
            if (!TakingInput())
            {
                if (!Scrying())
                {
                    //This code is used to get the camera to move in to the target
                    if (_bMoving)
                    {
                        Vector2 direction = Vector2.Zero;
                        Util.GetMoveSpeed(_vObserver, target, 18, ref direction);
                        _vObserver += direction;

                        if (_vObserver == target) {
                            _bMoving = false;
                        }
                    }
                    else { _vObserver = target; }
                }
                else
                {
                    KeyboardState ks = Keyboard.GetState();
                    int speed = 10;
                    if (ks.IsKeyDown(Keys.W))
                    {
                        _vObserver += new Vector2(0, -speed);
                    }
                    else if (ks.IsKeyDown(Keys.S))
                    {
                        _vObserver += new Vector2(0, speed);
                    }

                    if (ks.IsKeyDown(Keys.A))
                    {
                        _vObserver += new Vector2(-speed, 0);
                    }
                    else if (ks.IsKeyDown(Keys.D))
                    {
                        _vObserver += new Vector2(speed, 0);
                    }
                }
            }

            float BorderOffset = TileSize * Scale;
            bool xLocked = false;
            bool yLocked = false;

            //Checks if the given map width is smaller than the screen width.
            //If so, lock it so that we don't move
            if (MapManager.CurrentMap.GetMapWidth() < RiverHollow.ScreenWidth)
            {
                xLocked = true;
                _vObserver.X = (MapManager.CurrentMap.GetMapWidth() / 2);
            }

            //Checks if the given map is smaller than the screen. If so, lock it so we don't move.
            if (MapManager.CurrentMap.GetMapHeight() < RiverHollow.ScreenHeight)
            {
                yLocked = true;
                _vObserver.Y = (MapManager.CurrentMap.GetMapHeight() / 2);
            }

            if (!xLocked)
            {
                if (_vObserver.X <= (RiverHollow.ScreenWidth / 2))
                {
                    _vObserver.X = (RiverHollow.ScreenWidth / 2);
                    if (_vObserver.Y == target.Y) { _bMoving = false; }
                }
                else if (_vObserver.X >= MapManager.CurrentMap.GetMapWidth() - (RiverHollow.ScreenWidth / 2))
                {
                    _vObserver.X = MapManager.CurrentMap.GetMapWidth() - (RiverHollow.ScreenWidth / 2);
                    if(_vObserver.Y == target.Y) { _bMoving = false; }
                }
            }

            if (!yLocked)
            {
                if (_vObserver.Y <= (RiverHollow.ScreenHeight / 2) + BorderOffset)
                {
                    _vObserver.Y = (RiverHollow.ScreenHeight / 2) + BorderOffset;
                    if(_vObserver.X == target.X) { _bMoving = false; }
                }
                else if (_vObserver.Y >= MapManager.CurrentMap.GetMapHeight() - (RiverHollow.ScreenHeight / 2) - BorderOffset)
                {
                    _vObserver.Y = MapManager.CurrentMap.GetMapHeight() - (RiverHollow.ScreenHeight / 2) - BorderOffset;
                    if (_vObserver.X == target.X) { _bMoving = false; }
                }
            }

            if (xLocked && yLocked) { _bMoving = false; }                       //If the entirety of the map is smaller than the screen, don't move the camera at all.
            if (xLocked && _vObserver.Y == target.Y) { _bMoving = false; }      //If the x axis is locked, stop moving when we reach the correct y
            if (yLocked && _vObserver.X == target.X) { _bMoving = false; }      //If the y axis is locked, stop moving when we reach the correct x

            _center = new Vector2(_vObserver.X - (RiverHollow.ScreenWidth / 2), _vObserver.Y - (RiverHollow.ScreenHeight / 2));
            _transform = Matrix.CreateScale(new Vector3(Scale, Scale, 0)) * Matrix.CreateTranslation(new Vector3(-_center.X, -_center.Y, 0));
        }

        public static void SetObserver(WorldActor act, bool swoopToTarget = false)
        {
            _actObserver = act;
            _bMoving = swoopToTarget;
        }

        public static void UnsetObserver()
        {
            _vObserver = new Vector2(MapManager.CurrentMap.MapWidthTiles / 2, MapManager.CurrentMap.MapHeightTiles / 2);
        }

        public static void ResetObserver()
        {
            _vObserver = _actObserver.CharCenter.ToVector2();
        }

        public static bool IsMoving()
        {
            return _bMoving;
        }
    }
}
