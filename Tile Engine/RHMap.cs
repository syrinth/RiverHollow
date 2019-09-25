using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using RiverHollow.Actors;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.GUIObjects.HUDMenu;
using static RiverHollow.RiverHollow;
using static RiverHollow.WorldObjects.Door;
using static RiverHollow.WorldObjects.WorldItem;
using static RiverHollow.WorldObjects.WorldItem.Floor;

namespace RiverHollow.Tile_Engine
{
    public class RHMap
    {
        public int MapWidthTiles = 100;
        public int MapHeightTiles = 100;
        
        private string _name;
        public string Name { get => _name.Replace(@"Maps\", ""); set => _name = value; } //Fuck off with that path bullshit

        bool _bBuilding;
        public bool IsBuilding => _bBuilding;
        bool _bDungeon;
        public bool IsDungeon => _bDungeon;
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
        protected List<Door> _liDoors;
        public List<WorldActor> ToRemove;
        public List<WorldActor> ToAdd;
        protected List<Building> _liBuildings;
        protected List<RHTile> _liTilledTiles;
        protected List<WorldObject> _liPlacedWorldObjects;
        public List<RHTile> TilledTiles => _liTilledTiles;
        protected List<SpawnPoint> _liMonsterSpawnPoints;
        protected List<int> _liRandomSpawnItems;
        protected List<int> _liCutscenes;

        protected List<Item> _liItems;
        protected List<ShopData> _liShopData;

        private Dictionary<Rectangle, string> _dictExit;
        public Dictionary<Rectangle, string> DictionaryExit { get => _dictExit; }
        private Dictionary<string, Rectangle> _dictEntrance;
        public Dictionary<string, Rectangle> DictionaryEntrance { get => _dictEntrance; }
        private Dictionary<string, Vector2> _dictCharacterLayer;
        public Dictionary<string, Vector2> DictionaryCharacterLayer { get => _dictCharacterLayer; }
        private List<TiledMapObject> _liMapObjects;

        //The dictionary of all resource spawn points on the map, by the resource they spawn.
        private Dictionary<int, List<TiledMapObject>> _diResourceSpawns;

        public RHMap() {
            _liMonsterSpawnPoints = new List<SpawnPoint>();
            _liTestTiles = new List<RHTile>();
            _liTilesets = new List<TiledMapTileset>();
            _liActors = new List<WorldActor>();
            _liMonsters = new List<Monster>();
            _liBuildings = new List<Building>();
            _liTilledTiles = new List<RHTile>();
            _liItems = new List<Item>();
            _liMapObjects = new List<TiledMapObject>();
            _dictExit = new Dictionary<Rectangle, string>();
            _dictEntrance = new Dictionary<string, Rectangle>();
            _dictCharacterLayer = new Dictionary<string, Vector2>();
            _liShopData = new List<ShopData>();
            _liDoors = new List<Door>();
            _liPlacedWorldObjects = new List<WorldObject>();
            _liRandomSpawnItems = new List<int>();
            _liCutscenes = new List<int>();

            _diResourceSpawns = new Dictionary<int, List<TiledMapObject>>();

            ToRemove = new List<WorldActor>();
            ToAdd = new List<WorldActor>();
        }

        public RHMap(RHMap map) : this()
        {
            _map = map.Map;
            _name = map.Name+"Clone";
            _renderer = map._renderer;
            _arrTiles = map._arrTiles;

            _bBuilding = _map.Properties.ContainsKey("Building");
            _bDungeon = _map.Properties.ContainsKey("Dungeon");
            _bTown = _map.Properties.ContainsKey("Town");
            _bOutside = _map.Properties.ContainsKey("Outside");
            _bManor = _map.Properties.ContainsKey("Manor");

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
            _name = mapName;
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
                    _arrTiles[j, i] = new RHTile(j, i, _name);
                    _arrTiles[j, i].SetProperties(this);
                }
            }

            _bBuilding = _map.Properties.ContainsKey("Building");
            _bDungeon = _map.Properties.ContainsKey("Dungeon");
            _bTown = _map.Properties.ContainsKey("Town");
            _bOutside = _map.Properties.ContainsKey("Outside");
            _bManor = _map.Properties.ContainsKey("Manor");

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

            LoadMapObjects();
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
                        string ID = (mapObject.Properties.ContainsKey("ID")) ? (":" + mapObject.Properties["ID"]) : "";
                        Rectangle r = new Rectangle((int)mapObject.Position.X, (int)mapObject.Position.Y, (int)mapObject.Size.Width, (int)mapObject.Size.Height);

                        string map = string.Empty;
                        if (mapObject.Properties.ContainsKey("Exit"))
                        {
                            map = mapObject.Properties["Exit"] == "Home" ? MapManager.HomeMap : mapObject.Properties["Exit"] + ID;
                            _dictExit.Add(r, map);

                            for (float x = mapObject.Position.X; x < mapObject.Position.X + mapObject.Size.Width; x += TileSize)
                            {
                                for (float y = mapObject.Position.Y; y < mapObject.Position.Y + mapObject.Size.Height; y += TileSize)
                                {
                                    RHTile t = GetTileOffGrid((int)x, (int)y);
                                    if (t != null && mapObject.Properties.ContainsKey("Door"))
                                    {
                                        t.SetMapObject(Util.FloatRectangle(mapObject.Position.X, mapObject.Position.Y, mapObject.Size.Width, mapObject.Size.Height));
                                    }
                                }
                            }
                        }
                        else if (mapObject.Properties.ContainsKey("Entrance"))
                        {
                            map = mapObject.Properties["Entrance"] == "Home" ? MapManager.HomeMap : mapObject.Properties["Entrance"] + ID;
                            _dictEntrance.Add(map, r);
                        }
                    }
                }
                else if (ol.Name.Contains("Character"))
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        if (mapObject.Name.Equals("Shop"))
                        {
                            _liShopData.Add(new ShopData(_name, mapObject));
                        }
                        else
                        {
                            _dictCharacterLayer.Add(mapObject.Name, mapObject.Position);
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
                else if (ol.Name.Contains("Spawn"))
                {
                    //Sets up the Dictionaries for the resource spawn points
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        if (mapObject.Properties.ContainsKey("Resources"))
                        {
                            string[] spawnResources = mapObject.Properties["Resources"].Split('-');
                            foreach (string s in spawnResources)
                            {
                                int iSpawnResource = int.Parse(s);
                                if (!_diResourceSpawns.ContainsKey(iSpawnResource))
                                {
                                    _diResourceSpawns[iSpawnResource] = new List<TiledMapObject>();
                                }
                                _diResourceSpawns[iSpawnResource].Add(mapObject);
                            }
                        }
                    }
                }
            }
        }

        public void PopulateMap(bool loaded = false)
        {
            TiledMapProperties props = _map.Properties;
            List<int> _liMobs = new List<int>();
            int minMobs = 0;
            int maxMobs = 0;
            List<int> resources = new List<int>();
            int _iMinResources = 0;
            int _iMaxResources = 0;

            foreach (TiledMapObject obj in _liMapObjects)
            {
                if (obj.Name.Contains("Door"))
                {
                    Door d = null;
                    if (obj.Name.Equals("MobDoor")) { d = ObjectManager.GetDoor(obj.Name, obj.Position); }
                    else if (obj.Name.Equals("SeasonDoor"))
                    {
                        d = ObjectManager.GetDoor(obj.Name, obj.Position);
                        ((SeasonDoor)d).SetSeason(obj.Properties["Season"]);
                    }
                    else if (obj.Name.Equals("KeyDoor"))
                    {
                        d = ObjectManager.GetDoor(obj.Name, obj.Position);
                        ((KeyDoor)d).SetKey(int.Parse(obj.Properties["Open"]));
                    }

                    _liDoors.Add(d);
                    PlaceWorldObject(d);
                }
                else if (obj.Name.Equals("Rock"))
                {
                    PlaceWorldObject(ObjectManager.GetWorldObject(WorldItem.Rock, Util.SnapToGrid(obj.Position)));
                }
                else if (obj.Name.Equals("Tree"))
                {
                    PlaceWorldObject(ObjectManager.GetWorldObject(WorldItem.Tree, Util.SnapToGrid(obj.Position)));
                }
                else if (obj.Name.Equals("Monster"))
                {
                    Vector2 vect = obj.Position;
                    Monster mob = ObjectManager.GetMonster(int.Parse(obj.Properties["ID"]), vect);
                    AddMonster(mob);
                }
                else if (obj.Name.Equals("Chest"))
                {
                    Container c = (Container)ObjectManager.GetWorldObject(190);
                    InventoryManager.InitContainerInventory(c);
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
                        CurrentMapName = _name
                    };
                    _liActors.Add(s);
                }
                else if (obj.Name.Equals("SpawnPoint"))
                {
                    _liMonsterSpawnPoints.Add(new SpawnPoint(this, obj));
                }
                else if (obj.Name.Equals("Manor") && !loaded)
                {
                    Building manor = ObjectManager.GetManor();
                    manor.SetCoordinatesByGrid(obj.Position);
                    manor.SetName(PlayerManager.ManorName);
                    AddBuilding(manor, true);
                }
                else if (obj.Properties.ContainsKey("Item"))
                {
                    Item item = ObjectManager.GetItem(int.Parse(obj.Properties["Item"]));
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

            string[] split;
            foreach (KeyValuePair<string, string> prop in props)
            {
                if (prop.Key.Equals("Monsters"))
                {
                    split = prop.Value.Split('/');
                    foreach (string s in split)
                    {
                        _liMobs.Add(int.Parse(s));
                    }
                }
                else if (prop.Key.Equals("MonstersMax")) { maxMobs = int.Parse(prop.Value); }
                else if (prop.Key.Equals("MonstersMin")) { minMobs = int.Parse(prop.Value); }
                if (prop.Key.Equals("Resources"))
                {
                    split = prop.Value.Split('/');
                    foreach (string s in split)
                    {
                        resources.Add(int.Parse(s));
                    }
                }
                else if (prop.Key.Equals("ResourcesMax")) { _iMaxResources = int.Parse(prop.Value); }
                else if (prop.Key.Equals("ResourcesMin")) { _iMinResources = int.Parse(prop.Value); }
            }

            if(resources.Count > 0)
            {
                RHRandom r = new RHRandom();
                int numResources = r.Next(_iMinResources, _iMaxResources);
                while(numResources != 0)
                {
                    int chosenResource = r.Next(0, resources.Count - 1);

                    PlaceWorldObject(ObjectManager.GetWorldObject(resources[chosenResource], new Vector2(r.Next(1, _map.Width - 1) * TileSize, r.Next(1, _map.Height - 1) * TileSize)), true);

                    numResources--;
                }
            }

            if(_liRandomSpawnItems.Count > 0)
            {
                RHRandom r = new RHRandom();
                for (int i = 0; i < 30; i++)
                {
                    Plant obj = (Plant)ObjectManager.GetWorldObject(_liRandomSpawnItems[0], new Vector2(r.Next(1, _map.Width - 1) * TileSize, r.Next(1, _map.Height - 1) * TileSize));
                    obj.FinishGrowth();
                    PlaceWorldObject(obj, true);
                }
            }

        
            if (_liMobs.Count > 0)
            {
                RHRandom r = new RHRandom();
                int numMobs = r.Next(minMobs, maxMobs);
                while (numMobs != 0)
                {
                    int chosenMob = r.Next(0, _liMobs.Count - 1);

                    Vector2 vect = new Vector2(r.Next(1, _map.Width - 1) * TileSize, r.Next(1, _map.Height - 2) * TileSize);
                    Monster newMonster = ObjectManager.GetMonster(_liMobs[chosenMob], vect);
                    newMonster.CurrentMapName = _name;
                    AddMonster(newMonster);

                    numMobs--;
                }
            }

            SpawnMobs();

            //Spawns a random assortment of resources them ap will allow wherever they're allowed
            if (_diResourceSpawns.Count > 0)
            {
                RHRandom r = new RHRandom();
                List<int> whatResources = new List<int>(_diResourceSpawns.Keys);

                int spawnNumber = r.Next(_iMinResources, _iMaxResources);
                for (int i = 0; i < spawnNumber; i++)
                {
                    int whichResource = whatResources[r.Next(0, whatResources.Count - 1)];
                    List<TiledMapObject> spawnPoints = _diResourceSpawns[whichResource];

                    int whichPoint = r.Next(0, spawnPoints.Count - 1);
                    if (spawnPoints.Count > 0)
                    {
                        TiledMapObject mapObj = spawnPoints[whichPoint];
                        int xPoint = r.Next((int)(mapObj.Position.X), (int)(mapObj.Position.X + mapObj.Size.Width));
                        int yPoint = r.Next((int)(mapObj.Position.Y), (int)(mapObj.Position.Y + mapObj.Size.Height));

                        WorldObject wObj = ObjectManager.GetWorldObject(whichResource);
                        wObj.SetCoordinatesByGrid(new Vector2(xPoint, yPoint));

                        if (wObj.IsPlant())
                        {
                            ((Plant)wObj).FinishGrowth();
                        }
                        PlaceWorldObject(wObj, false);
                    }
                }
            }
        }

        public void Rollover()
        {
            foreach(RHTile tile in _liTilledTiles)
            {
                tile.Rollover();
            }

            SpawnMobs();

            CheckSpirits();
            _liItems.Clear();
        }

        private void SpawnMobs()
        {
            foreach (SpawnPoint sp in _liMonsterSpawnPoints)
            {
                sp.Despawn();
            }

            RHRandom rand = new RHRandom();
            for (int i = 0; i < _iActiveSpawnPoints; i++)
            {
                bool spawned = false;
                do
                {
                    int point = rand.Next(0, _liMonsterSpawnPoints.Count-1);
                    if (!_liMonsterSpawnPoints[point].HasSpawned())
                    {
                        spawned = true;
                        _liMonsterSpawnPoints[point].Spawn();
                    }

                } while (!spawned);
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
            List<Item> removedList = new List<Item>();
            for(int i = 0; i < _liItems.Count; i++)
            {
                int row = 0;
                int col = 0;
                Item it = _liItems[i];
                if (InventoryManager.HasSpaceInInventory(it.ItemID, it.Number, ref row, ref col, true))
                {
                    if (it.OnTheMap && it.AutoPickup)
                    {
                        if (it.FinishedMoving() && it.CollisionBox.Intersects(player.CollisionBox))
                        {
                            removedList.Add(it);
                            InventoryManager.AddItemToInventorySpot(ObjectManager.GetItem(it.ItemID, it.Number), row, col);
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

            foreach (Item i in removedList)
            {
                _liItems.Remove(i);
            }
            removedList.Clear();
        }

        public void Update(GameTime gTime)
        {
            if (this == MapManager.CurrentMap)
            {
                _renderer.Update(_map, gTime);
                if (IsRunning())
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
        }

        public void DrawBase(SpriteBatch spriteBatch)
        {
            SetLayerVisibility(false);

            _renderer.Draw(_map, Camera._transform);

            if (CombatManager.InCombat)
            {
                foreach (RHTile t in CombatManager.LegalTiles)
                {
                    t.Draw(spriteBatch);
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
                spriteBatch.Draw(GameContentManager.GetTexture(@"Textures\Dialog"), new Rectangle((int)t.Position.X, (int)t.Position.Y, TileSize, TileSize), new Rectangle(288, 128, TileSize, TileSize) , passable ? Color.Green *0.5f : Color.Red * 0.5f, 0, Vector2.Zero, SpriteEffects.None, 99999);
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
        private List<Rectangle> GetPossibleCollisions(WorldActor actor, Vector2 dir)
        {
            List<Rectangle> list = new List<Rectangle>();
            Rectangle newRectangle = new Rectangle((int)(actor.CollisionBox.X + dir.X), (int)(actor.CollisionBox.Y + dir.Y), actor.CollisionBox.Width, actor.CollisionBox.Height);

            if(dir.X > 0)
            {
                AddTile(ref list, newRectangle.Right, newRectangle.Top);
                AddTile(ref list, newRectangle.Right, newRectangle.Bottom);
            }
            else if(dir.X < 0)
            {
                AddTile(ref list, newRectangle.Left, newRectangle.Top);
                AddTile(ref list, newRectangle.Left, newRectangle.Bottom);
            }

            if (dir.Y > 0)
            {
                AddTile(ref list, newRectangle.Left, newRectangle.Bottom);
                AddTile(ref list, newRectangle.Right, newRectangle.Bottom);
            }
            else if (dir.Y < 0)
            {
                AddTile(ref list, newRectangle.Left, newRectangle.Top);
                AddTile(ref list, newRectangle.Right, newRectangle.Top);
            }

            foreach(WorldActor w in _liActors)
            {
                if (w.Active && w != actor) { list.Add(w.CollisionBox);}
            }

            if(actor != PlayerManager.World && !actor.IsMonster()) {
                list.Add(PlayerManager.World.CollisionBox);
            }

            return list;
        }
        private void AddTile(ref List<Rectangle> list, int one, int two)
        {
            RHTile tile = MapManager.CurrentMap.GetTileByGrid(Util.GetGridCoords(one, two));
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
            RHTile firstTile = MapManager.Maps[map].GetTileByGrid(Util.GetGridCoords(first));
            RHTile secondTile = MapManager.Maps[map].GetTileByGrid(Util.GetGridCoords(second));
            if (firstTile != null && firstTile.Passable() && secondTile != null && secondTile.Passable())
            {
                rv = modifier;
            }

            return rv;
        } 

        public bool CheckForCollisions(WorldActor c, Rectangle testX, Rectangle testY, ref Vector2 dir, bool ignoreCollisions = false)
        {
            bool rv = true;

            if (MapChange(c, testX) || MapChange(c, testY)) { return false; }
            else if (!ignoreCollisions)
            {
                List<Rectangle> list = GetPossibleCollisions(c, dir);
                ChangeDir(list, c.CollisionBox, ref dir, c.CurrentMapName);
            }

            return rv;
        }

        public bool MapChange(WorldActor c, Rectangle movingChar)
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
                RHTile testTile = GetTileByGrid((int)(varCol + (v.Equals("Col") ? 1 : 0)), (int)(varRow + (v.Equals("Row") ? 1 : 0)));
                if (testTile != null && testTile.Passable()) {
                    rv = 1;
                }
            }
            else if (centerDelta < 0)
            {
                RHTile testTile = GetTileByGrid((int)(varCol - (v.Equals("Col") ? 1 : 0)), (int)(varRow + (v.Equals("Row") ? 1 : 0)));
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
            Item displayItem = ObjectManager.GetItem(itemID);
            displayItem.AutoPickup = false;
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
            Vector2 rv = Vector2.Zero;
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

            RHTile tile = MapManager.RetrieveTile(mouseLocation);

            //Do nothing if no tile could be retrieved
            if (tile == null) { return rv; }

            if(tile.GetDoorObject() != null) {
                RHTile.TileObject obj = tile.GetDoorObject();

                if (PlayerManager.PlayerInRange(obj.Rect))
                {
                    if (obj.PlayerBuilding != null) { MapManager.EnterBuilding(obj.PlayerBuilding); }
                    else { MapManager.ChangeMaps(PlayerManager.World, this.Name, _dictExit[obj.Rect]); }
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
                    if (IsDungeon && DungeonManager.IsEndChest((Container)obj))
                    {
                        Staircase stairs = (Staircase)ObjectManager.GetWorldObject(3, Vector2.Zero);
                        stairs.SetExit(MapManager.HomeMap);
                        PlaceWorldObject(stairs, true);
                    }
                    GUIManager.OpenMainObject(new HUDInventoryDisplay((Container)obj));
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
                else if (obj.IsDoor())
                {
                    ((Door)obj).ReadInscription();
                }
                else if (obj.ID == 3) //Checks to see if the tile contains a staircase object
                {
                    MapManager.ChangeMaps(PlayerManager.World, this.Name, ((Staircase)obj).ToMap);
                }
            }

            if (tile.ContainsProperty("Journal", out string val) && val.Equals("true"))
            {
                GUIManager.OpenTextWindow(GameContentManager.GetGameText("Journal"));
                //GUIManager.SetScreen(new TextScreen(GameContentManager.GetGameText("Journal"), true));
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
                    if (PlayerManager.PlayerInRange(c.CollisionBox.Center, (int)(TileSize * 1.5)) && c.CollisionContains(mouseLocation) && c.CanTalk && c.Active)
                    {
                        ((TalkingActor)c).Talk(true);
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
                    if (it.ManualPickup && it.CollisionBox.Contains(GraphicCursor.GetTranslatedMouseLocation()))
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
                                    InventoryManager.AddItemToInventorySpot(ObjectManager.GetItem(w.TakeItem()), row, col);
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
                //Player tries to add something to the merchant chest to sell it.
                if (obj == PlayerManager._merchantChest)
                {
                    Item i = InventoryManager.GetCurrentItem();
                    PlayerManager._merchantChest.AddItem(i);
                    InventoryManager.RemoveItemFromInventory(InventoryManager.GetCurrentItem());
                }
                else if (obj.IsMachine())       //Player interacts with a machine to either take a finished item or start working
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
                    InventoryManager.AddToInventory(ObjectManager.GetItem(((Forageable)obj).ForageItem));
                    MapManager.RemoveWorldObject(obj);
                    obj.RemoveSelfFromTiles();
                }
                else if (obj.IsDestructible())  //Handle damaging destructibles
                {
                    Destructible d = (Destructible)_targetTile.WorldObject;

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
                StaticItem selectedItem = InventoryManager.GetCurrentStaticItem();
                if (selectedItem != null)
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

                RHTile t = GetTileOffGrid(GraphicCursor.GetTranslatedMouseLocation().ToPoint());
                if(t != null && t.GetDoorObject() != null)
                {
                    found = true;
                    GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Door;
                    GraphicCursor.Alpha = (PlayerManager.PlayerInRange(t.GetDoorObject().Rect) ? 1 : 0.5f);
                }

                foreach (WorldActor c in _liActors)
                {
                    if(!c.IsMonster() && c.CollisionContains(mouseLocation)){
                        if (c.Active)
                        {
                            GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Talk;
                            found = true;
                            break;
                        }
                    }
                }
                StaticItem selectedStaticItem = InventoryManager.GetCurrentStaticItem();
                if (selectedStaticItem != null)
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
            _liMonsters.Remove(m);
            foreach (Door d in _liDoors)
            {
                if (d.IsMobDoor()) { ((MobDoor)d).Check(_liMonsters.Count); }
            }
        }
        public void DropItemsOnMap(List<Item>items, Vector2 position)
        {
            foreach(Item i in items)
            {
                DropItemOnMap(i, position);
            }
        }

        public void DropItemOnMap(Item item, Vector2 position)
        {
            ((Item)item).Pop(position);
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
                        _dictEntrance.Remove(b.PersonalID.ToString());
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
                            _dictEntrance.Remove(b.PersonalID.ToString());
                            PlayerManager.RemoveBuilding(b);
                            LeaveBuildMode();
                            Unpause();
                            Scry(false);
                            ResetCamera();
                        }
                        else
                        {
                            GUIManager.OpenTextWindow(GameContentManager.GetGameText("Cannot Destroy occupied buildings."));
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
            _dictEntrance.Add(b.PersonalID.ToString(), b.BoxToExit); //TODO: FIX THIS
            for (float x = b.BoxToEnter.X; x < b.BoxToEnter.X + b.BoxToEnter.Width; x += TileSize)
            {
                for (float y = b.BoxToEnter.Y; y < b.BoxToEnter.Y + b.BoxToEnter.Height; y += TileSize)
                {
                    RHTile t = GetTileOffGrid((int)x, (int)y);
                    t.SetMapObject(b);
                }
            }
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
                            Adventurer w = ObjectManager.GetWorker(GraphicCursor.WorkerToPlace);
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
                RHRandom r = new RHRandom();
                Vector2 position = o.MapPosition;
                do
                {
                    position.X = (int)(r.Next(1, (MapWidthTiles - 1) * TileSize) / TileSize) * TileSize;
                    position.Y = (int)(r.Next(1, (MapHeightTiles - 1) * TileSize) / TileSize) * TileSize;
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
                    RHTile t = GetTileByGrid(Util.GetGridCoords(i, j));
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
                        liTiles.Add(GetTileOffGrid(x, y));
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
                c.CurrentMapName = _name;
                c.Position = c.NewMapPosition == Vector2.Zero ? c.Position : c.NewMapPosition;
                c.NewMapPosition = Vector2.Zero;
            }

            return rv;
        }

        public void AddMonster(Monster m)
        {
            bool rv = false;
            RHRandom r = new RHRandom();
            Vector2 position = m.Position;
            position.X = ((int)(position.X / TileSize)) * TileSize;
            position.Y = ((int)(position.Y / TileSize)) * TileSize;

            rv = _arrTiles[((int)position.X / TileSize), ((int)position.Y / TileSize)].Passable();
            if (!rv)
            {
                do
                {
                    position.X = (int)(r.Next(1, (MapWidthTiles - 1) * TileSize) / TileSize) * TileSize;
                    position.Y = (int)(r.Next(1, (MapHeightTiles - 1) * TileSize) / TileSize) * TileSize;
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
            m.CurrentMapName = _name;
            m.Position = Util.SnapToGrid(position);

            if (_liMonsters.Count == 0)
            {
                _liMonsters.Add(m);
            }
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

        public void CheckSeasonDoor()
        {
            foreach (Door d in _liDoors)
            {
                if (d.IsSeasonDoor())
                {
                    ((SeasonDoor)d).Check();
                }
            }
        }

        public void CheckForTriggeredCutScenes()
        {
            foreach(int cutsceneID in _liCutscenes)
            {
                CutsceneManager.CheckForTriggedCutscene(cutsceneID);
            }
        }

        public RHTile GetTileOffGrid(Vector2 targetLoc)
        {
            return GetTileOffGrid((int)targetLoc.X, (int)targetLoc.Y);
        }
        public RHTile GetTileOffGrid(Point targetLoc)
        {
            return GetTileOffGrid(targetLoc.X, targetLoc.Y);
        }
        public RHTile GetTileOffGrid(int x, int y)
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
        public RHTile GetTileByGrid(Point targetLoc)
        {
            return GetTileByGrid(targetLoc.ToVector2());
        }
        public RHTile GetTileByGrid(Vector2 pos)
        {
            return GetTileByGrid((int)pos.X, (int)pos.Y);
        }
        public RHTile GetTileByGrid(int x, int y)
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
                    WorldObject obj = ObjectManager.GetWorldObject(w.worldObjectID, new Vector2(w.x, w.y));
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

    public class SpawnPoint
    {
        Monster _monster;
        RHMap _map;
        Vector2 _vSpawnPoint;
        SpawnConditionEnum _eSpawnType = SpawnConditionEnum.Forest;

        public SpawnPoint(RHMap map, TiledMapObject obj)
        {
            _map = map;
            _eSpawnType = Util.ParseEnum<SpawnConditionEnum>(obj.Properties["SpawnType"]);
            _vSpawnPoint = map.GetTileByGrid(Util.GetGridCoords(obj.Position)).Center;
        }

        public void Spawn()
        {
            _monster = ObjectManager.GetMonsterByIndex(3);//ObjectManager.GetMobToSpawn(_eSpawnType);
            if (_monster != null)
            {
                _map.AddMonsterByPosition(_monster, _vSpawnPoint);
            }
        }

        public void Despawn()
        {
            _map.RemoveMonster(_monster);
            _monster = null;
        }

        public bool HasSpawned()
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

        bool _bSelected = false;
        public bool IsSelected => _bSelected;

        bool _bLegalTile;
        public bool IsLegal => _bLegalTile;

        public RHTile(int x, int y, string mapName)
        {
            _X = x;
            _Y = y;

            _mapName = mapName;
            _diProps = new Dictionary<TiledMapTileLayer, Dictionary<string, string>>();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_bLegalTile)
            {
                Rectangle source = new Rectangle(0, 112, 16, 16);
                Rectangle dest = new Rectangle((int)Position.X, (int)Position.Y, TileSize, TileSize);

                spriteBatch.Draw(GameContentManager.GetTexture(GameContentManager.FILE_WORLDOBJECTS), dest, source, Color.White);
            }
            if (_bSelected)
            {
                Rectangle source = new Rectangle(16, 112, 16, 16);
                Rectangle dest = new Rectangle((int)Position.X, (int)Position.Y, TileSize, TileSize);

                spriteBatch.Draw(GameContentManager.GetTexture(GameContentManager.FILE_WORLDOBJECTS), dest, source, Color.White);
            }
            if (_floorObj != null) {
                _floorObj.Draw(spriteBatch);
            }
            if (_obj != null) {
                _obj.Draw(spriteBatch);
            }
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
        /// For use only during Combat to see if can path through
        /// </summary>
        /// <returns>True if not in combat, or character is null</returns>
        public bool CanPathThroughInCombat()
        {
            bool rv = true;
            if (CombatManager.InCombat)
            {
                rv = Character == null;
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

        public void SetMapObject(Rectangle obj)
        {
            _tileMapObj = new TileObject(obj);
        }

        public TileObject GetDoorObject()
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
        /// <param name="c"></param>
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

        #region TileTraversal
        private RHMap MyMap()
        {
            return MapManager.Maps[MapName];
        }

        public List<RHTile> GetWalkableNeighbours()
        {
            Vector2[] DIRS = new[] { new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1) };
            List<RHTile> neighbours = new List<RHTile>();
            foreach (Vector2 d in DIRS)
            {
                RHTile tile = MyMap().GetTileByGrid(new Point((int)(_X + d.X), (int)(_Y + d.Y)));
                if (tile != null && ((tile.Passable() && tile.CanPathThroughInCombat()) || tile.GetDoorObject() != null) && tile.WorldObject == null)
                {
                    neighbours.Add(tile);
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Returns a list of all RHTiles adjacent to thistile
        /// </summary>
        /// <returns></returns>
        public List<RHTile> GetAdjacent()
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
                        rvTile = MyMap().GetTileByGrid(this.X, this.Y + 1);
                    }
                    break;
                case DirectionEnum.Left:
                    if (this.X > 0)
                    {
                        rvTile = MyMap().GetTileByGrid(this.X - 1, this.Y);
                    }
                    break;
                case DirectionEnum.Up:
                    RHTile rv = null;
                    if (this.Y > 0)
                    {
                        rvTile = MyMap().GetTileByGrid(this.X, this.Y - 1);
                    }
                    break;
                case DirectionEnum.Right:
                    if (this.X < MyMap().MapWidthTiles - 1)
                    {
                        rvTile = MyMap().GetTileByGrid(this.X + 1, this.Y);
                    }
                    break;
            }


            return rvTile;
        }
        #endregion

        public class TileObject
        {
            Building _building;
            public Building PlayerBuilding => _building;
            Rectangle _rectMapObject;
            public Rectangle Rect => _rectMapObject;

            public TileObject(Building b)
            {
                _rectMapObject = b.BoxToEnter;
                _building = b;
            }

            public TileObject(Rectangle rect)
            {
                _rectMapObject = rect;
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

            if(ObjectManager.DiNPC[_iShopID].CurrentMapName == _sMap)
            {
                if (MapManager.RetrieveTile(_iShopX, _iShopY).Contains(ObjectManager.DiNPC[_iShopID]))
                {
                    rv = true;
                }
            }

            return rv;
        }

        internal void Talk()
        {
            ((ShopKeeper)ObjectManager.DiNPC[_iShopID]).Talk(true);
        }
    }
}
