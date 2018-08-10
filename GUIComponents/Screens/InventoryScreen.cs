using Microsoft.Xna.Framework;
using RiverHollow.Screens;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using RiverHollow.Actors;
using System.Collections.Generic;
using static RiverHollow.WorldObjects.WorldItem;
using static RiverHollow.WorldObjects.Door;
using static RiverHollow.GUIObjects.GUIObject;

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
            _inventory = new Inventory(4, InventoryManager.maxItemColumns, 32);
            Controls.Add(_inventory);
        }

        public InventoryScreen(CharacterDetailWindow c)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(4, InventoryManager.maxItemColumns, 32);
            Controls.Add(_inventory);
            InventoryManager.PublicContainer = null;
        }

        public InventoryScreen(Container c)
        {
            Vector2 centerPoint = new Vector2(RiverHollow.ScreenWidth / 2, RiverHollow.ScreenHeight / 2);
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _container = new Inventory(c, 32);
            _inventory = new Inventory(4, InventoryManager.maxItemColumns, 32);

            Vector2 contWidthHeight = new Vector2(_container.MidWidth(), _container.InnerRectangle().Height);
            Vector2 mainWidthHeight = new Vector2(_inventory.MidWidth(), _inventory.InnerRectangle().Height);

            _inventory.Setup();
            _container.Setup();
            _container.AnchorAndAlignToObject(_inventory, SideEnum.Top, SideEnum.CenterX);           

            List<GUIObject> liWins = new List<GUIObject>() { _container, _inventory };
            GUIObject.CenterAndAlignToScreen(ref liWins);

            Controls.Add(_inventory);
            Controls.Add(_container);
            InventoryManager.PublicContainer = _container.Container;
        }

        public InventoryScreen(Villager n)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(n, 4, InventoryManager.maxItemColumns, 32);

            Vector2 mainWidthHeight = new Vector2(_inventory.InnerRectangle().Width, _inventory.InnerRectangle().Height);
            _inventory.Setup();

            Controls.Add(_inventory);
            InventoryManager.PublicContainer = null;
        }

        public InventoryScreen(KeyDoor door)
        {
            _font = GameContentManager.GetFont(@"Fonts\Font");
            _inventory = new Inventory(door, 4, InventoryManager.maxItemColumns, 32);

            Vector2 mainWidthHeight = new Vector2(_inventory.InnerRectangle().Width, _inventory.InnerRectangle().Height);
            _inventory.Setup();

            Controls.Add(_inventory);
            InventoryManager.PublicContainer = null;
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_inventory.Contains(mouse))
            {
                _inventory.ProcessLeftButtonClick(mouse, _container == null);
                rv = true;
            }
            else if (_container != null && _container.Contains(mouse))
            {
                _container.ProcessLeftButtonClick(mouse, _container == null);
                rv = true;
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = true;
            if (_inventory.Contains(mouse))
            {
                rv = _inventory.ProcessRightButtonClick(mouse);
                if (rv)
                {
                    if (GraphicCursor.HeldItem != null && _container != null)
                    {
                        //InventoryManager.AddNewItemToFirstAvailableInventorySpot(GraphicCursor.HeldItem.ItemID);
                       // GraphicCursor.DropItem();
                    }
                }
            }
            else if (_container != null && _container.DrawRectangle.Contains(mouse))
            {
               rv = _container.ProcessRightButtonClick(mouse);
            }
            else if (_container != null && !_container.DrawRectangle.Contains(mouse))
            {
                GameManager.GoToWorldMap();
            }
            
            return rv;
        }

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;
            if (!_inventory.ProcessHover(mouse))
            {
                rv = false;
            }
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
