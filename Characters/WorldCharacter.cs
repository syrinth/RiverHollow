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
        public DirectionEnum Facing = DirectionEnum.Up;
        public Texture2D Texture { get => _sprite.Texture; }
        public Point CharCenter => GetRectangle().Center;
        public override Vector2 Position
        {
            get { return new Vector2(_sprite.Position.X, _sprite.Position.Y + _sprite.Height - TileSize); } //MAR this is fucked up
            set { _sprite.Position = new Vector2(value.X, value.Y - _sprite.Height + TileSize); }
        }



        public int Speed = 3;
        #endregion

        public WorldCharacter() : base()
        {
            _characterType = CharacterEnum.WorldCharacter;
            _width = TileSize;
            _height = TileSize;
        }

        public void LoadContent(string textureToLoad)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(textureToLoad));
            _sprite.AddAnimation("WalkDown", TileSize, TileSize, 2, 0.2f, 0, 0);
            _sprite.AddAnimation("IdleDown", TileSize, TileSize, 1, 0.2f, 0, 0);
            _sprite.AddAnimation("WalkUp", TileSize, TileSize, 2, 0.2f, 0, TileSize);
            _sprite.AddAnimation("IdleUp", TileSize, TileSize, 1, 0.2f, 0, TileSize);
            _sprite.AddAnimation("WalkRight", TileSize, TileSize, 2, 0.2f, TileSize*2, 0);
            _sprite.AddAnimation("IdleRight", TileSize, TileSize, 1, 0.2f, TileSize * 2, 0);
            _sprite.AddAnimation("WalkLeft", TileSize, TileSize, 2, 0.2f, TileSize * 2, TileSize);
            _sprite.AddAnimation("IdleLeft", TileSize, TileSize, 1, 0.2f, TileSize * 2, TileSize);
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
        }

        public void MoveBy(int x, int y)
        {
            _sprite.MoveBy(x, y);
        }
        public void SetMoveObj(Vector2 vec) { _vMoveTo = vec; }
    }
}
