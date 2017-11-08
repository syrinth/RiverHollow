using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
        private static SpriteFont _displayFont = GameContentManager.GetFont(@"Fonts\DisplayFont");
        private Item _item;
        public Item Item { get => _item; set => _item = value; }
        public bool Open = true;
        private bool _hover;
        private GUITextWindow _textWindow;

        public GUIItemBox(Vector2 position, Rectangle sourceRect, int width, int height, string texture, Item item) : base(position, sourceRect, width, height, texture)
        {
            _item = item;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_item != null)
            {
                _item.Draw(spriteBatch, _drawRect);
                if (_item.DoesItStack)
                {
                    spriteBatch.DrawString(_displayFont, _item.Number.ToString(), new Vector2(_drawRect.X + 22, _drawRect.Y + 22), Color.White);
                }
            }
            if (_hover)
            {
                if (_textWindow != null) { _textWindow.Draw(spriteBatch, true); }
            }
        }

        public bool DrawDescription(SpriteBatch spriteBatch)
        {
            if (_textWindow != null) {
                _textWindow.Draw(spriteBatch, true);
            }

            return _hover;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                _hover = true;
                if (_item != null)
                {
                    _textWindow = new GUITextWindow(mouse.ToVector2(), _item.GetDescription());
                    _textWindow.MoveTo(new Vector2(mouse.ToVector2().X, mouse.ToVector2().Y + 32));
                }
                rv = true;
            }
            else
            {
                _hover = false;
                _textWindow = null;
            }
            return rv;
        }
    }
}
