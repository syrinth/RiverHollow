using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    using MerchDataDictionary = Dictionary<ClassTypeEnum, List<KeyValuePair<Merchandise, Building>>>;
    using MerchKVP = KeyValuePair<Merchandise, Building>;

    public class Traveler : TalkingActor
    {
        private TravelerNeed _TravelerNeed;
        public bool HasNeed => _TravelerNeed != null;
        public AffinityEnum Affinity => GetEnumByIDKey<AffinityEnum>("SubGroup");
        public ClassTypeEnum ClassType => GetEnumByIDKey<ClassTypeEnum>("Group");
        public int FoodID { get; private set;} = -1;
        public List<int> ShoppingList { get; private set;}
        public List<int> PurchasedItemList { get; private set;}
        public int GeneratedIncome { get; private set; } = 0;

        private int _iMerchBuildingID = -1;

        public int BuildingID => GetIntByIDKey("Building");

        private int NPC => GetIntByIDKey("NPC");
        private int TownScore => GetIntByIDKey("TownScore");

        private int Money = 0;

        public bool Inn = false;
        public bool Rare => GetBoolByIDKey("Rare");

        public TravelerGroupEnum TravelerGroup => GetEnumByIDKey<TravelerGroupEnum>("Subtype");

        public FoodTypeEnum FavoriteFood => GetEnumByIDKey<FoodTypeEnum>("FavFood");

        public FoodTypeEnum DislikedFood => GetEnumByIDKey<FoodTypeEnum>("Disliked");

        protected override string SpriteName()
        {
            return DataManager.TRAVELER_FOLDER + GetStringByIDKey("Key");
        }

        public Traveler(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            Money = GetIntByIDKey("Value");

            ShoppingList = new List<int>();
            PurchasedItemList = new List<int>();
            _fWanderSpeed = Constants.NPC_WALK_SPEED;
            _eCollisionState = ActorCollisionState.Slow;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (_pathingThread == null && _liTilePath.Count == 0 && _liSchedule.Count > 0 && Util.CompareTimeStrings(_liSchedule[0].Time, GameCalendar.GetTime()))
            {
                TravelManager.RequestPathing(this);
            }

            HandleState(gTime);

            if (PeriodicEmojiReady(gTime))
            {
                if (!FollowingPath)
                {
                    var tiles = GetOccupantTile().GetWalkableNeighbours(true);
                    foreach (var actor in CurrentMap.Actors)
                    {
                        if (tiles.Contains(actor.GetOccupantTile()) && !actor.FollowingPath)
                        {
                            if (RHRandom.RollPercent(Constants.EMOJI_CHAT_DEFAULT_RATE + TraitValue(ActorTraitsEnum.Chatty)))
                            {
                                SetEmoji(ActorEmojiEnum.Talk, true);
                            }
                            break;
                        }
                    }
                }
                _rhEmojiTimer.Reset(RHRandom.Instance().Next(3, 5));
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
            if (PurchasedItemList.Count() == 0)
            {
                var i = DataManager.GetItem(ShoppingList[0]);
                var location = Position + new Point(0, -Constants.TILE_SIZE / 2);
                i.Draw(spriteBatch, new Rectangle(location.X, location.Y, Constants.TILE_SIZE, Constants.TILE_SIZE), Constants.MAX_LAYER_DEPTH, 0.5f);
            }
        }

        private void HandleState(GameTime gTime)
        {
            if (_liTilePath.Count > 0)
            {
                return;
            }

            switch (CurrentSchedule.State)
            {
                case NPCActionState.PurchaseMerch:
                    if (CurrentMap.GetCharacterObject(Constants.MAPOBJ_PURCHASESPOT).Contains(CollisionCenter) && ShoppingList.Count > 0)
                    {
                        SetFacing(DirectionEnum.Up);
                        PlayAnimationVerb(VerbEnum.Idle); 

                        if (_timer == null)
                        {
                            _timer = new RHTimer(1);
                        }
                        else if (_timer.Finished())
                        {
                            _timer = null;

                            var b = CurrentMap.Building();
                            foreach (var i in b?.Merchandise)
                            {
                                if (i != null && ShoppingList.Contains(i.ID))
                                {
                                    BuyMerchandise(i, b.Merchandise);
                                    CurrentMap.AssignMerchandise();
                                }
                            }

                            Wandering = true;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override TextEntry GetOpeningText()
        {
            TownManager.DITravelerInfo[ID] = new ValueTuple<bool, int>(true, TownManager.DITravelerInfo[ID].Item2);

            if (ShoppingList.Contains(GameManager.CurrentItem.ID)){
                return DataManager.GetGameTextEntry("Sell_Merchandise", GameManager.CurrentItem.Name(), GameManager.CurrentItem.Value);
            }
            else {
                return GetDailyDialogue();
            }
        }

        public bool Validate(int townScore)
        {
            bool rv = true;

            if (BuildingID != -1 && !TownManager.TownObjectBuilt(BuildingID))
            {
                return false;
            }

            if (NPC != -1 && !TownManager.Villagers[NPC].LivesInTown)
            {
                return false;
            }

            if (TownScore != -1 && TownScore > townScore)
            {
                return false;
            }

            return rv;
        }

        #region Schedule
        public void CheckFinishedShopping()
        {
            if (ShoppingList.Count == 0 && !RollForInn())
            {
                //ToDo: Need to put a random amount of time ahead of this
                //Create a method to handle timeSpan nonsense.
                AddScheduleData(GameCalendar.GetTime(), NPCActionState.LeaveTown, ref _liSchedule);
            }
        }

        public void StartSchedule()
        {
            CreateScheduleData(GameCalendar.GetTime(), NPCActionState.Inn, ref _liSchedule);
        }
        public override void CreateDailySchedule()
        {
            //Should not create a Daily Schedule for Travelers. Travelers should have a single thing they do then re-evaluate.
            base.CreateDailySchedule();

            if (!Inn)
            {
                //If not at the Inn, set the first action to be to go to the Inn
                CreateScheduleData(string.Format("{0}:00", Constants.TRAVELER_SPAWN_START), NPCActionState.Inn, ref _liSchedule);
            }
        }

        protected override bool ProcessActionStateData(out Point targetPosition, out string targetMapName, out DirectionEnum dir)
        {
            dir = DirectionEnum.Down;
            var currSchedule = _liSchedule[0];
            var currentAction = currSchedule.State;
            switch (currentAction)
            {
                case NPCActionState.Inn:
                    return ProcessActionStateDataHandler(TownManager.Inn.ID, "Destination", out targetPosition, out targetMapName);
                case NPCActionState.LeaveTown:
                    targetMapName = MapManager.TownMap.Name;
                    targetPosition = MapManager.TownMap.GetRandomPointFromObject("Traveler_Entrance");
                    return true;
                case NPCActionState.Market:
                    if (TownManager.Merchant != null)
                    {
                        return ProcessActionStateDataHandler(TownManager.Market, out targetPosition, out targetMapName);
                    }
                    else { goto case NPCActionState.Inn; }
                case NPCActionState.PetCafe:
                    if (TownManager.PetCafe != null)
                    {
                        return ProcessActionStateDataHandler(TownManager.PetCafe.ID, "Destination", out targetPosition, out targetMapName);
                    }
                    break;
                case NPCActionState.PurchaseMerch:
                    if (int.TryParse(currSchedule.Data, out int buildingID)){
                        var targetBuilding = TownManager.GetBuildingByID(buildingID);
                        targetMapName = targetBuilding.InnerMapName;
                        targetPosition = targetBuilding.InnerMap.GetCharacterObject(Constants.MAPOBJ_PURCHASESPOT).Center;
                        dir = DirectionEnum.Up;

                        return true;
                    }
                    break;
                default:
                    break;
            }

            targetPosition = Point.Zero;
            targetMapName = string.Empty;

            return false;
        }

        protected override void GetPathToNextAction()
        {
            base.GetPathToNextAction();
            _currentPathData?.SetWander(true);
        }
        #endregion

        public bool RollForInn()
        {
            bool rv = false;
            if (Inn)
            {
                rv = true;
            }
            else
            {
                var rollChance = ShoppingList.Count > 0 ? 30 : 10;
                if (TownManager.Inn != null && RHRandom.RollPercent(rollChance))
                {
                    rv = true;
                    StayAtInn(true);
                }
            }

            return rv;
        }

        public void StayAtInn(bool val)
        {
            Inn = val;
        }

        #region Food
        public bool HasEaten()
        {
            return FoodID != -1;
        }
        public void TryEat(Food f)
        {
            if (!HasEaten() && f.Remove(1, false))
            {
                FoodID = f.ID;
                Money -= f.Value;
            }
        }

        public bool NeutralFood(FoodTypeEnum e)
        {
            bool rv = (e != FavoriteFood && e != DislikedFood);
            return rv;
        }
        #endregion

        #region Merchandise
        public bool CheckShoppingList(int merchID, int buildingID)
        {
            bool rv = false;

            if (ShoppingList.Contains(merchID))
            {
                rv = true;
                AddScheduleData(GameCalendar.GetTime(), NPCActionState.PurchaseMerch, ref _liSchedule, buildingID.ToString());
            }

            return rv;
        }
        public void FindShopping()
        {
            //If the Traveler is already planning to go shopping, do not do anything
            foreach(var data in _liSchedule)
            {
                if (data.State == NPCActionState.PurchaseMerch)
                {
                    return;
                }
            }

            foreach (var item in ShoppingList)
            {
                if (TownManager.FindMerchandise(item, out int buildingID) && TimeSpan.TryParse(GameCalendar.GetTime(), out TimeSpan timeSpan))
                {
                    int delay = RHRandom.Instance().Next(0, Constants.TRAVELER_SHOP_DELAY_RANGE);
                    Util.AddTime(ref timeSpan, 0, delay);
                    AddScheduleData(timeSpan, NPCActionState.PurchaseMerch, ref _liSchedule, buildingID.ToString());
                    break;
                }
            }
        }

        public void BuyMerchandise(Item merch, Item[,] inventory)
        {
            if (ShoppingList.Contains(merch.ID))
            {
                InventoryManager.InitExtraInventory(inventory);
                InventoryManager.RemoveItemFromInventory(merch, false);
                InventoryManager.ClearExtraInventory();

                ShoppingList.Remove(merch.ID);
                PurchasedItemList.Add(merch.ID);
                PlayerManager.AddMoney(merch.Value);

                merch.Remove(1);

                CheckFinishedShopping();
            }
        }
        public void AddToShopping(int x)
        {
            ShoppingList.Add(x);
        }
        public void ClearShopping()
        {
            ShoppingList.Clear();
        }
        public void AssignNeed(TravelerNeed need)
        {
            _TravelerNeed = need;
        }

        private ClassTypeEnum DetermineClassType(MerchDataDictionary merchData)
        {
            ClassTypeEnum rv = ClassTypeEnum.None;
            if (merchData[ClassType].Count > 0)
            {
                rv = ClassType;
            }

            return rv;
        }

        public void PurchaseNeedItem(MerchDataDictionary merchData)
        {
            if (Money > 0 && merchData.Count > 0 && _TravelerNeed != null)
            {
                var classtype = DetermineClassType(merchData);

                if (_TravelerNeed.MerchType != MerchandiseTypeEnum.None)
                {
                    List<MerchKVP> allMerch = merchData[classtype].FindAll(x => _TravelerNeed.Validate(x.Key) && x.Key.Value <= Money);
                    if (allMerch.Count > 0)
                    {
                        var randomKvp = Util.GetRandomItem(allMerch);

                        BuyFromBuilding(merchData, randomKvp.Value, randomKvp.Key, classtype, randomKvp);
                        _TravelerNeed = null;
                    }
                }
                else if (_TravelerNeed.MerchID > -1)
                {
                    var merchItem = merchData[classtype].Find(x => x.Key.ID == _TravelerNeed.MerchID);
                    BuyFromBuilding(merchData, merchItem.Value, merchItem.Key, classtype, merchItem);
                }
            }
        }
        public void PurchaseExtraItems(MerchDataDictionary merchData)
        {
            if (merchData.Count > 0)
            {
                var classtype = DetermineClassType(merchData);

                if (merchData[classtype].Count > 0)
                {
                    List<MerchKVP> canPurchase;
                    do
                    {
                        canPurchase = merchData[classtype].FindAll(x => x.Key.Value <= Money);
                        if (canPurchase.Count > 0)
                        {
                            var randomKvp = Util.GetRandomItem(canPurchase);
                            BuyFromBuilding(merchData, randomKvp.Value, randomKvp.Key, classtype, randomKvp);
                        }
                    } while (canPurchase.Count > 0 && PurchasedItemList.Count < Constants.TRAVELER_ITEM_PURCHASE_CAP);
                }
            }
        }

        private void BuyFromBuilding(MerchDataDictionary merchData, Building building, Item item, ClassTypeEnum classType, MerchKVP randomKvp)
        {
            InventoryManager.InitExtraInventory(building.Merchandise);
            Money -= item.Value;
            item.Remove(1, false);
            InventoryManager.ClearExtraInventory();
            PurchasedItemList.Add(item.ID);
            _iMerchBuildingID = building.ID;

            if (item.Number == 0)
            {
                merchData[classType].Remove(randomKvp);
            }
        }
        #endregion

        public int CalculateIncome()
        {
            foreach (var i in PurchasedItemList)
            {
                CalculateProfit(i, TownManager.GetBuildingByID(_iMerchBuildingID));
            }
            CalculateProfit(FoodID, TownManager.Inn);

            return GeneratedIncome;
        }
        private void CalculateProfit(int itemID, Building b)
        {
            if (itemID > -1 && b != null)
            {
                int value = (DataManager.GetIntByIDKey(itemID, "Value", DataType.Item));
                float profitMod = (1 + b.GetShopProfitModifier());
                int profit = (int)(value * profitMod);
                GeneratedIncome += profit;

                TownManager.AddToSoldGoods(itemID, profit);
            }
        }

        public TravelerData SaveData()
        {
            TravelerData npcData = new TravelerData()
            {
                id = ID,
                needID = _TravelerNeed != null ? _TravelerNeed.MerchID : -1,
                needType = _TravelerNeed != null ? _TravelerNeed.MerchType : MerchandiseTypeEnum.None,
                shoppingList = ShoppingList,
                stayAtInn = Inn
            };

            return npcData;
        }
        public void LoadData(TravelerData data)
        {
            if (data.needID > -1)
            {
                _TravelerNeed = new TravelerNeed(data.needID);
            }
            else if (data.needType != MerchandiseTypeEnum.None)
            {
                _TravelerNeed = new TravelerNeed(data.needType);
            }

            ShoppingList = data.shoppingList;
            Inn = data.stayAtInn;
        }
    }
}
