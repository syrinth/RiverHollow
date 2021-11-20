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
using System.Linq;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class LiteCombatActor : Actor
    {
        #region Properties
        protected const int MAX_STAT = 99;
        protected string _sUnique;

        public override string Name => String.IsNullOrEmpty(_sUnique) ? _sName : _sName + " " + _sUnique;

        private Vector2 _vStartPos;
        public Vector2 StartPos => _vStartPos;

        protected int _iCurrentHP;
        public int CurrentHP
        {
            get { return _iCurrentHP; }
            set { _iCurrentHP = value; }
        }
        public virtual int MaxHP => 20 + (int)Math.Pow(((double)StatVit / 3), 1.98);

        protected int _iCurrentMP;
        public int CurrentMP
        {
            get { return _iCurrentMP; }
            set { _iCurrentMP = value; }
        }
        public virtual int MaxMP => StatMag * 3;

        public int CurrentCharge;
        public int DummyCharge;
        public LiteCombatTile Tile;
        public GUICombatTile Location => Tile.GUITile;

        public virtual int Attack => 9;

        protected int _iStrength;
        public virtual int StatStr => _iStrength + _iBuffStr;
        protected int _iDefense;
        public virtual int StatDef => _iDefense + _iBuffDef;
        protected int _iVitality;
        public virtual int StatVit => _iVitality + _iBuffVit;
        protected int _iMagic;
        public virtual int StatMag => _iMagic + _iBuffMag;
        protected int _iResistance;
        public virtual int StatRes => _iResistance + _iBuffRes;
        protected int _iSpeed;
        public virtual int StatSpd => _iSpeed + _iBuffSpd;

        protected int _iBuffStr;
        protected int _iBuffDef;
        protected int _iBuffVit;
        protected int _iBuffMag;
        protected int _iBuffRes;
        protected int _iBuffSpd;
        protected int _iBuffCrit;
        protected int _iBuffEvade;

        protected int _iCrit = 10;
        public int CritRating => _iCrit + _iBuffCrit;

        public int Evasion => (int)(40 / (1 + 10 * (Math.Pow(Math.E, (-0.05 * StatSpd))))) + _iBuffEvade;
        public int ResistStatus => (int)(50 / (1 + 10 * (Math.Pow(Math.E, (-0.05 * StatRes)))));

        protected List<LiteMenuAction> _liActions;
        public virtual List<LiteMenuAction> AbilityList { get => _liActions; }

        protected List<LiteCombatAction> _liSpecialActions;
        public virtual List<LiteCombatAction> SpecialActions { get => _liSpecialActions; }

        protected List<StatusEffect> _liStatusEffects;
        public List<StatusEffect> LiBuffs { get => _liStatusEffects; }

        protected Dictionary<ConditionEnum, bool> _diConditions;
        public Dictionary<ConditionEnum, bool> DiConditions => _diConditions;

        private ElementEnum _elementAttackEnum;
        protected Dictionary<ElementEnum, ElementAlignment> _diElementalAlignment;
        public Dictionary<ElementEnum, ElementAlignment> DiElementalAlignment => _diElementalAlignment;

        private LiteSummon _linkedSummon;
        public LiteSummon LinkedSummon => _linkedSummon;

        public bool Counter;
        public bool GoToCounter;

        public LiteCombatActor MyGuard;
        public LiteCombatActor GuardTarget;
        protected bool _bGuard;
        public bool Guard => _bGuard;

        public bool Swapped;
        #endregion

        public LiteCombatActor() : base()
        {
            _eActorType = ActorEnum.CombatActor;
            _liSpecialActions = new List<LiteCombatAction>();
            _liActions = new List<LiteMenuAction>();
            _liStatusEffects = new List<StatusEffect>();
            _diConditions = new Dictionary<ConditionEnum, bool>
            {
                [ConditionEnum.KO] = false,
                [ConditionEnum.Poisoned] = false,
                [ConditionEnum.Silenced] = false
            };

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
            _sprBody.SetScale(LiteCombatManager.CombatScale);
            _iBodyWidth = _sprBody.Width;
            _iBodyHeight = _sprBody.Height;
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

            if (!_diConditions[ConditionEnum.KO] && IsCurrentAnimation(LiteCombatActionEnum.KO))
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
        public int ProcessAttack(LiteCombatActor attacker, int potency, int critRating, ElementEnum element = ElementEnum.None)
        {
            double compression = 0.8;
            double potencyMod = potency / 100;   //100 potency is considered an average attack
            double base_attack = attacker.Attack;  //Attack stat is either weapon damage or mod on monster str
            double StrMult = Math.Round(1 + ((double)attacker.StatStr / 4 * attacker.StatStr / MAX_STAT), 2);

            double dmg = (Math.Max(1, base_attack - StatDef) * compression * StrMult);
            dmg += ApplyResistances(dmg, element);

            if (RHRandom.Instance().Next(1, 100) <= (attacker.CritRating + critRating)) { dmg *= 2; }

            return DecreaseHealth(dmg);
        }
        public int ProcessSpell(LiteCombatActor attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            double maxDmg = (1 + potency) * 3;
            double divisor = 1 + (30 * Math.Pow(Math.E, -0.12 * (attacker.StatMag - StatRes) * Math.Round((double)attacker.StatMag / MAX_STAT, 2)));

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

        public int ProcessHealingSpell(LiteCombatActor attacker, int potency)
        {
            double maxDmg = (1 + potency) * 3;
            double divisor = 1 + (30 * Math.Pow(Math.E, -0.12 * (attacker.StatMag - StatRes) * Math.Round((double)attacker.StatMag / MAX_STAT, 2)));

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
                _diConditions[ConditionEnum.KO] = true;
                UnlinkSummon();
            }

            return iValue;
        }

        public int IncreaseHealth(int x)
        {
            int amountHealed = 0;
            if (!KnockedOut())
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

        public bool IsCritical()
        {
            return (float)CurrentHP / (float)MaxHP <= 0.25;
        }

        public void IncreaseMana(int x)
        {
            if (_iCurrentMP + x <= MaxMP)
            {
                _iCurrentMP += x;
            }
            else
            {
                _iCurrentMP = MaxMP;
            }
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
                if (--b.Duration == 0)
                {
                    toRemove.Add(b);
                    RemoveStatusEffect(b);
                }
                else
                {
                    if (b.DoT)
                    {
                        this.Tile.GUITile.AssignEffect(ProcessSpell(b.LiteCaster, b.Potency), true);
                    }
                    if (b.HoT)
                    {
                        this.Tile.GUITile.AssignEffect(ProcessHealingSpell(b.LiteCaster, b.Potency), false);
                    }
                }
            }

            foreach (StatusEffect b in toRemove)
            {
                _liStatusEffects.Remove(b);
            }
            toRemove.Clear();
        }

        /// <summary>
        /// Adds the StatusEffect objectto the character's list of status effects.
        /// </summary>
        /// <param name="b">Effect toadd</param>
        public void AddStatusEffect(StatusEffect b)
        {
            //Only one song allowed at a time so see if there is another
            //songand,if so, remove it.
            if (b.Song)
            {
                StatusEffect song = _liStatusEffects.Find(status => status.Song);
                if (song != null)
                {
                    RemoveStatusEffect(song);
                    _liStatusEffects.Remove(song);
                }
            }

            //Look to see if the status effect already exists, if so, just
            //set the duration to be the new duration. No stacking.
            StatusEffect find = _liStatusEffects.Find(status => status.Name == b.Name);
            if (find == null) { _liStatusEffects.Add(b); }
            else { find.Duration = b.Duration; }

            foreach (KeyValuePair<StatEnum, int> kvp in b.StatMods)
            {
                HandleStatBuffs(kvp);
            }

            //If the status effect provides counter, turn counter on.
            if (b.Counter) { Counter = true; }

            if (b.Guard) { _bGuard = true; }
        }

        /// <summary>
        /// Removes the status effect from the Actor
        /// </summary>
        /// <param name="b"></param>
        public void RemoveStatusEffect(StatusEffect b)
        {
            foreach (KeyValuePair<StatEnum, int> kvp in b.StatMods)
            {
                HandleStatBuffs(kvp, true);
            }

            if (b.Counter) { Counter = false; }
            if (b.Guard) { _bGuard = false; }
        }

        /// <summary>
        /// Helper to not repeat code for the Stat buffing/debuffing
        /// 
        /// Pass in the statmod kvp and an integer representing positive or negative
        /// and multiply the mod by it. If we are adding, it will remain unchanged, 
        /// if we are subtracting, the positive value will go negative.
        /// </summary>
        /// <param name="kvp">The stat to modifiy and how much</param>
        /// <param name="negative">Whether or not we need to add or remove the value</param>
        private void HandleStatBuffs(KeyValuePair<StatEnum, int> kvp, bool negative = false)
        {
            int modifier = negative ? -1 : 1;
            switch (kvp.Key)
            {
                case StatEnum.Str:
                    _iBuffStr += kvp.Value * modifier;
                    break;
                case StatEnum.Def:
                    _iBuffDef += kvp.Value * modifier;
                    break;
                case StatEnum.Vit:
                    _iBuffVit += kvp.Value * modifier;
                    break;
                case StatEnum.Mag:
                    _iBuffMag += kvp.Value * modifier;
                    break;
                case StatEnum.Res:
                    _iBuffRes += kvp.Value * modifier;
                    break;
                case StatEnum.Spd:
                    _iBuffSpd += kvp.Value * modifier;
                    break;
                case StatEnum.Crit:
                    _iBuffCrit += kvp.Value * modifier;
                    break;
                case StatEnum.Evade:
                    _iBuffEvade += kvp.Value * modifier;
                    break;
            }
        }

        public void LinkSummon(LiteSummon s)
        {
            _linkedSummon = s;
            s.Tile = Tile;

            foreach (KeyValuePair<StatEnum, int> kvp in _linkedSummon.BuffedStats)
            {
                this.HandleStatBuffs(kvp, false);
            }

            Tile.GUITile.LinkSummon(s);
        }

        public void UnlinkSummon()
        {
            if (_linkedSummon != null)
            {
                foreach (KeyValuePair<StatEnum, int> kvp in _linkedSummon.BuffedStats)
                {
                    this.HandleStatBuffs(kvp, true);
                }
            }
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

        public bool CanCast(int x)
        {
            return x <= CurrentMP;
        }

        public void SetUnique(string u)
        {
            _sUnique = u;
        }

        public bool KnockedOut()
        {
            return _diConditions[ConditionEnum.KO];
        }

        public bool Poisoned()
        {
            return _diConditions[ConditionEnum.Poisoned];
        }

        public bool Silenced()
        {
            return _diConditions[ConditionEnum.Silenced];
        }

        public void ChangeConditionStatus(ConditionEnum c, bool setTo)
        {
            _diConditions[c] = setTo;
        }

        public void ClearConditions()
        {
            foreach (ConditionEnum condition in Enum.GetValues(typeof(ConditionEnum)))
            {
                ChangeConditionStatus(condition, false);
            }
        }

        public void IncreaseStartPos()
        {
            if (_vStartPos.Y <LiteCombatManager.MAX_ROW)
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

        public void GetMP(ref double curr, ref double max)
        {
            curr = _iCurrentMP;
            max = MaxMP;
        }

        public virtual bool IsSummon() { return false; }
    }
}
