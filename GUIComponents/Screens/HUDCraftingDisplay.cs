using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;

using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.Items.WorldItem;
using static RiverHollow.Items.WorldItem.Machine;

namespace RiverHollow.GUIComponents.Screens
{
    class HUDCraftingDisplay : GUIMainObject
    {
        int _iBoxSize = GUIItemBox.RECT_IMG.Width * (int)GameManager.Scale;
        const int _iMaxColumns = 5;

        Machine _craftMachine;
        private GUIWindow _winCraftables;
        private GUIWindow _winMachineInfo;
        protected GUIItemBox[,] _arrDisplay;
        protected Item _autoItem;

        private List<GUIItemReq> _liRequiredItems;
        private GUIText _gName;
        private GUIText _gDescription;

        private int _iSelectedItemID = -1;

        private int _rows;
        private int _columns;

        public HUDCraftingDisplay(CraftingMachine crafter)
        {
            _winMain = SetMainWindow();

            _liRequiredItems = new List<GUIItemReq>();
            _craftMachine = crafter;
            _autoItem = DataManager.GetItem(crafter.AutomatedItem);
            _gName = new GUIText("");
            _gDescription = new GUIText("");

            Setup(crafter.CraftingDictionary);
            _winCraftables.AnchorAndAlignToObject(_winMain, SideEnum.Top, SideEnum.CenterX);

            ConfigureInfo();

            AddControl(_winCraftables);

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


            //Determine how big the creation window needs to be
            int creationWidth = (GUIWindow.RedWin.WidthEdges()) + (_columns * _iBoxSize) + (GUIManager.STANDARD_MARGIN * (_columns + 1));
            int creationHeight = (GUIWindow.RedWin.HeightEdges()) + (_rows * _iBoxSize) + (GUIManager.STANDARD_MARGIN * (_rows + 1));

            //Create the creation window
            _winCraftables = new GUIWindow(GUIWindow.RedWin, creationWidth, creationHeight);

            int i = 0; int j = 0;
            List<GUIObject> boxes = new List<GUIObject>();
            foreach (int id in canMake)
            {
                boxes.Add(new GUIItemBox(@"Textures\Dialog", GetItem(id), true));
            }

            //Create a grid for the recipes to be dispplayed in
            CreateSpacedGrid(ref boxes, _winCraftables.InnerTopLeft() + new Vector2(GUIManager.STANDARD_MARGIN, GUIManager.STANDARD_MARGIN), _winCraftables.MidWidth() - 2 * GUIManager.STANDARD_MARGIN, _columns);

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

                _winCraftables.AddControl(g);
            }
        }

        public void ConfigureInfo()
        {
            _gName.AnchorToInnerSide(_winMain, SideEnum.TopLeft, 4);
            _gDescription.AnchorAndAlignToObject(_gName, SideEnum.Bottom, SideEnum.Left);

            for(int i=0; i <_liRequiredItems.Count; i++)
            {
                GUIItemReq req = _liRequiredItems[i];
                if (i == 0) { req.AnchorToInnerSide(_winMain, SideEnum.BottomLeft, 10); }
                else { req.AnchorAndAlignToObject(_liRequiredItems[i - 1], SideEnum.Right, SideEnum.Bottom); }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_winCraftables.Contains(mouse))
            {
                foreach (GUIItemBox gIB in _arrDisplay)
                {
                    if (gIB != null && gIB.Contains(mouse))
                    {
                        //Check that all required items are there first
                        bool create = true;
                        foreach(KeyValuePair<int, int> kvp in gIB.Item.GetRequiredItems())
                        {
                            if(!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value))
                            {
                                create = false;
                            }
                        }
                        //If all items are found, then remove them.
                        if (create)
                        {
                            foreach (KeyValuePair<int, int> kvp in gIB.Item.GetRequiredItems())
                            {
                                InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                                if (_craftMachine != null) {
                                    _craftMachine.MakeChosenItem(gIB.Item.ItemID);
                                    GUIManager.CloseMainObject();
                                    GameManager.Unpause();
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
            if (!_winCraftables.Contains(mouse))
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
            if (_winCraftables.Contains(mouse))
            {
                foreach (GUIItemBox gIB in _arrDisplay)
                {
                    if (gIB != null && gIB.Contains(mouse))
                    {
                        Item chosenItem = gIB.Item;
                        if(chosenItem.ItemID != _iSelectedItemID)
                        {
                            foreach (GUIItemReq r in _liRequiredItems) { _winMain.RemoveControl(r); }

                            _liRequiredItems.Clear();
                            _iSelectedItemID = chosenItem.ItemID;
                            foreach (KeyValuePair<int, int> kvp in chosenItem.GetRequiredItems())
                            {
                                _liRequiredItems.Add(new GUIItemReq(kvp.Key, kvp.Value));
                            }

                            _gName.SetText(chosenItem.Name);
                            _gDescription.SetText(chosenItem.GetDescription());

                            ConfigureInfo();
                        }
                    }
                }
            }

            return rv;
        }
    }
}
