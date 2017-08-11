using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml.Linq;
using System;

namespace Adventure.Tile_Engine
{
    public class TileMap
    {
        public List<MapRow> Rows = new List<MapRow>();
        public int MapWidth = 100;
        public int MapHeight = 100;
        public int _tileWidth = 32;
        public int _tileHeight = 32;

        protected TiledMap _map;
        protected TiledMapRenderer renderer;
        protected List<TiledMapTileset> _tileSets;

        public TileMap()
        {
            _tileSets = new List<TiledMapTileset>();
        }

        public void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice, string newMap)
        {
            // ContentReader reader = new ContentReader(Content, GraphicsDevice);
            _map = Content.Load<TiledMap>(newMap);
            _tileWidth = _map.TileWidth;
            _tileHeight = _map.TileHeight;
            renderer = new TiledMapRenderer(GraphicsDevice);
            
            XDocument xDoc = XDocument.Load("..\\..\\..\\..\\Content\\" + newMap+".tmx");
            MapWidth = int.Parse(xDoc.Root.Attribute("width").Value);
            MapHeight = int.Parse(xDoc.Root.Attribute("height").Value);
            int tileCount = int.Parse(xDoc.Root.Element("tileset").Attribute("tilecount").Value);
            int columns = int.Parse(xDoc.Root.Element("tileset").Attribute("columns").Value);

            foreach (XElement e in xDoc.Root.Elements("tileset"))
            {
                int elem = int.Parse(e.Attribute("firstgid").Value);
                TiledMapTileset t = _map.GetTilesetByTileGlobalIdentifier(elem);
                _tileSets.Add(t);
                
            }
        }

        public void Draw()
        {
            renderer.Draw(_map, Camera._transform);
        }

        public bool CheckLeftMovement(Rectangle movingObject)
        {
            bool rv = true;
            int columnTile = movingObject.Left / _tileWidth;
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                for (int y = GetMinRow(movingObject); y <= GetMaxRow(movingObject); y++)
                {
                    Nullable<TiledMapTile> tile;
                    l.TryGetTile(columnTile, y, out tile);

                    if(tile != null)
                    {
                        Rectangle cellRect = new Rectangle(columnTile*_tileWidth, y*_tileHeight, _tileWidth, _tileHeight);
                        if (BlocksMovement((TiledMapTile)tile) && cellRect.Intersects(movingObject))
                        {
                            if (cellRect.Right >= movingObject.Left)
                            {
                                rv = false;
                            }
                        }
                    }
                }
            }

            return rv;
        }

        public bool CheckRightMovement(Rectangle movingObject)
        {
            bool rv = true;
            int columnTile = movingObject.Right / Tile.TILE_WIDTH;
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                for (int y = GetMinRow(movingObject); y <= GetMaxRow(movingObject); y++)
                {
                    Nullable<TiledMapTile> tile;
                    l.TryGetTile(columnTile, y, out tile);

                    if (tile != null)
                    {
                        Rectangle cellRect = new Rectangle(columnTile * _tileWidth, y * _tileHeight, _tileWidth, _tileHeight);
                        if (BlocksMovement((TiledMapTile)tile) && cellRect.Intersects(movingObject))
                        {
                            if (cellRect.Left <= movingObject.Right)
                            {
                                rv = false;
                            }
                        }
                    }
                }
            }

            return rv;
        }

        public bool CheckUpMovement(Rectangle movingObject)
        {
            bool rv = true;
            int rowTile = movingObject.Top / _tileHeight;
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                for (int x = GetMinColumn(movingObject); x <= GetMaxColumn(movingObject); x++)
                {
                    Nullable<TiledMapTile> tile;
                    l.TryGetTile(x, rowTile, out tile);

                    if (tile != null)
                    {
                        Rectangle cellRect = new Rectangle(x * _tileWidth, rowTile * _tileHeight, _tileWidth, _tileHeight);
                        if (BlocksMovement((TiledMapTile)tile) && cellRect.Intersects(movingObject))
                        {
                            if (cellRect.Bottom >= movingObject.Top)
                            {
                                rv = false;
                            }
                        }
                    }
                }
            }
            return rv;
        }

        public bool CheckDownMovement(Rectangle movingObject)
        {
            bool rv = true;
            int rowTile = movingObject.Bottom / _tileHeight;
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                for (int x = GetMinColumn(movingObject); x <= GetMaxColumn(movingObject); x++)
                {
                    Nullable<TiledMapTile> tile;
                    l.TryGetTile(x, rowTile, out tile);

                    if (tile != null)
                    {
                        Rectangle cellRect = new Rectangle(x * _tileWidth, rowTile * _tileHeight, _tileWidth, _tileHeight);
                        if (BlocksMovement((TiledMapTile)tile) && cellRect.Intersects(movingObject))
                        {
                            if (cellRect.Top <= movingObject.Bottom)
                            {
                                rv = false;
                            }
                        }
                    }
                }
            }

            return rv;
        }

        #region Collision Helpers
        public bool BlocksMovement(TiledMapTile tile)
        {
            foreach (TiledMapTilesetTile t in _tileSets[0].Tiles)
            {
                if (tile.GlobalIdentifier - 1 == t.LocalTileIdentifier)
                {
                    foreach (KeyValuePair<string, string> tp in t.Properties)
                    {
                        if (tp.Key.Equals("Impassable") && tp.Value.Equals("true"))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public int GetMinColumn(Rectangle movingObject)
        {
            return (movingObject.Left / _tileWidth);
        }

        public int GetMaxColumn(Rectangle movingObject)
        {
            int i = (movingObject.Right / _tileWidth);
            return i;
        }

        public int GetMinRow(Rectangle movingObject)
        {
            return (movingObject.Top / _tileHeight);
        }

        public int GetMaxRow(Rectangle movingObject)
        {
            return (movingObject.Bottom / _tileHeight);
        }
        #endregion

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
