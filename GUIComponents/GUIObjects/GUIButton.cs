using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindow;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIButton : GUIObject
    {
        public static int BTN_WIDTH = 128;
        public static int BTN_HEIGHT = 64;
        protected SpriteFont _font;
        public bool IsMouseHovering = false;
        public bool Enabled = true;

        private GUIObject _btnObject;

        private GUIImage _gImage;

        private GUIWindow _gWindow;
        private GUIText _gText;

        public delegate void BtnClickDelegate ();
        private BtnClickDelegate _delAction;

        internal static WindowData BaseBtn = new WindowData(96, 0, 2);

        public GUIButton(string text, BtnClickDelegate del = null)
        {
            _gWindow = new GUIWindow(BaseBtn, BTN_WIDTH, BTN_HEIGHT);
            LoadWindowButton(text, BTN_WIDTH, BTN_HEIGHT, del);
        }

        public GUIButton(string text, int width, int height, BtnClickDelegate del = null)
        {
            _gWindow = new GUIWindow(BaseBtn, width, height);
            LoadWindowButton(text, width, height, del);
        }

        public void LoadWindowButton(string text, int width, int height, BtnClickDelegate del)
        {
            Width = width;
            Height = height;
            _gText = new GUIText(text);
            _gText.SetColor(Color.Red);
            _gText.CenterOnWindow(_gWindow);

            _btnObject = _gWindow;

            if (del != null) { _delAction = del; }
        }

        public GUIButton(Rectangle sourceRect, int width, int height, string texture, BtnClickDelegate del = null)
        {
            Width = width;
            Height = height;
            _gImage = new GUIImage(sourceRect, width, height, texture);

            _btnObject = _gImage;

            if (del != null) { _delAction = del; }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _btnObject.Alpha = Enabled ? 1.0f : 0.5f;
            _btnObject.Draw(spriteBatch);
        }

        public void SetDelegate(BtnClickDelegate del)
        {
            _delAction = del;
        }

        public override bool Contains(Point mouse)
        {
            return _btnObject.Contains(mouse);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse) && Enabled && _delAction != null)
            {
                _delAction();
                rv = true;
            }

            return rv;
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _btnObject.Position(value);
        }

        public override void Enable(bool value)
        {
            base.Enable(value);
            _btnObject.Enable(value);
        }
    }
}
