using System;
using Microsoft.Xna.Framework;
using RiverHollow.Utilities;

namespace RiverHollow.Misc
{
    public class VectorBuffer
    {
        Vector2 _vAccumulatedMovement;
        public Vector2 AccumulatedMovement => _vAccumulatedMovement;

        public VectorBuffer() { }

        public void Clear()
        {
            _vAccumulatedMovement = Vector2.Zero;
        }

        public Point AddMovement(Vector2 vector)
        {
            _vAccumulatedMovement += vector;

            //If we've stopped moving in a direction, lose the movement along that axis.
            if (vector.X == 0) { _vAccumulatedMovement.X = 0; }
            if (vector.Y == 0) { _vAccumulatedMovement.Y = 0; }

            int xVal = (int)_vAccumulatedMovement.X;
            int yVal = (int)_vAccumulatedMovement.Y;
            _vAccumulatedMovement -= new Vector2(xVal, yVal);

            return new Point(xVal, yVal);
        }

        public Point ProjectMovement(Vector2 vector)
        {
            return new Point(Util.RoundForPoint(AccumulatedMovement.X + vector.X), Util.RoundForPoint(AccumulatedMovement.Y + vector.Y));
        }
    }
}
