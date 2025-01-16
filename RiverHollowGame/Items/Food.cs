using RiverHollow.Game_Managers;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Food : Item
    {
        public int EnergyRecovery => GetIntByIDKey("EnergyRecovery");
        public int FoodValue => GetIntByIDKey("FoodValue");
        public FoodTypeEnum FoodType => GetEnumByIDKey<FoodTypeEnum>("FoodType");

        public override bool Usable => true;

        public Food(int id, int num) : base(id, num)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Food");
        }

        public override bool HasUse() { return true; }

        public override void UseItem()
        {
            if (Number > 0)
            {
                Remove(1);
                PlayerManager.RecoverEnergy(EnergyRecovery);
            }
            ClearGMObjects();
        }
    }
}
