using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using System.Collections.Generic;

namespace RiverHollow.GUIObjects
{
    public abstract class GUIObject
    {
        public int Height;
        public int Width;

        private Vector2 _vPos;
        public Vector2 Position {
            get => _vPos;
            set {
                _vPos = value;
                _drawRect = new Rectangle((int)_vPos.X, (int)_vPos.Y, Width, Height);
            }
        }

        protected Rectangle _drawRect;
        public Rectangle DrawRectangle { get => _drawRect; }

        protected Rectangle _sourceRect;
        public Rectangle SourceRectangle { get => _sourceRect; }

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


        #region Positioning Code
        internal enum SideEnum { Bottom, BottomLeft, BottomRight, Left, Right, Top, TopLeft, TopRight, };
        public void SetX(float x) {
            _vPos.X = x;
            Position = _vPos;
        }
        public void SetY(float y)
        {
            _vPos.Y = y;
            Position = _vPos;
        }
        public static void CreateSpacedColumn(ref List<GUIObject> components, int columnLine, int totalHeight, int spacing, int width, int height, bool aligntoColumnLine = false)
        {
            int startY = ((totalHeight - (components.Count * height) - (spacing * components.Count - 1)) / 2) + height / 2;
            Vector2 position = new Vector2(aligntoColumnLine ? columnLine : columnLine - width / 2, startY);

            foreach(GUIObject o in components)
            {
                o.Height = height;
                o.Width = width;
                o.Position = position;
                position.Y += height + spacing;
            }
        }
        
        internal void PlaceAndAlignObject(GUIObject focus, SideEnum sidePlacement, SideEnum sideToAlign, int spacing)
        {
            PlaceByObject(focus, sidePlacement, spacing);
            AlignToObject(focus, sideToAlign);
        }
        internal void PlaceByObject(GUIObject focus, SideEnum sidePlacement, int spacing)
        {
            Vector2 position = focus.Position;
            switch (sidePlacement)
            {
                case SideEnum.Bottom:
                    position.Y += focus.Height + spacing;
                    break;
                case SideEnum.Left:
                    position.X -= spacing + this.Width;
                    break;
                case SideEnum.Right:
                    position.X += focus.Width + spacing;
                    break;
                case SideEnum.Top:
                    position.Y -= focus.Height + spacing;
                    break;
            }
            this.Position = position;
        }
        internal void AlignToObject(GUIObject focus, SideEnum sideToAlign)
        {
            Vector2 position = focus.Position;
            switch (sideToAlign)
            {
                case SideEnum.Bottom:
                    this.SetY(focus.Position.Y + Height);
                    break;
                case SideEnum.Left:
                    this.SetX(focus.Position.X);
                    break;
                case SideEnum.Right:
                    this.SetX(focus.Position.X + Width);
                    break;
                case SideEnum.Top:
                    this.SetY(focus.Position.Y);
                    break;
            }
        }

        #endregion
    }
}
