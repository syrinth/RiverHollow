using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Game_Managers.GUIObjects.Screens;
using RiverHollow.GUIObjects;

using System.Collections.Generic;

namespace RiverHollow.Characters.NPCs
{
    public class ShopKeeper : NPC
    {
        protected List<Merchandise> _merchandise;
        public List<Merchandise> Buildings { get => _merchandise; }

        public ShopKeeper(int index, string[] data)
        {
            _collection = new Dictionary<int, bool>();
            LoadContent();
            if (data.Length >= 5)
            {
                _index = index;
                int i = ImportBasics(data);
                _merchandise = new List<Merchandise>();
                while (i < data.Length)
                {
                    foreach (KeyValuePair<int, string> kvp in GameContentManager.GetMerchandise(data[i++]))
                    {
                        _merchandise.Add(new Merchandise(kvp.Value));
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
            text = ProcessText(text);
            GUIManager.SetScreen(new TextScreen(this, text));
        }

        public override string GetDialogEntry(string entry)
        {
            string rv = string.Empty;
            List<Merchandise> dialogueEntries = new List<Merchandise>();
            if (entry.Equals("BuyBuildings"))
            {
                foreach(Merchandise m in _merchandise)
                {
                    if (m._type == Merchandise.ItemType.Building) { dialogueEntries.Add(m); }
                }
                
                GUIManager.SetScreen(new PurchaseBuildingsScreen(dialogueEntries));
            }
            else if (entry.Equals("BuyWorkers"))
            {
                foreach (Merchandise m in _merchandise)
                {
                    if (m._type == Merchandise.ItemType.Worker) { dialogueEntries.Add(m); }
                }
                GUIManager.SetScreen(new PurchaseWorkersScreen(dialogueEntries));
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
        public enum ItemType { Building, Worker, Item }
        public ItemType _type;
        int _merchID = -1;
        public int MerchID { get => _merchID; }
        string _description;
        int _moneyCost;
        public int MoneyCost { get => _moneyCost; }
        List<KeyValuePair<int, int>> _items; //item, then num required
        public List<KeyValuePair<int, int>> RequiredItems { get => _items; }

        public Merchandise(string data)
        {
            _items = new List<KeyValuePair<int, int>>();
            string[] dataValues = data.Split('/');

            int i = 0;
            if (dataValues[0] == "Building")
            {
                _type = ItemType.Building;
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
                _type = ItemType.Worker;
                i = 1;
                _merchID = int.Parse(dataValues[i++]);
                _description = dataValues[i++];
                _moneyCost = int.Parse(dataValues[i++]);
            }
            else
            {
                _merchID = int.Parse(dataValues[i++]);
                i++;
                _description = dataValues[i++];
                _moneyCost = int.Parse(dataValues[i++]);
            }
        }
    }
}
