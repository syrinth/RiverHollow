using Adventure.Game_Managers;
using Adventure.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Characters
{
    public class CombatCharacter : Character
    {
        protected bool _hitOnce = false;

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

        protected void CheckForWeaponHits()
        {
            if (PlayerManager.Player.UsingWeapon && !_hitOnce)
            {
                Weapon wep = ((Weapon)PlayerManager.Player.CurrentItem);
                if (wep.CollisionBox != null && wep.CollisionBox.Intersects(CollisionBox))
                {
                    Random r = new Random();
                    DecreaseHealth(wep.Damage());
                }
            }
        }

        public void DecreaseHealth(int x)
        {
            _hitOnce = true;
            _hp -= x;
            if (_hp <= 0)
            {
                MapManager.CurrentMap.ToRemove.Add(this);
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
