using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public abstract class CombatActor : WorldActor
    {
        public virtual float MaxHP { get; protected set; } = 2;
        public float CurrentHP { get; protected set; } = 2;

        protected RHTimer _flickerTimer;
        protected RHTimer _damageTimer;

        public virtual Rectangle HitBox => CollisionBox;

        public CombatActor() : base() { }
        public CombatActor(int id, Dictionary<string, string> stringData) : base(id, stringData) { }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
            if (Constants.DRAW_HITBOX)
            {
                spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), HitBox, new Rectangle(160, 128, 2, 2), Color.Red * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, GetSprites()[0].LayerDepth - 1);
            }
        }

        #region Combat Logic

        public void RefillHealth()
        {
            CurrentHP = MaxHP;
        }

        /// <summary>
        /// Reduces health by the given value. Cannot deal more damage than health exists.
        /// </summary>
        public virtual bool DealDamage(int value, Rectangle hitbox)
        {
            bool rv = false;

            if (_damageTimer == null && CurrentHP > 0)
            {
                rv = true;

                _damageTimer = new RHTimer(Constants.INVULN_PERIOD);
                _flickerTimer = new RHTimer(Constants.FLICKER_PERIOD);

                DecreaseHealth(value);
                AssignKnockbackVelocity(hitbox);
            }

            return rv;
        }

        protected void DamageTimerEnd()
        {
            if (CurrentHP == 0) { PlayAnimation(AnimationEnum.KO); }
            ClearCombatStates();
        }
        
        protected virtual void CheckDamageTimers(GameTime gTime)
        {
            if (_damageTimer != null && _damageTimer.TickDown(gTime))
            {
                DamageTimerEnd();
            }

            if (_flickerTimer != null && _flickerTimer.TickDown(gTime))
            {
                if (_sprBody.SpriteColor == Color.Red)
                {
                    _flickerTimer = null;
                    _sprBody.SetColor(Color.White);
                }
            }
        }

        public void DecreaseHealth(int value)
        {
            CurrentHP -= (CurrentHP - value >= 0) ? value : CurrentHP;
        }

        /// <summary>
        /// As long as the target is not KnockedOut, recover the given amount of HP up to max
        /// </summary>
        public float IncreaseHealth(float x)
        {
            float amountHealed = x;
            if (CurrentHP + x <= MaxHP)
            {
                CurrentHP += x;
            }
            else
            {
                amountHealed = MaxHP - CurrentHP;
                CurrentHP = MaxHP;
            }

            return amountHealed;
        }

        /// <summary>
        /// Adds the StatusEffect objectto the character's list of status effects.
        /// </summary>
        /// <param name="effect">Effect toadd</param>
        public void ApplyStatusEffect(StatusEffect effect)
        {
            //if (effect.EffectType == StatusTypeEnum.DoT || effect.EffectType == StatusTypeEnum.HoT)
            //{
            //    StatusEffect find = _liStatusEffects.Find(status => status.ID == effect.ID);
            //    if (find != null)
            //    {
            //        _liStatusEffects.Remove(find);
            //    }
            //    _liStatusEffects.Add(effect);
            //}
            //else
            //{
            //    foreach (KeyValuePair<AttributeEnum, string> kvp in effect.AffectedAttributes)
            //    {
            //        AssignAttributeEffect(kvp.Key, kvp.Value, effect.Duration, effect.EffectType);
            //    }
            //    _liStatusEffects.Add(effect);
            //}
        }

        public bool HasKnockbackVelocity()
        {
            return _vKnockbackVelocity != Vector2.Zero;
        }
        public void ApplyKnockbackVelocity()
        {
            Vector2 dir = _vKnockbackVelocity;

            bool impeded = false;
            Vector2 initial = dir;
            if (CurrentMap.CheckForCollisions(this, ref dir, ref impeded))
            {
                MoveActor(dir);
            }
            if(initial != dir && CurrentHP == 0)
            {
                ClearCombatStates();
                PlayAnimation(AnimationEnum.KO);
            }

            _vKnockbackVelocity *= 0.96f;
        }

        private void AssignKnockbackVelocity(Rectangle hitbox)
        {
            _vKnockbackVelocity = hitbox.Center.ToVector2() - CollisionBox.Center.ToVector2();
            _vKnockbackVelocity.Normalize();
            switch (GetWeight())
            {
                case WeightEnum.Light:
                    _vKnockbackVelocity *= -4;
                    _fDecay = 0.96f;
                    goto default;
                case WeightEnum.Medium:
                    _vKnockbackVelocity *= -1.5f;
                    _fDecay = 0.96f;
                    goto default;
                case WeightEnum.Heavy:
                    _vKnockbackVelocity *= -0.75f;
                    _fDecay = 0.90f;
                    goto default;
                case WeightEnum.Immovable:
                    _vKnockbackVelocity *= 0;
                    break;
                default:
                    SetMoveTo(Point.Zero);
                    ClearPath();
                    break;
            }
        }

        protected virtual WeightEnum GetWeight()
        {
            return WeightEnum.Medium;
        }

        protected void ClearCombatStates()
        {
            _damageTimer = null;
            _flickerTimer = null;
            _vKnockbackVelocity = Vector2.Zero;
        }

        #endregion
    }
}
