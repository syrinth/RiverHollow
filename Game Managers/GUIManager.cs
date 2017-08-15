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
        private InventoryDisplay _inventoryDisplay;

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
            _inventoryDisplay = InventoryDisplay.GetInstance();
            GraphicCursor.LoadContent();
        }

        public void Update(GameTime gameTime) {
            _inventoryDisplay.Update(gameTime);
            GraphicCursor.Update();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _inventoryDisplay.Draw(spriteBatch);
            GraphicCursor.Draw(spriteBatch);
        }

        public bool ProcessLeftButtonClick(Vector2 mouse)
        {
            bool rv = false;
            if (_inventoryDisplay.Rectangle.Contains(mouse)) {
                _inventoryDisplay.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }
    }
}
