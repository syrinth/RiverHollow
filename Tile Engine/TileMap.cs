using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml.Linq;

namespace Adventure.Tile_Engine
{
    public class TileMap
    {
        public List<MapRow> Rows = new List<MapRow>();
        public int MapWidth = 100;
        public int MapHeight = 100;

        protected TiledMap map;
        protected TiledMapRenderer renderer;
        protected List<TiledMapLayer> tileLayers;

        public TileMap()
        {
        }

        public void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
           // ContentReader reader = new ContentReader(Content, GraphicsDevice);
            map = Content.Load<TiledMap>("Map1");
            renderer = new TiledMapRenderer(GraphicsDevice);
            xDocument

            TiledMapLayer tileLayer = map.GetLayer("Tile Layer 1");
            //TiledMapTileset tileset = new TiledMapTileset(reader);
            TiledMapTileset ts = map.GetTilesetByTileGlobalIdentifier(1);
            TiledMapProperties props = ts.Properties;
            if (props.ContainsKey("Impassable"))
            {
                int i = 0;
                i++;
            }
        }

        public void Draw()
        {
            renderer.Draw(map, Camera._transform);
        }

        public bool CheckXMovement(Rectangle movingObject)
        {
            bool rv = true;
            int columnTile = movingObject.Left / Tile.TILE_WIDTH;

            //foreach(MapRow r in Rows)
            //{
            //    MapCell cell = r.Columns[columnTile];
            //    Rectangle cellRect = cell._rectangle;
            //    if (!cell.IsPassable && cell._rectangle.Intersects(movingObject))
            //    {
            //        if (cell._rectangle.Right >= movingObject.Left)
            //        {
            //            rv = false;
            //        }
            //    }
            //}

            return rv;
        }

        public bool CheckRightMovement(Rectangle movingObject)
        {
            bool rv = true;
            int column = movingObject.Right / Tile.TILE_WIDTH;

            //foreach (MapRow r in Rows)
            //{
            //    MapCell cell = r.Columns[column];
            //    if (!cell.IsPassable && cell._rectangle.Intersects(movingObject))
            //    {
            //        if (cell._rectangle.Left <= movingObject.Right)
            //        {
            //            rv = false;
            //        }
            //    }
            //}

            return rv;
        }

        public bool CheckUpMovement(Rectangle movingObject)
        {
            bool rv = true;
            int row = movingObject.Top / Tile.TILE_HEIGHT;

            //foreach (MapCell cell in Rows[row].Columns)
            //{
            //    if (!cell.IsPassable && cell._rectangle.Intersects(movingObject))
            //    {
            //        if (cell._rectangle.Bottom >= movingObject.Top)
            //        {
            //            rv = false;
            //        }
            //    }
            //}
            return rv;
        }

        public bool CheckDownMovement(Rectangle movingObject)
        {
            bool rv = true;
            int row = movingObject.Bottom / Tile.TILE_HEIGHT;

            //foreach (MapCell cell in Rows[row].Columns)
            //{
            //    if (!cell.IsPassable && cell._rectangle.Intersects(movingObject))
            //    {
            //        if (cell._rectangle.Top <= movingObject.Bottom)
            //        {
            //            rv = false;
            //        }
            //    }
            //}

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
