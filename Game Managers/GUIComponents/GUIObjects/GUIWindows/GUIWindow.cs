using Adventure.Game_Managers.GUIObjects;
using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIWindow : GUIObject
    {
        protected const int SpaceFromBottom = 32;
        protected Vector2 _sourcePoint;

        protected int _innerBorder = 8;
        protected int _borderThickness = 7;
        protected float _edgeScale = 1;
        protected int _edgeSize;
        public int EdgeSize { get => _edgeSize; }

        protected int _visibleBorder;

        public GUIWindow()
        {
            _height = 148;
            _width = AdventureGame.ScreenWidth / 2;
            _position = new Vector2(AdventureGame.ScreenWidth / 4, AdventureGame.ScreenHeight - _height - SpaceFromBottom);

            _edgeSize = 32;
            _sourcePoint = new Vector2(0, 0);
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
        }

        public GUIWindow(Vector2 position, Vector2 sourcePos, int edgeSize, int width, int height) : this()
        {
            _position = position;
            _width = width;
            _height = height;
            _edgeSize = edgeSize;
            _edgeScale = _edgeSize / 32;

            _sourcePoint = sourcePos;

            _drawRect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
        }

        public Rectangle GetRectangle()
        {
            int X = (int)(_position.X - _borderThickness * _edgeScale);
            int Y = (int)(_position.Y - _borderThickness * _edgeScale);
            int Width = (int)(_width + 2 * _borderThickness * _edgeScale);
            int Height = (int)(_height + 2 * _borderThickness * _edgeScale);
            return new Rectangle(X, Y, Width, Height);
        }

        #region Draw
        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawTop(spriteBatch);
            DrawMiddle(spriteBatch);
            DrawBottom(spriteBatch);
        }
        public void DrawTop(SpriteBatch spriteBatch)
        {
            int topY = (int)_position.Y - _edgeSize;
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X - _edgeSize, topY, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X, (int)_sourcePoint.Y, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, topY, _width, _edgeSize), new Rectangle((int)_sourcePoint.X + 32, (int)_sourcePoint.Y + 0, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _width, topY, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + 64, (int)_sourcePoint.Y, 32, 32), Color.White);
        }
        public void DrawMiddle(SpriteBatch spriteBatch)
        {
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch);
        }
        public void DrawMiddleEdges(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X - _edgeSize, (int)_position.Y, _edgeSize, _height), new Rectangle((int)_sourcePoint.X + 0, (int)_sourcePoint.Y + 32, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _width, (int)_position.Y, _edgeSize, _height), new Rectangle((int)_sourcePoint.X + 64, (int)_sourcePoint.Y + 32, 32, 32), Color.White);
        }
        public void DrawCenter(SpriteBatch spriteBatch)
        {
            DrawCenter(spriteBatch, 1);
        }
        public void DrawCenter(SpriteBatch spriteBatch, float percentage)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, (int)(_width*percentage), _height), new Rectangle((int)_sourcePoint.X + 32, (int)_sourcePoint.Y + 32, 32, 32), Color.White);
        }
        public void DrawBottom(SpriteBatch spriteBatch)
        {
            int topY = (int)_position.Y + _height;
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X - _edgeSize, topY, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + 0, (int)_sourcePoint.Y + 64, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, topY, _width, _edgeSize), new Rectangle((int)_sourcePoint.X + 32, (int)_sourcePoint.Y + 64, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _width, topY, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + 64, (int)_sourcePoint.Y + 64, 32, 32), Color.White);
        }
        #endregion
    }
}
