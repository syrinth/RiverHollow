using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class HUDScreen : GUIScreen
    {
        private StatDisplay _healthDisplay;
        private StatDisplay _staminaDisplay;
        private SpriteFont _font;
        public HUDScreen()
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _healthDisplay = new StatDisplay(StatDisplay.DisplayEnum.Health);
            _healthDisplay.AnchorToScreen(GUIObject.SideEnum.TopLeft, 10);
            _staminaDisplay = new StatDisplay(StatDisplay.DisplayEnum.Energy);
            _staminaDisplay.AnchorAndAlignToObject(_healthDisplay, GUIObject.SideEnum.Bottom, GUIObject.SideEnum.Left);
            Controls.Add(_healthDisplay);
            Controls.Add(_staminaDisplay);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_healthDisplay.ProcessHover(mouse)) { rv = true; }
            if (_staminaDisplay.ProcessHover(mouse)) { rv = true; }
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(_font, PlayerManager.Money.ToString(), _staminaDisplay.OuterBottomLeft(), Color.White);
        }

        public override bool IsHUD() { return true; }
    }
}
