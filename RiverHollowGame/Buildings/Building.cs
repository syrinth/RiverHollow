using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Items;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Items.Structure;
using RiverHollow.Utilities;
using static RiverHollow.Items.Structure.AdjustableObject;

namespace RiverHollow.Buildings
{
    public class Building : WorldObject
    {
        private int _iNPCBuilderID;
        private int _iEntX;
        private int _iEntY;
        private int _iEntWidth;
        private int _iEntHeight;
        private int _iBaseX;
        private int _iBaseY;
        public override int BaseWidth => _iBaseWidth;
        public override int BaseHeight => _iBaseHeight;
        public int Level { get; private set; } = 1;

        private string _sDescription;
        public string Description => _sDescription;

        private string _sTextureName;

        private string _sHomeMap;
        public new string MapName => "map" + _sTextureName.Replace(" ", "") + "_" + (Level == 0 ? "" : Level.ToString());

        public override Rectangle CollisionBox => GenerateCollisionBox();
        public Rectangle SelectionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _sprite.Width, _sprite.Height);

        public Rectangle TravelBox { get; private set; }

        private int _iUpgradeTime;
        private int _iUpgradeTimer;

        public Vector2 BuildFromPosition { get; private set; }

        public bool _selected = false;
        public Container BuildingChest { get; set; }

        #region Non-Uniqueness
        public List<WorldObject> PlacedObjects { get; }
        public int PersonalID { get; private set; }
        public bool Unique { get; private set; } = true;
        #endregion

        public Building(int id, Dictionary<string, string> stringData)
        {
            PlacedObjects = new List<WorldObject>();
            ImportBasics(id, stringData);
        }

        private void ImportBasics(int id, Dictionary<string, string> stringData)
        {
            _iID = id;
            _eObjectType = ObjectTypeEnum.Building;

            DataManager.GetTextData("Building", _iID, ref _sName, "Name");
            DataManager.GetTextData("Building", _iID, ref _sDescription, "Description");

            //The dimensions of the Building in tiles
            string[] dimensions = stringData["Dimensions"].Split('-');
            _iWidth = int.Parse(dimensions[0]);
            _iHeight = int.Parse(dimensions[1]);

            //Starts at the top-left most tile that forms the base of the building
            if (stringData.ContainsKey("Base"))
            {
                string[] str = stringData["Base"].Split('-');
                _iBaseX = int.Parse(str[0]);
                _iBaseY = int.Parse(str[1]);
                _iBaseWidth = int.Parse(str[2]);
                _iBaseHeight = int.Parse(str[3]);
            }

            //The rectangle, in pixels, that forms the entrance to the building
            string[] ent = stringData["Entrance"].Split('-');
            _iEntX = int.Parse(ent[0]);
            _iEntY = int.Parse(ent[1]);
            _iEntWidth = int.Parse(ent[2]);
            _iEntHeight = int.Parse(ent[3]);

            //The amount of time it takes for a building to change stages
            if (stringData.ContainsKey("UpgradeTime")) { _iUpgradeTime = int.Parse(stringData["UpgradeTime"]); }

            if (stringData.ContainsKey("Builder"))
            {
                _iNPCBuilderID = int.Parse(stringData["Builder"]);
            }

            _diReqToMake = new Dictionary<int, int>();
            if (stringData.ContainsKey("ReqItems"))
            {
                //Split by "|" for each item set required
                string[] split = Util.FindParams(stringData["ReqItems"]);
                foreach (string s in split)
                {
                    string[] splitData = s.Split('-');
                    _diReqToMake[int.Parse(splitData[0])] = int.Parse(splitData[1]);
                }
            }

            //Sets the position from which the Mason will spawn tobuild the building
            if (stringData.ContainsKey("BuildSpot")) {
                string[] split = stringData["BuildSpot"].Split('-');
                BuildFromPosition = new Vector2(int.Parse(split[0]), int.Parse(split[1]));
            }

            //Flag for whether or not this building is unique
            //Unique = stringData.ContainsKey("Unique");

            //PersonalID = PlayerManager.GetNewBuildingID();

            Util.AssignValue(ref _sTextureName, "Texture", stringData);
            LoadSprite(stringData, DataManager.FOLDER_BUILDINGS + _sTextureName);
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = "Textures\\worldObjects")
        {
            int startX = 0;
            int startY = 0;

            _sprite = new AnimatedSprite(textureName);
            for (int i = 1; i <= MaxBldgLevel; i++)
            {
                _sprite.AddAnimation(i.ToString(), startX, startY, _iWidth, _iHeight);
                startX += _iWidth;
            }
            _sprite.PlayAnimation(Level.ToString());
        }

        /// <summary>
        /// Gets the Building to draw itself, the source rectangle needs to move along the x-axis
        /// in order to get the proper building image
        /// </summary>
        /// <param name="spriteBatch"></param>
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Level > 0)
            {
                _sprite.SetColor(_selected ? Color.Green : Color.White);
                base.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// Generates a new Rectangle that represents the area of the building that is collidabke.
        /// </summary>
        public Rectangle GenerateCollisionBox()
        {
            //Start at the top left corner of the building, then move over and
            //down by the number of pixels required to get to the base.
            int startX = (int)_vMapPosition.X + _iBaseX;
            int startY = (int)_vMapPosition.Y + _iBaseY;

            return new Rectangle(startX, startY, BaseWidth, BaseHeight);
        }

        /// <summary>
        /// Sets up the map position of the map based off of its screen position. Called
        /// when placing the Building onto the map.
        /// </summary>
        /// <param name="position"></param>
        public override void SnapPositionToGrid(Vector2 position)
        {
            //Set the top-left corner of the building position 
            base.SnapPositionToGrid(position);

            //Determine where the top-left corner of the entrance Rectangle should be
            int startX = (int)_vMapPosition.X + _iEntX;
            int startY = (int)_vMapPosition.Y + _iEntY;

            //Create the entrance and exit rectangles attached to the building
            TravelBox = new Rectangle(startX, startY, _iEntWidth, _iEntHeight);
        }

        /// <summary>
        /// During rollover, the worker only makes their item if they were not
        /// adventuring that day, and if they are assigned to a production building.
        /// </summary>
        public void Rollover()
        {
            //If we are upgrading the building, get closer to the build count
            if (_iUpgradeTimer > 0) {
                _iUpgradeTimer--;

                //When the timer reaches 0, Upgrade the Building
                if (_iUpgradeTimer == 0)
                {
                    Upgrade();
                }
            }
        }

        /// <summary>
        /// Sets the upgrade timer on the building so we know how long we have
        /// until we need to upgrade it to the next stage.
        /// 
        /// Increment it by one to account for the fact that the upgrade shouldn't
        /// officially start until the day after we call this method.
        /// 
        /// If we are starting at zero, we need to set the building's level as such,
        /// so that we know to not draw the building as well as set up the construction
        /// walls and flooring.
        /// 
        /// Parameter only used when building is first being built, otherwise leave
        /// the original building level.
        /// </summary>
        /// <param name="startAtZero"> Whether to reset the building's value to 0 or not</param>
        public void StartBuilding(bool startAtZero = true)
        {
            RHMap buildingMap = MapManager.Maps[_sHomeMap];
            foreach (RHTile t in Tiles)
            {
                WorldObject w = t.WorldObject;
                if (w != null)
                {
                    buildingMap.RemoveWorldObject(w);

                }
                w = t.Flooring;
                if (w != null)
                {
                    buildingMap.RemoveWorldObject(w);
                }
            }
            buildingMap.AssignMapTiles(this, Tiles);
            buildingMap.CreateBuildingEntrance(this);
            //if (startAtZero)
            //{
            //    _iBldgLvl = 0;

            //    Vector2 startAt = new Vector2(CollisionBox.X, CollisionBox.Y - TileSize);

            //    for (int x = (int)startAt.X; x < startAt.X + CollisionBox.Width; x += TileSize)
            //    {
            //        for (int y = (int)startAt.Y + TileSize; y < startAt.Y + TileSize + CollisionBox.Height; y += TileSize)
            //        {
            //            Floor obj = (Floor)DataManager.GetWorldObject(int.Parse(DataManager.Config[9]["Floor"]));
            //            obj.SetMapName(MapManager.CurrentMap.Name);
            //            obj.SnapPositionToGrid(new Vector2(x, y));
            //            MapManager.CurrentMap.TestMapTiles(obj);
            //            if (MapManager.PlacePlayerObject(obj))
            //            {
            //                obj.AdjustObject();
            //            }
            //        }
            //    }

            //    for (int x = (int)startAt.X; x < startAt.X + CollisionBox.Width; x += TileSize)
            //    {
            //        PlaceWall(new Vector2(x, startAt.Y));
            //    }

            //    for (int x = (int)startAt.X; x < startAt.X + CollisionBox.Width; x += TileSize)
            //    {
            //        if (!_rEntrance.Contains(new Vector2(x, startAt.Y + CollisionBox.Height - TileSize)))
            //        {
            //            PlaceWall(new Vector2(x, startAt.Y + CollisionBox.Height - TileSize));
            //        }
            //    }

            //    for (int y = (int)startAt.Y; y < startAt.Y + CollisionBox.Height; y += TileSize)
            //    {
            //        PlaceWall(new Vector2(startAt.X, y));
            //    }

            //    for (int y = (int)startAt.Y; y < startAt.Y + CollisionBox.Height; y += TileSize)
            //    {
            //        PlaceWall(new Vector2(startAt.X + CollisionBox.Width - TileSize, y));
            //    }
            //}
            //_iUpgradeTimer = _iUpgradeTime + 1;
            //_sprite.PlayAnimation(_iBldgLvl.ToString());

            //DataManager.DiNPC[_iNPCBuilderID].SetBuildTarget(this);
        }

        /// <summary>
        /// Helper method for when we start building to place a wall
        /// at the given location in the buildings collision box.
        /// </summary>
        /// <param name="location">The map location to create the wall.</param>
        private void PlaceWall(Vector2 location)
        {
            Structure obj = (Structure)DataManager.GetWorldObject(int.Parse(DataManager.Config[9]["Wall"]));
            ((Wall)obj).SetMapName(MapManager.CurrentMap.Name);
            obj.SnapPositionToGrid(location);
            MapManager.CurrentMap.TestMapTiles(obj);
            if (MapManager.PlacePlayerObject(obj))
            {
                ((Wall)obj).AdjustObject();
            }
        }

        /// <summary>
        /// Increases the building level as long as it will not exceed the Building's max level.
        /// 
        /// If we are coming off of a level 0 building, we need to clean up the walls that we placed
        /// during construction and actually set the building to the tiles, as well as creating the
        /// entryway so the building map can be accessed.
        /// 
        /// Once we've finished upgrading the building, unset the Mason's build target.
        /// </summary>
        public void Upgrade()
        {
            if (Level == 0)
            {
                RHMap buildingMap = MapManager.Maps[_sHomeMap];
                foreach (RHTile t in Tiles)
                {
                    WorldObject w = t.WorldObject;
                    if (w != null)
                    {
                        buildingMap.RemoveWorldObject(w);

                    }
                    w = t.Flooring;
                    if (w != null)
                    {
                        buildingMap.RemoveWorldObject(w);
                    }
                }
                buildingMap.AssignMapTiles(this, Tiles);
                buildingMap.CreateBuildingEntrance(this);
            }

            if (Level + 1 <= MaxBldgLevel)
            {
                Level++;
            }

            _sprite.PlayAnimation(Level.ToString());
        }

        /// <summary>
        /// Sets the name of the map the building lives on
        /// </summary>
        /// <param name="name"></param>
        public void SetHomeMap(string name)
        {
            _sHomeMap = name;
        }

        /// <summary>
        /// Tells the building which tiles it is sitting on for the base. Required
        /// for the set up and take down of the walls during initial construction.
        /// </summary>
        /// <param name="tiles">List of Tiles on the building's base.</param>
        public void SetTiles(List<RHTile> tiles)
        {
            Tiles = tiles;
        }

        public string TravelLink()
        {
            string rv = string.Empty;
            rv = MapName;
            //if (Unique) { rv = MapName; }
            //else { rv = PersonalID.ToString(); }

            return rv;
        }

        public BuildingData SaveData()
        {
            BuildingData buildingData = new BuildingData
            {
                iBldgLevel = this.Level,
                iBuildingID = this.ID,
                iPosX = (int)this.MapPosition.X,
                iPosY = (int)this.MapPosition.Y,
                iPersonalID = this.PersonalID,
                iUpgradeTimer = this._iUpgradeTimer,
            };

            buildingData.containers = new List<ContainerData>();
            buildingData.machines = new List<MachineData>();
            foreach (WorldObject w in PlacedObjects)
            {
                if (w.CompareType(ObjectTypeEnum.Machine))
                {
                    // buildingData.machines.Add(((Machine)w).SaveData());
                }
                if (w.CompareType(ObjectTypeEnum.Container))
                {
                    buildingData.containers.Add(((Container)w).SaveData());
                }
            }

            return buildingData;
        }
        public void LoadData(BuildingData data)
        {
            SnapPositionToGrid(new Vector2(data.iPosX, data.iPosY));
            PersonalID = data.iPersonalID;
            Level = data.iBldgLevel;
            _iUpgradeTimer = data.iUpgradeTimer;

            foreach (ContainerData c in data.containers)
            {
                Container con = (Container)DataManager.GetWorldObject(c.containerID);
                con.LoadData(c);
                PlacedObjects.Add(con);
            }

            foreach (MachineData mac in data.machines)
            {
                Machine theMachine = (Machine)DataManager.GetWorldObject(mac.ID);
                // theMachine.LoadData(mac);
                PlacedObjects.Add(theMachine);
            }
        }
    }

    /// <summary>
    /// Represents surface level information needed for buildings so we can avoid
    /// maintaining a complete list of all Building structures that aren't required.
    /// </summary>
    public class BuildInfo
    {
        public Dictionary<int, int> RequiredToMake { get; }

        private string _sName;
        public string Name => _sName;

        private string _sDescription;
        public string Description => _sDescription;

        protected int _iID;
        public int ID => _iID;

        private bool _bUnlocked = false;
        public bool Unlocked => _bUnlocked;
        public bool Built { get; set; } = false;

        public BuildInfo(int id, Dictionary<string, string> stringData)
        {
            _iID = id;

            DataManager.GetTextData("Building", _iID, ref _sName, "Name");
            DataManager.GetTextData("Building", _iID, ref _sDescription, "Description");

            RequiredToMake = new Dictionary<int, int>();
            if (stringData.ContainsKey("ReqItems"))
            {
                //Split by "|" for each item set required
                string[] split = Util.FindParams(stringData["ReqItems"]);
                foreach (string s in split)
                {
                    string[] splitData = s.Split('-');
                    RequiredToMake[int.Parse(splitData[0])] = int.Parse(splitData[1]);
                }
            }

            Util.AssignValue(ref _bUnlocked, "Unlocked", stringData);
        }

        public BuildInfoData SaveData()
        {
            BuildInfoData buildInfoData = new BuildInfoData
            {
                id = this.ID,
                built = this.Built,
                unlocked = this.Unlocked
            };

            return buildInfoData;
        }

        public void LoadData(BuildInfoData data)
        {
            Built = data.built;
            if (data.unlocked) { Unlock(); }
        }

        public void Unlock() { _bUnlocked = true; }
    }
}
