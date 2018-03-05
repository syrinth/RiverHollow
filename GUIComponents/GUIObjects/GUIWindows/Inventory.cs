using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Items;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Characters;

namespace RiverHollow.Screens
{
    public class Inventory : GUIWindow
    {
        protected GUIItemBox[,] _displayList;
        private Container _container;
        public Container Container { get => _container; }
        private NPC _giveTo;

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

        public Inventory(NPC n, int rows, int columns, int edgeSize) : this(rows, columns, edgeSize)
        {
            _giveTo = n;
        }

        public void Setup()
        {
            CenterOnScreen();
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);

            Rectangle displayBox = new Rectangle((int)Position().X + _winData.Edge + _iMargin, (int)Position().Y + _winData.Edge + _iMargin, _iBoxSize, _iBoxSize);
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _displayList[i, j] = new GUIItemBox(displayBox.Location.ToVector2(), new Rectangle(288, 32, 32, 32), displayBox.Width, displayBox.Height, @"Textures\Dialog", null);
                    Controls.Add(_displayList[i, j]);
                    displayBox.X += _iBoxSize + _iMargin;
                }
                displayBox.X = (int)Position().X + _winData.Edge + _iMargin;
                displayBox.Y += _iBoxSize + _iMargin;
            }
        }

        public override bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (GraphicCursor.HeldItem != null)
            {
                Item toSwitch = IsItemThere(mouse);
                if (toSwitch != null)
                {
                    if (GraphicCursor.HeldItem.ItemID == toSwitch.ItemID)
                    {
                        toSwitch.Number++;
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
                else
                {
                    rv = GraphicCursor.GrabItem(TakeItem(mouse));
                    GameManager.BackToMain();
                }
            }
            return rv;
        }

        public virtual bool ProcessRightButtonClick(Point mouse)
        {
            bool rv = false;

            if (GraphicCursor.HeldItem == null)
            {
                Item i = IsItemThere(mouse);
                if (i != null)
                {
                    if (InventoryManager.PublicContainer == null)
                    {
                        if (i.IsFood())
                        {
                            Food f = ((Food)i);
                            GUIManager.AddTextSelection(f, string.Format("Really eat the {0}? [Yes:Eat|No:DoNothing]", f.Name));
                        }
                    }
                    else
                    {
                        if (i != null)
                        {
                            i = TakeItem(mouse);
                            if (_container != null) { InventoryManager.AddItemToInventory(i); }
                            else { InventoryManager.AddItemToInventory(i, InventoryManager.PublicContainer); }
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
                    break;
                }
            }
            return rv;
        }

        private Item IsItemThere(Point mouse)
        {
            Item rv = null;
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_displayList[i, j].Contains(mouse) && _displayList[i, j].Item != null)
                    {
                        rv = _displayList[i, j].Item;
                        goto Exit;
                    }
                }
            }
        Exit:
            return rv;
        }

        public Item TakeItem(Point mouse)
        {
            Item rv = null;

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_displayList[i, j].Contains(mouse) && _displayList[i, j].Item != null)
                    {
                        if (_displayList[i, j].Item.IsEquipment())
                        {
                            rv = ((Equipment)(_displayList[i, j].Item));
                        }
                        else if (_displayList[i, j].Item.IsTool())
                        {
                            rv = ((Tool)(_displayList[i, j].Item));
                        }
                        else
                        {
                            rv = _displayList[i, j].Item;
                        }

                        if (_container == null)
                        {
                            InventoryManager.RemoveItemFromInventoryLocation(i, j);
                        }
                        else
                        {
                            InventoryManager.RemoveItemFromInventoryLocation(i,j, _container);
                        }
                        goto Exit;
                    }
                }
            }

Exit:
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
                        _displayList[i, j].Item = InventoryManager.PlayerInventory[i, j];
                    }
                    else
                    {
                        _displayList[i, j].Item = _container.Inventory[i, j];
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            foreach (GUIItemBox gIB in _displayList)
            {
                gIB.Draw(spriteBatch);
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
