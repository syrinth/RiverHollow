﻿using Microsoft.Xna.Framework;
using RiverHollow.Screens;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class HUDInventoryDisplay : GUIObject
    {
        private GUIInventory _inventory;
        private GUIInventory _container;

        public HUDInventoryDisplay()
        {
            InventoryManager.ClearExtraInventory();
            _inventory = new GUIInventory(true);
            AddControl(_inventory);

            DetermineSize();
            CenterOnScreen();
        }

        public HUDInventoryDisplay(Item[,] inventory)
        {
            InventoryManager.ClearExtraInventory();

            InventoryManager.InitContainerInventory(inventory);
            _container = new GUIInventory();
            _inventory = new GUIInventory(true);

            _inventory.Setup();
            _container.Setup();
            _container.AnchorAndAlignToObject(_inventory, SideEnum.Top, SideEnum.CenterX);           

            SetY(_container.Top);

            AddControl(_inventory);
            AddControl(_container);

            DetermineSize();
            CenterOnScreen();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_inventory.Contains(mouse))
            {
                rv = _inventory.ProcessLeftButtonClick(mouse);
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
                GUIManager.OpenTextWindow(GameManager.CurrentNPC.GetDialogEntry("Goodbye"));
                GameManager.RemoveCurrentNPCLockObject();
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
