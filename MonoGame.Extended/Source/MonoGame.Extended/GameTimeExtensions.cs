using Microsoft.Xna.Framework;

namespace MonoGame.Extended
{
    public static class GameTimeExtensions
    {
        public static float GetElapsedSeconds(this GameTime gTime)
        {
            return (float) gTime.ElapsedGameTime.TotalSeconds;
        }
    }
}