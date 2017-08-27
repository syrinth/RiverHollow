﻿using Adventure.Characters;
using Adventure.Game_Managers.GUIComponents.GUIObjects;
using Adventure.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIComponents.Screens
{
    class TextScreen : GUIScreen
    {
        private GUITextWindow _window;

        public TextScreen(NPC talker, string text)
        {
            AdventureGame.ChangeGameState(AdventureGame.GameState.Paused);

            _window = new GUITextWindow(talker, text);
            Controls.Add(_window);
        }

        public override void Update(GameTime gameTime)
        {
            _window.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _window.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = true;
            if (!_window._pause)
            {
                _window.printAll = true;
            }
            else
            {
                _window.Unpause();
            }
            return rv;
        }

        public bool TextFinished()
        {
            return _window.Done && !_window._pause;
        }
    }
}
