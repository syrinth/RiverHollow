using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using System;
using RiverHollow.SpriteAnimations;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.WorldObjects;

namespace RiverHollow.Characters
{
    public class WorldCharacter : Character
    {
        #region Properties
        protected Vector2 _vMoveTo;
        public Vector2 MoveToLocation { get => _vMoveTo; }
        public string CurrentMapName;
        public Vector2 NewMapPosition;
        public enum DirectionEnum { Up, Down, Right, Left };
        public DirectionEnum Facing = DirectionEnum.Down;
        public Texture2D Texture { get => _bodySprite.Texture; }
        public Point CharCenter => GetRectangle().Center;
        public override Vector2 Position
        {
            get { return new Vector2(_bodySprite.Position.X, _bodySprite.Position.Y + _bodySprite.Height - TileSize); } //MAR this is fucked up
            set { _bodySprite.Position = new Vector2(value.X, value.Y - _bodySprite.Height + TileSize); }
        }

        public Rectangle CollisionBox { get => new Rectangle((int)Position.X + (Width/4), (int)Position.Y, Width/2, TileSize); }

        protected bool _bActive = true;
        public bool Active => _bActive;

        public int Speed = 2;
        #endregion

        public WorldCharacter() : base()
        {
            _characterType = CharacterEnum.WorldCharacter;
            _width = TileSize;
            _height = TileSize;
        }

        public virtual void LoadContent(string textureToLoad)
        {
            AddDefaultAnimations(ref _bodySprite, textureToLoad, 0, 0);

            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
        }

        public void AddDefaultAnimations(ref AnimatedSprite sprite, string texture, int startX, int startY, bool pingpong = false)
        {
            sprite = new AnimatedSprite(GameContentManager.GetTexture(texture), pingpong);
            sprite.AddAnimation("WalkDown", TileSize, TileSize * 2, 3, 0.2f, startX, startY);
            sprite.AddAnimation("IdleDown", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize, startY);
            sprite.AddAnimation("WalkUp", TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 3, startY);
            sprite.AddAnimation("IdleUp", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 4, startY);
            sprite.AddAnimation("WalkLeft", TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 6, startY);
            sprite.AddAnimation("IdleLeft", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 7, startY);
            sprite.AddAnimation("WalkRight", TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 9, startY);
            sprite.AddAnimation("IdleRight", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 10, startY);
            sprite.SetCurrentAnimation("IdleDown");
        }

        public virtual bool CollisionContains(Point mouse)
        {
            return CollisionBox.Contains(mouse);
        }

        public virtual bool CollisionIntersects(Rectangle rect)
        {
            return CollisionBox.Intersects(rect);
        }

        public void SetWalkingDir(DirectionEnum d)
        {
            Facing = d;
            if(d == DirectionEnum.Up) { _bodySprite.CurrentAnimation = "Walk North"; }
            else if (d == DirectionEnum.Down) { _bodySprite.CurrentAnimation = "Walk South"; }
            else if (d == DirectionEnum.Right) { _bodySprite.CurrentAnimation = "Walk East"; }
            else if (d == DirectionEnum.Left) { _bodySprite.CurrentAnimation = "Walk West"; }
        }

        public void DetermineFacing(Vector2 direction)
        {
            string animation = string.Empty;

            if (Math.Abs((int)direction.X) > Math.Abs((int)direction.Y))
            {
                if (direction.X > 0)
                {
                    Facing = DirectionEnum.Right;
                    animation = "WalkRight";
                }
                else if (direction.X < 0)
                {
                    Facing = DirectionEnum.Left;
                    animation = "WalkLeft";
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    Facing = DirectionEnum.Down;
                    animation = "WalkDown";
                }
                else if (direction.Y < 0)
                {
                    Facing = DirectionEnum.Up;
                    animation = "WalkUp";
                }
            }

            if (direction.Length() == 0)
            {
                Idle();
            }

            if (_bodySprite.CurrentAnimation != animation)
            {
                PlayAnimation(animation);
            }
        }

        public virtual void Idle()
        {
            switch (Facing)
            {
                case DirectionEnum.Down:
                    PlayAnimation("IdleDown");
                    break;
                case DirectionEnum.Up:
                    PlayAnimation("IdleUp");
                    break;
                case DirectionEnum.Left:
                    PlayAnimation("IdleLeft");
                    break;
                case DirectionEnum.Right:
                    PlayAnimation("IdleRight");
                    break;
            }
        }

        protected bool CheckMapForCollisionsAndMove(Vector2 direction, bool ignoreCollisions = false)
        {
            bool rv = false;
            //Create the X and Y rectangles to test for collisions
            Rectangle testRectX = new Rectangle((int)(Position.X + direction.X), (int)Position.Y, Width, Height);
            Rectangle testRectY = new Rectangle((int)Position.X, (int)(Position.Y + direction.Y), Width, Height);

            //If the CheckForCollisions gave the all clear, move the sprite.
            if (MapManager.Maps[CurrentMapName].CheckForCollisions(this, testRectX, testRectY, ref direction, ignoreCollisions) && direction != Vector2.Zero)
            {
                _bodySprite.MoveBy((int)direction.X, (int)direction.Y);
                rv = true;
            }

            string animation = string.Empty;
            DetermineFacing(direction);

            return rv;
        }
        
        public void SetMoveObj(Vector2 vec) { _vMoveTo = vec; }
    }

    public class PlayerCharacter : WorldCharacter
    {
        AnimatedSprite _spriteEyes;
        public AnimatedSprite EyeSprite => _spriteEyes;
        AnimatedSprite _spriteArms;
        public AnimatedSprite ArmSprite => _spriteArms;
        AnimatedSprite _spriteHair;
        public AnimatedSprite HairSprite => _spriteHair;

        Color _cHairColor = Color.White;
        public Color HairColor => _cHairColor;

        int _iHairIndex = 0;
        public int HairIndex => _iHairIndex;

        public override Vector2 Position
        {
            get { return new Vector2(_bodySprite.Position.X, _bodySprite.Position.Y + _bodySprite.Height - TileSize); }
            set
            {
                _bodySprite.Position = new Vector2(value.X, value.Y - _bodySprite.Height + TileSize);
                _spriteEyes.Position = _bodySprite.Position;
                _spriteArms.Position = _bodySprite.Position;
                _spriteHair.Position = _bodySprite.Position;

                if (_chest != null) { _chest.SetSpritePosition(_bodySprite.Position); }
                if (Hat != null) { _hat.SetSpritePosition(_bodySprite.Position); }
            }
        }

        Clothes _hat;
        public Clothes Hat => _hat;
        Clothes _chest;
        public Clothes Chest => _chest;
        Clothes Back;
        Clothes Hands;
        Clothes Legs;
        Clothes Feet;

        public PlayerCharacter() :base()
        {
            _width = TileSize;
            _height = TileSize;

            _cHairColor = Color.Red;

            Speed = 2;
        }

        public override void Update(GameTime theGameTime)
        {
            _bodySprite.Update(theGameTime);
            _spriteEyes.Update(theGameTime);
            _spriteArms.Update(theGameTime);
            _spriteHair.Update(theGameTime);

            if(_chest != null) { _chest.Sprite.Update(theGameTime); }
            if (Hat != null) { Hat.Sprite.Update(theGameTime); }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            _bodySprite.Draw(spriteBatch, useLayerDepth);
            _spriteEyes.Draw(spriteBatch, useLayerDepth);
            _spriteArms.Draw(spriteBatch, useLayerDepth);
            _spriteHair.Draw(spriteBatch, useLayerDepth);

            if (_chest != null) { _chest.Sprite.Draw(spriteBatch, useLayerDepth); }
            if (Hat != null) { Hat.Sprite.Draw(spriteBatch, useLayerDepth); }
        }

        public override void LoadContent(string textureToLoad)
        {
            Color bodyColor = Color.White;

            AddDefaultAnimations(ref _bodySprite, textureToLoad, 0, 0, true);
            _bodySprite.SetColor(bodyColor);

            AddDefaultAnimations(ref _spriteEyes, textureToLoad, 0, TileSize * 2, true);
            _spriteEyes.SetDepthMod(0.001f);
            
            AddDefaultAnimations(ref _spriteArms, textureToLoad, 0, TileSize * 4, true);
            _spriteArms.SetDepthMod(0.002f);
            _spriteArms.SetColor(bodyColor);

            AddDefaultAnimations(ref _spriteHair, @"Textures\texPlayerHair", 0, _iHairIndex * TileSize * 2, true);
            _spriteHair.SetColor(_cHairColor);
            _spriteHair.SetDepthMod(0.003f);

            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
        }

        public void SetHairColor(Color c)
        {
            _cHairColor = c;
            SetColor(_spriteHair, c);
        }

        public void SetColor(AnimatedSprite sprite, Color c)
        {
            sprite.SetColor(c);
        }

        public void SetHairType(int index)
        {
            _iHairIndex = index;
            AddDefaultAnimations(ref _spriteHair, @"Textures\texPlayerHair", 0, _iHairIndex * TileSize * 2, true);
            _spriteHair.SetColor(_cHairColor);
            _spriteHair.SetDepthMod(0.003f);
        }

        public override void Idle()
        {
            string animation = string.Empty;
            switch (Facing)
            {
                case DirectionEnum.Down:
                    animation = "IdleDown";
                    break;
                case DirectionEnum.Up:
                    animation = "IdleUp";
                    break;
                case DirectionEnum.Left:
                    animation = "IdleLeft";
                    break;
                case DirectionEnum.Right:
                    animation = "IdleRight";
                    break;
            }

            PlayAnimation(animation);
        }

        public void MoveBy(int x, int y)
        {
            _bodySprite.MoveBy(x, y);
            _spriteEyes.MoveBy(x, y);
            _spriteArms.MoveBy(x, y);
            _spriteHair.MoveBy(x, y);
            if (_chest != null) { _chest.Sprite.MoveBy(x, y); }
            if (Hat != null) { Hat.Sprite.MoveBy(x, y); }
        }

        public override void PlayAnimation(string anim)
        {
            _bodySprite.SetCurrentAnimation(anim);
            _spriteEyes.SetCurrentAnimation(anim);
            _spriteArms.SetCurrentAnimation(anim);
            _spriteHair.SetCurrentAnimation(anim);

            if (_chest != null) { _chest.Sprite.SetCurrentAnimation(anim); }
            if (Hat != null) { Hat.Sprite.SetCurrentAnimation(anim); }
        }

        public void SetScale(int scale = 1)
        {
            _bodySprite.SetScale(scale);
            _spriteEyes.SetScale(scale);
            _spriteArms.SetScale(scale);
            _spriteHair.SetScale(scale);

            if (_chest != null) { _chest.Sprite.SetScale(scale); }
            if (_hat != null) { _hat.Sprite.SetScale(scale); }
        }

        public void SetClothes(Clothes c)
        {
            if (c != null)
            {
                if (c.IsShirt()) { _chest = c; }
                else if (c.IsHat())
                {
                    _spriteHair.FrameCutoff = 9;
                    _hat = c;
                }

                c.Sprite.Position = _bodySprite.Position;
                c.Sprite.CurrentAnimation = _bodySprite.CurrentAnimation;
                c.Sprite.SetDepthMod(0.004f);
            }
        }

        public void RemoveClothes(Clothes c)
        {
            if (c.IsShirt()) { _chest = null; }
            else if (c.IsHat())
            {
                _spriteHair.FrameCutoff = 0;
                _hat = null;
            }
        }
    }
}
