using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.GUIObjects.GUIObject;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class HUDScreen : GUIScreen
    {
        StatDisplay _healthDisplay;
        StatDisplay _staminaDisplay;
        GUIMoneyDisplay _gMoney;
//        GUIText _gTextMoney;
  //      GUIImage _giCoin;

        public HUDScreen()
        {
            _healthDisplay = new StatDisplay(StatDisplay.DisplayEnum.Health);
            _healthDisplay.AnchorToScreen(SideEnum.TopLeft, 10);
            _staminaDisplay = new StatDisplay(StatDisplay.DisplayEnum.Energy);
            _staminaDisplay.AnchorAndAlignToObject(_healthDisplay, SideEnum.Bottom, SideEnum.Left);
            _gMoney = new GUIMoneyDisplay();
            _gMoney.AnchorAndAlignToObject(_staminaDisplay, SideEnum.Bottom, SideEnum.Left);

            Controls.Add(_healthDisplay);
            Controls.Add(_staminaDisplay);
            Controls.Add(_gMoney);
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
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override bool IsHUD() { return true; }
    }
}
