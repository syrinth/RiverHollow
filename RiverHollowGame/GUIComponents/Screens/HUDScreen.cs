using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.GUIComponents.Screens.HUDComponents;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;
using RiverHollow.GUIComponents.Screens.HUDWindows;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDScreen : GUIScreen
    {
        bool _bTownMode = false;

        List<HUDNewAlert> _liAlertIcons;

        GUIButton _btnSkipCutscene;
        GUIOldStatDisplay _gHealthDisplay;
        GUIOldStatDisplay _gStaminaDisplay;
        GUIOldStatDisplay _gMagicDisplay;
        GUIWindow _gMoneyWindow;
        GUIMoneyDisplay _gMoney;
        GUIDungeonKeyDisplay _gDungeonKeys;

        GUIImage[] _gBuildIcons;

        HUDMiniInventory _gInventory;
        HUDCalendar _gCalendar;
        GUIItemBox _addedItem;

        double _dAlphaTimer;

        public HUDScreen()
        {
            GameManager.CurrentScreen = GameScreenEnum.World;

            _liAlertIcons = new List<HUDNewAlert>();
            _gHealthDisplay = new GUIOldStatDisplay(PlayerManager.GetHP, Color.Red);
            _gHealthDisplay.AnchorToScreen(SideEnum.TopLeft, 3);

            _gStaminaDisplay = new GUIOldStatDisplay(PlayerManager.GetStamina, Color.ForestGreen);
            _gStaminaDisplay.AnchorAndAlignWithSpacing(_gHealthDisplay, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);

            if (PlayerManager.MagicUnlocked)
            {
                _gMagicDisplay = new GUIOldStatDisplay(PlayerManager.GetMagic, Color.Blue);
                _gMagicDisplay.AnchorAndAlignWithSpacing(_gStaminaDisplay, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            }

            _gMoneyWindow = new GUIWindow(GUIUtils.WINDOW_BROWN, ScaleIt(48), ScaleIt(26));
            
            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorToInnerSide(_gMoneyWindow, SideEnum.TopLeft);
            _gMoneyWindow.AnchorAndAlignWithSpacing(PlayerManager.MagicUnlocked ? _gMagicDisplay : _gStaminaDisplay, SideEnum.Bottom, SideEnum.Left, 2);

            _gDungeonKeys = new GUIDungeonKeyDisplay();
            _gDungeonKeys.AnchorAndAlignWithSpacing(_gMoneyWindow, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);

            _gInventory = new HUDMiniInventory();
            _gInventory.AnchorToScreen(SideEnum.Bottom, 1);

            _gCalendar = new HUDCalendar();
            _gCalendar.AnchorToScreen(SideEnum.TopRight, 3);

            _gBuildIcons = new GUIImage[4];
            for (int i = 0; i < 4; i++)
            {
                _gBuildIcons[i] = new GUIImage(GUIUtils.ICON_BUILD);
                _gBuildIcons[i].Show(false);
            }

            _gBuildIcons[0].AnchorToScreen(SideEnum.TopLeft, 4);
            _gBuildIcons[1].AnchorToScreen(SideEnum.TopRight, 4);
            _gBuildIcons[2].AnchorToScreen(SideEnum.BottomLeft, 4);
            _gBuildIcons[3].AnchorToScreen(SideEnum.BottomRight, 4);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            HandleInput();

            ShowHideHUD();

            //If there are items queued to display and there is not currently a display up, create one.
            if (InventoryManager.AddedItemList.Count > 0 && _addedItem == null)
            {
                _addedItem = new GUIItemBox(InventoryManager.AddedItemList[0]);
                _addedItem.AnchorToScreen(SideEnum.BottomRight, 3);
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

        private void ShowHideHUD()
        {
            if (_bTownMode != InTownMode())
            {
                _bTownMode = InTownMode();
                for (int i = 0; i < 4; i++)
                {
                    _gBuildIcons[i].Show(InTownMode());
                }

                _gCalendar.Show(!InTownMode());
                _gMoneyWindow.Show(!InTownMode());
                _gMoney.Show(!InTownMode());
                _gDungeonKeys.Show(!InTownMode());
                _gHealthDisplay.Show(!InTownMode());
                _gStaminaDisplay.Show(!InTownMode());
                _gMagicDisplay?.Show(!InTownMode());
            }
        }

        protected override void HandleInput()
        {
            if (!TakingInput() && !CutsceneManager.Playing && !PlayerManager.Defeated())
            {
                if (InputManager.CheckForInitialKeyDown(Keys.Escape))
                {
                    GameManager.ExitTownMode();

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

                if (InputManager.CheckForInitialKeyDown(GameManager.HotkeyInventory))
                {
                    OpenMenuObject(new HUDPlayerInventory());
                }

                if (InputManager.CheckForInitialKeyDown(GameManager.HotkeyCodex) && PlayerManager.CodexUnlocked)
                {
                    OpenMenuObject(new HUDCodex());
                }

                if (InputManager.CheckForInitialKeyDown(GameManager.HotkeyTasks))
                {
                    OpenMenuObject(new HUDTaskLog());
                }

                if (InputManager.CheckForInitialKeyDown(GameManager.HotkeyOptions))
                {
                    OpenMenuObject(new HUDOptions());
                }

                if (InputManager.CheckForInitialKeyDown(GameManager.HotkeyBuild))
                {
                    OpenMenuObject(new HUDTownCrafting(CloseMenu));
                }

                if (InputManager.CheckForInitialKeyDown(GameManager.HotkeyEdit))
                {
                    GameManager.EnterTownModeEdit();
                }
            }
        }

        private void OpenMenuObject(GUIMainObject obj)
        {
            if (!IsMenuOpen())
            {
                OpenMenu();
            }

            GameManager.ExitTownMode();
            GUIManager.OpenMainObject(obj);
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
            _gMenu = new HUDMenu();
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

        public override void NewAlertIcon(string textEntryName, Color c)
        {
            HUDNewAlert newAlert = new HUDNewAlert(textEntryName, c, RemoveAlertIcon);

            if (_liAlertIcons.Count == 0) { newAlert.AnchorToScreen(SideEnum.Right, 3); }
            else
            {
                var bottomIcon = _liAlertIcons[_liAlertIcons.Count - 1];

                if (bottomIcon.Position().Y < RiverHollow.ScreenHeight *.4)
                {
                    newAlert.AnchorToScreen(SideEnum.Right, 3);
                }
                else
                {
                    newAlert.AnchorAndAlignWithSpacing(_liAlertIcons[_liAlertIcons.Count - 1], SideEnum.Bottom, SideEnum.Right, 1);
                }
            }

            _liAlertIcons.Add(newAlert);
            AddControl(newAlert);
        }
        private void RemoveAlertIcon(HUDNewAlert q)
        {
            _liAlertIcons.Remove(q);
            RemoveControl(q);
        }

        public override void AddSkipCutsceneButton()
        {
            _btnSkipCutscene = new GUIButton(GUIUtils.BTN_SKIP, CutsceneManager.SkipCutscene);
            _btnSkipCutscene.AnchorToScreen(SideEnum.BottomRight, 3);
        }
        public override void RemoveSkipCutsceneButton()
        {
            RemoveControl(_btnSkipCutscene);
        }
    }

    public class HUDCalendar : GUIWindow
    {
        static GUIText _gText;
        public HUDCalendar() : base(GUIUtils.WINDOW_DARKBLUE, GameManager.ScaleIt(75), GameManager.ScaleIt(21))
        {
            _gText = new GUIText("Day XX, XX:XX", DataManager.GetBitMapFont(DataManager.FONT_NEW));
            _gText.AnchorToInnerSide(this, SideEnum.TopLeft);
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

        bool _bSlow = false;
        public HUDNewAlert(string textEntryName, Color c, RemoveDelegate del) : base(GUIUtils.WINDOW_BROWN, 10, 10)
        {
            _delAction = del;
            if (c == Color.Red)
            {
                _bSlow = true;
                _gMarker = new GUIImage(GUIUtils.ICON_ERROR);
            }
            else { _gMarker = new GUIImage(GUIUtils.ICON_EXCLAMATION); }

            _gText = new GUIText(DataManager.GetGameTextEntry(textEntryName).GetFormattedText());
            _gText.SetColor(c);

            _gMarker.AnchorToInnerSide(this, SideEnum.TopLeft);
            _gText.AnchorAndAlignWithSpacing(_gMarker, SideEnum.Right, SideEnum.CenterY, 1);
            AddControl(_gMarker);
            AddControl(_gText);

            DetermineSize();
        }

        public override void Update(GameTime gTime)
        {
            if (Alpha() <= 0 || Position().Y < 0) {
                _delAction(this); }
            else
            {
                MoveBy(new Point(0, -1));
                Alpha(Alpha() - (_bSlow ? 0.002f : 0.005f));
            }
        }
    }
}
