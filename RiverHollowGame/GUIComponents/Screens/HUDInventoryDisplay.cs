using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.WorldObjects;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDInventoryDisplay : GUIMainObject
    {
        GUIInventoryWindow _inventory;
        GUIInventoryWindow _altInventory;

        public HUDInventoryDisplay(DisplayTypeEnum display = DisplayTypeEnum.Inventory)
        {
            InventoryManager.LockedInventory = false;

            InventoryManager.ClearExtraInventory();
            GameManager.CurrentInventoryDisplay = display;
            _inventory = new GUIInventoryWindow(true);
            AddControl(_inventory);

            DetermineSize();
            CenterOnScreen();
        }

        public HUDInventoryDisplay(Item[,] inventory, DisplayTypeEnum display, bool lockExtraInventory = false)
        {
            InventoryManager.LockedInventory = lockExtraInventory;

            InventoryManager.ClearExtraInventory();
            InventoryManager.InitExtraInventory(inventory);
            GameManager.CurrentInventoryDisplay = display;
            _altInventory = new GUIInventoryWindow();
            _inventory = new GUIInventoryWindow(true);

            _altInventory.AnchorAndAlignWithSpacing(_inventory, SideEnum.Top, SideEnum.CenterX, GUIManager.STANDARD_MARGIN);           

            SetY(_altInventory.Top);

            AddControl(_inventory);
            AddControl(_altInventory);

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
            else if (_altInventory != null && _altInventory.Contains(mouse))
            {
                if (!InventoryManager.LockedInventory)
                {
                    _altInventory.ProcessLeftButtonClick(mouse);
                }
                rv = true;
            }
            else
            {
                if(GameManager.HeldItem != null && GameManager.HeldItem.CanBeDropped())
                {
                    InventoryManager.DropItemOnMap(GameManager.HeldItem);
                    GameManager.DropItem();
                }
            }

            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if (_inventory.Contains(mouse))
            {
                rv = _inventory.ProcessRightButtonClick(mouse);
                if (!rv)
                {
                    Close();
                }
            }
            else if (_altInventory != null && _altInventory.DrawRectangle.Contains(mouse) && !InventoryManager.LockedInventory)
            {
                rv = _altInventory.ProcessRightButtonClick(mouse);
            }
            else
            {
                Close();
            }

            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        private void Close()
        {
            if (GameManager.HeldItem != null && _altInventory != null)
            {
                InventoryManager.AddToInventory(GameManager.HeldItem);
                GameManager.DropItem();
            }
            if (GameManager.CurrentNPC != null)
            {
                GUIManager.OpenTextWindow(GameManager.CurrentNPC.GetDialogEntry("Goodbye"));
            }

            if (GameManager.CurrentWorldObject != null && GameManager.CurrentWorldObject.BuildableType(BuildableEnum.Decor))
            {
                ((Decor)GameManager.CurrentWorldObject).SetDisplayEntity(InventoryManager.ExtraInventory[0,0], false);
            }

            InventoryManager.ExtraHoldSingular = false;
        }
    }
}
