using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Misc
{
    public class Shop
    {
        public int ShopkeeperID { get; private set; }
        int _iShopID;
        string _sRandomIndices = string.Empty;
        List<ShopItemSpot> _liShopItemSpots;
        public IList<ShopItemSpot> ItemSpots => _liShopItemSpots.AsReadOnly();
        List<Merchandise> _liMerchandise;
        public int Count => _liMerchandise.Count;

        public Shop(int id, Dictionary<string, string> stringDictionary)
        {
            _iShopID = id;
            _liShopItemSpots = new List<ShopItemSpot>();

            if (stringDictionary.ContainsKey("Shopkeeper"))
            {
                ShopkeeperID = int.Parse(stringDictionary["Shopkeeper"]);
            }

            _liMerchandise = new List<Merchandise>();
            if (stringDictionary.ContainsKey("ItemID"))
            {
                foreach (string s in Util.FindParams(stringDictionary["ItemID"]))
                {
                    _liMerchandise.Add(new Merchandise(Merchandise.MerchTypeEnum.Item, s));
                }
            }
            if (stringDictionary.ContainsKey("ObjectID"))
            {
                foreach (string s in Util.FindParams(stringDictionary["ObjectID"]))
                {
                    _liMerchandise.Add(new Merchandise(Merchandise.MerchTypeEnum.WorldObject, s));
                }
            }
        }

        public bool Interact(RHMap map, Point mouseLocation)
        {
            foreach (ShopItemSpot itemSpot in _liShopItemSpots)
            {
                if (itemSpot.Contains(mouseLocation) && PlayerManager.PlayerInRange(itemSpot.Box.Center, GameManager.TILE_SIZE * 2))
                {
                    if (DataManager.DIVillagers.ContainsKey(ShopkeeperID) && map.ContainsActor(DataManager.DIVillagers[ShopkeeperID]) ||
                        DataManager.DIMerchants.ContainsKey(ShopkeeperID) && map.ContainsActor(DataManager.DIMerchants[ShopkeeperID]))
                    {
                        itemSpot.Buy();
                        return true;
                    }
                    else { GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("BuyMerch_NoShopkeep")); }
                }
            }

            return false;
        }

        public void Purchase(Item purchaseItem)
        {
            if (PlayerManager.Money >= purchaseItem.TotalValue)
            {
                PlayerManager.TakeMoney(purchaseItem.TotalValue);
                if (purchaseItem.IsUnique())
                {
                    PlayerManager.AddToUniqueBoughtItems(purchaseItem.ItemID);
                    _liShopItemSpots.Find(x => x.MerchID == purchaseItem.ItemID).SetMerchandise(null);
                }
                InventoryManager.AddToInventory(purchaseItem);
            }
        }

        public void AddItemSpot(ShopItemSpot spot)
        {
            _liShopItemSpots.Add(spot);
        }
        public void ClearItemSpots()
        {
            _liShopItemSpots.Clear();
        }

        public void ClearRandom()
        {
            _sRandomIndices = string.Empty;
        }
        public void Randomize()
        {
            if (string.IsNullOrEmpty(_sRandomIndices))
            {
                int totalMerch = _liMerchandise.Count;

                List<int> indices = new List<int>();
                for (int i = 0; i < totalMerch; i++) { indices.Add(i); }

                for (int i = 0; i < _liShopItemSpots.Count && i < totalMerch; i++)
                {
                    int index = RHRandom.Instance().Next(indices.Count);
                    _sRandomIndices += indices[index] + "|";
                    indices.RemoveAt(index);
                }

                _sRandomIndices = _sRandomIndices.Remove(_sRandomIndices.Length - 1);
            }
        }

        public void PlaceStock(bool randomize)
        {
            _liShopItemSpots.ForEach(x => x.SetMerchandise(null));

            string[] random = Util.FindParams(_sRandomIndices);

            int totalMerch = _liMerchandise.Count;
            for (int i = 0; i < _liShopItemSpots.Count && i < totalMerch; i++)
            {
                if (randomize && !string.IsNullOrEmpty(_sRandomIndices)) {
                    Merchandise m = _liMerchandise[int.Parse(random[i])];
                    _liShopItemSpots[i].SetMerchandise(m);
                }
                else { _liShopItemSpots[i].SetMerchandise(_liMerchandise[i]); }
            }
        }

        public List<Merchandise> GetUnlockedMerchandise()
        {
            List<Merchandise> rv = new List<Merchandise>();
            foreach (Merchandise m in _liMerchandise)
            {
                if (m.Unlocked) { rv.Add(m); }
            }

            return rv;
        }

        public void UnlockMerchandise(int merchID)
        {
            Merchandise m = _liMerchandise.Find(x => x.MerchID == merchID);
            if (m != null)
            {
                m.Unlock();
            }
        }

        public void UnlockMerchandise(string unlockedMerchandise)
        {
            string[] split = unlockedMerchandise.Split('-');
            for (int i = 0; i < _liMerchandise.Count; i++)
            {
                Merchandise m = _liMerchandise.Find(x => x.MerchID == i);
                if (i < split.Length && split[i].Equals("True") && m != null)
                {
                    m.Unlock();
                }
            }
        }

        public ShopData SaveData()
        {
            string value = string.Empty;
            int index = 0;
            foreach (Merchandise m in _liMerchandise)
            {
                value += m.Unlocked;

                if (index < _liMerchandise.Count - 1) {
                    index++;
                    value += "-";
                }
            }

            ShopData sData = new ShopData
            {
                shopID = _iShopID,
                merchUnlockedString = value,
                randomized = _sRandomIndices
            };
            return sData;
        }

        public void LoadData(ShopData data)
        {
            UnlockMerchandise(data.merchUnlockedString);
            _sRandomIndices = data.randomized;
        }
    }

    public class Merchandise
    {
        public enum MerchTypeEnum { Item, WorldObject };
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
