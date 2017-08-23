using Microsoft.Xna.Framework;
using Adventure.Screens;
using Adventure.Game_Managers.GUIComponents.GUIObjects;

namespace Adventure.Game_Managers.GUIObjects
{
    public class HUDScreen : GUIScreen
    {
        private InventoryDisplay _display;
        public HUDScreen()
        {
            _display = new InventoryDisplay();
            Controls.Add(_display);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_display.Rectangle.Contains(mouse))
            {
                _display.ProcessLeftButtonClick(mouse);
                rv = true;
            }
            return rv;
        }
    }
}
