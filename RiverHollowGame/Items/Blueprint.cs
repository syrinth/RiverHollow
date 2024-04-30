using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Blueprint : Item
    {
        private readonly int[] _arrCraftingUnlocks;
        public Blueprint(int id) : base(id, 1)
        {
            if (GetBoolByIDKey("ObjectID")) { _arrCraftingUnlocks = Array.ConvertAll(FindParamsByIDKey("ObjectID"), s => int.Parse(s)); ; }

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Blueprints");
        }

        public override bool HasUse() { return true; }
        public override bool ItemBeingUsed()
        {
            StrikeAPose();
            GameManager.SetSelectedItem(this);
            ConfirmItemUse(DataManager.GetGameTextEntry("Read_Book", Name()));

            return true;
        }

        public override void UseItem()
        {
            if (_arrCraftingUnlocks != null)
            {
                PlayerManager.AddToCraftingDictionary(_arrCraftingUnlocks);
            }

            Remove(1);
        }
    }
}
