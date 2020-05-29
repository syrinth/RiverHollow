using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Screens;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.DataManager;
using RiverHollow.GUIObjects;
using static RiverHollow.WorldObjects.WorldItem;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    class HUDCraftingDisplay : GUIObject
    {
        const int _iBoxSize = 64;
        const int _iMargin = 3;
        const int _iMaxColumns = 5;

        Machine _craftMachine;
        private GUIInventory _inventory;
        private GUIWindow _creationWindow;
        protected GUIItemBox[,] _arrDisplay;

        private int _rows;
        private int _columns;

        public HUDCraftingDisplay(Machine crafter)
        {
            _craftMachine = crafter;
            Setup(crafter.CraftList);

            //Need to set the Y component because the _inventory will be created at 0, 0
            //and the creation window will be put on top of it. Must set the Y before adding
            //the controls so that it doesn't move the Controls around.
            this.SetY(_creationWindow.Top);

            AddControl(_creationWindow);
            AddControl(_inventory);

            DetermineSize();
            CenterOnScreen();
        }

        /// <summary>
        /// Does the setup for the crafting window, determines what the crafter can make
        /// and creates the appropriate boxes forthem
        /// </summary>
        /// <param name="recipes"></param>
        public void Setup(Dictionary<int, int> recipes)
        {
            //Pause the game while crafting
            GameManager.Pause();

            List<int> canMake = new List<int>();
            foreach (int id in recipes.Keys)
            {
                //Ensure that either the creation of the item is enabled by a crafter or that the player knows the recipe themselves
                if (_craftMachine != null || PlayerManager.CanMake.Contains(id))
                {
                    canMake.Add(id);
                }
            }

            //Dynamically determines the number of columns that will be created, based off of the number of items able to be made
            _columns = (canMake.Count < _iMaxColumns) ? canMake.Count : _iMaxColumns;

            //If there are less recipes than max columns, we only have one row, othwerise weneed to figure out how many times the columns get divided into the total number
            _rows = (canMake.Count < _iMaxColumns) ? 1 : (int)(Math.Round(((double)(canMake.Count + _columns - 1) / (double)_columns)));

            //Set up the GUIInventory of the player
            _inventory = new GUIInventory(true);
            _inventory.Setup();

            //Determine how big the creation window needs to be
            int creationWidth = (GUIWindow.RedWin.Edge * 2) + (_columns * _iBoxSize) + (_iMargin * (_columns + 1));
            int creationHeight = (GUIWindow.RedWin.Edge * 2) + (_rows * _iBoxSize) + (_iMargin * (_rows + 1));

            //Create the creation window and align it on top of the GUIInventory
            _creationWindow = new GUIWindow(GUIWindow.RedWin, creationWidth, creationHeight);
            _creationWindow.AnchorAndAlignToObject(_inventory, SideEnum.Top, SideEnum.CenterX);

            int i = 0; int j = 0;
            List<GUIObject> boxes = new List<GUIObject>();
            foreach (int id in canMake)
            {
                boxes.Add(new GUIItemBox(new Rectangle(288, 32, 32, 32), _iBoxSize, _iBoxSize, @"Textures\Dialog", GetItem(id), true));
            }

            //Create a grid for the recipes to be dispplayed in
            CreateSpacedGrid(ref boxes, _creationWindow.InnerTopLeft() + new Vector2(_iMargin, _iMargin), _creationWindow.MidWidth()-2*_iMargin, _columns);

            //Create a new array of the appropriate size, then assign all of the boxes to the array
            //and turn off number draw as well as addingthem to the Controls
            _arrDisplay = new GUIItemBox[_columns, _rows];
            foreach (GUIObject g in boxes)
            {
                GUIItemBox box = (GUIItemBox)g;
                box.DrawNum = false;
                _arrDisplay[i, j] = box;
                i++;
                if (i == _columns)
                {
                    i = 0;
                    j++;
                }

                _creationWindow.AddControl(g);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_creationWindow.Contains(mouse))
            {
                foreach (GUIItemBox gIB in _arrDisplay)
                {
                    if (gIB != null && gIB.Contains(mouse))
                    {
                        //Check that all required items are there first
                        bool create = true;
                        foreach(KeyValuePair<int, int> kvp in gIB.Item.GetIngredients())
                        {
                            if(!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value))
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
                                    GUIManager.CloseMainObject();
                                    GameManager.Unpause();
                                }
                                else {
                                    InventoryManager.AddToInventory(gIB.Item.ItemID);
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
                GUIManager.CloseMainObject();
                GameManager.Unpause();
                rv = true;
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_creationWindow.Contains(mouse))
            {
                foreach (GUIItemBox gIB in _arrDisplay)
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
    }
}
