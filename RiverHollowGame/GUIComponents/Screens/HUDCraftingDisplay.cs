using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.DataManager;
using RiverHollow.Items;

namespace RiverHollow.GUIComponents.Screens
{
    class HUDCraftingDisplay : GUIMainObject
    {
        int _iBoxSize = GUIItemBox.RECT_IMG.Width * (int)GameManager.CurrentScale;
        const int _iMaxColumns = 5;

        Machine _craftMachine;
        private GUIWindow _winCraftables;
        private GUIWindow _winExtra;
        private GUIButton _btnFinished;
        protected GUIItemBox[,] _arrDisplay;

        private List<GUIItem> _liRequiredItems;
        private GUIText _gName;
        private GUIText _gDescription;

        private int _iSelectedItemID = -1;

        private int _rows;
        private int _columns;

        public HUDCraftingDisplay(Machine crafter)
        {
            _winMain = SetMainWindow();
            _winMain.Height = _winMain.Height / 2;

            _liRequiredItems = new List<GUIItem>();
            _craftMachine = crafter;
            _gName = new GUIText("");
            _gDescription = new GUIText("");

            Setup(crafter.CraftingDictionary);
            AddControl(_winCraftables);

            ConfigureInfo();

            _winExtra = new GUIWindow(GUIWindow.Window_1, _winMain.Width, ScaleIt(GUIWindow.Window_1.HeightEdges()) + ScaleIt(TILE_SIZE));
            _btnFinished = new GUIButton("Done", Finished);
            _btnFinished.CenterOnObject(_winExtra);
            _winExtra.AddControl(_btnFinished);

            _winCraftables.AnchorAndAlignToObject(_winMain, SideEnum.Top, SideEnum.CenterX);
            _winExtra.AnchorAndAlignToObject(_winMain, SideEnum.Bottom, SideEnum.CenterX);

            DetermineSize();

            AddControl(_winExtra);
            Position();
        }

        /// <summary>
        /// Does the setup for the crafting window, determines what the crafter can make
        /// and creates the appropriate boxes forthem
        /// </summary>
        /// <param name="recipes"></param>
        public void Setup(Dictionary<int, int> recipes)
        {
            List<int> canMake = new List<int>();
            foreach (int id in recipes.Keys)
            {
                //Ensure that either the creation of the item is enabled by a crafter or that the player knows the recipe themselves
                if (_craftMachine != null)
                {
                    canMake.Add(id);
                }
            }

            //Dynamically determines the number of columns that will be created, based off of the number of items able to be made
            _columns = (canMake.Count < _iMaxColumns) ? canMake.Count : _iMaxColumns;

            //If there are less recipes than max columns, we only have one row, othwerise weneed to figure out how many times the columns get divided into the total number
            _rows = (canMake.Count < _iMaxColumns) ? 1 : (int)(Math.Round(((double)(canMake.Count + _columns - 1) / (double)_columns)));


            //Determine how big the creation window needs to be
            int creationWidth = (GUIWindow.Window_1.WidthEdges()) + (_columns * _iBoxSize) + (GUIManager.STANDARD_MARGIN * (_columns + 1));
            int creationHeight = (GUIWindow.Window_1.HeightEdges()) + (_rows * _iBoxSize) + (GUIManager.STANDARD_MARGIN * (_rows + 1));

            //Create the creation window
            _winCraftables = new GUIWindow(GUIWindow.Window_1, creationWidth, creationHeight);

            int i = 0; int j = 0;
            List<GUIObject> boxes = new List<GUIObject>();
            foreach (int id in canMake)
            {
                boxes.Add(new GUIItemBox(DataManager.DIALOGUE_TEXTURE, GetItem(id), true));
            }

            //Create a grid for the recipes to be dispplayed in
            CreateSpacedGrid(ref boxes, _winCraftables.InnerTopLeft() + new Vector2(GUIManager.STANDARD_MARGIN, GUIManager.STANDARD_MARGIN), _winCraftables.InnerWidth() - 2 * GUIManager.STANDARD_MARGIN, _columns);

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
            _gName.AnchorToInnerSide(_winMain, SideEnum.TopLeft, ScaleIt(1));
            _gDescription.AnchorAndAlignToObject(_gName, SideEnum.Bottom, SideEnum.Left);

            for(int i=0; i <_liRequiredItems.Count; i++)
            {
                GUIItem req = _liRequiredItems[i];
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
                        _craftMachine?.AttemptToCraftChosenItem(gIB.BoxItem);
                    }
                }
                rv = true;
            }
            else if (_winExtra.Contains(mouse))
            {
                _winExtra.ProcessLeftButtonClick(mouse);
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
                        Item chosenItem = gIB.BoxItem;
                        if (chosenItem.ItemID != _iSelectedItemID)
                        {
                            foreach (GUIItem r in _liRequiredItems) { _winMain.RemoveControl(r); }

                            _liRequiredItems.Clear();
                            _iSelectedItemID = chosenItem.ItemID;
                            foreach (KeyValuePair<int, int> kvp in chosenItem.GetRequiredItems())
                            {
                                GUIItem newItem = new GUIItem(DataManager.GetItem(kvp.Key, kvp.Value));
                                if(!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value))
                                {
                                    newItem.SetColor(Color.Red);
                                }
                                _liRequiredItems.Add(newItem);
                            }

                            _gName.SetText(chosenItem.Name());
                            
                            _gDescription.SetText(_gDescription.ParseText(chosenItem.Description(), _winMain.InnerWidth(), 5)[0]);

                            ConfigureInfo();
                        }
                    }
                }
            }
            else
            {
                foreach (GUIItem r in _liRequiredItems)
                {
                    r.ProcessHover(mouse);
                }
            }

            return rv;
        }

        private void Finished()
        {
            GUIManager.CloseMainObject();
        }
    }
}
