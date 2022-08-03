using Microsoft.Xna.Framework;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public abstract class CombatActor : Actor
    {
        #region Properties
        protected CombatActorTypeEnum _eActorType;

        protected const int MAX_STAT = 99;
        protected string _sUnique;

        private Vector2 _vStartPos;
        public Vector2 StartPosition => _vStartPos;

        public int CurrentHP { get; protected set; }
        public virtual int MaxHP => Math.Min(999, Attribute(AttributeEnum.Vitality));

        public CombatTile Tile;
        public GUICombatTile Location => Tile.GUITile;

        protected Dictionary<AttributeEnum, int> _diAttributes;
        protected Dictionary<AttributeEnum, AttributeStatusEffect> _diEffectedAttributes;

        protected int _iCrit = 10;
        public int CritRating => _iCrit;

        protected List<CombatAction> _liActions;
        public virtual List<CombatAction> Actions => _liActions;

        protected List<CombatAction> _liSpecialActions;
        public virtual List<CombatAction> SpecialActions => _liSpecialActions;

        protected List<StatusEffect> _liStatusEffects;
        public List<StatusEffect> StatusEffects => _liStatusEffects;

        private ElementEnum _elementAttackEnum;
        protected Dictionary<ElementEnum, ElementAlignment> _diElementalAlignment;
        public Dictionary<ElementEnum, ElementAlignment> DiElementalAlignment => _diElementalAlignment;
        public Summon LinkedSummon { get; private set; }

        public bool Counter;
        public bool GoToCounter;

        public CombatActor MyGuard;
        public CombatActor GuardTarget;
        protected bool _bGuard;
        public bool Guard => _bGuard;
        public bool KnockedOut { get; private set; } = false;

        public bool Swapped;
        #endregion

        public CombatActor() : base()
        {
            _liSpecialActions = new List<CombatAction>();
            _liActions = new List<CombatAction>();
            _liStatusEffects = new List<StatusEffect>();
            _diAttributes = new Dictionary<AttributeEnum, int>();
            foreach (AttributeEnum e in Enum.GetValues(typeof(AttributeEnum))) { _diAttributes[e] = 0; }

            _diEffectedAttributes = new Dictionary<AttributeEnum, AttributeStatusEffect>();
            foreach (AttributeEnum e in Enum.GetValues(typeof(AttributeEnum))) { _diEffectedAttributes[e] = new AttributeStatusEffect(); }

            _diElementalAlignment = new Dictionary<ElementEnum, ElementAlignment>
            {
                [ElementEnum.Fire] = ElementAlignment.Neutral,
                [ElementEnum.Ice] = ElementAlignment.Neutral,
                [ElementEnum.Lightning] = ElementAlignment.Neutral
            };

            _iBodyHeight = 32;
            _iBodyWidth = 32;
        }

        public override void Update(GameTime theGameTime)
        {
            //Finished being hit, determine action
            if (IsCurrentAnimation(AnimationEnum.Hurt) && BodySprite.GetPlayCount() >= 1)
            {
                if (CurrentHP == 0) { Tile.PlayAnimation(AnimationEnum.KO); }
                else if (IsCritical()) { Tile.PlayAnimation(AnimationEnum.Critical); }
                else { Tile.PlayAnimation(AnimationEnum.Idle); }
            }

            if (!KnockedOut && IsCurrentAnimation(AnimationEnum.KO))
            {
                if (IsCritical()) { Tile.PlayAnimation(AnimationEnum.Critical); }
                else { Tile.PlayAnimation(AnimationEnum.Idle); }
            }

            if (IsCurrentAnimation(AnimationEnum.Critical) && !IsCritical())
            {
                Tile.PlayAnimation(AnimationEnum.Idle);
            }

            if (LinkedSummon != null)
            {
                LinkedSummon.Update(theGameTime);
            }
        }

        protected void LoadSpriteAnimations(ref AnimatedSprite sprite, List<AnimationData> listAnimations, string textureName)
        {
            sprite = new AnimatedSprite(textureName);

            int xLocation = 0;
            foreach (AnimationData data in listAnimations)
            {
                sprite.AddAnimation(data.Animation, xLocation, 0, _iBodyWidth, _iBodyHeight, data.Frames, data.FrameSpeed, data.PingPong);
                xLocation += data.Frames * _iBodyWidth;
            }

            PlayAnimation(AnimationEnum.Idle);
            sprite.SetScale(Constants.NORMAL_SCALE);
        }

        public virtual void GoToIdle()
        {
            if (IsCritical()) { base.PlayAnimation(AnimationEnum.Critical); }
            else { base.PlayAnimation(AnimationEnum.Idle); }
        }
        public bool IsCritical() { return (CurrentHP / (float)MaxHP) <= 0.25; }

        /// <summary>
        /// Virtual method to placehold for subclasses
        /// </summary>
        public virtual GUIImage GetIcon() { return null; }
        public virtual GUISprite GetSprite(){ return Tile.GUITile.CharacterSprite; }

        public virtual int Attribute(AttributeEnum e)
        {
            int rv = 0;
            if (_diAttributes.ContainsKey(e))
            {
                return _diAttributes[e] + _diEffectedAttributes[e].Value;
            }
            return rv;
        }
        public AttributeStatusEffect GetEffectedAttributeInfo(AttributeEnum e)
        {
            return _diEffectedAttributes[e];
        }

        /// <summary>
        /// Reduces health by the given value. Cannot deal more damage than health exists.
        /// </summary>
        public virtual int DealDamage(int value)
        {
            Tile.PlayAnimation(AnimationEnum.Hurt);
            if (CurrentHP == 0)
            {
                KnockedOut = true;
                UnlinkSummon();
            }

            return value;
        }

        public void DecreaseHealth(int value)
        {
            CurrentHP -= (CurrentHP - value >= 0) ? value : CurrentHP;
        }

        /// <summary>
        /// As long as the target is not KnockedOut, recover the given amount of HP up to max
        /// </summary>
        public int IncreaseHealth(int x)
        {
            int amountHealed = 0;
            if (!KnockedOut)
            {
                amountHealed = x;
                if (CurrentHP + x <= MaxHP)
                {
                    CurrentHP += x;
                }
                else
                {
                    amountHealed = MaxHP - CurrentHP;
                    CurrentHP = MaxHP;
                }
            }

            return amountHealed;
        }

        #region Combat Action Handling

        private int GetRawDamage(int baseDamage, int attribute, int potency)
        {
            double potencyMod = potency / 100.0;   //100 potency is considered an average attack
            double AttributeMultiplier = 0.2 + ((double)attribute / 6 * attribute / MAX_STAT);

            return (int)(baseDamage * potencyMod * AttributeMultiplier);
        }
        /// <summary>
        /// Retrieves the raw, unmodified damage being dealt by this action
        /// </summary>
        /// <param name="min">The minimum damage to be dealt</param>
        /// <param name="max">The maximum damage to be dealt</param>
        /// <param name="attribute">The Action's Damage Attribute</param>
        /// <param name="potency">The Potency of the Action</param>
        public void GetRawDamageRange(out int min, out int max, AttributeEnum attribute, int potency)
        {
            double rawDamage = GetRawDamage(Attribute(AttributeEnum.Damage), Attribute(attribute), potency);

            min = (int)(rawDamage * 0.75);
            max = (int)(rawDamage * 1.25);
        }

        public void GetActualDamageRange(out int min, out int max, CombatActor attacker, AttributeEnum attribute, int potency, ElementEnum element = ElementEnum.None)
        {
            double rawDamage = GetRawDamage(attacker.Attribute(AttributeEnum.Damage), attacker.Attribute(attribute), potency);

            double offensiveAttribute = attacker.Attribute(attribute);
            double defensiveAttribute = Attribute(GameManager.GetDefenceType(attribute));

            double maxPen = 2;
            double minPen = 0.29;

            double penDelta = offensiveAttribute / defensiveAttribute;
            double penMinMax = (maxPen - minPen) / minPen;
            double sigmaValue = Math.Exp(-0.9 * penDelta);

            double penetrationModifier = maxPen / (1 + (penMinMax * sigmaValue));

            min = (int)(rawDamage * 0.75);
            max = (int)(rawDamage * 1.25);

            min = (int)(min * penetrationModifier * ElementalModifiers(element));
            max = (int)(max * penetrationModifier * ElementalModifiers(element));
        }

        /// <summary>
        /// Using the values from the attacker's GetRawPowerRange, apply defensive modifiers, calculate total damage and take it
        /// </summary>
        /// <returns>The outgoing amount of damage</returns>
        public int ProcessDamage(CombatActor attacker, AttributeEnum attribute, int potency, int critRating, ElementEnum element = ElementEnum.None)
        {
            GetActualDamageRange(out int min, out int max, attacker, attribute, potency, element);

            int dmgDealt = RHRandom.Instance().Next(min, max);

            DealDamage(dmgDealt);

            return dmgDealt;
        }

        /// <summary>
        /// Returns a modifier to the damage based on relevent elemental affinities
        /// </summary>
        /// <param name="element">The elemental alignment to check against</param>
        /// <returns>A multiplier for the elemental type.</returns>
        public double ElementalModifiers(ElementEnum element = ElementEnum.None)
        {
            double rv = 1;
            if (element != ElementEnum.None)
            {
                if (MapManager.CurrentMap.IsOutside && EnvironmentManager.IsRaining())
                {
                    if (element.Equals(ElementEnum.Lightning)) { rv = 1.2; }
                    else if (element.Equals(ElementEnum.Fire)) { rv = 0.8; }
                }
                else if (MapManager.CurrentMap.IsOutside && EnvironmentManager.IsSnowing())
                {
                    if (element.Equals(ElementEnum.Ice)) { rv = 1.2; }
                    else if (element.Equals(ElementEnum.Lightning)) { rv = 0.8; }
                }

                if (LinkedSummon != null && _diElementalAlignment[element].Equals(ElementAlignment.Neutral))
                {
                    if (LinkedSummon.Element.Equals(element))
                    {
                        rv = 0.8;
                    }
                }

                if (_diElementalAlignment[element].Equals(ElementAlignment.Resists))
                {
                    rv = 0.8;
                }
                else if (_diElementalAlignment[element].Equals(ElementAlignment.Vulnerable))
                {
                    rv = 1.2;
                }
            }

            return rv;
        }

        /// <summary>
        /// Calculates the amount of healing done by the given action
        /// </summary>
        /// <returns>The outgoing amount of healing</returns>
        public int ProcessHealingAction(CombatActor attacker, AttributeEnum attribute, int potency)
        {
            attacker.GetRawDamageRange(out int min, out int max, attribute, potency);

            return IncreaseHealth(RHRandom.Instance().Next(min, max));
        }

        /// <summary>
        /// Removes the KnockedOut condition and sets currentHP to 1
        /// </summary>
        public void Recover()
        {
            KnockedOut = false;
            CurrentHP = 1;
        }

        #endregion

        #region StatusEffect Handling
        /// <summary>
        /// Reduce the duration of each status effect on the Actor by one
        /// If the effect's duration reaches 0, remove it, otherwise have it run
        /// any upkeep effects it may need to do.
        /// </summary>
        public void TickStatusEffects()
        {
            List<StatusEffect> toRemove = new List<StatusEffect>();
            foreach (StatusEffect b in _liStatusEffects)
            {
                b.TickDown();
                if (b.Duration == 0)
                {
                    toRemove.Add(b);
                }
                else
                {
                    if (b.EffectType == StatusTypeEnum.DoT)
                    {
                        this.Tile.GUITile.AssignEffect(ProcessDamage(b.SkillUser, b.PowerAttribute, b.Potency, 0), true);
                    }
                    if (b.EffectType == StatusTypeEnum.HoT)
                    {
                        this.Tile.GUITile.AssignEffect(ProcessHealingAction(b.SkillUser, b.PowerAttribute, b.Potency), false);
                    }
                }
            }

            foreach (StatusEffect b in toRemove)
            {
                _liStatusEffects.Remove(b);
            }
            toRemove.Clear();

            foreach (AttributeEnum e in Enum.GetValues(typeof(AttributeEnum)))
            {
                TickDownAttributeEffect(e);
            }
        }

        /// <summary>
        /// Adds the StatusEffect objectto the character's list of status effects.
        /// </summary>
        /// <param name="effect">Effect toadd</param>
        public void ApplyStatusEffect(StatusEffect effect)
        {
            if (effect.EffectType == StatusTypeEnum.DoT || effect.EffectType == StatusTypeEnum.HoT)
            {
                StatusEffect find = _liStatusEffects.Find(status => status.ID == effect.ID);
                if (find != null)
                {
                    _liStatusEffects.Remove(find);
                }
                _liStatusEffects.Add(effect);
            }
            else
            {
                foreach(KeyValuePair<AttributeEnum, string> kvp in effect.AffectedAttributes)
                {
                    AssignAttributeEffect(kvp.Key, kvp.Value, effect.Duration, effect.EffectType);
                }
                _liStatusEffects.Add(effect);
            }
        }

        private void AssignAttributeEffect(AttributeEnum e, string value, int duration, StatusTypeEnum effectType)
        {
            int modValue = (int)(Attribute(e) * 0.1);
            if (effectType == StatusTypeEnum.Debuff)
            {
                modValue *= -1;
            }

            _diEffectedAttributes[e] = new AttributeStatusEffect(modValue, duration);
        }

        private void TickDownAttributeEffect(AttributeEnum e)
        {
            AttributeStatusEffect attribute = _diEffectedAttributes[e];
            if (attribute.Duration > 0)
            {
                attribute.Duration--;
                if (attribute.Duration == 0)
                {
                    attribute.Value = 0;
                }

                _diEffectedAttributes[e] = attribute;
            }
        }
        #endregion

        public void LinkSummon(Summon s)
        {
            LinkedSummon = s;
            s.Tile = Tile;

            Tile.GUITile.LinkSummon(s);
        }

        public void UnlinkSummon()
        {
            Tile.GUITile.LinkSummon(null);
            LinkedSummon = null;
        }

        public virtual ElementEnum GetAttackElement()
        {
            ElementEnum e = _elementAttackEnum;

            if (LinkedSummon != null && e.Equals(ElementEnum.None))
            {
                e = LinkedSummon.Element;
            }
            return e;
        }

        public void SetUnique(string u)
        {
            _sUnique = " " +  u;
        }

        public void IncreaseStartPos()
        {
            if (_vStartPos.Y < CombatManager.MAX_ROW)
            {
                _vStartPos.Y++;
            }
            else
            {
                _vStartPos = new Vector2(_vStartPos.X++, 0);
            }
        }
        public void SetStartPosition(Vector2 startPos)
        {
            _vStartPos = startPos;
        }

        public void GetHP(ref double curr, ref double max)
        {
            curr = CurrentHP;
            max = MaxHP;
        }

        public int GetEvasionChance()
        {
            return (40 * Attribute(AttributeEnum.Evasion)) / (Attribute(AttributeEnum.Evasion) + 40);
        }

        public virtual bool IsSummon() { return false; }

        public bool IsActorType(CombatActorTypeEnum act) { return _eActorType == act; }
    }
}
