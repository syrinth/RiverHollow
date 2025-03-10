﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens.HUDWindows
{
    public class HUDCraftRecipes : GUIMainObject
    {
        int _iIndex = 0;
        const int MAX_DISPLAY = 21;
        const int COLUMNS = 7;

        GUIImage _gSelection;
        GUIText _gName;
        GUIImage _gScroll;

        readonly GUIButton _btnLeft;
        readonly GUIButton _btnRight;
        readonly List<int> _liCraftingRecipes;
        readonly List<GUIItemBoxHover> _liItemDisplay;
        private List<GUIItemBox> _liRequiredItems;
        
        public HUDCraftRecipes()
        {
            _liItemDisplay = new List<GUIItemBoxHover>();
            _liRequiredItems = new List<GUIItemBox>();
            _liCraftingRecipes = PlayerManager.GetCraftingList().FindAll(x => !((Buildable)DataManager.CreateWorldObjectByID(x)).Unique);

            _winMain = SetMainWindow(GUIUtils.WINDOW_WOODEN_TITLE, GameManager.ScaleIt(186), GameManager.ScaleIt(134));
            _gSelection = new GUIImage(GUIUtils.SELECT_CORNER);
            _gSelection.Show(false);

            _btnLeft = new GUIButton(GUIUtils.BTN_LEFT_SMALL, BtnLeft);
            _btnLeft.PositionAndMove(_winMain, 7, 115);
            _btnLeft.Enable(false);

            _btnRight = new GUIButton(GUIUtils.BTN_RIGHT_SMALL, BtnRight);
            _btnRight.PositionAndMove(_winMain, 169, 115);

            SetUpItemWindows();

            _gScroll = new GUIImage(GUIUtils.HUD_SCROLL_L);
            _gScroll.AlignToObject(_winMain, SideEnum.CenterX);
            _gScroll.AnchorToObject(_liItemDisplay[MAX_DISPLAY - COLUMNS], SideEnum.Bottom, 4);

            _gName = new GUIText("");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _gSelection?.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = GUIUtils.ProcessLeftMouseButton(mouse, _btnLeft, _btnRight);

            if (!rv)
            { 
                for (int i = 0; i < _liItemDisplay.Count; i++)
                {
                    if (_liItemDisplay[i].Contains(mouse))
                    {
                        int objID = _liItemDisplay[i].BoxItem.ID;
                        Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(objID);
                        Dictionary<int, int> requiredToMake = obj.RequiredToMake;

                        if (InventoryManager.ExpendResources(requiredToMake))
                        {
                            InventoryManager.AddToInventory(DataManager.GetItem(obj));
                            SetUpItemWindows();
                            SoundManager.PlayEffect(SoundEffectEnum.Thump);
                        }
                    }
                }
            }

            return rv;
        }

        private void SetUpItemWindows()
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
            
            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liItemDisplay), _winMain, new Point(14, 14), COLUMNS, 3, 3);

            _btnLeft.Enable(_iIndex >= MAX_DISPLAY);
            _btnRight.Enable(_iIndex + MAX_DISPLAY < _liCraftingRecipes.Count);
        }

        private void ClearWindows()
        {
            _liItemDisplay.ForEach(x => x.RemoveSelfFromControl());
            _liItemDisplay.Clear();
        }

        private void UpdateInfo(GUIItemBoxHover obj)
        {
            if (obj.BoxItem != null)
            {
                _gSelection.CenterOnObject(obj);
                _gSelection.Show(true);

                int objID = obj.BoxItem.ID;
                Buildable building = (Buildable)DataManager.CreateWorldObjectByID(objID);
                GUIUtils.CreateRequiredItemsList(ref _liRequiredItems, building.RequiredToMake);

                _gName.SetText(building.Name);
                _gName.AnchorAndAlignWithSpacing(_gScroll, SideEnum.Bottom, SideEnum.CenterX, 4);

                GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(_liRequiredItems), base._winMain, _gScroll, 2, 22);
            }
        }

        public void BtnLeft()
        {
            _iIndex -= MAX_DISPLAY;
            _btnRight.Enable(true);
            SetUpItemWindows();
        }
        public void BtnRight()
        {
            _iIndex += MAX_DISPLAY;
            _btnLeft.Enable(true);
            SetUpItemWindows();
        }
    }
}
