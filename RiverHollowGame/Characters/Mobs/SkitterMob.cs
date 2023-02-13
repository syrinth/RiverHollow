using Microsoft.Xna.Framework;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters.Mobs
{
    public class SkitterMob : Mob
    {
        public SkitterMob(int id, Dictionary<string, string> stringdata) : base(id, stringdata)
        {
            _eCurrentState = NPCStateEnum.Wander;
            _fBaseWanderTimer = 0.5f;
            _iMinWander = 4;
            _iMaxWander = 16;
            _movementTimer = new RHTimer(_fBaseWanderTimer);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (CanAct())
            {
                if (_bBumpedIntoSomething)
                {
                    SetMoveTo(Point.Zero);
                    _bBumpedIntoSomething = false;
                }

                if (!HasMovement() || _bBumpedIntoSomething)
                {
                    Wander(gTime, 0);
                }

                //ProcessStateEnum(gTime, false);
            }
        }
    }
}
