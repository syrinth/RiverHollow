using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIObjects
{
    public class GUIImage : GUIObject
    {
        public Rectangle _sourceRectangle;
        public GUIImage(Vector2 position, Rectangle sourceRect, int width, int height, Texture2D texture)
        {
            _texture = texture;
            _position = position;
            _width = width;
            _height = height;
            _drawRect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
            _sourceRectangle = sourceRect;
        }

        public GUIImage(Vector2 position, Rectangle sourceRect, int width, int height, string texture)
        {
            _texture = GameContentManager.GetTexture(texture);
            _position = position;
            _width = width;
            _height = height;
            _drawRect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
            _sourceRectangle = sourceRect;
        }

        public void MoveImageTo(Vector2 pos)
        {
            _position = pos;
            _drawRect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
        }

        public void MoveImageBy(Vector2 pos)
        {
            _position += pos;
            _drawRect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRectangle, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, float alpha)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRectangle, Color.Black * alpha);
        }
    }
}
