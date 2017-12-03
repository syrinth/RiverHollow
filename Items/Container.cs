using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace RiverHollow.Items
{
    public class Container : StaticItem
    {
        private Item[,] _inventory;
        public Item[,] Inventory { get => _inventory; }

        private int _rows;
        public int Rows { get => _rows; }

        private int _columns;
        public int Columns { get => _columns; }

        public Container(int id, string[] itemValue)
        {
            int i = ImportBasics(itemValue, id, 1);
            _rows = int.Parse(itemValue[i++]);
            _columns = int.Parse(itemValue[i++]);
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            _pickup = false;
            _inventory = new Item[InventoryManager.maxItemRows, InventoryManager.maxItemColumns];

            CalculateSourcePos();
        }

        public bool IncrementExistingItem(int itemID)
        {
            bool rv = false;
            for (int i = 0; i < InventoryManager.maxItemRows; i++)
            {
                for (int j = 0; j < InventoryManager.maxItemColumns; j++)
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

        public void AddItemToFirstAvailableInventorySpot(int itemID)
        {
            if (!IncrementExistingItem(itemID))
            {
                bool added = false;
                for (int i = 0; i < _rows; i++)
                {
                    for (int j = 0; j < _columns; j++)
                    {
                        if (_inventory[i, j] == null)
                        {
                            _inventory[i, j] = ObjectManager.GetItem(itemID, 1);
                            added = true; ;
                        }
                        if (added)
                        {
                            break;
                        }
                    }
                    if (added)
                    {
                        break;
                    }
                }
            }
        }

        public bool AddItemToInventorySpot(Item item, int row, int column)
        {
            bool rv = false;
            if (item != null)
            {
                if (_inventory[row, column] == null)
                {
                    if (item.Type == Item.ItemType.Equipment)
                    {
                        _inventory[row, column] = (Equipment)(item);
                    }
                    else if (item.Type == Item.ItemType.Tool)
                    {
                        _inventory[row, column] = (Tool)(item);
                    }
                    else
                    {
                        _inventory[row, column] = item;
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

        public void RemoveItemFromInventory(int i, int j)
        {
            if (_inventory[i, j].Number > 1) {
                _inventory[i, j].Number--;
            }
            else {
                _inventory[i, j] = null;
            }
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            rv += "Holds " + Rows * Columns + " items";

            return rv;
        }
    }
}
