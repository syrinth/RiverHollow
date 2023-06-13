using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    internal class DefeatedScreen : GUIScreen
    {
        GUIText _gTimeText;
        RHTimer _timer;

        public DefeatedScreen()
        {
            GameManager.ShowMap(false);
            GameManager.CurrentScreen = GameScreenEnum.Info;

            GUIWindow win = new GUIWindow(GUIUtils.Brown_Window, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
            win.CenterOnScreen();

            GUIButton btn = new GUIButton("OK", BtnOk);
            btn.AnchorToInnerSide(win, GUIObject.SideEnum.Bottom);

            _gTimeText = new GUIText(GameCalendar.GetTime());
            _gTimeText.CenterOnWindow(win);

            _timer = new RHTimer(2);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if (RHTimer.TimerCheck(_timer, gTime))
            {
                _timer = null;
                GameCalendar.AddTime(2, 0);
                _gTimeText.SetText(GameCalendar.GetTime());
            }
        }

        private void BtnOk()
        {
            if (_timer == null ||!_timer.Finished())
            {
                _timer = null;
                GameCalendar.AddTime(2, 0);
                _gTimeText.SetText(GameCalendar.GetTime());
            }
            RHMap homeMap = MapManager.Maps[TownManager.Home.BuildingMapName];
            GameManager.GoToHUDScreen();
            MapManager.FadeToNewMap(homeMap, homeMap.GetCharacterSpawn("PlayerSpawn"), DirectionEnum.Down, TownManager.Home);
        }
    }
}
