using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
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

        public void Update()
        {
            int closestDistance = 999;
            ShopItemSpot closest = null;
            foreach(ShopItemSpot spot in _liShopItemSpots)
            {
                spot.ShowPrice = false;
                int distance = 0;
                if(PlayerManager.PlayerInRangeGetDist(spot.Box.Center, GameManager.TILE_SIZE * 2, ref distance))
                {
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closest = spot;
                    }
                }
            }

            if (closest != null)
            {
                closest.ShowPrice = true;
            }
        }

        public bool Interact(RHMap map, Point mouseLocation)
        {
            foreach (ShopItemSpot itemSpot in _liShopItemSpots)
            {
                if (itemSpot.Contains(mouseLocation) && PlayerManager.PlayerInRange(itemSpot.Box.Center, GameManager.TILE_SIZE * 2))
                {
                    if (map.ContainsActor(DataManager.DIVillagers[ShopkeeperID]))
                    {
                        itemSpot.Buy();
                        return true;
                    }
                    else { GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("BuyMerch_NoShopkeep")); }
                }
            }

            return false;
        }

        public void Purchase(Item buyMe)
        {
            if (PlayerManager.Money >= buyMe.Value)
            {
                PlayerManager.TakeMoney(buyMe.Value);
                if (buyMe.CompareType(ItemEnum.Blueprint) || buyMe.CompareType(ItemEnum.Tool) || buyMe.CompareType(ItemEnum.NPCToken))
                {
                    PlayerManager.AddToUniqueBoughtItems(buyMe.ItemID);
                    _liShopItemSpots.Find(x => x.MerchID == buyMe.ItemID).SetMerchandise(null);
                }
                InventoryManager.AddToInventory(buyMe);
            }
        }

        public void AddItemSpot(ShopItemSpot spot)
        {
            _liShopItemSpots.Add(spot);
        }

        public void PlaceStock()
        {
            int index = 0;
            _liShopItemSpots.ForEach(x => x.SetMerchandise(null));

            foreach (KeyValuePair<int, Merchandise> kvp in _diMerchandise)
            {
                if (PlayerManager.AlreadyBoughtUniqueItem(kvp.Value.MerchID)) { continue; }

                if (index < _liShopItemSpots.Count)
                {
                    _liShopItemSpots[index++].SetMerchandise(kvp.Value);
                }
                else { break; }
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
