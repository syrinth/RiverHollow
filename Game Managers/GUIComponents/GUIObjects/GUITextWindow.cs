using Adventure.Game_Managers.GUIObjects;
using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects
{
    public class GUITextWindow : GUIObject
    {
        private GUIWindow _window;
        private SpriteFont _font;
        private string _text;

        string typedText = string.Empty;
        double typedTextLength;
        int delayInMilliseconds;
        private bool isDoneDrawing = false;
        List<string> _parsedStrings;
        bool finishedScreen = false;

        int _currentParsedString = 0;
        public bool _pause = false;

        public GUITextWindow(string text)
        {
            _window = new GUIWindow();
            _position = _window.Position;
            _width = _window.Width;

            _text = text;
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _parsedStrings = new List<string>();

            _height = 148;
            delayInMilliseconds = 50;

            parseText(text);
        }
        public GUITextWindow(Vector2 position, string text) : this(text)
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

                if (measure.Length() > _window.MiddleWidth)
                {
                    returnString = returnString + line + '\n';
                    line = string.Empty;
                    numReturns++;
                }

                grabLast = true;
                line = line + word + ' ';

                if (measure.Y * numReturns > _window.MiddleHeight)
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
            _window.Draw(spritebatch);
            spritebatch.DrawString(_font, typedText, new Vector2(_position.X+16, _position.Y+16), Color.White);
        }
    }
}
