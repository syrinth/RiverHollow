using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters.Mobs
{
    public class Flyer : Mob
    {
        Vector2 _vFlightVelocity;
        public Flyer(int id, Dictionary<string, string> stringdata) : base(id, stringdata)
        {
            IgnoreCollisions = true;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (CanAct())
            {
                switch (_eCurrentState)
                {
                    case NPCStateEnum.Idle:
                        if (OnScreen() && PlayerManager.PlayerInRange(CollisionCenter, Constants.TILE_SIZE * 6))
                        {
                            _eCurrentState = NPCStateEnum.TrackPlayer;
                            _vFlightVelocity = GetPlayerDirectionNormal();
                        }
                        break;

                    case NPCStateEnum.TrackPlayer:
                        if (!HasKnockbackVelocity())
                        {
                            MoveActor(_vFlightVelocity);
                            PlayAnimation(VerbEnum.Walk, Facing);

                            Vector2 mod = GetPlayerDirection() * 0.015f;
                            _vFlightVelocity += mod;
                            _vFlightVelocity.Normalize();
                            _vFlightVelocity *= _fBaseSpeed;
                        }
                        break;
                }
            }
        }

        public override bool DealDamage(int value, Rectangle hitbox)
        {
            bool rv = base.DealDamage(value, hitbox);

            if (rv && !HasHP)
            {
                IgnoreCollisions = false;
            }

            return rv;
        }
    }
}
