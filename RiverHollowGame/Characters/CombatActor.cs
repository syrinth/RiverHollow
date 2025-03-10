﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public abstract class CombatActor : Actor
    {
        public bool HasHP => CurrentHP > 0;
        public virtual float MaxHP => 2;
        public float CurrentHP { get; protected set; } = 2;

        protected bool _bFlicker = false;
        protected RHTimer _flickerTimer;
        protected RHTimer _damageTimer;
        protected RHTimer _cooldownTimer;

        protected List<StatusEffect> _liEffects;

        public virtual Rectangle HitBox => new Rectangle(Position.X + HitBoxOffset.X, Position.Y + HitBoxOffset.Y, HitBoxSize.X, HitBoxSize.Y);
        public Point HitBoxOffset => GetPointByIDKey("HitBoxOffset",new Point(0, Height - Constants.TILE_SIZE));
        public Point HitBoxSize => GetPointByIDKey("HitBoxSize", new Point(Width, Constants.TILE_SIZE));

        public CombatActor() : base()
        {
            _liEffects = new List<StatusEffect>();
        }
        public CombatActor(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _liEffects = new List<StatusEffect>();
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
            if (Constants.DRAW_HITBOX)
            {
                spriteBatch.Draw(DataManager.GetTexture(DataManager.HUD_COMPONENTS), HitBox, GUIUtils.BLACK_BOX, Color.Red * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, GetSprites()[0].LayerDepth - 1);
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            for (int i = _liEffects.Count - 1; i >= 0; i--)
            {
                StatusEffect effect = _liEffects[i];
                effect.Update(gTime);
                if (effect.Duration <= 0)
                {
                    effect.RemoveEffects(this);
                    _liEffects.RemoveAt(i);
                }
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

            if (_damageTimer == null && HasHP)
            {
                rv = true;

                _damageTimer = new RHTimer(IsActorType(ActorTypeEnum.Mob) ? Constants.MOB_INVULN_PERIOD : Constants.PLAYER_INVULN_PERIOD);
                _flickerTimer = new RHTimer(Constants.FLICKER_PERIOD);

                DecreaseHealth(value);
                AssignKnockbackVelocity(hitbox);
            }

            return rv;
        }

        public void DamageTimerEnd()
        {
            _damageTimer = null;
            _flickerTimer = null;
            Flicker(true);
        }

        protected virtual void Flicker(bool value)
        {
            _bFlicker = value;
            _flickerTimer?.Reset();
        }

        protected void CheckDamageTimers(GameTime gTime)
        {
            if (RHTimer.TimerCheck(_damageTimer, gTime))
            {
                DamageTimerEnd();
            }

            if (RHTimer.TimerCheck(_flickerTimer, gTime))
            {
                Flicker(!_bFlicker);
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

        protected virtual void Kill()
        {
            _vKnockbackVelocity = Vector2.Zero;
            PlayAnimation(AnimationEnum.KO);
        }

        public void AssignStatusEffect(StatusEffect effect, float duration)
        {
            effect.SetDuration(duration);
            effect.AssignEffects(this);
            _liEffects.Add(effect);
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
                MoveActor(dir, false);
            }
            if(initial != dir && !HasHP)
            {
                _vKnockbackVelocity = Vector2.Zero;
            }

            KnockbackDecay();
        }

        protected void KnockbackDecay()
        {
            _vKnockbackVelocity *= _fDecay;
            if (_vKnockbackVelocity.Length() < (_vInitialKnockback / 4).Length())
            {
                _vKnockbackVelocity = Vector2.Zero;
                if (!HasHP)
                {
                    Kill();
                }
            }
        }

        private void AssignKnockbackVelocity(Rectangle hitbox)
        {
            _vKnockbackVelocity = hitbox.Center.ToVector2() - CollisionBox.Center.ToVector2();

            if (_vKnockbackVelocity.Length() != 0)
            {
                _vKnockbackVelocity.Normalize();
            }
            switch (GetWeight())
            {
                case WeightEnum.Light:
                    _vKnockbackVelocity *= -4;
                    _fDecay = 0.96f;
                    goto default;
                case WeightEnum.Medium:
                    _vKnockbackVelocity *= -2;
                    _fDecay = 0.96f;
                    goto default;
                case WeightEnum.Heavy:
                    _vKnockbackVelocity *= -0.75f;
                    _fDecay = 0.90f;
                    goto default;
                case WeightEnum.Immovable:
                    _vKnockbackVelocity *= 0;
                    if (hitbox == PlayerManager.PlayerActor.CollisionBox)
                    {
                        PlayerManager.PlayerActor.AssignKnockbackVelocity(CollisionBox);
                    }

                    if (!HasHP)
                    {
                        Kill();
                    }
                    break;
                default:
                    SetMoveTo(Point.Zero);
                    ClearPath();
                    break;
            }

            _vInitialKnockback = _vKnockbackVelocity;
        }

        protected virtual WeightEnum GetWeight()
        {
            return WeightEnum.Medium;
        }

        #endregion
    }
}
