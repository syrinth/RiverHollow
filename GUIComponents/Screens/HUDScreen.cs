using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.GUIObjects.GUIObject;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.WorldObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class HUDScreen : GUIScreen
    {
        GUIStatDisplay _healthDisplay;
        GUIStatDisplay _staminaDisplay;
        GUIMoneyDisplay _gMoney;
        GUIItemBox _gCurrentItem;

        public HUDScreen()
        {
            _healthDisplay = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Health);
            _healthDisplay.AnchorToScreen(this, SideEnum.TopLeft, 10);
            _staminaDisplay = new GUIStatDisplay(GUIStatDisplay.DisplayEnum.Energy);
            _staminaDisplay.AnchorAndAlignToObject(_healthDisplay, SideEnum.Bottom, SideEnum.Left);
            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorAndAlignToObject(_staminaDisplay, SideEnum.Bottom, SideEnum.Left);

            _gCurrentItem = new GUIItemBox();
            _gCurrentItem.SetScale(2);
            _gCurrentItem.AnchorToScreen(SideEnum.BottomRight, 20);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
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

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _gMoney.Update(gameTime);
            _gCurrentItem.SetItem(InventoryManager.PlayerItemAtLocation());
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            _gCurrentItem.Draw(spriteBatch);
        }

        public override bool IsHUD() { return true; }
    }
}
