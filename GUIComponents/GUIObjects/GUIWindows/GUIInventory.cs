using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Actors;
using static RiverHollow.WorldObjects.WorldItem;
using static RiverHollow.WorldObjects.Door;
using RiverHollow.Game_Managers.GUIComponents.Screens;

namespace RiverHollow.Screens
{
    public class GUIInventory : GUIWindow
    {
        protected GUIItemBox[,] _gItemBoxes;

        protected const int _iBoxSize = 64;
        protected const int _iMargin = 3;

        protected int _columns;
        protected int _rows;

        bool _bPlayerInventory;

        public GUIInventory(bool PlayerInventory = false)
        {
            _winData = GUIWindow.BrownWin;
            _bPlayerInventory = PlayerInventory;

            //Retrieve the dimensions of the Inventory we're workingo n frmo the InventoryManager
            InventoryManager.GetDimensions(ref _rows, ref _columns, _bPlayerInventory);

            _gItemBoxes = new GUIItemBox[_rows, _columns];
            Width = (_winData.Edge * 2) + (_columns * _iBoxSize) + (_iMargin * (_columns + 1));
            Height = (_winData.Edge * 2) + (_rows * _iBoxSize) + (_iMargin * (_rows + 1));
            Setup();

            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _gItemBoxes[i,j].SetItem(InventoryManager.GetItemFromLocation(i, j, _bPlayerInventory));
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    //float alpha = 1.0f;// _vSelectedItem == new Vector2(i, j) ? 0.5f : 1;
                    _gItemBoxes[i, j].Draw(spriteBatch);//, alpha);
                }
            }
        }

        /// <summary>
        /// Centers the gui object on the screen, then initializes
        /// the ItemBox array and positions them appropriately.
        /// </summary>
        public void Setup()
        {
            CenterOnScreen();

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _gItemBoxes[i, j] = new GUIItemBox(new Rectangle(288, 32, 32, 32), _iBoxSize, _iBoxSize, i, j, @"Textures\Dialog", null);

                    if (i == 0 && j == 0) { _gItemBoxes[i, j].AnchorToInnerSide(this, SideEnum.TopLeft, _iMargin); }
                    else if (j == 0) { _gItemBoxes[i, j].AnchorAndAlignToObject(_gItemBoxes[i - 1, j], SideEnum.Bottom, SideEnum.Left, _iMargin); }
                    else { _gItemBoxes[i, j].AnchorAndAlignToObject(_gItemBoxes[i, j - 1], SideEnum.Right, SideEnum.Bottom, _iMargin); }
                }
            }
        }

        public bool ProcessLeftButtonClick(Point mouse, bool onlyInv)
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
                if (GameManager.CurrentNPC != null)
                {
                    //CurrentNPC.Gift(IsItemThere(mouse));
                }
                else if (GameManager.CurrentDoor != null)
                {
                    string text = string.Empty;
                    if (GameManager.CurrentDoor.Check(IsItemThere(mouse))) { text = GameContentManager.GetGameText("KeyDoorOpen"); }
                    else { text = GameContentManager.GetGameText("KeyDoorClose"); }

                    GUIManager.OpenTextWindow(text);
                }
                else
                {
                    rv = true;
                    bool takeHalf = InputManager.IsKeyHeld(Keys.LeftShift) || InputManager.IsKeyHeld(Keys.RightShift);
                    rv = GameManager.GrabItem(TakeItem(mouse, takeHalf));
                    
                    if (onlyInv)
                    {
                        //GameManager.BackToMain();
                    }
                }
            }

            //Close any hover windows that may be open,otherwise they'll be open
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

            return rv;
        }

        /// <summary>
        /// Triggers a ProcessHover for each GUIItemBox
        /// </summary>
        /// <param name="mouse"></param>
        /// <returns></returns>
        public virtual bool ProcessHover(Point mouse)
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
                        rv = ObjectManager.GetItem(chosenItem.ItemID, num);
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
