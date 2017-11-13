using RiverHollow.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers
{
    static class InventoryManager
    {
        #region Properties
        public static int maxItemColumns = 10;
        public static int maxItemRows = 4;

        private static Item[,] _inventory;
        public static Item[,] Inventory { get => _inventory; }

        private static int _currentInventorySlot = 0;
        public static int CurrentItemNumber { get => _currentInventorySlot; set => _currentInventorySlot = value; }
        //private Item _currentItem;
        public static Item CurrentItem { get => _inventory[0, _currentInventorySlot]; }
        #endregion

        public static void Init()
        {
            _inventory = new Item[maxItemRows, maxItemColumns];
        }

        public static bool HasSpaceInInventory(int itemID)
        {
            bool rv = false;
            if (itemID != -1)
            {
                for (int i = 0; i < maxItemRows; i++)
                {
                    for (int j = 0; j < maxItemColumns; j++)
                    {
                        Item testItem = _inventory[i, j];
                        if (testItem == null)
                        {
                            rv = true;
                            break;
                        }
                        else
                        {
                            if (testItem.ItemID == itemID && testItem.Number < 999)
                            {
                                rv = true;
                                break;
                            }
                        }
                    }
                }
            }

            return rv;
        }

        public static bool HasItemInInventory(int itemID, int x)
        {
            bool rv = false;
            int leftToFind = x;
            if (itemID != -1)
            {
                for (int i = 0; i < maxItemRows; i++)
                {
                    for (int j = 0; j < maxItemColumns; j++)
                    {
                        Item testItem = _inventory[i, j];
                        if (testItem != null && testItem.ItemID == itemID)
                        {
                            leftToFind -= testItem.Number;
                            if (leftToFind <= 0)
                            {
                                rv = true;
                                break;
                            }
                        }
                    }
                    if (rv)
                    {
                        break;
                    }
                }
            }

            return rv;
        }

        public static void RemoveItemsFromInventory(int itemID, int x)
        {
            int leftToRemove = x;
            bool done = false;
            List<int> toRemove = new List<int>();
            for (int i = 0; i < maxItemRows; i++)
            {
                if (done) { break; }
                for (int j = 0; j < maxItemColumns; j++)
                {
                    if (done) { break; }
                    Item testItem = _inventory[i, j];
                    if (testItem != null && testItem.ItemID == itemID)
                    {
                        int temp = testItem.Number;
                        if (testItem.Number >= leftToRemove)
                        {
                            testItem.Number -= leftToRemove;
                            if (testItem.Number == 0)
                            {
                                toRemove.Add((i * maxItemColumns) + j);
                            }
                        }
                        else
                        {
                            testItem.Number = 0;
                            toRemove.Add((i * maxItemColumns) + j);
                            leftToRemove -= temp;
                        }
                    }
                }
            }

            foreach (int i in toRemove)
            {
                RemoveItemFromInventory(i);
            }
        }

        public static void AddItemToFirstAvailableInventorySpot(int itemID)
        {
            if (!IncrementExistingItem(itemID))
            {
                bool added = false;
                for (int i = 0; i < maxItemRows; i++)
                {
                    for (int j = 0; j < maxItemColumns; j++)
                    {
                        if (_inventory[i, j] == null)
                        {
                            _inventory[i, j] = ObjectManager.GetItem(itemID, 1);
                            if (_inventory[i, j].Type == Item.ItemType.Tool)
                            {
                                PlayerManager.CompareTools((Tool)_inventory[i, j]);
                            }
                            added = true;
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

        public static bool IncrementExistingItem(int itemID)
        {
            bool rv = false;
            for (int i = 0; i < maxItemRows; i++)
            {
                for (int j = 0; j < maxItemColumns; j++)
                {
                    if (_inventory[i, j] != null && _inventory[i, j].DoesItStack && _inventory[i, j].ItemID == itemID && _inventory[i, j].Number < 999)
                    {
                        _inventory[i, j].Number++;
                        return true;
                    }
                }
            }
            return rv;
        }

        public static bool AddItemToInventorySpot(Item item, int row, int column)
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
                        PlayerManager.CompareTools((Tool)_inventory[row, column]);
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

        public static void RemoveItemFromInventory(int index)
        {
            for (int i = 0; i < maxItemRows; i++)
            {
                for (int j = 0; j < maxItemColumns; j++)
                {
                    if ((i * maxItemColumns) + j == index)
                    {
                        if (_inventory[i, j].Type == Item.ItemType.Tool)
                        {
                            PlayerManager.CompareTools((Tool)_inventory[i, j]);
                        }
                        _inventory[i, j] = null;
                        break;
                    }
                }
            }
        }

        public static Tool FindTool(Tool.TypeOfTool tool)
        {
            Tool rv = null;
            for (int i = 0; i < maxItemRows; i++)
            {
                for (int j = 0; j < maxItemColumns; j++)
                {
                    if (_inventory[i, j] != null)
                    {
                        if(_inventory[i, j].Type == Item.ItemType.Tool)
                        {
                            Tool t = (Tool)_inventory[i, j];
                            if(t.ToolType == tool)
                            {
                                if(rv != null && t.DmgValue > rv.DmgValue) { rv = t; }
                                else { rv = t; }
                            }
                        }
                    }
                }
            }
            return rv;
        }
    }
}
