using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Windows.Input;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class Upgrade
    {
        public UpgradeTypeEnum UpgradeType => DataManager.GetEnumByIDKey<UpgradeTypeEnum>(ID, "Type", DataType.Upgrade);

        public int ID { get; private set; }
        public Point Icon => DataManager.GetPointByIDKey(ID, "Icon", DataType.Upgrade);
        public Dictionary<int, int> UpgradeRequirements => DataManager.IntDictionaryFromLookup(ID, "ItemID", DataType.Upgrade);

        public bool NewRecipes => DataManager.GetBoolByIDKey(ID, "NewRecipes", DataType.Upgrade);

        public int FormulaLevel => DataManager.GetIntByIDKey(ID, "Formula", DataType.Upgrade, 0);
        public int Profit => DataManager.GetIntByIDKey(ID, "Profit", DataType.Upgrade, 0);
        public int TownScore => DataManager.GetIntByIDKey(ID, "TownScore", DataType.Upgrade, 0);
        public int CraftAmount => DataManager.GetIntByIDKey(ID, "CraftAmount", DataType.Upgrade, 0);
        public int Cost => DataManager.GetIntByIDKey(ID, "Cost", DataType.Upgrade, 0);
        public int Priority => DataManager.GetIntByIDKey(ID, "Priority", DataType.Upgrade, 0);

        public string Name => DataManager.GetTextData(ID, "Name", DataType.Upgrade);
        public string Description => DataManager.GetTextData(ID, "Description", DataType.Upgrade);

        public UpgradeStatusEnum Status {  get; private set; }

        public Upgrade(int id)
        {
            ID = id;

            if (UpgradeType == UpgradeTypeEnum.Building)
            {
                Status = UpgradeStatusEnum.Unlocked;
            }
            else
            {
                Status = UpgradeStatusEnum.Locked;
            }
        }

        public void TriggerUpgrade()
        {
            ChangeStatus(UpgradeStatusEnum.Completed);

            if (DataManager.GetBoolByIDKey(ID, "RemoveObjectID", DataType.Upgrade))
            {
                var args = Util.FindArguments(DataManager.GetStringByIDKey(ID, "RemoveObjectID", DataType.Upgrade));
                if (MapManager.Maps.ContainsKey(args[0]))
                {
                    var targetMap = MapManager.Maps[args[0]];
                    if (int.TryParse(args[1], out var targetID))
                    {
                        var obj = targetMap.GetObjectsByID(targetID);
                        obj.ForEach(x => targetMap.RemoveWorldObject(x));
                    }
                }
            }
        }

        public void ChangeStatus(UpgradeStatusEnum status)
        {
            Status = status;
        }
    }
}
