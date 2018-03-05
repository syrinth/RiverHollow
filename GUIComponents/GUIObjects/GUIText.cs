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
        Color Color;

        Vector2 _vTextSize;
        public GUIText(string text)
        {
            Color = Color.Red;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _sText = text;

            _vTextSize = _font.MeasureString(_sText);
            Width = (int)_vTextSize.X;
            Height = (int)_vTextSize.Y;

        }

        public GUIText(string text, SpriteFont font) : this(text)
        {
            _font = font;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.DrawString(_font, _sText, Position(), Color);
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
    }
}
