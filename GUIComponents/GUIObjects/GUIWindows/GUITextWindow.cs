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
        string _sFontName = @"Fonts\Font";

        public double Duration;

        protected const int _maxRows = 3;

        #region Parsing
        protected List<GUIText> _liText;

        protected int _iCurrText = 0;

        protected int _numReturns = 0;

        protected int _iCharHeight;
        protected int _iCharWidth;
        #endregion

        #region Display
        public bool Paused = false;
        #endregion

        public GUITextWindow() : base() {
            _liText = new List<GUIText> { new GUIText() };
            _iCharWidth = _liText[0].CharWidth;
            _iCharHeight = _liText[0].CharHeight;
        }

        public GUITextWindow(NPC npc, string text) : this()
        {
            ParseText(text, true);
            Height = Math.Max(Height, (_iCharHeight * _maxRows));

            _next = new GUIImage(Vector2.Zero, new Rectangle(288, 64, 32, 32), _iCharHeight, _iCharHeight, @"Textures\Dialog");     //???
            _next.AnchorToInnerSide(this, SideEnum.BottomRight);
            GameManager.gmNPC = npc;
        }

        public GUITextWindow(string text) : this()
        {
            ParseText(text);

            Height = (int)_liText[0].TextSize.Y;
            Width = (int)_liText[0].TextSize.X;
            _liText[0].AnchorToInnerSide(this, SideEnum.TopLeft);
        }

        public GUITextWindow(Vector2 position, string text) : this()
        {
            ParseText(text);

            Height = (int)_liText[0].TextSize.Y;
            Width = (int)_liText[0].TextSize.X;
            Position(position);
            _liText[0].AnchorToInnerSide(this, SideEnum.TopLeft);
            Resize();
        }

        public GUITextWindow(string text, double duration) : this()
        {
            ParseText(text);
            Duration = duration;

            Height = _liText.Count * _iCharHeight;
            Width = (int)_liText[0].TextSize.X;
            Position(PositionSub(new Vector2(Width / 2, Height / 2)));
        }

        public override void Update(GameTime gameTime)
        {
            if (_iCurrText < _liText.Count)
            {
                if (ShowNextButton())
                {
                    _next.Update(gameTime);
                }

                if (Duration > 0) { Duration -= gameTime.ElapsedGameTime.TotalSeconds; }
                _liText[_iCurrText].Update(gameTime);

                if (_liText[_iCurrText].Done)
                {
                    Paused = true;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_iCurrText < _liText.Count)
            {
                base.Draw(spriteBatch);
            
                _liText[_iCurrText].Draw(spriteBatch);
                if (ShowNextButton())
                {
                    _next.Draw(spriteBatch);
                }

                if (GameManager.gmNPC != null)
                {
                    GameManager.gmNPC.DrawPortrait(spriteBatch, new Vector2(InnerTopLeft().X, InnerTopLeft().Y - EdgeSize));
                }
            }
        }

        protected void ParseText(string text, bool printAll = false)
        {
            bool grabLast = true;
            _numReturns = 0;
            string line = string.Empty;
            string returnString = string.Empty;
            string[] wordArray = text.Split(' ');

            foreach (string word in wordArray)
            {
                Vector2 measure = _liText[0].MeasureString(line + word);

                if (measure.Length() >= (Width) ||
                    _numReturns == _maxRows - 1 && measure.Length() >= (Width) - _liText[0].CharHeight)
                {
                    returnString = returnString + line + '\n';
                    line = string.Empty;
                    _numReturns++;
                }

                grabLast = true;
                line = line + word + ' ';

                //Spillover to another screen
                if (measure.Y * _numReturns >= (Height))
                {
                    grabLast = false;
                    GUIText t = new GUIText(returnString, printAll);
                    t.AnchorToInnerSide(this, SideEnum.TopLeft);
                    _liText.Add(t);
                    _numReturns = 0;
                    returnString = string.Empty;
                }
            }

            if (grabLast)
            {
                GUIText t = new GUIText(returnString + line, printAll);
                t.AnchorToInnerSide(this, SideEnum.TopLeft);
                _liText.Add(t);
            }

            if (string.IsNullOrEmpty(_liText[0].GetText()))
            {
                _liText.RemoveAt(0);
            }
        }

        private bool ShowNextButton()
        {
            return Paused && _next != null;
        }

        public void NextText()
        {
            Paused = false;
            _iCurrText++;
        }

        public void PrintAll()
        {
            _liText[_iCurrText].PrintAll = true;
        }

        public bool Done()
        {
            return _iCurrText == _liText.Count && _liText[_iCurrText-1].Done;
        }
    }
}
