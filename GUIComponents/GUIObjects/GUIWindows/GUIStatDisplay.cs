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
        public enum DisplayEnum { Energy, Health, Mana, XP};

        DisplayEnum _eDisplayType;
        CombatActor _character;
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
            _eDisplayType = what;
            _percentage = 0;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _iMidWidth = 192;

            _gLeft = new GUIImage(new Rectangle(48, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gMid = new GUIImage(new Rectangle(52, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gRight = new GUIImage(new Rectangle(60, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            
            _gFillLeft = new GUIImage(new Rectangle(64, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gFillMid = new GUIImage(new Rectangle(68, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gFillRight = new GUIImage(new Rectangle(76, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gText = new GUIText();

            PositionBars();

            Height = 16;
            Width = 200;

            SetColor();
            CalcPercent();
        }

        public GUIStatDisplay(DisplayEnum what, CombatActor c, int width)
        {
            _character = c;
            _eDisplayType = what;
            _percentage = 0;
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _iMidWidth = width - (EDGE * 2);

            _gLeft = new GUIImage(new Rectangle(48, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gMid = new GUIImage(new Rectangle(52, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gRight = new GUIImage(new Rectangle(60, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");

            _gFillLeft = new GUIImage(new Rectangle(64, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gFillMid = new GUIImage(new Rectangle(68, 32, 8, 16), _iMidWidth, 16, @"Textures\Dialog");
            _gFillRight = new GUIImage(new Rectangle(76, 32, EDGE, 16), EDGE, 16, @"Textures\Dialog");
            _gText = new GUIText();

            PositionBars();

            Height = 16;
            Width = width;

            SetColor();
            CalcPercent();
        }

        public void SetColor()
        {
            Color c = Color.White;
            if (_eDisplayType.Equals(DisplayEnum.Energy))
            {
                c = Color.LightGreen;
            }
            else if (_eDisplayType.Equals(DisplayEnum.Health))
            {
                c = Color.Red;
            }
            else if (_eDisplayType.Equals(DisplayEnum.Mana))
            {
                c = Color.LightBlue;
            }
            else if (_eDisplayType.Equals(DisplayEnum.XP))
            {
                c = Color.Yellow;
            }

            _gFillLeft.SetColor(c);
            _gFillMid.SetColor(c);
            _gFillRight.SetColor(c);
            _gLeft.SetColor(c);
            _gMid.SetColor(c);
            _gRight.SetColor(c);
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
            base.Update(gameTime);
            CalcPercent();
        }

        private void CalcPercent()
        {
            if (_eDisplayType == DisplayEnum.Energy) { _percentage = (PlayerManager.Stamina / (float)PlayerManager.MaxStamina); }
            else if (_eDisplayType == DisplayEnum.Health) { _percentage = ((float)_character.CurrentHP / (float)_character.MaxHP); }
            else if (_eDisplayType == DisplayEnum.Mana) { _percentage = ((float)_character.CurrentMP / (float)_character.MaxMP); }
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

                if (_eDisplayType == DisplayEnum.Energy) { text = string.Format("{0}/{1}", PlayerManager.Stamina, PlayerManager.MaxStamina); }
                else if (_eDisplayType == DisplayEnum.Health) { text = string.Format("{0}/{1}", _character.CurrentHP, _character.MaxHP); }
                else if (_eDisplayType == DisplayEnum.Mana) { text = string.Format("{0}/{1}", _character.CurrentMP, _character.MaxMP); }
                else if (_eDisplayType == DisplayEnum.XP) { text = string.Format("{0}/{1}", ((CombatAdventurer)_character).XP, CombatAdventurer.LevelRange[((CombatAdventurer)_character).ClassLevel]); }

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