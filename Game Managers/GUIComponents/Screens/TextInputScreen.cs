using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    public class TextInputScreen : GUIScreen
    {
        private GUITextInputWindow _window;

        public TextInputScreen(Adventurer w)
        {             
            _window = new GUITextInputWindow(w);
            Controls.Add(_window);
        }
    }
}
