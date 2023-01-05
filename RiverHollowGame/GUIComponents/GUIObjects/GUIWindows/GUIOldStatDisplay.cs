using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Game_Managers;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIOldStatDisplay : GUIWindow
    {
        BitmapFont _font;

        GUIImage _gFill;
        GUIText _gText;

        double _iMax;
        double _iCurr;
        const int EDGE = 4;

        public delegate void DelegateRetrieveValues(ref double curr, ref double max);
        DelegateRetrieveValues _delAction;

        public GUIOldStatDisplay(DelegateRetrieveValues del, Color c, int width = 200) : base(DisplayWin, width, GameManager.ScaledTileSize/2)
        {
            _iMax = 0;
            _iCurr = 0;

            _delAction = del;
            _font = DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY);

            _gFill = new GUIImage(new Rectangle(65, 33, 14, 14), Width - WidthEdges(), Height - HeightEdges(), DataManager.DIALOGUE_TEXTURE);
            _gFill.AnchorToInnerSide(this, SideEnum.TopLeft);
            _gText = new GUIText("", _font);
            _gText.SetColor(Color.White);

            SetColor(c);
            _gFill.SetColor(c);

            _delAction(ref _iCurr, ref _iMax);
        }


        public override void Draw(SpriteBatch spriteBatch)
        {
            if(_iMax != 0)
            {
                _gFill.Width = (int)((Width - WidthEdges()) * _iCurr / _iMax);
            }
            base.Draw(spriteBatch);
            RemoveControl(_gText);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            _delAction(ref _iCurr, ref _iMax);
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