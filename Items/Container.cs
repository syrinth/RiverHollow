using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace Adventure.Items
{
    public class Container : StaticItem
    {
        private InventoryItem[,] _inventory;
        public InventoryItem[,] Inventory { get => _inventory; }

        private int _rows;
        public int Rows { get => _rows; }

        private int _columns;
        public int Columns { get => _columns; }

        public Container(ObjectManager.ItemIDs ID, Vector2 sourcePos, Texture2D texture, string name, string description, int rows, int columns, List<KeyValuePair<ObjectManager.ItemIDs, int>> reagents) : base(ID, sourcePos, texture, name, description, reagents)
        {
            _pickup = false;
            _inventory = new InventoryItem[Player.maxItemRows, Player.maxItemColumns];
            _rows = rows;
            _columns = columns;
        }

        public bool IncrementExistingItem(ObjectManager.ItemIDs itemID)
        {
            bool rv = false;
            for (int i = 0; i < Player.maxItemRows; i++)
            {
                for (int j = 0; j < Player.maxItemColumns; j++)
                {
                    if (_inventory[i, j] != null && _inventory[i, j].ItemID == itemID && _inventory[i, j].Number < 999)
                    {
                        _inventory[i, j].Number++;
                        return true;
                    }
                }
            }
            return rv;
        }

        public bool AddItemToInventorySpot(InventoryItem item, int row, int column)
        {
            bool rv = false;
            if (item != null)
            {
                if (_inventory[row, column] == null)
                {
                    if (item.GetType().Equals(typeof(Weapon)))
                    {
                        _inventory[row, column] = (Weapon)(item);
                    }
                    else if (item.GetType().Equals(typeof(Tool)))
                    {
                        _inventory[row, column] = (Tool)(item);
                    }
                    else
                    {
                        _inventory[row, column] = new InventoryItem(item);
                    }
                    rv = true;
                }
                else
                {
                    if (_inventory[row, column].ItemID == item.ItemID && _inventory[row, column].DoesItStack && 999 >= (_inventory[row, column].Number + item.Number))
                    {
                        _inventory[row, column].Number += item.Number;
                        rv = true;
                    }
                }
            }
            return rv;
        }

        public void RemoveItemFromInventory(int index)
        {
            for (int i = 0; i < Player.maxItemRows; i++)
            {
                for (int j = 0; j < Player.maxItemColumns; j++)
                {
                    if ((i * Player.maxItemColumns) + j == index)
                    {
                        _inventory[i, j] = null;
                        break;
                    }
                }
            }
        }
    }
}
