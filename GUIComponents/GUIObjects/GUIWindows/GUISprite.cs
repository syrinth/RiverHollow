using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using RiverHollow.SpriteAnimations;
using System.Collections.Generic;

using static RiverHollow.Game_Managers.GameManager;
namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUISprite : GUIObject
    {
        protected AnimatedSprite _sprite;
        public AnimatedSprite Sprite => _sprite;

        public bool PlayedOnce
        {
            get { return _sprite.PlayedOnce; }
            set { _sprite.PlayedOnce = value; }
        }
        public bool IsAnimating {
            get { return _sprite.IsAnimating; }
            set { _sprite.IsAnimating = value; }
        }

        public GUISprite(AnimatedSprite sprite, bool overwrite = false)
        {
            _sprite = overwrite ? new AnimatedSprite(sprite) : sprite;
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

        /// <summary>
        /// Because the AnimatedSprite is nota GUI Object,is does not inherent
        /// the Position method like other GUIObjects,so this method call is required.
        /// </summary>
        /// <param name="value"></param>
        public override void Position(Vector2 value)
        {
            base.Position(value);
            _sprite.Position = Position();
        }

        public virtual void PlayAnimation<TEnum>(TEnum animation)
        {
            _sprite.SetCurrentAnimation(animation);
        }

        public override void SetScale(double x, bool anchorToPos = true)
        {
            _sprite.SetScale((int)x);
            Width = _sprite.Width;
            Height = _sprite.Height;
        }

        public void Reset()
        {
            _sprite.Reset();
        }
    }

    public class GUICharacterSprite : GUIObject
    {
        GUISprite _sprBody;
        public GUISprite BodySprite => _sprBody;
        GUISprite _sprEyes;
        public GUISprite EyeSprite => _sprEyes;
        GUISprite _sprHair;
        public GUISprite HairSprite => _sprHair;
        GUISprite _sprHat;
        public GUISprite HatSprite => _sprHat;
        GUISprite _sprChest;
        public GUISprite ShirtSprite => _sprChest;

        List<GUISprite> _liSprites;

        public GUICharacterSprite(AnimatedSprite sprite, bool overwrite = false)
        {
            _liSprites = new List<GUISprite>();

            _sprBody = new GUISprite(sprite, overwrite);
            _liSprites.Add(_sprBody);

            Width = _sprBody.Width;
            Height = _sprBody.Height;
        }
        public GUICharacterSprite(bool overwrite = false)
        {
            _liSprites = new List<GUISprite>();

            _sprBody = new GUISprite(PlayerManager.World.BodySprite, overwrite);
            _sprEyes = new GUISprite(PlayerManager.World.EyeSprite, overwrite);
            _sprHair = new GUISprite(PlayerManager.World.HairSprite, overwrite);

            if (_sprBody != null) { _liSprites.Add(_sprBody); }
            if (_sprEyes != null) { _liSprites.Add(_sprEyes); }
            if (_sprHair != null) { _liSprites.Add(_sprHair); }
            if (PlayerManager.World.Hat != null) {
                _sprHat = new GUISprite(PlayerManager.World.Hat.Sprite, overwrite);
                _liSprites.Add(_sprHat);
            }
            if (PlayerManager.World.Shirt != null)
            {
                _sprChest = new GUISprite(PlayerManager.World.Shirt.Sprite, overwrite);
                _liSprites.Add(_sprChest);
            }

            foreach(GUISprite sprite in _liSprites)
            {
                AddControl(sprite);
            }

            Width = _sprBody.Width;
            Height = _sprBody.Height;
        }

        public void PlayAnimation<TEnum>(TEnum animation)
        {
            foreach (GUISprite g in _liSprites)
            {
                g.PlayAnimation(animation);
            }
        }

        public void SetScale(int scale)
        {
            _dScale = scale;

            foreach (GUISprite g in _liSprites)
            {
                g.SetScale(scale);
            }

            Width = _sprBody.Width;
            Height = _sprBody.Height;
        }
    }
}
