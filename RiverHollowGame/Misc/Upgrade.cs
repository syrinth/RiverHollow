using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class Upgrade
    {
        public int ID { get; private set; }
        public int Cost => DataManager.GetIntByIDKey(ID, "Cost", DataType.Upgrade);
        public Point Icon => DataManager.GetPointByIDKey(ID, "Icon", DataType.Upgrade);
        public Dictionary<int, int> UpgradeRequirements => DataManager.IntDictionaryFromLookup(ID, "ItemID", DataType.Upgrade);

        public int Profit => DataManager.GetIntByIDKey(ID, "Profit", DataType.Upgrade);

        public Upgrade(int id)
        {
            ID = id;
        }
    }
}
