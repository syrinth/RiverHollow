using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Misc
{
    public class Merchandise
    {
        public enum MerchTypeEnum { Item, WorldObject, Actor };
        public MerchTypeEnum MerchType { get; }

        private bool _bLocked = false;
        public bool Unlocked => !_bLocked;
        public string UniqueData { get; }
        public int MerchID { get; } = -1;
        private int _iCost;
        public int MoneyCost => _iCost;

        private readonly int _iTaskReq = -1;

        public Merchandise(Dictionary<string, string> stringData)
        {
            MerchType = Util.ParseEnum<MerchTypeEnum>(stringData["Type"]);
            switch (MerchType)
            {
                case MerchTypeEnum.Item:
                    if (stringData.ContainsKey("ItemID"))
                    {
                        //Some items may have unique data so only parse the first entry
                        //tag is ItemID to differentiate the tag from in the GUI ItemData Manager
                        string[] data = stringData["ItemID"].Split('-');
                        MerchID = int.Parse(data[0]);
                        if (data.Length > 1) { UniqueData = data[1]; }
                    }
                    break;
                case MerchTypeEnum.WorldObject:
                    MerchID = int.Parse(stringData["ObjectID"]);
                    break;
                case MerchTypeEnum.Actor:
                    MerchID = int.Parse(stringData["NPC_ID"]);
                    break;
            }

            Util.AssignValue(ref _iCost, "Cost", stringData);
            Util.AssignValue(ref _iTaskReq, "TaskReq", stringData);
            Util.AssignValue(ref _bLocked, "Locked", stringData);
        }

        /// <summary>
        /// Call to unlock the Merchandise so that it can be purchased.
        /// </summary>
        public void Unlock()
        {
            _bLocked = false;
        }
    }
}
