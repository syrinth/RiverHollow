using Microsoft.Xna.Framework;
using Adventure.Screens;
using Adventure.Game_Managers.GUIComponents.GUIObjects;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIObjects
{
    public class InventoryScreen : GUIScreen
    {
        private Inventory _inventory;
        private SpriteFont _font;
        public InventoryScreen()
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(new Vector2(AdventureGame.ScreenWidth / 2, AdventureGame.ScreenHeight/2), Player.maxItemColumns, 4, 32);
            Controls.Add(_inventory);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_inventory.Rectangle.Contains(mouse))
            {
                _inventory.ProcessLeftButtonClick(mouse);
                rv = true;
            }
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
