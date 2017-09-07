using Adventure.Game_Managers;
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

        public enum Facing { North, South, East, West };
        protected Facing _facing = Facing.North;
        protected AnimatedSprite _sprite;
        public Texture2D Texture { get => _sprite.Texture;  }
        public Point Center => GetRectangle().Center;
        public Vector2 Position
        {
            get { return new Vector2(_sprite.Position.X, _sprite.Position.Y + _sprite.Height - TileMap.TileSize); }
            set { _sprite.Position = value; }
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }
        public Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, Width, TileMap.TileSize); }

        public int Width
        {
            get { return TileMap.TileSize; }
        }

        public int Height
        {
            get { return TileMap.TileSize; }
        }

        protected int _speed = 3;
        public int Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
        #endregion

        public virtual void LoadContent(string textureToLoad, int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(textureToLoad));
            _sprite.LoadContent(textureWidth, textureHeight, numFrames, frameSpeed);
        }

        public virtual void Update(GameTime theGameTime)
        {
            _sprite.Update(theGameTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }

        public bool Contains(Point mouse)
        {
            return CollisionBox.Contains(mouse);
        }
    }
}
