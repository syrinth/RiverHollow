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
            public Vector2 SourceVector { get; }
            public int Edge { get; }
            public int ScaledEdge => Edge * (int)GameManager.Scale;
            public int Size { get; }  //The size of the center of the image square

            public WindowData(int x, int y, int edge, int size)
            {
                SourceVector = new Vector2(x, y);
                Edge = edge;
                Size = size;
            }

            public int WidthEdges() { return ScaledEdge * 2; }
            public int HeightEdges() { return ScaledEdge * 2; }
        };
        internal static WindowData Window_1 = new WindowData(122, 58, 6, 16);
        internal static WindowData Window_2 = new WindowData(170, 58, 6, 16);
        internal static WindowData GreyWin = new WindowData(206, 62, 2, 16);
        internal static WindowData DisplayWin = new WindowData(48, 32, 1, 14);

        protected const int SpaceFromBottom = 32;

        protected WindowData _winData;
        public int EdgeSize => _winData.ScaledEdge;

        public GUIWindow()
        {
            Height = 184;
            Width = RiverHollow.ScreenWidth / 2;

            _winData = Window_1;
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
                if (g.DrawRectangle.Right > InnerRectangle().Right) { Width += g.DrawRectangle.Right - InnerRectangle().Right; }
                else if (g.DrawRectangle.Right < InnerRectangle().Right) { Width -= InnerRectangle().Right - g.DrawRectangle.Right; }

                if (g.DrawRectangle.Bottom > InnerRectangle().Bottom) { Height += g.DrawRectangle.Bottom - InnerRectangle().Bottom; }
                else if (g.DrawRectangle.Bottom < InnerRectangle().Bottom) { Height -= InnerRectangle().Bottom - g.DrawRectangle.Bottom; }
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
            if (Show())
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
            if (Show())
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

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, BorderTop, _winData.ScaledEdge, _winData.ScaledEdge), Util.FloatRectangle(_winData.SourceVector.X, _winData.SourceVector.Y, _winData.Edge, _winData.Edge), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), BorderTop, MidWidth(), _winData.ScaledEdge), Util.FloatRectangle(_winData.SourceVector.X + _winData.Edge, _winData.SourceVector.Y, _winData.Size, _winData.Edge), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), BorderTop, _winData.ScaledEdge, _winData.ScaledEdge), Util.FloatRectangle(_winData.SourceVector.X + SkipSize(), _winData.SourceVector.Y, _winData.Edge, _winData.Edge), EnabledColor * Alpha());
        }
        public void DrawMiddle(SpriteBatch spriteBatch)
        {
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch);
        }
        public void DrawMiddleEdges(SpriteBatch spriteBatch)
        {
            int BorderLeft = (int)Position().X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, MidStartY(), _winData.ScaledEdge, MidHeight()), Util.FloatRectangle(_winData.SourceVector.X, _winData.SourceVector.Y + _winData.Edge, _winData.Edge, _winData.Size), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), MidStartY(), _winData.ScaledEdge, MidHeight()), Util.FloatRectangle(_winData.SourceVector.X + SkipSize(), _winData.SourceVector.Y + _winData.Edge, _winData.Edge, _winData.Size), EnabledColor * Alpha());
        }
        public void DrawCenter(SpriteBatch spriteBatch)
        {
            DrawCenter(spriteBatch, 1);
        }
        public void DrawCenter(SpriteBatch spriteBatch, float percentage)
        {
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), MidStartY(), (int)(MidWidth() * percentage), MidHeight()), Util.FloatRectangle(_winData.SourceVector.X + _winData.Edge, _winData.SourceVector.Y + _winData.Edge, _winData.Size, _winData.Size), EnabledColor * Alpha());
        }
        public void DrawBottom(SpriteBatch spriteBatch)
        {
            int BorderLeft = (int)Position().X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, EndStartY(), _winData.ScaledEdge, _winData.ScaledEdge), Util.FloatRectangle(_winData.SourceVector.X, _winData.SourceVector.Y + SkipSize(), _winData.Edge, _winData.Edge), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), EndStartY(), MidWidth(), _winData.ScaledEdge), Util.FloatRectangle(_winData.SourceVector.X + _winData.Edge, _winData.SourceVector.Y + SkipSize(), _winData.Size, _winData.Edge), EnabledColor * Alpha());
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), EndStartY(), _winData.ScaledEdge, _winData.ScaledEdge), Util.FloatRectangle(_winData.SourceVector.X + SkipSize(), _winData.SourceVector.Y + SkipSize(), _winData.Edge, _winData.Edge), EnabledColor * Alpha());
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
