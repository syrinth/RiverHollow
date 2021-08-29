using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Misc
{
    public class Shop
    {
        int _iShopID;
        string _sName;
        public string Name => _sName;
        Dictionary<int, Merchandise> _diMerchandise;
        public int Count => _diMerchandise.Count;

        public Shop(int id, Dictionary<string, string> stringDictionary)
        {
            _iShopID = id;

            _diMerchandise = new Dictionary<int, Merchandise>();
            if (stringDictionary.ContainsKey("ItemID"))
            {
                foreach (string s in Util.FindParams(stringDictionary["ItemID"]))
                {
                    _diMerchandise[int.Parse(s)] = (new Merchandise(Merchandise.MerchTypeEnum.Item, s));
                }
            }
            if (stringDictionary.ContainsKey("ObjectID"))
            {
                foreach (string s in Util.FindParams(stringDictionary["ObjectID"]))
                {
                    _diMerchandise[int.Parse(s)] = (new Merchandise(Merchandise.MerchTypeEnum.WorldObject, s));
                }
            }
            if (stringDictionary.ContainsKey("NPC_ID"))
            {
                foreach (string s in Util.FindParams(stringDictionary["NPC_ID"]))
                {
                    string[] data = s.Split('-');
                    _diMerchandise[int.Parse(s)] = (new Merchandise(Merchandise.MerchTypeEnum.Actor, s));
                }
            }
        }

        public List<Merchandise> GetUnlockedMerchandise()
        {
            List<Merchandise> rv = new List<Merchandise>();
            foreach (Merchandise m in _diMerchandise.Values)
            {
                if (m.Unlocked) { rv.Add(m); }
            }

            return rv;
        }

        public void UnlockMerchandise(int merchID)
        {
            if (_diMerchandise.ContainsKey(merchID))
            {
                _diMerchandise[merchID].Unlock();
            }
        }

        public void UnlockMerchandise(string unlockedMerchandise)
        {
            string[] split = unlockedMerchandise.Split('-');
            for (int i = 0; i < _diMerchandise.Count; i++)
            {
                if (i < split.Length && split[i].Equals("True") && _diMerchandise.ContainsKey(i))
                {
                    _diMerchandise[i].Unlock();
                }
            }
        }

        public ShopData SaveData()
        {
            string value = string.Empty;
            int index = 0;
            foreach (Merchandise m in _diMerchandise.Values)
            {
                value += m.Unlocked;

                if (index < _diMerchandise.Values.Count - 1) {
                    index++;
                    value += "-";
                }
            }

            ShopData sData = new ShopData
            {
                shopID = _iShopID,
                merchUnlockedString = value
            };
            return sData;
        }
    }

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

        public Merchandise(MerchTypeEnum type, string merchData)
        {
            MerchType = type;

            string[] data = merchData.Split('-');
            MerchID = int.Parse(data[0]);

            if(data.Length > 1)
            {
                if (data[1].Equals("Unique")) { UniqueData = data[1]; }
                else if (data[1].Equals("Locked")) { _bLocked = true; }
            }

            switch (MerchType)
            {
                case MerchTypeEnum.Item:
                    _iCost = int.Parse(DataManager.GetItemValueByID(MerchID, "Value"));
                    break;
                case MerchTypeEnum.WorldObject:
                    _iCost = int.Parse(DataManager.GetWorldObjectValueByID(MerchID, "Value"));
                    break;
                case MerchTypeEnum.Actor:
                    _iCost = int.Parse(DataManager.GetNPCValueByID(MerchID, "Value"));
                    break;
            }
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
