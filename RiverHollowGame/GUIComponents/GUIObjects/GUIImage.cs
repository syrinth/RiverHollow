using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIImage : GUIObject
    {
        public GUIImage(Rectangle sourceRect, string texture) : this(sourceRect, DataManager.GetTexture(texture)) { }

        public GUIImage(Rectangle sourceRect, Texture2D texture)
        {
            _texture = texture;
            Width = GameManager.ScaleIt(sourceRect.Width);
            Height = GameManager.ScaleIt(sourceRect.Height);
            _drawRect = new Rectangle(Position().X, Position().Y, Width, Height);
            _sourceRect = sourceRect;
        }

        public GUIImage(Rectangle sourceRect, int width, int height, Texture2D texture)
        {
            _texture = texture;
            Width = width;
            Height = height;
            _drawRect = new Rectangle(Position().X, Position().Y, Width, Height);
            _sourceRect = sourceRect;
        }

        public GUIImage(Rectangle sourceRect, int width, int height, string texture)
        {
            _texture = DataManager.GetTexture(texture);
            Width = width;
            Height = height;
            _drawRect = new Rectangle(Position().X, Position().Y, Width, Height);
            _sourceRect = sourceRect;
        }

        public void Draw(SpriteBatch spriteBatch, float alpha)
        {
            if (Show())
            {
                spriteBatch.Draw(_texture, _drawRect, _sourceRect, Color.Black * alpha);
            }
        }
    }
}
