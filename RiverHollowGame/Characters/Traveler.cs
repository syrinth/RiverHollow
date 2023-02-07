using MonoGame.Extended.Sprites;
using Newtonsoft.Json.Linq;
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
        public int Income { get; private set; } = 0;

        public AnimationEnum MoodVerb { get;  private set; } = AnimationEnum.Angry;

        public int BuildingID()
        {
            return DataManager.GetIntByIDKey(ID, "Building", DataType.NPC);
        }

        private int NPC()
        {
            return DataManager.GetIntByIDKey(ID, "NPC", DataType.NPC);
        }

        private int Value()
        {
            return DataManager.GetIntByIDKey(ID, "Value", DataType.NPC);
        }

        public bool Rare()
        {
            return DataManager.GetBoolByIDKey(ID, "Rare", DataType.NPC);
        }

        public TravelerGroupEnum Group()
        {
            return DataManager.GetEnumByIDKey<TravelerGroupEnum>(ID, "Subtype", DataType.NPC);
        }

        public FoodTypeEnum FavoriteFood()
        {
            return DataManager.GetEnumByIDKey<FoodTypeEnum>(ID, "FavFood", DataType.NPC);
        }

        public FoodTypeEnum DislikedFood()
        {
            return DataManager.GetEnumByIDKey<FoodTypeEnum>(ID, "Disliked", DataType.NPC);
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
                _sprBody.AddAnimation(data.Animation, data.XLocation, data.YLocation, _iBodyWidth, _iBodyHeight);
            }
        }

        public void TryEat(Food f)
        {
            if (_fFoodModifier == Constants.HUNGER_MOD && f.Remove(1, false))
            {
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
                var modifier = 1 + shop.GetShopValueModifier() + _fFoodModifier;
                Income = (int)(Value() * modifier);
            }

            return Income;
        }
    }
}
