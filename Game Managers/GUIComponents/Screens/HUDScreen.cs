using Microsoft.Xna.Framework;
using Adventure.Screens;
using Adventure.Game_Managers.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIObjects
{
    public class HUDScreen : GUIScreen
    {
        private InventoryDisplay _invDisplay;
        private StatDisplay _healthDisplay;
        private StatDisplay _staminaDisplay;
        private SpriteFont _font;
        public HUDScreen()
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _invDisplay = new InventoryDisplay();
            _healthDisplay = new StatDisplay(StatDisplay.Display.Health, new Vector2(0,0), 5);
            _staminaDisplay = new StatDisplay(StatDisplay.Display.Energy, new Vector2(0,32), 5);
            Controls.Add(_invDisplay);
            Controls.Add(_healthDisplay);
            Controls.Add(_staminaDisplay);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_invDisplay.Rectangle.Contains(mouse))
            {
                _invDisplay.ProcessLeftButtonClick(mouse);
                rv = true;
            }
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(_font, PlayerManager.Player.Money.ToString(), new Vector2(0, 64), Color.White);
        }
    }
}
