using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIWindow : GUIObject
    {
        public struct WindowData
        {
            public Vector2 SourceVector { get; }
            public int TopEdge { get; }
            public int BottomEdge { get; }
            public int LeftEdge { get; }
            public int RightEdge { get; }
            public int ScaledTopEdge => TopEdge * GameManager.CurrentScale;
            public int ScaledBottomEdge => BottomEdge * GameManager.CurrentScale;
            public int ScaledLeftEdge => LeftEdge * GameManager.CurrentScale;
            public int ScaledRightEdge => RightEdge * GameManager.CurrentScale;
            public int Size { get; }  //The size of the center of the image square

            public WindowData(int x, int y, int topEdge, int botEdge, int leftEdge, int rightEdge, int size)
            {
                SourceVector = new Vector2(x, y);
                TopEdge = topEdge;
                BottomEdge = botEdge;
                LeftEdge = leftEdge;
                RightEdge = rightEdge;
                Size = size;
            }

            public int WidthEdges() { return ScaledLeftEdge + ScaledRightEdge; }
            public int HeightEdges() { return ScaledTopEdge + ScaledBottomEdge; }
        };
        internal static WindowData Window_1 = new WindowData(128, 65, 5, 5, 6, 6, 4);
        internal static WindowData Window_2 = new WindowData(144, 65, 5, 5, 6, 6, 4);
        internal static WindowData GreyWin = new WindowData(206, 62, 2, 2, 2, 2, 16);
        internal static WindowData DisplayWin = new WindowData(48, 32, 1, 1, 1, 1, 14);
        internal static WindowData WoodenPanel = new WindowData(128, 32, 3, 3, 3, 3, 10);

        protected const int SpaceFromBottom = 32;

        protected WindowData _winData;

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

        public virtual void Resize(bool shrink = true, int scaledEdge = 0)
        {
            bool xGrew = false;
            bool yGrew = false;
            foreach(GUIObject g in Controls)
            {
                if (g.DrawRectangle.Right + ScaleIt(scaledEdge) > InnerRectangle().Right) {
                    Width += g.DrawRectangle.Right - InnerRectangle().Right + ScaleIt(scaledEdge);
                    xGrew = true;
                }
                else if (!xGrew && shrink && g.DrawRectangle.Right < InnerRectangle().Right) { Width -= InnerRectangle().Right - g.DrawRectangle.Right; }

                if (g.DrawRectangle.Bottom + ScaleIt(scaledEdge) > InnerRectangle().Bottom) {
                    Height += g.DrawRectangle.Bottom - InnerRectangle().Bottom + ScaleIt(scaledEdge);
                    yGrew = true;
                }
                else if (!yGrew && shrink && g.DrawRectangle.Bottom < InnerRectangle().Bottom) { Height -= InnerRectangle().Bottom - g.DrawRectangle.Bottom; }
                Position(Position());
            }
        }

        public override void AddControl(GUIObject g)
        {
            if (g != null && !Controls.Contains(g)) {
                Controls.Add(g);
                g.ParentWindow = this;
            }
        }

        #region Draw
        public void DrawWindow(SpriteBatch spriteBatch)
        {
            if (Show())
            {
                spriteBatch.Draw(_texture, GetTopLeftDest(), GetTopLeftSource(), EnabledColor * Alpha());
                spriteBatch.Draw(_texture, GetTopMiddleDest(), GetTopMiddleSource(), EnabledColor * Alpha());
                spriteBatch.Draw(_texture, GetTopRightDest(), GetTopRightSource(), EnabledColor * Alpha());

                spriteBatch.Draw(_texture, GetLeftMiddleDest(), GetLeftMiddleSource(), EnabledColor * Alpha());
                spriteBatch.Draw(_texture, GetMiddleDest(), GetMiddleSource(), EnabledColor * Alpha());
                spriteBatch.Draw(_texture, GetRightMiddleDest(), GetRightMiddleSource(), EnabledColor * Alpha());

                spriteBatch.Draw(_texture, GetBottomLeftDest(), GetBottomLeftSource(), EnabledColor * Alpha());
                spriteBatch.Draw(_texture, GetBottomMiddleDest(), GetBottomMiddleSource(), EnabledColor * Alpha());
                spriteBatch.Draw(_texture, GetBottomRightDest(), GetBottomRightSource(), EnabledColor * Alpha());
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

        #region SourceRectangleMethods
        private float GetSourceInnerLeftEdge() { return _winData.SourceVector.X + _winData.LeftEdge; }
        private float GetSourceInnerRightEdge() { return GetSourceInnerLeftEdge() + _winData.Size; }

        private float GetSourceInnerTopEdge() { return _winData.SourceVector.Y + _winData.TopEdge; }
        private float GetSourceInnerBottomEdge() { return GetSourceInnerTopEdge() + _winData.Size; }

        private Rectangle GetTopLeftSource() { return Util.FloatRectangle(_winData.SourceVector.X, _winData.SourceVector.Y, _winData.LeftEdge, _winData.TopEdge); }
        private Rectangle GetTopMiddleSource() { return Util.FloatRectangle(GetSourceInnerLeftEdge(), _winData.SourceVector.Y, _winData.Size, _winData.TopEdge); }
        private Rectangle GetTopRightSource() { return Util.FloatRectangle(GetSourceInnerRightEdge(), _winData.SourceVector.Y, _winData.RightEdge, _winData.TopEdge); }

        private Rectangle GetLeftMiddleSource() { return Util.FloatRectangle(_winData.SourceVector.X, GetSourceInnerTopEdge(), _winData.LeftEdge, _winData.Size); }
        private Rectangle GetMiddleSource() { return Util.FloatRectangle(GetSourceInnerLeftEdge(), GetSourceInnerTopEdge(), _winData.Size, _winData.Size); }
        private Rectangle GetRightMiddleSource() { return Util.FloatRectangle(GetSourceInnerRightEdge(), GetSourceInnerTopEdge(), _winData.RightEdge, _winData.Size); }

        private Rectangle GetBottomLeftSource() {
            return Util.FloatRectangle(_winData.SourceVector.X, GetSourceInnerBottomEdge(), _winData.LeftEdge, _winData.BottomEdge); }
        private Rectangle GetBottomMiddleSource() {
            return Util.FloatRectangle(GetSourceInnerLeftEdge(), GetSourceInnerBottomEdge(), _winData.Size, _winData.BottomEdge); }
        private Rectangle GetBottomRightSource() {
            return Util.FloatRectangle(GetSourceInnerRightEdge(), GetSourceInnerBottomEdge(), _winData.RightEdge, _winData.BottomEdge); }
        #endregion

        #region DestRectangleMethods
        public int WidthEdges() { return _winData.WidthEdges(); }
        public int InnerWidth() { return Width - WidthEdges(); }
        public int InnerLeft() { return (int)Position().X + _winData.ScaledLeftEdge; }
        public int InnerRight() { return InnerLeft() + InnerWidth(); }

        public int HeightEdges() { return _winData.HeightEdges(); }
        public int InnerHeight() { return Height - HeightEdges(); }
        public int InnerTop() { return (int)Position().Y + _winData.ScaledTopEdge; }
        public int InnerBottom() { return InnerTop() + InnerHeight(); }

        private Rectangle GetTopLeftDest() { return Util.FloatRectangle((int)Position().X, (int)Position().Y, _winData.ScaledLeftEdge, _winData.ScaledTopEdge); }
        private Rectangle GetTopMiddleDest() { return Util.FloatRectangle(InnerLeft(), (int)Position().Y, InnerWidth(), _winData.ScaledTopEdge); }
        private Rectangle GetTopRightDest() { return Util.FloatRectangle(InnerRight(), (int)Position().Y, _winData.ScaledRightEdge, _winData.ScaledTopEdge); }

        private Rectangle GetLeftMiddleDest() { return Util.FloatRectangle((int)Position().X, InnerTop(), _winData.ScaledLeftEdge, InnerHeight()); }
        private Rectangle GetMiddleDest() { return Util.FloatRectangle(InnerLeft(), InnerTop(), InnerWidth(), InnerHeight()); }
        private Rectangle GetRightMiddleDest() { return Util.FloatRectangle(InnerRight(), InnerTop(), _winData.ScaledRightEdge, InnerHeight()); }

        private Rectangle GetBottomLeftDest() { return Util.FloatRectangle((int)Position().X, InnerBottom(), _winData.ScaledLeftEdge, _winData.ScaledBottomEdge); }
        private Rectangle GetBottomMiddleDest() { return Util.FloatRectangle(InnerLeft(), InnerBottom(), InnerWidth(), _winData.ScaledBottomEdge); }
        private Rectangle GetBottomRightDest() { return Util.FloatRectangle(InnerRight(), InnerBottom(), _winData.ScaledRightEdge, _winData.ScaledBottomEdge); }
        #endregion

        #endregion
        #region Location Retrival
        //Usable space needs to ignore the edges of the rectangle
        public Rectangle InnerRectangle()
        {
            return new Rectangle((int)Position().X + _winData.ScaledLeftEdge, (int)Position().Y + _winData.ScaledTopEdge, InnerWidth(), InnerHeight());
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
            return InnerRectangle().Location.ToVector2() + new Vector2(0, InnerWidth());
        }

        public Vector2 OuterTopLeft() { return Position(); }
        public Vector2 OuterTopRight() { return Position() + new Vector2(Width, 0); }
        public Vector2 OuterBottomLeft() { return Position() + new Vector2(0, Height); }
        public Vector2 OuterBottomRight() { return Position() + new Vector2(Width, Height); }

        #endregion
    }
}
