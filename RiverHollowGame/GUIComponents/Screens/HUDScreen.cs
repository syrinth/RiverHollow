using System.Collections.Generic;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIItemBox;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;
using RiverHollow.GUIComponents.Screens.HUDScreens;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDScreen : GUIScreen
    {
        List<HUDNewAlert> _liTaskIcons;

        GUIButton _btnSkipCutscene;
        GUIObject _gMenu;
        GUIOldStatDisplay _gHealthDisplay;
        GUIOldStatDisplay _gStaminaDisplay;
        GUIMoneyDisplay _gMoney;
        GUIDungeonKeyDisplay _gDungeonKeys;

        HUDMiniInventory _gInventory;
        HUDCalendar _gCalendar;
        GUIItemBox _addedItem;

        double _dAlphaTimer;

        public HUDScreen()
        {
            GameManager.CurrentScreen = GameScreenEnum.World;

            _liTaskIcons = new List<HUDNewAlert>();
            _gHealthDisplay = new GUIOldStatDisplay(PlayerManager.PlayerCombatant.GetHP, Color.Green);
            _gHealthDisplay.AnchorToScreen(this, SideEnum.TopLeft, 10);
            AddControl(_gHealthDisplay);
            _gStaminaDisplay = new GUIOldStatDisplay(PlayerManager.GetStamina, Color.Red);
            _gStaminaDisplay.AnchorAndAlignToObject(_gHealthDisplay, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            AddControl(_gStaminaDisplay);

            GUIWindow win = new GUIWindow(GUIWindow.Window_1, ScaleIt(48), ScaleIt(26));
            
            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorToInnerSide(win, SideEnum.TopLeft);
            //win.Resize(false, ScaleIt(1));
            win.AnchorAndAlignToObject(_gStaminaDisplay, SideEnum.Bottom, SideEnum.Left, ScaleIt(2));
            AddControl(win);

            _gDungeonKeys = new GUIDungeonKeyDisplay();
            _gDungeonKeys.AnchorAndAlignToObject(win, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            AddControl(_gDungeonKeys);

            _gInventory = new HUDMiniInventory();
            _gInventory.AnchorToScreen(SideEnum.Bottom);
            //AddControl(_gInventory);

            _gCalendar = new HUDCalendar();
            _gCalendar.AnchorToScreen(SideEnum.TopRight, 10);
            AddControl(_gCalendar);
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
                if (InventoryManager.AddedItemList.Count > 0 && _addedItem != null && InventoryManager.AddedItemList[0].ItemID == _addedItem.BoxItem.ItemID)
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

        protected override void HandleInput()
        {
            if (!TakingInput() && !CutsceneManager.Playing)
            {
                if (InputManager.CheckPressedKey(Keys.Escape))
                {
                    if (_gMainObject != null)
                    {
                        CloseMainObject();
                    }
                    else
                    {
                        if (_gMenu == null) { OpenMenu(); }
                        else { CloseMenu(); }
                    }
                }
            }
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = base.ProcessRightButtonClick(mouse);

            //If the right click has not been processed, we probably want to close anything that we have open.
            if (!rv)
            {
                if (_gMainObject == null)
                {
                    CloseMenu();
                }

                GUIManager.CloseMainObject();
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

    public class HUDMiniInventory : GUIWindow
    {
        List<GUIItemBox> _liItems;
        GUIButton _btnChangeRow;

        bool _bFadeOutBar = true;
        bool _bFadeItemsOut;
        bool _bFadeItemsIn;
        float _fBarFade;
        float _fItemFade = 1.0f;
        const float FADE_OUT = 0.1f;

        public HUDMiniInventory() : base(GUIWindow.Window_2, TILE_SIZE, TILE_SIZE)
        {
            _btnChangeRow = new GUIButton(new Rectangle(256, 96, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, RowUp);
            _btnChangeRow.FadeOnDisable(false);
            _liItems = new List<GUIItemBox>();

            for (int i = 0; i < InventoryManager.maxItemColumns; i++)
            {
                GUIItemBox ib = new GUIItemBox(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
                _liItems.Add(ib);

                if (i == 0) { ib.AnchorToInnerSide(this, SideEnum.TopLeft); }
                else { ib.AnchorAndAlignToObject(_liItems[i - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

                ib.SetAlpha(_fBarFade);
            }

            _liItems[GameManager.HUDItemCol].Select(true);
            Resize();

            _btnChangeRow.AnchorAndAlignToObject(this, SideEnum.Right, SideEnum.CenterY);
            AddControl(_btnChangeRow);

            _fBarFade = GameManager.HideMiniInventory ? FADE_OUT : 1.0f;
            Alpha(_fBarFade);
        }

        public override void Update(GameTime gTime)
        {
            if (Show())
            {
                base.Update(gTime);
                float startFade = _fBarFade;
                if (_bFadeOutBar && GameManager.HideMiniInventory)
                {
                    if (_fBarFade - FADE_OUT > FADE_OUT) { _fBarFade -= FADE_OUT; }
                    else
                    {
                        _fBarFade = FADE_OUT;
                    }
                }
                else
                {
                    if (_fBarFade < 1)
                    {
                        _fBarFade += FADE_OUT;
                    }

                    UpdateItemFade(gTime);

                }
                if (startFade != _fBarFade)
                {
                    Alpha(_fBarFade);

                    foreach (GUIItemBox gib in _liItems)
                    {
                        gib.SetAlpha(Alpha());
                    }
                    _btnChangeRow.Alpha(Alpha());
                }
            }

            for (int i = 0; i < _liItems.Count; i++)
            {
                _liItems[i].SetItem(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
                _liItems[i].SetAlpha(Alpha());
            }
        }

        /// <summary>
        /// Handles the fading in and out of Items for when we switch rows
        /// </summary>
        /// <param name="gTime"></param>
        private void UpdateItemFade(GameTime gTime)
        {
            if (_bFadeItemsOut)
            {
                float currFade = _fItemFade;
                if (currFade - FADE_OUT > FADE_OUT)
                {
                    _fItemFade -= FADE_OUT;
                    foreach (GUIItemBox gib in _liItems)
                    {
                        gib.SetItemAlpha(_fItemFade);
                    }
                }
                else
                {
                    currFade = FADE_OUT;
                    _bFadeItemsOut = false;
                    _bFadeItemsIn = true;
                    SyncItems();
                }
            }
            if (_bFadeItemsIn)
            {
                float currFade = _fItemFade;
                if (currFade < 1)
                {
                    _fItemFade += FADE_OUT;
                }
                else
                {
                    _bFadeItemsIn = false;
                }

                foreach (GUIItemBox gib in _liItems)
                {
                    gib.SetItemAlpha(_fItemFade);
                }
            }
        }

        public override bool Contains(Point mouse)
        {
            return base.Contains(mouse) || _btnChangeRow.Contains(mouse);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (!GameManager.GamePaused() && Contains(mouse))
            {
                rv = true;

                foreach (GUIItemBox gib in _liItems)
                {
                    if (gib.Contains(mouse))
                    {
                        _liItems[GameManager.HUDItemCol].Select(false);
                        GameManager.HUDItemCol = _liItems.IndexOf(gib);
                        _liItems[GameManager.HUDItemCol].Select(true);
                        break;
                    }
                }

                _btnChangeRow.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if (!GameManager.GamePaused() && Contains(mouse))
            {
                rv = true;

                foreach (GUIItemBox gib in _liItems)
                {
                    rv = gib.ProcessRightButtonClick(mouse);
                    if (rv)
                    {
                        break;
                    }
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (!GameManager.GamePaused())
            {
                if (Contains(mouse) && Alpha() != 1)
                {
                    rv = true;
                    _bFadeOutBar = false;
                }
                else if (!Contains(mouse) && GameManager.HideMiniInventory && Alpha() != 0.1f)
                {
                    _bFadeOutBar = true;
                }
            }

            return rv;
        }

        public void RowUp()
        {
            if (GameManager.HUDItemRow < PlayerManager.BackpackLevel - 1)
            {
                GameManager.HUDItemRow++;
            }
            else
            {
                GameManager.HUDItemRow = 0;
            }

            _bFadeItemsOut = true;
            _bFadeItemsIn = false;
        }

        public void SyncItems()
        {
            for (int i = 0; i < _liItems.Count; i++)
            {
                GUIItemBox ib = _liItems[i];
                ib.SetItem(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
            }
        }
    }

    public class HUDMenu : GUIObject
    {
        const int BTN_PADDING = 10;
        GUIMainObject _gMenuObject;
        List<GUIObject> _liButtons;

        bool _bOpen = false;
        bool _bClose = false;

        public delegate void CloseMenuDelegate();
        private CloseMenuDelegate _closeMenu;

        public HUDMenu(CloseMenuDelegate closeMenu)
        {
            _closeMenu = closeMenu;

            _liButtons = new List<GUIObject>() {
                new GUIButton("Inventory", BtnInventory),
                new GUIButton("Party", BtnParty)
            };

            GUIButton btnBuild = new GUIButton("Build", BtnBuild);
            btnBuild.Enable(!MapManager.CurrentMap.Modular);
            _liButtons.Add(btnBuild);

            _liButtons.Add(new GUIButton("Task Log", BtnTaskLog));
            _liButtons.Add(new GUIButton("Options", BtnOptions));
            _liButtons.Add(new GUIButton("Friends", BtnFriendship));
            _liButtons.Add(new GUIButton("Exit Game", BtnExitGame));

            AddControls(_liButtons);
           
            GUIObject.CreateSpacedColumn(ref _liButtons, -GUIButton.BTN_WIDTH, 0, RiverHollow.ScreenHeight, BTN_PADDING);

            _bOpen = true;
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            int _openingFinished = 0;
            foreach (GUIObject o in Controls)
            {
                int val = 0;
                if (_bOpen)
                {
                    if (o.Position().X < 0) { val = 16; }
                }
                if (_bClose)
                {
                    if (o.Position().X > -GUIButton.BTN_WIDTH) { val = -16; }
                }

                Vector2 temp = o.Position();
                temp.X += val;
                o.Position(temp);
                if (_bOpen && o.Position().X == 0) { _openingFinished++; }
                if (_bClose && o.Position().X == -GUIButton.BTN_WIDTH) { /*Finished closing */ }
            }
            if (_openingFinished == _liButtons.Count) { _bOpen = false; }
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            //Returns false here because we don't handle it
            //By returning false, we will start closing options
            return false;
        }

        #region Buttons
        public void BtnExitGame()
        {
            RiverHollow.PrepExit();
        }
        public void BtnInventory()
        {
            Item[,] toolBox = new Item[1, 7];
            toolBox[0, 0] = PlayerManager.RetrieveTool(ToolEnum.Axe);
            toolBox[0, 1] = PlayerManager.RetrieveTool(ToolEnum.Pick);
            toolBox[0, 2] = PlayerManager.RetrieveTool(ToolEnum.WateringCan);
            toolBox[0, 3] = PlayerManager.RetrieveTool(ToolEnum.Scythe);
            toolBox[0, 4] = PlayerManager.RetrieveTool(ToolEnum.Lantern);
            toolBox[0, 5] = PlayerManager.RetrieveTool(ToolEnum.Harp);
            toolBox[0, 6] = PlayerManager.RetrieveTool(ToolEnum.Backpack);

            _gMenuObject = new HUDInventoryDisplay(toolBox, DisplayTypeEnum.Inventory, true);
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnTaskLog()
        {
            _gMenuObject = new HUDTaskLog();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnParty()
        {
            _gMenuObject = new HUDParty();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnOptions()
        {
            _gMenuObject = new HUDOptions();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnBuild()
        {
            if (!MapManager.CurrentMap.Modular)
            {
                GUIManager.SetScreen(new BuildScreen());
            }
        }
        public void BtnFriendship()
        {
            _gMenuObject = new HUDFriendship();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        #endregion
    }

    public class HUDCalendar : GUIWindow
    {
        static GUIText _gText;
        public HUDCalendar() : base(GUIWindow.Window_2, ScaledTileSize, ScaledTileSize)
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

    class HUDUpgradeWindow : GUIMainObject
    {
        Building _bldg;
        public HUDUpgradeWindow(Building b)
        {
            _winMain = SetMainWindow();

            _bldg = b;

            GUIText name = new GUIText(_bldg.Name() + ", Level " + _bldg.Level);
            name.AnchorToInnerSide(_winMain, SideEnum.Top);

            GUIButton btn = new GUIButton("Upgrade", Upgrade);
            btn.AnchorToInnerSide(_winMain, SideEnum.Bottom);

            if (_bldg.UpgradeReqs() != null)
            {
                Color textColor = Color.White;
                if (!InventoryManager.HasSufficientItems(_bldg.UpgradeReqs()))
                {
                    textColor = Color.Red;
                    btn.Enable(false);
                }

                List<GUIItemBox> list = new List<GUIItemBox>();
                foreach (KeyValuePair<int, int> kvp in _bldg.UpgradeReqs())
                {
                    GUIItemBox box = new GUIItemBox(DataManager.GetItem(kvp.Key, kvp.Value));

                    if (list.Count == 0) { box.AnchorToInnerSide(_winMain, SideEnum.Left); }
                    else { box.AnchorAndAlignToObject(list[list.Count - 1], SideEnum.Right, SideEnum.Bottom); }

                    if (!InventoryManager.HasItemInPlayerInventory(kvp.Key, kvp.Value)) { box.SetColor(Color.Red); }

                    list.Add(box);
                }
            }

            AddControl(name);
        }

        private void Upgrade()
        {
            if (PlayerManager.ExpendResources(_bldg.UpgradeReqs())) { 
                _bldg.Upgrade();
            }
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                rv = true;
                GUIManager.CloseMainObject();
            }

            return rv;
        }
    }

    class HUDRequestWindow : GUIMainObject
    {
        Merchant _merchant;
        List<GUIItemBox> _liRequestedItemBoxes;
        public HUDRequestWindow(TextEntry requestText, Merchant m)
        {
            _winMain = SetMainWindow();

            _merchant = m;

            GUIText text = new GUIText(requestText.GetFormattedText());
            text.SetText(text.ParseText(requestText.GetFormattedText(), _winMain.InnerWidth(), 3, true)[0]);
            text.AnchorToInnerSide(_winMain, SideEnum.Top);

            int edgeSpacing = ScaledTileSize;
            int spacing = (_winMain.InnerWidth() - (edgeSpacing * 2) - (ScaleIt(RECT_IMG.Width) * 3)) / 2;
            _liRequestedItemBoxes = new List<GUIItemBox>();
            foreach (KeyValuePair<Item, bool> kvp in _merchant.DiChosenItems)
            {
                GUIItemBox box = new GUIItemBox(kvp.Key);
                if (kvp.Value) {
                    box.Enable(false);
                    box.SetColor(Color.Gray);
                }

                if (_liRequestedItemBoxes.Count == 0) { box.AnchorToInnerSide(_winMain, SideEnum.Left, edgeSpacing); }
                else { box.AnchorAndAlignToObject(_liRequestedItemBoxes[_liRequestedItemBoxes.Count - 1], SideEnum.Right, SideEnum.Bottom, spacing); }

                if (!InventoryManager.HasItemInPlayerInventory(kvp.Key.ItemID, kvp.Key.Number)) { box.SetColor(Color.Red); }

                GUIMoneyDisplay money = new GUIMoneyDisplay(kvp.Key.Value * 2 * kvp.Key.Number);
                money.AnchorAndAlignToObject(box, SideEnum.Bottom, SideEnum.CenterX, 10);

                _liRequestedItemBoxes.Add(box);
            }

            AddControl(text);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            bool sold = false;
            int givenRequests = 0;
            foreach (GUIItemBox gib in _liRequestedItemBoxes)
            {
                if (!gib.Enabled) { givenRequests++; }
                else if (gib.Contains(mouse))
                {
                    if (InventoryManager.HasItemInPlayerInventory(gib.BoxItem.ItemID, gib.BoxItem.Number))
                    {
                        _merchant.DiChosenItems[gib.BoxItem] = true;
                        InventoryManager.RemoveItemsFromInventory(gib.BoxItem.ItemID, gib.BoxItem.Number);
                        PlayerManager.AddMoney(gib.BoxItem.Value * 2 * gib.BoxItem.Number);
                        gib.Enable(false);
                        gib.SetColor(Color.Gray);
                        givenRequests++;
                        sold = true;

                        //In case the Merchant is asking for the same item multipe times, refresh the color on the request boxes
                        foreach(GUIItemBox obj in _liRequestedItemBoxes)
                        {
                            if (obj.Enabled && !InventoryManager.HasItemInPlayerInventory(obj.BoxItem.ItemID, obj.BoxItem.Number)) { obj.SetColor(Color.Red); }
                        }
                    }

                    rv = true;
                    break;
                }
            }

            if (sold && givenRequests == 3)
            {
                PlayerManager.AddMoney(1000);
                _merchant.FinishRequests();
            }

            return rv;
        }
    }

    class HUDNewAlert : GUIWindow
    {
        GUIImage _gMarker;
        GUIText _gText;

        public delegate void RemoveDelegate(HUDNewAlert q);
        private RemoveDelegate _delAction;
        public HUDNewAlert(string text, RemoveDelegate del) : base(Window_1, 10, 10)
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
                MoveBy(new Vector2(0, -1));
                Alpha(Alpha() - 0.005f);
            }
        }
    }
}
