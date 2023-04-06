using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class Upgrade
    {
        public int ID { get; private set; }
        public Point Icon => DataManager.GetPointByIDKey(ID, "Icon", DataType.Upgrade);
        public Dictionary<int, int> UpgradeRequirements => DataManager.IntDictionaryFromLookup(ID, "ItemID", DataType.Upgrade);

        public int Profit => DataManager.GetIntByIDKey(ID, "Profit", DataType.Upgrade, 0);
        public int Chance => DataManager.GetIntByIDKey(ID, "Chance", DataType.Upgrade, 0);
        public int CraftingSlots => DataManager.GetIntByIDKey(ID, "CraftingSlots", DataType.Upgrade, 0);
        public bool NewRecipes => DataManager.GetBoolByIDKey(ID, "NewRecipes", DataType.Upgrade);
        public int Cost => DataManager.GetIntByIDKey(ID, "Cost", DataType.Upgrade, 0);

        public Upgrade(int id)
        {
            ID = id;
        }
    }
}
