using Microsoft.Xna.Framework;
using RiverHollow.Tile_Engine;
using System;
using System.Threading;

using static RiverHollow.Game_Managers.GameManager;
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

        public static Vector2 Normalize(Vector2 p)
        {
            Vector2 newVec = Vector2.Zero;
            newVec.X = ((int)(p.X / TileSize)) * TileSize;
            newVec.Y = ((int)(p.Y / TileSize)) * TileSize;

            return newVec;
        }

        public static Point Normalize(Point p)
        {
            Point newVec = Point.Zero;
            newVec.X = ((int)(p.X / TileSize)) * TileSize;
            newVec.Y = ((int)(p.Y / TileSize)) * TileSize;

            return newVec;
        }

        public static void ParseContentFile(ref string filePath, ref string name)
        {
            filePath = filePath.Replace(@"Content\", "");
            filePath = filePath.Remove(filePath.Length - 4, 4);

            string[] split = filePath.Split('\\');
            name = split[split.Length - 1];
        }

        public static Rectangle FloatRectangle(Vector2 pos, float width, float height)
        {
            return FloatRectangle(pos.X, pos.Y, width, height);
        }
        public static Rectangle FloatRectangle(float x, float y, float width, float height)
        {
            return new Rectangle((int)x, (int)y, (int)width, (int)height);
        }
    }

    public class RHRandom : Random
    {
        public RHRandom() : base()
        {
            Thread.Sleep(1);
        }

        public override int Next(int min, int max)
        {
            //Thread.Sleep(1);
            int rv = 0;
            rv = base.Next(min, max + 1);
            return rv;
        }
    }
}
