﻿using Adventure.Characters;
using Adventure.Game_Managers;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework.Graphics;

namespace Adventure
{
    public class Mob : WorldCharacter
    {
        #region Properties
        protected int _idleFor;
        protected int _leash = 400;

        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;
        protected List<Monster> _monsters;
        public List<Monster> Monsters { get => _monsters; }
        #endregion

        public Mob(int id, string[] stringData)
        {
            _monsters = new List<Monster>();
            ImportBasics(stringData, id);
            _textureName = @"Textures\Monsters\Goblin Scout";
            LoadContent(_textureName, 32, 64, 4, 0.3f);
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            for(int i=0; i < stringData.Length; i++)
            {
                _monsters.Add(CharacterManager.GetMonsterByIndex(int.Parse(stringData[i])));
            }
            return 0;
        }

        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            base.LoadContent(_textureName, textureWidth, textureHeight, numFrames, frameSpeed);
        }

        public override void Update(GameTime theGameTime)
        {
            UpdateMovement();
            base.Update(theGameTime);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
        private void UpdateMovement()
        {
            Vector2 direction = Vector2.Zero;
            string animation = "";


            if (PlayerManager.PlayerInRange(Position, _leash))
            {
                _moveTo = Vector2.Zero;
                Vector2 targetPos = PlayerManager.World.Position;

                float deltaX = Math.Abs(targetPos.X - this.Position.X);
                float deltaY = Math.Abs(targetPos.Y - this.Position.Y);

                GetMoveSpeed(targetPos, ref direction);
                CheckMapForCollisionsAndMove(direction);

                DetermineAnimation(ref animation, direction, deltaX, deltaY);

                if (_sprite.CurrentAnimation != animation)
                {
                    _sprite.CurrentAnimation = animation;
                }

                if (CollisionBox.Intersects(PlayerManager.World.CollisionBox))
                {
                    CombatManager.NewBattle(this);
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
                if (decision == 1) { _moveTo = new Vector2(Position.X - r.Next(1, howFar) * RHMap.TileSize, Position.Y); }
                else if (decision == 2) { _moveTo = new Vector2(Position.X + r.Next(1, howFar) * RHMap.TileSize, Position.Y); }
                else if (decision == 3) { _moveTo = new Vector2(Position.X, Position.Y - r.Next(1, howFar) * RHMap.TileSize); }
                else if (decision == 4) { _moveTo = new Vector2(Position.X, Position.Y + r.Next(1, howFar) * RHMap.TileSize); }
                else
                {
                    _sprite.CurrentAnimation = "Float" + _sprite.CurrentAnimation.Substring(4);
                    _idleFor = 300;
                }
            }
            else if (_moveTo != Vector2.Zero)
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

            if (MapManager.CurrentMap.CheckUpMovement(this, testRectY) && MapManager.CurrentMap.CheckDownMovement(this, testRectY))
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
            if (position.Y != this.Position.Y)
            {
                newY = (position.Y > this.Position.Y) ? 1 : -1;
            }

            float deltaX = Math.Abs(position.X - Position.X);
            float deltaY = Math.Abs(position.Y - Position.Y);
            direction.X = (deltaX < Speed) ? newX * deltaX : newX * Speed;
            direction.Y = (deltaY < Speed) ? newY * deltaY : newY * Speed;
        }        
    }
}
