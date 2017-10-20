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

namespace RiverHollow.Characters
{
    public class CombatCharacter : Character
    {
        protected int _hp;
        public int HP
        {
            get { return _hp; }
            set { _hp = value; }
        }
        protected int _currentHP;
        public int CurrentHP
        {
            get { return _currentHP; }
            set { _currentHP = value; }
        }

        protected int _dmg;
        protected int Dmg { get => _dmg; }

        protected CharacterClass _class;
        public CharacterClass CharacterClass { get => _class; }

        protected List<Ability> _abilityList;
        public List<Ability> AbilityList { get => _abilityList; }

        public CombatCharacter() : base()
        {
            _abilityList = new List<Ability>();
        }

        public void SetMaxHp(int x)
        {
            _hp = x;
            _currentHP = x;
        }
        public void SetClass(CharacterClass x)
        {
            _class = x;
            foreach(Ability a in _class.AbilityList)
            {
                _abilityList.Add(a);
            }
        }

        public void Draw(SpriteBatch spriteBatch, Rectangle r)
        {
            _sprite.Draw(spriteBatch, r);
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
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
            if (_currentHP + x <= _hp)
            {
                _currentHP += x;
            }
            else
            {
                _currentHP = _hp;
            }
        }
    }
}
