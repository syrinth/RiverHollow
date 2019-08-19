using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Actors;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using System;

namespace RiverHollow.Game_Managers
{
    public static class GUIManager
    {
        public static int MINI_BTN_HEIGHT = 32;
        public static int MINI_BTN_WIDTH = 128;

        public static int MAIN_COMPONENT_WIDTH = RiverHollow.ScreenWidth / 3;
        public static int MAIN_COMPONENT_HEIGHT = RiverHollow.ScreenWidth / 3;
        private static GUIScreen _currentGUIScreen;
        private static GUIImage _fadeImg;
        private static float _fadeVal = 1f;
        private static bool _fading = false;
        private static bool _slowFade = false;
        public static bool Fading { get => _fading; }

        public static void LoadContent()
        {
            _fadeImg = new GUIImage(new Rectangle(160, 128, TileSize, TileSize), RiverHollow.ScreenWidth*2, RiverHollow.ScreenHeight*2, @"Textures\Dialog");
            GraphicCursor.LoadContent();
        }

        public static void Update(GameTime gameTime)
        {
            if (_fading)
            {
                UpdateFade();
            }
            if (_currentGUIScreen != null)
            {
                _currentGUIScreen.Update(gameTime);
            }
            GraphicCursor.Update();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_fading)
            {
                _fadeImg.Draw(spriteBatch, _fadeVal);

            }
            if (_currentGUIScreen != null)
            {
                _currentGUIScreen.Draw(spriteBatch);
            }

            GraphicCursor.Draw(spriteBatch);
        }

        public static void ClearScreen()
        {
            _currentGUIScreen = null;
            GameManager.Unpause();
        }
        public static bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_currentGUIScreen != null)
            {
                rv = _currentGUIScreen.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public static bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (_currentGUIScreen != null)
            {
                rv = _currentGUIScreen.ProcessRightButtonClick(mouse);
            }

            return rv;
        }

        public static bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (_currentGUIScreen != null)
            {
                rv = _currentGUIScreen.ProcessHover(mouse);
            }

            return rv;
        }

        public static bool CloseTextWindow(GUITextWindow win) {
            GameManager.Unpause();
            return _currentGUIScreen.CloseTextWindow(win);
        }
        public static bool IsTextWindowOpen() { return _currentGUIScreen.IsTextWindowOpen(); }
        public static void OpenTextWindow(string text, bool open = true)
        {
            GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Normal;
            GameManager.Pause();
            _currentGUIScreen.OpenTextWindow(text, open);
        }

        /// <summary>
        /// Super method for OpenTextWindow, sets the CurrentNPC to the talker and calls the sub method
        /// </summary>
        /// <param name="text">Text to speak</param>
        /// <param name="talker">NPC being talked to</param>
        /// <param name="open">Whether or not to play the animation for an opening window.</param>
        public static void OpenTextWindow(string text, TalkingActor talker, bool open = true)
        {
            GameManager.CurrentNPC = talker;
            OpenTextWindow(text, open);
        }
        public static void SetWindowText(string value)
        {
            _currentGUIScreen.SetWindowText(value);
        }

        public static void CloseHoverWindow() {
            _currentGUIScreen.CloseHoverWindow();
        }
        public static void OpenHoverWindow(GUITextWindow hoverWindow, GUIObject hoverObject)
        {
            _currentGUIScreen.OpenHoverWindow(hoverWindow, hoverObject);
        }

        public static void OpenMenu() { _currentGUIScreen.OpenMenu(); }
        public static void CloseMenu() { _currentGUIScreen.CloseMenu(); }

        public static void DisplayImage(GUIImage newImage)
        {
            _currentGUIScreen.CreateNewImage(newImage);
        }

        //Main Object
        public static void OpenMainObject(GUIObject o) { _currentGUIScreen.OpenMainObject(o); }
        public static void CloseMainObject() { _currentGUIScreen.CloseMainObject(); }

        public static void AddTextSelection(string text)
        {
            _currentGUIScreen.AddTextSelection(text);
        }

        public static void SetScreen(GUIScreen newScreen)
        {
            if( newScreen == null)
            {
                int i = 0;
            }
            _currentGUIScreen = newScreen;
        }

        public static void FadeOut()
        {
            _slowFade = false;
            _fading = true;
        }

        public static void SlowFadeOut()
        {
            _fading = true;
            _slowFade = true;
        }

        private static void UpdateFade()
        {
            _fadeVal -= _slowFade ? 0.01f : 0.05f;
            if (_fadeVal <= 0)
            {
                _fadeVal = 1;
                _fading = false;
            }
        }

        public static void SyncScreen()
        {
            _currentGUIScreen.Sync();
        }

        //public static bool IsTextScreen() { return _currentGUIScreen != null && _currentGUIScreen.IsTextScreen(); }
        public static bool IsMenuScreenOpen() { return _currentGUIScreen.IsMenuOpen(); }
        public static bool IsItemCreationScreen() { return _currentGUIScreen != null && _currentGUIScreen.IsItemCreationScreen(); }
        public static bool IsHUD() { return _currentGUIScreen != null && _currentGUIScreen.IsHUD(); }
    }
}
