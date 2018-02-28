using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using System;
using RiverHollow.SpriteAnimations;

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
        public DirectionEnum Facing = DirectionEnum.Up;
        public Texture2D Texture { get => _sprite.Texture; }
        public Point CharCenter => GetRectangle().Center;
        public override Vector2 Position
        {
            get { return new Vector2(_sprite.Position.X, _sprite.Position.Y + _sprite.Height - RHMap.TileSize); } //MAR this is fucked up
            set { _sprite.Position = new Vector2(value.X, value.Y - _sprite.Height + RHMap.TileSize); }
        }

        public Rectangle CollisionBox { get => new Rectangle((int)Position.X + (Width/8), (int)Position.Y, Width/2, RHMap.TileSize); }


        public int Speed = 3;
        #endregion

        public WorldCharacter() : base()
        {
            _characterType = CharacterEnum.WorldCharacter;
            _width = RHMap.TileSize;
            _height = RHMap.TileSize;
        }

        public void LoadContent(string textureToLoad)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(textureToLoad));
            _sprite.AddAnimation("WalkDown", 32, 32, 2, 0.2f, 0, 0);
            _sprite.AddAnimation("IdleDown", 32, 32, 1, 0.2f, 0, 0);
            _sprite.AddAnimation("WalkUp", 32, 32, 2, 0.2f, 0, 32);
            _sprite.AddAnimation("IdleUp", 32, 32, 1, 0.2f, 0, 32);
            _sprite.AddAnimation("WalkRight", 32, 32, 2, 0.2f, 64, 0);
            _sprite.AddAnimation("IdleRight", 32, 32, 1, 0.2f, 64, 0);
            _sprite.AddAnimation("WalkLeft", 32, 32, 2, 0.2f, 64, 32);
            _sprite.AddAnimation("IdleLeft", 32, 32, 1, 0.2f, 64, 32);
            _sprite.SetCurrentAnimation("WalkUp");

            _width = _sprite.Width;
            _height = _sprite.Height;
        }
        public virtual bool CollisionContains(Point mouse)
        {
            return CollisionBox.Contains(mouse);
        }

        public void SetDirection(DirectionEnum d)
        {
            Facing = d;
            if(d == DirectionEnum.Up) { _sprite.CurrentAnimation = "Walk North"; }
            else if (d == DirectionEnum.Down) { _sprite.CurrentAnimation = "Walk South"; }
            else if (d == DirectionEnum.Right) { _sprite.CurrentAnimation = "Walk East"; }
            else if (d == DirectionEnum.Left) { _sprite.CurrentAnimation = "Walk West"; }
        }

        public void DetermineFacing(Vector2 direction)
        {
            string animation = string.Empty;

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
            if (direction.Length() == 0)
            {
                switch (Facing)
                {
                    case WorldCharacter.DirectionEnum.Down:
                        animation = "IdleDown";
                        break;
                    case WorldCharacter.DirectionEnum.Up:
                        animation = "IdleUp";
                        break;
                    case WorldCharacter.DirectionEnum.Left:
                        animation = "IdleLeft";
                        break;
                    case WorldCharacter.DirectionEnum.Right:
                        animation = "IdleRight";
                        break;
                }
            }

            if (_sprite.CurrentAnimation != animation)
            {
                _sprite.SetCurrentAnimation(animation);
            }
        }

        protected void CheckMapForCollisionsAndMove(Vector2 direction)
        {
            Rectangle testRectX = new Rectangle((int)(Position.X + direction.X), (int)Position.Y, Width, Height);
            Rectangle testRectY = new Rectangle((int)Position.X, (int)(Position.Y + direction.Y), Width, Height);

            if (MapManager.Maps[CurrentMapName].CheckForCollisions(this, testRectX, testRectY, ref direction))
            {
                _sprite.MoveBy((int)direction.X, (int)direction.Y);
            }

            string animation = string.Empty;
            DetermineFacing(direction);

            if (_sprite.CurrentAnimation != animation)
            {
                _sprite.CurrentAnimation = animation;
            }
        }

        public void MoveBy(int x, int y)
        {
            _sprite.MoveBy(x, y);
        }
        public void SetMoveObj(Vector2 vec) { _vMoveTo = vec; }
    }
}
