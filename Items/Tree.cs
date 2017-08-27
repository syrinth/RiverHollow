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
    public class Tree : WorldObject
    {
        public Tree(ObjectManager.ObjectIDs id, float hp, bool breakIt, bool chopIt, Vector2 pos, Texture2D tex, int lvl, int width, int height):base(id, hp, breakIt, chopIt, pos, tex, lvl, width, height)
        {
            _collisionBox = new Rectangle((int)Position.X + TileMap.TileSize, (int)Position.Y + TileMap.TileSize*3, TileMap.TileSize, TileMap.TileSize);
        }

        public override bool IntersectsWith(Rectangle r)
        {
            return _collisionBox.Intersects(r);
        }

        public override bool Contains(Point m)
        {
            return _collisionBox.Contains(m);
        }
    }
}
