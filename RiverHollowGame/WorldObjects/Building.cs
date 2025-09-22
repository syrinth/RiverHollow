using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.WorldObjects;
using RiverHollow.Utilities;
using RiverHollow.Misc;
using System.Linq;
using static RiverHollow.Game_Managers.SaveManager;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;
using System;

namespace RiverHollow.Buildings
{
    public class Building : Buildable
    {
        public int Level { get; private set; } = 1;
        private Rectangle Entrance => GetRectangleByIDKey("Entrance");

        public string InnerMapName => "map" + GetStringByIDKey("Texture");
        public RHMap InnerMap => MapManager.Maps[InnerMapName];

        public Rectangle SelectionBox => new Rectangle(MapPosition.X, MapPosition.Y, Sprite.Width, Sprite.Height);
        private Rectangle _rShadowTarget;
        private Rectangle _rShadowSource;

        public bool Producer => GetBoolByIDKey("Producer");


        public Rectangle TravelBox { get; private set; }

        private int MaxAnimals => GetIntByIDKey("MaxAnimals", 0);

        public bool UpgradeQueued { get; private set; } = false;

        public Item[,] Stash;
        public Item[,] Merchandise;

        public Machine StoreMachine { get; private set; }

        public Building(int id) : base(id)
        {
            ID = id;

            Stash = new Item[5, 5];
            Merchandise = new Item[5, 5];
            Unique = true;
            OutsideOnly = true;

            LoadSprite();
        }
        protected override void LoadSprite()
        {
            int startX = 0;
            int startY = 0;

            Sprite = new AnimatedSprite(DataManager.FOLDER_BUILDINGS + GetStringByIDKey("Texture"));
            int maxLevel = GetAllUpgrades().Length > 0 ? GetAllUpgrades().Length : 1;
            for (int i = 1; i <= maxLevel; i++)
            {
                Sprite.AddAnimation(i.ToString(), startX, startY, _pSize);
                startX += _pSize.X * Constants.TILE_SIZE;
            }
            Sprite.PlayAnimation("1");

            _rShadowSource = new Rectangle(0, Sprite.Height, Sprite.Width, Constants.BUILDING_SHADOW_HEIGHT);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (GameManager.HeldObject != this)
            {
                spriteBatch.Draw(Sprite.Texture, _rShadowTarget, _rShadowSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, 1);
            }
        }

        public override bool ProcessLeftClick() { return true; }

        public override void Rollover()
        {
            base.Rollover();
            if (UpgradeQueued)
            {
                Upgrade();
            }

            //ToDo: Re-implement actual production number
            if (Producer)
            {
                ProduceItem();
            }

            InnerMap.AssignMerchandise();
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
            int startX = (int)MapPosition.X + (Entrance.X * Constants.TILE_SIZE);
            int startY = (int)MapPosition.Y + (Entrance.Y * Constants.TILE_SIZE);

            //Create the entrance and exit rectangles attached to the building
            TravelBox = new Rectangle(startX, startY, Entrance.Width * Constants.TILE_SIZE, Entrance.Height * Constants.TILE_SIZE);
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
                MapName = map.Name;
                map.CreateBuildingEntrance(this);

                SyncLightPositions();
                map.AddLights(GetLights());

                _rShadowTarget = _rShadowSource;
                _rShadowTarget.Location = Sprite.Position;
                _rShadowTarget.Offset(0, Sprite.Height - Constants.TILE_SIZE);
            }

            InnerMap.PopulateMap(true);

            if (InnerMap.GetMapProperties().ContainsKey("Pantry"))
            {
                TownManager.SetPantry(Merchandise);
            }

            return rv;
        }

        public void SetStoreMachine(Machine m)
        {
            StoreMachine = m;
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

        public override float GetTownScore()
        {
            float rv = base.GetTownScore();

            foreach (var upgrade in GetUnlockedUpgrades())
            {
                rv += upgrade.TownScore;
            }
            return rv;
        }
        public Dictionary<RarityEnum, List<int>> GetProductionDictionary()
        {
            var rv = new Dictionary<RarityEnum, List<int>>();

            string makes = GetStringByIDKey("Makes");
            if (!string.IsNullOrEmpty(makes))
            {
                //Read in what items the building can make
                string[] split = Util.FindParams(makes);
                foreach (string s in split)
                {
                    int resourceID = -1;
                    RarityEnum rarity = RarityEnum.C;
                    Util.GetRarity(s, ref resourceID, ref rarity);

                    Util.AddToListDictionary(ref rv, rarity, resourceID);
                }
            }

            return rv;
        }

        private void ProduceItem()
        {
            InventoryManager.InitExtraInventory(Merchandise);

            int craftsLeft = GetDailyProduction();
            var productionDictionary = GetProductionDictionary();
            while (craftsLeft > 0)
            {
                var rolledItem = Util.RollOnRarityTable(productionDictionary);
                if (InventoryManager.HasSpaceInInventory(rolledItem, 1, false))
                {
                    var item = DataManager.CraftItem(rolledItem);
                    InventoryManager.AddToInventory(item, false, true);
                    TaskManager.AttemptProgressCraft(item);
                    TownManager.AddToCodex(item.ID);
                    craftsLeft--;

                    TownManager.AddMerchandise(rolledItem, ID);
                }
                else { break; }
            }

            InventoryManager.ClearExtraInventory();
        }

        public int GetDailyProduction()
        {
            int rv = GetIntByIDKey("CraftAmount");
            foreach (var upgrade in GetUnlockedUpgrades())
            {
                rv += upgrade.CraftAmount;
            }

            List<RHTile> nearTiles = new List<RHTile>();
            var synergies = GetStringParamsByIDKey("Synergy");

            var myTiles = Tiles();
            var cornerTiles = new List<RHTile>
            {
                myTiles[0],
                CurrentMap.GetTileByGridCoords(myTiles[0].X + BaseWidth -1, myTiles[0].Y),
                CurrentMap.GetTileByGridCoords(myTiles[0].X, myTiles[0].Y + BaseHeight -1),
                CurrentMap.GetTileByGridCoords(myTiles[0].X + BaseWidth -1, myTiles[0].Y + BaseHeight -1)
            };

            int nearDistance = 4;
            int initialX = myTiles[0].X - nearDistance;
            int endX = initialX + BaseWidth + (nearDistance * 2);
            int initialY = myTiles[0].Y - nearDistance;
            int endY = initialY + BaseHeight + (nearDistance * 2);

            Tuple<int, int> xRange = new Tuple<int, int>(myTiles[0].X, myTiles[0].X + BaseWidth - 1);
            Tuple<int, int> yRange = new Tuple<int, int>(myTiles[0].Y, myTiles[0].Y + BaseHeight - 1);
            for (int i = initialX; i < endX; i++)
            {
                for (int j = initialY; j < endY; j++)
                {
                    var tile = CurrentMap.GetTileByGridCoords(i, j);
                    foreach (var cornerTile in cornerTiles)
                    {
                        //Works for corners but not the main base
                        if (tile.X >= xRange.Item1 && tile.X <= xRange.Item2 ||
                            tile.Y >= yRange.Item1 && tile.Y <= yRange.Item2 ||
                            Util.GetRHTileDelta(tile, cornerTile) <= nearDistance)
                        {
                            nearTiles.Add(tile);
                            break;
                        }
                    }
                }
            }

            //Now that we know all the nearby tiles for the Building, check for valid synergies
            foreach (var s in synergies)
            {
                var split = Util.FindArguments(s);
                if (int.TryParse(split[0], out int objID))
                {
                    var targetObj = TownManager.GetTownObjectsByID(objID);
                    foreach (var obj in targetObj)
                    {
                        foreach (var tile in obj.Tiles())
                        {
                            if (nearTiles.Contains(tile))
                            {
                                rv++;
                                break;
                            }
                        }
                    }
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
            UpgradeQueued = true;
        }
        private void Upgrade()
        {
            string initialLevel = MapName;
            if (!MaxLevel())
            {
                Level++;

                MapManager.Maps[InnerMapName].UpdateBuildingEntrance(initialLevel, MapName);
                MapManager.Maps[MapName].UpgradeMap(Level);

                //_sprite.PlayAnimation(Level.ToString());
            }
            UpgradeQueued = false;
        }
        #endregion

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();

            data.stringData += string.Format("[Level:{0}]", Level.ToString());

            data.stringData += "[Stash:";
            foreach (Item i in Stash)
            {
                data.stringData += string.Format("{0}/", Item.SaveItemToString(i));
            }
            data.stringData = data.stringData.Remove(data.stringData.Length - 1);
            data.stringData += "]";

            data.stringData += "[Merchandise:";
            foreach (Item i in Merchandise)
            {
                data.stringData += string.Format("{0}/", Item.SaveItemToString(i));
            }
            data.stringData = data.stringData.Remove(data.stringData.Length - 1);
            data.stringData += "]";

            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);

            var dataDictionary = Util.DictionaryFromTaggedString(data.stringData);
            Level = Util.ParseInt(dataDictionary["Level"]);

            string[] stashData = Util.FindParams(dataDictionary["Stash"]);
            if (stashData.Length > 0)
            {
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        int index = Util.ListIndexFromMultiArray(i, j, 5);
                        if (!string.IsNullOrEmpty(stashData[index]))
                        {
                            string[] itemData = Util.FindArguments(stashData[index]);
                            Item newItem = DataManager.GetItem(int.Parse(itemData[0]), int.Parse(itemData[1]));
                            if (newItem != null && itemData.Length > 2) { newItem.ApplyUniqueData(itemData[2]); }

                            InventoryManager.InitExtraInventory(this.Stash);
                            InventoryManager.AddItemToInventorySpot(newItem, i, j, false);
                            InventoryManager.ClearExtraInventory();
                        }
                    }
                }
            }

            string[] merchData = Util.FindParams(dataDictionary["Merchandise"]);
            if (merchData.Length > 0)
            {
                InventoryManager.InitExtraInventory(this.Merchandise);
                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        int index = Util.ListIndexFromMultiArray(i, j, 5);
                        if (!string.IsNullOrEmpty(merchData[index]))
                        {
                            string[] itemData = Util.FindArguments(merchData[index]);
                            Item newItem = DataManager.GetItem(int.Parse(itemData[0]), int.Parse(itemData[1]));
                            if (newItem != null && itemData.Length > 2) { newItem.ApplyUniqueData(itemData[2]); }

                            InventoryManager.AddItemToInventorySpot(newItem, i, j, false);
                            TownManager.AddMerchandise(newItem.ID, ID);
                        }
                    }
                }
                InventoryManager.ClearExtraInventory();
            }
        }
    }
}
