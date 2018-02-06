using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using System;

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

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
        }

        public void Draw(SpriteBatch spriteBatch, bool useLayerDepth = true)
        {
            _sprite.Draw(spriteBatch, useLayerDepth);
        }

        public void DecreaseHealth(int offensiveStat, int dmgMod)
        {
            int dmg = 1;
            int delta = StatDef - offensiveStat;
            if (delta > 0)
            {
                dmg = delta * dmgMod;
            }
            else { dmg = Math.Max(1, (int)(delta * dmgMod * 0.5)); }
            _currentHP -= dmg;
            if (_currentHP <= 0)
            {
                CombatManager.Kill(this);
            }
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
