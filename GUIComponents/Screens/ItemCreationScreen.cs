
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.ObjectManager;
using RiverHollow.Characters.NPCs;
using RiverHollow.GUIObjects;
using static RiverHollow.WorldObjects.WorldItem.Machine;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    class ItemCreationScreen : GUIScreen
    {
        const int _iBoxSize = 64;
        const int _iMargin = 3;
        const int _iMaxColumns = 4;

        Crafter _craftMachine;
        WorldAdventurer _craftAdventurer;
        private Inventory _inventory;
        private GUIWindow _creationWindow;
        protected GUIItemBox[,] _displayList;

        private int _rows;
        private int _columns;

        public ItemCreationScreen()
        {
            Setup(ObjectManager.DictCrafting);

            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
        }

        public ItemCreationScreen(Crafter crafter)
        {
            _craftMachine = crafter;
            Setup(crafter.CraftList);

            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
        }

        public ItemCreationScreen(WorldAdventurer crafter)
        {
            _craftAdventurer = crafter;
            Setup(crafter.CraftList);

            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
        }

        public void Setup(Dictionary<int, Recipe> recipes)
        {
            GameManager.Pause();
            Vector2 centerPoint = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

            List<int> canMake = new List<int>();
            foreach (int id in recipes.Keys)
            {
                //Ensure that either the creation of the item is enabled by a crafter or that the player knows the recipe themselves
                if (_craftAdventurer != null || _craftMachine != null || PlayerManager.CanMake.Contains(id))
                {
                    canMake.Add(id);
                }
            }

            _columns = (canMake.Count < _iMaxColumns) ? canMake.Count : _iMaxColumns;
            _rows = (canMake.Count < _iMaxColumns) ? 1 : Math.Max(1, canMake.Count / _columns);

            _displayList = new GUIItemBox[_columns, _rows];
            _inventory = new Inventory(4, InventoryManager.maxItemColumns, 32);
            _inventory.Setup();

            int creationWidth = (GUIWindow.RedWin.Edge * 2) + (_columns * _iBoxSize) + (_iMargin * (_columns + 1));
            int creationHeight = (GUIWindow.RedWin.Edge * 2) + (_rows * _iBoxSize) + (_iMargin * (_rows + 1));

            _creationWindow = new GUIWindow(new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2), GUIWindow.RedWin, creationWidth, creationHeight);
            _creationWindow.AnchorAndAlignToObject(_inventory, GUIObject.SideEnum.Top, GUIObject.SideEnum.CenterX);

            List<GUIObject> liWins = new List<GUIObject>() { _creationWindow, _inventory };
            GUIObject.CenterAndAlignToScreen(ref liWins);

            int i = 0; int j = 0;
            List<GUIObject> boxes = new List<GUIObject>();
            foreach (int id in canMake)
            {
                boxes.Add(new GUIItemBox(Vector2.Zero, new Rectangle(288, 32, 32, 32), _iBoxSize, _iBoxSize, @"Textures\Dialog", ObjectManager.GetItem(id)));
            }

            GUIObject.CreateSpacedGrid(ref boxes, _creationWindow.InnerTopLeft() + new Vector2(_iMargin, _iMargin), _creationWindow.MidWidth()-2*_iMargin, _columns);

            foreach (GUIObject g in boxes)
            {
                _displayList[i, j] = (GUIItemBox)g;
                i++;
                if (i == _columns)
                {
                    i = 0;
                    j++;
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_creationWindow.Contains(mouse))
            {
                foreach (GUIItemBox gIB in _displayList)
                {
                    if (gIB != null && gIB.Contains(mouse))
                    {
                        //Check that all required items are there first
                        bool create = true;
                        foreach(KeyValuePair<int, int> kvp in ObjectManager.DictCrafting[gIB.Item.ItemID].RequiredItems)
                        {
                            if(!InventoryManager.HasItemInInventory(kvp.Key, kvp.Value))
                            {
                                create = false;
                            }
                        }
                        //If all items are found, then remove them.
                        if (create)
                        {
                            foreach (KeyValuePair<int, int> kvp in ObjectManager.DictCrafting[gIB.Item.ItemID].RequiredItems)
                            {
                                InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                                if (_craftMachine != null) {
                                    _craftMachine.MakeChosenItem(gIB.Item.ItemID);
                                    GameManager.BackToMain();
                                }
                                else if (_craftAdventurer != null)
                                {
                                    _craftAdventurer.ProcessChosenItem(gIB.Item.ItemID);
                                    GameManager.BackToMain();
                                }
                                else {
                                    InventoryManager.AddNewItemToInventory(gIB.Item.ItemID);
                                }
                            }
                        }
                    }
                }
                rv = true;
            }
            return rv;
        }
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (!_creationWindow.Contains(mouse))
            {
                GameManager.BackToMain();
                rv = true;
            }
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (GUIItemBox gIB in _displayList)
            {
                if (gIB != null)
                {
                    gIB.Draw(spriteBatch);
                }
            }
            foreach (GUIItemBox gIB in _displayList)
            {
                if (gIB != null)
                {
                    if (gIB.DrawDescription(spriteBatch))
                    {
                        break;
                    }
                }
            }
        }

        public override bool IsItemCreationScreen() { return true; }
    }
}
