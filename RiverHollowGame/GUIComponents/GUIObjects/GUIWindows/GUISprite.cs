using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUISprite : GUIObject
    {
        protected AnimatedSprite _sprite;

        public GUISprite(AnimatedSprite sprite, bool newSprite = false)
        {
            _sprite = newSprite ? new AnimatedSprite(sprite) : sprite;
            _sprite.SetScale(GameManager.ScaledPixel);
            Width = _sprite.Width;
            Height = _sprite.Height;
        }

        public override void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Show())
            {
                _sprite.Draw(spriteBatch, false, _fAlpha);

                foreach (GUIObject g in Controls)
                {
                    g.Draw(spriteBatch);
                }
            }
        }

        /// <summary>
        /// Because the AnimatedSprite is nota GUI Object,is does not inherent
        /// the Position method like other GUIObjects,so this method call is required.
        /// </summary>
        /// <param name="value"></param>
        public override void Position(Point value)
        {
            base.Position(value);
            _sprite.Position = Position();
        }

        public virtual void PlayAnimation<TEnum>(TEnum e) { _sprite.PlayAnimation(e); }
        public virtual void PlayAnimation(VerbEnum verb, DirectionEnum dir) { _sprite.PlayAnimation(verb, dir); }

        public override void SetScale(double x, bool anchorToPos = true)
        {
            _sprite.SetScale((int)x);
            Width = _sprite.Width;
            Height = _sprite.Height;
        }

        public override void SetColor(Color c)
        {
            _sprite.SetColor(c);
        }

        /// <summary>
        /// Helper for the AnimatedSprite Reset method.
        /// </summary>
        public void Reset()
        {
            _sprite.Reset();
        }
    }
}
