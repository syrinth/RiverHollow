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

        string typedText = string.Empty;
        double typedTextLength;
        int delayInMilliseconds;
        private bool isDoneDrawing = false;
        List<string> _parsedStrings;
        bool finishedScreen = false;

        int _currentParsedString = 0;
        public bool _pause = false;

        public GUIDialog(string text)
        {
            _position = new Vector2(AdventureGame.ScreenWidth/4, AdventureGame.ScreenHeight-180);
            _width = AdventureGame.ScreenWidth / 2;
            _ninePatches = new GUIImage[9];
            _text = text;
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _parsedStrings = new List<string>();

            _height = 148;
            _midWidth = _width - 64;
            _midHeight = _height - 64;
            delayInMilliseconds = 50;

            parseText(text);
        }
        public GUIDialog(Vector2 position, string text) : this(text)
        {
            _position = position;
        }

        public override void Update(GameTime gameTime)
        {
            if (!isDoneDrawing && !_pause)
            {
                if (finishedScreen)
                {
                    finishedScreen = false;
                    _currentParsedString++;
                    typedText = string.Empty;
                    typedTextLength = 0;

                    if (_currentParsedString == _parsedStrings.Count)
                    {
                        _currentParsedString--;
                        typedTextLength = _parsedStrings[_currentParsedString].Length;
                        isDoneDrawing = true;
                    }
                }
                if (delayInMilliseconds == 0)
                {
                    _currentParsedString++;
                    typedText = string.Empty;
                    delayInMilliseconds = 50;
                    if (_currentParsedString == _parsedStrings.Count)
                    {
                        isDoneDrawing = true;
                    }
                }
                else if (typedTextLength < _parsedStrings[_currentParsedString].Length)
                {
                    typedTextLength = typedTextLength + gameTime.ElapsedGameTime.TotalMilliseconds / delayInMilliseconds;

                    if (typedTextLength >= _parsedStrings[_currentParsedString].Length)
                    {
                        _pause = true;
                        finishedScreen = true;
                    }

                    typedText = _parsedStrings[_currentParsedString].Substring(0, (int)typedTextLength);
                }
            }
        }

        private void parseText(string text)
        {
            bool grabLast = true;
            int numReturns = 0;
            string line = string.Empty;
            string returnString = string.Empty;
            string[] wordArray = text.Split(' ');

            foreach (string word in wordArray)
            {
                Vector2 measure = _font.MeasureString(line + word);

                if (measure.Length() > _midWidth)
                {
                    returnString = returnString + line + '\n';
                    line = string.Empty;
                    numReturns++;
                }

                grabLast = true;
                line = line + word + ' ';

                if (measure.Y * numReturns > _midHeight)
                {
                    grabLast = false;
                    _parsedStrings.Add(returnString);
                    numReturns = 0;
                    returnString = string.Empty;
                }
            }

            if (grabLast)
            {
                _parsedStrings.Add(returnString + line);
            }
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
