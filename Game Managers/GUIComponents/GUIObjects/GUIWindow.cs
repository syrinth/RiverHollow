using Adventure.Game_Managers.GUIObjects;
using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIWindow : GUIObject
    {
        private GUIImage[] _ninePatches;
        protected int _midWidth;
        public int MiddleWidth { get => _midWidth; }
        protected int _midHeight;
        public int MiddleHeight { get => _midHeight; }

        protected int _squareSize;

        public GUIWindow()
        {
            _position = new Vector2(AdventureGame.ScreenWidth / 4, AdventureGame.ScreenHeight - 180);
            _width = AdventureGame.ScreenWidth / 2;
            _squareSize = 32;
            _height = (28 * 3) + (2 * _squareSize); //Default for DialogWindow 28 is line height of text, 3 is threelines

            Load();
        }

        public GUIWindow(Vector2 position, int squareSize, int width, int height)
        {
            _position = position;
            _width = width;
            _height = height;
            _squareSize = squareSize;

            Load();
        }

        private void Load()
        {
            _midWidth = _width - _squareSize * 2;
            _midHeight = _height - _squareSize * 2;
            _ninePatches = new GUIImage[9];
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
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, _squareSize, _squareSize), new Rectangle(0, 0, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _squareSize, (int)_position.Y, _midWidth, _squareSize), new Rectangle(32, 0, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + _squareSize, (int)_position.Y, _squareSize, _squareSize), new Rectangle(64, 0, 32, 32), Color.White);
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
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y + _squareSize, _squareSize, _midHeight), new Rectangle(0, 32, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + _squareSize, (int)_position.Y + _squareSize, _squareSize, _midHeight), new Rectangle(64, 32, 32, 32), Color.White);
        }

        public void DrawCenter(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _squareSize, (int)_position.Y + _squareSize, _midWidth, _midHeight), new Rectangle(32, 32, 32, 32), Color.White);
        }

        public void DrawCenter(SpriteBatch spriteBatch, float midwidth)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _squareSize, (int)_position.Y + _squareSize, (int)midwidth, _midHeight), new Rectangle(32, 32, 32, 32), Color.White);
        }

        public void DrawBottom(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y + _midHeight + _squareSize, _squareSize, _squareSize), new Rectangle(0, 64, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _squareSize, (int)_position.Y + _midHeight + _squareSize, _midWidth, _squareSize), new Rectangle(32, 64, 32, 32), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _midWidth + _squareSize, (int)_position.Y + _midHeight + _squareSize, _squareSize, _squareSize), new Rectangle(64, 64, 32, 32), Color.White);
        }
    }
}
