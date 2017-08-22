using Adventure.Game_Managers;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Adventure
{
    public static class Camera
    {
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
            if (!AdventureGame.BuildingMode)
            {
                _observer = PlayerManager.Player.Center.ToVector2();
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

            if (_observer.X <= (AdventureGame.ScreenWidth / 2) + TileMap.TileSize)
            {
                _observer.X = (AdventureGame.ScreenWidth / 2) + TileMap.TileSize;
            }
            else if (_observer.X >= MapManager.CurrentMap.GetMapWidth() - (AdventureGame.ScreenWidth / 2) - TileMap.TileSize)
            {
                _observer.X = MapManager.CurrentMap.GetMapWidth() - (AdventureGame.ScreenWidth / 2) - TileMap.TileSize;
            }

            if (_observer.Y <= (AdventureGame.ScreenHeight / 2) + TileMap.TileSize)
            {
                _observer.Y = (AdventureGame.ScreenHeight / 2) + TileMap.TileSize;
            }
            else if (_observer.Y >= MapManager.CurrentMap.GetMapHeight() - (AdventureGame.ScreenHeight / 2) - TileMap.TileSize)
            {
                _observer.Y = MapManager.CurrentMap.GetMapHeight() - (AdventureGame.ScreenHeight / 2) - TileMap.TileSize;
            }

            _center = new Vector2(_observer.X - (AdventureGame.ScreenWidth / 2), _observer.Y - (AdventureGame.ScreenHeight / 2));
            _transform = Matrix.CreateScale(new Vector3(1, 1, 0)) * Matrix.CreateTranslation(new Vector3(-_center.X, -_center.Y, 0));
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
