using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.WorldObjects;
using RiverHollow.Utilities;
using RiverHollow.Misc;
using System.Linq;
using RiverHollow.GUIComponents.Screens.HUDWindows;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Buildings
{
    public class Building : Buildable
    {
        private Rectangle _rEntrance;

        public int Level { get; private set; } = 1;

        public string Description => GetTextData("Description");

        public string BuildingMapName => "map" + GetStringByIDKey("Texture");

        public Rectangle SelectionBox => new Rectangle(MapPosition.X, MapPosition.Y, Sprite.Width, Sprite.Height);

        public Rectangle TravelBox { get; private set; }

        private int MaxAnimals => GetIntByIDKey("MaxAnimals", 0);

        public bool UpgradeQueued { get; private set; } = false;

        public Building(int id, Dictionary<string, string> stringData) : base(id)
        {
            ID = id;
            _eObjectType = ObjectTypeEnum.Building;

            Unique = true;
            OutsideOnly = true;

            //The dimensions of the Building in tiles
            Util.AssignValue(ref _pSize, "Size", stringData);

            Util.AssignValue(ref _rBase, "Base", stringData);
            Util.AssignValue(ref _rEntrance, "Entrance", stringData);

            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);

            if (stringData.ContainsKey("LightID"))
            {
                _liLights = new List<LightInfo>();

                foreach (string s in Util.FindParams(stringData["LightID"]))
                {
                    string[] split = s.Split('-');

                    LightInfo info;
                    info.LightObject = DataManager.GetLight(int.Parse(split[0]));
                    info.Offset = new Point(int.Parse(split[1]), int.Parse(split[2]));

                    SyncLightPositions();
                    _liLights.Add(info);
                }
            }

            LoadSprite(stringData, DataManager.FOLDER_BUILDINGS + GetStringByIDKey("Texture"));
        }

        public override bool ProcessLeftClick()
        {
            
            return true;
        }

        public override void Rollover()
        {
            base.Rollover();
            if (UpgradeQueued)
            {
                Upgrade();
            }
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = "Textures\\worldObjects")
        {
            int startX = 0;
            int startY = 0;

            Sprite = new AnimatedSprite(textureName);
            int maxLevel = GetAllUpgrades().Length > 0 ? GetAllUpgrades().Length : 1;
            for (int i = 1; i <= maxLevel; i++)
            {
                Sprite.AddAnimation(i.ToString(), startX, startY, _pSize);
                startX += _pSize.X * Constants.TILE_SIZE;
            }
            Sprite.PlayAnimation("1");
        }

        /// <summary>
        /// Sets up the map position of the map based off of its screen position. Called
        /// when placing the Building onto the map.
        /// </summary>
        /// <param name="position"></param>
        public override void SnapPositionToGrid(Point position)
        {
            //Set the top-left corner of the building position 
            base.SnapPositionToGrid(position);

            //Determine where the top-left corner of the entrance Rectangle should be
            int startX = (int)MapPosition.X + (_rEntrance.X * Constants.TILE_SIZE);
            int startY = (int)MapPosition.Y + (_rEntrance.Y * Constants.TILE_SIZE);

            //Create the entrance and exit rectangles attached to the building
            TravelBox = new Rectangle(startX, startY, _rEntrance.Width * Constants.TILE_SIZE, _rEntrance.Height * Constants.TILE_SIZE);
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = false;

            pos = new Point(pos.X - (_rBase.X * Constants.TILE_SIZE), pos.Y - (_rBase.Y * Constants.TILE_SIZE));
            SnapPositionToGrid(pos);

            List<RHTile> tiles = new List<RHTile>();
            if (map.TestMapTiles(this, tiles))
            {
                rv = true;
                map.AssignMapTiles(this, tiles);
                map.AddBuilding(this);
                MapName = map.Name;
                map.CreateBuildingEntrance(this);

                SyncLightPositions();
                map.AddLights(GetLights());

                TownManager.AddToTownObjects(this);
            }

            return rv;
        }

        public int GetTravelerChance()
        {
            int rv = GetIntByIDKey("Traveller");
            foreach (var upgrade in GetUnlockedUpgrades())
            {
                rv += upgrade.Chance;
            }
            return rv;
        }

        public float GetShopProfitModifier()
        {
            float rv = GetFloatByIDKey("Profit", 0);
            foreach(var upgrade in GetUnlockedUpgrades())
            {
                rv += upgrade.Profit;
            }
            return rv / 100;
        }

        public int GetFormulaLevel()
        {
            int rv = 1;
            foreach (var upgrade in GetUnlockedUpgrades())
            {
                if(upgrade.FormulaLevel > rv)
                {
                    rv = upgrade.FormulaLevel;
                }
            }
            return rv;
        }

        #region Upgrade Handlers
        public bool MaxLevel()
        {
            return Level == GetAllUpgrades().Length + 1;
        }
        public Upgrade[] GetAllUpgrades()
        {
            int[] upgradeIDs = Util.FindIntArguments(GetStringByIDKey("UpgradeID"));
            Upgrade[] allUpgrades = new Upgrade[upgradeIDs.Length];
            for (int i = 0; i < upgradeIDs.Length; i++)
            {
                allUpgrades[i] = new Upgrade(upgradeIDs[i]);
            }

            return allUpgrades;
        }
        public Upgrade[] GetUnlockedUpgrades()
        {
            return GetAllUpgrades().Take(Level - 1).ToArray();
        }

        public Dictionary<int, int> UpgradeReqs()
        {
            Upgrade[] upgrades = GetAllUpgrades();
            if (!MaxLevel())
            {
                return upgrades[Level - 1].UpgradeRequirements;
            }

            return null;
        }

        public void QueueUpgrade()
        {
            TownManager.IncreaseTravelerBonus();
            UpgradeQueued = true;
        }
        private void Upgrade()
        {
            string initialLevel = MapName;
            if (!MaxLevel())
            {
                Level++;

                MapManager.Maps[BuildingMapName].UpdateBuildingEntrance(initialLevel, MapName);
                MapManager.Maps[MapName].UpgradeMap(Level);

                //_sprite.PlayAnimation(Level.ToString());
            }
            UpgradeQueued = false;
        }
        #endregion

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData = Level.ToString();

            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            string[] strData = Util.FindParams(data.stringData);

            Level = Util.ParseInt(strData[0]);
        }
    }
}
