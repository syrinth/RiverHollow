using Adventure.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers.GUIComponents.Screens
{
    public class DayEndScreen : GUIScreen
    {
        GUIButton _btnOK;
        public DayEndScreen()
        {
            _btnOK = new GUIButton(AdventureGame.ScreenWidth / 2, 500, @"Textures\ok");
            Controls.Add(_btnOK);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnOK.Rectangle.Contains(mouse))
            {
                GameCalendar.NextDay();
                AdventureGame.ChangeGameState(AdventureGame.GameState.Game);
                PlayerManager.Save();
                rv = true;
            }
            return rv;
        }
    }
}
