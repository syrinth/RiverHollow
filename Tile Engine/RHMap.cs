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
using MonoGame.Extended.Tiled;
using static RiverHollow.RiverHollow;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using static RiverHollow.WorldObjects.WorldItem.Machine;
using static RiverHollow.WorldObjects.WorldItem;
using RiverHollow.Game_Managers.GUIObjects;
using static RiverHollow.WorldObjects.Floor;
using MonoGame.Extended.Tiled.Graphics;
using static RiverHollow.WorldObjects.Door;

namespace RiverHollow.Tile_Engine
{
    public class RHMap
    {
        public int MapWidthTiles = 100;
        public int MapHeightTiles = 100;
        
        private string _name;
        public string Name { get => _name.Replace(@"Maps\", ""); set => _name = value; } //Fuck off with that path bullshit

        protected WorkerBuilding _mapBuilding;
        public WorkerBuilding MapBuilding { get => _mapBuilding; }

        public bool _bBuilding;
        public bool IsBuilding { get => _bBuilding; }
        public bool _bDungeon;
        public bool IsDungeon { get => _bDungeon; }
        public bool _bTown;
        public bool IsTown { get => _bTown; }
        public bool _bOutside;
        public bool IsOutside { get => _bOutside; }

        protected TiledMap _map;
        public TiledMap Map { get => _map; }

        protected RHTile[,] _tileArray;
        protected TiledMapRenderer _renderer;
        protected List<TiledMapTileset> _liTilesets;
        protected Dictionary<string, TiledMapTileLayer> _diLayers;
        public Dictionary<string, TiledMapTileLayer> Layers => _diLayers;

        protected List<RHTile> _liBuildingTiles;
        protected List<WorldCharacter> _liCharacters;
        protected List<Mob> _liMobs;
        protected List<Door> _liDoors;
        public List<WorldCharacter> ToRemove;
        public List<WorldCharacter> ToAdd;
        protected List<WorkerBuilding> _liBuildings;
        protected List<RHTile> _liModifiedTiles;
        public List<RHTile> ModTiles => _liModifiedTiles;

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
            _liBuildingTiles = new List<RHTile>();
            _liTilesets = new List<TiledMapTileset>();
            _liCharacters = new List<WorldCharacter>();
            _liMobs = new List<Mob>();
            _liBuildings = new List<WorkerBuilding>();
            _liModifiedTiles = new List<RHTile>();
            _liItems = new List<Item>();
            _liMapObjects = new List<TiledMapObject>();
            _dictExit = new Dictionary<Rectangle, string>();
            _dictEntrance = new Dictionary<string, Rectangle>();
            _dictCharacterLayer = new Dictionary<string, Vector2>();
            _liShopData = new List<ShopData>();
            _liDoors = new List<Door>();

            ToRemove = new List<WorldCharacter>();
            ToAdd = new List<WorldCharacter>();
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
            bool.TryParse(_map.Properties["Outside"], out _bOutside);

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
            if (_map.Properties.ContainsKey("Outside"))
            {
                bool.TryParse(_map.Properties["Outside"], out _bOutside);
            }

            if (_bTown)
            {
                foreach (KeyValuePair<string, Upgrade> kvp in GameManager.DiUpgrades)
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

                        if (mapObject.Properties.ContainsKey("Exit"))
                        {
                            _dictExit.Add(r, mapObject.Properties["Exit"] + ID);
                        }
                        else if (mapObject.Properties.ContainsKey("Entrance"))
                        {
                            _dictEntrance.Add(mapObject.Properties["Entrance"] + ID, r);
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
                    PlaceWorldObject(ObjectManager.GetWorldObject(WorldItem.Rock, Util.Normalize(obj.Position)));
                }
                else if (obj.Name.Equals("Tree"))
                {
                    PlaceWorldObject(ObjectManager.GetWorldObject(WorldItem.Tree, Util.Normalize(obj.Position)));
                }
                else if (obj.Name.Equals("Mob"))
                {
                    Vector2 vect = obj.Position;
                    Mob mob = CharacterManager.GetMobByIndex(int.Parse(obj.Properties["ID"]), vect);
                    mob.CurrentMapName = _name;
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
                        Position = Util.Normalize(obj.Position),
                        CurrentMapName = _name
                    };
                    _liCharacters.Add(s);
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
                    Mob mob = CharacterManager.GetMobByIndex(_liMobs[chosenMob], vect);
                    mob.CurrentMapName = _name;
                    AddMob(mob);

                    numMobs--;
                }
            }
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

            if (IsRunning())
            {
                foreach (WorldCharacter c in _liCharacters)
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

            CheckSpirits();
        }

        public void CheckSpirits()
        {
            foreach (WorldCharacter c in _liCharacters)
            {
                if (c.IsSpirit())
                {
                    ((Spirit)c).CheckCondition();
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

            foreach (RHTile t in _liModifiedTiles)
            {
                t.Draw(spriteBatch);
            }

            foreach (Item i in _liItems)
            {
                i.Draw(spriteBatch);
            }

            foreach(RHTile t in _liBuildingTiles)
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
                    bool determinant = l.Name.Contains("Upper");

                    if (revealUpper) { l.IsVisible = determinant; }
                    else { l.IsVisible = !determinant; }
                }

                if (l.IsVisible && _bOutside) {
                    l.IsVisible = l.Name.Contains(GameCalendar.GetSeason());
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

            int column = ((dir.X < 0) ? testX.Left : testX.Right) / TileSize;
            int row = ((dir.Y < 0) ? testY.Top : testY.Bottom) / TileSize;

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
                if (mover != c && !c.IsSpirit() && c.CollisionIntersects(movingChar))
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
                    Rectangle cellRect = new Rectangle(varCol * TileSize, varRow * TileSize, TileSize, TileSize);
                    if (!mapTile.Passable() && cellRect.Intersects(movingChar))
                    {
                        if (row == -1)
                        {
                            Rectangle r = movingChar;
                            r.X -= (int)dir.X;

                            dir.X = dir.X < 0 ? (cellRect.Right - r.Left) : (cellRect.Left - r.Right);
                            movingChar = r;
                        }
                        if (column == -1)
                        {
                            Rectangle r = movingChar;
                            r.Y -= (int)dir.Y;

                            dir.Y = dir.Y < 0 ? (cellRect.Bottom - r.Top) : (cellRect.Top - r.Bottom);
                            movingChar = r;
                        }
                    }
                }
            }
            catch(IndexOutOfRangeException ex)
            {

            }
        }

        public int GetMinColumn(Rectangle movingChar)
        {
            return (movingChar.Left / TileSize);
        }

        public int GetMaxColumn(Rectangle movingChar)
        {
            int i = (movingChar.Right / TileSize);
            return i;
        }

        public int GetMinRow(Rectangle movingChar)
        {
            return (movingChar.Top / TileSize);
        }

        public int GetMaxRow(Rectangle movingChar)
        {
            return (movingChar.Bottom / TileSize);
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
                foreach (WorldCharacter c in _liCharacters)
                {
                    if (PlayerManager.PlayerInRange(c.CollisionBox.Center, (int)(TileSize * 1.5)) && c.CollisionContains(mouseLocation) && c.CanTalk())
                    {
                        rv = true;
                        if (c.IsSpirit())
                        {
                            ((Spirit)c).Talk();
                        }
                        else
                        {
                            ((NPC)c).Talk();
                        }
                        break;
                    }
                }
            }

            if (!rv)
            {
                foreach (WorkerBuilding b in _liBuildings)
                {
                    if (b.BoxToEnter.Contains(mouseLocation) && PlayerManager.PlayerInRange(b.BoxToEnter.Center))
                    {
                        rv = true;
                        MapManager.EnterBuilding(b);
                        break;
                    }
                }
            }

            if (!rv)
            {
                RHTile tile = _tileArray[mouseLocation.X / TileSize, mouseLocation.Y / TileSize];
                if (tile != null)
                {
                    if (tile.WorldObject != null)
                    {
                        WorldObject s = tile.WorldObject;
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
                        GUIManager.SetScreen(new TextScreen(GameContentManager.GetDialogue("Journal"), true));
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

            if (Scrying())
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
                    if(!c.IsMob() && c.CollisionContains(mouseLocation)){
                        if (!c.IsSpirit() || ((Spirit)c).Active)
                        {
                            GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Talk;
                            found = true;
                            break;
                        }
                    }
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
            _liCharacters.Clear();
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
        public void RemoveCharacter(WorldCharacter c)
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
                            b.BuildingChest.MapPosition = Util.Normalize(mapObject.Position);
                            PlacePlayerObject(b.BuildingChest);
                        }
                        else if (mapObject.Name.Contains("Pantry"))
                        {
                            b.Pantry.MapPosition = Util.Normalize(mapObject.Position);
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

        public void PickUpBuilding(Point mouseLocation)
        {
            foreach(WorkerBuilding b in _liBuildings)
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

        public bool RemoveBuilding(Point mouseLocation)
        {
            bool rv = false;
            WorkerBuilding bldg = null;
            foreach (WorkerBuilding b in _liBuildings)
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
                        GUIManager.SetScreen(new TextScreen("Cannot Destroy occupied buildings.",false));
                    }
                }
            }
            if(bldg != null) {
                rv = true;
                _liBuildings.Remove(bldg);
            }

            return rv;
        }

        public bool AddBuilding(WorkerBuilding b, bool openNameInput = true)
        {
            bool rv = false;
            List<RHTile> tiles = new List<RHTile>();
            if (TestMapTiles(b, tiles))
            {
                _liBuildingTiles.Clear();
                AssignMapTiles(b, tiles);
                Vector3 translate = Camera._transform.Translation;
                Vector2 newPos = new Vector2((b.MapPosition.X - translate.X) / Scale, (b.MapPosition.Y - translate.Y) / Scale);
                _dictEntrance.Add(b.PersonalID.ToString(), b.BoxToExit); //TODO: FIX THIS
                GraphicCursor.DropBuilding();
                if (!_liBuildings.Contains(b)) {
                    _liBuildings.Add(b);
                } //For the use case of moving buildings
                PlayerManager.AddBuilding(b);
                if (openNameInput)
                {
                    GUIManager.SetScreen(new TextInputScreen(b));
                }
                rv = true;
            }

            return rv;
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
                        //Scry(false);
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

        public void PlacePlayerObject(WorldObject obj)
        {
            RHTile tile = _tileArray[(int)obj.MapPosition.X / TileSize, (int)obj.MapPosition.Y / TileSize];
            tile.SetWorldObject(obj);
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
                m.Position = position;

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
                    PlaceWorldObject(ObjectManager.GetWorldObject(w.worldObjectID, new Vector2(w.x, w.y)));
                }
                else
                {
                    int i = 0;
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

        public bool Contains(NPC n)
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
                    MapManager.DropWorldItems(DropManager.DropItemsFromWorldObject(_obj.ID), _obj.CollisionBox.Center.ToVector2());
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

            if(CharacterManager.DiNPC[_iShopID].CurrentMapName == _sMap)
            {
                if (MapManager.RetrieveTile(_iShopX, _iShopY).Contains(CharacterManager.DiNPC[_iShopID]))
                {
                    rv = true;
                }
            }

            return rv;
        }

        internal void Talk()
        {
            ((ShopKeeper)CharacterManager.DiNPC[_iShopID]).Talk(true);
        }
    }
}
