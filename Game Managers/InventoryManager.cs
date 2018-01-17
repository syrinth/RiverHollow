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

        private static Container _container;
        public static Container PublicContainer { get => _container; set => _container = value; }
        private static Item[,] _playerInventory;
        public static Item[,] PlayerInventory { get => _playerInventory; }

        private static int _currentInventorySlot = 0;
        public static int CurrentItemNumber { get => _currentInventorySlot; set => _currentInventorySlot = value; }
        //private Item _currentItem;
        public static Item CurrentItem { get => _playerInventory[0, _currentInventorySlot]; }
        #endregion

        public static void Init()
        {
            _playerInventory = new Item[maxItemRows, maxItemColumns];
        }
        public static void CheckOperation(Container c, ref Item[,] inventory)
        {
            if (c == null) { inventory = _playerInventory; }
            else { inventory = c.Inventory; }
        }
        public static void CheckOperation(Container c, ref int rows, ref int columns, ref Item[,] inventory)
        {
            if (c == null)
            {
                rows = maxItemRows;
                columns = maxItemColumns;

            }
            else
            {
                rows = c.Rows;
                columns = c.Columns;
            }
            CheckOperation(c, ref inventory);
        }

        public static bool HasSpaceInInventory(int itemID)
        {
            return HasSpaceInInventory(itemID, null);
        }
        public static bool HasSpaceInInventory(int itemID, Container c)
        {
            int maxRows = 0;
            int maxColumns = 0;
            Item[,] inventory = null;
            CheckOperation(c, ref maxRows, ref maxColumns, ref inventory);

            bool rv = false;
            if (itemID != -1)
            {
                for (int i = 0; i < maxRows; i++)
                {
                    for (int j = 0; j < maxColumns; j++)
                    {
                        Item testItem = inventory[i, j];
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
            return HasItemInInventory(itemID, x, null);
        }
        public static bool HasItemInInventory(int itemID, int x, Container c)
        {
            bool rv = false;
            int maxRows = 0;
            int maxColumns = 0;
            Item[,] inventory = null;
            CheckOperation(c, ref maxRows, ref maxColumns, ref inventory);

            int leftToFind = x;
            if (itemID != -1)
            {
                for (int i = 0; i < maxRows; i++)
                {
                    for (int j = 0; j < maxColumns; j++)
                    {
                        Item testItem = inventory[i, j];
                        if (testItem != null && testItem.ItemID == itemID)
                        {
                            leftToFind -= testItem.Number;
                            if (leftToFind <= 0)
                            {
                                rv = true;
                                goto Exit;
                            }
                        }
                    }
                }
            }
Exit:
            return rv;
        }

        public static void RemoveItemsFromInventory(int itemID, int x)
        {
            RemoveItemsFromInventory(itemID, x, null);
        }
        public static void RemoveItemsFromInventory(int itemID, int x, Container c)
        {
            int leftToRemove = x;
            bool done = false;
            int maxRows = 0;
            int maxColumns = 0;
            Item[,] inventory = null;
            CheckOperation(c, ref maxRows, ref maxColumns, ref inventory);

            List<Item> toRemove = new List<Item>();
            for (int i = 0; i < maxRows; i++)
            {
                if (done) { break; }
                for (int j = 0; j < maxColumns; j++)
                {
                    if (done) { break; }
                    Item testItem = inventory[i, j];
                    if (testItem != null && testItem.ItemID == itemID)
                    {
                        int temp = testItem.Number;
                        if (testItem.Number >= leftToRemove)
                        {
                            testItem.Number -= leftToRemove;
                            if (testItem.Number == 0)
                            {
                                toRemove.Add(inventory[i, j]);
                            }
                        }
                        else
                        {
                            testItem.Number = 0;
                            toRemove.Add(inventory[i, j]);
                            leftToRemove -= temp;
                        }
                    }
                }
            }

            foreach (Item i in toRemove)
            {
                RemoveItemFromInventory(i);
            }
        }

        public static void AddNewItemToInventory(int itemToAdd)
        {
            AddItemToInventory(ObjectManager.GetItem(itemToAdd), null);
        }
        public static void AddNewItemToInventory(int itemToAdd, Container c)
        {
            AddItemToInventory(ObjectManager.GetItem(itemToAdd), c);
        }
        public static void AddItemToInventory(Item itemToAdd)
        {
            AddItemToInventory(itemToAdd, null);
        }
        public static void AddItemToInventory(Item itemToAdd, Container c)
        {
            int maxRows = 0;
            int maxColumns = 0;
            Item[,] inventory = null;
            CheckOperation(c, ref maxRows, ref maxColumns, ref inventory);

            if (!IncrementExistingItem(itemToAdd, c))
            {
                for (int i = 0; i < maxRows; i++)
                {
                    for (int j = 0; j < maxColumns; j++)
                    {
                        if (inventory[i, j] == null)
                        {
                            inventory[i, j] = itemToAdd;
                            //Only perform this check if we are adding to the playerInventory
                            if (inventory == _playerInventory && _playerInventory[i, j].Type == Item.ItemType.Tool)
                            {
                                PlayerManager.CompareTools((Tool)_playerInventory[i, j]);
                            }
                            goto Exit;
                        }
                    }
                }
            }
Exit:
            return;
        }

        public static bool IncrementExistingItem(Item itemToAdd, Container c)
        {
            bool rv = false;
            int maxRows = 0;
            int maxColumns = 0;
            Item[,] inventory = null;
            CheckOperation(c, ref maxRows, ref maxColumns, ref inventory);

            for (int i = 0; i < maxRows; i++)
            {
                for (int j = 0; j < maxColumns; j++)
                {
                    if (inventory[i, j] != null && inventory[i, j].DoesItStack && inventory[i, j].ItemID == itemToAdd.ItemID && inventory[i, j].Number < 999)
                    {
                        inventory[i, j].Number += itemToAdd.Number;
                        rv = true;

                        goto Exit;
                    }
                }
            }
Exit:
            return rv;
        }

        public static bool AddItemToInventorySpot(Item item, int row, int column)
        {
            return AddItemToInventorySpot(item, row, column, null);
        }
        public static bool AddItemToInventorySpot(Item item, int row, int column, Container c)
        {
            bool rv = false;
            Item[,] inventory = null;
            CheckOperation(c, ref inventory);

            if (item != null)
            {
                if (inventory[row, column] == null)
                {
                    if (item.Type == Item.ItemType.Equipment)
                    {
                        inventory[row, column] = (Equipment)(item);
                    }
                    else if (item.Type == Item.ItemType.Tool)
                    {
                        _playerInventory[row, column] = (Tool)(item);
                        if (inventory == _playerInventory) { PlayerManager.CompareTools((Tool)_playerInventory[row, column]); }
                    }
                    else
                    {
                        inventory[row, column] = item;
                    }
                    rv = true;
                }
                else
                {
                    if (inventory[row, column].ItemID == item.ItemID && inventory[row, column].DoesItStack && 999 >= (inventory[row, column].Number + item.Number))
                    {
                        inventory[row, column].Number += item.Number;
                        rv = true;
                    }
                }
            }
            return rv;
        }

        public static void RemoveItemFromInventory(int i, int j)
        {
            RemoveItemFromInventory(i, j, null);
        }
        public static void RemoveItemFromInventory(int i, int j, Container c)
        {
            Item[,] inventory = null;
            CheckOperation(c, ref inventory);
            if (inventory == _playerInventory && inventory[i, j].Type == Item.ItemType.Tool)
            {
                PlayerManager.CompareTools((Tool)_playerInventory[i, j]);
            }

            inventory[i, j] = null;
        }

        public static void RemoveItemFromInventory(Item it)
        {
            RemoveItemFromInventory(it, null);
        }
        public static void RemoveItemFromInventory(Item it, Container c)
        {
            int maxRows = 0;
            int maxColumns = 0;
            Item[,] inventory = null;
            CheckOperation(c, ref maxRows, ref maxColumns, ref inventory);
            for (int i = 0; i < maxRows; i++)
            {
                for (int j = 0; j < maxColumns; j++)
                {
                    if (inventory[i, j] == it)
                    {
                        if (inventory == _playerInventory && _playerInventory[i, j].Type == Item.ItemType.Tool)
                        {
                            PlayerManager.CompareTools((Tool)_playerInventory[i, j]);
                        }
                        inventory[i, j] = null;
                        goto Exit;
                    }
                }
            }
        Exit:

            return;
        }

        public static Tool FindTool(Tool.TypeOfTool tool)
        {
            Tool rv = null;
            for (int i = 0; i < maxItemRows; i++)
            {
                for (int j = 0; j < maxItemColumns; j++)
                {
                    if (_playerInventory[i, j] != null)
                    {
                        if(_playerInventory[i, j].Type == Item.ItemType.Tool)
                        {
                            Tool t = (Tool)_playerInventory[i, j];
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
