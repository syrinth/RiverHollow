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
    class GUITextInputWindow : GUIWindow
    {
        private int _strLen;
        private string _statement;
        private string _text;
        private int _maxLength = 10;
        protected SpriteFont _font;
        private Worker _w;

        public GUITextInputWindow(Worker w) : base()
        {
            _statement = "Enter name:";
            _width = 164;
            _height = 92;
            _position = new Vector2(AdventureGame.ScreenWidth / 2 - _width / 2, AdventureGame.ScreenHeight / 2 - _height / 2);
            _strLen = 0;
            _w = w;
            _text = string.Empty;
            _font = GameContentManager.GetFont(@"Fonts\Font");

            Load(new Vector2(0, 0), 32);
        }

        public GUITextInputWindow(ref string text): base()
        {
            _strLen = 0;
            _text = text;
            _font = GameContentManager.GetFont(@"Fonts\Font");
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
                            AdventureGame.ResetCamera();
                            _w.SetName(_text);
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
            spriteBatch.DrawString(_font, _statement, new Vector2(_position.X + 16, _position.Y + 16), Color.White);
            spriteBatch.DrawString(_font, _text, new Vector2(_position.X + 16, _position.Y + 16+28), Color.White);
        }
    }
}
