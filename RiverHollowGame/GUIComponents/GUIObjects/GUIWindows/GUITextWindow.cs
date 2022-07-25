using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextWindow : GUIWindow
    {
        public bool ProcessClicks = true;

        GUIImage _gNext;
        protected GUIText _giText;
        protected GUIImage _giPortrait;
        protected List<string> _liTextPages;

        public double Duration;

        protected const int MAX_ROWS = 7;

        #region Parsing
        protected int _iCurrText = 0;

        protected int _iCharHeight;
        protected int _iCharWidth;

        protected bool _bOpening = false;
        protected double _dOpenTimer;
        protected double _dStartScale = 0.05;
        protected double _dOpenScale = 0.1;
        protected double _dTimer = 0.004;
        protected bool _bGoneOver = true;
        #endregion

        #region Display
        public bool Paused = false;
        protected bool _bDisplayDialogueIcon = false;
        #endregion

        protected TextEntry _textEntry;

        public GUITextWindow() : base()
        {
            _giText = new GUIText("", true, DataManager.FONT_NEW);
            _liTextPages = new List<string>();
            _iCharWidth = _giText.CharWidth;
            _iCharHeight = _giText.CharHeight;

            AddControl(_giText);
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
        }

        //Informational boxes that show up anywhere, like tooltips
        public GUITextWindow(TextEntry text, Vector2 position) : this()
        {
            _textEntry = text;
            _textEntry.HandlePreWindowActions();
            Height = (int)_giText.MeasureString(_textEntry.GetFormattedText()).Y + HeightEdges();
            Width = (int)(RiverHollow.ScreenWidth / 4);

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
            _giText.SetText(totalVal);
            Position(position);

            _giText.PrintAll = true;
            _giText.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
            Resize();
        }

        public void ClosingWindow()
        {
            _textEntry.HandlePostWindowActions();
            if (!GUIManager.IsMainObjectOpen()) {
                GameManager.CurrentNPC?.StopTalking();
            }
        }

        public void Setup(bool openUp)
        {
            AnchorToScreen(SideEnum.Bottom, SpaceFromBottom);
            if (openUp)
            {
                _bOpening = true;
                _dOpenTimer = _dTimer;
                SetScale(_dStartScale, false);
            }
            else
            {
                SyncObjects();
                SetScale(1, false);
            }
        }

        protected void ConfigureHeight()
        {
            Height = Math.Max(Height, (_iCharHeight * MAX_ROWS) + HeightEdges() + (2 * GUIManager.STANDARD_MARGIN));
        }

        protected void SyncText(string text, bool printAll = false)
        {
            _liTextPages = _giText.ParseText(text, InnerWidth(), MAX_ROWS, printAll);
            if (_giText.PrintAll) { _giText.SetText(_liTextPages[0]); }
            else { _giText.ResetText(_liTextPages[0]); }
        }
        /// <summary>
        /// Method used to ensure that the components for the TextWindow are synced
        /// together. Called on finishing the opening animation, or immediately after
        /// creating the window if we do not play it.
        /// </summary>
        protected void SyncObjects()
        {
            DisplayCharacterPortrait();

            _giText.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);

            DisplayDialogueFinishedIcon();

            AnchorToScreen(SideEnum.Bottom, SpaceFromBottom);
        }

        protected void DisplayCharacterPortrait()
        {
            if (GameManager.CurrentNPC != null && _textEntry.SelectionType != Utilities.Enums.TextEntrySelectionEnum.YesNo)
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
                if (ShowNextButton())
                {
                    _gNext.Update(gTime);
                }

                // if (Duration > 0) { Duration -= gTime.ElapsedGameTime.TotalSeconds; }

                _giText.Update(gTime);

                if (_giText.Done)
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
                _giText.Draw(spriteBatch);
                if (ShowNextButton())
                {
                    _gNext.Draw(spriteBatch);
                }

                if (_giPortrait != null)
                {
                    _giPortrait.Draw(spriteBatch);
                }

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
            _dOpenTimer -= gTime.ElapsedGameTime.TotalSeconds;
            if (_dOpenTimer <= 0)
            {
                _dStartScale += _dOpenScale;
                if(_dStartScale >= 1)
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
                    _dOpenTimer = _dTimer;
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
                _giText.ResetText(_liTextPages[_iCurrText]);
                rv = true;
            }

            return rv;
        }

        public void PrintAll()
        {
            _giText.PrintAll = true;
            DisplayDialogueFinishedIcon();
        }

        public bool Done()
        {
            return _iCurrText == _liTextPages.Count && _giText.Done;
        }

        private void DisplayDialogueFinishedIcon()
        {
            if (_bDisplayDialogueIcon)
            {
                if (_iCurrText < _liTextPages.Count - 1)
                {
                    _gNext = new GUIImage(new Rectangle(288, 64, GameManager.TILE_SIZE, GameManager.TILE_SIZE), GameManager.ScaledTileSize, GameManager.ScaledTileSize, DataManager.DIALOGUE_TEXTURE);     //???
                }
                else
                {
                    _gNext = new GUIImage(new Rectangle(304, 64, GameManager.TILE_SIZE, GameManager.TILE_SIZE), GameManager.ScaledTileSize, GameManager.ScaledTileSize, DataManager.DIALOGUE_TEXTURE);     //???
                }
                _gNext.AnchorAndAlignToObject(this, SideEnum.Right, SideEnum.Bottom);
            }
        }

        public void ResetText(TextEntry text)
        {
            _giText.ResetText(text.GetFormattedText());
        }

        protected void SetWidthMax(int val, int maxWidth)
        {
            val += (_winData.WidthEdges());
            Width = val > maxWidth ? maxWidth : val;
        }

        public virtual bool IsSelectionBox() { return false; }
    }
}
