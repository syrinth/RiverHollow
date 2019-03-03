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
    public class Inventory : GUIWindow
    {
        protected GUIItemBox[,] _displayList;
        private Container _container;
        public Container Container { get => _container; }
        private Villager _giveTo;
        private KeyDoor _doorToOpen;

        protected const int _iBoxSize = 64;
        protected const int _iMargin = 3;

        protected int _columns;
        protected int _rows;

        public Inventory(int rows, int columns, int edgeSize)
        {
            _container = null;
            _winData = GUIWindow.BrownWin;
            _rows = rows;
            _columns = columns;

            _displayList = new GUIItemBox[rows, columns];
            Width = (_winData.Edge * 2) + (_columns * _iBoxSize) + (_iMargin * (_columns + 1));
            Height = (_winData.Edge * 2) + (_rows * _iBoxSize) + (_iMargin * (_rows + 1));
            Setup();

            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
        }

        public Inventory(Container c, int edgeSize): this(c.Rows, c.Columns, edgeSize)
        {
            _container = c;
        }

        public Inventory(Villager n, int rows, int columns, int edgeSize) : this(rows, columns, edgeSize)
        {
            _giveTo = n;
        }

        public Inventory(KeyDoor door, int rows, int columns, int edgeSize) : this(rows, columns, edgeSize)
        {
            _doorToOpen = door;
        }

        public void Setup()
        {
            CenterOnScreen();
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _displayList[i, j] = new GUIItemBox(new Rectangle(288, 32, 32, 32), _iBoxSize, _iBoxSize, i, j, @"Textures\Dialog", null);

                    if (i == 0 && j == 0) { _displayList[i, j].AnchorToInnerSide(this, SideEnum.TopLeft, _iMargin); }
                    else if (j == 0) { _displayList[i, j].AnchorAndAlignToObject(_displayList[i - 1, j], SideEnum.Bottom, SideEnum.Left, _iMargin); }
                    else { _displayList[i, j].AnchorAndAlignToObject(_displayList[i, j - 1], SideEnum.Right, SideEnum.Bottom, _iMargin); }
                }
            }
        }

        public bool ProcessLeftButtonClick(Point mouse, bool onlyInv)
        {
            bool rv = false;

            if (GraphicCursor.HeldItem != null)
            {
                Item toSwitch = IsItemThere(mouse);
                if (toSwitch != null)
                {
                    if (GraphicCursor.HeldItem.ItemID == toSwitch.ItemID)
                    {
                        toSwitch.Add(1);
                        GraphicCursor.DropItem();
                    }
                    else
                    {
                        Item temp = GraphicCursor.HeldItem;
                        GraphicCursor.GrabItem(TakeItem(mouse));
                        GiveItem(temp, true);
                    }
                }
                else if (GiveItem(GraphicCursor.HeldItem))
                {
                    GraphicCursor.DropItem();
                    rv = true;
                }
            }
            else
            {
                if (_giveTo != null)
                {
                    _giveTo.Gift(IsItemThere(mouse));
                }
                else if (_doorToOpen != null)
                {
                    string text = string.Empty;
                    if (_doorToOpen.Check(IsItemThere(mouse))) { text = GameContentManager.GetGameText("KeyDoorOpen"); }
                    else { text = GameContentManager.GetGameText("KeyDoorClose"); }

                    GUIManager.OpenTextWindow(text);
                }
                else
                {
                    rv = true;
                    bool takeHalf = InputManager.IsKeyHeld(Keys.LeftShift) || InputManager.IsKeyHeld(Keys.RightShift);
                    rv = GraphicCursor.GrabItem(TakeItem(mouse, takeHalf));
                    
                    if (onlyInv)
                    {
                        //GameManager.BackToMain();
                    }
                }
            }
            return rv;
        }

        public override bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if (GraphicCursor.HeldItem == null)
            {
                GUIItemBox i = GetItemBox(mouse);
                if (i != null && i.Item != null)
                {
                    if (InventoryManager.PublicContainer == null)
                    {
                        if (i.Item.IsFood() || i.Item.IsClassItem())
                        {
                            string text = string.Empty;
                            if (i.Item.IsFood())
                            {
                                text = GameContentManager.GetGameText("FoodConfirm");
                            }
                            else if (i.Item.IsClassItem())
                            {
                                text = GameContentManager.GetGameText("ClassItemConfirm");
                            }
                            i.CloseDescription();
                            GUIManager.AddTextSelection(string.Format(text, i.Item.Name));
                            GameManager.gmActiveItem = i.Item;
                        }
                    }
                    else
                    {
                        if (i != null && i.Item != null)
                        {
                            GraphicCursor.GrabItem(TakeItem(mouse));

                            if (InventoryManager.PublicContainer != null)
                            {
                                if (_container != null) { InventoryManager.AddItemToInventory(i.Item); }
                                else { InventoryManager.AddItemToInventory(i.Item, InventoryManager.PublicContainer); }

                                GraphicCursor.DropItem();
                            }
                            rv = true;
                        }
                    }
                }
            }

            return rv;
        }

        public virtual bool ProcessHover(Point mouse)
        {
            bool rv = false;

            foreach(GUIItemBox i in _displayList)
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
                    if (_displayList[i, j].Contains(mouse) && _displayList[i, j].Item != null)
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

            foreach (GUIItemBox box in _displayList)
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

            foreach(GUIItemBox box in _displayList)
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

            foreach (GUIItemBox box in _displayList)
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
                        if (_container == null)
                        {
                            InventoryManager.RemoveItemFromInventoryLocation(box);
                        }
                        else
                        {
                            InventoryManager.RemoveItemFromInventoryLocation(box, _container);
                        }
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
                            if (_displayList[i, j].Contains(mouse) && (Force || _displayList[i, j].Item == null))
                            {
                                if (_container == null)
                                {
                                    rv = InventoryManager.AddItemToInventorySpot(item, i, j);
                                }
                                else{
                                    rv = InventoryManager.AddItemToInventorySpot(item, i, j, _container);
                                }
                                goto Exit;
                            }
                        }
                    }
                }
            }
Exit:

            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_container == null)
                    {
                        _displayList[i, j].SetItem(InventoryManager.PlayerInventory[i, j]);
                    }
                    else
                    {
                        _displayList[i, j].SetItem(_container.Inventory[i, j]);
                    }
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
                    _displayList[i, j].Draw(spriteBatch);//, alpha);
                }
            }

            foreach (GUIItemBox gIB in _displayList)
            {
                if (gIB != null)
                {
                    if (gIB.DrawDescription(spriteBatch))
                    {
                        break;
                    }
                }
            }
        }
    }
}
