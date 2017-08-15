using Adventure.Game_Managers;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure
{
    public static class Camera
    {
        private static MapManager _mapManager = MapManager.GetInstance();
        private static PlayerManager _playerManager = PlayerManager.GetInstance();
        public static Matrix _transform;
        public static Viewport _view;
        public static Vector2 _center;

        public static void SetViewport(Viewport view)
        {
            _view = view;
        }

        public static void Update(GameTime gametime)
        {
            float cameraCenterX = _playerManager.Player.Center.X;
            float cameraCenterY = _playerManager.Player.Center.Y;

            if (cameraCenterX <= (AdventureGame.SCREEN_WIDTH / 2) + TileMap.TileSize)
            {
                cameraCenterX = (AdventureGame.SCREEN_WIDTH / 2) + TileMap.TileSize;
            }
            else if (cameraCenterX >= _mapManager.CurrentMap.GetMapWidth() - (AdventureGame.SCREEN_WIDTH / 2) - TileMap.TileSize)
            {
                cameraCenterX = _mapManager.CurrentMap.GetMapWidth() - (AdventureGame.SCREEN_WIDTH / 2) - TileMap.TileSize;
            }

            if (cameraCenterY <= (AdventureGame.SCREEN_HEIGHT / 2) + TileMap.TileSize)
            {
                cameraCenterY = (AdventureGame.SCREEN_HEIGHT / 2) + TileMap.TileSize;
            }
            else if (cameraCenterY >= _mapManager.CurrentMap.GetMapHeight() - (AdventureGame.SCREEN_HEIGHT / 2) - TileMap.TileSize)
            {
                cameraCenterY = _mapManager.CurrentMap.GetMapHeight() - (AdventureGame.SCREEN_HEIGHT / 2) - TileMap.TileSize;
            }

            _center = new Vector2(cameraCenterX - (AdventureGame.SCREEN_WIDTH / 2), cameraCenterY - (AdventureGame.SCREEN_HEIGHT / 2));
            _transform = Matrix.CreateScale(new Vector3(1, 1, 0)) * Matrix.CreateTranslation(new Vector3(-_center.X, -_center.Y, 0));
        }
    }
}
