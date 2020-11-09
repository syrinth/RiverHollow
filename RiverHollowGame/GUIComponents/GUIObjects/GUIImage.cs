using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIImage : GUIObject
    {
        public GUIImage(Rectangle sourceRect, int width, int height, Texture2D texture)
        {
            _texture = texture;
            Width = width;
            Height = height;
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
            _sourceRect = sourceRect;
        }

        public GUIImage(Rectangle sourceRect, int width, int height, string texture)
        {
            _texture = DataManager.GetTexture(texture);
            Width = width;
            Height = height;
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
            _sourceRect = sourceRect;
        }

        public void MoveImageTo(Vector2 pos)
        {
            Position(pos);
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
        }

        public void MoveImageBy(Vector2 pos)
        {
            PositionAdd(pos);
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRect, _cColor * Alpha());
        }

        public void Draw(SpriteBatch spriteBatch, float alpha)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRect, Color.Black * alpha);
        }
    }
}
