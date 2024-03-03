using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIInventory : GUIObject
    {
        protected GUIItemBox[,] _arrItemBoxes;

        protected int BoxSize => GameManager.ScaleIt(GUIUtils.ITEM_BOX.Width);

        protected int _iColumns;
        protected int _iRows;

        private readonly bool _bStandard;
        private readonly bool _bPlayerInventory;

        public GUIInventory(bool PlayerInventory = false, bool standard = true)
        {
            Position(Point.Zero);
            _bStandard = standard;
            _bPlayerInventory = PlayerInventory;

            //Retrieve the dimensions of the Inventory we're working on from the InventoryManager
            InventoryManager.GetDimensions(ref _iRows, ref _iColumns, _bPlayerInventory);

            _arrItemBoxes = new GUIItemBox[_iRows, _iColumns];

            Setup(PlayerInventory);

            //_texture = DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE);
            DetermineSize();
            Width += GameManager.ScaleIt(2);
            Height += GameManager.ScaleIt(2);
        }

        public override void Update(GameTime gTime)
        {
            for (int i = 0; i < _iRows; i++)
            {
                for (int j = 0; j < _iColumns; j++)
                {
                    _arrItemBoxes[i, j].SetItem(InventoryManager.GetItemFromLocation(i, j, _bPlayerInventory));
                    if (_bPlayerInventory && TownManager.AtArchive() && _arrItemBoxes[i, j].BoxItem != null && !TownManager.CanArchiveItem(_arrItemBoxes[i, j].BoxItem))
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
            if (_bStandard)
            {
                for (int i = 0; i < _iRows; i++)
                {
                    for (int j = 0; j < _iColumns; j++)
                    {
                        _arrItemBoxes[i, j] = new GUIItemBox(i, j, null);

                        if (i == 0 && j == 0) { }
                        else if (j == 0) { _arrItemBoxes[i, j].AnchorAndAlignWithSpacing(_arrItemBoxes[i - 1, j], SideEnum.Bottom, SideEnum.Left, 2); }
                        else { _arrItemBoxes[i, j].AnchorAndAlignWithSpacing(_arrItemBoxes[i, j - 1], SideEnum.Right, SideEnum.Bottom, 2); }

                        AddControl(_arrItemBoxes[i, j]);
                    }
                }

                if (playerInventory)
                {
                    int delta = InventoryManager.maxItemRows - _iRows;
                    int rowDelta = InventoryManager.maxItemRows - PlayerManager.BackpackLevel;

                    GUIImage[,] temp = new GUIImage[delta, InventoryManager.maxItemColumns];
                    for (int i = 0; i < rowDelta; i++)
                    {
                        for (int j = 0; j < _iColumns; j++)
                        {
                            temp[i, j] = new GUIImage(GUIUtils.ITEM_BOX);
                            temp[i, j].Enable(false);

                            if (i == 0 && j == 0) { temp[i, j].AnchorAndAlignWithSpacing(_arrItemBoxes[PlayerManager.BackpackLevel - 1, 0], SideEnum.Bottom, SideEnum.Left, 2); }
                            else if (j == 0) { temp[i, j].AnchorAndAlignWithSpacing(temp[i - 1, j], SideEnum.Bottom, SideEnum.Left, 2); }
                            else { temp[i, j].AnchorAndAlignWithSpacing(temp[i, j - 1], SideEnum.Right, SideEnum.Bottom, 2); }

                            AddControl(temp[i, j]);
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < _iRows; i++)
                {
                    for (int j = 0; j < _iColumns; j++)
                    {
                        _arrItemBoxes[i, j] = new GUIItemBox(i, j, null);

                        if (i == 0 && j == 0) { }
                        else if (j == 0) { _arrItemBoxes[i, j].AnchorAndAlignWithSpacing(_arrItemBoxes[i - 1, j], SideEnum.Bottom, SideEnum.Left, 2); }
                        else { _arrItemBoxes[i, j].AnchorAndAlignWithSpacing(_arrItemBoxes[i, j - 1], SideEnum.Right, SideEnum.Bottom, 36); }

                        AddControl(_arrItemBoxes[i, j]);
                    }
                }
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            var itemBox = GetItemBox(mouse);
            if (itemBox != null && itemBox.Active)
            {
                rv = true;
                if (GameManager.HeldItem != null)
                {
                    if (!HoldsItem(GameManager.HeldItem, !_bPlayerInventory))
                    {
                        return false;
                    }

                    bool canPlaceItem = ItemCanGoThere(itemBox, GameManager.HeldItem);

                    var toSwitch = itemBox.BoxItem;
                    if (toSwitch != null)
                    {
                        if (GameManager.HeldItem.ID == toSwitch.ID)
                        {
                            toSwitch.Add(GameManager.HeldItem.Number);
                            GameManager.DropItem();
                        }
                        else if (canPlaceItem)
                        {
                            Item temp = GameManager.HeldItem;
                            GameManager.GrabItem(TakeItem(mouse));
                            GiveItem(temp, true);
                        }

                    }
                    else if (canPlaceItem && GiveItem(GameManager.HeldItem))
                    {
                        GameManager.DropItem();
                    }
                }
                else
                {
                    if (GameManager.CurrentWorldObject != null)
                    {
                        Item clickedItem = IsItemThere(mouse);
                        if (!GUIManager.IsTextWindowOpen() && Contains(mouse) && clickedItem != null)
                        {
                            if (!DungeonHandling(clickedItem))
                            {
                                bool takeHalf = InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift);
                                GameManager.GrabItem(TakeItem(mouse, takeHalf));
                            }
                        }
                    }
                    else
                    {
                        bool takeHalf = InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift);
                        GameManager.GrabItem(TakeItem(mouse, takeHalf));
                    }
                }

                //Close any hover windows that may be open
                if (rv && IsItemThere(mouse) == null)
                {
                    GUIManager.CloseHoverWindow();
                }
            }

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
                if (i != null && i.BoxItem != null && i.Active)
                {
                    if (!HoldsItem(i.BoxItem, _bPlayerInventory))
                    {
                        return false;
                    }

                    //If the only inventory we are working with is the player's inventory,
                    //pass the handler to the GUIItemBox and have it handle the item click
                    if (InventoryManager.CurrentInventoryDisplay == DisplayTypeEnum.PlayerInventory && i.BoxItem.ItemType != ItemEnum.Clothing)
                    {
                        rv = i.BoxItem.ItemBeingUsed();
                    }
                    else if(!DungeonHandling(i.BoxItem))  //We are managing an additional Inventory
                    {
                        int row = 0;
                        int col = 0;

                        Item singleItem = null;
                        bool takeHalf = InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift);
                        if (!takeHalf && _arrItemBoxes.Length != 1 && InventoryManager.ExtraInventory.Length == 1 && InventoryManager.ExtraInventory[0, 0] != null)
                        {
                            singleItem = InventoryManager.ExtraInventory[0, 0];
                            InventoryManager.RemoveItemFromInventorySpot(0, 0, false);
                        }

                        bool performSwap = false;
                        if (InventoryManager.CurrentInventoryDisplay == DisplayTypeEnum.PlayerInventory && _bPlayerInventory)
                        {
                            Point gearPosition = Point.Zero;
                            if (i.BoxItem.ItemType == ItemEnum.Clothing && PlayerManager.GetGearSlot(((Clothing)i.BoxItem).ClothingType, ref gearPosition))
                            {
                                performSwap = true;
                                row = gearPosition.X;
                                col = gearPosition.Y;
                            }
                        }
                        else if (InventoryManager.HasSpaceInInventory(i.BoxItem.ID, i.BoxItem.Number, ref row, ref col, !_bPlayerInventory))
                        {
                            performSwap = true;
                        }

                        if (performSwap)
                        {
                            Item temp = null;
                            if(InventoryManager.CurrentInventoryDisplay == DisplayTypeEnum.PlayerInventory && _bPlayerInventory)
                            {
                                temp = InventoryManager.GetItemFromLocation(row, col, !_bPlayerInventory);
                                if (temp != null)
                                {
                                    InventoryManager.RemoveItemFromInventorySpot(row, col, !_bPlayerInventory);
                                }
                            }

                            Item clickedItem = TakeItem(mouse, takeHalf);
                            //If the GUI represents a Container, move the Item to the PlayerInventory
                            //else, move the Item to the Container's inventory
                            InventoryManager.AddItemToInventorySpot(clickedItem, row, col, !_bPlayerInventory);

                            if (singleItem != null)
                            {
                                InventoryManager.AddToInventory(singleItem.ID, singleItem.Number, _bPlayerInventory, true);
                            }

                            if (temp != null)
                            {
                                _arrItemBoxes[i.RowID, i.ColumnID].SetItem(temp);
                                InventoryManager.AddItemToInventorySpot(temp, i.RowID, i.ColumnID, _bPlayerInventory);
                            }
                        }

                        rv = true;
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

        private bool DungeonHandling(Item clickedItem)
        {
            bool rv = false;
            if (GameManager.CurrentWorldObject != null && GameManager.CurrentWorldObject is TriggerObject triggerObj)
            {
                rv = true;

                if (triggerObj.CheckForKey(clickedItem))
                {
                    triggerObj.AttemptToTrigger(Constants.TRIGGER_ITEM_OPEN);
                    GUIManager.CloseMainObject();
                }
            }

            return rv;
        }

        private bool HoldsItem(Item i, bool goingToPlayerInventory)
        {
            bool playerInventoryCheck = _bPlayerInventory || InventoryManager.HoldItem == ItemEnum.None || i.ItemType == InventoryManager.HoldItem;
            bool validIDsCheck = !goingToPlayerInventory || InventoryManager.ValidIDs == null || InventoryManager.ValidIDs.Contains(i.ID);
            bool shopToolCheck = InventoryManager.CurrentInventoryDisplay != DisplayTypeEnum.ShopTable || !i.CompareType(ItemEnum.Special, ItemEnum.Tool);

            return playerInventoryCheck && validIDsCheck && shopToolCheck;
        }

        private GUIItemBox GetItemBox(Point mouse)
        {
            GUIItemBox rv = null;

            foreach (GUIItemBox box in _arrItemBoxes)
            {
                if (box.Contains(mouse))
                {
                    rv = box;
                    break;
                }
            }

            return rv;
        }

        private bool ItemCanGoThere(GUIItemBox itemBox, Item testItem)
        {
            return itemBox.EquipmentType == EquipmentEnum.None || itemBox.EquipmentType == testItem.GetEnumByIDKey<EquipmentEnum>("Subtype");
        }

        public GUIItemBox GetItemBox(int x, int y)
        {
            if (x < 0 || x >= _iRows) { return null; }
            else if (y < 0 && y >= _iColumns) { return null; }
            else { return _arrItemBoxes[x, y]; }
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
                                _arrItemBoxes[i, j].SetItem(item);
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
        public GUIInventory Inventory { get; protected set; }
        protected GUIInventoryWindow() { }
        public GUIInventoryWindow(bool PlayerInventory)
        {
            _winData = GUIUtils.WINDOW_DARKBLUE;

            Inventory = new GUIInventory(PlayerInventory);
            Inventory.ScaledMoveBy(7, 6);
            AddControl(Inventory);

            Width = WidthEdges() + Inventory.Width;
            Height = HeightEdges() + Inventory.Height;
        }
    }

    public class GUIPlayerGearInventoryWindow : GUIInventoryWindow
    {
        public GUIPlayerGearInventoryWindow()
        {
            _winData = GUIUtils.WINDOW_DARKBLUE;

            Width = GameManager.ScaleIt(90);
            Height = GameManager.ScaleIt(74);

            Inventory = new GUIInventory(false, false);
            Inventory.ScaledMoveBy(7, 6);
            AddControl(Inventory);

            Width = WidthEdges() + Inventory.Width;
            Height = HeightEdges() + Inventory.Height;
        }
    }
}
