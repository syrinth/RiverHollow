using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.GUIObjects
{
    public abstract class GUIObject
    {
        internal static Vector2 CenterScreen = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

        public int Height;
        public int Width;

        private Vector2 _vPos;

        protected Rectangle _drawRect;
        public Rectangle DrawRectangle { get => _drawRect; }

        protected Rectangle _sourceRect;

        protected Texture2D _texture = GameContentManager.GetTexture(@"Textures\Dialog");

        public virtual bool Contains(Point mouse)
        {
            return DrawRectangle.Contains(mouse);
        }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRect, Color.White);
        }

        public static Vector2 PosFromCenter(Vector2 center, int width, int height)
        {
            return new Vector2(center.X - width / 2, center.Y - height / 2);
        }
        public static Vector2 PosFromCenter(int x, int y, int width, int height)
        {
            return new Vector2(x - width / 2, y - height / 2);
        }
        public virtual bool ProcessLeftButtonClick(Point mouse)
        {
            return false;
        }

        public Vector2 Position()
        {
            return _vPos;
        }

        public Vector2 PositionAdd(Vector2 value){
            _vPos += value;
            return _vPos;
        }
        public Vector2 PositionSub(Vector2 value)
        {
            _vPos -= value;
            return _vPos;
        }
        public virtual void Position(Vector2 value)
        {
            _vPos = value;
            _drawRect = new Rectangle((int)_vPos.X, (int)_vPos.Y, Width, Height);
        }

        #region Positioning Code
        internal enum SideEnum { Bottom, BottomLeft, BottomRight, CenterX, CenterY, Left, Right, Top, TopLeft, TopRight, };
        public void SetX(float x) {
            Position(new Vector2(x, _vPos.Y));
        }
        public void SetY(float y)
        {
            Position(new Vector2(_vPos.X, y));
        }
        internal static void CreateSpacedColumn(ref List<GUIObject> components, int columnLine, float start, int totalHeight, int spacing, bool alignToColumnLine = false)
        {
            float startY = start + ((totalHeight - (components.Count * components[0].Height) - (spacing * components.Count - 1)) / 2) + components[0].Height / 2;
            Vector2 position = new Vector2(alignToColumnLine ? columnLine : columnLine - components[0].Width / 2, startY);

            foreach(GUIObject o in components)
            {
                o.Position(position);
                position.Y += o.Height + spacing;
            }
        }
        internal static void CreateSpacedRow(ref List<GUIObject> components, int rowLine, float start, int totalWidth, int spacing, bool alignToRowLine = false)
        {
            float startX = start + ((totalWidth - (components.Count * components[0].Width) - (spacing * components.Count - 1)) / 2) + components[0].Width / 2;
            Vector2 position = new Vector2(startX, alignToRowLine ? rowLine : rowLine - components[0].Height / 2);

            foreach (GUIObject o in components)
            {
                o.Position(position);
                position.X += o.Width + spacing;
            }
        }
        internal static void CenterAndAlignToScreen(ref List<GUIObject> components)
        {
            int top = (int)components[0].Position().Y;
            int bottom = (int)components[0].Position().Y + components[0].Height;
            int left = (int)components[0].Position().X;
            int right = (int)components[0].Position().X + components[0].Width;

            foreach (GUIObject o  in components)
            {
                top = (int)MathHelper.Min(top, o.Position().Y);
                bottom = (int)MathHelper.Max(bottom, o.Position().Y + o.Height);
                left = (int)MathHelper.Min(left, o.Position().X);
                right = (int)MathHelper.Max(right, o.Position().X + o.Width);
            }

            Vector2 stackCenter = new Rectangle(left, top, right - left, bottom - top).Center.ToVector2();
            Vector2 delta = CenterScreen - stackCenter;

            foreach (GUIObject o in components)
            {
                o.Position(o.Position() + delta);
            }
        }

        internal void AnchorAndAlignToObject(GUIObject focus, SideEnum sidePlacement, SideEnum sideToAlign, int spacing = 0)
        {
            AnchorToObject(focus, sidePlacement, spacing);
            AlignToObject(focus, sideToAlign);
        }
        internal void AnchorToObject(GUIObject focus, SideEnum sidePlacement, int spacing = 0)
        {
            Vector2 position = focus.Position();
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    position.X = this.Position().X;
                    position.Y += focus.Height + spacing;
                    break;
                case SideEnum.Left:
                    position.X -= this.Width + spacing;
                    position.Y = this.Position().Y;
                    break;
                case SideEnum.Right:
                    position.X += focus.Width + spacing;
                    position.Y = this.Position().Y;
                    break;
                case SideEnum.Top:
                    position.X = this.Position().X;
                    position.Y -= this.Height + spacing;
                    break;
            }
            this.Position(position);
        }
        internal void AnchorToInnerSide(GUIWindow window, SideEnum sidePlacement, int spacing = 0)
        {
            window.Controls.Add(this);
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    this.SetY(window.InnerBottom() - this.Height- spacing);
                    break;
                case SideEnum.Left:
                    this.SetX(window.InnerLeft() + spacing);
                    break;
                case SideEnum.Right:
                    this.SetX(window.InnerRight() - this.Width - spacing);
                    break;
                case SideEnum.Top:
                    this.SetY(window.InnerTop() + spacing);
                    break;
                case SideEnum.BottomLeft:
                    this.SetY(window.InnerBottom() - this.Height - spacing);
                    this.SetX(window.InnerLeft() + spacing);
                    break;
                case SideEnum.BottomRight:
                    this.SetY(window.InnerBottom() - this.Height - spacing);
                    this.SetX(window.InnerRight() - this.Width - spacing);
                    break;
                case SideEnum.TopLeft:
                    this.SetY(window.InnerTop() + spacing);
                    this.SetX(window.InnerLeft() + spacing);
                    break;
                case SideEnum.TopRight:
                    this.SetY(window.InnerTop()+ spacing);
                    this.SetX(window.InnerRight() - this.Width - spacing);
                    break;
                default:
                    break;
            }
        }
        internal void AnchorToScreen(SideEnum sidePlacement, int spacing = 0)
        {
            int screenHeight = RiverHollow.ScreenHeight;
            int screenWidth = RiverHollow.ScreenWidth;

            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    this.SetY(screenHeight - this.Height - spacing);
                    break;
                case SideEnum.Left:
                    this.SetX(0 + spacing);
                    break;
                case SideEnum.Right:
                    this.SetX(screenWidth - this.Width - spacing);
                    break;
                case SideEnum.Top:
                    this.SetY(0 + spacing);
                    break;
                case SideEnum.BottomLeft:
                    this.SetY(screenHeight - this.Height - spacing);
                    this.SetX(0 + spacing);
                    break;
                case SideEnum.BottomRight:
                    this.SetY(screenHeight - this.Height - spacing);
                    this.SetX(screenWidth - this.Width - spacing);
                    break;
                case SideEnum.TopLeft:
                    this.SetY(0 + spacing);
                    this.SetX(0 + spacing);
                    break;
                case SideEnum.TopRight:
                    this.SetY(0 + spacing);
                    this.SetX(screenWidth - this.Width - spacing);
                    break;
                default:
                    break;
            }
        }
        internal void AlignToObject(GUIObject focus, SideEnum sideToAlign)
        {
            Vector2 position = focus.Position();
            switch (sideToAlign)
            {
                case SideEnum.Bottom:
                    this.SetY(focus.Position().Y + focus.Height - this.Height);
                    break;
                case SideEnum.Left:
                    this.SetX(focus.Position().X);
                    break;
                case SideEnum.Right:
                    this.SetX(focus.Position().X + focus.Width - this.Width);
                    break;
                case SideEnum.Top:
                    this.SetY(focus.Position().Y);
                    break;
                case SideEnum.CenterX:
                    this.SetX(focus.DrawRectangle.Center.X - this.Width/2);
                    break;
                case SideEnum.CenterY:
                    this.SetX(focus.DrawRectangle.Center.Y - this.Height/2);
                    break;
                default:
                    break;
            }
        }

        internal void CenterOnScreen()
        {
            int centerX = RiverHollow.ScreenWidth / 2;
            int centerY = RiverHollow.ScreenHeight / 2;

            this.SetX(centerX - Width / 2);
            this.SetY(centerY - Height / 2);
        }
        internal void CenterOnWindow(GUIWindow win)
        {
            if (!win.Controls.Contains(this)) { win.Controls.Add(this); }
            float centerX = win.Position().X + (win.Width / 2);
            float centerY = win.Position().Y + (win.Height / 2);

            this.SetX(centerX - Width / 2);
            this.SetY(centerY - Height / 2);
        }

        #endregion
    }
}
