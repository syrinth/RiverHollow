﻿using RiverHollow.Game_Managers.GUIObjects;
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
        }
    }
}
