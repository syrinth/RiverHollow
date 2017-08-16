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
        private InventoryDisplay _inventoryDisplay;
        private ShopWindow _shopWindow;

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
            if (_shopWindow != null) { _shopWindow.Draw(spriteBatch); }
            GraphicCursor.Draw(spriteBatch);
        }

        public void RestoreDefault()
        {
            _inventoryDisplay.Visible = true;
        }

        public bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_inventoryDisplay.Visible && _inventoryDisplay.Rectangle.Contains(mouse)) {
                rv =_inventoryDisplay.ProcessLeftButtonClick(mouse);
            }
            else if(_shopWindow != null && _shopWindow.Visible && _shopWindow.Rectangle.Contains(mouse)) {
                rv = _shopWindow.ProcessLeftButtonClick(mouse);
            }

            return rv;
        }

        public bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if (_shopWindow != null && _shopWindow.Visible && !_shopWindow.Rectangle.Contains(mouse)) {
                _shopWindow.Visible = false;
                _inventoryDisplay.Visible = true;
                rv = true;
            }

            return rv;
        }

        public void OpenShopWindow(ShopKeeper shop)
        {
            _inventoryDisplay.Visible = false;
            _shopWindow = new ShopWindow(shop);
        }
    }
}
