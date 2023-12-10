using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDInventoryDisplay : GUIMainObject
    {
        readonly GUIInventoryWindow _inventory;
        readonly GUIInventoryWindow _altInventory;

        public HUDInventoryDisplay(DisplayTypeEnum display = DisplayTypeEnum.Inventory)
        {
            InventoryManager.LockedInventory = false;

            InventoryManager.ClearExtraInventory();
            InventoryManager.CurrentInventoryDisplay = display;
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
            InventoryManager.CurrentInventoryDisplay = display;

            _inventory = new GUIInventoryWindow(true);

            _altInventory = new GUIInventoryWindow(false);
            _altInventory.AnchorAndAlignWithSpacing(_inventory, SideEnum.Top, SideEnum.CenterX, 2);

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
                    InventoryManager.CleanupInventoryDisplay();
                }
            }
            else if (_altInventory != null && _altInventory.DrawRectangle.Contains(mouse) && !InventoryManager.LockedInventory)

            {
                rv = _altInventory.ProcessRightButtonClick(mouse);
            }
            else
            {
                InventoryManager.CleanupInventoryDisplay();
            }

            return rv;
        }

        public override void CloseMainWindow()
        {
            InventoryManager.CleanupInventoryDisplay();
            base.CloseMainWindow();
        }
    }
}
