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
        public Blueprint(int id, Dictionary<string, string> stringData) : base(id, stringData, 1)
        {
            if (stringData.ContainsKey("ObjectID")) { _arrCraftingUnlocks = Array.ConvertAll(Util.FindParams(stringData["ObjectID"]), s => int.Parse(s)); ; }

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Blueprints");
        }

        public override bool HasUse() { return true; }
        public override bool ItemBeingUsed()
        {
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
