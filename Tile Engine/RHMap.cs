﻿using RiverHollow.Characters;
using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using RiverHollow.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RiverHollow.Misc;

namespace RiverHollow.Tile_Engine
{
    public class RHMap
    {
        private static float Scale = RiverHollow.Scale;
        public int MapWidth = 100;
        public int MapHeight = 100;
        public static int _tileSize = 32;
        public static int TileSize { get => _tileSize; }
        private string _name;
        public string Name { get => _name.Replace(@"Maps\", ""); set => _name = value; } //Fuck off with that path bullshit

        protected WorkerBuilding _mapBuilding;
        public WorkerBuilding MapBuilding { get => _mapBuilding; }

        public bool _isBuilding;
        public bool IsBuilding { get => _isBuilding; }
        public bool _isDungeon;
        public bool IsDungeon { get => _isDungeon; }

        protected TiledMap _map;
        public TiledMap Map { get => _map; }

        protected RHTile[,] _tileArray;
        protected TiledMapRenderer renderer;
        protected List<TiledMapTileset> _tileSets;
        protected Dictionary<string, TiledMapTileLayer> _dictionaryLayers;
        public Dictionary<string, TiledMapTileLayer> Layers { get => _dictionaryLayers; }

        protected List<RHTile> _buildingTiles;
        protected List<WorldCharacter> _characterList;
        protected List<Mob> _mobList;
        public List<WorldCharacter> ToRemove;
        public List<WorldCharacter> ToAdd;
        protected List<WorkerBuilding> _buildingList;
        protected List<WorldObject> _worldObjectList;
        public List<WorldObject> WorldObjects { get => _worldObjectList; }
        protected List<Item> _itemList;
        protected List<StaticItem> _staticItemList;
        public List<StaticItem> StaticItems { get => _staticItemList; }

        private Dictionary<Rectangle, string> _dictExit;
        public Dictionary<Rectangle, string> DictionaryExit { get => _dictExit; }
        private Dictionary<string, Rectangle> _dictEntrance;
        public Dictionary<string, Rectangle> DictionaryEntrance { get => _dictEntrance; }
        private Dictionary<string, Vector2> _dictPathing;
        public Dictionary<string, Vector2> DictionaryPathing { get => _dictPathing; }

        public RHMap()
        {
            _buildingTiles = new List<RHTile>();
            _tileSets = new List<TiledMapTileset>();
            _characterList = new List<WorldCharacter>();
            _mobList = new List<Mob>();
            ToRemove = new List<WorldCharacter>();
            ToAdd = new List<WorldCharacter>();
            _buildingList = new List<WorkerBuilding>();
            _worldObjectList = new List<WorldObject>();
            _itemList = new List<Item>();
            _staticItemList = new List<StaticItem>();
            _dictExit = new Dictionary<Rectangle, string>();
            _dictEntrance = new Dictionary<string, Rectangle>();
            _dictPathing = new Dictionary<string, Vector2>();
        }

        public void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice, string newMap)
        {
           _map = Content.Load<TiledMap>(newMap);
            _name = _map.Name;
            _tileSize = _map.TileWidth;
            MapWidth = _map.Width;
            MapHeight = _map.Height;

            _dictionaryLayers = new Dictionary<string, TiledMapTileLayer>();
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                _dictionaryLayers.Add(l.Name, l);
            }

            _tileArray = new RHTile[MapWidth, MapHeight];
            for (int i = 0; i < MapHeight; i++) {
                for (int j = 0; j < MapWidth; j++)
                {
                    _tileArray[j, i] = new RHTile(j, i);
                    _tileArray[j, i].SetProperties(this);
                }
            }

            
            _isBuilding = _map.Properties.ContainsKey("Building");
            _isDungeon = _map.Properties.ContainsKey("Dungeon");
            renderer = new TiledMapRenderer(GraphicsDevice);

            LoadMapObjects();
        }

        public void LoadMapObjects()
        {
            ReadOnlyCollection<TiledMapObjectLayer> objectLayers = _map.ObjectLayers;
            foreach (TiledMapObjectLayer ol in objectLayers)
            {
                if (ol.Name == "Travel Layer")
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        Rectangle r = new Rectangle((int)mapObject.Position.X, (int)mapObject.Position.Y, (int)mapObject.Size.Width, (int)mapObject.Size.Height);
                        if (mapObject.Properties.ContainsKey("Exit"))
                        {
                            _dictExit.Add(r, mapObject.Properties["Exit"]);
                        }
                        else if (mapObject.Properties.ContainsKey("Entrance"))
                        {
                            _dictEntrance.Add(mapObject.Properties["Entrance"], r);
                        }
                        else if (mapObject.Properties.ContainsKey("Valid Area"))
                        {
                            //_validArea = mapObject.Properties["Valid Area"];
                        }
                    }
                }
                if (ol.Name == "Character Layer")
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        _dictPathing.Add(mapObject.Name, mapObject.Position);
                    }
                }
            }
        }

        public void Update(GameTime theGameTime)
        {
            if (this == MapManager.CurrentMap)
            {
                renderer.Update(_map, theGameTime);
                foreach (Mob m in _mobList)
                {
                    m.Update(theGameTime);
                }
                
                foreach (Item i in _itemList)
                {
                    ((Item)i).Update();
                }
            }

            foreach (WorldCharacter c in ToRemove)
            {
                if (c.GetType().Equals(typeof(Mob)) && _mobList.Contains((Mob)c)) { _mobList.Remove((Mob)c); }
                else if (_characterList.Contains(c)) { _characterList.Remove(c); }
            }
            ToRemove.Clear();

            if (ToAdd.Count > 0)
            {
                List<WorldCharacter> moved = new List<WorldCharacter>();
                foreach (WorldCharacter c in ToAdd)
                {
                    if (!MapManager.Maps[c.CurrentMapName].Contains(c))
                    {
                        if (c.GetType().Equals(typeof(Mob)) && !_mobList.Contains((Mob)c)) { _mobList.Add((Mob)c); }
                        else if (!_characterList.Contains(c)) { _characterList.Add(c); }
                        c.CurrentMapName = _name.Replace(@"Maps\", "");
                        c.Position = c.NewMapPosition == Vector2.Zero ? c.Position : c.NewMapPosition;
                        c.NewMapPosition = Vector2.Zero;
                        moved.Add(c);
                    }
                }
                foreach (WorldCharacter c in moved)
                {
                    ToAdd.Remove(c);
                }
                moved.Clear();
            }

            foreach (WorldCharacter c in _characterList)
            {
                c.Update(theGameTime);
            }
            

            ItemPickUpdate();
        }

        public bool Contains(WorldCharacter c)
        {
            return _characterList.Contains(c);
        }

        public void ItemPickUpdate()
        {
            WorldCharacter player = PlayerManager.World;
            List<Item> removedList = new List<Item>();
            foreach (Item i in _itemList)
            {
                if (i.OnTheMap && i.Pickup)
                {
                    if (((Item)i).FinishedMoving() && i.CollisionBox.Intersects(player.CollisionBox))
                    {
                        removedList.Add(i);
                        InventoryManager.AddNewItemToInventory(i.ItemID);
                    }
                    else if (PlayerManager.PlayerInRange(i.CollisionBox.Center, 80))
                    {
                        float speed = 3;
                        Vector2 direction = new Vector2((player.Position.X < i.Position.X) ? -speed : speed, (player.Position.Y < i.Position.Y) ? -speed : speed);
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

        public void DrawBase(SpriteBatch spriteBatch)
        {
            SetLayerVisibility(false);

            renderer.Draw(_map, Camera._transform);
            foreach(WorldCharacter c in _characterList)
            {
                c.Draw(spriteBatch);
            }

            foreach (Mob m in _mobList)
            {
                m.Draw(spriteBatch);
            }

            foreach (WorkerBuilding b in _buildingList)
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

            foreach(RHTile t in _buildingTiles)
            {
                bool passable = t.Passable();
                spriteBatch.Draw(GameContentManager.GetTexture(@"Textures\Dialog"), new Rectangle((int)t.GetPos.X, (int)t.GetPos.Y, 32, 32), new Rectangle(288, 128, 32, 32) , passable ? Color.Green *0.5f : Color.Red * 0.5f, 0, new Vector2(0, 0), SpriteEffects.None, 99999);
            }
        }
        public void DrawUpper(SpriteBatch spriteBatch)
        {
            SetLayerVisibility(true);
            renderer.Draw(_map, Camera._transform);
            SetLayerVisibility(false);
        }

        public void SetLayerVisibility(bool revealUpper)
        {
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                if (l.Name != "North" && l.Name != "South" && l.Name != "East" && l.Name != "West") {
                    if (revealUpper) { l.IsVisible = (l.Name == "Upper Layer"); }
                    else { l.IsVisible = (l.Name != "Upper Layer"); }
                }
            }
        }

        #region Collision Code
        public bool CheckLeftMovement(WorldCharacter c, Rectangle movingChar)
        {
            bool rv = true;
            if (CheckForCollision(c, movingChar)) { return false; }

            int columnTile = movingChar.Left / _tileSize;
            for (int y = GetMinRow(movingChar); y <= GetMaxRow(movingChar); y++)
            {
                RHTile mapTile = _tileArray[columnTile, y];
                Rectangle cellRect = new Rectangle(columnTile * _tileSize, y * _tileSize, _tileSize, _tileSize);
                if (!mapTile.Passable() && cellRect.Intersects(movingChar))
                {
                    if (cellRect.Right >= movingChar.Left) { rv = false; }
                }

                if (MapChange(c, movingChar)) { return false; }
            }

            return rv;
        }

        public bool CheckRightMovement(WorldCharacter c, Rectangle movingChar)
        {
            bool rv = true;
            if (CheckForCollision(c, movingChar)) { return false; }

            int columnTile = movingChar.Right / _tileSize;
            for (int y = GetMinRow(movingChar); y <= GetMaxRow(movingChar); y++)
            {
                RHTile mapTile = _tileArray[columnTile, y];
                Rectangle cellRect = new Rectangle(columnTile * _tileSize, y * _tileSize, _tileSize, _tileSize);
                if (!mapTile.Passable() && cellRect.Intersects(movingChar))
                {
                    if (cellRect.Left <= movingChar.Right) { rv = false; }
                }

                if (MapChange(c, movingChar)) { return false; }
            }

            return rv;
        }

        public bool CheckUpMovement(WorldCharacter c, Rectangle movingChar)
        {
            bool rv = true;
            if (CheckForCollision(c, movingChar)) { return false; }

            int rowTile = movingChar.Top / _tileSize;
            for (int x = GetMinColumn(movingChar); x <= GetMaxColumn(movingChar); x++)
            {
                RHTile mapTile = _tileArray[x, rowTile];
                Rectangle cellRect = new Rectangle(x * _tileSize, rowTile * _tileSize, _tileSize, _tileSize);
                if (!mapTile.Passable() && cellRect.Intersects(movingChar))
                {
                    if (cellRect.Bottom >= movingChar.Top) { rv = false; }
                }

                if (MapChange(c, movingChar)) { return false; }
            }

            return rv;
        }

        public bool CheckDownMovement(WorldCharacter c, Rectangle movingChar)
        {
            bool rv = true;
            if (CheckForCollision(c, movingChar)) { return false; }

            int rowTile = movingChar.Bottom / _tileSize;
            for (int x = GetMinColumn(movingChar); x <= GetMaxColumn(movingChar); x++)
            {
                RHTile mapTile = _tileArray[x, rowTile];
                Rectangle cellRect = new Rectangle(x * _tileSize, rowTile * _tileSize, _tileSize, _tileSize);
                if (!mapTile.Passable() && cellRect.Intersects(movingChar))
                {
                    if (cellRect.Top <= movingChar.Bottom) { rv = false; }
                }

                if (MapChange(c, movingChar)) { return false; }
            }

            return rv;
        }

        public bool MapChange(WorldCharacter c, Rectangle movingChar)
        {
            foreach(KeyValuePair<Rectangle, string>  kvp in _dictExit)
            {
                if (kvp.Key.Intersects(movingChar))
                {
                    if (IsDungeon)
                    {
                        if (c == PlayerManager.World)
                        {
                            MapManager.ChangeDungeonRoom(kvp.Value);
                            return true;
                        }
                    }
                    else
                    {
                        MapManager.ChangeMaps(c, this.Name, _dictExit[kvp.Key]);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool IsLocationValid(Vector2 pos)
        {
            bool rv = false;
            //_map.Layers[_map.Layers.IndexOf("EntranceLayer")].
            return rv;
        }

        public bool CheckForCollision(WorldCharacter mover, Rectangle movingChar)
        {
            bool rv = false;
            foreach (WorldCharacter c in _characterList)
            {
                if (mover != c && c.CollisionBox.Intersects(movingChar))
                {
                    rv = true;
                    break;
                }
            }
            
            return rv;
        }

        public Dictionary<string, string> GetProperties(TiledMapTile tile)
        {
            Dictionary<string, string> propList = new Dictionary<string, string>();
            foreach (TiledMapTileset ts in _map.Tilesets)
            {
                foreach (TiledMapTilesetTile t in ts.Tiles)
                {
                    if (tile.GlobalIdentifier - ts.FirstGlobalIdentifier == t.LocalTileIdentifier)
                    {
                        foreach (KeyValuePair<string, string> tp in t.Properties)
                        {
                            if (!propList.ContainsKey(tp.Key)){
                                propList.Add(tp.Key, tp.Value);
                            }
                        }
                    }
                }
            }
            return propList;
        }

        public Vector2 GetCharacterSpawn(string val)
        {
            Vector2 rv = new Vector2(0, 0);
            if (_dictPathing.ContainsKey(val))
            {
                rv = _dictPathing[val];
            }
            return rv;
        }

        #region Collision Helpers
        public int GetMinColumn(Rectangle movingChar)
        {
            return (movingChar.Left / _tileSize);
        }

        public int GetMaxColumn(Rectangle movingChar)
        {
            int i = (movingChar.Right / _tileSize);
            return i;
        }

        public int GetMinRow(Rectangle movingChar)
        {
            return (movingChar.Top / _tileSize);
        }

        public int GetMaxRow(Rectangle movingChar)
        {
            return (movingChar.Bottom / _tileSize);
        }
        #endregion
        #endregion

        #region Input Processing
        public bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            foreach (WorldCharacter c in _characterList)
            {
                Type cType = c.GetType();
                if (c.Contains(mouseLocation) && (cType.Equals(typeof(NPC)) || cType.IsSubclassOf(typeof(NPC))))
                {
                    ((NPC)c).Talk();
                    break;
                }
            }
            foreach (WorkerBuilding b in _buildingList)
            {
                if (b.BoxToEnter.Contains(mouseLocation) && PlayerManager.PlayerInRange(b.BoxToEnter.Center))
                {
                    MapManager.EnterBuilding(b);
                    break;
                }
            }
            foreach (StaticItem s in _staticItemList)
            {
                if (s.CollisionBox.Contains(mouseLocation))
                {
                    if (IsDungeon && DungeonManager.IsEndChest((Container)s))
                    {
                        Staircase stairs = (Staircase)ObjectManager.GetWorldObject(3, new Vector2(0, 0));
                        stairs.SetExit("NearWilds");
                        AddWorldObject(stairs, true);
                    }
                    GUIManager.LoadContainerScreen((Container)s);
                    break;
                }
            }

            RHTile tile = _tileArray[mouseLocation.X / 32, mouseLocation.Y / 32];
            if(tile != null)
            {
                if (tile.ContainsProperty("Journal", out string val) && val.Equals("true"))
                {
                    GUIManager.LoadTextScreen(GameContentManager.GetDialogue("Journal"));
                }
                if (tile.Object != null && tile.Object.ID == 3) //Checks to see if the tile contains a staircase object
                {
                    MapManager.ChangeMaps(PlayerManager.World, this.Name, ((Staircase)tile.Object).ToMap);
                }

            }

            return rv;
        }

        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (RiverHollow.State == RiverHollow.GameState.Build)
            {
                if (GraphicCursor.HeldBuilding != null)
                {
                    AddBuilding(mouseLocation);
                    rv = true;
                }
                else if (GraphicCursor.WorkerToPlace > -1)
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
                    Item i = InventoryManager.CurrentItem;
                    PlayerManager._merchantChest.AddItem(i);
                    InventoryManager.RemoveItemFromInventory(InventoryManager.CurrentItem);
                }
                foreach (WorldCharacter c in _characterList)
                {
                    Type cType = c.GetType();
                    if (cType.IsSubclassOf(typeof(WorldAdventurer)))
                    {
                        WorldAdventurer w = (WorldAdventurer)c;
                        if (w.Contains(mouseLocation) && PlayerManager.PlayerInRange(w.Center) &&
                            InventoryManager.HasSpaceInInventory(w.WhatAreYouHolding()))
                        {
                            InventoryManager.AddNewItemToInventory(w.TakeItem());
                            rv = true;
                        }
                    }
                    else if (cType.Equals(typeof(NPC)))
                    {
                        NPC n = (NPC)c;
                        if (InventoryManager.CurrentItem != null && 
                            n.Contains(mouseLocation) && PlayerManager.PlayerInRange(n.Center) &&
                            InventoryManager.CurrentItem.Type != Item.ItemType.Tool &&
                            InventoryManager.CurrentItem.Type != Item.ItemType.Equipment)
                        {
                            n.Gift(InventoryManager.CurrentItem);
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

            if(GraphicCursor.HeldBuilding == null)
            {
                if (_buildingTiles.Count > 0) { _buildingTiles.Clear(); }
            }
            if (RiverHollow.State == RiverHollow.GameState.Build)
            {
                WorkerBuilding building = GraphicCursor.HeldBuilding;
                _buildingTiles = new List<RHTile>();
                if (building != null)
                {
                    TestMapTiles(building, _buildingTiles);
                }

                foreach (WorkerBuilding b in _buildingList)
                {
                    if (b.SelectionBox.Contains(mouseLocation) && GraphicCursor.HeldBuilding == null)
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
                foreach(WorldCharacter c in _characterList)
                {
                    if (InventoryManager.CurrentItem != null && 
                        !c.GetType().IsSubclassOf(typeof(Mob)) && c.CollisionBox.Contains(mouseLocation) &&
                        InventoryManager.CurrentItem.Type != Item.ItemType.Tool &&
                        InventoryManager.CurrentItem.Type != Item.ItemType.Equipment)
                    {
                        GraphicCursor._currentType = GraphicCursor.CursorType.Gift;
                        found = true;
                        break;
                    }
                    else if(!c.GetType().IsSubclassOf(typeof(Mob)) && c.Contains(mouseLocation)){
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

        public void ClearWorkers()
        {
            _characterList.Clear();
        }

        public RHTile RetrieveTile(Point mouseLocation)
        {
            return _tileArray[mouseLocation.X / TileSize, mouseLocation.Y / TileSize];
        }
        public void RemoveWorldObject(WorldObject o)
        {
            _worldObjectList.Remove(o);
        }
        public void RemoveCharacter(WorldCharacter c)
        {
            ToRemove.Add(c);
        }
        public void RemoveMob(Mob m)
        {
            _mobList.Remove(m);
        }
        public void DropWorldItems(List<Item>items, Vector2 position)
        {
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

        public void LoadBuilding(WorkerBuilding b)
        {
            _mapBuilding = b;
            ClearWorkers();
            AddBuildingObjectsToMap(b);
        }

        public void LayerVisible(string name, bool val) {
            foreach (TiledMapTileLayer layer in _map.TileLayers) {
                if (layer.Name == name)
                {
                    layer.IsVisible = val;
                    break;
                }
            }
        }

        #region Adders
        public void AddBuildingObjectsToMap(WorkerBuilding b)
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
            WorkerBuilding b = GraphicCursor.HeldBuilding;
            AddBuilding(b);
        }

        public void AddBuilding(WorkerBuilding b)
        {
            List<RHTile> tiles = new List<RHTile>();
            if (TestMapTiles(b, tiles))
            {
                AssignMapTiles(b, tiles);
                Vector3 translate = Camera._transform.Translation;
                Vector2 newPos = new Vector2((b.Position.X - translate.X) / Scale, (b.Position.Y - translate.Y) / Scale);
                _dictEntrance.Add(b.PersonalID.ToString(), b.BoxToExit); //TODO: FIX THIS
                GraphicCursor.DropBuilding();
                _buildingList.Add(b);
                PlayerManager.AddBuilding(b);
                RiverHollow.ChangeGameState(RiverHollow.GameState.Running);
                RiverHollow.ResetCamera();
            }
        }

        public bool AddWorkerToBuilding(Point mouseLocation)
        {
            bool rv = false;
            foreach(WorkerBuilding b in _buildingList)
            {
                if (b.SelectionBox.Contains(mouseLocation))
                {
                    if (b.HasSpace())
                    {
                        RHRandom r = new RHRandom();
                        WorldAdventurer w = ObjectManager.GetWorker(GraphicCursor.WorkerToPlace);
                        b.AddWorker(w, r);
                        b._selected = false;
                        GUIManager.LoadScreen(GUIManager.Screens.TextInput, w);
                        rv = true;
                    }
                }
            }
            return rv;
        }

        public bool AddWorldObject(WorldObject o, bool bounce)
        {
            bool rv = false;

            List<RHTile> tiles = new List<RHTile>();
            rv = TestMapTiles(o, tiles);
            if (rv)
            {
                AssignMapTiles(o, tiles);
            }
            else if (bounce)
            {
                RHRandom r = new RHRandom();
                Vector2 position = o.Position;
                do
                {
                    position.X = (int)(r.Next(1, (MapWidth - 1) * TileSize) / 32) * 32;
                    position.Y = (int)(r.Next(1, (MapHeight - 1) * TileSize) / 32) * 32;
                    o.SetCoordinates(position);

                    rv = TestMapTiles(o, tiles);
                    if (rv)
                    {
                        AssignMapTiles(o, tiles);
                    }
                } while (!rv);
            }  

            if (rv)
            {
                _worldObjectList.Add(o);
            }

            return rv;
        }

        public bool TestMapTiles(WorldObject o, List<RHTile> tiles)
        {
            bool rv = false;
            Vector2 position = o.Position;
            position.X = ((int)(position.X / 32)) * 32;
            position.Y = ((int)(position.Y / 32)) * 32;

            int colColumns = o.CollisionBox.Width / 32;
            int colRows = o.CollisionBox.Height / 32;

            rv = true;
            //BUG: Went out of bounds?
            for (int i = 0; i < colRows; i++)
            {
                for (int j = 0; j < colColumns; j++)
                {
                    int x = Math.Min((o.CollisionBox.Left + (j * 32)) / 32, MapWidth-1);
                    int y = Math.Min((o.CollisionBox.Top + (i * 32)) / 32, MapHeight-1);
                    if (x < 0 || x > 99 || y < 0 || y > 99)
                    {
                        rv = false;
                        break;
                    }
                    RHTile tempTile = _tileArray[x, y];

                    if ((!o.WallObject && tempTile.Passable()) || (o.WallObject && tempTile.IsValidWall()))
                    {
                        tiles.Add(tempTile);
                    }
                    else
                    {
                        tiles.Add(tempTile);
                        rv = false;
                    }
                }
            }

            return rv;
        }

        public void AssignMapTiles(WorldObject o, List<RHTile> tiles)
        {
            o.Tiles = tiles;
            foreach (RHTile t in tiles)
            {
                t.SetWorldObject(o);
            }
        }
        public void PlaceStaticItem(StaticItem container, Vector2 position, bool bounce = true)
        {
            bool rv = false;
            RHRandom r = new RHRandom();
            position = Utilities.Normalize(position);

            rv = _tileArray[((int)position.X / 32), ((int)position.Y / 32)].SetStaticItem(container);
            if (!rv && bounce)
            {
                do
                {
                    position.X = (int)(r.Next(1, (MapWidth - 1) * TileSize) / 32) * 32;
                    position.Y = (int)(r.Next(1, (MapHeight - 1) * TileSize) / 32) * 32;
                    rv = _tileArray[((int)position.X / 32), ((int)position.Y / 32)].SetStaticItem(container);
                } while (!rv);

            }

            if (rv)
            {
                container.OnTheMap = true;
                container.Position = position;

                if (_mapBuilding != null) { _mapBuilding.StaticItems.Add(container); }
                else { _staticItemList.Add(container); }
            }
        }

        public void AddCharacter(WorldCharacter c)
        {
            ToAdd.Add(c);
        }

        public void AddMob(Mob m)
        {
            bool rv = false;
            RHRandom r = new RHRandom();
            Vector2 position = m.Position;
            position.X = ((int)(position.X / 32)) * 32;
            position.Y = ((int)(position.Y / 32)) * 32;

            rv = _tileArray[((int)position.X / 32), ((int)position.Y / 32)].Passable();
            if (!rv)
            {
                do
                {
                    position.X = (int)(r.Next(1, (MapWidth - 1) * TileSize) / 32) * 32;
                    position.Y = (int)(r.Next(1, (MapHeight - 1) * TileSize) / 32) * 32;
                    rv = _tileArray[((int)position.X / 32), ((int)position.Y / 32)].Passable();
                } while (!rv);
            }

            if (rv)
            {
                m.Position = position;

                _mobList.Add(m);
            }
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

    public class RHTile
    {
        private bool _tileExists;
        private int _X;
        public int X { get => _X; }
        private int _Y;
        public int Y { get => _Y; }
        public Vector2 GetPos { get => new Vector2(_X * 32, _Y * 32); }
        private Dictionary<TiledMapTileLayer, Dictionary<string, string>> _properties;
        private WorldObject _obj;
        public WorldObject Object { get => _obj; }

        private StaticItem _staticItem;
        public StaticItem StaticItem { get => _staticItem; }

        public RHTile(int x, int y)
        {
            _X = x;
            _Y = y;

            _properties = new Dictionary<TiledMapTileLayer, Dictionary<string, string>>();
        }

        public void SetProperties(RHMap map)
        {
            foreach (TiledMapTileLayer l in map.Layers.Values)
            {
                
                if (l.IsVisible && l.TryGetTile(_X, _Y, out TiledMapTile? tile) && tile != null)
                {
                    if (tile.Value.GlobalIdentifier == 46)
                    {
                        int i = 0;
                    }

                    if (tile.Value.GlobalIdentifier != 0)
                    {
                        _tileExists = true;
                    }
                    if (((TiledMapTile)tile).GlobalIdentifier != 0)
                    {
                        _properties.Add(l, map.GetProperties((TiledMapTile)tile));
                    }
                }
            }
        }

        public bool SetWallWorldObject(WorldObject o)
        {
            bool rv = false;
            if (IsValidWall())
            {
                _obj = o;
                rv = true;
            }
            return rv;
        }
        public bool SetWorldObject(WorldObject o)
        {
            bool rv = false;
            if ((!o.WallObject && Passable()) || (o.WallObject && IsValidWall()))
            {
                _obj = o;
                rv = true;
            }
            return rv;
        }
        public bool SetStaticItem(StaticItem i)
        {
            bool rv = false;
            if (Passable())
            {
                _staticItem = i;
                rv = true;
            }
            return rv;
        }

        public bool Passable()
        {
            bool rv = _tileExists && _obj == null && _staticItem == null;
            if (_tileExists)
            {
                foreach (TiledMapTileLayer l in _properties.Keys)
                {
                    if (l.IsVisible && ContainsProperty(l, "Impassable", out string val) && val.Equals("true"))
                    {
                        rv = false;
                    }
                }
            }

            return rv;
        }

        public bool IsValidWall()
        {
            bool rv = false;
            if (_tileExists && _obj == null && _staticItem == null)
            {
                foreach (TiledMapTileLayer l in _properties.Keys)
                {
                    if (l.IsVisible && ContainsProperty(l, "Impassable", out string val) && val.Equals("true"))
                    {
                        rv = true;
                    }
                }
            }
            return rv;
        }

        public bool ContainsProperty(string property, out string value)
        {
            bool rv = false;
            value = string.Empty;
            foreach (TiledMapTileLayer l in _properties.Keys)
            {
                rv = ContainsProperty(l, property, out value);
            }

            return rv;
        }
        public bool ContainsProperty(TiledMapTileLayer l, string property, out string value)
        {
            bool rv = false;
            value = string.Empty;
            if (_properties.ContainsKey(l) && _properties[l].ContainsKey(property))
            {
                value = _properties[l][property];
                rv = true;
            }

            return rv;
        }

        public bool DamageObject(float dmg)
        {
            bool rv = false;
            rv = _obj.DealDamage(dmg);
            if (rv)
            {
                MapManager.DropWorldItems(DropManager.DropItemsFromWorldObject(_obj.ID), _obj.CollisionBox.Center.ToVector2());
                MapManager.RemoveWorldObject(_obj);
                _obj.ClearTiles();
            }

            return rv;
        }

        public void Clear()
        {
            _obj = null;
            _staticItem = null;
        }
    }
}
