using Adventure.Game_Managers;
using Adventure.GUIObjects;
using Adventure.Items;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Adventure.Characters.NPCs
{
    public class ShopKeeper : NPC
    {
        protected bool _isOpen;
        public bool IsOpen { get => _isOpen; set => _isOpen = value; }

        protected List<Merchandise> _merchandise;
        public List<Merchandise> Buildings { get => _merchandise; }

        public ShopKeeper(int index, string[] data)
        {
            LoadContent();
            if (data.Length >= 5)
            {
                int i = LoadBasic(data);

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

                _schedule = new List<Vector2>
                {
                    new Vector2(Position.X - 100, Position.Y + 100),
                    Position
                };
                _moveTo = _schedule[0];
                IsOpen = true;

                MapManager.Maps[_currentMap].AddCharacter(this);
            }
        }

        public override void Talk()
        {
            GraphicCursor._currentType = GraphicCursor.CursorType.Talk;
            string text = string.Empty;
            if (CharacterManager._talkedTo.ContainsKey(_name) && CharacterManager._talkedTo[_name] == false)
            {
                text = _dialogueDictionary["Introduction"];
                CharacterManager._talkedTo[_name] = true;
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
            GUIManager.LoadScreen(GUIManager.Screens.Text, this, text);
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
                
                GUIManager.LoadScreen(GUIManager.Screens.BuildingShop, dialogueEntries);
            }
            else if (entry.Equals("BuyWorkers"))
            {
                foreach (Merchandise m in _merchandise)
                {
                    if (m._type == Merchandise.ItemType.Worker) { dialogueEntries.Add(m); }
                }
                GUIManager.LoadScreen(GUIManager.Screens.WorkerShop, dialogueEntries);
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
