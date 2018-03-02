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
        public static int GreyDialogEdge = 7;
        public static Vector2 GreyDialog = new Vector2(64, 0);
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
            Height = 148;
            Width = RiverHollow.ScreenWidth / 2;
            Position = new Vector2(RiverHollow.ScreenWidth / 4, RiverHollow.ScreenHeight - Height - SpaceFromBottom);

            _edgeSize = RedDialogEdge;
            _sourcePoint = RedDialog;
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
        }

        public GUIWindow(Vector2 position, Vector2 sourcePos, int edgeSize, int width, int height) : this()
        {
            Position = position;
            Width = width;
            Height = height;
            _edgeSize = RedDialogEdge;

            _sourcePoint = sourcePos;

            _drawRect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public Vector2 Corner()
        {
            return UsableRectangle().Location.ToVector2();
        }
        public Rectangle UsableRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        public Vector2 GetUsableRectangleVec()
        {
            return UsableRectangle().Location.ToVector2();
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
            int BorderTop = (int)Position.Y - _edgeSize;
            int BorderLeft = (int)Position.X - _edgeSize;

            spriteBatch.Draw(_texture, new Rectangle((int)BorderLeft, BorderTop, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X, (int)_sourcePoint.Y, _edgeSize, _edgeSize), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X, BorderTop, Width, _edgeSize), new Rectangle((int)_sourcePoint.X + _edgeSize, (int)_sourcePoint.Y, Size, _edgeSize), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X + Width, BorderTop, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + Skip(), (int)_sourcePoint.Y, _edgeSize, _edgeSize), Color.White);
        }
        public void DrawMiddle(SpriteBatch spriteBatch)
        {
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch);
        }
        public void DrawMiddleEdges(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X - _edgeSize, (int)Position.Y, _edgeSize, Height), new Rectangle((int)_sourcePoint.X, (int)_sourcePoint.Y + _edgeSize, _edgeSize, Size), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X + Width, (int)Position.Y, _edgeSize, Height), new Rectangle((int)_sourcePoint.X + Skip(), (int)_sourcePoint.Y + _edgeSize, _edgeSize, Size), Color.White);
        }
        public void DrawCenter(SpriteBatch spriteBatch)
        {
            DrawCenter(spriteBatch, 1);
        }
        public void DrawCenter(SpriteBatch spriteBatch, float percentage)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X, (int)Position.Y, (int)(Width*percentage), Height), new Rectangle((int)_sourcePoint.X + _edgeSize, (int)_sourcePoint.Y + _edgeSize, Size, Size), Color.White);
        }
        public void DrawBottom(SpriteBatch spriteBatch)
        {
            int topY = (int)Position.Y + Height;
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X - _edgeSize, topY, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X, (int)_sourcePoint.Y + Skip(), _edgeSize, _edgeSize), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X, topY, Width, _edgeSize), new Rectangle((int)_sourcePoint.X + _edgeSize, (int)_sourcePoint.Y + Skip(), Size, _edgeSize), Color.White);
            spriteBatch.Draw(_texture, new Rectangle((int)Position.X + Width, topY, _edgeSize, _edgeSize), new Rectangle((int)_sourcePoint.X + Skip(), (int)_sourcePoint.Y + Skip(), _edgeSize, _edgeSize), Color.White);
        }
        #endregion
    }
}
