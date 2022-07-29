using System.Collections.Generic;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    static class InventoryManager
    {
        #region Properties
        public static bool LockedInventory = false;
        public static int maxItemRows = 4;
        public static int maxItemColumns = 10;
        public static Item[,] PlayerInventory { get; private set; }
        public static Item[,] ExtraInventory { get; private set; }

        public static List<Item> AddedItemList;
        #endregion

        /// <summary>
        /// Called on initial game loading. Readies the players inventory
        /// to hold items.
        /// </summary>
        public static void InitPlayerInventory()
        {
            AddedItemList = new List<Item>();

            Item[,] temp = PlayerInventory;
            PlayerInventory = new Item[PlayerManager.BackpackLevel, maxItemColumns];

            if(temp != null)
            {
                for (int x = 0; x < PlayerManager.BackpackLevel && x < temp.GetLength(0); x++)
                {
                    for (int y = 0; y < maxItemColumns; y++)
                    {
                        PlayerInventory[x, y] = temp[x, y];
                    }
                }
            }
        }

        public static void InitExtraInventory(Item[,] inventory)
        {
            ExtraInventory = inventory;
        }

        public static void ClearExtraInventory()
        {
            ExtraInventory = null;
        }

        #region Helpers
        /// <summary>
        /// Determines which Inventory we're working on based off the boolean
        /// </summary>
        private static Item[,] GetInventory(bool PlayerInventory)
        {
            return (PlayerInventory ? InventoryManager.PlayerInventory : ExtraInventory);
        }

        /// <summary>
        /// Gets the deimensions of the Inventory
        /// </summary>
        public static void GetDimensions(ref int rows, ref int cols, bool PlayerInventory)
        {
            GetDimensions(GetInventory(PlayerInventory), ref rows, ref cols);
        }
        private static void GetDimensions(Item[,] inventory, ref int rows, ref int cols)
        {
            rows = inventory.GetLength(0);
            cols = inventory.GetLength(1);
        }
        #endregion

        #region Has Space in Inventory
        /// <summary>
        /// Helper for HasPSaceInInventory
        /// </summary>
        public static bool HasSpaceInInventory(int itemID, int num, bool playerInventory = true)
        {
            int validRow = 0;
            int validCol = 0;
            return HasSpaceInInventory(itemID, num, GetInventory(playerInventory), ref validRow, ref validCol);
        }

        public static bool HasSpaceInInventory(int itemID, int num, ref int validRow, ref int validCol, bool playerInventory = true)
        {
            return HasSpaceInInventory(itemID, num, GetInventory(playerInventory), ref validRow, ref validCol);
        }

        /// <summary>
        /// Call to see if the indicated item and number can fit in the Inventory
        /// </summary>
        /// <param name="itemID">ID of the item to try to add</param>
        /// <param name="num">Number of items</param>
        /// <param name="validRow">Row # of the first valid item space</param>
        /// <param name="validCol">Col # of the first valid item space</param>
        /// <param name="inventory">Inventory to test</param>
        /// <returns></returns>
        private static bool HasSpaceInInventory(int itemID, int num, Item[,] inventory, ref int validRow, ref int validCol)
        {
            bool rv = false;
            int maxRows = 0;
            int maxColumns = 0;
            GetDimensions(inventory, ref maxRows, ref maxColumns);

            //Confirm a valid itemId range
            if (itemID != -1)
            {

                //Iterate through the Inventory
                for (int i = 0; i < maxRows; i++)
                {
                    for (int j = 0; j < maxColumns; j++)
                    {
                        //Retrieve any item that exists in the indicated spot
                        Item testItem = inventory[i, j];
                        if (testItem == null && !rv)   //If no item exists, and we have found no previous matching node, then we are clear to place it here
                        {
                            rv = true;
                            validRow = i;
                            validCol = j;
                            goto Exit;
                        }
                        else if(testItem != null)
                        {
                            //If there is an item there, check to see if it has the same ID, can stack,
                            //and if the number we want to add is less than the max item stack. Once we find a matching stack
                            //we need to exit, we are done.
                            if (testItem.ItemID == itemID && testItem.DoesItStack && testItem.Number + num < 999)
                            {
                                rv = true;
                                validRow = i;
                                validCol = j;
                                goto Exit;
                            }
                        }
                    }
                }
            }
            Exit:

            return rv;
        }
        #endregion

        /// <summary>
        /// Helper to redirect to the Players Inventory
        /// </summary>
        public static bool HasItemInPlayerInventory(int itemID, int x)
        {
            return HasItemInInventory(itemID, x, PlayerInventory);
        }

        /// <summary>
        /// Iterates through the given Inventory, looking for an item
        /// of the given type and number
        /// </summary>
        /// <param name="itemID">Type of item to look for</param>
        /// <param name="number">The number to find</param>
        /// <param name="inventory">Inventory to search</param>
        /// <returns></returns>
        public static bool HasItemInInventory(int itemID, int number, Item [,] inventory)
        {
            bool rv = false;

            int maxRows = 0;
            int maxColumns = 0;
            GetDimensions(inventory, ref maxRows, ref maxColumns);

            int leftToFind = number;
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

        #region Remove items from inventory
        /// <summary>
        /// Helper for RemoveItemsFromInventory
        /// </summary>
        public static void RemoveItemsFromInventory(int itemID, int number, bool playerInventory = true)
        {
            RemoveItemsFromInventory(itemID, number, PlayerInventory);
        }

        /// <summary>
        /// Finds and removes the specified number of the given item from the given inventory
        /// </summary>
        /// <param name="itemID">The item ID to match</param>
        /// <param name="number">The number of items to remove</param>
        /// <param name="inventory">The inventory to act on</param>
        private static void RemoveItemsFromInventory(int itemID, int number, Item[,] inventory)
        {
            int leftToRemove = number;
            bool done = false;

            int maxRows = 0;
            int maxColumns = 0;
            GetDimensions(inventory, ref maxRows, ref maxColumns);

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
                            testItem.Remove(leftToRemove);
                            if (testItem.Number == 0)
                            {
                                toRemove.Add(inventory[i, j]);
                            }
                        }
                        else
                        {
                            testItem.Remove(testItem.Number);
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
        #endregion
        public static void DropItemOnMap(Item item)
        {
            item.AutoPickup = false;
            item.ManualPickup = true;
            MapManager.CurrentMap.DropItemOnMap(item, PlayerManager.PlayerActor.Position);
        }

        #region Add Item to Inventory
        /// <summary>
        /// Creates a new item wth the indicated number and adds it to the indicated inventory
        /// </summary>
        /// <param name="itemToAdd">ID of the item to create</param>
        /// <param name="num">Number of items</param>
        /// <param name="playerInventory">Bool representing whether or not to act on the players Inventory</param>
        /// <returns>True if successful</returns>
        public static bool AddToInventory(int itemToAdd, int num = 1, bool playerInventory = true, bool noDisplay = false)
        {
            return AddToInventory(DataManager.GetItem(itemToAdd, num), playerInventory, noDisplay);
        }

        /// <summary>
        /// Adds the given item to the indicated inventory
        /// </summary>
        /// <param name="itemToAdd">The Item object to add</param>
        /// <param name="playerInventory">Bool representing whether or not to act on the players Inventory</param>
        /// <returns>True if successful</returns>
        public static bool AddToInventory(Item itemToAdd, bool playerInventory = true, bool noDisplay = false)
        {
            return AddToInventory(itemToAdd, GetInventory(playerInventory), playerInventory, noDisplay);
        }

        /// <summary>
        /// Adds the givenItem object to the given Inventory
        /// </summary>
        /// <param name="itemToAdd">The Item object to add</param>
        /// <param name="inventory">The Inventory to act on</param>
        /// <returns></returns>
        private static bool AddToInventory(Item itemToAdd, Item[,] inventory, bool playerInventory = true, bool noDisplay = false)
        {
            bool rv = false;

            if(itemToAdd == null)
            {
                return false;
            }

            if (playerInventory && itemToAdd.AddToInventoryTrigger())
            {
                return true;
            }

            int validRow = -1;
            int validCol = -1;
            //First,confirm that the inventory has space to proceed
            if (HasSpaceInInventory(itemToAdd.ItemID, itemToAdd.Number, inventory, ref validRow, ref validCol))
            {
                //If there is no item there, assign it, otherwise increment it.
                if(inventory[validRow, validCol] == null){
                    inventory[validRow, validCol] = itemToAdd;
                }
                else
                { 
                    //If we are trying to add more items than the stack can hold, we need to
                    //break it up, remove the added numbers, and then attempt to add what's
                    //left to the indicated inventory.
                    if (inventory[validRow, validCol].Number + itemToAdd.Number > 999)
                    {
                        int numToAdd = 999 - inventory[validRow, validCol].Number;
                        int leftOver = itemToAdd.Number - numToAdd;

                        inventory[validRow, validCol].Add(numToAdd, inventory == PlayerInventory);
                        itemToAdd.Remove(numToAdd);
                        AddToInventory(itemToAdd, inventory);
                    }
                    else
                    {
                        inventory[validRow, validCol].Add(itemToAdd.Number, inventory == PlayerInventory);
                    }
                }

                //Only perform this check if we are adding to the playerInventory
                if (inventory == PlayerInventory)
                {
                    TaskManager.AdvanceTaskProgress(itemToAdd);

                    if (!noDisplay)
                    {
                        //Used to display an item that was just added to the inventory
                        AddedItemList.Add(new Item(itemToAdd));
                    }
                }

                rv = true;

            }

            //If we failed to add it, then we need to drop it onto the world map.
            if (!rv && !itemToAdd.ManualPickup)
            {
                DropItemOnMap(itemToAdd);
            }
            return rv;
        }
        #endregion

        #region Add item to Inventory Spot
        /// <summary>
        /// Helper to redirect to the right inventory
        /// </summary>
        public static bool AddItemToInventorySpot(Item item, int row, int column, bool playerInventory = true)
        {
            return AddItemToInventorySpot(item, row, column, GetInventory(playerInventory), playerInventory);
        }
        /// <summary>
        /// Adds an item to a specified inventory location
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <param name="row">The row to add it to</param>
        /// <param name="column">The column to add it to</param>
        /// <param name="inventory">Which inventory to add the item to</param>
        /// <returns></returns>
        private static bool AddItemToInventorySpot(Item item, int row, int column, Item[,] inventory, bool playerInventory = true)
        {
            bool rv = false;

            //Ensure that the item is not null, we do not place null =.
            if (item != null)
            {
                if (playerInventory && item.AddToInventoryTrigger())
                {
                    return true;
                }

                if (inventory[row, column] == null)
                {
                    if (item.CompareType(ItemEnum.Equipment))
                    {
                        inventory[row, column] = (Equipment)(item);
                    }
                    else if (item.CompareType(ItemEnum.Tool))
                    {
                        inventory[row, column] = (Tool)(item);
                    }
                    else
                    {
                        inventory[row, column] = item;
                    }
                    if (inventory == PlayerInventory)
                    {
                        TaskManager.AdvanceTaskProgress(item);
                    }
                    rv = true;
                }
                else
                {
                    if (inventory[row, column].ItemID == item.ItemID && inventory[row, column].DoesItStack && 999 >= (inventory[row, column].Number + item.Number))
                    {
                        inventory[row, column].Add(item.Number, inventory == PlayerInventory);
                        rv = true;
                    }
                }
            }
            return rv;
        }
        #endregion

        #region Remove Item from Inventory Spot
        /// <summary>
        /// These methods remove an item completely from the Inventory location
        /// </summary>
        /// <param name="row">The row of the Item to remove</param>
        /// <param name="column">The col of the Item to remove</param>
        public static void RemoveItemFromInventorySpot(int row, int column, bool playerInventory)
        {
            RemoveItemFromPlayerInventorySpot(row, column, GetInventory(playerInventory));
        }
        private static void RemoveItemFromPlayerInventorySpot(int row, int column, Item[,] inventory)
        {
            if (inventory == PlayerInventory)
            {
                TaskManager.RemoveTaskProgress(inventory[row, column]);
            }

            inventory[row, column] = null;
        }
        #endregion

        #region Remove Item from Inventory
        /// <summary>
        /// Helper to redirect method to the players Inventory
        /// </summary>
        /// <param name="it">The Item object to remove</param>
        public static void RemoveItemFromInventory(Item it, bool playerInventory = true)
        {
            RemoveItemFromInventory(it, GetInventory(playerInventory));
        }

        /// <summary>
        /// Removes the given item object from the inventory
        /// </summary>
        /// <param name="it">The item object to remove</param>
        /// <param name="inventory">The inventory to search</param>
        public static void RemoveItemFromInventory(Item it, Item[,] inventory)
        {
            //Retrieve the max rows and columns from the inventory
            int maxRows = 0;
            int maxColumns = 0;
            GetDimensions(inventory, ref maxRows, ref maxColumns);

            //iterate the the Inventory
            for (int i = 0; i < maxRows; i++)
            {
                for (int j = 0; j < maxColumns; j++)
                {
                    //if the inventoryitem is the exact object we are looking for
                    if (inventory[i, j] == it)
                    {
                        //Do the customary comparisons for updating information
                        if (inventory == PlayerInventory)
                        {
                            if (PlayerInventory[i, j] != null)
                            {
                                TaskManager.RemoveTaskProgress(inventory[i, j]);
                            }
                        }
                        //null the item and exit
                        inventory[i, j] = null;
                        goto Exit;
                    }
                }
            }
        Exit:

            return;
        }
        #endregion

        public static Item GetCurrentItem()
        {
            return PlayerInventory[GameManager.HUDItemRow, GameManager.HUDItemCol];
        }

        internal static List<Consumable> GetConsumables()
        {
            List<Consumable> items = new List<Consumable>();
            for (int i = 0; i < PlayerManager.BackpackLevel; i++)
            {
                for (int j = 0; j < maxItemColumns; j++)
                {
                    if (PlayerInventory[i, j] != null && PlayerInventory[i, j].CompareType(ItemEnum.Consumable))
                    {
                        items.Add((Consumable)PlayerInventory[i, j]);
                    }
                }
            }
            return items;
        }

        internal static bool ManagingExtraInventory()
        {
            return !LockedInventory && ExtraInventory != null;
        }

        public static Item GetItemFromLocation(int row, int column, bool PlayerInventory = true)
        {
            return GetInventory(PlayerInventory)[row, column];
        }

        /// <summary>
        /// Runs through the given list and checks that the player has the required items.
        /// </summary>
        /// <param name="requiredItems">The list of required items and the numbers of each item</param>
        /// <returns></returns>
        public static bool HasSufficientItems(Dictionary<int, int> requiredItems)
        {
            bool rv = true;
            foreach (KeyValuePair<int, int> kvp in requiredItems)
            {
                if (!HasItemInPlayerInventory(kvp.Key, kvp.Value))
                {
                    rv = false;
                    break;
                }
            }

            return rv;
        }
    }
}
