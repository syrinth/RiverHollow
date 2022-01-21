using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;

namespace RiverHollow.Items
{
    public class Blueprint : Item
    {
        private readonly int[] _arrItemUnlocks;
        private readonly string[] _arrBuildingUnlocks;
        public Blueprint(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            if (stringData.ContainsKey("ItemID")) { _arrItemUnlocks = Array.ConvertAll(Util.FindParams(stringData["ItemID"]), s => int.Parse(s)); ; }
            if (stringData.ContainsKey("BuildingID")) { _arrBuildingUnlocks = Util.FindParams(stringData["BuildingID"]); }

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Resources");

            _bStacks = false;
        }

        public override void ItemBeingUsed()
        {
            TextEntry entry = DataManager.GetGameTextEntry("Read_Book");
            entry.FormatText(Name);
            ConfirmItemUse(entry);
        }

        public override void UseItem(GameManager.TextEntryVerbEnum action)
        {
            if (action == GameManager.TextEntryVerbEnum.Yes)
            {
                if (_arrItemUnlocks != null)
                {
                    PlayerManager.AddToCraftingDictionary(_arrItemUnlocks);
                }

                if (_arrBuildingUnlocks != null)
                {
                    for (int i = 0; i < _arrBuildingUnlocks.Length; i++)
                    {
                        PlayerManager.DIBuildInfo[int.Parse(_arrBuildingUnlocks[i])].Unlock();
                    }
                }

                Remove(1);
            }
        }
    }
}
