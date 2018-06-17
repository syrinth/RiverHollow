using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters.NPCs;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    class GUITextInputWindow : GUITextWindow
    {
        
        int _iMaxLength = 10;
        WorldAdventurer _w;
        WorkerBuilding _b;
        SideEnum _textLoc;
        GUIText _gStatement;
        GUIText _gText;
        GUIMarker _gMarker;

        int _iCurr;


        public GUITextInputWindow() : base()
        {
            GameManager.ReadInput();
            GameManager.Pause();

            _gMarker = new GUIMarker();
            _iCurr = 0;
        }

        public GUITextInputWindow(string statement, SideEnum textLoc) : this()
        {
            StatementSetup(statement, textLoc);
            _textLoc = textLoc;
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
        }

        public GUITextInputWindow(WorldAdventurer w) : this()
        {
            _textLoc = SideEnum.Top;
            StatementSetup("Enter name:");
            Width = Math.Max(_gStatement.Width, _gStatement.CharWidth * 10);
            Height = _gStatement.CharHeight * 2;
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
            _w = w;
        }

        public GUITextInputWindow(WorkerBuilding b) : this()
        {
            _textLoc = SideEnum.Top;
            StatementSetup("Name Building:");
            Width = Math.Max(_gStatement.Width, _gStatement.CharWidth * 10);
            Height = _gStatement.CharHeight * 2;
            Position(new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2));
            _b = b;
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

            _gStatement.AnchorToInnerSide(this, SideEnum.TopLeft);
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

            Resize();
        }

        public override void Update(GameTime gameTime)
        {
            _gMarker.Update(gameTime);
            foreach (Keys k in InputManager.KeyDownDictionary.Keys.ToList())
            {
                if (InputManager.CheckPressedKey(k))
                {
                    if (k == Keys.Enter)
                    {
                        bool finished = false;
                        if (_w != null)
                        {
                            finished = true;
                            _w.SetName(_gText.GetText());
                        }
                        if (_b != null)
                        {
                            finished = true;
                            _b.SetName(_gText.GetText());  
                        }

                        if(finished){
                            RiverHollow.ResetCamera();
                            GameManager.Unpause();
                            GameManager.Scry(false);
                            GameManager.DontReadInput();
                        }

                    }
                    else if(k >= Keys.A && k <= Keys.Z || k == Keys.Delete || k == Keys.Back || k == Keys.Left || k == Keys.Right || k == Keys.Delete)
                    {
                        if (k == Keys.Left)
                        {
                            DecrementMarker();
                        }
                        else if(k == Keys.Right)
                        {
                            IncrementMarker();
                        }
                        else
                        {
                            string input = InputManager.GetCharFromKey(k);
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
                                    _gText.Remove(_iCurr+1);
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
            base.Draw(spriteBatch);
        }

        public string GetText()
        {
            return _gText.GetText();
        }

        public void HideCursor()
        {
            _gMarker.Hide();
        }

        private class GUIMarker : GUIText
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

            public override void Update(GameTime gameTime)
            {
                _dRefresh -= gameTime.ElapsedGameTime.TotalSeconds;
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
