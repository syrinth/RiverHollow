using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Adventure.SpriteAnimations;
using Adventure.Tile_Engine;
using Adventure.Characters;

namespace Adventure
{
    public class Player : CombatCharacter
    {
        public Player(ContentManager theContentManager)
        {
            LoadContent(theContentManager);
            Position = new Vector2(200, 200);
            Speed = 5;
        }
        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, @"T_Vlad_Sword_Walking_48x48");
        }

        public override void Update(GameTime theGameTime, TileMap currMap)
        {
            Vector2 moveVector = Vector2.Zero;
            Vector2 moveDir = Vector2.Zero;
            string animation = "";

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.W))
            {
                moveDir += new Vector2(0, -_speed);
                animation = "WalkNorth";
                moveVector += new Vector2(0, -_speed);
            }
            else if (ks.IsKeyDown(Keys.S))
            {
                moveDir += new Vector2(0, _speed);
                animation = "WalkSouth";
                moveVector += new Vector2(0, _speed);
            }

            if (ks.IsKeyDown(Keys.A))
            {
                moveDir += new Vector2(-_speed, 0);
                animation = "WalkWest";
                moveVector += new Vector2(-_speed, 0);
            }
            else if (ks.IsKeyDown(Keys.D))
            {
                moveDir += new Vector2(_speed, 0);
                animation = "WalkEast";
                moveVector += new Vector2(_speed, 0);
            }

            if (moveDir.Length() != 0)
            {
                Rectangle testRect = new Rectangle((int)Position.X + (int)moveDir.X, (int)Position.Y + (int)moveDir.Y, Width, Height);
                bool moveX = true;
                bool moveY = true;

                //if (!currMap.CheckXMovement(testRect) || !currMap.CheckRightMovement(testRect))
                //{
                //    moveX = false;
                //}
                //if (!currMap.CheckUpMovement(testRect) || !currMap.CheckDownMovement(testRect))
                //{
                //    moveY = false;
                //}

                sprite.MoveBy(moveX ? (int)moveDir.X : 0, moveY ? (int)moveDir.Y : 0);

                if (sprite.CurrentAnimation != animation)
                {
                    sprite.CurrentAnimation = animation;
                }
            }
            else
            {
                sprite.CurrentAnimation = "Idle" + sprite.CurrentAnimation.Substring(4);
            }

            this.Position = new Vector2(sprite.Position.X, sprite.Position.Y);

            sprite.Update(theGameTime, currMap);
        }
    }
}
