using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using System;
using RiverHollow.SpriteAnimations;

using static RiverHollow.Game_Managers.GameManager;
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

        public Rectangle CollisionBox { get => new Rectangle((int)Position.X + (Width/4), (int)Position.Y+TileSize, Width/2, TileSize); }


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

        protected void CheckMapForCollisionsAndMove(Vector2 direction)
        {
            Rectangle testRectX = new Rectangle((int)(Position.X + direction.X), (int)Position.Y, Width, Height);
            Rectangle testRectY = new Rectangle((int)Position.X, (int)(Position.Y + direction.Y), Width, Height);

            if (MapManager.Maps[CurrentMapName].CheckForCollisions(this, testRectX, testRectY, ref direction))
            {
                _bodySprite.MoveBy((int)direction.X, (int)direction.Y);
            }

            string animation = string.Empty;
            DetermineFacing(direction);
        }
        
        public void SetMoveObj(Vector2 vec) { _vMoveTo = vec; }
    }

    public class PlayerCharacter : WorldCharacter
    {
        AnimatedSprite _spriteEyes;
        AnimatedSprite _spriteArms;

        public override Vector2 Position
        {
            get { return new Vector2(_bodySprite.Position.X, _bodySprite.Position.Y); }
            set {
                _bodySprite.Position = value;
                _spriteEyes.Position = value;
                _spriteArms.Position = value;
            }
        }

        public PlayerCharacter() :base()
        {
            _width = TileSize;
            _height = TileSize;
        }

        public override void Update(GameTime theGameTime)
        {
            _bodySprite.Update(theGameTime);
            _spriteEyes.Update(theGameTime);
            _spriteArms.Update(theGameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            _bodySprite.Draw(spriteBatch, useLayerDepth);
            _spriteEyes.Draw(spriteBatch, useLayerDepth);
            _spriteArms.Draw(spriteBatch, useLayerDepth);
        }

        public override void LoadContent(string textureToLoad)
        {
            int startX = 0;
            int startY = 0;

            Color bodyColor = Color.White;

            AddDefaultAnimations(ref _bodySprite, textureToLoad, startX, startY, true);
            _bodySprite.SetColor(bodyColor);

            startX = 0;
            startY = TileSize * 2;
            AddDefaultAnimations(ref _spriteEyes, textureToLoad, startX, startY, true);
            _spriteEyes.SetDepthMod(0.001f);
            

            startX = 0;
            startY = TileSize * 4;
            AddDefaultAnimations(ref _spriteArms, textureToLoad, startX, startY, true);
            _spriteArms.SetDepthMod(0.002f);
            _spriteArms.SetColor(bodyColor);

            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
        }

        public void SetColor(AnimatedSprite sprite, Color c)
        {
            sprite.SetColor(c);
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
        }

        public override void PlayAnimation(string anim)
        {
            _bodySprite.SetCurrentAnimation(anim);
            _spriteEyes.SetCurrentAnimation(anim);
            _spriteArms.SetCurrentAnimation(anim);
        }
    }
}
