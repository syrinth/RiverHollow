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
            Vector2 _direction = Vector2.Zero;
            string animation = "";

            if (System.Math.Abs(player.Position.X - this.Position.X) <= LEASH && System.Math.Abs(player.Position.Y - this.Position.Y) <= LEASH)
            {
                float newX = (player.Position.X > this.Position.X) ? 1 : -1;
                float newY = (player.Position.Y > this.Position.Y) ? 1 : -1;

                float deltaX = Math.Abs(player.Position.X - this.Position.X);
                float deltaY = Math.Abs(player.Position.Y - this.Position.Y);

                _direction.X = newX *_speed;
                _direction.Y = newY * _speed;

                Rectangle testRect = new Rectangle((int)Position.X + (int)_direction.X, (int)Position.Y + (int)_direction.Y, Width, Height);
                bool moveX = true;
                bool moveY = true;

                if (!currMap.CheckXMovement(testRect) || !currMap.CheckRightMovement(testRect))
                {
                    moveX = false;
                }
                if (!currMap.CheckUpMovement(testRect) || !currMap.CheckDownMovement(testRect))
                {
                    moveY = false;
                }

                if (deltaX > deltaY)
                {
                    if (_direction.X > 0)
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
                    if (_direction.Y > 0)
                    {
                        animation = "WalkSouth";
                    }
                    else
                    {
                        animation = "WalkNorth";
                    }
                }

                sprite.MoveBy(moveX ? (int)_direction.X : 0, moveY ? (int)_direction.Y : 0);

                if (sprite.CurrentAnimation != animation)
                {
                    sprite.CurrentAnimation = animation;
                }
            }
        }
    }
}
