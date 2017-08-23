using Adventure.Characters.NPCs;
using Adventure.Game_Managers.GUIComponents.GUIObjects;
using Adventure.Game_Managers.GUIComponents.Screens;
using Adventure.Game_Managers.GUIObjects;
using Adventure.GUIObjects;
using Adventure.Screens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers
{
    public static class GUIManager
    {
        private static GUIScreen _currentGUIScreen;

        private static Texture2D _fadeTexture;
        private static float _fadeVal = 1f;
        private static bool _fading = false;
        public static bool Fading { get => _fading; }

        public static void LoadContent()
        {
            _fadeTexture = GameContentManager.GetTexture(@"Textures\ok");
            GraphicCursor.LoadContent();
        }

        public static void Update(GameTime gameTime) {
            if (_fading)
            {
                UpdateFade();
            }
            if (_currentGUIScreen.GetType().Equals(typeof(TextScreen)))
            {
                if (((TextScreen)_currentGUIScreen).TextFinished()){
                    AdventureGame.ChangeGameState(AdventureGame.GameState.Game);
                    LoadMainGame();
                }
            }
            _currentGUIScreen.Update(gameTime);
            GraphicCursor.Update();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_fading)
            {
                spriteBatch.Draw(_fadeTexture, new Rectangle(0, 0, AdventureGame.ScreenWidth, AdventureGame.ScreenHeight), Color.Black * _fadeVal);
            }
            _currentGUIScreen.Draw(spriteBatch);

            GraphicCursor.Draw(spriteBatch);
        }

        public static bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            rv = _currentGUIScreen.ProcessLeftButtonClick(mouse);

            return rv;
        }

        public static bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            rv = _currentGUIScreen.ProcessRightButtonClick(mouse);

            return rv;
        }

        public static bool ProcessHover(Point mouse)
        {
            bool rv = false;
            rv = _currentGUIScreen.ProcessHover(mouse);

            return rv;
        }

        public static void OpenShopWindow(ShopKeeper shop)
        {
            _currentGUIScreen = new ShopScreen(shop);
        }

        public static void LoadMainMenu()
        {
            _currentGUIScreen = new MainMenuScreen();
        }

        public static void LoadMainGame()
        {
            _currentGUIScreen = new HUDScreen();
        }

        public static void LoadEndOfDay()
        {
            _currentGUIScreen = new DayEndScreen();
        }

        public static void OpenTextWindow(string text)
        {
            AdventureGame.ChangeGameState(AdventureGame.GameState.Paused);
            _currentGUIScreen = new TextScreen(text);
        }

        public static void FadeOut()
        {
            _fading = true;
        }

        private static void UpdateFade()
        {
            _fadeVal -= 0.05f;
            if (_fadeVal <= 0)
            {
                _fadeVal = 1;
                _fading = false;
            }
        }
    }
}
