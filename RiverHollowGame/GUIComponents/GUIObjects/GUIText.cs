using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIText : GUIObject
    {
        enum LastCharacter { Even, Odd, Space };
        LastCharacter _eCurrChar;

        protected string _sText;
        protected string _sFullText;
        public string Text => _sText;
        protected string _sFont;
        protected Color _cShadowColor = Color.White;

        protected Vector2 _vTextSize;
        protected Vector2 _vCharSize;
        public int CharWidth => (int)_vCharSize.X;
        public int CharHeight => (int)_vCharSize.Y;

        public int Length => _sText.Length;

        #region Parsing and Display
        double _dTextTimer = 0;
        double _dTypedTextLen;
        protected int _iMaxRows = 3;

        public bool PrintAll = false;
        bool _bDone = false;
        public bool Done => _bDone;
        #endregion

        public GUIText(bool printAll = true)
        {
            _Color = Color.White;
            PrintAll = printAll;
            _sText = "";
            _sFullText = _sText;

            _sFont = DataManager.FONT_MAIN;
            SetShadowTextDefault();

            SetDimensions("X");
        }

        public GUIText(int val, bool printAll = true, string f = DataManager.FONT_MAIN) : this(val.ToString(), printAll, f) {}

        public GUIText(string text, bool printAll = true, string f = DataManager.FONT_MAIN) : this()
        {
            _sFont = f;
            SetShadowTextDefault();

            PrintAll = printAll;

            if (printAll) { 
                _sText = text;
            }
            _sFullText = text;

            SetDimensions(text);
        }

        private void SetShadowTextDefault()
        {
            _cShadowColor = _sFont == DataManager.FONT_MAIN ? Color.Black : Color.White;
        }

        private BitmapFont Font()
        {
            return DataManager.GetBitMapFont(_sFont);
        }
        public Vector2 MeasureSubstring(int toLoc)
        {
            return MeasureString(_sText.Substring(0, toLoc));
        }

        public Vector2 MeasureString(string val)
        {
            var v = Font().MeasureString(val);
            return new Vector2(v.Width * GameManager.CurrentScale, v.Height * GameManager.CurrentScale);
        }

        protected void SetDimensions(string val)
        {
            _vCharSize = MeasureString("X");
            _vTextSize = MeasureString(val);
            Width = (int)_vTextSize.X;
            Height = (int)_vTextSize.Y;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(_sText) && Show())
            {
                spriteBatch.DrawString(Font(), _sText, Position().ToVector2(), _Color * Alpha(), 0, Vector2.Zero, GameManager.CurrentScale, SpriteEffects.None, 0);

                if (_cShadowColor != Color.White) {
                    spriteBatch.DrawString(DataManager.GetBitMapFont(DataManager.FONT_MAIN_DROPSHADOW), _sText, Position().ToVector2(), _cShadowColor, 0, Vector2.Zero, GameManager.CurrentScale, SpriteEffects.None, 0);
                }
            }
        }

        public void SetTextColors(Color mainColor, Color shadowColor)
        {
            _Color = mainColor;
            _cShadowColor = shadowColor;
        }

        public void SetText(string text)
        {
            _sText = text;
            _sFullText = _sText;
            SetDimensions(text);
        }
        public void ParseAndSetText(string text, int width, int maxRows, bool printAll = false, bool changePos = false)
        {
            SetText(ParseText(text, width, maxRows, printAll)[0]);
        }

        public void SetText(int num)
        {
            SetText(num.ToString());
        }
        public void ResetText(string text)
        {
            _sFullText = text;
            _sText = string.Empty;
            _bDone = false;
            _dTypedTextLen = 0;
            SetDimensions(text);
        }
        public string GetText()
        {
            return _sText;
        }

        public void Insert(string s, int loc)
        {
            if (_sText.Length > 0)
            {
                _sText = _sText.Insert(loc, s);
            }
            else { _sText = s; }
            SetDimensions(_sText);
        }

        public void Remove(int loc)
        {
            if (_sText.Length > 0)
            {
                _sText = _sText.Remove(loc - 1, 1);
                SetDimensions(_sText);
            }
        }

        public override void Update(GameTime gTime)
        {
            if (!PrintAll)
            {
                if (!_bDone)
                {
                    if (_dTypedTextLen < _sFullText.Length)
                    {
                        if (_dTextTimer < Constants.GUI_TEXT_DELAY)
                        {
                            _dTextTimer += gTime.ElapsedGameTime.TotalSeconds;
                        }
                        else
                        {
                            _dTextTimer = 0;
                            _dTypedTextLen++;

                            if (_dTypedTextLen == _sFullText.Length) { _bDone = true; }

                            _sText = _sFullText.Substring(0, (int)_dTypedTextLen);

                            char currChar = _sText[_sText.Length - 1];
                            if (currChar.Equals(' ')) { _eCurrChar = LastCharacter.Space; }
                            else if (currChar.Equals(',')) { _dTextTimer -= 0.2; }
                            else if (currChar.Equals('!')) { _dTextTimer -= 0.8; }
                            else if (currChar.Equals('.') || currChar.Equals('?')) {
                                _eCurrChar = LastCharacter.Even;
                                _dTextTimer -= 0.5;
                            }

                            if (_eCurrChar == LastCharacter.Space) { _eCurrChar = LastCharacter.Even; }
                            else if (_eCurrChar == LastCharacter.Odd) { _eCurrChar = LastCharacter.Even; }
                            else if (_eCurrChar == LastCharacter.Even)
                            {
                                SoundManager.PlayEffect(SoundEffectEnum.Text);
                                _eCurrChar = LastCharacter.Odd;
                            }
                        }
                    }
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(_sFullText) && _sText != _sFullText)
                {
                    _sText = _sFullText;
                }
                PrintAll = false;
                _bDone = true;
            }
        }

        /// <summary>
        /// Iterates over the given text word by word to create a list of text entries that will
        /// be on each screen. These text entries will have /n entries manually inserted to properly
        /// display based off of the GUITextWindow dimensions.
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="printAll">Whether we will print everything at once</param>
        public List<string> ParseText(string text, int width, int maxRows, bool printAll = true)
        {
            PrintAll = printAll;
            List<string> textPages = new List<string>();
            bool grabLast = true;
            int numReturns = 0;
            string currentLineOfText = string.Empty;
            string textToDisplay = string.Empty;
            string[] wordArray = text.Split(' ');   //Split the given entry around each word. Note that it is important that /n be its own word

            foreach (string word in wordArray)
            {
                //If there is a new line character in the word, prepare the text dialogue for the next screen.
                if (word.Contains("\n") || word.Contains("\r\n"))
                {
                    string[] returnSplit = word.Split(new string[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                    if (returnSplit.Length > 0)
                    {
                        CompareStringLength(ref currentLineOfText, ref textToDisplay, ref numReturns, returnSplit[0], width, maxRows);
                        textToDisplay += currentLineOfText + returnSplit[0];

                        TextSpilloverToNextScreen(ref textToDisplay, ref grabLast, ref numReturns, ref textPages);

                        if (returnSplit.Length > 1)
                        {
                            currentLineOfText = returnSplit[1] + ' ';
                        }
                    }
                }
                else
                {
                    CompareStringLength(ref currentLineOfText, ref textToDisplay, ref numReturns, word, width, maxRows);

                    grabLast = true;
                    currentLineOfText += word + ' ';

                    //Spill over to another screen when we have too many returns
                    if (numReturns + 1 > maxRows)
                    {
                        TextSpilloverToNextScreen(ref textToDisplay, ref grabLast, ref numReturns, ref textPages);
                    }
                }
            }

            if (grabLast)
            {
                textPages.Add(textToDisplay + currentLineOfText);
            }

            return textPages;
        }

        private bool CompareStringLength(ref string currentLineOfText, ref string textToDisplay, ref int numReturns, string word, int width, int maxRows)
        {
            bool rv = false;
            Vector2 vMeasurement = MeasureString(currentLineOfText + word);

            //Measure the current line and the new word and see if adding the word will put the line out of bounds.
            //If so, we need to insert a carriage return character and clear the current line of text.

            bool checkOne = (vMeasurement.X >= width);
            if (checkOne)
            {
                textToDisplay += currentLineOfText + '\n';
                currentLineOfText = string.Empty;
                numReturns++;
                rv = true;
            }

            return rv;
        }

        private void TextSpilloverToNextScreen(ref string textToDisplay, ref bool grabLast, ref int numReturns, ref List<string> textPages)
        {
            grabLast = false;
            textPages.Add(textToDisplay);
            numReturns = 0;
            textToDisplay = string.Empty;
        }
    }
}
