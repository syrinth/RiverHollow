using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Projectile : Actor
    {
        public int Damage => GetIntByIDKey("Damage");

        Vector2 _vVelocity;
        public bool Finished { get; private set; }

        public Projectile(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            BodySprite = LoadSpriteAnimations(Util.LoadWorldAnimations(stringData), DataManager.PROJECTILE_TEXTURE);
        }

        public override void Update(GameTime gTime)
        {
            BodySprite.Update(gTime);

            if (BodySprite.AnimationFinished(AnimationEnum.KO))
            {
                Finished = true;
            }
            else if (!Finished)
            {
                bool impeded = false;
                Vector2 initial = _vVelocity;
                if (CollisionBox.Intersects(PlayerManager.PlayerActor.HitBox))
                {
                    PlayerManager.PlayerActor.DealDamage(Damage, CollisionBox);
                    PlayAnimation(AnimationEnum.KO);
                    _vVelocity = Vector2.Zero;
                }
                else if (CurrentMap.CheckForCollisions(this, ref _vVelocity, ref impeded))
                {
                    MoveActor(_vVelocity, false);
                }

                if (initial != _vVelocity)
                {
                    PlayAnimation(AnimationEnum.KO);
                }
            }
        }

        public void Kickstart(Mob user, bool aims)
        {
            SetPosition(user.CollisionBoxLocation);
            CurrentMapName = user.CurrentMapName;

            if (aims)
            {
                _vVelocity = user.GetPlayerDirectionNormal() * GetFloatByIDKey("Speed");
            }
            else
            {
                _vVelocity = Util.GetPointFromDirection(user.Facing).ToVector2() * GetFloatByIDKey("Speed");
            }
        }
    }
}
