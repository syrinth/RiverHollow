﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.MainObjects;
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
        public MapTypeEnum MapType => Util.ParseEnum<MapTypeEnum>(_map.Properties.ContainsKey("MapType") ? _map.Properties["MapType"] : string.Empty);

        public bool Visited { get; private set; } = false;
        public bool MobsSpawned { get; private set; } = false;
        private bool Randomize => _map.Properties.ContainsKey("Randomize");
        public MonsterFood PrimedFood { get; private set; }

        public RHTile TargetTile { get; private set; } = null;
        private RHTile MouseTile => GetMouseOverTile();

        protected TiledMap _map;
        public TiledMap Map => _map;

        protected int _iTotalFillableTiles;
        protected RHTile[,] _arrTiles;
        public List<RHTile> TileList => _arrTiles.Cast<RHTile>().ToList();
        private List<RHTile> _liSpecialTiles;

        protected TiledMapRenderer _renderer;
        protected List<TiledMapTileset> _liTilesets;
        protected Dictionary<string, List<TiledMapTileLayer>> _diTileLayers;
        public Dictionary<string, List<TiledMapTileLayer>> Layers => _diTileLayers;

        private List<Light> _liLights;
        private List<Actor> _liActors;
        public IList<Actor> Actors { get { return _liActors.AsReadOnly(); } }
        protected List<Mob> _liMobs;
        public IList<Mob> Mobs { get { return _liMobs.AsReadOnly(); } }
        public List<Actor> ToAdd;
        private readonly Dictionary<int, List<WorldObject>> _diWorldObjects;
        private List<Tuple<Item, Point, float>> _liMerchDisplayPoints;
        private readonly List<ResourceSpawn> _liResourceSpawns;
        private readonly List<MobSpawn> _liMobSpawns;
        private readonly List<int> _liCutscenes;
        protected List<MapItem> _liItems;
        private Dictionary<string, TravelPoint> _diTravelPoints;
        public IReadOnlyDictionary<string, TravelPoint> TravelPoints => _diTravelPoints;
        private Dictionary<string, Rectangle> _diCharacterLayer;
        public IReadOnlyDictionary<string, Rectangle> CharacterObjects => _diCharacterLayer;
        private readonly List<TiledMapObject> _liMapObjects;
        private int _iShopID = -1;
        public Shop TheShop => (_iShopID > -1) ? GameManager.DIShops[_iShopID] : null;

        private readonly List<MapItem> _liItemsToRemove;
        private List<Actor> _liActorsToRemove;
        private readonly List<WorldObject> _liObjectsToRemove;

        private readonly List<WorldObject> _liHoverObjects;

        public MapNode WorldMapNode { get; private set; }

        Buildable _objSelectedObject = null;

        public RHMap()
        {
            _liResourceSpawns = new List<ResourceSpawn>();
            _liMobSpawns = new List<MobSpawn>();
            _liSpecialTiles = new List<RHTile>();
            _liTilesets = new List<TiledMapTileset>();
            _liActors = new List<Actor>();
            _liMobs = new List<Mob>();
            _liItems = new List<MapItem>();
            _liMapObjects = new List<TiledMapObject>();

            _diWorldObjects = new Dictionary<int, List<WorldObject>>();
            _liLights = new List<Light>();
            _liCutscenes = new List<int>();

            _diTravelPoints = new Dictionary<string, TravelPoint>();
            _diCharacterLayer = new Dictionary<string, Rectangle>();

            _liItemsToRemove = new List<MapItem>();
            _liActorsToRemove = new List<Actor>();
            _liObjectsToRemove = new List<WorldObject>();
            _liHoverObjects = new List<WorldObject>();

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
            _liSpecialTiles = map._liSpecialTiles;
            _diTileLayers = map._diTileLayers;

            if (_map.Properties.ContainsKey("Dungeon"))
            {
                DungeonName = _map.Properties["Dungeon"];
                DungeonManager.AddMapToDungeon(_map.Properties["Dungeon"], _map.Properties.ContainsKey("Procedural"), this);
            }
            _diWorldObjects = map._diWorldObjects;
            _liLights = map._liLights;
            _iShopID = map._iShopID;

            MapWidthTiles = _map.Width;
            MapHeightTiles = _map.Height;

            LoadMapObjects(true);
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

            var props = _map.Properties;
            if (props.ContainsKey("Shop"))
            {
                _iShopID = int.Parse(props["Shop"]);
            }
            if (props.ContainsKey("Dungeon"))
            {
                DungeonName = props["Dungeon"];
                DungeonManager.AddMapToDungeon(props["Dungeon"], props.ContainsKey("Procedural"), this);
            }
            if (props.ContainsKey("BuildingID"))
            {
                _liMerchDisplayPoints = new List<Tuple<Item, Point, float>>();
            }

            if (props.ContainsKey("Cutscenes"))
            {
                string[] split = Util.FindParams(props["Cutscenes"]);
                foreach (string cutsceneID in split)
                {
                    _liCutscenes.Add(int.Parse(cutsceneID));
                }
            }

            if (props.ContainsKey("WorldData"))
            {
                string worldData = props["WorldData"];
                WorldMapNode = new MapNode(Util.ParsePoint(worldData), int.Parse(props["WorldMapCost"]), props["WorldLink"]);
            }

            _renderer = new TiledMapRenderer(GraphicsDevice);

            DetermineUsableTiles();
        }

        private void DetermineUsableTiles()
        {
            List<RHTile> possibleTiles = TileList;
            var impassable = possibleTiles.FindAll(x => !x.TileCheck());

            impassable.ForEach(x => possibleTiles.Remove(x));

            RemoveTilesNearTravelPoints(ref possibleTiles);
            RemoveTilesNearSpecialObjects(ref possibleTiles);
            RemoveSkipTiles(ref possibleTiles);
            _iTotalFillableTiles = possibleTiles.Count;
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
                DIRemoveObject(obj);
            }
            _liObjectsToRemove.Clear();

            if (this == MapManager.CurrentMap)
            {
                foreach (var objectList in _diWorldObjects.Values)
                {
                    objectList.ForEach(x => x.Update(gTime));
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

        #region Draw
        public void SetLayerVisibiltyByName(bool visible, string designation)
        {
            foreach (TiledMapTileLayer layer in _diTileLayers[designation])
            {
                layer.IsVisible = visible;
            }
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

            _liSpecialTiles.ForEach(x => x.Draw(spriteBatch));
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
            _liMobs.FindAll(x => !x.GetBoolByIDKey("Fly") || !x.HasHP).ForEach(x => x.Draw(spriteBatch, true));
            _liItems.ForEach(x => x.Draw(spriteBatch));

            foreach(var objectList in _diWorldObjects.Values)
            {
                objectList.ForEach(x => x.Draw(spriteBatch));
            }

            if (_liMerchDisplayPoints != null)
            {
                foreach(var obj in _liMerchDisplayPoints)
                {
                    obj.Item1?.DrawShadow(true);
                    obj.Item1?.Draw(spriteBatch, new Rectangle(obj.Item2.X, obj.Item2.Y, Constants.TILE_SIZE, Constants.TILE_SIZE), obj.Item3);
                }
            }


            if (HeldObject != null && (!GameManager.GamePaused() || Scrying()))
            {
                foreach (RHTile t in TestTiles)
                {
                    bool passable = CanPlaceObject(t, HeldObject);
                    if (!passable || (passable && !HeldObject.BuildableType(BuildableEnum.Wallpaper)))
                    {
                        spriteBatch.Draw(DataManager.GetTexture(DataManager.HUD_COMPONENTS), new Rectangle(t.Position, Constants.BASIC_TILE), GUIUtils.PLACEMENT_BOX, passable ? Color.Green * 0.5f : Color.Red * 0.5f, 0, Vector2.Zero, SpriteEffects.None, Constants.MAX_LAYER_DEPTH);
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

            _liMobs.FindAll(x => x.GetBoolByIDKey("Fly") && x.HasHP).ForEach(x => x.Draw(spriteBatch, true));
            EnvironmentManager.Draw(spriteBatch);

            SetLayerVisibiltyByName(true, "Base");
            SetLayerVisibiltyByName(true, "Ground");
            SetLayerVisibiltyByName(false, "Upper");
        }
        #endregion

        #region Lights
        public void DrawLights(SpriteBatch spriteBatch)
        {
            foreach (Light obj in _liLights) { obj.Draw(spriteBatch); }

            //spriteBatch.Draw(lightMask, new Vector2(PlayerManager.World.CollisionCenter.X - lightMask.Width / 2, PlayerManager.World.CollisionBox.Y - lightMask.Height / 2), Color.White);
        }
        public void AddLights(List<Light> newLights)
        {
            if (newLights != null)
            {
                foreach (Light obj in newLights)
                {
                    AddLight(obj);
                }
            }
        }
        public void AddLight(Light obj)
        {
            if (obj != null)
            {
                Util.AddUniquelyToList(ref _liLights, obj);
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
        #endregion

        #region Accessors
        public IReadOnlyDictionary<int, List<WorldObject>> GetObjects()
        {
            return _diWorldObjects;
        }
        public void AddToWorldObjects(WorldObject obj)
        {
            TownManager.TownManagerCheck(this, obj);

            if (!_diWorldObjects.ContainsKey(obj.ID))
            {
                _diWorldObjects[obj.ID] = new List<WorldObject>();
            }

            if (!_diWorldObjects[obj.ID].Contains(obj))
            {
                _diWorldObjects[obj.ID].Add(obj);

                if (obj.HasHover())
                {
                    _liHoverObjects.Add(obj);
                }
            }
        }
        public void DIRemoveObject(WorldObject obj)
        {
            if (_diWorldObjects.ContainsKey(obj.ID))
            {
                if (_diWorldObjects[obj.ID].Contains(obj))
                {
                    _diWorldObjects[obj.ID].Remove(obj);

                    if (obj.HasHover())
                    {
                        _liHoverObjects.Remove(obj);
                    }

                    if (_diWorldObjects[obj.ID].Count == 0)
                    {
                        _diWorldObjects.Remove(obj.ID);
                    }
                }
            }
            else
            {
                LogManager.WriteToLog(string.Format("Cannot remove object ID \"{0}\", it does not exist.", obj.ID));
            }
        }
        public int GetNumberObjects(int objID, bool onlyFinished)
        {
            int rv = 0;

            if (_diWorldObjects.ContainsKey(objID))
            {
                var objects = _diWorldObjects[objID];

                if (onlyFinished && DataManager.GetEnumByIDKey<ObjectTypeEnum>(objID, "Type", DataType.WorldObject) == ObjectTypeEnum.Plant)
                {
                    for (int i = 0; i < objects.Count; i++)
                    {
                        if (objects[i] is Plant plantObj)
                        {
                            if (plantObj.FinishedGrowing())
                            {
                                rv++;
                            }
                        }
                    }
                }
                else { rv = objects.Count; }
            }
            return rv;
        }
        public List<WorldObject> GetObjectsByID(int objID)
        {
            List<WorldObject> rv = new List<WorldObject>();

            if (_diWorldObjects.ContainsKey(objID))
            {
                rv = _diWorldObjects[objID];
            }
            return rv;
        }

        public List<T> GetObjectsByType<T>()
        {
            List<T> rv = new List<T>();

            foreach (var objList in _diWorldObjects.Values)
            {
                if (objList[0] is T)
                {
                    foreach (var o in objList)
                    {
                        if (o is T castObj)
                        {
                            rv.Add(castObj);
                        }
                    }

                }
            }

            return rv;
        }

        public TravelPoint GetTravelPoint(string key)
        {
            TravelPoint rv = null;

            if (_diTravelPoints.ContainsKey(key))
            {
                rv = _diTravelPoints[key];
            }

            return rv;
        }

        public Rectangle GetCharacterObject(string key)
        {
            Rectangle rv = Rectangle.Empty;

            if (_diCharacterLayer.ContainsKey(key))
            {
                rv = _diCharacterLayer[key];
            }

            return rv;
        }
        #endregion

        #region Resource Generation
        private List<RHTile> GetValidTiles()
        {
            List<RHTile> possibleTiles = TileList;

            var impassable = possibleTiles.FindAll(x => !x.TileCheck(true));
            var obj = possibleTiles.FindAll(x => x.WorldObject != null);
            var flooring = possibleTiles.FindAll(x => x.Flooring != null);

            impassable.ForEach(x => possibleTiles.Remove(x));
            obj.ForEach(x => possibleTiles.Remove(x));
            flooring.ForEach(x => possibleTiles.Remove(x));

            //possibleTiles.RemoveAll(x => !x.TileIsPassable() || x.WorldObject != null || x.Flooring != null);
            RemoveTilesNearTravelPoints(ref possibleTiles);
            RemoveTilesNearSpecialObjects(ref possibleTiles);
            RemoveSkipTiles(ref possibleTiles);

            return possibleTiles;
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

                GenerateMapObjects();
            }

            SpawnMobs();
        }
        private void GenerateMapObjects()
        {
            //Step 1 Resource Spawn points need to fire
            _liResourceSpawns.ForEach(x => x.Spawn());

            //Step 2 Determine valid Tiles
            var possibleTiles = GetValidTiles();

            //Step 3 Map Resources need to be generated
            List<RHTile> hiddenTiles = possibleTiles.FindAll(x => x.HasUpperLayer());
            hiddenTiles.ForEach(x => possibleTiles.Remove(x));
            GenerateMapResources(false,  ref possibleTiles);

            //Step 4 Filler Resources need to be generated
            hiddenTiles.ForEach(x => possibleTiles.Add(x));
            GenerateFillerResources(false, ref possibleTiles);

            //Step 5 generate Fish
            foreach(var obj in _liResourceSpawns)
            {
                if (obj.FishingHole)
                {
                    var tiles = GetTilesFromRectangleExcludeEdgePoints(obj.GetRectangle());
                    var waterTiles = tiles.FindAll(x => x.IsWaterTile);

                    int max = (int)Math.Ceiling(waterTiles.Count() / 30f);
                    int spawnNum = RHRandom.Instance().Next(1, max);
                    for (int i = 0; i < spawnNum; i++)
                    {
                        var random = Util.GetRandomItem(waterTiles);
                        var fish = DataManager.CreateActor<Fish>(Constants.FISH_PUDDLE);

                        fish.SetPosition(random.Position);
                        AddActor(fish);

                        tiles.Remove(random);
                    }
                }
            }
        }
        private void GenerateMapResources(bool refresh, ref List<RHTile> possibleTiles)
        {
            if (_map.Properties.ContainsKey("ResourceTotal"))
            {
                var resources = new Dictionary<RarityEnum, KeyValuePair<int, int>>();

                int min = 1;
                string[] spawnResources = Util.FindParams(_map.Properties["ResourceTotal"]);
                foreach (string s in spawnResources)
                {
                    var split = Util.FindArguments(s);
                    if (!int.TryParse(split[0], out int max))
                    {
                        max = min;
                    }

                    var rarity = split.Length > 1 ? Util.ParseEnum<RarityEnum>(split[1]) : RarityEnum.C;
                    var kvp = new KeyValuePair<int, int>(min, max);
                    Util.SafeAddToDictionary(ref resources, rarity, kvp);
                    min = max;
                }

                var rarityRoll = Util.RollOnRarityTable(resources);
                int totalResources = RHRandom.Instance().Next(rarityRoll.Key, rarityRoll.Value);

                var rarityTable = new Dictionary<RarityEnum, List<SpawnData>>();
                if (_map.Properties.ContainsKey("ResourceItemID"))
                {
                    Util.AssignSpawnData(ref rarityTable, _map.Properties["ResourceItemID"], SpawnTypeEnum.Item);
                }

                if (_map.Properties.ContainsKey("ResourceObjectID"))
                {
                    Util.AssignSpawnData(ref rarityTable, _map.Properties["ResourceObjectID"], SpawnTypeEnum.Object);
                }

                int presentResources = 0;
                var keys = new List<RarityEnum>(rarityTable.Keys);
                foreach (var key in keys)
                {
                    foreach (var data in rarityTable[key])
                    {
                        if (data.Type == SpawnTypeEnum.Object)
                        {
                            presentResources += GetObjectsByID(data.ID).Count;
                        }
                        else if (data.Type == SpawnTypeEnum.Item)
                        {
                            // -1 because we're looking for WrappedItem Objects here
                            var objList = GetObjectsByID(-1);
                            foreach (var obj in objList)
                            {
                                var wrappedObj = (WrappedItem)obj;
                                if (wrappedObj.ItemID == data.ID)
                                {
                                    presentResources++;
                                }
                            }
                        }
                    }
                }
                int spawnNumber = totalResources - presentResources;
                if (refresh && _map.Properties.ContainsKey("ResourceRate"))
                {
                    spawnNumber = Math.Min(spawnNumber, int.Parse(_map.Properties["ResourceRate"]));
                }

                for (int i = 0; i < spawnNumber; i++)
                {
                    WorldObject obj = Util.RollOnRarityTable(rarityTable).GetDataObject();
                    PlaceGeneratedObject(obj, ref possibleTiles, refresh);
                }
            }

            if (_map.Properties.ContainsKey("RelicID"))
            {
                bool repeat = false;
                do
                {
                    if (RHRandom.RollPercent(Constants.RELIC_BASE_CHANCE))
                    {
                        var RelicSplit = Util.FindIntParams(_map.Properties["RelicID"]);
                        if (RelicSplit.Count > 0)
                        {
                            var chosenID = Util.GetRandomItem(RelicSplit);
                            PlaceGeneratedObject(DataManager.CreateWorldObjectByID(chosenID), ref possibleTiles, refresh);
                        }

                        repeat = RHRandom.RollPercent(Constants.RELIC_BASE_CHANCE);
                    }
                } while (repeat);
            }
        }

        private void GenerateFillerResources(bool refresh, ref List<RHTile> possibleTiles)
        {
            if (_map.Properties.ContainsKey("FillerPercent") && _map.Properties.ContainsKey("FillerID"))
            {
                if (!float.TryParse(_map.Properties["FillerPercent"], out float fillPercent)) 
                {
                    return;
                }

                int totalWeight = 0;
                int currentFillerObjects = 0;
                string[] fillerParams = Util.FindParams(_map.Properties["FillerID"]);

                for (int i = 0; i < fillerParams.Length; i++)
                {
                    var fillData = Util.FindArguments(fillerParams[i]);
                    totalWeight += int.Parse(fillData[1]);
                    currentFillerObjects += GetObjectsByID(int.Parse(fillData[0])).Count;
                }

                int maxFiller = (int)(_iTotalFillableTiles * fillPercent);

                //We want to make sure that the filler isn't going to go over the margin
                if (maxFiller <= currentFillerObjects)
                {
                    return;
                }

                for (int i = 0; i < fillerParams.Length; i++)
                {
                    var fillData = Util.FindArguments(fillerParams[i]);

                    int id = int.Parse(fillData[0]);
                    int currentExisting = GetObjectsByID(id).Count;
                    int maxItemFiller = (int)(maxFiller * float.Parse(fillData[1]) / totalWeight);
                    int spawnNumber = maxItemFiller - currentExisting;

                    //FillerRate is expressed weekly, so need to divide it by days/week
                    if (refresh && _map.Properties.ContainsKey("FillerRate"))
                    {
                        if (!float.TryParse(_map.Properties["FillerRate"], out float fillerRate))
                        {
                            return;
                        }

                        int maxRefresh = (int)Math.Ceiling(maxItemFiller * (fillerRate / Enum.GetNames(typeof(DayEnum)).Length));
                        if (spawnNumber > maxRefresh)
                        {
                            spawnNumber = maxRefresh;
                        }
                    }

                    for (int j = 0; j < spawnNumber; j++)
                    {
                        WorldObject obj = DataManager.CreateWorldObjectByID(id);
                        PlaceGeneratedObject(obj, ref possibleTiles, refresh);
                    }
                }
            }
        }
        public void PlaceGeneratedObject(WorldObject obj, ref List<RHTile> possibleTiles, bool refresh)
        {
            if (obj != null)
            {
                bool placed;
                do
                {
                    var tile = Util.GetRandomItem(possibleTiles);
                    obj.SnapPositionToGrid(new Point(tile.Position.X, tile.Position.Y));
                    placed = obj.PlaceOnMap(this, false);

                    if (possibleTiles.Count <= 1)
                    {
                        LogManager.WriteToLog("No possible tiles for object ID {0} on map {1}", obj.ID, Name);
                        return;
                    }
                } while (!placed);

                foreach (RHTile t in obj.Tiles())
                {
                    possibleTiles.Remove(t);
                }

                if (obj is Plant plantObj)
                {
                    if (plantObj.NeedsWatering) { plantObj.FinishGrowth(); }
                    else if (!refresh) { plantObj.RandomizeState(); }
                }
            }
        }
        private void GenerateRolloverResources()
        {
            if (Visited && (this != MapManager.TownMap  || GameCalendar.CurrentDay == 1))
            {
                //Step 1 Resource Spawn Points refresh themselves
                _liResourceSpawns.ForEach(x => x.Rollover(Randomize));

                //Step 2 determine how many tiles are available
                var possibleTiles = GetValidTiles();

                //Step 3 Map Resources need to be generated
                List<RHTile> hiddenTiles = possibleTiles.FindAll(x => !x.GetTileByDirection(DirectionEnum.Down).TileCheck());
                hiddenTiles.ForEach(x => possibleTiles.Remove(x));
                GenerateMapResources(true, ref possibleTiles);

                //Step 4 Filler Resources need to be generated
                hiddenTiles.ForEach(x => possibleTiles.Add(x));
                GenerateFillerResources(true, ref possibleTiles);
            }
        }
        #endregion

        public void LoadMapObjects(bool copying)
        {
            ReadOnlyCollection<TiledMapObjectLayer> objectLayers = _map.ObjectLayers;
            foreach (TiledMapObjectLayer ol in objectLayers)
            {
                if (!copying && ol.Name.Contains("Travel"))
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

                            if (!string.IsNullOrEmpty(trvlPt.MapLink)) { _diTravelPoints.Add(trvlPt.MapLink, trvlPt); }
                            else { Util.SafeAddToDictionary(ref _diTravelPoints, Util.GetEnumString(trvlPt.EntranceDir), trvlPt); }
                        }
                    }
                }
                else if (ol.Name.Contains("Character"))
                {
                    foreach (TiledMapObject obj in ol.Objects)
                    {
                        if (obj.Name.Equals("Spirit"))
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
                                Util.SafeAddToDictionary(ref _diCharacterLayer, Constants.MAPOBJ_HOME + obj.Properties["NPC_ID"], Util.RectFromTiledMapObject(obj));
                            }
                        }
                        else
                        {
                            Util.SafeAddToDictionary(ref _diCharacterLayer, obj.Name, Util.RectFromTiledMapObject(obj));
                        }
                    }
                }
                else if (ol.Name.Contains("MapObject"))
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        if (mapObject.Name.StartsWith("Light"))
                        {
                            Light newLight = DataManager.GetLight(int.Parse(mapObject.Properties["ID"]));
                            var center = Util.RectFromTiledMapObject(mapObject).Center;
                            newLight.Position = center - new Point(newLight.Width/2, newLight.Height/2);
                            AddLight(newLight);
                        }
                        else if (mapObject.Name.Equals("Wall"))
                        {
                            foreach (Point p in Util.GetAllPointsInArea(mapObject.Position, mapObject.Size, Constants.TILE_SIZE))
                            {
                                RHTile tile = GetTileByPixelPosition(p);
                                Util.AddUniquelyToList(ref _liSpecialTiles, tile);
                                tile.SetWallTrue();
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

        private bool IsTownBuilding()
        {
            return BuildingID > -1 && this != MapManager.HomeMap;
        }

        public void PopulateMap(bool forceRepop)
        {
            foreach (TiledMapObject tiledObj in _liMapObjects)
            {
                if (forceRepop || tiledObj.Properties.ContainsKey("Reset") || IsTownBuilding())
                {
                    int xCount = 0;
                    int yCount = 0;

                    int objWidth = Constants.TILE_SIZE;
                    int objHeight = Constants.TILE_SIZE;
                    for (int y = (int)tiledObj.Position.Y; y < (int)tiledObj.Position.Y + tiledObj.Size.Height; y += objHeight)
                    {
                        for (int x = (int)tiledObj.Position.X; x < (int)tiledObj.Position.X + tiledObj.Size.Width; x += objWidth)
                        {
                            if (tiledObj.Properties.ContainsKey("ObjectID"))
                            {
                                int placeEvery = 1;
                                if (tiledObj.Properties.ContainsKey("Every"))
                                {
                                    if (!int.TryParse(tiledObj.Properties["Every"], out placeEvery))
                                    {
                                        LogManager.WriteToLog("Failed on Every");
                                    }
                                }

                                if (xCount % placeEvery == 0 && yCount % placeEvery == 0)
                                {
                                    WorldObject obj = DataManager.CreateWorldObjectByID(int.Parse(tiledObj.Properties["ObjectID"]), tiledObj.Properties);
                                    if (obj is Plant plantObj)
                                    {
                                        plantObj.FinishGrowth();
                                    }

                                    if (tiledObj.Properties.ContainsKey("TillEarth"))
                                    {
                                        var targetTile = GetTileByPixelPosition(x, y);
                                        targetTile.TillTile();
                                    }

                                    if (IsTownBuilding())
                                    {
                                        obj.SetReset();
                                    }

                                    if (obj.PlaceOnMap(new Point(x, y), this))
                                    {
                                        objWidth = obj.BaseWidth * Constants.TILE_SIZE;
                                        objHeight = obj.BaseHeight * Constants.TILE_SIZE;
                                    }
                                    else
                                    {
                                        LogManager.WriteToLog("Could not place objID {0} at {1}", obj.ID, new Point(x, y));
                                    }
                                }
                            }
                            else if (tiledObj.Properties.ContainsKey("ItemID"))
                            {
                                new WrappedItem(int.Parse(tiledObj.Properties["ItemID"])).PlaceOnMap(tiledObj.Position.ToPoint(), this);
                            }
                            else
                            {
                                int placeEvery = 1;
                                if (xCount % placeEvery == 0 && yCount % placeEvery == 0)
                                {
                                    if (tiledObj.Properties.ContainsKey("TillEarth"))
                                    {
                                        var targetTile = GetTileByPixelPosition(x, y);
                                        targetTile.TillTile();
                                    }
                                }
                            }
                            xCount++;
                        }
                        xCount = 0;
                        yCount++;
                    }
                }
            }
        }

        public ResourceSpawn GetFishingHole(Point p)
        {
            return GetFishingHole(GetTileByPixelPosition(p));
        }
        public ResourceSpawn GetFishingHole(RHTile t)
        {
            foreach(var obj in _liResourceSpawns)
            {
                if (obj.ContainsTile(t) && obj.FishingHole)
                {
                    return obj;
                }
            }

            return null;
        }

        private void SpawnMobs()
        {
            if (!MobsSpawned && Map.Properties.ContainsKey("Mobs") && _liMobSpawns.Count > 0)
            {
                MobsSpawned = true;

                for (int i = 0; i < _liMobs.Count; i++)
                {
                    RemoveActor(_liMobs[i]);
                }
                _liMobs.Clear();

                string[] mobRange = Util.FindArguments(Map.Properties["Mobs"]);
                int.TryParse(mobRange[0], out int min);
                if(!int.TryParse(mobRange[1], out int max))
                {
                    max = min;
                }

                if (_liMobSpawns.Count < max)
                {
                    max = _liMobSpawns.Count;
                }

                int roll = RHRandom.Instance().Next(min, max);
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

            foreach (var objectList in _diWorldObjects.Values)
            {
                foreach (WorldObject obj in objectList)
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
                            DIRemoveObject(obj);
                            break;
                    }
                }
            }
        }

        public void Rollover()
        {
            MobsSpawned = false;

            if (Randomize)
            {
                Visited = false;
                foreach (var objectList in _diWorldObjects.Values)
                {
                    var firstObj = objectList[0];

                    if (firstObj is Building) { continue; }

                    if (firstObj is Trigger || firstObj is TriggerDoor) 
                    {
                        foreach (var trigger in objectList)
                        {
                            if (trigger.Reset)
                            {
                                RemoveWorldObject(trigger);
                            }
                        }
                    }
                    else
                    {
                        foreach (var obj in objectList)
                        {
                            RemoveWorldObject(obj);
                        }
                    }
                }
            }
            else
            {
                //Skipping the Rollover here because the WorldObjects that were cleared still exist on the map list
                var keys = new List<int>(_diWorldObjects.Keys);
                foreach (var key in keys)
                {
                    new List<WorldObject>(_diWorldObjects[key]).ForEach(x => x.Rollover());
                }
            }

            _liSpecialTiles.ForEach(x => x.Rollover());
            
            GenerateRolloverResources();

            PopulateMap(Randomize);
            CheckSpirits();
            _liItems.Clear();
        }

        public void AssignMerchandise()
        {
            if (Building() is Building building)
            {
                //Assign setting drawing to ShopTables
                var worldObjects = GetObjectsByType<WorldObject>();
                var displayTables = worldObjects.Where(x => x.GetBoolByIDKey("ShopTable")).ToList();
                if (displayTables.Count == 0)
                {
                    return;
                }

                _liMerchDisplayPoints.Clear();

                int index = 0;
                int tableCount = 0;
                int maxTableSpots = GetMaxTableSpots(displayTables[index]);
                foreach (var merch in building.Merchandise)
                {
                    if (merch == null)
                    {
                        continue;
                    }

                    bool placed = false;
                    do
                    {
                        if (index < displayTables.Count && merch != null)
                        {
                            var table = displayTables[index];

                            var strData = table.GetStringParamsByIDKey("ShopTable");
                            var point = Util.ParsePoint(strData[0]);
                            var offset = int.Parse(strData[1]);
                            var spots = int.Parse(strData[2]);

                            if (tableCount < maxTableSpots)
                            {
                                if (merch != null)
                                {
                                    placed = true;

                                    var merchSpot = point + new Point(offset * tableCount++, 0);
                                    _liMerchDisplayPoints.Add(new Tuple<Item, Point, float>(merch, table.MapPosition + merchSpot, table.Sprite.LayerDepth + 1));
                                }
                            }
                            else
                            {
                                index++;
                                if (index < displayTables.Count)
                                {
                                    tableCount = 0;
                                    maxTableSpots = GetMaxTableSpots(displayTables[index]);
                                }
                            }
                        }
                        else { break; }
                    } while (!placed);
                }
            }
        }

        private int GetMaxTableSpots(WorldObject obj)
        {
            var strData = obj.GetStringParamsByIDKey("ShopTable");
            return int.Parse(strData[2]);
        }

        //        if (_liMerchIDs != null && _liMerchIDs.Count > 0 && GetBoolByIDKey("ShopTable"))
        //            {
        //                var strData = GetStringParamsByIDKey("ShopTable");
        //        var point = Util.ParsePoint(strData[0]);
        //        var offset = int.Parse(strData[1]);
        //        var spots = int.Parse(strData[2]);
        //                for (int i = 0; i<spots && i<_liMerchIDs.Count; i++)
        //                {
        //                    _liMerchIDs[i]?.DrawShadow(true);
        //        _liMerchIDs[i]?.Draw(spriteBatch, new Rectangle(MapPosition.X + point.X, MapPosition.Y + point.Y, Constants.TILE_SIZE, Constants.TILE_SIZE), Sprite.LayerDepth + 1);

        //                    point.X += offset;
        //                }
        //}

        //          public bool MerchandiseSpaceLeft()
        //        {
        //            bool rv = false;
        //            if (GetBoolByIDKey("ShopTable"))
        //            {
        //                var strData = GetStringParamsByIDKey("ShopTable");
        //                rv = _liMerchIDs.Count + 1 < int.Parse(strData[2]);
        //            }
        //            return rv;
        //        }
        //        public void AddMerchandiseItem(Item i)
        //        {
        //            if (i != null)
        //            {
        //                _liMerchIDs.Add(i);
        //            }
        //        }

        //        public void ClearMerchandise()
        //        {
        //            _liMerchIDs.Clear();
        //        }

        /// <summary>
        /// Creates the TravelPoint object necessary for the given Building
        /// and calls CreateDoor to add the travel info to the correct RHTiles
        /// </summary>
        /// <param name="b">The building to create the door for</param>
        public void CreateBuildingEntrance(Building b)
        {
            TravelPoint buildPoint = new TravelPoint(b, this.Name, b.ID);
            _diTravelPoints.Add(b.InnerMapName, buildPoint); //TODO: FIX THIS
            CreateDoor(buildPoint, b.TravelBox.X, b.TravelBox.Y, b.TravelBox.Width, b.TravelBox.Height);
        }

        public void UpdateBuildingEntrance(string initialMapName, string newMapName)
        {
            if (_diTravelPoints.ContainsKey(initialMapName))
            {
                TravelPoint pt = _diTravelPoints[initialMapName];
                _diTravelPoints.Remove(initialMapName);
                _diTravelPoints[newMapName] = pt;
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
                GetTileByPixelPosition(vec)?.SetTravelPoint(trvlPt);
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
            string mapName = b.InnerMapName;
            TravelPoint pt = _diTravelPoints[mapName];

            foreach (Point vec in Util.GetAllPointsInArea(pt.Location.X, pt.Location.Y, pt.CollisionBox.Width, pt.CollisionBox.Height, Constants.TILE_SIZE))
            {
                RHTile t = GetTileByPixelPosition(vec);
                if (t != null)
                {
                    t.SetTravelPoint(null);
                }
            }

            _diTravelPoints.Remove(mapName);
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
                    else
                    {
                        it.PickupState = ItemPickupState.Manual;
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
                    if (npc != null && npc.ImpedesActor(actor)) { impeded = true; }
                    else if (npc == null || npc.BlocksActor(actor)) { diagonalChange.X = 0; }
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
            var intersect = Rectangle.Intersect(collisionKvp.Key, testRect);
            var tile = GetTileByPixelPosition(intersect.Center);

            Actor npc = collisionKvp.Value;
            Rectangle r = tile.TileBox;
            Point coords = Util.GetGridCoords(tile.TileBox.Location);


            Point location = actor.CollisionBoxLocation;
            Point size = actor.CollisionBox.Size;

            if (npc != null && npc.ImpedesActor(actor))
            {
                impeded = true;
            }
            else if (npc == null || npc.BlocksActor(actor))
            {
                int distance;
                bool positionLessThan;

                if (vertical) { positionLessThan = (r.Location.Y - actor.CollisionBox.Location.Y) > 0; }
                else { positionLessThan = (r.Location.X - actor.CollisionBox.Location.X) > 0; }

                if (vertical) { distance = positionLessThan ? r.Top - actor.CollisionBox.Bottom : r.Bottom - actor.CollisionBox.Top; }
                else { distance = positionLessThan ? r.Left - actor.CollisionBox.Right : r.Right - actor.CollisionBox.Left; }

                dirToCancel = Math.Abs(distance) <= Math.Abs(dirToCancel) ? distance : 0;

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
        public bool CheckForCollisions(Actor actor, ref Vector2 dir, ref bool impeded)
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
            else if (!actor.IgnoreCollisions)
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

            var tile = actor.CurrentMap.GetTileByPixelPosition(actor.CollisionBox.Center);
            if (tile != null && tile.Flooring != null)
            {
                dir *= 1.1f;
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
                foreach (KeyValuePair<string, TravelPoint> kvp in _diTravelPoints)
                {
                    if (kvp.Value.Intersects(movingChar) && !kvp.Value.IsDoor && kvp.Value.IsActive)
                    {
                        if (c != PlayerManager.PlayerActor || (!GameManager.InTownMode() && !PlayerManager.PlayerActor.HasKnockbackVelocity()))
                        {
                            MapManager.ChangeMaps(c, this.Name, kvp.Value);
                        }
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

        public void AddSpecialTile(RHTile t)
        {
            Util.AddUniquelyToList(ref _liSpecialTiles, t);
        }

        public void RemoveSpecialTile(RHTile t)
        {
            _liSpecialTiles.Remove(t);
        }

        public void AddCollectionItem(int itemID, int npcIndex, int index)
        {
            MapItem displayItem = new MapItem(DataManager.GetItem(itemID))
            {
                PickupState = ItemPickupState.None,
                Position = _diCharacterLayer[npcIndex + "Col" + index].Location
            };
            _liItems.Add(displayItem);
        }

        public Dictionary<string, string> GetMapProperties()
        {
            return _map.Properties;
        }
        public Dictionary<string, string> GetTileProperties(TiledMapTile tile)
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
            if (_diCharacterLayer.ContainsKey(val))
            {
                rv = _diCharacterLayer[val].Location;
            }
            return rv;
        }

        #region Input Processing
        private bool SkipClickProcessing()
        {
            return GamePaused() || CutsceneManager.Playing || PlayerManager.ToolInUse != null || !PlayerManager.PlayerActor.HasHP || PlayerManager.GrabbedObject != null || !GUIManager.NotFading;
        }
        /// <summary>
        /// The Map's left-click handler
        /// </summary>
        /// <param name="mouseLocation"></param>
        /// <returns></returns>
        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (SkipClickProcessing())
            {
                if (FishingManager.ProcessLeftButtonClick())
                {
                    rv = true;
                }
                return rv;
            }
            else if (!PlayerManager.Busy && !CutsceneManager.Playing)
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

                    if (i != null && i.HasUse() && i.CompareType(ItemTypeEnum.Tool))
                    {
                        int distance = 0;
                        if (PlayerManager.PlayerInRangeGetDist(GUICursor.GetWorldMousePosition(), 3 * Constants.TILE_SIZE, ref distance))
                        {
                            PlayerManager.FaceCursor();
                        }

                        RHTile playerTile = GetTileByPixelPosition(PlayerManager.PlayerActor.CollisionCenter);
                        TargetTile = playerTile.GetTileByDirection(PlayerManager.PlayerActor.Facing);

                        i.UseItem();
                    }
                    else
                    {
                        if (TargetTile != null)
                        {
                            if (!PlayerManager.InRangeOfPlayer(TargetTile.CollisionBox))
                            {
                                var max = TargetTile.IsWaterTile ? 10 : 1;
                                var init = TargetTile;
                                var temp = MapManager.CurrentMap.GetTileByPixelPosition(PlayerManager.PlayerActor.CollisionBox.Center);
                                for (int x = 0; x < max; x++)
                                {
                                    var dir = Util.GetDirectionFromPlayer(TargetTile.CollisionBox.Center);
                                    TargetTile = temp.GetTileByDirection(dir);
                                    temp = TargetTile;

                                    if(TargetTile == init)
                                    {
                                        break;
                                    }
                                }
                            }

                            //Retrieves any object associated with the tile, this will include
                            //both actual tiles, and Shadow Tiles because the user sees Shadow Tiles
                            //as being on the tile.
                            WorldObject obj = TargetTile.GetWorldObject(false);
                            if (TargetTile.IsTilled && !TargetTile.HasBeenWatered)
                            {
                                PlayerManager.FaceCursor();
                                PlayerManager.SetTool(PlayerManager.RetrieveTool(ToolEnum.WateringCan));
                                rv = true;
                            }
                            else if (obj != null && obj.Tiles().Any(x => PlayerManager.InRangeOfPlayer(x.CollisionBox)))
                            {
                                rv = obj.ProcessLeftClick();
                            }
                            else if (TargetTile.IsWaterTile)
                            {
                                PlayerManager.FaceCursor();
                                PlayerManager.SetTool(PlayerManager.RetrieveTool(ToolEnum.FishingRod));
                                rv = true;
                            }
                        }
                    }
                }
            }

            return rv;
        }

        public bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (SkipClickProcessing())
            {
                if (FishingManager.ProcessRightButtonClick())
                {
                    return true;
                }
                else
                {
                    return rv;
                }
            }

            if (InTownMode())
            {
                rv = true;
                TownModeRightClick();
            }
            else
            {
                RHTile tile = MouseTile;

                if (PlayerManager.PlayerActor.Mounted && (tile == null || tile.GetTravelPoint() == null || !PlayerManager.PlayerActor.ActiveMount.CanEnterBuilding(tile.GetTravelPoint().LinkedMap)))
                {
                    PlayerManager.PlayerActor.Dismount();
                    return true;
                }

                //Do nothing if no tile could be retrieved
                if (tile != null)
                {
                    if (!rv)
                    {
                        rv = tile.ProcessRightClick();
                    }

                    if (!rv)
                    {
                        foreach(var obj in _liHoverObjects)
                        {
                            if (PlayerManager.InRangeOfPlayer(obj.BaseRectangle) && obj.GetHoverBox().Contains(mouseLocation) && obj.ProcessRightClick())
                            {
                                rv = true;
                                break;
                            }
                        }
                    }

                    if (!rv)
                    {
                        foreach (Actor c in _liActors)
                        {
                            if (PlayerManager.InRangeOfPlayer(c.HoverBox) && c.HoverContains(mouseLocation) && c.OnTheMap)
                            {
                                c.ProcessRightButtonClick();
                                return true;
                            }
                        }
                    }
                }

                var removedList = new List<MapItem>();
                for (int i = 0; i < _liItems.Count; i++)
                {
                    MapItem it = _liItems[i];
                    if (it.PickupState == ItemPickupState.Manual && it.CollisionBox.Contains(GUICursor.GetWorldMousePosition()) && PlayerManager.InRangeOfPlayer(it.CollisionBox))
                    {
                        if (InventoryManager.AddMapItemToInventory(it.WrappedItem))
                        {
                            removedList.Add(it);
                            break;
                        }
                    }
                }
                removedList.ForEach(x => _liItems.Remove(x));
                removedList.Clear();

                if (!rv)
                {
                    foreach (var obj in CharacterObjects)
                    {
                        var name = obj.Key;
                        Rectangle objRect = obj.Value;
                        DirectionEnum facing = DirectionEnum.None;
                        if (objRect.Contains(mouseLocation) && PlayerManager.InRangeOfPlayer(objRect, ref facing))
                        {
                            if (name.Equals("Town_Display"))
                            {
                                rv = true;
                                GUIManager.OpenMainObject(new HUDTownManagement());
                            }
                            else if (name.Equals("Display_Upgrade"))
                            {
                                rv = true;
                                GUIManager.OpenMainObject(new HUDBuildingUpgrade(TownManager.GetBuildingByID(this.BuildingID)));
                            }
                            else if (name.StartsWith(Constants.MAPOBJ_SHOP))
                            {
                                foreach(var actor in _liActors)
                                {
                                    if(actor.CollisionBox.Intersects(objRect))
                                    {
                                        rv = true;
                                        actor.ProcessRightButtonClick();
                                        break;
                                    }
                                }
                            }
                        }

                        if (rv)
                        {
                            PlayerManager.PlayerActor.SetFacing(facing);
                            break;
                        }

                    }
                }

                if (!rv)
                {
                    PlayerManager.SetActiveItem(GameManager.CurrentItem);
                }
            }

            return rv;
        }

        public bool ProcessHover(Point mouseLocation)
        {
            bool rv = false;

            if (CutsceneManager.Playing)
            {
                return rv;
            }

            GameManager.TestTiles.Clear();

            _objSelectedObject?.SelectObject(false);
            _objSelectedObject = null;
            if (TownModeEdit() && GameManager.HeldObject == null && MouseTile != null && MouseTile.HasObject())
            {
                WorldObject obj = MouseTile.RetrieveObjectFromLayer(true);
                if (obj != null && obj.PlayerCanEdit())
                {
                    _objSelectedObject = (Buildable)obj;
                    _objSelectedObject.SelectObject(true);
                }
            }

            if (GameManager.HeldObject != null)
            {
                GameManager.TestTiles.Clear();
                TestMapTiles(GameManager.HeldObject, GameManager.TestTiles);
            }
            else if (!InTownMode())
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
                        GUICursor.SetCursor(GUICursor.CursorTypeEnum.Pickup, t.GetWorldObject().BaseRectangle);
                    }

                    MapItem hoverItem = _liItems.Find(x => x.CollisionBox.Contains(GUICursor.GetWorldMousePosition()));
                    if (hoverItem != null && hoverItem.PickupState == ItemPickupState.Manual)
                    {
                        found = true;
                        GUICursor.SetCursor(GUICursor.CursorTypeEnum.Pickup, hoverItem.CollisionBox);
                    }

                    foreach(var obj in _liHoverObjects)
                    {
                        if (obj.ProcessHover(mouseLocation))
                        {
                            found = true;
                            break;
                        }
                    }
                }

                foreach (Actor c in _liActors)
                {
                    if (c.HoverContains(mouseLocation) && c.OnTheMap)
                    {
                        switch (c.ActorType)
                        {
                            case ActorTypeEnum.Merchant:
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

                foreach (var obj in CharacterObjects)
                {
                    var name = obj.Key;
                    Rectangle objRect = obj.Value;

                    if (objRect.Contains(mouseLocation))
                    {
                        if (name.StartsWith(Constants.MAPOBJ_SHOP))
                        {
                            foreach (var actor in _liActors)
                            {
                                if (actor.CollisionBox.Intersects(objRect))
                                {
                                    found = true;
                                    GUICursor.SetCursor(GUICursor.CursorTypeEnum.Talk, objRect);
                                    break;
                                }
                            }
                        }
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
            if (InTownMode())
            {
                var toBuild = GameManager.HeldObject;

                //If we are holding a WorldObject, we should attempt to place it
                if (GameManager.HeldObject != null)
                {
                    if (toBuild.CompareType(ObjectTypeEnum.Buildable))
                    {
                        switch (toBuild.GetEnumByIDKey<BuildableEnum>("Subtype"))
                        {
                            case BuildableEnum.Floor:
                                if (MouseTile.Flooring == null) { goto default; }
                                break;
                            case BuildableEnum.Wallpaper:
                                rv = TownModePlaceWallpaper((Wallpaper)toBuild);
                                break;
                            default:
                                rv = TownPlaceObject((Buildable)toBuild);
                                break;
                        }
                    }
                    else if (toBuild is Plant plantObj)
                    {
                        rv = TownPlaceSeed(plantObj);
                    }

                    if (rv)
                    {
                        SoundManager.PlayEffect(SoundEffectEnum.Thump);
                    }
                    else
                    {
                        SoundManager.PlayEffect(SoundEffectEnum.Cancel);
                    }
                }
                else if (GameManager.HeldObject == null && MouseTile != null)
                {
                    if (MouseTile.HasObject())
                    {
                        WorldObject targetObj = MouseTile.RetrieveObjectFromLayer(true);
                        if (targetObj != null)
                        {
                            targetObj = targetObj.Pickup;
                            if (targetObj != null && targetObj.PlayerCanEdit())
                            {
                                Buildable b = (Buildable)targetObj;
                                switch (b.GetEnumByIDKey<BuildableEnum>("Subtype"))
                                {
                                    case BuildableEnum.Building:
                                        RemoveDoor((Building)targetObj);
                                        GUICursor.ResetCursor();
                                        goto default;
                                    case BuildableEnum.Decor:
                                        Decor obj = (Decor)targetObj;
                                        if (obj.HasDisplay)
                                        {
                                            obj.StoreDisplayEntity();
                                            break;
                                        }
                                        else
                                        {
                                            goto default;
                                        }
                                    default:
                                        PickUpWorldObject(mouseLocation, targetObj);
                                        break;
                                }
                                rv = true;
                            }
                        }
                    }
                }
            }

            return rv;
        }

        private void TownModeRightClick()
        {
            if ((HeldObject == null && !TownModeEdit()) || HeldObject == null)
            {
                GameManager.ExitTownMode();
            }
            else
            {
                Item dummyItem;
                if (HeldObject.Type == ObjectTypeEnum.Plant && HeldObject.GetBoolByIDKey("SeedID")) { dummyItem = DataManager.GetItem(HeldObject.GetIntByIDKey("SeedID")); }
                else { dummyItem = DataManager.GetItem((Buildable)HeldObject); }

                if (!TownModeBuild() && dummyItem != null && InventoryManager.HasSpaceInInventory(dummyItem.ID, 1) && (HeldObject.Type == ObjectTypeEnum.Plant || !((Buildable)HeldObject).Unique))
                {
                    InventoryManager.AddToInventory(dummyItem.ID, 1, true, true);
                    GameManager.EmptyHeldObject();
                }
                else if (TownModeBuild())
                {
                    PostBuildingCleanup(true);
                    GameManager.EmptyHeldObject();
                }
                else if (dummyItem == null)
                {
                    LogManager.WriteToLog("Dummy item is NULL", LogEnum.Warning);
                }
            }
        }

        /// <summary>
        /// Helper method for building one object and allowing the
        /// possibility of building additional ones
        /// </summary>
        /// <param name="objectToPlace">The Structure that will act as the template to build offo f</param>
        /// <returns>True if we successfully build.</returns>
        private bool TownPlaceObject(Buildable objectToPlace)
        {
            bool rv = false;

            //PlaceOnMap uses the CollisionBox as the base, then calculates backwards
            objectToPlace.SnapPositionToGrid(objectToPlace.BaseRectangle.Location);

            if (objectToPlace.PlaceOnMap(this) && (!TownModeBuild() || InventoryManager.ExpendResources(objectToPlace.RequiredToMake)))
            {
                if (this == MapManager.TownMap)
                {
                    TaskManager.AdvanceTaskProgress(objectToPlace);
                }

                if (objectToPlace is AdjustableObject adjustable)
                {
                    adjustable.AdjustObject();
                }

                Item dummyItem = DataManager.GetItem((Buildable)HeldObject);
                bool directBuild = TownModeBuild() && InventoryManager.HasSufficientItems(objectToPlace.RequiredToMake);
                if (!directBuild)
                {
                    if (dummyItem == null || !InventoryManager.HasItemInPlayerInventory(dummyItem.ID, 1))
                    {
                        GameManager.EmptyHeldObject();

                        if (TownModeBuild())
                        {
                            PostBuildingCleanup(false);
                        }
                    }
                    else if (InventoryManager.HasItemInPlayerInventory(dummyItem.ID, 1))
                    {
                        InventoryManager.RemoveItemsFromInventory(dummyItem.ID, 1);
                        var dummyObject = DataManager.CreateWorldObjectByID(dummyItem.ID);
                        
                        if (objectToPlace is Decor decorObject && dummyObject is Decor decorDummy)
                        {
                            decorDummy.RotateToDirection(decorObject.Facing);
                        }

                        GameManager.PickUpWorldObject(dummyObject);
                    }
                }

                rv = true;
            }

            return rv;
        }

        /// <summary>
        /// Helper method for building one object and allowing the
        /// possibility of building additional ones
        /// </summary>
        /// <param name="templateObject">The Structure that will act as the template to build offo f</param>
        /// <returns>True if we successfully build.</returns>
        private bool TownPlaceSeed(Plant templateObject)
        {
            bool rv = false;
            var placeObject = DataManager.CreateWorldObjectByID(templateObject.ID);

            //PlaceOnMap uses the CollisionBox as the base, then calculates backwards
            placeObject.SnapPositionToGrid(templateObject.BaseRectangle.Location);

            var seedID = placeObject.GetIntByIDKey("SeedID");
            if (placeObject.PlaceOnMap(this) && (!TownModeBuild() || InventoryManager.HasItemInPlayerInventory(seedID, 1)))
            {
                if (this == MapManager.TownMap)
                {
                    AddToWorldObjects(placeObject);
                    TaskManager.AdvanceTaskProgress(placeObject);
                }

                //Check for if we are done placing the object of that type
                if (InventoryManager.HasItemInPlayerInventory(seedID, 1))
                {
                    InventoryManager.RemoveItemsFromInventory(seedID, 1);
                }
                else
                {
                    GameManager.EmptyHeldObject();
                }

                rv = true;
            }

            return rv;
        }

        private bool TownModePlaceWallpaper(Wallpaper wallpaperObject)
        {
            bool rv = false;

            if (TargetTile.IsWallpaperWall && InventoryManager.ExpendResources(wallpaperObject.RequiredToMake))
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

        public void PostBuildingCleanup(bool openMenu)
        {
            SetGameScale(Constants.NORMAL_SCALE);

            if (openMenu)
            {
                GUIManager.OpenMenu();
                GUIManager.GetMenu().BtnBuild();
            }

            GameManager.ExitTownMode();
        }

        public void RemoveWorldObject(WorldObject o, bool immediately = false)
        {
            if (immediately) { DIRemoveObject(o); }
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
        /// </summary>
        /// <param name="mouseLocation"></param>
        public void PickUpWorldObject(Point mouseLocation, WorldObject targetObj)
        {
            SoundManager.PlayEffect(SoundEffectEnum.GrabBuilding);
            GameManager.PickUpWorldObject(targetObj);
            targetObj.SetPickupOffset(mouseLocation.ToVector2());
            targetObj.RemoveSelfFromTiles();
            RemoveLights(targetObj.GetLights());

            DIRemoveObject(targetObj);
        }

        //This should only be called by the object during PlaceOnMap
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
            Vector2 position = obj.BaseRectangle.Location.ToVector2();
            position.X = ((int)(position.X / Constants.TILE_SIZE)) * Constants.TILE_SIZE;
            position.Y = ((int)(position.Y / Constants.TILE_SIZE)) * Constants.TILE_SIZE;

            int colColumns = obj.BaseRectangle.Width / Constants.TILE_SIZE;
            int colRows = obj.BaseRectangle.Height / Constants.TILE_SIZE;

            //This is used to get all the tiles based off the collisonbox size
            for (int i = 0; i < colRows; i++)
            {
                for (int j = 0; j < colColumns; j++)
                {
                    int x = Math.Min((obj.BaseRectangle.Left + (j * Constants.TILE_SIZE)) / Constants.TILE_SIZE, MapWidthTiles - 1);
                    int y = Math.Min((obj.BaseRectangle.Top + (i * Constants.TILE_SIZE)) / Constants.TILE_SIZE, MapHeightTiles - 1);
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
            if (obj.BuildableType(BuildableEnum.Floor))
            {
                if (testTile.Flooring == null && (testTile.TileCheck() && testTile.WorldObject == null))
                {
                    rv = true;
                }
            }
            else if (obj.CheckPlacement(ObjectPlacementEnum.Wall))
            {
                if (obj is Wallpaper)
                {
                    rv = testTile.WorldObject == null && testTile.IsWallpaperWall;
                }
                else
                {
                    rv = testTile.WorldObject == null && testTile.ContainsProperty("Wall");
                }
            }
            else if (obj.CheckPlacement(ObjectPlacementEnum.Impassable))
            {
                rv = testTile.WorldObject == null && !testTile.TileCheck();
            }
            else if (ignoreActors || !TileContainsBlockingActor(testTile))
            {
                if (testTile.CanPlaceOnTabletop(obj) || (testTile.TileCheck() && testTile.WorldObject == null))
                {
                    rv = true;
                }
            }

            if (obj.WideOnTop())
            {
                List<RHTile> arr = testTile.GetAdjacentTiles(true);
                for (int i = 0; i < arr.Count; i++)
                {
                    if (arr[i].WorldObject != null && arr[i].WorldObject.WideOnTop())
                    {
                        rv = false;
                        break;
                    }
                }
            }

            if(obj is Plant plantObj)
            {
                if (GameManager.HeldObject != null && plantObj.ID == GameManager.HeldObject.ID && plantObj.NeedsWatering && !testTile.IsTilled)
                {
                    rv = false;
                }
            }

            if (rv && GameManager.TownModeEdit() && Lighting == 0)
            {
                if (!PlayerManager.PlayerInRange(testTile.Center, (int)(Constants.TILE_SIZE * 1.5)))
                {
                    bool found = false;
                    foreach (Light v in _liLights)
                    {
                        if (v.Contains(testTile.Center))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found) { rv = false; }
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
            AddToWorldObjects(obj);

            //Sets the WorldObject to each RHTile
            tiles.ForEach(x => x.SetObject(obj));

            var tileList = obj.Tiles();
            //Iterate over the WorldObject image in Constants.TILE_SIZE increments to discover any tiles
            //that the image overlaps. Add those tiles as Shadow Tiles as long as they're not
            //actual Tiles the object sits on. Also add the Tiles to the objects Shadow Tiles list
            for (int i = obj.MapPosition.X; i < obj.MapPosition.X + obj.Width; i += Constants.TILE_SIZE)
            {
                for (int j = obj.MapPosition.Y; j < obj.MapPosition.Y + obj.Height; j += Constants.TILE_SIZE)
                {
                    RHTile t = GetTileByGridCoords(Util.GetGridCoords(i, j));
                    if (t != null && !tileList.Contains(t))
                    {
                        t.SetShadowObject(obj);
                    }
                }
            }

            var actors = _liActors.FindAll(x => obj.BaseRectangle.Contains(x.CollisionBox) && x.IsActorType(ActorTypeEnum.Critter)).Cast<Critter>().ToList();
            actors.ForEach(x => x.Flee());
        }

        public void SendVillagersToTown()
        {
            var copy = new List<Actor>(_liActors);
            foreach(var actor in copy)
            {
                if (actor.IsActorType(ActorTypeEnum.Villager))
                {
                    Villager v = (Villager)_liActors.Find(x => x.ID == actor.ID);
                    v.SendToTown();
                }
            }
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

        public void AddMobByPosition(Mob m, Point position)
        {
            m.CurrentMapName = _sName;
            m.SetPosition(Util.SnapToGrid(position));

            _liMobs.Add(m);
        }
        #endregion

        public Building Building()
        {
            return TownManager.GetBuildingByID(BuildingID);
        }

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
            _liMobs.ForEach(x => x.Reset());
            _liItems.Clear();

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

            if (GetMapProperties().ContainsKey("PetCafe"))
            {
                foreach(var p in _liActors.Where(x => x is Pet))
                {
                    RemoveActor(p);
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
                    if (tile != null && !rvList.Contains(tile))
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

        public void RemoveTilesNearTravelPoints(ref List<RHTile> tiles)
        {
            foreach(var travelPoint in _diTravelPoints)
            {
                foreach(var t in GetTilesFromRectangleExcludeEdgePoints(travelPoint.Value.CollisionBox))
                {
                    if (tiles.Contains(t)) { tiles.Remove(t); }
                    
                    foreach (var o in t.GetAdjacentTiles())
                    {
                        if (tiles.Contains(o))
                        {
                            tiles.Remove(o);
                        }
                    }
                }
            }
        }

        public void RemoveTilesNearSpecialObjects(ref List<RHTile> tiles)
        {
            foreach (TiledMapObject tiledObj in _liMapObjects)
            {
                if (tiledObj.Properties.ContainsKey("Reset") && tiledObj.Properties.ContainsKey("ObjectID"))
                {
                    var box = new Rectangle((int)tiledObj.Position.X, (int)tiledObj.Position.Y, (int)tiledObj.Size.Width, (int)tiledObj.Size.Height);

                    var objectTiles = GetTilesFromRectangleExcludeEdgePoints(box);
                    foreach (var objTile in objectTiles)
                    {
                        var adjTiles = objTile.GetAdjacentTiles();
                        foreach (var adjTile in adjTiles)
                        {
                            if (tiles.Contains(adjTile))
                            {
                                tiles.Remove(adjTile);
                            }
                        }
                    }
                }
            }
        }

        private void RemoveSkipTiles(ref List<RHTile> tiles)
        {
            var objectLayer = Map.ObjectLayers.First(x => x.Name.Contains("MapObject"));
            var skipObjs = objectLayer.Objects.Where(x => x.Name.Equals("Skip"));
            foreach (var obj in skipObjs)
            {
                foreach (var tile in GetTilesFromRectangleExcludeEdgePoints(Util.RectFromTiledMapObject(obj)))
                {
                    tiles.Remove(tile);
                }
            }
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
                mapName = Name,
                visited = Visited,
                worldObjects = new List<WorldObjectData>(),
                specialTiles = new List<RHTileData>()
            };

            if (!IsDungeon)
            {
                foreach (var objectList in _diWorldObjects.Values)
                {
                    foreach (WorldObject wObj in objectList)
                    {
                        if (!wObj.Reset && !(wObj is SubObject))
                        {
                            mapData.worldObjects.Add(wObj.SaveData());
                        }
                    }
                }

                _liSpecialTiles.ForEach(x => mapData.specialTiles.Add(x.SaveData()));
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
                }

                //obj?.PlaceOnMap(new Vector2(w.x, w.y), this);
                //if (obj != null && this == MapManager.TownMap) { TownManager.AddToTownObjects(obj); }
            }
        }
    }
}
