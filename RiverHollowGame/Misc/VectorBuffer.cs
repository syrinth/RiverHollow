using System;
using Microsoft.Xna.Framework;
using RiverHollow.Utilities;

namespace RiverHollow.Misc
{
    public class VectorBuffer
    {
        public Vector2 AccumulatedMovement { get; private set; }
        public VectorBuffer() { }

        public void Clear()
        {
            AccumulatedMovement = Vector2.Zero;
        }

        public Point AddMovement(Vector2 vector)
        {
            AccumulatedMovement += vector;

            int xVal = (int)AccumulatedMovement.X;
            int yVal = (int)AccumulatedMovement.Y;
            AccumulatedMovement -= new Vector2(xVal, yVal);

            return new Point(xVal, yVal);
        }

        public Point ProjectMovement(Vector2 vector)
        {
            return new Point(Util.RoundForPoint(AccumulatedMovement.X + vector.X), Util.RoundForPoint(AccumulatedMovement.Y + vector.Y));
        }
    }
}
