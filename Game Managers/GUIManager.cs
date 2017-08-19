using Adventure.Characters.NPCs;
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
    class GUIManager
    {
        static GUIManager instance;
        private GUIScreen _currentGUIScreen;

        private GUIManager()
        {
        }

        public static GUIManager GetInstance()
        {
            if (instance == null)
            {
                instance = new GUIManager();
            }
            return instance;
        }

        public void LoadContent()
        {
            GraphicCursor.LoadContent();
        }

        public void Update(GameTime gameTime) {
            _currentGUIScreen.Update(gameTime);
            GraphicCursor.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _currentGUIScreen.Draw(spriteBatch);

            GraphicCursor.Draw(spriteBatch);
        }

        public bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            rv = _currentGUIScreen.ProcessLeftButtonClick(mouse);

            return rv;
        }

        public bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            rv = _currentGUIScreen.ProcessRightButtonClick(mouse);

            return rv;
        }

        public bool ProcessHover(Point mouse)
        {
            bool rv = false;
            rv = _currentGUIScreen.ProcessHover(mouse);

            return rv;
        }

        public void OpenShopWindow(ShopKeeper shop)
        {
            _currentGUIScreen = new ShopScreen(shop);
        }

        public void LoadMainMenu()
        {
            _currentGUIScreen = new MainMenuScreen();
        }

        public void LoadMainGame()
        {
            _currentGUIScreen = new HUDScreen();
        }
    }
}
