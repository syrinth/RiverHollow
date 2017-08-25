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

        protected const int boxSize = 32;
        protected const int margin = 3;

        protected int _columns;
        protected int _rows;

        public Inventory(Vector2 center, int columns, int rows, int edgeSize)
        {
            _displayList = new GUIItemBox[rows, columns];
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
            _width = (edgeSize * 2) + (columns * boxSize) + (margin * (columns + 1));
            _height = (edgeSize * 2) + (rows * boxSize) + (margin * (rows + 1));

            _position = new Vector2(center.X - _width / 2, center.Y - _height / 2);
            _rect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height);
            _columns = columns;
            _rows = rows;

            Load(source, edgeSize);

            Rectangle displayBox = new Rectangle((int)Position.X + edgeSize + margin, (int)Position.Y + edgeSize + margin, boxSize, boxSize);
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _columns; j++)
                {
                    _displayList[i, j] = new GUIItemBox(displayBox.Location.ToVector2(), new Rectangle(288, 32, 32, 32), displayBox.Width, displayBox.Height, @"Textures\Dialog", null);
                    displayBox.X += boxSize + margin;
                }
                displayBox.X = (int)Position.X + edgeSize + margin;
                displayBox.Y += boxSize + margin;
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

        public bool IsItemThere(Point mouse)
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
                        PlayerManager.Player.RemoveItemFromInventory((i*_columns)+j); //Wrong
                        break;
                    }
                }
            }

            return rv;
        }

        public bool GiveItem(InventoryItem item)
        {
            return GiveItem(item, false);
        }

        public bool GiveItem(InventoryItem item, bool Force)
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
                                rv = PlayerManager.Player.AddItemToInventorySpot(item, i, j);
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
                    _displayList[i,j].Item = PlayerManager.Player.Inventory[i, j];
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
