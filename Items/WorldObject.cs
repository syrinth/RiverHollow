using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RiverHollow.Items
{
    public class WorldObject
    {
        #region Properties
        public enum ObjectType { Container, WorldObject, Destructible };
        public ObjectType Type;

        public List<RHTile> Tiles;

        protected bool _wallObject;
        public bool WallObject { get => _wallObject; }

        protected Vector2 _position;
        public Vector2 Position { get => _position; }

        protected Rectangle _sourceRectangle;
        public Rectangle SourceRectangle { get => _sourceRectangle;  }

        public virtual Rectangle CollisionBox {  get => new Rectangle((int) Position.X, (int) Position.Y, _width, _height); }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected int _width;
        protected int _height;

        protected int _id;
        public int ID { get => _id; }
        #endregion

        protected WorldObject() { }

        public WorldObject(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height)
        {
            Type = ObjectType.WorldObject;
            _id = id;
            _position = pos;
            _width = width;
            _height = height;
            _texture = tex;
            _wallObject = false;

            _sourceRectangle = sourceRectangle;
            Tiles = new List<RHTile>();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, _width, _height), _sourceRectangle, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, _position.Y + _height + (_position.X / 100));
        }

        public virtual bool IntersectsWith(Rectangle r)
        {
            return CollisionBox.Intersects(r);
        }

        public virtual bool Contains(Point m)
        {
            return CollisionBox.Contains(m);
        }

        public virtual void SetCoordinates(Vector2 position)
        {
            _position = position;
        }

        public void ClearTiles()
        {
            foreach (RHTile t in Tiles)
            {
                t.Clear();
            }
        }

        public bool IsDestructible() { return Type == ObjectType.Destructible; }
        public bool IsWorldObject() { return Type == ObjectType.WorldObject; }
    }

    public class Destructible : WorldObject
    {
        protected int _hp;
        public int HP { get => _hp; }

        protected bool _breakable;
        public bool Breakable { get => _breakable; }

        protected bool _choppable;
        public bool Choppable { get => _choppable; }

        protected int _lvltoDmg;
        public int LvlToDmg { get => _lvltoDmg; }

        public Destructible(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height, bool breakIt, bool chopIt, int lvl, int hp) : base(id, pos, sourceRectangle, tex, width, height)
        {
            Type = ObjectType.Destructible;

            _hp = hp;
            _breakable = breakIt;
            _choppable = chopIt;
            _lvltoDmg = lvl;
        }

        public bool DealDamage(int dmg)
        {
            bool rv = false;
            _hp -= dmg;
            rv = _hp <= 0;

            return rv;
        }
    }

    public class Tree : Destructible
    {
        public override Rectangle CollisionBox { get => new Rectangle((int)Position.X + RHMap.TileSize, (int)Position.Y + RHMap.TileSize * 3, RHMap.TileSize, RHMap.TileSize); }

        public Tree(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height, bool breakIt, bool chopIt, int lvl, int hp) : base(id, pos, sourceRectangle, tex, width, height, breakIt, chopIt, lvl, hp)
        {
            Type = ObjectType.Destructible;
        }
    }

    public class Staircase : WorldObject
    {
        protected string _toMap;
        public string ToMap { get => _toMap; }

        public Staircase(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height) : base(id, pos, sourceRectangle, tex, width, height)
        {
            Type = ObjectType.WorldObject;
            _wallObject = true;
        }

        public void SetExit(string map)
        {
            _toMap = map;
        }
    }
}
