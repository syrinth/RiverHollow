using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextWindow : GUIWindow
    {
        protected const int DIALOGUE_MAX_ROWS = 3;
        protected const int BORDER_EDGE = 2;
        protected int _iMaxRows = DIALOGUE_MAX_ROWS;

        public bool ProcessClicks = true;

        GUIImage _gNext;
        protected GUIText _gText;
        protected GUIImage _giPortrait;
        protected List<string> _liTextPages;

        #region Parsing
        protected int _iCurrText = 0;

        protected int _iCharHeight;
        protected int _iCharWidth;

        protected bool _bOpening = false;
        protected double _dStartScale = 0.05;
        protected double _dOpenScale = 0.1;
        protected bool _bGoneOver = true;

        protected RHTimer _timer;
        #endregion

        #region Display
        public bool Paused = false;
        protected bool _bDisplayDialogueIcon = false;
        #endregion

        protected TextEntry _textEntry;

        public GUITextWindow() : base()
        {
            Width = (int)(RiverHollow.ScreenWidth * 0.75);

            _gText = new GUIText("", true, DataManager.FONT_MAIN);

            _liTextPages = new List<string>();
            _iCharWidth = _gText.CharWidth;
            _iCharHeight = _gText.CharHeight;

            AddControl(_gText);

            if (GameManager.PrintTextImmediately)
            {
                PrintAll();
            }
        }

        //Used for the default TextWindow that sits on the bottom of the screen
        public GUITextWindow(TextEntry text, bool open = true, bool displayDialogueIcon = false) : this()
        {
            _textEntry = text;
            _textEntry.HandlePreWindowActions(GameManager.CurrentNPC);
            _bDisplayDialogueIcon = displayDialogueIcon;
            ConfigureHeight();
            SyncText(_textEntry.GetFormattedText());
            Setup(open);

            if (GameManager.PrintTextImmediately)
            {
                PrintAll();
            }
        }

        //Informational boxes that show up anywhere, like tooltips
        public GUITextWindow(TextEntry text, Point position) : this()
        {
            _textEntry = text;
            _textEntry.HandlePreWindowActions();
            Height = (int)_gText.MeasureString(_textEntry.GetFormattedText()).Y + HeightEdges();
            Width = RiverHollow.ScreenWidth / 4;

            SyncText(_textEntry.GetFormattedText());

            string totalVal = string.Empty;
            for(int i = 0; i < _liTextPages.Count; i++)
            {
                totalVal += _liTextPages[i];
                if (i < _liTextPages.Count - 1)
                {
                    totalVal += System.Environment.NewLine;
                }
            }
            _gText.SetText(totalVal);
            Position(position);

            _gText.PrintAll = true;
            _gText.AnchorToInnerSide(this, SideEnum.TopLeft, BORDER_EDGE);
            Resize();

            if (GameManager.PrintTextImmediately)
            {
                PrintAll();
            }
        }

        public void ClosingWindow()
        {
            _textEntry.HandlePostWindowActions();
            if (!GUIManager.IsMainObjectOpen() && GUIManager.QueuedWindowText == null ) {
                GameManager.CurrentNPC?.StopTalking();
            }
         }

        public void Setup(bool openUp)
        {
            AnchorToScreen(SideEnum.Bottom, SpaceFromBottom);
            if (openUp)
            {
                _bOpening = true;
                _timer = new RHTimer(Constants.GUI_WINDOW_OPEN_SPEED);
                SetScale(_dStartScale, false);
            }
            else
            {
                SyncObjects();
                SetScale(1, false);
            }
        }

        public int TotalScaledBorder()
        {
            return BORDER_EDGE * 2 * GameManager.CurrentScale;
        }
        protected void ConfigureHeight()
        {
            var heightEdges = HeightEdges();
            Height = Math.Max(Height, (_iCharHeight * _iMaxRows) + HeightEdges() + TotalScaledBorder());
        }

        protected void SyncText(string text, bool printAll = false)
        {
            _liTextPages = _gText.ParseText(text, InnerWidth() - TotalScaledBorder(), DIALOGUE_MAX_ROWS, printAll);
            if (_gText.PrintAll) { _gText.SetText(_liTextPages[0]); }
            else { _gText.ResetText(_liTextPages[0]); }
        }
        /// <summary>
        /// Method used to ensure that the components for the TextWindow are synced
        /// together. Called on finishing the opening animation, or immediately after
        /// creating the window if we do not play it.
        /// </summary>
        protected virtual void SyncObjects()
        {
            DisplayCharacterPortrait();

            _gText.AnchorToInnerSide(this, SideEnum.TopLeft, BORDER_EDGE);

            DisplayDialogueFinishedIcon();

            AnchorToScreen(SideEnum.Bottom, SpaceFromBottom);
        }

        protected void DisplayCharacterPortrait()
        {
            if (GameManager.CurrentNPC != null && GameManager.CurrentNPC.Portrait != null && _textEntry.SelectionType != Utilities.Enums.TextEntrySelectionEnum.YesNo)
            {
                RemoveControl(_giPortrait);
                TalkingActor talker = GameManager.CurrentNPC;
                talker.DeQueueActorFace();
                _giPortrait = new GUIImage(talker.GetPortraitRectangle(), talker.GetPortraitRectangle().Width, talker.GetPortraitRectangle().Height, talker.Portrait);
                _giPortrait.SetScale(GameManager.CurrentScale);
                _giPortrait.AnchorToInnerSide(this, SideEnum.Left);
                _giPortrait.AnchorToObject(this, SideEnum.Top);
                AddControl(_giPortrait);
            }
        }

        public override void Update(GameTime gTime)
        {
            if (_bOpening)
            {
                HandleOpening(gTime);
            }
            else
            {
                _gText.Update(gTime);

                if (_gText.Done)
                {
                    Paused = true;
                    DisplayDialogueFinishedIcon();
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawWindow(spriteBatch);

            if (!_bOpening)
            {
                _gNext?.Show(Paused);
                _gText.Draw(spriteBatch);

                foreach (GUIObject g in Controls)
                {
                    g.Draw(spriteBatch);
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            if (ProcessClicks) { return HandleButtonClick(); }
            else { return false; }
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            if (ProcessClicks) { return HandleButtonClick(); }
            else { return false; }
        }

        private bool HandleButtonClick()
        {
            bool rv = false;
            if (!Paused)
            {
                PrintAll();
            }
            else
            {
                DisplayCharacterPortrait();
                if (!NextText())
                {
                    rv = GUIManager.CloseTextWindow();
                }
                else
                {
                    rv = true;
                }
            }

            return rv;
        }

        protected void HandleOpening(GameTime gTime)
        {
            
            if (_timer.TickDown(gTime))
            {
                _dStartScale += _dOpenScale;
                if (_dStartScale >= 1)
                {
                    if (_bGoneOver)
                    {
                        SetScale(1, false);
                        _bOpening = false;
                        SyncObjects();
                    }
                    else
                    {
                        SetScale(1 + 0.05, false);
                        _bGoneOver = true;
                    }
                }
                else
                {
                    SetScale(Math.Min(1, _dStartScale), false);
                    _timer.Reset();
                }
            }
        }

        private bool ShowNextButton()
        {
            return Paused && _gNext != null;
        }

        public bool NextText()
        {
            bool rv = false;

            Paused = false;
            _iCurrText++;
            if (_iCurrText < _liTextPages.Count)
            {
                _gText.ResetText(_liTextPages[_iCurrText]);
                rv = true;

                if (GameManager.PrintTextImmediately)
                {
                    PrintAll();
                }
            }

            return rv;
        }

        public void PrintAll()
        {
            _gText.PrintAll = true;
            DisplayDialogueFinishedIcon();
        }

        public bool Done()
        {
            return _iCurrText == _liTextPages.Count && _gText.Done;
        }

        private void DisplayDialogueFinishedIcon()
        {
            if (_bDisplayDialogueIcon)
            {
                RemoveControl(_gNext);
                _gNext = new GUIImage((_iCurrText < _liTextPages.Count - 1) ? GUIUtils.DIALOGUE_MORE : GUIUtils.DIALOGUE_DONE);
                _gNext.AnchorAndAlign(this, SideEnum.Right, SideEnum.Bottom);
                AddControl(_gNext);
            }
        }

        public void ResetText(TextEntry text)
        {
            _gText.ResetText(text.GetFormattedText());
        }

        protected void SetWidthMax(int val, int maxWidth)
        {
            val += (_winData.WidthEdges());
            Width = val > maxWidth ? maxWidth : val;
        }

        public virtual bool IsSelectionBox() { return false; }
    }
}
