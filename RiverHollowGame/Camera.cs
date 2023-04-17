using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow
{
    public static class Camera
    {
        static bool _bTrackToTarget = false;
        static Vector2 _vObserver;
        static Actor _actObserver; //The WorldActor the Camera needs to be following

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
            Vector2 target = Scrying() ? _vObserver : (_actObserver.Center.ToVector2() + _actObserver.AccumulatedMovement) * CurrentScale;

            //If Scrying is turned on and we are not taking input, process input commands to move the camera
            if (!TakingInput() && Scrying())
            {
                int speed = 10;
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Keys.W)) { target += new Vector2(0, -speed); }
                else if (ks.IsKeyDown(Keys.S)) { target += new Vector2(0, speed); }

                if (ks.IsKeyDown(Keys.A)) { target += new Vector2(-speed, 0); }
                else if (ks.IsKeyDown(Keys.D)) { target += new Vector2(speed, 0); }

                if (GUICursor.Position.Y == 0) { target += new Vector2(0, -speed); }
                else if (GUICursor.Position.Y == RiverHollow.ScreenHeight - 1) { target += new Vector2(0, speed); }

                if (GUICursor.Position.X <= 0) { target += new Vector2(-speed, 0); }
                else if (GUICursor.Position.X == RiverHollow.ScreenWidth - 1) { target += new Vector2(speed, 0); }
            }

            //This ensures that the camera observer position cannot go farther than the allowed positions,
            if (target.X <= (RiverHollow.ScreenWidth / 2)) { target.X = (RiverHollow.ScreenWidth / 2); }
            else if (target.X >= MapManager.CurrentMap.GetMapWidthInScaledPixels() - (RiverHollow.ScreenWidth / 2)) { target.X = MapManager.CurrentMap.GetMapWidthInScaledPixels() - (RiverHollow.ScreenWidth / 2); }
            if (target.Y <= (RiverHollow.ScreenHeight / 2)) { target.Y = (RiverHollow.ScreenHeight / 2); }
            else if (target.Y >= MapManager.CurrentMap.GetMapHeightInScaledPixels() - (RiverHollow.ScreenHeight / 2)) { target.Y = MapManager.CurrentMap.GetMapHeightInScaledPixels() - (RiverHollow.ScreenHeight / 2); }

            if (MapManager.CurrentMap.GetMapWidthInScaledPixels() / Constants.TILE_SIZE <= Math.Ceiling((double)RiverHollow.ScreenWidth / Constants.TILE_SIZE)) { target.X = (MapManager.CurrentMap.GetMapWidthInScaledPixels() / 2); }

            double val = Math.Ceiling((double)RiverHollow.ScreenHeight / Constants.TILE_SIZE);
            if (MapManager.CurrentMap.GetMapHeightInScaledPixels() / Constants.TILE_SIZE <= val) { target.Y = (MapManager.CurrentMap.GetMapHeightInScaledPixels() / 2); }

            //We are moving to the target
            if (!Scrying() && _bTrackToTarget)
            {
                _vObserver += Util.GetMoveSpeed(_vObserver, target, 18);

                if (_vObserver == target) { _bTrackToTarget = false; }
            }
            else //We need to snap to the target
            {
                _vObserver = target;
            }

            _vCenter = new Vector2(_vObserver.X - (RiverHollow.ScreenWidth / 2), _vObserver.Y - (RiverHollow.ScreenHeight / 2));
            _transform = Matrix.CreateScale(new Vector3(CurrentScale, CurrentScale, 0)) * Matrix.CreateTranslation(new Vector3(-_vCenter.X, -_vCenter.Y, 0));
        }

        public static void SetObserver(Actor act, bool swoopToTarget = false)
        {
            _actObserver = act;
            _bTrackToTarget = swoopToTarget;
        }

        public static void UnsetObserver(Vector2 pos)
        {
            if (pos != Vector2.Zero) { _vObserver = pos; }
            else { _vObserver = new Vector2(MapManager.CurrentMap.GetMapWidthInScaledPixels() / 2, MapManager.CurrentMap.GetMapHeightInScaledPixels() / 2); }
        }

        public static void ResetObserver()
        {
            _vObserver = _actObserver.Center.ToVector2();
        }

        public static bool IsMoving()
        {
            return _bTrackToTarget;
        }

        public static Point GetWorldPosition(Point screenPosition)
        {
            Vector3 translate = Camera._transform.Translation;
            Point mousePoint = Point.Zero;
            mousePoint.X = (int)((screenPosition.X - translate.X) / CurrentScale);
            mousePoint.Y = (int)((screenPosition.Y - translate.Y) / CurrentScale);

            return mousePoint;
        }

        public static Point GetScreenPosition(Point worldPosition)
        {
            Vector3 translate = Camera._transform.Translation;
            Point mousePoint = Point.Zero;
            mousePoint.X = (int)((worldPosition.X * CurrentScale) + translate.X);
            mousePoint.Y = (int)((worldPosition.Y * CurrentScale) + translate.Y);

            return mousePoint;
        }
    }
}
