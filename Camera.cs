using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure
{
    public static class Camera
    {
        public static Matrix _transform;
        public static Viewport _view;
        public static Vector2 _center;

        public static void SetViewport(Viewport view)
        {
            _view = view;
        }

        public static void Update(GameTime gametime, AdventureGame adv)
        {
            float playerPosX = adv._player.Center.X;
            float playerPosY = adv._player.Center.Y;

            if (playerPosX <= (adv.SCREEN_WIDTH / 2))
            {
                playerPosX = adv.SCREEN_WIDTH / 2;
            }
            else if (playerPosX >= adv._currentMap.GetMapWidth() - (adv.SCREEN_WIDTH / 2))
            {
                playerPosX = adv._currentMap.GetMapWidth() - (adv.SCREEN_WIDTH / 2);
            }

            if (playerPosY <= (adv.SCREEN_HEIGHT / 2))
            {
                playerPosY = adv.SCREEN_HEIGHT / 2;
            }
            else if (playerPosY >= adv._currentMap.GetMapHeight() - (adv.SCREEN_HEIGHT / 2))
            {
                playerPosY = adv._currentMap.GetMapHeight() - (adv.SCREEN_HEIGHT / 2);
            }

            _center = new Vector2(playerPosX - (adv.SCREEN_WIDTH / 2), playerPosY - (adv.SCREEN_HEIGHT / 2));
            _transform = Matrix.CreateScale(new Vector3(1, 1, 0)) * Matrix.CreateTranslation(new Vector3(-_center.X, -_center.Y, 0));
        }
    }
}
