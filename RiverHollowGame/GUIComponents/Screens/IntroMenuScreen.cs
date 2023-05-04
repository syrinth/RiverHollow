using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;

using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    public class IntroMenuScreen : GUIScreen
    {
        const int BTN_PADDING = 50;
        private GUIButton _btnNewGame;
        private GUIButton _btnLoadGame;
        private GUIButton _btnExit;
        private GUIButton _btnGameData;

        public IntroMenuScreen()
        {
            GameManager.ShowMap(false);
            GameManager.CurrentScreen = GameScreenEnum.Info;

            _btnNewGame = new GUIButton("New Game", BtnNewGame);
            _btnLoadGame = new GUIButton("Load Game", BtnLoadGame);
            _btnExit = new GUIButton("Exit Game", BtnExit);

            _btnGameData = new GUIButton("Config", BtnItems);
            _btnGameData.AnchorToScreen(SideEnum.BottomRight, 1);

            GUIUtils.CreateSpacedColumn(new List<GUIObject>() { _btnNewGame, _btnLoadGame, _btnExit }, RiverHollow.ScreenWidth/2, 0, RiverHollow.ScreenHeight, BTN_PADDING);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            foreach (GUIObject c in Controls)
            {
                rv = c.ProcessLeftButtonClick(mouse);
                if (rv) { break; }
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            return false;
        }

        #region Button Actions
        public void BtnNewGame()
        {
            GUIManager.SetScreen(new NewGameScreen());
        }

        public void BtnLoadGame()
        {
            GUIManager.SetScreen(new LoadScreen());
        }

        public void BtnExit()
        {
            RiverHollow.PrepExit();
        }

        public void BtnItems()
        {
            GUIManager.SetScreen(new DataScreen());
        }
        #endregion
    }
}
