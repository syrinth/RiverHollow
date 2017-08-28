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

        public Container(int id, string[] itemValue)
        {
            if (itemValue.Length == 7)
            {
                _num = 1;

                int i = 1;
                _itemType = ItemType.Resource;
                _name = itemValue[i++];
                _description = itemValue[i++];
                _textureIndex = int.Parse(itemValue[i++]);
                i++; //Holding out for Enum
                _rows = int.Parse(itemValue[i++]);
                _columns = int.Parse(itemValue[i++]);
                _itemID = id; //(ObjectManager.ItemIDs)Enum.Parse(typeof(ObjectManager.ItemIDs), itemValue[i++]);
                _texture = GameContentManager.GetTexture(@"Textures\chest");

                _pickup = false;
                _inventory = new InventoryItem[Player.maxItemRows, Player.maxItemColumns];

                CalculateSourcePos();
            }
        }

        public bool IncrementExistingItem(int itemID)
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
