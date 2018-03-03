using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.NPCs;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    class GUITextInputWindow : GUITextWindow
    {
        int _strLen;
        string _statement;
        int _maxLength = 10;
        WorldAdventurer _w;
        SideEnum _textLoc;

        public GUITextInputWindow(string statement) : base()
        {
            GameManager.Pause();
            _statement = statement;
            Width = Math.Max((int)_font.MeasureString(_statement).X, (int)_characterWidth * 10) + _iInnerBorder * 2;
            Height = (int)_characterHeight * 2 + _iInnerBorder * 2;
            Position = new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2);
            _strLen = 0;
            _text = string.Empty;
        }

        public GUITextInputWindow(string statement, SideEnum textLoc) : base()
        {
            GameManager.Pause();
            _statement = statement;
            _textLoc = textLoc;
            if (_textLoc == SideEnum.Top)
            {
                Width = Math.Max((int)_font.MeasureString(_statement).X, (int)_characterWidth * 10) + _iInnerBorder * 2;
                Height = (int)_characterHeight * 2 + _iInnerBorder * 2;
            }
            else if (_textLoc == SideEnum.Left)
            {
                Width = (int)_font.MeasureString(_statement).X + (int)_characterWidth * 10 + _iInnerBorder * 2;
                Height = (int)_characterHeight + _iInnerBorder * 2;
            }
            Position = new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2);
            _strLen = 0;
            _text = string.Empty;
        }

        public GUITextInputWindow(WorldAdventurer w) : base()
        {
            GameManager.Pause();
            _statement = "Enter name:";
            Width = Math.Max((int)_font.MeasureString(_statement).X, (int)_characterWidth * 10) + _iInnerBorder * 2;
            Height = (int)_characterHeight * 2 + _iInnerBorder * 2;
            Position = new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2);
            _strLen = 0;
            _w = w;
            _text = string.Empty;
        }

        public GUITextInputWindow(ref string text): base()
        {
            _strLen = 0;
            _text = text;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (Keys k in InputManager.KeyDownDictionary.Keys.ToList())
            {
                if (InputManager.CheckKey(k))
                {
                    if (k == Keys.Enter)
                    {
                        if (_w != null)
                        {
                            RiverHollow.ResetCamera();
                            _w.SetName(_text);
                            GameManager.Unpause();
                        }
                    }
                    else
                    {
                        string input = InputManager.GetCharFromKey(k);
                        if (input != "--" && _strLen < _maxLength)
                        {
                            _text = _text.Insert(_strLen++, input);
                        }
                        else if (input == "--")
                        {
                            if (_text.Length > 0)
                            {
                                _text = _text.Remove(--_strLen);
                            }
                        }
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            int posX = (int)Position.X;
            Vector2 stringLen = _font.MeasureString(_statement+"X");
            spriteBatch.DrawString(_font, _statement, new Vector2(posX + _iInnerBorder, Position.Y + _iInnerBorder), Color.White);

            int startX = (_textLoc == SideEnum.Top) ? (posX + _iInnerBorder) : posX + (int)stringLen.X;
            int addition = (_textLoc == SideEnum.Top) ? 28 : 0;
            spriteBatch.DrawString(_font, _text, new Vector2(startX, Position.Y + _iInnerBorder + addition), Color.White);
        }

        public string GetText()
        {
            return _text;
        }
    }
}
