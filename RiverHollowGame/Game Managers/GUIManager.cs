using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

using RiverHollow.Misc;
using RiverHollow.GUIComponents;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.GUIComponents.Screens.HUDComponents;

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
        public static GUIScreen NewScreen { get; private set; }
        public static GUIScreen CurrentScreen { get; private set; }
        public static TextEntry QueuedWindowText { get; private set; }

        private static GUIImage _fadeImg;
        private static float _fFadeVal = 0f;
        private static bool _bFadeSlow = false;
        public static bool Fading  => _eFade != Fade.None;
        public static bool FadingIn => _eFade == Fade.In;
        public static bool NotFading => _eFade == Fade.None;

        public static void LoadContent()
        {
            _fadeImg = new GUIImage(GUIUtils.BLACK_BOX, RiverHollow.ScreenWidth*2, RiverHollow.ScreenHeight*2, DataManager.HUD_COMPONENTS);
            GUICursor.LoadContent();
        }

        public static void Update(GameTime gTime)
        {
            if (Fading)
            {
                UpdateFade();
            }

            CurrentScreen?.Update(gTime);
            GUICursor.Update();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (Fading)
            {
                _fadeImg.Draw(spriteBatch, _fFadeVal);

            }
            else
            {
                CurrentScreen?.Draw(spriteBatch);

                GUICursor.Draw(spriteBatch);
            }
        }

        public static bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (CurrentScreen != null)
            {
                rv = CurrentScreen.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public static bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;
            if (CurrentScreen != null)
            {
                rv = CurrentScreen.ProcessRightButtonClick(mouse);
            }

            return rv;
        }

        public static bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (CurrentScreen != null)
            {
                rv = CurrentScreen.ProcessHover(mouse);
            }

            return rv;
        }

        public static void OpenMenu() { CurrentScreen.OpenMenu(); }
        public static void CloseMenu() { CurrentScreen.CloseMenu(); }
        public static bool IsMenuOpen() { return CurrentScreen.IsMenuOpen(); }
        public static HUDMenu GetMenu() { return CurrentScreen.GetMenu(); }

        public static void QueueTextWindow(TextEntry txt)
        {
            QueuedWindowText = txt;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">Text to speak</param>
        /// <param name="talker">NPC being talked to</param>
        /// <param name="open">Whether or not to play the animation for an opening window.</param>
        public static void OpenTextWindow(string textEntryKey, params object[] formatParameters)
        {
            OpenTextWindow(DataManager.GetGameTextEntry(textEntryKey, formatParameters));
        }
        public static void OpenTextWindow(string textEntryKey, bool open = true)
        {
            OpenTextWindow(DataManager.GetGameTextEntry(textEntryKey), open);
        }
        public static void OpenTextWindow(TextEntry text, bool open = true, bool displayDialogueIcon = false)
        {
            CurrentScreen.OpenTextWindow(text, open, displayDialogueIcon);
        }
        public static void OpenTextWindow(TextEntry text, TalkingActor talker, bool open = true, bool displayDialogueIcon = false)
        {
            CurrentScreen.OpenTextWindow(text, talker, open, displayDialogueIcon);
        }
        public static bool CloseTextWindow()
        {
            bool rv = CurrentScreen.CloseTextWindow();

            if (QueuedWindowText != null)
            {
                OpenTextWindow(QueuedWindowText);
                QueuedWindowText = null;
            }
            return rv;
        }
        public static bool IsTextWindowOpen() { return CurrentScreen.IsTextWindowOpen(); }

        public static void SetWindowText(TextEntry value, bool displayDialogueIcon = false)
        {
            CurrentScreen.SetWindowText(value, displayDialogueIcon);
        }

        public static bool IsHoverWindowOpen() { return CurrentScreen.IsHoverWindowOpen(); }
        public static void CloseHoverWindow() {
            CurrentScreen.CloseHoverWindow();
        }
        public static void OpenHoverObject(GUITextWindow window, Rectangle area, bool guiObject)
        {
            window.ProcessClicks = false;
            CurrentScreen.OpenHoverWindow(window, area, guiObject);
        }
        public static void OpenHoverObject(GUIObject hoverObj, Rectangle area, bool guiObject)
        {
            CurrentScreen.OpenHoverWindow(hoverObj, area, guiObject);
        }

        public static void AssignBackgroundImage(GUIImage newImage)
        {
            CurrentScreen.AssignBackgroundImage(newImage);
        }

        public static void ClearBackgroundImage()
        {
            CurrentScreen.ClearBackgroundImage();
        }

        #region MainObject Control
        public static bool IsMainObjectOpen() { return CurrentScreen.IsMainObjectOpen(); }
        public static void OpenMainObject(GUIMainObject o) { CurrentScreen.OpenMainObject(o); }
        public static bool CloseMainObject() { return CurrentScreen.CloseMainObject(); }
        #endregion

        public static void SetScreen(GUIScreen newScreen)
        {
            CurrentScreen = newScreen;
            NewScreen = null;
        }
        public static void SetNewScreen(GUIScreen newScreen)
        {
            NewScreen = newScreen;
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

        public static void BeginFadeIn(bool slowFadeout = false)
        {
            _fFadeVal = 1;
            _bFadeSlow = slowFadeout;
            _eFade = Fade.In;
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
                BeginFadeIn(_bFadeSlow);
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

        public static void NewAlertIcon(string text, Color c)
        {
            CurrentScreen.NewAlertIcon(text, c);
        }
        public static void NewAlertIcon(string text) {
            CurrentScreen.NewAlertIcon(text, Color.Black);
        }

        public static void AddSkipCutsceneButton()
        {
            CurrentScreen.AddSkipCutsceneButton();
        }
        public static void RemoveSkipCutsceneButton()
        {
            CurrentScreen.RemoveSkipCutsceneButton();
        }
    }

    public abstract class GUIMainObject : GUIObject
    {
        protected GUIWindow _winMain;

        public GUIMainObject() : base()
        {
            RemoveScreen();
        }

        /// <summary>
        /// Creates a new GUIWindow, adds it to the Controls of the object, sets the Height
        /// and Width of the object to that of the GUIWindow then centers it on the screen.
        /// </summary>
        /// <returns>The created GUIWindow</returns>
        protected GUIWindow SetMainWindow()
        {
            return SetMainWindow(GUIUtils.WINDOW_BROWN, GUIManager.MAIN_COMPONENT_WIDTH, GUIManager.MAIN_COMPONENT_HEIGHT);
        }
        protected GUIWindow SetMainWindow(int w, int h)
        {
            return SetMainWindow(GUIUtils.WINDOW_BROWN, w, h);
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

        public virtual void CloseMainWindow()
        {

        }
    }
}
