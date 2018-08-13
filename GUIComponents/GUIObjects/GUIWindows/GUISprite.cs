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

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _sprite.Position = Position();
        }

        public virtual void PlayAnimation<TEnum>(TEnum animation)
        {
            _sprite.SetCurrentAnimation(animation);
        }

        public virtual void SetScale(int scale)
        {
            _sprite.SetScale(scale);
            Width = _sprite.Width;
            Height = _sprite.Height;
        }
    }

    public class GUICharacterSprite : GUIObject
    {
        GUISprite _bodySprite;
        public GUISprite BodySprite => _bodySprite;
        GUISprite _eyeSprite;
        public GUISprite EyeSprite => _eyeSprite;
        GUISprite _hairSprite;
        public GUISprite HairSprite => _hairSprite;
        GUISprite _armSprite;
        public GUISprite ArmSprite => _armSprite;
        GUISprite _hatSprite;
        public GUISprite HatSprite => _hatSprite;
        GUISprite _shirtSprite;
        public GUISprite ShirtSprite => _shirtSprite;

        List<GUISprite> _liSprites;

        public GUICharacterSprite(AnimatedSprite sprite, bool overwrite = false)
        {
            _liSprites = new List<GUISprite>();

            _bodySprite = new GUISprite(sprite, overwrite);
            _liSprites.Add(_bodySprite);

            Width = _bodySprite.Width;
            Height = _bodySprite.Height;
        }
        public GUICharacterSprite(bool overwrite = false)
        {
            _liSprites = new List<GUISprite>();

            _bodySprite = new GUISprite(PlayerManager.World.BodySprite, overwrite);
            _eyeSprite = new GUISprite(PlayerManager.World.EyeSprite, overwrite);
            _hairSprite = new GUISprite(PlayerManager.World.HairSprite, overwrite);
            _armSprite = new GUISprite(PlayerManager.World.ArmSprite, overwrite);

            if (_bodySprite != null) { _liSprites.Add(_bodySprite); }
            if (_eyeSprite != null) { _liSprites.Add(_eyeSprite); }
            if (_hairSprite != null) { _liSprites.Add(_hairSprite); }
            if (_armSprite != null) { _liSprites.Add(_armSprite); }
            if (PlayerManager.World.Hat != null) {
                _hatSprite = new GUISprite(PlayerManager.World.Hat.Sprite, overwrite);
                _liSprites.Add(_hatSprite);
            }
            if (PlayerManager.World.Shirt != null)
            {
                _shirtSprite = new GUISprite(PlayerManager.World.Shirt.Sprite, overwrite);
                _liSprites.Add(_shirtSprite);
            }

            Width = _bodySprite.Width;
            Height = _bodySprite.Height;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (GUISprite g in _liSprites)
            {
                g.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach(GUISprite g in _liSprites)
            {
                g.Draw(spriteBatch);
            }
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            foreach (GUISprite g in _liSprites)
            {
                g.Position(value);
            }
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
            foreach (GUISprite g in _liSprites)
            {
                g.SetScale(scale);
            }

            Width = _bodySprite.Width;
            Height = _bodySprite.Height;
        }
    }
}
