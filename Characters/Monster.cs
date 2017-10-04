using Adventure.Characters;
using Adventure.Game_Managers;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

using ItemIDs = Adventure.Game_Managers.ObjectManager.ItemIDs;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure
{
    public class Monster : CombatCharacter
    {
        #region Properties
        #region CombatProperties
        protected double _actionDuration = 0;
        protected double _globalCooldown = 0;
        protected double CoolDown = 1;
        protected double _attackSpeed = 0.1;    //total time for one attack.

        protected bool _canCharge;
        protected Vector2 _chargeTarget;
        protected double _chargeCooldown = 3;
        protected int _chargeSpeed = 2;
        protected int _minCharge;
        protected int _maxCharge;
        protected bool _canJump;
        protected bool _canSmash;
        #endregion

        public enum ActionType { Idle, Attack, Charge, Jump, Smash}
        public ActionType _currentAction = ActionType.Idle;
        protected string _name;
        protected string _monsterType;
        protected int _minDmg;
        protected int _maxDmg;
        protected int _idleFor;
        protected int _leash = 400;
        
        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;

        #endregion

        public Monster(int id, string[] monsterData)
        {
            ImportBasics(monsterData, id);
            _textureName = @"Textures\Monsters\" + _name;
            LoadContent(_textureName, 32, 64, 4, 0.3f);
        }

        protected int ImportBasics(string[] monsterData, int id)
        {
            int i = 0;
            _name = monsterData[i++];
            _monsterType = monsterData[i++];
            _maxHP = int.Parse(monsterData[i++]);
            _speed = int.Parse(monsterData[i++]);
            string[] dmg = monsterData[i++].Split(' ');
            _minDmg = int.Parse(dmg[0]);
            _minDmg = int.Parse(dmg[1]);
            string[] actions = monsterData[i++].Split('|');
            foreach(string s in actions)
            {
                string[] data = s.Split(' ');
                switch (data[0])
                {
                    case "Charge":
                        _canCharge = true;
                        _minCharge = int.Parse(data[1]);
                        _maxCharge = int.Parse(data[2]);
                        _chargeCooldown = int.Parse(data[3]);
                        break;
                    default:
                        break;
                }

            }

            return i;
        }

        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            base.LoadContent(_textureName, textureWidth, textureHeight, numFrames, frameSpeed);
        }

        public override void Update(GameTime theGameTime)
        {
            _hitOnce = false;
            HandleTimers(theGameTime);
            UpdateAttacks(theGameTime);
            CheckForWeaponHits();
            if (_currentAction != ActionType.Attack || _currentAction != ActionType.Smash)
            {
                UpdateMovement();
            }

            base.Update(theGameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_currentAction == ActionType.Attack)
            {
                spriteBatch.Draw(GameContentManager.GetTexture("Textures\\weapons"), _attackRectangle, Color.Black);
            }
        }
        public void HandleTimers(GameTime theGameTime)
        {
            ReduceVelocity(ref _knockback);
            CountDown(ref _globalCooldown, theGameTime.ElapsedGameTime.TotalSeconds);
            CountDown(ref _chargeCooldown, theGameTime.ElapsedGameTime.TotalSeconds);
            CountDown(ref _actionDuration, theGameTime.ElapsedGameTime.TotalSeconds);

            if (_currentAction == ActionType.Charge && (Position == _chargeTarget || PlayerManager.PlayerInRange(Position, 32)))
            {
                _speed = _speed / _chargeSpeed;
                _currentAction = ActionType.Idle;
                Attack(true);
                _actionDuration = 2;
            }
            else if (_currentAction == ActionType.Attack && _actionDuration == 0)
            {
                _currentAction = ActionType.Idle;
            }
            else if (_actionDuration == 0)
            {
                ActionType temp = _currentAction;
                _currentAction = ActionType.Idle;
            }

        }
        public bool Charge()
        {
            bool rv = false;
            if (_canCharge)
            {
                if (_globalCooldown == 0 && _chargeCooldown == 0 && PlayerManager.PlayerInRange(Position, _minCharge, _maxCharge) && _currentAction == ActionType.Idle)
                {
                    _globalCooldown = CoolDown;
                    rv = true;
                    _currentAction = ActionType.Charge;
                    _speed = _speed * _chargeSpeed;
                    _actionDuration = 0.5;
                    _chargeCooldown = 4;
                    _chargeTarget = PlayerManager.Player.Position;
                }
            }
            return rv;
        }
        public void Attack(bool force = false)
        {
            if ((force || _globalCooldown == 0) && PlayerManager.PlayerInRange(Position, 32) && _currentAction == ActionType.Idle)
            {
                _globalCooldown = CoolDown;
                _currentAction = ActionType.Attack;
                _actionDuration = _attackSpeed;

                if (PlayerManager.Player.Position.X <= Position.X)
                {
                    _currentDirection = Direction.Left;
                    _attackRectangle = new Rectangle((int)Position.X - 32, (int)Position.Y + 32, 32, 32);
                }
                else if (PlayerManager.Player.Position.X >= Position.X)
                {
                    _currentDirection = Direction.Right;
                    _attackRectangle = new Rectangle((int)Position.X + 32, (int)Position.Y - 32, 32, 32);
                }
                else if (PlayerManager.Player.Position.Y <= Position.Y)
                {
                    _currentDirection = Direction.Up;
                    _attackRectangle = new Rectangle((int)Position.X - 32, (int)Position.Y - 32, 32, 32);
                }
                else if (PlayerManager.Player.Position.Y <= Position.Y)
                {
                    _currentDirection = Direction.Down;
                    _attackRectangle = new Rectangle((int)Position.X + 32, (int)Position.Y + 32, 32, 32);
                }
            }
        }
        public void UpdateAttacks(GameTime theGameTime)
        {
            if(_currentAction == ActionType.Attack)
            {
                int movement = (int)(64 /(_attackSpeed / theGameTime.ElapsedGameTime.TotalSeconds));
                if (_currentDirection == Direction.Left) { _attackRectangle.Location -= new Point(0, movement); }
                else if (_currentDirection == Direction.Right) { _attackRectangle.Location += new Point(0, movement); }
                else if (_currentDirection == Direction.Up) { _attackRectangle.Location += new Point(movement, 0); }
                else if (_currentDirection == Direction.Down) { _attackRectangle.Location -= new Point(movement, 0); }

                if (_attackRectangle.Intersects(PlayerManager.Player.CollisionBox))
                {
                    PlayerManager.Player.DecreaseHealth(_minDmg, Position);
                }
            }
        }

        private void UpdateMovement()
        {
            Vector2 direction = Vector2.Zero;
            string animation = "";

            if (_knockback != Vector2.Zero) {
                CheckMapForCollisionsAndMove(_knockback);
            }
            else if (PlayerManager.PlayerInRange(Position, _leash))
            {
                _moveTo = Vector2.Zero;

                Vector2 targetPos = PlayerManager.Player.Position;
                //If the Monster can charge, do so.
                if (!Charge())
                {
                    Attack();
                }
                if (_currentAction == ActionType.Idle || _currentAction == ActionType.Charge)
                {
                    if (_actionDuration > 0 && _currentAction == ActionType.Charge) { targetPos = _chargeTarget; }
                    float deltaX = Math.Abs(targetPos.X - this.Position.X);
                    float deltaY = Math.Abs(targetPos.Y - this.Position.Y);

                    GetMoveSpeed(targetPos, ref direction);
                    CheckMapForCollisionsAndMove(direction);

                    DetermineAnimation(ref animation, direction, deltaX, deltaY);

                    if (_sprite.CurrentAnimation != animation)
                    {
                        _sprite.CurrentAnimation = animation;
                    }
                }
            }
            else
            {
                IdleMovement();
            }
        }

        private void IdleMovement()
        {
            if (_moveTo == Vector2.Zero && _idleFor == 0)
            {
                int howFar = 2;
                Random r = new Random();
                int decision = r.Next(1, 5);
                if (decision == 1) { _moveTo = new Vector2(Position.X - r.Next(1, howFar) * RHTileMap.TileSize, Position.Y); }
                else if (decision == 2) { _moveTo = new Vector2(Position.X + r.Next(1, howFar) * RHTileMap.TileSize, Position.Y); }
                else if (decision == 3) { _moveTo = new Vector2(Position.X, Position.Y - r.Next(1, howFar) * RHTileMap.TileSize); }
                else if (decision == 4) { _moveTo = new Vector2(Position.X, Position.Y + r.Next(1, howFar) * RHTileMap.TileSize); }
                else {
                    _sprite.CurrentAnimation = "Float" + _sprite.CurrentAnimation.Substring(4);
                    _idleFor = 300;
                }
            }
            else if(_moveTo != Vector2.Zero)
            {
                string animation = "";
                Vector2 direction = Vector2.Zero;
                float deltaX = Math.Abs(_moveTo.X - this.Position.X);
                float deltaY = Math.Abs(_moveTo.Y - this.Position.Y);

                GetMoveSpeed(_moveTo, ref direction);
                CheckMapForCollisionsAndMove(direction);

                DetermineAnimation(ref animation, direction, deltaX, deltaY);

                if (_sprite.CurrentAnimation != animation)
                {
                    _sprite.CurrentAnimation = animation;
                }


                if (Position.X == _moveTo.X && Position.Y == _moveTo.Y) { _moveTo = Vector2.Zero; }
            }
            else
            {
                _idleFor--;
            }
        }

        private void CheckMapForCollisionsAndMove(Vector2 direction)
        {
            Rectangle testRectX = new Rectangle((int)Position.X + (int)direction.X, (int)Position.Y, Width, Height);
            Rectangle testRectY = new Rectangle((int)Position.X, (int)Position.Y + (int)direction.Y, Width, Height);

            if (MapManager.CurrentMap.CheckLeftMovement(this, testRectX) && MapManager.CurrentMap.CheckRightMovement(this, testRectX))
            {
                
                _sprite.MoveBy((int)direction.X, 0);
            }
            else{ if (_currentAction == ActionType.Charge) { _currentAction = ActionType.Idle; } }

            if (MapManager.CurrentMap.CheckUpMovement(this, testRectY) && MapManager.CurrentMap.CheckDownMovement(this, testRectY))
            {
                _sprite.MoveBy(0, (int)direction.Y);
            }
            else { if (_currentAction == ActionType.Charge) { _currentAction = ActionType.Idle; } }
        }

        private void GetMoveSpeed(Vector2 position, ref Vector2 direction)
        {
            float newX = 0; float newY = 0;
            if (position.X != this.Position.X)
            {
                newX = (position.X > this.Position.X) ? 1 : -1;
            }
            if (position.Y != this.Position.Y)
            {
                newY = (position.Y > this.Position.Y) ? 1 : -1;
            }

            float deltaX = Math.Abs(position.X - Position.X);
            float deltaY = Math.Abs(position.Y - Position.Y);
            direction.X = (deltaX < _speed) ? newX * deltaX : newX * _speed;
            direction.Y = (deltaY < _speed) ? newY * deltaY : newY * _speed;
        }

        private void DetermineAnimation(ref string animation, Vector2 direction, float deltaX, float deltaY)
        {
            if (deltaX > deltaY)
            {
                if (direction.X > 0)
                {
                    _facing = Facing.West;
                    animation = "Float";
                }
                else
                {
                    _facing = Facing.East;
                    animation = "Float";
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    _facing = Facing.South;
                    animation = "Float";
                }
                else
                {
                    _facing = Facing.North;
                    animation = "Float";
                }
            }
        }
    }
}
