using Adventure.SpriteAnimations;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Characters
{
    public class Character
    {
        #region Properties
        protected AnimatedSprite sprite;

        public Vector2 Center => sprite.Position;
        public Vector2 Position
        {
            get { return new Vector2(sprite.Position.X, sprite.Position.Y + sprite.Height - Tile.TILE_HEIGHT); }
            set { sprite.Position = value; }
        }

        public int Width
        {
            get { return Tile.TILE_WIDTH; }
        }

        public int Height
        {
            get { return Tile.TILE_HEIGHT; }
        }

        protected int _speed = 3;
        public int Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        #endregion

        public virtual void LoadContent(ContentManager theContentManager, string textureToLoad)
        {
            sprite = new AnimatedSprite(theContentManager.Load<Texture2D>(textureToLoad));
            sprite.LoadContent();
        }

        public virtual void Update(GameTime theGameTime, TileMap curr)
        {
            sprite.Update(theGameTime, curr);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            sprite.Draw(spriteBatch);
        }
    }
}
