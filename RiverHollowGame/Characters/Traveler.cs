using Microsoft.Xna.Framework;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Traveler : TalkingActor
    {
        public AffinityEnum Affinity => GetEnumByIDKey<AffinityEnum>("SubGroup");
        public ClassTypeEnum ClassType => GetEnumByIDKey<ClassTypeEnum>("Group");
        public int FoodID { get; private set;} = -1;
        public int ItemID { get; private set;} = -1;
        public int Income { get; private set; } = 0;

        private int _iMerchBuildingID = -1;

        public int BuildingID => GetIntByIDKey("Building");

        private int NPC => GetIntByIDKey("NPC");
        private int TownScore => GetIntByIDKey("TownScore");

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
            _fWanderSpeed = Constants.NPC_WALK_SPEED;
            Wandering = true;
            _eCollisionState = ActorCollisionState.Slow;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
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

        public override TextEntry GetOpeningText()
        {
            TownManager.DITravelerInfo[ID] = new ValueTuple<bool, int>(true, TownManager.DITravelerInfo[ID].Item2);
            return GetDailyDialogue();
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

            if(TownScore != -1 && TownScore > townScore)
            {
                return false;
            }

            return rv;
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

        public void PurchaseItem(Dictionary<ClassTypeEnum, List<KeyValuePair<Item, Building>>> merchData)
        {
            if (merchData.Count > 0)
            {
                ClassTypeEnum itemType = ClassTypeEnum.None;
                if (merchData[ClassType].Count > 0)
                {
                    itemType = ClassType;
                }

                if (merchData[itemType].Count > 0)
                {
                    var randomKvp = Util.GetRandomItem(merchData[itemType]);
                    var building = randomKvp.Value;
                    var item = randomKvp.Key;

                    InventoryManager.InitExtraInventory(building.Merchandise);
                    item.Remove(1, false);
                    InventoryManager.ClearExtraInventory();
                    ItemID = item.ID;
                    _iMerchBuildingID = building.ID;

                    if (item.Number == 0)
                    {
                        merchData[itemType].Remove(randomKvp);
                    }
                }
            }
        }

        public bool NeutralFood(FoodTypeEnum e)
        {
            bool rv = (e != FavoriteFood && e != DislikedFood);
            return rv;
        }

        public int CalculateIncome()
        {
            CalculateProfit(ItemID, TownManager.GetBuildingByID(_iMerchBuildingID));
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
