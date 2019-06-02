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

        int _iMidWidth;
        const int EDGE = 4;

        public delegate void DelegateRetrieveValues(ref int curr, ref int max);
        DelegateRetrieveValues _delAction;

        public GUIStatDisplay(DelegateRetrieveValues del, Color c, int width = 200)
        {
            _delAction = del;
            _font = GameContentManager.GetFont(@"Fonts\Font");
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

            PositionBars();

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
            int curr = 0;
            int max = 0;

            _delAction(ref curr, ref max);

            _gFillLeft.Draw(spriteBatch);
            _gFillMid.Width = (int)(_iMidWidth * curr/max);
            _gFillMid.Draw(spriteBatch);
            _gFillRight.Draw(spriteBatch);
            _gLeft.Draw(spriteBatch);
            _gMid.Draw(spriteBatch);
            _gRight.Draw(spriteBatch);

            if (_bHover)
            {
                _gText.SetText(string.Format("{0}/{1}", curr, max));
                _gText.AlignToObject(_gMid, SideEnum.Center);
                _gText.Draw(spriteBatch);
            }
        }

        public override bool ProcessHover(Point mouse)
        {
            _bHover = _gLeft.Contains(mouse) || _gMid.Contains(mouse) || _gRight.Contains(mouse);
            return _bHover;
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            _gLeft.Position(value);
            _gFillLeft.Position(value);
            PositionBars();
        }
    }
}