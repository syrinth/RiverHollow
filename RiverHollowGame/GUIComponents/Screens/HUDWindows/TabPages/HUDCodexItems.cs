using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens.HUDComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows.TabPages
{
    internal class HUDCodexItems : GUIObject
    {
        const int MAX_DISPLAY = 35;
        const int COLUMNS = 7;

        readonly GUIText _gTotal;
        readonly GUIText _gLabel;
        readonly GUIButton _btnLeft;
        readonly GUIButton _btnRight;
        readonly GUIWindow _mainWindow;
        protected List<GUIToggle> _gItemToggles;
        readonly List<ItemDisplayWindow> _liItemDisplay;

        int _iIndex = 0;
        ItemTypeEnum _eItemDisplay = ItemTypeEnum.Resource;

        public HUDCodexItems(GUIWindow winMain)
        {
            _liItemDisplay = new List<ItemDisplayWindow>();

            _mainWindow = winMain;

            _gLabel = new GUIText("Items");
            _gLabel.AnchorToInnerSide(winMain, SideEnum.Top);

            _gTotal = new GUIText("");

            _gItemToggles = new List<GUIToggle>();
            AddItemToggle(ItemResourceToggle, GUIUtils.TOGGLE_ITEMS_RESOURCES_ICON);
            AddItemToggle(ItemPotionToggle, GUIUtils.TOGGLE_ITEMS_POTIONS_ICON);
            AddItemToggle(ItemToolToggle, GUIUtils.TOGGLE_ITEMS_TOOLS_ICON);
            AddItemToggle(ItemFoodToggle, GUIUtils.TOGGLE_ITEMS_FOOD_ICON);
            AddItemToggle(ItemSpecialToggle, GUIUtils.TOGGLE_ITEMS_SPECIAL_ICON);

            _btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            _btnLeft.PositionAndMove(winMain, 7, 158);
            _btnLeft.Enable(false);

            _btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            _btnRight.PositionAndMove(winMain, 169, 158);

            _gItemToggles[0].AssignToggleGroup(false, _gItemToggles.Where(x => x != _gItemToggles[0]).ToArray());

            SetUpItemWindows(_eItemDisplay, false);
        }

        private void AddItemToggle(EmptyDelegate del, Rectangle icon)
        {
            int index = _gItemToggles.Count;
            _gItemToggles.Add(new GUIToggle(GUIUtils.TOGGLE_ITEMS_OFF, GUIUtils.TOGGLE_ITEMS_ON, icon, DataManager.HUD_COMPONENTS, del));
            if (index == 0)
            {
                _gItemToggles[index].PositionAndMove(_mainWindow, 49, 19);
            }
            else
            {
                _gItemToggles[index].AnchorAndAlignWithSpacing(_gItemToggles[index - 1], SideEnum.Right, SideEnum.Bottom, 2);
            }
        }

        protected void ClearWindows()
        {
            _liItemDisplay.ForEach(x => x.RemoveSelfFromControl());
            _liItemDisplay.Clear();
        }

        private void SetUpItemWindows(ItemTypeEnum itemWindow, bool reset)
        {
            if (reset)
            {
                _iIndex = 0;
                _eItemDisplay = itemWindow;
            }
            ClearWindows();

            int found = 0;
            List<int> itemIDs = new List<int>(TownManager.DIArchive.Keys.ToList().Where(x => DataManager.GetEnumByIDKey<ItemTypeEnum>(x, "Type", DataType.Item) == _eItemDisplay));

            switch (itemWindow)
            {
                case ItemTypeEnum.Tool:
                    itemIDs = itemIDs.OrderBy(x => (int)DataManager.GetEnumByIDKey<ToolEnum>(x, "Subtype", DataType.Item)).ThenBy(x => DataManager.GetIntByIDKey(x, "Level", DataType.Item)).ToList();
                    break;
                default:
                    itemIDs = itemIDs.OrderBy(x => (int)DataManager.GetEnumByIDKey<ResourceTypeEnum>(x, "Subtype", DataType.Item)).ThenBy(x => DataManager.GetTextData(x, "Name", DataType.Item)).ToList();
                    break;
            }

            for (int i = _iIndex; i < _iIndex + MAX_DISPLAY; i++)
            {
                if (itemIDs.Count <= i)
                {
                    break;
                }

                int museumIndex = itemIDs[i];
                var displayWindow = new ItemDisplayWindow(museumIndex, TownManager.DIArchive[museumIndex]);
                _liItemDisplay.Add(displayWindow);
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liItemDisplay), _mainWindow, new Point(14, 40), COLUMNS, 3, 3);

            _gTotal.SetText(string.Format("{0}/{1}", found, itemIDs.Count));
            _gTotal.AlignToObject(_mainWindow, SideEnum.Center);
            _gTotal.AlignToObject(_btnLeft, SideEnum.CenterY);


            _btnLeft.Enable(_iIndex >= MAX_DISPLAY);
            _btnRight.Enable(_iIndex + MAX_DISPLAY < itemIDs.Count);
        }

        public void BtnLeft()
        {
            _iIndex -= MAX_DISPLAY;
            _btnRight.Enable(true);
            SetUpItemWindows(_eItemDisplay, false);
        }
        public void BtnRight()
        {
            _iIndex += MAX_DISPLAY;
            _btnLeft.Enable(true);
            SetUpItemWindows(_eItemDisplay, false);
        }

        #region ItemTypeToggles
        public void ItemResourceToggle()
        {
            SetUpItemWindows(ItemTypeEnum.Resource, true);
        }
        public void ItemPotionToggle()
        {
            SetUpItemWindows(ItemTypeEnum.Consumable, true);
        }
        public void ItemToolToggle()
        {
            SetUpItemWindows(ItemTypeEnum.Tool, true);
        }
        public void ItemSpecialToggle()
        {
            SetUpItemWindows(ItemTypeEnum.Special, true);
        }
        public void ItemFoodToggle()
        {
            SetUpItemWindows(ItemTypeEnum.Food, true);
        }
        #endregion
    }
}
