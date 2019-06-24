using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class IntroMenuScreen : GUIScreen
    {
        const int BTN_PADDING = 50;
        private GUIButton _btnNewGame;
        private GUIButton _btnLoadGame;
        private GUIButton _btnExit;

        public IntroMenuScreen()
        {
            _btnNewGame = new GUIButton("New Game", BtnNewGame);
            _btnLoadGame = new GUIButton("Load Game", BtnLoadGame);
            _btnExit = new GUIButton("Exit Game", BtnExit);
            List <GUIObject> listButtons = new List<GUIObject>() { _btnNewGame, _btnLoadGame, _btnExit };
            GUIObject.CreateSpacedColumn(ref listButtons, RiverHollow.ScreenWidth/2, 0, RiverHollow.ScreenHeight, BTN_PADDING);

            AddControl(_btnNewGame);
            AddControl(_btnLoadGame);
            AddControl(_btnExit);

            GameManager.GoToInformation();
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
        #endregion

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            _btnNewGame.IsMouseHovering = _btnNewGame.Contains(mouse);
            _btnLoadGame.IsMouseHovering = _btnLoadGame.Contains(mouse);
            return rv;
        }
    }
}
