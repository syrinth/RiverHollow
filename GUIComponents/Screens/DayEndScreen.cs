using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using System;
using static RiverHollow.GUIObjects.GUIObject;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    public class DayEndScreen : GUIScreen
    {
        private GUIText _gText;
        private GUIButton _btnOK;
        private GUIWindow _moneyWindow;

        public DayEndScreen()
        {
            GameManager.ShowMap(false);

            string totalVal = String.Format("Total: {0}", GameManager.ShippingGremlin.SellAll());
            _moneyWindow = new GUIWindow();
            _moneyWindow.CenterOnScreen(this);

            _gText = new GUIText(totalVal);
            _gText.AnchorToInnerSide(_moneyWindow, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN);

            _btnOK = new GUIButton("OK");
            _btnOK.AnchorToInnerSide(_moneyWindow, SideEnum.BottomRight, GUIManager.STANDARD_MARGIN);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnOK.Contains(mouse))
            {
                GameCalendar.NextDay();
                RiverHollow.Rollover();
                SaveManager.Save();
                GUIManager.BeginFadeOut();
                PlayerManager.Stamina = PlayerManager.MaxStamina;

                GameManager.GoToHUDScreen();
                rv = true;
            }
            return rv;
        }
    }
}
