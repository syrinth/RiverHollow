using Adventure.Characters.NPCs;
using Adventure.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using Adventure.Game_Managers.GUIObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers.GUIComponents.Screens
{
    public class TextInputScreen : GUIScreen
    {
        private GUITextInputWindow _window;

        public TextInputScreen(Worker w)
        {
            _window = new GUITextInputWindow(w);
            Controls.Add(_window);
        }
    }
}
