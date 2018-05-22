using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using RiverHollow.SpriteAnimations;


namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    class GUISprite : GUIObject
    {
        AnimatedSprite _sprite;
        public AnimatedSprite Sprite => _sprite;

        public GUISprite(AnimatedSprite sprite)
        {
            _sprite = sprite;
            Width = sprite.Width;
            Height = sprite.Height;
        }

        public override void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, false);
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _sprite.Position = Position();
        }

        public void PlayAnimation(string animation)
        {
            _sprite.SetCurrentAnimation(animation);
        }

        public void SetScale(int scale)
        {
            _sprite.SetScale(scale);
            Width = _sprite.Width;
            Height = _sprite.Height;
        }

        public void SetSprite(AnimatedSprite sprite)
        {
            _sprite = sprite;
            Width = sprite.Width;
            Height = sprite.Height;
        }
    }
}
