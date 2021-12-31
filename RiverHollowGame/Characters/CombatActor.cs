using Microsoft.Xna.Framework;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public abstract class CombatActor : Actor
    {
        #region Properties
        protected const int MAX_STAT = 99;
        protected string _sUnique;

        private Vector2 _vStartPos;
        public Vector2 StartPosition => _vStartPos;

        public int CurrentHP { get; protected set; }
        public virtual int MaxHP => 20 + (int)Math.Pow(((double)_diAttributes[AttributeEnum.MaxHealth] / 3), 1.98);

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
        public LiteSummon LinkedSummon { get; private set; }

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
        }

        public virtual void LoadContent(string texture)
        {
            _sprBody = new AnimatedSprite(texture.Replace(" ", ""));
            int xCrawl = 0;
            RHSize frameSize = new RHSize(24, 32);
            _sprBody.AddAnimation(CombatActionEnum.Idle, (xCrawl * frameSize.Width), 0, frameSize, 2, 0.5f);
            xCrawl += 2;
            _sprBody.AddAnimation(CombatActionEnum.Cast, (xCrawl * frameSize.Width), 0, frameSize, 2, 0.4f);
            xCrawl += 2;
            _sprBody.AddAnimation(CombatActionEnum.Hurt, (xCrawl * frameSize.Width), 0, frameSize, 1, 0.5f);
            xCrawl += 1;
            _sprBody.AddAnimation(CombatActionEnum.Attack, (xCrawl * frameSize.Width), 0, frameSize, 1, 0.3f);
            xCrawl += 1;
            _sprBody.AddAnimation(CombatActionEnum.Critical, (xCrawl * frameSize.Width), 0, frameSize, 2, 0.9f);
            xCrawl += 2;
            _sprBody.AddAnimation(CombatActionEnum.KO, (xCrawl * frameSize.Width), 0, frameSize, 1, 0.5f);

            _sprBody.PlayAnimation(CombatActionEnum.Idle);
            _sprBody.SetScale((int)GameManager.NORMAL_SCALE);
            _iBodyWidth = frameSize.Width * (int)GameManager.NORMAL_SCALE;
            _iBodyHeight = frameSize.Height * (int)GameManager.NORMAL_SCALE;
        }

        public override void Update(GameTime theGameTime)
        {
            //Finished being hit, determine action
            if (IsCurrentAnimation(CombatActionEnum.Hurt) && BodySprite.GetPlayCount() >= 1)
            {
                if (CurrentHP == 0) { Tile.PlayAnimation(CombatActionEnum.KO); }
                else if (IsCritical()) { Tile.PlayAnimation(CombatActionEnum.Critical); }
                else { Tile.PlayAnimation(CombatActionEnum.Idle); }
            }

            if (!KnockedOut && IsCurrentAnimation(CombatActionEnum.KO))
            {
                if (IsCritical()) { Tile.PlayAnimation(CombatActionEnum.Critical); }
                else { Tile.PlayAnimation(CombatActionEnum.Idle); }
            }

            if (IsCurrentAnimation(CombatActionEnum.Critical) && !IsCritical())
            {
                Tile.PlayAnimation(CombatActionEnum.Idle);
            }

            if (LinkedSummon != null)
            {
                LinkedSummon.Update(theGameTime);
            }
        }

        public virtual void GoToIdle()
        {
            if (IsCritical()) { base.PlayAnimation(VerbEnum.Critical); }
            else { base.PlayAnimation(VerbEnum.Walk); }
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
        public virtual int DecreaseHealth(int value)
        {
            CurrentHP -= (CurrentHP - value >= 0) ? value : CurrentHP;
            Tile.PlayAnimation(CombatActionEnum.Hurt);
            if (CurrentHP == 0)
            {
                KnockedOut = true;
                UnlinkSummon();
            }

            return value;
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
        /// <summary>
        /// Retrieves the raw, unmodified damage being dealt by this action
        /// </summary>
        /// <param name="min">The minimum damage to be dealt</param>
        /// <param name="max">The maximum damage to be dealt</param>
        /// <param name="attribute">The Action's Damage Attribute</param>
        /// <param name="potency">The Potency of the Action</param>
        public void GetRawPowerRange(out int min, out int max, AttributeEnum attribute, int potency)
        {
            double potencyMod = potency / 100.0;   //100 potency is considered an average attack
            double base_damage = Attribute(AttributeEnum.Damage);  //Damage is the most important attribute for raw damage
            double AttributeMultiplier = Math.Round(1 + ((double)Attribute(attribute) / 4 * Attribute(attribute) / MAX_STAT), 2);

            double dmg = (Math.Max(1, base_damage) * potencyMod * AttributeMultiplier);

            min = (int)(dmg * 0.75);
            max = (int)(dmg * 1.25);
        }

        /// <summary>
        /// Using the values from the attacker's GetRawPowerRange, apply defensive modifiers, calculate total damage and take it
        /// </summary>
        /// <returns>The outgoing amount of damage</returns>
        public int ProcessDamage(CombatActor attacker, AttributeEnum attribute, int potency, int critRating, ElementEnum element = ElementEnum.None)
        {
            attacker.GetRawPowerRange(out int min, out int max, attribute, potency);

            double dmgDealt = RHRandom.Instance().Next(min, max);

            double offensiveAttribute = attacker.Attribute(attribute);
            double defensiveAttribute = Attribute(GameManager.GetDefenseType(attribute));

            //The minimum penetration modifier is 0.2, the maximum is 2
            double penetrationModifier = Math.Min(Math.Max(0.2, offensiveAttribute / defensiveAttribute), 2);
            int finalDamage = (int)(dmgDealt * penetrationModifier * ElementalModifiers(element));

            DecreaseHealth(finalDamage);

            return finalDamage;
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
            attacker.GetRawPowerRange(out int min, out int max, attribute, potency);

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
                foreach(KeyValuePair<AttributeEnum, int> kvp in effect.AttributeEffects)
                {
                    AssignAttributeEffect(kvp.Key, kvp.Value, effect.Duration);
                }
            }
        }

        private void AssignAttributeEffect(AttributeEnum e, int value, int duration)
        {
            _diEffectedAttributes[e] = new AttributeStatusEffect(value, duration);
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

        public void LinkSummon(LiteSummon s)
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
            _sUnique = u;
        }

        public void IncreaseStartPos()
        {
            if (_vStartPos.Y <CombatManager.MAX_ROW)
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

        public virtual bool IsSummon() { return false; }
    }
}
