using Adventure.Game_Managers;
using Adventure.SpriteAnimations;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Characters
{
    public class WorldCharacter : Character
    {
        #region Properties
        public enum Facing { North, South, East, West };
        protected Facing _facing = Facing.North;
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

        protected int _speed = 3;
        public int Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }
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
                    _facing = Facing.West;
                    animation = "Float";
                }
                else
                {
                    _facing = Facing.East;
                    animation = "Float";
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    _facing = Facing.South;
                    animation = "Float";
                }
                else
                {
                    _facing = Facing.North;
                    animation = "Float";
                }
            }
        }
    }
}
