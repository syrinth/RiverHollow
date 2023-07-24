using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIInventory : GUIObject
    {
        protected GUIItemBox[,] _arrItemBoxes;

        protected int BoxSize => GameManager.ScaleIt(GUIUtils.ITEM_BOX.Width);

        protected int _iColumns;
        protected int _iRows;

        private bool _bPlayerInventory;

        public GUIInventory(bool PlayerInventory = false)
        {
            Position(Point.Zero);
            
            _bPlayerInventory = PlayerInventory;

            //Retrieve the dimensions of the Inventory we're working on from the InventoryManager
            InventoryManager.GetDimensions(ref _iRows, ref _iColumns, _bPlayerInventory);

            //If it's the player inventory, we need to show the max and then grey them out, otherwise show the real number of rows
            int rows = PlayerInventory ? InventoryManager.maxItemRows : _iRows;

            _arrItemBoxes = new GUIItemBox[_iRows, _iColumns];
            
            Setup(PlayerInventory);

            //_texture = DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE);
            Width = (_iColumns * BoxSize) + GameManager.ScaleIt(_iColumns + 1);
            Height = (rows * BoxSize) + GameManager.ScaleIt(rows + 1);
        }

        public override void Update(GameTime gTime)
        {
            for (int i = 0; i < _iRows; i++)
            {
                for (int j = 0; j < _iColumns; j++)
                {
                    _arrItemBoxes[i, j].SetItem(InventoryManager.GetItemFromLocation(i, j, _bPlayerInventory));
                    if (TownManager.AtArchive() && !TownManager.CanArchiveItem(_arrItemBoxes[i, j].BoxItem))
                    {
                        _arrItemBoxes[i, j].Enable(false);
                    }
                    _arrItemBoxes[i, j].Update(gTime);
                }
            }
        }

        /// <summary>
        /// Centers the gui object on the screen, then initializes
        /// the ItemBox array and positions them appropriately.
        /// </summary>
        private void Setup(bool playerInventory)
        {
            int delta = InventoryManager.maxItemRows - _iRows;
            for (int i = 0; i < _iRows; i++)
            {
                for (int j = 0; j < _iColumns; j++)
                {
                    _arrItemBoxes[i, j] = new GUIItemBox(i, j, null);

                    if (i == 0 && j == 0) { }
                    else if (j == 0) { _arrItemBoxes[i, j].AnchorAndAlignWithSpacing(_arrItemBoxes[i - 1, j], SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN); }
                    else { _arrItemBoxes[i, j].AnchorAndAlignWithSpacing(_arrItemBoxes[i, j - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

                    AddControl(_arrItemBoxes[i, j]);
                }
            }

            if (playerInventory)
            {
                int rowDelta = InventoryManager.maxItemRows - PlayerManager.BackpackLevel;

                GUIImage[,] temp = new GUIImage[delta, InventoryManager.maxItemColumns];
                for (int i = 0; i < rowDelta; i++)
                {
                    for (int j = 0; j < _iColumns; j++)
                    {
                        temp[i, j] = new GUIImage(GUIUtils.ITEM_BOX);
                        temp[i, j].Enable(false);

                        if (i == 0 && j == 0) { temp[i, j].AnchorAndAlignWithSpacing(_arrItemBoxes[PlayerManager.BackpackLevel - 1, 0], SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN); }
                        else if (j == 0) { temp[i, j].AnchorAndAlignWithSpacing(temp[i - 1, j], SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN); }
                        else { temp[i, j].AnchorAndAlignWithSpacing(temp[i, j - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

                        AddControl(temp[i, j]);
                    }
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (GameManager.HeldItem != null)
            {
                Item toSwitch = IsItemThere(mouse);
                if (toSwitch != null)
                {
                    if (GameManager.HeldItem.ID == toSwitch.ID)
                    {
                        toSwitch.Add(GameManager.HeldItem.Number);
                        GameManager.DropItem();
                    }
                    else
                    {
                        Item temp = GameManager.HeldItem;
                        GameManager.GrabItem(TakeItem(mouse));
                        GiveItem(temp, true);
                    }
                }
                else if (GiveItem(GameManager.HeldItem))
                {
                    GameManager.DropItem();
                    rv = true;
                }
            }
            else
            {
                if (GameManager.CurrentWorldObject != null)
                {
                    Item clickedItem = IsItemThere(mouse);
                    if (!GUIManager.IsTextWindowOpen() && Contains(mouse) && clickedItem != null)
                    {
                        if (GameManager.CurrentWorldObject.CompareType(ObjectTypeEnum.DungeonObject))
                        {
                            if (((TriggerObject)GameManager.CurrentWorldObject).CheckForKey(clickedItem))
                            {
                                rv = true;
                                ((TriggerObject)GameManager.CurrentWorldObject).AttemptToTrigger(Constants.TRIGGER_ITEM_OPEN);
                                GUIManager.CloseMainObject();
                            }
                        }
                    }
                }
                else
                {
                    bool takeHalf = InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift);
                    rv = GameManager.GrabItem(TakeItem(mouse, takeHalf));
                }
            }

            //Close any hover windows that may be open
            if (rv && IsItemThere(mouse) == null) { GUIManager.CloseHoverWindow(); }

            return rv;
        }

        /// <summary>
        /// Handler for right clicking the GUI Inventory.
        /// </summary>
        /// <param name="mouse"></param>
        /// <returns></returns>
        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            //If we're not holding anything
            if (GameManager.HeldItem == null)
            {
                //Retrieve the item from the GUIBox we clicked on
                GUIItemBox i = GetItemBox(mouse);
                if (i != null && i.BoxItem != null)
                {
                    //If the only inventory we are working with is the player's inventory,
                    //pass the handler to the GUIItemBox and have it handle the item click
                    if (!InventoryManager.ManagingExtraInventory())
                    {
                        rv = i.BoxItem.ItemBeingUsed();
                    }
                    else  //We are managing an additional Inventory
                    {
                        //Ensure that there is a GUIBox where we clicked, and the box has an Item
                        if (i != null && i.BoxItem != null)
                        {
                            int row = 0;
                            int col = 0;

                            Item singleItem = null;
                            bool takeHalf = InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift);
                            if (!takeHalf && this._arrItemBoxes.Length != 1 && InventoryManager.ExtraInventory.Length == 1 && InventoryManager.ExtraInventory[0, 0] != null)
                            {
                                singleItem = InventoryManager.ExtraInventory[0, 0];
                                InventoryManager.RemoveItemFromInventorySpot(0, 0, false);
                            }

                            //Use _container != null to get the status of the inverse of whichever we are clicking on
                            if (InventoryManager.HasSpaceInInventory(i.BoxItem.ID, i.BoxItem.Number, ref row, ref col, !_bPlayerInventory))
                            {
                                Item clickedItem = TakeItem(mouse, takeHalf);
                                //If the GUI represents a Container, move the Item to the PlayerInventory
                                //else, move the Item to the Container's inventory
                                InventoryManager.AddItemToInventorySpot(clickedItem, row, col, !_bPlayerInventory);

                                if(singleItem != null)
                                {
                                    InventoryManager.AddToInventory(singleItem.ID, singleItem.Number, _bPlayerInventory, true);
                                }
                            }

                            rv = true;
                        }
                    }
                }
            }
            else
            {
                InventoryManager.AddToInventory(GameManager.HeldItem);
                GameManager.DropItem();
            }

            //Close any hover windows that may be open
            if (rv && IsItemThere(mouse) == null) { GUIManager.CloseHoverWindow(); }

            return rv;
        }

        private GUIItemBox GetItemBox(Point mouse)
        {
            GUIItemBox rv = null;

            foreach (GUIItemBox box in _arrItemBoxes)
            {
                if (box.Contains(mouse) && box.BoxItem != null)
                {
                    rv = box;
                    break;
                }
            }

            return rv;
        }

        private Item IsItemThere(Point mouse)
        {
            Item rv = null;

            foreach(GUIItemBox box in _arrItemBoxes)
            {
                if(box.Contains(mouse) && box.BoxItem != null)
                {
                    rv = box.BoxItem;
                    break;
                }
            }

            return rv;
        }

        private Item TakeItem(Point mouse, bool takeHalf = false)
        {
            Item rv = null;

            foreach (GUIItemBox box in _arrItemBoxes)
            {
                if (box.Contains(mouse) && box.BoxItem != null)
                {
                    Item chosenItem = box.BoxItem;
                    if (InventoryManager.ExtraHoldSingular && chosenItem.Number > 1)
                    {
                        chosenItem.Remove(1);
                        rv = DataManager.GetItem(chosenItem.ID, 1);
                    }
                    else
                    {
                        if (takeHalf && chosenItem.Stacks())
                        {
                            int num = chosenItem.Number;
                            if (num > 1) { num /= 2; }
                            chosenItem.Remove(num);
                            rv = DataManager.GetItem(chosenItem.ID, num);
                        }
                        else
                        {
                            rv = chosenItem;
                        }

                        if (!takeHalf || !chosenItem.Stacks())
                        {
                            box.SetItem(null);
                            InventoryManager.RemoveItemFromInventorySpot(box.RowID, box.ColumnID, _bPlayerInventory);
                        }
                    }

                    break;
                }
            }

            return rv;
        }

        private bool GiveItem(Item item)
        {
            return GiveItem(item, false);
        }

        private bool GiveItem(Item item, bool Force)
        {
            bool rv = false;
            if (item != null)
            {
                Point mouse = new Point(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);
                if (_drawRect.Contains(mouse))
                {
                    for (int i = 0; i < _iRows; i++)
                    {
                        for (int j = 0; j < _iColumns; j++)
                        {
                            if (_arrItemBoxes[i, j].Contains(mouse) && (Force || _arrItemBoxes[i, j].BoxItem == null))
                            {
                                rv = InventoryManager.AddItemToInventorySpot(item, i, j, _bPlayerInventory);
                                goto Exit;
                            }
                        }
                    }
                }
            }
Exit:

            return rv;
        }
    }

    public class GUIInventoryWindow : GUIWindow
    {
        GUIInventory _inventory;
        public GUIInventoryWindow(bool PlayerInventory = false)
        {
            _winData = GUIUtils.DarkBlue_Window;

            _inventory = new GUIInventory(PlayerInventory);
            _inventory.ScaledMoveBy(7, 6);
            AddControl(_inventory);

            Width = WidthEdges() + _inventory.Width;
            Height = HeightEdges() + _inventory.Height;
        }
    }
}
