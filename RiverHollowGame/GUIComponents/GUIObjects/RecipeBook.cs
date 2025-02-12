using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Buildings;

using static RiverHollow.Utilities.Enums;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.GUIComponents.Screens
{
    class RecipeBook : GUIObject
    {
        const int MAX_DISPLAY = 10;
        int _iIndex = 0;
        readonly int _iMaxUsed = MAX_DISPLAY;

        private readonly Building _objBuilding;

        readonly GUIWindow _winMain;
        //readonly GUIButton _btnLeft;
        //readonly GUIButton _btnRight;

        readonly List<RecipeDisplay> _liRecipes;
        readonly Dictionary<int, bool> _diCraftingList;

        public RecipeBook(Building b)
        {
            _objBuilding = b;

            _liRecipes = new List<RecipeDisplay>();
            _diCraftingList = new Dictionary<int, bool>();
            if (b.Producer)
            {
                foreach (var item in b.GetProductionDictionary())
                {
                    foreach (var listItem in item.Value)
                    {
                        _diCraftingList[listItem] = true;
                    }
                }
            }
            else
            {
                foreach (var item in b.GetFullCraftingList())
                {
                    _diCraftingList[item.Item1] = item.Item2;
                }
            }

            //_iMaxUsed needs to be an even number or the next page factor will skip early
            _iMaxUsed = Math.Min(_diCraftingList.Count, MAX_DISPLAY);
            if(_iMaxUsed % 2 != 0) { _iMaxUsed++; }

            _winMain = new GUIWindow(GUIUtils.WINDOW_WOODEN_TITLE, GameManager.ScaleIt(224), GameManager.ScaleIt(67));    //116
            AddControl(_winMain);

            //Add Building Name
            var buildingName = new GUIText(_objBuilding.Name);
            buildingName.Position(_winMain);
            buildingName.AlignToObject(_winMain, SideEnum.CenterX);
            buildingName.MoveBy(0, (GUIUtils.WINDOW_WOODEN_TITLE.ScaledTopEdge - buildingName.Height) / 2);

            //Add page lines
            var top = new GUIImage(GUIUtils.RECIPE_TOP);
            top.PositionAndMove(_winMain, 6, 16);

            GUIImage temp = top;
            DynamicSectionHelper(4, ref temp);
            DynamicSectionHelper(6, ref temp);
            DynamicSectionHelper(8, ref temp);

            var bottom = new GUIImage(GUIUtils.RECIPE_BOTTOM);
            bottom.AnchorAndAlign(temp, SideEnum.Bottom, SideEnum.Left);

            //Add Crafts/Day

                    int craftsLeft = _objBuilding.GetDailyCraftingLimit();

                    var craftIcon = new GUIIconText(craftsLeft.ToString(), 2, GUIUtils.ICON_HAMMER, GameIconEnum.Hammer, SideEnum.Right, SideEnum.CenterY);
                    craftIcon.PositionAndMove(_winMain, 187, 1);


            DetermineSize();

            //_btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            //_btnLeft.PositionAndMove(_winMain, -11, 58);
            //_btnLeft.Show(false);

            //_btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            //_btnRight.PositionAndMove(_winMain, 189, 58);
            //_btnRight.Show(false);
            //AddControls(_btnLeft, _btnRight);

            SetupRecipes();
            CenterOnScreen();
        }

        private void DynamicSectionHelper(int value, ref GUIImage temp)
        {
            if (_iMaxUsed > value)
            {
                var middleTwo = new GUIImage(GUIUtils.RECIPE_MIDDLE);
                middleTwo.AnchorAndAlign(temp, SideEnum.Bottom, SideEnum.Left);
                temp = middleTwo;
                _winMain.Height += temp.Height;
            }
        }

        public void SetupRecipes()
        {
            foreach(var recipe in _liRecipes)
            {
                recipe.RemoveSelfFromControl();
            }
            _liRecipes.Clear();

            Point start = new Point(9, 17);
            for (int i = _iIndex; i < _iIndex + _iMaxUsed; i++)
            {
                if (_diCraftingList.Count > i)
                {
                    var id = _diCraftingList.Keys.ToList()[i];
                    RecipeDisplay newDisplay = new RecipeDisplay(id, _diCraftingList, _objBuilding);

                    if (i == _iIndex)
                    {
                        newDisplay.PositionAndMove(_winMain, start);
                    }
                    else if (i == ((_iIndex + _iMaxUsed) / 2))
                    {
                        newDisplay.AnchorAndAlignWithSpacing(_liRecipes[0], SideEnum.Right, SideEnum.Bottom, 10, GUIUtils.ParentRuleEnum.ForceToParent);
                    }
                    else
                    {
                        newDisplay.AnchorAndAlignWithSpacing(_liRecipes[i - _iIndex - 1], SideEnum.Bottom, SideEnum.Left, 1, GUIUtils.ParentRuleEnum.ForceToParent);
                    }

                    _liRecipes.Add(newDisplay);
                }
            }

            //_btnLeft.Show(_iIndex >= _iMaxUsed);
            //_btnRight.Show(_iIndex + _iMaxUsed < _diCraftingList.Count);
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            GUIManager.CloseMainObject();
            return true;
        }

        //public void BtnLeft()
        //{
        //    _iIndex -= _iMaxUsed;
        //    _btnRight.Enable(true);
        //    SetupRecipes();
        //}
        //public void BtnRight()
        //{
        //    _iIndex += _iMaxUsed;
        //    _btnLeft.Enable(true);
        //    SetupRecipes();
        //}

        private class RecipeDisplay : GUIObject
        {
            public RecipeDisplay(int id, Dictionary<int, bool> craftingList, Building b)
            {
                var item = DataManager.CraftItem(id);
                GUIItem itemToCraft = new GUIItem(item, ItemBoxDraw.MoreThanOne, false);
                AddControl(itemToCraft);

                bool blackout = !craftingList[item.ID];
                if (blackout)
                {
                    itemToCraft.SetImageColor(Color.Black);
                }
                else
                {
                    var reqItems = item.GetRequiredItems();
                    if (reqItems != null)
                    {
                        itemToCraft.ScaledMoveBy(0, 4);
                        itemToCraft.SetNumberOffset(new Point(1, 1));

                        GUIImage dots = new GUIImage(GUIUtils.RECIPE_DOTS);
                        dots.AnchorAndAlign(itemToCraft, SideEnum.Right, SideEnum.Bottom);
                        AddControl(dots);

                        var recipeList = new List<GUIObject>();
                        foreach (var kvp in reqItems)
                        {
                            var recipeItem = DataManager.GetItem(kvp.Key, kvp.Value);
                            var guiRecipeItem = new GUIItem(recipeItem, ItemBoxDraw.MoreThanOne, false);
                            guiRecipeItem.SetNumberOffset(new Point(1, 5));

                            if (!guiRecipeItem.CompareNumToInventory(b.Stash))
                            {
                                itemToCraft.SetImageAlpha(0.3f);
                                dots.Alpha(0.3f);
                                guiRecipeItem.SetImageAlpha(0.3f);
                            }

                            if (recipeList.Count == 0)
                            {
                                guiRecipeItem.AnchorAndAlign(dots, SideEnum.Right, SideEnum.Bottom);
                                guiRecipeItem.ScaledMoveBy(0, -4);
                            }
                            else
                            {
                                guiRecipeItem.AnchorAndAlignWithSpacing(recipeList.Last(), SideEnum.Right, SideEnum.Bottom, 8);
                            }

                            recipeList.Add(guiRecipeItem);
                            AddControl(guiRecipeItem);
                        }
                    }
                    else
                    {
                        itemToCraft.ScaledMoveBy(41, 4);
                        itemToCraft.SetNumberOffset(new Point(1, 1));
                    }
                }

                //Always needs to be the same width, regarless of if it's missing objects
                Width = GameManager.ScaleIt(98);
                Height = GameManager.ScaleIt(22);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
            }
        }
    }
}
