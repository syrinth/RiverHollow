using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.GUIObjects.GUIObject;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using System.Collections.Generic;
using RiverHollow.GUIObjects;
using RiverHollow.Actors;
using RiverHollow.Screens;
using RiverHollow.Misc;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using static RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIItemBox;
using RiverHollow.WorldObjects;
using static RiverHollow.WorldObjects.Item;
using static RiverHollow.WorldObjects.Clothes;
using RiverHollow.Buildings;
using static RiverHollow.Game_Managers.GUIObjects.HUDMenu.HUDManagement.MgmtWindow;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using static RiverHollow.GUIComponents.GUIObjects.NPCDisplayBox;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class HUDScreen : GUIScreen
    {
        GUIObject _gMenu;
        GUIObject _gMainObject;
        GUIStatDisplay _gHealthDisplay;
        GUIStatDisplay _gStaminaDisplay;
        GUIMoneyDisplay _gMoney;

        HUDInventory _gInventory;
        GUIItemBox _addedItem;
        double _dTimer;

        public HUDScreen()
        {
            _gHealthDisplay = new GUIStatDisplay(PlayerManager.Combat.GetHP, Color.Green);
            _gHealthDisplay.AnchorToScreen(this, SideEnum.TopLeft, 10);
            AddControl(_gHealthDisplay);
            _gStaminaDisplay = new GUIStatDisplay(PlayerManager.GetStamina, Color.Red);
            _gStaminaDisplay.AnchorAndAlignToObject(_gHealthDisplay, SideEnum.Bottom, SideEnum.Left);
            AddControl(_gStaminaDisplay);

            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorAndAlignToObject(_gStaminaDisplay, SideEnum.Bottom, SideEnum.Left);
            AddControl(_gMoney);

            _gInventory = new HUDInventory();
            _gInventory.AnchorToScreen(SideEnum.Bottom);
            AddControl(_gInventory);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (InventoryManager.AddedItem != null && _addedItem == null)
            {
                _addedItem = new GUIItemBox(InventoryManager.AddedItem);
                _addedItem.AnchorAndAlignToObject(_gInventory, SideEnum.Left, SideEnum.CenterY, 10);
                _dTimer = 1;
            }
            else
            {
                if (_addedItem != null && _addedItem.Alpha > 0)
                {
                    _dTimer -= gameTime.ElapsedGameTime.TotalSeconds;
                    _addedItem.SetAlpha((float)_dTimer);
                }
                else if (_addedItem != null)
                {
                    _addedItem = null;
                    InventoryManager.AddedItem = null;
                }
            }
        }

        public override void Sync()
        {
            _gInventory.SyncItems();
        }

        public override bool IsHUD() { return true; }

        /// <summary>
        /// Overrides the Screens OpenTextWindow method to first hide any HUD components desired.
        /// </summary>
        /// <param name="text">Text to open with</param>
        /// <param name="open">Whether to play the open animation</param>
        public override void OpenTextWindow(string text, bool open = true)
        {
            base.OpenTextWindow(text, open);
            _gInventory.Show = false;
        }
        public override bool CloseTextWindow(GUITextWindow win)
        {
            bool rv = base.CloseTextWindow(win);
            _gInventory.Show = true;
            return rv;
        }

        public override void OpenMenu()
        {
            _gMenu = new HUDMenu();
            AddControl(_gMenu);
        }
        public override void CloseMenu()
        {
            RemoveControl(_gMenu);
            _gMenu = null;
        }
        public override bool IsMenuOpen() { return _gMenu != null; }

        public override void OpenMainObject(GUIObject o)
        {
            RemoveControl(_gMainObject);
            _gMainObject = o;
            AddControl(_gMainObject);
        }
        public override void CloseMainObject(GUIObject o)
        {
            if (_gMainObject == o)
            {
                RemoveControl(_gMainObject);
                _gMainObject = null;
            }
        }
    }

    public class HUDInventory : GUIWindow
    {
        List<GUIItemBox> _liItems;
        GUIButton _btnChangeRow;

        bool _bFadeOutBar = true;
        bool _bFadeItemsOut;
        bool _bFadeItemsIn;
        float _fBarFade;
        float _fItemFade = 1.0f;
        float FADE_OUT = 0.1f;

        public HUDInventory() : base(GUIWindow.BrownWin, TileSize, TileSize)
        {
            _btnChangeRow = new GUIButton(new Rectangle(256, 96, 32, 32), 64, 64, @"Textures\Dialog", RowUp);
            _liItems = new List<GUIItemBox>();
            _fBarFade = GameManager.HideMiniInventory ? FADE_OUT : 1.0f;
            Alpha = _fBarFade;
            for (int i = 0; i < InventoryManager.maxItemColumns; i++)
            {
                GUIItemBox ib = new GUIItemBox(InventoryManager.PlayerInventory[GameManager.HUDItemRow, i]);
                _liItems.Add(ib);

                if (i == 0) { ib.AnchorToInnerSide(this, SideEnum.TopLeft); }
                else { ib.AnchorAndAlignToObject(_liItems[i - 1], SideEnum.Right, SideEnum.Bottom); }

                ib.SetAlpha(_fBarFade);
            }

            _liItems[GameManager.HUDItemCol].Select(true);
            Resize();

            _btnChangeRow.AnchorAndAlignToObject(this, SideEnum.Right, SideEnum.CenterY);
            AddControl(_btnChangeRow);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
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

            if (startFade != _fBarFade)
            {
                Alpha = _fBarFade;

                foreach (GUIItemBox gib in _liItems)
                {
                    gib.SetAlpha(Alpha);
                }
                _btnChangeRow.Alpha = Alpha;
            }
        }

        public override bool Contains(Point mouse)
        {
            return base.Contains(mouse) || _btnChangeRow.Contains(mouse);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (Contains(mouse))
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

            if (Contains(mouse))
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

            if (Contains(mouse) && Alpha != 1)
            {
                rv = true;
                _bFadeOutBar = false;
            }
            else if (!Contains(mouse) && GameManager.HideMiniInventory && Alpha != 0.1f)
            {
                _bFadeOutBar = true;
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
        GUIObject _gMenuObject;
        List<GUIObject> _liButtons;

        bool _open = false;
        bool _close = false;

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

            GameManager.Pause();
            _open = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            int _openingFinished = 0;
            foreach (GUIObject o in Controls)
            {
                int val = 0;
                if (_open)
                {
                    if (o.Position().X < 0) { val = 16; }
                }
                if (_close)
                {
                    if (o.Position().X > -GUIButton.BTN_WIDTH) { val = -16; }
                }

                Vector2 temp = o.Position();
                temp.X += val;
                o.Position(temp);
                if (_open && o.Position().X == 0) { _openingFinished++; }
                if (_close && o.Position().X == -GUIButton.BTN_WIDTH) { GameManager.BackToMain(); }
            }
            if (_openingFinished == _liButtons.Count) { _open = false; }
        }

        #region Buttons
        public void BtnExitGame()
        {
            RiverHollow.PrepExit();
        }
        public void BtnInventory()
        {
            _gMenuObject = new GUIInventory(true);
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

        public class HUDQuestLog : GUIWindow
        {
            public static int BTNSIZE = 32;
            public static int MAX_SHOWN_QUESTS = 4;
            List<QuestBox> _questList;
            DetailBox _detailWindow;
            GUIButton _btnUp;
            GUIButton _btnDown;

            bool _bMoved;
            int _topQuest;
            public HUDQuestLog() : base(GUIWindow.RedWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT)
            {
                this.CenterOnScreen();
                _questList = new List<QuestBox>();
                _detailWindow = new DetailBox(GUIWindow.RedWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
                _detailWindow.CenterOnScreen();

                _btnUp = new GUIButton(new Rectangle(256, 64, 32, 32), BTNSIZE, BTNSIZE, @"Textures\Dialog", BtnUpClick);
                _btnDown = new GUIButton(new Rectangle(256, 96, 32, 32), BTNSIZE, BTNSIZE, @"Textures\Dialog", BtnDownClick);

                _btnUp.AnchorAndAlignToObject(this, GUIObject.SideEnum.Right, GUIObject.SideEnum.Top);
                _btnDown.AnchorAndAlignToObject(this, GUIObject.SideEnum.Right, GUIObject.SideEnum.Bottom);
                _topQuest = 0;

                for (int i = 0; i < MAX_SHOWN_QUESTS && i < PlayerManager.QuestLog.Count; i++)
                {
                    QuestBox q = new QuestBox(this, i);
                    q.SetQuest(PlayerManager.QuestLog[_topQuest + i]);
                    _questList.Add(q);
                }

                AddControl(_btnUp);
                AddControl(_btnDown);
                foreach (QuestBox q in _questList)
                {
                    AddControl(q);
                }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;
                _bMoved = false;
                if (!Controls.Contains(_detailWindow))
                {
                    foreach (GUIObject c in Controls)
                    {
                        rv = c.ProcessLeftButtonClick(mouse);

                        if (rv) { break; }
                    }

                    if (_bMoved)
                    {
                        for (int i = 0; i < _questList.Count; i++)
                        {
                            _questList[i].SetQuest(PlayerManager.QuestLog[_topQuest + i]);
                        }
                    }

                    foreach (QuestBox c in _questList)
                    {
                        if (c.Contains(mouse))
                        {
                            _detailWindow.SetData(c.TheQuest);
                            AddControl(_detailWindow);
                            RemoveControl(_btnUp);
                            RemoveControl(_btnDown);

                            rv = true;
                        }
                        if (rv) { break; }
                    }
                }
                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = true;
                if (Controls.Contains(_detailWindow))
                {
                    RemoveControl(_detailWindow);
                    AddControl(_btnUp);
                    AddControl(_btnDown);
                }
                return rv;
            }

            public void BtnUpClick()
            {
                if (_topQuest - 1 >= 0) { _topQuest--; _bMoved = true; }
            }
            public void BtnDownClick()
            {
                if (_topQuest + MAX_SHOWN_QUESTS < PlayerManager.QuestLog.Count) { _topQuest++; _bMoved = true; }
            }

            public override bool ProcessHover(Point mouse)
            {
                bool rv = true;
                foreach (QuestBox c in _questList)
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

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
            }

            public class QuestBox : GUIObject
            {
                GUIWindow _window;
                Quest _quest;
                public Quest TheQuest => _quest;
                SpriteFont _font;
                private int _index;
                public int Index { get => _index; }
                public bool ClearThis;


                public QuestBox(GUIWindow win, int i)
                {
                    _index = i;

                    int boxHeight = (GUIManager.MAIN_COMPONENT_HEIGHT / HUDQuestLog.MAX_SHOWN_QUESTS) - (win.EdgeSize * 2);
                    int boxWidth = (GUIManager.MAIN_COMPONENT_WIDTH) - (win.EdgeSize * 2) - HUDQuestLog.BTNSIZE;
                    _window = new GUIWindow(GUIWindow.RedWin, boxWidth, boxHeight);
                    _window.AnchorToInnerSide(win, SideEnum.TopLeft);

                    _font = GameContentManager.GetFont(@"Fonts\Font");
                    _quest = null;
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    if (_quest != null)
                    {
                        _window.Draw(spriteBatch);
                        spriteBatch.DrawString(_font, _quest.Name, _window.InnerRecVec(), Color.White);
                        spriteBatch.DrawString(_font, _quest.Accomplished + @"/" + _quest.TargetGoal, _window.InnerRecVec() + new Vector2(200, 0), Color.White);
                    }
                }

                public void SetQuest(Quest q)
                {
                    _quest = q;
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
        public class HUDParty : GUIObject
        {
            CharacterDetailObject _charBox;
            NPCDisplayBox _selectedBox;
            GUIButton _btnMap;
            PositionMap _map;

            NPCDisplayBox[] _arrDisplayBoxes;

            public HUDParty()
            {
                _btnMap = new GUIButton("Map", SwitchModes);
                AddControl(_btnMap);

                _charBox = new CharacterDetailObject(PlayerManager.Combat, SyncCharacter);
                _charBox.CenterOnScreen();
                AddControl(_charBox);

                _btnMap.AnchorAndAlignToObject(_charBox.WinDisplay, SideEnum.Right, SideEnum.Top);

                int partySize = PlayerManager.GetParty().Count;
                _arrDisplayBoxes = new NPCDisplayBox[partySize];

                for (int i = 0; i < partySize; i++)
                {
                    if (PlayerManager.GetParty()[i] == PlayerManager.Combat)
                    {
                        _arrDisplayBoxes[i] = new PlayerDisplayBox(true, ChangeSelectedCharacter);
                    }
                    else
                    {
                        CombatAdventurer c = PlayerManager.GetParty()[i];
                        if (c.World != null)
                        {
                            _arrDisplayBoxes[i] = new CharacterDisplayBox(c.World, ChangeSelectedCharacter);
                        }
                    }

                    _arrDisplayBoxes[i].Enable(false);
                    AddControl(_arrDisplayBoxes[i]);

                    if (i == 0)
                    {
                        _arrDisplayBoxes[i].AnchorAndAlignToObject(_charBox, SideEnum.Top, SideEnum.Left);
                    }
                    else
                    {
                        _arrDisplayBoxes[i].AnchorAndAlignToObject(_arrDisplayBoxes[i - 1], SideEnum.Right, SideEnum.Bottom);
                    }

                }
                _selectedBox = _arrDisplayBoxes[0];
                _selectedBox.Enable(true);

                Width = _btnMap.Right - _charBox.Left;
                Height = _charBox.Bottom - _arrDisplayBoxes[0].Top;
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

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
            }

            public void UpdateCharacterBox(CombatAdventurer displayCharacter)
            {
                _charBox.SetAdventurer(displayCharacter);
            }

            public void ChangeSelectedCharacter(CombatAdventurer selectedCharacter)
            {
                _selectedBox.Enable(false);
                if (_charBox != null)
                {
                    _charBox.SetAdventurer(selectedCharacter);
                }
                else if (_map != null)
                {
                    _map.SetOccupancy(selectedCharacter);
                }

                foreach (NPCDisplayBox box in _arrDisplayBoxes)
                {
                    if (box.Actor == selectedCharacter)
                    {
                        _selectedBox = box;
                        break;
                    }
                }

                _selectedBox.Enable(true);
            }

            public void SwitchModes()
            {
                if (_charBox != null)
                {
                    RemoveControl(_charBox);
                    _charBox = null;
                    _map = new PositionMap(_selectedBox.Actor, ChangeSelectedCharacter);
                    _map.AnchorAndAlignToObject(_arrDisplayBoxes[0], SideEnum.Bottom, SideEnum.Left);
                    AddControl(_map);
                }
                else if (_map != null)
                {
                    RemoveControl(_map);
                    _map = null;
                    _charBox = new CharacterDetailObject(_selectedBox.Actor, SyncCharacter);
                    _charBox.AnchorAndAlignToObject(_arrDisplayBoxes[0], SideEnum.Bottom, SideEnum.Left);
                    AddControl(_charBox);
                }
            }

            public void SyncCharacter()
            {
                ((PlayerDisplayBox)_arrDisplayBoxes[0]).Configure();
            }

            private class PositionMap : GUIWindow
            {
                CombatAdventurer _currentCharacter;
                StartPosition _currPosition;
                StartPosition[,] _arrStartPositions;

                public delegate void ClickDelegate(CombatAdventurer selectedCharacter);
                private ClickDelegate _delAction;

                public PositionMap(CombatAdventurer adv, ClickDelegate del) : base(BrownWin, 16, 16)
                {
                    _delAction = del;
                    _currentCharacter = adv;

                    int maxCols = 4;
                    int maxRows = 3;

                    int spacing = 10;
                    int totalSpaceCol = (maxCols + 1) * spacing;
                    int totalSpaceRow = (maxRows + 1) * spacing;
                    _arrStartPositions = new StartPosition[maxCols, maxRows];
                    for (int cols = 0; cols < maxCols; cols++)
                    {
                        for (int rows = 0; rows < maxRows; rows++)
                        {
                            StartPosition pos = new StartPosition(cols, rows);
                            _arrStartPositions[cols, rows] = pos;
                            if (cols == 0 && rows == 0)
                            {
                                pos.AnchorToInnerSide(this, SideEnum.TopLeft, spacing);
                            }
                            else if (cols == 0)
                            {
                                pos.AnchorAndAlignToObject(_arrStartPositions[0, rows - 1], SideEnum.Bottom, SideEnum.Left, spacing);
                            }
                            else
                            {
                                pos.AnchorAndAlignToObject(_arrStartPositions[cols - 1, rows], SideEnum.Right, SideEnum.Bottom, spacing);
                            }
                        }
                    }

                    SetOccupancy(_currentCharacter);

                    this.Resize();
                    this.IncreaseSizeTo((GUIManager.MAIN_COMPONENT_WIDTH) - (BrownWin.Edge * 2), (GUIManager.MAIN_COMPONENT_HEIGHT) - (BrownWin.Edge * 2));
                }

                public void SetOccupancy(CombatAdventurer currentCharacter)
                {
                    _currentCharacter = currentCharacter;
                    foreach (CombatAdventurer c in PlayerManager.GetParty())
                    {
                        Vector2 vec = c.StartPos;
                        bool current = (c == currentCharacter);
                        _arrStartPositions[(int)vec.X, (int)vec.Y].SetCharacter(c);
                        if (current)
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
                        if (sp.Contains(mouse))
                        {
                            if (!sp.Occupied())
                            {
                                rv = true;
                                _currPosition.SetCharacter(null);
                                _currPosition = sp;
                                _currPosition.SetCharacter(_currentCharacter);
                                _currentCharacter.SetStartPosition(new Vector2(_currPosition.Col, _currPosition.Row));
                            }
                            else
                            {
                                _currentCharacter = sp.Character;
                                _delAction(_currentCharacter);
                            }

                            break;
                        }
                    }

                    return rv;
                }

                private class StartPosition : GUIImage
                {
                    CombatAdventurer _character;
                    public CombatAdventurer Character => _character;
                    int _iCol;
                    int _iRow;
                    public int Col => _iCol;
                    public int Row => _iRow;

                    private GUICharacterSprite _sprite;

                    public StartPosition(int col, int row) : base(new Rectangle(0, 80, 16, 16), TileSize, TileSize, @"Textures\Dialog")
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

                    public void SetCharacter(CombatAdventurer c)
                    {
                        _character = c;
                        if (c != null)
                        {
                            if (c.World != null)
                            {
                                if (c == PlayerManager.Combat) { _sprite = new GUICharacterSprite(true); }
                                else { _sprite = new GUICharacterSprite(c.World.BodySprite, true); }

                                _sprite.SetScale(2);
                                _sprite.PlayAnimation(WActorBaseAnim.IdleDown);
                                _sprite.CenterOnObject(this);
                                _sprite.MoveBy(new Vector2(0, -(this.Width / 4)));
                                AddControl(_sprite);
                            }
                        }
                        else
                        {
                            RemoveControl(_sprite);
                            _sprite = null;
                        }
                    }

                    public bool Occupied() { return _character != null; }
                }
            }

            public class CharacterDetailObject : GUIObject
            {
                const int SPACING = 10;
                EquipWindow _equipWindow;

                CombatAdventurer _character;
                SpriteFont _font;

                List<SpecializedBox> _liGearBoxes;

                GUIWindow _winName;
                public GUIWindow WinDisplay;
                GUIWindow _winClothes;

                SpecializedBox _sBoxArmor;
                SpecializedBox _sBoxHead;
                SpecializedBox _sBoxWeapon;
                SpecializedBox _sBoxWrist;
                SpecializedBox _sBoxShirt;
                SpecializedBox _sBoxHat;
                GUIButton _btnSwap;

                GUIText _gName, _gClass, _gLvl, _gStr, _gDef, _gMagic, _gRes, _gSpd;
                GUIStatDisplay _gBarXP, _gBarHP, _gBarMP;

                public delegate void SyncCharacter();
                private SyncCharacter _delSyncCharacter;

                public CharacterDetailObject(CombatAdventurer c, SyncCharacter del = null)
                {
                    _winName = new GUIWindow(GUIWindow.RedWin, (GUIManager.MAIN_COMPONENT_WIDTH) - (GUIWindow.RedWin.Edge * 2), 10);
                    WinDisplay = new GUIWindow(GUIWindow.RedWin, (GUIManager.MAIN_COMPONENT_WIDTH) - (GUIWindow.RedWin.Edge * 2), (GUIManager.MAIN_COMPONENT_HEIGHT / 4) - (GUIWindow.RedWin.Edge * 2));
                    WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left);
                    _winClothes = new GUIWindow(GUIWindow.RedWin, 10, 10);
                    _winClothes.AnchorAndAlignToObject(WinDisplay, SideEnum.Bottom, SideEnum.Left);

                    _delSyncCharacter = del;
                    _character = c;
                    _font = GameContentManager.GetFont(@"Fonts\Font");

                    _liGearBoxes = new List<SpecializedBox>();
                    Load();

                    _winName.Resize();
                    _winName.Height += SPACING;

                    WinDisplay.Resize();
                    WinDisplay.Height += SPACING;

                    _winClothes.Resize();
                    _winClothes.Height += SPACING;
                    _winClothes.Width += SPACING;

                    WinDisplay.AnchorAndAlignToObject(_winName, SideEnum.Bottom, SideEnum.Left);
                    _winClothes.AnchorAndAlignToObject(WinDisplay, SideEnum.Bottom, SideEnum.Left);

                    AddControl(_winName);
                    AddControl(_winClothes);
                    AddControl(WinDisplay);

                    Width = _winName.Width;
                    Height = _winClothes.Bottom - _winName.Top;
                }

                private void Load()
                {
                    _winClothes.Controls.Clear();
                    _winName.Controls.Clear();
                    WinDisplay.Controls.Clear();

                    _liGearBoxes.Clear();

                    string nameLen = "";
                    for (int i = 0; i < GameManager.MAX_NAME_LEN; i++) { nameLen += "X"; }

                    _gName = new GUIText(nameLen);
                    _gName.AnchorToInnerSide(_winName, SideEnum.TopLeft, SPACING);
                    _gClass = new GUIText("XXXXXXXX");
                    _gClass.AnchorAndAlignToObject(_gName, SideEnum.Right, SideEnum.Bottom, 10);

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

                    if (_character == PlayerManager.Combat)
                    {
                        _sBoxHat = new SpecializedBox(ClothesEnum.Hat, PlayerManager.World.Hat, FindMatchingItems);
                        _sBoxShirt = new SpecializedBox(ClothesEnum.Chest, PlayerManager.World.Shirt, FindMatchingItems);

                        _sBoxHat.AnchorToInnerSide(_winClothes, SideEnum.TopLeft, SPACING);
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
                    _winClothes.AddControl(_equipWindow);
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
                        if (_character == PlayerManager.Combat) { _winClothes.Draw(spriteBatch); }

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
                            Item olditem = _equipWindow.Box.Item;

                            _equipWindow.Box.SetItem(_equipWindow.SelectedItem);
                            if (_equipWindow.Box.ItemType.Equals(ItemEnum.Equipment))
                            {
                                AssignEquipment((Equipment)_equipWindow.SelectedItem);
                            }
                            else if (_equipWindow.Box.ItemType.Equals(ItemEnum.Clothes))
                            {
                                PlayerManager.World.SetClothes((Clothes)_equipWindow.SelectedItem);
                                _delSyncCharacter();
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

                            if (!rv && _character == PlayerManager.Combat)
                            {
                                foreach (GUIObject c in _winClothes.Controls)
                                {
                                    rv = c.ProcessLeftButtonClick(mouse);
                                    if (rv)
                                    {
                                        GUIManager.CloseHoverWindow();
                                        break;
                                    }
                                }
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
                            if (box.Contains(mouse) && box.Item != null)
                            {
                                if (!box.WeaponType.Equals(WeaponEnum.None)) { _character.Weapon = null; }
                                else if (!box.ArmorType.Equals(ArmorEnum.None)) { _character.Armor = null; }
                                else if (!box.ClothingType.Equals(ClothesEnum.None))
                                {
                                    PlayerManager.World.RemoveClothes(((Clothes)box.Item).ClothesType);
                                    _delSyncCharacter();
                                }

                                InventoryManager.AddToInventory(box.Item);
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

                public void SetAdventurer(CombatAdventurer c)
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
                            if (boxMatch.ItemType.Equals(ItemEnum.Equipment) && i.IsEquipment())
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
                            else if (boxMatch.ItemType.Equals(ItemEnum.Clothes) && i.IsClothes())
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
                            _selectedItem = g.Item;
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
                            temp = (Equipment)box.Item;
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
        public class HUDFriendship : GUIWindow
        {
            List<FriendshipBox> _villagerList;

            public HUDFriendship() : base(GUIWindow.RedWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT)
            {
                this.CenterOnScreen();
                _villagerList = new List<FriendshipBox>();

                foreach (Villager n in ObjectManager.DiNPC.Values)
                {
                    FriendshipBox f = new FriendshipBox(n, this.MidWidth());

                    if (_villagerList.Count == 0) { f.AnchorToInnerSide(this, GUIObject.SideEnum.TopLeft); }
                    else
                    {
                        f.AnchorAndAlignToObject(_villagerList[_villagerList.Count - 1], GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left);   //-2 because we start at i=1
                    }

                    _villagerList.Add(f);
                }
            }

            public override bool ProcessLeftButtonClick(Point mouse)
            {
                bool rv = false;

                return rv;
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                bool rv = true;
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

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
            }
            public class FriendshipBox : GUIWindow
            {
                private SpriteFont _font;
                GUIText _gTextName;
                GUIImage _gAdventure;
                GUIImage _gGift;
                List<GUIImage> _liFriendship;

                public FriendshipBox(Villager c, int mainWidth) : base(GUIWindow.BrownWin, mainWidth, 16)
                {
                    _liFriendship = new List<GUIImage>();
                    _font = GameContentManager.GetFont(@"Fonts\Font");
                    _gTextName = new GUIText("XXXXXXXXXX");
                    if (c.GetFriendshipLevel() == 0)
                    {
                        _liFriendship.Add(new GUIImage(new Rectangle(0, 64, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog"));
                    }
                    else
                    {
                        int notches = c.GetFriendshipLevel() - 1;
                        int x = 0;
                        if (notches <= 3) { x = 16; }
                        else if (notches <= 6) { x = 32; }
                        else { x = 48; }

                        while (notches > 0)
                        {
                            _liFriendship.Add(new GUIImage(new Rectangle(x, 64, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog"));
                            notches--;
                        }
                    }

                    _gTextName.AnchorToInnerSide(this, SideEnum.TopLeft);
                    for (int j = 0; j < _liFriendship.Count; j++)
                    {
                        if (j == 0) { _liFriendship[j].AnchorAndAlignToObject(_gTextName, SideEnum.Right, SideEnum.CenterY); }
                        else { _liFriendship[j].AnchorAndAlignToObject(_liFriendship[j - 1], SideEnum.Right, SideEnum.CenterY); }
                    }
                    _gTextName.SetText(c.Name);

                    _gGift = new GUIImage(new Rectangle(16, 48, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog");
                    _gGift.AnchorToInnerSide(this, SideEnum.Right);
                    _gGift.AlignToObject(_gTextName, SideEnum.CenterY);
                    _gGift.Alpha = (c.CanGiveGift) ? 1 : 0.3f;

                    if (c.IsEligible())
                    {
                        EligibleNPC e = (EligibleNPC)c;
                        _gAdventure = new GUIImage(new Rectangle(0, 48, TileSize, TileSize), TileSize, TileSize, @"Textures\Dialog");
                        _gAdventure.AnchorAndAlignToObject(_gGift, SideEnum.Left, SideEnum.CenterY);
                        if (PlayerManager.GetParty().Contains(e.Combat))
                        {
                            _gAdventure.SetColor(Color.Gold);
                        }
                        else { _gAdventure.Alpha = (e.CanJoinParty) ? 1 : 0.3f; }
                    }

                    Resize();
                }
            }
        }
        public class HUDManagement : GUIObject
        {
            public enum ActionTypeEnum { View, Sell, Buy, Upgrade };
            private ActionTypeEnum _eAction;
            public ActionTypeEnum Action => _eAction;
            public static int BTN_PADDING = 20;

            MgmtWindow _mgmtWindow;

            List<GUIObject> _liWorkers;

            WorldAdventurer _worker;
            int _iCost;

            public HUDManagement(ActionTypeEnum action = ActionTypeEnum.View)
            {
                _eAction = action;
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

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
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
                    RHRandom r = new RHRandom();
                    if (_worker.Building != null)
                    {
                        _worker.Building.RemoveWorker(_worker);
                    }

                    selectedBuilding.AddWorker(_worker);

                    if (_eAction == ActionTypeEnum.Buy)
                    {
                        PlayerManager.TakeMoney(_iCost);
                        GameManager.BackToMain();
                        GUIManager.SetScreen(new NamingScreen(_worker));
                    }

                    _worker = null;
                }
            }

            public void HandleMoveWorker(WorldAdventurer worldAdventurer)
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
                _eAction = ActionTypeEnum.Sell;
            }

            public bool Selling()
            {
                return _eAction == ActionTypeEnum.Sell;
            }

            public void PurchaseWorker(WorldAdventurer w, int cost)
            {
                if (w != null)
                {
                    _iCost = cost;
                    _worker = w;
                    _eAction = ActionTypeEnum.Buy;
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

                    _window.CenterOnScreen();
                }

                public class MainBuildingsWin : MgmtWindow
                {
                    public MainBuildingsWin(HUDManagement s, WorldAdventurer w = null) : base(s)
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
                                if (_parent._eAction == ActionTypeEnum.Upgrade)
                                {
                                    b.Building.Upgrade();
                                    GameManager.BackToMain();
                                }
                                else { _parent.HandleBuildingSelection(b.Building); }
                                rv = true;
                                break;
                            }
                        }

                        return rv;
                    }

                    public override bool ProcessRightButtonClick(Point mouse)
                    {
                        GameManager.BackToMain();
                        return false;
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
                        foreach (WorldAdventurer w in selectedBuilding.Workers)
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
                                    GameManager.CurrentNPC = w.Worker;
                                    GUIManager.AddTextSelection("Really sell contract? [Yes:SellContract|No:Cancel]");
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
                        WorldAdventurer _w;
                        public WorldAdventurer Worker => _w;
                        public WorkerBox(WorldAdventurer w)
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
                    WorldAdventurer _character;
                    GUIText _gName, _actionText, _gClass, _gXP, _gStr, _gDef, _gVit, _gMagic, _gRes, _gSpd;
                    GUIItemBox _weapon, _armor;
                    public WorkerDetailsWin(HUDManagement s, WorldAdventurer selectedAdventurer) : base(s)
                    {
                        int statSpacing = 10;
                        _character = selectedAdventurer;
                        _btnMove = new GUIButton("Move", 128, 32);
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
                        _gClass.SetText(_character.Combat.CharacterClass.Name + " " + _character.Combat.ClassLevel);
                        _gXP.SetText("Exp:" + _character.Combat.XP);

                        _weapon = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Combat.Weapon.GetItem());
                        _weapon.AnchorToInnerSide(_window, SideEnum.TopRight);

                        _armor = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Combat.Armor.GetItem());
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

                        _gStr.SetText("Str: " + _character.Combat.StatStr);
                        _gDef.SetText("Def: " + _character.Combat.StatDef);
                        _gVit.SetText("Vit: " + _character.Combat.StatVit);
                        _gMagic.SetText("Mag: " + _character.Combat.StatMag);
                        _gRes.SetText("Res: " + _character.Combat.StatRes);
                        _gSpd.SetText("Spd: " + _character.Combat.StatSpd);


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
        public class HUDOptions : GUIWindow
        {
            GUICheck _gAutoDisband;
            GUICheck _gHideMiniInventory;
            GUIButton _btnSave;

            public HUDOptions() : base(GUIWindow.RedWin, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_WIDTH)
            {
                this.CenterOnScreen();

                _gAutoDisband = new GUICheck("Auto-Disband", GameManager.AutoDisband);
                _gAutoDisband.AnchorToInnerSide(this, SideEnum.TopLeft);

                _gHideMiniInventory = new GUICheck("Hide Mini Inventory", GameManager.HideMiniInventory);
                _gHideMiniInventory.AnchorAndAlignToObject(_gAutoDisband, SideEnum.Bottom, SideEnum.Left);

                _btnSave = new GUIButton("Save", BtnSave);
                _btnSave.AnchorToInnerSide(this, SideEnum.BottomRight);
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
                bool rv = true;
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

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
            }

            public void BtnSave()
            {
                GameManager.AutoDisband = _gAutoDisband.Checked();
                GameManager.HideMiniInventory = _gHideMiniInventory.Checked();
                GameManager.BackToMain();
            }
        }
    }
}
