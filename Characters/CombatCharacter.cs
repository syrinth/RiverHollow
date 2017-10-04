using Adventure.Game_Managers;
using Adventure.Items;
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
        public enum Direction { Up, Down, Left, Right}
        protected bool _hitOnce = false;
        protected Direction _currentDirection;
        protected Rectangle _attackRectangle;
        protected bool _invulnerable = false;
        protected double _invulCountdown;
        protected Vector2 _knockback;

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

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
            CountDown(ref _invulCountdown, theGameTime.ElapsedGameTime.TotalSeconds);
            _invulnerable = _invulCountdown != 0;
        }
        protected void CheckForWeaponHits()
        {
            if (PlayerManager.Player.UsingWeapon && !_hitOnce)
            {
                Weapon wep = ((Weapon)PlayerManager.Player.CurrentItem);
                if (wep.CollisionBox != null && wep.CollisionBox.Intersects(CollisionBox))
                {
                    Random r = new Random();
                    DecreaseHealth(wep.Damage(), wep.CollisionBox.Location.ToVector2());
                }
            }
        }

        public void DecreaseHealth(int x, Vector2 pos)
        {
            if (!_invulnerable)
            {
                _invulCountdown = 1;
                _invulnerable = true;
                _hitOnce = true;
                _hp -= x;
                if (this != PlayerManager.Player)
                {
                    Vector2 delta = Position - pos;
                    delta.Normalize();
                    _knockback = delta * 10;
                }

                if (_hp <= 0)
                {
                    if (this == PlayerManager.Player)
                    {

                    }
                    else
                    {
                        MapManager.CurrentMap.ToRemove.Add(this);
                    }
                }
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

        public void CountDown(ref double countThis, double secs)
        {
            if (countThis > 0)
            {
                countThis = countThis - secs;
                if (countThis < 0) { countThis = 0; }
            }
        }

        public void ReduceVelocity(ref Vector2 velocity)
        {
            float velReduc = 0.5f;
            if (velocity != Vector2.Zero)
            {
                if (velocity.X != 0)
                {
                    if (velocity.X > 0)
                    {
                        velocity.X = velocity.X - velReduc;
                        if (velocity.X < 0) { velocity.X = 0; }
                    }
                    if (velocity.X < 0)
                    {
                        velocity.X = velocity.X + velReduc;
                        if (velocity.X > 0) { velocity.X = 0; }
                    }
                }
                if (velocity.Y != 0)
                {
                    if (velocity.Y > 0)
                    {
                        velocity.Y = velocity.Y - velReduc;
                        if (velocity.Y < 0) { velocity.Y = 0; }
                    }
                    if (velocity.Y < 0)
                    {
                        velocity.Y = velocity.Y + velReduc;
                        if (velocity.Y > 0) { velocity.Y = 0; }
                    }
                }
            }
        }

    }
}
