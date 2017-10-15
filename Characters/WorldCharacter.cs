using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Characters
{
    public class WorldCharacter : Character
    {
        #region Properties
        public enum Direction { North, South, East, West };
        public Direction Facing = Direction.North;
        public Texture2D Texture { get => _sprite.Texture; }
        public Point Center => GetRectangle().Center;
        public override Vector2 Position
        {
            get { return new Vector2(_sprite.Position.X, _sprite.Position.Y + _sprite.Height - RHMap.TileSize); }
            set { _sprite.Position = value; }
        }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }
        public Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, Width, RHMap.TileSize); }

        public int Width
        {
            get { return RHMap.TileSize; }
        }

        public int Height
        {
            get { return RHMap.TileSize; }
        }

        public int Speed = 3;
        #endregion

        public bool Contains(Point mouse)
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
                    animation = "Float";
                }
                else
                {
                    Facing = Direction.East;
                    animation = "Float";
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    Facing = Direction.South;
                    animation = "Float";
                }
                else
                {
                    Facing = Direction.North;
                    animation = "Float";
                }
            }
        }

        public void MoveBy(int x, int y)
        {
            _sprite.MoveBy(x, y);
        }
    }
}
