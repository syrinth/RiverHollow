﻿using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;

namespace RiverHollow.Characters
{
    public class WorldCharacter : Character
    {
        #region Properties
        protected string _name;
        public string Name { get => _name; }

        public enum Direction { North, South, East, West };
        public Direction Facing = Direction.North;
        public Texture2D Texture { get => _sprite.Texture; }
        public Point Center => GetRectangle().Center;
        public override Vector2 Position
        {
            get { return new Vector2(_sprite.Position.X, _sprite.Position.Y + _sprite.Height - RHMap.TileSize); } //MAR this is fucked up
            set { _sprite.Position = value; }
        }

        public Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, Width, RHMap.TileSize); }


        public int Speed = 3;
        #endregion

        public WorldCharacter() : base()
        {
            _width = RHMap.TileSize;
            _height = RHMap.TileSize;
        }

        public virtual bool Contains(Point mouse)
        {
            return CollisionBox.Contains(mouse);
        }

        protected void DetermineAnimation(ref string animation, Vector2 direction, float deltaX, float deltaY)
        {
            if (deltaX > deltaY)
            {
                if (direction.X > 0)
                {
                    Facing = Direction.West;
                    //animation = "Float";
                }
                else
                {
                    Facing = Direction.East;
                    //animation = "Float";
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    Facing = Direction.South;
                    //animation = "Float";
                }
                else
                {
                    Facing = Direction.North;
                    //animation = "Float";
                }
            }
        }

        protected void CheckMapForCollisionsAndMove(Vector2 direction)
        {
            Rectangle testRectX = new Rectangle((int)(Position.X + direction.X), (int)Position.Y, Width, Height);
            Rectangle testRectY = new Rectangle((int)Position.X, (int)(Position.Y + direction.Y), Width, Height);

            if (MapManager.CurrentMap.CheckLeftMovement(this, testRectX) && MapManager.CurrentMap.CheckRightMovement(this, testRectX))
            {

                _sprite.MoveBy(direction.X, 0);
            }

            if (MapManager.CurrentMap.CheckUpMovement(this, testRectY) && MapManager.CurrentMap.CheckDownMovement(this, testRectY))
            {
                _sprite.MoveBy(0, direction.Y);
            }
        }

        public void MoveBy(int x, int y)
        {
            _sprite.MoveBy(x, y);
        }
    }
}
