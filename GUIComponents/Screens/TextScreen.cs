﻿using RiverHollow.Characters;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    class TextScreen : GUIScreen
    {
        private GUITextWindow _window;

        private TextScreen()
        {
            GraphicCursor._currentType = GraphicCursor.CursorType.Normal;
            GameManager.Pause();
        }

        public TextScreen(string text) : this()
        {
            _window = new GUITextSelectionWindow(text);
            Controls.Add(_window);
        }

        public TextScreen(NPC talker, string text) : this()
        {
            if (text.Contains("["))
            {
                _window = new GUITextSelectionWindow(talker, text);
            }
            else
            {
                _window = new GUITextWindow(talker, text);
            }
            Controls.Add(_window);
        }

        public override void Update(GameTime gameTime)
        {
            if (TextFinished())
            {
                if (DungeonManager.Maps.Count > 0)
                {
                    MapManager.EnterDungeon();
                }
                GameManager.BackToMain();
            }
            else
            {
                base.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _window.Draw(spriteBatch);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = true;
            if (_window != null)
            {
                if (!_window._pause)
                {
                    _window.printAll = true;
                }
                else
                {
                    _window.Unpause();
                }
            }
            return rv;
        }

        public bool TextFinished()
        {
            return _window.Done && !_window._pause;
        }
    }
}
