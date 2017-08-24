using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Adventure.Characters.NPCs;

namespace Adventure.Game_Managers.GUIObjects
{
    public class ShopScreen : GUIScreen
    {
        private ShopWindow _window;
        public ShopScreen(ShopKeeper shop)
        {
            _window = new ShopWindow(shop);
            Controls.Add(_window);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_window.Rectangle.Contains(mouse))
            {
                rv = _window.ProcessLeftButtonClick(mouse);
            }
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (!_window.Rectangle.Contains(mouse))
            {
                rv = true;
            }

            return rv;
        }
    }
}
