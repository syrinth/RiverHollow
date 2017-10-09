using Adventure.Game_Managers;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public class WorldObject
    {
        #region Properties
        public List<RHMapTile> Tiles;
        protected float _hp;
        public float HP { get => _hp; }

        protected bool _wallObject;
        public bool WallObject { get => _wallObject; }

        protected bool _destructible;
        public bool Destructible { get => _destructible; }

        protected bool _breakable;
        public bool Breakable { get => _breakable;}

        protected bool _choppable;
        public bool Choppable { get => _choppable; }

        protected Vector2 _position;
        public Vector2 Position { get => _position; }

        protected Rectangle _sourceRectangle;
        public Rectangle SourceRectangle { get => _sourceRectangle;  }

        public virtual Rectangle CollisionBox {  get => new Rectangle((int) Position.X, (int) Position.Y, _width, _height); }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected int _width;
        protected int _height;

        protected int _lvltoDmg;
        public int LvlToDmg { get => _lvltoDmg; }

        protected int _id;
        public int ID { get => _id; }
        #endregion

        protected WorldObject() { }

        public WorldObject(int id, float hp, bool destructible, bool breakIt, bool chopIt, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int lvl, int width, int height)
        {
            _id = id;
            _hp = hp;
            _destructible = destructible;
            _breakable = breakIt;
            _choppable = chopIt;
            _position = pos;
            _width = width;
            _height = height;
            _texture = tex;
            _lvltoDmg = lvl;
            _wallObject = false;

            _sourceRectangle = sourceRectangle;
            Tiles = new List<RHMapTile>();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, _width, _height), _sourceRectangle, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, _position.Y + _height + (_position.X / 100));
        }

        public bool DealDamage(float dmg)
        {
            bool rv = false;
            _hp -= dmg;
            rv = _hp <= 0;

            return rv;
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
            foreach (RHMapTile t in Tiles)
            {
                t.Clear();
            }
        }
    }

    public class Tree : WorldObject
    {
        public override Rectangle CollisionBox { get => new Rectangle((int)Position.X + RHTileMap.TileSize, (int)Position.Y + RHTileMap.TileSize * 3, RHTileMap.TileSize, RHTileMap.TileSize); }

        public Tree(int id, float hp, bool breakIt, bool chopIt, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int lvl, int width, int height) : base(id, hp, true, breakIt, chopIt, pos, sourceRectangle, tex, lvl, width, height)
        {
        }
    }

    public class Staircase : WorldObject
    {
        protected string _toMap;
        public string ToMap { get => _toMap; }

        public Staircase(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int lvl, int width, int height) : base(id, 0, false, false, false, pos, sourceRectangle, tex, lvl, width, height)
        {
            _wallObject = true;
        }

        public void SetExit(string map)
        {
            _toMap = map;
        }
    }
}
