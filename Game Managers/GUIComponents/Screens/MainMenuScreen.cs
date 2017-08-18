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

        public MainMenuScreen()
        {
            _btnNewGame = new GUIButton(500, 500, 70, 70, GameContentManager.GetInstance().GetTexture(@"Textures\New"));
            Controls.Add(_btnNewGame);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_btnNewGame.Rectangle.Contains(mouse))
            {
                AdventureGame.ChangeGameState(AdventureGame.GameState.Game);
            }
            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            //_btnNewGame.Update(gameTime);
        }
    }
}
