﻿using Microsoft.Xna.Framework;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class MainMenuScreen : GUIScreen
    {
        const int BTN_PADDING = 50;
        const int BTN_HEIGHT = 64;
        const int BTN_WIDTH = 128;
        private GUIButton _btnNewGame;
        private GUIButton _btnLoadGame;
        private GUIButton _btnExit;

        public MainMenuScreen()
        {
            _btnNewGame = new GUIButton("New Game");
            _btnLoadGame = new GUIButton("Load Game");
            _btnExit = new GUIButton("Exit Game");
            List <GUIObject> listButtons = new List<GUIObject>() { _btnNewGame, _btnLoadGame, _btnExit };
            GUIObject.CreateSpacedColumn(ref listButtons, RiverHollow.ScreenWidth/2, RiverHollow.ScreenHeight, BTN_PADDING, BTN_WIDTH, BTN_HEIGHT);

            Controls.Add(_btnNewGame);
            Controls.Add(_btnLoadGame);
            Controls.Add(_btnExit);

            GameManager.GoToInformation();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnNewGame.Contains(mouse))
            {
                GUIManager.SetScreen(new NewGameScreen());
                //RiverHollow.NewGame();
                rv = true;
            }
            if (_btnLoadGame.Contains(mouse))
            {
                RiverHollow.LoadGame();
                rv = true;
            }
            if (_btnExit.Contains(mouse))
            {
                RiverHollow.PrepExit();
                rv = true;
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            _btnNewGame.IsMouseHovering = _btnNewGame.Contains(mouse);
            _btnLoadGame.IsMouseHovering = _btnLoadGame.Contains(mouse);
            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            //_btnNewGame.Update(gameTime);
        }
    }
}
