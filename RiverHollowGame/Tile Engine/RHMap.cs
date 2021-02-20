using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using static RiverHollow.RiverHollow;
using static RiverHollow.Game_Managers.CombatManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Items.WorldItem;
using static RiverHollow.Items.Item;
using static RiverHollow.Items.WorldItem.Floor;
using static RiverHollow.GUIComponents.Screens.HUDMenu;
using static RiverHollow.Items.WorldItem.Machine;

namespace RiverHollow.Tile_Engine
{
    public class RHMap
    {
        public int MapWidthTiles = 100;
        public int MapHeightTiles = 100;
        
        private string _sName;
        public string Name { get => _sName.Replace(@"Maps\", ""); } //Fuck off with that path bullshit

        public bool IsCombatMap => _liMonsterSpawnPoints.Count > 0;
        public bool IsBuilding { get; private set; }
        public string ConstructionZone { get; private set; } = string.Empty;
        public string DungeonName { get; private set; } = string.Empty;
        public bool IsDungeon => !string.IsNullOrEmpty(DungeonName);
        public bool IsTown { get; private set; }
        public bool IsManor { get; private set; }
        bool _bOutside;
        public bool IsOutside => _bOutside;
        bool _bProduction = true;
        public bool Production => _bProduction;
        int _iActiveSpawnPoints;
        public int ActiveSpawnPoints => _iActiveSpawnPoints;
        public string BackgroundMusic { get; private set; }
        public MonsterFood PrimedFood { get; private set; }
        public RHTile TargetTile { get; private set; } = null;

        protected TiledMap _map;
        public TiledMap Map { get => _map; }

        protected RHTile[,] _arrTiles;
        protected TiledMapRenderer _renderer;
        protected List<TiledMapTileset> _liTilesets;
        protected Dictionary<string, TiledMapTileLayer> _diLayers;
        public Dictionary<string, TiledMapTileLayer> Layers => _diLayers;

        private List<RHTile> _liTestTiles;
        private List<WorldActor> _liActors;
        public List<Monster> Monsters { get; }
        private List<Summon> _liSummons;
        public List<WorldActor> ToAdd;
        private List<Building> _liBuildings;
        private List<WorldObject> _liPlacedWorldObjects;
        public List<RHTile> TilledTiles { get; }
        private List<MonsterSpawn> _liMonsterSpawnPoints;
        private List<ResourceSpawn> _liResourceSpawnPoints;
        private List<int> _liRandomSpawnItems;
        private List<int> _liCutscenes;
        private List<TiledMapObject> _liBarrenObjects;
        private Dictionary<RarityEnum, List<int>> _diResources;
        protected List<Item> _liItems;
        protected List<ShopData> _liShopData;
        public Dictionary<string, RHTile[,]> DictionaryCombatTiles { get; }
        public Dictionary<string, TravelPoint> DictionaryTravelPoints { get; }
        public Dictionary<string, Vector2> DictionaryCharacterLayer { get; }
        private List<TiledMapObject> _liMapObjects;

        private List<Item> _liItemsToRemove;
        private List<WorldActor> _liActorsToRemove;
        private List<WorldObject> _liObjectsToRemove;

        public RHMap() {
            _liMonsterSpawnPoints = new List<MonsterSpawn>();
            _liResourceSpawnPoints = new List<ResourceSpawn>();
            _liTestTiles = new List<RHTile>();
            _liTilesets = new List<TiledMapTileset>();
            _liActors = new List<WorldActor>();
            Monsters = new List<Monster>();
            _liSummons = new List<Summon>();
            _liBuildings = new List<Building>();
            TilledTiles = new List<RHTile>();
            _liItems = new List<Item>();
            _liMapObjects = new List<TiledMapObject>();
            _liShopData = new List<ShopData>();
            _liPlacedWorldObjects = new List<WorldObject>();
            _liRandomSpawnItems = new List<int>();
            _liCutscenes = new List<int>();
            _liBarrenObjects = new List<TiledMapObject>();
            _diResources = new Dictionary<RarityEnum, List<int>>();

            DictionaryCombatTiles = new Dictionary<string, RHTile[,]>();
            DictionaryTravelPoints = new Dictionary<string, TravelPoint>();
            DictionaryCharacterLayer = new Dictionary<string, Vector2>();

            _liItemsToRemove = new List<Item>();
            _liActorsToRemove = new List<WorldActor>();
            _liObjectsToRemove = new List<WorldObject>();

            ToAdd = new List<WorldActor>();
        }

        public RHMap(RHMap map) : this()
        {
            _map = map.Map;
            _sName = map.Name+"Clone";
            _renderer = map._renderer;
            _arrTiles = map._arrTiles;

            ConstructionZone = map.ConstructionZone;
            IsBuilding = _map.Properties.ContainsKey("Building");
            IsTown = _map.Properties.ContainsKey("Town");
            _bOutside = _map.Properties.ContainsKey("Outside");
            IsManor = _map.Properties.ContainsKey("Manor");

            if (_map.Properties.ContainsKey("Dungeon"))
            {
                DungeonName = _map.Properties["Dungeon"];
                DungeonManager.AddMapToDungeon(_map.Properties["Dungeon"], this);
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
            if (_map.Properties.ContainsKey("ConstructionZone"))
            {
                ConstructionZone = _map.Properties["ConstructionZone"];
            }
            IsBuilding = _map.Properties.ContainsKey("Building");
            IsTown = _map.Properties.ContainsKey("Town");
            _bOutside = _map.Properties.ContainsKey("Outside");
            IsManor = _map.Properties.ContainsKey("Manor");

            if (_map.Properties.ContainsKey("Dungeon"))
            {
                DungeonName = _map.Properties["Dungeon"];
                DungeonManager.AddMapToDungeon(_map.Properties["Dungeon"], this);
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

            if (_map.Properties.ContainsKey("Resources"))
            {
                string[] spawnResources = Util.FindParams(_map.Properties["Resources"]);
                foreach (string s in spawnResources)
                {
                    int resourceID = -1;
                    RarityEnum rarity = RarityEnum.C;
                    Util.GetRarity(s, ref resourceID, ref rarity);

                    if (!_diResources.ContainsKey(rarity)) {
                        _diResources[rarity] = new List<int>();
                    }

                    _diResources[rarity].Add(resourceID);
                }
            }

            if (_map.Properties.ContainsKey("Background"))
            {
                BackgroundMusic = _map.Properties["Background"];
            }

            if (_map.Properties.ContainsKey("Cutscenes"))
            {
                string[] split = _map.Properties["Cutscenes"].Split(' ');
                foreach(string cutsceneID in split)
                {
                    _liCutscenes.Add(int.Parse(cutsceneID));
                }
            }

            if (IsTown)
            {
                foreach (KeyValuePair<int, Upgrade> kvp in GameManager.DiUpgrades)
                {
                    if (kvp.Value.Enabled) { EnableUpgradeVisibility(kvp.Key); }
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
                if (CombatManager.InCombat || IsRunning())
                {
                    foreach (Monster m in Monsters)
                    {
                        m.Update(gTime);
                    }

                    foreach (Summon s in _liSummons)
                    {
                        s.Update(gTime);
                    }
                }

                foreach (Item i in _liItems)
                {
                    ((Item)i).Update(gTime);
                }
            }

            foreach (WorldObject obj in _liObjectsToRemove) { _liPlacedWorldObjects.Remove(obj); }
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
                if (c.IsActorType(ActorEnum.Monster) && Monsters.Contains((Monster)c)) { Monsters.Remove((Monster)c); }
                else if (_liActors.Contains(c)) { _liActors.Remove(c); }
                else if (c.IsActorType(ActorEnum.Summon) && _liSummons.Contains((Summon)c)) { _liSummons.Remove((Summon)c); }
            }
            _liActorsToRemove.Clear();

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
                if (CombatManager.ActiveCharacter != null && CombatManager.ActiveCharacter.IsActorType(ActorEnum.Adventurer))
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

            foreach (Monster m in Monsters)
            {
                m.Draw(spriteBatch, true);
            }

            foreach (Summon s in _liSummons)
            {
                s.Draw(spriteBatch, true);
            }

            foreach (Building b in _liBuildings)
            {
                b.Draw(spriteBatch);
            }

            foreach (RHTile t in TilledTiles)
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

            foreach (RHTile t in _liTestTiles)
            {
                WorldObject it = GameManager.ConstructionObject;
                bool checkPlayer = true;

                if (it != null) { checkPlayer = !it.CompareType(ObjectTypeEnum.Floor); }

                bool passable = t.Passable() && !TileContainsActor(t, checkPlayer);
                spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), new Rectangle((int)t.Position.X, (int)t.Position.Y, TileSize, TileSize), new Rectangle(288, 128, TileSize, TileSize), passable ? Color.Green * 0.5f : Color.Red * 0.5f, 0, Vector2.Zero, SpriteEffects.None, 99999);
            }
        }

        public void DrawLights(SpriteBatch spriteBatch)
        {
            foreach (WorldObject obj in _liPlacedWorldObjects)
            {
                if (obj.CompareType(ObjectTypeEnum.Light))
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
                if (IsTown)
                {
                    foreach (KeyValuePair<int, Upgrade> s in GameManager.DiUpgrades)    //Check each upgrade to see if it's enabled
                    {
                        if (l.Name.Contains(s.Key.ToString()))
                        {
                            upgrade = true;
                        }
                        if (s.Value.Enabled)
                        {
                            bool determinant = l.Name.Contains("Upper");
                            if (revealUpper)
                            {
                                l.IsVisible = determinant;
                            }
                            else { l.IsVisible = !determinant; }
                        }
                    }
                }

                if (!upgrade)
                {
                    bool determinant = l.Name.Contains("Upper");

                    if (revealUpper)
                    {
                        l.IsVisible = determinant;
                    }
                    else { l.IsVisible = !determinant; }
                }

                if (l.IsVisible && _bOutside)
                {
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

        public void WaterTiles()
        {
            foreach(RHTile t in TilledTiles)
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
                            TravelPoint trvlPt = new TravelPoint(mapObject, this.Name);
                            if (mapObject.Properties.ContainsKey("Door")) {
                                trvlPt.SetDoor();
                                CreateDoor(ref trvlPt, mapObject.Position.X, mapObject.Position.Y, mapObject.Size.Width, mapObject.Size.Height);
                            }
                            DictionaryTravelPoints.Add(trvlPt.LinkedMap, trvlPt);
                        }
                    }
                }
                else if (ol.Name.Contains("Character"))
                {
                    foreach (TiledMapObject obj in ol.Objects)
                    {
                        if (obj.Name.Equals("CombatStart"))
                        {
                            string entrance = obj.Properties["Map"];
                            RHTile[,] tiles = new RHTile[3, 3];

                            DirectionEnum sidle = DirectionEnum.Right;
                            DirectionEnum change = DirectionEnum.Down;

                            string startPoint = obj.Properties["Position"];
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

                            tiles[0, 0] = GetTileByPixelPosition(obj.Position);
                            tiles[1, 0] = tiles[0, 0].GetTileByDirection(sidle);
                            tiles[2, 0] = tiles[1, 0].GetTileByDirection(sidle);

                            tiles[0, 1] = tiles[0, 0].GetTileByDirection(change);
                            tiles[1, 1] = tiles[0, 1].GetTileByDirection(sidle);
                            tiles[2, 1] = tiles[1, 1].GetTileByDirection(sidle);

                            tiles[0, 2] = tiles[0, 1].GetTileByDirection(change);
                            tiles[1, 2] = tiles[0, 2].GetTileByDirection(sidle);
                            tiles[2, 2] = tiles[1, 2].GetTileByDirection(sidle);
                            DictionaryCombatTiles[entrance] = tiles;
                        }
                        else if (obj.Name.Equals("Shop"))
                        {
                            _liShopData.Add(new ShopData(_sName, obj));
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
                        else if (obj.Name.Equals("Barren"))
                        {
                            _liBarrenObjects.Add(obj);
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
                    TriggerObject d = DataManager.GetDungeonObject(obj.Properties, Util.SnapToGrid(obj.Position));

                    PlaceWorldObject(d);
                    GameManager.AddTrigger(d);
                 }
                else if (obj.Name.Equals("WorldObject"))
                {
                    //AddMachine
                    WorldObject w = DataManager.GetWorldObject(int.Parse(obj.Properties["ObjectID"]), Util.SnapToGrid(obj.Position));
                    if (PlaceWorldObject(w))
                    {
                        if (w.CompareType(ObjectTypeEnum.Machine)) { GameManager.AddMachine((Machine)w, this.Name); }
                    }
                }
                else if (obj.Name.Equals("Chest"))
                {
                    Container c = (Container)DataManager.GetWorldObject(190);
                    InventoryManager.InitContainerInventory(c.Inventory);
                    c.SnapPositionToGrid(obj.Position);
                    PlacePlayerObject(c);
                    string[] holdSplit = obj.Properties["Holding"].Split('/');
                    foreach (string s in holdSplit)
                    {
                        InventoryManager.AddToInventory(int.Parse(s), 1, false);
                    }
                    InventoryManager.ClearExtraInventory();
                }
                else if (obj.Name.Equals("SpawnPoint"))
                {
                    _liMonsterSpawnPoints.Add(new MonsterSpawn(this, obj));
                }
                else if (obj.Name.Equals("Manor") && !loaded)
                {
                    Building manor = DataManager.GetManor();
                    manor.SnapPositionToGrid(obj.Position);
                    manor.SetName(PlayerManager.ManorName);
                    AddBuilding(manor, true);
                }
                else if (obj.Name.Equals("Item"))
                {
                    Item item = DataManager.GetItem(int.Parse(obj.Properties["ItemID"]));
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

            List<RHTile> skipTiles = new List<RHTile>();
            foreach(RHTile[,] tileArray in DictionaryCombatTiles.Values)
            {
                foreach (RHTile tile in tileArray)
                {
                    skipTiles.Add(tile);
                }
            }
            foreach(TravelPoint tp in DictionaryTravelPoints.Values)
            {
                foreach(RHTile tile in GetTilesFromRectangle(tp.CollisionBox))
                {
                    Util.AddUniquelyToList(ref skipTiles, tile);

                    foreach (RHTile neighbour in tile.GetWalkableNeighbours())
                    {
                        Util.AddUniquelyToList(ref skipTiles, neighbour);
                    }
                }
            }
            foreach(MonsterSpawn spawn in _liMonsterSpawnPoints)
            {
                foreach (KeyValuePair<string, RHTile[,]> kvp in DictionaryCombatTiles)
                {
                    Vector2 pos = spawn.Position;
                    List<RHTile> path = TravelManager.FindPathToLocation(ref pos, kvp.Value[0, 0].Position, this.Name, false, true);
                    if (path != null)
                    {
                        bool connected = false;
                        foreach (RHTile tile in path)
                        {
                            if (!skipTiles.Contains(tile))
                            {
                                foreach(RHTile neighbour in tile.GetWalkableNeighbours())
                                {
                                    if (skipTiles.Contains(neighbour) && !path.Contains(neighbour))
                                    {
                                        connected = true;
                                        break;
                                    }
                                }
                                skipTiles.Add(tile);
                                if (connected) { break; }

                            }
                        }
                    }
                    else
                    {
                        int i = 0;
                    }
                }
                skipTiles.Add(GetTileByPixelPosition(spawn.Position));
            }

            SpawnMonsters();
            SpawnResources(skipTiles);
        }

        public void Rollover()
        {
            foreach(RHTile tile in TilledTiles)
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

        /// <summary>
        /// Call this to make the MonsterSpawns on the map spawn their monsters
        /// </summary>
        private void SpawnMonsters()
        {
            //Remove all mobs from the map
            foreach (Monster m in Monsters)
            {
                RemoveMonster(m);
            }
            Monsters.Clear();

            if (PrimedFood != null)
            {
                //Check to see if the spawn points have been set by MonsterFood and
                //resets any monsters that may already be set to them.
                foreach (MonsterSpawn spawn in _liMonsterSpawnPoints)
                {
                    if (spawn.IsPrimed)
                    {
                        spawn.Spawn();
                    }
                    else
                    {
                        spawn.ClearSpawn();
                    }
                }
                _liItemsToRemove.Add(PrimedFood);
                PrimedFood = null;
            }
            else
            {
                //Copy the spawn points to a list we can safely modify
                List<MonsterSpawn> spawnCopy = new List<MonsterSpawn>();
                spawnCopy.AddRange(_liMonsterSpawnPoints);

                //Trigger x number of SpawnPoints
                for (int i = 0; i < _iActiveSpawnPoints; i++)
                {
                    try
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
                    catch (Exception ex)
                    {

                    }
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

            //Copy the spawn points to a list we can safely modify
            List<MonsterSpawn> spawnCopy = new List<MonsterSpawn>();
            spawnCopy.AddRange(_liMonsterSpawnPoints);

            int spawnNum = PrimedFood.SpawnNumber;
            //For safety, but this looks like bad design
            if (spawnNum > spawnCopy.Count) { spawnNum = spawnCopy.Count; }

            //Trigger x number of SpawnPoints
            for (int i = 0; i < spawnNum; i++)
            {
                //Get a random Spawn Point
                int point = RHRandom.Instance.Next(0, spawnCopy.Count - 1);
                spawnCopy[point].SetSpawn(PrimedFood.SpawnID);

                //remove it from the copied list so it won't be an option for future spawning.
                spawnCopy.RemoveAt(point);
            }
        }

        /// <summary>
        /// Spawns resources from the ResourceSpawn points on the map
        /// </summary>
        private void SpawnResources(List<RHTile> skipTiles)
        {
            List<RHTile> validTiles = new List<RHTile>();
            List<RHTile> usedTiles = new List<RHTile>();
            foreach (RHTile x in _arrTiles)
            {
                if (!skipTiles.Contains(x) && x.Passable()) {
                    validTiles.Add(x);
                }
            }

            if (_diResources.Count > 0)
            {
                string[] val = _map.Properties["ResourcesMinMax"].Split('-');
                int spawnNumber = RHRandom.Instance.Next(int.Parse(val[0]), int.Parse(val[1]));

                for (int i = 0; i < spawnNumber; i++)
                {
                    //from the array as it gets filled so that we bounce less.
                    RHTile targetTile = validTiles[RHRandom.Instance.Next(0, validTiles.Count - 1)];

                    //If the object could not be placed, keep trying until you find one that can be
                    bool objectIsValid = true;
                    do
                    {
                        objectIsValid = true;
                        RarityEnum rarityKey = Util.RollAgainstRarity(_diResources);

                        WorldObject wObj = DataManager.GetWorldObject(_diResources[rarityKey][RHRandom.Instance.Next(0, _diResources[rarityKey].Count - 1)]);
                        wObj.SnapPositionToGrid(new Vector2(targetTile.Position.X, targetTile.Position.Y));

                        if (wObj.CompareType(ObjectTypeEnum.Plant))
                        {
                            ((Plant)wObj).FinishGrowth();
                        }

                        PlaceWorldObject(wObj, false);

                        //If the object is larger than one tile, we need to ensure it can actually fit ont he tile(s) we've placed it
                        if (wObj.CollisionBox.Width > TileSize || wObj.CollisionBox.Height > TileSize)
                        {
                            foreach (RHTile t in wObj.Tiles)
                            {
                                if (!validTiles.Contains(t) || usedTiles.Contains(t))
                                {
                                    objectIsValid = false;
                                    wObj.RemoveSelfFromTiles();
                                    RemoveWorldObject(wObj);
                                    break;
                                }
                            }
                        }
                    }
                    while (!objectIsValid);

                    //Remove the targetTile once it has been properly used
                    validTiles.Remove(targetTile);

                    //Keep track of which tiles were used
                    usedTiles.Add(targetTile);
                }
            }
        }

        public void CheckSpirits()
        {
            foreach (WorldActor c in _liActors)
            {
                if (c.IsActorType(ActorEnum.Spirit))
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
                if(a.IsActorType(ActorEnum.Spirit) && PlayerManager.PlayerInRange(a.Position.ToPoint(), 500))
                {
                    rv = (Spirit)a;
                }
            }

            return rv;
        }

        public bool ContainsActor(WorldActor c)
        {
            return _liActors.Contains(c) || (c.IsActorType(ActorEnum.Monster) && Monsters.Contains((Monster)c));
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

        #region Collision Code
        private bool TileContainsActor(RHTile t, bool checkPlayer = true)
        {
            bool rv = false;

            if (checkPlayer && this == PlayerManager.World.CurrentMap && PlayerManager.World.CollisionIntersects(t.Rect)) { rv = true; }
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
            if(dir.X > 0)
            {
                AddTile(ref list, rEndCollision.Right, rEndCollision.Top, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Bottom, actor.CurrentMapName);
            }
            else if(dir.X < 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Top, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Left, rEndCollision.Bottom, actor.CurrentMapName);
            }

            if (dir.Y > 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Bottom, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Bottom, actor.CurrentMapName);
            }
            else if (dir.Y < 0)
            {
                AddTile(ref list, rEndCollision.Left, rEndCollision.Top, actor.CurrentMapName);
                AddTile(ref list, rEndCollision.Right, rEndCollision.Top, actor.CurrentMapName);
            }

            //Because RHTiles do not contain WorldActors outside of combat, we need to add each
            //WorldActor's CollisionBox to the list, as long as the WorldActor in question is not the moving WorldActor.
            foreach(WorldActor w in _liActors)
            {
                if (w.Active && w != actor) { list.Add(w.CollisionBox);}
            }

            //If the actor is not the Player Character, add the Player Character's CollisionBox to the list as well
            if(actor != PlayerManager.World  && MapManager.CurrentMap == actor.CurrentMap) {
                list.Add(PlayerManager.World.CollisionBox);
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

            Rectangle box = c.CollisionBox;
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
            foreach(KeyValuePair<string, TravelPoint> kvp in DictionaryTravelPoints)
            {
                if (kvp.Value.Intersects(movingChar) && !kvp.Value.IsDoor)
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
            displayItem.Position = DictionaryCharacterLayer[npcIndex + "Col" + index];
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

            if (IsPaused()) { return false; }

            RHTile tile = MapManager.RetrieveTile(mouseLocation);

            //Do nothing if no tile could be retrieved
            if (tile == null) { return rv; }

            if (tile.GetTravelPoint() != null)
            {
                TravelPoint obj = tile.GetTravelPoint();

                if (PlayerManager.PlayerInRange(obj.CollisionBox) && !MapManager.ChangingMaps())
                {
                    if (obj.BuildingID > 1) {MapManager.EnterBuilding(obj, PlayerManager.Buildings.Find(x => x.PersonalID == obj.BuildingID)); }
                    else { MapManager.ChangeMaps(PlayerManager.World, this.Name, obj); }
                    SoundManager.PlayEffect("close_door_1");
                }
            }
            else if (tile.GetWorldObject() != null)
            {
                WorldObject obj = tile.GetWorldObject();
                if (obj.CompareType(ObjectTypeEnum.Machine))
                {
                    Machine p = (Machine)obj;
                    if (p.HasItem()) { p.TakeFinishedItem(); }
                    else
                    {
                        if (!p.MakingSomething())
                        {
                            p.StartAutoWork();
                        }
                        else if (p.IsCraftingMachine())
                        {
                            ((CraftingMachine)p).SetToWork();
                        }
                    }
                }
                else if (obj.CompareType(ObjectTypeEnum.Container))
                {
                    if (IsDungeon && DungeonManagerOld.IsEndChest((Container)obj))
                    {
                        //Staircase stairs = (Staircase)DataManager.GetWorldObject(3, Vector2.Zero);
                        //stairs.SetExit(MapManager.HomeMap);
                        //PlaceWorldObject(stairs, true);
                    }
                    GUIManager.OpenMainObject(new HUDInventoryDisplay(((Container)obj).Inventory));
                }
                else if (obj.CanPickUp() && PlayerManager.PlayerInRange(obj.CollisionBox))
                {
                    if (obj.CompareType(ObjectTypeEnum.Plant)){ ((Plant)obj).Harvest(); }
                    if(obj.CompareType(ObjectTypeEnum.Gatherable)) { ((Gatherable)obj).Gather(); }
                }
                else if (obj.CompareType(ObjectTypeEnum.DungeonObject))
                {
                    GameManager.CurrentTriggerObject = (TriggerObject)obj;
                    ((TriggerObject)obj).Interact();
                }
            }

            if (tile.ContainsProperty("Save", out string val) && val.Equals("true"))
            {
                GUIManager.OpenTextWindow(DataManager.GetGameText("Save"));
            }

            foreach (ShopData shop in _liShopData)
            {
                if (shop.Contains(mouseLocation) && shop.IsOpen())
                {
                    shop.Talk();
                    return true;
                }
            }

            foreach (WorldActor c in _liActors)
            {
                if (PlayerManager.PlayerInRange(c.HoverBox, (int)(TileSize * 1.5)) && c.HoverContains(mouseLocation) && c.CanTalk && c.Active)
                {
                    ((TalkingActor)c).Talk();
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
                GameManager.DropBuilding();
                LeaveBuildMode();
                Unpause();
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

            if (IsPaused()) { return false; }

            if (!PlayerManager.Busy && !CombatManager.InCombat)
            {
                if (Scrying())
                {
                    rv = ProcessLeftButtonClickHandleBuilding(mouseLocation);
                }
                else
                {
                    //Ensure that we have a tile that we clicked on and that the player is close enough to interact with it.
                    TargetTile = MapManager.RetrieveTile(mouseLocation);
                    if (TargetTile != null)
                    {

                        //Handles interactions with objects on the tile, both actual and Shadow
                        rv = ProcessLeftButtonClickOnObject(mouseLocation);

                        //Handles interactions with a tile that is actually empty, ignores Shadow Tiles
                        rv = ProcessLeftButtonClickOnEmptyTile(mouseLocation);

                        //Handles interacting with NPCs
                        foreach (WorldActor c in _liActors)
                        {
                            if (c.IsActorType(ActorEnum.Adventurer))
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
                            else if (c.IsActorType(ActorEnum.NPC) || c.IsActorType(ActorEnum.ShippingGremlin))
                            {
                                Villager n = (Villager)c;
                                if (n.CanGiveGift && InventoryManager.GetCurrentItem() != null &&
                                    n.CollisionContains(mouseLocation) && PlayerManager.PlayerInRange(n.CharCenter) &&
                                    InventoryManager.GetCurrentItem().Giftable())
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

        private bool UseTool(Point mouseLocation)
        {
            bool rv = false;

            if (InventoryManager.GetCurrentItem() != null && InventoryManager.GetCurrentItem().CompareType(ItemEnum.Tool))
            {
                Tool currentTool = (Tool)InventoryManager.GetCurrentItem();
                rv = PlayerManager.SetTool(currentTool, mouseLocation);
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
                    if (CurrentMerch != null)
                    {
                        PlayerManager.TakeMoney(CurrentMerch.MoneyCost);
                        foreach (KeyValuePair<int, int> kvp in CurrentMerch.RequiredItems)
                        {
                            InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                        }

                        CurrentMerch = null;
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
                if (GUICursor.WorkerToPlace > -1)
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

            if (PlayerManager.PlayerInRange(TargetTile.Center.ToPoint())){
                //Retrieves any object associated with the tile, this will include
                //both actual tiles, and Shadow Tiles because the user sees Shadow Tiles
                //as being on the tile.
                WorldObject obj = TargetTile.GetWorldObject();
                if (obj != null)
                {
                    if (obj.CompareType(ObjectTypeEnum.Machine))       //Player interacts with a machine to either take a finished item or start working
                    {
                        Machine p = (Machine)obj;
                        if (p.HasItem()) { p.TakeFinishedItem(); }
                        else
                        {
                            if (!p.MakingSomething())
                            {
                                p.StartAutoWork();
                            }
                            else if (p.IsCraftingMachine()) {
                                ((CraftingMachine)p).SetToWork();
                            }
                        }
                        rv = true;
                    }
                    else if (obj.CompareType(ObjectTypeEnum.ClassChanger))
                    {
                        ((ClassChanger)obj).ProcessClick();
                        rv = true;
                    }
                    else if (obj.CompareType(ObjectTypeEnum.Plant))
                    {
                        ((Plant)obj).Harvest();
                        rv = true;
                    }
                    else
                    {
                        rv = UseTool(mouseLocation);
                    }
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
            WorldObject obj = TargetTile.GetWorldObject(false);

            Item currentItem = InventoryManager.GetCurrentItem();
            if (obj == null)
            {
                if (currentItem != null && currentItem.CompareType(ItemEnum.StaticItem))    //Only procees if tile is empty and we are holding an item)
                {

                }
                else  if (!IsCombatMap && GameManager.ConstructionObject != null)
                {
                    Machine constructToBuild = GameManager.ConstructionObject;

                    //Check that all required items are there first
                    bool create = InventoryManager.SufficientItems(constructToBuild.RequiredToMake);
                    
                    //If all items are found, then remove them.
                    if (create)
                    {
                        foreach (KeyValuePair<int, int> kvp in constructToBuild.RequiredToMake)
                        {
                            InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                        }

                        //If the player is currently holding a StaticItem, we need to place it
                        //Do not, however, allow the placing of StaticItems on combat maps.
                        bool isFloor = constructToBuild.CompareType(ObjectTypeEnum.Floor);
                        if (!isFloor || (isFloor && TargetTile.Flooring == null))
                        {
                            WorldObject newObj = DataManager.GetWorldObject(constructToBuild.ID);
                            newObj.SetCoordinates(Util.SnapToGrid(constructToBuild.MapPosition));
                            if (MapManager.PlacePlayerObject(newObj))
                            {
                                rv = true;
                                //newItem.SetMapName(this.Name);      //Assign the map name tot he WorldItem

                                //If the item placed was a wall object, we need to adjust it based off any adjacent walls
                                if (newObj.CompareType(ObjectTypeEnum.Wall))
                                {
                                    ((Wall)newObj).AdjustObject();
                                }
                            }
                        }
                    }

                    if(!InventoryManager.SufficientItems(constructToBuild.RequiredToMake))
                    {
                        GameManager.ConstructionObject = null;
                    }
                }
                else if(PlayerManager.PlayerInRange(TargetTile.Center.ToPoint())){
                    rv = UseTool(mouseLocation);
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

                RHTile t = GetTileByPixelPosition(GUICursor.GetWorldMousePosition().ToPoint());
                if(t != null)
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
                    if(!c.IsActorType(ActorEnum.Monster) && c.HoverContains(mouseLocation)){
                        if (c.Active)
                        {
                            GUICursor.SetCursor(GUICursor.CursorTypeEnum.Talk, c.HoverBox);
                            found = true;
                            break;
                        }
                    }
                }

                //Do not draw test tiles on a map for combat
                WorldObject constructToBuild = GameManager.ConstructionObject;
                if (!IsCombatMap && constructToBuild != null)
                {
                    Vector2 vec = mouseLocation.ToVector2() - new Vector2(0, constructToBuild.Height - constructToBuild.BaseHeight);
                    constructToBuild.SetCoordinates(Util.SnapToGrid(vec));
                    TestMapTiles(constructToBuild, _liTestTiles);
                }

                if (!found)
                {
                    GUICursor.ResetCursor();
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
            _liObjectsToRemove.Add(o);

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

            o.RemoveSelfFromTiles();
        }
        public void RemoveCharacter(WorldActor c)
        {
            _liActorsToRemove.Add(c);
        }
        public void RemoveMonster(Monster m)
        {
            _liActorsToRemove.Add(m);
        }
        public void RemoveSummon(Summon s)
        {
            _liActorsToRemove.Add(s);
        }
        public void CleanupSummons()
        {
            foreach(Summon s in _liSummons)
            {
                s.KO();
            }
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
            if (flyingPop){ item.Pop(position); }
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
                            b.BuildingChest.SnapPositionToGrid(mapObject.Position);
                            PlacePlayerObject(b.BuildingChest);
                        }
                        else if (mapObject.Name.Contains("Pantry"))
                        {
                            b.Pantry.SnapPositionToGrid(mapObject.Position);
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
                        DictionaryTravelPoints.Remove(b.PersonalID.ToString());
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
                            DictionaryTravelPoints.Remove(b.PersonalID.ToString());
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
            DictionaryTravelPoints.Add(b.TravelLink(), buildPoint); //TODO: FIX THIS
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
                            Adventurer w = DataManager.GetAdventurer(GUICursor.WorkerToPlace);
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

        public bool PlaceWorldObject(WorldObject o, bool bounce = false, bool respectBarrens = false)
        {
            bool rv = false;

            List<RHTile> tiles = new List<RHTile>();
            if (RespectBarrens(respectBarrens, o.CollisionBox))
            {
                rv = TestMapTiles(o, tiles);
            }

            if (!rv && bounce)
            {
                Vector2 position = o.MapPosition;
                do
                {
                    position.X = (int)(RHRandom.Instance.Next(1, (MapWidthTiles - 1) * TileSize) / TileSize) * TileSize;
                    position.Y = (int)(RHRandom.Instance.Next(1, (MapHeightTiles - 1) * TileSize) / TileSize) * TileSize;
                    o.SnapPositionToGrid(position);


                    if (RespectBarrens(respectBarrens, o.CollisionBox))
                    {
                        rv = TestMapTiles(o, tiles);
                    }
                } while (!rv);
            }

            if (rv)
            {
                AssignMapTiles(o, tiles);
            }

            return rv;
        }

        private bool RespectBarrens(bool respect, Rectangle collisionBox)
        {
            bool rv = true;
            if (respect)
            {
                foreach (TiledMapObject mapObj in _liBarrenObjects)
                {
                    Rectangle r = Util.FloatRectangle(mapObj.Position.X, mapObj.Position.Y, mapObj.Size.Width, mapObj.Size.Height);
                    if (r.Intersects(collisionBox))
                    {
                        rv = false;
                        break;
                    }
                }
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

                    if (!TileContainsActor(tempTile, !o.CompareType(ObjectTypeEnum.Floor)) && ((!o.WallObject && tempTile.Passable() && tempTile.WorldObject == null) || (o.WallObject && tempTile.IsValidWall())))
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

            if (o.CompareType(ObjectTypeEnum.CombatHazard)){
                foreach (RHTile t in tiles)
                {
                    t.SetHazard((CombatHazard)o);
                }
            }
            else {
                //Sets the WorldObject to each RHTile
                foreach (RHTile t in tiles)
                {
                    t.SetObject(o);
                }
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
                        o.AddTile(t);
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
                        RHTile t = GetTileByPixelPosition(x, y);
                        if (t != null)
                        {
                            liTiles.Add(t);
                        }
                    }
                }
            }

            bool placeIt = true;
            foreach (RHTile t in liTiles)
            {
                if (!t.Passable() || TileContainsActor(t, !obj.CompareType(ObjectTypeEnum.Floor)))
                {
                    placeIt = false;
                    break;
                }
            }

            if (placeIt)
            {
                if (obj.CompareType(ObjectTypeEnum.Machine))
                {
                    GameManager.AddMachine((Machine)obj, this.Name);
                }
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
            if (MapManager.Maps[c.CurrentMapName].ContainsActor(c))
            {
                rv = true;
                if (c.IsActorType(ActorEnum.Monster) && Monsters.Contains((Monster)c)) { Monsters.Remove((Monster)c); }
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

                if (c.IsActorType(ActorEnum.Monster) && !Monsters.Contains((Monster)c)) { Monsters.Add((Monster)c); }
                else { Util.AddUniquelyToList(ref _liActors, c); }

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

            Monsters.Add(m);
        }

        public void AddSummon(Summon obj)
        {
            obj.CurrentMapName = _sName;
            _liSummons.Add(obj);
        }
        #endregion
        
        public int GetMapWidth()
        {
            return MapWidthTiles * ScaledTileSize;
        }

        public int GetMapHeight()
        {
            return MapHeightTiles * ScaledTileSize;
        }

        public List<RHTile> CheckForCombatHazards(CombatHazard.HazardTypeEnum e)
        {
            List<RHTile> liRv = new List<RHTile>();
            foreach(RHTile t in _arrTiles)
            {
                if(t.HazardObject != null && t.HazardObject.SubtypeMatch(e))
                {
                    liRv.Add(t);
                }
            }

            return liRv;
        }

        public void CheckForTriggeredCutScenes()
        {
            foreach(int cutsceneID in _liCutscenes)
            {
                CutsceneManager.CheckForTriggedCutscene(cutsceneID);
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
            catch (Exception)
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

        /// <summary>
        /// Returns a list of all RHTiles that exist
        /// </summary>
        /// <param name="obj">The Rectangle to check against</param>
        /// <returns>A list of all RHTiles that exist in the Rectangle</returns>
        public List<RHTile> GetTilesFromRectangle(Rectangle obj)
        {
            List<RHTile> rvList = new List<RHTile>();

            for (int y = obj.Top; y <= obj.Top + obj.Height; y += TileSize)
            {
                for (int x = obj.Left; x <= obj.Left + obj.Width; x += TileSize)
                {
                    RHTile tile = GetTileByPixelPosition(new Point(x, y));
                    if (!rvList.Contains(tile)) {
                        rvList.Add(tile);
                    }
                }
            }

            return rvList;
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
                    if (wObj.CompareType(ObjectTypeEnum.Machine))
                    {
                        //mapData.machines.Add(((Machine)wObj).SaveData());
                    }
                    else if (wObj.CompareType(ObjectTypeEnum.Container))
                    {
                        mapData.containers.Add(((Container)wObj).SaveData());
                    }
                    else if (wObj.CompareType(ObjectTypeEnum.Plant))
                    {
                        mapData.plants.Add(((Plant)wObj).SaveData());
                    }
                    else if (wObj.CompareType(ObjectTypeEnum.ClassChanger))
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
                        if (f.CompareType(ObjectTypeEnum.Earth))
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
                    WorldObject obj = DataManager.GetWorldObject(w.worldObjectID, new Vector2(w.x, w.y));
                    if (obj != null)
                    {
                        if (obj.CompareType(ObjectTypeEnum.Wall) || obj.CompareType(ObjectTypeEnum.Floor))
                        {
                            if (PlacePlayerObject(obj))
                            {
                                ((AdjustableObject)obj).SetMapName(this.Name);
                                ((AdjustableObject)obj).AdjustObject();
                            }
                        }

                        if (obj.CompareType(ObjectTypeEnum.ClassChanger))
                        {
                            PlacePlayerObject(obj);
                        }
                        else
                        {
                            PlaceWorldObject(obj);
                        }
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
                //theMachine.LoadData(mac);
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
                TilledTiles.Add(tile);
            }
        } 
    }

    public abstract class SpawnPoint
    {
        protected RHMap _map;
        protected Vector2 _vPosition;
        public Vector2 Position => _vPosition;

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
                string[] spawnResources = Util.FindParams(obj.Properties["Resources"]);
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
            wObj.SnapPositionToGrid(new Vector2(xPoint, yPoint));

            if (wObj.CompareType(ObjectTypeEnum.Plant))
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
        Dictionary<string, Dictionary<RarityEnum, List<int>>> _diMonsterSpawns;
        int _iPrimedMonsterID;
        public bool IsPrimed => _iPrimedMonsterID != -1;

        public MonsterSpawn(RHMap map, TiledMapObject obj) : base(map, obj)
        {
            _iPrimedMonsterID = -1;
            _diMonsterSpawns = new Dictionary<string, Dictionary<RarityEnum, List<int>>>();
            foreach(KeyValuePair<string, string> kvp in obj.Properties)
            {
                //If the property starts with Spawn, it defines what mobs spawn under what conditions
                //All, Weather, or Season are supported
                if (kvp.Key.StartsWith("Spawn-"))
                {
                    //Prune out the word 'Spawn' and keep the other word 
                    string[] split = kvp.Key.Split('-');
                    string spawnType = split[1];

                    string[] monsterParams = Util.FindParams(kvp.Value);
                    foreach(string s in monsterParams)
                    {
                        int monsterID = -1;
                        RarityEnum monsterRarity = RarityEnum.C;

                        Util.GetRarity(s, ref monsterID, ref monsterRarity);

                        //If we haven't added a new dictionary for the spawnType, add one.
                        if (!_diMonsterSpawns.ContainsKey(spawnType))
                        {
                            _diMonsterSpawns[spawnType] = new Dictionary<RarityEnum, List<int>>();
                        }

                        //If we haven't made a new list for the rarity yet, add one.
                        if (!_diMonsterSpawns[spawnType].ContainsKey(monsterRarity)) {
                            _diMonsterSpawns[spawnType][monsterRarity] = new List<int>();
                        }

                        //Ad the MonsterID to the SpawnType and Rarity dictionaries
                        _diMonsterSpawns[spawnType][monsterRarity].Add(monsterID);
                    }
                }
            }
        }

        /// <summary>
        /// Tells the Spawn point to spawn a monster by checking it's spawnType and rarity dictionary.
        /// If there is a primed monster, use that one instead.
        /// </summary>
        public override void Spawn()
        {
            if (_iPrimedMonsterID != -1) { _monster = DataManager.GetMonsterByIndex(_iPrimedMonsterID); }
            else
            {
                //Find which spawn type we're using
                string key = "All";
                if (!_diMonsterSpawns.ContainsKey("All"))
                {
                    if (_diMonsterSpawns.ContainsKey(GameCalendar.GetWeatherString())) { key = GameCalendar.GetWeatherString(); }
                    else { key = GameCalendar.GetSeason(); }
                }

                //Roll against rarity and backtrack until we find one of the rolled type that exists.
                RarityEnum rarityKey = Util.RollAgainstRarity(_diMonsterSpawns[key]);

                int spawnArrIndex = (int)RHRandom.Instance.Next(0, _diMonsterSpawns[key][rarityKey].Count - 1);
                _monster = DataManager.GetMonsterByIndex(_diMonsterSpawns[key][rarityKey][spawnArrIndex]);
            }

            _monster.SpawnPoint = this;
            _map.AddMonsterByPosition(_monster, _vPosition);

            if (_iPrimedMonsterID != -1)
            {
                _iPrimedMonsterID = -1;
            }
        }

        public override bool HasSpawned()
        {
            return _monster != null;
        }

        /// <summary>
        /// Sets what the next Monster to be spawned is
        /// </summary>
        /// <param name="id"></param>
        public void SetSpawn(int id)
        {
            _iPrimedMonsterID = id;
        }

        public void ClearSpawn()
        {
            _monster = null;
        }
    }

    public class RHTile
    {
        bool _tileExists;
        public string MapName { get; }
        public int X { get; }
        public int Y { get; }
        public Vector2 Position => new Vector2(X * TileSize, Y * TileSize);
        public Vector2 Center => new Vector2(Position.X + TileSize/2, Position.Y + TileSize/2);
        public Rectangle Rect => Util.FloatRectangle(Position, TileSize, TileSize);

        TravelPoint _travelPoint;
        public CombatActor Character { get; private set; }

        Dictionary<TiledMapTileLayer, Dictionary<string, string>> _diProps;

        public WorldObject WorldObject { get; private set; }
        public WorldObject ShadowObject { get; private set; }
        public CombatHazard HazardObject { get; private set; }
        public Floor Flooring { get; private set; }
        public bool IsRoad { get; private set; }

        bool _bArea = false;
        bool _bSelected = false;
        bool _bLegalTile = false;

        public RHTile(int x, int y, string mapName)
        {
            X = x;
            Y = y;

            MapName = mapName;
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

            if (Flooring != null) { Flooring.Draw(spriteBatch); }
            if (WorldObject != null) { WorldObject.Draw(spriteBatch); }
        }

        private bool DisplaySelectedTile()
        {
            return CombatPhaseCheck(CmbtPhaseEnum.ChooseActionTarget) || CombatPhaseCheck(CmbtPhaseEnum.ChooseMoveTarget) || CombatPhaseCheck(CmbtPhaseEnum.MainSelection);
        }

        public void Dig()
        {
            if (Flooring == null)
            {
                SetFloor(new Earth());
                if (GameCalendar.IsRaining())
                {
                    Water(true);
                }
            }
            else
            {
                Flooring = null;
            }
        }

        public bool SetFloor(Floor f)
        {
            bool rv = false;
            if (Flooring == null && Passable())
            {
                rv = true;
                Flooring = f;
                Flooring.SnapPositionToGrid(Position);
                Flooring.SetMapName(MapName);
                Flooring.Tiles.Clear();
                Flooring.AddTile(this);
                Flooring.AdjustObject();
            }

            return rv;
        }
        public void Water(bool value)
        {
            if (Flooring != null && Flooring.CompareType(ObjectTypeEnum.Earth))
            {
                Earth e = (Earth)Flooring;
                e.Watered(value);
            }
        }
        public bool IsWatered()
        {
            bool rv = false;
            if (Flooring != null && Flooring.CompareType(ObjectTypeEnum.Earth))
            {
                rv = ((Earth)Flooring).Watered();
            }

            return rv;
        }
        public bool HasBeenDug()
        {
            return Flooring != null && Flooring.CompareType(ObjectTypeEnum.Earth);
        }

        public void SetProperties(RHMap map)
        {
            foreach (TiledMapTileLayer l in map.Layers.Values)
            {
                if (l.TryGetTile(X, Y, out TiledMapTile? tile) && tile != null)
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
            IsRoad = ContainsProperty("Road", out string value) && value.Equals("true");
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

            if(WorldObject != null) { obj = WorldObject; }
            else if(AlsoCheckShadow) { obj = ShadowObject; }

            return obj;
        }

        public void RemoveWorldObject()
        {
            WorldObject = null;
        }
        public void RemoveShadowObject()
        {
            ShadowObject = null;
        }
        public void RemoveFlooring()
        {
            Flooring = null;
        }
        public bool SetObject(WorldObject o)
        {
            bool rv = false;
            if (o.CompareType(ObjectTypeEnum.Floor))
            {
                rv = SetFloor((Floor)o);
            }
            else if ((!o.WallObject && Passable()) || (o.WallObject && IsValidWall()))
            {
                WorldObject = o;
                rv = true;
            }
            return rv;
        }
        public bool SetShadowObject(WorldObject o)
        {
            bool rv = false;
            if ((!o.WallObject && Passable()) || (o.WallObject && IsValidWall()))
            {
                ShadowObject = o;
                rv = true;
            }
            return rv;
        }
        public Floor GetFloorObject()
        {
            Floor f = null;

            if (Flooring != null) { f = Flooring; }

            return f;
        }

        /// <summary>
        /// Sets the Hazard object for the RHTile
        /// </summary>
        /// <param name="h">The Hazard object to set.</param>
        public void SetHazard(CombatHazard h)
        {
            HazardObject = h;
        }

        /// <summary>
        /// Determines whether or not the tile is a valid target for being dug.
        /// 
        /// Currently can be dug if the property is set, and there are no objects sitting on it.
        /// </summary>
        /// <returns></returns>
        public bool CanDig()
        {
            bool rv = false;
            foreach (TiledMapTileLayer l in _diProps.Keys)
            {
                if (l.IsVisible && ContainsProperty(l, "CanDig", out string val) && val.Equals("true") && WorldObject == null && Flooring == null)
                {
                    rv = true;
                }
            }

            return rv;
        }

        public bool IsValidWall()
        {
            bool rv = false;
            if (_tileExists && WorldObject == null)
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

        public void SetMapObject(TravelPoint obj)
        {
            _travelPoint = obj;
        }

        public TravelPoint GetTravelPoint()
        {
            return _travelPoint;
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

        /// <summary>
        /// Attempt to damage the object assuming it is destructible and can be affected by the used tool
        /// </summary>
        /// <param name="toolUsed">The tool being used</param>
        /// <returns>True if the tool was able to deal damage</returns>
        public bool DamageObject(Tool toolUsed)
        {
            bool rv = false;
            if (WorldObject != null && WorldObject.CompareType(ObjectTypeEnum.Destructible))
            {
                if (((Destructible)WorldObject).WhichTool == toolUsed.ToolType){
                    SoundManager.PlayEffectAtLoc(toolUsed.SoundEffect, MapName, Center, toolUsed);
                    rv = ((Destructible)WorldObject).DealDamage(toolUsed.Power);
                    if (rv)
                    {
                        MapManager.DropItemsOnMap(WorldObject.GetDroppedItems(), WorldObject.CollisionBox.Location.ToVector2());
                    }
                }
            }

            return rv;
        }

        public void Clear()
        {
            WorldObject = null;
            Flooring = null;
        }

        public void Rollover()
        {
            if (WorldObject != null && WorldObject.CompareType(ObjectTypeEnum.Plant))
            {
                ((Plant)WorldObject).Rollover();
            }
            if (Flooring != null && Flooring.CompareType(ObjectTypeEnum.Earth))
            {
                ((Earth)Flooring).Watered(false);
            }
        }

        /// <summary>
        /// Returns if the tile has a CombatActor assigned to it. 
        /// </summary>
        public bool HasCombatant()
        {
            return Character != null;
        }

        /// <summary>
        /// Assigns a CombatActor to the RHTile
        /// </summary>
        /// <param name="c">The combatant to set to this tile</param>
        public void SetCombatant(CombatActor c)
        {
            Character = c;
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
            bool rv = _tileExists && (WorldObject == null || !WorldObject.Blocking);
            if (_tileExists)
            {
                foreach (TiledMapTileLayer l in _diProps.Keys)
                {
                    if (l.IsVisible && !l.Name.Contains("Upper") && ContainsProperty(l, "Impassable", out string val) && val.Equals("true"))
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
            //&& GetTravelPoint() == null
            return Passable()  && WorldObject == null;
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
                rv = Character == null || Character.IsSummon() || CombatManager.OnSameTeam(Character);
            }
            return rv;
        }
        #endregion
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
            ((ShopKeeper)DataManager.DiNPC[_iShopID]).SetOpen(true);
            ((ShopKeeper)DataManager.DiNPC[_iShopID]).Talk();
            ((ShopKeeper)DataManager.DiNPC[_iShopID]).SetOpen(false);
        }
    }

    public class TravelPoint
    {
        public int BuildingID { get; private set; } = -1;
        public Rectangle CollisionBox { get; private set; }
        public Point Location => CollisionBox.Location;
        string _sMapName;
        public string LinkedMap { get; private set; }
        public Vector2 Center => CollisionBox.Center.ToVector2();
        public bool IsDoor { get; private set; }

        DirectionEnum _eEntranceDir;

        public TravelPoint(TiledMapObject obj, string mapName)
        {
            _sMapName = mapName;
            CollisionBox = Util.FloatRectangle(obj.Position, obj.Size.Width, obj.Size.Height);
            if (obj.Properties.ContainsKey("Map"))
            {
                LinkedMap = obj.Properties["Map"] == "Home" ? MapManager.HomeMap : obj.Properties["Map"];
            }
            if (obj.Properties.ContainsKey("EntranceDir"))
            {
                _eEntranceDir = Util.ParseEnum<DirectionEnum>(obj.Properties["EntranceDir"]);
            }
        }
        public TravelPoint(Rectangle collision, string linkedMap, int buildingID)
        {
            CollisionBox = collision;
            LinkedMap = linkedMap;
            BuildingID = buildingID;
            _eEntranceDir = DirectionEnum.Down;
            IsDoor = true;
        }

        public bool Intersects(Rectangle value)
        {
            return CollisionBox.Intersects(value);
        }

        /// <summary>
        /// USe to determine the exit point of the TravelObject based on the distance of the Actor to the
        /// linked TravelObject they interacted with
        /// </summary>
        /// <param name="oldPointCenter">The center of the previous TravelPoint</param>
        /// <param name="c">The moving Actor</param>
        /// <returns></returns>
        public Vector2 FindLinkedPointPosition(Vector2 oldPointCenter, WorldActor c)
        {
            //Find the difference between the position of the center of the actor's collisionBox
            //and the TravelPoint that the actor interacted with.
            Point actorCollisionCenter = c.CollisionBox.Center;
            Vector2 vDiff = actorCollisionCenter.ToVector2() - oldPointCenter;

            //If we move Left/Right, ignore the X axis, Up/Down, ignore the Y axis then just set
            //the difference in the relevant axis to the difference between the centers of those two boxes
            switch (_eEntranceDir)
            {
                case DirectionEnum.Left:
                    vDiff.X = -1 * (CollisionBox.Width / 2 + c.CollisionBox.Width/2);
                    break;
                case DirectionEnum.Right:
                    vDiff.X = (CollisionBox.Width / 2 + c.CollisionBox.Width/2);
                    break;
                case DirectionEnum.Up:
                    vDiff.Y = -1 * (CollisionBox.Height/2 + c.CollisionBox.Height/2);
                    break;
                case DirectionEnum.Down:
                    vDiff.Y = (CollisionBox.Height / 2 + c.CollisionBox.Height/2);
                    break;
            }

            //Add the diff to the center of the current TravelPoint
            Vector2 rv = new Vector2(Center.X + vDiff.X, Center.Y + vDiff.Y);

            //Get the difference between the Position of the character and the center of their collision box
            rv += c.Position - actorCollisionCenter.ToVector2();

            return rv;
        }

        public void SetDoor()
        {
            IsDoor = true;
        }

        /// <summary>
        /// Finds the center point ofthe TravelPoint and returns the RHTile the center point
        /// resides on.
        /// 
        /// This method is primarily/mostly used for NPC pathfinding to TravelPoints
        /// </summary>
        /// <returns></returns>
        public Vector2 GetCenterTilePosition()
        {
            return CollisionBox.Center.ToVector2();
        }

        public Vector2 GetMovedCenter()
        {
            RHTile rv = MapManager.Maps[_sMapName].GetTileByPixelPosition(GetCenterTilePosition());
            return rv.GetTileByDirection(_eEntranceDir).Position;
        }
    }
}
