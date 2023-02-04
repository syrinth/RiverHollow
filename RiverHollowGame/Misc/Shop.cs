﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
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
        int _iShopBuildingID = -1;
        public string RandomIndices { get; private set; } = string.Empty;
        List<ShopItemSpot> _liShopItemSpots;
        public IList<ShopItemSpot> ItemSpots => _liShopItemSpots.AsReadOnly();
        List<Merchandise> _liMerchandise;
        public int Count => _liMerchandise.Count;

        public Shop(int id, Dictionary<string, string> stringDictionary)
        {
            _iShopID = id;
            _liShopItemSpots = new List<ShopItemSpot>();

            Util.AssignValue(ref _iShopBuildingID, "BuildingID", stringDictionary);

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
                if (itemSpot.Contains(mouseLocation) && PlayerManager.PlayerInRange(itemSpot.Box.Center, Constants.TILE_SIZE * 2))
                {
                    if (TownManager.DIVillagers.ContainsKey(ShopkeeperID) && map.ContainsActor(TownManager.DIVillagers[ShopkeeperID]) ||
                        TownManager.DIMerchants.ContainsKey(ShopkeeperID) && map.ContainsActor(TownManager.DIMerchants[ShopkeeperID]))
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
            if (PlayerManager.Money >= purchaseItem.TotalBuyValue)
            {
                PlayerManager.TakeMoney(purchaseItem.TotalBuyValue);
                if (purchaseItem.IsUnique())
                {
                    PlayerManager.AddUniqueItemToList(purchaseItem.ID);
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
            RandomIndices = string.Empty;
        }
        public void Randomize()
        {
            ClearItemSpots();

            if (string.IsNullOrEmpty(RandomIndices))
            {
                int totalMerch = _liMerchandise.Count;

                List<int> indices = new List<int>();
                for (int i = 0; i < totalMerch; i++) { indices.Add(i); }

                for (int i = 0; i < TownManager.Market.ObjectInfo.Count && i < totalMerch; i++)
                {
                    int index = RHRandom.Instance().Next(indices.Count);
                    RandomIndices += indices[index] + "|";
                    indices.RemoveAt(index);
                }

                RandomIndices = RandomIndices.Remove(RandomIndices.Length - 1);
            }
        }

        public void PlaceStock(bool randomize)
        {
            _liShopItemSpots.ForEach(x => x.SetMerchandise(null));

            string[] random = Util.FindParams(RandomIndices);

            List<Merchandise> copy = new List<Merchandise>(_liMerchandise.Where(x => !PlayerManager.AlreadyBoughtUniqueItem(x.MerchID)));

            int totalMerch = copy.Count;
            for (int i = 0; i < _liShopItemSpots.Count && i < totalMerch; i++)
            {
                if (randomize && !string.IsNullOrEmpty(RandomIndices) && random.Length > i)
                {
                    Merchandise m = copy[int.Parse(random[i])];
                    _liShopItemSpots[i].SetMerchandise(m);
                }
                else { _liShopItemSpots[i].SetMerchandise(copy[i]); }
            }
        }

        public void CheckForUniqueItems()
        {
            for (int i = 0; i < _liShopItemSpots.Count; i++)
            {
                if (PlayerManager.AlreadyBoughtUniqueItem(_liShopItemSpots[i].MerchID))
                {
                    _liShopItemSpots[i].SetMerchandise(null);
                }
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

                if (index < _liMerchandise.Count - 1)
                {
                    index++;
                    value += "-";
                }
            }

            ShopData sData = new ShopData
            {
                shopID = _iShopID,
                merchUnlockedString = value,
                randomized = RandomIndices
            };
            return sData;
        }

        public void LoadData(ShopData data)
        {
            UnlockMerchandise(data.merchUnlockedString);
            RandomIndices = data.randomized;

            if (_iShopBuildingID != -1 && TownManager.TownObjectBuilt(_iShopBuildingID))
            {
                PlaceStock(false);
            }
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

            if (data.Length > 1)
            {
                if (data[1].Equals("Unique")) { UniqueData = data[1]; }
                else if (data[1].Equals("Locked")) { _bLocked = true; }
            }

            switch (MerchType)
            {
                case MerchTypeEnum.Item:
                    _iCost = int.Parse(DataManager.GetStringByIDKey(MerchID, "Value", DataType.Item));
                    break;
                case MerchTypeEnum.WorldObject:
                    _iCost = int.Parse(DataManager.GetStringByIDKey(MerchID, "Value", DataType.WorldObject));
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
