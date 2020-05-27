using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Actors;
using RiverHollow.Misc;
using System;

namespace RiverHollow
{
    public static class Camera
    {
        static bool _bTrackToTarget = false;
        static Vector2 _vObserver;
        static WorldActor _actObserver; //The WorldActor the Camera needs to be following

        public static Matrix _transform;
        public static Viewport _view;
        public static Vector2 _vCenter;

        public static void SetViewport(Viewport view)
        {
            //MAR
            //_view = view;
        }

        public static void Update(GameTime gTime)
        {
            Vector2 target = _actObserver.CharCenter.ToVector2() * Scale;

            if (!TakingInput() && IsRunning())
            {
                KeyboardState ks = Keyboard.GetState();
                int speed = 10;
                if (ks.IsKeyDown(Keys.W)) { target += new Vector2(0, -speed); }
                else if (ks.IsKeyDown(Keys.S)) { target += new Vector2(0, speed); }

                if (ks.IsKeyDown(Keys.A)) { target += new Vector2(-speed, 0); }
                else if (ks.IsKeyDown(Keys.D)) { target += new Vector2(speed, 0); }
            }

            if (target.X <= (RiverHollow.ScreenWidth / 2)) { target.X = (RiverHollow.ScreenWidth / 2); }
            else if (target.X >= MapManager.CurrentMap.GetMapWidth() - (RiverHollow.ScreenWidth / 2)) { target.X = MapManager.CurrentMap.GetMapWidth() - (RiverHollow.ScreenWidth / 2); }
            if (target.Y <= (RiverHollow.ScreenHeight / 2)) { target.Y = (RiverHollow.ScreenHeight / 2); }
            else if (target.Y >= MapManager.CurrentMap.GetMapHeight() - (RiverHollow.ScreenHeight / 2)) { target.Y = MapManager.CurrentMap.GetMapHeight() - (RiverHollow.ScreenHeight / 2); }

            if (MapManager.CurrentMap.GetMapWidth() / TileSize <= Math.Ceiling((double)RiverHollow.ScreenWidth / TileSize)) { target.X = (MapManager.CurrentMap.GetMapWidth() / 2); }

            double val = Math.Ceiling((double)RiverHollow.ScreenHeight / TileSize);
            if (MapManager.CurrentMap.GetMapHeight() / TileSize <= val) { target.Y = (MapManager.CurrentMap.GetMapHeight() / 2); }

            if (!Scrying())
            {
                //We are moving to the target
                if (_bTrackToTarget)
                {
                    Vector2 direction = Vector2.Zero;
                    Util.GetMoveSpeed(_vObserver, target, 18, ref direction);
                    _vObserver += direction;

                    if (_vObserver == target) { _bTrackToTarget = false; }
                }
                else //We need to snap to the target
                {
                    _vObserver = target;
                }
            }

            _vCenter = new Vector2(_vObserver.X - (RiverHollow.ScreenWidth / 2), _vObserver.Y - (RiverHollow.ScreenHeight / 2));
            _transform = Matrix.CreateScale(new Vector3(Scale, Scale, 0)) * Matrix.CreateTranslation(new Vector3(-_vCenter.X, -_vCenter.Y, 0));
        }

        public static void SetObserver(WorldActor act, bool swoopToTarget = false)
        {
            _actObserver = act;
            _bTrackToTarget = swoopToTarget;
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
            return _bTrackToTarget;
        }
    }
}
