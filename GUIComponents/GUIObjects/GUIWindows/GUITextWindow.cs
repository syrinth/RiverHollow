using RiverHollow.Actors;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIComponents.GUIObjects;
using System.Collections.Generic;
using System;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects
{
    public class GUITextWindow : GUIWindow
    {
        GUIImage _next;
        protected GUIText _giText;
        protected GUIImage _giPortrait;
        List<string> _liText;

        public double Duration;

        protected const int _maxRows = 3;

        #region Parsing
        protected int _iCurrText = 0;

        protected int _numReturns = 0;

        protected int _iCharHeight;
        protected int _iCharWidth;
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
        }

        //Used for the default TextWindow that sits on the bottom of the screen
        public GUITextWindow(string text) : this()
        {
            ParseText(text);
            Height = Math.Max(Height, (_iCharHeight * _maxRows));
            Resize();

            _giText.AnchorToInnerSide(this, SideEnum.TopLeft);

            _next = new GUIImage(new Rectangle(288, 64, 32, 32), _iCharHeight, _iCharHeight, @"Textures\Dialog");     //???
            _next.AnchorToInnerSide(this, SideEnum.BottomRight);

            Setup();
        }

        //Informational boxes that show up anywhere, like tooltips
        public GUITextWindow(Vector2 position, string text) : this()
        {
            Height = (int)_giText.MeasureString(text).Y + (_winData.Edge * 2);
            SetWidthMax((int)_giText.MeasureString(text).X, (int)(RiverHollow.ScreenWidth - position.X));

            ParseText(text);

            string totalVal = string.Empty;
            foreach(string s in _liText)
            {
                totalVal += s;
            }
            _giText.SetText(totalVal);
            Position(position);

            _giText.AnchorToInnerSide(this, SideEnum.TopLeft);
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
            Position(Vector2.Zero);
            _giText.AnchorToInnerSide(this, SideEnum.TopLeft);
            Resize();
        }

        public void Setup()
        {
            if (GameManager.gmNPC != null)
            {
                TalkingActor talker = GameManager.gmNPC;
                _giPortrait = new GUIImage(talker.PortraitRectangle, talker.PortraitRectangle.Width, talker.PortraitRectangle.Height, talker.Portrait);
                _giPortrait = new GUIImage(talker.PortraitRectangle, talker.PortraitRectangle.Width, talker.PortraitRectangle.Height, talker.Portrait);
                _giPortrait.SetScale(GameManager.Scale);
                _giPortrait.AnchorAndAlignToObject(this, SideEnum.Top, SideEnum.Left);
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (ShowNextButton())
            {
                _next.Update(gameTime);
            }

            if (Duration > 0) { Duration -= gameTime.ElapsedGameTime.TotalSeconds; }
            _giText.Update(gameTime);

            if (_giText.Done)
            {
                Paused = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            _giText.Draw(spriteBatch);
            if (ShowNextButton())
            {
                _next.Draw(spriteBatch);
            }

            if (_giPortrait != null)
            {
                _giPortrait.Draw(spriteBatch);
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

        protected void ParseText(string text, bool printAll = true)
        {
            bool grabLast = true;
            _numReturns = 0;
            string line = string.Empty;
            string returnString = string.Empty;
            string[] wordArray = text.Split(' ');

            foreach (string word in wordArray)
            {
                Vector2 measure = _giText.MeasureString(line + word);

                if (measure.Length() >= (Width) ||
                    _numReturns == _maxRows - 1 && measure.Length() >= (Width) - _giText.CharHeight)
                {
                    returnString = returnString + line + '\n';
                    line = string.Empty;
                    _numReturns++;
                }

                grabLast = true;
                line = line + word + ' ';

                //Spillover to another screen
                if (measure.Y * (_numReturns + 1) >= (Height))
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
            return Paused && _next != null;
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

        protected void SetWidthMax(int val)
        {
            SetWidthMax(val, RiverHollow.ScreenWidth / 3);   
        }

        protected void SetWidthMax(int val, int maxWidth)
        {
            val += (_winData.Edge * 2);
            Width = val > maxWidth ? maxWidth : val;
        }

        public virtual bool IsSelectionBox() { return false; }
    }
}
