﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
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
        List<RHTile> _liShopObjectSpots;
        public IList<ShopItemSpot> ItemSpots => _liShopItemSpots.AsReadOnly();
        List<Merchandise> _liMerchandise;
        public int Count => _liMerchandise.Count;

        public Merchandise SelectedMerchandise { get; private set; }

        public Shop(int id, Dictionary<string, string> stringDictionary)
        {
            _iShopID = id;
            _liShopItemSpots = new List<ShopItemSpot>();
            _liShopObjectSpots = new List<RHTile>();

            _iShopBuildingID = Util.AssignValue("BuildingID", stringDictionary);

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

        public void SetSelectedMerchandise(Merchandise m)
        {
            SelectedMerchandise = m;
            SelectedMerchandise.GenerateSaleItem();
        }

        public bool Interact(RHMap map, Point mouseLocation)
        {
            foreach (ShopItemSpot itemSpot in _liShopItemSpots)
            {
                if (itemSpot.Contains(mouseLocation) && PlayerManager.InRangeOfPlayer(itemSpot.Box))
                {
                    if (TownManager.DIVillagers.ContainsKey(ShopkeeperID) && map.ContainsActor(TownManager.DIVillagers[ShopkeeperID]) ||
                        TownManager.DIMerchants.ContainsKey(ShopkeeperID) && map.ContainsActor(TownManager.DIMerchants[ShopkeeperID]))
                    {
                        itemSpot.Buy();
                        return true;
                    }
                    else { GUIManager.OpenTextWindow("BuyMerch_NoShopkeep"); }
                }
            }

            RHTile tile = map.GetMouseOverTile();
            WorldObject obj = tile.WorldObject;
            if (tile != null && obj != null && obj.ShopItem)
            {
                var merch = _liMerchandise.Find(x => x.MerchID == obj.ID);
                var item = merch.MerchItem;
                if (PlayerManager.Money < item.Value)
                {
                    GUIManager.OpenTextWindow("BuyMerch_NoMoney");
                }
                else if (!InventoryManager.HasSpaceInInventory(item.ID, 1))
                {
                    GUIManager.OpenTextWindow("BuyMerch_NoSpace");
                }
                else
                {
                    GUIManager.CloseHoverWindow();
                    SetSelectedMerchandise(merch);
                    if (!item.Stacks())
                    {
                        GUIManager.OpenTextWindow("BuyMerch_Confirm", item.Name(), merch.Price);
                    }
                    else
                    {
                        GUIManager.OpenMainObject(new QuantityWindow());
                    }
                }
            }

            return false;
        }

        public void Purchase(Merchandise merch)
        {
            if (PlayerManager.Money >= merch.TotalPrice)
            {
                PlayerManager.TakeMoney(merch.TotalPrice);
                if (merch.MerchItem.IsUnique())
                {
                    PlayerManager.AddUniqueItemToList(merch.MerchID);
                }

                InventoryManager.AddToInventory(merch.MerchItem);

                if (merch.MerchItem.CompareType(ItemEnum.Tool))
                {
                    merch.MerchItem.StrikeAPose();
                }

                merch.CleanSaleItem();
            }
        }

        public void AddObjectSpot(RHTile t)
        {
            _liShopObjectSpots.Add(t);
        }
        public void ClearObjectSpots()
        {
            _liShopObjectSpots.Clear();
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
                    RandomIndices += indices[index] + "/";
                    indices.RemoveAt(index);
                }

                RandomIndices = RandomIndices.Remove(RandomIndices.Length - 1);
            }
        }

        public void PlaceStock(bool randomize)
        {
            _liShopItemSpots.ForEach(x => x.SetMerchandise(null));
            _liShopObjectSpots.ForEach(x => x.WorldObject?.RemoveSelfFromTiles());

            string[] random = Util.FindParams(RandomIndices);

            List<Merchandise> copies = new List<Merchandise>(_liMerchandise.Where(x => !PlayerManager.AlreadyBoughtUniqueItem(x.MerchID) && x.MerchType == Merchandise.MerchTypeEnum.Item && ValidateMerchandise(x.MerchID)));

            int totalMerch = copies.Count;
            for (int i = 0; i < _liShopItemSpots.Count && i < totalMerch; i++)
            {
                if (randomize && !string.IsNullOrEmpty(RandomIndices) && random.Length > i)
                {
                    Merchandise m = copies[int.Parse(random[i])];
                    _liShopItemSpots[i].SetMerchandise(m);
                }
                else { _liShopItemSpots[i].SetMerchandise(copies[i]); }
            }

            copies = new List<Merchandise>(_liMerchandise.Where(x => x.MerchType == Merchandise.MerchTypeEnum.WorldObject));
            totalMerch = copies.Count;
            for (int i = 0; i < _liShopObjectSpots.Count && i < totalMerch; i++)
            {
                Merchandise m;

                if (randomize && !string.IsNullOrEmpty(RandomIndices) && random.Length > i) { m = copies[int.Parse(random[i])]; }
                else { m = copies[i]; }

                WorldObject obj = DataManager.CreateWorldObjectByID(m.MerchID);
                obj.SetShopItem();
                obj.PlaceOnMap(_liShopObjectSpots[i].Position, MapManager.Maps[_liShopObjectSpots[i].MapName]);
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
                if (m.Unlocked && ValidateMerchandise(m.MerchID)) { rv.Add(m); }
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
            string[] split = Util.FindArguments(unlockedMerchandise);
            for (int i = 0; i < _liMerchandise.Count; i++)
            {
                Merchandise m = _liMerchandise.Find(x => x.MerchID == i);
                if (i < split.Length && split[i].Equals("True") && m != null)
                {
                    m.Unlock();
                }
            }
        }

        public bool ValidateMerchandise(int id)
        {
            bool rv = true;

            if (DataManager.GetBoolByIDKey(id, "Season", DataType.Item))
            {
                rv = false;
                var seasons = DataManager.GetStringParamsByIDKey(id, "Season", DataType.Item);
                for (int i = 0; i < seasons.Count(); i++)
                {
                    if (Util.ParseEnum<SeasonEnum>(seasons[i]) == GameCalendar.CurrentSeason)
                    {
                        rv = true;
                        break;
                    }
                }
            }

            return rv;
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
        }
    }

    public class Merchandise
    {
        public enum MerchTypeEnum { Item, WorldObject };
        public MerchTypeEnum MerchType { get; }

        private bool _bLocked = false;
        public bool Unlocked => !_bLocked;
        public string UniqueData { get; }

        public int Price { get; private set; }
        public int TotalPrice => Price * MerchItem.Number;

        public int MerchID { get; } = -1;
        public int ItemID => MerchType == MerchTypeEnum.Item ? MerchID : MerchID + Constants.BUILDABLE_ID_OFFSET;
        public Item MerchItem { get; private set; }

        private readonly int _iTaskReq = -1;

        public Merchandise(MerchTypeEnum type, string merchData)
        {
            MerchType = type;

            string[] data = Util.FindArguments(merchData);
            MerchID = int.Parse(data[0]);
            Price = int.Parse(data[1]);

            if (data.Length > 2)
            {
                if (data[2].Equals("Unique")) { UniqueData = data[2]; }
                else if (data[2].Equals("Locked")) { _bLocked = true; }
            }
        }

        public void GenerateSaleItem() { MerchItem = DataManager.GetItem(ItemID); }
        public void CleanSaleItem() { MerchItem = null; }

        /// <summary>
        /// Call to unlock the Merchandise so that it can be purchased.
        /// </summary>
        public void Unlock()
        {
            _bLocked = false;
        }
    }
}
