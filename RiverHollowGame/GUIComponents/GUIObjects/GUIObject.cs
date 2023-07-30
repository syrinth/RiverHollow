using System;
using System.Collections.Generic;
using System.Runtime.Remoting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens;
using static RiverHollow.GUIComponents.GUIUtils;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIObject
    {
        private readonly List<GUIObject> ToAdd;
        private readonly List<GUIObject> ToRemove;
        internal List<GUIObject> Controls;

        public delegate void EmptyDelegate();

        public GUIObject Parent { get; private set; }

        public GUIScreen Screen { get; private set; }

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

        public int Top => _pPos.Y;
        public int Left => _pPos.X;
        public int Bottom => _pPos.Y + Height;
        public int Right => _pPos.X + Width;

        private Point _pPos;
        protected float _fAlpha = 1.0f;

        protected Rectangle _drawRect;
        public Rectangle DrawRectangle { get => _drawRect; }

        protected Rectangle _sourceRect;

        protected Color _Color = Color.White;
        public Color ObjColor => _Color;
        protected Texture2D _texture = DataManager.GetTexture(DataManager.HUD_COMPONENTS);
        protected Color EnabledColor => _bEnabled ? _Color : Color.Gray;
        protected bool _bEnabled = true;
        public bool Active => _bEnabled && Visible;

        public bool Visible { get; protected set; } = true;

        public bool HoverControls = true;
        protected bool _bMouseOver = false;
        protected bool _bInitScaleSet = false;
        protected Point _pInitVals;       //X = Width, Y = Height
        protected Point _pInitPos;

        public GUIObject() {
            Controls = new List<GUIObject>();
            ToAdd = new List<GUIObject>();
            ToRemove = new List<GUIObject>();

            Screen = GUIManager.NewScreen ?? GUIManager.CurrentScreen;
            Screen?.AddControl(this);
        }
        public GUIObject(GUIObject g) : this()
        {
            Position(g.Position());
            Width = g.Width;
            Height = g.Height;
        }

        public virtual bool Contains(Point mouse)
        {
            return Visible && DrawRectangle.Contains(mouse);
        }
        public virtual bool Contains(GUIObject obj)
        {
            return DrawRectangle.Contains(obj.DrawRectangle);
        }
        public virtual void Update(GameTime gTime) {
            foreach (GUIObject g in ToRemove)
            {
                Controls.Remove(g);
            }
            ToRemove.Clear();

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
            if (Visible)
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
            foreach(GUIObject obj in Controls)
            {
                obj.Enable(value);
            }
        }

        public virtual void SetColor(Color c)
        {
            _Color = c;
        }

        public virtual bool ProcessLeftButtonClick(Point mouse)
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

            //This consumes the left button click if the button is inside
            //the object but wasn't otherwise consumed.
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
                if (c.Contains(mouse))
                {
                    rv = c.ProcessRightButtonClick(mouse);
                    if (rv) { break; }
                }
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

            if (HoverControls)
            {
                for(int i = 0; i < Controls.Count; i++)
                {
                    rv = Controls[i].ProcessHover(mouse) || rv;
                }
            }

            if (!rv)
            {
                if (Contains(mouse))
                {
                    rv = true;
                    if (!_bMouseOver)
                    {
                        _bMouseOver = true;
                        BeginHover();
                    }
                }
                else if (_bMouseOver)
                {
                    _bMouseOver = false;
                    EndHover();
                }
            }

            return rv;
        }

        protected virtual void BeginHover() { }
        protected virtual void EndHover() { }

        public virtual Point Position()
        {
            return _pPos;
        }

        /// <summary>
        /// Adds the given value from the location of the GUIObject,
        /// then, it subtracts the same value from all controls of the GUIObject.
        /// 
        /// This recursively goes up the entire chain.
        /// </summary>
        /// <param name="value">The value to subtract from the GUIObject's location</param>
        /// <returns>The new value of the Object</returns>
        public Point PositionAdd(Point value) {
            _pPos += value;
            _drawRect.Location += value;

            foreach (GUIObject g in Controls)
            {
                g.PositionSub(value);
            }

            //This is an important redundancy, because it will ensure that any Positional
            //overrides are called. This is NEEDED for GUISprites.
            Position(_pPos);
            return _pPos;
        }

        /// <summary>
        /// Subtracts the given value from the location of the GUIObject,
        /// then, it subtracts the same value from all controls of the GUIObject.
        /// 
        /// This recursively goes up the entire chain.
        /// </summary>
        /// <param name="value">The value to subtract from the GUIObject's location</param>
        /// <returns>The new value of the Object</returns>
        public Point PositionSub(Point value)
        {
            _pPos -= value;
            _drawRect.Location -= value;

            foreach (GUIObject g in Controls)
            {
                g.PositionSub(value);
            }

            //This is an important redundancy, because it will ensure that any Positional
            //overrides are called. This is NEEDED for GUISprites.
            Position(_pPos);

            return _pPos;
        }
        public virtual void Position(GUIObject obj)
        {
            Position(obj.Position());
            AddParent(obj);
        }
        /// <summary>
        /// Set the location of this GUIObject to the indicated value.
        /// 
        /// Then, calculate how far it moved, and subtract that value 
        /// from the location of every control within the object.
        /// </summary>
        /// <param name="value">The new TopLeft value of the GUIObject</param>
        public virtual void Position(Point value)
        {
            Point startVec = Position();

            _pPos = value;
            _drawRect = new Rectangle((int)_pPos.X, (int)_pPos.Y, Width, Height);

            Point delta = startVec - value;

            foreach (GUIObject g in Controls)
            {
                g.PositionSub(delta);
            }
        }

        public void PositionAndMove(GUIObject obj, int xMove, int yMove)
        {
            PositionAndMove(obj, new Point(xMove, yMove));
        }
        public void PositionAndMove(GUIObject obj, Point offset)
        {
            Position(obj.Position());
            ScaledMoveBy(offset);
            AddParent(obj);
        }

        public virtual void SetScale(double x, bool anchorToPos = true)
        {
            if(!_bInitScaleSet)
            {
                _bInitScaleSet = true;

                _pInitPos = Position();
                _pInitVals = new Point(Width, Height);
            }

            int oldWidth = Width;
            int oldHeight = Height;

            if (x == 1)
            {
                Width = (int)_pInitVals.X;
                Height = (int)_pInitVals.Y;
                Position(_pInitPos);
            }
            else
            {
                Width = (int)(_pInitVals.X * x);
                Height = (int)(_pInitVals.Y * x);
            }

            int deltaWidth = Math.Abs(oldWidth - Width) / 2;
            int deltaHeight = Math.Abs(oldHeight - Height) / 2;

            if (!anchorToPos)
            {
                Point newPos = Position();
                if (_dScale < x)
                {
                    newPos -= new Point(deltaWidth, deltaHeight);
                }
                else
                {
                    newPos += new Point(deltaWidth, deltaHeight);
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

        public virtual void AddControls(params GUIObject[] obj)
        {
            foreach (GUIObject o in obj)
            {
                AddControl(o);
            }
        }
        public virtual void AddControl(GUIObject g)
        {
            if (g != null && g.Parent == null && !Controls.Contains(g))
            {
                Controls.Add(g);
                g.Parent = this;
                g.Screen?.RemoveControl(g);
                g.RemoveScreen();
            }
        }
        public virtual void AddControlDelayed(GUIObject g)
        {
            if (g != null && g.Parent == null && (!Controls.Contains(g) && !ToAdd.Contains(g)))
            {
                ToAdd.Add(g);
                g.Parent = this;
            }
        }
        public void CleanControls()
        {
            List<GUIObject> ControlCopy = new List<GUIObject>(Controls);
            for(int i = 0; i < ControlCopy.Count; i++)
            {
                RemoveControl(ControlCopy[i]);
            }
        }
        public virtual void RemoveControl(GUIObject control)
        {
            if (control != null && Controls.Contains(control))
            {
                Controls.Remove(control);
                control.Parent = null;
            }
        }

        private void RemoveScreen()
        {
            Screen = null;
        }

        public void RemoveSelfFromControl()
        {
            Parent?.RemoveControl(this);
            Screen?.RemoveControl(this);
        }

        #region Alpha
        public virtual void Alpha(float val) {
            _fAlpha = val;

            foreach(GUIObject g in Controls)
            {
                g.Alpha(val);
            }
        }

        public float Alpha() { return _fAlpha; }
        #endregion

        #region Positioning Code
        public enum SideEnum { Bottom, BottomLeft, BottomRight, Center, CenterX, CenterY, Left, Right, Top, TopLeft, TopRight, };
        public void ScaledMoveBy(int x, int y)
        {
            ScaledMoveBy(new Point(x, y));
        }
        private void ScaledMoveBy(Point dir)
        {
            MoveBy(GameManager.ScaleIt(dir.X), GameManager.ScaleIt(dir.Y));
        }
        public void MoveBy(Point dir)
        {
            MoveBy(dir.X, dir.Y);
        }
        public void MoveBy(int x, int y)
        {
            Point pos = Position();
            Position(new Point(pos.X + x, pos.Y + y));
        }
        public void SetX(int x) {
            Position(new Point(x, _pPos.Y));
        }
        public void SetY(int y)
        {
            Position(new Point(_pPos.X, y));
        }

        internal void AddParent(GUIObject focus, ParentRuleEnum rule = ParentRuleEnum.Auto)
        {
            switch (rule)
            {
                case ParentRuleEnum.Skip:
                    return;
                case ParentRuleEnum.ForceToParent:
                    focus.Parent?.AddControl(this);
                    break;
            }

            while (Parent == null)
            {
                if (focus != null && rule == ParentRuleEnum.ForceToObject || focus.DrawRectangle.Intersects(DrawRectangle))
                {
                    focus.AddControl(this);
                    break;
                }
                else if (focus.Parent != null)
                {
                    focus = focus.Parent;
                }
                else
                {
                    break;
                }
            }
        }

        internal void AnchorAndAlignThenMove(GUIObject focus, SideEnum sidePlacement, SideEnum sideToAlign, int moveX, int moveY)
        {
            AnchorAndAlign(focus, sidePlacement, sideToAlign);
            ScaledMoveBy(moveX, moveY);
            AddParent(focus);
        }

        internal void AnchorAndAlign(GUIObject focus, SideEnum sidePlacement, SideEnum sideToAlign, ParentRuleEnum rule = ParentRuleEnum.Auto)
        {
            AnchorAndAlignWithSpacing(focus, sidePlacement, sideToAlign, 0, rule);
        }
        internal void AnchorAndAlignWithSpacing(GUIObject focus, SideEnum sidePlacement, SideEnum sideToAlign, int spacing, ParentRuleEnum rule = ParentRuleEnum.Auto)
        {
            AnchorToObject(focus, sidePlacement, spacing, rule);
            AlignToObject(focus, sideToAlign, rule);
        }
        internal void AnchorToObject(GUIObject focus, SideEnum sidePlacement, int spacing = 0, ParentRuleEnum rule = ParentRuleEnum.Auto)
        {
            int scaledSpace = GameManager.ScaleIt(spacing);
            Point position = focus.Position();
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    position.X = this.Position().X;
                    position.Y += focus.Height + scaledSpace;
                    break;
                case SideEnum.Left:
                    position.X -= (this.Width + scaledSpace);
                    position.Y = this.Position().Y;
                    break;
                case SideEnum.Right:
                    position.X += focus.Width + scaledSpace;
                    position.Y = this.Position().Y;
                    break;
                case SideEnum.Top:
                    position.X = this.Position().X;
                    position.Y -= (this.Height + scaledSpace);
                    break;
            }
            this.Position(position);

            AddParent(focus, rule);
        }

        internal void AlignToObject(GUIObject focus, SideEnum sideToAlign, ParentRuleEnum rule = ParentRuleEnum.Auto)
        {
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

            AddParent(focus, rule);
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
        internal void AnchorToObjectInnerSide(GUIObject obj, SideEnum sidePlacement, int spacing = 0)
        {
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    this.SetY(obj.Bottom - this.Height - spacing);
                    this.AlignToObject(obj, SideEnum.CenterX);
                    break;
                case SideEnum.Left:
                    this.SetX(obj.Left + spacing);
                    this.AlignToObject(obj, SideEnum.CenterY);
                    break;
                case SideEnum.Right:
                    this.SetX(obj.Right - this.Width - spacing);
                    this.AlignToObject(obj, SideEnum.CenterY);
                    break;
                case SideEnum.Top:
                    this.SetY(obj.Top + spacing);
                    this.AlignToObject(obj, SideEnum.CenterX);
                    break;
                case SideEnum.BottomLeft:
                    this.SetY(obj.Bottom - this.Height - spacing);
                    this.SetX(obj.Left + spacing);
                    break;
                case SideEnum.BottomRight:
                    this.SetY(obj.Bottom - this.Height - spacing);
                    this.SetX(obj.Right - this.Width - spacing);
                    break;
                case SideEnum.TopLeft:
                    this.SetY(obj.Top + spacing);
                    this.SetX(obj.Left + spacing);
                    break;
                case SideEnum.TopRight:
                    this.SetY(obj.Top + spacing);
                    this.SetX(obj.Right - this.Width - spacing);
                    break;
                default:
                    break;
            }
        }
        internal void AnchorToInnerSide(GUIWindow window, SideEnum sidePlacement, int spacing = 0)
        {
            int scaledSpacing = GameManager.ScaleIt(spacing);
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    this.SetY(window.InnerBottom() - this.Height - scaledSpacing);
                    this.AlignToObject(window, SideEnum.CenterX);
                    break;
                case SideEnum.Left:
                    this.SetX(window.InnerLeft() + scaledSpacing);
                    this.AlignToObject(window, SideEnum.CenterY);
                    break;
                case SideEnum.Right:
                    this.SetX(window.InnerRight() - this.Width - scaledSpacing);
                    this.AlignToObject(window, SideEnum.CenterY);
                    break;
                case SideEnum.Top:
                    this.SetY(window.InnerTop() + scaledSpacing);
                    this.AlignToObject(window, SideEnum.CenterX);
                    break;
                case SideEnum.BottomLeft:
                    this.SetY(window.InnerBottom() - this.Height - scaledSpacing);
                    this.SetX(window.InnerLeft() + scaledSpacing);
                    break;
                case SideEnum.BottomRight:
                    this.SetY(window.InnerBottom() - this.Height - scaledSpacing);
                    this.SetX(window.InnerRight() - this.Width - scaledSpacing);
                    break;
                case SideEnum.TopLeft:
                    this.SetY(window.InnerTop() + scaledSpacing);
                    this.SetX(window.InnerLeft() + scaledSpacing);
                    break;
                case SideEnum.TopRight:
                    this.SetY(window.InnerTop() + scaledSpacing);
                    this.SetX(window.InnerRight() - this.Width - scaledSpacing);
                    break;
                default:
                    break;
            }
            AddParent(window, ParentRuleEnum.ForceToObject);
        }
        internal void AnchorToInnerSide(GUIObject obj, SideEnum sidePlacement, int spacing = 0)
        {
            int scaledSpacing = GameManager.ScaleIt(spacing);
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    this.SetY(obj.DrawRectangle.Bottom - this.Height - scaledSpacing);
                    break;
                case SideEnum.Left:
                    this.SetX(obj.DrawRectangle.Left + scaledSpacing);
                    break;
                case SideEnum.Right:
                    this.SetX(obj.DrawRectangle.Right - this.Width - scaledSpacing);
                    break;
                case SideEnum.Top:
                    this.SetY(obj.DrawRectangle.Top + scaledSpacing);
                    break;
                case SideEnum.BottomLeft:
                    this.SetY(obj.DrawRectangle.Bottom - this.Height - scaledSpacing);
                    this.SetX(obj.DrawRectangle.Left + scaledSpacing);
                    break;
                case SideEnum.BottomRight:
                    this.SetY(obj.DrawRectangle.Bottom - this.Height - scaledSpacing);
                    this.SetX(obj.DrawRectangle.Right - this.Width - scaledSpacing);
                    break;
                case SideEnum.TopLeft:
                    this.SetY(obj.DrawRectangle.Top + scaledSpacing);
                    this.SetX(obj.DrawRectangle.Left + scaledSpacing);
                    break;
                case SideEnum.TopRight:
                    this.SetY(obj.DrawRectangle.Top + scaledSpacing);
                    this.SetX(obj.DrawRectangle.Right - this.Width - scaledSpacing);
                    break;
                default:
                    break;
            }
        }
        internal void AnchorToScreen(SideEnum sidePlacement, int spacing = 0)
        {
            int screenHeight = RiverHollow.ScreenHeight;
            int screenWidth = RiverHollow.ScreenWidth;

            int scaledSpace = GameManager.ScaleIt(spacing);

            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    this.SetY(screenHeight - this.Height - scaledSpace);
                    this.SetX(screenWidth / 2 - this.Width / 2);
                    break;
                case SideEnum.Left:
                    this.SetY(screenHeight / 2 - this.Height / 2);
                    this.SetX(0 + scaledSpace);
                    break;
                case SideEnum.Right:
                    this.SetY(screenHeight / 2 - this.Height / 2);
                    this.SetX(screenWidth - this.Width - scaledSpace);
                    break;
                case SideEnum.Top:
                    this.SetY(0 + scaledSpace);
                    this.SetX(screenWidth / 2 - this.Width / 2);
                    break;
                case SideEnum.BottomLeft:
                    this.SetY(screenHeight - this.Height - scaledSpace);
                    this.SetX(0 + scaledSpace);
                    break;
                case SideEnum.BottomRight:
                    this.SetY(screenHeight - this.Height - scaledSpace);
                    this.SetX(screenWidth - this.Width - scaledSpace);
                    break;
                case SideEnum.TopLeft:
                    this.SetY(0 + scaledSpace);
                    this.SetX(0 + scaledSpace);
                    break;
                case SideEnum.TopRight:
                    this.SetY(0 + scaledSpace);
                    this.SetX(screenWidth - this.Width - scaledSpace);
                    break;
                default:
                    break;
            }
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
        internal void CenterOnObject(GUIObject obj, ParentRuleEnum rule = ParentRuleEnum.Auto)
        {
            int centerX = obj.Position().X + (obj.Width / 2);
            int centerY = obj.Position().Y + (obj.Height / 2);

            this.SetX(centerX - Width / 2);
            this.SetY(centerY - Height / 2);
            AddParent(obj, rule);
        }

        internal virtual void DetermineSize(int edge = 0)
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

                Width = iRight - iLeft + edge;
                Height = iBottom - iTop + edge;
            }
        }

        internal virtual void Show(bool val)
        {
            Visible = val;
            foreach(GUIObject obj in Controls)
            {
                obj.Show(val);
            }
        }
        internal bool Show()
        {
            return Visible;
        }

        #endregion
    }
}
