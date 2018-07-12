using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System;
using RiverHollow.SpriteAnimations;
using RiverHollow.Game_Managers.GUIObjects;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.CombatStuff;
using RiverHollow.Misc;

namespace RiverHollow.Characters.CombatStuff
{
    public class CombatCharacter : Character
    {
        #region Properties
        const int MAX_STAT = 99;
        protected string _sUnique;

        public override string Name => String.IsNullOrEmpty(_sUnique) ? _sName : _sName + " " + _sUnique;

        protected int _currentHP;
        public int CurrentHP
        {
            get { return _currentHP; }
            set { _currentHP = value; }
        }
        public int MaxHP {  get => StatVit * 3; }

        protected int _currentMP;
        public int CurrentMP
        {
            get { return _currentMP; }
            set { _currentMP = value; }
        }
        public int MaxMP {  get => StatMag * 3; }

        public int CurrentCharge;
        public CombatManager.CombatTile Tile;
        public GUICmbtTile Location => Tile.GUITile;

        public virtual int Attack => (int)(StatStr*0.5);

        protected int _statStr;
        public virtual int StatStr { get => _statStr + _buffStr; }
        protected int _statDef;
        public virtual int StatDef { get => _statDef + _buffDef; }
        protected int _statVit;
        public virtual int StatVit { get => _statVit + _buffVit; }
        protected int _statMag;
        public virtual int StatMag { get => _statMag + _buffMag; }
        protected int _statRes;
        public virtual int StatRes { get => _statRes + _buffRes; }
        protected int _statSpd;
        public virtual int StatSpd { get => _statSpd + _buffSpd; }

        protected int _buffStr;
        protected int _buffDef;
        protected int _buffVit;
        protected int _buffMag;
        protected int _buffRes;
        protected int _buffSpd;

        public int Evasion => (int)(40/(1+10*(Math.Pow(Math.E,(-0.05*StatSpd)))));
        public int ResistStatus => (int)(50 / (1 + 10 * (Math.Pow(Math.E, (-0.05 * StatRes)))));

        protected List<MenuAction> _liActions;
        public virtual List<MenuAction> AbilityList { get => _liActions; }

        protected List<CombatAction> _liSpells;
        public virtual List<CombatAction> SpellList { get => _liSpells; }

        protected List<Buff> _liBuffs;
        public List<Buff> LiBuffs { get => _liBuffs; }

        protected Dictionary<ConditionEnum, bool> _diConditions;
        public Dictionary<ConditionEnum, bool> DiConditions => _diConditions;

        protected Dictionary<ElementEnum, ElementAlignment> _diElementalAlignment;
        public Dictionary<ElementEnum, ElementAlignment> DiElementalAlignment => _diElementalAlignment;

        private Summon _linkedSummon;
        public Summon LinkedSummon =>  _linkedSummon;
        #endregion

        public CombatCharacter() : base()
        {
            _characterType = CharacterEnum.CombatCharacter;
            _liSpells = new List<CombatAction>();
            _liActions = new List<MenuAction>();
            _liBuffs = new List<Buff>();
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

        //ONLY USE THIS FOR CALCULATING TURNS
        public CombatCharacter(CombatCharacter c)
        {
            _sName = c.Name;
            CurrentCharge = c.CurrentCharge;
            _diConditions = c._diConditions;
            _statStr = c.StatStr;
            _statDef = c.StatDef;
            _statVit = c.StatVit;
            _statMag = c.StatMag;
            _statRes = c.StatRes;
            _statSpd = c.StatSpd;
        }

        public void LoadContent(string texture)
        {
            _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(texture));
            int xCrawl = 0;
            int frameWidth = 24;
            int frameHeight = 32;
            _bodySprite.AddAnimation("Walk", frameWidth, frameHeight, 2, 0.5f, (xCrawl * frameWidth), 32);
            xCrawl += 2;
            _bodySprite.AddAnimation("Cast", frameWidth, frameHeight, 2, 0.2f, (xCrawl * frameWidth), 32);
            xCrawl += 2;
            _bodySprite.AddAnimation("Hurt", frameWidth, frameHeight, 1, 0.5f, (xCrawl * frameWidth), 32, "Walk");
            xCrawl += 1;
            _bodySprite.AddAnimation("Attack", frameWidth, frameHeight, 1, 0.3f, (xCrawl * frameWidth), 32);
            xCrawl += 1;
            _bodySprite.AddAnimation("Critical", frameWidth, frameHeight, 2, 0.5f, (xCrawl * frameWidth), 32);
            xCrawl += 2;
            _bodySprite.AddAnimation("KO", frameWidth, frameHeight, 1, 0.5f, (xCrawl * frameWidth), 32);

            _bodySprite.SetCurrentAnimation("Walk");
            _bodySprite.SetScale(CombatManager.CombatScale);
            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
            if (CurrentHP > 0)
            {
                if ((float)CurrentHP / (float)MaxHP <= 0.25 && (IsCurrentAnimation("Walk") || IsCurrentAnimation("KO")))
                {
                    PlayAnimation("Critical");
                }
                else if ((float)CurrentHP / (float)MaxHP > 0.25 && IsCurrentAnimation("Critical"))
                {
                    PlayAnimation("Walk");
                }
            }
            else
            {
                if (IsCurrentAnimation("Walk"))
                {
                    PlayAnimation("KO");
                }
            }

            if (_linkedSummon != null)
            {
                _linkedSummon.Update(theGameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _bodySprite.Draw(spriteBatch, false);
            if(_linkedSummon != null)
            {
                _linkedSummon.Draw(spriteBatch);
            }
        }

        public int ProcessAttack(CombatCharacter attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            int base_attack = attacker.Attack;

            double power = Math.Pow(((double)base_attack / (double)StatDef), 2) + attacker.StatStr;
            double dMult = Math.Min(2, Math.Max(0.01, power));
            int dmg = (int)Math.Max(1, base_attack * dMult); 

            dmg += ApplyResistances(dmg, element);
            return DecreaseHealth(dmg);
        }
        public int ProcessSpell(CombatCharacter attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            int base_damage = (attacker.StatMag - StatRes/2) * potency;
            int bonus = 0;

            base_damage += ApplyResistances(base_damage, element);
            return DecreaseHealth(base_damage);
        }
        public int ApplyResistances(int dmg, ElementEnum element = ElementEnum.None)
        {
            int modifiedDmg = 0;
            if (element != ElementEnum.None)
            {
                if (MapManager.CurrentMap.IsOutside && GameCalendar.IsRaining())
                {
                    if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (int)(dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Fire)) { modifiedDmg += (int)(dmg * 0.8) - dmg; }
                }
                else if (MapManager.CurrentMap.IsOutside && GameCalendar.IsSnowing())
                {
                    if (element.Equals(ElementEnum.Ice)) { modifiedDmg += (int)(dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (int)(dmg * 0.8) - dmg; }
                }

                if (_diElementalAlignment[element].Equals(ElementAlignment.Resists))
                {
                    modifiedDmg += (int)(dmg * 0.8) - dmg;
                }
                else if (_diElementalAlignment[element].Equals(ElementAlignment.Vulnerable))
                {
                    modifiedDmg += (int)(dmg * 1.2) - dmg;
                }
            }

            return modifiedDmg;
        }

        public int DecreaseHealth(int value)
        {
            _currentHP -= (_currentHP - value >= 0) ? value : _currentHP;
            PlayAnimation("Hurt");
            if (_currentHP == 0)
            {
                _diConditions[ConditionEnum.KO] = true;
                CombatManager.Kill(this);
            }

            return value;
        }

        public int IncreaseHealth(int x)
        {
            int amountHealed = 0;
            if (!KnockedOut())
            {
                amountHealed = x;
                if (_currentHP + x <= MaxHP)
                {
                    _currentHP += x;
                }
                else
                {
                    amountHealed = MaxHP - _currentHP;
                    _currentHP = MaxHP;
                }
            }

            return amountHealed;
        }

        public void IncreaseMana(int x)
        {
            if (_currentMP + x <= MaxMP)
            {
                _currentMP += x;
            }
            else
            {
                _currentMP = MaxMP;
            }
        }

        public void TickBuffs()
        {
            List<Buff> toRemove = new List<Buff>();
            foreach(Buff b in _liBuffs)
            {
                if(--b.Duration == 0)
                {
                    toRemove.Add(b);
                    foreach (KeyValuePair<string, int> kvp in b.StatMods)
                    {
                        switch (kvp.Key)
                        {
                            case "Str":
                                _buffStr -= kvp.Value;
                                break;
                            case "Def":
                                _buffDef -= kvp.Value;
                                break;
                            case "Vit":
                                _buffVit -= kvp.Value;
                                break;
                            case "Mag":
                                _buffMag -= kvp.Value;
                                break;
                            case "Res":
                                _buffRes -= kvp.Value;
                                break;
                            case "Spd":
                                _buffSpd -= kvp.Value;
                                break;
                        }
                    }
                }
            }

            foreach (Buff b in toRemove)
            {
                _liBuffs.Remove(b);
            }
            toRemove.Clear();
        }

        public void AddBuff(Buff b)
        {
            Buff find = _liBuffs.Find(buff => buff.Name == b.Name);
            if (find == null) { _liBuffs.Add(b); }
            else { find.Duration += b.Duration; }
            
            foreach(KeyValuePair<string, int> kvp in b.StatMods)
            {
                switch (kvp.Key)
                {
                    case "Str":
                        _buffStr -= kvp.Value;
                        break;
                    case "Def":
                        _buffDef -= kvp.Value;
                        break;
                    case "Vit":
                        _buffVit -= kvp.Value;
                        break;
                    case "Mag":
                        _buffMag -= kvp.Value;
                        break;
                    case "Res":
                        _buffRes -= kvp.Value;
                        break;
                    case "Spd":
                        _buffSpd -= kvp.Value;
                        break;
                }
            }
        }

        public void LinkSummon(Summon s)
        {
            _linkedSummon = s;
            _linkedSummon.Position = Position - new Vector2(100, 100);
        }

        public void UnlinkSummon()
        {
            _linkedSummon = null;
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
    }
}
