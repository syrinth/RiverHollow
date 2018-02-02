using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIButton : GUIObject
    {
        protected SpriteFont _font;
        public bool IsMouseHovering = false;
        public bool _enabled;
        private string _text;
        private Vector2 _textSize;

        public GUIButton(Vector2 position, Rectangle sourceRect, int width, int height, string text, string texture, bool usePosition = false)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _texture = GameContentManager.GetTexture(texture);
            _width = width;
            _height = height;
            Position = usePosition ? position : position - new Vector2(width / 2, height / 2);
            _text = text;
            _enabled = true;

            _textSize = _font.MeasureString(_text);

            _sourceRect = sourceRect;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRect, (IsMouseHovering) ? Color.LightGray : Color.White);
            spriteBatch.DrawString(_font, _text, new Vector2(Position.X+(_width/2) - (_textSize.X/2), Position.Y+(_height/2) - (_textSize.Y / 2)), Color.Black);
        }
    }
}
