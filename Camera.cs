using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow
{
    public static class Camera
    {
        public static Matrix _transform;
        public static Viewport _view;
        public static Vector2 _center;

        private static Vector2 _observer;

        public static void SetViewport(Viewport view)
        {
            //MAR
            //_view = view;
        }

        public static void Update(GameTime gTime)
        {
            if (!TakingInput())
            {
                if (!Scrying())
                {
                    _observer = PlayerManager.World.CharCenter.ToVector2() * Scale;
                }
                else
                {
                    KeyboardState ks = Keyboard.GetState();
                    int speed = 10;
                    if (ks.IsKeyDown(Keys.W))
                    {
                        _observer += new Vector2(0, -speed);
                    }
                    else if (ks.IsKeyDown(Keys.S))
                    {
                        _observer += new Vector2(0, speed);
                    }

                    if (ks.IsKeyDown(Keys.A))
                    {
                        _observer += new Vector2(-speed, 0);
                    }
                    else if (ks.IsKeyDown(Keys.D))
                    {
                        _observer += new Vector2(speed, 0);
                    }
                }
            }

            float BorderOffset = TileSize * Scale;
            bool xLocked = false;
            bool yLocked = false;
            if (MapManager.CurrentMap.GetMapWidth() < RiverHollow.ScreenWidth)
            {
                xLocked = true;
                _observer.X = (MapManager.CurrentMap.GetMapWidth() / 2);
            }

            if (MapManager.CurrentMap.GetMapHeight() < RiverHollow.ScreenHeight)
            {
                yLocked = true;
                _observer.Y = (MapManager.CurrentMap.GetMapHeight() / 2);
            }

            if (!xLocked)
            {
                if (_observer.X <= (RiverHollow.ScreenWidth / 2))
                {
                    _observer.X = (RiverHollow.ScreenWidth / 2);
                }
                else if (_observer.X >= MapManager.CurrentMap.GetMapWidth() - (RiverHollow.ScreenWidth / 2))
                {
                    _observer.X = MapManager.CurrentMap.GetMapWidth() - (RiverHollow.ScreenWidth / 2);
                }
            }

            if (!yLocked)
            {
                if (_observer.Y <= (RiverHollow.ScreenHeight / 2) + BorderOffset)
                {
                    _observer.Y = (RiverHollow.ScreenHeight / 2) + BorderOffset;
                }
                else if (_observer.Y >= MapManager.CurrentMap.GetMapHeight() - (RiverHollow.ScreenHeight / 2) - BorderOffset)
                {
                    _observer.Y = MapManager.CurrentMap.GetMapHeight() - (RiverHollow.ScreenHeight / 2) - BorderOffset;
                }
            }

            _center = new Vector2(_observer.X - (RiverHollow.ScreenWidth / 2), _observer.Y - (RiverHollow.ScreenHeight / 2));
            _transform = Matrix.CreateScale(new Vector3(Scale, Scale, 0)) * Matrix.CreateTranslation(new Vector3(-_center.X, -_center.Y, 0));
        }

        public static void UnsetObserver()
        {
            _observer = new Vector2(MapManager.CurrentMap.MapWidthTiles / 2, MapManager.CurrentMap.MapHeightTiles / 2);
        }
        public static void ResetObserver()
        {
            _observer = PlayerManager.World.CharCenter.ToVector2();
        }
    }
}
