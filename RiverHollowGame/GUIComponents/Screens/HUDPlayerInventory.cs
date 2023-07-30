using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDPlayerInventory : GUIMainObject
    {
        readonly GUIInventoryWindow _playerInventory;
        readonly GUIInventoryWindow _altInventory;
        readonly PlayerDisplayBox _playerDisplay;

        public HUDPlayerInventory()
        {
            InventoryManager.LockedInventory = false;

            InventoryManager.ClearExtraInventory();
            InventoryManager.InitExtraInventory(PlayerManager.PlayerActor.PlayerGear);
            InventoryManager.CurrentInventoryDisplay = DisplayTypeEnum.PlayerInventory;

            _playerInventory = new GUIInventoryWindow(true);

            _altInventory = new GUIPlayerGearInventoryWindow();
            _altInventory.Inventory.GetItemBox(0, 0).SetEquipmentType(EquipmentEnum.Hat);
            _altInventory.Inventory.GetItemBox(1, 0).SetEquipmentType(EquipmentEnum.Shirt);
            _altInventory.Inventory.GetItemBox(2, 0).SetEquipmentType(EquipmentEnum.Pants);
            _altInventory.Inventory.GetItemBox(0, 1).SetEquipmentType(EquipmentEnum.Neck);
            _altInventory.Inventory.GetItemBox(1, 1).SetEquipmentType(EquipmentEnum.Ring);
            _altInventory.Inventory.GetItemBox(2, 1).SetEquipmentType(EquipmentEnum.Ring);

            AddControl(_altInventory);

            _playerDisplay = new PlayerDisplayBox(PlayerManager.PlayerActor, GUIUtils.PLAYER_INVENTORY_PANE, new Point(6, 1));
            _playerDisplay.PositionAndMove(_altInventory, new Point(31, 17));

            _playerInventory.AnchorAndAlignWithSpacing(_altInventory, SideEnum.Bottom, SideEnum.Left, 2, GUIUtils.ParentRuleEnum.ForceToParent);

            var toolboxWindow = new GUIWindow();
            GUIItemBox[] toolbox = new GUIItemBox[2];
            toolbox[0] = new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.Backpack));
            toolbox[0].PositionAndMove(toolboxWindow, new Point(7, 6));

            toolbox[1] = new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.Lantern));
            toolbox[1].AnchorAndAlignWithSpacing(toolbox[0], SideEnum.Right, SideEnum.Bottom, 2);
            toolboxWindow.DetermineSize(2);
            toolboxWindow.AnchorAndAlignWithSpacing(_playerInventory, SideEnum.Top, SideEnum.Right, 2, GUIUtils.ParentRuleEnum.ForceToParent);

            DetermineSize();
            CenterOnScreen();
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;
            if (_playerInventory.Contains(mouse))
            {
                rv = _playerInventory.ProcessLeftButtonClick(mouse);
            }
            else if (_altInventory != null && _altInventory.Contains(mouse))
            {
                if (!InventoryManager.LockedInventory)
                {
                    _altInventory.ProcessLeftButtonClick(mouse);
                }
                rv = true;
            }
            else if (InventoryManager.CurrentInventoryDisplay == DisplayTypeEnum.PlayerInventory)
            {
                if (GameManager.HeldItem != null && GameManager.HeldItem.CanBeDropped())
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

            if (_playerInventory.Contains(mouse))
            {
                rv = _playerInventory.ProcessRightButtonClick(mouse);
                if (!rv)
                {
                    InventoryManager.CleanupInventoryDisplay();
                }
            }
            else if (_altInventory.Contains(mouse))
            {
                rv = _altInventory.ProcessRightButtonClick(mouse);
            }
            else
            {
                InventoryManager.CleanupInventoryDisplay();
            }

            return rv;
        }
    }
}
