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
        List<ShopItemSpot> _liShopItemSpots;
        public IList<ShopItemSpot> ItemSpots => _liShopItemSpots.AsReadOnly();
        Dictionary<int, Merchandise> _diMerchandise;
        public int Count => _diMerchandise.Count;

        public Shop(int id, Dictionary<string, string> stringDictionary)
        {
            _iShopID = id;
            _liShopItemSpots = new List<ShopItemSpot>();

            if (stringDictionary.ContainsKey("Shopkeeper"))
            {
                ShopkeeperID = int.Parse(stringDictionary["Shopkeeper"]);
            }

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
            if (PlayerManager.Money >= purchaseItem.Value)
            {
                PlayerManager.TakeMoney(purchaseItem.Value);
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

        public void PlaceStock(bool randomize)
        {
            _liShopItemSpots.ForEach(x => x.SetMerchandise(null));

            List<Merchandise> merchList = Enumerable.ToList(_diMerchandise.Values);
            int totalMerch = merchList.Count;
            for (int i = 0; i < _liShopItemSpots.Count && i < totalMerch; i++)
            {
                if (randomize) {
                    int index = RHRandom.Instance().Next(merchList.Count);
                    _liShopItemSpots[i].SetMerchandise(merchList[index]);
                    merchList.RemoveAt(index);
                }
                else { _liShopItemSpots[i].SetMerchandise(merchList[i]); }
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
