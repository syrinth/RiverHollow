using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters.Mobs
{
    public class Crawler : Mob
    {
        public Crawler(int id, Dictionary<string, string> stringdata) : base(id, stringdata)
        {
            _eCurrentState = NPCStateEnum.Wander;
            _fBaseWanderTimer = 0.5f;
            _iMinWander = 4;
            _iMaxWander = 16;
            _movementTimer = new RHTimer(_fBaseWanderTimer);
            _cooldownTimer = new RHTimer(0.5f, true);
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

                if (HasProjectiles)
                {
                    DirectionEnum playerDir = Util.GetDirection(GetPlayerDirection());
                    if (_cooldownTimer.TickDown(gTime) && Facing == playerDir)
                    {
                        _cooldownTimer.Reset(Cooldown + (RHRandom.Instance().Next(0, 20) / 10));
                        Projectile p = DataManager.CreateProjectile(GetIntByIDKey("Projectile"));
                        p.Kickstart(this);

                        _liProjectiles.Add(p);
                    }
                }

                if (!HasMovement() || _bBumpedIntoSomething)
                {
                    Wander(gTime, 0);
                }
            }
        }
    }
}
