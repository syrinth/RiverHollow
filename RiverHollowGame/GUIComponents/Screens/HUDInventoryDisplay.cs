using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDInventoryDisplay : GUIMainObject
    {
        GUIInventory _inventory;
        GUIInventory _altInventory;

        public HUDInventoryDisplay(DisplayTypeEnum display = DisplayTypeEnum.Inventory)
        {
            InventoryManager.LockedInventory = false;

            InventoryManager.ClearExtraInventory();
            GameManager.CurrentInventoryDisplay = display;
            _inventory = new GUIInventory(true);
            AddControl(_inventory);

            DetermineSize();
            CenterOnScreen();
        }

        public HUDInventoryDisplay(Item[,] inventory, DisplayTypeEnum display, bool lockExtraInventory = false)
        {
            InventoryManager.LockedInventory = lockExtraInventory;

            InventoryManager.ClearExtraInventory();
            InventoryManager.InitContainerInventory(inventory);
            GameManager.CurrentInventoryDisplay = display;
            _altInventory = new GUIInventory();
            _inventory = new GUIInventory(true);

            _altInventory.AnchorAndAlignToObject(_inventory, SideEnum.Top, SideEnum.CenterX, GUIManager.STANDARD_MARGIN);           

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

                if(GameManager.CurrentNPC != null && GameManager.CurrentItem != null && GameManager.CurrentInventoryDisplay == DisplayTypeEnum.Gift)
                {
                    TextEntry entry = DataManager.GetGameTextEntry("GiftConfirm");
                    entry.FormatText(GameManager.CurrentItem.Name(), GameManager.CurrentNPC.Name());
                    GUIManager.OpenTextWindow(entry);
                }
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

        public override bool ProcessHover(Point mouse)
        {
            bool rv = true;
            if (!_inventory.ProcessHover(mouse))
            {
                rv = false;
            }
            if (_altInventory != null && !_altInventory.ProcessHover(mouse))
            {
                rv = false;
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
        }
    }
}
