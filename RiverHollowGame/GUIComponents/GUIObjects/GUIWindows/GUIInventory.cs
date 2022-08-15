using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.GUIComponents.GUIObjects.GUIWindows
{
    public class GUIInventory : GUIWindow
    {
        protected GUIItemBox[,] _arrItemBoxes;

        protected int _iBoxSize = GUIItemBox.RECT_IMG.Width * GameManager.CurrentScale;

        protected int _columns;
        protected int _rows;

        bool _bPlayerInventory;

        public GUIInventory(bool PlayerInventory = false)
        {
            Position(Vector2.Zero);
            _winData = GUIWindow.Window_2;
            _bPlayerInventory = PlayerInventory;

            //Retrieve the dimensions of the Inventory we're working on from the InventoryManager
            InventoryManager.GetDimensions(ref _rows, ref _columns, _bPlayerInventory);

            //If it's the player inventory, we need to show the max and then grey them out, otherwise show the real number of rows
            int rows = PlayerInventory ? InventoryManager.maxItemRows : _rows;

            _arrItemBoxes = new GUIItemBox[_rows, _columns];
            Width = WidthEdges() + (_columns * _iBoxSize) + (GUIManager.STANDARD_MARGIN * (_columns + 1));
            Height = HeightEdges() + (rows * _iBoxSize) + (GUIManager.STANDARD_MARGIN * (rows + 1));
            Setup(PlayerInventory);

            _texture = DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE);
        }

        public override void Update(GameTime gTime)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _arrItemBoxes[i,j].SetItem(InventoryManager.GetItemFromLocation(i, j, _bPlayerInventory));
                    _arrItemBoxes[i, j].Update(gTime);
                }
            }
        }

        /// <summary>
        /// Centers the gui object on the screen, then initializes
        /// the ItemBox array and positions them appropriately.
        /// </summary>
        public void Setup(bool playerInventory)
        {
            int delta = InventoryManager.maxItemRows - _rows;
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _arrItemBoxes[i, j] = new GUIItemBox(i, j, DataManager.DIALOGUE_TEXTURE, null);

                    if (i == 0 && j == 0) { _arrItemBoxes[i, j].AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN); }
                    else if (j == 0) { _arrItemBoxes[i, j].AnchorAndAlignToObject(_arrItemBoxes[i - 1, j], SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN); }
                    else { _arrItemBoxes[i, j].AnchorAndAlignToObject(_arrItemBoxes[i, j - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

                    AddControl(_arrItemBoxes[i, j]);
                }
            }

            if (playerInventory)
            {
                int rowDelta = InventoryManager.maxItemRows - PlayerManager.BackpackLevel;

                GUIImage[,] temp = new GUIImage[delta, InventoryManager.maxItemColumns];
                for (int i = 0; i < rowDelta; i++)
                {
                    for (int j = 0; j < _columns; j++)
                    {
                        temp[i, j] = new GUIImage(GUIItemBox.RECT_IMG, GameManager.ScaleIt(GUIItemBox.RECT_IMG.Width), GameManager.ScaleIt(GUIItemBox.RECT_IMG.Height), DataManager.DIALOGUE_TEXTURE);
                        temp[i, j].Enable(false);

                        if (i == 0 && j == 0) { temp[i, j].AnchorAndAlignToObject(_arrItemBoxes[PlayerManager.BackpackLevel - 1, 0], SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN); }
                        else if (j == 0) { temp[i, j].AnchorAndAlignToObject(temp[i - 1, j], SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN); }
                        else { temp[i, j].AnchorAndAlignToObject(temp[i, j - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

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
                    if (GameManager.HeldItem.ItemID == toSwitch.ItemID)
                    {
                        toSwitch.Add(1);
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
                Item clickedItem = IsItemThere(mouse);
                if (GameManager.CurrentInventoryDisplay == DisplayTypeEnum.Gift)
                {
                    if (clickedItem != null && clickedItem.Giftable())
                    {
                        rv = true;
                        //Do not pick the item up, instead assign it.
                        GameManager.SetSelectedItem(clickedItem);
                        
                    }
                }
                else if (GameManager.CurrentWorldObject != null)
                {

                    if (!GUIManager.IsTextWindowOpen() && Contains(mouse) && clickedItem != null)
                    {
                        if (GameManager.CurrentWorldObject.CompareType(ObjectTypeEnum.DungeonObject))
                        {
                            if (((TriggerObject)GameManager.CurrentWorldObject).CheckForKey(clickedItem))
                            {
                                ((TriggerObject)GameManager.CurrentWorldObject).AttemptToTrigger(Constants.TRIGGER_ITEM_OPEN);
                                GUIManager.CloseMainObject();
                            }
                        }
                        else if (GameManager.CurrentWorldObject.CompareType(ObjectTypeEnum.Decor))
                        {
                            ((Decor)GameManager.CurrentWorldObject).SetDisplayItem(clickedItem);
                        }
                    }
                }
                else
                {
                    rv = true;
                    bool takeHalf = InputManager.IsKeyDown(Keys.LeftShift) || InputManager.IsKeyDown(Keys.RightShift);
                    rv = GameManager.GrabItem(TakeItem(mouse, takeHalf));
                }
            }

            //Close any hover windows that may be open
            if (rv) { GUIManager.CloseHoverWindow(); }


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
                        rv = i.ProcessRightButtonClick(mouse);
                    }
                    else  //We are managing an additional Inventory
                    {
                        //Ensure that there is a GUIBox where we clicked, and the box has an Item
                        if (i != null && i.BoxItem != null)
                        {
                            int row = 0;
                            int col = 0;

                            //Use _container != null to get the status of the inverse of whichever we are clicking on
                            if (InventoryManager.HasSpaceInInventory(i.BoxItem.ItemID, i.BoxItem.Number, ref row, ref col, !_bPlayerInventory))
                            {
                                Item clickedItem = TakeItem(mouse);
                                //If the GUI represents a Container, move the Item to the PlayerInventory
                                //else, move the Item to the Container's inventory
                                InventoryManager.AddItemToInventorySpot(clickedItem, row, col, !_bPlayerInventory);
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
            if (rv) { GUIManager.CloseHoverWindow(); }

            return rv;
        }

        /// <summary>
        /// Triggers a ProcessHover for each GUIItemBox
        /// </summary>
        /// <param name="mouse"></param>
        /// <returns></returns>
        public override bool ProcessHover(Point mouse)
        {
            bool rv = false;
            if (!GUIManager.IsTextWindowOpen())
            {
                foreach (GUIItemBox i in _arrItemBoxes)
                {
                    if (i.ProcessHover(mouse))
                    {
                        rv = true;
                    }
                }
            }
            return rv;
        }

        private Vector2 GetItemVector(Point mouse)
        {
            Vector2 rv = new Vector2(-1, -1);
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_arrItemBoxes[i, j].Contains(mouse) && _arrItemBoxes[i, j].BoxItem != null)
                    {
                        rv = new Vector2(i, j);
                        goto Exit;
                    }
                }
            }
        Exit:
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

        public Item TakeItem(Point mouse, bool takeHalf = false)
        {
            Item rv = null;

            foreach (GUIItemBox box in _arrItemBoxes)
            {
                if (box.Contains(mouse) && box.BoxItem != null)
                {
                    Item chosenItem = box.BoxItem;
                    if (takeHalf && chosenItem.DoesItStack)
                    {
                        int num = chosenItem.Number;
                        num = num / 2;
                        chosenItem.Remove(num);
                        rv = DataManager.GetItem(chosenItem.ItemID, num);
                    }
                    else
                    {
                        rv = chosenItem;
                    }

                    if (!takeHalf)
                    {
                        box.SetItem(null);
                        InventoryManager.RemoveItemFromInventorySpot(box.Rows, box.Columns, _bPlayerInventory);
                    }

                    break;
                }
            }

            return rv;
        }

        public bool GiveItem(Item item)
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
                    for (int i = 0; i < _rows; i++)
                    {
                        for (int j = 0; j < _columns; j++)
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
}
