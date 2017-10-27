using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.CombatStuff;

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
        public virtual int StatDmg { get => _statDmg; }
        protected int _statDef;
        public virtual int StatDef { get => _statDef; }
        protected int _statHP;
        public virtual int StatHP { get => _statHP; }
        protected int _statMagic;
        public virtual int StatMagic { get => _statMagic; }
        protected int _statSpd;
        public virtual int StatSpd { get => _statSpd; }

        protected List<Ability> _abilityList;
        public List<Ability> AbilityList { get => _abilityList; }
        #endregion

        public CombatCharacter() : base()
        {
            _abilityList = new List<Ability>();
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
        }

        public void Draw(SpriteBatch spriteBatch, bool useLayerDepth = true)
        {
            _sprite.Draw(spriteBatch, useLayerDepth);
        }

        public void DecreaseHealth(int x)
        {
            _currentHP -= x;
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
                _currentHP = _statHP;
            }
        }
    }
}
