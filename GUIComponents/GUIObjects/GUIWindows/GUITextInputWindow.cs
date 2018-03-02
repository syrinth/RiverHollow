using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Characters.NPCs;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    class GUITextInputWindow : GUITextWindow
    {
        private int _strLen;
        private string _statement;
        private int _maxLength = 10;
        private WorldAdventurer _w;

        public GUITextInputWindow(string statement) : base()
        {
            GameManager.Pause();
            _statement = statement;
            Width = Math.Max((int)_font.MeasureString(_statement).X, (int)_characterWidth * 10) + _innerBorder * 2;
            Height = (int)_characterHeight * 2 + _innerBorder * 2;
            Position = new Vector2(RiverHollow.ScreenWidth / 2 - Width / 2, RiverHollow.ScreenHeight / 2 - Height / 2);
            _strLen = 0;
            _text = string.Empty;
        }

        public GUITextInputWindow(WorldAdventurer w) : base()
        {
            GameManager.Pause();
            _statement = "Enter name:";
            Width = Math.Max((int)_font.MeasureString(_statement).X, (int)_characterWidth * 10) + _innerBorder * 2;
            Height = (int)_characterHeight * 2 + _innerBorder * 2;
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
            spriteBatch.DrawString(_font, _statement, new Vector2(Position.X + _innerBorder, Position.Y + _innerBorder), Color.White);
            spriteBatch.DrawString(_font, _text, new Vector2(Position.X + _innerBorder, Position.Y + _innerBorder + 28), Color.White);
        }

        public string GetText()
        {
            return _text;
        }
    }
}
