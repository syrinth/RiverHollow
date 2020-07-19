using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using System;
using System.Collections.Generic;

namespace RiverHollow.GUIObjects
{
    public class GUIObject
    {
        public GUIObject MemberOf;
        private List<GUIObject> ToAdd;
        internal List<GUIObject> Controls;

        GUIWindow _parentControl;
        public GUIWindow ParentWindow
        {
            get { return _parentControl; }
            set { _parentControl = value; }
        }

        GUIScreen _parentScreen;
        public GUIScreen ParentScreen
        {
            get { return _parentScreen; }
            set { _parentScreen = value; }
        }
        internal static Vector2 CenterScreen = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);

        protected double _dScale = 1;

        int _iHeight;
        public int Height
        {
            get { return _iHeight; }
            set
            {
                _iHeight = value;
                _drawRect = new Rectangle(_drawRect.X, _drawRect.Y, Width, Height);
            }
        }
        int _iWidth;
        public int Width
        {
            get { return _iWidth; }
            set {
                _iWidth = value;
                _drawRect = new Rectangle(_drawRect.X, _drawRect.Y, Width, Height);
            }
        }

        public int Top => (int)_vPos.Y;
        public int Left => (int)_vPos.X;
        public int Bottom => (int)_vPos.Y + Height;
        public int Right => (int)_vPos.X + Width;

        private Vector2 _vPos;
        protected float _fAlpha = 1.0f;

        protected Rectangle _drawRect;
        public Rectangle DrawRectangle { get => _drawRect; }

        protected Rectangle _sourceRect;

        protected Color _cColor = Color.White;
        protected Texture2D _texture = DataManager.GetTexture(@"Textures\Dialog");
        protected Color EnabledColor => _bEnabled ? _cColor : Color.Gray;
        protected bool _bEnabled = true;
        public bool Enabled => _bEnabled;

        public bool Show = true;

        protected bool _bInitScaleSet = false;
        protected Vector2 _vInitVals;       //X = Width, Y = Height
        protected Vector2 _vInitPos;

        public GUIObject() {
            Controls = new List<GUIObject>();
            ToAdd = new List<GUIObject>();
        }
        public GUIObject(GUIObject g) : this()
        {
            Position(g.Position());
            Width = g.Width;
            Height = g.Height;
        }

        public virtual bool Contains(Point mouse)
        {
            return Show && DrawRectangle.Contains(mouse);
        }
        public virtual void Update(GameTime gTime) {
            foreach (GUIObject g in ToAdd)
            {
                Controls.Add(g);
            }
            ToAdd.Clear();

            foreach (GUIObject g in Controls)
            {
                g.Update(gTime);
            }
        }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (Show)
            {
                spriteBatch.Draw(_texture, _drawRect, _sourceRect, EnabledColor * _fAlpha);

                foreach (GUIObject g in Controls)
                {
                    g.Draw(spriteBatch);
                }
            }
        }

        public virtual void Enable(bool value)
        {
            _bEnabled = value;
        }

        public virtual void SetColor(Color c)
        {
            _cColor = c;
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
            bool rv = false;

            foreach(GUIObject c in Controls)
            {
                rv = c.ProcessLeftButtonClick(mouse);
            }

            if (!rv)
            {
                rv = Contains(mouse);
            }

            return rv;
        }
        public virtual bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject c in Controls)
            {
                rv = c.ProcessRightButtonClick(mouse);

                if (rv) { break; }
            }

            if (!rv)
            {
                rv = Contains(mouse);
            }

            return rv;
        }
        public virtual bool ProcessHover(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject c in Controls)
            {
                rv = c.ProcessHover(mouse);
            }

            if (!rv)
            {
                rv = Contains(mouse);
            }

            return rv;
        }

        public Vector2 Position()
        {
            return _vPos;
        }

        /// <summary>
        /// Adds the given value from the location of the GUIObject,
        /// then, it subtracts the same value from all controls of the GUIObject.
        /// 
        /// This recursively goes up the entire chain.
        /// </summary>
        /// <param name="value">The value to subtract from the GUIObject's location</param>
        /// <returns>The new value of the Object</returns>
        public Vector2 PositionAdd(Vector2 value) {
            _vPos += value;
            _drawRect.Location += value.ToPoint();

            foreach (GUIObject g in Controls)
            {
                g.PositionSub(value);
            }

            //This is an important redundancy, because it will ensure that any Positional
            //overrides are called. This is NEEDED for GUISprites.
            Position(_vPos);
            return _vPos;
        }

        /// <summary>
        /// Subtracts the given value from the location of the GUIObject,
        /// then, it subtracts the same value from all controls of the GUIObject.
        /// 
        /// This recursively goes up the entire chain.
        /// </summary>
        /// <param name="value">The value to subtract from the GUIObject's location</param>
        /// <returns>The new value of the Object</returns>
        public Vector2 PositionSub(Vector2 value)
        {
            _vPos -= value;
            _drawRect.Location -= value.ToPoint();

            foreach (GUIObject g in Controls)
            {
                g.PositionSub(value);
            }

            //This is an important redundancy, because it will ensure that any Positional
            //overrides are called. This is NEEDED for GUISprites.
            Position(_vPos);

            return _vPos;
        }
        /// <summary>
        /// Set the location of this GUIObject to the indicated value.
        /// 
        /// Then, calculate how far it moved, and subtract that value 
        /// from the location of every control within the object.
        /// </summary>
        /// <param name="value">The new TopLeft value of the GUIObject</param>
        public virtual void Position(Vector2 value)
        {
            Vector2 startVec = Position();

            _vPos = value;
            _drawRect = new Rectangle((int)_vPos.X, (int)_vPos.Y, Width, Height);

            Vector2 delta = startVec - value;

            foreach (GUIObject g in Controls)
            {
                g.PositionSub(delta);
            }
        }

        public virtual void SetScale(double x, bool anchorToPos = true)
        {
            if(!_bInitScaleSet)
            {
                _bInitScaleSet = true;

                _vInitPos = Position();
                _vInitVals = new Vector2(Width, Height);
            }

            int oldWidth = Width;
            int oldHeight = Height;

            if (x == 1)
            {
                Width = (int)_vInitVals.X;
                Height = (int)_vInitVals.Y;
                Position(_vInitPos);
            }
            else
            {
                Width = (int)(_vInitVals.X * x);
                Height = (int)(_vInitVals.Y * x);
            }

            int deltaWidth = Math.Abs(oldWidth - Width) / 2;
            int deltaHeight = Math.Abs(oldHeight - Height) / 2;

            if (!anchorToPos)
            {
                Vector2 newPos = Position();
                if (_dScale < x)
                {
                    newPos -= new Vector2(deltaWidth, deltaHeight);
                }
                else
                {
                    newPos += new Vector2(deltaWidth, deltaHeight);
                }
                Position(newPos);
            }

            _dScale = x;
        }
        public virtual void AddControls(List<GUIObject> list)
        {
            foreach (GUIObject o in list)
            {
                AddControl(o);
            }
        }
        public virtual void AddControl(GUIObject g)
        {
            if (g != null && g.MemberOf == null && !Controls.Contains(g))
            {
                Controls.Add(g);
                g.MemberOf = this;
            }
        }
        public virtual void AddControlDelayed(GUIObject g)
        {
            if (g != null && g.MemberOf == null && (!Controls.Contains(g) && !ToAdd.Contains(g)))
            {
                ToAdd.Add(g);
                g.MemberOf = this;
            }
        }
        public virtual void RemoveControl(GUIObject control)
        {
            if (control != null && Controls.Contains(control))
            {
                Controls.Remove(control);
                control.MemberOf = null;
            }
        }

        #region Alpha
        public virtual void Alpha(float val) { _fAlpha = val; }

        public float Alpha() { return _fAlpha; }
        #endregion

        #region Positioning Code
        public enum SideEnum { Bottom, BottomLeft, BottomRight, Center, CenterX, CenterY, Left, Right, Top, TopLeft, TopRight, };
        public void MoveBy(Vector2 dir)
        {
            MoveBy(dir.X, dir.Y);
        }
        public void MoveBy(float x, float y)
        {
            Vector2 pos = Position();
            Position(new Vector2(pos.X + x, pos.Y + y));
        }
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

            foreach (GUIObject o in components)
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
        internal static void CreateSpacedGrid(ref List<GUIObject> components, Vector2 start, int totalWidth, int columns, int spacing = 0)
        {
            if (components.Count == 0) { return; }
            if (spacing == 0) { spacing = (totalWidth - (components[0].Width * columns)) / columns; }

            int i = 0; int j = 0;
            for (int index = 0; index < components.Count; index++)
            {
                GUIObject g = components[index];
                if (index == 0) { g.Position(start); }
                else
                {
                    i++;
                    if (i == columns)
                    {
                        i = 0;
                        j++;
                        g.AnchorAndAlignToObject(components[index - columns], SideEnum.Bottom, SideEnum.Left, spacing);
                    }
                    else
                    {
                        g.AnchorAndAlignToObject(components[index - 1], SideEnum.Right, SideEnum.Top, spacing);
                    }
                }
            }
        }
        internal static void CenterAndAlignToScreen(ref List<GUIObject> components)
        {
            int top = (int)components[0].Position().Y;
            int bottom = (int)components[0].Position().Y + components[0].Height;
            int left = (int)components[0].Position().X;
            int right = (int)components[0].Position().X + components[0].Width;

            foreach (GUIObject o in components)
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
        internal Vector2 GetAnchorAndAlignToObject(GUIObject focus, SideEnum sidePlacement, SideEnum sideToAlign, int spacing = 0)
        {
            GUIObject g = new GUIObject(this);
            g.AnchorToObject(focus, sidePlacement, spacing);
            g.AlignToObject(focus, sideToAlign);

            return g.Position();
        }
        internal void AnchorToObject(GUIObject focus, SideEnum sidePlacement, int spacing = 0)
        {
            if (focus.ParentWindow != null) { focus.ParentWindow.AddControl(this); }

            Vector2 position = focus.Position();
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    position.X = this.Position().X;
                    position.Y += focus.Height + spacing;
                    break;
                case SideEnum.Left:
                    position.X -= (this.Width + spacing);
                    position.Y = this.Position().Y;
                    break;
                case SideEnum.Right:
                    position.X += focus.Width + spacing;
                    position.Y = this.Position().Y;
                    break;
                case SideEnum.Top:
                    position.X = this.Position().X;
                    position.Y -= (this.Height + spacing);
                    break;
            }
            this.Position(position);
        }
        internal Vector2 GetAnchorToObject(GUIWindow focus, SideEnum sidePlacement, int spacing = 0)
        {
            GUIObject g = new GUIObject(this);
            g.AnchorToObject(focus, sidePlacement, spacing);

            return g.Position();
        }

        internal void AffixToCenter(GUIWindow window, SideEnum whichCenter, bool onMain, int spacing = 0)
        {
            window.AddControl(this);
            switch (whichCenter)
            {
                case SideEnum.CenterX:
                    this.SetX(window.DrawRectangle.Center.X - (!onMain ? this.Width : 0));
                    break;
                case SideEnum.CenterY:
                    this.SetY(window.DrawRectangle.Center.Y - (!onMain ? this.Height : 0));
                    break;
                default:
                    break;
            }

        }
        internal Vector2 GetAffixToCenter(GUIWindow window, SideEnum whichCenter, bool onMain, int spacing = 0)
        {
            GUIObject g = new GUIObject(this);
            g.AffixToCenter(window, whichCenter, onMain, spacing);

            return g.Position();
        }
        internal void AnchorToInnerSide(GUIWindow window, SideEnum sidePlacement, int spacing = 0)
        {
            window.AddControl(this);
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    this.SetY(window.InnerBottom() - this.Height - spacing);
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
                    this.SetY(window.InnerTop() + spacing);
                    this.SetX(window.InnerRight() - this.Width - spacing);
                    break;
                default:
                    break;
            }
        }
        internal Vector2 GetAnchorToInnerSide(GUIWindow window, SideEnum sidePlacement, int spacing = 0)
        {
            GUIObject g = new GUIObject(this);
            g.AnchorToInnerSide(window, sidePlacement, spacing);

            return g.Position();
        }
        internal void AnchorToInnerSide(GUIObject obj, SideEnum sidePlacement, int spacing = 0)
        {
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    this.SetY(obj.DrawRectangle.Bottom - this.Height - spacing);
                    break;
                case SideEnum.Left:
                    this.SetX(obj.DrawRectangle.Left + spacing);
                    break;
                case SideEnum.Right:
                    this.SetX(obj.DrawRectangle.Right - this.Width - spacing);
                    break;
                case SideEnum.Top:
                    this.SetY(obj.DrawRectangle.Top + spacing);
                    break;
                case SideEnum.BottomLeft:
                    this.SetY(obj.DrawRectangle.Bottom - this.Height - spacing);
                    this.SetX(obj.DrawRectangle.Left + spacing);
                    break;
                case SideEnum.BottomRight:
                    this.SetY(obj.DrawRectangle.Bottom - this.Height - spacing);
                    this.SetX(obj.DrawRectangle.Right - this.Width - spacing);
                    break;
                case SideEnum.TopLeft:
                    this.SetY(obj.DrawRectangle.Top + spacing);
                    this.SetX(obj.DrawRectangle.Left + spacing);
                    break;
                case SideEnum.TopRight:
                    this.SetY(obj.DrawRectangle.Top + spacing);
                    this.SetX(obj.DrawRectangle.Right - this.Width - spacing);
                    break;
                default:
                    break;
            }
        }
        internal Vector2 GetAnchorToInnerSide(GUIObject obj, SideEnum sidePlacement, int spacing = 0)
        {
            GUIObject g = new GUIObject(this);
            g.AnchorToInnerSide(obj, sidePlacement, spacing);

            return g.Position();
        }
        internal void AnchorToScreen(GUIScreen screen, SideEnum sidePlacement, int spacing = 0)
        {
            screen.AddControl(this);
            AnchorToScreen(sidePlacement, spacing);
        }
        internal void AnchorToScreen(SideEnum sidePlacement, int spacing = 0)
        {
            int screenHeight = RiverHollow.ScreenHeight;
            int screenWidth = RiverHollow.ScreenWidth;

            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    this.SetY(screenHeight - this.Height - spacing);
                    this.SetX(screenWidth / 2 - this.Width / 2);
                    break;
                case SideEnum.Left:
                    this.SetY(screenHeight / 2 - this.Height / 2);
                    this.SetX(0 + spacing);
                    break;
                case SideEnum.Right:
                    this.SetY(screenHeight / 2 - this.Height / 2);
                    this.SetX(screenWidth - this.Width - spacing);
                    break;
                case SideEnum.Top:
                    this.SetY(0 + spacing);
                    this.SetX(screenWidth / 2 - this.Width / 2);
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
        internal Vector2 GetAnchorToScreen(SideEnum sidePlacement, int spacing = 0)
        {
            GUIObject g = new GUIObject(this);
            g.AnchorToScreen(sidePlacement, spacing);

            return g.Position();
        }
        internal void AlignToObject(GUIObject focus, SideEnum sideToAlign, bool AddToParentWindow = true)
        {
            if (focus.ParentWindow != null && AddToParentWindow) { focus.ParentWindow.AddControl(this); }

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
                    this.SetX(focus.DrawRectangle.Center.X - this.Width / 2);
                    break;
                case SideEnum.CenterY:
                    this.SetY(focus.DrawRectangle.Center.Y - this.Height / 2);
                    break;
                case SideEnum.Center:
                    this.SetX(focus.DrawRectangle.Center.X - this.Width / 2);
                    this.SetY(focus.DrawRectangle.Center.Y - this.Height / 2);
                    break;
                default:
                    break;
            }
        }
        internal Vector2 GetAlignToObject(GUIObject focus, SideEnum sideToAlign)
        {
            GUIObject g = new GUIObject(this);
            g.AlignToObject(focus, sideToAlign);

            return g.Position();
        }

        internal void CenterOnScreen(GUIScreen screen)
        {
            screen.AddControl(this);
            CenterOnScreen();
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
            if (!win.Controls.Contains(this)) { win.AddControl(this); }
            this.CenterOnObject(win);
        }
        internal void CenterOnObject(GUIObject obj)
        {
            float centerX = obj.Position().X + (obj.Width / 2);
            float centerY = obj.Position().Y + (obj.Height / 2);

            this.SetX(centerX - Width / 2);
            this.SetY(centerY - Height / 2);
        }

        internal void DetermineSize()
        {
            if (Controls.Count > 0)
            {
                int iTop = Controls[0].Top;
                int iBottom = Controls[0].Bottom;
                int iLeft = Controls[0].Left;
                int iRight = Controls[0].Right;
                foreach (GUIObject obj in Controls)
                {
                    if (obj.Top < iTop) { iTop = obj.Top; }
                    if (obj.Left < iLeft) { iLeft = obj.Left; }
                    if (obj.Bottom > iBottom) { iBottom = obj.Bottom; }
                    if (obj.Right > iRight) { iRight = obj.Right; }
                }

                Width = iRight - iLeft;
                Height = iBottom - iTop;
            }
        }

        #endregion
    }
}
