using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIText : GUIObject
    {
        enum LastCharacter { Even, Odd, Space };
        LastCharacter _eCurrChar;
        const string TYPE_SOUND = "Sample";

        protected string _sText;
        protected string _sFullText;
        public string Text => _sText;
        protected BitmapFont _font;

        protected Vector2 _vTextSize;
        public Vector2 TextSize => _vTextSize;
        protected Vector2 _vCharSize;
        public Vector2 CharacterSize => _vCharSize;
        public int CharWidth => (int)_vCharSize.X;
        public int CharHeight => (int)_vCharSize.Y;

        public int Length => _sText.Length;

        #region Parsing and Display
        double _dTextTimer = 0;
        double _dTypedTextLen;
        const double TEXT_DELAY = 0.06;
        protected int _iMaxRows = 3;

        public bool PrintAll = false;
        bool _bDone = false;
        public bool Done => _bDone;
        #endregion

        public GUIText()
        {
            _sText = "";
            _font = DataManager.GetBitMapFont(DataManager.FONT_MAIN);
            SetDimensions("X");
        }

        public GUIText(int val, bool printAll = true, string f = DataManager.FONT_MAIN) : this(val.ToString(), printAll, f){}

        public GUIText(string text, bool printAll = true, string f = DataManager.FONT_MAIN) : this()
        {
            _font = DataManager.GetBitMapFont(f);
            PrintAll = printAll;

            if (!printAll) { _sFullText = text; }
            else {
                _sText = text;
                _sFullText = _sText;
            }

            SetDimensions(text);
        }

        public GUIText(string text, BitmapFont font) : this(text)
        {
            _font = font;
        }

        protected void SetDimensions(string val)
        {
            _vCharSize = _font.MeasureString("X");
            _vTextSize = _font.MeasureString(val);
            Width = (int)_vTextSize.X;
            Height = (int)_vTextSize.Y;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(_sText))
            {
                spriteBatch.DrawString(_font, _sText, Position(), _cColor * Alpha());
            }
        }

        public void Draw(SpriteBatch spriteBatch, string newText)
        {
            _sText = newText;
            _vTextSize = _font.MeasureString(_sText);

            Width = (int)_vTextSize.X;
            Height = (int)_vTextSize.Y;

            this.Draw(spriteBatch);
        }

        public Vector2 MeasureString()
        {
            return _font.MeasureString(_sText);
        }
        public Vector2 MeasureString(int toLoc)
        {
            return _font.MeasureString(_sText.Substring(0, toLoc));
        }
        public Vector2 MeasureString(string s)
        {
            return _font.MeasureString(s);
        }

        public void SetText(string text, bool changePos = false)
        {
            _sText = text;
            _sFullText = _sText;
            SetDimensions(text);
        }

        public void SetText(int num, bool changePos = false)
        {
            SetText(num.ToString(), changePos);
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
                        if (_dTextTimer < TEXT_DELAY)
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
                                SoundManager.PlayEffect(TYPE_SOUND);
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

        public void ParseText(int maxRows, int width, bool printAll = true)
        {
            int numReturns = 0;
            string totalText = string.Empty;
            string line = string.Empty;
            string returnString = string.Empty;
            string[] wordArray = _sFullText.Split(' ');

            foreach (string word in wordArray)
            {
                Vector2 measure = MeasureString(line + word);

                if (measure.Length() >= (width) ||
                    numReturns == maxRows - 1 && measure.Length() >= (width) - CharHeight)
                {
                    returnString = returnString + line + '\n';
                    totalText += returnString;
                    line = string.Empty;
                    numReturns++;
                }

                line = line + word + ' ';
            }

            SetText(returnString + line);
        }
    }
}
