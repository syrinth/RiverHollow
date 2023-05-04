using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers;
using static RiverHollow.GUIComponents.GUIObjects.GUIWindows.GUIWindow;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public class GUIButton : GUIImage
    {
        protected BitmapFont _font;

        protected GUIText _gText;

        protected bool _bFadeOnDisable = true;

        protected EmptyDelegate _delAction;

        internal static WindowData BaseBtn = new WindowData(96, 0, 2, 2, 2, 2, 12);

        public GUIButton(string text, EmptyDelegate del = null) : base(GUIUtils.BTN_MAIN, DataManager.HUD_COMPONENTS)
        {
            _gText = new GUIText(text);
            _gText.CenterOnObject(this);

            if (del != null)
            {
                _delAction = del;
            }
        }

        public GUIButton(Rectangle sourceRect, EmptyDelegate del, string texture = DataManager.HUD_COMPONENTS) : base(sourceRect, texture)
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
