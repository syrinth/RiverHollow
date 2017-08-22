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
            GUIDialog g = new GUIDialog("Lorem Ipsum Lorem Ipsum Lorem Ipsum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum sum Lorem IpsumLorem IpsumLorem Lorem Ipsum Lorem Ipsum Ipsum Lorem Ipsum");
            _display = new InventoryDisplay();
            Controls.Add(_display);
            Controls.Add(g);
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
