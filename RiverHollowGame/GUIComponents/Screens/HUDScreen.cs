using System.Linq;
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
using RiverHollow.Items;
using RiverHollow.Misc;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIItemBox;
using static RiverHollow.GUIComponents.GUIObjects.NPCDisplayBox;
using static RiverHollow.GUIComponents.Screens.HUDMenu.HUDManagement.MgmtWindow;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Items.Item;


namespace RiverHollow.GUIComponents.Screens
{
    public class HUDScreen : GUIScreen
    {
        List<HUDNewQuest> _liQuestIcons;

        GUIButton _btnSkipCutscene;
        GUIObject _gMenu;
        GUIStatDisplay _gHealthDisplay;
        GUIStatDisplay _gStaminaDisplay;
        GUIMoneyDisplay _gMoney;
        GUIMonsterEnergyDisplay _gEnergy;
        GUIDungeonKeyDisplay _gDungeonKeys;

        HUDMiniInventory _gInventory;
        HUDCalendar _gCalendar;
        GUIItemBox _addedItem;
        double _dTimer;

        public HUDScreen()
        {
            _liQuestIcons = new List<HUDNewQuest>();
            _gHealthDisplay = new GUIStatDisplay(PlayerManager.World.GetHP, Color.Green);
            _gHealthDisplay.AnchorToScreen(this, SideEnum.TopLeft, 10);
            AddControl(_gHealthDisplay);
            _gStaminaDisplay = new GUIStatDisplay(PlayerManager.GetStamina, Color.Red);
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
            AddControl(_gInventory);

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
                _addedItem.AnchorAndAlignToObject(_gInventory, SideEnum.Left, SideEnum.CenterY, 10);
                _dTimer = 1;
                AddControl(_addedItem);
                InventoryManager.AddedItemList.Remove(InventoryManager.AddedItemList[0]);
            }
            else
            {
                //If there are more items to add, there is currently an ItemPickup Display and the next Item to add is the same as the one being displayed
                //Remove it fromt he list of items to show added, add the current number tot he display, and refresh the display.
                if(InventoryManager.AddedItemList.Count > 0 && _addedItem != null && InventoryManager.AddedItemList[0].ItemID == _addedItem.BoxItem.ItemID)
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
            if (!TakingInput())
            {
                if (InputManager.CheckPressedKey(Keys.Escape))
                {
                    if (_gMenu == null) { OpenMenu(); }
                    else { CloseMenu(); }
                }
                if (InputManager.CheckPressedKey(Keys.P))
                {
                    if (IsPaused()) { Pause(); }
                    else { Unpause(); }
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
        public override void OpenTextWindow(string text, bool open = true)
        {
            base.OpenTextWindow(text, open);
            _gInventory.Show(false);
        }
        public override void OpenTextWindow(string text, TalkingActor talker, bool open = true)
        {
            base.OpenTextWindow(text, talker, open);
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
        public bool IsMenuOpen() { return _gMenu != null; }
        public void OpenMenu()
        {
            _gMenu = new HUDMenu();
            AddControl(_gMenu);
            GameManager.Pause();
        }
        public void CloseMenu()
        {
            if (_gMenu != null)
            {
                RemoveControl(_gMenu);
                _gMenu = null;
                GameManager.Unpause();
            }
        }
        #endregion

        public override void NewQuestIcon(bool complete) {
            HUDNewQuest newQuest = new HUDNewQuest(complete, RemoveQuestIcon);

            if (_liQuestIcons.Count == 0) { newQuest.AnchorToScreen(SideEnum.Right, 12); }
            else { newQuest.AnchorAndAlignToObject(_liQuestIcons[_liQuestIcons.Count - 1], SideEnum.Top, SideEnum.Left, 4); }

            _liQuestIcons.Add(newQuest);
            AddControl(newQuest);
        }
        private void RemoveQuestIcon(HUDNewQuest q)
        {
            _liQuestIcons.Remove(q);
            RemoveControl(q);
        }

        public override void AddSkipCutsceneButton() {
            _btnSkipCutscene = new GUIButton(new Rectangle(64, 80, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, CutsceneManager.SkipCutscene);
            _btnSkipCutscene.AnchorToScreen(SideEnum.BottomRight);
            AddControl(_btnSkipCutscene);
        }
        public override void RemoveSkipCutsceneButton() {
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

        public HUDMiniInventory() : base(GUIWindow.BrownWin, TileSize, TileSize)
        {
            _btnChangeRow = new GUIButton(new Rectangle(256, 96, 16, 16), ScaledTileSize, ScaledTileSize, DataManager.DIALOGUE_TEXTURE, RowUp);
            _btnChangeRow.FadeOnDisable(false);
            _liItems = new List<GUIItemBox>();
            _fBarFade = GameManager.HideMiniInventory ? FADE_OUT : 1.0f;
            Alpha(_fBarFade);
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
        }

        public override void Update(GameTime gTime)
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

            if (GameManager.IsRunning() && Contains(mouse))
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

            if (GameManager.IsRunning() && Contains(mouse))
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
            if (GameManager.IsRunning())
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
            if (GameManager.HUDItemRow < InventoryManager.maxItemRows - 1)
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
        GUIButton _btnExitGame;
        GUIButton _btnQuestLog;
        GUIButton _btnInventory;
        GUIButton _btnParty;
        GUIButton _btnManagement;
        GUIButton _btnOptions;
        GUIButton _btnFriendship;
        GUIMainObject _gMenuObject;
        List<GUIObject> _liButtons;

        bool _bOpen = false;
        bool _bClose = false;

        public HUDMenu()
        {
            _btnInventory = new GUIButton("Inventory", BtnInventory);
            AddControl(_btnInventory);

            _btnParty = new GUIButton("Party", BtnParty);
            AddControl(_btnParty);

            _btnQuestLog = new GUIButton("Quest Log", BtnQuestLog);
            AddControl(_btnQuestLog);

            _btnExitGame = new GUIButton("Exit Game", BtnExitGame);
            AddControl(_btnExitGame);

            _btnOptions = new GUIButton("Options", BtnOptions);
            AddControl(_btnOptions);

            _btnManagement = new GUIButton("Buildings", BtnManagement);
            AddControl(_btnManagement);

            _btnFriendship = new GUIButton("Friends", BtnFriendship);
            AddControl(_btnFriendship);

            _liButtons = new List<GUIObject>() { _btnInventory, _btnParty, _btnManagement, _btnQuestLog, _btnOptions, _btnFriendship, _btnExitGame };
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
            _gMenuObject = new HUDInventoryDisplay();
            _gMenuObject.CenterOnScreen();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnQuestLog()
        {
            _gMenuObject = new HUDQuestLog();
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
        public void BtnManagement()
        {
            _gMenuObject = new HUDManagement();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        public void BtnFriendship()
        {
            _gMenuObject = new HUDFriendship();
            GUIManager.OpenMainObject(_gMenuObject);
        }
        #endregion

        public class HUDQuestLog : GUIMainObject
        {
            //public static int BTNSIZE = ScaledTileSize;
            public static int MAX_SHOWN_QUESTS = 4;
            public static int QUEST_SPACING = 20;
            public static int QUESTBOX_WIDTH = 544; //(GUIManager.MAIN_COMPONENT_WIDTH) - (_gWindow.EdgeSize * 2) - ScaledTileSize
            public static int QUESTBOX_HEIGHT = 128; //(GUIManager.MAIN_COMPONENT_HEIGHT / HUDQuestLog.MAX_SHOWN_QUESTS) - (_gWindow.EdgeSize * 2)
            List<GUIObject> _liQuests;
            DetailBox _detailWindow;
            GUIList _gList;

            public HUDQuestLog()
            {
                _winMain = SetMainWindow();

                _liQuests = new List<GUIObject>();
                _detailWindow = new DetailBox(GUIWindow.RedWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
                _detailWindow.Show(false);
                _detailWindow.CenterOnScreen();
                AddControl(_detailWindow);

                for (int i = 0; i < PlayerManager.QuestLog.Count; i++)
                {
                    QuestBox q = new QuestBox(QUESTBOX_WIDTH, QUESTBOX_HEIGHT, OpenDetailBox);
                    q.SetQuest(PlayerManager.QuestLog[i]);
                    _liQuests.Add(q);
                }

                _gList = new GUIList(_liQuests, MAX_SHOWN_QUESTS, QUEST_SPACING/*, _gWindow.Height*/);
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
                foreach (QuestBox c in _liQuests)
                {
                    rv = c.ProcessHover(mouse);
                    if (rv)
                    {
                        break;
                    }
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

            private void OpenDetailBox(Quest q)
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

            public class QuestBox : GUIObject
            {
                GUIWindow _window;
                GUIText _gName;
                GUIText _gGoalProgress;
                public Quest TheQuest { get; private set; }
                public bool ClearThis;
                public delegate void ClickDelegate(Quest q);
                private ClickDelegate _delAction;

                public QuestBox(int width, int height, ClickDelegate del)
                {
                    _delAction = del;

                    int boxHeight = height;
                    int boxWidth = width;
                    _window = new GUIWindow(GUIWindow.RedWin, boxWidth, boxHeight);
                    AddControl(_window);
                    Width = _window.Width;
                    Height = _window.Height;
                    TheQuest = null;
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    if (TheQuest != null && Show())
                    {
                        _window.Draw(spriteBatch);
                    }
                }
                public override bool ProcessLeftButtonClick(Point mouse)
                {
                    bool rv = false;
                    if (Contains(mouse))
                    {
                        _delAction(TheQuest);
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

                public void SetQuest(Quest q)
                {
                    TheQuest = q;
                    _gName = new GUIText(TheQuest.Name);
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

                public void SetData(Quest q)
                {
                    Controls.Clear();
                    _name = new GUIText(q.Name);
                    _name.AnchorToInnerSide(this, SideEnum.TopLeft);

                    _desc = new GUIText(q.Description);
                    _desc.ParseText(3, this.MidWidth(), true);
                    _desc.AnchorAndAlignToObject(_name, SideEnum.Bottom, SideEnum.Left, _name.CharHeight);

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
                _charBox = new CharacterDetailObject(PlayerManager.World);
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

                public PositionMap(ClickDelegate del) : base(BrownWin, 16, 16)
                {
                    _delAction = del;

                    //Actual entries will be one higher since we go to 0 inclusive
                    int maxColIndex = 2;
                    int maxRowIndex = 2;

                    int spacing = 10;
                    _arrStartPositions = new StartPosition[maxColIndex +1, maxRowIndex +1]; //increment by one as stated above
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
                    _currentCharacter = PlayerManager.World;

                    //Iterate over each member of the party and retrieve their starting position.
                    //Assigns the character to the starting position and assigns the current position
                    //to the Player Character's
                    foreach (ClassedCombatant c in PlayerManager.GetParty())
                    {
                        Vector2 vec = c.StartPosition;
                        _arrStartPositions[(int)vec.X, (int)vec.Y].SetCharacter(c, (c == _currentCharacter));
                        if (c == _currentCharacter)
                        {
                            _currPosition = _arrStartPositions[(int)vec.X, (int)vec.Y];
                        }
                    }
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
                                _currPosition.SetCharacter(_currentCharacter, true);
                                _currentCharacter.SetStartPosition(new Vector2(_currPosition.Col, _currPosition.Row));
                            }
                            else
                            {
                                _currPosition?.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                                _currPosition = sp;
                                //Set the currentCharacter to the selected character.
                                //Call up to the parent object to redisplay data.
                                _currentCharacter = sp.Character;
                                _delAction(_currentCharacter);
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
                    public StartPosition(int col, int row) : base(new Rectangle(0, 112, 16, 16), TileSize, TileSize, DataManager.FILE_WORLDOBJECTS)
                    {
                        _iCol = col;
                        _iRow = row;

                        SetScale(Scale);
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
                        _character = c;
                        if (c != null)
                        {
                            if (c == PlayerManager.World) { _sprite = new GUICharacterSprite(true); }
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
                        _sprite.PlayAnimation(verb,dir);
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
                SpecializedBox _sBoxWrist;
                SpecializedBox _sBoxShirt;
                SpecializedBox _sBoxHat;
                GUIButton _btnSwap;

                GUIText _gName, _gClass, _gLvl, _gStr, _gDef, _gMagic, _gRes, _gSpd;
                GUIStatDisplay _gBarXP, _gBarHP, _gBarMP;

                public CharacterDetailObject(ClassedCombatant c)
                {
                    _winName = new GUIWindow(GUIWindow.RedWin, (GUIManager.MAIN_COMPONENT_WIDTH) - (GUIWindow.RedWin.Edge * 2), 10);
                    WinDisplay = new GUIWindow(GUIWindow.RedWin, (GUIManager.MAIN_COMPONENT_WIDTH) - (GUIWindow.RedWin.Edge * 2), (GUIManager.MAIN_COMPONENT_HEIGHT / 4) - (GUIWindow.RedWin.Edge * 2));
                    WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left);
                    //_winClothes = new GUIWindow(GUIWindow.RedWin, 10, 10);
                    //_winClothes.AnchorAndAlignToObject(WinDisplay, SideEnum.Bottom, SideEnum.Left);

                    _character = c;
                    _font = DataManager.GetBitMapFont(DataManager.FONT_MAIN);

                    _liGearBoxes = new List<SpecializedBox>();
                    Load();

                    _winName.Resize();
                    _winName.Height += SPACING;

                    WinDisplay.Resize();
                    WinDisplay.Height += SPACING;

                    //_winClothes.Resize();
                    //_winClothes.Height += SPACING;
                    //_winClothes.Width += SPACING;

                    WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left, 4);
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

                    _sBoxHead = new SpecializedBox(_character.CharacterClass.ArmorType, _character.Head.GetItem(), FindMatchingItems);
                    _sBoxArmor = new SpecializedBox(_character.CharacterClass.ArmorType, _character.Armor.GetItem(), FindMatchingItems);
                    _sBoxWeapon = new SpecializedBox(_character.CharacterClass.WeaponType, _character.Weapon.GetItem(), FindMatchingItems);
                    _sBoxWrist = new SpecializedBox(_character.CharacterClass.ArmorType, _character.Wrist.GetItem(), FindMatchingItems);

                    _sBoxArmor.AnchorToInnerSide(WinDisplay, SideEnum.TopRight, SPACING);
                    _sBoxHead.AnchorAndAlignToObject(_sBoxArmor, SideEnum.Left, SideEnum.Top, SPACING);
                    _sBoxWrist.AnchorAndAlignToObject(_sBoxArmor, SideEnum.Bottom, SideEnum.Right, SPACING);
                    _sBoxWeapon.AnchorAndAlignToObject(_sBoxWrist, SideEnum.Left, SideEnum.Top, SPACING);

                    _liGearBoxes.Add(_sBoxHead);
                    _liGearBoxes.Add(_sBoxArmor);
                    _liGearBoxes.Add(_sBoxWeapon);
                    _liGearBoxes.Add(_sBoxWrist);

                    int barWidth = _sBoxArmor.DrawRectangle.Right - _sBoxHead.DrawRectangle.Left;
                    _gBarXP = new GUIStatDisplay(_character.GetXP, Color.Yellow, barWidth);
                    _gBarXP.AnchorToInnerSide(_winName, SideEnum.Right, SPACING);
                    _gBarXP.AlignToObject(_gName, SideEnum.CenterY);

                    _gLvl = new GUIText("LV. X");
                    _gLvl.AnchorAndAlignToObject(_gBarXP, SideEnum.Left, SideEnum.CenterY, SPACING);
                    _gLvl.SetText("LV. " + _character.ClassLevel);

                    _gBarHP = new GUIStatDisplay(_character.GetHP, Color.Green, barWidth);
                    _gBarHP.AnchorAndAlignToObject(_sBoxHead, SideEnum.Left, SideEnum.Top, SPACING);
                    _gBarMP = new GUIStatDisplay(_character.GetMP, Color.LightBlue, barWidth);
                    _gBarMP.AnchorAndAlignToObject(_gBarHP, SideEnum.Bottom, SideEnum.Right, SPACING);

                    if (_character == PlayerManager.World)
                    {
                        _sBoxHat = new SpecializedBox(ClothesEnum.Hat, PlayerManager.World.Hat, FindMatchingItems);
                        _sBoxShirt = new SpecializedBox(ClothesEnum.Body, PlayerManager.World.Body, FindMatchingItems);

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

                    _gName.SetText(_character.Name);
                    _gClass.SetText(_character.CharacterClass.Name);

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
                        if (tempGear.WeaponType != WeaponEnum.None) { _character.Weapon.SetTemp(tempGear); }
                        else if (tempGear.ArmorType != ArmorEnum.None) { _character.Armor.SetTemp(tempGear); }
                        else
                        {
                            compareTemp = false;
                        }
                    }
                    else
                    {
                        compareTemp = false;
                        _character.Weapon.SetTemp(null);
                        _character.Armor.SetTemp(null);
                    }

                    AssignStatText(_gStr, "Str", _character.StatStr, _character.TempStatStr, compareTemp);
                    AssignStatText(_gDef, "Def", _character.StatDef, _character.TempStatDef, compareTemp);
                    AssignStatText(_gMagic, "Mag", _character.StatMag, _character.TempStatMag, compareTemp);
                    AssignStatText(_gRes, "Res", _character.StatRes, _character.TempStatRes, compareTemp);
                    AssignStatText(_gSpd, "Spd", _character.StatSpd, _character.TempStatSpd, compareTemp);
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
                            else if (_equipWindow.Box.ItemType.Equals(ItemEnum.Clothes))
                            {
                                PlayerManager.World.SetClothes((Clothes)_equipWindow.SelectedItem);

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

                            if (!rv && _character == PlayerManager.World)
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
                                if (!box.WeaponType.Equals(WeaponEnum.None)) { _character.Weapon = null; }
                                else if (!box.ArmorType.Equals(ArmorEnum.None)) { _character.Armor = null; }
                                else if (!box.ClothingType.Equals(ClothesEnum.None))
                                {
                                    PlayerManager.World.RemoveClothes(((Clothes)box.BoxItem).ClothesType);
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
                        _gBarMP.ProcessHover(mouse);
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
                                if (boxMatch.ArmorType != ArmorEnum.None && ((Equipment)i).ArmorType == boxMatch.ArmorType)
                                {
                                    liItems.Add(i);
                                }
                                if (boxMatch.WeaponType != WeaponEnum.None && ((Equipment)i).WeaponType == boxMatch.WeaponType)
                                {
                                    liItems.Add(i);
                                }
                            }
                            else if (boxMatch.ItemType.Equals(ItemEnum.Clothes) && i.CompareType(ItemEnum.Clothes))
                            {
                                if (boxMatch.ClothingType != ClothesEnum.None && ((Clothes)i).ClothesType == boxMatch.ClothingType)
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
                    if (item.WeaponType != WeaponEnum.None) { _character.Weapon.SetGear(item); }
                    else if (item.ArmorType != ArmorEnum.None) { _character.Armor.SetGear(item); }
                }
            }

            public class EquipWindow : GUIWindow
            {
                List<GUIItemBox> _gItemBoxes;
                Item _selectedItem;
                public Item SelectedItem => _selectedItem;

                ItemEnum _itemType;

                SpecializedBox _box;
                public SpecializedBox Box => _box;

                public delegate void DisplayEQ(Equipment test);
                private DisplayEQ _delDisplayEQ;

                public EquipWindow() : base(BrownWin, 20, 20)
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
                            _selectedItem = g.BoxItem;
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

                foreach (Villager n in DataManager.DiNPC.Values)
                {
                    FriendshipBox f = new FriendshipBox(n, _gWindow.MidWidth() - GUIList.BTNSIZE);

                    /*if (vList.Count == 0) { f.AnchorToInnerSide(_gWindow, GUIObject.SideEnum.TopLeft); }
                    else
                    {
                        f.AnchorAndAlignToObject(vList[vList.Count - 1], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left);   //-2 because we start at i=1
                    }*/

                    vList.Add(f);
                }

                _villagerList = new GUIList(vList, 10, 4, _gWindow.MidHeight());
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

                public FriendshipBox(Villager c, int mainWidth) : base(GUIWindow.BrownWin, mainWidth, 16)
                {
                    _liFriendship = new List<GUIImage>();
                    _font = DataManager.GetBitMapFont(DataManager.FONT_MAIN);
                    _gTextName = new GUIText("XXXXXXXXXX");
                    if (c.GetFriendshipLevel() == 0)
                    {
                        _liFriendship.Add(new GUIImage(new Rectangle(51, 68, 10, 9), ScaleIt(10), ScaleIt(9), DataManager.DIALOGUE_TEXTURE));
                    }
                    else
                    {
                        int notches = c.GetFriendshipLevel() - 1;
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
                    _gTextName.SetText(c.Name);

                    _gGift = new GUIImage(new Rectangle(19, 52, 10, 8), ScaleIt(10), ScaleIt(8), DataManager.DIALOGUE_TEXTURE);
                    _gGift.AnchorToInnerSide(this, SideEnum.Right);
                    _gGift.AlignToObject(_gTextName, SideEnum.CenterY);
                    _gGift.Alpha((c.CanGiveGift) ? 1 : 0.3f);

                    if (c.IsEligible())
                    {
                        EligibleNPC e = (EligibleNPC)c;
                        _gAdventure = new GUIImage(new Rectangle(4, 52, 8, 9), ScaleIt(8), ScaleIt(9), DataManager.DIALOGUE_TEXTURE);
                        _gAdventure.AnchorAndAlignToObject(_gGift, SideEnum.Left, SideEnum.CenterY, GUIManager.STANDARD_MARGIN);
                        if (PlayerManager.GetParty().Contains(e))
                        {
                            _gAdventure.SetColor(Color.Gold);
                        }
                        else { _gAdventure.Alpha(e.CanJoinParty ? 1 : 0.3f); }
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
        public class HUDManagement : GUIMainObject
        {
            public enum ActionTypeEnum { View, Sell, Buy, Upgrade };
            public ActionTypeEnum Action { get; private set; }
            public static int BTN_PADDING = 20;

            MgmtWindow _mgmtWindow;

            List<GUIObject> _liWorkers;

            Adventurer _worker;
            int _iCost;

            public HUDManagement(ActionTypeEnum action = ActionTypeEnum.View)
            {
                Action = action;
                _liWorkers = new List<GUIObject>();

                _mgmtWindow = new MainBuildingsWin(this);
                AddControl(_mgmtWindow);
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                foreach (GUIObject g in Controls)
                {
                    rv = g.ProcessLeftButtonClick(mouse);
                    if (rv) { break; }
                }

                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;
                rv = _mgmtWindow.ProcessRightButtonClick(mouse);

                return rv;
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = true;

                rv = _mgmtWindow.ProcessHover(mouse);

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

            public void HandleBuildingSelection(Building selectedBuilding)
            {
                if (_worker == null)
                {
                    if (selectedBuilding != null)
                    {
                        RemoveControl(_mgmtWindow);
                        _mgmtWindow = new BuildingDetailsWin(this, selectedBuilding);
                        AddControl(_mgmtWindow);
                    }
                }
                else
                {
                    if (_worker.Building != null)
                    {
                        _worker.Building.RemoveWorker(_worker);
                    }

                    bool addSuccess = selectedBuilding.AddWorker(_worker);

                    if (Action == ActionTypeEnum.Buy)
                    {
                        if (addSuccess)
                        {
                            PlayerManager.TakeMoney(_iCost);
                            GUIManager.OpenMainObject(new HUDNamingWindow(_worker));
                            _worker = null;
                        }
                        else
                        {
                            GUIManager.OpenTextWindow("Please choose an empty building.");
                        }
                    }
                    else
                    {
                        _worker = null;
                    }
                }
            }

            public void HandleMoveWorker(Adventurer worldAdventurer)
            {
                if (worldAdventurer != null)
                {
                    RemoveControl(_mgmtWindow);
                    _mgmtWindow = new MainBuildingsWin(this);
                    _worker = worldAdventurer;
                    AddControl(_mgmtWindow);
                }
            }

            public void SetMgmtWindow(MgmtWindow newWin)
            {
                RemoveControl(_mgmtWindow);
                _mgmtWindow = newWin;
                AddControl(_mgmtWindow);
            }

            public void Sell()
            {
                Action = ActionTypeEnum.Sell;
            }

            public bool Selling()
            {
                return Action == ActionTypeEnum.Sell;
            }

            public void PurchaseWorker(Adventurer w, int cost)
            {
                if (w != null)
                {
                    _iCost = cost;
                    _worker = w;
                    Action = ActionTypeEnum.Buy;
                    SetMgmtWindow(new MainBuildingsWin(this, w));
                }
            }

            public override bool Contains(Point mouse)
            {
                bool rv = false;
                foreach (GUIObject g in Controls)
                {
                    if (g.Contains(mouse))
                    {
                        rv = true;
                        break;
                    }
                }

                return rv;
            }

            public class MgmtWindow : GUIObject
            {
                protected GUIWindow _window;
                protected HUDManagement _parent;
                protected List<GUIObject> _liButtons;

                private MgmtWindow(HUDManagement s)
                {
                    _liButtons = new List<GUIObject>();
                    Controls = new List<GUIObject>();
                    _parent = s;

                    _window = new GUIWindow(GUIWindow.RedWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
                    AddControl(_window);

                    AddControl(_window);
                    Width = _window.Width;
                    Height = _window.Height;

                    this.CenterOnScreen();
                }

                public override bool ProcessRightButtonClick(Point mouse)
                {
                    bool rv = base.ProcessRightButtonClick(mouse);
                    return rv;
                }

                public class MainBuildingsWin : MgmtWindow
                {
                    public MainBuildingsWin(HUDManagement s, Adventurer w = null) : base(s)
                    {
                        foreach (Building b in PlayerManager.Buildings)
                        {
                            bool good = false;

                            if (_parent.Action == ActionTypeEnum.Upgrade) { good = b.Level < GameManager.MaxBldgLevel; }
                            else if (w == null || b.CanHold(w)) { good = true; }

                            if (good)
                            {
                                BuildingBox box = new BuildingBox(b, w != null);
                                _liButtons.Add(box);
                                AddControl(box);
                            }
                        }

                        CreateSpacedGrid(ref _liButtons, _window.InnerTopLeft(), _window.MidWidth(), 3);
                    }

                    public override bool ProcessLeftButtonClick(Point mouse)
                    {
                        bool rv = false;
                        foreach (BuildingBox b in _liButtons)
                        {
                            if (b.Contains(mouse))
                            {
                                if (_parent.Action == ActionTypeEnum.Upgrade)
                                {
                                    b.Building.StartBuilding(false);
                                    GUIManager.CloseMainObject();
                                }
                                else { _parent.HandleBuildingSelection(b.Building); }
                                rv = true;
                                break;
                            }
                        }

                        return rv;
                    }

                    private class BuildingBox : GUIObject
                    {
                        bool _bShowWorkers;
                        GUIButton _btn;
                        GUIText _gText;
                        Building _b;
                        public Building Building => _b;

                        public BuildingBox(Building b, bool showWorkerNum)
                        {
                            _b = b;
                            _btn = new GUIButton(b.GivenName);
                            _bShowWorkers = showWorkerNum;

                            _gText = new GUIText(b.Workers.Count + @"/" + b.MaxWorkers);
                            _gText.AnchorAndAlignToObject(_btn, SideEnum.Bottom, SideEnum.CenterX);

                            AddControl(_btn);
                            AddControl(_gText);
                            Width = _btn.Width > _gText.Width ? _btn.Width : _gText.Width;
                            Height = _btn.Height + _gText.Height;
                        }

                        public override void Draw(SpriteBatch spriteBatch)
                        {
                            _btn.Draw(spriteBatch);
                            if (_bShowWorkers)
                            {
                                _gText.Draw(spriteBatch);
                            }
                        }
                    }
                }
                public class BuildingDetailsWin : MgmtWindow
                {
                    public BuildingDetailsWin(HUDManagement s, Building selectedBuilding) : base(s)
                    {
                        foreach (Adventurer w in selectedBuilding.Workers)
                        {
                            WorkerBox btn = new WorkerBox(w);
                            _liButtons.Add(btn);
                            AddControl(btn);
                        }
                        CreateSpacedGrid(ref _liButtons, _window.InnerTopLeft(), _window.MidWidth(), 3);
                    }

                    public override bool ProcessLeftButtonClick(Point mouse)
                    {
                        bool rv = false;
                        foreach (WorkerBox w in _liButtons)
                        {
                            if (w.Contains(mouse))
                            {
                                if (_parent.Selling())
                                {
                                    GameManager.CurrentAdventurer = w.Worker;
                                    GUIManager.OpenTextWindow("Really sell contract? [Yes:SellContract|No:Cancel]");
                                }
                                else
                                {
                                    _parent.SetMgmtWindow(new WorkerDetailsWin(_parent, w.Worker));
                                }
                                rv = true;
                                break;
                            }
                        }
                        return rv;
                    }
                    public override bool ProcessRightButtonClick(Point mouse)
                    {
                        _parent.SetMgmtWindow(new MainBuildingsWin(_parent));
                        return true;
                    }

                    private class WorkerBox : GUIObject
                    {
                        GUIButton _btn;
                        Adventurer _w;
                        public Adventurer Worker => _w;
                        public WorkerBox(Adventurer w)
                        {
                            _w = w;
                            _btn = new GUIButton(w.Name);
                            AddControl(_btn);

                            Width = _btn.Width;
                            Height = _btn.Height;
                        }
                    }
                }

                public class WorkerDetailsWin : MgmtWindow
                {
                    GUIButton _btnMove;
                    Adventurer _character;
                    GUIText _gName, _actionText, _gClass, _gXP, _gStr, _gDef, _gVit, _gMagic, _gRes, _gSpd;
                    GUIItemBox _weapon, _armor;
                    public WorkerDetailsWin(HUDManagement s, Adventurer selectedAdventurer) : base(s)
                    {
                        int statSpacing = 10;
                        _character = selectedAdventurer;
                        _btnMove = new GUIButton("Move");
                        _btnMove.AnchorToInnerSide(_window, SideEnum.BottomRight);

                        string nameLen = "";
                        for (int i = 0; i < GameManager.MAX_NAME_LEN; i++) { nameLen += "X"; }

                        _gName = new GUIText(nameLen);
                        _gName.AnchorToInnerSide(_window, SideEnum.TopLeft);
                        _gClass = new GUIText("XXXXXXXX 99");
                        _gClass.AnchorAndAlignToObject(_gName, SideEnum.Bottom, SideEnum.Left);

                        _gXP = new GUIText(@"9999/9999");//new GUIText(_character.XP + @"/" + CombatAdventurer.LevelRange[_character.ClassLevel]);
                        _gXP.AnchorAndAlignToObject(_gClass, SideEnum.Right, SideEnum.Top, 10);

                        _gName.SetText(_character.Name);
                        _gClass.SetText(_character.CharacterClass.Name + " " + _character.ClassLevel);
                        _gXP.SetText("Exp:" + _character.XP);

                        _weapon = new GUIItemBox(DataManager.DIALOGUE_TEXTURE, _character.Weapon.GetItem());
                        _weapon.AnchorToInnerSide(_window, SideEnum.TopRight);

                        _armor = new GUIItemBox( DataManager.DIALOGUE_TEXTURE, _character.Armor.GetItem());
                        _armor.AnchorAndAlignToObject(_weapon, SideEnum.Left, SideEnum.Bottom);

                        _gStr = new GUIText("Dmg: 999");
                        _gDef = new GUIText("Def: 999");
                        _gVit = new GUIText("HP: 999");
                        _gMagic = new GUIText("Mag: 999");
                        _gRes = new GUIText("Res: 999");
                        _gSpd = new GUIText("Spd: 999");
                        _gMagic.AnchorAndAlignToObject(_gClass, SideEnum.Bottom, SideEnum.Left);
                        _gDef.AnchorAndAlignToObject(_gMagic, SideEnum.Right, SideEnum.Bottom, statSpacing);
                        _gStr.AnchorAndAlignToObject(_gDef, SideEnum.Right, SideEnum.Bottom, statSpacing);
                        _gVit.AnchorAndAlignToObject(_gStr, SideEnum.Right, SideEnum.Bottom, statSpacing);
                        _gSpd.AnchorAndAlignToObject(_gVit, SideEnum.Right, SideEnum.Bottom, statSpacing);
                        _gRes.AnchorAndAlignToObject(_gSpd, SideEnum.Right, SideEnum.Bottom, statSpacing);

                        _gStr.SetText("Str: " + _character.StatStr);
                        _gDef.SetText("Def: " + _character.StatDef);
                        _gVit.SetText("Vit: " + _character.StatVit);
                        _gMagic.SetText("Mag: " + _character.StatMag);
                        _gRes.SetText("Res: " + _character.StatRes);
                        _gSpd.SetText("Spd: " + _character.StatSpd);


                        _actionText = new GUIText(_character.GetStateText());
                        _actionText.AnchorToInnerSide(_window, SideEnum.BottomLeft);
                    }

                    public override bool ProcessLeftButtonClick(Point mouse)
                    {
                        bool rv = false;
                        if (_btnMove.Contains(mouse))
                        {
                            _parent.HandleMoveWorker(_character);
                            rv = true;
                        }

                        return rv;
                    }
                    public override bool ProcessRightButtonClick(Point mouse)
                    {
                        _parent.SetMgmtWindow(new BuildingDetailsWin(_parent, _character.Building));
                        return true;
                    }
                }
            }
        }
        public class HUDOptions : GUIMainObject
        {
            GUICheck _gAutoDisband;
            GUICheck _gHideMiniInventory;
            GUIButton _btnSave;

            GUIText _gSoundSettings;
            GUINumberControl _gVolumeControl;
            GUINumberControl _gEffectControl;

            public HUDOptions()
            {
                _winMain = SetMainWindow();

                _gAutoDisband = new GUICheck("Auto-Disband", GameManager.AutoDisband);
                _gAutoDisband.AnchorToInnerSide(_winMain, SideEnum.TopLeft, 8);

                _gHideMiniInventory = new GUICheck("Hide Mini Inventory", GameManager.HideMiniInventory);
                _gHideMiniInventory.AnchorAndAlignToObject(_gAutoDisband, SideEnum.Bottom, SideEnum.Left, 8);

                _gSoundSettings = new GUIText("Sound Settings");
                _gSoundSettings.AnchorAndAlignToObject(_gHideMiniInventory, SideEnum.Bottom, SideEnum.Left, 32);

                _gVolumeControl = new GUINumberControl("Music", SoundManager.MusicVolume * 100, UpdateMusicVolume);
                _gVolumeControl.AnchorAndAlignToObject(_gSoundSettings, SideEnum.Bottom, SideEnum.Left);
                _gVolumeControl.MoveBy(new Vector2(32, 0));

                _gEffectControl = new GUINumberControl("Effects", SoundManager.EffectVolume * 100, UpdateEffectsVolume);
                _gEffectControl.AnchorAndAlignToObject(_gVolumeControl, SideEnum.Bottom, SideEnum.Left);

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

            public void UpdateMusicVolume()
            {
                SoundManager.SetMusicVolume((float)_gVolumeControl.Value/100.0f);
            }

            public void UpdateEffectsVolume()
            {
                SoundManager.SetEffectVolume((float)_gEffectControl.Value / 100.0f);
            }
            public void BtnSave()
            {
                GameManager.AutoDisband = _gAutoDisband.Checked();
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

                private void UpdateValue()
                {
                    _gValue.SetText((int)_fValue, false);
                    _gValue.AnchorAndAlignToObject(_btnRight, SideEnum.Left, SideEnum.CenterY, 12);
                }
            }
        }

        public class HUDNamingWindow : GUIMainObject
        {
            GUITextInputWindow _gInputWindow;
            Building _bldg;
            Adventurer _adv;

            /// <summary>
            /// Never called outwardly, only for private use
            /// </summary>
            private HUDNamingWindow()
            {
                _gInputWindow = new GUITextInputWindow();
                _gInputWindow.SetupNaming();
                _gInputWindow.Activate();

                Width = _gInputWindow.Width;
                Height = _gInputWindow.Height;
                AddControl(_gInputWindow);
            }

            /// <summary>
            /// Constructor to name a WorldAdventurer
            /// </summary>
            /// <param name="w">WorldAdventurer to name</param>
            public HUDNamingWindow(Adventurer w) : this()
            {
                _adv = w;
            }

            /// <summary>
            /// Constructor to name a Building.
            /// 
            /// Buildings are allowed to have spaces in their names.
            /// </summary>
            /// <param name="b">Building to name</param>
            public HUDNamingWindow(Building b) : this()
            {
                _bldg = b;
                _gInputWindow.AcceptSpace = true;
            }

            /// <summary>
            /// Update function for the window.
            /// 
            /// Only setthe name of the component when it is finished taking input
            /// </summary>
            /// <param name="gTime"></param>
            public override void Update(GameTime gTime)
            {
                base.Update(gTime);
                if (_gInputWindow.Finished)
                {
                    if (_adv != null)
                    {
                        _adv.SetName(_gInputWindow.EnteredText);
                    }
                    if (_bldg != null)
                    {
                        _bldg.SetName(_gInputWindow.EnteredText);
                    }

                    //We know that this window only gets created under special circumstances, so unset them
                    RiverHollow.ResetCamera();
                    GUIManager.CloseMainObject();
                    GameManager.Unpause();
                    GameManager.Scry(false);
                    GameManager.StopTakingInput();
                }
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                return false;
            }
        }
    }

    public class HUDCalendar : GUIWindow
    {
        static GUIText _gText;
        public HUDCalendar() : base(GUIWindow.BrownWin, ScaledTileSize, ScaledTileSize)
        {
            _gText = new GUIText("Day XX, XX:XX", DataManager.GetBitMapFont(DataManager.FONT_MAIN));

            _gText.AnchorToInnerSide(this, SideEnum.TopLeft);
            Resize();
        }

        public override void Update(GameTime gTime)
        {
            _gText.SetText(GameCalendar.GetCalendarString());
        }
    }

    class HUDMissionWindow : GUIMainObject
    {
        public static int MAX_SHOWN_MISSIONS = 4;

        int _iTopMission = 0;

        GUIButton _btnUp;
        GUIButton _btnDown;
        GUIWindow _gWin;
        DetailWindow _gDetailWindow;
        WorkerWindow _gWinWorkers;

        List<MissionBox> _liMissions;

        public HUDMissionWindow()
        {
            GameManager.Pause();
            _gWin = new GUIWindow(GUIWindow.RedWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
            _liMissions = new List<MissionBox>();

            AddControl(_gWin);

            AssignMissionWindows();

            Width = _gWin.Width;
            Height = _gWin.Height;

            CenterOnScreen();

            if (MissionManager.AvailableMissions.Count > MAX_SHOWN_MISSIONS)
            {
                _btnUp = new GUIButton(new Rectangle(272, 96, 16, 16), GUIManager.MINI_BTN_HEIGHT, GUIManager.MINI_BTN_HEIGHT, DataManager.DIALOGUE_TEXTURE, BtnUpClick);
                _btnDown = new GUIButton(new Rectangle(256, 96, 16, 16), GUIManager.MINI_BTN_HEIGHT, GUIManager.MINI_BTN_HEIGHT, DataManager.DIALOGUE_TEXTURE, BtnDownClick);

                _btnUp.AnchorAndAlignToObject(_gWin, GUIObject.SideEnum.Right, GUIObject.SideEnum.Top);
                _btnDown.AnchorAndAlignToObject(_gWin, GUIObject.SideEnum.Right, GUIObject.SideEnum.Bottom);

                _btnUp.Enable(false);
                _btnDown.Enable(false);

                RemoveControl(_btnDown);
                RemoveControl(_btnUp);
                _gWin.AddControl(_btnDown);
                _gWin.AddControl(_btnUp);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (_gWinWorkers != null)
            {
                rv = _gWinWorkers.ProcessLeftButtonClick(mouse);
            }
            else if (_gDetailWindow != null)
            {
                rv = _gDetailWindow.ProcessLeftButtonClick(mouse);
            }
            else
            {
                if (_btnDown != null && _btnDown.ProcessLeftButtonClick(mouse))
                {
                    rv = true;
                }
                else if (_btnUp != null && _btnUp.ProcessLeftButtonClick(mouse))
                {
                    rv = true;
                }
                else
                {
                    foreach (MissionBox box in _liMissions)
                    {
                        if (box.ProcessLeftButtonClick(mouse))
                        {
                            rv = true;
                            break;
                        }
                    }
                }
            }

            return rv;
        }
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if (_gDetailWindow != null)
            {
                rv = _gDetailWindow.ProcessRightButtonClick(mouse);
                if (!rv)
                {
                    RemoveControl(_gDetailWindow);
                    _gDetailWindow = null;
                    AddControl(_gWin);

                    //set it true here so that higher up calls know that it has been handled
                    rv = true;
                }
            }
            else
            {
                rv = base.ProcessRightButtonClick(mouse);
                if (!rv)
                {
                    MissionManager.ClearMissionAcceptance();
                }
            }

            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;

            if (_gWin.Contains(mouse))
            {
                GUICursor.Alpha = 1;
            }

            return rv;
        }

        /// <summary>
        /// Clears and assigns the list of mission boxes based off of the top mission
        /// and the total number of available missions
        /// </summary>
        public void AssignMissionWindows()
        {
            _liMissions.Clear();
            for (int i = _iTopMission; i < _iTopMission + MAX_SHOWN_MISSIONS && i < MissionManager.AvailableMissions.Count; i++)
            {
                MissionBox q = new MissionBox(MissionManager.AvailableMissions[i], OpenDetailBox, _gWin.MidWidth(), _gWin.MidHeight() / MAX_SHOWN_MISSIONS);

                if (_liMissions.Count == 0) { q.AnchorToInnerSide(_gWin, SideEnum.TopLeft); }
                else { q.AnchorAndAlignToObject(_liMissions[_liMissions.Count() - 1], SideEnum.Bottom, SideEnum.Left); }

                _gWin.AddControl(q);

                _liMissions.Add(q);
            }
        }

        /// <summary>
        /// Opens a new window for advanced information on the indicated Mission.
        /// Inform the Mission Manager which Mission we're looking at, so it can better
        /// handle the information.
        /// </summary>
        /// <param name="m">The selected Mission</param>
        public void OpenDetailBox(Mission m)
        {
            MissionManager.SelectMission(m);
            _gDetailWindow = new DetailWindow(m, OpenWorkerWindow, AcceptMission);
            AddControl(_gDetailWindow);
            RemoveControl(_gWin);
        }

        /// <summary>
        /// Opens the WorkerWindow so that we can select from among the workers
        /// the player has.
        /// 
        /// We remove the Detail window so it won't be drawn while the WorkerWindow is open.
        /// </summary>
        public void OpenWorkerWindow()
        {
            _gWinWorkers = new WorkerWindow(WorkerAssigned);
            _gWinWorkers.CenterOnScreen();
            AddControl(_gWinWorkers);
            RemoveControl(_gDetailWindow);
        }

        /// <summary>
        /// Delegate method for the WorkerWindow to handle when a worker
        /// has been selected from it.
        /// 
        /// Close the WorkerWindow, and add the selected worker to the DetailWindow.
        /// Null out the WorkerWindow, and then check to see if we have enough workers
        /// to enable the Accept button.
        /// </summary>
        /// <param name="adv"></param>
        public void WorkerAssigned(Adventurer adv)
        {
            AddControl(_gDetailWindow);
            RemoveControl(_gWinWorkers);
            _gDetailWindow.AssignToBox(adv);
            _gWinWorkers = null;

            if (MissionManager.MissionReady())
            {
                _gDetailWindow.EnableAccept();
            }
        }

        /// <summary>
        /// Delegate method called by the DetailWindow to accept the mission and return 
        /// the user to the main screen.
        /// </summary>
        public void AcceptMission()
        {
            MissionManager.AcceptMission();
            GUIManager.CloseMainObject();
        }

        /// <summary>
        /// Moves the topmost displayed mission up one and reassigns the mission boxes
        /// </summary>
        public void BtnUpClick()
        {
            if (_iTopMission - 1 >= 0)
            {
                _iTopMission--;

                AssignMissionWindows();
            }
        }

        /// <summary>
        /// Moves the topmost displayed mission up one and reassigns the mission boxes
        /// </summary>
        public void BtnDownClick()
        {
            if (_iTopMission + MAX_SHOWN_MISSIONS < MissionManager.AvailableMissions.Count)
            {
                _iTopMission++;
            }
            AssignMissionWindows();
        }

        /// <summary>
        /// Displays the short form details of a mission.
        /// </summary>
        public class MissionBox : GUIWindow
        {
            Mission _mission;
            GUIText _gName;
            GUIMoneyDisplay _gMoney;

            List<GUIItemBox> _liItems;

            public delegate void BoxClickDelegate(Mission m);
            private BoxClickDelegate _delAction;

            public MissionBox(Mission m, BoxClickDelegate action, int width, int height) : base(GUIWindow.RedWin, width, height)
            {
                _mission = m;
                _delAction = action;

                _gName = new GUIText(m.Name);
                _liItems = new List<GUIItemBox>();
                _gMoney = new GUIMoneyDisplay(m.Money);

                _gName.AnchorToInnerSide(this, SideEnum.TopLeft);
                _gMoney.AnchorToInnerSide(this, SideEnum.Right);
                _gMoney.AlignToObject(this, SideEnum.CenterY);

                for (int i = 0; i < m.Items.Count(); i++)
                {
                    GUIItemBox box = new GUIItemBox(m.Items[i]);

                    if (i == 0) { box.AnchorAndAlignToObject(_gMoney, SideEnum.Left, SideEnum.CenterY); }
                    else { box.AnchorAndAlignToObject(_liItems[i - 1], SideEnum.Left, SideEnum.Bottom); }

                    _liItems.Add(box);
                    AddControl(box);
                }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                if (this.Contains(mouse))
                {
                    rv = true;
                    _delAction(_mission);
                }

                return rv;
            }
        }

        /// <summary>
        /// Displays the actual details of the selected mission
        /// </summary>
        public class DetailWindow : GUIWindow
        {
            GUIButton _btnAccept;

            GUIText _gName;
            GUIText _gClass;
            GUIText _gDaysToFinish;
            GUIText _gDaysUntilExpiry;
            GUIText _gReqLevel;

            GUIMoneyDisplay _gMoney;

            List<GUIItemBox> _liItems;

            List<CharacterDisplayBox> _liParty;

            CharacterDisplayBox _selected;

            public delegate void BoxClickDelegate();
            private BoxClickDelegate _delOpen;

            public DetailWindow(Mission m, BoxClickDelegate open, BtnClickDelegate accept) : base(GUIWindow.RedWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT)
            {
                _liItems = new List<GUIItemBox>();
                _liParty = new List<CharacterDisplayBox>();

                _delOpen = open;

                _gName = new GUIText(m.Name);
                _gDaysToFinish = new GUIText("Requires " + m.DaysToComplete + " days");
                _gDaysUntilExpiry = new GUIText("Expires in " + (m.TotalDaysToExpire - m.DaysExpired) + " days");
                _gReqLevel = new GUIText("Required Level: " + m.ReqLevel);

                _gMoney = new GUIMoneyDisplay(m.Money);

                _gName.AnchorToInnerSide(this, SideEnum.TopLeft);
                _gReqLevel.AnchorAndAlignToObject(_gName, SideEnum.Bottom, SideEnum.Left);
                _gDaysToFinish.AnchorAndAlignToObject(_gReqLevel, SideEnum.Bottom, SideEnum.Left);

                if (m.CharClass != null)
                {
                    _gClass = new GUIText("Requires " + m.CharClass.Name);
                    _gClass.AnchorAndAlignToObject(_gDaysToFinish, SideEnum.Bottom, SideEnum.Left);
                }

                _gDaysUntilExpiry.AnchorToInnerSide(this, SideEnum.BottomLeft);
                _gMoney.AnchorToInnerSide(this, SideEnum.BottomRight);

                //Adds the GUIItemBoxes to display Mission rewards
                for (int i = 0; i < m.Items.Count(); i++)
                {
                    GUIItemBox box = new GUIItemBox(m.Items[i]);

                    if (i == 0) { box.AnchorAndAlignToObject(_gMoney, SideEnum.Left, SideEnum.Bottom); }
                    else { box.AnchorAndAlignToObject(_liItems[i - 1], SideEnum.Left, SideEnum.Bottom); }

                    _liItems.Add(box);
                    AddControl(box);
                }

                //Adds the CharacterDisplayBox to display assigned Adventurers
                for (int i = 0; i < m.PartySize; i++)
                {
                    CharacterDisplayBox box = new CharacterDisplayBox(null, null);
                    _liParty.Add(box);

                    if (i == 0) { box.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.Right); }
                    else { box.AnchorAndAlignToObject(_liParty[i - 1], SideEnum.Left, SideEnum.Top); }

                    AddControl(box);
                }

                _btnAccept = new GUIButton("Accept", accept);
                _btnAccept.AnchorAndAlignToObject(this, SideEnum.Right, SideEnum.Bottom);
                _btnAccept.Enable(false);
                AddControl(_btnAccept);

                CenterOnScreen();
            }

            /// <summary>
            /// If a CharacterDisplayBox was clicked, call the delegate to open up
            /// the worker select window. Otherwise, try to see if the button was clicked.
            /// </summary>
            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                foreach (CharacterDisplayBox box in _liParty)
                {
                    if (box.Contains(mouse))
                    {
                        _selected = box;
                        if (_selected.Actor != null)
                        {
                            MissionManager.RemoveFromParty(_selected.WorldAdv);
                            _selected.AssignToBox(null);
                        }
                        _delOpen();
                        rv = true;
                        break;
                    }
                }

                //If we didn't click on a character box, checkif we clicked the button.
                if (!rv)
                {
                    rv = _btnAccept.ProcessLeftButtonClick(mouse);
                }

                return rv;
            }

            /// <summary>
            /// Right clicking on a CharacterDisplayBox will remove the WorldAdventurer
            /// from the party.
            /// </summary>
            /// <param name="mouse"></param>
            /// <returns></returns>
            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = false;

                foreach (CharacterDisplayBox box in _liParty)
                {
                    if (box.Contains(mouse))
                    {
                        rv = true;
                        MissionManager.RemoveFromParty(box.WorldAdv);
                        box.AssignToBox(null);
                        break;
                    }
                }

                if (!rv)
                {
                    MissionManager.ClearMissionAcceptance();
                }

                return rv;
            }

            /// <summary>
            /// Assign the indicated WorldAdventurer to the CharacterDisplayBox
            /// </summary>
            public void AssignToBox(Adventurer adv)
            {
                _selected.AssignToBox(adv);
                _selected = null;
            }

            /// <summary>
            /// Enables the Accept button
            /// </summary>
            public void EnableAccept()
            {
                _btnAccept.Enable(true);
            }

        }

        /// <summary>
        /// Displays all workers among all buildings.
        /// </summary>
        public class WorkerWindow : GUIWindow
        {
            List<CharacterDisplayBox> _liWorkers;

            //Delegate method for when it's time to close this window.
            public delegate void BoxClickDelegate(Adventurer adv);
            private BoxClickDelegate _delClose;

            /// <summary>
            /// Constructs a new Worker window by iterating through all the buildings and workers, and adding
            /// them to the list of workers. Workers that are already Adventuring cannot appear.
            /// </summary>
            /// <param name="delClose">Delegate method for the Screen to know what to do when we're done here.</param>
            public WorkerWindow(BoxClickDelegate delClose) : base(GUIWindow.BrownWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT)
            {
                _delClose = delClose;
                _liWorkers = new List<CharacterDisplayBox>();

                //Find all the relevant workers and create a CharacterDisplayBox for them
                foreach (Building b in PlayerManager.Buildings)
                {
                    foreach (Adventurer adv in b.Workers)
                    {
                        if (adv.AvailableForMissions() && adv.ClassLevel >= MissionManager.SelectedMission.ReqLevel)
                        {
                            CharacterDisplayBox box = new CharacterDisplayBox(adv, null);
                            box.WorldAdv = adv;
                            _liWorkers.Add(box);
                        }
                    }
                }

                //Organize all the CharacterDisplayBoxes.
                for (int i = 0; i < _liWorkers.Count(); i++)
                {
                    if (i == 0) { _liWorkers[i].AnchorToInnerSide(this, SideEnum.TopLeft); }
                    else { _liWorkers[i].AnchorAndAlignToObject(_liWorkers[i - 1], SideEnum.Right, SideEnum.Bottom); }

                    AddControl(_liWorkers[i]);
                }
            }

            /// <summary>
            /// When we select one of the adventurers, add it to the MissionManager party
            /// and then inform the MissionScreen that we're ready to close.
            /// </summary>
            /// <param name="mouse"></param>
            /// <returns></returns>
            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                foreach (CharacterDisplayBox box in _liWorkers)
                {
                    if (box.Contains(mouse))
                    {
                        rv = true;
                        MissionManager.AddToParty(box.WorldAdv);
                        _delClose(box.WorldAdv);
                        break;
                    }
                }

                return rv;
            }
        }
    }

    class HUDNewQuest : GUIObject
    {
        double _dTimer = 0;
        GUIImage _gMarker;
        GUIText _gText;

        public delegate void RemoveDelegate(HUDNewQuest q);
        private RemoveDelegate _delAction;
        public HUDNewQuest(bool questComplete, RemoveDelegate del)
        {
            _delAction = del;
            _gMarker = new GUIImage(new Rectangle(48, 80, 16, 16), ScaleIt(16), ScaleIt(16), DataManager.DIALOGUE_TEXTURE);
            _gText = new GUIText(questComplete ? "Quest Complete" : "New Quest");

            _gText.AnchorAndAlignToObject(_gMarker, SideEnum.Right, SideEnum.CenterY, 4);
            AddControl(_gMarker);
            AddControl(_gText);

            Width = _gText.Right - _gMarker.Left;
            Height = _gMarker.Height;
        }

        public override void Update(GameTime gTime)
        {
            _dTimer += gTime.ElapsedGameTime.TotalSeconds;

            if(_dTimer > 3) { _delAction(this); }
            else {
                MoveBy(new Vector2(0, -1));
                Alpha(Alpha()-0.01f);
            }
        }
    }
}
