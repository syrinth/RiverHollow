using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Adventure.Tile_Engine
{
    public class TileMap
    {
        public List<MapRow> Rows = new List<MapRow>();
        public int MapWidth = 100;
        public int MapHeight = 100;

        public TileMap()
        {
            for (int y = 0; y < MapHeight; y++)
            {
                MapRow thisRow = new MapRow();
                for (int x = 0; x < MapWidth; x++)
                {
                    bool passable = true;
                    int tileID = 0;
                    if (x == 0 || y == 0 || x == 99 || y == 99)
                    {
                        passable = false;
                        tileID = 1;
                    }
                    if(x==25 && y == 25)
                    {
                        passable = false;
                        tileID = 1;
                    }
                    thisRow.Columns.Add(new MapCell(tileID, passable,new Rectangle(x*Tile.TILE_WIDTH, y*Tile.TILE_HEIGHT, Tile.TILE_WIDTH, Tile.TILE_HEIGHT)));
                }
                Rows.Add(thisRow);
            }
        }


        public bool CheckXMovement(Rectangle movingObject)
        {
            bool rv = true;
            int columnTile = movingObject.Left / Tile.TILE_WIDTH;

            foreach(MapRow r in Rows)
            {
                MapCell cell = r.Columns[columnTile];
                Rectangle cellRect = cell._rectangle;
                if (!cell.IsPassable && cell._rectangle.Intersects(movingObject))
                {
                    if (cell._rectangle.Right >= movingObject.Left)
                    {
                        rv = false;
                    }
                }
            }

            return rv;
        }

        public bool CheckRightMovement(Rectangle movingObject)
        {
            bool rv = true;
            int column = movingObject.Right / Tile.TILE_WIDTH;

            foreach (MapRow r in Rows)
            {
                MapCell cell = r.Columns[column];
                if (!cell.IsPassable && cell._rectangle.Intersects(movingObject))
                {
                    if (cell._rectangle.Left <= movingObject.Right)
                    {
                        rv = false;
                    }
                }
            }

            return rv;
        }

        public bool CheckUpMovement(Rectangle movingObject)
        {
            bool rv = true;
            int row = movingObject.Top / Tile.TILE_HEIGHT;

            foreach (MapCell cell in Rows[row].Columns)
            {
                if (!cell.IsPassable && cell._rectangle.Intersects(movingObject))
                {
                    if (cell._rectangle.Bottom >= movingObject.Top)
                    {
                        rv = false;
                    }
                }
            }
            return rv;
        }

        public bool CheckDownMovement(Rectangle movingObject)
        {
            bool rv = true;
            int row = movingObject.Bottom / Tile.TILE_HEIGHT;

            foreach (MapCell cell in Rows[row].Columns)
            {
                if (!cell.IsPassable && cell._rectangle.Intersects(movingObject))
                {
                    if (cell._rectangle.Top <= movingObject.Bottom)
                    {
                        rv = false;
                    }
                }
            }

            return rv;
        }

        public int GetMapWidth()
        {
            return MapWidth * Tile.TILE_WIDTH;
        }

        public int GetMapHeight()
        {
            return MapHeight * Tile.TILE_HEIGHT;
        }
    }

    public class MapRow
    {
        public List<MapCell> Columns = new List<MapCell>();
    }
}
