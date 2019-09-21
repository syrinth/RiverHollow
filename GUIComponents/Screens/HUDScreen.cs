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
using static RiverHollow.GUIComponents.GUIObjects.NPCDisplayBox;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using static RiverHollow.Game_Managers.GUIObjects.GUIButton;
using System.Linq;

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
            _gHealthDisplay = new GUIStatDisplay(PlayerManager.World.GetHP, Color.Green);
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
        public override void CloseMainObject()
        {
            RemoveControl(_gMainObject);
            _gMainObject = null;
        }

        public override bool CloseOnRightClick() {
            if (_gSelectionWindow != null) { return false; }
            if (_gMainObject != null) { return true; }

            return false;
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

                _charBox = new CharacterDetailObject(PlayerManager.World, SyncCharacter);
                _charBox.CenterOnScreen();
                AddControl(_charBox);

                _btnMap.AnchorAndAlignToObject(_charBox.WinDisplay, SideEnum.Right, SideEnum.Top);

                int partySize = PlayerManager.GetParty().Count;
                _arrDisplayBoxes = new NPCDisplayBox[partySize];

                for (int i = 0; i < partySize; i++)
                {
                    if (PlayerManager.GetParty()[i] == PlayerManager.World)
                    {
                        _arrDisplayBoxes[i] = new PlayerDisplayBox(true, ChangeSelectedCharacter);
                    }
                    else
                    {
                        ClassedCombatant c = PlayerManager.GetParty()[i];
                        if (c != null)
                        {
                            _arrDisplayBoxes[i] = new CharacterDisplayBox(c, ChangeSelectedCharacter);
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

            public void UpdateCharacterBox(ClassedCombatant displayCharacter)
            {
                _charBox.SetAdventurer(displayCharacter);
            }

            public void ChangeSelectedCharacter(ClassedCombatant selectedCharacter)
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
                ClassedCombatant _currentCharacter;
                StartPosition _currPosition;
                StartPosition[,] _arrStartPositions;

                public delegate void ClickDelegate(ClassedCombatant selectedCharacter);
                private ClickDelegate _delAction;

                public PositionMap(ClassedCombatant adv, ClickDelegate del) : base(BrownWin, 16, 16)
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

                public void SetOccupancy(ClassedCombatant currentCharacter)
                {
                    _currentCharacter = currentCharacter;
                    foreach (ClassedCombatant c in PlayerManager.GetParty())
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
                    ClassedCombatant _character;
                    public ClassedCombatant Character => _character;
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

                    public void SetCharacter(ClassedCombatant c)
                    {
                        _character = c;
                        if (c != null)
                        {
                            if (c != null)
                            {
                                if (c == PlayerManager.World) { _sprite = new GUICharacterSprite(true); }
                                else { _sprite = new GUICharacterSprite(c.BodySprite, true); }

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

                ClassedCombatant _character;
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

                public CharacterDetailObject(ClassedCombatant c, SyncCharacter del = null)
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

                    if (_character == PlayerManager.World)
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
                        if (_character == PlayerManager.World) { _winClothes.Draw(spriteBatch); }

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

                            if (!rv && _character == PlayerManager.World)
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
                        if (PlayerManager.GetParty().Contains(e))
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

            Adventurer _worker;
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
                        GUIManager.OpenMainObject(new HUDNamingWindow(_worker));
                    }

                    _worker = null;
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
                _eAction = ActionTypeEnum.Sell;
            }

            public bool Selling()
            {
                return _eAction == ActionTypeEnum.Sell;
            }

            public void PurchaseWorker(Adventurer w, int cost)
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
                                if (_parent._eAction == ActionTypeEnum.Upgrade)
                                {
                                    b.Building.StartBuilding(false);
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
                        _gClass.SetText(_character.CharacterClass.Name + " " + _character.ClassLevel);
                        _gXP.SetText("Exp:" + _character.XP);

                        _weapon = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Weapon.GetItem());
                        _weapon.AnchorToInnerSide(_window, SideEnum.TopRight);

                        _armor = new GUIItemBox(new Rectangle(288, 32, 32, 32), 32, 32, @"Textures\Dialog", _character.Armor.GetItem());
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

        public class HUDNamingWindow : GUITextInputWindow
        {
            Building _bldg;
            Adventurer _adv;

            /// <summary>
            /// Never called outwardly, only for private use
            /// </summary>
            private HUDNamingWindow()
            {
                SetupNaming();
                TakeInput = true;
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
                AcceptSpace = true;
            }

            /// <summary>
            /// Update function for the window.
            /// 
            /// Only setthe name of the component when it is finished taking input
            /// </summary>
            /// <param name="gameTime"></param>
            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
                if (Finished)
                {
                    if (_adv != null)
                    {
                        _adv.SetName(EnteredText);
                    }
                    if (_bldg != null)
                    {
                        _bldg.SetName(EnteredText);
                    }

                    //We know that this window only gets created under special circumstances, so unset them
                    RiverHollow.ResetCamera();
                    GameManager.Unpause();
                    GameManager.Scry(false);
                    GameManager.DontReadInput();
                }
            }

            public override bool ProcessRightButtonClick(Point mouse)
            {
                return false;
            }
        }
    }

    class HUDMissionWindow : GUIObject
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
                _btnUp = new GUIButton(new Rectangle(256, 64, 32, 32), GUIManager.MINI_BTN_HEIGHT, GUIManager.MINI_BTN_HEIGHT, @"Textures\Dialog", BtnUpClick);
                _btnDown = new GUIButton(new Rectangle(256, 96, 32, 32), GUIManager.MINI_BTN_HEIGHT, GUIManager.MINI_BTN_HEIGHT, @"Textures\Dialog", BtnDownClick);

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
                GraphicCursor.Alpha = 1;
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
            GameManager.BackToMain();
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
}
