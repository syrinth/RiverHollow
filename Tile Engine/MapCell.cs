using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Tile_Engine
{
    public class MapCell
    {
        public List<int> BaseTiles = new List<int>();
        public Rectangle _rectangle;
        private bool _passable;
        public bool IsPassable
        {
            get { return _passable; }
            set { _passable = value; }
        }

        public int TileID
        {
            get { return BaseTiles.Count > 0 ? BaseTiles[0] : 0; }
            set
            {
                if (BaseTiles.Count > 0)
                {
                    BaseTiles[0] = value;
                }
                else
                {
                    AddBaseTile(value);
                }
            }
        }

        public MapCell(int tileID, bool passable, Rectangle rect)
        {
            TileID = tileID;
            _passable = passable;
            _rectangle = rect;
        }

        public void AddBaseTile(int tileID)
        {
            BaseTiles.Add(tileID);
        }
    }
}
