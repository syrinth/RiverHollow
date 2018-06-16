using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIButton : GUIWindow
    {
        public static int BTN_WIDTH = 128;
        public static int BTN_HEIGHT = 64;
        protected SpriteFont _font;
        public bool IsMouseHovering = false;
        public bool Enabled;
        private GUIText _text;

        public delegate void BtnClickDelegate ();
        private BtnClickDelegate _delAction;

        internal static WindowData BaseBtn = new WindowData(96, 0, 2);

        public GUIButton(string text)
        {
            Controls = new List<GUIObject>();
            _winData = BaseBtn;
            Position(Vector2.Zero);
            Width = BTN_WIDTH;
            Height = BTN_HEIGHT;
            _text = new GUIText(text);
            _text.SetColor(Color.Red);
            _text.CenterOnWindow(this);

            Enabled = true;
        }

        public GUIButton(string text, int width, int height, BtnClickDelegate del = null) : this(text)
        {
            Width = width;
            Height = height;
            _text.CenterOnWindow(this);

            if(del != null) { _delAction = del; }
        }

        public GUIButton(Rectangle sourceRect, int width, int height, BtnClickDelegate del = null) : this("", width, height, del)
        {
        }

        public GUIButton(Vector2 position, Rectangle sourceRect, int width, int height, string text, string texture, bool usePosition = false) : this (text, width, height)
        {
            Position(usePosition ? position : position - new Vector2(width / 2, height / 2));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _fAlpha = Enabled ? 1.0f : 0.5f;

            base.Draw(spriteBatch);
            _text.Draw(spriteBatch);
        }

        public void SetDelegate(BtnClickDelegate del)
        {
            _delAction = del;
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse) && Enabled)
            {
                _delAction();
            }

            return rv;
        }
    }
}
