using Adventure.Characters;
using Adventure.Game_Managers;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

using ItemIDs = Adventure.Game_Managers.ObjectManager.ItemIDs;
namespace Adventure
{
    public class Monster : CombatCharacter
    {
        protected PlayerManager _playerManager = PlayerManager.GetInstance();
        protected int _damage;
        protected int _idleFor;
        protected int _leash = 200;
        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;
        protected List<KeyValuePair<ItemIDs, double>> _dropTable;

        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            base.LoadContent(_textureName, textureWidth, textureHeight, numFrames, frameSpeed);
        }

        public override void Update(GameTime theGameTime)
        {
            UpdateMovement();

            base.Update(theGameTime);
        }

        private void UpdateMovement()
        {
            Vector2 direction = Vector2.Zero;
            string animation = "";

            if (System.Math.Abs(_playerManager.Player.Position.X - this.Position.X) <= _leash && System.Math.Abs(_playerManager.Player.Position.Y - this.Position.Y) <= _leash)
            {
                _moveTo = Vector2.Zero;
                float deltaX = Math.Abs(_playerManager.Player.Position.X - this.Position.X);
                float deltaY = Math.Abs(_playerManager.Player.Position.Y - this.Position.Y);

                GetMoveSpeed(_playerManager.Player.Position, ref direction);
                CheckMapForCollisionsAndMove(direction);

                DetermineAnimation(ref animation, direction, deltaX, deltaY);

                if (_sprite.CurrentAnimation != animation)
                {
                    _sprite.CurrentAnimation = animation;
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
                int decision = r.Next(1, 6);
                if (decision == 1) { _moveTo = new Vector2(Position.X - r.Next(1, howFar) * TileMap.TileSize, Position.Y); }
                else if (decision == 2) { _moveTo = new Vector2(Position.X + r.Next(1, howFar) * TileMap.TileSize, Position.Y); }
                else if (decision == 3) { _moveTo = new Vector2(Position.X, Position.Y - r.Next(1, howFar) * TileMap.TileSize); }
                else if (decision == 4) { _moveTo = new Vector2(Position.X, Position.Y + r.Next(1, howFar) * TileMap.TileSize); }
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
                else { _moveTo = Vector2.Zero; }
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

            if (_mapManager.CurrentMap.CheckLeftMovement(testRectX) && _mapManager.CurrentMap.CheckRightMovement(testRectX))
            {
                _sprite.MoveBy((int)direction.X, 0);
            }

            if (_mapManager.CurrentMap.CheckUpMovement(testRectY) && _mapManager.CurrentMap.CheckDownMovement(testRectY))
            {
                _sprite.MoveBy(0, (int)direction.Y);
            }
        }

        private void GetMoveSpeed(Vector2 position, ref Vector2 direction)
        {
            float newX = 0; float newY = 0;
            if (position.X != this.Position.X)
            {
                newX = (position.X > this.Position.X) ? 1 : -1;
            }
            if (position.X != this.Position.Y)
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
                    animation = "Float";
                }
                else
                {
                    animation = "Float";
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    animation = "Float";
                }
                else
                {
                    animation = "Float";
                }
            }
        }
    }
}
