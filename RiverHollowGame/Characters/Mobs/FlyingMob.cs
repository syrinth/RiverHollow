using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Characters.Mobs
{
    public class FlyingMob : Mob
    {
        Vector2 _vFlightVelocity;
        public FlyingMob(int id, Dictionary<string, string> stringdata) : base(id, stringdata)
        {
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (CanAct())
            {
                switch (_eCurrentState)
                {
                    case Enums.NPCStateEnum.Idle:
                        if (OnScreen() && PlayerManager.PlayerInRange(CollisionCenter, Constants.TILE_SIZE * 6))
                        {
                            _eCurrentState = Enums.NPCStateEnum.TrackPlayer;
                            _vFlightVelocity = GetPlayerDirectionNormal();
                        }
                        break;

                    case Enums.NPCStateEnum.TrackPlayer:
                        MoveActor(_vFlightVelocity);

                        _vFlightVelocity += GetPlayerDirectionNormal() * 0.15f;

                        _vFlightVelocity.Normalize();
                        _vFlightVelocity *= _fBaseSpeed;
                        break;
                }
            }
        }

        public override bool DealDamage(int value, Rectangle hitbox)
        {
            bool rv = base.DealDamage(value, hitbox);
            if (rv)
            {
                if(CurrentHP > 0)
                {
                    _vFlightVelocity = _vKnockbackVelocity;
                    _vKnockbackVelocity = Vector2.Zero;
                }
            }

            return rv;
        }
    }
}
