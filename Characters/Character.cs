using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Characters
{
    public class Character
    {
        protected AnimatedSprite _sprite;
        public AnimatedSprite Sprite { get => _sprite; }

        public virtual Vector2 Position
        {
            get { return new Vector2(_sprite.Position.X, _sprite.Position.Y); }
            set { _sprite.Position = value; }
        }
        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        protected int _width;
        public int Width { get => _width; }
        protected int _height;
        public int Height { get => _height; }

        public Character()
        {

        }

        public virtual void LoadContent(string textureToLoad, int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(textureToLoad));
            _sprite.AddAnimation(textureToLoad, textureWidth, textureHeight, numFrames, frameSpeed);
            _sprite.SetCurrentAnimation(textureToLoad);
        }

        public virtual void Update(GameTime theGameTime)
        {
            _sprite.Update(theGameTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }
    }
}
