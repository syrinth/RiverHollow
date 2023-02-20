using Microsoft.Xna.Framework;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Traveler : TalkingActor
    {
        private float _fFoodModifier = Constants.HUNGER_MOD;
        private bool _bEaten = false;
        public int Income { get; private set; } = 0;

        public AnimationEnum MoodVerb { get;  private set; } = AnimationEnum.Angry;

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

            List<AnimationData> listAnimations = new List<AnimationData>();
            Util.AddToAnimationsList(ref listAnimations, stringData, AnimationEnum.Angry);
            Util.AddToAnimationsList(ref listAnimations, stringData, AnimationEnum.Sad);
            Util.AddToAnimationsList(ref listAnimations, stringData, AnimationEnum.Neutral);
            Util.AddToAnimationsList(ref listAnimations, stringData, AnimationEnum.Happy);

            foreach (AnimationData data in listAnimations)
            {
                BodySprite.AddAnimation(data.Animation, data.XLocation, data.YLocation, Width, Height);
            }
        }

        public void TryEat(Food f)
        {
            if (!_bEaten && f.Remove(1, false))
            {
                _bEaten = true;
                _fFoodModifier = (f.FoodValue / 100f);

                if (f.FoodType == FavoriteFood())
                {
                    MoodVerb = AnimationEnum.Happy;
                    _fFoodModifier += .5f;
                }
                else if (NeutralFood(f.FoodType))
                {
                    MoodVerb = AnimationEnum.Neutral;
                }
                else if (f.FoodType == DislikedFood() || f.FoodType == FoodTypeEnum.Forage)
                {
                    MoodVerb = AnimationEnum.Sad;
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
                var modifier = !_bEaten ? 0 : (1 + shop.GetShopValueModifier() + _fFoodModifier);
                Income = (int)(Value() * modifier);
            }

            return Income;
        }
    }
}
