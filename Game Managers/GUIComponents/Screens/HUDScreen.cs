using Microsoft.Xna.Framework;
using Adventure.Screens;
using Microsoft.Xna.Framework.Graphics;
using Adventure.Game_Managers.GUIComponents.GUIObjects.GUIWindows;

namespace Adventure.Game_Managers.GUIObjects
{
    public class HUDScreen : GUIScreen
    {
        private Inventory _shortInventory;
        private StatDisplay _healthDisplay;
        private StatDisplay _staminaDisplay;
        private SpriteFont _font;
        public HUDScreen()
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _shortInventory = new ShortInventory(new Vector2(AdventureGame.ScreenWidth / 2, AdventureGame.ScreenHeight-32), Player.maxItemColumns, 4);
            _healthDisplay = new StatDisplay(StatDisplay.Display.Health, new Vector2(32,32), 5);
            _staminaDisplay = new StatDisplay(StatDisplay.Display.Energy, new Vector2(32,64), 5);
            Controls.Add(_shortInventory);
            Controls.Add(_healthDisplay);
            Controls.Add(_staminaDisplay);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_shortInventory.Rectangle.Contains(mouse))
            {
                _shortInventory.ProcessLeftButtonClick(mouse);
                rv = true;
            }
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            spriteBatch.DrawString(_font, PlayerManager.Player.Money.ToString(), new Vector2(32, 96), Color.White);
        }
    }
}
