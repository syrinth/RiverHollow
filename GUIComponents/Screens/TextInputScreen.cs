using RiverHollow.Characters;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    public class TextInputScreen : GUIScreen
    {
        private GUITextInputWindow _window;

        public TextInputScreen(NPC w)
        {             
            _window = new GUITextInputWindow(w);
            Controls.Add(_window);
        }
    }
}
