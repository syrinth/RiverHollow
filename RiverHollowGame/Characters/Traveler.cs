using Microsoft.Xna.Framework;
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
        public ClassTypeEnum ClassType => GetEnumByIDKey<ClassTypeEnum>("Group");
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

        public bool Validate()
        {
            return (BuildingID() == -1 || TownManager.TownObjectBuilt(BuildingID())) &&
                        (NPC() == -1 || TownManager.Villagers[NPC()].LivesInTown);
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

        public void PurchaseItem(Dictionary<ClassTypeEnum, List<KeyValuePair<Item, Container>>> merchData)
        {
            if (BuildingID() != -1)
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
                        var table = randomKvp.Value;
                        var item = randomKvp.Key;

                        InventoryManager.InitExtraInventory(table.Inventory);
                        item.Remove(1, false);
                        InventoryManager.ClearExtraInventory();
                        ItemID = item.ID;

                        if(item.Number == 0)
                        {
                            merchData[itemType].Remove(randomKvp);
                        }
                    }
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
