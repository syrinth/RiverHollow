using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
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

        public string DungeonName { get; private set; } = string.Empty;
        public bool IsDungeon => !string.IsNullOrEmpty(DungeonName);
        public bool IsTown => _map.Properties.ContainsKey("Town");
        public int BuildingID => _map.Properties.ContainsKey("BuildingID") ? int.Parse(_map.Properties["BuildingID"]) : -1;
        public bool Modular => _map.Properties.ContainsKey("Modular");
        public bool IsOutside => _map.Properties.ContainsKey("Outside");
        public string MapType => _map.Properties.ContainsKey("MapType") ? _map.Properties["MapType"] : string.Empty;
        bool _bSpawned = false;
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
        private List<WorldActor> _liActors;
        protected List<Mob> _liMobs;
        public List<WorldActor> ToAdd;
        private List<Building> _liBuildings;
        private List<WorldObject> _liPlacedWorldObjects;
        private List<ResourceSpawn> _liResourceSpawns;
        private List<MobSpawn> _liMobSpawns;
        private List<int> _liCutscenes;
        private Dictionary<RarityEnum, List<int>> _diResources;
        protected List<Item> _liItems;
        protected List<ShopLocation> _liShopData;
        public Dictionary<string, TravelPoint> DictionaryTravelPoints { get; }
        public Dictionary<string, Vector2> DictionaryCharacterLayer { get; }
        private List<TiledMapObject> _liMapObjects;
        private List<KeyValuePair<Rectangle, string>> _liClickObjects;

        private List<Item> _liItemsToRemove;
        private List<WorldActor> _liActorsToRemove;
        private List<WorldObject> _liObjectsToRemove;

        Buildable _objSelectedObject = null;

        public RHMap()
        {
            _liResourceSpawns = new List<ResourceSpawn>();
            _liMobSpawns = new List<MobSpawn>();
            _liWallTiles = new List<RHTile>();
            _liTestTiles = new List<RHTile>();
            _liTilesets = new List<TiledMapTileset>();
            _liActors = new List<WorldActor>();
            _liMobs = new List<Mob>();
            _liBuildings = new List<Building>();
            _liItems = new List<Item>();
            _liMapObjects = new List<TiledMapObject>();
            _liClickObjects = new List<KeyValuePair<Rectangle, string>>();
            _liShopData = new List<ShopLocation>();
            _liPlacedWorldObjects = new List<WorldObject>();
            _liLights = new List<Light>();
            _liHeldLights = new List<Light>();
            _liCutscenes = new List<int>();
            _diResources = new Dictionary<RarityEnum, List<int>>();

            DictionaryTravelPoints = new Dictionary<string, TravelPoint>();
            DictionaryCharacterLayer = new Dictionary<string, Vector2>();

            _liItemsToRemove = new List<Item>();
            _liActorsToRemove = new List<WorldActor>();
            _liObjectsToRemove = new List<WorldObject>();

            ToAdd = new List<WorldActor>();
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
                DungeonManager.AddMapToDungeon(_map.Properties["Dungeon"], this);
            }

            _liBuildings = map._liBuildings;
            _liPlacedWorldObjects = map._liPlacedWorldObjects;
            _liLights = map._liLights;
            _liHeldLights = map._liHeldLights;

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

            if (_map.Properties.ContainsKey("Dungeon"))
            {
                DungeonName = _map.Properties["Dungeon"];
                DungeonManager.AddMapToDungeon(_map.Properties["Dungeon"], this);
            }

            if (_map.Properties.ContainsKey("Cutscenes"))
            {
                string[] split = _map.Properties["Cutscenes"].Split('|');
                foreach (string cutsceneID in split)
                {
                    _liCutscenes.Add(int.Parse(cutsceneID));
                }
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

            if (this == MapManager.CurrentMap)
            {
                _renderer.Update(_map, gTime);

                EnvironmentManager.Update(gTime);

                foreach (Mob m in _liMobs)
                {
                    m.Update(gTime);
                }

                foreach (Item i in _liItems)
                {
                    ((Item)i).Update(gTime);
                }
            }
        
            foreach (WorldObject obj in _liObjectsToRemove)
            {
                _liPlacedWorldObjects.Remove(obj);
            }
            _liObjectsToRemove.Clear();

            foreach (WorldObject obj in _liPlacedWorldObjects) { obj.Update(gTime); }

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

            foreach (WorldActor c in _liActorsToRemove)
            {
                switch (c.ActorType)
                {
                    case WorldActorTypeEnum.Mob:
                        _liMobs.Remove((Mob)c);
                        break;
                    default:
                        _liActors.Remove(c);
                        break;
                }
            }

            _liActorsToRemove.Clear();

            if (!GamePaused())
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

        public void DrawGround(SpriteBatch spriteBatch)
        {
            SetLayerVisibiltyByName(false, "Base");
            SetLayerVisibiltyByName(true, "Ground");
            SetLayerVisibiltyByName(false, "Upper");

            _renderer.Draw(_map, Camera._transform);

            foreach (WorldActor c in _liActors)
            {
                c.Draw(spriteBatch, true);
            }

            foreach (Mob m in _liMobs)
            {
                m.Draw(spriteBatch, true);
            }

            foreach (Building b in _liBuildings)
            {
                b.Draw(spriteBatch);
            }

            foreach (WorldObject obj in _liPlacedWorldObjects)
            {
                if (obj.Tiles.Count > 0)
                {
                    obj.Draw(spriteBatch);
                }
            }

            foreach (Item i in _liItems)
            {
                i.Draw(spriteBatch);
            }

            if (HeldObject != null && (!GameManager.GamePaused() || Scrying()))
            {
                foreach (RHTile t in _liTestTiles)
                {
                    bool passable = CanPlaceObject(t, HeldObject);
                    if (!passable || (passable && !HeldObject.CompareType(ObjectTypeEnum.Wallpaper)))
                    {
                        spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), new Rectangle((int)t.Position.X, (int)t.Position.Y, TILE_SIZE, TILE_SIZE), new Rectangle(288, 128, TILE_SIZE, TILE_SIZE), passable ? Color.Green * 0.5f : Color.Red * 0.5f, 0, Vector2.Zero, SpriteEffects.None, 99999);
                    }
                }
            }
        }

        public void DrawUpper(SpriteBatch spriteBatch)
        {
            SetLayerVisibiltyByName(false, "Base");
            SetLayerVisibiltyByName(false, "Ground");
            SetLayerVisibiltyByName(true, "Upper");
            _renderer.Draw(_map, Camera._transform);

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

            //spriteBatch.Draw(lightMask, new Vector2(PlayerManager.World.CollisionBox.Center.X - lightMask.Width / 2, PlayerManager.World.CollisionBox.Y - lightMask.Height / 2), Color.White);
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
                        if (obj.Name.Equals("Shop"))
                        {
                            _liShopData.Add(new ShopLocation(_sName, obj));
                        }
                        else if (obj.Name.Equals("Spirit"))
                        {
                            Spirit s = new Spirit(obj.Properties)
                            {
                                Position = Util.SnapToGrid(obj.Position),
                                CurrentMapName = _sName
                            };
                            GameManager.AddSpirit(s);
                            _liActors.Add(s);
                        }
                        else if (obj.Name.Equals("NPC"))
                        {
                            if (obj.Properties.ContainsKey("NPC_ID"))
                            {
                                DictionaryCharacterLayer.Add("NPC_" + obj.Properties["NPC_ID"], obj.Position);
                            }
                        }
                        else
                        {
                            DictionaryCharacterLayer.Add(obj.Name, obj.Position);
                        }
                    }
                }
                else if (ol.Name.Contains("MapObject"))
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        if (mapObject.Name.Equals("Wall"))
                        {
                            foreach (Vector2 v in Util.GetAllPointsInArea(mapObject.Position, mapObject.Size, TILE_SIZE))
                            {
                                RHTile tile = GetTileByPixelPosition(v);
                                Util.AddUniquelyToList(ref _liWallTiles, tile);
                                tile.SetWallTrue();
                            }
                        }
                        else if (mapObject.Name.StartsWith("Display"))
                        {
                            foreach (Vector2 v in Util.GetAllPointsInArea(mapObject.Position, mapObject.Size, TILE_SIZE))
                            {
                                GetTileByPixelPosition(v).SetClickAction(mapObject.Name);
                            }
                        }
                        else if (mapObject.Name.StartsWith("Resource"))
                        {
                            _liResourceSpawns.Add(new ResourceSpawn(this, mapObject));
                        }
                        else if (mapObject.Name.StartsWith("Mob")) { _liMobSpawns.Add(new MobSpawn(this, mapObject)); }
                        else { _liMapObjects.Add(mapObject); }
                    }
                }
            }
        }

        public void PopulateMap(bool loaded = false)
        {
            RHRandom rand = RHRandom.Instance();
            TiledMapProperties props = _map.Properties;
            List<int> _liMobs = new List<int>();
            List<int> resources = new List<int>();

            foreach (TiledMapObject tiledObj in _liMapObjects)
            {
                if (!loaded)
                {
                    if (tiledObj.Name.Equals("DungeonObject"))
                    {
                        TriggerObject d = DataManager.GetDungeonObject(tiledObj.Properties);

                        d.PlaceOnMap(Util.SnapToGrid(tiledObj.Position), this);
                        GameManager.AddTrigger(d);
                    }
                    else if (tiledObj.Name.Equals("WorldObject"))
                    {
                        if (!tiledObj.Properties.ContainsKey("UpgradeLevel"))
                        {
                            DataManager.CreateAndPlaceNewWorldObject(int.Parse(tiledObj.Properties["ObjectID"]), tiledObj.Position, this);
                        }
                    }
                    else if (tiledObj.Name.Equals("Floor"))
                    {
                        for (int y = (int)tiledObj.Position.Y; y < (int)tiledObj.Position.Y + tiledObj.Size.Height; y += TILE_SIZE)
                        {
                            for (int x = (int)tiledObj.Position.X; x < (int)tiledObj.Position.X + tiledObj.Size.Width; x += TILE_SIZE)
                            {
                                DataManager.CreateAndPlaceNewWorldObject(int.Parse(tiledObj.Properties["ObjectID"]), new Vector2(x, y), this);
                            }
                        }
                    }
                    else if (tiledObj.Name.Equals("Chest"))
                    {
                        Container c = (Container)DataManager.CreateWorldObjectByID(int.Parse(tiledObj.Properties["ObjectID"]));
                        if (c.PlaceOnMap(tiledObj.Position, this))
                        {

                            InventoryManager.InitContainerInventory(c.Inventory);
                            string[] holdSplit = Util.FindParams(tiledObj.Properties["Holding"]);
                            foreach (string s in holdSplit)
                            {
                                InventoryManager.AddToInventory(int.Parse(s), 1, false);
                            }
                            InventoryManager.ClearExtraInventory();
                        }
                    }
                    else if (tiledObj.Name.Equals("Building") && !loaded)
                    {
                        Building building = (Building)DataManager.CreateWorldObjectByID(int.Parse(tiledObj.Properties["BuildingID"]));
                        building.SnapPositionToGrid(tiledObj.Position);
                        building.PlaceOnMap(building.MapPosition, this);
                        PlayerManager.AddToTownObjects(building);
                    }
                    else if (tiledObj.Name.Equals("Item"))
                    {
                        new Gatherable(int.Parse(tiledObj.Properties["ItemID"])).PlaceOnMap(tiledObj.Position, this);
                    }
                }
            }
        }

        public void SpawnMapEntities()
        {
            if (!_bSpawned)
            {
                _bSpawned = true;
                SpawnResources();
                SpawnMobs();
            }
        }

        private void SpawnResources()
        {
            for (int i = 0; i < _liResourceSpawns.Count; i++)
            {
                _liResourceSpawns[i].Spawn();
            }
        }

        private void SpawnMobs()
        {
            if (Map.Properties.ContainsKey("Mobs")){
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

        ///// <summary>
        ///// Call this to make the MonsterSpawns on the map spawn their monsters
        ///// </summary>
        //private void SpawnMonsters()
        //{
        //    //Remove all monsters from the map
        //    foreach (TacticalMonster m in Monsters)
        //    {
        //        RemoveActor(m);
        //    }
        //    Monsters.Clear();

        //    if (PrimedFood != null)
        //    {
        //        //Check to see if the spawn points have been set by MonsterFood and
        //        //resets any monsters that may already be set to them.
        //        foreach (MonsterSpawn spawn in MonsterSpawnPoints)
        //        {
        //            if (spawn.IsPrimed)
        //            {
        //                spawn.Spawn();
        //            }
        //            else
        //            {
        //                spawn.ClearSpawn();
        //            }
        //        }
        //        _liItemsToRemove.Add(PrimedFood);
        //        PrimedFood = null;
        //    }
        //    else
        //    {
        //        //Copy the spawn points to a list we can safely modify
        //        List<MonsterSpawn> spawnCopy = new List<MonsterSpawn>();
        //        spawnCopy.AddRange(MonsterSpawnPoints);

        //        //Trigger x number of SpawnPoints
        //        for (int i = 0; i < _iActiveSpawnPoints; i++)
        //        {
        //            try
        //            {
        //                //Get a random Spawn Point
        //                int point = RHRandom.Instance().Next(0, spawnCopy.Count - 1);
        //                if (!spawnCopy[point].HasSpawned())
        //                {
        //                    //Trigger the Spawn point and remove it from the copied list
        //                    //so it won't be an option for future spawning.
        //                    spawnCopy[point].Spawn();
        //                    spawnCopy.RemoveAt(point);
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //            }
        //        }
        //    }
        //}

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
                    case ObjectTypeEnum.CombatHazard:
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
        /// are chosen due to proximity to monster spawn points, combat start tiles, and travel points.
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
                List<RHTile> tiles = GetTilesFromRectangle(tp.CollisionBox);
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
            for (int i = 0; i < _liPlacedWorldObjects.Count; i++) { _liPlacedWorldObjects[i].Rollover(); }
            for (int i = 0; i < _liResourceSpawns.Count; i++) { _liResourceSpawns[i].Rollover(); }

            _bSpawned = false;

            CheckSpirits();
            _liItems.Clear();
        }

        /// <summary>
        /// Creates the TravelPoint object necessary for the given Building
        /// and calls CreateDoor to add the travel info to the correct RHTiles
        /// </summary>
        /// <param name="b">The building to create the door for</param>
        public void CreateBuildingEntrance(Building b)
        {
            TravelPoint buildPoint = new TravelPoint(b, this.Name, b.ID);
            DictionaryTravelPoints.Add(b.MapName, buildPoint); //TODO: FIX THIS
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
            foreach (Vector2 vec in Util.GetAllPointsInArea((int)rectX, (int)rectY, (int)width, (int)height, TILE_SIZE))
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
            string mapName = b.MapName;
            TravelPoint pt = DictionaryTravelPoints[mapName];

            foreach (Vector2 vec in Util.GetAllPointsInArea(pt.Location.X, pt.Location.Y, pt.CollisionBox.Width, pt.CollisionBox.Height, TILE_SIZE))
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
                        DataManager.CreateAndPlaceNewWorldObject(int.Parse(tiledObj.Properties["ObjectID"]), tiledObj.Position, this);
                    }

                }
            }
        }

        public void CheckSpirits()
        {
            foreach (WorldActor c in _liActors)
            {
                if (c.IsActorType(WorldActorTypeEnum.Spirit))
                {
                    ((Spirit)c).CheckCondition();
                }
            }
        }

        public Spirit FindSpirit()
        {
            Spirit rv = null;

            foreach (WorldActor a in _liActors)
            {
                if (a.IsActorType(WorldActorTypeEnum.Spirit) && PlayerManager.PlayerInRange(a.Position.ToPoint(), 500))
                {
                    rv = (Spirit)a;
                }
            }

            return rv;
        }

        public bool ContainsActor(WorldActor c)
        {
            return _liActors.Contains(c);//|| (c.IsActorType(ActorEnum.Monster) && Monsters.Contains((TacticalMonster)c));
        }

        public void ItemPickUpdate()
        {
            WorldActor player = PlayerManager.PlayerActor;
            for (int i = 0; i < _liItems.Count; i++)
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

        #region Collision Code
        public Vector2 GetFarthestUnblockedPath(Vector2 target, WorldActor traveler) {
            Vector2 rv = Vector2.Zero;


            Vector2 rayPosition = traveler.Position;
            while (true)
            {
                Vector2 direction = Vector2.Zero;
                Util.GetMoveSpeed(rayPosition, target, traveler.BuffedSpeed, ref direction);

                rayPosition += direction;

                //Rectangles use ints for Location, cannot update a Rectangle's Location with floats
                Rectangle testCollision = Util.FloatRectangle(rayPosition.X, rayPosition.Y, traveler.CollisionBox.Width, traveler.CollisionBox.Height);
                bool passable = true;
                List<RHTile> collisionTiles = GetTilesFromRectangle(testCollision);
                for (int i = 0; i < collisionTiles.Count; i++)
                {
                    if (!collisionTiles[i].Passable())
                    {
                        passable = false;
                        break;
                    }
                }
                if (passable) { rv = rayPosition; }

                if (!passable || rayPosition.X == target.X && rayPosition.Y == target.Y)
                {
                    break;
                }
            }

            return rv;
        }

        private bool TileContainsActor(RHTile t, bool checkPlayer = true)
        {
            bool rv = false;

            if (checkPlayer && this == PlayerManager.PlayerActor.CurrentMap && PlayerManager.PlayerActor.CollisionIntersects(t.Rect)) { rv = true; }
            else
            {
                foreach (WorldActor act in _liActors)
                {
                    if (act.CollisionIntersects(t.Rect))
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
        private List<Rectangle> GetPossibleCollisions(WorldActor actor, Vector2 dir)
        {
            List<Rectangle> list = new List<Rectangle>();
            Rectangle rEndCollision = new Rectangle((int)(actor.CollisionBox.X + dir.X), (int)(actor.CollisionBox.Y + dir.Y), actor.CollisionBox.Width, actor.CollisionBox.Height);
            //The following if-blocks get the tiles that the four corners of the
            //moved CollisionBox will be inside of, based off of the movement direction.
            if (dir.X > 0)
            {
                AddTile(ref list, rEndCollision.Right, rEndCollision.Top, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Bottom, actor.CurrentMapName);
                if (rEndCollision.Height > TILE_SIZE)
                {
                    foreach (Vector2 v in Util.GetAllPointsInArea(rEndCollision.Right - 1, rEndCollision.Top, 1, rEndCollision.Height, TILE_SIZE))
                    {
                        AddTile(ref list, (int)v.X, (int)v.Y, actor.CurrentMapName);
                    }
                }
            }
            else if (dir.X < 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Top, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Left, rEndCollision.Bottom, actor.CurrentMapName);
                if (rEndCollision.Height > TILE_SIZE)
                {
                    foreach (Vector2 v in Util.GetAllPointsInArea(rEndCollision.Left - 1, rEndCollision.Top, 1, rEndCollision.Height, TILE_SIZE))
                    {
                        AddTile(ref list, (int)v.X, (int)v.Y, actor.CurrentMapName);
                    }
                }
            }

            if (dir.Y > 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Bottom, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Bottom, actor.CurrentMapName);
                if (rEndCollision.Width > TILE_SIZE)
                {
                    foreach (Vector2 v in Util.GetAllPointsInArea(rEndCollision.Left, rEndCollision.Bottom - 1, rEndCollision.Width, 1, TILE_SIZE))
                    {
                        AddTile(ref list, (int)v.X, (int)v.Y, actor.CurrentMapName);
                    }
                }
            }
            else if (dir.Y < 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Top, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Top, actor.CurrentMapName);
                if (rEndCollision.Width > TILE_SIZE)
                {
                    foreach (Vector2 v in Util.GetAllPointsInArea(rEndCollision.Left, rEndCollision.Top - 1, rEndCollision.Width, 1, TILE_SIZE))
                    {
                        AddTile(ref list, (int)v.X, (int)v.Y, actor.CurrentMapName);
                    }
                }
            }

            //Because RHTiles do not contain WorldActors outside of combat, we need to add each
            //WorldActor's CollisionBox to the list, as long as the WorldActor in question is not the moving WorldActor.
            foreach (WorldActor w in _liActors)
            {
                if (w.OnTheMap && w != actor)
                {
                    switch (w.ActorType)
                    {
                        case WorldActorTypeEnum.Critter:
                        case WorldActorTypeEnum.Pet:
                            break;
                        case WorldActorTypeEnum.Mount:
                            if (!PlayerManager.PlayerActor.Mounted) { goto default; }
                            else { break; }
                        default:
                            list.Add(w.CollisionBox);
                            break;
                    }
                }
            }

            //If the actor is not the Player Character, add the Player Character's CollisionBox to the list as well
            if (actor != PlayerManager.PlayerActor && MapManager.CurrentMap == actor.CurrentMap && !actor.IsActorType(WorldActorTypeEnum.Pet))
            {
                list.Add(PlayerManager.PlayerActor.CollisionBox);
            }

            return list;
        }
        private void AddTile(ref List<Rectangle> list, int one, int two, string mapName)
        {
            RHTile tile = MapManager.Maps[mapName].GetTileByGridCoords(Util.GetGridCoords(one, two));
            if (TileValid(tile, list)) { list.Add(tile.Rect); }
        }
        private bool TileValid(RHTile tile, List<Rectangle> list)
        {
            return tile != null && !tile.Passable() && !list.Contains(tile.Rect);
        }

        private void ChangeDir(List<Rectangle> possibleCollisions, Rectangle originalRectangle, ref Vector2 dir, string map)
        {
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
                    dir.Y = 0;

                    //Modifier is to determine if the nudge is positive or negative
                    int modifier = (int)CheckToNudge(newRectangleY.Center.X, r.Center.X, coords.X, coords.Y, "Col");
                    int xVal = (int)(modifier > 0 ? newRectangleX.Right : newRectangleX.Left) + modifier;               //Constructs the new rectangle based on the mod

                    dir.X += CheckNudgeAllowed(modifier, new Point(xVal, newRectangleY.Top), new Point(xVal, newRectangleY.Bottom), map);
                }
                if (dir.X != 0 && r.Intersects(newRectangleX))
                {
                    dir.X = 0;

                    //Modifier is to determine if the nudge is positive or negative
                    int modifier = (int)CheckToNudge(newRectangleY.Center.Y, r.Center.Y, coords.X, coords.Y, "Row");
                    int yVal = (int)(modifier > 0 ? newRectangleX.Bottom : newRectangleX.Top) + modifier;               //Constructs the new rectangle based on the mod

                    dir.Y += CheckNudgeAllowed(modifier, new Point(newRectangleY.Left, yVal), new Point(newRectangleY.Right, yVal), map);
                }

                //Because of diagonal movement, it's possible to have no issue on either the X axis or Y axis but have a collision
                //diagonal to the actor. In this case, just null out the X movement.
                if (dir.X != 0 && dir.Y != 0 && r.Intersects(newRectangleX) && r.Intersects(newRectangleY))
                {
                    dir.X = 0;
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
        /// <param name="c">The moving WorldActor</param>
        /// <param name="testX">Movement along the X Axis</param>
        /// <param name="testY">Movement along the Y Axis</param>
        /// <param name="dir">Reference to the direction to move the WorldActor</param>
        /// <param name="ignoreCollisions">Whether or not to check collisions</param>
        /// <returns>False if we are to prevent movement</returns>
        public bool CheckForCollisions(WorldActor c, Rectangle testX, Rectangle testY, ref Vector2 dir, bool ignoreCollisions = false)
        {
            bool rv = true;

            Rectangle box = c.CollisionBox;
            //Checking for a MapChange takes priority overlooking for collisions.
            if (CheckForMapChange(c, testX) || CheckForMapChange(c, testY))
            {
                return false;
            }
            else if (!ignoreCollisions)
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
            foreach (KeyValuePair<string, TravelPoint> kvp in DictionaryTravelPoints)
            {
                if (kvp.Value.Intersects(movingChar) && !kvp.Value.IsDoor && kvp.Value.IsActive)
                {
                    if (c.IsActorType(WorldActorTypeEnum.Pet)) { return false; }

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
            if (varCol == -1 && varRow == -1)
            {
                if (centerDelta > 0) { rv = 1; }
                else if (centerDelta < 0) { rv = -1; }
            }
            else if (centerDelta > 0)
            {
                RHTile testTile = GetTileByGridCoords((int)(varCol + (v.Equals("Col") ? 1 : 0)), (int)(varRow + (v.Equals("Row") ? 1 : 0)));
                if (testTile != null && testTile.Passable())
                {
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
            displayItem.Position = DictionaryCharacterLayer[npcIndex + "Col" + index];
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

        public Vector2 GetCharacterSpawn(string val)
        {
            Vector2 rv = Vector2.Zero;
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

            foreach (ShopLocation shop in _liShopData)
            {
                if (shop.Contains(mouseLocation) && shop.IsOpen())
                {
                    shop.Talk();
                    return true;
                }
            }

            foreach (WorldActor c in _liActors)
            {
                if (PlayerManager.PlayerInRange(c.HoverBox, (int)(TILE_SIZE * 1.5)) && c.HoverContains(mouseLocation) && c.OnTheMap)
                {
                    c.ProcessRightButtonClick();
                    return true;
                }
            }

            List<Item> removedList = new List<Item>();
            for (int i = 0; i < _liItems.Count; i++)
            {
                Item it = _liItems[i];
                if (it.ManualPickup && it.CollisionBox.Contains(GUICursor.GetWorldMousePosition()))
                {
                    if (InventoryManager.AddToInventory(it))
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

            if (Scrying())
            {
                SetGameScale(NORMAL_SCALE);
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

                    if (TargetTile != null)
                    {
                        if (PlayerManager.PlayerInRange(TargetTile.Center.ToPoint()))
                        {
                            //Retrieves any object associated with the tile, this will include
                            //both actual tiles, and Shadow Tiles because the user sees Shadow Tiles
                            //as being on the tile.
                            WorldObject obj = TargetTile.GetWorldObject(false);
                            if (obj != null)
                            {
                                obj.ProcessLeftClick();
                                rv = true;
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

                RHTile t = GetTileByPixelPosition(GUICursor.GetWorldMousePosition().ToPoint());
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
                }

                foreach (WorldActor c in _liActors)
                {
                    if (c.HoverContains(mouseLocation) && c.OnTheMap)
                    {
                        switch (c.ActorType)
                        {
                            case WorldActorTypeEnum.Merchant:
                            case WorldActorTypeEnum.ShippingGremlin:
                            case WorldActorTypeEnum.Spirit:
                            case WorldActorTypeEnum.Villager:
                                GUICursor.SetCursor(GUICursor.CursorTypeEnum.Talk, c.HoverBox);
                                found = true;
                                break;
                        }
                        if (found) { break; }
                    }
                }

                //Do not draw test tiles on a map for combat
                //WorldObject constructToBuild = GameManager.ConstructionObject;
                //if (!IsCombatMap && constructToBuild != null)
                //{
                //    Vector2 vec = mouseLocation.ToVector2() - new Vector2(0, constructToBuild.Height - constructToBuild.BaseHeight);
                //    constructToBuild.SetCoordinates(Util.SnapToGrid(vec));
                //    TestMapTiles(constructToBuild, _liTestTiles);
                //}

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
                                PlayerManager.AddToStorage(toRemove.ID);
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
                        GUIManager.OpenMainObject(new HUDUpgradeWindow((Building)obj));
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
                        PlayerManager.AddToTownObjects(toBuild);
                    }

                    //Drop the Building from the GameManger
                    GameManager.DropWorldObject();
                    ClearHeldLights();

                    //Only leave TownMode if we were in Build Mode
                    if (TownModeBuild())
                    {
                        if (GameManager.BuildFromStorage) { PlayerManager.RemoveFromStorage(toBuild.ID); }
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
                    PlayerManager.AddToTownObjects(placeObject);
                }

                if (GameManager.BuildFromStorage) { PlayerManager.RemoveFromStorage(placeObject.ID); }

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
                if (TownModeMoving() || (GameManager.BuildFromStorage && !PlayerManager.HasInStorage(placeObject.ID)) || (!GameManager.BuildFromStorage && !InventoryManager.HasSufficientItems(placeObject.RequiredToMake)))
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
            SetGameScale(NORMAL_SCALE);

            //Re-open the Building Menu
            GUIManager.OpenMenu();
        }

        public void ClearWorkers()
        {
            _liActors.Clear();
        }

        public void RemoveWorldObject(WorldObject o)
        {
            _liObjectsToRemove.Add(o);
            o.RemoveSelfFromTiles();
        }
        public void RemoveActor(WorldActor c)
        {
            _liActorsToRemove.Add(c);
        }

        public void DropItemsOnMap(List<Item> items, Vector2 position, bool flyingPop = true)
        {
            foreach (Item i in items)
            {
                DropItemOnMap(i, position, flyingPop);
            }
        }

        public void DropItemOnMap(Item item, Vector2 position, bool flyingPop = true)
        {
            if (flyingPop) { item.Pop(position); }
            else { item.Position = position; }

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
            ClearWorkers();
            AddBuildingObjectsToMap(b);
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
                            b.BuildingChest.SnapPositionToGrid(mapObject.Position);
                            b.BuildingChest.PlaceOnMap(this);
                        }
                    }
                }
            }
        }

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

        public bool PlaceWorldObject(WorldObject o)
        {
            bool rv = false;

            List<RHTile> tiles = new List<RHTile>();
            if (TestMapTiles(o, tiles))
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
        public bool TestMapTiles(WorldObject obj, List<RHTile> collisionTiles)
        {
            bool rv = true;
            collisionTiles.Clear();
            Vector2 position = obj.CollisionBox.Location.ToVector2();
            position.X = ((int)(position.X / TILE_SIZE)) * TILE_SIZE;
            position.Y = ((int)(position.Y / TILE_SIZE)) * TILE_SIZE;

            int colColumns = obj.CollisionBox.Width / TILE_SIZE;
            int colRows = obj.CollisionBox.Height / TILE_SIZE;

            //This is used to get all the tiles based off the collisonbox size
            for (int i = 0; i < colRows; i++)
            {
                for (int j = 0; j < colColumns; j++)
                {
                    int x = Math.Min((obj.CollisionBox.Left + (j * TILE_SIZE)) / TILE_SIZE, MapWidthTiles - 1);
                    int y = Math.Min((obj.CollisionBox.Top + (i * TILE_SIZE)) / TILE_SIZE, MapHeightTiles - 1);
                    if (x < 0 || x > this.MapWidthTiles || y < 0 || y > this.MapHeightTiles)
                    {
                        rv = false;
                        break;
                    }

                    RHTile tempTile = _arrTiles[x, y];
                    if (CanPlaceObject(tempTile, obj))
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
        private bool CanPlaceObject(RHTile testTile, WorldObject obj)
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
            else if (obj.WallObject)
            {
                rv = (testTile.WorldObject == null && testTile.IsWallpaperWall);
            }
            else if (!TileContainsActor(testTile) || obj.Walkable)
            {
                if (testTile.CanPlaceOnTabletop(obj) || (testTile.Passable() && testTile.WorldObject == null))
                {
                    rv = true;
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
            tiles.FindAll(t => !o.Tiles.Contains(t)).ForEach(t => o.Tiles.Add(t));

            if (!_liPlacedWorldObjects.Contains(o))
            {
                _liPlacedWorldObjects.Add(o);
            }

            if (o.CompareType(ObjectTypeEnum.CombatHazard))
            {
                foreach (RHTile t in tiles)
                {
                    t.SetHazard((CombatHazard)o);
                }
            }
            else
            {
                //Sets the WorldObject to each RHTile
                foreach (RHTile t in tiles)
                {
                    t.SetObject(o);
                }
            }

            //Iterate over the WorldObject image in TILE_SIZE increments to discover any tiles
            //that the image overlaps. Add those tiles as Shadow Tiles as long as they're not
            //actual Tiles the object sits on. Also add the Tiles to the objects Shadow Tiles list
            for (int i = (int)o.MapPosition.X; i < o.MapPosition.X + o.Width; i += TILE_SIZE)
            {
                for (int j = (int)o.MapPosition.Y; j < o.MapPosition.Y + o.Height; j += TILE_SIZE)
                {
                    RHTile t = GetTileByGridCoords(Util.GetGridCoords(i, j));
                    if (t != null && !o.Tiles.Contains(t))
                    {
                        t.SetShadowObject(o);
                        o.AddTile(t);
                    }
                }
            }
        }

        public void AddActor(WorldActor c)
        {
            ToAdd.Add(c);
        }

        public bool RemoveCharacterImmediately(WorldActor c)
        {
            bool rv = false;
            if (MapManager.Maps[c.CurrentMapName].ContainsActor(c))
            {
                rv = true;
                if (c.IsActorType(WorldActorTypeEnum.Mob) && _liMobs.Contains((Mob)c)) { _liMobs.Remove((Mob)c); }
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
            if (string.IsNullOrEmpty(c.CurrentMapName) || !MapManager.Maps[c.CurrentMapName].ContainsActor(c))
            {
                rv = true;

                if (c.IsActorType(WorldActorTypeEnum.Mob) && !_liMobs.Contains((Mob)c)) { _liMobs.Add((Mob)c); }
                else { Util.AddUniquelyToList(ref _liActors, c); }

                c.CurrentMapName = _sName;
                c.Position = c.NewMapPosition == Vector2.Zero ? c.Position : c.NewMapPosition;
                c.NewMapPosition = Vector2.Zero;
            }

            return rv;
        }

        //public void AddMonster(TacticalMonster m)
        //{
        //    bool rv = false;

        //    RHRandom rand = RHRandom.Instance();
        //    Vector2 position = m.Position;
        //    position.X = ((int)(position.X / TILE_SIZE)) * TILE_SIZE;
        //    position.Y = ((int)(position.Y / TILE_SIZE)) * TILE_SIZE;

        //    rv = _arrTiles[((int)position.X / TILE_SIZE), ((int)position.Y / TILE_SIZE)].Passable();
        //    if (!rv)
        //    {
        //        do
        //        {
        //            position.X = (int)(rand.Next(1, (MapWidthTiles - 1) * TILE_SIZE) / TILE_SIZE) * TILE_SIZE;
        //            position.Y = (int)(rand.Next(1, (MapHeightTiles - 1) * TILE_SIZE) / TILE_SIZE) * TILE_SIZE;
        //            rv = _arrTiles[((int)position.X / TILE_SIZE), ((int)position.Y / TILE_SIZE)].Passable();
        //        } while (!rv);
        //    }

        //    if (rv)
        //    {
        //        AddMonsterByPosition(m, position);
        //    }
        //}

        public void AddMonsterByPosition(Mob m, Vector2 position)
        {
            m.CurrentMapName = _sName;
            m.Position = Util.SnapToGrid(position);

            _liMobs.Add(m);
        }

        public void AddBuilding(Building b)
        {
            if (!_liBuildings.Contains(b))
            {
                _liBuildings.Add(b);
                b.SetHomeMap(_sName);
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

        public List<RHTile> CheckForCombatHazards(CombatHazard.HazardTypeEnum e)
        {
            List<RHTile> liRv = new List<RHTile>();
            foreach (RHTile t in _arrTiles)
            {
                if (t.HazardObject != null && t.HazardObject.SubtypeMatch(e))
                {
                    liRv.Add(t);
                }
            }

            return liRv;
        }

        public void LeaveMap()
        {
            EnvironmentManager.UnloadEnvironment();
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
            if (x >= MapWidthTiles * TILE_SIZE || x < 0) { return null; }
            if (y >= MapHeightTiles * TILE_SIZE || y < 0) { return null; }

            return _arrTiles[x / TILE_SIZE, y / TILE_SIZE];
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
            foreach (Vector2 vec in Util.GetAllPointsInArea(startX, startY, dimensions, dimensions))
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
        public List<RHTile> GetTilesFromRectangle(Rectangle obj)
        {
            List<RHTile> rvList = new List<RHTile>();

            for (int y = obj.Top; y < obj.Top + obj.Height; y += TILE_SIZE)
            {
                for (int x = obj.Left; x < obj.Left + obj.Width; x += TILE_SIZE)
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

        internal MapData SaveData()
        {
            MapData mapData = new MapData
            {
                mapName = this.Name,
                worldObjects = new List<WorldObjectData>(),
                decor = new List<DecorData>(),
                containers = new List<ContainerData>(),
                machines = new List<MachineData>(),
                plants = new List<PlantData>(),
                gardens = new List<GardenData>(),
                beehives = new List<BeehiveData>(),
                warpPoints = new List<WarpPointData>()
            };

            foreach (WorldObject wObj in _liPlacedWorldObjects)
            {
                switch (wObj.Type)
                {
                    case ObjectTypeEnum.Beehive:
                        mapData.beehives.Add(((Beehive)wObj).SaveData());
                        break;
                    case ObjectTypeEnum.Decor:
                        mapData.decor.Add(((Decor)wObj).SaveData());
                        break;
                    case ObjectTypeEnum.Container:
                        mapData.containers.Add(((Container)wObj).SaveData());
                        break;
                    case ObjectTypeEnum.Garden:
                        mapData.gardens.Add(((Garden)wObj).SaveData());
                        break;
                    case ObjectTypeEnum.Machine:
                        mapData.machines.Add(((Machine)wObj).SaveData());
                        break;
                    case ObjectTypeEnum.Plant:
                        mapData.plants.Add(((Plant)wObj).SaveData());
                        break;
                    case ObjectTypeEnum.WarpPoint:
                        mapData.warpPoints.Add(((WarpPoint)wObj).SaveData());
                        break;
                    default:
                        WorldObjectData d = new WorldObjectData
                        {
                            worldObjectID = wObj.ID,
                            x = (int)wObj.CollisionBox.X,
                            y = (int)wObj.CollisionBox.Y
                        };
                        mapData.worldObjects.Add(d);
                        break;
                }
            }

            return mapData;
        }
        internal void LoadData(MapData mData)
        {
            foreach (WorldObjectData w in mData.worldObjects)
            {
                WorldObject obj = DataManager.CreateWorldObjectByID(w.worldObjectID);
                obj?.PlaceOnMap(new Vector2(w.x, w.y), this);
                if (obj != null && this == MapManager.TownMap) { PlayerManager.AddToTownObjects(obj); }
            }

            foreach (DecorData d in mData.decor)
            {
                Decor obj = (Decor)DataManager.CreateWorldObjectByID(d.ID);
                obj.LoadData(d);
                obj.PlaceOnMap(this);
                if (this == MapManager.TownMap) { PlayerManager.AddToTownObjects(obj); }
            }
            foreach (ContainerData c in mData.containers)
            {
                Container obj = (Container)DataManager.CreateWorldObjectByID(c.containerID);
                obj.LoadData(c);
                obj.PlaceOnMap(this);
                if (this == MapManager.TownMap) { PlayerManager.AddToTownObjects(obj); }
            }
            foreach (MachineData mac in mData.machines)
            {
                Machine obj = (Machine)DataManager.CreateWorldObjectByID(mac.ID);
                obj.LoadData(mac);
                obj.PlaceOnMap(this);
                if (this == MapManager.TownMap) { PlayerManager.AddToTownObjects(obj); }
            }
            foreach (PlantData plantData in mData.plants)
            {
                Plant plant = (Plant)DataManager.CreateWorldObjectByID(plantData.ID);
                plant.LoadData(plantData);
                plant.PlaceOnMap(this);
            }
            foreach (GardenData gardenData in mData.gardens)
            {
                Garden obj = (Garden)DataManager.CreateWorldObjectByID(gardenData.ID);
                obj.LoadData(gardenData);
                obj.PlaceOnMap(this);
                if (this == MapManager.TownMap) { PlayerManager.AddToTownObjects(obj); }
            }
            foreach (BeehiveData data in mData.beehives)
            {
                Beehive obj = (Beehive)DataManager.CreateWorldObjectByID(data.ID);
                obj.LoadData(data);
                obj.PlaceOnMap(this);
                if (this == MapManager.TownMap) { PlayerManager.AddToTownObjects(obj); }
            }
            foreach (WarpPointData warpData in mData.warpPoints)
            {
                WarpPoint obj = (WarpPoint)DataManager.CreateWorldObjectByID(warpData.ID);
                obj.LoadData(warpData);
                obj.PlaceOnMap(this);
                if (this == MapManager.TownMap) { PlayerManager.AddToTownObjects(obj); }
            }
        }
    }
}
