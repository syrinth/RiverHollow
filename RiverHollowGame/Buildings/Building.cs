using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.WorldObjects;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using RiverHollow.Utilities;
using RiverHollow.GUIComponents.Screens;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Buildings
{
    public class Building : Buildable
    {
        private int _iNPCBuilderID;
        private Rectangle _rEntrance;
        public int Level { get; private set; } = 1;

        private Dictionary<int, Dictionary<int, int>> _diUpgradeInfo;

        public string Description => DataManager.GetTextData("WorldObject", _iID, "Description");

        private string _sTextureName;

        private string _sBuildingMap;
        public new string MapName => DetermineMapName();

        public Rectangle SelectionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _sprite.Width, _sprite.Height);

        public Rectangle TravelBox { get; private set; }

        public Vector2 BuildFromPosition { get; private set; }

        public Container BuildingChest { get; set; }

        public Building(int id, Dictionary<string, string> stringData) : base(id)
        {
            Unique = true;
            OutsideOnly = true;
            ImportBasics(id, stringData);
        }

        public override void ProcessLeftClick()
        {
            if (Level < MAX_BUILDING_LEVEL)
            {
                GUIManager.OpenMainObject(new HUDUpgradeWindow(this));
            }
        }

        private void ImportBasics(int id, Dictionary<string, string> stringData)
        {
            _iID = id;
            _eObjectType = ObjectTypeEnum.Building;

            //The dimensions of the Building in tiles
            Util.AssignValue(ref _uSize, "Size", stringData);

            Util.AssignValue(ref _rBase, "Base", stringData);
            Util.AssignValue(ref _rEntrance, "Entrance", stringData);

            _diUpgradeInfo = new Dictionary<int, Dictionary<int, int>>();
            foreach (string s in new List<string>(stringData.Keys).FindAll(x => x.StartsWith("Upgrade_")))
            {
                int upgradeLevel = int.Parse(s.Substring(s.IndexOf("_") + 1));
                string val = stringData[s];

                Dictionary<int, int> reqs = new Dictionary<int, int>();
                foreach (string arg in Util.FindParams(val))
                {
                    string[] split = arg.Split('-');
                    reqs[int.Parse(split[0])] = int.Parse(split[1]);
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

            if (stringData.ContainsKey("LightID"))
            {
                _liLights = new List<LightInfo>();

                foreach (string s in Util.FindParams(stringData["LightID"]))
                {
                    string[] split = s.Split('-');

                    LightInfo info;
                    info.LightObject = DataManager.GetLight(int.Parse(split[0]));
                    info.Offset = new Vector2(int.Parse(split[1]), int.Parse(split[2]));

                    SyncLightPositions();
                    _liLights.Add(info);
                }
            }

            Util.AssignValue(ref _sTextureName, "Texture", stringData);
            LoadSprite(stringData, DataManager.FOLDER_BUILDINGS + _sTextureName);
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = "Textures\\worldObjects")
        {
            int startX = 0;
            int startY = 0;

            _sprite = new AnimatedSprite(textureName);
            for (int i = 1; i <= MAX_BUILDING_LEVEL; i++)
            {
                _sprite.AddAnimation(i.ToString(), startX, startY, _uSize);
                startX += _uSize.Width * TILE_SIZE;
            }
            _sprite.PlayAnimation("1");
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
            int startX = (int)_vMapPosition.X + (_rEntrance.X * TILE_SIZE);
            int startY = (int)_vMapPosition.Y + (_rEntrance.Y * TILE_SIZE);

            //Create the entrance and exit rectangles attached to the building
            TravelBox = new Rectangle(startX, startY, _rEntrance.Width * TILE_SIZE, _rEntrance.Height * TILE_SIZE);
        }

        public override bool PlaceOnMap(Vector2 pos, RHMap map)
        {
            bool rv = false;

            pos = new Vector2(pos.X - (_rBase.X * TILE_SIZE), pos.Y - (_rBase.Y * TILE_SIZE));
            SnapPositionToGrid(pos);

            if (map.TestMapTiles(this, Tiles))
            {
                rv = true;
                map.AssignMapTiles(this, Tiles);
                map.CreateBuildingEntrance(this);
                map.AddBuilding(this);

                SyncLightPositions();
                map.AddLights(GetLights());
            }

            return rv;
        }

        public Dictionary<int, int> UpgradeReqs()
        {
            if (_diUpgradeInfo.Count > 0 && _diUpgradeInfo.ContainsKey(Level + 1)) { return _diUpgradeInfo[Level + 1]; }
            else { return null; }
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
            string initialLevel = MapName;
            if (Level + 1 <= MAX_BUILDING_LEVEL)
            {
                Level++;
                _sprite.PlayAnimation(Level.ToString());
            }

            MapManager.Maps[_sBuildingMap].UpdateBuildingEntrance(initialLevel, MapName);
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

        private string DetermineMapName()
        {
            string rv = "map" + _sTextureName;

            if(Level > 1)
            {
                string append = "_" + Level.ToString();
                if(MapManager.Maps.ContainsKey(rv + append))
                {
                    rv = rv + append;
                }
            }

            return rv;
        }

        public BuildingData SaveData()
        {
            BuildingData buildingData = new BuildingData
            {
                iBldgLevel = this.Level,
                iBuildingID = this.ID,
                iPosX = (int)this.CollisionBox.X,
                iPosY = (int)this.CollisionBox.Y,
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

            _sDescription = DataManager.GetTextData("Building", _iID, "Description");

            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);

            Util.AssignValue(ref _bUnlocked, "Unlocked", stringData);
        }

        public string Name()
        {
            return DataManager.GetTextData("Building", _iID, "Name");
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
