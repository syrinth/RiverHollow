using Microsoft.Xna.Framework;
using RiverHollow.Screens;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using Microsoft.Xna.Framework.Input;

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
            _healthDisplay = new StatDisplay(StatDisplay.DisplayEnum.Health, new Vector2(32,32), 5);
            _staminaDisplay = new StatDisplay(StatDisplay.DisplayEnum.Energy, new Vector2(32,64), 5);
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
            spriteBatch.DrawString(_font, PlayerManager.Money.ToString(), new Vector2(32, 96), Color.White);
        }

        public override bool IsHUD() { return true; }
    }
}
