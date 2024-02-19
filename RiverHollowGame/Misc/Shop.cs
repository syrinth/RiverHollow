using RiverHollow.Game_Managers;
using RiverHollow.Items;
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
        readonly int _iShopID;
        readonly int _iShopBuildingID = -1;
        readonly List<Merchandise> _liMerchandise;
        public int Count => _liMerchandise.Count;

        public Merchandise SelectedMerchandise { get; private set; }

        public Shop(int id, Dictionary<string, string> stringDictionary)
        {
            _iShopID = id;

            _iShopBuildingID = Util.AssignValue("BuildingID", stringDictionary);

            if (stringDictionary.ContainsKey("Shopkeeper"))
            {
                if (int.TryParse(stringDictionary["Shopkeeper"], out id))
                {
                    ShopkeeperID = id;
                }
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
            m?.Unlock();
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
            };
            return sData;
        }

        public void LoadData(ShopData data)
        {
            UnlockMerchandise(data.merchUnlockedString);
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
        public int Amount { get; } = 1;
        public Item MerchItem { get; private set; }

        private readonly int _iTaskReq = -1;

        public Merchandise(MerchTypeEnum type, string merchData)
        {
            MerchType = type;

            string[] data = Util.FindArguments(merchData);
            MerchID = int.Parse(data[0]);
            Price = int.Parse(data[1]);

            Amount = DataManager.GetIntByIDKey(MerchID, "Amount", (type == MerchTypeEnum.WorldObject ? DataType.WorldObject : DataType.Item), 1);

            if (data.Length > 2)
            {
                if (data[2].Equals("Unique")) { UniqueData = data[2]; }
                else if (data[2].Equals("Locked")) { _bLocked = true; }
            }
        }

        public void GenerateSaleItem() { MerchItem = DataManager.GetItem(ItemID, Amount); }
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
