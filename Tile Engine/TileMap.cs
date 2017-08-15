using Microsoft.Xna.Framework;
using System.Collections.Generic;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Xml.Linq;
using System;
using Adventure.Characters;
using Adventure.Characters.Monsters;
using Adventure.Characters.NPCs;
using Adventure.Game_Managers;

namespace Adventure.Tile_Engine
{
    public class TileMap
    {
        public int MapWidth = 100;
        public int MapHeight = 100;
        public static int _tileSize = 32;
        public static int TileSize { get => _tileSize; }
        public string _name;

        protected TiledMap _map;
        protected TiledMapRenderer renderer;
        protected List<TiledMapTileset> _tileSets;

        protected List<Character> _characterList;

        private PlayerManager _playerManager = PlayerManager.GetInstance();

        public TileMap()
        {
            _tileSets = new List<TiledMapTileset>();
            _characterList = new List<Character>();
        }

        public void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice, string newMap)
        {
            // ContentReader reader = new ContentReader(Content, GraphicsDevice);
            _map = Content.Load<TiledMap>(newMap);
            _tileSize = _map.TileWidth;
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

            _name = _map.Name;
            if (_name.Contains("Map1"))
            {
                _characterList.Add(new Goblin(new Vector2(500, 800)));
                _characterList.Add(new Wizard(new Vector2(300, 300)));
                ((Wizard)_characterList[1]).MakeDailyItem();
            }
        }

        public void Update(GameTime theGameTime)
        {
            foreach(Character m in _characterList)
            {
                if (m.GetType().Equals(typeof(Monster))){
                    ((Monster)m).Update(theGameTime);
                }
                else
                {
                    m.Update(theGameTime);
                }
            }

        }

        public void Draw(SpriteBatch spriteBatch)
        {
            renderer.Draw(_map, Camera._transform);
            foreach(Character m in _characterList)
            {
                m.Draw(spriteBatch);
            }
        }

        #region Collision Code
        public bool CheckLeftMovement(Rectangle movingObject)
        {
            bool rv = true;
            int columnTile = movingObject.Left / _tileSize;
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                for (int y = GetMinRow(movingObject); y <= GetMaxRow(movingObject); y++)
                {
                    Nullable<TiledMapTile> tile;
                    l.TryGetTile(columnTile, y, out tile);

                    if(tile != null)
                    {
                        Rectangle cellRect = new Rectangle(columnTile* _tileSize, y* _tileSize, _tileSize, _tileSize);
                        if (BlocksMovement((TiledMapTile)tile) && cellRect.Intersects(movingObject))
                        {
                            if (cellRect.Right >= movingObject.Left)
                            {
                                rv = false;
                            }
                        }
                        MapChange((TiledMapTile)tile);
                    }
                }
            }

            return rv;
        }

        public bool CheckRightMovement(Rectangle movingObject)
        {
            bool rv = true;
            int columnTile = movingObject.Right / _tileSize;
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                for (int y = GetMinRow(movingObject); y <= GetMaxRow(movingObject); y++)
                {
                    Nullable<TiledMapTile> tile;
                    l.TryGetTile(columnTile, y, out tile);

                    if (tile != null)
                    {
                        Rectangle cellRect = new Rectangle(columnTile * _tileSize, y * _tileSize, _tileSize, _tileSize);
                        if (BlocksMovement((TiledMapTile)tile) && cellRect.Intersects(movingObject))
                        {
                            if (cellRect.Left <= movingObject.Right)
                            {
                                rv = false;
                            }
                        }
                        MapChange((TiledMapTile)tile);
                    }
                }
            }

            return rv;
        }

        public bool CheckUpMovement(Rectangle movingObject)
        {
            bool rv = true;
            int rowTile = movingObject.Top / _tileSize;
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                for (int x = GetMinColumn(movingObject); x <= GetMaxColumn(movingObject); x++)
                {
                    Nullable<TiledMapTile> tile;
                    l.TryGetTile(x, rowTile, out tile);

                    if (tile != null)
                    {
                        Rectangle cellRect = new Rectangle(x * _tileSize, rowTile * _tileSize, _tileSize, _tileSize);
                        if (BlocksMovement((TiledMapTile)tile) && cellRect.Intersects(movingObject))
                        {
                            if (cellRect.Bottom >= movingObject.Top)
                            {
                                rv = false;
                            }
                        }
                        MapChange((TiledMapTile)tile);
                    }
                }
            }
            return rv;
        }

        public bool CheckDownMovement(Rectangle movingObject)
        {
            bool rv = true;
            int rowTile = movingObject.Bottom / _tileSize;
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                for (int x = GetMinColumn(movingObject); x <= GetMaxColumn(movingObject); x++)
                {
                    Nullable<TiledMapTile> tile;
                    l.TryGetTile(x, rowTile, out tile);

                    if (tile != null)
                    {
                        Rectangle cellRect = new Rectangle(x * _tileSize, rowTile * _tileSize, _tileSize, _tileSize);
                        if (BlocksMovement((TiledMapTile)tile) && cellRect.Intersects(movingObject))
                        {
                            if (cellRect.Top <= movingObject.Bottom)
                            {
                                rv = false;
                            }
                        }
                        MapChange((TiledMapTile)tile);
                    }
                }
            }

            return rv;
        }

        public bool BlocksMovement(TiledMapTile tile)
        {
            bool rv = false;
            foreach (KeyValuePair<string, string> tp in GetProperties(tile))
            {
                if (tp.Key.Equals("Impassable") && tp.Value.Equals("true"))
                {
                    rv = true;
                }
            }
            return rv;
        }

        public void MapChange(TiledMapTile tile)
        {
            foreach (KeyValuePair<string, string> tp in GetProperties(tile))
            {
                if (tp.Key.Equals("GoTo"))
                {
                    MapManager.GetInstance().SetCurrentMap(tp.Value);

                }
            }
        }

        public List<KeyValuePair<string, string>> GetProperties(TiledMapTile tile)
        {
            List<KeyValuePair<string, string>> propList = new List<KeyValuePair<string, string>>();
            foreach (TiledMapTileset ts in _tileSets)
            {
                foreach (TiledMapTilesetTile t in ts.Tiles)
                {
                    if (tile.GlobalIdentifier - 1 == t.LocalTileIdentifier)
                    {
                        foreach (KeyValuePair<string, string> tp in t.Properties)
                        {
                            propList.Add(tp);
                        }
                    }
                }
            }
            return propList;
        }

        #region Collision Helpers
        public int GetMinColumn(Rectangle movingObject)
        {
            return (movingObject.Left / _tileSize);
        }

        public int GetMaxColumn(Rectangle movingObject)
        {
            int i = (movingObject.Right / _tileSize);
            return i;
        }

        public int GetMinRow(Rectangle movingObject)
        {
            return (movingObject.Top / _tileSize);
        }

        public int GetMaxRow(Rectangle movingObject)
        {
            return (movingObject.Bottom / _tileSize);
        }
        #endregion
        #endregion

        public bool ProcessMapClick(Point mouseLocation)
        {
            bool rv = false;

            foreach(Character c in _characterList)
            {
                if (c.GetType().Equals(typeof(Wizard)))
                {
                    Worker wiz = (Worker)c;
                    if (wiz.MouseInside(mouseLocation) && wiz.PlayerInRange(_playerManager.Player.GetRectangle()) &&
                        _playerManager.Player.HasSpaceInInventory(wiz.WhatAreYouHolding()))
                    {
                        _playerManager.Player.AddItemToFirstAvailableInventory(wiz.TakeItem());
                        wiz.MakeDailyItem();
                    }
                }
            }

            return rv;
        }
        public int GetMapWidth()
        {
            return MapWidth * _tileSize;
        }

        public int GetMapHeight()
        {
            return MapHeight * _tileSize;
        }
    }
}
