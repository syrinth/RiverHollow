using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIStatDisplay : GUIObject
    {
        public enum DisplayEnum { Energy, Health, Mana};

        DisplayEnum _toDisplay;
        CombatCharacter _character;
        float _percentage;
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

        public GUIStatDisplay(DisplayEnum what)
        {
            _character = PlayerManager.Combat;
            _toDisplay = what;
            _percentage = 0;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _iMidWidth = 192;

            _gLeft = new GUIImage(Vector2.Zero, new Rectangle(48, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gMid = new GUIImage(Vector2.Zero, new Rectangle(52, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gRight = new GUIImage(Vector2.Zero, new Rectangle(60, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            
            _gFillLeft = new GUIImage(Vector2.Zero, new Rectangle(64, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gFillMid = new GUIImage(Vector2.Zero, new Rectangle(68, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gFillRight = new GUIImage(Vector2.Zero, new Rectangle(76, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gText = new GUIText();

            PositionBars();

            Height = 16;
            Width = 200;
        }

        public GUIStatDisplay(DisplayEnum what, CombatCharacter c, int width)
        {
            _character = c;
            _toDisplay = what;
            _percentage = 0;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _iMidWidth = width - (EDGE * 2);

            _gLeft = new GUIImage(Vector2.Zero, new Rectangle(48, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gMid = new GUIImage(Vector2.Zero, new Rectangle(52, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gRight = new GUIImage(Vector2.Zero, new Rectangle(60, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");

            _gFillLeft = new GUIImage(Vector2.Zero, new Rectangle(64, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gFillMid = new GUIImage(Vector2.Zero, new Rectangle(68, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gFillRight = new GUIImage(Vector2.Zero, new Rectangle(76, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gText = new GUIText();

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

        public override void Update(GameTime gameTime)
        {
            if (_toDisplay == DisplayEnum.Energy)
            {
                _percentage = (PlayerManager.Stamina / (float)PlayerManager.MaxStamina);
            }
            else
            {
                _percentage = ((float)_character.CurrentHP / (float)_character.MaxHP);
            }
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _gFillLeft.Draw(spriteBatch);
            _gFillMid.Width = (int)(_iMidWidth * _percentage);
            _gFillMid.Draw(spriteBatch);
            _gFillRight.Draw(spriteBatch);
            _gLeft.Draw(spriteBatch);
            _gMid.Draw(spriteBatch);
            _gRight.Draw(spriteBatch);

            if (_bHover)
            {
                string text = string.Empty;

                if (_toDisplay == DisplayEnum.Energy) { text = string.Format("{0}/{1}", PlayerManager.Stamina, PlayerManager.MaxStamina); }
                else { text = string.Format("{0}/{1}", _character.CurrentHP, _character.MaxHP); }

                _gText.SetText(text);
                _gText.AlignToObject(_gMid, SideEnum.Center);
                _gText.Draw(spriteBatch);
            }
        }

        public bool ProcessHover(Point mouse)
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