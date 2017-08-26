using Adventure.Characters;
using Adventure.Characters.NPCs;
using Adventure.Game_Managers.GUIComponents.GUIObjects;
using Adventure.Game_Managers.GUIComponents.Screens;
using Adventure.Game_Managers.GUIObjects;
using Adventure.GUIObjects;
using Adventure.Items;
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
        public static GUIScreen CurrentGUIScreen { get => _currentGUIScreen; }
        public  enum Screens {None, DayEnd, HUD, Inventory, MainMenu,  Shop,  Text };
        private static Texture2D _fadeTexture;
        private static float _fadeVal = 1f;
        private static bool _fading = false;
        public static bool Fading { get => _fading; }

        public static void LoadContent()
        {
            _fadeTexture = GameContentManager.GetTexture(@"Textures\ok");
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
                if (_currentGUIScreen.GetType().Equals(typeof(TextScreen)))
                {
                    if (((TextScreen)_currentGUIScreen).TextFinished())
                    {
                        AdventureGame.ChangeGameState(AdventureGame.GameState.Game);
                        LoadScreen(GUIManager.Screens.HUD);
                    }
                }
                _currentGUIScreen.Update(gameTime);
            }
            GraphicCursor.Update();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_fading)
            {
                spriteBatch.Draw(_fadeTexture, new Rectangle(0, 0, AdventureGame.ScreenWidth, AdventureGame.ScreenHeight), Color.Black * _fadeVal);
            }
            if (_currentGUIScreen != null)
            {
                _currentGUIScreen.Draw(spriteBatch);
            }

            GraphicCursor.Draw(spriteBatch);
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

        public static void LoadScreen(Screens newScreen)
        {
            switch (newScreen)
            {
                case Screens.DayEnd:
                    _currentGUIScreen = new DayEndScreen();
                    return;
                case Screens.HUD:
                    _currentGUIScreen = new HUDScreen();
                    return;
                case Screens.Inventory:
                    _currentGUIScreen = new InventoryScreen();
                    return;
                case Screens.MainMenu:
                    _currentGUIScreen = new MainMenuScreen();
                    return;
                case Screens.None:
                    _currentGUIScreen = null;
                    return;
            }
        }

        public static void LoadScreen(Screens newScreen, Character c)
        {
            switch (newScreen)
            {
                case Screens.Shop:
                    _currentGUIScreen = new ShopScreen((ShopKeeper)c);
                    return;
            }
        }

        public static void LoadScreen(Screens newScreen, Container c)
        {
            switch (newScreen)
            {
                case Screens.Inventory:
                    _currentGUIScreen = new InventoryScreen(c);
                    return;
            }
        }

        public static void LoadScreen(Screens newScreen, string text)
        {
            switch (newScreen)
            {
                case Screens.Text:
                    _currentGUIScreen = new TextScreen(text);
                    return;
            }
        }

        public static void LoadScreen(Screens newScreen, Character c, string text)
        {
            //
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
