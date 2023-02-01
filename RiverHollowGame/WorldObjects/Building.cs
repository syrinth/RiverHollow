using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.WorldObjects;
using static RiverHollow.Game_Managers.SaveManager;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Misc;
using System.Linq;
using RiverHollow.GUIComponents.Screens.HUDScreens;
using RiverHollow.Items;

namespace RiverHollow.Buildings
{
    public class Building : Buildable
    {
        public MerchType StoreType => DataManager.GetEnumByIDKey<MerchType>(ID, "ShopSells", DataType.WorldObject);

        private Rectangle _rEntrance;
        public Item[,] Inventory { get; }

        public int Income { get; private set; } = 1;
        public int Level { get; private set; } = 1;

        public string Description => DataManager.GetTextData(ID, "Description", DataType.WorldObject);

        public string BuildingMapName => "map" + DataManager.GetStringByIDKey(ID, "Texture", DataType.WorldObject);

        public Rectangle SelectionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _sprite.Width, _sprite.Height);

        public Rectangle TravelBox { get; private set; }

        private int MaxAnimals => DataManager.GetIntByIDKey(ID, "MaxAnimals", DataType.WorldObject, 0);

        public Building(int id, Dictionary<string, string> stringData) : base(id)
        {
            ID = id;
            _eObjectType = ObjectTypeEnum.Building;

            Inventory = new Item[Constants.BUILDING_STOCK_SIZE, Constants.BUILDING_STOCK_SIZE];

            Unique = true;
            OutsideOnly = true;

            //The dimensions of the Building in tiles
            Util.AssignValue(ref _uSize, "Size", stringData);

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
                    info.Offset = new Vector2(int.Parse(split[1]), int.Parse(split[2]));

                    SyncLightPositions();
                    _liLights.Add(info);
                }
            }

            LoadSprite(stringData, DataManager.FOLDER_BUILDINGS + DataManager.GetStringByIDKey(ID, "Texture", DataType.WorldObject));
        }

        public override bool ProcessLeftClick()
        {
            GUIManager.OpenMainObject(new HUDBuildingUpgrade(this));
            return true;
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = "Textures\\worldObjects")
        {
            int startX = 0;
            int startY = 0;

            _sprite = new AnimatedSprite(textureName);
            int maxLevel = GetAllUpgrades().Length > 0 ? GetAllUpgrades().Length : 1;
            for (int i = 1; i <= maxLevel; i++)
            {
                _sprite.AddAnimation(i.ToString(), startX, startY, _uSize);
                startX += _uSize.Width * Constants.TILE_SIZE;
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
            int startX = (int)_vMapPosition.X + (_rEntrance.X * Constants.TILE_SIZE);
            int startY = (int)_vMapPosition.Y + (_rEntrance.Y * Constants.TILE_SIZE);

            //Create the entrance and exit rectangles attached to the building
            TravelBox = new Rectangle(startX, startY, _rEntrance.Width * Constants.TILE_SIZE, _rEntrance.Height * Constants.TILE_SIZE);
        }

        public override bool PlaceOnMap(Vector2 pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = false;

            pos = new Vector2(pos.X - (_rBase.X * Constants.TILE_SIZE), pos.Y - (_rBase.Y * Constants.TILE_SIZE));
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

        public int CalculateIncome()
        {
            int rv = 0;

            InventoryManager.InitExtraInventory(Inventory);
            List<Item> items = new List<Item>();
            for (int row = 0; row < Constants.BUILDING_STOCK_SIZE; row++)
            {
                for (int column = 0; column < Constants.BUILDING_STOCK_SIZE; column++)
                {
                    Item i = Inventory[row, column];
                    if (i != null)
                    {
                        if (i.MerchType == StoreType)
                        {
                            items.Add(i);
                        }
                    }
                }
            }

            if (items.Count > 0)
            {
                for (int i = 0; i < 2; i++)
                {
                    Item chosenItem = items[RHRandom.Instance().Next(0, items.Count - 1)];
                    if (chosenItem.Remove(1, false))
                    {
                        rv += chosenItem.Value;
                    }
                }

                int profit = 0;
                Upgrade[] unlockedUpgrades = GetUnlockedUpgrades();
                for (int i = 0; i < unlockedUpgrades.Length; i++)
                {
                    if (unlockedUpgrades[i].Profit != -1)
                    {
                        profit += unlockedUpgrades[i].Profit;
                    }
                }

                if (profit > 0)
                {
                    rv += rv * profit / 100;
                }
            }

            InventoryManager.ClearExtraInventory();

            return rv;
        }

        public void AddToStock(Item i)
        {
            InventoryManager.InitExtraInventory(Inventory);
            InventoryManager.AddToInventory(i, false);
            InventoryManager.ClearExtraInventory();
        }

        #region Upgrade Handlers
        public bool MaxLevel()
        {
            return Level <= GetAllUpgrades().Length;
        }
        public Upgrade[] GetAllUpgrades()
        {
            int[] upgradeIDs = Util.FindIntArguments(DataManager.GetStringByIDKey(ID, "UpgradeID", DataType.WorldObject));
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
        public void Upgrade()
        {
            string initialLevel = MapName;
            if (MaxLevel())
            {
                Level++;
            //    _sprite.PlayAnimation(Level.ToString());
            }

            MapManager.Maps[BuildingMapName].UpdateBuildingEntrance(initialLevel, MapName);
            MapManager.Maps[MapName].UpgradeMap(Level);

            //_sprite.PlayAnimation(Level.ToString());
        }
        #endregion
        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData = Level.ToString();

            foreach (Item i in (this.Inventory))
            {
                if (i == null) { data.stringData += "|null"; }
                else { data.stringData += "|" + Item.SaveItemToString(i); }
            }

            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            string[] strData = Util.FindParams(data.stringData);

            Level = Util.ParseInt(strData[0]);

            int index = 1;
            if (strData.Length > 1)
            {
                for (int row = 0; row < Constants.BUILDING_STOCK_SIZE; row++)
                {
                    for (int column = 0; column < Constants.BUILDING_STOCK_SIZE; column++)
                    {
                        if (!string.Equals(strData[index], "null"))
                        {
                            string[] itemData = Util.FindArguments(strData[index]);
                            Inventory[column, row] = DataManager.GetItem(int.Parse(itemData[0]), int.Parse(itemData[1]));
                        }
                        index++;
                    }
                }
            }
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

            _sDescription = DataManager.GetTextData(_iID, "Description", DataType.WorldObject);

            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);

            Util.AssignValue(ref _bUnlocked, "Unlocked", stringData);
        }

        public string Name()
        {
            return DataManager.GetTextData(_iID, "Name", DataType.WorldObject);
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
