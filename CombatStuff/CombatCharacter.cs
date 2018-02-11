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

        protected List<Ability> _abilityList;
        public List<Ability> AbilityList { get => _abilityList; }

        protected List<Buff> _liBuffs;
        public List<Buff> LiBuffs { get => _liBuffs; }
        #endregion

        public CombatCharacter() : base()
        {
            _abilityList = new List<Ability>();
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
            int dmg = 1;
            int delta = StatDef - offensiveStat;

            if (delta <= 0) { dmg = Math.Abs(delta) * dmgMod; }
            else { dmg = Math.Max(1, (int)(delta/dmgMod)); }

            _currentHP -= (_currentHP - dmg >= 0) ? dmg : _currentHP;
            PlayAnimation("Hurt");
            if (_currentHP == 0)
            {
                CombatManager.Kill(this);
            }

            return dmg;
        }

        public void IncreaseHealth(int x)
        {
            if (_currentHP + x <= MaxHP)
            {
                _currentHP += x;
            }
            else
            {
                _currentHP = MaxHP;
            }
        }

        public void TickBuffs()
        {
            List<Buff> toRemove = new List<Buff>();
            foreach(Buff b in LiBuffs)
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
                LiBuffs.Remove(b);
            }
            toRemove.Clear();
        }

        public void AddBuff(Buff b)
        {
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
    }
}
