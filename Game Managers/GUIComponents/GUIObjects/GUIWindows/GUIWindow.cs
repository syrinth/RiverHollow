using Adventure.Game_Managers.GUIObjects;
using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIWindow : GUIObject
    {
        private Vector2 _sourcePoint;
        protected int _midWidth;
        public int MiddleWidth { get => _midWidth; }
        protected int _midHeight;
        public int MiddleHeight { get => _midHeight; }

        protected int _edgeSize;

        public GUIWindow()
        {
            _position = new Vector2(AdventureGame.ScreenWidth / 4, AdventureGame.ScreenHeight - 180);
            _width = AdventureGame.ScreenWidth / 2;
            _edgeSize = 32;
            _height = 148;

            Load(new Vector2(0, 0), 32);
        }

        public GUIWindow(Vector2 position, Vector2 sourcePos, int edgeSize, int width, int height)
        {
            _position = position;
            _width = width;
            _height = height;
            _edgeSize = edgeSize;

            Load(sourcePos, edgeSize);
        }

        protected void Load(Vector2 sourcePos, int edgeSize)
        {
            _sourcePoint = sourcePos;

            if (_width / 3 >= edgeSize && _height / 3 >= edgeSize)
            {
                _edgeSize = edgeSize;
            }
            else if (_width / 3 < edgeSize)
            {
                _edgeSize = _width / 3;
            }
            else if (_height / 3 < edgeSize)
            {
                _edgeSize = _height / 3;
            }

            _midWidth = _width - _edgeSize * 2;
            _midHeight = _height - _edgeSize * 2;
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawTop(spriteBatch);
            DrawMiddle(spriteBatch);
            DrawBottom(spriteBatch);
        }

        public void DrawTop(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + 0, (int)_sourcePoint.Y + 0, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _edgeSize, (int)_position.Y, _midWidth, _edgeSize), new Rectangle((int)_sourcePoint.X + 32, (int)_sourcePoint.Y + 0, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + _edgeSize, (int)_position.Y, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + 64, (int)_sourcePoint.Y + 0, 32, 32), Color.White);
        }

        public void DrawMiddle(SpriteBatch spriteBatch)
        {
            DrawMiddle(spriteBatch, true);
        }

        public void DrawMiddle(SpriteBatch spriteBatch,bool showCenter)
        {
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch);
        }

        public void DrawMiddleEdges(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y + _edgeSize, _edgeSize, _midHeight), new Rectangle((int)_sourcePoint.X + 0, (int)_sourcePoint.Y + 32, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + _edgeSize, (int)_position.Y + _edgeSize, _edgeSize, _midHeight), new Rectangle((int)_sourcePoint.X + 64, (int)_sourcePoint.Y + 32, 32, 32), Color.White);
        }

        public void DrawCenter(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _edgeSize, (int)_position.Y + _edgeSize, _midWidth, _midHeight), new Rectangle((int)_sourcePoint.X + 32, (int)_sourcePoint.Y + 32, 32, 32), Color.White);
        }

        public void DrawCenter(SpriteBatch spriteBatch, float midwidth)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _edgeSize, (int)_position.Y + _edgeSize, (int)midwidth, _midHeight), new Rectangle((int)_sourcePoint.X + 32, (int)_sourcePoint.Y + 32, 32, 32), Color.White);
        }

        public void DrawBottom(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y + _midHeight + _edgeSize, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + 0, (int)_sourcePoint.Y + 64, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _edgeSize, (int)_position.Y + _midHeight + _edgeSize, _midWidth, _edgeSize), new Rectangle((int)_sourcePoint.X + 32, (int)_sourcePoint.Y + 64, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + _edgeSize, (int)_position.Y + _midHeight + _edgeSize, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + 64, (int)_sourcePoint.Y + 64, 32, 32), Color.White);
        }
    }
}
