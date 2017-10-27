using Microsoft.Xna.Framework;
using System;

namespace RiverHollow.Misc
{
    public class Utilities
    {
        public static void GetMoveSpeed(Vector2 currentPos, Vector2 targetPos, int speed, ref Vector2 direction)
        {
            float newX = 0; float newY = 0;
            if (targetPos.X != currentPos.X)
            {
                newX = (targetPos.X > currentPos.X) ? 1 : -1;
            }
            if (targetPos.Y != currentPos.Y)
            {
                newY = (targetPos.Y > currentPos.Y) ? 1 : -1;
            }

            float deltaX = Math.Abs(targetPos.X - currentPos.X);
            float deltaY = Math.Abs(targetPos.Y - currentPos.Y);
            Vector2 dir = new Vector2(deltaX, deltaY);
            dir.Normalize();
            dir = dir * speed;
            
            direction.X = (deltaX < speed) ? newX * deltaX : newX * dir.X;
            direction.Y = (deltaY < speed) ? newY * deltaY : newY * dir.Y;
        }
    }
}
