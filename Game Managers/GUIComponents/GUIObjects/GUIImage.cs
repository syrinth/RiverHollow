using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIObjects
{
    public class GUIImage : GUIObject
    {
        public Rectangle _sourceRectangle;
        public GUIImage(Vector2 position, Rectangle sourceRect, int width, int height, string texture)
        {
            _texture = GameContentManager.GetTexture(texture);
            _position = position;
            _width = width;
            _height = height;
            _rect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
            _sourceRectangle = sourceRect;
        }

        public void MoveImage(Vector2 pos)
        {
            _position = pos;
            _rect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _rect, _sourceRectangle, Color.White);
        }
    }
}
