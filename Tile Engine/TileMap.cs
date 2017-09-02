using Adventure.Characters;
using Adventure.Characters.Monsters;
using Adventure.Characters.NPCs;
using Adventure.Game_Managers;
using Adventure.GUIObjects;
using Adventure.Items;
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
        public List<Character> ToRemove;
        protected List<Building> _buildingList;
        protected List<WorldObject> _worldObjectList;
        public List<WorldObject> WorldObjects { get => _worldObjectList; }
        protected List<Item> _itemList;
        protected List<StaticItem> _staticItemList;
        public List<StaticItem> StaticItems { get => _staticItemList; }

        private Dictionary<Rectangle, string> _exitDictionary;
        private Dictionary<string, Rectangle> _entranceDictionary;
        public Dictionary<string, Rectangle> EntranceDictionary { get => _entranceDictionary; }

        public TileMap()
        {
            _tileSets = new List<TiledMapTileset>();
            _characterList = new List<Character>();
            ToRemove = new List<Character>();
            _buildingList = new List<Building>();
            _worldObjectList = new List<WorldObject>();
            _itemList = new List<Item>();
            _staticItemList = new List<StaticItem>();
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
            foreach (Item i in _itemList)
            {
                ((Item)i).Update();
            }
            foreach (Character m in _characterList)
            { 
                if (m.GetType().Equals(typeof(Monster))){
                    ((Monster)m).Update(theGameTime);
                }
                else
                {
                    m.Update(theGameTime);
                }
            }
            foreach(Character c in ToRemove)
            {
                _characterList.Remove(c);
            }

            ItemPickUpdate();
        }

        public void ItemPickUpdate()
        {
            Player _p = PlayerManager.Player;
            List<Item> removedList = new List<Item>();
            foreach (Item i in _itemList)
            {
                if (i.OnTheMap && i.Pickup)
                {
                    if (((Item)i).FinishedMoving() && i.CollisionBox.Intersects(PlayerManager.Player.CollisionBox))
                    {
                        removedList.Add(i);
                        PlayerManager.Player.AddItemToFirstAvailableInventory(i.ItemID);
                    }
                    else if (PlayerInRange(_p.CollisionBox, i.CollisionBox.Center, 80))
                    {
                        float speed = 3;
                        Vector2 direction = new Vector2((_p.Position.X < i.Position.X) ? -speed : speed, (_p.Position.Y < i.Position.Y) ? -speed : speed);
                        i.Position += direction;
                    }
                }
            }

            foreach (Item i in removedList)
            {
                _itemList.Remove(i);
            }
            removedList.Clear();
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

            foreach (WorldObject o in _worldObjectList)
            {
                o.Draw(spriteBatch);
            }

            foreach (Item i in _itemList)
            {
                i.Draw(spriteBatch);
            }

            foreach (StaticItem s in _staticItemList)
            {
                s.Draw(spriteBatch);
            }
        }

        #region Collision Code
        public bool CheckLeftMovement(Character c, Rectangle movingObject)
        {
            bool rv = true;
            if (CheckForObjectCollision(c, movingObject))
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

        public bool CheckRightMovement(Character c, Rectangle movingObject)
        {
            bool rv = true;
            if (CheckForObjectCollision(c, movingObject))
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

        public bool CheckUpMovement(Character c, Rectangle movingObject)
        {
            bool rv = true;
            if (CheckForObjectCollision(c, movingObject))
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

        public bool CheckDownMovement(Character c, Rectangle movingObject)
        {
            bool rv = true;
            if (CheckForObjectCollision(c, movingObject))
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
                if (tp.Key.Equals("Sleep") && tp.Value.Equals("true"))
                {
                    GUIManager.LoadScreen(GUIManager.Screens.Text, GameContentManager.GetDialogue("Sleep"));
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
                    MapManager.ChangeMaps(_exitDictionary[r]);
                    return true;
                }
            }
            return false;
        }

        public bool CheckForObjectCollision(Character mover, Rectangle movingObject)
        {
            bool rv = false;
            foreach (Building b in _buildingList)
            {
                if (b.CollisionBox.Intersects(movingObject))
                {
                    rv = true;
                    break;
                }
            }
            foreach (Character c in _characterList)
            {
                if (mover != c && c.CollisionBox.Intersects(movingObject))
                {
                    rv = true;
                    break;
                }
            }
            foreach (WorldObject o in _worldObjectList)
            {
                if (o.IntersectsWith(movingObject))
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

        #region Input Processing
        public bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            foreach (Character c in _characterList)
            {
                Type cType = c.GetType();
                if (cType.IsSubclassOf(typeof(Worker)))
                {
                    Worker w = (Worker)c;
                    if (w.MouseInside(mouseLocation) && PlayerInRange(PlayerManager.Player.GetRectangle(), w.Center) &&
                        PlayerManager.Player.HasSpaceInInventory(w.WhatAreYouHolding()))
                    {
                        ((NPC)c).Talk();
                        PlayerManager.Player.AddItemToFirstAvailableInventory(w.TakeItem());
                    }
                }
                else if (c.CollisionBox.Contains(mouseLocation) && (cType.Equals(typeof(NPC)) || cType.IsSubclassOf(typeof(NPC))))
                {
                    ((NPC)c).Talk();
                }
            }
            foreach (Building b in _buildingList)
            {
                if (b.BoxToEnter.Contains(mouseLocation) && PlayerInRange(PlayerManager.Player.GetRectangle(), b.BoxToEnter.Center))
                {
                    MapManager.EnterBuilding(b._map, b.ID.ToString(), b.Workers);
                    break;
                }
            }
            foreach (StaticItem s in _staticItemList)
            {
                if (s.CollisionBox.Contains(mouseLocation))
                {
                    GUIManager.LoadScreen(GUIManager.Screens.Inventory, (Container)s);
                }
            }

            return rv;
        }

        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (AdventureGame.State == AdventureGame.GameState.Build)
            {
                if (GraphicCursor.HeldBuilding != null)
                {
                    AddBuilding();
                    rv = true;
                }
                else if (GraphicCursor.WorkerToPlace != ObjectManager.WorkerID.Nothing)
                {
                    if (AddWorkerToBuilding())
                    {
                        rv = true;
                    }
                }
            }
            else
            {
                if (PlayerManager._merchantChest.Contains(mouseLocation))
                {
                    Item i = PlayerManager.Player.CurrentItem;
                    PlayerManager._merchantChest.AddItem(i);
                    PlayerManager.Player.RemoveItemFromInventory(PlayerManager.Player.CurrentItemNumber);
                }
                foreach (Character c in _characterList)
                {
                    Type cType = c.GetType();
                    if (cType.IsSubclassOf(typeof(Worker)))
                    {
                        Worker w = (Worker)c;
                        if (w.MouseInside(mouseLocation) && PlayerInRange(PlayerManager.Player.GetRectangle(), w.Center) &&
                            PlayerManager.Player.HasSpaceInInventory(w.WhatAreYouHolding()))
                        {
                            PlayerManager.Player.AddItemToFirstAvailableInventory(w.TakeItem());
                            w.MakeDailyItem();
                        }
                    }
                }
            }

            return rv;
        }
        public bool ProcessHover(Point mouseLocation)
        {
            bool rv = false;

            if (AdventureGame.State == AdventureGame.GameState.Build)
            {
                foreach(Building b in _buildingList)
                {
                    if (b.SelectionBox.Contains(mouseLocation))
                    {
                        b._selected = true;
                    }
                    else
                    {
                        b._selected = false;
                    }
                }
            }
            else{
                bool found = false;
                foreach(Character c in _characterList)
                {
                    if(!c.GetType().IsSubclassOf(typeof(Monster)) && c.CollisionBox.Contains(mouseLocation)){
                        GraphicCursor.talk = true;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    GraphicCursor.talk = false;
                }
            }

            return rv;
        }
        #endregion

        public bool PlayerInRange(Rectangle playerRect, Point centre)
        {
            return PlayerInRange(playerRect, centre, TileMap.TileSize * 2);
        }
        public bool PlayerInRange(Rectangle playerRect, Point centre, int range)
        {
            bool rv = false;

            if (Math.Abs(playerRect.Center.X - centre.X) <= range &&
                Math.Abs(playerRect.Center.Y - centre.Y) <= range)
            {
                rv = true;
            }

            return rv;
        }

        public void ClearWorkers()
        {
            _characterList.Clear();
        }

        public WorldObject FindWorldObject(Point mouseLocation)
        {
            WorldObject rv = null;
            foreach(WorldObject o in _worldObjectList)
            {
                if (o.Contains(mouseLocation))
                {
                    rv = o;
                    break;
                }
            }

            return rv;
        }
        public void RemoveWorldObject(WorldObject o)
        {
            _worldObjectList.Remove(o);
        }
        public void RemoveCharacter(Character c)
        {
            _characterList.Remove(c);
        }
        public void DropWorldItems(List<Item>items, Vector2 position)
        {
            Random r = new Random();
            foreach(Item i in items)
            {
                ((Item)i).Pop(position);
                _itemList.Add(i);
            }
        }
        public void PlaceWorldItem(StaticItem container, Vector2 position)
        {
            position.X = ((int)((position.X) / 32)) * 32;
            position.Y = ((int)((position.Y) / 32)) * 32;
            container.OnTheMap = true;
            container.Position = position;
            _staticItemList.Add(container);
        }

        #region Adders
        public void AddWorkersToMap(List<Worker> workers)
        {
            _characterList.AddRange(workers);
        }

        public void AddBuilding()
        {
            Building b = GraphicCursor.HeldBuilding;
            AddBuilding(b);
        }

        public void AddBuilding(Building b)
        {
            Vector3 translate = Camera._transform.Translation;
            Vector2 newPos = new Vector2(b.Position.X - translate.X, b.Position.Y - translate.Y);
            b.SetCoordinates(newPos);
            _entranceDictionary.Add(b.ID.ToString(), b.BoxToExit); //TODO: FIX THIS
            GraphicCursor.DropBuilding();
            _buildingList.Add(b);
            PlayerManager.AddBuilding(b);
            AdventureGame.ChangeGameState(AdventureGame.GameState.Running);
            AdventureGame.ResetCamera();
        }

        public bool AddWorkerToBuilding()
        {
            bool rv = false;
            foreach(Building b in _buildingList)
            {
                if (b.SelectionBox.Contains(GraphicCursor.Position))
                {
                    if (b.HasSpace())
                    {
                        Random r = new Random();
                        Worker w = ObjectManager.GetWorker(GraphicCursor.WorkerToPlace);
                        b.AddWorker(w, r);
                        b._selected = false;
                        GUIManager.LoadScreen(GUIManager.Screens.TextInput, w);
                        rv = true;
                    }
                }
            }
            return rv;
        }

        public void AddWorldObject(WorldObject o)
        {
            _worldObjectList.Add(o);
        }
        public void AddCharacter(Character c)
        {
            _characterList.Add(c);
        }

        public void LoadMapData(List<WorldObject> wList)
        {
            foreach(WorldObject w in wList)
            {
                _worldObjectList.Add(w);
            }
        }

        #endregion

        
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
