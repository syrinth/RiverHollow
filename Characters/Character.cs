using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Characters
{
    public class Character
    {
        protected string _name;
        public string Name { get => _name; }

        protected AnimatedSprite _sprite;
        public AnimatedSprite Sprite { get => _sprite; }

        public virtual Vector2 Position
        {
            get { return new Vector2(_sprite.Position.X, _sprite.Position.Y); }
            set { _sprite.Position = value; }
        }
        public virtual Vector2 Center { get => _sprite.Center; }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        protected int _width;
        public int Width { get => _width; }
        protected int _height;
        public int Height { get => _height; }
        public int SpriteWidth { get => _sprite.Width; }
        public int SpriteHeight { get => _sprite.Height; }

        public Character()
        {

        }

        public virtual void LoadContent(string textureToLoad, int frameWidth, int frameHeight, int numFrames, float frameSpeed,int startX = 0, int startY = 0)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(textureToLoad));
            _sprite.AddAnimation("Walk", frameWidth, frameHeight, numFrames, frameSpeed, startX, startY);
            _sprite.AddAnimation("Attack", frameWidth, frameHeight, numFrames, frameSpeed, startX, startY);
            _sprite.SetCurrentAnimation("Walk");

            _width = _sprite.Width;
            _height = _sprite.Height;
        }

        public virtual void Update(GameTime theGameTime)
        {
            _sprite.Update(theGameTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch,bool useLayerDepth = false)
        {
            _sprite.Draw(spriteBatch, useLayerDepth);
        }

        public virtual void SetName(string text)
        {
            _name = text;
        }

        public void PlayAnimation(string animation)
        {
            _sprite.CurrentAnimation = animation;
        }

        public bool Contains(Point x) { return _sprite.BoundingBox.Contains(x); }
        public bool AnimationFinished() { return _sprite.PlayedOnce && _sprite.IsAnimating; }
        public bool IsCurrentAnimation(string val) { return _sprite.CurrentAnimation.Equals(val); }
        public bool IsAnimating() { return _sprite.IsAnimating; }
        public bool AnimationPlayedXTimes(int x) { return _sprite.GetPlayCount() == x; }
    }
}
