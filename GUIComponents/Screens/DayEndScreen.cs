using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RiverHollow.GUIObjects;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    public class DayEndScreen : GUIScreen
    {
        private GUIButton _btnOK;
        private GUITextWindow _moneyWindow;

        public DayEndScreen()
        {
            GameManager.GoToInformation();
            _btnOK = new GUIButton( "OK", 128, 64);
            _btnOK.AnchorToScreen(this, SideEnum.Bottom);
            string totalVal = String.Format("Total: {0}", GameManager.ShippingGremlin.SellAll());
            _moneyWindow = new GUITextWindow(totalVal);
            _moneyWindow.CenterOnScreen(this);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnOK.Contains(mouse))
            {
                GameCalendar.NextDay();
                RiverHollow.Rollover();
                GameManager.Save();
                GUIManager.BeginFadeOut();
                PlayerManager.Stamina = PlayerManager.MaxStamina;

                GameManager.BackToMain();
                rv = true;
            }
            return rv;
        }
    }
}
