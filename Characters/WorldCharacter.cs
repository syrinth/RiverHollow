using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using System;

namespace RiverHollow.Characters
{
    public class WorldCharacter : Character
    {
        #region Properties
        protected Vector2 _vMoveTo;
        public Vector2 MoveToLocation { get => _vMoveTo; }
        public string CurrentMapName;
        public Vector2 NewMapPosition;
        public enum DirectionEnum { North, South, East, West };
        public DirectionEnum Facing = DirectionEnum.North;
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

        public virtual bool CollisionContains(Point mouse)
        {
            return CollisionBox.Contains(mouse);
        }

        public void SetDirection(DirectionEnum d)
        {
            Facing = d;
            if(d == DirectionEnum.North) { _sprite.CurrentAnimation = "Walk North"; }
            else if (d == DirectionEnum.South) { _sprite.CurrentAnimation = "Walk South"; }
            else if (d == DirectionEnum.East) { _sprite.CurrentAnimation = "Walk East"; }
            else if (d == DirectionEnum.West) { _sprite.CurrentAnimation = "Walk West"; }
        }

        protected void DetermineAnimation(ref string animation, Vector2 direction, float deltaX, float deltaY)
        {
            if (Math.Abs(deltaX) > Math.Abs(deltaY))
            {
                if (direction.X > 0)
                {
                    Facing = DirectionEnum.East;
                    animation = "Walk East";
                }
                else
                {
                    Facing = DirectionEnum.West;
                    animation = "Walk West";
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    Facing = DirectionEnum.South;
                    animation = "Walk South";
                }
                else
                {
                    Facing = DirectionEnum.North;
                    animation = "Walk North";
                }
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
            DetermineAnimation(ref animation, direction, direction.X, direction.Y);

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
