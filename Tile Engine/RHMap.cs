using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using RiverHollow.Actors;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.RiverHollow;
using static RiverHollow.WorldObjects.Door;
using static RiverHollow.WorldObjects.Floor;
using static RiverHollow.WorldObjects.WorldItem;
using static RiverHollow.WorldObjects.WorldItem.Machine;

namespace RiverHollow.Tile_Engine
{
    public class RHMap
    {
        public int MapWidthTiles = 100;
        public int MapHeightTiles = 100;
        
        private string _name;
        public string Name { get => _name.Replace(@"Maps\", ""); set => _name = value; } //Fuck off with that path bullshit

        protected Building _mapBuilding;
        public Building MapBuilding { get => _mapBuilding; }

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

        protected TiledMap _map;
        public TiledMap Map { get => _map; }

        protected RHTile[,] _tileArray;
        protected TiledMapRenderer _renderer;
        protected List<TiledMapTileset> _liTilesets;
        protected Dictionary<string, TiledMapTileLayer> _diLayers;
        public Dictionary<string, TiledMapTileLayer> Layers => _diLayers;

        protected List<RHTile> _liTestTiles;
        protected List<WorldActor> _liActors;
        protected List<Mob> _liMobs;
        protected List<Door> _liDoors;
        public List<WorldActor> ToRemove;
        public List<WorldActor> ToAdd;
        protected List<Building> _liBuildings;
        protected List<RHTile> _liModifiedTiles;
        public List<RHTile> ModTiles => _liModifiedTiles;
        protected List<SpawnPoint> _liMonsterSpawnPoints;

        protected List<Item> _liItems;
        protected List<ShopData> _liShopData;

        private Dictionary<Rectangle, string> _dictExit;
        public Dictionary<Rectangle, string> DictionaryExit { get => _dictExit; }
        private Dictionary<string, Rectangle> _dictEntrance;
        public Dictionary<string, Rectangle> DictionaryEntrance { get => _dictEntrance; }
        private Dictionary<string, Vector2> _dictCharacterLayer;
        public Dictionary<string, Vector2> DictionaryCharacterLayer { get => _dictCharacterLayer; }
        private List<TiledMapObject> _liMapObjects;

        public RHMap() {
            _liMonsterSpawnPoints = new List<SpawnPoint>();
            _liTestTiles = new List<RHTile>();
            _liTilesets = new List<TiledMapTileset>();
            _liActors = new List<WorldActor>();
            _liMobs = new List<Mob>();
            _liBuildings = new List<Building>();
            _liModifiedTiles = new List<RHTile>();
            _liItems = new List<Item>();
            _liMapObjects = new List<TiledMapObject>();
            _dictExit = new Dictionary<Rectangle, string>();
            _dictEntrance = new Dictionary<string, Rectangle>();
            _dictCharacterLayer = new Dictionary<string, Vector2>();
            _liShopData = new List<ShopData>();
            _liDoors = new List<Door>();

            ToRemove = new List<WorldActor>();
            ToAdd = new List<WorldActor>();
        }

        public RHMap(RHMap map) : this()
        {
            _map = map.Map;
            _name = map.Name;
            _renderer = map._renderer;
            _tileArray = map._tileArray;

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

            _tileArray = new RHTile[MapWidthTiles, MapHeightTiles];
            for (int i = 0; i < MapHeightTiles; i++) {
                for (int j = 0; j < MapWidthTiles; j++)
                {
                    _tileArray[j, i] = new RHTile(j, i, _name);
                    _tileArray[j, i].SetProperties(this);
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
            foreach(RHTile t in _liModifiedTiles)
            {
                if (t.HasBeenDug())
                {
                    t.Water(true);
                }
            }
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
                        string ID = (mapObject.Properties.ContainsKey("ID")) ? (":" + mapObject.Properties["ID"]) : "";
                        Rectangle r = new Rectangle((int)mapObject.Position.X, (int)mapObject.Position.Y, (int)mapObject.Size.Width, (int)mapObject.Size.Height);

                        string map = string.Empty;
                        if (mapObject.Properties.ContainsKey("Exit"))
                        {
                            map = mapObject.Properties["Exit"] == "Home" ? MapManager.HomeMap : mapObject.Properties["Exit"] + ID;
                            _dictExit.Add(r, map);
                        }
                        else if (mapObject.Properties.ContainsKey("Entrance"))
                        {
                            map = mapObject.Properties["Entrance"] == "Home" ? MapManager.HomeMap : mapObject.Properties["Entrance"] + ID;
                            _dictEntrance.Add(map, r);
                        }
                    }
                }
                else if (ol.Name == "Character Layer")
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
                else if (ol.Name == "MapObject Layer")
                {
                    foreach (TiledMapObject mapObject in ol.Objects)
                    {
                        _liMapObjects.Add(mapObject);
                    }
                }
            }
        }

        public void PopulateMap()
        {
            TiledMapProperties props = _map.Properties;
            List<int> _liMobs = new List<int>();
            int minMobs = 0;
            int maxMobs = 0;
            List<int> resources = new List<int>();
            int minRes = 0;
            int maxRes = 0;

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
                else if (obj.Name.Equals("Mob"))
                {
                    Vector2 vect = obj.Position;
                    Mob mob = ObjectManager.GetMobByIndex(int.Parse(obj.Properties["ID"]), vect);
                    AddMob(mob);
                }
                else if (obj.Name.Equals("Chest"))
                {
                    Container c = (Container)ObjectManager.GetWorldObject(190);
                    c.MapPosition = obj.Position;
                    PlacePlayerObject(c);
                    string[] holdSplit = obj.Properties["Holding"].Split('/');
                    foreach (string s in holdSplit)
                    {
                        InventoryManager.AddNewItemToInventory(int.Parse(s), c);
                    }
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
                else if (obj.Name.Equals("Manor"))
                {
                    Building manor = ObjectManager.GetManor();
                    manor.SetCoordinates(obj.Position);
                    manor.SetName(PlayerManager.ManorName);          
                    AddBuilding(manor);
                }
            }

            string[] split;
            foreach (KeyValuePair<string, string> prop in props)
            {
                if (prop.Key.Equals("Mobs"))
                {
                    split = prop.Value.Split('/');
                    foreach (string s in split)
                    {
                        _liMobs.Add(int.Parse(s));
                    }
                }
                else if (prop.Key.Equals("MobsMax")) { maxMobs = int.Parse(prop.Value); }
                else if (prop.Key.Equals("MobsMin")) { minMobs = int.Parse(prop.Value); }
                if (prop.Key.Equals("Resources"))
                {
                    split = prop.Value.Split('/');
                    foreach (string s in split)
                    {
                        resources.Add(int.Parse(s));
                    }
                }
                else if (prop.Key.Equals("ResourcesMax")) { maxRes = int.Parse(prop.Value); }
                else if (prop.Key.Equals("ResourcesMin")) { minRes = int.Parse(prop.Value); }
            }

            if(resources.Count > 0)
            {
                RHRandom r = new RHRandom();
                int numResources = r.Next(minRes, maxRes);
                while(numResources != 0)
                {
                    int chosenResource = r.Next(0, resources.Count - 1);

                    PlaceWorldObject(ObjectManager.GetWorldObject(resources[chosenResource], new Vector2(r.Next(1, _map.Width - 1) * TileSize, r.Next(1, _map.Height - 1) * TileSize)), true);

                    numResources--;
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
                    Mob mob = ObjectManager.GetMobByIndex(_liMobs[chosenMob], vect);
                    mob.CurrentMapName = _name;
                    AddMob(mob);

                    numMobs--;
                }
            }

            SpawnMobs();
        }

        public void Update(GameTime gameTime)
        {
            if (this == MapManager.CurrentMap)
            {
                _renderer.Update(_map, gameTime);
                if (IsRunning())
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

            foreach (RHTile tile in _liModifiedTiles)
            {
                WorldObject obj = tile.WorldObject;
                if (obj != null)
                {
                    if (obj.IsProcessor())
                    {
                        Processor p = (Processor)obj;
                        p.Update(gameTime);
                    }
                    else if (obj.IsCrafter())
                    {
                        Crafter c = (Crafter)obj;
                        c.Update(gameTime);
                    }
                }
            }

            foreach (WorldActor c in ToRemove)
            {
                if (c.IsMob() && _liMobs.Contains((Mob)c)) { _liMobs.Remove((Mob)c); }
                else if (_liActors.Contains(c)) { _liActors.Remove(c); }
            }
            ToRemove.Clear();

            if (ToAdd.Count > 0)
            {
                List<WorldActor> moved = new List<WorldActor>();
                foreach (WorldActor c in ToAdd)
                {
                    if (!MapManager.Maps[c.CurrentMapName].Contains(c))
                    {
                        if (c.IsMob() && !_liMobs.Contains((Mob)c)) { _liMobs.Add((Mob)c); }
                        else if (!_liActors.Contains(c)) { _liActors.Add(c); }
                        c.CurrentMapName = _name;
                        c.Position = c.NewMapPosition == Vector2.Zero ? c.Position : c.NewMapPosition;
                        c.NewMapPosition = Vector2.Zero;
                        moved.Add(c);
                    }
                }
                foreach (WorldActor c in moved)
                {
                    ToAdd.Remove(c);
                }
                moved.Clear();
            }

            if (IsRunning())
            {
                foreach (WorldActor c in _liActors)
                {
                    c.Update(gameTime);
                }
            }
            

            ItemPickUpdate();
        }

        public void Rollover()
        {
            foreach(RHTile tile in _liModifiedTiles)
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
                Item it = _liItems[i];
                if (InventoryManager.HasSpaceInInventory(it.ItemID, it.Number))
                {
                    if (it.OnTheMap && it.AutoPickup)
                    {
                        if (it.FinishedMoving() && it.CollisionBox.Intersects(player.CollisionBox))
                        {
                            removedList.Add(it);
                            InventoryManager.AddNewItemToInventory(it.ItemID, it.Number);
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

        public void DrawBase(SpriteBatch spriteBatch)
        {
            SetLayerVisibility(false);

            _renderer.Draw(_map, Camera._transform);

            foreach(WorldActor c in _liActors)
            {
                c.Draw(spriteBatch, true);
            }

            foreach (Mob m in _liMobs)
            {
                m.Draw(spriteBatch, true);
            }

            foreach(Building b in _liBuildings)
            {
                b.Draw(spriteBatch);
            }

            foreach (RHTile t in _liModifiedTiles)
            {
                t.Draw(spriteBatch);
            }

            foreach (Item i in _liItems)
            {
                i.Draw(spriteBatch);
            }

            foreach(RHTile t in _liTestTiles)
            {
                bool passable = t.Passable();
                spriteBatch.Draw(GameContentManager.GetTexture(@"Textures\Dialog"), new Rectangle((int)t.Position.X, (int)t.Position.Y, TileSize, TileSize), new Rectangle(288, 128, TileSize, TileSize) , passable ? Color.Green *0.5f : Color.Red * 0.5f, 0, Vector2.Zero, SpriteEffects.None, 99999);
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
        public bool MoveAndCollide(WorldActor c, Rectangle originalRectangle, Vector2 dir, bool ignoreCollisions = false)
        {
            bool rv = true;


            return rv;
        }

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

            if(actor != PlayerManager.World && !actor.IsMob()) {
                list.Add(PlayerManager.World.CollisionBox);
            }

            return list;
        }
        private void AddTile(ref List<Rectangle> list, int one, int two)
        {
            RHTile tile = MapManager.CurrentMap.GetTile(Util.GetGridCoords(one, two));
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
            RHTile firstTile = MapManager.Maps[map].GetTile(Util.GetGridCoords(first));
            RHTile secondTile = MapManager.Maps[map].GetTile(Util.GetGridCoords(second));
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
                RHTile testTile = GetTile((int)(varCol + (v.Equals("Col") ? 1 : 0)), (int)(varRow + (v.Equals("Row") ? 1 : 0)));
                if (testTile != null && testTile.Passable()) {
                    rv = 1;
                }
            }
            else if (centerDelta < 0)
            {
                RHTile testTile = GetTile((int)(varCol - (v.Equals("Col") ? 1 : 0)), (int)(varRow + (v.Equals("Row") ? 1 : 0)));
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
                        ((TalkingActor)c).Talk();
                        rv = true;
                        break;
                    }
                }
            }

            if (!rv)
            {
                foreach (Building b in _liBuildings)
                {
                    if (b.BoxToEnter.Contains(mouseLocation) && PlayerManager.PlayerInRange(b.BoxToEnter))
                    {
                        rv = true;
                        MapManager.EnterBuilding(b);
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
                    if(it.ManualPickup && it.CollisionBox.Contains(GraphicCursor.GetTranslatedMouseLocation())){
                        if(InventoryManager.AddItemToInventory(it))
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
                RHTile tile = GetTile(mouseLocation.X / TileSize, mouseLocation.Y / TileSize);
                if (tile != null)
                {
                    if (tile.WorldObject != null)
                    {
                        WorldObject s = tile.WorldObject;
                        if (s.IsMachine())
                        {
                            Machine p = (Machine)s;
                            if (p.ProcessingFinished()) { p.TakeFinishedItem(); }
                            else if (InventoryManager.GetCurrentItem() != null && !p.Processing()) { p.ProcessClick(); }
                        }
                        else if (s.IsContainer())
                        {
                            if (IsDungeon && DungeonManager.IsEndChest((Container)s))
                            {
                                Staircase stairs = (Staircase)ObjectManager.GetWorldObject(3, Vector2.Zero);
                                stairs.SetExit(MapManager.HomeMap);
                                PlaceWorldObject(stairs, true);
                            }
                            GUIManager.SetScreen(new InventoryScreen((Container)s));
                        }
                        else if (s.IsPlant())
                        {
                            Plant p = (Plant)s;
                            if (p.FinishedGrowing())
                            {
                                Item i = p.Harvest();
                                if (i != null)
                                {
                                    _liItems.Add(i);
                                }
                                MapManager.RemoveWorldObject(p);
                                p.RemoveSelfFromTiles();
                            }
                        }
                        else if (s.IsDoor())
                        {
                            ((Door)s).ReadInscription();
                        }
                    }

                    if (tile.ContainsProperty("Journal", out string val) && val.Equals("true"))
                    {
                        GUIManager.SetScreen(new TextScreen(GameContentManager.GetGameText("Journal"), true));
                    }

                    if (tile.WorldObject != null && tile.WorldObject.ID == 3) //Checks to see if the tile contains a staircase object
                    {
                        MapManager.ChangeMaps(PlayerManager.World, this.Name, ((Staircase)tile.WorldObject).ToMap);
                    }
                }
            }

            if (!rv)
            {
                if (Scrying())
                {
                    FinishedBuilding();
                    Unpause();
                    Scry(false);
                    ResetCamera();
                }
            }

            return rv;
        }

        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (Scrying()) {

                if (Constructing() || MovingBuildings())
                {
                    if (GraphicCursor.HeldBuilding != null)
                    {
                        if (AddBuilding(GraphicCursor.HeldBuilding))
                        {
                            GUIManager.SetScreen(new NamingScreen(GraphicCursor.HeldBuilding));

                            PlayerManager.TakeMoney(gmMerchandise.MoneyCost);
                            foreach (KeyValuePair<int, int> kvp in gmMerchandise.RequiredItems)
                            {
                                InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                            }

                            gmMerchandise = null;
                            FinishedBuilding();
                            rv = true;
                        }
                    }
                    else if (GraphicCursor.HeldBuilding == null)
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
            }
            else
            {
                if (PlayerManager._merchantChest.Contains(mouseLocation))
                {
                    Item i = InventoryManager.GetCurrentItem();
                    PlayerManager._merchantChest.AddItem(i);
                    InventoryManager.RemoveItemFromInventory(InventoryManager.GetCurrentItem());
                }
                foreach (WorldActor c in _liActors)
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
                foreach (RHTile tile in _liModifiedTiles)
                {
                    WorldObject obj = tile.WorldObject;
                    if (obj != null && obj.CollisionBox.Contains(mouseLocation))
                    {
                        if (obj.IsMachine())
                        {
                            Machine p = (Machine)obj;
                            if (p.ProcessingFinished()) { p.TakeFinishedItem(); }
                            else if (!p.Processing()) { p.ProcessClick(); }
                        }
                        else if (obj.IsClassChanger())
                        {
                            ((ClassChanger)obj).ProcessClick();
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

            if (_liTestTiles.Count > 0) { _liTestTiles.Clear(); }

            if (Scrying())
            {
                Building building = GraphicCursor.HeldBuilding;
                _liTestTiles = new List<RHTile>();
                if (building != null)
                {
                    TestMapTiles(building, _liTestTiles);
                }

                foreach (Building b in _liBuildings)
                {
                    if (!b.IsManor)
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
            }
            else{
                bool found = false;
                foreach(WorldActor c in _liActors)
                {
                    if(!c.IsMob() && c.CollisionContains(mouseLocation)){
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
                    Vector2 vec = mouseLocation.ToVector2() - new Vector2(0, selectedStaticItem.GetWorldItem().Height- selectedStaticItem.GetWorldItem().BaseHeight);
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

        public RHTile RetrieveTile(int x, int y)
        {
            if (x >= MapWidthTiles || x < 0) { return null; }
            if (y >= MapHeightTiles || y < 0) { return null; }

            return _tileArray[x, y];
        }
        public RHTile RetrieveTile(Point targetLoc)
        {
            if(targetLoc.X >= GetMapWidth() || targetLoc.X < 0) { return null;  }
            if (targetLoc.Y >= GetMapHeight() || targetLoc.Y < 0) { return null; }

            try
            {
                return _tileArray[targetLoc.X / TileSize, targetLoc.Y / TileSize];
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public RHTile RetrieveTileFromGridPosition(Point targetLoc)
        {
            if (targetLoc.X >= MapWidthTiles || targetLoc.X < 0) { return null; }
            if (targetLoc.Y >= MapHeightTiles || targetLoc.Y < 0) { return null; }

            return _tileArray[targetLoc.X, targetLoc.Y];
        }
        public void RemoveWorldObject(WorldObject o)
        {
            List<RHTile> toRemove = new List<RHTile>();
            foreach (RHTile tile in _liModifiedTiles)
            {
                if (tile.WorldObject == o)
                {
                    tile.RemoveWorldObject();
                }
                if (tile.Flooring == o)
                {
                    tile.RemoveFlooring();
                    
                }
                if (tile.Flooring == null && tile.WorldObject == null)
                {
                    toRemove.Add(tile);
                }
            }

            foreach (RHTile tile in toRemove)
            {
                _liModifiedTiles.Remove(tile);
            }
        }
        public void RemoveCharacter(WorldActor c)
        {
            ToRemove.Add(c);
        }
        public void RemoveMob(Mob m)
        {
            _liMobs.Remove(m);
            foreach (Door d in _liDoors)
            {
                if (d.IsMobDoor()) { ((MobDoor)d).Check(_liMobs.Count); }
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

        public void LoadBuilding(Building b)
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
                            b.BuildingChest.MapPosition = Util.SnapToGrid(mapObject.Position);
                            PlacePlayerObject(b.BuildingChest);
                        }
                        else if (mapObject.Name.Contains("Pantry"))
                        {
                            b.Pantry.MapPosition = Util.SnapToGrid(mapObject.Position);
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
                        GraphicCursor.PickUpBuilding(b);
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
                            FinishedBuilding();
                            Unpause();
                            Scry(false);
                            ResetCamera();
                        }
                        else
                        {
                            GUIManager.SetScreen(new TextScreen("Cannot Destroy occupied buildings.", false));
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

        public bool AddBuilding(Building b)
        {
            bool rv = false;
            List<RHTile> tiles = new List<RHTile>();
            if (TestMapTiles(b, tiles))
            {
                _liTestTiles.Clear();
                AssignMapTiles(b, tiles);
                _dictEntrance.Add(b.PersonalID.ToString(), b.BoxToExit); //TODO: FIX THIS
                GraphicCursor.DropBuilding();
                if (!_liBuildings.Contains(b)) //For the use case of moving buildings
                { 
                    _liBuildings.Add(b);
                } 
                PlayerManager.AddBuilding(b);
                
                rv = true;
            }

            return rv;
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
                            WorldAdventurer w = ObjectManager.GetWorker(GraphicCursor.WorkerToPlace);
                            b.AddWorker(w);
                            b._selected = false;
                            GUIManager.SetScreen(new NamingScreen(w));
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
                    o.SetCoordinates(position);

                    rv = TestMapTiles(o, tiles);
                } while (!rv);
            }

            if (rv)
            {
                AssignMapTiles(o, tiles);
            }

            return rv;
        }

        public bool TestMapTiles(WorldObject o, List<RHTile> tiles)
        {
            bool rv = false;
            tiles.Clear();
            Vector2 position = o.MapPosition;
            position.X = ((int)(position.X / TileSize)) * TileSize;
            position.Y = ((int)(position.Y / TileSize)) * TileSize;

            int colColumns = o.CollisionBox.Width / TileSize;
            int colRows = o.CollisionBox.Height / TileSize;

            rv = true;
            //BUG: Went out of bounds?
            for (int i = 0; i < colRows; i++)
            {
                for (int j = 0; j < colColumns; j++)
                {
                    int x = Math.Min((o.CollisionBox.Left + (j * TileSize)) / TileSize, MapWidthTiles-1);
                    int y = Math.Min((o.CollisionBox.Top + (i * TileSize)) / TileSize, MapHeightTiles-1);
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
            if (!_liModifiedTiles.Contains(tiles[0]))
            {
                _liModifiedTiles.Add(tiles[0]);
            }
            foreach (RHTile t in tiles)
            {
                t.SetWorldObject(o);
            }
        }

        public bool PlacePlayerObject(WorldObject obj)
        {
            bool rv = false;
            if (_liTestTiles.Count == 0)
            {
                RHTile tile = _tileArray[(int)obj.MapPosition.X / TileSize, (int)obj.MapPosition.Y / TileSize];
                if (tile.Passable())
                {
                    tile.SetWorldObject(obj);
                    AssignMapTiles(obj, new List<RHTile>() { tile });
                    rv = true;
                }
            }
            else
            {
                bool placeIt = true;
                foreach (RHTile t in _liTestTiles)
                {
                    if (!t.Passable())
                    {
                        placeIt = false;
                        break;
                    }
                }

                if (placeIt)
                {
                    AssignMapTiles(obj, _liTestTiles);
                    rv = true;
                }
            }

            return rv;
        }

        public void AddCharacter(WorldActor c)
        {
            ToAdd.Add(c);
        }

        public void AddMob(Mob m)
        {
            bool rv = false;
            RHRandom r = new RHRandom();
            Vector2 position = m.Position;
            position.X = ((int)(position.X / TileSize)) * TileSize;
            position.Y = ((int)(position.Y / TileSize)) * TileSize;

            rv = _tileArray[((int)position.X / TileSize), ((int)position.Y / TileSize)].Passable();
            if (!rv)
            {
                do
                {
                    position.X = (int)(r.Next(1, (MapWidthTiles - 1) * TileSize) / TileSize) * TileSize;
                    position.Y = (int)(r.Next(1, (MapHeightTiles - 1) * TileSize) / TileSize) * TileSize;
                    rv = _tileArray[((int)position.X / TileSize), ((int)position.Y / TileSize)].Passable();
                } while (!rv);
            }

            if (rv)
            {
                AddMob(m, position);
            }
        }

        public void AddMob(Mob m, Vector2 position)
        {
            m.CurrentMapName = _name;
            m.Position = position;
            m.NewFoV();

            if (_liMobs.Count == 0)
            {
                _liMobs.Add(m);
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

        public RHTile GetTile(Vector2 pos)
        {
            return GetTile((int)pos.X, (int)pos.Y);
        }
        public RHTile GetTile(int x, int y)
        {
            RHTile tile = null;

            if(x >= 0 && x < MapWidthTiles && y >= 0 && y < MapHeightTiles)
            {
                tile = _tileArray[x, y];
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
                foreach (RHTile tile in ModTiles)
                {
                    WorldObject w = tile.WorldObject;
                    if (w != null)
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
                        else if (w.IsClassChanger())
                        {
                            WorldObjectData d = new WorldObjectData
                            {
                                worldObjectID = tile.WorldObject.ID,
                                x = (int)((ClassChanger)tile.WorldObject).MapPosition.X,
                                y = (int)((ClassChanger)tile.WorldObject).MapPosition.Y
                            };
                            mapData.worldObjects.Add(d);
                        }
                        else
                        {
                            WorldObjectData d = new WorldObjectData
                            {
                                worldObjectID = tile.WorldObject.ID,
                                x = (int)tile.WorldObject.MapPosition.X,
                                y = (int)tile.WorldObject.MapPosition.Y
                            };
                            mapData.worldObjects.Add(d);
                        }
                    }

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
                    WorldObject obj = ObjectManager.GetWorldObject(w.worldObjectID, new Vector2(w.x, w.y));
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
                RHTile tile = _tileArray[(int)e.MapPosition.X / TileSize, (int)e.MapPosition.Y / TileSize];
                tile.SetFloorObject(e);
                _liModifiedTiles.Add(tile);
            }
        } 
    }

    public class SpawnPoint
    {
        Mob _mob;
        RHMap _map;
        Vector2 _vSpawnPoint;
        SpawnConditionEnum _eSpawnType = SpawnConditionEnum.Forest;

        public SpawnPoint(RHMap map, TiledMapObject obj)
        {
            _map = map;
            _eSpawnType = Util.ParseEnum<SpawnConditionEnum>(obj.Properties["SpawnType"]);
            _vSpawnPoint = map.GetTile(Util.GetGridCoords(obj.Position)).Center;
        }

        public void Spawn()
        {
            _mob = ObjectManager.GetMobByIndex(4);//ObjectManager.GetMobToSpawn(_eSpawnType);
            if (_mob != null)
            {
                _map.AddMob(_mob, _vSpawnPoint);
            }
        }

        public void Despawn()
        {
            _map.RemoveMob(_mob);
            _mob = null;
        }

        public bool HasSpawned()
        {
            return _mob != null;
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

        Dictionary<TiledMapTileLayer, Dictionary<string, string>> _diProps;
        WorldObject _obj;
        public WorldObject WorldObject => _obj;

        Floor _floorObj;
        public Floor Flooring => _floorObj;

        bool _isRoad;
        public bool IsRoad => _isRoad;

        public RHTile(int x, int y, string mapName)
        {
            _X = x;
            _Y = y;

            _mapName = mapName;
            _diProps = new Dictionary<TiledMapTileLayer, Dictionary<string, string>>();
        }

        internal void Draw(SpriteBatch spriteBatch)
        {
            if (_floorObj != null) { _floorObj.Draw(spriteBatch); }
            if (_obj != null) { _obj.Draw(spriteBatch); }
        }

        public void Dig()
        {
            if (_floorObj == null)
            {
                _floorObj = new Earth
                {
                    MapPosition = Position
                };
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

        public List<RHTile> GetWalkableNeighbours()
        {
            Vector2[] DIRS = new[] { new Vector2(1, 0), new Vector2(0, -1), new Vector2(-1, 0), new Vector2(0, 1) };
            List<RHTile> neighbours = new List<RHTile>();
            foreach (Vector2 d in DIRS)
            {
                RHTile tile = MapManager.Maps[MapName].RetrieveTileFromGridPosition(new Point((int)(_X + d.X), (int)(_Y + d.Y)));
                if (tile != null && tile.Passable() && tile.WorldObject == null) {
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
                        _diProps.Add(l, map.GetProperties((TiledMapTile)tile));
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
        public void RemoveWorldObject()
        {
            _obj = null;
        }
        public void RemoveFlooring()
        {
            _floorObj = null;
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
        public bool SetFloorObject(Floor f)
        {
            bool rv = false;
            if (_floorObj == null && Passable())
            {
                _floorObj = f;
                rv = true;
            }
            return rv;
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
                    MapManager.DropItemsOnMap(DropManager.DropItemsFromWorldObject(_obj.ID), _obj.CollisionBox.Center.ToVector2());
                    _obj.RemoveSelfFromTiles();
                    MapManager.RemoveWorldObject(_obj);
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
