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
            Width = GameManager.ScaleIt(sprite.Width);
            Height = GameManager.ScaleIt(sprite.Height);
        }

        public override void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Show())
            {
                _sprite.Draw(spriteBatch, false);
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

            _sprBody = new GUISprite(PlayerManager.PlayerActor.BodySprite, overwrite);
            _sprEyes = new GUISprite(PlayerManager.PlayerActor.EyeSprite, overwrite);
            _sprHair = new GUISprite(PlayerManager.PlayerActor.HairSprite, overwrite);

            if (_sprBody != null) { _liSprites.Add(_sprBody); }
            if (_sprEyes != null) { _liSprites.Add(_sprEyes); }
            if (_sprHair != null) { _liSprites.Add(_sprHair); }
            if (PlayerManager.PlayerActor.Hat != null) {
                _sprHat = new GUISprite(PlayerManager.PlayerActor.Hat.Sprite, overwrite);
                _liSprites.Add(_sprHat);
            }
            if (PlayerManager.PlayerActor.Chest != null)
            {
                _sprChest = new GUISprite(PlayerManager.PlayerActor.Chest.Sprite, overwrite);
                _liSprites.Add(_sprChest);
            }
            if (PlayerManager.PlayerActor.Legs != null)
            {
                _sprLegs = new GUISprite(PlayerManager.PlayerActor.Legs.Sprite, overwrite);
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
