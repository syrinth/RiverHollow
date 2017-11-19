﻿using RiverHollow.GUIObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class MainMenuScreen : GUIScreen
    {
        private GUIButton _btnNewGame;
        private GUIButton _btnLoadGame;

        public MainMenuScreen()
        {
            _btnNewGame = new GUIButton(new Vector2(RiverHollow.ScreenWidth/2, 500), new Rectangle(0, 128, 64, 32), 128, 64, "New Game", @"Textures\Dialog");
            _btnLoadGame = new GUIButton(new Vector2(RiverHollow.ScreenWidth / 2, 800), new Rectangle(0, 128, 64, 32), 128, 64, "Load Game", @"Textures\Dialog");
            Controls.Add(_btnNewGame);
            Controls.Add(_btnLoadGame);
            RiverHollow.ChangeGameState(RiverHollow.GameState.Information);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnNewGame.Contains(mouse))
            {
                RiverHollow.NewGame();
                rv = true;
            }
            if (_btnLoadGame.Contains(mouse))
            {
                RiverHollow.LoadGame();
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
