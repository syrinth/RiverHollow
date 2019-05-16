
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.ObjectManager;
using RiverHollow.GUIObjects;
using static RiverHollow.WorldObjects.WorldItem.Machine;
using static RiverHollow.GUIObjects.GUIObject;
using RiverHollow.Actors;
using static RiverHollow.WorldObjects.WorldItem;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    class CraftingScreen : GUIScreen
    {
        const int _iBoxSize = 64;
        const int _iMargin = 3;
        const int _iMaxColumns = 5;

        Machine _craftMachine;
        WorldAdventurer _craftAdventurer;
        private Inventory _inventory;
        private GUIWindow _creationWindow;
        protected GUIItemBox[,] _liDisplay;

        private int _rows;
        private int _columns;

        public CraftingScreen()
        {
            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
        }

        public CraftingScreen(Machine crafter)
        {
            _craftMachine = crafter;
            Setup(crafter.CraftList);

            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
        }

        public CraftingScreen(WorldAdventurer crafter)
        {
            _craftAdventurer = crafter;
            Setup(crafter.CraftList);

            Controls.Add(_creationWindow);
            Controls.Add(_inventory);
        }

        public void Setup(Dictionary<int, int> recipes)
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
            _rows = (canMake.Count < _iMaxColumns) ? 1 : (int)(Math.Round(((double)(canMake.Count + _columns - 1) / (double)_columns)));

            _liDisplay = new GUIItemBox[_columns, _rows];
            _inventory = new Inventory(4, InventoryManager.maxItemColumns, 32);
            _inventory.Setup();

            int creationWidth = (GUIWindow.RedWin.Edge * 2) + (_columns * _iBoxSize) + (_iMargin * (_columns + 1));
            int creationHeight = (GUIWindow.RedWin.Edge * 2) + (_rows * _iBoxSize) + (_iMargin * (_rows + 1));

            _creationWindow = new GUIWindow(GUIWindow.RedWin, creationWidth, creationHeight);
            _creationWindow.AnchorAndAlignToObject(_inventory, SideEnum.Top, SideEnum.CenterX);

            List<GUIObject> liWins = new List<GUIObject>() { _creationWindow, _inventory };
            CenterAndAlignToScreen(ref liWins);

            int i = 0; int j = 0;
            List<GUIObject> boxes = new List<GUIObject>();
            foreach (int id in canMake)
            {
                boxes.Add(new GUIItemBox(new Rectangle(288, 32, 32, 32), _iBoxSize, _iBoxSize, @"Textures\Dialog", GetItem(id), true));
            }

            CreateSpacedGrid(ref boxes, _creationWindow.InnerTopLeft() + new Vector2(_iMargin, _iMargin), _creationWindow.MidWidth()-2*_iMargin, _columns);

            foreach (GUIObject g in boxes)
            {
                GUIItemBox box = (GUIItemBox)g;
                box.DrawNum = false;
                _liDisplay[i, j] = box;
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
                foreach (GUIItemBox gIB in _liDisplay)
                {
                    if (gIB != null && gIB.Contains(mouse))
                    {
                        //Check that all required items are there first
                        bool create = true;
                        foreach(KeyValuePair<int, int> kvp in gIB.Item.GetIngredients())
                        {
                            if(!InventoryManager.HasItemInInventory(kvp.Key, kvp.Value))
                            {
                                create = false;
                            }
                        }
                        //If all items are found, then remove them.
                        if (create)
                        {
                            foreach (KeyValuePair<int, int> kvp in gIB.Item.GetIngredients())
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

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_creationWindow.Contains(mouse))
            {
                foreach (GUIItemBox gIB in _liDisplay)
                {
                    if (gIB != null)
                    {
                        rv = gIB.ProcessHover(mouse);
                        if (rv) { break; }
                    }
                }
            }
            rv = rv || _inventory.ProcessHover(mouse);

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (GUIItemBox gIB in _liDisplay)
            {
                if (gIB != null)
                {
                    gIB.Draw(spriteBatch);
                }
            }
        }

        public override bool IsItemCreationScreen() { return true; }
    }
}
