using Adventure.Game_Managers.GUIObjects;
using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIDialog : GUIObject
    {
        private SpriteFont _font;
        private GUIImage[] _ninePatches;
        private int _midWidth;
        private int _midHeight;
        private string _text;

        string parsedText;
        string typedText = string.Empty;
        double typedTextLength;
        int delayInMilliseconds;
        private bool isDoneDrawing = false;

        public GUIDialog(string text)
        {
            _position = new Vector2(AdventureGame.ScreenWidth/4, AdventureGame.ScreenHeight-180);
            _width = AdventureGame.ScreenWidth / 2;
            _ninePatches = new GUIImage[9];
            _text = text;
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
            _font = GameContentManager.GetFont(@"Fonts\Font");

            _height = 148;
            _midWidth = _width - 64;
            _midHeight = _height - 64;
            delayInMilliseconds = 50;

            parsedText = parseText(text);
        }
        public GUIDialog(Vector2 position, string text) : this(text)
        {
            _position = position;
        }

        public override void Update(GameTime gameTime)
        {
            if (!isDoneDrawing)
            {
                if (delayInMilliseconds == 0)
                {
                    typedText = parsedText;
                    isDoneDrawing = true;
                }
                else if (typedTextLength < parsedText.Length)
                {
                    typedTextLength = typedTextLength + gameTime.ElapsedGameTime.TotalMilliseconds / delayInMilliseconds;

                    if (typedTextLength >= parsedText.Length)
                    {
                        typedTextLength = parsedText.Length;
                        isDoneDrawing = true;
                    }

                    typedText = parsedText.Substring(0, (int)typedTextLength);
                }
            }
        }

        private string parseText(string text)
        {
            string line = string.Empty;
            string returnString = string.Empty;
            string[] wordArray = text.Split(' ');

            foreach (string word in wordArray)
            {
                Vector2 measure = _font.MeasureString(line + word);
                if (measure.Length() > _width-32)
                {
                    returnString = returnString + line + '\n';
                    line = string.Empty;
                }

                line = line + word + ' ';
            }

            return returnString + line;
        }

        public override void Draw(SpriteBatch spritebatch)
        {
            {
                spritebatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, 32, 32), new Rectangle(0, 0, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + 32, (int)_position.Y, _midWidth, 32), new Rectangle(32, 0, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + 32, (int)_position.Y, 32, 32), new Rectangle(64, 0, 32, 32), Color.White);

                spritebatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y + 32, 32, _midHeight), new Rectangle(0, 32, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + 32, (int)_position.Y + 32, _midWidth, _midHeight), new Rectangle(32, 32, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + 32, (int)_position.Y + 32, 32, _midHeight), new Rectangle(64, 32, 32, 32), Color.White);

                spritebatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y + _midHeight + 32, 32, 32), new Rectangle(0, 64, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + 32, (int)_position.Y + _midHeight + 32, _midWidth, 32), new Rectangle(32, 64, 32, 32), Color.White);
                spritebatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + 32, (int)_position.Y + _midHeight + 32, 32, 32), new Rectangle(64, 64, 32, 32), Color.White);
            }
            spritebatch.DrawString(_font, typedText, new Vector2(_position.X+16, _position.Y+16), Color.White);
        }
    }
}
