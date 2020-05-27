﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using RiverHollow.Actors;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static RiverHollow.Game_Managers.CombatManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.GUIObjects.HUDMenu;
using static RiverHollow.RiverHollow;
using static RiverHollow.WorldObjects.WorldItem;
using static RiverHollow.WorldObjects.WorldItem.Floor;

namespace RiverHollow.Tile_Engine
{
    public class RHMap
    {
        public int MapWidthTiles = 100;
        public int MapHeightTiles = 100;
        
        private string _sName;
        public string Name { get => _sName.Replace(@"Maps\", ""); set => _sName = value; } //Fuck off with that path bullshit

        bool _bCombatMap;
        public bool IsCombatMap => _bCombatMap;
        bool _bBuilding;
        public bool IsBuilding => _bBuilding;
        string _sDungeonName;
        public string DungeonName => _sDungeonName;
        public bool IsDungeon => !string.IsNullOrEmpty(_sDungeonName);
        bool _bTown;
        public bool IsTown => _bTown;
        bool _bManor;
        public bool IsManor => _bManor;
        bool _bOutside;
        public bool IsOutside => _bOutside;
        bool _bProduction = true;
        public bool Production => _bProduction;
        int _iActiveSpawnPoints;
        public int ActiveSpawnPoints => _iActiveSpawnPoints;
        int _iTotalResourceWeight = 0;  //The total space on the map in tiles occupied by resource spawns

        private RHTile _targetTile = null;
        public RHTile TargetTile => _targetTile;

        protected TiledMap _map;
        public TiledMap Map { get => _map; }

        protected RHTile[,] _arrTiles;
        protected TiledMapRenderer _renderer;
        protected List<TiledMapTileset> _liTilesets;
        protected Dictionary<string, TiledMapTileLayer> _diLayers;
        public Dictionary<string, TiledMapTileLayer> Layers => _diLayers;

        protected List<RHTile> _liTestTiles;
        protected List<WorldActor> _liActors;
        protected List<Monster> _liMonsters;
        public List<Monster> Monsters => _liMonsters;
        protected List<DungeonObject> _liDungeonObjects;
        public List<WorldActor> ToRemove;
        public List<WorldActor> ToAdd;
        protected List<Building> _liBuildings;
        protected List<RHTile> _liTilledTiles;
        protected List<WorldObject> _liPlacedWorldObjects;
        public List<RHTile> TilledTiles => _liTilledTiles;
        protected List<MonsterSpawn> _liMonsterSpawnPoints;
        protected List<ResourceSpawn> _liResourceSpawnPoints;
        protected List<int> _liRandomSpawnItems;
        protected List<int> _liCutscenes;

        protected List<Item> _liItems;
        protected List<Item> _liItemsToRemove;
        protected List<ShopData> _liShopData;

        private Dictionary<string, RHTile[,]> _diCombatTiles;
        public Dictionary<string, RHTile[,]> DictionaryCombatTiles => _diCombatTiles;
        private Dictionary<string, TravelPoint> _diTravelPoints;
        public Dictionary<string, TravelPoint> DictionaryTravelPoints => _diTravelPoints;
        private Dictionary<string, Vector2> _diCharacterLayer;
        public Dictionary<string, Vector2> DictionaryCharacterLayer => _diCharacterLayer;
        private List<TiledMapObject> _liMapObjects;

        public RHMap() {
            _liMonsterSpawnPoints = new List<MonsterSpawn>();
            _liResourceSpawnPoints = new List<ResourceSpawn>();
            _liTestTiles = new List<RHTile>();
            _liTilesets = new List<TiledMapTileset>();
            _liActors = new List<WorldActor>();
            _liMonsters = new List<Monster>();
            _liBuildings = new List<Building>();
            _liTilledTiles = new List<RHTile>();
            _liItems = new List<Item>();
            _liItemsToRemove = new List<Item>();
            _liMapObjects = new List<TiledMapObject>();
            _diTravelPoints = new Dictionary<string, TravelPoint>();
            _diCharacterLayer = new Dictionary<string, Vector2>();
            _liShopData = new List<ShopData>();
            _liDungeonObjects = new List<DungeonObject>();
            _liPlacedWorldObjects = new List<WorldObject>();
            _liRandomSpawnItems = new List<int>();
            _liCutscenes = new List<int>();
            _diCombatTiles = new Dictionary<string, RHTile[,]>();

            ToRemove = new List<WorldActor>();
            ToAdd = new List<WorldActor>();
        }

        public RHMap(RHMap map) : this()
        {
            _map = map.Map;
            _sName = map.Name+"Clone";
            _renderer = map._renderer;
            _arrTiles = map._arrTiles;

            _bBuilding = _map.Properties.ContainsKey("Building");
            _bCombatMap = _map.Properties.ContainsKey("Combat");
            _bTown = _map.Properties.ContainsKey("Town");
            _bOutside = _map.Properties.ContainsKey("Outside");
            _bManor = _map.Properties.ContainsKey("Manor");

            if (_map.Properties.ContainsKey("Dungeon"))
            {
                _sDungeonName = _map.Properties["Dungeon"];
                DungeonManager.AddMapToDungeon(_map.Properties["Dungeon"], _sName);
            }

            if (_map.Properties.ContainsKey("Production")) {
                bool.TryParse(_map.Properties["Production"], out _bProduction);
            }

            if (_map.Properties.ContainsKey("ActiveSpawn")) {
                int.TryParse(_map.Properties["ActiveSpawn"].ToString(), out _iActiveSpawnPoints);
            }

            _liBuildings = map._liBuildings;
            _liPlacedWorldObjects = map._liPlacedWorldObjects;

            MapWidthTiles = _map.Width;
            MapHeightTiles = _map.Height;

            LoadMapObjects();
        }

        public void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice, string newMap, string mapName)
        {
            _map = Content.Load<TiledMap>(newMap);
            _sName = mapName;
            MapWidthTiles = _map.Width;
            MapHeightTiles = _map.Height;

            _diLayers = new Dictionary<string, TiledMapTileLayer>();
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                _diLayers.Add(l.Name, l);
            }

            _arrTiles = new RHTile[MapWidthTiles, MapHeightTiles];
            for (int i = 0; i < MapHeightTiles; i++) {
                for (int j = 0; j < MapWidthTiles; j++)
                {
                    _arrTiles[j, i] = new RHTile(j, i, _sName);
                    _arrTiles[j, i].SetProperties(this);
                }
            }

            _bBuilding = _map.Properties.ContainsKey("Building");
            _bCombatMap = _map.Properties.ContainsKey("Combat");
            _bTown = _map.Properties.ContainsKey("Town");
            _bOutside = _map.Properties.ContainsKey("Outside");
            _bManor = _map.Properties.ContainsKey("Manor");

            if (_map.Properties.ContainsKey("Dungeon"))
            {
                _sDungeonName = _map.Properties["Dungeon"];
                DungeonManager.AddMapToDungeon(_map.Properties["Dungeon"], _sName);
            }

            if (_map.Properties.ContainsKey("Production"))
            {
                bool.TryParse(_map.Properties["Production"], out _bProduction);
            }

            if (_map.Properties.ContainsKey("Outside"))
            {
                bool.TryParse(_map.Properties["Outside"], out _bOutside);
            }

            if (_map.Properties.ContainsKey("ActiveSpawn"))
            {
                int.TryParse(_map.Properties["ActiveSpawn"].ToString(), out _iActiveSpawnPoints);
            }

            if (_map.Properties.ContainsKey("Cutscenes"))
            {
                string[] split = _map.Properties["Cutscenes"].Split(' ');
                foreach(string cutsceneID in split)
                {
                    _liCutscenes.Add(int.Parse(cutsceneID));
                }
            }

            if (_bTown)
            {
                foreach (KeyValuePair<int, Upgrade> kvp in GameManager.DiUpgrades)
                {
                    if (kvp.Value.Enabled) { EnableUpgradeVisibility(kvp.Key); }
                }
            }
            _renderer = new TiledMapRenderer(GraphicsDevice);
        }

        public void WaterTiles()
        {
            foreach(RHTile t in _liTilledTiles)
            {
                if (t.HasBeenDug())
                {
                    t.Water(true);
                }
            }
        }

        public void LoadMapObjects()
        {
            List<TiledMapObject> spawnObjects = new List<TiledMapObject>();
            ReadOnlyCollection<TiledMapObjectLayer> objectLayers = _map.ObjectLayers;
            foreach (TiledMapObjectLayer ol in objectLayers)
            {
                if (ol.Name.Contains("Travel"))
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        if (mapObject.Name.Equals("Entrance"))
                        {
                            TravelPoint trvlPt = new TravelPoint(mapObject);
                            if (mapObject.Properties.ContainsKey("Door")) { CreateDoor(ref trvlPt, mapObject.Position.X, mapObject.Position.Y, mapObject.Size.Width, mapObject.Size.Height); }
                            _diTravelPoints.Add(trvlPt.LinkedMap, trvlPt);
                        }
                    }
                }
                else if (ol.Name.Contains("Character"))
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        if (mapObject.Name.Equals("CombatStart"))
                        {
                            string entrance = mapObject.Properties["Map"];
                            RHTile[,] tiles = new RHTile[3, 3];

                            DirectionEnum sidle = DirectionEnum.Right;
                            DirectionEnum change = DirectionEnum.Down;

                            string startPoint = mapObject.Properties["Position"];
                            if (startPoint == "NW")
                            {
                                sidle = DirectionEnum.Right;
                                change = DirectionEnum.Down;
                            }
                            else if (startPoint == "NE")
                            {
                                sidle = DirectionEnum.Down;
                                change = DirectionEnum.Left;
                            }
                            else if (startPoint == "SE")
                            {
                                sidle = DirectionEnum.Left;
                                change = DirectionEnum.Up;
                            }
                            else if (startPoint == "SW")
                            {
                                sidle = DirectionEnum.Up;
                                change = DirectionEnum.Right;
                            }

                            tiles[0, 0] = GetTileByPixelPosition(mapObject.Position);
                            tiles[1, 0] = tiles[0, 0].GetTileByDirection(sidle);
                            tiles[2, 0] = tiles[1, 0].GetTileByDirection(sidle);

                            tiles[0, 1] = tiles[0, 0].GetTileByDirection(change);
                            tiles[1, 1] = tiles[0, 1].GetTileByDirection(sidle);
                            tiles[2, 1] = tiles[1, 1].GetTileByDirection(sidle);

                            tiles[0, 2] = tiles[0, 1].GetTileByDirection(change);
                            tiles[1, 2] = tiles[0, 2].GetTileByDirection(sidle);
                            tiles[2, 2] = tiles[1, 2].GetTileByDirection(sidle);
                            _diCombatTiles[entrance] = tiles;
                        }
                        else if (mapObject.Name.Equals("Shop"))
                        {
                            _liShopData.Add(new ShopData(_sName, mapObject));
                        }
                        else
                        {
                            _diCharacterLayer.Add(mapObject.Name, mapObject.Position);
                        }
                    }
                }
                else if (ol.Name.Contains("MapObject"))
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        _liMapObjects.Add(mapObject);
                    }
                }
            }
        }

        public void PopulateMap(bool loaded = false)
        {
            RHRandom rand = RHRandom.Instance;
            TiledMapProperties props = _map.Properties;
            List<int> _liMobs = new List<int>();
            int minMobs = 0;
            int maxMobs = 0;
            List<int> resources = new List<int>();

            foreach (TiledMapObject obj in _liMapObjects)
            {
                if (obj.Name.Equals("DungeonObject"))
                {
                    DungeonObject d = DataManager.GetDungeonObject(int.Parse(obj.Properties["ID"]), Util.SnapToGrid(obj.Position));
                    if (obj.Properties.ContainsKey("Key")) { d.SetKey(obj.Properties["Key"]); }
                    if (obj.Properties.ContainsKey("Trigger")) { d.SetTrigger(obj.Properties["Trigger"]); }

                    PlaceWorldObject(d);
                    _liDungeonObjects.Add(d);
                 }
                else if (obj.Name.Equals("WorldObject"))
                {
                    PlaceWorldObject(DataManager.GetWorldObject(int.Parse(obj.Properties["ID"]), Util.SnapToGrid(obj.Position)));
                }
                else if (obj.Name.Equals("Chest"))
                {
                    Container c = (Container)DataManager.GetWorldObject(190);
                    InventoryManager.InitContainerInventory(c.Inventory);
                    c.MapPosition = obj.Position;
                    PlacePlayerObject(c);
                    string[] holdSplit = obj.Properties["Holding"].Split('/');
                    foreach (string s in holdSplit)
                    {
                        InventoryManager.AddToInventory(int.Parse(s), 1, false);
                    }
                    InventoryManager.ClearExtraInventory();
                }
                else if (obj.Name.Equals("Spirit"))
                {
                    Spirit s = new Spirit(obj.Properties["Name"], obj.Properties["Type"], obj.Properties["Condition"], obj.Properties["Text"])
                    {
                        Position = Util.SnapToGrid(obj.Position),
                        CurrentMapName = _sName
                    };
                    _liActors.Add(s);
                }
                else if (obj.Name.Equals("SpawnPoint"))
                {
                    _liMonsterSpawnPoints.Add(new MonsterSpawn(this, obj));
                }
                else if (obj.Name.Equals("ResourceSpawn"))
                {
                    ResourceSpawn a = new ResourceSpawn(this, obj);
                    _liResourceSpawnPoints.Add(a);
                    _iTotalResourceWeight += a.Size;
                    
                }
                else if (obj.Name.Equals("Manor") && !loaded)
                {
                    Building manor = DataManager.GetManor();
                    manor.SetCoordinatesByGrid(obj.Position);
                    manor.SetName(PlayerManager.ManorName);
                    AddBuilding(manor, true);
                }
                else if (obj.Properties.ContainsKey("Item"))
                {
                    Item item = DataManager.GetItem(int.Parse(obj.Properties["Item"]));
                    item.AutoPickup = false;
                    item.ManualPickup = true;
                    item.OnTheMap = true;
                    item.Position = Util.SnapToGrid(obj.Position);
                    _liItems.Add(item);
                }
                //else if (obj.Name.Equals("Building"))
                //{
                //    Building b = ObjectManager.GetBuilding(int.Parse(obj.Properties["ID"]));
                //    b.SetCoordinatesByGrid(obj.Position);
                //    //b.SetName(PlayerManager.ManorName);
                //    AddBuilding(b);
                //}
            }

            if(_liRandomSpawnItems.Count > 0)
            {
                for (int i = 0; i < 30; i++)
                {
                    Plant obj = (Plant)DataManager.GetWorldObject(_liRandomSpawnItems[0], new Vector2(rand.Next(1, _map.Width - 1) * TileSize, rand.Next(1, _map.Height - 1) * TileSize));
                    obj.FinishGrowth();
                    PlaceWorldObject(obj, true);
                }
            }

            if (_liMobs.Count > 0)
            {
                int numMobs = rand.Next(minMobs, maxMobs);
                while (numMobs != 0)
                {
                    int chosenMob = rand.Next(0, _liMobs.Count - 1);

                    Vector2 vect = new Vector2(rand.Next(1, _map.Width - 1) * TileSize, rand.Next(1, _map.Height - 2) * TileSize);
                    Monster newMonster = DataManager.GetMonster(_liMobs[chosenMob], vect);
                    newMonster.CurrentMapName = _sName;
                    AddMonster(newMonster);

                    numMobs--;
                }
            }

            SpawnMonsters();
            SpawnResources();
        }

        public void Rollover()
        {
            foreach(RHTile tile in _liTilledTiles)
            {
                tile.Rollover();
            }

            SpawnMonsters();

            CheckSpirits();
            _liItems.Clear();
        }

        public void CreateDoor(ref TravelPoint trvlPt, float rectX, float rectY, float width, float height)
        {
            for (float x = rectX; x < rectX + width; x += TileSize)
            {
                for (float y = rectY; y < rectY + height; y += TileSize)
                {
                    RHTile t = GetTileByPixelPosition((int)x, (int)y);
                    if (t != null)
                    {
                        t.SetMapObject(trvlPt);
                        t.GetTileByDirection(DirectionEnum.Up).SetMapObject(trvlPt);
                    }
                }
            }
        }
        private void SpawnMonsters()
        {
            //Remove all mobs from the map
            foreach (Monster m in _liMonsters)
            {
                RemoveMonster(m);
            }
            _liMonsters.Clear();

            //Copy the spawn points to a list we can safely modify
            List<MonsterSpawn> spawnCopy = new List<MonsterSpawn>();
            spawnCopy.AddRange(_liMonsterSpawnPoints);

            //Trigger x number of SpawnPoints
            for (int i = 0; i < _iActiveSpawnPoints; i++)
            {
                //Get a random Spawn Point
                int point = RHRandom.Instance.Next(0, spawnCopy.Count - 1);
                if (!spawnCopy[point].HasSpawned())
                {
                    //Trigger the Spawn point and remove it from the copied list
                    //so it won't be an option for future spawning.
                    spawnCopy[point].Spawn();
                    spawnCopy.RemoveAt(point);
                }
            }
        }

        /// <summary>
        /// Spawns resources from the ResourceSpawn points on the map
        /// </summary>
        private void SpawnResources()
        {
            //Spawns a random assortment of resources them ap will allow wherever they're allowed
            if (_liResourceSpawnPoints.Count > 0)
            {
                //Determine how many resources to spawn
                string[] val = _map.Properties["ResourcesMinMax"].Split('-');
                int spawnNumber = RHRandom.Instance.Next(int.Parse(val[0]), int.Parse(val[1]));

                for (int i = 0; i < spawnNumber; i++)
                {
                    int index = 0;
                    int current = 0;    //This variable will store how far we've gone in the list
                    int roll = RHRandom.Instance.Next(0, _iTotalResourceWeight);

                    ResourceSpawn ToSpawn = null;
                    do
                    {
                        ToSpawn = _liResourceSpawnPoints[index];
                        current += ToSpawn.Size;
                        index++;
                    } while (roll > current);

                    ToSpawn.Spawn();  
                }
            }
        }

        public void CheckSpirits()
        {
            foreach (WorldActor c in _liActors)
            {
                if (c.IsSpirit())
                {
                    ((Spirit)c).CheckCondition();
                }
            }
        }

        public Spirit FindSpirit()
        {
            Spirit rv = null;

            foreach(Actor a in _liActors)
            {
                if(a.IsSpirit() && PlayerManager.PlayerInRange(a.Position.ToPoint(), 500))
                {
                    rv = (Spirit)a;
                }
            }

            return rv;
        }

        public bool Contains(WorldActor c)
        {
            return _liActors.Contains(c);
        }

        public void ItemPickUpdate()
        {
            WorldActor player = PlayerManager.World;
            for(int i = 0; i < _liItems.Count; i++)
            {
                Item it = _liItems[i];
                if (InventoryManager.HasSpaceInInventory(it.ItemID, it.Number))
                {
                    if (it.OnTheMap && it.AutoPickup)
                    {
                        if (it.FinishedMoving() && it.CollisionBox.Intersects(player.CollisionBox))
                        {
                            AddItemToPlayerInventory(it);
                        }
                        else if (PlayerManager.PlayerInRange(it.CollisionBox.Center, 80))
                        {
                            float speed = 3;
                            Vector2 direction = Util.MoveUpTo(it.Position, player.CollisionBox.Location.ToVector2(), speed);
                            direction.Normalize();
                            it.Position += (direction * speed);
                        }
                    }
                }
            }
        }

        public void AddItemToPlayerInventory(Item it)
        {
           _liItemsToRemove.Add(it);
           InventoryManager.AddToInventory(DataManager.GetItem(it.ItemID, it.Number));
        }

        public void Update(GameTime gTime)
        {
            if (this == MapManager.CurrentMap)
            {
                _renderer.Update(_map, gTime);
                if (CombatManager.InCombat || IsRunning())
                {
                    foreach (Monster m in _liMonsters)
                    {
                        m.Update(gTime);
                    }
                }

                foreach (Item i in _liItems)
                {
                    ((Item)i).Update();
                }
            }

            foreach (WorldObject obj in _liPlacedWorldObjects)
            {
                obj.Update(gTime);
            }

            if (ToAdd.Count > 0)
            {
                List<WorldActor> moved = new List<WorldActor>();
                foreach (WorldActor c in ToAdd)
                {
                    if (AddCharacterImmediately(c))
                    {
                        moved.Add(c);
                    }
                }
                foreach (WorldActor c in moved)
                {
                    ToAdd.Remove(c);
                }
                moved.Clear();
            }

            foreach (WorldActor c in ToRemove)
            {
                if (c.IsMonster() && _liMonsters.Contains((Monster)c)) { _liMonsters.Remove((Monster)c); }
                else if (_liActors.Contains(c)) { _liActors.Remove(c); }
            }
            ToRemove.Clear();

            if (IsRunning())
            {
                foreach (WorldActor c in _liActors)
                {
                    c.Update(gTime);
                }
            }


            ItemPickUpdate();

            foreach (Item i in _liItemsToRemove)
            {
                _liItems.Remove(i);
            }
            _liItemsToRemove.Clear();
        }

        public void DrawBase(SpriteBatch spriteBatch)
        {
            SetLayerVisibility(false);

            _renderer.Draw(_map, Camera._transform);

            if (CombatManager.InCombat)
            {
                if(CombatManager.ActiveCharacter != null && CombatManager.ActiveCharacter.IsAdventurer())
                {
                    CombatManager.ActiveCharacter.BaseTile?.Draw(spriteBatch);
                }

                foreach (RHTile t in CombatManager.LegalTiles)
                {
                    t.Draw(spriteBatch);
                }

                foreach (RHTile t in CombatManager.AreaTiles)
                {
                    if (!CombatManager.LegalTiles.Contains(t))
                    {
                        t.Draw(spriteBatch);
                    }
                }
            }

            foreach (WorldActor c in _liActors)
            {
                c.Draw(spriteBatch, true);
            }

            foreach (Monster m in _liMonsters)
            {
                m.Draw(spriteBatch, true);
            }

            foreach(Building b in _liBuildings)
            {
                b.Draw(spriteBatch);
            }

            foreach (RHTile t in _liTilledTiles)
            {
                t.Draw(spriteBatch);
            }

            foreach (WorldObject obj in _liPlacedWorldObjects)
            {
                obj.Draw(spriteBatch);
            }

            foreach (Item i in _liItems)
            {
                i.Draw(spriteBatch);
            }

            foreach(RHTile t in _liTestTiles)
            {
                bool passable = t.Passable() && (Scrying() || PlayerManager.PlayerInRange(t.Rect));
                spriteBatch.Draw(DataManager.GetTexture(@"Textures\Dialog"), new Rectangle((int)t.Position.X, (int)t.Position.Y, TileSize, TileSize), new Rectangle(288, 128, TileSize, TileSize) , passable ? Color.Green *0.5f : Color.Red * 0.5f, 0, Vector2.Zero, SpriteEffects.None, 99999);
            }
        }

        public void DrawLights(SpriteBatch spriteBatch)
        {
            foreach (WorldObject obj in _liPlacedWorldObjects)
            {
                if(obj.Type == WorldObject.ObjectType.Light)
                {
                    spriteBatch.Draw(lightMask, new Vector2(obj.CollisionBox.Center.X - lightMask.Width / 2, obj.CollisionBox.Y - lightMask.Height / 2), Color.White);
                }
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
                if (_bTown)
                {
                    foreach (KeyValuePair<int, Upgrade> s in GameManager.DiUpgrades)    //Check each upgrade to see if it's enabled
                    {
                        if(l.Name.Contains(s.Key.ToString())) {
                            upgrade = true;
                        }
                        if (s.Value.Enabled)
                        {                           
                            bool determinant = l.Name.Contains("Upper");
                            if (revealUpper) {
                                l.IsVisible = determinant;
                            }
                            else { l.IsVisible = !determinant; }
                        }
                    }
                }

                if (!upgrade)
                {
                    bool determinant = l.Name.Contains("Upper");

                    if (revealUpper) {
                        l.IsVisible = determinant;
                    }
                    else { l.IsVisible = !determinant; }
                }

                if (l.IsVisible && _bOutside) {
                    l.IsVisible = l.Name.Contains(GameCalendar.GetSeason());
                }
            }
        }

        public void EnableUpgradeVisibility(int upgradeID)
        {
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                if (l.Name.Contains(upgradeID.ToString())) { l.IsVisible = true; }
            }
        }

        #region Collision Code
        /// <summary>
        /// Find every tile that could possibly cause a collision for the moving WorldActor
        /// </summary>
        /// <param name="actor">The moving WorldActor</param>
        /// <param name="dir"></param>
        /// <returns>The direction they are moving in</returns>
        private List<Rectangle> GetPossibleCollisions(WorldActor actor, Vector2 dir)
        {
            List<Rectangle> list = new List<Rectangle>();
            Rectangle rEndCollision = new Rectangle((int)(actor.CollisionBox.X + dir.X), (int)(actor.CollisionBox.Y + dir.Y), actor.CollisionBox.Width, actor.CollisionBox.Height);

            //The following if-blocks get the tiles that the four corners of the
            //moved CollisionBox will be inside of, based off of the movement direction.
            if(dir.X > 0)
            {
                AddTile(ref list, rEndCollision.Right, rEndCollision.Top);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Bottom);
            }
            else if(dir.X < 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Top);
                AddTile(ref list, rEndCollision.Left, rEndCollision.Bottom);
            }

            if (dir.Y > 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Bottom);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Bottom);
            }
            else if (dir.Y < 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Top);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Top);
            }

            //Because RHTiles do not contain WorldActors outside of combat, we need to add each
            //WorldActor's CollisionBox to the list, as long as the WorldActor in question is not the moving WorldActor.
            foreach(WorldActor w in _liActors)
            {
                if (w.Active && w != actor) { list.Add(w.CollisionBox);}
            }

            //If the actor is not the Player Character, add the Player Character's CollisionBox to the list as well
            if(actor != PlayerManager.World) {
                list.Add(PlayerManager.World.CollisionBox);
            }

            return list;
        }
        private void AddTile(ref List<Rectangle> list, int one, int two)
        {
            RHTile tile = MapManager.CurrentMap.GetTileByGridCoords(Util.GetGridCoords(one, two));
            if (TileValid(tile, list)) { list.Add(tile.Rect); }
        }
        private bool TileValid(RHTile tile, List<Rectangle> list)
        {
            return tile != null && !tile.Passable() && !list.Contains(tile.Rect);
        }

        private bool ChangeDir(List<Rectangle> possibleCollisions, Rectangle originalRectangle, ref Vector2 dir, string map)
        {
            bool rv = false;

            //Because of how objects interact with each other, this check needs to be broken up so that the x and y movement can be
            //calculated seperately. If an object is above you and you move into it at an angle, if you check the collision as one rectangle
            //then the collision nullification will hit the entire damn movement mode.
            Rectangle newRectangleX = new Rectangle((int)(originalRectangle.X + dir.X), (int)(originalRectangle.Y), originalRectangle.Width, originalRectangle.Height);
            Rectangle newRectangleY = new Rectangle((int)(originalRectangle.X), (int)(originalRectangle.Y + dir.Y), originalRectangle.Width, originalRectangle.Height);
            foreach (Rectangle r in possibleCollisions)
            {
                Vector2 coords = Util.GetGridCoords(r.Location);
                if (dir.Y != 0 && r.Intersects(newRectangleY))
                {
                    float initY = dir.Y;

                    if (initY > 0) { dir.Y -= Math.Min((newRectangleY.Bottom - r.Top + 1), dir.Y); }
                    else if (initY < 0) { dir.Y += Math.Max((r.Bottom - newRectangleY.Top + 1), -dir.Y); }

                    //Modifier is to determine if the nudge is positive or negative
                    int modifier = (int)CheckToNudge(newRectangleY.Center.X, r.Center.X, coords.X, coords.Y, "Col");
                    int xVal = (int)(modifier > 0 ? newRectangleX.Right : newRectangleX.Left) + modifier;               //Constructs the new rectangle based on the mod

                    dir.X += CheckNudgeAllowed(modifier, new Point(xVal, newRectangleY.Top), new Point(xVal, newRectangleY.Bottom), map);
                }
                if (dir.X != 0 && r.Intersects(newRectangleX))
                {
                    float initX = dir.X;
                    if (initX > 0) { dir.X -= Math.Min((newRectangleX.Right - r.Left + 1), dir.X); }
                    else if (initX < 0) { dir.X += Math.Max((r.Right - newRectangleX.Left + 1), -dir.X); }

                    //Modifier is to determine if the nudge is positive or negative
                    int modifier = (int)CheckToNudge(newRectangleY.Center.Y, r.Center.Y, coords.X, coords.Y, "Row");
                    int yVal = (int)(modifier > 0 ? newRectangleX.Bottom : newRectangleX.Top) + modifier;               //Constructs the new rectangle based on the mod

                    dir.Y += CheckNudgeAllowed(modifier, new Point(newRectangleY.Left, yVal), new Point(newRectangleY.Right, yVal), map);
                }

                //Because of diagonal movement, it's possible to have no issue on either the x axis or y axis but have a collision
                //diagonal to the actor. In this case, just mnull out the x movement.
                if (dir.X != 0 && dir.Y != 0 && r.Intersects(newRectangleX) && r.Intersects(newRectangleY))
                {
                    dir.X = 0;
                }
            }

            return rv;
        }
        private float CheckNudgeAllowed(float modifier, Point first, Point second, string map)
        {
            float rv = 0;
            RHTile firstTile = MapManager.Maps[map].GetTileByGridCoords(Util.GetGridCoords(first));
            RHTile secondTile = MapManager.Maps[map].GetTileByGridCoords(Util.GetGridCoords(second));
            if (firstTile != null && firstTile.Passable() && secondTile != null && secondTile.Passable())
            {
                rv = modifier;
            }

            return rv;
        }

        /// <summary>
        /// Check for collisions between the given actor and any world objects. Do not perform collision detection 
        /// at this level if we are in Combat and the WorldActor is on a Combat Map.
        /// 
        /// Also checks for collisions with objects that will change maps. If we connect with one, do not move by command anymore.
        /// </summary>
        /// <param name="c">The moving WorldActor</param>
        /// <param name="testX">Movement along the X Axis</param>
        /// <param name="testY">Movement along the Y Axis</param>
        /// <param name="dir">Reference to the direction to move the WorldActor</param>
        /// <param name="ignoreCollisions">Whether or not to check collisions</param>
        /// <returns>False if we are to prevent movement</returns>
        public bool CheckForCollisions(WorldActor c, Rectangle testX, Rectangle testY, ref Vector2 dir, bool ignoreCollisions = false)
        {
            bool rv = true;

            //Checking for a MapChange takes priority overlooking for collisions.
            if (CheckForMapChange(c, testX) || CheckForMapChange(c, testY)) {
                return false;
            }
            else if (!ignoreCollisions && !(CombatManager.InCombat && c.CurrentMap.IsCombatMap))
            {
                List<Rectangle> list = GetPossibleCollisions(c, dir);
                ChangeDir(list, c.CollisionBox, ref dir, c.CurrentMapName);
            }

            return rv;
        }

        /// <summary>
        /// Checks to see if the moving character has intersected with a MapChange object.
        /// If so, inform the MapManager and move the WorldActor to the new map
        /// </summary>
        /// <param name="c">The WorldActor to check</param>
        /// <param name="movingChar">The prospective endpoint of the movement</param>
        /// <returns></returns>
        public bool CheckForMapChange(WorldActor c, Rectangle movingChar)
        {
            foreach(KeyValuePair<string, TravelPoint> kvp in _diTravelPoints)
            {
                if (kvp.Value.Intersects(movingChar))
                {
                    MapManager.ChangeMaps(c, this.Name, kvp.Value);
                    return true;

                    //Unused code for now since AdventureMaps are unused
                    //if (IsDungeon) { if (c == PlayerManager.World) { MapManager.ChangeDungeonRoom(kvp.Value); return true; } }
                }
            }
            return false;
        }

        #region Collision Helpers
        public float CheckToNudge(float movingCenter, float objCenter, float varCol, float varRow, string v)
        {
            float rv = 0;
            float centerDelta = movingCenter - objCenter;
            if (varCol == -1 && varRow == -1) {
                if (centerDelta > 0) { rv = 1; }
                else if (centerDelta < 0) { rv = -1; }
            }
            else if (centerDelta > 0)
            {
                RHTile testTile = GetTileByGridCoords((int)(varCol + (v.Equals("Col") ? 1 : 0)), (int)(varRow + (v.Equals("Row") ? 1 : 0)));
                if (testTile != null && testTile.Passable()) {
                    rv = 1;
                }
            }
            else if (centerDelta < 0)
            {
                RHTile testTile = GetTileByGridCoords((int)(varCol - (v.Equals("Col") ? 1 : 0)), (int)(varRow + (v.Equals("Row") ? 1 : 0)));
                if (testTile != null && testTile.Passable()) { rv = -1; }
            }

            return rv;
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
            Item displayItem = DataManager.GetItem(itemID);
            displayItem.AutoPickup = false;
            displayItem.OnTheMap = true;
            displayItem.Position = _diCharacterLayer[npcIndex + "Col" + index];
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
            Vector2 rv = Vector2.Zero;
            if (_diCharacterLayer.ContainsKey(val))
            {
                rv = _diCharacterLayer[val];
            }
            return rv;
        }

        #region Input Processing
        public bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            RHTile tile = MapManager.RetrieveTile(mouseLocation);

            //Do nothing if no tile could be retrieved
            if (tile == null) { return rv; }

            if(tile.GetTravelPoint() != null) {
                RHTile.TileObject obj = tile.GetTravelPoint();

                if (PlayerManager.PlayerInRange(obj.TravelPointInfo.CollisionBox))
                {
                    if (obj.PlayerBuilding != null) { MapManager.EnterBuilding(obj.PlayerBuilding); }
                    else { MapManager.ChangeMaps(PlayerManager.World, this.Name, obj.TravelPointInfo); }
                }
            }
            else if (tile.GetWorldObject() != null)
            {
                WorldObject obj = tile.GetWorldObject();
                if (obj.IsMachine())
                {
                    Machine p = (Machine)obj;
                    if (p.HasItem()) { p.TakeFinishedItem(); }
                    else if (InventoryManager.GetCurrentItem() != null && !p.Working()) { p.ProcessClick(); }
                }
                else if (obj.IsContainer())
                {
                    if (IsDungeon && DungeonManagerOld.IsEndChest((Container)obj))
                    {
                        Staircase stairs = (Staircase)DataManager.GetWorldObject(3, Vector2.Zero);
                        stairs.SetExit(MapManager.HomeMap);
                        PlaceWorldObject(stairs, true);
                    }
                    GUIManager.OpenMainObject(new HUDInventoryDisplay(((Container)obj).Inventory));
                }
                else if (obj.IsPlant())
                {
                    Plant p = (Plant)obj;
                    if (p.FinishedGrowing())
                    {
                        Item i = p.Harvest(false);
                        //if (i != null)
                        //{
                        //    _liItems.Add(i);
                        //}
                        MapManager.RemoveWorldObject(p);
                        p.RemoveSelfFromTiles();
                    }
                }
                else if (obj.IsDungeonObject())
                {
                    GameManager.gmDungeonObject = (DungeonObject)obj;
                    ((DungeonObject)obj).Interact();
                }
            }

            if (tile.ContainsProperty("Journal", out string val) && val.Equals("true"))
            {
                GUIManager.OpenTextWindow(DataManager.GetGameText("Journal"));
            }

            foreach (ShopData shop in _liShopData)
            {
                if (shop.Contains(mouseLocation) && shop.IsOpen())
                {
                    rv = true;
                    shop.Talk();
                    break;
                }
            }

            if (!rv)
            {
                foreach (WorldActor c in _liActors)
                {
                    if (PlayerManager.PlayerInRange(c.HoverBox, (int)(TileSize * 1.5)) && c.HoverContains(mouseLocation) && c.CanTalk && c.Active)
                    {
                        ((TalkingActor)c).Talk();
                        rv = true;
                        break;
                    }
                }
            }


            if (!rv)
            {
                List<Item> removedList = new List<Item>();
                for (int i = 0; i < _liItems.Count; i++)
                {
                    Item it = _liItems[i];
                    if (it.ManualPickup && it.CollisionBox.Contains(GraphicCursor.GetWorldMousePosition()))
                    {
                        if(it.ItemID == 300)
                        {
                            DungeonManager.AddDungeonKey();
                            removedList.Add(it);
                            break;
                        }
                        else if (InventoryManager.AddToInventory(it))
                        {
                            removedList.Add(it);
                            break;
                        }
                    }
                }

                foreach (Item i in removedList)
                {
                    _liItems.Remove(i);
                }
                removedList.Clear();
            }

            if (!rv)
            {
                if (Scrying())
                {
                    GameManager.DropBuilding();
                    LeaveBuildMode();
                    Unpause();
                    Scry(false);
                    ResetCamera();
                }
            }

            return rv;
        }

        /// <summary>
        /// The Map's left-click handler
        /// </summary>
        /// <param name="mouseLocation"></param>
        /// <returns></returns>
        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (!PlayerManager.Busy && !CombatManager.InCombat)
            {
                if (Scrying())
                {
                    rv = ProcessLeftButtonClickHandleBuilding(mouseLocation);
                }
                else
                {
                    //Ensure that we have a tile that we clicked on and that the player is close enough to interact with it.
                    _targetTile = MapManager.RetrieveTile(mouseLocation);
                    if (_targetTile != null && PlayerManager.PlayerInRange(_targetTile.Center.ToPoint()))
                    {
                        //Handles interactions with objects on the tile, both actual and Shadow
                        rv = ProcessLeftButtonClickOnObject(mouseLocation);

                        //Handles interactions with a tile that is actually empty, ignores Shadow Tiles
                        rv = ProcessLeftButtonClickOnEmptyTile(mouseLocation);

                        //Handles interacting with NPCs
                        foreach (WorldActor c in _liActors)
                        {
                            if (c.IsAdventurer())
                            {
                                int row = 0;
                                int col = 0;
                                Adventurer w = (Adventurer)c;
                                if (w.CollisionContains(mouseLocation) && PlayerManager.PlayerInRange(w.CharCenter) &&
                                    InventoryManager.HasSpaceInInventory(w.WhatAreYouHolding(), 1, ref row, ref col, true))
                                {
                                    InventoryManager.AddItemToInventorySpot(DataManager.GetItem(w.TakeItem()), row, col);
                                    rv = true;
                                }
                            }
                            else if (c.IsNPC())
                            {
                                Villager n = (Villager)c;
                                if (InventoryManager.GetCurrentItem() != null &&
                                    n.CollisionContains(mouseLocation) && PlayerManager.PlayerInRange(n.CharCenter) &&
                                    InventoryManager.GetCurrentItem().ItemType != Item.ItemEnum.Tool &&
                                    InventoryManager.GetCurrentItem().ItemType != Item.ItemEnum.Equipment)
                                {
                                    n.Gift(InventoryManager.GetCurrentItem());
                                    rv = true;
                                }
                            }
                        }
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// Handles building interactions. Called after a successful Scrying check in the main handler.
        /// </summary>
        /// <param name="mouseLocation">Location of the mouse</param>
        /// <returns>True if we successfully interacted with a building</returns>
        private bool ProcessLeftButtonClickHandleBuilding(Point mouseLocation)
        {
            bool rv = false;

            //Are we constructing or moving a building?
            if (Constructing() || MovingBuildings())
            {
                //If we are holding a building, we should attempt to drop it.
                if (GameManager.HeldBuilding != null && AddBuilding(GameManager.HeldBuilding, false, false))
                {
                    GameManager.HeldBuilding.StartBuilding();   //Set the building to start being built
                    GUIManager.OpenMainObject(new HUDNamingWindow(GameManager.HeldBuilding));   //Open a naming window
                    GameManager.DropBuilding();                 //Drop the Building from the GameManger

                    //Take the resources from the player if there is merchandise
                    if (gmMerchandise != null)
                    {
                        PlayerManager.TakeMoney(gmMerchandise.MoneyCost);
                        foreach (KeyValuePair<int, int> kvp in gmMerchandise.RequiredItems)
                        {
                            InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                        }

                        gmMerchandise = null;
                    }

                    LeaveBuildMode();
                    rv = true;
                }
                else if (GameManager.HeldBuilding == null)
                {
                    PickUpBuilding(mouseLocation);
                    rv = true;
                }
            }
            else if (DestroyingBuildings())
            {
                rv = RemoveBuilding(mouseLocation);
            }
            else
            {
                if (GraphicCursor.WorkerToPlace > -1)
                {
                    if (AddWorkerToBuilding(mouseLocation))
                    {
                        rv = true;
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// Handles clicking on any objects that may exist on the clicked tile
        /// </summary>
        /// <param name="mouseLocation">Location of the mouse</param>
        /// <returns></returns>
        private bool ProcessLeftButtonClickOnObject(Point mouseLocation)
        {
            bool rv = false;

            //Retrieves any object associated with the tile, this will include
            //both actual tiles, and Shadow Tiles because the user sees Shadow Tiles
            //as being on the tile.
            WorldObject obj = _targetTile.GetWorldObject();
            if (obj != null)
            {
                if (obj.IsMachine())       //Player interacts with a machine to either take a finished item or start working
                {
                    Machine p = (Machine)obj;
                    if (p.HasItem()) { p.TakeFinishedItem(); }
                    else if (!p.Working()) { p.ProcessClick(); }
                }
                else if (obj.IsClassChanger())
                {
                    ((ClassChanger)obj).ProcessClick();
                }
                else if (obj.IsPlant())
                {
                    Plant p = (Plant)obj;
                    Item i = p.Harvest(false);
                    if (i != null)
                    {
                        _liItems.Add(i);
                        MapManager.RemoveWorldObject(p);
                        p.RemoveSelfFromTiles();
                    }   //If we failed to harvest, water the plant if possible
                    else if (_targetTile.Flooring != null && _targetTile.Flooring.IsEarth())
                    {
                        rv = PlayerManager.SetTool(GameManager.ToolEnum.WateringCan, mouseLocation);
                    }
                }
                else if (obj.IsForageable())    //Remove self from the map and harvest the item
                {
                    InventoryManager.AddToInventory(DataManager.GetItem(((Forageable)obj).ForageItem));
                    MapManager.RemoveWorldObject(obj);
                    obj.RemoveSelfFromTiles();
                }
                else if (obj.IsDestructible())  //Handle damaging destructibles
                {
                    Destructible d = (Destructible)_targetTile.GetWorldObject();

                    //Sets the appropriate player tool to use
                    if (d.WhichTool == ToolEnum.Pick) { rv = PlayerManager.SetTool(GameManager.ToolEnum.Pick, mouseLocation); }
                    else if (d.WhichTool == ToolEnum.Axe) { rv = PlayerManager.SetTool(GameManager.ToolEnum.Axe, mouseLocation); }
                    else if (d.WhichTool == ToolEnum.Lantern) { rv = PlayerManager.SetTool(GameManager.ToolEnum.Lantern, mouseLocation); }
                }
            }

            return rv;
        }

        /// <summary>
        /// Handles clicking on tiles that have nothing actually set to them.
        /// This ignores Shadow Tiles.
        /// </summary>
        /// <param name="mouseLocation">Location of the mouse</param>
        /// <returns></returns>
        private bool ProcessLeftButtonClickOnEmptyTile(Point mouseLocation)
        {
            bool rv = false;

            //Get any actual tiles, the false excludes Shadow Tiles
            WorldObject obj = _targetTile.GetWorldObject(false);
            if (obj == null)    //Only procees if tile is empty
            {
                //If the player is currently holding a StaticItem, we need to place it
                //Do not, however, allowt he placing of StaticItems on combat maps.
                StaticItem selectedItem = InventoryManager.GetCurrentStaticItem();
                if (!IsCombatMap && selectedItem != null)
                {
                    //Take the actual WorldObject item from the item and attempt to place it on the map
                    WorldItem newItem = selectedItem.GetWorldItem();
                    if (MapManager.PlacePlayerObject(newItem))
                    {
                        newItem.SetMapName(this.Name);      //Assign the map name tot he WorldItem
                        selectedItem.Remove(1);             //Remove one of them from the inventory

                        //If the item placed was a wall object, we need to adjust it based off any adjacent walls
                        if (newItem.Type == WorldObject.ObjectType.Wall)
                        {
                            ((Wall)newItem).AdjustObject();
                        }
                    }
                }
                else if (_targetTile.CanDig())      //If you can dig, set the shovel
                {
                    rv = PlayerManager.SetTool(GameManager.ToolEnum.Shovel, mouseLocation);
                }
            }

            return rv;
        }

        public bool ProcessHover(Point mouseLocation)
        {
            bool rv = false;

            if (_liTestTiles.Count > 0) { _liTestTiles.Clear(); }

            if (Scrying())
            {
                Building building = GameManager.HeldBuilding;
                _liTestTiles = new List<RHTile>();
                if (building != null)
                {
                    TestMapTiles(building, _liTestTiles);
                }

                foreach (Building b in _liBuildings)
                {
                    if (!b.IsManor)
                    {
                        if (b.SelectionBox.Contains(mouseLocation) && GameManager.HeldBuilding == null)
                        {
                            b._selected = true;
                        }
                        else
                        {
                            b._selected = false;
                        }
                    }
                }
            }
            else{
                bool found = false;

                RHTile t = GetTileByPixelPosition(GraphicCursor.GetWorldMousePosition().ToPoint());
                if(t != null && t.GetTravelPoint() != null)
                {
                    found = true;
                    GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Door;
                    GraphicCursor.Alpha = (PlayerManager.PlayerInRange(t.GetTravelPoint().TravelPointInfo.CollisionBox) ? 1 : 0.5f);
                }

                foreach (WorldActor c in _liActors)
                {
                    if(!c.IsMonster() && c.HoverContains(mouseLocation)){
                        if (c.Active)
                        {
                            GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Talk;
                            found = true;
                            break;
                        }
                    }
                }

                //Do not draw test tiles on a map for combat
                StaticItem selectedStaticItem = InventoryManager.GetCurrentStaticItem();
                if (!IsCombatMap && selectedStaticItem != null)
                {
                    Vector2 vec = mouseLocation.ToVector2() - new Vector2(selectedStaticItem.GetWorldItem().Width / 4, selectedStaticItem.GetWorldItem().Height - selectedStaticItem.GetWorldItem().BaseHeight);
                    selectedStaticItem.SetWorldObjectCoords(vec);
                    TestMapTiles(selectedStaticItem.GetWorldItem(), _liTestTiles);
                }
                if (!found)
                {
                    GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Normal;
                }
            }

            return rv;
        }
        #endregion

        public void ClearWorkers()
        {
            _liActors.Clear();
        }

        public void RemoveWorldObject(WorldObject o)
        {
            if (_liPlacedWorldObjects.Contains(o))
            {
                _liPlacedWorldObjects.Remove(o);
            }

            //if (tile.Flooring == o)
            //{
            //    tile.RemoveFlooring();

            //}
            //if (tile.Flooring == null && tile.WorldObject == null)
            //{
            //    toRemove.Add(tile);
            //}

            //foreach (RHTile tile in toRemove)
            //{
            //    _liModifiedTiles.Remove(tile);
            //}
        }
        public void RemoveCharacter(WorldActor c)
        {
            ToRemove.Add(c);
        }
        public void RemoveMonster(Monster m)
        {
            ToRemove.Add(m);
        }
        public void DropItemsOnMap(List<Item>items, Vector2 position, bool flyingPop = true)
        {
            foreach(Item i in items)
            {
                DropItemOnMap(i, position, flyingPop);
            }
        }

        public void DropItemOnMap(Item item, Vector2 position, bool flyingPop = true)
        {
            item.Pop(position, flyingPop);

            item.OnTheMap = true;
            _liItems.Add(item);
        }

        /// <summary>
        /// When we enter a Building, we may need to load up whatever has been saved to the Building.
        /// This is because non unique buildings like Arcane Tower, Barracks, etc all use the same map,
        /// but different specific buildings will have different content.
        /// 
        /// If the building is Unique, do not modify the map at all as everything in it will
        /// be saved to the map itself.
        /// </summary>
        /// <param name="b">The building to load</param>
        public void LoadBuilding(Building b)
        {
            if (!b.Unique)
            {
                ClearWorkers();
                AddBuildingObjectsToMap(b);
            }
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
        public void AddBuildingObjectsToMap(Building b)
        {
            ReadOnlyCollection<TiledMapObjectLayer> entrLayer = _map.ObjectLayers;
            foreach (TiledMapObjectLayer ol in entrLayer)
            {
                if (ol.Name == "MapObject Layer")
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        if (mapObject.Name.Contains("BuildingChest"))
                        {
                            b.BuildingChest.SetCoordinatesByGrid(mapObject.Position);
                            PlacePlayerObject(b.BuildingChest);
                        }
                        else if (mapObject.Name.Contains("Pantry"))
                        {
                            b.Pantry.SetCoordinatesByGrid(mapObject.Position);
                            PlacePlayerObject(b.Pantry);
                        }
                    }
                }
            }
            for (int i = 0; i < b.Workers.Count; i++)
            {
                b.Workers[i].Position = GetCharacterSpawn("WSpawn"+i);
                b.Workers[i].CurrentMapName = _sName;
                _liActors.Add(b.Workers[i]);
            }
            foreach (WorldObject w in b.PlacedObjects)
            {
                PlacePlayerObject(w);
            }
        }

        public void PickUpBuilding(Point mouseLocation)
        {
            foreach (Building b in _liBuildings)
            {
                if (!b.IsManor)
                {
                    if (b.Contains(mouseLocation))
                    {
                        GameManager.PickUpBuilding(b);
                        b.RemoveSelfFromTiles();
                        _diTravelPoints.Remove(b.PersonalID.ToString());
                        break;
                    }
                }
            }
        }

        public bool RemoveBuilding(Point mouseLocation)
        {
            bool rv = false;
            Building bldg = null;
            foreach (Building b in _liBuildings)
            {
                if (!b.IsManor)
                {
                    if (b.Contains(mouseLocation))
                    {
                        if (b.Workers.Count == 0)
                        {
                            bldg = b;
                            b.RemoveSelfFromTiles();
                            _diTravelPoints.Remove(b.PersonalID.ToString());
                            PlayerManager.RemoveBuilding(b);
                            LeaveBuildMode();
                            Unpause();
                            Scry(false);
                            ResetCamera();
                        }
                        else
                        {
                            GUIManager.OpenTextWindow(DataManager.GetGameText("Cannot Destroy occupied buildings."));
                            //GUIManager.SetScreen(new TextScreen("Cannot Destroy occupied buildings.", false));
                        }
                    }
                }
            }
            if(bldg != null) {
                rv = true;
                _liBuildings.Remove(bldg);
            }

            return rv;
        }

        public bool AddBuilding(Building b, bool placeImmediately = false, bool createEntrance = true)
        {
            bool rv = false;
            List<RHTile> tiles = new List<RHTile>();
            if (TestMapTiles(b, tiles))
            {
                _liTestTiles.Clear();
                b.SetTiles(tiles);

                if (placeImmediately)
                {
                    AssignMapTiles(b, b.Tiles);
                }

                b.SetHomeMap(this.Name);
                //Only create the entrance is the bool is set
                if (createEntrance){
                    CreateBuildingEntrance(b);
                }

                if (!_liBuildings.Contains(b)) //For the use case of moving buildings
                { 
                    _liBuildings.Add(b);
                } 
                PlayerManager.AddBuilding(b);
                
                rv = true;
            }

            return rv;
        }

        public void CreateBuildingEntrance(Building b)
        {
            TravelPoint buildPoint = new TravelPoint(b.TravelBox, b.MapName, b.PersonalID);
            _diTravelPoints.Add(b.MapName, buildPoint); //TODO: FIX THIS
            CreateDoor(ref buildPoint, b.TravelBox.X, b.TravelBox.Y, b.TravelBox.Width, b.TravelBox.Height);
        }

        public bool AddWorkerToBuilding(Point mouseLocation)
        {
            bool rv = false;
            foreach (Building b in _liBuildings)
            {
                if (!b.IsManor)
                {
                    if (b.SelectionBox.Contains(mouseLocation))
                    {
                        if (b.HasSpace())
                        {
                            Adventurer w = DataManager.GetAdventurer(GraphicCursor.WorkerToPlace);
                            b.AddWorker(w);
                            b._selected = false;
                            GUIManager.OpenMainObject(new HUDNamingWindow(w));
                            //Scry(false);
                            rv = true;
                        }
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
            
            if (!rv && bounce)
            {
                Vector2 position = o.MapPosition;
                do
                {
                    position.X = (int)(RHRandom.Instance.Next(1, (MapWidthTiles - 1) * TileSize) / TileSize) * TileSize;
                    position.Y = (int)(RHRandom.Instance.Next(1, (MapHeightTiles - 1) * TileSize) / TileSize) * TileSize;
                    o.SetCoordinatesByGrid(position);

                    rv = TestMapTiles(o, tiles);
                } while (!rv);
            }

            if (rv)
            {
                AssignMapTiles(o, tiles);
            }

            return rv;
        }

        /// <summary>
        /// Helper for TestMapTiles for programatic assignation of objects
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool TestMapTiles(WorldObject o)
        {
            return TestMapTiles(o, _liTestTiles);
        }

        /// <summary>
        /// Given a World Object item, determine which tiles on the map collide with
        /// the defined CollisionBox of the WorldObject.
        /// </summary>
        /// <param name="o">The WorldObject to test</param>
        /// <param name="collisionTiles">The Tiles on the map that are in the object's CollisionBox</param>
        /// <returns></returns>
        public bool TestMapTiles(WorldObject o, List<RHTile> collisionTiles)
        {
            bool rv = true;
            collisionTiles.Clear();
            Vector2 position = o.MapPosition;
            position.X = ((int)(position.X / TileSize)) * TileSize;
            position.Y = ((int)(position.Y / TileSize)) * TileSize;

            int colColumns = o.CollisionBox.Width / TileSize;
            int colRows = o.CollisionBox.Height / TileSize;
            
            //This is used to get all the tiles based off the collisonbox size
            for (int i = 0; i < colRows; i++)
            {
                for (int j = 0; j < colColumns; j++)
                {
                    int x = Math.Min((o.CollisionBox.Left + (j * TileSize)) / TileSize, MapWidthTiles-1);
                    int y = Math.Min((o.CollisionBox.Top + (i * TileSize)) / TileSize, MapHeightTiles-1);
                    if (x < 0 || x > this.MapWidthTiles || y < 0 || y > this.MapHeightTiles)
                    {
                        rv = false;
                        break;
                    }

                    RHTile tempTile = _arrTiles[x, y];
                    if ((!o.WallObject && tempTile.Passable() && tempTile.WorldObject == null) || (o.WallObject && tempTile.IsValidWall()))
                    {
                        collisionTiles.Add(tempTile);
                    }
                    else
                    {
                        collisionTiles.Add(tempTile);
                        rv = false;
                    }
                }
            }

            return rv;
        }


        /// <summary>
        /// Assigns the Tiles that a WorldObject will occupy to the object, adds the
        /// object to the list of placed player objects, and also sets the world object
        /// to the tiles that it belongs to. Both must know each other
        /// 
        /// After setting actual Tiles, determine if there are any Shadow Tiles that must
        /// be added, and set those as well. Shadow Tiles are tiles that the image is on
        /// a part of, but is not technically sitting on.
        /// </summary>
        /// <param name="o">The object to add</param>
        /// <param name="tiles">The list of tiles to add to the object</param>
        public void AssignMapTiles(WorldObject o, List<RHTile> tiles)
        {
            //Call AddRange to ensure that it has the actual tiles, and isn't just a copy of the list
            o.Tiles.AddRange(tiles);                    

            //Adds the object to the list of player objects
            if (!_liPlacedWorldObjects.Contains(o))
            {
                _liPlacedWorldObjects.Add(o);
            }

            //Sets the WorldObject to each RHTile
            foreach (RHTile t in tiles)
            {
                t.SetObject(o);
            }

            //Iterate over the WorldObject image in TileSize increments to discover any tiles
            //that the image overlaps. Add those tiles as Shadow Tiles as long as they're not
            //actual Tiles the object sits on. Also add the Tiles to the objects Shadow Tiles list
            for (int i = (int)o.MapPosition.X; i < o.MapPosition.X + o.Width; i += TileSize)
            {
                for (int j = (int)o.MapPosition.Y; j < o.MapPosition.Y + o.Height; j += TileSize)
                {
                    RHTile t = GetTileByGridCoords(Util.GetGridCoords(i, j));
                    if (t != null && !o.Tiles.Contains(t))
                    {
                        t.SetShadowObject(o);
                        o.ShadowTiles.Add(t);
                    }
                }
            }
        }

        public bool PlacePlayerObject(WorldObject obj)
        {
            bool rv = false;
            List<RHTile> liTiles = new List<RHTile>();
            liTiles.AddRange(_liTestTiles);
            if (liTiles.Count == 0)
            {
                for (int x = obj.CollisionBox.X; x < obj.CollisionBox.X + obj.CollisionBox.Width; x += TileSize)
                {
                    for (int y = obj.CollisionBox.Y; y < obj.CollisionBox.Y + obj.CollisionBox.Height; y += TileSize)
                    {
                        liTiles.Add(GetTileByPixelPosition(x, y));
                    }
                }
            }

            bool placeIt = true;
            foreach (RHTile t in liTiles)
            {
                if (!t.Passable())
                {
                    placeIt = false;
                    break;
                }
            }

            if (placeIt)
            {
                AssignMapTiles(obj, liTiles);
                rv = true;
            }

            return rv;
        }

        public void AddCharacter(WorldActor c)
        {
            ToAdd.Add(c);
        }

        public bool RemoveCharacterImmediately(WorldActor c)
        {
            bool rv = false;
            if (MapManager.Maps[c.CurrentMapName].Contains(c))
            {
                rv = true;
                if (c.IsMonster() && _liMonsters.Contains((Monster)c)) { _liMonsters.Remove((Monster)c); }
                else if (_liActors.Contains(c)) { _liActors.Remove(c); }
            }

            return rv;
        }
        /// <summary>
        /// Use this only during loading periods when the maps will not be calling Update.
        /// Otherwise we crash :D
        /// 
        /// NewMapPosition is used when we ChangeMaps.
        /// </summary>
        /// <param name="c">The WorldActor to add</param>
        /// <returns>True if successful</returns>
        public bool AddCharacterImmediately(WorldActor c)
        {
            bool rv = false;
            if (!MapManager.Maps[c.CurrentMapName].Contains(c))
            {
                rv = true;
                if (c.IsMonster() && !_liMonsters.Contains((Monster)c)) { _liMonsters.Add((Monster)c); }
                else if (!_liActors.Contains(c)) { _liActors.Add(c); }
                c.CurrentMapName = _sName;
                c.Position = c.NewMapPosition == Vector2.Zero ? c.Position : c.NewMapPosition;
                c.NewMapPosition = Vector2.Zero;
            }

            return rv;
        }

        public void AddMonster(Monster m)
        {
            bool rv = false;

            RHRandom rand = RHRandom.Instance;
            Vector2 position = m.Position;
            position.X = ((int)(position.X / TileSize)) * TileSize;
            position.Y = ((int)(position.Y / TileSize)) * TileSize;

            rv = _arrTiles[((int)position.X / TileSize), ((int)position.Y / TileSize)].Passable();
            if (!rv)
            {
                do
                {
                    position.X = (int)(rand.Next(1, (MapWidthTiles - 1) * TileSize) / TileSize) * TileSize;
                    position.Y = (int)(rand.Next(1, (MapHeightTiles - 1) * TileSize) / TileSize) * TileSize;
                    rv = _arrTiles[((int)position.X / TileSize), ((int)position.Y / TileSize)].Passable();
                } while (!rv);
            }

            if (rv)
            {
                AddMonsterByPosition(m, position);
            }
        }

        public void AddMonsterByPosition(Monster m, Vector2 position)
        {
            m.CurrentMapName = _sName;
            m.Position = Util.SnapToGrid(position);

            _liMonsters.Add(m);
        }
        #endregion
        
        public int GetMapWidth()
        {
            return MapWidthTiles * TileSize * (int)Scale;
        }

        public int GetMapHeight()
        {
            return MapHeightTiles * TileSize * (int)Scale;
        }

        public void CheckForTriggeredCutScenes()
        {
            foreach(int cutsceneID in _liCutscenes)
            {
                CutsceneManager.CheckForTriggedCutscene(cutsceneID);
            }
        }

        /// <summary>
        /// Search through this map for any trigger objects and activate them as appropriate.
        /// </summary>
        /// <param name="triggerName">Name of the trigger to activate</param>
        public void Trigger(string triggerName)
        {
            foreach(DungeonObject obj in _liDungeonObjects)
            {
                obj.Trigger(triggerName);
            }
        }

        public RHTile GetTileByPixelPosition(Vector2 targetLoc)
        {
            return GetTileByPixelPosition((int)targetLoc.X, (int)targetLoc.Y);
        }
        public RHTile GetTileByPixelPosition(Point targetLoc)
        {
            return GetTileByPixelPosition(targetLoc.X, targetLoc.Y);
        }
        public RHTile GetTileByPixelPosition(int x, int y)
        {
            if (x >= GetMapWidth() || x < 0) { return null; }
            if (y >= GetMapHeight() || y < 0) { return null; }

            try
            {
                return _arrTiles[x / TileSize, y / TileSize];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public RHTile GetTileByGridCoords(Point targetLoc)
        {
            return GetTileByGridCoords(targetLoc.ToVector2());
        }
        public RHTile GetTileByGridCoords(Vector2 pos)
        {
            return GetTileByGridCoords((int)pos.X, (int)pos.Y);
        }
        public RHTile GetTileByGridCoords(int x, int y)
        {
            RHTile tile = null;

            if(x >= 0 && x < MapWidthTiles && y >= 0 && y < MapHeightTiles)
            {
                tile = _arrTiles[x, y];
            }

            return tile;
        }

        internal MapData SaveData()
        {
            MapData mapData = new MapData
            {
                mapName = this.Name,
                worldObjects = new List<WorldObjectData>(),
                containers = new List<ContainerData>(),
                machines = new List<MachineData>(),
                plants = new List<PlantData>(),
                floors = new List<FloorData>(),
                earth = new List<FloorData>()
            };

            if (!this.IsBuilding)
            {
                foreach (WorldObject wObj in _liPlacedWorldObjects)
                {
                    if (wObj.IsMachine())
                    {
                        mapData.machines.Add(((Machine)wObj).SaveData());
                    }
                    else if (wObj.IsContainer())
                    {
                        mapData.containers.Add(((Container)wObj).SaveData());
                    }
                    else if (wObj.IsPlant())
                    {
                        mapData.plants.Add(((Plant)wObj).SaveData());
                    }
                    else if (wObj.IsClassChanger())
                    {
                        WorldObjectData d = new WorldObjectData
                        {
                            worldObjectID = wObj.ID,
                            x = (int)((ClassChanger)wObj).MapPosition.X,
                            y = (int)((ClassChanger)wObj).MapPosition.Y
                        };
                        mapData.worldObjects.Add(d);
                    }
                    else
                    {
                        WorldObjectData d = new WorldObjectData
                        {
                            worldObjectID = wObj.ID,
                            x = (int)wObj.MapPosition.X,
                            y = (int)wObj.MapPosition.Y
                        };
                        mapData.worldObjects.Add(d);
                    }
                }
                foreach (RHTile tile in TilledTiles)
                {
                    Floor f = tile.Flooring;
                    if(f != null)
                    {
                        if (f.IsEarth())
                        {
                            Earth e = (Earth)f;
                            mapData.earth.Add(e.SaveData());
                        }
                        else
                        {
                            mapData.floors.Add(f.SaveData());
                        }
                    }
                }
            }

            return mapData;
        }
        internal void LoadData(MapData data)
        {
            foreach (WorldObjectData w in data.worldObjects)
            {
                if (w.worldObjectID != -1)
                {
                    if(w.worldObjectID == 240)
                    {
                        int i = 0;
                    }
                    WorldObject obj = DataManager.GetWorldObject(w.worldObjectID, new Vector2(w.x, w.y));
                    if(obj.Type == WorldObject.ObjectType.Wall || obj.Type == WorldObject.ObjectType.Floor)
                    {
                        if (PlacePlayerObject(obj))
                        {
                            ((AdjustableObject)obj).SetMapName(this.Name);
                            ((AdjustableObject)obj).AdjustObject();
                        }
                    }

                    if (obj.IsClassChanger())
                    {
                        PlacePlayerObject(obj);
                    }
                    else
                    {
                        PlaceWorldObject(obj);
                    }
                    
                }
            }
            foreach (ContainerData c in data.containers)
            {
                Container con = (Container)DataManager.GetWorldObject(c.containerID);
                con.LoadData(c);
                PlacePlayerObject(con);
            }
            foreach (MachineData mac in data.machines)
            {
                Machine theMachine = (Machine)DataManager.GetWorldObject(mac.ID);
                theMachine.LoadData(mac);
                PlacePlayerObject(theMachine);
            }
            foreach (PlantData plantData in data.plants)
            {
                Plant plant = (Plant)DataManager.GetWorldObject(plantData.ID);
                plant.LoadData(plantData);
                PlacePlayerObject(plant);
            }
            foreach (FloorData floorData in data.floors)
            {
                //Floor floor = (Floor)ObjectManager.GetWorldObject(plantData.ID);
                //plant.LoadData(plantData);
                //PlacePlayerObject(plant);
            }
            foreach (FloorData earthData in data.earth)
            {
                Earth e = new Earth();
                e.LoadData(earthData);
                RHTile tile = _arrTiles[(int)e.MapPosition.X / TileSize, (int)e.MapPosition.Y / TileSize];
                tile.SetFloor(e);
                _liTilledTiles.Add(tile);
            }
        } 
    }

    public abstract class SpawnPoint
    {
        protected RHMap _map;
        protected Vector2 _vPosition;

        protected SpawnPoint(RHMap map, TiledMapObject obj)
        {
            _map = map;
            _vPosition = map.GetTileByGridCoords(Util.GetGridCoords(obj.Position)).Center;
        }

        public virtual void Spawn() { }
        public virtual bool HasSpawned() { return false; }
    }

    public class ResourceSpawn : SpawnPoint
    {
        int _iWidth;
        int _iHeight;
        public int Size => _iWidth * _iHeight;
        List<int> _liPossibleResources;
        //List<WorldObject> _liResources;
        public ResourceSpawn(RHMap map, TiledMapObject obj) : base(map,obj)
        {
            _liPossibleResources = new List<int>();
            _iWidth = (int)obj.Size.Width;
            _iHeight = (int)obj.Size.Height;
            if (obj.Properties.ContainsKey("Resources"))
            {
                string[] spawnResources = obj.Properties["Resources"].Split('-');
                foreach (string s in spawnResources)
                {
                    _liPossibleResources.Add(int.Parse(s));
                }
            }
        }

        public override void Spawn()
        {
            //ToDo //Give ResourceSpawns an internal array corresponding to the size and remove objects
            //fromt he array as it gets filled so that we bounce less.
            int xPoint = RHRandom.Instance.Next((int)(_vPosition.X), (int)(_vPosition.X + _iWidth));
            int yPoint = RHRandom.Instance.Next((int)(_vPosition.Y), (int)(_vPosition.Y + _iHeight));

            WorldObject wObj = DataManager.GetWorldObject(_liPossibleResources[RHRandom.Instance.Next(0, _liPossibleResources.Count-1)]);
            wObj.SetCoordinatesByGrid(new Vector2(xPoint, yPoint));

            if (wObj.IsPlant())
            {
                ((Plant)wObj).FinishGrowth();
            }
            _map.PlaceWorldObject(wObj, false);
        }

        public override bool HasSpawned()
        {
            return true;//_monster != null;
        }
    }

    public class MonsterSpawn : SpawnPoint
    {
        Monster _monster;
        SpawnConditionEnum _eSpawnType = SpawnConditionEnum.Forest;

        public MonsterSpawn(RHMap map, TiledMapObject obj) : base(map, obj)
        {
            _eSpawnType = Util.ParseEnum<SpawnConditionEnum>(obj.Properties["SpawnType"]);
        }

        public override void Spawn()
        {
            _monster = DataManager.GetMonsterByIndex(RHRandom.Instance.Next(1,4));
            if (_monster != null)
            {
                _map.AddMonsterByPosition(_monster, _vPosition);
            }
        }

        public override bool HasSpawned()
        {
            return _monster != null;
        }
    }

    public class RHTile
    {
        string _mapName;
        public string MapName => _mapName;
        bool _tileExists;
        int _X;
        public int X => _X;
        int _Y;
        public int Y => _Y;
        public Vector2 Position => new Vector2(_X * TileSize, _Y * TileSize);
        public Vector2 Center => new Vector2(Position.X + TileSize/2, Position.Y + TileSize/2);
        public Rectangle Rect => Util.FloatRectangle(Position, TileSize, TileSize);

        TileObject _tileMapObj;
        CombatActor _combatActor;
        public CombatActor Character => _combatActor;

        Dictionary<TiledMapTileLayer, Dictionary<string, string>> _diProps;
        WorldObject _obj;
        public WorldObject WorldObject => _obj;
        //A WorldObject that doesn't live on this tile but is drawn over it.
        WorldObject _shadowObj;
        public WorldObject ShadowObject => _shadowObj;

        Floor _floorObj;
        public Floor Flooring => _floorObj;

        bool _isRoad;
        public bool IsRoad => _isRoad;

        bool _bArea = false;
        bool _bSelected = false;
        bool _bLegalTile = false;

        public RHTile(int x, int y, string mapName)
        {
            _X = x;
            _Y = y;

            _mapName = mapName;
            _diProps = new Dictionary<TiledMapTileLayer, Dictionary<string, string>>();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Rectangle dest = new Rectangle((int)Position.X, (int)Position.Y, TileSize, TileSize);

            if (CombatManager.InCombat)
            {
                //Only draw one of the tile targetting types
                if (this == CombatManager.ActiveCharacter?.BaseTile && DisplaySelectedTile()) { spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS), dest, new Rectangle(48, 112, 16, 16), Color.White); }
                else if (_bSelected) { spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS), dest, new Rectangle(16, 112, 16, 16), Color.White); }
                else if (_bArea) { spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS), dest, new Rectangle(32, 112, 16, 16), Color.White); }
                else if (_bLegalTile) { spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS), dest, new Rectangle(0, 112, 16, 16), Color.White); }
            }

            if (_floorObj != null) { _floorObj.Draw(spriteBatch); }
            if (_obj != null) { _obj.Draw(spriteBatch); }
        }

        private bool DisplaySelectedTile()
        {
            return CombatPhaseCheck(CmbtPhaseEnum.ChooseActionTarget) || CombatPhaseCheck(CmbtPhaseEnum.ChooseMoveTarget) || CombatPhaseCheck(CmbtPhaseEnum.MainSelection);
        }

        public void Dig()
        {
            if (_floorObj == null)
            {
                SetFloor(new Earth());
                if (GameCalendar.IsRaining())
                {
                    Water(true);
                }
            }
            else
            {
                _floorObj = null;
            }
        }

        public bool SetFloor(Floor f)
        {
            bool rv = false;
            if (_floorObj == null && Passable())
            {
                rv = true;
                _floorObj = f;
                _floorObj.MapPosition = Position;
                _floorObj.SetMapName(MapName);
                _floorObj.Tiles.Clear();
                _floorObj.Tiles.Add(this);
                _floorObj.AdjustObject();
            }

            return rv;
        }
        public void Water(bool value)
        {
            if (_floorObj != null && _floorObj.IsEarth())
            {
                Earth e = (Earth)_floorObj;
                e.Watered(value);
            }
        }
        public bool IsWatered()
        {
            bool rv = false;
            if (_floorObj != null && _floorObj.IsEarth())
            {
                rv = ((Earth)_floorObj).Watered();
            }

            return rv;
        }
        public bool HasBeenDug()
        {
            return _floorObj != null && _floorObj.IsEarth();
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
                        _diProps.Add(l, map.GetProperties((TiledMapTile)tile));
                    }
                }
            }
            _isRoad = ContainsProperty("Road", out string value) && value.Equals("true");
        }

        /// <summary>
        /// Retrieves the WorldObject on the Tile. If the parameter is false,
        /// it will not return the shadow object
        /// </summary>
        /// <param name="AlsoCheckShadow">Whether or not to return a shadow object</param>
        /// <returns>The relevant associated WorldObject</returns>
        public WorldObject GetWorldObject(bool AlsoCheckShadow = true)
        {
            WorldObject obj = null;

            if(_obj != null) { obj = _obj; }
            else if(AlsoCheckShadow) { obj = _shadowObj; }

            return obj;
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
        public void RemoveWorldObject()
        {
            _obj = null;
        }
        public void RemoveShadowObject()
        {
            _shadowObj = null;
        }
        public void RemoveFlooring()
        {
            _floorObj = null;
        }
        public bool SetObject(WorldObject o)
        {
            bool rv = false;
            if (o.Type == WorldObject.ObjectType.Floor)
            {
                rv = SetFloor((Floor)o);
            }
            else if ((!o.WallObject && Passable()) || (o.WallObject && IsValidWall()))
            {
                _obj = o;
                rv = true;
            }
            return rv;
        }
        public bool SetShadowObject(WorldObject o)
        {
            bool rv = false;
            if ((!o.WallObject && Passable()) || (o.WallObject && IsValidWall()))
            {
                _shadowObj = o;
                rv = true;
            }
            return rv;
        }
        public Floor GetFloorObject()
        {
            Floor f = null;

            if (_floorObj != null) { f = _floorObj; }

            return f;
        }

        public bool CanDig()
        {
            bool rv = false;
            foreach (TiledMapTileLayer l in _diProps.Keys)
            {
                if (l.IsVisible && ContainsProperty(l, "CanDig", out string val) && val.Equals("true"))
                {
                    rv = true;
                }
            }

            return rv;
        }

        public bool IsValidWall()
        {
            bool rv = false;
            if (_tileExists && _obj == null)
            {
                foreach (TiledMapTileLayer l in _diProps.Keys)
                {
                    if (l.IsVisible && ContainsProperty(l, "Impassable", out string val) && val.Equals("true"))
                    {
                        rv = true;
                    }
                }
            }
            return rv;
        }

        public void SetMapObject(Building b)
        {
            _tileMapObj = new TileObject(b);
        }

        public void SetMapObject(TravelPoint obj)
        {
            _tileMapObj = new TileObject(obj);
        }

        public TileObject GetTravelPoint()
        {
            return _tileMapObj;
        }

        public bool Contains(Villager n)
        {
            bool rv = false;

            rv = Rect.Contains(n.CollisionBox.Center);

            return rv;
        }
        public bool ContainsProperty(string property, out string value)
        {
            bool rv = false;
            value = string.Empty;
            foreach (TiledMapTileLayer l in _diProps.Keys)
            {
                rv = ContainsProperty(l, property, out value);
                if (rv) { break; }
            }

            return rv;
        }
        public bool ContainsProperty(TiledMapTileLayer l, string property, out string value)
        {
            bool rv = false;
            value = string.Empty;
            if (_diProps.ContainsKey(l) && _diProps[l].ContainsKey(property))
            {
                value = _diProps[l][property];
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
                    MapManager.DropItemsOnMap(_obj.GetDroppedItems(), _obj.CollisionBox.Center.ToVector2());
                    MapManager.RemoveWorldObject(_obj);
                    _obj.RemoveSelfFromTiles();
                }
            }

            return rv;
        }

        public void Clear()
        {
            _obj = null;
            _floorObj = null;
        }

        public void Rollover()
        {
            if (_obj != null && _obj.IsPlant())
            {
                ((Plant)_obj).Rollover();
            }
            if (_floorObj != null && _floorObj.IsEarth())
            {
                ((Earth)_floorObj).Watered(false);
            }
        }

        /// <summary>
        /// Returns if the tile has a CombatActor assigned to it. 
        /// </summary>
        public bool HasCombatant()
        {
            return _combatActor != null;
        }

        /// <summary>
        /// Assigns a CombatActor to the RHTile
        /// </summary>
        /// <param name="c">The combatant to set to this tile</param>
        public void SetCombatant(CombatActor c)
        {
            _combatActor = c;
        }

        /// <summary>
        /// Sets the selected value of the RHTile
        /// </summary>
        /// <param name="val">Whether to set or unset the selected value</param>
        public void Select(bool val)
        {
            _bSelected = val;
        }

        /// <summary>
        /// Sets the legal value of the RHTile
        /// </summary>
        /// <param name="val">Whether to set or unset the selected value</param>
        public void LegalTile(bool val)
        {
            _bLegalTile = val;
        }

        /// <summary>
        /// Sets the area value of the RHTile
        /// </summary>
        /// <param name="val">Whether to set or unset the area value</param>
        public void AreaTile(bool val)
        {
            _bArea = val;
        }

        #region TileTraversal
        private RHMap MyMap()
        {
            return MapManager.Maps[MapName];
        }

        public List<RHTile> GetWalkableNeighbours()
        {
            List<RHTile> rvList = new List<RHTile>();
            foreach (RHTile tile in GetAdjacentTiles())
            {
                if (tile != null && tile.CanWalkThrough())
                {
                    rvList.Add(tile);
                }
            }

            return rvList;
        }

        /// <summary>
        /// Returns a list of all RHTiles adjacent to this tile
        /// </summary>
        /// <returns></returns>
        public List<RHTile> GetAdjacentTiles()
        {
            List<RHTile> adj = new List<RHTile>();

            //Have to null check
            RHTile temp = GetTileByDirection(DirectionEnum.Up);
            if (temp != null) { adj.Add(temp); }
            temp = GetTileByDirection(DirectionEnum.Down);
            if (temp != null) { adj.Add(temp); }
            temp = GetTileByDirection(DirectionEnum.Left);
            if (temp != null) { adj.Add(temp); }
            temp = GetTileByDirection(DirectionEnum.Right);
            if (temp != null) { adj.Add(temp); }

            return adj;
        }

        /// <summary>
        /// Returns the tile on the TileMap in the given direction from the MapTile
        /// </summary>
        /// <param name="t">The direction to look in</param>
        /// <returns>The Tile if it exists, or null</returns>
        public RHTile GetTileByDirection(DirectionEnum t)
        {
            RHTile rvTile = null;
            switch (t)
            {
                case DirectionEnum.Down:
                    if (this.Y < MyMap().MapHeightTiles - 1)
                    {
                        rvTile = MyMap().GetTileByGridCoords(this.X, this.Y + 1);
                    }
                    break;
                case DirectionEnum.Left:
                    if (this.X > 0)
                    {
                        rvTile = MyMap().GetTileByGridCoords(this.X - 1, this.Y);
                    }
                    break;
                case DirectionEnum.Up:
                    RHTile rv = null;
                    if (this.Y > 0)
                    {
                        rvTile = MyMap().GetTileByGridCoords(this.X, this.Y - 1);
                    }
                    break;
                case DirectionEnum.Right:
                    if (this.X < MyMap().MapWidthTiles - 1)
                    {
                        rvTile = MyMap().GetTileByGridCoords(this.X + 1, this.Y);
                    }
                    break;
            }


            return rvTile;
        }

        /// <summary>
        /// Returns whether the RHTile itself can be walked through or not. This relates
        /// to the core status of the tile, and has nothing to do with objects or actors on it.
        /// </summary>
        /// <returns>True if the tile is not itself locked down</returns>
        public bool Passable()
        {
            bool rv = _tileExists && (_obj == null || !_obj.Blocking);
            if (_tileExists)
            {
                foreach (TiledMapTileLayer l in _diProps.Keys)
                {
                    if (l.IsVisible && ContainsProperty(l, "Impassable", out string val) && val.Equals("true"))
                    {
                        rv = false;
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// This method defines which RHTiles are allowed to be the target of abilities and spells.
        /// You cannot use skills on an RHTile that is impassable, or occupied by an object.
        /// </summary>
        /// <returns>Returns True if the Tile is a legal tile to target</returns>
        public bool CanTargetTile()
        {
            return Passable() && GetTravelPoint() == null && WorldObject == null;
        }

        /// <summary>
        /// Determines if the RHTile can be walked through. 
        /// </summary>
        /// <returns>True if the RHTile can be walked through. Does not mean the RHTile
        /// can be assigned to if it returns true.</returns>
        public bool CanWalkThrough()
        {
            return CanTargetTile() && CanWalkThroughInCombat();
        }

        /// <summary>
        /// For use only during Combat to see if can path through.
        /// Characters can path through tiles occupied by an ally.
        /// </summary>
        /// <returns>True if not in combat, or character is null</returns>
        public bool CanWalkThroughInCombat()
        {
            bool rv = true;
            if (CombatManager.InCombat)
            {
                rv = Character == null || CombatManager.OnSameTeam(Character);
            }
            return rv;
        }
        #endregion

        public class TileObject
        {
            Building _building;
            public Building PlayerBuilding => _building;
            TravelPoint _travelPoint;
            public TravelPoint TravelPointInfo => _travelPoint;

            public TileObject(Building b)
            {
                _travelPoint = new TravelPoint(b.TravelBox, b.MapName, b.PersonalID);
                _building = b;
            }

            public TileObject(TravelPoint rect)
            {
                _travelPoint = rect;
            }
        }
    }

    public class ShopData
    {
        string _sMap;
        int _iShopID;
        Rectangle _rCLick;
        int _iShopX;
        int _iShopY;
        public ShopData(string map, TiledMapObject shopObj)
        {
            _sMap = map;
            _rCLick = Util.FloatRectangle(shopObj.Position, shopObj.Size.Width, shopObj.Size.Height);
            _iShopID = int.Parse(shopObj.Properties["Owner"]);
            _iShopX = int.Parse(shopObj.Properties["ShopKeepX"]);
            _iShopY = int.Parse(shopObj.Properties["ShopKeepY"]);
        }

        internal bool Contains(Point mouseLocation)
        {
            return _rCLick.Contains(mouseLocation);
        }

        internal bool IsOpen()
        {
            bool rv = false;

            if(DataManager.DiNPC[_iShopID].CurrentMapName == _sMap)
            {
                if (MapManager.RetrieveTile(_iShopX, _iShopY).Contains(DataManager.DiNPC[_iShopID]))
                {
                    rv = true;
                }
            }

            return rv;
        }

        internal void Talk()
        {
            ((ShopKeeper)DataManager.DiNPC[_iShopID]).Talk();
        }
    }

    public class TravelPoint
    {
        int _ibuildingID;
        public int BuildingID => _ibuildingID;
        Rectangle _rCollisionBox;
        public Rectangle CollisionBox => _rCollisionBox;
        public Point Location => _rCollisionBox.Location;
        string _sLinkedMap;
        public string LinkedMap => _sLinkedMap;
        public Vector2 Center => _rCollisionBox.Center.ToVector2();


        DirectionEnum _eEntranceDir;

        public TravelPoint(TiledMapObject obj)
        {
            _rCollisionBox = Util.FloatRectangle(obj.Position, obj.Size.Width, obj.Size.Height);
            if (obj.Properties.ContainsKey("Map"))
            {
                _sLinkedMap = obj.Properties["Map"] == "Home" ? MapManager.HomeMap : obj.Properties["Map"];
            }
            if (obj.Properties.ContainsKey("EntranceDir"))
            {
                _eEntranceDir = Util.ParseEnum<DirectionEnum>(obj.Properties["EntranceDir"]);
            }
        }
        public TravelPoint(Rectangle collision, string linkedMap, int buildingID)
        {
            _rCollisionBox = collision;
            _sLinkedMap = linkedMap;
            _ibuildingID = buildingID;
            _eEntranceDir = DirectionEnum.Down;
        }

        public bool Intersects(Rectangle value)
        {
            return _rCollisionBox.Intersects(value);
        }

        public Vector2 FindLinkedPointPosition(Vector2 oldPointCenter, WorldActor c)
        {
            Point actorCollisionCenter = c.CollisionBox.Center;
            Vector2 vDiff = actorCollisionCenter.ToVector2() - oldPointCenter;

            if (_eEntranceDir == DirectionEnum.Left || _eEntranceDir == DirectionEnum.Right)
            {
                vDiff.X *= -1;
                int mod = (_rCollisionBox.Width / 2) + (c.CollisionBox.Width / 2) + 1;
                if (_eEntranceDir == DirectionEnum.Left) { vDiff.X -= mod; }
                else if (_eEntranceDir == DirectionEnum.Right) { vDiff.X += mod; }
            }
            else if (_eEntranceDir == DirectionEnum.Up || _eEntranceDir == DirectionEnum.Down)
            {
                vDiff.Y *= -1;
                int mod = (_rCollisionBox.Height / 2) + (c.CollisionBox.Height / 2)+1;
                if (_eEntranceDir == DirectionEnum.Up) { vDiff.Y -= mod; }
                else if (_eEntranceDir == DirectionEnum.Down) { vDiff.Y += mod; }
            }

            //Modify the reflected vector2 to correlate to the Actor's positio ninstead of the collisionbox center
            Vector2 rv = new Vector2(Center.X + vDiff.X, Center.Y + vDiff.Y);

            Vector2 pos = c.Position;
            Vector2 collisionDiff = pos - actorCollisionCenter.ToVector2();
            rv += collisionDiff;

            return rv;
        }
    }
}
