using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Misc;

namespace RiverHollow.Game_Managers
{
    public static class GUIManager
    {
        enum Fade { None, Out, In};
        static Fade _eFade;

        public static int STANDARD_MARGIN = 1;
        public static int MINI_BTN_HEIGHT = ScaledTileSize;
        public static int MINI_BTN_WIDTH = 168;

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
            _fadeImg = new GUIImage(new Rectangle(160, 128, TILE_SIZE, TILE_SIZE), RiverHollow.ScreenWidth*2, RiverHollow.ScreenHeight*2, DataManager.DIALOGUE_TEXTURE);
            GUICursor.LoadContent();
        }

        public static void Update(GameTime gTime)
        {
            if (Fading)
            {
                UpdateFade();
            }
            else
            {
                if (_currentGUIScreen != null)
                {
                    _currentGUIScreen.Update(gTime);
                }
                GUICursor.Update();
            }
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

                GUICursor.Draw(spriteBatch);
            }
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

        public static bool IsMenuOpen() { return _currentGUIScreen.IsMenuOpen(); }
        public static void OpenMenu() { _currentGUIScreen.OpenMenu(); }
        public static void CloseMenu() { _currentGUIScreen.CloseMenu(); }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">Text to speak</param>
        /// <param name="talker">NPC being talked to</param>
        /// <param name="open">Whether or not to play the animation for an opening window.</param>
        public static void OpenTextWindow(string textEntryKey, bool open = true)
        {
            OpenTextWindow(DataManager.GetGameTextEntry(textEntryKey), open);
        }
        public static void OpenTextWindow(TextEntry text, bool open = true, bool displayDialogueIcon = false)
        {
            _currentGUIScreen.OpenTextWindow(text, open, displayDialogueIcon);
        }
        public static void OpenTextWindow(TextEntry text, TalkingActor talker, bool open = true, bool displayDialogueIcon = false)
        {
            _currentGUIScreen.OpenTextWindow(text, talker, open, displayDialogueIcon);
        }
        public static bool CloseTextWindow()
        {
            return _currentGUIScreen.CloseTextWindow();
        }
        public static bool IsTextWindowOpen() { return _currentGUIScreen.IsTextWindowOpen(); }

        public static void SetWindowText(TextEntry value, bool displayDialogueIcon = false)
        {
            _currentGUIScreen.SetWindowText(value, displayDialogueIcon);
        }

        public static void CloseHoverWindow() {
            _currentGUIScreen.CloseHoverWindow();
        }
        public static void OpenHoverWindow(GUITextWindow hoverWindow, GUIObject hoverObject)
        {
            _currentGUIScreen.OpenHoverWindow(hoverWindow, hoverObject);
        }

        public static void AssignBackgroundImage(GUIImage newImage)
        {
            _currentGUIScreen.AssignBackgroundImage(newImage);
        }

        public static void ClearBackgroundImage()
        {
            _currentGUIScreen.ClearBackgroundImage();
        }

        #region MainObject Control
        public static bool IsMainObjectOpen() { return _currentGUIScreen.IsMainObjectOpen(); }
        public static void OpenMainObject(GUIMainObject o) { _currentGUIScreen.OpenMainObject(o); }
        public static void CloseMainObject() { _currentGUIScreen.CloseMainObject(); }
        #endregion

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
                if (!CutsceneManager.Playing)
                {
                    PlayerManager.AllowMovement = true;
                }
            }
        }

        public static void NewAlertIcon(string text) {
            _currentGUIScreen.NewAlertIcon(text);
        }

        public static void AddSkipCutsceneButton()
        {
            _currentGUIScreen.AddSkipCutsceneButton();
        }
        public static void RemoveSkipCutsceneButton()
        {
            _currentGUIScreen.RemoveSkipCutsceneButton();
        }
    }

    public abstract class GUIMainObject : GUIObject
    {
        protected GUIWindow _winMain;

        /// <summary>
        /// Creates a new GUIWindow, adds it to the Controls of the object, sets the Height
        /// and Width of the object to that of the GUIWindow then centers it on the screen.
        /// </summary>
        /// <returns>The created GUIWindow</returns>
        protected GUIWindow SetMainWindow()
        {
            return SetMainWindow(GUIWindow.Window_1, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
        }
        protected GUIWindow SetMainWindow(int w, int h)
        {
            return SetMainWindow(GUIWindow.Window_1, w, h);
        }
        protected GUIWindow SetMainWindow(GUIWindow.WindowData wd, int w, int h)
        {
            GUIWindow win = new GUIWindow(wd, w, h);
            AddControl(win);
            Width = win.Width;
            Height = win.Height;
            this.CenterOnScreen();

            return win;
        }
    }
}
