using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.GUIComponents.Screens.HUDComponents.HUDMenu;
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
        GUIToggle[] _gFilterTabs;

        readonly GUIButton _btnLeft;
        readonly GUIButton _btnRight;
        readonly List<int> _liCraftingRecipes;
        readonly List<GUIItemBox> _liItemDisplay;
        private List<GUIItemBox> _liRequiredItems;

        private GUIItemBox _gHoverBox;

        CraftFilterEnum _eFilter = CraftFilterEnum.All;
        
        public HUDCraftRecipes()
        {
            _liItemDisplay = new List<GUIItemBox>();
            _liRequiredItems = new List<GUIItemBox>();
            _liCraftingRecipes = PlayerManager.GetCraftingList().FindAll(x => !((Buildable)DataManager.CreateWorldObjectByID(x)).Unique);

            _winMain = SetMainWindow(GUIWindow.DarkBlue_Window, GameManager.ScaleIt(186), GameManager.ScaleIt(134));
            _gSelection = new GUIImage(new Rectangle(260, 0, 20, 20), DataManager.DIALOGUE_TEXTURE);
            _gSelection.Show(false);

            _btnLeft = new GUIButton(new Rectangle(102, 34, 10, 13), DataManager.DIALOGUE_TEXTURE, BtnLeft);
            _btnLeft.Position(this);
            _btnLeft.ScaledMoveBy(7, 115);
            _btnLeft.Enable(false);
            _winMain.AddControl(_btnLeft);

            _btnRight = new GUIButton(new Rectangle(112, 34, 10, 13), DataManager.DIALOGUE_TEXTURE, BtnRight);
            _btnRight.Position(this);
            _btnRight.ScaledMoveBy(169, 115);
            _winMain.AddControl(_btnRight);

            SetUpItemWindows();

            _gScroll = new GUIImage(new Rectangle(209, 96, 142, 3), DataManager.HUD_COMPONENTS);
            _gScroll.AlignToObject(_winMain, SideEnum.CenterX);
            _gScroll.AnchorToObject(_liItemDisplay[MAX_DISPLAY - COLUMNS], SideEnum.Bottom, GameManager.ScaleIt(4));
            _winMain.AddControl(_gScroll);

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

                        if (PlayerManager.ExpendResources(requiredToMake))
                        {
                            InventoryManager.AddToInventory(DataManager.GetItem(obj));
                            SetUpItemWindows();
                            SoundManager.PlayEffect("thump3");
                        }
                    }
                }
            }

            return rv ;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = _winMain.Contains(mouse);

            for(int i =0; i < _liItemDisplay.Count; i++)
            {
                if (_liItemDisplay[i].Contains(mouse) && _gHoverBox != _liItemDisplay[i] && _liItemDisplay[i].BoxItem != null)
                {
                    _gHoverBox = _liItemDisplay[i];
                    _gSelection.CenterOnObject(_gHoverBox);
                    _gSelection.Show(true);
                    UpdateInfo();
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
                
                var displayWindow = new GUIItemBox(newItem);
                displayWindow.DrawNumber(ItemBoxDraw.Never);

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
                AddControl(displayWindow);
            }
            
            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liItemDisplay), this.Position() + GameManager.ScaleIt(new Point(14, 14)), _iIndex, _iIndex + MAX_DISPLAY, COLUMNS, 3, 3);

            _btnLeft.Enable(_iIndex >= MAX_DISPLAY);
            _btnRight.Enable(_iIndex + MAX_DISPLAY < _liCraftingRecipes.Count);
        }

        private void ClearWindows()
        {
            _liItemDisplay.ForEach(x => RemoveControl(x));
            _liItemDisplay.Clear();
        }

        private void UpdateInfo()
        {
            int objID = _gHoverBox.BoxItem.ID;
            Buildable obj = (Buildable)DataManager.CreateWorldObjectByID(objID);
            GUIUtils.CreateRequiredItemsList(ref _liRequiredItems, obj.RequiredToMake);

            _gName.SetText(obj.Name());
            _gName.AnchorAndAlignToObject(_gScroll, SideEnum.Bottom, SideEnum.CenterX, GameManager.ScaleIt(4));

            GUIUtils.CreateSpacedRowAgainstObject(new List<GUIObject>(_liRequiredItems), base._winMain, _gScroll, 2, 22);
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
