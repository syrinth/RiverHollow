using Adventure.Game_Managers;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Adventure
{
    public static class Camera
    {
        private static float Scale = RiverHollow.Scale;
        public static Matrix _transform;
        public static Viewport _view;
        public static Vector2 _center;

        private static Vector2 _observer;

        public static void SetViewport(Viewport view)
        {
            _view = view;
        }

        public static void Update(GameTime gametime)
        {
            if (RiverHollow.State != RiverHollow.GameState.Build)
            {
                _observer = PlayerManager.Player.Center.ToVector2()*Scale;
            }
            else {
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
                    _observer += new Vector2(-speed,0);
                }
                else if (ks.IsKeyDown(Keys.D))
                {
                    _observer += new Vector2(speed, 0);
                }
            }

            float BorderOffset = RHMap.TileSize * Scale;
            if (_observer.X <= (RiverHollow.ScreenWidth / 2) + BorderOffset)
            {
                _observer.X = (RiverHollow.ScreenWidth / 2) + BorderOffset;
            }
            else if (_observer.X >= MapManager.CurrentMap.GetMapWidth() * Scale - (RiverHollow.ScreenWidth / 2) - BorderOffset)
            {
                _observer.X = MapManager.CurrentMap.GetMapWidth() * Scale - (RiverHollow.ScreenWidth / 2) - BorderOffset;
            }

            if (_observer.Y <= (RiverHollow.ScreenHeight / 2) + BorderOffset)
            {
                _observer.Y = (RiverHollow.ScreenHeight / 2) + BorderOffset;
            }
            else if (_observer.Y >= MapManager.CurrentMap.GetMapHeight() * Scale - (RiverHollow.ScreenHeight / 2) - BorderOffset)
            {
                _observer.Y = MapManager.CurrentMap.GetMapHeight() * Scale - (RiverHollow.ScreenHeight / 2) - BorderOffset;
            }

            _center = new Vector2(_observer.X - (RiverHollow.ScreenWidth / 2), _observer.Y - (RiverHollow.ScreenHeight / 2));
            _transform = Matrix.CreateScale(new Vector3(Scale, Scale, 0)) * Matrix.CreateTranslation(new Vector3(-_center.X, -_center.Y, 0));
        }

        public static void UnsetObserver()
        {
            _observer = new Vector2(MapManager.CurrentMap.MapWidth / 2, MapManager.CurrentMap.MapHeight / 2);
        }
        public static void ResetObserver()
        {
            _observer = PlayerManager.Player.Center.ToVector2();
        }
    }
}
