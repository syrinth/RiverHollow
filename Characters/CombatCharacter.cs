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

namespace RiverHollow.Characters
{
    public class CombatCharacter : Character
    {
        protected int _maxHP = 10;
        public int MaxHitPoints
        {
            get { return _maxHP; }
            set { _maxHP = value; }
        }
        protected int _hp = 10;
        public int HitPoints
        {
            get { return _hp; }
            set { _hp = value; }
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
            _hp -= x;
            if (_hp <= 0)
            {
                CombatManager.Kill(this);
            }
        }

        public void IncreaseHealth(int x)
        {
            if (_hp + x <= _maxHP)
            {
                _hp += x;
            }
            else
            {
                _hp = _maxHP;
            }
        }
    }
}
