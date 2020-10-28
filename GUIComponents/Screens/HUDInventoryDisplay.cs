using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDInventoryDisplay : GUIMainObject
    {
        GUIInventory _inventory;
        GUIInventory _container;

        public HUDInventoryDisplay(DisplayTypeEnum display = DisplayTypeEnum.Inventory)
        {
            InventoryManager.ClearExtraInventory();
            GameManager.CurrentInventoryDisplay = display;
            _inventory = new GUIInventory(true);
            AddControl(_inventory);

            DetermineSize();
            CenterOnScreen();
        }

        public HUDInventoryDisplay(Item[,] inventory, DisplayTypeEnum display = DisplayTypeEnum.Inventory)
        {
            InventoryManager.ClearExtraInventory();
            InventoryManager.InitContainerInventory(inventory);
            GameManager.CurrentInventoryDisplay = display;
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

                if(GameManager.CurrentNPC != null && GameManager.gmActiveItem != null && GameManager.CurrentInventoryDisplay == DisplayTypeEnum.Gift)
                {
                    GUIManager.OpenTextWindow(string.Format(DataManager.GetGameText("GiftConfirm"), GameManager.gmActiveItem.Name, GameManager.CurrentNPC.Name), GameManager.CurrentNPC);
                }
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
            bool rv = false;
            if (_inventory.Contains(mouse))
            {
                rv = _inventory.ProcessRightButtonClick(mouse);
                if (!rv)
                {
                    Close();
                }
            }
            else if (_container != null && _container.DrawRectangle.Contains(mouse))
            {
                rv = _container.ProcessRightButtonClick(mouse);
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
            return rv;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        private void Close()
        {
            if (GameManager.HeldItem != null && _container != null)
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
