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
        private GUIInventory _inventory;
        private GUIInventory _container;

        public InventoryScreen()
        {
            InventoryManager.ClearExtraInventory();
            _inventory = new GUIInventory(true);
            Controls.Add(_inventory);
        }

        public InventoryScreen(Container c)
        {
            InventoryManager.ClearExtraInventory();

            InventoryManager.InitContainerInventory(c);
            _container = new GUIInventory();
            _inventory = new GUIInventory(true);

            _inventory.Setup();
            _container.Setup();
            _container.AnchorAndAlignToObject(_inventory, SideEnum.Top, SideEnum.CenterX);           

            List<GUIObject> liWins = new List<GUIObject>() { _container, _inventory };
            GUIObject.CenterAndAlignToScreen(ref liWins);

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
            else if (_container != null && _container.Contains(mouse))
            {
                _container.ProcessLeftButtonClick(mouse);
                rv = true;
            }
            else
            {
                if(GameManager.HeldItem != null)
                {
                    InventoryManager.DropItemOnMap(GameManager.HeldItem);
                    GameManager.DropItem();
                }
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
                    if (GameManager.HeldItem != null && _container != null)
                    {
                        //InventoryManager.AddNewItemToFirstAvailableInventorySpot(GameManager.HeldItem.ItemID);
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
                GameManager.DropItem();
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
