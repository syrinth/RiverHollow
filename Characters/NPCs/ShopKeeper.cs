using Adventure.Buildings;
using Adventure.Game_Managers;
using Adventure.Items;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using BuildingID = Adventure.Game_Managers.ObjectManager.BuildingID;
using WorkerID = Adventure.Game_Managers.ObjectManager.WorkerID;
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
            List<Merchandise> send = new List<Merchandise>();
            if (entry.Equals("BuyBuildings"))
            {
                foreach(Merchandise m in _merchandise)
                {
                    if (m.BuildingID != ObjectManager.BuildingID.Nothing) { send.Add(m); }
                }
                
                AdventureGame.ChangeGameState(AdventureGame.GameState.Running);
                GUIManager.LoadScreen(GUIManager.Screens.BuildingShop, send);
            }
            else if (entry.Equals("BuyWorkers"))
            {
                foreach (Merchandise m in _merchandise)
                {
                    if (m.WorkerID != ObjectManager.WorkerID.Nothing) { send.Add(m); }
                }
                AdventureGame.ChangeGameState(AdventureGame.GameState.Running);
                GUIManager.LoadScreen(GUIManager.Screens.WorkerShop, send);
                //else if (m.ItemID != -1) { send.Add(m); }
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
        BuildingID _buildingID;
        public BuildingID BuildingID { get => _buildingID; }
        WorkerID _workerID;
        public WorkerID WorkerID { get => _workerID; }
        int _itemID = -1;
        public int ItemID { get => _itemID; }
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
            if (!Enum.TryParse(dataValues[i], out _buildingID))
            {
                if (!Enum.TryParse(dataValues[i], out _workerID))
                {
                    _itemID = int.Parse(dataValues[i]);
                }
                i++;
                _description = dataValues[i++];
                _moneyCost = int.Parse(dataValues[i++]);
            }
            else
            {
                i++;
                _description = dataValues[i++];
                _moneyCost = int.Parse(dataValues[i++]);

                string[] reqItems = dataValues[i++].Split(':');
                foreach (string str in reqItems)
                {
                    string[] itemsSplit = str.Split(' ');
                    _items.Add(new KeyValuePair<int, int>(int.Parse(itemsSplit[0]), int.Parse(itemsSplit[1])));
                }
            }
        }
    }
}
