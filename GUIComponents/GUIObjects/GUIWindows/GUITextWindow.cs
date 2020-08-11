using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextWindow : GUIWindow
    {
        GUIImage _gNext;
        protected GUIText _giText;
        protected GUIImage _giPortrait;
        List<string> _liText;

        public double Duration;

        protected const int MAX_ROWS = 7;

        #region Parsing
        protected int _iCurrText = 0;

        protected int _numReturns = 0;

        protected int _iCharHeight;
        protected int _iCharWidth;

        protected bool _bOpening = false;
        protected double _dOpenTimer;
        protected double _dStartScale = 0.05;
        protected double _dOpenScale = 0.1;
        protected double _dTimer = 0.005;
        protected bool _bGoneOver = true;
        #endregion

        #region Display
        public bool Paused = false;
        #endregion

        public GUITextWindow() : base()
        {
            _giText = new GUIText();
            _liText = new List<string>();
            _iCharWidth = _giText.CharWidth;
            _iCharHeight = _giText.CharHeight;

            AddControl(_giText);
        }

        //Used for the default TextWindow that sits on the bottom of the screen
        public GUITextWindow(string text, bool open = true) : this()
        {
            ConfigureHeight();
            ParseText(text);

            Setup(open);
        }

        //Informational boxes that show up anywhere, like tooltips
        public GUITextWindow(Vector2 position, string text) : this()
        {
            Height = (int)_giText.MeasureString(text).Y + HeightEdges();
            SetWidthMax((int)_giText.MeasureString(text).X, (int)(RiverHollow.ScreenWidth/4));

            ParseText(text);

            string totalVal = string.Empty;
            foreach(string s in _liText)
            {
                totalVal += s;
            }
            _giText.SetText(totalVal);
            Position(position);

            _giText.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
            Resize();
        }

        //Temporary TextWindow that just shows up briefly and is disposed of later.
        //MAR this should probably be deleted.
        public GUITextWindow(string text, double duration) : this()
        {
            ParseText(text);
            Duration = duration;

            Height = _iCharHeight;
            Width = (int)_giText.TextSize.X;
            _giText.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
            Resize();
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

        /// <summary>
        /// Method used to ensure that the components for the TextWindow are synced
        /// together. Called on finishing the opening animation, or immediately after
        /// creating the window if we do not play it.
        /// </summary>
        protected void SyncObjects()
        {
            if (GameManager.CurrentNPC != null)
            {
                TalkingActor talker = GameManager.CurrentNPC;
                _giPortrait = new GUIImage(talker.PortraitRectangle, talker.PortraitRectangle.Width, talker.PortraitRectangle.Height, talker.Portrait);
                _giPortrait.SetScale(GameManager.Scale);
                _giPortrait.AnchorToInnerSide(this, SideEnum.Left);
                _giPortrait.AnchorToObject(this, SideEnum.Top);
                AddControl(_giPortrait);
            }

            _giText.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);

            if (_liText.Count > 1)
            {
                _gNext = new GUIImage(new Rectangle(288, 64, GameManager.TileSize, GameManager.TileSize), GameManager.ScaledTileSize, GameManager.ScaledTileSize, @"Textures\Dialog");     //???
                _gNext.AnchorToInnerSide(this, SideEnum.BottomRight);
            }

            AnchorToScreen(SideEnum.Bottom, SpaceFromBottom);
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

                if (Duration > 0) { Duration -= gTime.ElapsedGameTime.TotalSeconds; }
                _giText.Update(gTime);

                if (_giText.Done)
                {
                    Paused = true;
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
           return HandleButtonClick();
            
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            return HandleButtonClick();
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
                if (!NextText())
                {
                    rv = GUIManager.CloseTextWindow(this);
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

        /// <summary>
        /// Iterates over the given text word by word to create a list of text entries that will
        /// be on each screen. These text entries will have /n entries manually inserted to properly
        /// display based off of the GUITextWindow dimensions.
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="printAll">Whether we will print everything at once</param>
        protected void ParseText(string text, bool printAll = true)
        {
            bool grabLast = true;
            _numReturns = 0;
            string line = string.Empty;
            string returnString = string.Empty;
            string[] wordArray = text.Split(' ');   //Split the given entry around each word. Note that it is important that /n be its own word

            foreach (string word in wordArray)
            {
                Vector2 measure = _giText.MeasureString(line + word);

                if (word.Contains("\n"))
                {
                    returnString = returnString + line + word;
                    line = string.Empty;
                    _numReturns++;
                }
                else
                {
                    if ((measure.Length() >= MidWidth() - GUIManager.STANDARD_MARGIN * 2) ||
                        (_numReturns == MAX_ROWS - 1 && measure.Length() >= (Width) - _giText.CharHeight))
                    {
                        returnString = returnString + line + '\n';
                        line = string.Empty;
                        _numReturns++;
                    }

                    grabLast = true;
                    line = line + word + ' ';
                }

                //Spillover to another screen when we have too many returns
                if (_numReturns + 1 > MAX_ROWS)
                {
                    grabLast = false;
                    _liText.Add(returnString);
                    _numReturns = 0;
                    returnString = string.Empty;
                }
            }

            if (grabLast)
            {
                _liText.Add(returnString + line);
            }

            if (printAll) { _giText.SetText(_liText[0]); }
            else { _giText.ResetText(_liText[0]); }
        }

        private bool ShowNextButton()
        {
            return Paused && _gNext != null && _iCurrText < _liText.Count -1;
        }

        public bool NextText()
        {
            bool rv = false;

            Paused = false;
            _iCurrText++;
            if (_iCurrText < _liText.Count)
            {
                _giText.ResetText(_liText[_iCurrText]);
                rv = true;
            }

            return rv;
        }

        public void PrintAll()
        {
            _giText.PrintAll = true;
        }

        public bool Done()
        {
            return _iCurrText == _liText.Count && _giText.Done;
        }

        public void ResetText(string text)
        {
            _giText.ResetText(text);
        }

        protected void SetWidthMax(int val, int maxWidth)
        {
            val += (_winData.Edge * 2);
            Width = val > maxWidth ? maxWidth : val;
        }

        public virtual bool IsSelectionBox() { return false; }
    }
}
