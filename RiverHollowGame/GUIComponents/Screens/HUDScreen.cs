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
using RiverHollow.Characters.Lite;
using static RiverHollow.Utilities.Enums;

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
        GUIMonsterEnergyDisplay _gEnergy;
        GUIDungeonKeyDisplay _gDungeonKeys;

        HUDMiniInventory _gInventory;
        HUDCalendar _gCalendar;
        GUIItemBox _addedItem;

        double _dTimer;

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

            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorAndAlignToObject(_gStaminaDisplay, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            AddControl(_gMoney);

            _gEnergy = new GUIMonsterEnergyDisplay();
            _gEnergy.AnchorAndAlignToObject(_gMoney, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
            AddControl(_gEnergy);

            _gDungeonKeys = new GUIDungeonKeyDisplay();
            _gDungeonKeys.AnchorAndAlignToObject(_gEnergy, SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN);
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
                _dTimer = 1;
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

                    _dTimer = 1;
                    _addedItem.SetAlpha(1);
                }
                else if (_addedItem != null && _addedItem.Alpha() > 0)  //Otherwise, if there is a display and the Alpha isn't yet 0, decrease the Alpha
                {
                    _dTimer -= gTime.ElapsedGameTime.TotalSeconds;
                    _addedItem.SetAlpha((float)_dTimer);
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

        public class HUDTaskLog : GUIMainObject
        {
            //public static int BTNSIZE = ScaledTileSize;
            public static int MAX_SHOWN_TASKS = 4;
            public static int TASK_SPACING = 20;
            public static int TASKBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
            public static int TASKBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDTaskLog.MAX_SHOWN_TASKS) - (_gWindow.EdgeSize * 2)
            List<GUIObject> _liTasks;
            DetailBox _detailWindow;
            GUIList _gList;

            public HUDTaskLog()
            {
                _winMain = SetMainWindow();

                _liTasks = new List<GUIObject>();
                _detailWindow = new DetailBox(GUIWindow.Window_1, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
                _detailWindow.Show(false);
                _detailWindow.CenterOnScreen();
                AddControl(_detailWindow);

                for (int i = 0; i < TaskManager.TaskLog.Count; i++)
                {
                    TaskBox q = new TaskBox(TASKBOX_WIDTH, TASKBOX_HEIGHT, OpenDetailBox);
                    q.SetTask(TaskManager.TaskLog[i]);
                    _liTasks.Add(q);
                }

                _gList = new GUIList(_liTasks, MAX_SHOWN_TASKS, TASK_SPACING/*, _gWindow.Height*/);
                _gList.CenterOnObject(_winMain);

                AddControl(_gList);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                if (!_detailWindow.Show())
                {
                    foreach (GUIObject c in Controls)
                    {
                        rv = c.ProcessLeftButtonClick(mouse);

                        if (rv) { break; }
                    }
                }
                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;
                if (_detailWindow.Show())
                {
                    rv = true;
                    ShowDetails(false);
                }
                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = true;
                if (_detailWindow.Show())
                {
                    rv = _detailWindow.ProcessHover(mouse);
                }
                return rv;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
            }

            public override void Update(GameTime gTime)
            {
                base.Update(gTime);
            }

            private void OpenDetailBox(RHTask q)
            {
                _detailWindow.SetData(q);
                ShowDetails(true);
            }

            private void ShowDetails(bool val)
            {
                _detailWindow.Show(val);
                _winMain.Show(!val);
                _gList.Show(!val);
            }

            public class TaskBox : GUIObject
            {
                GUIWindow _window;
                GUIText _gName;
                GUIText _gGoalProgress;
                public RHTask TheTask { get; private set; }
                public delegate void ClickDelegate(RHTask q);
                private ClickDelegate _delAction;

                public TaskBox(int width, int height, ClickDelegate del)
                {
                    _delAction = del;

                    int boxHeight = height;
                    int boxWidth = width;
                    _window = new GUIWindow(GUIWindow.Window_1, boxWidth, boxHeight);
                    AddControl(_window);
                    Width = _window.Width;
                    Height = _window.Height;
                    TheTask = null;
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    if (TheTask != null && Show())
                    {
                        _window.Draw(spriteBatch);
                    }
                }
                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (Contains(mouse))
                    {
                        _delAction(TheTask);
                    }

                    return rv;
                }
                public override bool ProcessHover(Point mouse)
                {
                    bool rv = false;
                    return rv;
                }
                public override bool Contains(Point mouse)
                {
                    return _window.Contains(mouse);
                }

                public void SetTask(RHTask q)
                {
                    TheTask = q;
                    _gName = new GUIText(TheTask.Name);
                    _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);

                    string progressString = q.GetProgressString();
                    if (!string.IsNullOrEmpty(progressString))
                    {
                        _gGoalProgress = new GUIText(progressString);
                        _gGoalProgress.AnchorToInnerSide(_window, SideEnum.BottomRight);
                    }
                }
            }

            public class DetailBox : GUIWindow
            {
                GUIText _name;
                GUIText _desc;
                GUIText _progress;
                public DetailBox(WindowData winData, int width, int height) : base(winData, width, height)
                {
                }

                public void SetData(RHTask q)
                {
                    Controls.Clear();
                    _name = new GUIText(q.Name);
                    _name.AnchorToInnerSide(this, SideEnum.TopLeft);

                    _desc = new GUIText();
                    _desc.ParseAndSetText(q.Description, InnerWidth(), 3, true);
                    _desc.AnchorAndAlignToObject(_name, SideEnum.Bottom, SideEnum.Left, _name.CharHeight);

                    List<GUIObject> boxes = new List<GUIObject>();
                    for (int i = 0; i < q.LiRewardItems.Count; i++)
                    {
                        GUIItemBox newBox = new GUIItemBox(DataManager.DIALOGUE_TEXTURE, q.LiRewardItems[i], true);
                        boxes.Add(newBox);

                        if(i == 0) { newBox.AnchorAndAlignToObject(_desc, SideEnum.Bottom, SideEnum.Left); }
                        else { newBox.AnchorAndAlignToObject(boxes[i-1], SideEnum.Right, SideEnum.Top); }
                        AddControl(newBox);
                    }

                    _progress = new GUIText(q.GetProgressString());
                    _progress.AnchorToInnerSide(this, SideEnum.BottomRight);
                }
            }
        }
        public class HUDParty : GUIMainObject
        {
            PositionMap _map;
            CharacterDetailObject _charBox;

            public HUDParty()
            {
                _charBox = new CharacterDetailObject(PlayerManager.PlayerCombatant);
                _charBox.CenterOnScreen();
                AddControl(_charBox);

                int partySize = PlayerManager.GetParty().Count;

                _map = new PositionMap(SetSelectedCharacter);
                _map.AnchorAndAlignToObject(_charBox, SideEnum.Bottom, SideEnum.CenterX);
                AddControl(_map);

                Width = _charBox.Width;
                Height = _map.Bottom - _charBox.Top;
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                foreach (GUIObject o in Controls)
                {
                    rv = o.ProcessLeftButtonClick(mouse);
                    if (rv)
                    {
                        break;
                    }
                }

                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;
                if (_charBox != null)
                {
                    rv = _charBox.ProcessRightButtonClick(mouse);
                }
                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = true;

                if (_charBox != null)
                {
                    rv = _charBox.ProcessHover(mouse);
                }

                return rv;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
            }

            public override void Update(GameTime gTime)
            {
                base.Update(gTime);
            }

            /// <summary>
            /// Sets the selected character for the CharacterDetailBox and loads
            /// the relevant data.
            /// </summary>
            /// <param name="selectedCharacter"></param>
            public void SetSelectedCharacter(ClassedCombatant selectedCharacter)
            {
                _charBox?.SetAdventurer(selectedCharacter);
            }

            private class PositionMap : GUIWindow
            {
                ClassedCombatant _currentCharacter;
                StartPosition _currPosition;
                StartPosition[,] _arrStartPositions;

                public delegate void ClickDelegate(ClassedCombatant selectedCharacter);
                private ClickDelegate _delAction;

                public PositionMap(ClickDelegate del) : base(Window_2, 16, 16)
                {
                    _delAction = del;

                    //Actual entries will be one higher since we go to 0 inclusive
                    int maxColIndex = 2;
                    int maxRowIndex = 2;

                    int spacing = 10;
                    _arrStartPositions = new StartPosition[maxColIndex + 1, maxRowIndex + 1]; //increment by one as stated above
                    for (int cols = maxColIndex; cols >= 0; cols--)
                    {
                        for (int rows = maxRowIndex; rows >= 0; rows--)
                        {
                            StartPosition pos = new StartPosition(cols, rows);
                            _arrStartPositions[cols, rows] = pos;
                            if (cols == maxColIndex && rows == maxRowIndex)
                            {
                                pos.AnchorToInnerSide(this, SideEnum.TopLeft, spacing);
                            }
                            else if (cols == maxColIndex)
                            {
                                pos.AnchorAndAlignToObject(_arrStartPositions[maxColIndex, rows + 1], SideEnum.Bottom, SideEnum.Left, spacing);
                            }
                            else
                            {
                                pos.AnchorAndAlignToObject(_arrStartPositions[cols + 1, rows], SideEnum.Right, SideEnum.Bottom, spacing);
                            }
                        }
                    }

                    PopulatePositionMap();

                    this.Resize();
                }

                public override void Update(GameTime gTime)
                {
                    base.Update(gTime);
                }

                /// <summary>
                /// Populates the PositionMap with the initial starting positions of the party
                /// </summary>
                public void PopulatePositionMap()
                {
                    //_currentCharacter = PlayerManager.World;

                    ////Iterate over each member of the party and retrieve their starting position.
                    ////Assigns the character to the starting position and assigns the current position
                    ////to the Player Character's
                    //foreach (ClassedCombatant c in PlayerManager.GetTacticalParty())
                    //{
                    //    Vector2 vec = c.StartPosition;
                    //    _arrStartPositions[(int)vec.X, (int)vec.Y].SetCharacter(c, (c == _currentCharacter));
                    //    if (c == _currentCharacter)
                    //    {
                    //        _currPosition = _arrStartPositions[(int)vec.X, (int)vec.Y];
                    //    }
                    //}
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;

                    if (Contains(mouse))
                    {
                        rv = true;
                    }

                    foreach (StartPosition sp in _arrStartPositions)
                    {
                        //If we have clicked on a StartPosition
                        if (sp.Contains(mouse))
                        {
                            //If the StartPosition is not occupied set the currentPosition to null
                            //then set it to the clicked StartPosition and assign the current Character.
                            //Finally, reset the characters internal start position vector.
                            if (!sp.Occupied())
                            {
                                rv = true;
                                _currPosition.SetCharacter(null);
                                _currPosition = sp;
                                //_currPosition.SetCharacter(_currentCharacter, true);
                                _currentCharacter.SetStartPosition(new Vector2(_currPosition.Col, _currPosition.Row));
                            }
                            else
                            {
                                _currPosition?.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                                _currPosition = sp;
                                //Set the currentCharacter to the selected character.
                                //Call up to the parent object to redisplay data.
                                _currentCharacter = sp.Character;
                                //_delAction(_currentCharacter);
                                _currPosition?.PlayAnimation(VerbEnum.Walk, DirectionEnum.Down);
                            }

                            break;
                        }
                    }

                    return rv;
                }

                private class StartPosition : GUIImage
                {
                    ClassedCombatant _character;
                    public ClassedCombatant Character => _character;
                    int _iCol;
                    int _iRow;
                    public int Col => _iCol;
                    public int Row => _iRow;

                    private GUICharacterSprite _sprite;
                    public StartPosition(int col, int row) : base(new Rectangle(0, 112, 16, 16), TILE_SIZE, TILE_SIZE, DataManager.FILE_WORLDOBJECTS)
                    {
                        _iCol = col;
                        _iRow = row;

                        SetScale(CurrentScale);
                    }

                    public override void Draw(SpriteBatch spriteBatch)
                    {
                        base.Draw(spriteBatch);
                        if (_sprite != null)
                        {
                            _sprite.Draw(spriteBatch);
                        }
                    }

                    /// <summary>
                    /// Assigns the character that the StartPosition is referring to.
                    /// 
                    /// Configures the Sprite and Adds it to the Controls ifthere is a character.
                    /// Removes it if not.
                    /// </summary>
                    /// <param name="c">The Character to assign to the StartPosition</param>
                    /// <param name="currentCharacter">Whether the character is the current character and should walk</param>
                    public void SetCharacter(ClassedCombatant c, bool currentCharacter = false)
                    {
                        //_character = c;
                        if (c != null)
                        {
                            if (c == PlayerManager.PlayerCombatant) { _sprite = new GUICharacterSprite(true); }
                            else { _sprite = new GUICharacterSprite(c.BodySprite, true); }

                            _sprite.SetScale(2);
                            _sprite.CenterOnObject(this);
                            _sprite.MoveBy(new Vector2(0, -(this.Width / 4)));

                            if (currentCharacter) { _sprite.PlayAnimation(VerbEnum.Walk, DirectionEnum.Down); }
                            else { _sprite.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down); }
                            AddControl(_sprite);
                        }
                        else
                        {
                            RemoveControl(_sprite);
                            _sprite = null;
                        }
                    }

                    public bool Occupied() { return _character != null; }

                    /// <summary>
                    /// Wrapper for the PositionMap to call down to the Sprite directly
                    /// </summary>
                    /// <typeparam name="TEnum">Template for any enum type</typeparam>
                    /// <param name="animation">The animation enum to play</param>
                    public void PlayAnimation(VerbEnum verb, DirectionEnum dir)
                    {
                        _sprite.PlayAnimation(verb, dir);
                    }
                }
            }

            public class CharacterDetailObject : GUIObject
            {
                const int SPACING = 10;
                EquipWindow _equipWindow;

                ClassedCombatant _character;
                BitmapFont _font;

                List<SpecializedBox> _liGearBoxes;

                GUIWindow _winName;
                public GUIWindow WinDisplay;
                //GUIWindow _winClothes;

                SpecializedBox _sBoxArmor;
                SpecializedBox _sBoxHead;
                SpecializedBox _sBoxWeapon;
                SpecializedBox _sBoxAccessory;
                SpecializedBox _sBoxShirt;
                SpecializedBox _sBoxHat;
                GUIButton _btnSwap;

                GUIText _gName, _gClass, _gLvl, _gStr, _gDef, _gMagic, _gRes, _gSpd;
                GUIOldStatDisplay _gBarXP, _gBarHP;

                public CharacterDetailObject(ClassedCombatant c)
                {
                    _winName = new GUIWindow(GUIWindow.Window_1, (GUIManager.MAIN_COMPONENT_WIDTH) - (GUIWindow.Window_1.WidthEdges()), 10);
                    WinDisplay = new GUIWindow(GUIWindow.Window_1, (GUIManager.MAIN_COMPONENT_WIDTH) - (GUIWindow.Window_1.WidthEdges()), (GUIManager.MAIN_COMPONENT_HEIGHT / 4) - (GUIWindow.Window_1.HeightEdges()));
                    WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left);
                    //_winClothes = new GUIWindow(GUIWindow.RedWin, 10, 10);
                    //_winClothes.AnchorAndAlignToObject(WinDisplay, SideEnum.Bottom, SideEnum.Left);

                    _character = c;
                    _font = DataManager.GetBitMapFont(DataManager.FONT_NEW);

                    _liGearBoxes = new List<SpecializedBox>();
                    Load();

                    _winName.Resize();
                    _winName.Height += SPACING;

                    WinDisplay.Resize();
                    WinDisplay.Height += SPACING;

                    //_winClothes.Resize();
                    //_winClothes.Height += SPACING;
                    //_winClothes.Width += SPACING;

                    WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left, ScaleIt(1));
                    //_winClothes.AnchorAndAlignToObject(WinDisplay, SideEnum.Bottom, SideEnum.Left);

                    AddControl(_winName);
                    //AddControl(_winClothes);
                    AddControl(WinDisplay);

                    Width = _winName.Width;
                    Height = WinDisplay.Bottom - _winName.Top;
                }

                private void Load()
                {
                    //_winClothes.Controls.Clear();
                    _winName.Controls.Clear();
                    WinDisplay.Controls.Clear();

                    _liGearBoxes.Clear();

                    string nameLen = "";
                    for (int i = 0; i < GameManager.MAX_NAME_LEN; i++) { nameLen += "X"; }

                    _gName = new GUIText(nameLen);
                    _gName.AnchorToInnerSide(_winName, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
                    _gClass = new GUIText("XXXXXXXX");
                    _gClass.AnchorAndAlignToObject(_gName, SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN);

                    _sBoxHead = new SpecializedBox(_character.CharacterClass.ArmorType, _character.GetEquipment(GearTypeEnum.Head), FindMatchingItems);
                    _sBoxArmor = new SpecializedBox(_character.CharacterClass.ArmorType, _character.GetEquipment(GearTypeEnum.Chest), FindMatchingItems);
                    _sBoxWeapon = new SpecializedBox(_character.CharacterClass.WeaponType, _character.GetEquipment(GearTypeEnum.Weapon), FindMatchingItems);
                    _sBoxAccessory = new SpecializedBox(_character.CharacterClass.ArmorType, _character.GetEquipment(GearTypeEnum.Accessory), FindMatchingItems);

                    _sBoxArmor.AnchorToInnerSide(WinDisplay, SideEnum.TopRight, SPACING);
                    _sBoxHead.AnchorAndAlignToObject(_sBoxArmor, SideEnum.Left, SideEnum.Top, SPACING);
                    _sBoxAccessory.AnchorAndAlignToObject(_sBoxArmor, SideEnum.Bottom, SideEnum.Right, SPACING);
                    _sBoxWeapon.AnchorAndAlignToObject(_sBoxAccessory, SideEnum.Left, SideEnum.Top, SPACING);

                    _liGearBoxes.Add(_sBoxHead);
                    _liGearBoxes.Add(_sBoxArmor);
                    _liGearBoxes.Add(_sBoxWeapon);
                    _liGearBoxes.Add(_sBoxAccessory);

                    int barWidth = _sBoxArmor.DrawRectangle.Right - _sBoxHead.DrawRectangle.Left;
                    _gBarXP = new GUIOldStatDisplay(_character.GetXP, Color.Yellow, barWidth);
                    _gBarXP.AnchorToInnerSide(_winName, SideEnum.Right, SPACING);
                    _gBarXP.AlignToObject(_gName, SideEnum.CenterY);

                    _gLvl = new GUIText("LV. X");
                    _gLvl.AnchorAndAlignToObject(_gBarXP, SideEnum.Left, SideEnum.CenterY, SPACING);
                    _gLvl.SetText("LV. " + _character.ClassLevel);

                    _gBarHP = new GUIOldStatDisplay(_character.GetHP, Color.Green, barWidth);
                    _gBarHP.AnchorAndAlignToObject(_sBoxHead, SideEnum.Left, SideEnum.Top, SPACING);

                    if (_character == PlayerManager.PlayerCombatant)
                    {
                        _sBoxHat = new SpecializedBox(ClothingEnum.Hat, PlayerManager.PlayerActor.Hat, FindMatchingItems);
                        _sBoxShirt = new SpecializedBox(ClothingEnum.Chest, PlayerManager.PlayerActor.Chest, FindMatchingItems);

                        //_sBoxHat.AnchorToInnerSide(_winClothes, SideEnum.TopLeft, SPACING);
                        _sBoxShirt.AnchorAndAlignToObject(_sBoxHat, SideEnum.Right, SideEnum.Top, SPACING);

                        _liGearBoxes.Add(_sBoxHat);
                        _liGearBoxes.Add(_sBoxShirt);
                    }

                    _gStr = new GUIText("Str: 99");
                    _gDef = new GUIText("Def: 99");
                    _gMagic = new GUIText("Mag: 99");
                    _gRes = new GUIText("Res: 999");
                    _gSpd = new GUIText("Spd: 999");
                    _gStr.AnchorToInnerSide(WinDisplay, SideEnum.TopLeft, SPACING);
                    _gDef.AnchorAndAlignToObject(_gStr, SideEnum.Bottom, SideEnum.Left, SPACING);
                    _gMagic.AnchorAndAlignToObject(_gDef, SideEnum.Bottom, SideEnum.Left, SPACING);
                    _gRes.AnchorAndAlignToObject(_gMagic, SideEnum.Bottom, SideEnum.Left, SPACING);
                    _gSpd.AnchorAndAlignToObject(_gRes, SideEnum.Bottom, SideEnum.Left, SPACING);

                    _gName.SetText(_character.Name());
                    _gClass.SetText(_character.CharacterClass.Name());

                    DisplayStatText();

                    _equipWindow = new EquipWindow();
                    WinDisplay.AddControl(_equipWindow);
                    //_winClothes.AddControl(_equipWindow);
                }

                /// <summary>
                /// Delegate for hovering over equipment to equip. Updates the characters stats as apporpriate
                /// </summary>
                /// <param name="tempGear"></param>
                public void DisplayStatText(Equipment tempGear = null)
                {
                    bool compareTemp = true;
                    if (tempGear != null)
                    {
                        if (tempGear.WeaponType != WeaponEnum.None) { _character.EquipComparator(tempGear); }
                        else if (tempGear.ArmorType != ArmorTypeEnum.None) { _character.EquipComparator(tempGear); }
                        else
                        {
                            compareTemp = false;
                        }
                    }
                    else
                    {
                        compareTemp = false;
                        _character.ClearEquipmentCompare();
                    }

                    AssignStatText(_gStr, "Str", _character.Attribute(AttributeEnum.Strength), _character.TempAttribute(AttributeEnum.Strength), compareTemp);
                    AssignStatText(_gDef, "Def", _character.Attribute(AttributeEnum.Defence), _character.TempAttribute(AttributeEnum.Defence), compareTemp);
                    AssignStatText(_gMagic, "Mag", _character.Attribute(AttributeEnum.Magic), _character.TempAttribute(AttributeEnum.Magic), compareTemp);
                    AssignStatText(_gRes, "Res", _character.Attribute(AttributeEnum.Resistance), _character.TempAttribute(AttributeEnum.Resistance), compareTemp);
                    AssignStatText(_gSpd, "Spd", _character.Attribute(AttributeEnum.Speed), _character.TempAttribute(AttributeEnum.Speed), compareTemp);
                }

                private void AssignStatText(GUIText txtStat, string statString, int startStat, int tempStat, bool compareTemp)
                {
                    txtStat.SetText(statString + ": " + (compareTemp ? tempStat : startStat));
                    if (!compareTemp) { txtStat.SetColor(Color.White); }
                    else
                    {
                        if (startStat < tempStat) { txtStat.SetColor(Color.Green); }
                        else if (startStat > tempStat) { txtStat.SetColor(Color.Red); }
                        else { txtStat.SetColor(Color.White); }
                    }
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    if (_character != null)
                    {
                        _winName.Draw(spriteBatch);
                        WinDisplay.Draw(spriteBatch);
                        //if (_character == PlayerManager.World) { _winClothes.Draw(spriteBatch); }

                        if (_equipWindow.HasEntries())
                        {
                            _equipWindow.Draw(spriteBatch);
                        }
                    }
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (_character != null)
                    {
                        if (_equipWindow.HasEntries() && _equipWindow.ProcessLeftButtonClick(mouse))
                        {
                            Item olditem = _equipWindow.Box.BoxItem;

                            _equipWindow.Box.SetItem(_equipWindow.SelectedItem);
                            if (_equipWindow.Box.ItemType.Equals(ItemEnum.Equipment))
                            {
                                AssignEquipment((Equipment)_equipWindow.SelectedItem);
                            }
                            else if (_equipWindow.Box.ItemType.Equals(ItemEnum.Clothing))
                            {
                                PlayerManager.PlayerActor.SetClothes((Clothing)_equipWindow.SelectedItem);

                            }

                            InventoryManager.RemoveItemFromInventory(_equipWindow.SelectedItem);
                            if (olditem != null) { InventoryManager.AddToInventory(olditem); }

                            DisplayStatText();
                            GUIManager.CloseHoverWindow();
                            _equipWindow.Clear();
                        }
                        else
                        {
                            foreach (GUIObject c in WinDisplay.Controls)
                            {
                                rv = c.ProcessLeftButtonClick(mouse);
                                if (rv)
                                {
                                    GUIManager.CloseHoverWindow();
                                    break;
                                }
                            }

                            if (!rv && _character == PlayerManager.PlayerCombatant)
                            {
                                //foreach (GUIObject c in _winClothes.Controls)
                                //{
                                //    rv = c.ProcessLeftButtonClick(mouse);
                                //    if (rv)
                                //    {
                                //        GUIManager.CloseHoverWindow();
                                //        break;
                                //    }
                                //}
                            }
                        }
                    }
                    return rv;
                }

                public override bool ProcessRightButtonClick(Point mouse)
                {
                    bool rv = false;

                    if (_equipWindow.HasEntries())
                    {
                        _equipWindow.Clear();
                    }
                    else
                    {
                        foreach (SpecializedBox box in _liGearBoxes)
                        {
                            if (box.Contains(mouse) && box.BoxItem != null)
                            {
                                if (!box.WeaponType.Equals(WeaponEnum.None)) { _character.Unequip(GearTypeEnum.Weapon); }
                                else if (!box.ArmorType.Equals(ArmorTypeEnum.None)) { _character.Unequip(GearTypeEnum.Chest); }
                                else if (!box.ClothingType.Equals(ClothingEnum.None))
                                {
                                    PlayerManager.PlayerActor.RemoveClothes(((Clothing)box.BoxItem).ClothesType);
                                }

                                InventoryManager.AddToInventory(box.BoxItem);
                                box.SetItem(null);
                                rv = true;
                            }
                        }
                    }

                    return rv;
                }

                public override bool ProcessHover(Point mouse)
                {
                    bool rv = false;
                    if (_equipWindow.HasEntries())
                    {
                        rv = _equipWindow.ProcessHover(mouse);
                    }
                    else
                    {
                        foreach (SpecializedBox box in _liGearBoxes)
                        {
                            if (box.ProcessHover(mouse))
                            {
                                rv = true;
                            }
                        }

                        _gBarXP.ProcessHover(mouse);
                        _gBarHP.ProcessHover(mouse);
                    }
                    return rv;
                }

                public void SetAdventurer(ClassedCombatant c)
                {
                    _character = c;
                    Load();
                }

                /// <summary>
                /// Delegate method asssigned to the SpecializedItemBoxes
                /// When clicked, the itembox will find matching items int he players inventory.
                /// </summary>
                /// <param name="boxMatch"></param>
                private void FindMatchingItems(SpecializedBox boxMatch)
                {
                    GUIManager.CloseHoverWindow();

                    List<Item> liItems = new List<Item>();
                    foreach (Item i in InventoryManager.PlayerInventory)
                    {
                        if (i != null && i.ItemType.Equals(boxMatch.ItemType))
                        {
                            if (boxMatch.ItemType.Equals(ItemEnum.Equipment) && i.CompareType(ItemEnum.Equipment))
                            {
                                if (boxMatch.ArmorType != ArmorTypeEnum.None && ((Equipment)i).ArmorType == boxMatch.ArmorType)
                                {
                                    liItems.Add(i);
                                }
                                if (boxMatch.WeaponType != WeaponEnum.None && ((Equipment)i).WeaponType == boxMatch.WeaponType)
                                {
                                    liItems.Add(i);
                                }
                            }
                            else if (boxMatch.ItemType.Equals(ItemEnum.Clothing) && i.CompareType(ItemEnum.Clothing))
                            {
                                if (boxMatch.ClothingType != ClothingEnum.None && ((Clothing)i).ClothesType == boxMatch.ClothingType)
                                {
                                    liItems.Add(i);
                                }
                            }
                        }
                    }

                    _equipWindow.Load(boxMatch, liItems, DisplayStatText);
                    _equipWindow.AnchorAndAlignToObject(boxMatch, SideEnum.Right, SideEnum.CenterY);
                }

                private void AssignEquipment(Equipment item)
                {
                    _character.Equip(item);
                }
            }

            public class EquipWindow : GUIWindow
            {
                List<GUIItemBox> _gItemBoxes;
                public Item SelectedItem { get; private set; }

                ItemEnum _itemType;

                SpecializedBox _box;
                public SpecializedBox Box => _box;

                public delegate void DisplayEQ(Equipment test);
                private DisplayEQ _delDisplayEQ;

                public EquipWindow() : base(Window_2, 20, 20)
                {
                    _gItemBoxes = new List<GUIItemBox>();
                }

                public void Load(SpecializedBox box, List<Item> items, DisplayEQ del)
                {
                    _itemType = box.ItemType;
                    _delDisplayEQ = del;
                    _gItemBoxes = new List<GUIItemBox>();
                    Controls.Clear();
                    Width = 20;
                    Height = 20;
                    _box = box;

                    for (int i = 0; i < items.Count; i++)
                    {
                        GUIItemBox newBox = new GUIItemBox(items[i]);

                        if (i == 0) { newBox.AnchorToInnerSide(this, SideEnum.TopLeft); }
                        else { newBox.AnchorAndAlignToObject(_gItemBoxes[i - 1], SideEnum.Right, SideEnum.Bottom); }

                        _gItemBoxes.Add(newBox);
                    }

                    Resize();
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    foreach (GUIItemBox g in _gItemBoxes)
                    {
                        if (g.Contains(mouse))
                        {
                            SelectedItem = g.BoxItem;
                            rv = true;
                            break;
                        }
                    }
                    return rv;
                }
                public override bool ProcessHover(Point mouse)
                {
                    bool rv = false;

                    Equipment temp = null;
                    foreach (GUIItemBox box in _gItemBoxes)
                    {
                        rv = box.ProcessHover(mouse);
                        if (rv && _itemType.Equals(ItemEnum.Equipment))
                        {
                            temp = (Equipment)box.BoxItem;
                        }
                    }

                    if (_itemType.Equals(ItemEnum.Equipment))
                    {
                        _delDisplayEQ(temp);
                    }

                    return rv;
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    if (HasEntries())
                    {
                        base.Draw(spriteBatch);
                    }
                }
                public bool HasEntries() { return _gItemBoxes.Count > 0; }

                public void Clear()
                {
                    Controls.Clear();
                    _gItemBoxes.Clear();
                }
            }
        }
        public class HUDFriendship : GUIMainObject
        {
            GUIWindow _gWindow;
            GUIList _villagerList;

            public HUDFriendship()
            {
                _gWindow = SetMainWindow(GUIManager.MAIN_COMPONENT_WIDTH + 100, GUIManager.MAIN_COMPONENT_WIDTH);
                List<GUIObject> vList;
                vList = new List<GUIObject>();

                foreach (Villager n in DataManager.DIVillagers.Values)
                {
                    FriendshipBox f = new FriendshipBox(n, _gWindow.InnerWidth() - GUIList.BTNSIZE);

                    /*if (vList.Count == 0) { f.AnchorToInnerSide(_gWindow, GUIObject.SideEnum.TopLeft); }
                    else
                    {
                        f.AnchorAndAlignToObject(vList[vList.Count - 1], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left);   //-2 because we start at i=1
                    }*/

                    vList.Add(f);
                }

                _villagerList = new GUIList(vList, 10, 4, _gWindow.InnerHeight());
                _villagerList.CenterOnScreen(); //.AnchorToInnerSide(_gWindow, GUIObject.SideEnum.TopLeft);//
                AddControl(_villagerList);
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
            }

            public override void Update(GameTime gTime)
            {
                base.Update(gTime);
            }
            public class FriendshipBox : GUIWindow
            {
                private BitmapFont _font;
                GUIText _gTextName;
                GUIImage _gAdventure;
                GUIImage _gGift;
                List<GUIImage> _liFriendship;

                public FriendshipBox(Villager v, int mainWidth) : base(GUIWindow.Window_2, mainWidth, 16)
                {
                    _liFriendship = new List<GUIImage>();
                    _font = DataManager.GetBitMapFont(DataManager.FONT_NEW);
                    _gTextName = new GUIText("XXXXXXXXXX");
                    if (v.GetFriendshipLevel() == 0)
                    {
                        _liFriendship.Add(new GUIImage(new Rectangle(51, 68, 10, 9), ScaleIt(10), ScaleIt(9), DataManager.DIALOGUE_TEXTURE));
                    }
                    else
                    {
                        int notches = v.GetFriendshipLevel() - 1;
                        int x = 0;
                        if (notches <= 3) { x = 16; }
                        else if (notches <= 6) { x = 32; }
                        else { x = 51; }


                        while (notches > 0)
                        {
                            _liFriendship.Add(new GUIImage(new Rectangle(x, 68, 10, 9), ScaleIt(10), ScaleIt(9), DataManager.DIALOGUE_TEXTURE));
                            notches--;
                        }
                    }

                    _liFriendship[0].AnchorToInnerSide(this, SideEnum.TopLeft);
                    _gTextName.AlignToObject(_liFriendship[0], SideEnum.CenterY);
                    _gTextName.AnchorToInnerSide(this, SideEnum.Left);
                    for (int j = 0; j < _liFriendship.Count; j++)
                    {
                        if (j == 0) { _liFriendship[j].AnchorAndAlignToObject(_gTextName, SideEnum.Right, SideEnum.CenterY, GUIManager.STANDARD_MARGIN); }
                        else { _liFriendship[j].AnchorAndAlignToObject(_liFriendship[j - 1], SideEnum.Right, SideEnum.CenterY, GUIManager.STANDARD_MARGIN); }
                    }
                    _gTextName.SetText(v.Name());

                    _gGift = new GUIImage(new Rectangle(19, 52, 10, 8), ScaleIt(10), ScaleIt(8), DataManager.DIALOGUE_TEXTURE);
                    _gGift.AnchorToInnerSide(this, SideEnum.Right);
                    _gGift.AlignToObject(_gTextName, SideEnum.CenterY);
                    _gGift.Alpha((v.CanGiveGift) ? 1 : 0.3f);

                    if (v.CanBeMarried)
                    {
                        _gAdventure = new GUIImage(new Rectangle(4, 52, 8, 9), ScaleIt(8), ScaleIt(9), DataManager.DIALOGUE_TEXTURE);
                        _gAdventure.AnchorAndAlignToObject(_gGift, SideEnum.Left, SideEnum.CenterY, GUIManager.STANDARD_MARGIN);
                        if (PlayerManager.GetParty().Contains(v.CombatVersion))
                        {
                            _gAdventure.SetColor(Color.Gold);
                        }
                        else { _gAdventure.Alpha(v.Combatant ? 1 : 0.3f); }
                    }

                    Resize();
                }

                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    return rv;
                }

                public override bool ProcessRightButtonClick(Point mouse)
                {
                    bool rv = false;
                    return rv;
                }

                public override bool ProcessHover(Point mouse)
                {
                    bool rv = true;
                    return rv;
                }
            }
        }

        public class HUDOptions : GUIMainObject
        {
            GUICheck _gHideMiniInventory;
            GUIButton _btnSave;

            GUIText _gSoundSettings;
            GUINumberControl _gVolumeControl;
            GUINumberControl _gEffectControl;
            GUICheck _gMute;

            const int SOUND_VOLUME_SCALAR = 100;

            public HUDOptions()
            {
                _winMain = SetMainWindow();

                _gHideMiniInventory = new GUICheck("Hide Mini Inventory", GameManager.HideMiniInventory);
                _gHideMiniInventory.AnchorToInnerSide(_winMain, SideEnum.TopLeft, 8);

                _gSoundSettings = new GUIText("Sound Settings");
                _gSoundSettings.AnchorAndAlignToObject(_gHideMiniInventory, SideEnum.Bottom, SideEnum.Left, 32);

                _gVolumeControl = new GUINumberControl("Music", SoundManager.MusicVolume * SOUND_VOLUME_SCALAR, UpdateMusicVolume);
                _gVolumeControl.AnchorAndAlignToObject(_gSoundSettings, SideEnum.Bottom, SideEnum.Left);
                _gVolumeControl.MoveBy(new Vector2(32, 0));

                _gEffectControl = new GUINumberControl("Effects", SoundManager.EffectVolume * SOUND_VOLUME_SCALAR, UpdateEffectsVolume);
                _gEffectControl.AnchorAndAlignToObject(_gVolumeControl, SideEnum.Bottom, SideEnum.Left);

                _gMute = new GUICheck("Mute All", SoundManager.IsMuted, ProcessMuteAll);
                _gMute.AnchorAndAlignToObject(_gEffectControl, SideEnum.Bottom, SideEnum.Left);

                _btnSave = new GUIButton("Save", BtnSave);
                _btnSave.AnchorToInnerSide(_winMain, SideEnum.BottomRight);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                foreach (GUIObject c in Controls)
                {
                    rv = c.ProcessLeftButtonClick(mouse);
                    if (rv) { break; }
                }
                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;
                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = true;

                return rv;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
            }

            public override void Update(GameTime gTime)
            {
                base.Update(gTime);
            }

            public void ProcessMuteAll()
            {
                if(SoundManager.IsMuted) {
                    SoundManager.UnmuteAllSound();
                    _gVolumeControl.RefreshValue(SoundManager.MusicVolume * SOUND_VOLUME_SCALAR);
                    _gEffectControl.RefreshValue(SoundManager.EffectVolume * SOUND_VOLUME_SCALAR);
                } else {
                    SoundManager.MuteAllSound();
                    _gVolumeControl.RefreshValue(SoundManager.MusicVolume * SOUND_VOLUME_SCALAR);
                    _gEffectControl.RefreshValue(SoundManager.EffectVolume * SOUND_VOLUME_SCALAR);
                }
            }
            public void UpdateMusicVolume()
            {
                SoundManager.SetMusicVolume((float)_gVolumeControl.Value / 100.0f);
            }

            public void UpdateEffectsVolume()
            {
                SoundManager.SetEffectVolume((float)_gEffectControl.Value / 100.0f);
            }
            public void BtnSave()
            {
                GameManager.HideMiniInventory = _gHideMiniInventory.Checked();
                GUIManager.CloseMainObject();
            }

            private class GUINumberControl : GUIObject
            {
                float _fValChange = 10;
                float _fMin;
                float _fMax;
                float _fValue;
                public float Value => _fValue;

                GUIText _gText;
                GUIText _gValue;
                GUIButton _btnLeft;
                GUIButton _btnRight;

                public delegate void ActionDelegate();
                ActionDelegate _del;

                public GUINumberControl(string text, float baseValue, ActionDelegate del, int min = 0, int max = 100)
                {
                    _del = del;
                    _fMin = min;
                    _fMax = max;
                    _gText = new GUIText("XXXXXXXXXX");

                    _btnLeft = new GUIButton(new Rectangle(272, 112, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, BtnLeftClick);
                    _btnLeft.AnchorAndAlignToObject(_gText, SideEnum.Right, SideEnum.CenterY, 12);

                    _gValue = new GUIText("000");
                    _gValue.AnchorAndAlignToObject(_btnLeft, SideEnum.Right, SideEnum.CenterY, 12);

                    _btnRight = new GUIButton(new Rectangle(256, 112, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, BtnRightClick);
                    _btnRight.AnchorAndAlignToObject(_gValue, SideEnum.Right, SideEnum.CenterY, 12);

                    _fValue = baseValue;
                    UpdateValue();

                    _gText.SetText(text);

                    AddControl(_gText);
                    AddControl(_btnLeft);
                    AddControl(_gValue);
                    AddControl(_btnRight);

                    Height = _btnLeft.Height;
                    Width = _btnRight.Right - _gText.Left;
                }

                public void BtnLeftClick()
                {
                    if (_fValue - _fValChange >= _fMin)
                    {
                        _fValue -= _fValChange;
                        UpdateValue();

                        _del();
                    }
                }
                public void BtnRightClick()
                {
                    if (_fValue + _fValChange <= _fMax)
                    {
                        _fValue += _fValChange;
                        UpdateValue();

                        _del();
                    }
                }
                public void RefreshValue(float value)
                {
                    _fValue = value;
                    UpdateValue();
                }
                private void UpdateValue()
                {
                    _gValue.SetText((int)_fValue);
                    _gValue.AnchorAndAlignToObject(_btnRight, SideEnum.Left, SideEnum.CenterY, 12);
                }
            }
        }
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
