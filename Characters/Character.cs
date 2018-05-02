using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Characters
{
    public class Character
    {
        public enum CharacterEnum { Character, CombatAdventurer, CombatCharacter, Mob, Monster, NPC, Spirit, WorldAdventurer, WorldCharacter};
        protected CharacterEnum _characterType = CharacterEnum.Character;
        public CharacterEnum CharacterType => _characterType;

        protected string _sName;
        public string Name { get => _sName; }

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

        public Character() { }

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
            _sName = text;
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
