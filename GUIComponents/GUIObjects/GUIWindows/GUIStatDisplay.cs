using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Actors;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIStatDisplay : GUIObject
    {
        bool _bHover;
        SpriteFont _font;

        GUIImage _gLeft;
        GUIImage _gMid;
        GUIImage _gRight;
        GUIImage _gFillLeft;
        GUIImage _gFillMid;
        GUIImage _gFillRight;
        GUIText _gText;

        int _iMax;
        int _iCurr;

        int _iMidWidth;
        const int EDGE = 4;

        public delegate void DelegateRetrieveValues(ref int curr, ref int max);
        DelegateRetrieveValues _delAction;

        public GUIStatDisplay(DelegateRetrieveValues del, Color c, int width = 200)
        {
            _iMax = 0;
            _iCurr = 0;

            _delAction = del;
            _font = DataManager.GetFont(@"Fonts\Font");
            _iMidWidth = width - (EDGE * 2);

            _gLeft = new GUIImage(new Rectangle(48, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gMid = new GUIImage(new Rectangle(52, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gRight = new GUIImage(new Rectangle(60, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");

            _gFillLeft = new GUIImage(new Rectangle(64, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gFillMid = new GUIImage(new Rectangle(68, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gFillRight = new GUIImage(new Rectangle(76, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gText = new GUIText();

            _gLeft.SetColor(c);
            _gMid.SetColor(c);
            _gRight.SetColor(c);
            _gFillLeft.SetColor(c);
            _gFillMid.SetColor(c);
            _gFillRight.SetColor(c);

            AddControl(_gLeft);
            AddControl(_gMid);
            AddControl(_gRight);
            AddControl(_gFillLeft);
            AddControl(_gFillMid);
            AddControl(_gFillRight);

            PositionBars();

            _delAction(ref _iCurr, ref _iMax);

            Height = 16;
            Width = width;
        }

        public void PositionBars()
        {
            _gMid.AnchorAndAlignToObject(_gLeft, SideEnum.Right, SideEnum.CenterY);
            _gRight.AnchorAndAlignToObject(_gMid, SideEnum.Right, SideEnum.CenterY);
            _gFillMid.AnchorAndAlignToObject(_gFillLeft, SideEnum.Right, SideEnum.CenterY);
            _gFillRight.AnchorAndAlignToObject(_gFillMid, SideEnum.Right, SideEnum.CenterY);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(_iMax != 0)
            {
                _gFillMid.Width = (int)(_iMidWidth * _iCurr / _iMax);
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
                _gText.AlignToObject(_gMid, SideEnum.Center);
                AddControl(_gText);
                rv = true;
            }
            return rv;
        }
    }
}