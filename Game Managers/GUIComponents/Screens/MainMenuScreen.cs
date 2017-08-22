using Adventure.GUIObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Adventure.Game_Managers.GUIObjects
{
    public class MainMenuScreen : GUIScreen
    {
        private GUIButton _btnNewGame;
        private GUIButton _btnLoadGame;

        public MainMenuScreen()
        {
            _btnNewGame = new GUIButton(AdventureGame.ScreenWidth/2, 500, @"Textures\New");
            _btnLoadGame = new GUIButton(AdventureGame.ScreenWidth / 2, 800, @"Textures\Load");
            Controls.Add(_btnNewGame);
            Controls.Add(_btnLoadGame);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnNewGame.Rectangle.Contains(mouse))
            {
                AdventureGame.ChangeGameState(AdventureGame.GameState.Game);
                AdventureGame.NewGame();
                rv = true;
            }
            if (_btnLoadGame.Rectangle.Contains(mouse))
            {
                PlayerManager.Load();
                AdventureGame.ChangeGameState(AdventureGame.GameState.Game);
                rv = true;
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            _btnNewGame.IsMouseHovering = _btnNewGame.Rectangle.Contains(mouse);
            _btnLoadGame.IsMouseHovering = _btnLoadGame.Rectangle.Contains(mouse);
            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            //_btnNewGame.Update(gameTime);
        }
    }
}
