using RiverHollow.Characters;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects
{
    public class GUITextWindow : GUIWindow
    {
        private GUIImage _next;
        protected SpriteFont _font;
        protected string _text;
        public double Duration;

        protected const int _maxRows = 3;
        protected float _characterWidth;
        protected float _characterHeight;

        protected string _typedText = string.Empty;
        double typedTextLength;
        int delayInMilliseconds = 10;
        public bool printAll = false;
        private bool _doneDrawing = false;
        public bool Done { get => _doneDrawing; }
        protected List<string> _parsedStrings;
        bool finishedScreen = false;

        protected int _currentParsedString = 0;
        public bool _pause = false;
        protected int _numReturns = 0;

        protected GUITextWindow() : base()
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _characterHeight = _font.MeasureString("Q").Y;
            _characterWidth = _font.MeasureString("_").X;
            _parsedStrings = new List<string>();
        }

        public GUITextWindow(NPC npc, string text) : this()
        {
            _text = text;
            Height = Math.Max(Height, ((int)_characterHeight * _maxRows));
            _next = new GUIImage(Vector2.Zero, new Rectangle(288, 64, 32, 32), (int)_characterHeight, (int)_characterHeight, @"Textures\Dialog");
            _next.AnchorToInnerSide(this, SideEnum.BottomRight);

            ParseText(text);
            GameManager.gmNPC = npc;
        }

        public GUITextWindow(Vector2 position, string text) : this()
        {
            _text = text;
            ParseText(_text);
            Vector2 vMeasure = _font.MeasureString(_text);

            Height = (int)vMeasure.Y;
            Width = (int)vMeasure.X;
            Position(PositionSub(new Vector2(Width / 2, Height / 2)));
        }

        public GUITextWindow(string text) : this()
        {
            _text = text;
            ParseText(text);

            Height = (int)_parsedStrings.Count * (int)_characterHeight;
            Width = (int)_font.MeasureString(_text).X;
            Vector2 pos = new Vector2(RiverHollow.ScreenWidth/2, RiverHollow.ScreenHeight/2);
            Position(PositionSub(new Vector2(Width / 2, Height / 2)));
        }

        public GUITextWindow(string text, double duration) : this()
        {
            _text = text;
            Duration = duration;
            ParseText(text);

            Height = (int)_parsedStrings.Count * (int)_characterHeight;
            Width = (int)_font.MeasureString(_text).X;
            Vector2 pos = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
            Position(PositionSub(new Vector2(Width / 2, Height / 2)));
        }

        public override void Update(GameTime gameTime)
        {
            if (_pause && _next != null)
            {
                _next.Update(gameTime);
            }

            if(Duration > 0) { Duration -= gameTime.ElapsedGameTime.TotalSeconds; }
            if (!printAll)
            {
                if (!_doneDrawing && !_pause)
                {
                    if (finishedScreen)
                    {
                        finishedScreen = false;
                        _currentParsedString++;
                        _typedText = string.Empty;
                        typedTextLength = 0;

                        if (_currentParsedString == _parsedStrings.Count)
                        {
                            _currentParsedString--;
                            typedTextLength = _parsedStrings[_currentParsedString].Length;
                            _doneDrawing = true;
                        }
                    }
                    if (delayInMilliseconds == 0)
                    {
                        _currentParsedString++;
                        _typedText = string.Empty;
                        delayInMilliseconds = 10;
                        if (_currentParsedString == _parsedStrings.Count)
                        {
                            _doneDrawing = true;
                        }
                    }
                    else if (typedTextLength < _parsedStrings[_currentParsedString].Length)
                    {
                        typedTextLength = (int)(typedTextLength + gameTime.ElapsedGameTime.TotalMilliseconds / delayInMilliseconds);

                        if (typedTextLength >= _parsedStrings[_currentParsedString].Length)
                        {
                            _pause = true;
                            finishedScreen = true;
                        }

                        _typedText = _parsedStrings[_currentParsedString].Substring(0, (int)typedTextLength);
                    }
                }
            }
            else
            {
                _typedText = _parsedStrings[_currentParsedString++];
                printAll = false;
                _pause = true;
                if (_currentParsedString == _parsedStrings.Count)
                {
                    _doneDrawing = true;
                }
            }
        }

        public void Unpause()
        {
            _pause = false;
            typedTextLength = 0;
        }

        protected void ParseText(string text)
        {
            bool grabLast = true;
            _numReturns = 0;
            string line = string.Empty;
            string returnString = string.Empty;
            string[] wordArray = text.Split(' ');

            foreach (string word in wordArray)
            {
                Vector2 measure = _font.MeasureString(line + word);

                if (measure.Length() >= (Width) ||
                    _numReturns == _maxRows-1 && measure.Length() >= (Width) - _characterHeight)
                {
                    returnString = returnString + line + '\n';
                    line = string.Empty;
                    _numReturns++;
                }

                grabLast = true;
                line = line + word + ' ';

                //Spillover to another screen
                if (measure.Y * _numReturns >= (Height))
                {
                    grabLast = false;
                    _parsedStrings.Add(returnString);
                    _numReturns = 0;
                    returnString = string.Empty;
                }
            }

            if (grabLast)
            {
                _parsedStrings.Add(returnString + line);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, false);
        }

        public void Draw(SpriteBatch spriteBatch, bool force)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(_font, force ? _text : _typedText, new Vector2(Position().X, Position().Y), Color.White);
            if (_pause && _next != null)
            {
                _next.Draw(spriteBatch);
            }

            if (GameManager.gmNPC != null)
            {
                GameManager.gmNPC.DrawPortrait(spriteBatch, new Vector2(InnerTopLeft().X, InnerTopLeft().Y - EdgeSize));
            }
        }
    }
}
