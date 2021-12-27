using Microsoft.Xna.Framework;
using RiverHollow.Actors.CombatStuff;
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

        protected int _iCurrentHP;
        public int CurrentHP
        {
            get { return _iCurrentHP; }
            set { _iCurrentHP = value; }
        }
        public virtual int MaxHP => 20 + (int)Math.Pow(((double)_diAttributes[AttributeEnum.MaxHealth] / 3), 1.98);

        public int CurrentCharge;
        public int DummyCharge;
        public LiteCombatTile Tile;
        public GUICombatTile Location => Tile.GUITile;

        protected Dictionary<AttributeEnum, int> _diAttributes;
        protected Dictionary<AttributeEnum, AttributeStatusEffect> _diEffectedAttributes;

        protected int _iCrit = 10;
        public int CritRating => _iCrit;

        protected List<CombatAction> _liActions;
        public virtual List<CombatAction> Actions { get => _liActions; }

        protected List<CombatAction> _liSpecialActions;
        public virtual List<CombatAction> SpecialActions { get => _liSpecialActions; }

        protected List<StatusEffect> _liStatusEffects;
        public List<StatusEffect> StatusEffects { get => _liStatusEffects; }

        private ElementEnum _elementAttackEnum;
        protected Dictionary<ElementEnum, ElementAlignment> _diElementalAlignment;
        public Dictionary<ElementEnum, ElementAlignment> DiElementalAlignment => _diElementalAlignment;

        private LiteSummon _linkedSummon;
        public LiteSummon LinkedSummon => _linkedSummon;

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
            _sprBody.AddAnimation(LiteCombatActionEnum.Idle, (xCrawl * frameSize.Width), 0, frameSize, 2, 0.5f);
            xCrawl += 2;
            _sprBody.AddAnimation(LiteCombatActionEnum.Cast, (xCrawl * frameSize.Width), 0, frameSize, 2, 0.4f);
            xCrawl += 2;
            _sprBody.AddAnimation(LiteCombatActionEnum.Hurt, (xCrawl * frameSize.Width), 0, frameSize, 1, 0.5f);
            xCrawl += 1;
            _sprBody.AddAnimation(LiteCombatActionEnum.Attack, (xCrawl * frameSize.Width), 0, frameSize, 1, 0.3f);
            xCrawl += 1;
            _sprBody.AddAnimation(LiteCombatActionEnum.Critical, (xCrawl * frameSize.Width), 0, frameSize, 2, 0.9f);
            xCrawl += 2;
            _sprBody.AddAnimation(LiteCombatActionEnum.KO, (xCrawl * frameSize.Width), 0, frameSize, 1, 0.5f);

            _sprBody.PlayAnimation(LiteCombatActionEnum.Idle);
            _sprBody.SetScale((int)GameManager.NORMAL_SCALE);
            _iBodyWidth = frameSize.Width * (int)GameManager.NORMAL_SCALE;
            _iBodyHeight = frameSize.Height * (int)GameManager.NORMAL_SCALE;
        }

        public override void Update(GameTime theGameTime)
        {
            //Finished being hit, determine action
            if (IsCurrentAnimation(LiteCombatActionEnum.Hurt) && BodySprite.GetPlayCount() >= 1)
            {
                if (CurrentHP == 0) { Tile.PlayAnimation(LiteCombatActionEnum.KO); }
                else if (IsCritical()) { Tile.PlayAnimation(LiteCombatActionEnum.Critical); }
                else { Tile.PlayAnimation(LiteCombatActionEnum.Idle); }
            }

            if (!KnockedOut && IsCurrentAnimation(LiteCombatActionEnum.KO))
            {
                if (IsCritical()) { Tile.PlayAnimation(LiteCombatActionEnum.Critical); }
                else { Tile.PlayAnimation(LiteCombatActionEnum.Idle); }
            }

            if (IsCurrentAnimation(LiteCombatActionEnum.Critical) && !IsCritical())
            {
                Tile.PlayAnimation(LiteCombatActionEnum.Idle);
            }

            if (_linkedSummon != null)
            {
                _linkedSummon.Update(theGameTime);
            }
        }

        public virtual void GoToIdle()
        {
            if (IsCritical()) { base.PlayAnimation(VerbEnum.Critical); }
            else { base.PlayAnimation(VerbEnum.Walk); }
        }

        public virtual GUIImage GetIcon() { return null; }

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

        public void GetDamageRange(out int min, out int max, CombatAction action)
        {
            AttributeEnum attribute = action.DamageAttribue;
            double potencyMod = action.Potency / 100;   //100 potency is considered an average attack
            double base_damage = Attribute(AttributeEnum.Damage);  //Damage is the most important attribute for raw damage
            double AttributeMultiplier = Math.Round(1 + ((double)Attribute(attribute) / 4 * Attribute(attribute) / MAX_STAT), 2);

            double dmg = (Math.Max(1, base_damage) * potencyMod * AttributeMultiplier);

            min = (int)(dmg * 0.75);
            max = (int)(dmg * 1.25);
        }

        /// <summary>
        /// Calculates the damage to be dealt against the actor.
        /// 
        /// Run the damage equation against the defender, then apply any 
        /// relevant elemental resistances.
        /// 
        /// Finally, roll against the crit rating. Rolling higher than the 
        /// rating on a percentile roll means no crit. Crit Rating 10 means
        /// roll 10 or less
        /// </summary>
        /// <param name="attacker">Who is attacking</param>
        /// <param name="potency">The potency of the attack</param>
        /// <param name="element">any associated element</param>
        /// <returns></returns>
        public int ProcessAttack(CombatActor attacker, int potency, int critRating, ElementEnum element = ElementEnum.None)
        {
            double compression = 0.8;
            double potencyMod = potency / 100;   //100 potency is considered an average attack
            double base_attack = attacker.Attribute(AttributeEnum.Damage);  //Attack stat is either weapon damage or mod on monster str
            double StrMult = Math.Round(1 + ((double)attacker.Attribute(AttributeEnum.Strength) / 4 * attacker.Attribute(AttributeEnum.Strength) / MAX_STAT), 2);

            double dmg = (Math.Max(1, base_attack - Attribute(AttributeEnum.Defense)) * compression * StrMult);
            dmg += ApplyResistances(dmg, element);

            if (RHRandom.Instance().Next(1, 100) <= (attacker.CritRating + critRating)) { dmg *= 2; }

            return DecreaseHealth(dmg);
        }
        public int ProcessSpell(CombatActor attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            double maxDmg = (1 + potency) * 3;
            double divisor = 1 + (30 * Math.Pow(Math.E, -0.12 * (attacker.Attribute(AttributeEnum.Magic) - Attribute(AttributeEnum.Resistance)) * Math.Round((double)attacker.Attribute(AttributeEnum.Magic) / MAX_STAT, 2)));

            double damage = Math.Round(maxDmg / divisor);
            damage += ApplyResistances(damage, element);

            return DecreaseHealth(damage);
        }
        public double ApplyResistances(double dmg, ElementEnum element = ElementEnum.None)
        {
            double modifiedDmg = 0;
            if (element != ElementEnum.None)
            {
                if (MapManager.CurrentMap.IsOutside && EnvironmentManager.IsRaining())
                {
                    if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Fire)) { modifiedDmg += (dmg * 0.8) - dmg; }
                }
                else if (MapManager.CurrentMap.IsOutside && EnvironmentManager.IsSnowing())
                {
                    if (element.Equals(ElementEnum.Ice)) { modifiedDmg += (dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (dmg * 0.8) - dmg; }
                }

                if (_linkedSummon != null && _diElementalAlignment[element].Equals(ElementAlignment.Neutral))
                {
                    if (_linkedSummon.Element.Equals(element))
                    {
                        modifiedDmg += (dmg * 0.8) - dmg;
                    }
                }

                if (_diElementalAlignment[element].Equals(ElementAlignment.Resists))
                {
                    modifiedDmg += (dmg * 0.8) - dmg;
                }
                else if (_diElementalAlignment[element].Equals(ElementAlignment.Vulnerable))
                {
                    modifiedDmg += (dmg * 1.2) - dmg;
                }
            }

            return modifiedDmg;
        }

        public int ProcessHealingSpell(CombatActor attacker, int potency)
        {
            double maxDmg = (1 + potency) * 3;
            double divisor = 1 + (30 * Math.Pow(Math.E, -0.12 * (attacker.Attribute(AttributeEnum.Magic) - Attribute(AttributeEnum.Resistance)) * Math.Round((double)attacker.Attribute(AttributeEnum.Magic) / MAX_STAT, 2)));

            int damage = (int)Math.Round(maxDmg / divisor);

            return IncreaseHealth(damage);
        }
        public virtual GUISprite GetSprite()
        {
            return Tile.GUITile.CharacterSprite;
        }

        public virtual int DecreaseHealth(double value)
        {
            int iValue = (int)Math.Round(value);
            _iCurrentHP -= (_iCurrentHP - iValue >= 0) ? iValue : _iCurrentHP;
            Tile.PlayAnimation(LiteCombatActionEnum.Hurt);
            if (_iCurrentHP == 0)
            {
                KnockedOut = true;
                UnlinkSummon();
            }

            return iValue;
        }

        public int IncreaseHealth(int x)
        {
            int amountHealed = 0;
            if (!KnockedOut)
            {
                amountHealed = x;
                if (_iCurrentHP + x <= MaxHP)
                {
                    _iCurrentHP += x;
                }
                else
                {
                    amountHealed = MaxHP - _iCurrentHP;
                    _iCurrentHP = MaxHP;
                }
            }

            return amountHealed;
        }

        /// <summary>
        /// Removes the KnockedOut condition and sets currentHP to 1
        /// </summary>
        public void Recover()
        {
            KnockedOut = false;
            _iCurrentHP = 1;
        }

        public bool IsCritical()
        {
            return (float)CurrentHP / (float)MaxHP <= 0.25;
        }

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
                        this.Tile.GUITile.AssignEffect(ProcessSpell(b.SkillUser, b.Potency), true);
                    }
                    if (b.EffectType == StatusTypeEnum.HoT)
                    {
                        this.Tile.GUITile.AssignEffect(ProcessHealingSpell(b.SkillUser, b.Potency), false);
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

        public void LinkSummon(LiteSummon s)
        {
            _linkedSummon = s;
            s.Tile = Tile;

            Tile.GUITile.LinkSummon(s);
        }

        public void UnlinkSummon()
        {
            Tile.GUITile.LinkSummon(null);
            _linkedSummon = null;
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
            curr = _iCurrentHP;
            max = MaxHP;
        }

        public virtual bool IsSummon() { return false; }
    }
}
