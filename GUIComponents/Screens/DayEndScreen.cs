using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    public class DayEndScreen : GUIScreen
    {
        private GUIButton _btnOK;
        private GUITextWindow _moneyWindow;

        public DayEndScreen()
        {
            GameManager.GoToInformation();
            _btnOK = new GUIButton(new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight - 128), new Rectangle(0, 128, 128, 64), 256, 128, "OK", @"Textures\Dialog");
            string totalVal = String.Format("Total: {0}", PlayerManager._merchantChest.SellAll());
            _moneyWindow = new GUITextWindow(new Vector2(RiverHollow.ScreenWidth / 2, 500), totalVal);
            Controls.Add(_btnOK);
            Controls.Add(_moneyWindow);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnOK.Contains(mouse))
            {
                RiverHollow.RollOver();
                GameCalendar.NextDay();
                GUIManager.FadeOut();
                GameManager.Save();
                PlayerManager.Stamina = PlayerManager.MaxStamina;

                GameManager.BackToMain();
                rv = true;
            }
            return rv;
        }
    }
}
