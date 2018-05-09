using RiverHollow.Characters;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using static RiverHollow.WorldObjects.Door;
using RiverHollow.Characters.NPCs;
using RiverHollow.GUIComponents.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers.GUIComponents.Screens
{
    class TextScreen : GUIScreen
    {
        private GUITextWindow _window;



        private TextScreen()
        {
            GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Normal;
            GameManager.Pause();
        }

        public TextScreen(string text, bool selection) : this()
        {
            if (selection)
            {
                _window = new GUITextSelectionWindow(text);
            }
            else
            {
                _window = new GUITextWindow(text);
            }
            Controls.Add(_window);
        }

        public TextScreen(KeyDoor door, string text) : this()
        {
            GameManager.gmDoor = door;
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

        public TextScreen(Spirit talker, string text) : this()
        {
            GameManager.gmSpirit = talker;
            _window = new GUITextWindow(text);

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
                if (CutsceneManager.Playing) { GUIManager.ClearScreen(); }
                else { GameManager.BackToMain(); }
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

            _window.ProcessLeftButtonClick(mouse);

            if (_window != null)
            {
                if (!_window.Paused)
                {
                    _window.PrintAll();
                }
                else
                {
                    _window.NextText();
                }
            }
            return rv;
        }

        public bool TextFinished()
        {
            return _window.Done() && !_window.Paused;
        }

        public override bool IsTextScreen() { return true; }
    }
}
