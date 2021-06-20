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

namespace RiverHollow.Buildings
{
    public class Building : Structure
    {
        private int _iNPCBuilderID;
        private int _iEntX;
        private int _iEntY;
        private int _iEntWidth;
        private int _iEntHeight;
        public int Level { get; private set; } = 1;

        private Dictionary<int, Dictionary<int, int>> _diUpgradeInfo;

        private string _sDescription;
        public string Description => _sDescription;

        private string _sTextureName;

        private string _sBuildingMap;
        public new string MapName => "map" + _sTextureName.Replace(" ", "") + "_1";// + (Level == 0 ? "" : Level.ToString());

        public Rectangle SelectionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _sprite.Width, _sprite.Height);

        public Rectangle TravelBox { get; private set; }

        public Vector2 BuildFromPosition { get; private set; }

        public bool _bSelected = false;
        public Container BuildingChest { get; set; }

        public Building(int id, Dictionary<string, string> stringData) : base(id)
        {
            ImportBasics(id, stringData);
        }

        private void ImportBasics(int id, Dictionary<string, string> stringData)
        {
            _iID = id;
            _eObjectType = ObjectTypeEnum.Building;

            DataManager.GetTextData("Building", _iID, ref _sName, "Name");
            DataManager.GetTextData("Building", _iID, ref _sDescription, "Description");

            //The dimensions of the Building in tiles
            Util.AssignValues(ref _iSpriteWidth, ref _iSpriteHeight, "Dimensions", stringData);

            Util.AssignValues(ref _iBaseXOffset, ref _iBaseYOffset, "BaseOffset", stringData);
            Util.AssignValues(ref _iBaseWidth, ref _iBaseHeight, "Base", stringData);

            //The rectangle, in pixels, that forms the entrance to the building
            string[] ent = stringData["Entrance"].Split('-');
            _iEntX = int.Parse(ent[0]);
            _iEntY = int.Parse(ent[1]);
            _iEntWidth = int.Parse(ent[2]);
            _iEntHeight = int.Parse(ent[3]);

            _diUpgradeInfo = new Dictionary<int, Dictionary<int, int>>();
            foreach (string s in new List<string>(stringData.Keys).FindAll(x => x.StartsWith("Upgrade_")))
            {
                int upgradeLevel = int.Parse(s.Substring(s.IndexOf("_") + 1));
                string val = stringData[s];

                Dictionary<int, int> reqs = new Dictionary<int, int>();
                foreach (string arg in Util.FindParams(val))
                {
                    string[] split = arg.Split('-');
                    reqs[int.Parse(split[0])] =  int.Parse(split[1]);
                }

                //Upgrade 1 is actually Level 2, so increment
                _diUpgradeInfo[upgradeLevel + 1] = reqs;
            }

            Util.AssignValue(ref _iNPCBuilderID, "Builder", stringData);

            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);

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
                _sprite.AddAnimation(i.ToString(), startX, startY, _iSpriteWidth, _iSpriteHeight);
                startX += _iSpriteWidth;
            }
            _sprite.PlayAnimation("1");
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
                _sprite.SetColor(_bSelected ? Color.Green : Color.White);
                base.Draw(spriteBatch);
            }
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

        public override void Rollover()
        {
        }

        public override void PlaceOnMap(Vector2 pos, RHMap map)
        {
            SetMapName(map.Name);
            if (map.TestMapTiles(this, Tiles))
            {
                map.AssignMapTiles(this, Tiles);
            }
            map.CreateBuildingEntrance(this);

            map.AddBuilding(this);
            PlayerManager.AddBuilding(this);
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
        //public void StartBuilding(bool startAtZero = true)
        //{
        //    RHMap buildingMap = MapManager.Maps[_sBuildingMap];
        //    foreach (RHTile t in Tiles)
        //    {
        //        WorldObject w = t.WorldObject;
        //        if (w != null)
        //        {
        //            buildingMap.RemoveWorldObject(w);

        //        }
        //        w = t.Flooring;
        //        if (w != null)
        //        {
        //            buildingMap.RemoveWorldObject(w);
        //        }
        //    }
        //    buildingMap.AssignMapTiles(this, Tiles);
        //    buildingMap.CreateBuildingEntrance(this);
        //    //if (startAtZero)
        //    //{
        //    //    _iBldgLvl = 0;

        //    //    Vector2 startAt = new Vector2(CollisionBox.X, CollisionBox.Y - TileSize);

        //    //    for (int x = (int)startAt.X; x < startAt.X + CollisionBox.Width; x += TileSize)
        //    //    {
        //    //        for (int y = (int)startAt.Y + TileSize; y < startAt.Y + TileSize + CollisionBox.Height; y += TileSize)
        //    //        {
        //    //            Floor obj = (Floor)DataManager.GetWorldObject(int.Parse(DataManager.Config[9]["Floor"]));
        //    //            obj.SetMapName(MapManager.CurrentMap.Name);
        //    //            obj.SnapPositionToGrid(new Vector2(x, y));
        //    //            MapManager.CurrentMap.TestMapTiles(obj);
        //    //            if (MapManager.PlacePlayerObject(obj))
        //    //            {
        //    //                obj.AdjustObject();
        //    //            }
        //    //        }
        //    //    }

        //    //    for (int x = (int)startAt.X; x < startAt.X + CollisionBox.Width; x += TileSize)
        //    //    {
        //    //        PlaceWall(new Vector2(x, startAt.Y));
        //    //    }

        //    //    for (int x = (int)startAt.X; x < startAt.X + CollisionBox.Width; x += TileSize)
        //    //    {
        //    //        if (!_rEntrance.Contains(new Vector2(x, startAt.Y + CollisionBox.Height - TileSize)))
        //    //        {
        //    //            PlaceWall(new Vector2(x, startAt.Y + CollisionBox.Height - TileSize));
        //    //        }
        //    //    }

        //    //    for (int y = (int)startAt.Y; y < startAt.Y + CollisionBox.Height; y += TileSize)
        //    //    {
        //    //        PlaceWall(new Vector2(startAt.X, y));
        //    //    }

        //    //    for (int y = (int)startAt.Y; y < startAt.Y + CollisionBox.Height; y += TileSize)
        //    //    {
        //    //        PlaceWall(new Vector2(startAt.X + CollisionBox.Width - TileSize, y));
        //    //    }
        //    //}
        //    //_iUpgradeTimer = _iUpgradeTime + 1;
        //    //_sprite.PlayAnimation(_iBldgLvl.ToString());

        //    //DataManager.DiNPC[_iNPCBuilderID].SetBuildTarget(this);
        //}

        /// <summary>
        /// Helper method for when we start building to place a wall
        /// at the given location in the buildings collision box.
        /// </summary>
        /// <param name="location">The map location to create the wall.</param>
        private void PlaceWall(Vector2 location)
        {
            DataManager.CreateAndPlaceNewWorldObject(int.Parse(DataManager.Config[9]["Wall"]), location, MapManager.CurrentMap);
        }

        public Dictionary<int, int> UpgradeReqs()
        {
            return _diUpgradeInfo[Level + 1];
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
            RHMap buildingMap = MapManager.Maps[_sBuildingMap];
            if (Level == 0)
            {
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

            MapManager.Maps[MapName].UpgradeMap(Level);

            //_sprite.PlayAnimation(Level.ToString());
        }

        /// <summary>
        /// Sets the name of the map the building lives on
        /// </summary>
        /// <param name="name"></param>
        public void SetHomeMap(string name)
        {
            _sBuildingMap = name;
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
            };

            return buildingData;
        }
        public void LoadData(BuildingData data)
        {
            SnapPositionToGrid(new Vector2(data.iPosX, data.iPosY));
            Level = data.iBldgLevel;
        }
    }

    /// <summary>
    /// Represents surface level information needed for buildings so we can avoid
    /// maintaining a complete list of all Building structures that aren't required.
    /// </summary>
    public class BuildInfo
    {
        private Dictionary<int, int> _diReqToMake;
        public Dictionary<int, int> RequiredToMake => _diReqToMake;

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

            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);

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
