using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Characters
{
    public class Character
    {
        protected string _sTexture;
        public enum CharacterEnum { Character, CombatAdventurer, CombatCharacter, Mob, Monster, NPC, Spirit, WorldAdventurer, WorldCharacter};
        protected CharacterEnum _characterType = CharacterEnum.Character;
        public CharacterEnum CharacterType => _characterType;

        protected string _sName;
        public virtual string Name { get => _sName; }

        protected AnimatedSprite _bodySprite;
        public AnimatedSprite BodySprite { get => _bodySprite; }

        public virtual Vector2 Position
        {
            get { return new Vector2(_bodySprite.Position.X, _bodySprite.Position.Y); }
            set { _bodySprite.Position = value; }
        }
        public virtual Vector2 Center { get => _bodySprite.Center; }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        protected int _width;
        public int Width { get => _width; }
        protected int _height;
        public int Height { get => _height; }
        public int SpriteWidth { get => _bodySprite.Width; }
        public int SpriteHeight { get => _bodySprite.Height; }

        public Character() { }

        public virtual void LoadContent(string textureToLoad, int frameWidth, int frameHeight, int numFrames, float frameSpeed,int startX = 0, int startY = 0)
        {
            _sTexture = textureToLoad;
            _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(_sTexture));
            _bodySprite.AddAnimation("Walk", frameWidth, frameHeight, numFrames, frameSpeed, startX, startY);
            _bodySprite.AddAnimation("Attack", frameWidth, frameHeight, numFrames, frameSpeed, startX, startY);
            _bodySprite.SetCurrentAnimation("Walk");

            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
        }

        public virtual void Update(GameTime theGameTime)
        {
            _bodySprite.Update(theGameTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            _bodySprite.Draw(spriteBatch, useLayerDepth);
        }

        public virtual void SetName(string text)
        {
            _sName = text;
        }

        public virtual void PlayAnimation(string animation)
        {
            _bodySprite.SetCurrentAnimation(animation);
        }

        public bool Contains(Point x) { return _bodySprite.BoundingBox.Contains(x); }
        public bool AnimationFinished() { return _bodySprite.PlayedOnce && _bodySprite.IsAnimating; }
        public bool IsCurrentAnimation(string val) { return _bodySprite.CurrentAnimation.Equals(val); }
        public bool IsAnimating() { return _bodySprite.IsAnimating; }
        public bool AnimationPlayedXTimes(int x) { return _bodySprite.GetPlayCount() >= x; }

        public bool IsCombatAdventurer() { return _characterType == CharacterEnum.CombatAdventurer; }
        public bool IsMob() { return _characterType == CharacterEnum.Mob; }
        public bool IsMonster() { return _characterType == CharacterEnum.Monster; }
        public bool IsNPC() { return _characterType == CharacterEnum.NPC; }
        public bool IsWorldAdventurer() { return _characterType == CharacterEnum.WorldAdventurer; }
        public bool IsWorldCharacter() { return _characterType == CharacterEnum.WorldCharacter; }
        public bool CanTalk() { return IsWorldCharacter() || IsNPC() || IsWorldAdventurer() || IsSpirit(); }
        public bool IsSpirit() { return _characterType == CharacterEnum.Spirit; }
    }
}
