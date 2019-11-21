using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Actors;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using System;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.Game_Managers
{
    public static class GUIManager
    {
        enum Fade { None, Out, In};
        static Fade _eFade;

        public static int MINI_BTN_HEIGHT = 32;
        public static int MINI_BTN_WIDTH = 128;

        public static int MAIN_COMPONENT_WIDTH = RiverHollow.ScreenWidth / 3;
        public static int MAIN_COMPONENT_HEIGHT = RiverHollow.ScreenWidth / 3;
        private static GUIScreen _currentGUIScreen;
        private static GUIImage _fadeImg;
        private static float _fFadeVal = 0f;
        private static bool _bFadeSlow = false;
        public static bool Fading  => _eFade != Fade.None;
        public static bool FadingIn => _eFade == Fade.In;

        public static void LoadContent()
        {
            _fadeImg = new GUIImage(new Rectangle(160, 128, TileSize, TileSize), RiverHollow.ScreenWidth*2, RiverHollow.ScreenHeight*2, @"Textures\Dialog");
            GraphicCursor.LoadContent();
        }

        public static void Update(GameTime gTime)
        {
            if (Fading)
            {
                UpdateFade();
            }
            if (_currentGUIScreen != null)
            {
                _currentGUIScreen.Update(gTime);
            }
            GraphicCursor.Update();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (Fading)
            {
                _fadeImg.Draw(spriteBatch, _fFadeVal);

            }
            else
            {
                if (_currentGUIScreen != null)
                {
                    _currentGUIScreen.Draw(spriteBatch);
                }

                GraphicCursor.Draw(spriteBatch);
            }
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

        public static void AssignBackgroundImage(GUIImage newImage)
        {
            _currentGUIScreen.AssignBackgroundImage(newImage);
        }

        public static void ClearBackgroundImage()
        {
            _currentGUIScreen.ClearBackgroundImage();
        }

        #region MainObject Control
        public static void OpenMainObject(GUIObject o) { _currentGUIScreen.OpenMainObject(o); }
        public static void CloseMainObject() { _currentGUIScreen.CloseMainObject(); }
        #endregion

        public static void AddTextSelection(string text)
        {
            _currentGUIScreen.AddTextSelection(text);
        }

        public static void SetScreen(GUIScreen newScreen)
        {
            _currentGUIScreen = newScreen;
        }

        /// <summary>
        /// Starts a FadeOut
        /// </summary>
        /// <param name="slowFadeout">Whether or not the fading is fast or slow</param>
        public static void BeginFadeOut(bool slowFadeout = false)
        {
            _bFadeSlow = slowFadeout;
            _eFade = Fade.Out;
            PlayerManager.AllowMovement = false;
        }

        /// <summary>
        /// When we're fading, we need to update how  opaque the blackout is. When fading out
        /// we need to increase the Opacity. When Fading In we need to decrease it.
        /// </summary>
        private static void UpdateFade()
        {
            float modVal = _bFadeSlow ? 0.01f : 0.05f;

            if (_eFade == Fade.Out) { _fFadeVal += modVal; }
            else if (_eFade == Fade.In) { _fFadeVal -= modVal; }

            if(_fFadeVal >= 1)              //We've faded out, start fading back in
            {
                _eFade = Fade.In;
                _fFadeVal = 1;
            }
            else if (_fFadeVal <= 0)        //We've faded in, so turn off fading and allow the player to move
            {
                _fFadeVal = 0;
                _eFade = Fade.None;

                //Only unlock movement if not in combat and not in a cutscene
                if (!CutsceneManager.Playing && !CombatManager.InCombat)
                {
                    PlayerManager.AllowMovement = true;
                }
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
