using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Items;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.Screens
{
    class HUDCraftingDisplay : GUIMainObject
    {
        private GUIImage _gSelection;
        private GUIImage _gComponents;
        private GUIWindow _winMaking;

        private GUIText _gName;
        private GUIButton _btnBuild;
        private GUIButton _btnLeft;
        private GUIButton _btnRight;
        private GUIItemBox[] _arrRecipes;
        private List<GUIItemBox> _liRequiredItems;
        private GUIItemBox[] _arrMaking;

        private Machine _objMachine;

        private int _iSelectedItemID = -1;

        public HUDCraftingDisplay(Machine m)
        {
            _liRequiredItems = new List<GUIItemBox>();
            _objMachine = m;
            _gName = new GUIText("");

            Setup(m.CraftingList);

            UpdateInfo(DataManager.CraftItem(_iSelectedItemID));

            DetermineSize();

            //AddControl(_winExtra);
            Position();
        }

        /// <summary>
        /// Does the setup for the crafting window, determines what the crafter can make
        /// and creates the appropriate boxes forthem
        /// </summary>
        /// <param name="recipes"></param>
        public void Setup(List<int> recipes)
        {
            int recipeNumber = Math.Min(recipes.Count, Constants.MAX_RECIPE_DISPLAY);
            _arrRecipes = new GUIItemBox[recipeNumber];
            _winMain = new GUIWindow(GUIWindow.Window_1, 10, 10);

            for (int i = 0; i < recipeNumber; i++)
            {
                Item recipe = DataManager.CraftItem(recipes[i]);
                GUIItemBox newBox = new GUIItemBox(recipe);
                newBox.DrawNumber(recipe.Number > 1);
                if (i == 0)
                {
                    _iSelectedItemID = recipe.ID;
                    newBox.AnchorToInnerSide(_winMain, SideEnum.TopLeft, ScaleIt(1));
                }
                else { newBox.AnchorAndAlignToObject(_arrRecipes[i - 1], SideEnum.Right, SideEnum.Top, ScaleIt(2)); }

                if (!InventoryManager.HasSufficientItems(recipe.GetRequiredItems()))
                {
                    newBox.Enable(false);
                }

                _arrRecipes[i] = newBox;
            }

            _gSelection = new GUIImage(new Rectangle(260, 0, 20, 20), DataManager.DIALOGUE_TEXTURE);
            _gSelection.CenterOnObject(_arrRecipes[0]);
            _winMain.AddControl(_gSelection);

            int widthBetween = (recipeNumber - 1) * ScaleIt(2);
            _winMain.Width = (recipeNumber * _arrRecipes[0].Width) + _winMain.WidthEdges() + ScaleIt(2) + widthBetween;
            _winMain.Height = _arrRecipes[0].Height + _winMain.HeightEdges() + ScaleIt(2);
            AddControl(_winMain);

            _gComponents = new GUIImage(new Rectangle(192, 0, 160, 71), DataManager.HUD_COMPONENTS);
            _winMain.MoveBy(new Vector2((_gComponents.Width - _winMain.Width) / 2, 0));
            _gComponents.AnchorAndAlignToObject(_winMain, SideEnum.Bottom, SideEnum.CenterX, ScaleIt(2));
            _gComponents.AddControl(_gName);

            _btnBuild = new GUIButton("Build", BtnBuild);
            _btnBuild.Position(_gComponents);
            _btnBuild.AlignToObject(_gComponents, SideEnum.CenterX);
            _btnBuild.ScaledMoveBy(0, 49);

            _gComponents.AddControl(_btnBuild);

            AddControl(_gComponents);

            // Making Window
            _winMaking = new GUIWindow(GUIWindow.Window_1, 10, 10);
            _arrMaking = new GUIItemBox[_objMachine.Capacity];
            for (int i = 0; i < _arrMaking.Length; i++)
            {
                if (_objMachine.CurrentlyMaking == -1) { _arrMaking[i] = new GUIItemBox(); }
                else { _arrMaking[i] = new GUIItemBox(DataManager.CraftItem(_objMachine.CurrentlyMaking)); }

                if (i == 0) { _arrMaking[i].AnchorToInnerSide(_winMaking, SideEnum.TopLeft, ScaleIt(1)); }
                else { _arrMaking[i].AnchorAndAlignToObject(_arrMaking[i - 1], SideEnum.Right, SideEnum.Top, ScaleIt(2)); }

            }

            widthBetween = (_objMachine.Capacity - 1) * ScaleIt(2);
            _winMaking.Width = (_objMachine.Capacity * _arrRecipes[0].Width) + _winMain.WidthEdges() + ScaleIt(2) + widthBetween;
            _winMaking.Height = _arrRecipes[0].Height + _winMain.HeightEdges() + ScaleIt(2);

            _winMaking.AnchorAndAlignToObject(_gComponents, SideEnum.Bottom, SideEnum.CenterX, ScaleIt(2));
            AddControl(_winMaking);

            if (!_objMachine.CraftDaily)
            {
                _winMaking.Show(false);
            }

            Width = _gComponents.Width;
            Height = _winMaking.Bottom - _winMain.Top;

            CenterOnScreen();

            _btnLeft = new GUIButton(new Rectangle(163, 43, 10, 12), DataManager.HUD_COMPONENTS, ShiftLeft);
            _btnLeft.AnchorAndAlignToObject(_winMain, SideEnum.Left, SideEnum.CenterY);
            AddControl(_btnLeft);

            _btnRight = new GUIButton(new Rectangle(173, 43, 10, 12), DataManager.HUD_COMPONENTS, ShiftRight);
            _btnRight.AnchorAndAlignToObject(_winMain, SideEnum.Right, SideEnum.CenterY);
            AddControl(_btnRight);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (_winMain.Contains(mouse))
            {
                rv = true;

                for (int i = 0; i < _arrRecipes.Length; i++)
                {
                    if (_arrRecipes[i].Contains(mouse))
                    {
                        if (_arrRecipes[i].BoxItem.ID != _iSelectedItemID)
                        {
                            _gSelection.CenterOnObject(_arrRecipes[i]);
                            UpdateInfo(_arrRecipes[i].BoxItem);

                            break;
                        }
                    }
                }
            }
            else if (_gComponents.Contains(mouse))
            {
                rv = true;
                _btnBuild.ProcessLeftButtonClick(mouse);
            }
            else if (_winMaking.Contains(mouse)) { rv = true; }

            return rv;
        }
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (!_gComponents.Contains(mouse) && !_winMain.Contains(mouse) && !_winMaking.Contains(mouse))
            {
                GUIManager.CloseMainObject();
                rv = true;
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            //if (_winCraftables.Contains(mouse))
            //{
            //    foreach (GUIItemBox gIB in _arrDisplay)
            //    {
            //        if (gIB != null && gIB.Contains(mouse))
            //        {
            //            Item chosenItem = gIB.BoxItem;
            //            if (chosenItem.ID != _iSelectedItemID)
            //            {
            //                foreach (GUIItem r in _liRequiredItems) { _winMain.RemoveControl(r); }

            //                _liRequiredItems.Clear();
            //                _iSelectedItemID = chosenItem.ID;
            //                foreach (KeyValuePair<int, int> kvp in chosenItem.GetRequiredItems())
            //                {
            //                    GUIItem newItem = new GUIItem(DataManager.GetItem(kvp.Key, kvp.Value));
            //                    if(!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value))
            //                    {
            //                        newItem.SetColor(Color.Red);
            //                    }
            //                    _liRequiredItems.Add(newItem);
            //                }

            //                _gName.SetText(chosenItem.Name());
                            
            //                _gDescription.SetText(_gDescription.ParseText(chosenItem.Description(), _winMain.InnerWidth(), 5)[0]);

            //                ConfigureInfo();
            //            }
            //        }
            //    }
            //}
            //else
            //{
            //    foreach (GUIItem r in _liRequiredItems)
            //    {
            //        r.ProcessHover(mouse);
            //    }
            //}

            return rv;
        }

        private void BtnBuild()
        {
            Item craft = DataManager.CraftItem(_iSelectedItemID);
            _objMachine?.AttemptToCraftChosenItem(craft);

            if (_objMachine.CraftDaily)
            {
                for (int i = 0; i < _arrMaking.Length; i++)
                {
                    _arrMaking[i].SetItem(DataManager.CraftItem(_objMachine.CurrentlyMaking));
                }
            }

            UpdateInfo(craft);
        }
        private void ShiftLeft()
        {

        }

        private void ShiftRight()
        {

        }

        private void UpdateInfo(Item chosenItem)
        {
            for (int i = 0; i < _arrRecipes.Length; i++)
            {
                _arrRecipes[i].Enable(InventoryManager.HasSufficientItems(_arrRecipes[i].BoxItem.GetRequiredItems()) && !_objMachine.MakingSomething());
            }

            for (int j = 0; j < _liRequiredItems.Count; j++)
            {
                _gComponents.RemoveControl(_liRequiredItems[j]);
            }

            _liRequiredItems.Clear();
            _iSelectedItemID = chosenItem.ID;
            foreach (KeyValuePair<int, int> kvp in chosenItem.GetRequiredItems())
            {
                GUIItemBox newItem = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));
                if (!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value))
                {
                    newItem.SetColor(Color.Red);
                }
                _liRequiredItems.Add(newItem);
            }

            _gName.SetText(chosenItem.Name());
            _gName.Position(_gComponents.Position());// AnchorToInnerSide(_winMain, SideEnum.TopLeft, ScaleIt(1));
            _gName.ScaledMoveBy(0, 6);
            _gName.AlignToObject(_gComponents, SideEnum.CenterX);

            int totalReqWidth = (_liRequiredItems.Count * _liRequiredItems[0].Width) + (_liRequiredItems.Count - 1 * ScaleIt(2));
            int firstPosition = (_gComponents.Width / 2) - (totalReqWidth / 2);
            for (int i = 0; i < _liRequiredItems.Count; i++)
            {
                if (i == 0)
                {
                    _liRequiredItems[i].Position(_gComponents.Position());
                    _liRequiredItems[i].MoveBy(firstPosition, ScaleIt(24));
                }
                else
                {
                    _liRequiredItems[i].AnchorAndAlignToObject(_liRequiredItems[i - 1], SideEnum.Right, SideEnum.Top, ScaleIt(2));
                }
                _gComponents.AddControl(_liRequiredItems[i]);
            }

            _btnBuild.Enable(InventoryManager.HasSufficientItems(chosenItem.GetRequiredItems()) && !_objMachine.MakingSomething());
        }
    }
}
