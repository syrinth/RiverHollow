using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIComponents.GUIObjects;
using MonoGame.Extended.BitmapFonts;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIStatDisplay : GUIWindow
    {
        BitmapFont _font;

        GUIImage _gFill;
        GUIText _gText;

        int _iMax;
        int _iCurr;
        const int EDGE = 4;

        public delegate void DelegateRetrieveValues(ref int curr, ref int max);
        DelegateRetrieveValues _delAction;

        public GUIStatDisplay(DelegateRetrieveValues del, Color c, int width = 200) : base(DisplayWin, width, GameManager.ScaledTileSize/2)
        {
            _iMax = 0;
            _iCurr = 0;

            _delAction = del;
            _font = DataManager.GetBitMapFont(DataManager.FONT_STAT_DISPLAY);

            _gFill = new GUIImage(new Rectangle(65, 33, 14, 14), Width - WidthEdges(), Height - HeightEdges(), @"Textures\Dialog");
            _gFill.AnchorToInnerSide(this, SideEnum.TopLeft);
            _gText = new GUIText("", _font);

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
                _gText.AlignToObject(this, SideEnum.Center);
                AddControl(_gText);
                rv = true;
            }
            return rv;
        }
    }
}