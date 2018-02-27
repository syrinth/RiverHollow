using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System;
using RiverHollow.SpriteAnimations;

namespace RiverHollow.Characters.CombatStuff
{
    public class CombatCharacter : Character
    {
        #region Properties
        protected int _currentHP;
        public int CurrentHP
        {
            get { return _currentHP; }
            set { _currentHP = value; }
        }
        public int MaxHP {  get => StatHP * 3; }

        protected int _currentMP;
        public int CurrentMP
        {
            get { return _currentMP; }
            set { _currentMP = value; }
        }
        public int MaxMP {  get => StatMagic * 3; }

        public int Initiative;

        protected int _statDmg;
        public virtual int StatDmg { get => _statDmg + _buffDmg; }
        protected int _statDef;
        public virtual int StatDef { get => _statDef + _buffDef; }
        protected int _statHP;
        public virtual int StatHP { get => _statHP; }
        protected int _statMagic;
        public virtual int StatMagic { get => _statMagic + _buffMagic; }
        protected int _statSpd;
        public virtual int StatSpd { get => _statSpd + _buffSpd; }

        protected int _buffDmg;
        protected int _buffDef;
        protected int _buffMagic;
        protected int _buffSpd;

        protected List<MenuAction> _liActions;
        public virtual List<MenuAction> AbilityList { get => _liActions; }

        protected List<CombatAction> _liSpells;
        public virtual List<CombatAction> SpellList { get => _liSpells; }

        protected List<Buff> _liBuffs;
        public List<Buff> LiBuffs { get => _liBuffs; }
        #endregion

        public CombatCharacter() : base()
        {
            _characterType = CharacterEnum.CombatCharacter;
            _liSpells = new List<CombatAction>();
            _liActions = new List<MenuAction>();
            _liBuffs = new List<Buff>();
        }

        public void LoadContent(string texture)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(texture));
            int xCrawl = 0;
            int frameWidth = 24;
            int frameHeight = 32;
            _sprite.AddAnimation("Walk", frameWidth, frameHeight, 2, 0.5f, (xCrawl * frameWidth), 32);
            xCrawl += 2;
            _sprite.AddAnimation("Cast", frameWidth, frameHeight, 2, 0.2f, (xCrawl * frameWidth), 32);
            xCrawl += 2;
            _sprite.AddAnimation("Hurt", frameWidth, frameHeight, 1, 0.5f, (xCrawl * frameWidth), 32, "Walk");
            xCrawl += 1;
            _sprite.AddAnimation("Attack", frameWidth, frameHeight, 1, 0.3f, (xCrawl * frameWidth), 32);
            xCrawl += 1;
            _sprite.AddAnimation("Critical", frameWidth, frameHeight, 2, 0.5f, (xCrawl * frameWidth), 32);
            xCrawl += 2;
            _sprite.AddAnimation("KO", frameWidth, frameHeight, 1, 0.5f, (xCrawl * frameWidth), 32);

            _sprite.SetCurrentAnimation("Walk");
            _sprite.SetScale(5);
            _width = _sprite.Width;
            _height = _sprite.Height;
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
            if (CurrentHP > 0)
            {
                if ((float)CurrentHP / (float)MaxHP <= 0.25 && IsCurrentAnimation("Walk"))
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
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, false);
        }

        public int DecreaseHealth(int offensiveStat, int dmgMod)
        {
            int iATK = offensiveStat* dmgMod;
            double power = Math.Pow(((double)iATK / (double)StatDef), 2);
            double dMult = Math.Min(2, Math.Max(0.01, power));
            int dmg = (int)Math.Max(1, iATK * dMult);

            _currentHP -= (_currentHP - dmg >= 0) ? dmg : _currentHP;
            PlayAnimation("Hurt");
            if (_currentHP == 0)
            {
                CombatManager.Kill(this);
            }

            return dmg;
        }

        public int IncreaseHealth(int x)
        {
            int amountHealed = x;
            if (_currentHP + x <= MaxHP)
            {
                _currentHP += x;
            }
            else
            {
                amountHealed = MaxHP - _currentHP;
                _currentHP = MaxHP;
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
                            case "Dmg":
                                _buffDmg -= kvp.Value;
                                break;
                            case "Def":
                                _buffDef -= kvp.Value;
                                break;
                            case "Magic":
                                _buffMagic -= kvp.Value;
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
                    case "Dmg":
                        _buffDmg += kvp.Value;
                        break;
                    case "Def":
                        _buffDef += kvp.Value;
                        break;
                    case "Magic":
                        _buffMagic += kvp.Value;
                        break;
                    case "Spd":
                        _buffSpd += kvp.Value;
                        break;
                }
            }
        }

        public bool CanCast(int x)
        {
            return x <= CurrentMP;
        }
    }
}
