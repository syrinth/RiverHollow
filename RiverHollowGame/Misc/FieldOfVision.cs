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
        Vector2 _vFirst;            //The LeftMost of the TopMost
        Vector2 _vSecond;           //The RightMost of the BottomMost
        DirectionEnum _eDir;

        public FieldOfVision(Mob theMob, int maxRange)
        {
            int sideRange = Constants.TILE_SIZE * 2;
            _iMaxRange = maxRange;
            _eDir = theMob.Facing;
            if (_eDir == DirectionEnum.Up || _eDir == DirectionEnum.Down)
            {
                _vFirst = theMob.Center - new Vector2(sideRange, 0);
                _vSecond = theMob.Center + new Vector2(sideRange, 0);
            }
            else
            {
                _vFirst = theMob.Center - new Vector2(0, sideRange);
                _vSecond = theMob.Center + new Vector2(0, sideRange);
            }
        }

        public void MoveBy(Vector2 v)
        {
            _vFirst += v;
            _vSecond += v;
        }

        public bool Contains(WorldActor actor)
        {
            bool rv = false;
            Vector2 center = actor.CollisionCenter.ToVector2();

            Vector2 firstFoV = _vFirst;
            Vector2 secondFoV = _vSecond;
            //Make sure the actor could be in range
            if (_eDir == DirectionEnum.Up && Util.InBetween(center.Y, firstFoV.Y - _iMaxRange, firstFoV.Y))
            {
                float yMod = Math.Abs(center.Y - firstFoV.Y);
                firstFoV += new Vector2(-yMod, -yMod);
                secondFoV += new Vector2(yMod, -yMod);

                rv = Util.InBetween(center.X, firstFoV.X, secondFoV.X) && Util.InBetween(center.Y, firstFoV.Y, _vFirst.Y);
            }
            else if (_eDir == DirectionEnum.Down && Util.InBetween(center.Y, firstFoV.Y, firstFoV.Y + _iMaxRange))
            {
                float yMod = Math.Abs(center.Y - firstFoV.Y);
                firstFoV += new Vector2(-yMod, yMod);
                secondFoV += new Vector2(yMod, yMod);

                rv = Util.InBetween(center.X, firstFoV.X, secondFoV.X) && Util.InBetween(center.Y, _vFirst.Y, firstFoV.Y);
            }
            else if (_eDir == DirectionEnum.Left && Util.InBetween(center.X, firstFoV.X - _iMaxRange, firstFoV.X))
            {
                float xMod = Math.Abs(center.X - firstFoV.X);
                firstFoV += new Vector2(-xMod, -xMod);
                secondFoV += new Vector2(-xMod, xMod);

                rv = Util.InBetween(center.Y, firstFoV.Y, secondFoV.Y) && Util.InBetween(center.X, firstFoV.X, _vFirst.X);
            }
            else if (_eDir == DirectionEnum.Right && Util.InBetween(center.X, firstFoV.X, firstFoV.X + _iMaxRange))
            {
                float xMod = Math.Abs(center.X - firstFoV.X);
                firstFoV += new Vector2(xMod, -xMod);
                secondFoV += new Vector2(xMod, xMod);

                rv = Util.InBetween(center.Y, firstFoV.Y, secondFoV.Y) && Util.InBetween(center.X, _vFirst.X, firstFoV.X);
            }

            return rv;
        }
    }
}
