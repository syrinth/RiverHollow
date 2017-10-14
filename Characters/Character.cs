using Adventure.Game_Managers;
using Adventure.SpriteAnimations;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Characters
{
    public class Character
    {
        protected AnimatedSprite _sprite;
        public virtual Vector2 Position
        {
            get { return new Vector2(_sprite.Position.X, _sprite.Position.Y); }
            set { _sprite.Position = value; }
        }

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
    }
}
