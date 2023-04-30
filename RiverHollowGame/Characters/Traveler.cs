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
        private float _fFoodModifier = Constants.HUNGER_MOD;
        public int FoodID { get; private set;} = -1;
        public int Income { get; private set; } = 0;

        public TravelerMoodEnum MoodVerb { get;  private set; } = TravelerMoodEnum.Angry;

        public int BuildingID()
        {
            return GetIntByIDKey( "Building");
        }

        private int NPC()
        {
            return GetIntByIDKey("NPC");
        }

        private int Value()
        {
            return GetIntByIDKey("Value");
        }

        public bool Rare()
        {
            return GetBoolByIDKey("Rare");
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
            Wandering = true;
            SlowDontBlock = true;
        }

        public override TextEntry GetOpeningText()
        {
            TownManager.DITravelerInfo[ID] = new ValueTuple<bool, int>(true, TownManager.DITravelerInfo[ID].Item2);
            return base.GetOpeningText();
        }

        public void TryEat(Food f)
        {
            if (FoodID == -1 && f.Remove(1, false))
            {
                FoodID = f.ID;
                _fFoodModifier = (f.FoodValue / 100f);

                if (f.FoodType == FavoriteFood())
                {
                    MoodVerb = TravelerMoodEnum.Happy;
                    _fFoodModifier += .5f;
                }
                else if (NeutralFood(f.FoodType))
                {
                    MoodVerb = TravelerMoodEnum.Neutral;
                }
                else if (f.FoodType == DislikedFood() || f.FoodType == FoodTypeEnum.Forage)
                {
                    MoodVerb = TravelerMoodEnum.Sad;
                    _fFoodModifier -= .5f;
                }
            }
        }

        public bool Validate()
        {
            return (BuildingID() == -1 || TownManager.TownObjectBuilt(BuildingID())) &&
                        (NPC() == -1 || TownManager.DIVillagers[NPC()].LivesInTown);
        }

        public bool NeutralFood(FoodTypeEnum e)
        {
            bool rv = (e != FavoriteFood() && e != DislikedFood() && e != FoodTypeEnum.Forage);
            return rv;
        }

        public int CalculateIncome()
        {
            Building shop = TownManager.GetBuildingByID(BuildingID());
            if (shop != null)
            {
                var modifier = FoodID == -1 ? 0 : (1 + shop.GetShopProfitModifier() + _fFoodModifier);
                Income = (int)(Value() * modifier);
            }

            return Income;
        }
    }
}
