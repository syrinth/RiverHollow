using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIWindow : GUIObject
    {
        public struct WindowData
        {
            Vector2 _vSource;
            public Vector2 Src => _vSource;
            int _iEdge;
            public int Edge => _iEdge;
            public int ScaledEdge => _iEdge * (int)GameManager.Scale;
            int _iBottomEdge;
            public int BottomEdge => _iBottomEdge;
            public int ScaledBottomEdge => _iBottomEdge * (int)GameManager.Scale;
            int _iSize;
            public int Size => _iSize;  //The size of the center of the image square

            public WindowData(int x, int y, int edge, int size, int BottomEdge = -1)
            {
                _vSource = new Vector2(x, y);
                _iEdge = edge;
                _iBottomEdge = (BottomEdge == -1 ? _iEdge : BottomEdge);
                _iSize = size;
            }

            public int WidthEdges() { return ScaledEdge * 2; }
            public int HeightEdges() { return ScaledEdge + ScaledBottomEdge; }
        };
        internal static WindowData RedWin = new WindowData(124, 60, 4, 16, 6);
        internal static WindowData BrownWin = new WindowData(172, 60, 4, 16, 6);
        internal static WindowData GreyWin = new WindowData(206, 62, 2, 16);
        internal static WindowData DisplayWin = new WindowData(48, 32, 1, 14);

        protected const int SpaceFromBottom = 32;

        protected WindowData _winData;
        public int EdgeSize => _winData.ScaledEdge;

        public GUIWindow()
        {
            Height = 184;
            Width = RiverHollow.ScreenWidth / 2;

            _winData = RedWin;
        }

        public GUIWindow(WindowData winData, int width, int height)
        {
            Width = width;
            Height = height;

            _winData = winData;
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach(GUIObject c in Controls)
            {
                if (c.Contains(mouse))
                {
                    rv = c.ProcessLeftButtonClick(mouse);
                    if (rv) { break; }
                }
            }

            return rv;
        }

        public virtual void Resize()
        {
            foreach(GUIObject g in Controls)
            {
                if (g.DrawRectangle.Right > InnerRectangle().Right)
                {
                    Width += g.DrawRectangle.Right - InnerRectangle().Right;
                }
                if (g.DrawRectangle.Bottom > InnerRectangle().Bottom)
                {
                    Height += g.DrawRectangle.Bottom - InnerRectangle().Bottom;
                }
                Position(Position());
            }
        }
        public virtual void IncreaseSizeTo(int endWidth, int endHeight)
        {
            int modWidth = (endWidth - Width) / 2;
            int modHeight = (endHeight - Height) / 2;

            base.Position(Position() - new Vector2(modWidth, modHeight));
            Width += modWidth * 2;
            Height += modHeight * 2;
        }

        public override void AddControl(GUIObject g)
        {
            if (g != null && !Controls.Contains(g)) {
                Controls.Add(g);
                g.ParentWindow = this;
            }
        }

        #region Draw
        public int SkipSize() { return _winData.Size + _winData.Edge; }
        public void DrawWindow(SpriteBatch spriteBatch)
        {
            if (Show)
            {
                Vector2 pos = Position();
                int k = Width;
                int j = Height;
                DrawTop(spriteBatch);
                DrawMiddle(spriteBatch);
                DrawBottom(spriteBatch);
            }
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Show)
            {
                DrawWindow(spriteBatch);
                base.Draw(spriteBatch);
            }
        }

        //Draw the edging
        public void DrawTop(SpriteBatch spriteBatch)
        {
            int BorderTop = (int)Position().Y;
            int BorderLeft = (int)Position().X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, BorderTop, _winData.ScaledEdge, _winData.ScaledEdge), Util.FloatRectangle(_winData.Src.X, _winData.Src.Y, _winData.Edge, _winData.Edge), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), BorderTop, MidWidth(), _winData.ScaledEdge), Util.FloatRectangle(_winData.Src.X + _winData.Edge, _winData.Src.Y, _winData.Size, _winData.Edge), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), BorderTop, _winData.ScaledEdge, _winData.ScaledEdge), Util.FloatRectangle(_winData.Src.X + SkipSize(), _winData.Src.Y, _winData.Edge, _winData.Edge), EnabledColor * Alpha());
        }
        public void DrawMiddle(SpriteBatch spriteBatch)
        {
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch);
        }
        public void DrawMiddleEdges(SpriteBatch spriteBatch)
        {
            int BorderLeft = (int)Position().X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, MidStartY(), _winData.ScaledEdge, MidHeight()), Util.FloatRectangle(_winData.Src.X, _winData.Src.Y + _winData.Edge, _winData.Edge, _winData.Size), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), MidStartY(), _winData.ScaledEdge, MidHeight()), Util.FloatRectangle(_winData.Src.X + SkipSize(), _winData.Src.Y + _winData.Edge, _winData.Edge, _winData.Size), EnabledColor * Alpha());
        }
        public void DrawCenter(SpriteBatch spriteBatch)
        {
            DrawCenter(spriteBatch, 1);
        }
        public void DrawCenter(SpriteBatch spriteBatch, float percentage)
        {
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), MidStartY(), (int)(MidWidth() * percentage), MidHeight()), Util.FloatRectangle(_winData.Src.X + _winData.Edge, _winData.Src.Y + _winData.Edge, _winData.Size, _winData.Size), EnabledColor * Alpha());
        }
        public void DrawBottom(SpriteBatch spriteBatch)
        {
            int BorderLeft = (int)Position().X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, EndStartY(), _winData.ScaledEdge, _winData.ScaledBottomEdge), Util.FloatRectangle(_winData.Src.X, _winData.Src.Y + SkipSize(), _winData.Edge, _winData.BottomEdge), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), EndStartY(), MidWidth(), _winData.ScaledBottomEdge), Util.FloatRectangle(_winData.Src.X + _winData.Edge, _winData.Src.Y + SkipSize(), _winData.Size, _winData.BottomEdge), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), EndStartY(), _winData.ScaledEdge, _winData.ScaledBottomEdge), Util.FloatRectangle(_winData.Src.X + SkipSize(), _winData.Src.Y + SkipSize(), _winData.Edge, _winData.BottomEdge), EnabledColor * Alpha());
        }

        public int MidStartX() { return (int)Position().X + _winData.ScaledEdge; }
        public int MidStartY() { return (int)Position().Y + _winData.ScaledEdge; }
        public int EndStartX() { return (int)Position().X + MidWidth() + _winData.ScaledEdge; }
        public int EndStartY() { return (int)Position().Y + MidHeight() + _winData.ScaledEdge; }
        public int MidHeight() { return Height - HeightEdges(); }
        public int MidWidth() { return Width - WidthEdges(); }
        public int WidthEdges() { return _winData.WidthEdges(); }
        public int HeightEdges() { return _winData.HeightEdges(); }
        #endregion
        #region Location Retrival
        //Usable space needs to ignore the edges of the rectangle
        public Rectangle InnerRectangle()
        {
            return new Rectangle((int)Position().X + _winData.ScaledEdge, (int)Position().Y + _winData.ScaledEdge, MidWidth(), MidHeight());
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

        public int InnerLeft() { return MidStartX(); }
        public int InnerTop() { return MidStartY(); }
        public int InnerRight() { return EndStartX(); }
        public int InnerBottom() { return EndStartY(); }

        public Vector2 OuterTopLeft() { return Position(); }
        public Vector2 OuterTopRight() { return Position() + new Vector2(Width, 0); }
        public Vector2 OuterBottomLeft() { return Position() + new Vector2(0, Height); }
        public Vector2 OuterBottomRight() { return Position() + new Vector2(Width, Height); }

        #endregion
    }
}
