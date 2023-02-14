using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class FieldOfVision
    {
        int _iMaxRange;
        Point _pFirst;            //The LeftMost of the TopMost
        Point _pSecond;           //The RightMost of the BottomMost
        DirectionEnum _eDir;

        public FieldOfVision(Mob theMob, int maxRange)
        {
            int sideRange = Constants.TILE_SIZE * 2;
            _iMaxRange = maxRange;
            _eDir = theMob.Facing;
            if (_eDir == DirectionEnum.Up || _eDir == DirectionEnum.Down)
            {
                _pFirst = theMob.Center - new Point(sideRange, 0);
                _pSecond = theMob.Center + new Point(sideRange, 0);
            }
            else
            {
                _pFirst = theMob.Center - new Point(0, sideRange);
                _pSecond = theMob.Center + new Point(0, sideRange);
            }
        }

        public void MoveBy(Point v)
        {
            _pFirst += v;
            _pSecond += v;
        }

        public bool Contains(Actor actor)
        {
            bool rv = false;
            Point center = actor.CollisionCenter;

            Point firstFoV = _pFirst;
            Point secondFoV = _pSecond;
            //Make sure the actor could be in range
            if (_eDir == DirectionEnum.Up && Util.InBetween(center.Y, firstFoV.Y - _iMaxRange, firstFoV.Y))
            {
                int yMod = Math.Abs(center.Y - firstFoV.Y);
                firstFoV += new Point(-yMod, -yMod);
                secondFoV += new Point(yMod, -yMod);

                rv = Util.InBetween(center.X, firstFoV.X, secondFoV.X) && Util.InBetween(center.Y, firstFoV.Y, _pFirst.Y);
            }
            else if (_eDir == DirectionEnum.Down && Util.InBetween(center.Y, firstFoV.Y, firstFoV.Y + _iMaxRange))
            {
                int yMod = Math.Abs(center.Y - firstFoV.Y);
                firstFoV += new Point(-yMod, yMod);
                secondFoV += new Point(yMod, yMod);

                rv = Util.InBetween(center.X, firstFoV.X, secondFoV.X) && Util.InBetween(center.Y, _pFirst.Y, firstFoV.Y);
            }
            else if (_eDir == DirectionEnum.Left && Util.InBetween(center.X, firstFoV.X - _iMaxRange, firstFoV.X))
            {
                int xMod = Math.Abs(center.X - firstFoV.X);
                firstFoV += new Point(-xMod, -xMod);
                secondFoV += new Point(-xMod, xMod);

                rv = Util.InBetween(center.Y, firstFoV.Y, secondFoV.Y) && Util.InBetween(center.X, firstFoV.X, _pFirst.X);
            }
            else if (_eDir == DirectionEnum.Right && Util.InBetween(center.X, firstFoV.X, firstFoV.X + _iMaxRange))
            {
                int xMod = Math.Abs(center.X - firstFoV.X);
                firstFoV += new Point(xMod, -xMod);
                secondFoV += new Point(xMod, xMod);

                rv = Util.InBetween(center.Y, firstFoV.Y, secondFoV.Y) && Util.InBetween(center.X, _pFirst.X, firstFoV.X);
            }

            return rv;
        }
    }
}
