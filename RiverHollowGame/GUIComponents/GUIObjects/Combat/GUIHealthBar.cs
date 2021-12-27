using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers;
using System;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIHealthBar : GUIObject
    {
        BitmapFont _font;

        GUIImage _gBar;
        GUIImage _gFillStart;
        GUIImage _gFillMiddle;
        GUIImage _gFillEnd;

        GUIText _gText;

        double _iMax;
        double _iCurr;
        const int EDGE = 4;

        public GUIHealthBar(int current, int max)
        {
            _iCurr = current;
            _iMax = max;

            _font = DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY);

            _gBar = new GUIImage(new Rectangle(0, 154,22, 5), DataManager.COMBAT_TEXTURE);
            AddControl(_gBar);

            Width = _gBar.Width;
            Height = _gBar.Height;

            _gText = new GUIText("", _font);
            SetCurrentValue(current);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            RemoveControl(_gText);
        }

        public void SetCurrentValue(int current)
        {
            _iCurr = current/2;
            if(_iCurr > 0)
            {
                _gFillStart = new GUIImage(new Rectangle(23, 155, 1, 3), DataManager.COMBAT_TEXTURE);
                _gFillStart.ScaledMoveBy(1, 1);

                //Three for either edge of the bar
                int totalFillableWidth = Width - (3 * GameManager.CurrentScale);
                double percent = _iCurr / _iMax;

                //Get the number of squares to fill
                int fill = (int)Math.Round(percent * totalFillableWidth, MidpointRounding.AwayFromZero);

                int endSpace = 0;
                if(fill < totalFillableWidth) { endSpace = 2; }
                else { endSpace = 1; }

                _gFillMiddle = new GUIImage(new Rectangle(24, 155, 1, 3), (fill - endSpace) / GameManager.CurrentScale, 3, DataManager.COMBAT_TEXTURE); ;
                _gFillMiddle.SetScale(GameManager.CurrentScale);
                _gFillMiddle.AnchorAndAlignToObject(_gFillStart, SideEnum.Right, SideEnum.Top);

                _gFillEnd = new GUIImage(new Rectangle(25, 155, 1 + endSpace - 1, 3), DataManager.COMBAT_TEXTURE); ;
                _gFillEnd.AnchorAndAlignToObject(_gFillMiddle, SideEnum.Right, SideEnum.Top);

                AddControl(_gFillStart);
                AddControl(_gFillMiddle);
                AddControl(_gFillEnd);
            }
            else
            {
                RemoveControl(_gFillStart);
                RemoveControl(_gFillMiddle);
                RemoveControl(_gFillEnd);
            }

        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (Contains(mouse))
            {
                _gText.SetText(string.Format("{0}/{1}", _iCurr, _iMax));
                _gText.AlignToObject(this, SideEnum.Center, false);
                AddControl(_gText);
                rv = true;
            }
            return rv;
        }
    }
}