using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUISprite : GUIObject
    {
        protected AnimatedSprite _sprite;
        public AnimatedSprite Sprite => _sprite;

        public bool PlayedOnce => _sprite.PlayedOnce;

        public int PlayCount => _sprite.GetPlayCount();

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

        public override void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Show)
            {
                _sprite.Draw(spriteBatch, false);
            }
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

        public virtual void PlayAnimation(AnimationEnum animation) { _sprite.PlayAnimation(animation); }
        public virtual void PlayAnimation(VerbEnum verb, DirectionEnum dir) { _sprite.PlayAnimation(verb, dir); }

        public override void SetScale(double x, bool anchorToPos = true)
        {
            _sprite.SetScale((int)x);
            Width = _sprite.Width;
            Height = _sprite.Height;
        }

        /// <summary>
        /// Helper for the AnimatedSprite Reset method.
        /// </summary>
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
        GUISprite _sprLegs;
        public GUISprite Legsprite => _sprLegs;

        List<GUISprite> _liSprites;

        public GUICharacterSprite(AnimatedSprite sprite, bool overwrite = false)
        {
            _liSprites = new List<GUISprite>();

            _sprBody = new GUISprite(sprite, overwrite);
            AddControl(_sprBody);
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
            if (PlayerManager.World.Body != null)
            {
                _sprChest = new GUISprite(PlayerManager.World.Body.Sprite, overwrite);
                _liSprites.Add(_sprChest);
            }
            if (PlayerManager.World.Legs != null)
            {
                _sprLegs = new GUISprite(PlayerManager.World.Legs.Sprite, overwrite);
                _liSprites.Add(_sprLegs);
            }

            foreach (GUISprite sprite in _liSprites)
            {
                AddControl(sprite);
            }

            Width = _sprBody.Width;
            Height = _sprBody.Height;
        }

        public void PlayAnimation(AnimationEnum animation)
        {
            foreach (GUISprite g in _liSprites)
            {
                g.PlayAnimation(animation);
            }
        }

        public void PlayAnimation(VerbEnum verb, DirectionEnum dir)
        {
            foreach (GUISprite g in _liSprites)
            {
                g.PlayAnimation(verb, dir);
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
