using Adventure.Characters;
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

namespace Adventure.Tile_Engine
{
    public class TileMap
    {
        private static float Scale = AdventureGame.Scale;
        public int MapWidth = 100;
        public int MapHeight = 100;
        public static int _tileSize = 32;
        public static int TileSize { get => _tileSize; }
        private string _name;
        public string Name { get => _name.Replace(@"Maps\", ""); set => _name = value; } //Fuck off with that path bullshit

        protected Building _mapBuilding;
        public Building MapBuilding { get => _mapBuilding; }

        public bool _isBuilding;
        public bool IsBuilding { get => _isBuilding; }
        public bool _isDungeon;
        public bool IsDungeon { get => _isDungeon; }
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
        public Dictionary<Rectangle, string> ExitDictionary { get => _exitDictionary; }
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
           _map = Content.Load<TiledMap>(newMap);
            MapWidth = _map.Width;
            MapHeight = _map.Height;
            _isBuilding = _map.Properties.ContainsKey("Building");
            _isDungeon = _map.Properties.ContainsKey("Dungeon");
            _tileSize = _map.TileWidth;
            renderer = new TiledMapRenderer(GraphicsDevice);

            _name = _map.Name;

            LoadMapObjects();
        }

        public void LoadMapObjects()
        {
            ReadOnlyCollection<TiledMapObjectLayer> entrLayer = _map.ObjectLayers;
            foreach (TiledMapObjectLayer ol in entrLayer)
            {
                if (ol.Name == "Entrance Layer")
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
                        PlayerManager.Player.AddItemToFirstAvailableInventorySpot(i.ItemID);
                    }
                    else if (PlayerInRange(i.CollisionBox.Center, 80))
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
                if (l.IsVisible)
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
                if (l.IsVisible)
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
                            if (MapChange(movingObject))
                            {
                                return false;
                            }
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
                if (l.IsVisible)
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
                if (l.IsVisible)
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
                            if (MapChange(movingObject))
                            {
                                return false;
                            }
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
            foreach(KeyValuePair<Rectangle, string>  kvp in _exitDictionary)
            {
                if (kvp.Key.Intersects(movingObject))
                {
                    if(IsDungeon)
                    {
                        MapManager.ChangeDungeonRoom(kvp.Value);
                    }
                    else {
                        MapManager.ChangeMaps(_exitDictionary[kvp.Key]);
                    }
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
            foreach (TiledMapTileset ts in _map.Tilesets)
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
                //if (cType.IsSubclassOf(typeof(Worker)))
                //{
                //    Worker w = (Worker)c;
                //    if (w.Contains(mouseLocation) && PlayerInRange(w.Center))
                //    {
                //        ((NPC)c).Talk();
                //        break;
                //    }
                //}
                //else
                if (c.Contains(mouseLocation) && (cType.Equals(typeof(NPC)) || cType.IsSubclassOf(typeof(NPC))))
                {
                    ((NPC)c).Talk();
                    break;
                }
            }
            foreach (Building b in _buildingList)
            {
                if (b.BoxToEnter.Contains(mouseLocation) && PlayerInRange(b.BoxToEnter.Center))
                {
                    MapManager.EnterBuilding(b);
                    break;
                }
            }
            foreach (StaticItem s in _staticItemList)
            {
                if (s.CollisionBox.Contains(mouseLocation))
                {
                    GUIManager.LoadScreen(GUIManager.Screens.Inventory, (Container)s);
                    break;
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
                    AddBuilding(mouseLocation);
                    rv = true;
                }
                else if (GraphicCursor.WorkerToPlace != ObjectManager.WorkerID.Nothing)
                {
                    if (AddWorkerToBuilding(mouseLocation))
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
                        if (w.Contains(mouseLocation) && PlayerInRange(w.Center) &&
                            PlayerManager.Player.HasSpaceInInventory(w.WhatAreYouHolding()))
                        {
                            PlayerManager.Player.AddItemToFirstAvailableInventorySpot(w.TakeItem());
                            rv = true;
                        }
                    }
                    else if (cType.Equals(typeof(NPC)))
                    {
                        NPC n = (NPC)c;
                        if (PlayerManager.Player.CurrentItem != null && 
                            n.Contains(mouseLocation) && PlayerInRange(n.Center) &&
                            PlayerManager.Player.CurrentItem.Type != Item.ItemType.Tool &&
                            PlayerManager.Player.CurrentItem.Type != Item.ItemType.Weapon)
                        {
                            string text = string.Empty;
                            Item i = PlayerManager.Player.CurrentItem;
                            i.Remove(1);
                            if (i.Type == Item.ItemType.Map && n.Type == NPC.NPCType.Ranger)
                            {
                                text = n.GetDialogEntry("Adventure");
                                DungeonManager.LoadNewDungeon((AdventureMap)i);
                            }
                            else
                            {
                                text = n.GetDialogEntry("Gift");
                                n.Friendship += 10;
                            }
                            
                            if (!string.IsNullOrEmpty(text))
                            {
                                GUIManager.LoadScreen(GUIManager.Screens.Text, n, text);
                            }
                            rv = true;
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
                    if (PlayerManager.Player.CurrentItem != null && 
                        !c.GetType().IsSubclassOf(typeof(Monster)) && c.CollisionBox.Contains(mouseLocation) &&
                        PlayerManager.Player.CurrentItem.Type != Item.ItemType.Tool &&
                        PlayerManager.Player.CurrentItem.Type != Item.ItemType.Weapon)
                    {
                        GraphicCursor._currentType = GraphicCursor.CursorType.Gift;
                        found = true;
                        break;
                    }
                    else if(!c.GetType().IsSubclassOf(typeof(Monster)) && c.CollisionBox.Contains(mouseLocation)){
                        GraphicCursor._currentType = GraphicCursor.CursorType.Talk;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    GraphicCursor._currentType = GraphicCursor.CursorType.Normal;
                }
            }

            return rv;
        }
        #endregion

        public bool PlayerInRange(Point centre)
        {
            return PlayerInRange(centre, TileMap.TileSize * 2);
        }
        public bool PlayerInRange(Point centre, int range)
        {
            bool rv = false;

            Rectangle playerRect = PlayerManager.Player.GetRectangle();
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
        public void LoadStaticItem(StaticItem container)
        {
            container.OnTheMap = true;
            _staticItemList.Add(container);
        }
        public void PlaceStaticItem(StaticItem container, Vector2 position)
        {
            position.X = ((int)((position.X) / 32)) * 32;
            position.Y = ((int)((position.Y) / 32)) * 32;
            container.OnTheMap = true;
            container.Position = position;
            _staticItemList.Add(container);
            if(_mapBuilding != null)
            {
                _mapBuilding.StaticItems.Add(container);
            }
        }

        public void LoadBuilding(Building b)
        {
            _mapBuilding = b;
            ClearWorkers();
            AddBuildingObjectsToMap(b);
        }

        public void LayerVisible(string name, bool val) {
            foreach (TiledMapLayer layer in _map.Layers) {
                if (layer.Name == name)
                {
                    layer.IsVisible = val;
                    break;
                }
            }
        }

        #region Adders
        public void AddBuildingObjectsToMap(Building b)
        {
            List<Vector2> spawnPoints = new List<Vector2>();
            ReadOnlyCollection<TiledMapObjectLayer> entrLayer = _map.ObjectLayers;
            foreach (TiledMapObjectLayer ol in entrLayer)
            {
                if (ol.Name == "Building Layer")
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        if (mapObject.Name.Contains("Spawn"))
                        {
                            spawnPoints.Add(mapObject.Position);
                        }
                        else if (mapObject.Name.Contains("BuildingChest"))
                        {
                            b.BuildingChest.Position = mapObject.Position;
                            LoadStaticItem(b.BuildingChest);
                        }
                        else if (mapObject.Name.Contains("Pantry"))
                        {
                            b.Pantry.Position = mapObject.Position;
                            LoadStaticItem(b.Pantry);
                        }
                    }
                }
            }
            for (int i = 0; i < b.Workers.Count; i++)
            {
                b.Workers[i].Position = spawnPoints[i];
                _characterList.Add(b.Workers[i]);
            }
            foreach (StaticItem s in b.StaticItems)
            {
                LoadStaticItem(s);
            }
        }

        public void AddBuilding(Point mouseLocation)
        {
            Building b = GraphicCursor.HeldBuilding;
            AddBuilding(b);
        }

        public void AddBuilding(Building b)
        {
            Vector3 translate = Camera._transform.Translation;
            Vector2 newPos = new Vector2((b.Position.X - translate.X)/Scale, (b.Position.Y - translate.Y)/Scale);
            _entranceDictionary.Add(b.ID.ToString(), b.BoxToExit); //TODO: FIX THIS
            GraphicCursor.DropBuilding();
            _buildingList.Add(b);
            PlayerManager.AddBuilding(b);
            AdventureGame.ChangeGameState(AdventureGame.GameState.Running);
            AdventureGame.ResetCamera();
        }

        public bool AddWorkerToBuilding(Point mouseLocation)
        {
            bool rv = false;
            foreach(Building b in _buildingList)
            {
                if (b.SelectionBox.Contains(mouseLocation))
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
