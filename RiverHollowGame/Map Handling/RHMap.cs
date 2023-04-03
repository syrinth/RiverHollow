using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens.HUDWindows;
using RiverHollow.Items;
using RiverHollow.Items.Tools;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.RiverHollow;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Map_Handling
{
    public class RHMap
    {
        public int MapWidthTiles = 100;
        public int MapHeightTiles = 100;

        private string _sName;
        public string Name => _sName.Replace(@"Maps\", ""); //Fuck off with that path bullshit

        public string MapBelow => (_map.Properties.ContainsKey("MapBelow") ? _map.Properties["MapBelow"] : string.Empty);
        public string MapAbove => _map.Properties.ContainsKey("MapAbove") ? _map.Properties["MapAbove"] : string.Empty;

        public string DungeonName { get; private set; } = string.Empty;
        public bool IsDungeon => !string.IsNullOrEmpty(DungeonName);
        public bool IsTown => _map.Properties.ContainsKey("Town");
        public int BuildingID => _map.Properties.ContainsKey("BuildingID") ? int.Parse(_map.Properties["BuildingID"]) : -1;
        public bool Modular => _map.Properties.ContainsKey("Modular");
        public bool IsOutside => _map.Properties.ContainsKey("Outside");
        public float Lighting => _map.Properties.ContainsKey("Lighting") ? float.Parse(_map.Properties["Lighting"]) : 1;
        public string MapType => _map.Properties.ContainsKey("MapType") ? _map.Properties["MapType"] : string.Empty;

        public bool Visited { get; private set; } = false;
        public MobSpawnStateEnum MobsSpawned { get; private set; } = MobSpawnStateEnum.None;
        private bool Randomize => _map.Properties.ContainsKey("Randomize");
        public MonsterFood PrimedFood { get; private set; }

        public RHTile TargetTile { get; private set; } = null;
        private RHTile MouseTile => GetMouseOverTile();

        protected TiledMap _map;
        public TiledMap Map => _map;

        protected RHTile[,] _arrTiles;
        public List<RHTile> TileList => _arrTiles.Cast<RHTile>().ToList();
        private List<RHTile> _liWallTiles;

        protected TiledMapRenderer _renderer;
        protected List<TiledMapTileset> _liTilesets;
        protected Dictionary<string, List<TiledMapTileLayer>> _diTileLayers;
        public Dictionary<string, List<TiledMapTileLayer>> Layers => _diTileLayers;

        private List<Light> _liLights;
        private List<Light> _liHeldLights;
        private List<RHTile> _liTestTiles;
        private List<Actor> _liActors;
        protected List<Mob> _liMobs;
        public IList<Mob> Mobs { get { return _liMobs.AsReadOnly(); } }
        public List<Actor> ToAdd;
        private List<Building> _liBuildings;
        private List<WorldObject> _liPlacedWorldObjects;
        private List<WorldObject> _liResourceObjects;
        private List<ResourceSpawn> _liResourceSpawns;
        private List<MobSpawn> _liMobSpawns;
        private List<int> _liCutscenes;
        private Dictionary<RarityEnum, List<int>> _diResources;
        protected List<MapItem> _liItems;
        public Dictionary<string, TravelPoint> DictionaryTravelPoints { get; }
        public Dictionary<string, Point> DictionaryCharacterLayer { get; }
        private List<TiledMapObject> _liMapObjects;
        private List<KeyValuePair<Rectangle, string>> _liClickObjects;
        private int _iShopID = -1;
        public Shop TheShop => (_iShopID > -1) ? GameManager.DIShops[_iShopID] : null;

        private List<MapItem> _liItemsToRemove;
        private List<Actor> _liActorsToRemove;
        private List<WorldObject> _liObjectsToRemove;

        public MapNode WorldMapNode { get; private set; }

        Buildable _objSelectedObject = null;

        public RHMap()
        {
            _liResourceSpawns = new List<ResourceSpawn>();
            _liResourceObjects = new List<WorldObject>();
            _liMobSpawns = new List<MobSpawn>();
            _liWallTiles = new List<RHTile>();
            _liTestTiles = new List<RHTile>();
            _liTilesets = new List<TiledMapTileset>();
            _liActors = new List<Actor>();
            _liMobs = new List<Mob>();
            _liBuildings = new List<Building>();
            _liItems = new List<MapItem>();
            _liMapObjects = new List<TiledMapObject>();
            _liClickObjects = new List<KeyValuePair<Rectangle, string>>();

            _liPlacedWorldObjects = new List<WorldObject>();
            _liLights = new List<Light>();
            _liHeldLights = new List<Light>();
            _liCutscenes = new List<int>();
            _diResources = new Dictionary<RarityEnum, List<int>>();

            DictionaryTravelPoints = new Dictionary<string, TravelPoint>();
            DictionaryCharacterLayer = new Dictionary<string, Point>();

            _liItemsToRemove = new List<MapItem>();
            _liActorsToRemove = new List<Actor>();
            _liObjectsToRemove = new List<WorldObject>();

            ToAdd = new List<Actor>();
        }

        /// <summary>
        /// THIS IS THE COPY CONSTRUCTOR
        /// </summary>
        /// <param name="map"></param>
        public RHMap(RHMap map) : this()
        {
            _map = map.Map;
            _sName = map.Name + "Clone";
            _renderer = map._renderer;
            _arrTiles = map._arrTiles;
            _liWallTiles = map._liWallTiles;
            _diTileLayers = map._diTileLayers;

            if (_map.Properties.ContainsKey("Dungeon"))
            {
                DungeonName = _map.Properties["Dungeon"];
                DungeonManager.AddMapToDungeon(_map.Properties["Dungeon"], _map.Properties.ContainsKey("Procedural"), this);
            }

            _liBuildings = map._liBuildings;
            _liPlacedWorldObjects = map._liPlacedWorldObjects;
            _liLights = map._liLights;
            _liHeldLights = map._liHeldLights;
            _iShopID = map._iShopID;

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

            _diTileLayers = new Dictionary<string, List<TiledMapTileLayer>>() { ["Base"] = new List<TiledMapTileLayer>(), ["Ground"] = new List<TiledMapTileLayer>(), ["Upper"] = new List<TiledMapTileLayer>() };
            foreach (TiledMapTileLayer l in _map.TileLayers)
            {
                if (l.Name.Contains("Base")) { _diTileLayers["Base"].Add(l); }
                else if (l.Name.Contains("Ground")) { _diTileLayers["Ground"].Add(l); }
                else if (l.Name.Contains("Upper")) { _diTileLayers["Upper"].Add(l); }
                else
                {
                    if (!_diTileLayers.ContainsKey(l.Name))
                    {
                        _diTileLayers[l.Name] = new List<TiledMapTileLayer>();
                    }
                    _diTileLayers[l.Name].Add(l);
                }
            }

            _arrTiles = new RHTile[MapWidthTiles, MapHeightTiles];
            for (int i = 0; i < MapHeightTiles; i++)
            {
                for (int j = 0; j < MapWidthTiles; j++)
                {
                    _arrTiles[j, i] = new RHTile(j, i, _sName);
                    _arrTiles[j, i].SetProperties(this);
                }
            }

            if (_map.Properties.ContainsKey("Shop"))
            {
                _iShopID = int.Parse(_map.Properties["Shop"]);
            }
            if (_map.Properties.ContainsKey("Dungeon"))
            {
                DungeonName = _map.Properties["Dungeon"];
                DungeonManager.AddMapToDungeon(_map.Properties["Dungeon"], _map.Properties.ContainsKey("Procedural"), this);
            }

            if (_map.Properties.ContainsKey("Cutscenes"))
            {
                string[] split = _map.Properties["Cutscenes"].Split('|');
                foreach (string cutsceneID in split)
                {
                    _liCutscenes.Add(int.Parse(cutsceneID));
                }
            }

            if (_map.Properties.ContainsKey("WorldData"))
            {
                string worldData = _map.Properties["WorldData"];
                WorldMapNode = new MapNode(Util.ParsePoint(worldData), int.Parse(_map.Properties["WorldMapCost"]), _map.Properties["WorldLink"]);
            }

            _renderer = new TiledMapRenderer(GraphicsDevice);
        }

        public void Update(GameTime gTime)
        {
            //Used to prevent updates when we leave a cutscene.
            //Maps should only be loose at the end of a cutscene.
            if (!MapManager.Maps.ContainsKey(this.Name))
            {
                return;
            }

            if (this == MapManager.CurrentMap || this.Name == MapManager.CurrentMap.MapAbove || this.Name == MapManager.CurrentMap.MapBelow)
            {
                _renderer.Update(_map, gTime);

                EnvironmentManager.Update(gTime);

                _liMobs.ForEach(x => x.Update(gTime));
                _liItems.ForEach(x => x.Update(gTime));
            }

            foreach (WorldObject obj in _liObjectsToRemove)
            {
                _liPlacedWorldObjects.Remove(obj);
            }
            _liObjectsToRemove.Clear();

            foreach (WorldObject obj in _liPlacedWorldObjects) {
                if (this == MapManager.CurrentMap || obj.CompareType(ObjectTypeEnum.Machine))
                {
                    obj.Update(gTime);
                }
            }

            if (ToAdd.Count > 0)
            {
                List<Actor> moved = new List<Actor>();
                foreach (Actor c in ToAdd)
                {
                    if (AddCharacterImmediately(c))
                    {
                        moved.Add(c);
                    }
                }
                foreach (Actor c in moved)
                {
                    ToAdd.Remove(c);
                }
                moved.Clear();
            }

            foreach (Actor c in _liActorsToRemove)
            {
                switch (c.ActorType)
                {
                    case ActorTypeEnum.Mob:
                        _liMobs.Remove((Mob)c);
                        if (_liMobs.Count == 0)
                        {
                            DungeonManager.ActivateTrigger(Constants.TRIGGER_MOB_OPEN);
                        }
                        break;
                    default:
                        _liActors.Remove(c);
                        break;
                }
            }

            _liActorsToRemove.Clear();

            if (!GamePaused())
            {
                foreach (Actor c in _liActors)
                {
                    c.Update(gTime);
                }
            }

            ItemPickUpdate();

            _liItemsToRemove.ForEach(x => _liItems.Remove(x));
            _liItemsToRemove.Clear();
        }

        public void DrawBelowBase(SpriteBatch spriteBatch)
        {
            if (MapBelowValid()) {
                MapManager.Maps[MapBelow].DrawBase(spriteBatch);
            }
        }
        public void DrawAboveBase(SpriteBatch spriteBatch)
        {
            if (MapAboveValid()) {
                MapManager.Maps[MapAbove].DrawBase(spriteBatch);
            }
        }
        public void DrawBase(SpriteBatch spriteBatch)
        {
            SetLayerVisibiltyByName(true, "Base");
            SetLayerVisibiltyByName(false, "Ground");
            SetLayerVisibiltyByName(false, "Upper");
            _renderer.Draw(_map, Camera._transform);

            if (_liWallTiles.Count > 0)
            {
                foreach (RHTile t in _liWallTiles)
                {
                    t.DrawWallpaper(spriteBatch);
                }
            }
        }

        public void DrawBelowGround(SpriteBatch spriteBatch) {
            if (MapBelowValid())
            {
                MapManager.Maps[MapBelow].DrawGround(spriteBatch);
            }
        }
        public void DrawAboveGround(SpriteBatch spriteBatch)
        {
            if (MapAboveValid())
            {
                MapManager.Maps[MapAbove].DrawGround(spriteBatch);
            }
        }
        public void DrawGround(SpriteBatch spriteBatch)
        {
            SetLayerVisibiltyByName(false, "Base");
            SetLayerVisibiltyByName(true, "Ground");
            SetLayerVisibiltyByName(false, "Upper");

            _renderer.Draw(_map, Camera._transform);

            _liActors.ForEach(x => x.Draw(spriteBatch, true));
            _liMobs.FindAll(x => x.Subtype != MobTypeEnum.Flyer || !x.HasHP).ForEach(x => x.Draw(spriteBatch, true));
            _liBuildings.ForEach(x => x.Draw(spriteBatch));
            _liPlacedWorldObjects.ForEach(x => x.Draw(spriteBatch));
            _liItems.ForEach(x => x.Draw(spriteBatch));

            if (TheShop != null)
            {
                foreach (ShopItemSpot itemSpot in TheShop.ItemSpots)
                {
                    itemSpot.Draw(spriteBatch);
                }
            }

            if (HeldObject != null && (!GameManager.GamePaused() || Scrying()))
            {
                foreach (RHTile t in _liTestTiles)
                {
                    bool passable = CanPlaceObject(t, HeldObject);
                    if (!passable || (passable && !HeldObject.CompareType(ObjectTypeEnum.Wallpaper)))
                    {
                        spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), new Rectangle((int)t.Position.X, (int)t.Position.Y, Constants.TILE_SIZE, Constants.TILE_SIZE), new Rectangle(288, 128, Constants.TILE_SIZE, Constants.TILE_SIZE), passable ? Color.Green * 0.5f : Color.Red * 0.5f, 0, Vector2.Zero, SpriteEffects.None, Constants.MAX_LAYER_DEPTH);
                    }
                }
            }
        }

        public void DrawBelowUpper(SpriteBatch spriteBatch)
        {
            if (MapBelowValid())
            {
                MapManager.Maps[MapBelow].DrawUpper(spriteBatch);
            }
        }
        public void DrawAboveUpper(SpriteBatch spriteBatch)
        {
            if (MapAboveValid())
            {
                MapManager.Maps[MapAbove].DrawUpper(spriteBatch);
            }
        }
        public void DrawUpper(SpriteBatch spriteBatch)
        {
            SetLayerVisibiltyByName(false, "Base");
            SetLayerVisibiltyByName(false, "Ground");
            SetLayerVisibiltyByName(true, "Upper");
            _renderer.Draw(_map, Camera._transform);

            _liMobs.FindAll(x => x.Subtype == MobTypeEnum.Flyer && x.HasHP).ForEach(x => x.Draw(spriteBatch, true));
            EnvironmentManager.Draw(spriteBatch);

            SetLayerVisibiltyByName(true, "Base");
            SetLayerVisibiltyByName(true, "Ground");
            SetLayerVisibiltyByName(false, "Upper");
        }

        public void SetLayerVisibiltyByName(bool visible, string designation)
        {
            foreach (TiledMapTileLayer layer in _diTileLayers[designation])
            {
                layer.IsVisible = visible;
            }
        }

        public void DrawLights(SpriteBatch spriteBatch)
        {
            foreach (Light obj in _liLights) { obj.Draw(spriteBatch); }
            foreach (Light obj in _liHeldLights) { obj.Draw(spriteBatch); }

            //spriteBatch.Draw(lightMask, new Vector2(PlayerManager.World.CollisionCenter.X - lightMask.Width / 2, PlayerManager.World.CollisionBox.Y - lightMask.Height / 2), Color.White);
        }
        public void AddHeldLights(List<Light> newLights)
        {
            if (newLights != null)
            {
                _liHeldLights.AddRange(newLights);
            }
        }
        public void ClearHeldLights() { _liHeldLights.Clear(); }
        public void AddLights(List<Light> newLights)
        {
            if (newLights != null)
            {
                foreach (Light obj in newLights)
                {
                    if (!_liLights.Contains(obj))
                    {
                        _liLights.Add(obj);
                    }
                }
            }
        }
        public void RemoveLights(List<Light> newLights)
        {
            if (newLights != null)
            {
                foreach (Light obj in newLights)
                {
                    _liLights.Remove(obj);
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
                            TravelPoint trvlPt = new TravelPoint(mapObject, this.Name);
                            if (mapObject.Properties.ContainsKey("Door"))
                            {
                                trvlPt.SetDoor();
                                CreateDoor(trvlPt, mapObject.Position.X, mapObject.Position.Y, mapObject.Size.Width, mapObject.Size.Height);
                            }

                            if (!string.IsNullOrEmpty(trvlPt.LinkedMap)) { DictionaryTravelPoints.Add(trvlPt.LinkedMap, trvlPt); }
                            else { DictionaryTravelPoints.Add(Util.GetEnumString(trvlPt.Dir), trvlPt); }
                        }
                    }
                }
                else if (ol.Name.Contains("Character"))
                {
                    foreach (TiledMapObject obj in ol.Objects)
                    {
                        if (obj.Name.Equals("ShopItem"))
                        {
                            TheShop.AddItemSpot(new ShopItemSpot(this.Name, obj.Position, obj.Size.Width, obj.Size.Height));
                        }
                        else if (obj.Name.Equals("Spirit"))
                        {
                            Spirit s = new Spirit(obj.Properties)
                            {
                                CurrentMapName = _sName
                            };
                            s.SetPosition(Util.SnapToGrid(obj.Position.ToPoint()));
                            GameManager.AddSpirit(s);
                            _liActors.Add(s);
                        }
                        else if (obj.Name.Equals("NPC"))
                        {
                            if (obj.Properties.ContainsKey("NPC_ID"))
                            {
                                DictionaryCharacterLayer.Add("NPC_" + obj.Properties["NPC_ID"], obj.Position.ToPoint());
                            }
                        }
                        else
                        {
                            DictionaryCharacterLayer.Add(obj.Name, obj.Position.ToPoint());
                        }
                    }
                }
                else if (ol.Name.Contains("MapObject"))
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        if (mapObject.Name.Equals("Wall"))
                        {
                            foreach (Point p in Util.GetAllPointsInArea(mapObject.Position, mapObject.Size, Constants.TILE_SIZE))
                            {
                                RHTile tile = GetTileByPixelPosition(p);
                                Util.AddUniquelyToList(ref _liWallTiles, tile);
                                tile.SetWallTrue();
                            }
                        }
                        else if (mapObject.Name.StartsWith("Display"))
                        {
                            foreach (Point p in Util.GetAllPointsInArea(mapObject.Position, mapObject.Size, Constants.TILE_SIZE))
                            {
                                GetTileByPixelPosition(p).SetClickAction(mapObject.Name);
                            }
                        }
                        else if (mapObject.Name.StartsWith("Resource"))
                        {
                            _liResourceSpawns.Add(new ResourceSpawn(this, mapObject));
                        }
                        else if (mapObject.Name.Equals("Mob")) { _liMobSpawns.Add(new MobSpawn(this, mapObject)); }
                        else { _liMapObjects.Add(mapObject); }
                    }
                }
            }
        }

        public void PopulateMap(bool gameStart)
        {
            foreach (TiledMapObject tiledObj in _liMapObjects)
            {
                if (gameStart || !tiledObj.Properties.ContainsKey("DoNotLoad"))
                {
                    int objWidth = Constants.TILE_SIZE;
                    int objHeight = Constants.TILE_SIZE;
                    for (int y = (int)tiledObj.Position.Y; y < (int)tiledObj.Position.Y + tiledObj.Size.Height; y += objWidth)
                    {
                        for (int x = (int)tiledObj.Position.X; x < (int)tiledObj.Position.X + tiledObj.Size.Width; x += objHeight)
                        {
                            if (tiledObj.Properties.ContainsKey("ObjectID"))
                            {
                                WorldObject obj = DataManager.CreateWorldObjectByID(int.Parse(tiledObj.Properties["ObjectID"]), tiledObj.Properties);
                                obj.PlaceOnMap(new Point(x, y), this);
                                objWidth = obj.BaseWidth * Constants.TILE_SIZE;
                                objHeight = obj.BaseHeight * Constants.TILE_SIZE;
                            }
                            else if (tiledObj.Properties.ContainsKey("ItemID"))
                            {
                                new WrappedItem(int.Parse(tiledObj.Properties["ItemID"])).PlaceOnMap(tiledObj.Position.ToPoint(), this);
                            }
                        }
                    }
                }
            }
        }

        public void SpawnMapEntities(bool spawnAboveAndBelow = true)
        {
            if (!Visited)
            {
                Visited = true;
                if (spawnAboveAndBelow)
                {
                    if (MapAboveValid()) { MapManager.Maps[MapAbove].SpawnMapEntities(false); }
                    if (MapBelowValid()) { MapManager.Maps[MapBelow].SpawnMapEntities(false); }
                }

                _liResourceSpawns.ForEach(x => x.Spawn());
            }

            if (MobsSpawned == MobSpawnStateEnum.None) {

                SpawnMobs();
                if (GameCalendar.IsNight()) { MobsSpawned = MobSpawnStateEnum.Night; }
                else { MobsSpawned = MobSpawnStateEnum.Day; }
            }
            else if (MobsSpawned == MobSpawnStateEnum.Day)
            {
                if (GameCalendar.IsNight())
                {
                    SpawnMobs();
                    MobsSpawned = MobSpawnStateEnum.Night;
                }
            }
        }

        private void SpawnMobs()
        {
            if (Map.Properties.ContainsKey("Mobs")) {
                for (int i = 0; i < _liMobs.Count; i++)
                {
                    RemoveActor(_liMobs[i]);
                }
                _liMobs.Clear();

                string[] mobRange = Map.Properties["Mobs"].Split('-');
                int roll = RHRandom.Instance().Next(int.Parse(mobRange[0]), int.Parse(mobRange[1]));

                List<SpawnPoint> spawnCopy = new List<SpawnPoint>();
                spawnCopy.AddRange(_liMobSpawns);

                for (int spawnNum = 0; spawnNum < roll; spawnNum++)
                {
                    int index = RHRandom.Instance().Next(0, spawnCopy.Count - 1);

                    spawnCopy[index].Spawn();
                    spawnCopy.RemoveAt(index);
                }
            }
        }

        public void TestHitboxOnMobs(HitboxTool t)
        {
            foreach (Mob npc in _liMobs)
            {
                if (npc.CollisionBox.Intersects(t.Hitbox))
                {
                    npc.DealDamage(t.ToolLevel, PlayerManager.PlayerActor.CollisionBox);
                }
            }
        }
        public bool AllMobsDefeated()
        {
            return _liMobs.Count - _liActorsToRemove.FindAll(x => x.ActorType == ActorTypeEnum.Mob).Count <= 0;
        }

        public void ResetMobs()
        {
            _liMobs.ForEach(x => x.Reset());
        }

        public void AlertSpawnPoint(WorldObject obj)
        {
            for (int i = 0; i < _liResourceSpawns.Count; i++)
            {
                if (_liResourceSpawns[i].AlertSpawnPoint(obj))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Sets the MonsterFood for the map
        /// </summary>
        /// <param name="f">The MonsterFood to prime off of</param>
        public void PrimeMonsterSpawns(MonsterFood f)
        {
            PrimedFood = f;

            ////Copy the spawn points to a list we can safely modify
            //List<MonsterSpawn> spawnCopy = new List<MonsterSpawn>();
            //spawnCopy.AddRange(MonsterSpawnPoints);

            //int spawnNum = PrimedFood.SpawnNumber;
            ////For safety, but this looks like bad design
            //if (spawnNum > spawnCopy.Count) { spawnNum = spawnCopy.Count; }

            ////Trigger x number of SpawnPoints
            //for (int i = 0; i < spawnNum; i++)
            //{
            //    //Get a random Spawn Point
            //    int point = RHRandom.Instance().Next(0, spawnCopy.Count - 1);
            //    spawnCopy[point].SetSpawn(PrimedFood.SpawnID);

            //    //remove it from the copied list so it won't be an option for future spawning.
            //    spawnCopy.RemoveAt(point);
            //}
        }

        public void ClearMapEntities()
        {
            foreach (Mob m in _liMobs) { RemoveActor(m); }
            foreach (WorldObject obj in _liPlacedWorldObjects)
            {
                switch (obj.Type)
                {
                    case ObjectTypeEnum.DungeonObject:
                        if (Modular) { goto case ObjectTypeEnum.WorldObject; }
                        else { break; }
                    case ObjectTypeEnum.Hazard:
                    case ObjectTypeEnum.Destructible:
                    case ObjectTypeEnum.Gatherable:
                    case ObjectTypeEnum.Plant:
                    case ObjectTypeEnum.WorldObject:
                        RemoveWorldObject(obj);
                        break;
                }
            }
        }

        /// <summary>
        /// Call to retrieve a list of RHTiles that cannot be used to spawn resources on. These tiles
        /// are chosen due to proximity to monster spawn points, and travel points.
        /// 
        /// Additionally, it ensures that there exists a path between each monster spawn point and each
        /// potential player combat start area
        /// </summary>
        /// <param name="loaded">Whether the map was loaded or not. To avoid double spawning</param>
        /// <returns>A list of tiles to ignore when spawning resources.</returns>
        private List<RHTile> GetSkipTiles()
        {
            List<RHTile> skipTiles = new List<RHTile>();

            foreach (TravelPoint tp in DictionaryTravelPoints.Values)
            {
                List<RHTile> tiles = GetTilesFromRectangleExcludeEdgePoints(tp.CollisionBox);
                for (int tileIndex = 0; tileIndex < tiles.Count; tileIndex++)
                {
                    Util.AddUniquelyToList(ref skipTiles, tiles[tileIndex]);

                    List<RHTile> neighbourList = tiles[tileIndex].GetWalkableNeighbours();
                    for (int neighbourIndex = 0; neighbourIndex < neighbourList.Count; neighbourIndex++)
                    {
                        Util.AddUniquelyToList(ref skipTiles, neighbourList[neighbourIndex]);
                    }
                }
            }

            return skipTiles;
        }

        public void Rollover()
        {
            if (Randomize)
            {
                Visited = false;
                _liPlacedWorldObjects.ForEach(x => x.RemoveSelfFromTiles());
                _liPlacedWorldObjects.Clear();
            }
            MobsSpawned = MobSpawnStateEnum.None;
            _liPlacedWorldObjects.ForEach(x => x.Rollover());
            _liResourceSpawns.ForEach(x => x.Rollover(Randomize));

            StockShop();
            CheckSpirits();
            _liItems.Clear();
        }

        public void StockShop()
        {
            if (this != MapManager.TownMap && TownManager.TownObjectBuilt(BuildingID))
            {
                TheShop?.PlaceStock(false);
            }
        }

        /// <summary>
        /// Creates the TravelPoint object necessary for the given Building
        /// and calls CreateDoor to add the travel info to the correct RHTiles
        /// </summary>
        /// <param name="b">The building to create the door for</param>
        public void CreateBuildingEntrance(Building b)
        {
            TravelPoint buildPoint = new TravelPoint(b, this.Name, b.ID);
            DictionaryTravelPoints.Add(b.BuildingMapName, buildPoint); //TODO: FIX THIS
            CreateDoor(buildPoint, b.TravelBox.X, b.TravelBox.Y, b.TravelBox.Width, b.TravelBox.Height);
        }

        public void UpdateBuildingEntrance(string initialMapName, string newMapName)
        {
            if (DictionaryTravelPoints.ContainsKey(initialMapName))
            {
                TravelPoint pt = DictionaryTravelPoints[initialMapName];
                DictionaryTravelPoints.Remove(initialMapName);
                DictionaryTravelPoints[newMapName] = pt;
            }
        }

        /// <summary>
        /// Using the provided information, assign the given TravelPoint to
        /// the appropriate RHTiles.
        /// </summary>
        /// <param name="trvlPt">The TravelPoint to assign</param>
        /// <param name="rectX">The starting x position</param>
        /// <param name="rectY">The starting y position</param>
        /// <param name="width">The Width</param>
        /// <param name="height">The Height</param>
        public void CreateDoor(TravelPoint trvlPt, float rectX, float rectY, float width, float height)
        {
            foreach (Point vec in Util.GetAllPointsInArea((int)rectX, (int)rectY, (int)width, (int)height, Constants.TILE_SIZE))
            {
                RHTile t = GetTileByPixelPosition(vec);
                if (t != null)
                {
                    t.SetTravelPoint(trvlPt);
                }
            }
        }

        /// <summary>
        /// Removes the doorway attached to the building.
        /// 
        /// Iterates over all tiles described by the TravelPoint and
        /// clears the relevant Tile's TravelPoint object.
        /// </summary>
        /// <param name="b">The building to remove the door of</param>
        public void RemoveDoor(Building b)
        {
            string mapName = b.BuildingMapName;
            TravelPoint pt = DictionaryTravelPoints[mapName];

            foreach (Point vec in Util.GetAllPointsInArea(pt.Location.X, pt.Location.Y, pt.CollisionBox.Width, pt.CollisionBox.Height, Constants.TILE_SIZE))
            {
                RHTile t = GetTileByPixelPosition(vec);
                if (t != null)
                {
                    t.SetTravelPoint(null);
                }
            }

            DictionaryTravelPoints.Remove(mapName);
        }

        public List<RHTile> FindFreeTiles()
        {
            List<RHTile> rv = new List<RHTile>();
            foreach (RHTile x in _arrTiles)
            {
                if (x.Passable() && (x.WorldObject == null || x.WorldObject.Walkable))
                {
                    rv.Add(x);
                }
            }

            return rv;
        }

        /// <summary>
        /// Call this to trigger any changes on the map that needed to occur
        /// as a result of the Building being upgraded.
        /// </summary>
        /// <param name="newLevel"></param>
        public void UpgradeMap(int newLevel)
        {
            foreach (TiledMapObject tiledObj in _liMapObjects)
            {
                if (tiledObj.Properties.ContainsKey("UpgradeLevel"))
                {
                    int upgradeLevel = int.Parse(tiledObj.Properties["UpgradeLevel"]);
                    if (newLevel == upgradeLevel)
                    {
                        DataManager.CreateAndPlaceNewWorldObject(int.Parse(tiledObj.Properties["ObjectID"]), tiledObj.Position.ToPoint(), this);
                    }

                }
            }
        }

        public void CheckSpirits()
        {
            foreach (Actor c in _liActors)
            {
                if (c.IsActorType(ActorTypeEnum.Spirit))
                {
                    ((Spirit)c).CheckCondition();
                }
            }
        }

        public Spirit FindSpirit()
        {
            Spirit rv = null;

            foreach (Actor a in _liActors)
            {
                if (a.IsActorType(ActorTypeEnum.Spirit) && PlayerManager.PlayerInRange(a.CollisionBoxLocation, 500))
                {
                    rv = (Spirit)a;
                }
            }

            return rv;
        }

        public bool ContainsActor(Actor c)
        {
            return _liActors.Contains(c);//|| (c.IsActorType(ActorEnum.Monster) && Monsters.Contains((TacticalMonster)c));
        }

        public void ItemPickUpdate()
        {
            if (this == MapManager.CurrentMap)
            {
                Actor player = PlayerManager.PlayerActor;
                for (int i = 0; i < _liItems.Count; i++)
                {
                    MapItem it = _liItems[i];
                    Item wrapped = it.WrappedItem;
                    if (InventoryManager.HasSpaceInInventory(wrapped.ID, wrapped.Number))
                    {
                        if (it.PickupState == ItemPickupState.Auto)
                        {
                            if (it.FinishedMoving() && it.CollisionBox.Intersects(player.CollisionBox))
                            {
                                AddItemToPlayerInventory(it);
                            }
                            else if (PlayerManager.PlayerInRange(it.CollisionBox.Center, 80))
                            {
                                int speed = 3;
                                Vector2 direction = Util.MoveUpTo(it.Position, player.CollisionBox.Location, 3);
                                direction.Normalize();
                                it.MoveItem(direction * speed);
                            }
                        }
                    }
                }
            }
        }

        public void AddItemToPlayerInventory(MapItem it)
        {
            _liItemsToRemove.Add(it);
            InventoryManager.AddToInventory(it.WrappedItem);
        }

        #region Collision Code
        public bool TileContainsBlockingActor(RHTile t, bool checkPlayer = true)
        {
            bool rv = false;

            if (checkPlayer && this == PlayerManager.PlayerActor.CurrentMap && PlayerManager.PlayerActor.CollisionIntersects(t.CollisionBox)) { rv = true; }
            else
            {
                foreach (Actor act in _liActors)
                {
                    if (act.CollisionIntersects(t.CollisionBox) && !act.IsActorType(ActorTypeEnum.Critter))
                    {
                        rv = true;
                        break;
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// Find every tile that could possibly cause a collision for the moving WorldActor
        /// </summary>
        /// <param name="actor">The moving WorldActor</param>
        /// <param name="dir"></param>
        /// <returns>The direction they are moving in</returns>
        private List<KeyValuePair<Rectangle, Actor>> GetPossibleCollisions(Actor actor, Vector2 dir)
        {
            List<KeyValuePair<Rectangle, Actor>> list = new List<KeyValuePair<Rectangle, Actor>>();
            Rectangle rEndCollision = new Rectangle((int)(actor.CollisionBox.X + dir.X), (int)(actor.CollisionBox.Y + dir.Y), actor.CollisionBox.Width, actor.CollisionBox.Height);
            //The following if-blocks get the tiles that the four corners of the
            //moved CollisionBox will be inside of, based off of the movement direction.
            if (dir.X > 0)
            {
                AddTile(ref list, rEndCollision.Right, rEndCollision.Top, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Bottom, actor.CurrentMapName);
                if (rEndCollision.Height > Constants.TILE_SIZE)
                {
                    foreach (Point p in Util.GetAllPointsInArea(rEndCollision.Right - 1, rEndCollision.Top, 1, rEndCollision.Height, Constants.TILE_SIZE))
                    {
                        AddTile(ref list, p.X, p.Y, actor.CurrentMapName);
                    }
                }
            }
            else if (dir.X < 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Top, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Left, rEndCollision.Bottom, actor.CurrentMapName);
                if (rEndCollision.Height > Constants.TILE_SIZE)
                {
                    foreach (Point p in Util.GetAllPointsInArea(rEndCollision.Left - 1, rEndCollision.Top, 1, rEndCollision.Height, Constants.TILE_SIZE))
                    {
                        AddTile(ref list, p.X, p.Y, actor.CurrentMapName);
                    }
                }
            }

            if (dir.Y > 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Bottom, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Bottom, actor.CurrentMapName);
                if (rEndCollision.Width > Constants.TILE_SIZE)
                {
                    foreach (Point p in Util.GetAllPointsInArea(rEndCollision.Left, rEndCollision.Bottom - 1, rEndCollision.Width, 1, Constants.TILE_SIZE))
                    {
                        AddTile(ref list, p.X, p.Y, actor.CurrentMapName);
                    }
                }
            }
            else if (dir.Y < 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Top, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Top, actor.CurrentMapName);
                if (rEndCollision.Width > Constants.TILE_SIZE)
                {
                    foreach (Point p in Util.GetAllPointsInArea(rEndCollision.Left, rEndCollision.Top - 1, rEndCollision.Width, 1, Constants.TILE_SIZE))
                    {
                        AddTile(ref list, p.X, p.Y, actor.CurrentMapName);
                    }
                }
            }

            if (!actor.IsActorType(ActorTypeEnum.Projectile))
            {
                //Because RHTiles do not contain WorldActors outside of combat, we need to add each
                //WorldActor's CollisionBox to the list, as long as the WorldActor in question is not the moving WorldActor.
                foreach (Actor npc in _liActors)
                {
                    if (npc.OnTheMap && npc != actor)
                    {
                        switch (npc.ActorType)
                        {
                            case ActorTypeEnum.Mount:
                            case ActorTypeEnum.Critter:
                                break;
                            default:
                                list.Add(new KeyValuePair<Rectangle, Actor>(npc.CollisionBox, npc));
                                break;
                        }
                    }
                }

                //If the actor is not the Player Character, add the Player Character's CollisionBox to the list as well
                if (actor != PlayerManager.PlayerActor && MapManager.CurrentMap == actor.CurrentMap && actor.CollidesWithPlayer())
                {
                    list.Add(new KeyValuePair<Rectangle, Actor>(PlayerManager.PlayerActor.CollisionBox, PlayerManager.PlayerActor));
                }
            }

            return list;
        }
        private void AddTile(ref List<KeyValuePair<Rectangle, Actor>> list, int one, int two, string mapName)
        {
            RHTile tile = MapManager.Maps[mapName].GetTileByGridCoords(Util.GetGridCoords(one, two));
            if (TileValid(tile, list)) { list.Add(new KeyValuePair<Rectangle, Actor>(tile.CollisionBox, null)); }
        }
        private bool TileValid(RHTile tile, List<KeyValuePair<Rectangle, Actor>> list)
        {
            return tile != null && !tile.Passable() && !list.Any(x => x.Key == tile.CollisionBox);
        }

        private void ChangeDir(Actor actor, List<KeyValuePair<Rectangle, Actor>> possibleCollisions, ref Vector2 dir, ref bool impeded)
        {
            if (actor.IgnoreCollisions)
            {
                return;
            }

            //Because of how objects interact with each other, this check needs to be broken up so that the x and y movement can be
            //calculated seperately. If an object is above you and you move into it at an angle, if you check the collision as one rectangle
            //then the collision nullification will hit the entire damn movement mode.

            Vector2 initialDir = dir;
            Vector2 diagonalChange = dir;

            Point projected = actor.ProjectedMovement(dir);
            Point location = actor.CollisionBoxLocation;
            Point size = actor.CollisionBox.Size;

            Rectangle testHorizontal = new Rectangle(location.X + projected.X, location.Y, size.X, size.Y);
            Rectangle testVertical = new Rectangle(location.X, location.Y + projected.Y, size.X, size.Y);
            Rectangle testDiagonal = new Rectangle(location + projected, size);
            foreach (KeyValuePair<Rectangle, Actor> kvp in possibleCollisions)
            {
                Rectangle r = kvp.Key;
                Actor npc = kvp.Value;

                if (dir.Y != 0 && r.Intersects(testVertical))
                {
                    ChangeDirHelper(actor, kvp, dir, testVertical, true, ref dir.Y, ref dir.X, ref impeded);
                }
                else if (dir.X != 0 && r.Intersects(testHorizontal))
                {
                    ChangeDirHelper(actor, kvp, dir, testHorizontal, false, ref dir.X, ref dir.Y, ref impeded);
                }
                else if (kvp.Key.Intersects(testDiagonal))
                {
                    if (kvp.Value != null && kvp.Value.SlowDontBlock) { impeded = true; }
                    else { diagonalChange.X = 0; }
                }
            }

            //Diagonal change only gets applied after if the direction hasn't been changed
            if (initialDir == dir && diagonalChange != dir)
            {
                dir = diagonalChange;
            }
        }

        private void ChangeDirHelper(Actor actor, KeyValuePair<Rectangle, Actor> collisionKvp, Vector2 dir, Rectangle testRect, bool vertical, ref float dirToCancel, ref float dirToNudge, ref bool impeded)
        {
            Actor npc = collisionKvp.Value;
            Rectangle r = collisionKvp.Key;
            Point coords = Util.GetGridCoords(r.Location);

            Point location = actor.CollisionBoxLocation;
            Point size = actor.CollisionBox.Size;

            if (npc != null && npc.SlowDontBlock)
            {
                impeded = true;
            }
            else
            {
                int distance;
                bool positionLessThan;

                if (vertical) { positionLessThan = (r.Location.Y - actor.CollisionBox.Location.Y) > 0; }
                else { positionLessThan = (r.Location.X - actor.CollisionBox.Location.X) > 0; }

                if (vertical) { distance = positionLessThan ? r.Top - actor.CollisionBox.Bottom : r.Bottom - actor.CollisionBox.Top; }
                else { distance = positionLessThan ? r.Left - actor.CollisionBox.Right : r.Right - actor.CollisionBox.Left; }

                dirToCancel = Math.Abs(distance) < Math.Abs(dirToCancel) ? distance : 0;

                //Modifier is to determine if the nudge is positive or negative
                float modifier;
                if (vertical) { modifier = CheckToNudge(testRect.Center.X, r.Center.X, coords.X, coords.Y, "Col"); }
                else { modifier = CheckToNudge(testRect.Center.Y, r.Center.Y, coords.X, coords.Y, "Row"); }

                //Constructs the new rectangle based on the mod. Need to change the Edge values by one because the Edge values are
                //the first pixel where they DON'T exist. Not the last where they do.
                int value;
                if (vertical) { value = Util.RoundForPoint(((modifier > 0 ? testRect.Right - 1 : testRect.Left + 1)) + modifier); }
                else { value = Util.RoundForPoint(((modifier > 0 ? testRect.Bottom - 1 : testRect.Top + 1)) + modifier); }

                Point projected = actor.ProjectedMovement(dir + new Vector2(modifier, 0));
                testRect = new Rectangle(location + projected, size);
                if (dirToNudge == 0 && modifier != 0)
                {
                    if (vertical) { dirToNudge += CheckNudgeAllowed(modifier, new Point(value, testRect.Top + 1), new Point(value, testRect.Bottom - 1), actor.CurrentMapName); }
                    else { dirToNudge += CheckNudgeAllowed(modifier, new Point(testRect.Left + 1, value), new Point(testRect.Right - 1, value), actor.CurrentMapName); }
                }
            }
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
        /// <param name="actor">The moving WorldActor</param>
        /// <param name="testX">Movement along the X Axis</param>
        /// <param name="testY">Movement along the Y Axis</param>
        /// <param name="dir">Reference to the direction to move the WorldActor</param>
        /// <param name="ignoreCollisions">Whether or not to check collisions</param>
        /// <returns>False if we are to prevent movement</returns>
        public bool CheckForCollisions(Actor actor, ref Vector2 dir, ref bool impeded, bool ignoreCollisions = false)
        {
            bool rv = true;

            Point projected = actor.ProjectedMovement(dir);
            Point location = actor.CollisionBoxLocation;
            Point size = actor.CollisionBox.Size;

            Rectangle testRectX = new Rectangle(location.X + projected.X, location.Y, size.X, size.Y);
            Rectangle testRectY = new Rectangle(location.X, location.Y + projected.Y, size.X, size.Y);

            //Checking for a MapChange takes priority overlooking for collisions.
            if (CheckForMapChange(actor, testRectX) || CheckForMapChange(actor, testRectY))
            {
                return false;
            }
            else if (!ignoreCollisions)
            {
                List<KeyValuePair<Rectangle, Actor>> list = GetPossibleCollisions(actor, dir);
                ChangeDir(actor, list, ref dir, ref impeded);

                Rectangle newBox = actor.CollisionBox;
                newBox.Offset(actor.ProjectedMovement(dir));

                if (!OnMap(newBox))
                {
                    rv = false;
                    dir = Vector2.Zero;
                    actor.SetMoveTo(Point.Zero);
                }
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
        public bool CheckForMapChange(Actor c, Rectangle movingChar)
        {
            if (!c.Wandering && (c.ActorType == ActorTypeEnum.Villager || c == PlayerManager.PlayerActor))
            {
                foreach (KeyValuePair<string, TravelPoint> kvp in DictionaryTravelPoints)
                {
                    if (kvp.Value.Intersects(movingChar) && !kvp.Value.IsDoor && kvp.Value.IsActive)
                    {
                        MapManager.ChangeMaps(c, this.Name, kvp.Value);
                        return true;

                        //Unused code for now since AdventureMaps are unused
                        //if (IsDungeon) { if (c == PlayerManager.World) { MapManager.ChangeDungeonRoom(kvp.Value); return true; } }
                    }
                }
            }
            return false;
        }

        #region Collision Helpers
        public float CheckToNudge(float movingCenter, float objCenter, float varCol, float varRow, string v)
        {
            float rv = 0;
            float centerDelta = movingCenter - objCenter;
            if (varCol == -1 && varRow == -1)
            {
                if (centerDelta > 0) { rv = Constants.NUDGE_RATE; }
                else if (centerDelta < 0) { rv = -Constants.NUDGE_RATE; }
            }
            else if (centerDelta > 0)
            {
                RHTile testTile = GetTileByGridCoords((int)(varCol + (v.Equals("Col") ? 1 : 0)), (int)(varRow + (v.Equals("Row") ? 1 : 0)));
                if (testTile != null && testTile.Passable())
                {
                    rv = Constants.NUDGE_RATE;
                }
            }
            else if (centerDelta < 0)
            {
                RHTile testTile = GetTileByGridCoords((int)(varCol - (v.Equals("Col") ? 1 : 0)), (int)(varRow - (v.Equals("Row") ? 1 : 0)));
                if (testTile != null && testTile.Passable()) { rv = -Constants.NUDGE_RATE; }
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
            MapItem displayItem = new MapItem(DataManager.GetItem(itemID))
            {
                PickupState = ItemPickupState.None,
                Position = DictionaryCharacterLayer[npcIndex + "Col" + index]
            };
            _liItems.Add(displayItem);
        }

        public Dictionary<string, string> GetMapProperties()
        {
            return _map.Properties;
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
                            if (!propList.ContainsKey(tp.Key))
                            {
                                propList.Add(tp.Key, tp.Value);
                            }
                        }
                    }
                }
            }
            return propList;
        }

        public Point GetCharacterSpawn(string val)
        {
            Point rv = Point.Zero;
            if (DictionaryCharacterLayer.ContainsKey(val))
            {
                rv = DictionaryCharacterLayer[val];
            }
            return rv;
        }

        #region Input Processing
        public bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (GamePaused()) { return false; }

            RHTile tile = MouseTile;

            if (PlayerManager.PlayerActor.Mounted && (tile.GetTravelPoint() == null || !PlayerManager.PlayerActor.ActiveMount.CanEnterBuilding(tile.GetTravelPoint().LinkedMap)))
            {
                PlayerManager.PlayerActor.Dismount();
                return true;
            }

            //Do nothing if no tile could be retrieved
            if (tile == null) { return rv; }

            rv = tile.ProcessRightClick();

            TheShop?.Interact(this, mouseLocation);

            foreach (Actor c in _liActors)
            {
                if (PlayerManager.PlayerInRange(c.HoverBox, (int)(Constants.TILE_SIZE * 1.5)) && c.HoverContains(mouseLocation) && c.OnTheMap)
                {
                    c.ProcessRightButtonClick();
                    return true;
                }
            }

            var removedList = new List<MapItem>();
            for (int i = 0; i < _liItems.Count; i++)
            {
                MapItem it = _liItems[i];
                if (it.PickupState == ItemPickupState.Manual && it.CollisionBox.Contains(GUICursor.GetWorldMousePosition()) && PlayerManager.PlayerInRange(it.CollisionBox))
                {
                    if (InventoryManager.AddToInventory(it.WrappedItem))
                    {
                        removedList.Add(it);
                        break;
                    }
                }
            }
            removedList.ForEach(x => _liItems.Remove(x));
            removedList.Clear();

            if (Scrying())
            {
                SetGameScale(Constants.NORMAL_SCALE);
                GameManager.DropWorldObject();
                LeaveTownMode();
                Scry(false);
                ResetCamera();
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

            if (!PlayerManager.Busy && !CutsceneManager.Playing)
            {
                //Ensure that we have a tile that we clicked on and that the player is close enough to interact with it.
                TargetTile = MouseTile;

                if (InTownMode())
                {
                    rv = TownModeLeftClick(mouseLocation);
                }
                else
                {
                    if (GamePaused()) { return false; }

                    Item i = InventoryManager.GetCurrentItem();

                    if (i != null && i.HasUse())
                    {
                        int distance = 0;
                        if (PlayerManager.PlayerInRangeGetDist(GUICursor.GetWorldMousePosition(), 3 * Constants.TILE_SIZE, ref distance))
                        {
                            PlayerManager.PlayerActor.DetermineFacing(MapManager.CurrentMap.GetTileByPixelPosition(GUICursor.GetWorldMousePosition()));
                        }

                        RHTile playerTile = GetTileByPixelPosition(PlayerManager.PlayerActor.CollisionCenter);
                        TargetTile = playerTile.GetTileByDirection(PlayerManager.PlayerActor.Facing);

                        i.ItemBeingUsed();
                    }
                    else
                    {
                        if (TargetTile != null && TargetTile.PlayerIsAdjacent())
                        {
                            //Retrieves any object associated with the tile, this will include
                            //both actual tiles, and Shadow Tiles because the user sees Shadow Tiles
                            //as being on the tile.
                            WorldObject obj = TargetTile.GetWorldObject(false);
                            if (obj != null)
                            {
                                rv = obj.ProcessLeftClick();
                            }

                            if (!rv && !TargetTile.Passable())
                            {
                                PlayerManager.GrabTile(TargetTile);
                            }
                        }
                    }
                }
            }

            return rv;
        }

        public bool ProcessHover(Point mouseLocation)
        {
            bool rv = false;

            if (_liTestTiles.Count > 0) { _liTestTiles.Clear(); }

            _objSelectedObject?.SelectObject(false);
            if ((TownModeMoving() || TownModeDestroy() || TownModeStorage()) && GameManager.HeldObject == null && MouseTile != null && MouseTile.HasBuildableObject())
            {
                WorldObject obj = MouseTile.RetrieveUppermostStructureObject();
                if (obj != null && obj.IsBuildable())
                {
                    _objSelectedObject = (Buildable)obj;
                    _objSelectedObject.SelectObject(true);
                }
            }

            if (TownModeUpgrade() && GameManager.HeldObject == null && MouseTile != null && MouseTile.HasBuildableObject())
            {
                WorldObject obj = MouseTile.RetrieveUppermostStructureObject();
                if (obj != null && obj.CompareType(ObjectTypeEnum.Building))
                {
                    _objSelectedObject = (Buildable)obj;
                    _objSelectedObject.SelectObject(true);
                }
            }

            if (Scrying())
            {
                _liTestTiles = new List<RHTile>();
                if (GameManager.HeldObject != null)
                {
                    TestMapTiles(GameManager.HeldObject, _liTestTiles);
                }
            }
            else
            {
                bool found = false;

                RHTile t = GetTileByPixelPosition(GUICursor.GetWorldMousePosition());
                if (t != null)
                {
                    if (t.GetTravelPoint() != null)
                    {
                        found = true;
                        GUICursor.SetCursor(GUICursor.CursorTypeEnum.Door, t.GetTravelPoint().CollisionBox);
                    }
                    else if (t.GetWorldObject(false) != null && t.GetWorldObject().CanPickUp())
                    {
                        found = true;
                        GUICursor.SetCursor(GUICursor.CursorTypeEnum.Pickup, t.GetWorldObject().CollisionBox);
                    }

                    MapItem hoverItem = _liItems.Find(x => x.CollisionBox.Contains(GUICursor.GetWorldMousePosition()));
                    if (hoverItem != null && hoverItem.PickupState == ItemPickupState.Manual)
                    {
                        found = true;
                        GUICursor.SetCursor(GUICursor.CursorTypeEnum.Pickup, hoverItem.CollisionBox);
                    }
                }

                foreach (Actor c in _liActors)
                {
                    if (c.HoverContains(mouseLocation) && c.OnTheMap)
                    {
                        switch (c.ActorType)
                        {
                            case ActorTypeEnum.Merchant:
                            case ActorTypeEnum.ShippingGremlin:
                            case ActorTypeEnum.Spirit:
                            case ActorTypeEnum.TalkingActor:
                            case ActorTypeEnum.Traveler:
                            case ActorTypeEnum.Villager:
                                GUICursor.SetCursor(GUICursor.CursorTypeEnum.Talk, c.HoverBox);
                                found = true;
                                break;
                        }
                        if (found) { break; }
                    }
                }

                if (!found)
                {
                    GUICursor.ResetCursor();
                }
            }

            return rv;
        }

        #region Town Mode handling
        /// <summary>
        /// Handles TownMode interactions
        /// </summary>
        /// <param name="mouseLocation">Location of the mouse</param>
        /// <returns>True if we successfully interacted with an object</returns>
        private bool TownModeLeftClick(Point mouseLocation)
        {
            bool rv = false;

            //Are we constructing or moving a building?
            if (TownModeBuild() || TownModeMoving())
            {
                Buildable toBuild = (Buildable)GameManager.HeldObject;

                //If we are holding a WorldObject, we should attempt to place it
                if (GameManager.HeldObject != null)
                {
                    switch (toBuild.Type)
                    {
                        case ObjectTypeEnum.Building:
                        case ObjectTypeEnum.Mailbox:
                        case ObjectTypeEnum.Structure:
                            rv = PlaceSingleObject(toBuild);
                            break;
                        case ObjectTypeEnum.Floor:
                            if (MouseTile.Flooring == null) { goto case ObjectTypeEnum.Wall; }
                            break;
                        case ObjectTypeEnum.Wallpaper:
                            rv = PlaceWallpaper((Wallpaper)toBuild);
                            break;
                        case ObjectTypeEnum.Beehive:
                        case ObjectTypeEnum.Buildable:
                        case ObjectTypeEnum.Container:
                        case ObjectTypeEnum.Decor:
                        case ObjectTypeEnum.Garden:
                        case ObjectTypeEnum.Wall:
                            rv = PlaceMultiObject(toBuild);
                            break;
                    }
                    SoundManager.PlayEffect(rv ? "thump3" : "Cancel");
                }
                else if (GameManager.HeldObject == null)
                {
                    if (MouseTile.HasBuildableObject())
                    {
                        WorldObject targetObj = MouseTile.RetrieveUppermostStructureObject();
                        if (targetObj != null)
                        {
                            switch (targetObj.Type)
                            {
                                case ObjectTypeEnum.Building:
                                    RemoveDoor((Building)targetObj);
                                    goto case ObjectTypeEnum.Structure;
                                case ObjectTypeEnum.Beehive:
                                case ObjectTypeEnum.Buildable:
                                case ObjectTypeEnum.Container:
                                case ObjectTypeEnum.Decor:
                                case ObjectTypeEnum.Floor:
                                case ObjectTypeEnum.Garden:
                                case ObjectTypeEnum.Mailbox:
                                case ObjectTypeEnum.Structure:
                                case ObjectTypeEnum.Wall:
                                    PickUpWorldObject(mouseLocation, targetObj);
                                    break;
                            }
                            rv = true;
                        }
                    }
                }
            }
            else if (TownModeDestroy() || TownModeStorage())
            {
                if (MouseTile != null && MouseTile.HasBuildableObject())
                {
                    WorldObject toRemove = MouseTile.RetrieveUppermostStructureObject();

                    switch (toRemove.Type)
                    {
                        case ObjectTypeEnum.Decor:
                            if (((Decor)toRemove).HasDisplay) { ((Decor)toRemove).RemoveDisplayEntity(); }
                            else { goto case ObjectTypeEnum.Wall; }
                            break;
                        case ObjectTypeEnum.Beehive:
                        case ObjectTypeEnum.Buildable:
                        case ObjectTypeEnum.Container:
                            if (((Container)toRemove).HasItem()) { break; }
                            else { goto case ObjectTypeEnum.Wall; }
                        case ObjectTypeEnum.Floor:
                        case ObjectTypeEnum.Garden:
                        case ObjectTypeEnum.Wall:
                            SoundManager.PlayEffect("buildingGrab");
                            RemoveWorldObject(toRemove);
                            if (TownModeDestroy())
                            {
                                foreach (KeyValuePair<int, int> kvp in ((Buildable)toRemove).RequiredToMake)
                                {
                                    InventoryManager.AddToInventory(kvp.Key, kvp.Value);
                                }
                            }
                            else if (TownModeStorage())
                            {
                                TownManager.AddToStorage(toRemove.ID);
                            }
                            break;
                    }
                }
            }
            else if (TownModeUpgrade())
            {
                if (MouseTile != null && MouseTile.HasBuildableObject())
                {
                    WorldObject obj = MouseTile.RetrieveUppermostStructureObject();

                    if (obj.CompareType(ObjectTypeEnum.Building))
                    {
                        GUIManager.OpenMainObject(new HUDBuildingUpgrade((Building)obj));
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// Helper method to process the placement of a singular object
        /// </summary>
        /// <param name="toBuild">The Structure object we are placing down.</param>
        /// <returns>True if we successfully build.</returns>
        private bool PlaceSingleObject(Buildable toBuild)
        {
            bool rv = false;

            if (TownModeMoving() || GameManager.BuildFromStorage || PlayerManager.ExpendResources(toBuild.RequiredToMake))
            {
                toBuild.SnapPositionToGrid(toBuild.CollisionBox.Location);
                if (toBuild.PlaceOnMap(this))
                {
                    if (this == MapManager.TownMap)
                    {
                        TownManager.AddToTownObjects(toBuild);
                    }

                    //Drop the Building from the GameManger
                    GameManager.DropWorldObject();
                    ClearHeldLights();

                    //Only leave TownMode if we were in Build Mode
                    if (TownModeBuild())
                    {
                        if (GameManager.BuildFromStorage) { TownManager.RemoveFromStorage(toBuild.ID); }
                        TaskManager.AdvanceTaskProgress(toBuild);

                        LeaveTownMode();
                        FinishBuilding();
                    }
                    rv = true;
                }
            }

            return rv;
        }

        /// <summary>
        /// Helper method for building one object and allowing the
        /// possibility of building additional ones
        /// </summary>
        /// <param name="templateObject">The Structure that will act as the template to build offo f</param>
        /// <returns>True if we successfully build.</returns>
        private bool PlaceMultiObject(Buildable templateObject)
        {
            bool rv = false;
            Buildable placeObject;

            //If we're moving the object, set it as the object to be placed. Otherwise, we need
            //to make a new object based off theo ne we're holding.
            if (TownModeMoving()) { placeObject = templateObject; }
            else
            {
                placeObject = (Buildable)DataManager.CreateWorldObjectByID(templateObject.ID);
                if (templateObject.CompareType(ObjectTypeEnum.Decor))
                {
                    ((Decor)placeObject).RotateToDirection(((Decor)templateObject).Facing);
                }
            }

            //PlaceOnMap uses the CollisionBox as the base, then calculates backwards
            placeObject.SnapPositionToGrid(templateObject.CollisionBox.Location);

            if (placeObject.PlaceOnMap(this) && (TownModeMoving() || GameManager.BuildFromStorage || PlayerManager.ExpendResources(placeObject.RequiredToMake)))
            {
                if (this == MapManager.TownMap)
                {
                    TownManager.AddToTownObjects(placeObject);
                }

                if (GameManager.BuildFromStorage) { TownManager.RemoveFromStorage(placeObject.ID); }

                switch (placeObject.Type)
                {
                    case ObjectTypeEnum.Garden:
                        ((Garden)placeObject).SetPlant(((Garden)placeObject).GetPlant());
                        goto case ObjectTypeEnum.Wall;
                    case ObjectTypeEnum.Floor:
                    case ObjectTypeEnum.Wall:
                        ((AdjustableObject)placeObject).AdjustObject();
                        break;
                }

                //If we cannot build the WorldObject again due to lack of resources or running out of storage,
                //clean up after ourselves and drop the WorldObject that we're holding
                //If we're moving, we need to drop the object but do not leave build mode
                if (TownModeMoving() || (GameManager.BuildFromStorage && !TownManager.HasInStorage(placeObject.ID)) || (!GameManager.BuildFromStorage && !InventoryManager.HasSufficientItems(placeObject.RequiredToMake)))
                {
                    if (!TownModeMoving())
                    {
                        LeaveTownMode();
                        FinishBuilding();
                    }

                    GameManager.DropWorldObject();
                    ClearHeldLights();
                }

                rv = true;
            }

            return rv;
        }

        private bool PlaceWallpaper(Wallpaper wallpaperObject)
        {
            bool rv = false;

            if (TargetTile.IsWallpaperWall && PlayerManager.ExpendResources(wallpaperObject.RequiredToMake))
            {
                rv = true;

                List<RHTile> usedTiles = new List<RHTile>();
                List<RHTile> wallpaperTiles = new List<RHTile> { TargetTile };
                while (wallpaperTiles.Count > 0)
                {
                    RHTile tile = wallpaperTiles[0];
                    Wallpaper paper = (Wallpaper)DataManager.CreateWorldObjectByID(wallpaperObject.ID);
                    paper.SnapPositionToGrid(tile.Position);
                    tile.SetWallpaper(paper);
                    wallpaperTiles.AddRange(tile.GetAdjacentTiles().FindAll(x => x.IsWallpaperWall && !usedTiles.Contains(x)));

                    wallpaperTiles.Remove(tile);
                    usedTiles.Add(tile);
                }
            }

            return rv;
        }
        #endregion

        #endregion

        public void FinishBuilding()
        {
            SetGameScale(Constants.NORMAL_SCALE);

            //Re-open the Building Menu
            GUIManager.OpenMenu();
        }

        public void RemoveWorldObject(WorldObject o, bool immediately = false)
        {
            if (immediately) { _liPlacedWorldObjects.Remove(o); }
            else { _liObjectsToRemove.Add(o); }
            o.RemoveSelfFromTiles();
        }
        public void RemoveActor(Actor c)
        {
            Util.AddUniquelyToList(ref _liActorsToRemove, c);
        }

        public void SpawnItemsOnMap(List<Item> items, Point position, bool flyingPop = true)
        {
            foreach (Item i in items)
            {
                SpawnItemOnMap(i, position, flyingPop);
            }
        }

        public void SpawnItemOnMap(Item item, Point position, bool flyingPop, ItemPickupState pickupState = ItemPickupState.Auto)
        {
            MapItem mItem = new MapItem(item)
            {
                PickupState = pickupState
            };

            if (flyingPop)
            {
                mItem.Pop(position);
            }
            else
            {
                mItem.Position = position;
            }

            _liItems.Add(mItem);
        }

        public void LayerVisible(string name, bool val)
        {
            foreach (TiledMapTileLayer layer in _map.TileLayers)
            {
                if (layer.Name == name)
                {
                    layer.IsVisible = val;
                    break;
                }
            }
        }

        #region Adders
        /// <summary>
        /// Look at the tile the mouse is over and, if the tile has an object present,
        /// pick it up.
        /// 
        /// We prioritize picking up a ShadowObject so we grab whatever is in front, instead
        /// of picking up objects hiding behind a building.
        /// </summary>
        /// <param name="mouseLocation"></param>
        public void PickUpWorldObject(Point mouseLocation, WorldObject targetObj)
        {
            SoundManager.PlayEffect("buildingGrab");
            GameManager.PickUpWorldObject(targetObj);
            targetObj.SetPickupOffset(mouseLocation.ToVector2());
            targetObj.RemoveSelfFromTiles();
            RemoveLights(targetObj.GetLights());
            AddHeldLights(targetObj.GetLights());
        }

        public bool PlaceWorldObject(WorldObject o, bool ignoreActors = false)
        {
            bool rv = false;

            List<RHTile> tiles = new List<RHTile>();
            if (TestMapTiles(o, tiles, ignoreActors))
            {
                AssignMapTiles(o, tiles);
                rv = true;
            }

            return rv;
        }

        /// <summary>
        /// Given a World Object item, determine which tiles on the map collide with
        /// the defined CollisionBox of the WorldObject.
        /// </summary>
        /// <param name="obj">The WorldObject to test</param>
        /// <param name="collisionTiles">The Tiles on the map that are in the object's CollisionBox</param>
        /// <returns></returns>
        public bool TestMapTiles(WorldObject obj, List<RHTile> collisionTiles, bool ignoreActors = false)
        {
            bool rv = true;
            collisionTiles.Clear();
            Vector2 position = obj.CollisionBox.Location.ToVector2();
            position.X = ((int)(position.X / Constants.TILE_SIZE)) * Constants.TILE_SIZE;
            position.Y = ((int)(position.Y / Constants.TILE_SIZE)) * Constants.TILE_SIZE;

            int colColumns = obj.CollisionBox.Width / Constants.TILE_SIZE;
            int colRows = obj.CollisionBox.Height / Constants.TILE_SIZE;

            //This is used to get all the tiles based off the collisonbox size
            for (int i = 0; i < colRows; i++)
            {
                for (int j = 0; j < colColumns; j++)
                {
                    int x = Math.Min((obj.CollisionBox.Left + (j * Constants.TILE_SIZE)) / Constants.TILE_SIZE, MapWidthTiles - 1);
                    int y = Math.Min((obj.CollisionBox.Top + (i * Constants.TILE_SIZE)) / Constants.TILE_SIZE, MapHeightTiles - 1);
                    if (x < 0 || x > this.MapWidthTiles || y < 0 || y > this.MapHeightTiles)
                    {
                        rv = false;
                        break;
                    }

                    RHTile tempTile = _arrTiles[x, y];
                    if (CanPlaceObject(tempTile, obj, ignoreActors))
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
        /// Method to call to make it easier to parse if a given WorldObject can be placed.
        /// </summary>
        /// <param name="testTile">The RHtile to test</param>
        /// <param name="obj">The WorldObject we want to place</param>
        /// <returns></returns>
        private bool CanPlaceObject(RHTile testTile, WorldObject obj, bool ignoreActors = false)
        {
            bool rv = false;
            //We can place flooring anywhere there isn't flooring as long as the base tile is passable.
            if (obj.CompareType(ObjectTypeEnum.Floor))
            {
                if (testTile.Flooring == null && (testTile.Passable()
                    || testTile.WorldObject.CompareType(ObjectTypeEnum.Building)))
                {
                    rv = true;
                }
            }
            else if (obj.WallObject())
            {
                rv = (testTile.WorldObject == null && testTile.IsWallpaperWall);
            }
            else if (ignoreActors || !TileContainsBlockingActor(testTile))
            {
                if (testTile.CanPlaceOnTabletop(obj) || (testTile.Passable() && testTile.WorldObject == null))
                {
                    rv = true;
                }
            }

            if (obj.WideOnTop())
            {
                List<RHTile> arr = testTile.GetAdjacentTiles(true);
                for(int i =0; i < arr.Count; i++)
                {
                    if(DataManager.GetBoolByIDKey(obj.ID, "Tree", DataType.WorldObject) && (!arr[i].TileIsPassable()) || (arr[i].WorldObject != null && arr[i].WorldObject.WideOnTop()))
                    {
                        rv = false;
                        break;
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
        /// <param name="obj">The object to add</param>
        /// <param name="tiles">The list of tiles to add to the object</param>
        public void AssignMapTiles(WorldObject obj, List<RHTile> tiles)
        {
            tiles.FindAll(t => !obj.Tiles.Contains(t)).ForEach(t => obj.Tiles.Add(t));

            if (!_liPlacedWorldObjects.Contains(obj))
            {
                _liPlacedWorldObjects.Add(obj);
            }

            //Sets the WorldObject to each RHTile
            tiles.ForEach(x => x.SetObject(obj));

            //Iterate over the WorldObject image in Constants.TILE_SIZE increments to discover any tiles
            //that the image overlaps. Add those tiles as Shadow Tiles as long as they're not
            //actual Tiles the object sits on. Also add the Tiles to the objects Shadow Tiles list
            for (int i = obj.MapPosition.X; i < obj.MapPosition.X + obj.Width; i += Constants.TILE_SIZE)
            {
                for (int j = obj.MapPosition.Y; j < obj.MapPosition.Y + obj.Height; j += Constants.TILE_SIZE)
                {
                    RHTile t = GetTileByGridCoords(Util.GetGridCoords(i, j));
                    if (t != null && !obj.Tiles.Contains(t))
                    {
                        t.SetShadowObject(obj);
                        obj.AddTile(t);
                    }
                }
            }

            var actors = _liActors.FindAll(x => obj.CollisionBox.Contains(x.CollisionBox) && x.IsActorType(ActorTypeEnum.Critter)).Cast<Critter>().ToList();
            actors.ForEach(x => x.Flee());

        }

        public void AddActor(Actor c)
        {
            ToAdd.Add(c);
        }

        public bool RemoveCharacterImmediately(Actor c)
        {
            bool rv = false;
            if (MapManager.Maps[c.CurrentMapName].ContainsActor(c))
            {
                rv = true;
                if (c.IsActorType(ActorTypeEnum.Mob) && _liMobs.Contains((Mob)c)) { _liMobs.Remove((Mob)c); }
                else if (_liActors.Contains(c)) { _liActors.Remove(c); }

                if (c.IsActorType(ActorTypeEnum.Merchant))
                {
                    _iShopID = -1;
                }
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
        public bool AddCharacterImmediately(Actor c)
        {
            bool rv = false;
            if (string.IsNullOrEmpty(c.CurrentMapName) || !MapManager.Maps[c.CurrentMapName].ContainsActor(c))
            {
                rv = true;

                if (c.IsActorType(ActorTypeEnum.Mob) && !_liMobs.Contains((Mob)c)) { _liMobs.Add((Mob)c); }
                else { Util.AddUniquelyToList(ref _liActors, c); }

                c.CurrentMapName = _sName;
                c.SetPosition(c.NewMapPosition == Point.Zero ? c.CollisionBoxLocation : c.NewMapPosition);
                c.NewMapPosition = Point.Zero;

                if (c.IsActorType(ActorTypeEnum.Merchant))
                {
                    _iShopID = ((Merchant)c).ShopID;
                }
            }

            return rv;
        }

        //public void AddMonster(TacticalMonster m)
        //{
        //    bool rv = false;

        //    RHRandom rand = RHRandom.Instance();
        //    Vector2 position = m.Position;
        //    position.X = ((int)(position.X / Constants.TILE_SIZE)) * Constants.TILE_SIZE;
        //    position.Y = ((int)(position.Y / Constants.TILE_SIZE)) * Constants.TILE_SIZE;

        //    rv = _arrTiles[((int)position.X / Constants.TILE_SIZE), ((int)position.Y / Constants.TILE_SIZE)].Passable();
        //    if (!rv)
        //    {
        //        do
        //        {
        //            position.X = (int)(rand.Next(1, (MapWidthTiles - 1) * Constants.TILE_SIZE) / Constants.TILE_SIZE) * Constants.TILE_SIZE;
        //            position.Y = (int)(rand.Next(1, (MapHeightTiles - 1) * Constants.TILE_SIZE) / Constants.TILE_SIZE) * Constants.TILE_SIZE;
        //            rv = _arrTiles[((int)position.X / Constants.TILE_SIZE), ((int)position.Y / Constants.TILE_SIZE)].Passable();
        //        } while (!rv);
        //    }

        //    if (rv)
        //    {
        //        AddMonsterByPosition(m, position);
        //    }
        //}

        public void AddMobByPosition(Mob m, Point position)
        {
            m.CurrentMapName = _sName;
            m.SetPosition(Util.SnapToGrid(position));

            _liMobs.Add(m);
        }

        public void AddBuilding(Building b)
        {
            if (!_liBuildings.Contains(b))
            {
                _liBuildings.Add(b);
            }
        }
        #endregion

        public List<TiledMapObject> GetMapObjectsByName(string name)
        {
            return _liMapObjects.FindAll(x => x.Name.Equals(name));
        }

        public TiledMapObject GetMapObjectByTagAndValue(string tag, string value)
        {
            return _liMapObjects.Find(x => x.Properties.ContainsKey(tag) && x.Properties[tag].Equals(value));
        }
        public int GetMapWidthInScaledPixels()
        {
            return MapWidthTiles * ScaledTileSize;
        }

        public int GetMapHeightInScaledPixels()
        {
            return MapHeightTiles * ScaledTileSize;
        }

        public void LeaveMap()
        {
            EnvironmentManager.UnloadEnvironment();
            List<Actor> copy = new List<Actor>(_liActors);
            for (int i = 0; i < copy.Count; i++)
            {
                if (copy[i].IsActorType(ActorTypeEnum.Villager))
                {
                    Villager v = (Villager)copy[i];
                    v.SendToTown();
                }
            }
        }

        public void EnterMap()
        {
            EnvironmentManager.LoadEnvironment(this);
            SoundManager.PlayBackgroundMusic();

            foreach (int cutsceneID in _liCutscenes)
            {
                CutsceneManager.CheckForTriggedCutscene(cutsceneID);
            }
        }

        /// <summary>
        /// Returns the RHTile the mouse is pointing to
        /// </summary>
        /// <returns></returns>
        public RHTile GetMouseOverTile()
        {
            return GetTileByPixelPosition(GUICursor.GetWorldMousePosition());
        }
        public RHTile GetTileByPixelPosition(Point targetLoc)
        {
            return GetTileByPixelPosition(targetLoc.X, targetLoc.Y);
        }
        public RHTile GetTileByPixelPosition(int x, int y)
        {
            if (x >= MapWidthTiles * Constants.TILE_SIZE || x < 0) { return null; }
            if (y >= MapHeightTiles * Constants.TILE_SIZE || y < 0) { return null; }

            return _arrTiles[x / Constants.TILE_SIZE, y / Constants.TILE_SIZE];
        }

        public RHTile GetTileByGridCoords(Point targetLoc)
        {
            return GetTileByGridCoords(targetLoc.X, targetLoc.Y);
        }
        public RHTile GetTileByGridCoords(int x, int y)
        {
            RHTile tile = null;

            if (x >= 0 && x < MapWidthTiles && y >= 0 && y < MapHeightTiles)
            {
                tile = _arrTiles[x, y];
            }

            return tile;
        }

        public List<RHTile> GetAllTilesInRange(RHTile startTile, int range)
        {
            List<RHTile> rv = new List<RHTile>();
            int startX = startTile.X - range;
            int startY = startTile.Y - range;

            int dimensions = ((range * 2) + 1);
            foreach (Point vec in Util.GetAllPointsInArea(startX, startY, dimensions, dimensions))
            {
                RHTile tile = GetTileByGridCoords(vec);
                if (tile != null)
                {
                    if (Util.GetRHTileDelta(startTile, tile) <= range)
                    {
                        rv.Add(tile);
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// Returns a list of all RHTiles that exist
        /// </summary>
        /// <param name="obj">The Rectangle to check against</param>
        /// <returns>A list of all RHTiles that exist in the Rectangle</returns>
        public List<RHTile> GetTilesFromRectangleExcludeEdgePoints(Rectangle obj)
        {
            List<RHTile> rvList = new List<RHTile>();

            for (int y = obj.Top; y < obj.Top + obj.Height; y += Constants.TILE_SIZE)
            {
                for (int x = obj.Left; x < obj.Left + obj.Width; x += Constants.TILE_SIZE)
                {
                    RHTile tile = GetTileByPixelPosition(new Point(x, y));
                    if (!rvList.Contains(tile))
                    {
                        rvList.Add(tile);
                    }
                }
            }

            return rvList;
        }

        public List<RHTile> GetTilesFromRectangleIncludeEdgePoints(Rectangle obj)
        {
            List<RHTile> rvList = new List<RHTile>();

            for (int y = obj.Top; y <= obj.Top + obj.Height; y += Constants.TILE_SIZE)
            {
                for (int x = obj.Left; x <= obj.Left + obj.Width; x += Constants.TILE_SIZE)
                {
                    RHTile tile = GetTileByPixelPosition(new Point(x, y));
                    if (!rvList.Contains(tile))
                    {
                        rvList.Add(tile);
                    }
                }
            }

            return rvList;
        }

        public Point GetRandomPosition()
        {
            return GetRandomPosition(new Rectangle(0, 0, MapWidthTiles * Constants.TILE_SIZE, MapHeightTiles * Constants.TILE_SIZE));
        }
        public Point GetRandomPosition(Rectangle r)
        {
            Point rv = Point.Zero;

            List<RHTile> tiles = GetTilesFromRectangleExcludeEdgePoints(r);
            do
            {
                RHTile tile = tiles[RHRandom.Instance().Next(tiles.Count)];
                if (tile.Passable() && !TileContainsBlockingActor(tile))
                {
                    rv = tile.Position;
                    break;
                }
                else { tiles.Remove(tile); }

            } while (tiles.Count > 0);

            return rv;
        }

        public List<RHTile> GetAllTiles(bool passableOnly)
        {
            List<RHTile> tileList = new List<RHTile>();

            foreach (RHTile tile in _arrTiles)
            {
                if (!passableOnly || tile.Passable())
                {
                    tileList.Add(tile);
                }
            }
            return tileList;
        }

        public bool MapAboveValid()
        {
            return !string.IsNullOrEmpty(MapAbove) && MapAbove != MapManager.CurrentMap.Name;
        }

        public bool MapBelowValid()
        {
            return !string.IsNullOrEmpty(MapBelow) && MapBelow != MapManager.CurrentMap.Name;
        }

        public bool OnMap(Rectangle box)
        {
            bool rv = false;
            Rectangle map = new Rectangle(0, 0, Map.WidthInPixels, Map.HeightInPixels);
            if (map.Contains(box))
            {
                rv = true;
            }

            return rv;
        }

        internal MapData SaveData()
        {
            MapData mapData = new MapData
            {
                mapName = this.Name,
                visited = this.Visited,
                worldObjects = new List<WorldObjectData>()
            };

            if (!IsDungeon)
            {
                foreach (WorldObject wObj in _liPlacedWorldObjects)
                {
                    mapData.worldObjects.Add(wObj.SaveData());
                }
            }

            return mapData;
        }
        internal void LoadData(MapData mData)
        {
            Visited = mData.visited;
            foreach (WorldObjectData data in mData.worldObjects)
            {
                WorldObject obj = DataManager.CreateWorldObjectByID(data.ID);
                if (data.ID == -1)
                {
                    obj = new WrappedItem(int.Parse(data.stringData));
                }

                if (obj != null)
                {
                    obj.LoadData(data);
                    obj.PlaceOnMap(this);
                    if (this == MapManager.TownMap) { TownManager.AddToTownObjects(obj); }
                }

                //obj?.PlaceOnMap(new Vector2(w.x, w.y), this);
                //if (obj != null && this == MapManager.TownMap) { TownManager.AddToTownObjects(obj); }
            }
        }
    }
}
