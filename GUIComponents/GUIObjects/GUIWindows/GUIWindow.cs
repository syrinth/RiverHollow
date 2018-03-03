using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIWindow : GUIObject
    {
        public struct WindowData
        {
            Vector2 _vSource;
            public Vector2 Src => _vSource;
            int _iEdge;
            public int Edge => _iEdge;

            public WindowData(int x, int y, int edge)
            {
                _vSource = new Vector2(x, y);
                _iEdge = edge;
            }
        };
        public static WindowData RedWin = new WindowData(0, 0, 7);
        public static WindowData BrownWin = new WindowData(32, 0, 7);
        public static WindowData GreyWin = new WindowData(64, 0, 7);

        protected int Size = 18;
        protected const int SpaceFromBottom = 32;

        protected int _iInnerBorder = 8;

        protected WindowData _winData;
        public int EdgeSize { get => _winData.Edge; }

        public GUIWindow()
        {
            Height = 148;
            Width = RiverHollow.ScreenWidth / 2;
            Position = new Vector2(RiverHollow.ScreenWidth / 4, RiverHollow.ScreenHeight - Height - SpaceFromBottom);

            _winData = RedWin;
        }

        public GUIWindow(WindowData winData, int width, int height)
        {
            Width = width;
            Height = height;

            _winData = winData;
        }

        public GUIWindow(Vector2 position, WindowData winData, int width, int height) : this()
        {
            Position = position;
            Width = width;
            Height = height;
            _winData = winData;
        }

        #region Draw
        public int Skip()
        {
            return Size + _winData.Edge;
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawTop(spriteBatch);
            DrawMiddle(spriteBatch);
            DrawBottom(spriteBatch);
        }

        //Draw the edging
        public void DrawTop(SpriteBatch spriteBatch)
        {
            int BorderTop = (int)Position.Y;
            int BorderLeft = (int)Position.X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, BorderTop, _winData.Edge, _winData.Edge), new Rectangle((int)_winData.Src.X, (int)_winData.Src.Y, _winData.Edge, _winData.Edge), Color.White);
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), BorderTop, MidWidth(), _winData.Edge), new Rectangle((int)_winData.Src.X + _winData.Edge, (int)_winData.Src.Y, Size, _winData.Edge), Color.White);
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), BorderTop, _winData.Edge, _winData.Edge), new Rectangle((int)_winData.Src.X + Skip(), (int)_winData.Src.Y, _winData.Edge, _winData.Edge), Color.White);
        }
        public void DrawMiddle(SpriteBatch spriteBatch)
        {
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch);
        }
        public void DrawMiddleEdges(SpriteBatch spriteBatch)
        {
            int BorderLeft = (int)Position.X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, MidStartY(), _winData.Edge, MidHeight()), new Rectangle((int)_winData.Src.X, (int)_winData.Src.Y + _winData.Edge, _winData.Edge, Size), Color.White);
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), MidStartY(), _winData.Edge, MidHeight()), new Rectangle((int)_winData.Src.X + Skip(), (int)_winData.Src.Y + _winData.Edge, _winData.Edge, Size), Color.White);
        }
        public void DrawCenter(SpriteBatch spriteBatch)
        {
            DrawCenter(spriteBatch, 1);
        }
        public void DrawCenter(SpriteBatch spriteBatch, float percentage)
        {
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), MidStartY(), (int)(MidWidth() * percentage), MidHeight()), new Rectangle((int)_winData.Src.X + _winData.Edge, (int)_winData.Src.Y + _winData.Edge, Size, Size), Color.White);
        }
        public void DrawBottom(SpriteBatch spriteBatch)
        {
            int BorderLeft = (int)Position.X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, EndStartY(), _winData.Edge, _winData.Edge), new Rectangle((int)_winData.Src.X, (int)_winData.Src.Y + Skip(), _winData.Edge, _winData.Edge), Color.White);
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), EndStartY(), MidWidth(), _winData.Edge), new Rectangle((int)_winData.Src.X + _winData.Edge, (int)_winData.Src.Y + Skip(), Size, _winData.Edge), Color.White);
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), EndStartY(), _winData.Edge, _winData.Edge), new Rectangle((int)_winData.Src.X + Skip(), (int)_winData.Src.Y + Skip(), _winData.Edge, _winData.Edge), Color.White);
        }

        public int MidStartX() { return (int)Position.X + _winData.Edge; }
        public int MidStartY() { return (int)Position.Y + _winData.Edge; }
        public int EndStartX() { return (int)Position.X + MidWidth() + _winData.Edge; }
        public int EndStartY() { return (int)Position.Y + MidHeight() + _winData.Edge; }
        public int MidHeight() { return Height - DblEdge(); }
        public int MidWidth() { return Width - DblEdge(); }
        #endregion
        #region Location Retrival
        //Usable space needs to ignore the edges of the rectangle
        public Rectangle InnerRectangle()
        {
            return new Rectangle((int)Position.X + _winData.Edge, (int)Position.Y + _winData.Edge, MidWidth(), MidHeight());
        }
        public Vector2 InnerRecVec()
        {
            return InnerRectangle().Location.ToVector2();
        }
        public Vector2 InnerTopLeft()
        {
            return InnerRectangle().Location.ToVector2();
        }
        public Vector2 InnerBottomLeft()
        {
            return InnerRectangle().Location.ToVector2() + new Vector2(0, MidWidth());
        }
        public int DblEdge()
        {
            return _winData.Edge * 2;
        }

        public int InnerLeft() { return MidStartX() + _iInnerBorder; }
        public int InnerTop() { return MidStartY() + _iInnerBorder; }
        public int InnerRight() { return EndStartX() - _iInnerBorder; }
        public int InnerBottom() { return EndStartY() - _iInnerBorder; }

        public Vector2 OuterTopLeft() { return Position; }
        public Vector2 OuterTopRight() { return Position + new Vector2(Width, 0); }
        public Vector2 OuterBottomLeft() { return Position + new Vector2(0, Height); }
        public Vector2 OuterBottomRight() { return Position + new Vector2(Width, Height); }

        #endregion
    }
}
