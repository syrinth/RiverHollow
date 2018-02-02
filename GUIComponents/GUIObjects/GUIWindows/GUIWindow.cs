using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using RiverHollow.Tile_Engine;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIWindow : GUIObject
    {
        public static Vector2 RedDialog = new Vector2(0, 0);
        public static int RedDialogEdge = 7;
        public static Vector2 BrownDialog = new Vector2(32, 0);
        public static int BrownDialogEdge = 7;
        protected int Size = 18;
        protected const int SpaceFromBottom = 32;
        protected Vector2 _sourcePoint;

        protected int _innerBorder = 8;
        protected int _edgeSize;
        public int EdgeSize { get => _edgeSize; }

        protected int _visibleBorder;

        public GUIWindow()
        {
            _height = 148;
            _width = RiverHollow.ScreenWidth / 2;
            _position = new Vector2(RiverHollow.ScreenWidth / 4, RiverHollow.ScreenHeight - _height - SpaceFromBottom);

            _edgeSize = RedDialogEdge;
            _sourcePoint = RedDialog;
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
        }

        public GUIWindow(Vector2 position, Vector2 sourcePos, int edgeSize, int width, int height) : this()
        {
            _position = position;
            _width = width;
            _height = height;
            _edgeSize = RedDialogEdge;

            _sourcePoint = sourcePos;

            _drawRect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
        }

        public Vector2 Corner()
        {
            return UsableRectangle().Location.ToVector2();
        }
        public Rectangle UsableRectangle()
        {
            return new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
        }

        #region Draw
        public int Skip()
        {
            return Size + _edgeSize;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawTop(spriteBatch);
            DrawMiddle(spriteBatch);
            DrawBottom(spriteBatch);
        }
        public void DrawTop(SpriteBatch spriteBatch)
        {
            int BorderTop = (int)_position.Y - _edgeSize;
            int BorderLeft = (int)_position.X - _edgeSize;

            spriteBatch.Draw(_texture, new Rectangle((int)BorderLeft, BorderTop, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X, (int)_sourcePoint.Y, _edgeSize, _edgeSize), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, BorderTop, _width, _edgeSize), new Rectangle((int)_sourcePoint.X + _edgeSize, (int)_sourcePoint.Y, Size, _edgeSize), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _width, BorderTop, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + Skip(), (int)_sourcePoint.Y, _edgeSize, _edgeSize), Color.White);
        }
        public void DrawMiddle(SpriteBatch spriteBatch)
        {
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch);
        }
        public void DrawMiddleEdges(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X - _edgeSize, (int)_position.Y, _edgeSize, _height), new Rectangle((int)_sourcePoint.X, (int)_sourcePoint.Y + _edgeSize, _edgeSize, Size), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _width, (int)_position.Y, _edgeSize, _height), new Rectangle((int)_sourcePoint.X + Skip(), (int)_sourcePoint.Y + _edgeSize, _edgeSize, Size), Color.White);
        }
        public void DrawCenter(SpriteBatch spriteBatch)
        {
            DrawCenter(spriteBatch, 1);
        }
        public void DrawCenter(SpriteBatch spriteBatch, float percentage)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, (int)(_width*percentage), _height), new Rectangle((int)_sourcePoint.X + _edgeSize, (int)_sourcePoint.Y + _edgeSize, Size, Size), Color.White);
        }
        public void DrawBottom(SpriteBatch spriteBatch)
        {
            int topY = (int)_position.Y + _height;
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X - _edgeSize, topY, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X, (int)_sourcePoint.Y + Skip(), _edgeSize, _edgeSize), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, topY, _width, _edgeSize), new Rectangle((int)_sourcePoint.X + _edgeSize, (int)_sourcePoint.Y + Skip(), Size, _edgeSize), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X + _width, topY, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + Skip(), (int)_sourcePoint.Y + Skip(), _edgeSize, _edgeSize), Color.White);
        }
        #endregion
    }
}
