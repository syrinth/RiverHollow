using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIObjects;
using static RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindow;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIButton : GUIObject
    {
        public static int BTN_WIDTH = 208;
        public static int BTN_HEIGHT = 64;
        protected BitmapFont _font;
        public bool IsMouseHovering = false;

        private GUIObject _btnObject;

        private GUIImage _gImage;

        private GUIWindow _gWindow;
        private GUIText _gText;

        protected bool _bFadeOnDisable = true;
        public delegate void BtnClickDelegate ();
        private BtnClickDelegate _delAction;

        internal static WindowData BaseBtn = new WindowData(96, 0, 2, 12);

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
            _gText.CenterOnWindow(_gWindow);

            _btnObject = _gWindow;

            if (del != null) { _delAction = del; }
            Setup();
        }

        public GUIButton(Rectangle sourceRect, int width, int height, string texture, BtnClickDelegate del = null)
        {
            Width = width;
            Height = height;
            _gImage = new GUIImage(sourceRect, width, height, texture);

            _btnObject = _gImage;

            if (del != null) { _delAction = del; }

            Setup();
        }

        private void Setup()
        {
            AddControl(_gImage);
            AddControl(_btnObject);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Show)
            {
                if (_bFadeOnDisable) { _btnObject.Alpha(Enabled ? 1.0f : 0.5f); }
                _btnObject.Draw(spriteBatch);
            }
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

        public override void Enable(bool value)
        {
            base.Enable(value);
            _btnObject.Enable(value);
        }

        /// <summary>
        /// Called to toggle whether or not the button fades out when disabled.
        /// </summary>
        /// <param name="val">Whether the button should fade out of not</param>
        public void FadeOnDisable(bool val)
        {
            _bFadeOnDisable = val;
        }

        /// <summary>
        /// Sets the Slpha value for the Button and the button object
        /// </summary>
        /// <param name="val"></param>
        public override void Alpha(float val)
        {
            base.Alpha(val);
            _btnObject?.Alpha(val);
            _gText?.Alpha(val);
        }
    }
}
