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

        public ShopKeeper(int index, string[] data)
        {
            _collection = new Dictionary<int, bool>();
            _completeSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();
            _currentPath = new List<RHTile>();
            LoadContent();
            if (data.Length >= 5)
            {
                _index = index;
                int i = ImportBasics(data);
                _liMerchandise = new List<Merchandise>();
                while (i < data.Length)
                {
                    foreach (KeyValuePair<int, string> kvp in GameContentManager.GetMerchandise(data[i++]))
                    {
                        _liMerchandise.Add(new Merchandise(kvp.Value));
                    }
                }

                _dialogueDictionary = GameContentManager.LoadDialogue(@"Data\Dialogue\NPC" + index);
                _portrait = GameContentManager.GetTexture(@"Textures\portraits");

                MapManager.Maps[CurrentMapName].AddCharacter(this);
            }
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
                    if (m.MerchType == Merchandise.ItemType.Building && m.Activated()) { _liMerchandise.Add(m); }
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
            else if (entry.Contains("Upgrade"))
            {
                string upgradeWhat = entry.Remove(0, string.Format("Upgrade").Length);  //Removes Upgrade fromt he string to get what we're upgrading
                Upgrade theUpgrade = GameManager.DiUpgrades[upgradeWhat];
                bool create = true;
                create = PlayerManager.Money >= theUpgrade.MoneyCost;
                if (create)
                {
                    foreach(KeyValuePair<int, int> kvp in theUpgrade.LiRquiredItems)
                    {
                        if (!InventoryManager.HasItemInInventory(kvp.Key, kvp.Value))
                        {
                            create = false;
                        }
                    }
                }
                //If all items are found, then remove them.
                if (create)
                {
                    PlayerManager.TakeMoney(theUpgrade.MoneyCost);

                    foreach (KeyValuePair<int, int> kvp in theUpgrade.LiRquiredItems)
                    {
                        InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                    }
                    theUpgrade.Enabled = true;
                    GameManager.BackToMain();
                    GameManager.ClearGMObjects();
                }
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
        public enum ItemType { Building, Worker, Item }
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
            else
            {
                MerchType = ItemType.Item;
                string[] itemData = dataValues[i++].Split('-');
                _merchID = int.Parse(itemData[0]);
                if(itemData.Length > 1) { _sUniqueData = itemData[1]; }
                _moneyCost = int.Parse(dataValues[i++]);
                if (dataValues.Length >= i + 1)
                {
                    _iQuestReq = int.Parse(dataValues[i++]);
                }
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
