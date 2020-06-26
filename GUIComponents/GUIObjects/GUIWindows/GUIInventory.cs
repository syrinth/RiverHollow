﻿using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;

namespace RiverHollow.Screens
{
    public class GUIInventoriesDisplay : GUIObject
    {
        GUIInventory _gPlayerInventory;
        GUIInventory _gExtraInventory;

        public GUIInventoriesDisplay()
        {
            _gPlayerInventory = new GUIInventory(true);
            _gExtraInventory = new GUIInventory(false);

            _gPlayerInventory.AnchorAndAlignToObject(_gExtraInventory, SideEnum.Bottom, SideEnum.CenterX);

            Width = _gPlayerInventory.Width;
            Height = _gPlayerInventory.Height + _gExtraInventory.Height;

            MoveBy(new Vector2(-((_gPlayerInventory.Width-_gExtraInventory.Width)/2) ,0));

            AddControl(_gPlayerInventory);
            AddControl(_gExtraInventory);

            this.CenterOnScreen();
        }

    }
    public class GUIInventory : GUIWindow
    {
        protected GUIItemBox[,] _gItemBoxes;

        protected int _iBoxSize = GUIItemBox.RECT_IMG.Width * (int)GameManager.Scale;

        protected int _columns;
        protected int _rows;

        bool _bPlayerInventory;

        public GUIInventory(bool PlayerInventory = false)
        {
            Position(Vector2.Zero);
            _winData = GUIWindow.BrownWin;
            _bPlayerInventory = PlayerInventory;

            //Retrieve the dimensions of the Inventory we're working on from the InventoryManager
            InventoryManager.GetDimensions(ref _rows, ref _columns, _bPlayerInventory);

            _gItemBoxes = new GUIItemBox[_rows, _columns];
            Width = WidthEdges() + (_columns * _iBoxSize) + (GUIManager.STANDARD_MARGIN * (_columns + 1));
            Height = HeightEdges() + (_rows * _iBoxSize) + (GUIManager.STANDARD_MARGIN * (_rows + 1));
            Setup();

            _texture = DataManager.GetTexture(@"Textures\Dialog");
        }

        public override void Update(GameTime gTime)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _gItemBoxes[i,j].SetItem(InventoryManager.GetItemFromLocation(i, j, _bPlayerInventory));
                }
            }
        }

        /// <summary>
        /// Centers the gui object on the screen, then initializes
        /// the ItemBox array and positions them appropriately.
        /// </summary>
        public void Setup()
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _gItemBoxes[i, j] = new GUIItemBox(i, j, @"Textures\Dialog", null);

                    if (i == 0 && j == 0) { _gItemBoxes[i, j].AnchorToInnerSide(this, SideEnum.TopLeft, GUIManager.STANDARD_MARGIN); }
                    else if (j == 0) { _gItemBoxes[i, j].AnchorAndAlignToObject(_gItemBoxes[i - 1, j], SideEnum.Bottom, SideEnum.Left, GUIManager.STANDARD_MARGIN); }
                    else { _gItemBoxes[i, j].AnchorAndAlignToObject(_gItemBoxes[i, j - 1], SideEnum.Right, SideEnum.Bottom, GUIManager.STANDARD_MARGIN); }

                    AddControl(_gItemBoxes[i, j]);
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
                if (GameManager.CurrentInventoryDisplay == GameManager.DisplayTypeEnum.Gift)
                {
                    rv = true;
                    //Do not pick the item up, instead assign it.
                   GameManager.gmActiveItem = IsItemThere(mouse);
                }
                else if (GameManager.gmDungeonObject != null)
                {
                    Item it = IsItemThere(mouse);
                    if (!GUIManager.IsTextWindowOpen() && Contains(mouse) && it != null)
                    {
                        string text = string.Empty;
                        if (GameManager.gmDungeonObject.CheckForKey(it))
                        {
                            GameManager.gmDungeonObject.Trigger(GameManager.ITEM_OPEN);
                            GUIManager.CloseMainObject();
                            text = DataManager.GetGameText("ItemDoorOpen");
                        }
                        else { text = DataManager.GetGameText("ItemDoorClose"); }

                        GUIManager.OpenTextWindow(text);
                    }
                }
                else
                {
                    rv = true;
                    bool takeHalf = InputManager.IsKeyHeld(Keys.LeftShift) || InputManager.IsKeyHeld(Keys.RightShift);
                    rv = GameManager.GrabItem(TakeItem(mouse, takeHalf));
                }
            }

            //Close any hover windows that may be open, otherwise they'll be open
            //on an empty object
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
                if (i != null && i.Item != null)
                {
                    //If the only inventory we are working with is the player's inventory,
                    //pass the handler to the GUIItemBox ad have it handle the item click
                    if (!InventoryManager.ManagingExtraInventory())
                    {
                        rv = i.ProcessRightButtonClick(mouse);
                    }
                    else  //We are managing an additional Inventory
                    {
                        //Ensure that there is a GUIBox where we clicked, and the box has an Item
                        if (i != null && i.Item != null)
                        {
                            int row = 0;
                            int col = 0;

                            //Use _container != null to get the status of the inverse of whichever we are clicking on
                            if (InventoryManager.HasSpaceInInventory(i.Item.ItemID, i.Item.Number, ref row, ref col, !_bPlayerInventory))
                            {
                                GameManager.GrabItem(TakeItem(mouse));
                                //If the GUI represents a Container, move the Item to the PlayerInventory
                                //else, move the Item to the Container's inventory
                                InventoryManager.AddItemToInventorySpot(i.Item, row, col, !_bPlayerInventory);
                                GameManager.DropItem();

                                //Close any hover windows that may be open,otherwise they'll be open on an empty object
                                GUIManager.CloseHoverWindow();

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

            foreach(GUIItemBox i in _gItemBoxes)
            {
                if (i.ProcessHover(mouse))
                {
                    rv = true;
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
                    if (_gItemBoxes[i, j].Contains(mouse) && _gItemBoxes[i, j].Item != null)
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

            foreach (GUIItemBox box in _gItemBoxes)
            {
                if (box.Contains(mouse) && box.Item != null)
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

            foreach(GUIItemBox box in _gItemBoxes)
            {
                if(box.Contains(mouse) && box.Item != null)
                {
                    rv = box.Item;
                    break;
                }
            }

            return rv;
        }

        public Item TakeItem(Point mouse, bool takeHalf = false)
        {
            Item rv = null;

            foreach (GUIItemBox box in _gItemBoxes)
            {
                if (box.Contains(mouse) && box.Item != null)
                {
                    Item chosenItem = box.Item;
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
                        InventoryManager.RemoveItemFromInventorySpot(box.Row, box.Col, _bPlayerInventory);
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
                            if (_gItemBoxes[i, j].Contains(mouse) && (Force || _gItemBoxes[i, j].Item == null))
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
