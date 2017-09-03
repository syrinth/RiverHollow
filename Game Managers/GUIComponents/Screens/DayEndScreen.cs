using Adventure.Game_Managers.GUIComponents.GUIObjects;
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
        private GUIButton _btnOK;
        private GUITextWindow _moneyWindow;

        public DayEndScreen()
        {
            AdventureGame.ChangeGameState(AdventureGame.GameState.Information);
            _btnOK = new GUIButton(new Vector2(AdventureGame.ScreenWidth / 2, AdventureGame.ScreenHeight - 128), new Rectangle(0, 128, 128, 64), 256, 128, @"Textures\Dialog");
            string totalVal = String.Format("Total: {0}", PlayerManager._merchantChest.SellAll());
            _moneyWindow = new GUITextWindow(new Vector2(AdventureGame.ScreenWidth / 2, 500), totalVal);
            Controls.Add(_btnOK);
            Controls.Add(_moneyWindow);
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
                PlayerManager.Player.Stamina = PlayerManager.Player.MaxStamina;
                
                rv = true;
            }
            return rv;
        }
    }
}
