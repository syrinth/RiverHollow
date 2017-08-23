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
            //g = new GUITextWindow("O Romeo, Romeo! Wherefore art thou Romeo? Deny thy father and refuse thy name. Or, if thou wilt not, be but sworn my love, And I'll no longer be a Capulet. 'Tis but thy name that is my enemy. Thou art thyself, though not a Montague. What's Montague? It is nor hand, nor foot, Nor arm, nor face, nor any other part Belonging to a man. O, be some other name! What's in a name? That which we call a rose By any other word would smell as sweet. So Romeo would, were he not Romeo called, Retain that dear perfection which he owes Without that title.Romeo, doff thy name, And for that name, which is no part of thee Take all myself.");
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
