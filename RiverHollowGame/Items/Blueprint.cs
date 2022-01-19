using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Items
{
    public class Blueprint : Item
    {
        private readonly string[] _arrItemUnlocks;
        private readonly string[] _arrBuildingUnlocks;
        public Blueprint(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            if (stringData.ContainsKey("ItemID")) { _arrItemUnlocks = Util.FindParams(stringData["ItemID"]); }
            if (stringData.ContainsKey("BuildingID")) { _arrBuildingUnlocks = Util.FindParams(stringData["BuildingID"]); }

            _bStacks = false;
            _texTexture = DataManager.GetTexture(@"Textures\items");
        }

        /// <summary>
        /// When called, unlocks all World Objects as craftable
        /// </summary>
        public void UnlockCraftables()
        {
            for (int i = 0; i < _arrItemUnlocks.Length; i++)
            {
                PlayerManager.AddToCraftingDictionary(int.Parse(_arrItemUnlocks[i]));
            }
        }

        /// <summary>
        /// When called, unlocks all World Objects as craftable
        /// </summary>
        public void UnlockBuildings()
        {
            for (int i = 0; i < _arrItemUnlocks.Length; i++)
            {
                PlayerManager.DIBuildInfo[int.Parse(_arrBuildingUnlocks[i])].Unlock();
            }
        }
    }
}
