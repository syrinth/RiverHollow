using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Adventure.Items;
using Microsoft.Xna.Framework.Input;
using Adventure.Game_Managers;
using Adventure.Game_Managers.GUIObjects;
using Adventure.Game_Managers.GUIComponents.GUIObjects;

namespace Adventure.Screens
{
    public class Inventory : GUIWindow
    {
        protected GUIItemBox[,] _displayList;
        protected Vector2 source = new Vector2(96, 0);

        private Container _container;

        protected const int boxSize = 32;
        protected const int _margin = 3;

        protected int _columns;
        protected int _rows;

        public Inventory(Vector2 center, int rows, int columns, int edgeSize)
        {
            _container = null;
            _edgeSize = edgeSize;
            _rows = rows;
            _columns = columns;

            _displayList = new GUIItemBox[rows, columns];
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
            _width = (_edgeSize * 2) + (_columns * boxSize) + (_margin * (_columns + 1));
            _height = (_edgeSize * 2) + (_rows * boxSize) + (_margin * (_rows + 1));
            SetPosition(new Vector2(center.X - _width / 2, center.Y - _height / 2));

            Load(source, edgeSize);
        }

        public Inventory(Container c, Vector2 center, int edgeSize): this(center, c.Rows, c.Columns, edgeSize)
        {
            _container = c;
        }

        public void SetPosition(Vector2 position)
        {
            _position = position;
            _rect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);

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
                if (IsItemThere(mouse))
                {
                    InventoryItem temp = GraphicCursor.HeldItem;
                    GraphicCursor.GrabItem(TakeItem(mouse));
                    GiveItem(temp, true);
                }
                else if (GiveItem(GraphicCursor.HeldItem)) {
                    GraphicCursor.DropItem();
                    rv = true;
                }
            }
            else
            {
               rv =  GraphicCursor.GrabItem(TakeItem(mouse));
            }
            return rv;
        }

        private bool IsItemThere(Point mouse)
        {
            bool rv = false;
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_displayList[i, j].Rectangle.Contains(mouse) && _displayList[i, j].Item != null)
                    {
                        rv = true;
                        break;
                    }
                }
            }
            return rv;
        }

        public InventoryItem TakeItem(Point mouse)
        {
            InventoryItem rv = null;

            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    if (_displayList[i, j].Rectangle.Contains(mouse) && _displayList[i, j].Item != null)
                    {
                        if (_displayList[i, j].Item.GetType().Equals(typeof(Weapon)))
                        {
                            rv = ((Weapon)(_displayList[i, j].Item));
                        }
                        else if (_displayList[i, j].Item.GetType().Equals(typeof(Tool)))
                        {
                            rv = ((Tool)(_displayList[i, j].Item));
                        }
                        else
                        {
                            rv = new InventoryItem(_displayList[i, j].Item);
                        }
                        if (_container == null)
                        {
                            PlayerManager.Player.RemoveItemFromInventory((i * _columns) + j);
                        }
                        else
                        {
                            _container.RemoveItemFromInventory((i * _columns) + j);
                        }
                        break;
                    }
                }
            }

            return rv;
        }

        private bool GiveItem(InventoryItem item)
        {
            return GiveItem(item, false);
        }

        private bool GiveItem(InventoryItem item, bool Force)
        {
            bool rv = false;
            if (item != null)
            {
                Vector2 mouse = new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);
                if (_rect.Contains(mouse))
                {
                    for (int i = 0; i < _rows; i++)
                    {
                        for (int j = 0; j < _columns; j++)
                        {
                            if (_displayList[i, j].Rectangle.Contains(mouse) && (Force || _displayList[i, j].Item == null))
                            {
                                if (_container == null)
                                {
                                    rv = PlayerManager.Player.AddItemToInventorySpot(item, i, j);
                                }
                                else{
                                    rv = _container.AddItemToInventorySpot(item, i, j);
                                }
                            }
                        }
                    }
                }
            }

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
                        _displayList[i, j].Item = PlayerManager.Player.Inventory[i, j];
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
        }
    }
}
