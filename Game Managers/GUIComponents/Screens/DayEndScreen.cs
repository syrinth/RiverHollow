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
            AdventureGame.ChangeGameState(AdventureGame.GameState.Information);
            _btnOK = new GUIButton(new Vector2(AdventureGame.ScreenWidth / 2, 500), new Rectangle(0, 128, 128, 64), 256, 128, @"Textures\Dialog");
            Controls.Add(_btnOK);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnOK.Contains(mouse))
            {
                GameCalendar.NextDay();
                GUIManager.FadeOut();
                AdventureGame.ChangeGameState(AdventureGame.GameState.Running);
                PlayerManager.Save();
                rv = true;
            }
            return rv;
        }
    }
}
