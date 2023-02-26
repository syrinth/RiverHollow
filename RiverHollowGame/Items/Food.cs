using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Food : Item
    {
        public int Stamina => GetIntByIDKey("Stam");
        public int FoodValue => GetIntByIDKey("FoodValue");
        public FoodTypeEnum FoodType => GetEnumByIDKey<FoodTypeEnum>("FoodType");

        public Food(int id, Dictionary<string, string> stringData, int num) : base(id, stringData, num)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Food");
        }

        public override string Description()
        {
            string rv = base.Description();
            rv += System.Environment.NewLine;
            if (Stamina > 0) { rv += "Stamina: +" + Stamina + " "; }
            rv = rv.Trim();

            return rv;
        }

        public override bool HasUse() { return true; }
        public override bool ItemBeingUsed()
        {
            GameManager.SetSelectedItem(this);
            ConfirmItemUse(DataManager.GetGameTextEntry("FoodConfirm", Name()));

            return true;
        }

        public override void UseItem()
        {
            if (Number > 0)
            {
                Remove(1);
                PlayerManager.RecoverEnergy(Stamina);
            }
            ClearGMObjects();
        }
    }
}
