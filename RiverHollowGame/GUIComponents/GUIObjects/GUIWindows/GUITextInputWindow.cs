using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUITextInputWindow : GUITextWindow
    {
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
            _iCurr = 0;
            _iMaxLength = maxLength != -1 ? maxLength : GameManager.MAX_NAME_LEN;

            _gText = new GUIText("");
            _gMarker = new GUIMarker();

            _gText.AnchorToInnerSide(this, SideEnum.TopLeft);
            _gMarker.Position(_gText.Position());

            AddControl(_gText);
            AddControl(_gMarker);

            Width = GameManager.ScaleIt(80);
            Height = GameManager.ScaleIt(21);
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

                                UpdateMarkerPosition();
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
        public void SetText(string text)
        {
            _iCurr = text.Length;
            _gText.SetText(text);
            UpdateMarkerPosition();
        }

        public void UpdateMarkerPosition()
        {
            _gText.AnchorToInnerSide(this, SideEnum.Top);
            _gMarker.Position(_gText.Position());
            if (_gText.Text.Length > 0)
            {
                _gMarker.PositionAdd(new Point((int)_gText.MeasureString(_iCurr).X, 0));
            }
        }

        public void HideCursor()
        {
            _gMarker.Hide();
        }

        protected class GUIMarker : GUIText
        {
            

            bool _bShow;
            RHTimer _timer;

            public GUIMarker()
            {
                _sText = "|";
                SetDimensions(_sText);
                _timer = new RHTimer(Constants.GUI_TEXT_MARKER_FLASH_RATE);
            }

            public override void Update(GameTime gTime)
            {
                if (_timer.TickDown(gTime, true))
                {
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
