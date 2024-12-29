using RiverHollow.Game_Managers;
using System;

using static RiverHollow.Utilities.Constants;

namespace RiverHollow.Items
{
    internal class CosmeticItem : Item
    {
        public CosmeticItem(int id) : base(id, 1)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Cosmetics");
        }

        public override bool HasUse() { return true; }
        public override bool ItemBeingUsed()
        {
            StrikeAPose();
            GameManager.SetSelectedItem(this);
            ConfirmItemUse(DataManager.GetGameTextEntry("Item_Confirm", Name()));

            return true;
        }
        
        public override void UseItem()
        {
            if (GetBoolByIDKey("CosmeticID"))
            {
                var unlock = Array.ConvertAll(FindParamsByIDKey("CosmeticID"), s => int.Parse(s));
                PlayerManager.AddToCosmeticDictionary(unlock);
            }

            Remove(1);
        }
    }
}
