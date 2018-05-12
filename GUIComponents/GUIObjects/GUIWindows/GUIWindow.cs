using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIComponents.GUIObjects
{
    public class GUIWindow : GUIObject
    {
        const int DialogSize = 32;

        internal List<GUIObject> Controls;
        public struct WindowData
        {
            Vector2 _vSource;
            public Vector2 Src => _vSource;
            int _iEdge;
            public int Edge => _iEdge;
            int _iSize;
            public int Size => _iSize;  //The size of the non-edges

            public WindowData(int x, int y, int edge)
            {
                _vSource = new Vector2(x, y);
                _iEdge = edge;
                _iSize = DialogSize - (_iEdge * 2);
            }
        };
        internal static WindowData RedWin = new WindowData(0, 0, 5);
        internal static WindowData BrownWin = new WindowData(32, 0, 5);
        internal static WindowData GreyWin = new WindowData(64, 0, 5);

        protected const int SpaceFromBottom = 32;

        protected WindowData _winData;
        public int EdgeSize { get => _winData.Edge; }

        public GUIWindow()
        {
            Controls = new List<GUIObject>();
            Height = 148;
            Width = RiverHollow.ScreenWidth / 2;
            Position(new Vector2(RiverHollow.ScreenWidth / 4, RiverHollow.ScreenHeight - Height - SpaceFromBottom));

            _winData = RedWin;
        }

        public GUIWindow(WindowData winData, int width, int height)
        {
            Controls = new List<GUIObject>();
            Width = width;
            Height = height;

            _winData = winData;
        }

        public GUIWindow(Vector2 position, WindowData winData, int width, int height) : this()
        {
            Width = width;
            Height = height;
            _winData = winData;
            Position(position);
        }

        public override void Position(Vector2 value)
        {
            Vector2 delta = Position() - value;
            foreach (GUIObject g in Controls)
            {
                g.Position(g.PositionSub(delta));
            }
            base.Position(value);
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

        public void AddControl(GUIObject g)
        {
            if (!Controls.Contains(g)) { Controls.Add(g); }
        }
        #region Draw
        public int SkipSize() { return _winData.Size + _winData.Edge; }
        public void DrawWindow(SpriteBatch spriteBatch)
        {
            DrawTop(spriteBatch);
            DrawMiddle(spriteBatch);
            DrawBottom(spriteBatch);
        }
        public override void Draw(SpriteBatch spriteBatch)
        {
            DrawTop(spriteBatch);
            DrawMiddle(spriteBatch);
            DrawBottom(spriteBatch);

            foreach(GUIObject g in Controls)
            {
                g.Draw(spriteBatch);
            }
        }

        //Draw the edging
        public void DrawTop(SpriteBatch spriteBatch)
        {
            int BorderTop = (int)Position().Y;
            int BorderLeft = (int)Position().X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, BorderTop, _winData.Edge, _winData.Edge), new Rectangle((int)_winData.Src.X, (int)_winData.Src.Y, _winData.Edge, _winData.Edge), Color.White * _fAlpha);
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), BorderTop, MidWidth(), _winData.Edge), new Rectangle((int)_winData.Src.X + _winData.Edge, (int)_winData.Src.Y, _winData.Size, _winData.Edge), Color.White * _fAlpha);
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), BorderTop, _winData.Edge, _winData.Edge), new Rectangle((int)_winData.Src.X + SkipSize(), (int)_winData.Src.Y, _winData.Edge, _winData.Edge), Color.White * _fAlpha);
        }
        public void DrawMiddle(SpriteBatch spriteBatch)
        {
            DrawMiddleEdges(spriteBatch);
            DrawCenter(spriteBatch);
        }
        public void DrawMiddleEdges(SpriteBatch spriteBatch)
        {
            int BorderLeft = (int)Position().X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, MidStartY(), _winData.Edge, MidHeight()), new Rectangle((int)_winData.Src.X, (int)_winData.Src.Y + _winData.Edge, _winData.Edge, _winData.Size), Color.White * _fAlpha);
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), MidStartY(), _winData.Edge, MidHeight()), new Rectangle((int)_winData.Src.X + SkipSize(), (int)_winData.Src.Y + _winData.Edge, _winData.Edge, _winData.Size), Color.White * _fAlpha);
        }
        public void DrawCenter(SpriteBatch spriteBatch)
        {
            DrawCenter(spriteBatch, 1);
        }
        public void DrawCenter(SpriteBatch spriteBatch, float percentage)
        {
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), MidStartY(), (int)(MidWidth() * percentage), MidHeight()), new Rectangle((int)_winData.Src.X + _winData.Edge, (int)_winData.Src.Y + _winData.Edge, _winData.Size, _winData.Size), Color.White * _fAlpha);
        }
        public void DrawBottom(SpriteBatch spriteBatch)
        {
            int BorderLeft = (int)Position().X;

            spriteBatch.Draw(_texture, new Rectangle(BorderLeft, EndStartY(), _winData.Edge, _winData.Edge), new Rectangle((int)_winData.Src.X, (int)_winData.Src.Y + SkipSize(), _winData.Edge, _winData.Edge), Color.White * _fAlpha);
            spriteBatch.Draw(_texture, new Rectangle(MidStartX(), EndStartY(), MidWidth(), _winData.Edge), new Rectangle((int)_winData.Src.X + _winData.Edge, (int)_winData.Src.Y + SkipSize(), _winData.Size, _winData.Edge), Color.White * _fAlpha);
            spriteBatch.Draw(_texture, new Rectangle(EndStartX(), EndStartY(), _winData.Edge, _winData.Edge), new Rectangle((int)_winData.Src.X + SkipSize(), (int)_winData.Src.Y + SkipSize(), _winData.Edge, _winData.Edge), Color.White * _fAlpha);
        }

        public int MidStartX() { return (int)Position().X + _winData.Edge; }
        public int MidStartY() { return (int)Position().Y + _winData.Edge; }
        public int EndStartX() { return (int)Position().X + MidWidth() + _winData.Edge; }
        public int EndStartY() { return (int)Position().Y + MidHeight() + _winData.Edge; }
        public int MidHeight() { return Height - DblEdge(); }
        public int MidWidth() { return Width - DblEdge(); }
        #endregion
        #region Location Retrival
        //Usable space needs to ignore the edges of the rectangle
        public Rectangle InnerRectangle()
        {
            return new Rectangle((int)Position().X + _winData.Edge, (int)Position().Y + _winData.Edge, MidWidth(), MidHeight());
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
