using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIButton : GUIObject
    {
        protected Rectangle _rDefaultButton = new Rectangle(0, 128, 64, 32);
        public Rectangle DefaultButton { get => _rDefaultButton; }
        protected SpriteFont _font;
        public bool IsMouseHovering = false;
        public bool _bEnabled;
        private string _sText;
        private Vector2 _textSize;

        public GUIButton(string text)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");

            _sText = text;
            _textSize = _font.MeasureString(_sText);

            _bEnabled = true;
            _sourceRect = DefaultButton;
        }

        public GUIButton(string text, int width, int height) : this(text)
        {
            Width = width;
            Height = height;
        }

        public GUIButton(Vector2 position, Rectangle sourceRect, int width, int height, string text, string texture, bool usePosition = false) : this (text)
        {
            Width = width;
            Height = height;
            Position = usePosition ? position : position - new Vector2(width / 2, height / 2);
            _sText = text;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRect, (IsMouseHovering) ? Color.LightGray : Color.White);
            spriteBatch.DrawString(_font, _sText, new Vector2(Position.X+(Width/2) - (_textSize.X/2), Position.Y+(Height/2) - (_textSize.Y / 2)), Color.Black);
        }
    }
}
