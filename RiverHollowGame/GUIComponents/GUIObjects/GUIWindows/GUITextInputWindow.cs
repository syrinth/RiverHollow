using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextInputWindow : GUITextWindow
    {
        SideEnum _textLoc;
        protected GUIText _gStatement;
        protected GUIText _gText;
        protected GUIMarker _gMarker;

        protected int _iCurr;
        protected int _iMaxLength;

        public bool AllowAll = false;

        protected bool _bFinished;
        public bool Finished => _bFinished;
        public bool AcceptSpace;
        bool _bTakeInput;

        public string EnteredText => _gText.Text;

        public GUITextInputWindow(int maxLength = -1) : base()
        {
            GameManager.TakeInput();

            _gMarker = new GUIMarker();
            _iCurr = 0;
            _iMaxLength = maxLength != -1 ? maxLength : GameManager.MAX_NAME_LEN;

            _bTakeInput = false;
        }

        public GUITextInputWindow(string statement, SideEnum textLoc, int maxLength = -1) : this (maxLength)
        {
            StatementSetup(statement, textLoc);
            _textLoc = textLoc;
        }

       public void SetupNaming()
        {
            _textLoc = SideEnum.Top;
            StatementSetup("Enter Name:  ");
            Width = Math.Max(_gStatement.Width, _gStatement.CharWidth * 10);
            Resize(false);

            CenterOnScreen();
        }

        private void StatementSetup(string text, SideEnum textLoc = SideEnum.Top)
        {
            _gStatement = new GUIText(text);

            if (textLoc == SideEnum.Top)
            {
                Width = Math.Max(_gStatement.Width, _gStatement.CharWidth * 10);
                Height = _gStatement.Height*2;
            }
            else if (textLoc == SideEnum.Left)
            {
                Width = _gStatement.Width + _gStatement.CharWidth * 10;
                Height = _gStatement.Height;
            }

            _gStatement.AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);
            _gStatement.SetColor(Color.White);

            _gText = new GUIText();
            _gStatement.SetColor(Color.White);

            if (textLoc == SideEnum.Top)
            {
                _gText.AnchorAndAlignToObject(_gStatement, SideEnum.Bottom, SideEnum.Left);
            }
            else if (textLoc == SideEnum.Left)
            {
                _gText.AnchorAndAlignToObject(_gStatement, SideEnum.Right, SideEnum.Bottom, 10);
            }
            _gMarker.Position(_gText.Position());
            
            AddControl(_gText);
            AddControl(_gMarker);

            Resize(false);
        }

        public override void Update(GameTime gTime)
        {
            if (_bTakeInput)
            {
                _gMarker.Update(gTime);
                foreach (Keys k in InputManager.KeyDownDictionary.Keys.ToList())
                {
                    if (InputManager.CheckPressedKey(k))
                    {
                        if (k == Keys.Enter)
                        {
                            _bFinished = true;
                            break;
                        }
                        else if (AllowAll || k >= Keys.A && k <= Keys.Z || (AcceptSpace && k == Keys.Space) || k == Keys.Delete || k == Keys.Back || k == Keys.Left || k == Keys.Right || k == Keys.Delete)
                        {
                            if (k == Keys.Left)
                            {
                                DecrementMarker();
                            }
                            else if (k == Keys.Right)
                            {
                                IncrementMarker();
                            }
                            else
                            {
                                string input = InputManager.GetCharFromKey(k);
                                if (!string.IsNullOrEmpty(input)) {
                                    if (input == "--")
                                    {
                                        if (_gText.Length > 0)
                                        {
                                            _gText.Remove(_iCurr);
                                            DecrementMarker();
                                        }
                                    }
                                    else if (input == "-+")
                                    {
                                        if (_gText.Length > 0)
                                        {
                                            _gText.Remove(_iCurr + 1);
                                        }
                                    }
                                    else if (_gText.Length < _iMaxLength)
                                    {
                                        _gText.Insert(input, _iCurr);
                                        IncrementMarker();
                                    }
                                }

                                _gMarker.Position(_gText.Position());
                                if (_gText.Text.Length > 0)
                                {
                                    _gMarker.PositionAdd(new Vector2(_gText.MeasureString(_iCurr).X, 0));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void Activate(bool value = true)
        {
            _bTakeInput = value;
            if (!value) { HideCursor(); }
        }

        public void IncrementMarker()
        {
            if (_iCurr < _iMaxLength)
            {
                _iCurr++;
            }
        }

        public void DecrementMarker()
        {
            if (_iCurr > 0)
            {
                _iCurr--;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Show())
            {
                DrawWindow(spriteBatch);

                foreach (GUIObject g in Controls)
                {
                    g.Draw(spriteBatch);
                }
            }
        }

        public string GetText()
        {
            return _gText.GetText();
        }

        public void HideCursor()
        {
            _gMarker.Hide();
        }

        protected class GUIMarker : GUIText
        {
            readonly double DBL_FLASH_RATE = 0.5;

            bool _bShow;
            double _dRefresh;

            public GUIMarker()
            {
                _sText = "|";
                SetDimensions(_sText);
                _dRefresh = DBL_FLASH_RATE;
            }

            public override void Update(GameTime gTime)
            {
                _dRefresh -= gTime.ElapsedGameTime.TotalSeconds;
                if (_dRefresh <= 0)
                {
                    _dRefresh = DBL_FLASH_RATE;
                    _bShow = !_bShow;
                }
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                if (_bShow)
                {
                    base.Draw(spriteBatch);
                }
            }

            public void Hide()
            {
                _bShow = false;
            }
        }
    }
}
