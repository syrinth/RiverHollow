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
        public Blueprint(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            if (stringData.ContainsKey("ObjectID")) { _arrCraftingUnlocks = Array.ConvertAll(Util.FindParams(stringData["ObjectID"]), s => int.Parse(s)); ; }

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Resources");

            _bStacks = false;
        }

        public override void ItemBeingUsed()
        {
            base.ItemBeingUsed();
            TextEntry entry = DataManager.GetGameTextEntry("Read_Book");
            entry.FormatText(Name());
            ConfirmItemUse(entry);
        }

        public override void UseItem(TextEntryVerbEnum action)
        {
            if (action == TextEntryVerbEnum.Yes)
            {
                if (_arrCraftingUnlocks != null)
                {
                    PlayerManager.AddToCraftingDictionary(_arrCraftingUnlocks);
                }

                Remove(1);
            }
        }
    }
}
