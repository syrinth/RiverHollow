using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.Screens
{
    public class HUDPlayerInventory : GUIMainObject
    {
        readonly GUIInventoryWindow _playerInventory;
        readonly GUIInventoryWindow _altInventory;
        readonly PlayerDisplayBox _playerDisplay;

        public HUDPlayerInventory() : base()
        {
            InventoryManager.LockedInventory = false;

            InventoryManager.ClearExtraInventory();
            InventoryManager.CurrentInventoryDisplay = DisplayTypeEnum.PlayerInventory;

            _playerInventory = new GUIInventoryWindow(true);

            AddControl(_altInventory);

            _playerDisplay = new PlayerDisplayBox(GUIUtils.PLAYER_INVENTORY_PANE, new Point(6, 1));
            _playerDisplay.PositionAndMove(_altInventory, new Point(31, 17));

            _playerInventory.AnchorAndAlignWithSpacing(_altInventory, SideEnum.Bottom, SideEnum.Left, 2, GUIUtils.ParentRuleEnum.ForceToParent);

            var toolboxWindow = new GUIWindow(GUIUtils.WINDOW_DARKBLUE);
            List<GUIItemBox> _liToolbox = new List<GUIItemBox>();
            if (Constants.AUTO_TOOL)
            {
                _liToolbox.Add(new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.Backpack)));
                _liToolbox.Add(new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.Axe)));
                _liToolbox.Add(new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.Pick)));
                _liToolbox.Add(new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.Lantern)));
                _liToolbox.Add(new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.FishingRod)));
                _liToolbox.Add(new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.Harp)));
                _liToolbox.Add(new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.WateringCan)));
                _liToolbox.Add(new GUIItemBox());
            }
            else
            {
                _liToolbox.Add(new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.Backpack)));
                _liToolbox.Add(new GUIItemBox(PlayerManager.RetrieveTool(ToolEnum.Lantern)));
            }

            GUIUtils.CreateSpacedGrid(new List<GUIObject>(_liToolbox), toolboxWindow, new Point(7, 6), 4, 2, 2, GUIUtils.ParentRuleEnum.ForceToParent);

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

        public override void CloseMainWindow()
        {
            InventoryManager.CleanupInventoryDisplay();
            PlayerManager.PlayerActor.SetFacing(PlayerManager.PlayerActor.Facing);
            base.CloseMainWindow();
        }
    }
}
