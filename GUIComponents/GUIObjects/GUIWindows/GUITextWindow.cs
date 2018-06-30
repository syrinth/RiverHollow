using RiverHollow.Characters;
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

        public GUITextWindow(NPC npc, string text) : this()
        {
            GameManager.gmNPC = npc;
            ParseText(text, false);
            Height = Math.Max(Height, (_iCharHeight * _maxRows));

            _next = new GUIImage(Vector2.Zero, new Rectangle(288, 64, 32, 32), _iCharHeight, _iCharHeight, @"Textures\Dialog");     //???
            _next.AnchorToInnerSide(this, SideEnum.BottomRight);

            Resize();
            _giText.AnchorToInnerSide(this, SideEnum.TopLeft);
        }

        public GUITextWindow(string text) : this()
        {
            ParseText(text);

            Height = (int)_giText.TextSize.Y;
            SetWidthMax((int)_giText.TextSize.X);
            _giText.AnchorToInnerSide(this, SideEnum.TopLeft);
            Resize();
        }

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

            if (GameManager.gmNPC != null && GUIManager.IsTextScreen())
            {
                GameManager.gmNPC.DrawPortrait(spriteBatch, new Vector2(InnerTopLeft().X, InnerTopLeft().Y - EdgeSize));
            }
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

        public void NextText()
        {
            Paused = false;
            _iCurrText++;
            if (_iCurrText < _liText.Count)
            {
                _giText.ResetText(_liText[_iCurrText]);
            }
        }

        public void PrintAll()
        {
            _giText.PrintAll = true;
        }

        public bool Done()
        {
            return _iCurrText == _liText.Count && _giText.Done;
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
    }
}
