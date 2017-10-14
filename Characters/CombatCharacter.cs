using Adventure.Game_Managers;
using Adventure.Items;
using Adventure.SpriteAnimations;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Characters
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

        public void Draw(Rectangle _rectangle)
        {

        }

        public override void Update(GameTime theGameTime)
        {
            //base.Update(theGameTime);
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
