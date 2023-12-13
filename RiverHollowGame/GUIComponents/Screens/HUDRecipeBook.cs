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
    class HUDRecipeBook : GUIMainObject
    {
        const int MAX_DISPLAY = 28;
        const int COLUMNS = 7;

        private GUIImage _gSelection;
        private GUIImage _gComponents;

        private GUIText _gName;
        private GUIItemBoxHover[] _arrRecipes;
        private List<GUIItemBox> _liRequiredItems;

        private readonly Machine _objMachine;

        private readonly List<int> _liCraftFormula;

        public HUDRecipeBook(Machine m)
        {
            _liRequiredItems = new List<GUIItemBox>();
            _objMachine = m;
            _gName = new GUIText("");

            _liCraftFormula = m.GetCraftingList();

            Setup(_liCraftFormula);

            UpdateInfo(_arrRecipes[0]);

            DetermineSize();

            Position();
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            GUIManager.CloseMainObject();
            return true;
        }

        /// <summary>
        /// Does the setup for the crafting window, determines what the crafter can make
        /// and creates the appropriate boxes forthem
        /// </summary>
        /// <param name="recipes"></param>
        public void Setup(List<int> recipes)
        {
            int recipeNumber = Math.Min(recipes.Count, Constants.MAX_RECIPE_DISPLAY);
            _arrRecipes = new GUIItemBoxHover[MAX_DISPLAY];
            _winMain = new GUIWindow(GUIUtils.WINDOW_BROWN, GameManager.ScaleIt(172), GameManager.ScaleIt(101));

            for (int i = 0; i < MAX_DISPLAY; i++)
            {
                Item newItem = null;
                if (recipes.Count > i)
                {
                    newItem = DataManager.GetItem(recipes[i]);
                }

                var displayWindow = new GUIItemBoxHover(newItem, ItemBoxDraw.Never, UpdateInfo);

                if (newItem != null)
                {
                    if (!_objMachine.HasSufficientItems(newItem))
                    {
                        displayWindow.SetItemAlpha(0.3f);
                    }
                }
                else
                {
                    displayWindow.Enable(false);
                }

                _arrRecipes[i] = displayWindow;
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_arrRecipes), _winMain, new Point(7, 6), COLUMNS, 3, 3);


            _gSelection = new GUIImage(GUIUtils.SELECT_CORNER);
            _winMain.AddControl(_gSelection);
            _gSelection.CenterOnObject(_arrRecipes[0]);

            int widthBetween = (recipeNumber - 1) * ScaleIt(2);
            AddControl(_winMain);

            _gComponents = new GUIImage(GUIUtils.WIN_IMAGE_CRAFTING);
            _winMain.MoveBy(new Point((_gComponents.Width - _winMain.Width) / 2, 0));
            _gComponents.AnchorAndAlignWithSpacing(_winMain, SideEnum.Bottom, SideEnum.CenterX, 2);
            _gComponents.AddControl(_gName);

            AddControl(_gComponents);

            DetermineSize();
            CenterOnScreen();
        }

        private void UpdateInfo(GUIItemBoxHover obj)
        {
            if (obj.BoxItem != null)
            {
                _gSelection.CenterOnObject(obj);

                GUIUtils.CreateRequiredItemsList(ref _liRequiredItems, obj.BoxItem.GetRequiredItems(), _objMachine.Stash);

                _gName.SetText(obj.BoxItem.Name());
                _gName.Position(_gComponents);
                _gName.ScaledMoveBy(0, 6);
                _gName.AlignToObject(_gComponents, SideEnum.CenterX);

                GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(_liRequiredItems), _gComponents, _gComponents, 2, 24);
            }
        }
    }
}
