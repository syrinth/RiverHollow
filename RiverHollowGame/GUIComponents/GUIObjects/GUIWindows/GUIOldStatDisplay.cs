﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIOldStatDisplay : GUIWindow
    {
        readonly GUIImage _gFill;
        readonly GUIText _gText;

        float _fMax;
        float _fCurr;

        public delegate void DelegateRetrieveValues(ref float curr, ref float max);
        readonly DelegateRetrieveValues _delAction;

        public GUIOldStatDisplay(DelegateRetrieveValues del, Color c, int width = 200) : base(GUIUtils.WINDOW_DISPLAY, width, GameManager.ScaledTileSize/2)
        {
            HoverControls = false;

            _fMax = 0;
            _fCurr = 0;

            _delAction = del;

            _gFill = new GUIImage(GUIUtils.HUD_FILL, Width - WidthEdges(), Height - HeightEdges(), DataManager.HUD_COMPONENTS);
            _gFill.AnchorToInnerSide(this, SideEnum.TopLeft);

            _gText = new GUIText("", true, DataManager.FONT_NUMBERS);
            _gText.SetColor(Color.White);
            _gText.AlignToObject(this, SideEnum.Center);

            SetColor(c);
            _gFill.SetColor(c);

            _delAction(ref _fCurr, ref _fMax);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(_fMax != 0)
            {
                _gFill.Width = (int)((Width - WidthEdges()) * _fCurr / _fMax);
            }
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            _delAction(ref _fCurr, ref _fMax);
        }

        protected override void BeginHover()
        {
            _gText.SetText(string.Format("{0}/{1}", (int)_fCurr, (int)_fMax));
            _gText.AlignToObject(this, SideEnum.Center);
            _gText.Show(true);
        }

        protected override void EndHover()
        {
            _gText.SetText("");
            _gText.Show(false);
        }
    }
}