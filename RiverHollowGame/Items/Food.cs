using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Food : Item
    {
        public int Stamina => DataManager.GetIntByIDKey(ID, "Stam", DataType.Item);
        public int FoodValue => DataManager.GetIntByIDKey(ID, "FoodValue", DataType.Item);
        public FoodTypeEnum FoodType => DataManager.GetEnumByIDKey<FoodTypeEnum>(ID, "FoodType", DataType.Item);

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
            TextEntry entry = DataManager.GetGameTextEntry("FoodConfirm");
            entry.FormatText(Name());
            ConfirmItemUse(entry);

            return true;
        }

        public override void UseItem(TextEntryVerbEnum action)
        {
            if (Number > 0)
            {
                Remove(1);
                PlayerManager.IncreaseStamina(Stamina);
            }
            ClearGMObjects();
        }
    }
}
