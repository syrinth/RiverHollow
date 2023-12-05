using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.GUIObjects;
using System.Collections.Generic;
using static RiverHollow.GUIComponents.Screens.HUDComponents.HUDMenu;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using static RiverHollow.Utilities.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.GUIComponents.Screens.HUDWindows.TabPages
{
    internal class HUDTownCraftingPage : GUIObject
    {
        const int MAX_DISPLAY = 28;
        const int COLUMNS = 7;

        readonly List<int> _liCraftingRecipes;
        readonly List<GUIItemBoxHover> _liItemDisplay;

        readonly GUIImage _gSelection;

        int _iIndex = 0;

        readonly GUIWindow _mainWindow;

        readonly GUIButton _btnLeft;
        readonly GUIButton _btnRight;
        readonly GUIImage _gScroll;
        GUIText _gName;

        List<GUIItemBox> _liRequiredItems;

        private CloseMenuDelegate _closeMenu;

        public HUDTownCraftingPage(GUIWindow winMain, List<int> recipes, CloseMenuDelegate closeMenu)
        {
            _mainWindow = winMain;
            _closeMenu = closeMenu;

            _liRequiredItems = new List<GUIItemBox>();
            _liItemDisplay = new List<GUIItemBoxHover>();

            _liCraftingRecipes = recipes;
            _gSelection = new GUIImage(GUIUtils.SELECT_CORNER);
            _gSelection.Show(false);

            _gName = new GUIText("");
            _gScroll = new GUIImage(GUIUtils.HUD_SCROLL_L);

            _btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            _btnLeft.AnchorToInnerSide(_mainWindow, SideEnum.BottomLeft, 2);

            _btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            _btnRight.AnchorToInnerSide(_mainWindow, SideEnum.BottomRight, 2);

            _btnLeft.Enable(_iIndex >= MAX_DISPLAY);
            _btnRight.Enable(_iIndex + MAX_DISPLAY < _liCraftingRecipes.Count);

            SetupCraftingWindows();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _gSelection?.Draw(spriteBatch);
        }

        protected void ClearWindows()
        {
            _liItemDisplay.ForEach(x => x.RemoveSelfFromControl());
            _liItemDisplay.Clear();
        }

        private void SetupCraftingWindows()
        {
            ClearWindows();

            for (int i = _iIndex; i < _iIndex + MAX_DISPLAY; i++)
            {
                Item newItem = null;
                if (_liCraftingRecipes.Count > i)
                {
                    newItem = DataManager.GetItem(_liCraftingRecipes[i] + Constants.BUILDABLE_ID_OFFSET);
                }

                var displayWindow = new GUIItemBoxHover(newItem, ItemBoxDraw.Never, UpdateInfo);

                if (newItem != null)
                {
                    Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(newItem.ID);
                    if (!InventoryManager.HasSufficientItems(obj.RequiredToMake))
                    {
                        displayWindow.SetItemAlpha(0.3f);
                    }
                }
                else
                {
                    displayWindow.Enable(false);
                }

                _liItemDisplay.Add(displayWindow);
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liItemDisplay), _mainWindow, new Point(14, 10), COLUMNS, 3, 3);

            _gScroll.AlignToObject(_mainWindow, SideEnum.CenterX);
            _gScroll.AnchorToObject(_liItemDisplay[MAX_DISPLAY - 1], SideEnum.Bottom, 4);
        }

        private void UpdateInfo(GUIItemBoxHover obj)
        {
            if (obj.BoxItem != null)
            {
                _gSelection.CenterOnObject(obj);
                _gSelection.Show(true);

                int objID = obj.BoxItem.ID;
                Buildable building = (Buildable)DataManager.CreateWorldObjectByID(objID);
                bool sufficientItems = GUIUtils.CreateRequiredItemsList(ref _liRequiredItems, building.RequiredToMake);

                _gName.SetText(building.Name());
                _gName.SetColor(sufficientItems ? Color.Black : Color.Red);
                _gName.AnchorAndAlignWithSpacing(_gScroll, SideEnum.Bottom, SideEnum.CenterX, 4);

                GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(_liRequiredItems), _mainWindow, _gScroll, 2, 22);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = base.ProcessLeftButtonClick(mouse);

            if (!rv)
            {
                for (int i = 0; i < _liItemDisplay.Count; i++)
                {
                    if (_liItemDisplay[i].Contains(mouse))
                    {
                        rv = true;

                        if (_liItemDisplay[i].BoxItem != null)
                        {
                            int objID = _liItemDisplay[i].BoxItem.ID;
                            Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(objID);
                            Dictionary<int, int> requiredToMake = obj.RequiredToMake;

                            if (!obj.IsDirectBuild())
                            {
                                if (InventoryManager.HasSpaceInInventory(objID, 1) && PlayerManager.ExpendResources(requiredToMake))
                                {
                                    InventoryManager.AddToInventory(DataManager.GetItem(obj));
                                    SetupCraftingWindows();
                                    SoundManager.PlayEffect(SoundEffectEnum.Thump);
                                }
                            }
                            else
                            {
                                GameManager.BuildInTownMode(requiredToMake, obj, _closeMenu);
                            }
                        }
                        break;
                    }
                }
            }

            return rv;
        }

        public void BtnLeft()
        {
            _iIndex -= MAX_DISPLAY;
            _btnRight.Enable(true);
            SetupCraftingWindows();
        }
        public void BtnRight()
        {
            _iIndex += MAX_DISPLAY;
            _btnLeft.Enable(true);
            SetupCraftingWindows();
        }
    }

    internal class HUDTownCraftingFloors : HUDTownCraftingPage
    {
        public HUDTownCraftingFloors(GUIWindow winMain, CloseMenuDelegate closeMenu) : base(winMain, PlayerManager.GetNonUniqueByTypes(new List<BuildableEnum>() { BuildableEnum.Floor }), closeMenu) { }
    }

    internal class HUDTownCraftingWalls : HUDTownCraftingPage
    {
        public HUDTownCraftingWalls(GUIWindow winMain, CloseMenuDelegate closeMenu) : base(winMain, PlayerManager.GetNonUniqueByTypes(new List<BuildableEnum>() { BuildableEnum.Wall }), closeMenu) { }
    }

    internal class HUDTownCraftingFurniture : HUDTownCraftingPage
    {
        public HUDTownCraftingFurniture(GUIWindow winMain, CloseMenuDelegate closeMenu) : base(winMain, PlayerManager.GetNonUniqueByTypes(new List<BuildableEnum>() { BuildableEnum.Decor, BuildableEnum.Basic, BuildableEnum.Container }), closeMenu) { }
    }
}