using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using RiverHollow.Misc;
using RiverHollow.Characters;
using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Graphics;
using MonoGame.Extended.Tiled;
using static RiverHollow.RiverHollow;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using static RiverHollow.WorldObjects.WorldItem.Machine;
using static RiverHollow.WorldObjects.WorldItem;
using RiverHollow.Game_Managers.GUIObjects;

namespace RiverHollow.Tile_Engine
{
    public class RHMap
    {
        private static float Scale = GameManager.Scale;
        public int MapWidthTiles = 100;
        public int MapHeightTiles = 100;
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
        public bool _isTown;
        public bool IsTown { get => _isTown; }

        protected TiledMap _map;
        public TiledMap Map { get => _map; }

        protected RHTile[,] _tileArray;
        protected TiledMapRenderer _renderer;
        protected List<TiledMapTileset> _liTilesets;
        protected Dictionary<string, TiledMapTileLayer> _diLayers;
        public Dictionary<string, TiledMapTileLayer> Layers { get => _diLayers; }

        protected List<RHTile> _liBuildingTiles;
        protected List<WorldCharacter> _liCharacters;
        protected List<Mob> _liMobs;
        public List<WorldCharacter> ToRemove;
        public List<WorldCharacter> ToAdd;
        protected List<WorkerBuilding> _liBuildings;
        protected List<WorldObject> _liWorldObjects;
        public List<WorldObject> WorldObjects { get => _liWorldObjects; }
        protected List<WorldObject> _liPlacedObjects;
        public List<WorldObject> PlacedObjects { get => _liPlacedObjects; }
        protected List<Item> _liItems;

        private Dictionary<Rectangle, string> _dictExit;
        public Dictionary<Rectangle, string> DictionaryExit { get => _dictExit; }
        private Dictionary<string, Rectangle> _dictEntrance;
        public Dictionary<string, Rectangle> DictionaryEntrance { get => _dictEntrance; }
        private Dictionary<string, Vector2> _dictCharacterLayer;
        public Dictionary<string, Vector2> DictionaryCharacterLayer { get => _dictCharacterLayer; }

        public RHMap() {
            _liBuildingTiles = new List<RHTile>();
            _liTilesets = new List<TiledMapTileset>();
            _liCharacters = new List<WorldCharacter>();
            _liMobs = new List<Mob>();
            _liBuildings = new List<WorkerBuilding>();
            _liWorldObjects = new List<WorldObject>();
            _liItems = new List<Item>();
            _dictExit = new Dictionary<Rectangle, string>();
            _dictEntrance = new Dictionary<string, Rectangle>();
            _dictCharacterLayer = new Dictionary<string, Vector2>();
            _liPlacedObjects = new List<WorldObject>();

            ToRemove = new List<WorldCharacter>();
            ToAdd = new List<WorldCharacter>();
        }

        public RHMap(RHMap map) : this()
        {
            _map = map.Map;
            _name = map.Name;
            _renderer = map._renderer;
            _tileArray = map._tileArray;
            _tileSize = _map.TileWidth;

            _isBuilding = _map.Properties.ContainsKey("Building");
            _isDungeon = _map.Properties.ContainsKey("Dungeon");
            _isTown = _map.Properties.ContainsKey("Town");

            MapWidthTiles = _map.Width;
            MapHeightTiles = _map.Height;

            LoadMapObjects();
        }

        public void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice, string newMap)
        {
            _map = Content.Load<TiledMap>(newMap);
            _name = _map.Name.Replace(@"Maps\", "");
            _tileSize = _map.TileWidth;
            MapWidthTiles = _map.Width;
            MapHeightTiles = _map.Height;

            _diLayers = new Dictionary<string, TiledMapTileLayer>();
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                _diLayers.Add(l.Name, l);
            }

            _tileArray = new RHTile[MapWidthTiles, MapHeightTiles];
            for (int i = 0; i < MapHeightTiles; i++) {
                for (int j = 0; j < MapWidthTiles; j++)
                {
                    _tileArray[j, i] = new RHTile(j, i, _name);
                    _tileArray[j, i].SetProperties(this);
                }
            }

            
            _isBuilding = _map.Properties.ContainsKey("Building");
            _isDungeon = _map.Properties.ContainsKey("Dungeon");
            _isTown = _map.Properties.ContainsKey("Town");

            if (_isTown)
            {
                foreach (KeyValuePair<string, Upgrade> kvp in GameManager.DiUpgrades)
                {
                    if (kvp.Value.Enabled) { EnableUpgradeVisibility(kvp.Key); }
                }
            }
            _renderer = new TiledMapRenderer(GraphicsDevice);

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
                        _dictCharacterLayer.Add(mapObject.Name, mapObject.Position);
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            if (this == MapManager.CurrentMap)
            {
                _renderer.Update(_map, gameTime);
                if (GameManager.IsRunning())
                {
                    foreach (Mob m in _liMobs)
                    {
                        m.Update(gameTime);
                    }
                }
                
                foreach (Item i in _liItems)
                {
                    ((Item)i).Update();
                }
            }

            foreach (WorldObject s in _liPlacedObjects)
            {
                if (s.IsProcessor())
                {
                    Processor p = (Processor)s;
                    p.Update(gameTime);
                }
                else if (s.IsCrafter())
                {
                    Crafter c = (Crafter)s;
                    c.Update(gameTime);
                }
            }

            foreach (WorldCharacter c in ToRemove)
            {
                if (c.IsMob() && _liMobs.Contains((Mob)c)) { _liMobs.Remove((Mob)c); }
                else if (_liCharacters.Contains(c)) { _liCharacters.Remove(c); }
            }
            ToRemove.Clear();

            if (ToAdd.Count > 0)
            {
                List<WorldCharacter> moved = new List<WorldCharacter>();
                foreach (WorldCharacter c in ToAdd)
                {
                    if (!MapManager.Maps[c.CurrentMapName].Contains(c))
                    {
                        if (c.IsMob() && !_liMobs.Contains((Mob)c)) { _liMobs.Add((Mob)c); }
                        else if (!_liCharacters.Contains(c)) { _liCharacters.Add(c); }
                        c.CurrentMapName = _name;
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

            if (GameManager.IsRunning())
            {
                foreach (WorldCharacter c in _liCharacters)
                {
                    c.Update(gameTime);
                }
            }
            

            ItemPickUpdate();
        }

        public void RollOver()
        {
            foreach(WorldObject w in _liPlacedObjects)
            {
                if (w.IsPlant())
                {
                    ((Plant)w).Rollover();
                }
            }
        }

        public bool Contains(WorldCharacter c)
        {
            return _liCharacters.Contains(c);
        }

        public void ItemPickUpdate()
        {
            WorldCharacter player = PlayerManager.World;
            List<Item> removedList = new List<Item>();
            foreach (Item i in _liItems)
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
                _liItems.Remove(i);
            }
            removedList.Clear();
        }

        public void DrawBase(SpriteBatch spriteBatch)
        {
            SetLayerVisibility(false);

            _renderer.Draw(_map, Camera._transform);
            foreach(WorldCharacter c in _liCharacters)
            {
                c.Draw(spriteBatch, true);
            }

            foreach (Mob m in _liMobs)
            {
                m.Draw(spriteBatch);
            }

            foreach (WorkerBuilding b in _liBuildings)
            {
                b.Draw(spriteBatch);
            }

            foreach (WorldObject o in _liWorldObjects)
            {
                o.Draw(spriteBatch);
            }

            foreach (WorldObject o in _liPlacedObjects)
            {
                o.Draw(spriteBatch);
            }

            foreach (Item i in _liItems)
            {
                i.Draw(spriteBatch);
            }

            foreach(RHTile t in _liBuildingTiles)
            {
                bool passable = t.Passable();
                spriteBatch.Draw(GameContentManager.GetTexture(@"Textures\Dialog"), new Rectangle((int)t.Position.X, (int)t.Position.Y, 32, 32), new Rectangle(288, 128, 32, 32) , passable ? Color.Green *0.5f : Color.Red * 0.5f, 0, new Vector2(0, 0), SpriteEffects.None, 99999);
            }
        }

        public void DrawUpper(SpriteBatch spriteBatch)
        {
            SetLayerVisibility(true);
            _renderer.Draw(_map, Camera._transform);
            SetLayerVisibility(false);
        }

        public void SetLayerVisibility(bool revealUpper)
        {
            foreach (TiledMapTileLayer l in _map.TileLayers)                            //Iterate over each TileLayer in the map
            {
                if (l.Name.StartsWith("ent"))                                           //The layer is a dungeon entrancel layer. Don't touch.
                {
                    continue;
                }

                bool upgrade = false;
                if (_isTown)
                {
                    foreach (KeyValuePair<string, Upgrade> s in GameManager.DiUpgrades)    //Check each upgrade to see if it's enabled
                    {
                        if(l.Name.Contains(s.Key)) { upgrade = true; }
                        if (s.Value.Enabled)
                        {                           
                            bool determinant = l.Name.Contains("Upper");
                            if (revealUpper) { l.IsVisible = determinant; }
                            else { l.IsVisible = !determinant; }
                        }
                    }
                }

                if (!upgrade)
                {
                    bool determinant = (l.Name == "Upper Layer");

                    if (revealUpper) { l.IsVisible = determinant; }
                    else { l.IsVisible = !determinant; }
                }
            }
        }

        public void EnableUpgradeVisibility(string upgrade)
        {
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                if (l.Name.Contains(upgrade)) { l.IsVisible = true; }
            }
        }

        #region Collision Code
        public bool CheckForCollisions(WorldCharacter c, Rectangle testX, Rectangle testY, ref Vector2 dir)
        {
            bool rv = true;
            if (CheckForCollision(c, testX) || CheckForCollision(c, testY)) { return false; }
            if (MapChange(c, testX) || MapChange(c, testY)) { return false; }

            int column = ((dir.X < 0) ? testX.Left : testX.Right) / _tileSize;
            int row = ((dir.Y < 0) ? testY.Top : testY.Bottom) / _tileSize;

            //Do X-Axis comparison
            if (dir.X != 0) { CollisionDetectionHelper(testX, ref dir, column, -1, GetMinRow(testX), GetMaxRow(testX)); }
            //Do Y-Axis comparison
            if (dir.Y != 0) { CollisionDetectionHelper(testY, ref dir, -1, row, GetMinColumn(testY), GetMaxColumn(testY)); }    

            return rv;
        }

        public bool CheckForCollision(WorldCharacter mover, Rectangle movingChar)
        {
            bool rv = false;
            foreach (WorldCharacter c in _liCharacters)
            {
                if (mover != c && c.CollisionBox.Intersects(movingChar))
                {
                    rv = true;
                    break;
                }
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

        #region Collision Helpers
        private void CollisionDetectionHelper(Rectangle movingChar, ref Vector2 dir, int column, int row, int min, int max)
        {
            try
            {
                for (int var = min; var <= max; var++)
                {
                    int varCol = (column != -1) ? column : var;
                    int varRow = (row != -1) ? row : var;

                    RHTile mapTile = _tileArray[varCol, varRow];
                    Rectangle cellRect = new Rectangle(varCol * _tileSize, varRow * _tileSize, _tileSize, _tileSize);
                    if (!mapTile.Passable() && cellRect.Intersects(movingChar))
                    {
                        if (row == -1 && (cellRect.Right >= movingChar.Left || cellRect.Left <= movingChar.Right)) { dir.X = 0; }
                        if (column == -1 && (cellRect.Bottom >= movingChar.Top || cellRect.Top <= movingChar.Bottom)) { dir.Y = 0; }
                    }
                }
            }
            catch(IndexOutOfRangeException ex)
            {

            }
        }

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

        public bool IsLocationValid(Vector2 pos)
        {
            bool rv = false;
            //_map.Layers[_map.Layers.IndexOf("EntranceLayer")].
            return rv;
        }

        public void AddCollectionItem(int itemID, int npcIndex, int index)
        {
            Item displayItem = ObjectManager.GetItem(itemID);
            displayItem.Pickup = false;
            displayItem.OnTheMap = true;
            displayItem.Position = _dictCharacterLayer[npcIndex + "Col" + index];
            _liItems.Add(displayItem);
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
            if (_dictCharacterLayer.ContainsKey(val))
            {
                rv = _dictCharacterLayer[val];
            }
            return rv;
        }

        #region Input Processing
        public bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            foreach (WorldCharacter c in _liCharacters)
            {
                if (c.CollisionContains(mouseLocation) && c.CanTalk())
                {
                    ((NPC)c).Talk();
                    break;
                }
            }
            foreach (WorkerBuilding b in _liBuildings)
            {
                if (b.BoxToEnter.Contains(mouseLocation) && PlayerManager.PlayerInRange(b.BoxToEnter.Center))
                {
                    MapManager.EnterBuilding(b);
                    break;
                }
            }

            RHTile tile = _tileArray[mouseLocation.X / 32, mouseLocation.Y / 32];
            if (tile != null)
            {
                if (tile.WldObject != null)
                {
                    WorldObject s = tile.WldObject;
                    if (s.IsMachine())
                    {
                        Machine p = (Machine)s;
                        if (p.ProcessingFinished()) { p.TakeFinishedItem(); }
                        else if (GraphicCursor.HeldItem != null && !p.Processing()) { p.ProcessClick(); }
                    }
                    else if (s.IsContainer())
                    {
                        if (IsDungeon && DungeonManager.IsEndChest((Container)s))
                        {
                            Staircase stairs = (Staircase)ObjectManager.GetWorldObject(3, new Vector2(0, 0));
                            stairs.SetExit(MapManager.HomeMap);
                            PlaceWorldObject(stairs, true);
                        }
                        GUIManager.SetScreen(new InventoryScreen((Container)s));
                    }
                    else if (s.IsPlant())
                    {
                        Plant p = (Plant)s;
                        if (p.FinishedGrowing()) {
                            Item i = p.Harvest();
                            if(i != null)
                            {
                                _liItems.Add(i);
                            }
                            MapManager.RemoveWorldObject(p);
                            p.ClearTiles();
                        }
                    }
                }

                if (tile.ContainsProperty("Journal", out string val) && val.Equals("true"))
                {
                    GUIManager.SetScreen(new TextScreen(GameContentManager.GetDialogue("Journal")));
                }

                if (tile.WldObject != null && tile.WldObject.ID == 3) //Checks to see if the tile contains a staircase object
                {
                    MapManager.ChangeMaps(PlayerManager.World, this.Name, ((Staircase)tile.WldObject).ToMap);
                }

            }

            return rv;
        }

        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (GameManager.Scrying())
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
                foreach (WorldCharacter c in _liCharacters)
                {
                    if (c.IsWorldAdventurer())
                    {
                        WorldAdventurer w = (WorldAdventurer)c;
                        if (w.CollisionContains(mouseLocation) && PlayerManager.PlayerInRange(w.CharCenter) &&
                            InventoryManager.HasSpaceInInventory(w.WhatAreYouHolding()))
                        {
                            InventoryManager.AddNewItemToInventory(w.TakeItem());
                            rv = true;
                        }
                    }
                    else if (c.IsNPC())
                    {
                        NPC n = (NPC)c;
                        if (InventoryManager.CurrentItem != null &&
                            n.CollisionContains(mouseLocation) && PlayerManager.PlayerInRange(n.CharCenter) &&
                            InventoryManager.CurrentItem.ItemType != Item.ItemEnum.Tool &&
                            InventoryManager.CurrentItem.ItemType != Item.ItemEnum.Equipment)
                        {
                            n.Gift(InventoryManager.CurrentItem);
                            rv = true;
                        }
                    }
                }
                foreach (WorldObject s in _liPlacedObjects)
                {
                    if (s.CollisionBox.Contains(mouseLocation))
                    {
                        if (s.IsMachine())
                        {
                            Machine p = (Machine)s;
                            if (p.ProcessingFinished()) { p.TakeFinishedItem(); }
                            else if (!p.Processing()) { p.ProcessClick(); }
                        }

                        break;
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
                if (_liBuildingTiles.Count > 0) { _liBuildingTiles.Clear(); }
            }

            if (GameManager.Scrying())
            {
                WorkerBuilding building = GraphicCursor.HeldBuilding;
                _liBuildingTiles = new List<RHTile>();
                if (building != null)
                {
                    TestMapTiles(building, _liBuildingTiles);
                }

                foreach (WorkerBuilding b in _liBuildings)
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
                foreach(WorldCharacter c in _liCharacters)
                {
                    if (InventoryManager.CurrentItem != null && 
                        !c.IsMob() && c.CollisionBox.Contains(mouseLocation) &&
                        InventoryManager.CurrentItem.ItemType != Item.ItemEnum.Tool &&
                        InventoryManager.CurrentItem.ItemType != Item.ItemEnum.Equipment)
                    {
                        GraphicCursor._currentType = GraphicCursor.CursorType.Gift;
                        found = true;
                        break;
                    }
                    else if(!c.IsMob() && c.CollisionContains(mouseLocation)){
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
            _liCharacters.Clear();
        }

        public RHTile RetrieveTile(Point targetLoc)
        {
            if(targetLoc.X >= GetMapWidth() || targetLoc.X < 0) { return null;  }
            if (targetLoc.Y >= GetMapHeight() || targetLoc.Y < 0) { return null; }

            return _tileArray[targetLoc.X / TileSize, targetLoc.Y / TileSize];
        }
        public RHTile RetrieveTileFromGridPosition(Point targetLoc)
        {
            if (targetLoc.X >= MapWidthTiles || targetLoc.X < 0) { return null; }
            if (targetLoc.Y >= MapHeightTiles || targetLoc.Y < 0) { return null; }

            return _tileArray[targetLoc.X, targetLoc.Y];
        }
        public void RemoveWorldObject(WorldObject o)
        {
            if (_liWorldObjects.Contains(o)){
                _liWorldObjects.Remove(o);
            }
            if (_liPlacedObjects.Contains(o)){
                _liPlacedObjects.Remove(o);
            }
        }
        public void RemoveCharacter(WorldCharacter c)
        {
            ToRemove.Add(c);
        }
        public void RemoveMob(Mob m)
        {
            _liMobs.Remove(m);
        }
        public void DropWorldItems(List<Item>items, Vector2 position)
        {
            foreach(Item i in items)
            {
                ((Item)i).Pop(position);
                _liItems.Add(i);
            }
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
                            b.BuildingChest.MapPosition = Utilities.Normalize(mapObject.Position);
                            PlacePlayerObject(b.BuildingChest);
                        }
                        else if (mapObject.Name.Contains("Pantry"))
                        {
                            b.Pantry.MapPosition = Utilities.Normalize(mapObject.Position);
                            PlacePlayerObject(b.Pantry);
                        }
                    }
                }
            }
            for (int i = 0; i < b.Workers.Count; i++)
            {
                b.Workers[i].Position = spawnPoints[i];
                _liCharacters.Add(b.Workers[i]);
            }
            foreach (WorldObject w in b.PlacedObjects)
            {
                PlacePlayerObject(w);
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
                Vector2 newPos = new Vector2((b.MapPosition.X - translate.X) / Scale, (b.MapPosition.Y - translate.Y) / Scale);
                _dictEntrance.Add(b.PersonalID.ToString(), b.BoxToExit); //TODO: FIX THIS
                GraphicCursor.DropBuilding();
                _liBuildings.Add(b);
                PlayerManager.AddBuilding(b);
                GameManager.Unpause();
                GameManager.Scry(false);
                ResetCamera();
            }
        }

        public bool AddWorkerToBuilding(Point mouseLocation)
        {
            bool rv = false;
            foreach(WorkerBuilding b in _liBuildings)
            {
                if (b.SelectionBox.Contains(mouseLocation))
                {
                    if (b.HasSpace())
                    {
                        RHRandom r = new RHRandom();
                        WorldAdventurer w = ObjectManager.GetWorker(GraphicCursor.WorkerToPlace);
                        b.AddWorker(w, r);
                        b._selected = false;
                        GUIManager.SetScreen(new TextInputScreen(w));
                        GameManager.Scry(false);
                        rv = true;
                    }
                }
            }
            return rv;
        }

        public bool PlaceWorldObject(WorldObject o, bool bounce = false)
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
                Vector2 position = o.MapPosition;
                do
                {
                    position.X = (int)(r.Next(1, (MapWidthTiles - 1) * TileSize) / 32) * 32;
                    position.Y = (int)(r.Next(1, (MapHeightTiles - 1) * TileSize) / 32) * 32;
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
                _liWorldObjects.Add(o);
            }

            return rv;
        }

        public bool TestMapTiles(WorldObject o, List<RHTile> tiles)
        {
            bool rv = false;
            Vector2 position = o.MapPosition;
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
                    int x = Math.Min((o.CollisionBox.Left + (j * 32)) / 32, MapWidthTiles-1);
                    int y = Math.Min((o.CollisionBox.Top + (i * 32)) / 32, MapHeightTiles-1);
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

        public void PlacePlayerObject(WorldObject obj)
        {
            RHTile tile = _tileArray[(int)obj.MapPosition.X / 32, (int)obj.MapPosition.Y / 32];
            tile.SetWorldObject(obj);
            _liPlacedObjects.Add(obj);
            AssignMapTiles(obj, new List<RHTile>() { tile });
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
                    position.X = (int)(r.Next(1, (MapWidthTiles - 1) * TileSize) / 32) * 32;
                    position.Y = (int)(r.Next(1, (MapHeightTiles - 1) * TileSize) / 32) * 32;
                    rv = _tileArray[((int)position.X / 32), ((int)position.Y / 32)].Passable();
                } while (!rv);
            }

            if (rv)
            {
                m.Position = position;

                _liMobs.Add(m);
            }
        }

        #endregion
        
        public int GetMapWidth()
        {
            return MapWidthTiles * _tileSize * (int)Scale;
        }

        public int GetMapHeight()
        {
            return MapHeightTiles * _tileSize * (int)Scale;
        }

        internal MapData SaveData()
        {
            MapData mapData = new MapData
            {
                mapName = this.Name,
                worldObjects = new List<WorldObjectData>(),
                containers = new List<ContainerData>(),
                machines = new List<MachineData>(),
                plants = new List<PlantData>()
            };

            if (!this.IsBuilding)
            {
                foreach (WorldObject w in this.WorldObjects)
                {
                    WorldObjectData d = new WorldObjectData
                    {
                        worldObjectID = w.ID,
                        x = (int)w.MapPosition.X,
                        y = (int)w.MapPosition.Y
                    };
                    mapData.worldObjects.Add(d);
                }

                foreach(WorldObject w in _liPlacedObjects)
                {
                    if (w.IsMachine())
                    {
                        mapData.machines.Add(((Machine)w).SaveData());
                    }
                    else if (w.IsContainer())
                    {
                        mapData.containers.Add(((Container)w).SaveData());
                    }
                    else if (w.IsPlant())
                    {
                        mapData.plants.Add(((Plant)w).SaveData());
                    }
                }
            }

            return mapData;
        }
        internal void LoadData(MapData data)
        {
            foreach (WorldObjectData w in data.worldObjects)
            {
                PlaceWorldObject(ObjectManager.GetWorldObject(w.worldObjectID, new Vector2(w.x, w.y)), true);
            }
            foreach (ContainerData c in data.containers)
            {
                Container con = (Container)ObjectManager.GetWorldObject(c.containerID);
                con.LoadData(c);
                PlacePlayerObject(con);
            }
            foreach (MachineData mac in data.machines)
            {
                Machine theMachine = (Machine)ObjectManager.GetWorldObject(mac.ID);
                theMachine.LoadData(mac);
                PlacePlayerObject(theMachine);
            }
            foreach (PlantData plantData in data.plants)
            {
                Plant plant = (Plant)ObjectManager.GetWorldObject(plantData.ID);
                plant.LoadData(plantData);
                PlacePlayerObject(plant);
            }
        }
    }

    public class RHTile
    {
        private string _mapName;
        public string MapName { get => _mapName; }
        private bool _tileExists;
        private int _X;
        public int X { get => _X; }
        private int _Y;
        public int Y { get => _Y; }
        public Vector2 Position { get => new Vector2(_X * RHMap.TileSize, _Y * RHMap.TileSize); }
        private Dictionary<TiledMapTileLayer, Dictionary<string, string>> _properties;
        private WorldObject _obj;
        public WorldObject WldObject { get => _obj; }

        private bool _isRoad;
        public bool IsRoad { get => _isRoad; }

        public RHTile(int x, int y,string mapName)
        {
            _X = x;
            _Y = y;

            _mapName = mapName;
            _properties = new Dictionary<TiledMapTileLayer, Dictionary<string, string>>();
        }
        public List<RHTile> GetWalkableNeighbours()
        {
            Vector2[] DIRS = new[] { new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1) };
            List<RHTile> neighbours = new List<RHTile>();
            foreach(Vector2 d in DIRS)
            {
                RHTile tile = MapManager.Maps[MapName].RetrieveTileFromGridPosition(new Point((int)(_X + d.X), (int)(_Y + d.Y)));
                if (tile != null && tile.Passable() && tile.WldObject == null){
                    neighbours.Add(tile);
                }
            }

            return neighbours;
        }

        public void SetProperties(RHMap map)
        {
            foreach (TiledMapTileLayer l in map.Layers.Values)
            {
                if (l.TryGetTile(_X, _Y, out TiledMapTile? tile) && tile != null)
                {
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
            _isRoad = ContainsProperty("Road", out string value) && value.Equals("true");
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

        public bool Passable()
        {
            bool rv = _tileExists && (_obj == null || !_obj.Blocking);
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
            if (_tileExists && _obj == null)
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

        public bool DamageObject(int dmg)
        {
            bool rv = false;
            if (_obj.IsDestructible())
            {
                rv = ((Destructible)_obj).DealDamage(dmg);
                if (rv)
                {
                    MapManager.DropWorldItems(DropManager.DropItemsFromWorldObject(_obj.ID), _obj.CollisionBox.Center.ToVector2());
                    MapManager.RemoveWorldObject(_obj);
                    _obj.ClearTiles();
                }
            }

            return rv;
        }

        public void Clear()
        {
            _obj = null;
        }
    }
}
