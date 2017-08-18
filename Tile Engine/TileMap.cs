using Adventure.Characters;
using Adventure.Characters.Monsters;
using Adventure.Characters.NPCs;
using Adventure.Game_Managers;
using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Xml.Linq;

namespace Adventure.Tile_Engine
{
    public class TileMap
    {
        public int MapWidth = 100;
        public int MapHeight = 100;
        public static int _tileSize = 32;
        public static int TileSize { get => _tileSize; }
        private string _name;
        public string Name { get => _name.Replace(@"Maps\", ""); set => _name = value; } //Fuck off with that path bullshit

        protected TiledMap _map;
        protected TiledMapRenderer renderer;
        protected List<TiledMapTileset> _tileSets;

        protected List<Character> _characterList;
        protected List<Building> _buildingList;

        private Dictionary<Rectangle, string> _exitDictionary;
        private Dictionary<string, Rectangle> _entranceDictionary;
        public Dictionary<string, Rectangle> EntranceDictionary { get => _entranceDictionary; }

        private PlayerManager _playerManager = PlayerManager.GetInstance();

        public TileMap()
        {
            _tileSets = new List<TiledMapTileset>();
            _characterList = new List<Character>();
            _buildingList = new List<Building>();
            _exitDictionary = new Dictionary<Rectangle, string>();
            _entranceDictionary = new Dictionary<string, Rectangle>();
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
            }
            else if (_name.Contains("Map2"))
            {
                _characterList.Add(new ShopKeeper(new Vector2(1350, 1420)));
                //((Wizard)_characterList[0]).MakeDailyItem();
            }

            LoadEntranceObjects();
        }

        public void LoadEntranceObjects()
        {
            ReadOnlyCollection<TiledMapObjectLayer> entrLayer = _map.ObjectLayers;
            foreach (TiledMapObjectLayer ol in entrLayer)
            {
                foreach (TiledMapObject mapObject in ol.Objects)
                {
                    Rectangle r = new Rectangle((int)mapObject.Position.X, (int)mapObject.Position.Y, (int)mapObject.Size.Width, (int)mapObject.Size.Height);
                    if (mapObject.Properties.ContainsKey("Exit"))
                    {
                        _exitDictionary.Add(r, mapObject.Properties["Exit"]);
                    }
                    else if (mapObject.Properties.ContainsKey("Entrance"))
                    {
                        _entranceDictionary.Add(mapObject.Properties["Entrance"], r);
                    }
                }
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

            foreach (Building b in _buildingList)
            {
                b.Draw(spriteBatch);
            }
        }

        #region Collision Code
        public bool CheckLeftMovement(Rectangle movingObject)
        {
            bool rv = true;
            if (CheckForObjectCollision(movingObject))
            {
                return false;
            }

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
                        if (MapChange(movingObject))
                        {
                            return false;
                        }
                    }
                }
            }

            return rv;
        }

        public bool CheckRightMovement(Rectangle movingObject)
        {
            bool rv = true;
            if (CheckForObjectCollision(movingObject))
            {
                return false;
            }

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
                        if(MapChange(movingObject)) {
                            return false;
                        }
                    }
                }
            }

            return rv;
        }

        public bool CheckUpMovement(Rectangle movingObject)
        {
            bool rv = true;
            if (CheckForObjectCollision(movingObject))
            {
                return false;
            }

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
                        if (MapChange(movingObject))
                        {
                            return false;
                        }
                    }
                }
            }
            return rv;
        }

        public bool CheckDownMovement(Rectangle movingObject)
        {
            bool rv = true;
            if (CheckForObjectCollision(movingObject))
            {
                return false;
            }
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
                        if (MapChange(movingObject)) {
                            return false;
                        }
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

        public bool MapChange(Rectangle movingObject)
        {
            foreach(Rectangle r in _exitDictionary.Keys)
            {
                if (r.Intersects(movingObject))
                {
                    MapManager.GetInstance().ChangeMaps(_exitDictionary[r]);
                    return true;
                }
            }
            return false;
        }

        public bool CheckForObjectCollision(Rectangle movingObject)
        {
            bool rv = false;
            foreach (Building b in _buildingList)
            {
                if (b.BoundingBox.Intersects(movingObject))
                {
                    rv = true;
                    break;
                }
            }
            return rv;
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

        public bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            foreach(Character c in _characterList)
            {
                Type cType = c.GetType();
                if (cType.IsSubclassOf(typeof(Worker)))
                {
                    Worker w = (Worker)c;
                    if (w.MouseInside(mouseLocation) && PlayerInRange(_playerManager.Player.GetRectangle(), w.Center) &&
                        _playerManager.Player.HasSpaceInInventory(w.WhatAreYouHolding()))
                    {
                        _playerManager.Player.AddItemToFirstAvailableInventory(w.TakeItem());
                    }
                }
                else if (cType.Equals(typeof(ShopKeeper)) || (cType.IsSubclassOf(typeof(ShopKeeper))) && ((ShopKeeper)c).IsOpen)
                {
                    GUIManager.GetInstance().OpenShopWindow((ShopKeeper)c);
                }
            }
            foreach (Building b in _buildingList)
            {
                if (b.BoxToEnter.Contains(mouseLocation) && PlayerInRange(_playerManager.Player.GetRectangle(), b.BoxToEnter.Center))
                {
                    MapManager.GetInstance().EnterBuilding(b._map, b.ID.ToString(), b.Workers);
                    break;
                }
            }

            return rv;
        }

        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (AdventureGame.BuildingMode)
            {
                if(GraphicCursor.HeldBuilding != null)
                {
                    AddBuilding();
                    rv = true;
                }
                else if(GraphicCursor.WorkerToPlace != ItemManager.WorkerID.Nothing)
                {
                    if (AddWorkerToBuilding())
                    {
                        rv = true;
                    }
                }
            }
            else
            {
                foreach (Character c in _characterList)
                {
                    Type cType = c.GetType();
                    if (cType.IsSubclassOf(typeof(Worker)))
                    {
                        Worker w = (Worker)c;
                        if (w.MouseInside(mouseLocation) && PlayerInRange(_playerManager.Player.GetRectangle(), w.Center) &&
                            _playerManager.Player.HasSpaceInInventory(w.WhatAreYouHolding()))
                        {
                            _playerManager.Player.AddItemToFirstAvailableInventory(w.TakeItem());
                            w.MakeDailyItem();
                        }
                    }
                    else if (cType.Equals(typeof(ShopKeeper)) || (cType.IsSubclassOf(typeof(ShopKeeper))) && ((ShopKeeper)c).IsOpen)
                    {
                        GUIManager.GetInstance().OpenShopWindow((ShopKeeper)c);
                    }
                }
            }

            return rv;
        }
        public bool ProcessHover(Point mouseLocation)
        {
            bool rv = false;

            if (AdventureGame.BuildingMode)
            {
                foreach(Building b in _buildingList)
                {
                    if (b.BoundingBox.Contains(mouseLocation))
                    {
                        b._selected = true;
                    }
                    else
                    {
                        b._selected = false;
                    }
                }
            }

            return rv;
        }

        public bool PlayerInRange(Rectangle playerRect, Point centre)
        {
            bool rv = false;

            if (Math.Abs(playerRect.Center.X - centre.X) <= TileMap.TileSize*2 &&
                Math.Abs(playerRect.Center.Y - centre.Y) <= TileMap.TileSize*2)
            {
                rv = true;
            }

            return rv;
        }

        public void ClearWorkers()
        {
            _characterList.Clear();
        }
        public void AddWorkersToMap(List<Worker> workers)
        {
            _characterList.AddRange(workers);
        }
        public void AddBuilding()
        {
            Building b = GraphicCursor.HeldBuilding;
            Vector3 translate = Camera._transform.Translation;
            Vector2 newPos = new Vector2(b.Position.X - translate.X, b.Position.Y - translate.Y);
            b.SetCoordinates(newPos);
            _entranceDictionary.Add(b.ID.ToString(), b.BoxToExit); //TODO: FIX THIS
            GraphicCursor.DropBuilding();
            _buildingList.Add(b);
            _playerManager.AddBuilding(b);

            LeaveBuildingMode();
        }
        public bool AddWorkerToBuilding()
        {
            bool rv = false;
            foreach(Building b in _buildingList)
            {
                if (b.BoundingBox.Contains(GraphicCursor.Position))
                {
                    if (b.HasSpace())
                    {
                        b.AddWorker(ItemManager.GetWorker(GraphicCursor.WorkerToPlace));
                        LeaveBuildingMode();
                        b._selected = false;
                        rv = true;
                    }
                }
            }
            return rv;
        }

        public void LeaveBuildingMode()
        {
            AdventureGame.BuildingMode = false;
            Camera.ResetObserver();
            MapManager.GetInstance().BackToPlayer();
            GUIManager.GetInstance().LoadMainGame();
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
