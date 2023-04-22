using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;
using RiverHollow.GUIComponents.Screens.HUDComponents;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDScreen : GUIScreen
    {
        List<HUDNewAlert> _liTaskIcons;

        GUIButton _btnSkipCutscene;
        GUIObject _gMenu;
        GUIOldStatDisplay _gHealthDisplay;
        GUIOldStatDisplay _gStaminaDisplay;
        GUIOldStatDisplay _gMagicDisplay;
        GUIMoneyDisplay _gMoney;
        GUIDungeonKeyDisplay _gDungeonKeys;

        GUIImage _gBuildIcon;

        HUDMiniInventory _gInventory;
        HUDCalendar _gCalendar;
        GUIItemBox _addedItem;

        double _dAlphaTimer;

        public HUDScreen()
        {
            GameManager.CurrentScreen = GameScreenEnum.World;

            _liTaskIcons = new List<HUDNewAlert>();
            _gHealthDisplay = new GUIOldStatDisplay(PlayerManager.GetHP, Color.Red);
            _gHealthDisplay.AnchorToScreen(this, SideEnum.TopLeft, 10);
            AddControl(_gHealthDisplay);

            _gStaminaDisplay = new GUIOldStatDisplay(PlayerManager.GetStamina, Color.ForestGreen);
            _gStaminaDisplay.AnchorAndAlignToObject(_gHealthDisplay, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            AddControl(_gStaminaDisplay);

            if (PlayerManager.MagicUnlocked)
            {
                _gMagicDisplay = new GUIOldStatDisplay(PlayerManager.GetMagic, Color.Blue);
                _gMagicDisplay.AnchorAndAlignToObject(_gStaminaDisplay, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
                AddControl(_gMagicDisplay);
            }

            GUIWindow win = new GUIWindow(GUIWindow.Brown_Window, ScaleIt(48), ScaleIt(26));
            
            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorToInnerSide(win, SideEnum.TopLeft);
            //win.Resize(false, ScaleIt(1));
            win.AnchorAndAlignToObject(PlayerManager.MagicUnlocked ? _gMagicDisplay : _gStaminaDisplay, SideEnum.Bottom, SideEnum.Left, ScaleIt(2));
            AddControl(win);

            _gDungeonKeys = new GUIDungeonKeyDisplay();
            _gDungeonKeys.AnchorAndAlignToObject(win, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            AddControl(_gDungeonKeys);

            _gInventory = new HUDMiniInventory();
            _gInventory.AnchorToScreen(SideEnum.Bottom, ScaleIt(2));
            AddControl(_gInventory);

            _gCalendar = new HUDCalendar();
            _gCalendar.AnchorToScreen(SideEnum.TopRight, 10);
            AddControl(_gCalendar);

            _gBuildIcon = GUIUtils.GetIcon(GameIconEnum.Hammer);
            _gBuildIcon.AnchorAndAlignToObject(_gCalendar, SideEnum.Left, SideEnum.CenterY, ScaleIt(4));

        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            HandleInput();

            //If there are items queued to display and there is not currently a display up, create one.
            if (InventoryManager.AddedItemList.Count > 0 && _addedItem == null)
            {
                _addedItem = new GUIItemBox(InventoryManager.AddedItemList[0]);
                _addedItem.AnchorToScreen(SideEnum.BottomRight, 12);
                _dAlphaTimer = 1;
                AddControl(_addedItem);
                InventoryManager.AddedItemList.Remove(InventoryManager.AddedItemList[0]);
            }
            else
            {
                //If there are more items to add, there is currently an ItemPickup Display and the next Item to add is the same as the one being displayed
                //Remove it fromt he list of items to show added, add the current number tot he display, and refresh the display.
                if (InventoryManager.AddedItemList.Count > 0 && _addedItem != null && InventoryManager.AddedItemList[0].ID == _addedItem.BoxItem.ID)
                {
                    _addedItem.BoxItem.Add(InventoryManager.AddedItemList[0].Number);
                    InventoryManager.AddedItemList.Remove(InventoryManager.AddedItemList[0]);

                    _dAlphaTimer = 1;
                    _addedItem.SetAlpha(1);
                }
                else if (_addedItem != null && _addedItem.Alpha() > 0)  //Otherwise, if there is a display and the Alpha isn't yet 0, decrease the Alpha
                {
                    _dAlphaTimer -= gTime.ElapsedGameTime.TotalSeconds;
                    _addedItem.SetAlpha((float)_dAlphaTimer);
                }
                else if (_addedItem != null)    //If we get here, there is a display, and the Alpha has reached 0, so remove it.
                {
                    RemoveControl(_addedItem);
                    _addedItem = null;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (InTownMode())
            {
                _gBuildIcon.Draw(spriteBatch);
            }
        }

        protected override void HandleInput()
        {
            if (!TakingInput() && !CutsceneManager.Playing && !PlayerManager.Defeated())
            {
                if (InputManager.CheckPressedKey(Keys.Escape))
                {
                    if (_gMainObject != null)
                    {
                        CloseMainObject();
                    }
                    else if (_guiTextWindow == null)
                    {
                        if (_gMenu == null) { OpenMenu(); }
                        else { CloseMenu(); }
                    }
                }

                if (InputManager.CheckPressedKey(Keys.D1)) { }
            }
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = !PlayerManager.Defeated() && base.ProcessRightButtonClick(mouse);

            //If the right click has not been processed, we probably want to close anything that we have open.
            if (!rv)
            {
                if (_gMainObject == null)
                {
                    CloseMenu();
                }

                rv = GUIManager.CloseMainObject();
            }

            return rv;
        }

        #region Text Window
        /// <summary>
        /// Overrides the Screens OpenTextWindow method to first hide any HUD components desired.
        /// </summary>
        /// <param name="text">Text to open with</param>
        /// <param name="open">Whether to play the open animation</param>
        public override void OpenTextWindow(TextEntry text, bool open = true, bool displayDialogueIcon = false)
        {
            base.OpenTextWindow(text, open, displayDialogueIcon);
            _gInventory.Show(false);
        }
        public override bool CloseTextWindow()
        {
            bool rv = base.CloseTextWindow();
            _gInventory.Show(true);
            return rv;
        }
        #endregion

        #region Menu
        public override bool IsMenuOpen() { return _gMenu != null; }
        public override void OpenMenu()
        {
            GUICursor.ResetCursor();
            _gMenu = new HUDMenu(CloseMenu);
            AddControl(_gMenu);
        }
        public override void CloseMenu()
        {
            if (_gMenu != null)
            {
                RemoveControl(_gMenu);
                _gMenu = null;
            }
        }
        #endregion

        public override void NewAlertIcon(string text)
        {
            HUDNewAlert newAlert = new HUDNewAlert(text, RemoveTaskIcon);

            if (_liTaskIcons.Count == 0) { newAlert.AnchorToScreen(SideEnum.Right, 12); }
            else { newAlert.AnchorAndAlignToObject(_liTaskIcons[_liTaskIcons.Count - 1], SideEnum.Top, SideEnum.Right, ScaleIt(1)); }

            _liTaskIcons.Add(newAlert);
            AddControl(newAlert);
        }
        private void RemoveTaskIcon(HUDNewAlert q)
        {
            _liTaskIcons.Remove(q);
            RemoveControl(q);
        }

        public override void AddSkipCutsceneButton()
        {
            _btnSkipCutscene = new GUIButton(new Rectangle(64, 80, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, CutsceneManager.SkipCutscene);
            _btnSkipCutscene.AnchorToScreen(SideEnum.BottomRight, 12);
            AddControl(_btnSkipCutscene);
        }
        public override void RemoveSkipCutsceneButton()
        {
            RemoveControl(_btnSkipCutscene);
        }
    }

    public class HUDCalendar : GUIWindow
    {
        static GUIText _gText;
        public HUDCalendar() : base(GUIWindow.DarkBlue_Window, ScaledTileSize, ScaledTileSize)
        {
            _gText = new GUIText("Day XX, XX:XX", DataManager.GetBitMapFont(DataManager.FONT_NEW));

            _gText.AnchorToInnerSide(this, SideEnum.TopLeft);
            Resize();
            Height = GameManager.ScaleIt(21);
        }

        public override void Update(GameTime gTime)
        {
            _gText.SetText(GameCalendar.GetCalendarString());
        }
    }

    class HUDNewAlert : GUIWindow
    {
        GUIImage _gMarker;
        GUIText _gText;

        public delegate void RemoveDelegate(HUDNewAlert q);
        private RemoveDelegate _delAction;
        public HUDNewAlert(string text, RemoveDelegate del) : base(Brown_Window, 10, 10)
        {
            _delAction = del;
            _gMarker = new GUIImage(new Rectangle(54, 83, 4, 10), ScaleIt(4), ScaleIt(10), DataManager.DIALOGUE_TEXTURE);
            _gText = new GUIText(text);

            _gMarker.AnchorToInnerSide(this, SideEnum.TopLeft);
            _gText.AnchorAndAlignToObject(_gMarker, SideEnum.Right, SideEnum.CenterY, ScaleIt(1));
            AddControl(_gMarker);
            AddControl(_gText);

            Resize();
        }

        public override void Update(GameTime gTime)
        {
            if (Alpha() <= 0) {
                _delAction(this); }
            else
            {
                MoveBy(new Point(0, -1));
                Alpha(Alpha() - 0.005f);
            }
        }
    }
}
