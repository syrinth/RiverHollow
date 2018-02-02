using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Items;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Characters;

namespace RiverHollow.Screens
{
    public class Inventory : GUIWindow
    {
        protected GUIItemBox[,] _displayList;
        private Container _container;
        public Container Container { get => _container; }
        private NPC _giveTo;

        protected const int boxSize = 32;
        protected const int _margin = 3;

        protected int _columns;
        protected int _rows;

        public Inventory(Vector2 center, int rows, int columns, int edgeSize)
        {
            _container = null;
            _edgeSize = GUIWindow.BrownDialogEdge;
            _rows = rows;
            _columns = columns;

            _displayList = new GUIItemBox[rows, columns];
            _width = (_edgeSize * 2) + (_columns * boxSize) + (_margin * (_columns + 1));
            _height = (_edgeSize * 2) + (_rows * boxSize) + (_margin * (_rows + 1));
            SetPosition(new Vector2(center.X - _width / 2, center.Y - _height / 2));

            _sourcePoint = GUIWindow.BrownDialog;
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
        }

        public Inventory(Container c, Vector2 center, int edgeSize): this(center, c.Rows, c.Columns, edgeSize)
        {
            _container = c;
        }

        public Inventory(NPC n, Vector2 center, int rows, int columns, int edgeSize) : this(center, rows, columns, edgeSize)
        {
            _giveTo = n;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
            _drawRect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);

            Rectangle displayBox = new Rectangle((int)Position.X + _edgeSize + _margin, (int)Position.Y + _edgeSize + _margin, boxSize, boxSize);
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _displayList[i, j] = new GUIItemBox(displayBox.Location.ToVector2(), new Rectangle(288, 32, 32, 32), displayBox.Width, displayBox.Height, @"Textures\Dialog", null);
                    displayBox.X += boxSize + _margin;
                }
                displayBox.X = (int)Position.X + _edgeSize + _margin;
                displayBox.Y += boxSize + _margin;
            }
        }

        public virtual bool ProcessLeftButtonClick(Point mouse)
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
                    RiverHollow.BackToMain();
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
                        if (i.Type == Item.ItemType.Food)
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
                        if (_displayList[i, j].Item.Type == Item.ItemType.Equipment)
                        {
                            rv = ((Equipment)(_displayList[i, j].Item));
                        }
                        else if (_displayList[i, j].Item.Type == Item.ItemType.Tool)
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
