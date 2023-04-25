using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIWindows.GUIWindow;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIButton : GUIImage
    {
        public static int BTN_WIDTH = 208;
        public static int BTN_HEIGHT = 64;
        protected BitmapFont _font;
        public bool IsMouseHovering = false;

        protected GUIText _gText;

        protected bool _bFadeOnDisable = true;

        protected EmptyDelegate _delAction;

        internal static WindowData BaseBtn = new WindowData(96, 0, 2, 2, 2, 2, 12);

        public GUIButton(string text, EmptyDelegate del = null) : base(new Rectangle(64, 0, 52, 16), DataManager.DIALOGUE_TEXTURE)
        {
            _gText = new GUIText(text);
            _gText.CenterOnObject(this);

            if (del != null)
            {
                _delAction = del;
            }
        }

        public GUIButton(Rectangle sourceRect, string texture, EmptyDelegate del = null) : base(sourceRect, texture)
        {
            if (del != null)
            {
                _delAction = del;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Show())
            {
                if (_bFadeOnDisable)
                {
                    Alpha(Active ? 1.0f : 0.5f);
                }
                base.Draw(spriteBatch);
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse) && Active && _delAction != null)
            {
                SoundManager.PlayEffect(SoundEffectEnum.Button);
                _delAction();
                rv = true;
            }

            return rv;
        }

        /// <summary>
        /// Called to toggle whether or not the button fades out when disabled.
        /// </summary>
        /// <param name="val">Whether the button should fade out of not</param>
        public void FadeOnDisable(bool val)
        {
            _bFadeOnDisable = val;
        }
    }
}
