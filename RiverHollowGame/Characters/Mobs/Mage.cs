using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters.Mobs
{
    internal class Mage : Mob
    {
        public Mage(int id, Dictionary<string, string> stringdata) : base(id, stringdata)
        {
        }

        protected override void DetermineAction(GameTime gTime)
        {
            if (_bUsingAction && BodySprite.AnimationFinished(VerbEnum.Action1, Facing))
            {
                _bUsingAction = false;
                var strParams = DataManager.GetStringArgsByIDKey(ID, "EffectID", DataType.Actor);
                var effect = DataManager.CreateEffect(int.Parse(strParams[0]));

                if (strParams.Length > 1 && strParams[1] == "Self") { effect.SetPosition(CollisionBoxLocation); }
                else { effect.SetPosition(PlayerManager.PlayerActor.CollisionBoxLocation); }

                CurrentMap.AddActor(effect);
            }
            else if (_cooldownTimer.TickDown(gTime))
            {
                if (!_bUsingAction && PlayerManager.PlayerInRange(CollisionCenter, Constants.TILE_SIZE * 6))
                {
                    _bUsingAction = true;
                    PlayAnimation(VerbEnum.Action1);
                    _cooldownTimer.Reset();
                }
            }
        }
    }
}
