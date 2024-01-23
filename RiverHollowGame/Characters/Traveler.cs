using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Traveler : TalkingActor
    {
        public int FoodID { get; private set;} = -1;
        public int ItemID { get; private set;} = -1;
        public int Income { get; private set; } = 0;

        public int BuildingID()
        {
            return GetIntByIDKey("Building");
        }

        private int NPC()
        {
            return GetIntByIDKey("NPC");
        }

        public bool Rare()
        {
            return GetBoolByIDKey("Rare");
        }

        protected override string SpriteName()
        {
            return DataManager.TRAVELER_FOLDER + GetStringByIDKey("Key");
        }

        public TravelerGroupEnum Group()
        {
            return GetEnumByIDKey<TravelerGroupEnum>("Subtype");
        }

        public FoodTypeEnum FavoriteFood()
        {
            return GetEnumByIDKey<FoodTypeEnum>("FavFood");
        }

        public FoodTypeEnum DislikedFood()
        {
            return GetEnumByIDKey<FoodTypeEnum>("Disliked");
        }

        public Traveler(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _fWanderSpeed = Constants.NPC_WALK_SPEED;
            Wandering = true;
            _eCollisionState = ActorCollisionState.Slow;
        }

        public override TextEntry GetOpeningText()
        {
            TownManager.DITravelerInfo[ID] = new ValueTuple<bool, int>(true, TownManager.DITravelerInfo[ID].Item2);
            return GetDailyDialogue();
        }

        public bool Validate()
        {
            return (BuildingID() == -1 || TownManager.TownObjectBuilt(BuildingID())) &&
                        (NPC() == -1 || TownManager.DIVillagers[NPC()].LivesInTown);
        }

        public bool HasEaten()
        {
            return FoodID != -1;
        }
        public void TryEat(Food f)
        {
            if (!HasEaten() && f.Remove(1, false))
            {
                FoodID = f.ID;
            }
        }

        public void PurchaseItem()
        {
            if (BuildingID() != -1)
            {
                var building = TownManager.GetBuildingByID(BuildingID());
                var map = MapManager.Maps[building.BuildingMapName];

                var containers = map.GetObjectsByType<Container>().Cast<Container>().ToList();
                var shopTables = containers.Where(x => x.GetBoolByIDKey("ShopTable")).ToList();

                var merchTables = new List<Container>();
                foreach (var table in shopTables)
                {
                    foreach (var item in table.Inventory)
                    {
                        if (item != null)
                        {
                            merchTables.Add(table);
                            break;
                        }
                    }
                }

                if (merchTables.Count > 0)
                {
                    var randomTable = Util.GetRandomItem(merchTables);
                    var randomItem = Util.GetRandomItem(Util.MultiArrayToList(randomTable.Inventory));

                    InventoryManager.InitExtraInventory(randomTable.Inventory);
                    randomItem.Remove(1, false);
                    InventoryManager.ClearExtraInventory();
                    ItemID = randomItem.ID;
                }
            }
        }

        public bool NeutralFood(FoodTypeEnum e)
        {
            bool rv = (e != FavoriteFood() && e != DislikedFood());
            return rv;
        }

        public int CalculateIncome()
        {
            CalculateProfit(ItemID, TownManager.GetBuildingByID(BuildingID()));
            CalculateProfit(FoodID, TownManager.Inn);

            return Income;
        }

        private void CalculateProfit(int itemID, Building b)
        {
            if (itemID > -1 && b != null)
            {
                int value = (DataManager.GetIntByIDKey(itemID, "Value", DataType.Item));
                float profitMod = (1 + b.GetShopProfitModifier());
                int profit = (int)(value * profitMod);
                Income += profit;

                TownManager.AddToSoldGoods(itemID, profit);
            }
        }
    }
}
