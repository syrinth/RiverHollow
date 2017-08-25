using Adventure.Game_Managers.GUIObjects;
using Adventure.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIItemBox : GUIImage
    {
        private static SpriteFont _displayFont = GameContentManager.GetFont(@"Fonts\DisplayFont");
        private InventoryItem _item;
        public InventoryItem Item { get => _item; set => _item = value; }
        public bool Open = true;

        public GUIItemBox(Vector2 position, Rectangle sourceRect, int width, int height, string texture, InventoryItem item) : base(position, sourceRect, width, height, texture)
        {

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_item != null)
            {
                _item.Draw(spriteBatch, _rect);
                if (_item.DoesItStack)
                {
                    spriteBatch.DrawString(_displayFont, _item.Number.ToString(), new Vector2(_rect.X + 22, _rect.Y + 22), Color.White);
                }
            }
        }
    }
}
