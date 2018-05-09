using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;

namespace RiverHollow.GUIComponents.GUIObjects
{
    class GUIText : GUIObject
    {
        string _sText;
        public string Text => _sText;
        SpriteFont _font;
        Color _cTextColor;

        Vector2 _vTextSize;

        public int Length => _sText.Length;
        public GUIText()
        {
            _sText = "";
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _cTextColor = Color.Red;
            SetDimensions("X");
        }

        public GUIText(string text, string f = @"Fonts\Font") : this()
        {
            _font = GameContentManager.GetFont(f);
            _sText = text;

            SetDimensions(text);
        }

        public GUIText(string text, SpriteFont font) : this(text)
        {
            _font = font;
        }

        private void SetDimensions(string val)
        {
            _vTextSize = _font.MeasureString(val);
            Width = (int)_vTextSize.X;
            Height = (int)_vTextSize.Y;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!string.IsNullOrEmpty(_sText))
            {
                spriteBatch.DrawString(_font, _sText, Position(), _cTextColor);
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
        public Vector2 MeasureString(string s)
        {
            return _font.MeasureString(s);
        }

        public void SetText(string text, bool changePos = false)
        {
            _sText = text;
        }
        public string GetText()
        {
            return _sText;
        }

        public void SetColor(Color c)
        {
            _cTextColor = c;
        }

        public void Insert(string s)
        {
            if (_sText.Length > 0)
            {
                _sText = _sText.Insert(_sText.Length, s);
            }
            else { _sText = s; }
        }

        public void RemoveLast()
        {
            if (_sText.Length > 0)
            {
                _sText = _sText.Remove(_sText.Length - 1);
            }
        }
    }
}
