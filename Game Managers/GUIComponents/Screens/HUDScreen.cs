using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Adventure.Screens;

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
                rv = _display.ProcessLeftButtonClick(mouse);
            }
            return rv;
        }
    }
}
