using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Windows.Input;
using Microsoft.Xna.Framework.Graphics;
using Adventure.Characters.NPCs;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects.GUIWindows
{
    class GUITextInputWindow : GUITextWindow
    {
        private int _strLen;
        private string _statement;
        private int _maxLength = 10;
        private Worker _w;

        public GUITextInputWindow(Worker w) : base()
        {
            RiverHollow.ChangeGameState(RiverHollow.GameState.Input);
            _statement = "Enter name:";
            _width = Math.Max((int)_font.MeasureString(_statement).X, (int)_characterWidth * 10) + _innerBorder * 2;
            _height = (int)_characterHeight * 2 + _innerBorder * 2;
            _position = new Vector2(RiverHollow.ScreenWidth / 2 - _width / 2, RiverHollow.ScreenHeight / 2 - _height / 2);
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
                            RiverHollow.ChangeGameState(RiverHollow.GameState.WorldMap);
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
            spriteBatch.DrawString(_font, _statement, new Vector2(_position.X + _innerBorder, _position.Y + _innerBorder), Color.White);
            spriteBatch.DrawString(_font, _text, new Vector2(_position.X + _innerBorder, _position.Y + _innerBorder + 28), Color.White);
        }
    }
}
