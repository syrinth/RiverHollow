using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Game_Managers
{
    public static class GUIManager
    {
        private static GUIScreen _currentGUIScreen;
        private static GUIImage _fadeImg;
        private static float _fadeVal = 1f;
        private static bool _fading = false;
        private static bool _slowFade = false;
        public static bool Fading { get => _fading; }

        public static void LoadContent()
        {
            _fadeImg = new GUIImage(Vector2.Zero, new Rectangle(160, 128, 32, 32), RiverHollow.ScreenWidth*2, RiverHollow.ScreenHeight*2, @"Textures\Dialog");
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

        public static void AddTextSelection(Food f, string text)
        {
            _currentGUIScreen.AddTextSelection(f, text);
        }

        public static void RemoveComponent(GUIObject g)
        {
            _currentGUIScreen.RemoveComponent(g);
        }

        public static void SetScreen(GUIScreen newScreen)
        {
            _currentGUIScreen = newScreen;
        }

        public static void FadeOut()
        {
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

        public static bool IsTextScreen() { return _currentGUIScreen != null && _currentGUIScreen.IsTextScreen(); }
        public static bool IsGameMenuScreen() { return _currentGUIScreen != null && _currentGUIScreen.IsGameMenuScreen(); }
        public static bool IsItemCreationScreen() { return _currentGUIScreen != null && _currentGUIScreen.IsItemCreationScreen(); }
        public static bool IsHUD() { return _currentGUIScreen != null && _currentGUIScreen.IsHUD(); }
    }
}
