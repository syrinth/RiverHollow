using Adventure.Characters;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace Adventure
{
    public class Monster : Character
    {
        private int LEASH = 200;
        private string _textureName;
        private Vector2 _moveTo = Vector2.Zero;
        private int _idleFor;

        public Monster(ContentManager theContentManager, Vector2 position)
        {
            _textureName = @"T_Vlad_Sword_Walking_48x48";
            LoadContent(theContentManager);
            Position = position;
        }

        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, _textureName);
        }

        public void Update(GameTime theGameTime, TileMap currMap, Player player)
        {
            UpdateMovement(player, currMap);

            base.Update(theGameTime, currMap);
        }

        private void UpdateMovement(Player player, TileMap currMap)
        {
            Vector2 direction = Vector2.Zero;
            string animation = "";

            if (System.Math.Abs(player.Position.X - this.Position.X) <= LEASH && System.Math.Abs(player.Position.Y - this.Position.Y) <= LEASH)
            {
                _moveTo = Vector2.Zero;
                bool moveX = true;
                bool moveY = true;
                float deltaX = Math.Abs(player.Position.X - this.Position.X);
                float deltaY = Math.Abs(player.Position.Y - this.Position.Y);

                GetMoveSpeed(player.Position, ref direction);
                CheckMapForCollisions(currMap, direction, ref moveX, ref moveY);

                DetermineAnimation(ref animation, direction, deltaX, deltaY);

                sprite.MoveBy(moveX ? (int)direction.X : 0, moveY ? (int)direction.Y : 0);

                if (sprite.CurrentAnimation != animation)
                {
                    sprite.CurrentAnimation = animation;
                }
            }
            else
            {
                IdleMovement(currMap);
            }
        }

        private void IdleMovement(TileMap currMap)
        {
            if (_moveTo == Vector2.Zero && _idleFor == 0)
            {
                int howFar = 2;
                Random r = new Random();
                int decision = r.Next(1, 6);
                if (decision == 1) { _moveTo = new Vector2(Position.X - r.Next(1, howFar) * Tile.TILE_WIDTH, Position.Y); }
                else if (decision == 2) { _moveTo = new Vector2(Position.X + r.Next(1, howFar) * Tile.TILE_WIDTH, Position.Y); }
                else if (decision == 3) { _moveTo = new Vector2(Position.X, Position.Y - r.Next(1, howFar) * Tile.TILE_HEIGHT); }
                else if (decision == 4) { _moveTo = new Vector2(Position.X, Position.Y + r.Next(1, howFar) * Tile.TILE_HEIGHT); }
                else {
                    sprite.CurrentAnimation = "Idle" + sprite.CurrentAnimation.Substring(4);
                    _idleFor = 300;
                }
            }
            else if(_moveTo != Vector2.Zero)
            {
                string animation = "";
                Vector2 direction = Vector2.Zero;
                bool moveX = true; bool moveY = true;
                float deltaX = Math.Abs(_moveTo.X - this.Position.X);
                float deltaY = Math.Abs(_moveTo.Y - this.Position.Y);

                GetMoveSpeed(_moveTo, ref direction);
                CheckMapForCollisions(currMap, direction, ref moveX, ref moveY);

                DetermineAnimation(ref animation, direction, deltaX, deltaY);

                if (sprite.CurrentAnimation != animation)
                {
                    sprite.CurrentAnimation = animation;
                }

                if (moveX && moveY)
                {
                    if (_moveTo.X != Position.X) { sprite.MoveBy((int)direction.X, 0); }
                    if (_moveTo.Y != Position.Y) { sprite.MoveBy(0, (int)direction.Y); }

                    if (Position.X == _moveTo.X && Position.Y == _moveTo.Y) { _moveTo = Vector2.Zero; }
                }
                else { _moveTo = Vector2.Zero; }
            }
            else
            {
                _idleFor--;
            }
        }

        private void CheckMapForCollisions(TileMap currMap, Vector2 direction, ref bool moveX, ref bool moveY)
        {
            Rectangle testRect = new Rectangle((int)Position.X + (int)direction.X, (int)Position.Y + (int)direction.Y, Width, Height);

            if (!currMap.CheckXMovement(testRect) || !currMap.CheckRightMovement(testRect))
            {
                moveX = false;
            }
            if (!currMap.CheckUpMovement(testRect) || !currMap.CheckDownMovement(testRect))
            {
                moveY = false;
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

            direction.X = newX * _speed;
            direction.Y = newY * _speed;
        }

        private void DetermineAnimation(ref string animation, Vector2 direction, float deltaX, float deltaY)
        {
            if (deltaX > deltaY)
            {
                if (direction.X > 0)
                {
                    animation = "WalkEast";
                }
                else
                {
                    animation = "WalkWest";
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    animation = "WalkSouth";
                }
                else
                {
                    animation = "WalkNorth";
                }
            }
        }
    }
}
