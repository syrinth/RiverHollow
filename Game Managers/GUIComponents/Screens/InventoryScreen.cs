﻿using Microsoft.Xna.Framework;
using RiverHollow.Screens;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Items;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class InventoryScreen : GUIScreen
    {
        private Inventory _inventory;
        private Inventory _container;
        private SpriteFont _font;
        public InventoryScreen()
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight/2), 4, InventoryManager.maxItemColumns, 32);
            Controls.Add(_inventory);
        }
        public InventoryScreen(Container c)
        {
            Vector2 centerPoint = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _container = new Inventory(c, centerPoint, 32);
            _inventory = new Inventory(centerPoint, 4, InventoryManager.maxItemColumns, 32);

            Vector2 contWidthHeight = new Vector2(_container.Rectangle().Width, _container.Rectangle().Height);
            Vector2 mainWidthHeight = new Vector2(_inventory.Rectangle().Width, _inventory.Rectangle().Height);
            _container.SetPosition(centerPoint - new Vector2((contWidthHeight.X/2), contWidthHeight.Y));
            _inventory.SetPosition(centerPoint - new Vector2(mainWidthHeight.X / 2, 0));

            Controls.Add(_inventory);
            Controls.Add(_container);
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_inventory.Contains(mouse))
            {
                _inventory.ProcessLeftButtonClick(mouse);
                rv = true;
            }
            if (_container != null && _container.Contains(mouse))
            {
                _container.ProcessLeftButtonClick(mouse);
                rv = true;
            }
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            if (_inventory.Contains(mouse))
            {
                _inventory.ProcessRightButtonClick(mouse);
                rv = true;
            }
            else if (!_inventory.Contains(mouse) && _container != null && !_container.DrawRectangle.Contains(mouse))
            {
                GUIManager.SetScreen(GUIManager.Screens.HUD);
            }
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;
            rv = _inventory.ProcessHover(mouse);
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
