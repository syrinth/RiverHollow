using Adventure.Game_Managers;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public class Weapon
    {
        protected GameContentManager _gcManager = GameContentManager.GetInstance();

        private Texture2D SpriteTexture;
        private Vector2 origin;
        private Vector2 screenpos;

        public bool attack = false;

        public Weapon(Player player)
        {
            LoadContent(player);
        }

        protected void LoadContent(Player player)
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            SpriteTexture = _gcManager.GetTexture(@"Textures\Sword");
            origin.X = SpriteTexture.Width / 2;
            origin.Y = SpriteTexture.Height / 2;
            screenpos.X = player.Position.X;
            screenpos.Y = player.Position.Y;
        }

        private float RotationAngle;
        public void Update(GameTime gameTime)
        {
            if (attack)
            {
                // The time since Update was called last.
                RotationAngle += 0.2f;
                float circle = MathHelper.Pi * 2;
                RotationAngle = RotationAngle % circle;

                if(RotationAngle >= 3)
                {
                    attack = false;
                    RotationAngle = 0;
                }
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (attack)
            {
                // TODO: Add your drawing code here
                spriteBatch.Draw(SpriteTexture, screenpos, null, Color.White, RotationAngle, origin, 1.0f, SpriteEffects.None, 0f);
            }
        }

        public float GetRotationAngle()
        {
            return RotationAngle;
        }

        public void Attack(Player player)
        {
            attack = true;
            screenpos.X = player.Position.X + TileMap.TileSize;
            screenpos.Y = player.Position.Y;
        }
    }
}
