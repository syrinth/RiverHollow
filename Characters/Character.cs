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
        protected AnimatedSprite _sprite;

        public Vector2 Center => _sprite.Position;
        public Vector2 Position
        {
            get { return new Vector2(_sprite.Position.X, _sprite.Position.Y + _sprite.Height - TileMap._tileHeight); }
            set { _sprite.Position = value; }
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public int Width
        {
            get { return TileMap._tileWidth; }
        }

        public int Height
        {
            get { return TileMap._tileHeight; }
        }

        protected int _speed = 3;
        public int Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        #endregion

        public virtual void LoadContent(ContentManager theContentManager, string textureToLoad, int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            _sprite = new AnimatedSprite(theContentManager.Load<Texture2D>(textureToLoad));
            _sprite.LoadContent(textureWidth, textureHeight, numFrames, frameSpeed);
        }

        public virtual void Update(GameTime theGameTime, TileMap curr)
        {
            _sprite.Update(theGameTime, curr);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }
    }
}
