using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class ActionEffect : Actor
    {
        Rectangle _rSourceCollisionBox;
        public ActionEffect(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            CollisionState = ActorCollisionState.PassThrough;
            BodySprite = LoadSpriteAnimations(Util.LoadWorldAnimations(stringData), DataManager.FOLDER_EFFECTS + stringData["Key"]);
        }

        public void SetSourceCollision(Rectangle r)
        {
            _rSourceCollisionBox = r;
        }

        public override void Update(GameTime gTime)
        {
            BodySprite.Update(gTime);

            var hitFrame = GetIntByIDKey("HitFrame");
            if(BodySprite.CurrentFrame == hitFrame)
            {
                if (CollisionBox.Intersects(PlayerManager.PlayerActor.CollisionBox))
                {
                    PlayerManager.PlayerActor.DealDamage(GetIntByIDKey("Damage"), _rSourceCollisionBox);
                }
            }

            if (BodySprite.AnimationFinished(VerbEnum.Action1, Facing))
            {
                CurrentMap.RemoveActor(this);
            }
        }
    }
}
