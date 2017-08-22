using Adventure.Characters.NPCs;
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

        public static void LoadContent()
        {
            GraphicCursor.LoadContent();
        }

        public static void Update(GameTime gameTime) {
            _currentGUIScreen.Update(gameTime);
            GraphicCursor.Update();
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
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
    }
}
