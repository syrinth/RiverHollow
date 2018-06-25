using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects.Screens;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GUIObjects.ManagementScreen;

namespace RiverHollow.Characters.NPCs
{
    public class ShopKeeper : NPC
    {
        protected List<Merchandise> _liMerchandise;
        public List<Merchandise> Buildings { get => _liMerchandise; }

        public ShopKeeper(int index, string[] stringData)
        {
            _currentPath = new List<RHTile>();
            _liMerchandise = new List<Merchandise>();
            _collection = new Dictionary<int, bool>();
            _completeSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();

            LoadContent();

            _index = index;
            int i = ImportBasics(index, stringData);
            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("ShopData"))
                {
                    foreach (KeyValuePair<int, string> kvp in GameContentManager.GetMerchandise(tagType[1])) { 
                        _liMerchandise.Add(new Merchandise(kvp.Value));
                    }
                }
            }

            MapManager.Maps[CurrentMapName].AddCharacter(this);
        }

        public void Talk(bool IsOpen = false)
        {
            GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Talk;
            string text = string.Empty;
            if (!Introduced)
            {
                text = _dialogueDictionary["Introduction"];
                Introduced = true;
            }
            else
            {
                if (IsOpen)
                {
                    text = _dialogueDictionary["ShopOpen"];
                }
                else
                {
                    text = GetText();
                }
            }
            text = Util.ProcessText(text, _sName);
            GUIManager.SetScreen(new TextScreen(this, text));
        }

        public override string GetDialogEntry(string entry)
        {
            string rv = string.Empty;
            List<Merchandise> _liMerchandise = new List<Merchandise>();
            if (entry.Equals("BuyBuildings"))
            {
                foreach(Merchandise m in this._liMerchandise)
                {
                    if ((m.MerchType == Merchandise.ItemType.Building || m.MerchType == Merchandise.ItemType.Upgrade) && m.Activated()) { _liMerchandise.Add(m); }
                }
                
                GUIManager.SetScreen(new PurchaseBuildingsScreen(_liMerchandise));
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("BuyWorkers"))
            {
                foreach (Merchandise m in this._liMerchandise)
                {
                    if (m.MerchType == Merchandise.ItemType.Worker && m.Activated()) { _liMerchandise.Add(m); }
                }
                GUIManager.SetScreen(new PurchaseWorkersScreen(_liMerchandise));
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("BuyItems"))
            {
                foreach (Merchandise m in this._liMerchandise)
                {
                    if (m.MerchType == Merchandise.ItemType.Item && m.Activated()) { _liMerchandise.Add(m); }
                }
                GUIManager.SetScreen(new PurchaseItemsScreen(_liMerchandise));
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("SellWorkers"))
            {
                ManagementScreen s = new ManagementScreen();
                s.Sell();
                GUIManager.SetScreen(s);
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("Move"))
            {
                GUIManager.SetScreen(null);
                GameManager.Scry(true);
                GameManager.MoveBuilding();
                Camera.UnsetObserver();
                MapManager.ViewMap(MapManager.HomeMap);
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("UpgradeBuilding"))
            {
                ManagementScreen m = new ManagementScreen(ActionTypeEnum.Upgrade);
                GUIManager.SetScreen(m);
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("Destroy"))
            {
                GUIManager.SetScreen(null);
                GameManager.Scry(true);
                GameManager.DestroyBuilding();
                Camera.UnsetObserver();
                MapManager.ViewMap(MapManager.HomeMap);
                GameManager.ClearGMObjects();
            }
            else
            {
                rv =  base.GetDialogEntry(entry);
            }

            return rv;
        }
    }

    public class Merchandise
    {
        string _sUniqueData;
        public string UniqueData => _sUniqueData;
        public enum ItemType { Building, Worker, Item, Upgrade }
        public ItemType MerchType;
        int _merchID = -1;
        public int MerchID { get => _merchID; }
        string _description;
        int _moneyCost;
        public int MoneyCost { get => _moneyCost; }
        int _iQuestReq = -1;

        List<KeyValuePair<int, int>> _items; //item, then num required
        public List<KeyValuePair<int, int>> RequiredItems { get => _items; }

        public Merchandise(string data)
        {
            _items = new List<KeyValuePair<int, int>>();
            string[] dataValues = data.Split('/');

            int i = 0;
            if (dataValues[0] == "Building")
            {
                MerchType = ItemType.Building;
                i = 1;
                _merchID = int.Parse(dataValues[i++]);
                _description = dataValues[i++];
                _moneyCost = int.Parse(dataValues[i++]);

                string[] reqItems = dataValues[i++].Split(':');
                foreach (string str in reqItems)
                {
                    string[] itemsSplit = str.Split(' ');
                    _items.Add(new KeyValuePair<int, int>(int.Parse(itemsSplit[0]), int.Parse(itemsSplit[1])));
                }
            }
            else if (dataValues[0] == "Worker")
            {
                MerchType = ItemType.Worker;
                i = 1;
                _merchID = int.Parse(dataValues[i++]);
                _description = dataValues[i++];
                _moneyCost = int.Parse(dataValues[i++]);
            }
            else if (dataValues[0] == "Item")
            {
                MerchType = ItemType.Item;
                i = 1;
                string[] itemData = dataValues[i++].Split('-');
                _merchID = int.Parse(itemData[0]);
                if(itemData.Length > 1) { _sUniqueData = itemData[1]; }
                _moneyCost = int.Parse(dataValues[i++]);
                if (dataValues.Length >= i + 1)
                {
                    _iQuestReq = int.Parse(dataValues[i++]);
                }
            }
            else if (dataValues[0] == "Upgrade")
            {
                MerchType = ItemType.Upgrade;
                i = 1;
                _merchID = int.Parse(dataValues[i++]);
                _description = dataValues[i++];
                _moneyCost = int.Parse(dataValues[i++]);

                string[] reqItems = dataValues[i++].Split(':');
                foreach (string str in reqItems)
                {
                    string[] itemsSplit = str.Split(' ');
                    _items.Add(new KeyValuePair<int, int>(int.Parse(itemsSplit[0]), int.Parse(itemsSplit[1])));
                }
            }
            else
            {
                int huff = 0;
            }
        }

        public bool Activated()
        {
            bool rv = false;
            rv = _iQuestReq == -1 || GameManager.DIQuests[_iQuestReq].Finished;
            return rv;
        }
    }
}
