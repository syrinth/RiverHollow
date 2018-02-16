using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.GUIObjects
{
    public abstract class GUIObject
    {
        protected int _height;
        public int Height { get => _height; set => _height = value; }
        protected int _width;
        public int Width { get => _width; }

        protected Vector2 _position;
        public Vector2 Position {
            get => _position;
            set {
                _position = value;
                _drawRect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
            }
        }

        protected Rectangle _drawRect;
        public Rectangle DrawRectangle { get => _drawRect; }

        protected Rectangle _sourceRect;
        public Rectangle SourceRectangle { get => _sourceRect; }

        protected Texture2D _texture;

        public virtual bool Contains(Point mouse)
        {
            return DrawRectangle.Contains(mouse);
        }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRect, Color.White);
        }

        public static Vector2 PosFromCenter(Vector2 center, int width, int height)
        {
            return new Vector2(center.X - width / 2, center.Y - height / 2);
        }
        public static Vector2 PosFromCenter(int x, int y, int width, int height)
        {
            return new Vector2(x - width / 2, y - height / 2);
        }
        public virtual bool ProcessLeftButtonClick(Point mouse)
        {
            return false;
        }
    }
}
