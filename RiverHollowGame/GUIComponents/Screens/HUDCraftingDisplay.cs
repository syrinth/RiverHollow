using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.WorldObjects;

using RiverHollow.Items;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;

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
        private GUIButton _btnUp;
        private GUIButton _btnDown;
        private GUIItemBox[] _arrRecipes;
        private List<GUIItemBox> _liRequiredItems;
        private GUIItemBox[] _arrMaking;

        private readonly Machine _objMachine;

        private int _iSelectedItemID = -1;
        private int _iRecipeListStart = 0;
        private int _iBatchSize = 1;

        private List<int> _liCraftFormula;

        public HUDCraftingDisplay(Machine m)
        {
            _liRequiredItems = new List<GUIItemBox>();
            _objMachine = m;
            _gName = new GUIText("");

            _liCraftFormula = m.GetCraftingList();

            Setup(_liCraftFormula);

            UpdateInfo(_iSelectedItemID);

            DetermineSize();

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
            _winMain = new GUIWindow(GUIUtils.Brown_Window, GameManager.ScaledTileSize, GameManager.ScaledTileSize);

            for (int i = 0; i < recipeNumber; i++)
            {
                Item recipe = DataManager.CraftItem(recipes[i]);
                GUIItemBox newBox = new GUIItemBox(recipe, ItemBoxDraw.Always);
                if (i == 0)
                {
                    _iSelectedItemID = recipe.ID;
                    newBox.AnchorToInnerSide(_winMain, SideEnum.TopLeft, 1);
                }
                else { newBox.AnchorAndAlignWithSpacing(_arrRecipes[i - 1], SideEnum.Right, SideEnum.Top, 2, GUIUtils.ParentRuleEnum.ForceToParent); }

                if (!InventoryManager.HasSufficientItems(recipe.GetRequiredItems()))
                {
                    newBox.Enable(false);
                }

                _arrRecipes[i] = newBox;
            }

            _gSelection = new GUIImage(GUIUtils.SELECT_HIGHLIGHT);
            _winMain.AddControl(_gSelection);
            _gSelection.CenterOnObject(_arrRecipes[0]);

            int widthBetween = (recipeNumber - 1) * ScaleIt(2);
            _winMain.Width = (recipeNumber * _arrRecipes[0].Width) + _winMain.WidthEdges() + ScaleIt(2) + widthBetween;
            _winMain.Height = _arrRecipes[0].Height + _winMain.HeightEdges() + ScaleIt(2);
            AddControl(_winMain);

            _gComponents = new GUIImage(GUIUtils.WIN_IMAGE_CRAFTING);
            _winMain.MoveBy(new Point((_gComponents.Width - _winMain.Width) / 2, 0));
            _gComponents.AnchorAndAlignWithSpacing(_winMain, SideEnum.Bottom, SideEnum.CenterX, 2);
            _gComponents.AddControl(_gName);

            _btnBuild = new GUIButton("Build", BtnBuild);
            _btnBuild.Position(_gComponents);
            _btnBuild.AlignToObject(_gComponents, SideEnum.CenterX);
            _btnBuild.ScaledMoveBy(0, 49);

            if (_objMachine.MaxBatch > 1)
            {
                _btnDown = new GUIButton(GUIUtils.BTN_DECREASE, BatchDecrease);
                _btnDown.AnchorAndAlignWithSpacing(_btnBuild, SideEnum.Left, SideEnum.CenterY, 2);

                _btnUp = new GUIButton(GUIUtils.BTN_INCREASE, BatchIncrease);
                _btnUp.AnchorAndAlignWithSpacing(_btnBuild, SideEnum.Right, SideEnum.CenterY, 2);
            }

            AddControl(_gComponents);

            // Making Window
            _winMaking = new GUIWindow(GUIUtils.Brown_Window, GameManager.ScaledTileSize, GameManager.ScaledTileSize);
            _arrMaking = new GUIItemBox[_objMachine.Capacity];
            for (int i = 0; i < _objMachine.Capacity; i++)
            {
                if (_objMachine.CraftingSlots[i].ID == -1) { _arrMaking[i] = new GUIItemBox(); }
                else
                {
                    _arrMaking[i] = new GUIItemBox(DataManager.CraftItem(_objMachine.CraftingSlots[i].ID, _objMachine.CraftingSlots[i].BatchSize));
                    if (_objMachine.CraftingSlots[i].CraftTime > 0)
                    {
                        _arrMaking[i].SetAlpha(0.5f);
                    }
                }

                if (i == 0) { _arrMaking[i].AnchorToInnerSide(_winMaking, SideEnum.TopLeft, 1); }
                else { _arrMaking[i].AnchorAndAlignWithSpacing(_arrMaking[i - 1], SideEnum.Right, SideEnum.Top, 2); }

            }

            widthBetween = (_objMachine.Capacity - 1) * ScaleIt(2);
            _winMaking.Width = (_objMachine.Capacity * _arrRecipes[0].Width) + _winMain.WidthEdges() + ScaleIt(2) + widthBetween;
            _winMaking.Height = _arrRecipes[0].Height + _winMain.HeightEdges() + ScaleIt(2);

            _winMaking.AnchorAndAlignWithSpacing(_gComponents, SideEnum.Bottom, SideEnum.CenterX, 2);
            AddControl(_winMaking);

            if (!_objMachine.CraftDaily)
            {
                _winMaking.Show(false);
            }

            Width = _gComponents.Width;
            Height = _winMaking.Bottom - _winMain.Top;

            CenterOnScreen();

            bool overflow = recipes.Count > Constants.MAX_RECIPE_DISPLAY;
            _btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, ShiftLeft);
            _btnLeft.AnchorAndAlign(_winMain, SideEnum.Left, SideEnum.CenterY);
            _btnLeft.Enable(false);

            _btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, ShiftRight);
            _btnRight.AnchorAndAlign(_winMain, SideEnum.Right, SideEnum.CenterY);
            _btnRight.Enable(overflow);
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
                        if (_arrRecipes[i].BoxItem != null && _arrRecipes[i].BoxItem.ID != _iSelectedItemID)
                        {
                            _gSelection.CenterOnObject(_arrRecipes[i]);
                            _iBatchSize = 1;
                            UpdateInfo(_arrRecipes[i].BoxItem.ID);

                            break;
                        }
                    }
                }
            }
            else if (_gComponents.Contains(mouse))
            {
                rv = _gComponents.ProcessLeftButtonClick(mouse);
            }
            else if (_btnLeft.ProcessLeftButtonClick(mouse) || _btnRight.ProcessLeftButtonClick(mouse))
            {
                rv = true;
            }
            else if (_winMaking.Contains(mouse))
            {
                rv = true;
                for (int i = 0; i < _arrMaking.Length; i++)
                {
                    GUIItemBox box = _arrMaking[i];
                    if (box.Contains(mouse) && box.BoxItem != null && _objMachine.CraftingSlots[i].CraftTime == 0 && InventoryManager.HasSpaceInInventory(box.BoxItem.ID, box.BoxItem.Number))
                    {
                        _objMachine.TakeItem(i);
                        box.SetItem(null);
                        UpdateInfo(_iSelectedItemID);
                    }
                }
            }

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

        private void BatchDecrease()
        {
            if (_iBatchSize > 1) { _iBatchSize--; }
            else { _iBatchSize = _objMachine.MaxBatch; }

            UpdateInfo(_iSelectedItemID);
        }
        private void BatchIncrease()
        {
            if (_iBatchSize < _objMachine.MaxBatch) { _iBatchSize++; }
            else { _iBatchSize = 1; }

            UpdateInfo(_iSelectedItemID);
        }

        private void BtnBuild()
        {
            Item craft = DataManager.CraftItem(_iSelectedItemID, _iBatchSize);
            _objMachine?.AttemptToCraftChosenItem(craft, _iBatchSize);

            if (_objMachine.CraftDaily)
            {
                for (int i = 0; i < _arrMaking.Length; i++)
                {
                    if (_arrMaking[i].BoxItem == null)
                    {
                        _iBatchSize = 1;
                        _arrMaking[i].SetItem(craft);
                        _arrMaking[i].SetAlpha(.5f);
                        break;
                    }
                }
            }

            UpdateInfo(_iSelectedItemID);
        }
        private void ShiftLeft()
        {
            _iRecipeListStart -= Constants.MAX_RECIPE_DISPLAY;
            _btnLeft.Enable(_iRecipeListStart > 0);
            _btnRight.Enable(true);

            _gSelection.CenterOnObject(_arrRecipes[0]);
            UpdateInfo(_liCraftFormula[_iRecipeListStart]);
        }
        private void ShiftRight()
        {
            _iRecipeListStart += Constants.MAX_RECIPE_DISPLAY;
            _btnRight.Enable(_iRecipeListStart + Constants.MAX_RECIPE_DISPLAY <= _arrRecipes.Length);
            _btnLeft.Enable(true);

            _gSelection.CenterOnObject(_arrRecipes[0]);
            UpdateInfo(_liCraftFormula[_iRecipeListStart]);
        }

        private void UpdateInfo(int chosenID)
        {
            Item chosenItem = DataManager.CraftItem(chosenID);
            _iSelectedItemID = chosenItem.ID;

            for (int i = 0; i < _arrRecipes.Length; i++)
            {
                int index = i + _iRecipeListStart;
                if (i + _iRecipeListStart < _liCraftFormula.Count) { _arrRecipes[i].SetItem(DataManager.CraftItem(_liCraftFormula[index], _iBatchSize)); }
                else { _arrRecipes[i].SetItem(null); }

                _arrRecipes[i].Enable(_arrRecipes[i].BoxItem != null && InventoryManager.HasSufficientItems(_arrRecipes[i].BoxItem.GetRequiredItems()) && !_objMachine.CapacityFull());
            }

            bool sufficientItems = GUIUtils.CreateRequiredItemsList(ref _liRequiredItems, chosenItem.GetRequiredItems());

            _gName.SetText(chosenItem.Name());
            _gName.Position(_gComponents);
            _gName.ScaledMoveBy(0, 6);
            _gName.AlignToObject(_gComponents, SideEnum.CenterX);

            GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(_liRequiredItems), _gComponents, _gComponents, 2, 24);

            _btnBuild.Enable(sufficientItems && !_objMachine.CapacityFull() && _objMachine.SufficientStamina() && _objMachine.SpaceToCraft(chosenItem, _iBatchSize));
        }
    }
}
