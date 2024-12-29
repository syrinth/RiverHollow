using System.Collections.Generic;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    static class InventoryManager
    {
        #region Properties
        public static DisplayTypeEnum CurrentInventoryDisplay;
        public static ItemTypeEnum HoldItemType;

        public static bool LockedInventory = false;
        public static int maxItemRows = 4;
        public static int maxItemColumns = 10;
        public static bool ExtraHoldSingular = false;
        public static Item[,] PlayerInventory { get; private set; }
        public static Item[,] ExtraInventory { get; private set; }

        public static List<Item> AddedItemList;
        public static List<int> ValidIDs;

        #endregion

        /// <summary>
        /// Called on initial game loading. Readies the players inventory
        /// to hold items.
        /// </summary>
        public static void InitPlayerInventory(bool increaseSize = true)
        {
            AddedItemList = new List<Item>();

            Item[,] temp = PlayerInventory;
            PlayerInventory = new Item[PlayerManager.BackpackLevel, maxItemColumns];

            if(increaseSize && temp != null)
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

        public static void SetHoldItem(ItemTypeEnum itemType)
        {
            HoldItemType = itemType;
        }
        public static void ClearExtraInventory()
        {
            ExtraInventory = null;
            HoldItemType = ItemTypeEnum.None;
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
                        }
                        else if (testItem != null)
                        {
                            //If there is an item there, check to see if it has the same ID, can stack,
                            //and if the number we want to add is less than the max item stack. Once we find a matching stack
                            //we need to exit, we are done.
                            if (testItem.ID == itemID && testItem.Stacks() && testItem.Number + num < Constants.MAX_STACK_SIZE)
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
        public static int GetNumberInPlayerInventory(int itemID)
        {
            return GetNumberInInventory(itemID, PlayerInventory);
        }

        public static int GetNumberInInventory(int itemID, Item[,] inventory)
        {
            int rv = 0;

            int maxRows = 0;
            int maxColumns = 0;
            GetDimensions(inventory, ref maxRows, ref maxColumns);

            if (itemID != -1)
            {
                for (int i = 0; i < maxRows; i++)
                {
                    for (int j = 0; j < maxColumns; j++)
                    {
                        Item testItem = inventory[i, j];
                        if (testItem != null && testItem.ID == itemID)
                        {
                            rv += testItem.Number;
                        }
                    }
                }
            }

            return rv;
        }

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
                        if (testItem != null && testItem.ID == itemID)
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
        public static void RemoveItemsFromInventory(int itemID, int number, Container c = null)
        {
            RemoveItemsFromInventory(itemID, number, c == null ? PlayerInventory : c.Inventory);
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

            int maxRows = 0;
            int maxColumns = 0;
            GetDimensions(inventory, ref maxRows, ref maxColumns);

            List<Item> toRemove = new List<Item>();
            for (int i = 0; i < maxRows; i++)
            {
                if (leftToRemove == 0) { break; }
                for (int j = 0; j < maxColumns; j++)
                {
                    if (leftToRemove == 0) { break; }
                    Item testItem = inventory[i, j];
                    if (testItem != null && testItem.ID == itemID)
                    {
                        int temp = testItem.Number;
                        if (testItem.Number >= leftToRemove)
                        {
                            testItem.Remove(leftToRemove, inventory == PlayerInventory);
                            if (testItem.Number == 0)
                            {
                                toRemove.Add(inventory[i, j]);
                            }
                        }
                        else
                        {
                            testItem.Remove(testItem.Number, inventory == PlayerInventory);
                            toRemove.Add(inventory[i, j]);
                        }

                        leftToRemove -= temp;
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
            MapManager.CurrentMap.SpawnItemOnMap(item, PlayerManager.PlayerActor.CollisionBoxLocation, true, ItemPickupState.Manual);
            TaskManager.CheckItemCount();
        }

        #region Add Item to Inventory
        public static bool AddMapItemToInventory(Item itemToAdd)
        {
            return AddToInventory(itemToAdd, GetInventory(true), true, false, false);
        }
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
        private static bool AddToInventory(Item itemToAdd, Item[,] inventory, bool playerInventory = true, bool noDisplay = false, bool dropOnMap = true)
        {
            bool rv = false;

            if(itemToAdd == null || itemToAdd.Number == 0)
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
            if (HasSpaceInInventory(itemToAdd.ID, itemToAdd.Number, inventory, ref validRow, ref validCol))
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
                    if (inventory[validRow, validCol].Number + itemToAdd.Number > Constants.MAX_STACK_SIZE)
                    {
                        int numToAdd = Constants.MAX_STACK_SIZE - inventory[validRow, validCol].Number;

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
                    TaskManager.CheckItemCount();

                    if (!noDisplay)
                    {
                        //Used to display an item that was just added to the inventory
                        AddedItemList.Add(DataManager.GetItem(itemToAdd.ID, itemToAdd.Number));
                    }
                }

                rv = true;
            }

            //If we failed to add it, then we need to drop it onto the world map.
            if (!rv && dropOnMap)
            {
                DropItemOnMap(itemToAdd);
                //Todo mail Tools to player
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
                    if (item.CompareType(ItemTypeEnum.Tool))
                    {
                        inventory[row, column] = (Tool)(item);
                    }
                    else
                    {
                        inventory[row, column] = item;
                    }
                    if (inventory == PlayerInventory)
                    {
                        TaskManager.CheckItemCount();
                    }
                    rv = true;
                }
                else
                {
                    if (inventory[row, column].ID == item.ID && inventory[row, column].Stacks() && Constants.MAX_STACK_SIZE >= (inventory[row, column].Number + item.Number))
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
                TaskManager.CheckItemCount();
            }

            inventory[row, column] = null;
        }
        #endregion

        #region Remove Item from Inventory
        /// <summary>
        /// Helper to redirect method to the players Inventory
        /// </summary>
        /// <param name="it">The Item object to remove</param>
        public static bool RemoveItemFromInventory(Item it, bool playerInventory = true)
        {
            return RemoveItemFromInventory(it, GetInventory(playerInventory));
        }

        /// <summary>
        /// Removes the given item object from the inventory
        /// </summary>
        /// <param name="it">The item object to remove</param>
        /// <param name="inventory">The inventory to search</param>
        public static bool RemoveItemFromInventory(Item it, Item[,] inventory)
        {
            bool rv = false;

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
                                TaskManager.CheckItemCount();
                            }
                        }
                        //null the item and exit
                        inventory[i, j] = null;
                        rv = true;
                        goto Exit;
                    }
                }
            }
            Exit:

            return rv;
        }
        #endregion

        public static bool ExpendResources(Dictionary<int, int> requiredItems, Container c = null)
        {
            bool rv = false;
            if (requiredItems == null)
            {
                return false;
            }

            if (InventoryManager.HasSufficientItems(requiredItems, c))
            {
                rv = true;

                if(c != null)
                {
                    InitExtraInventory(c.Inventory);
                }

                foreach (KeyValuePair<int, int> kvp in requiredItems)
                {
                    InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value, c);
                }

                if (c != null)
                {
                    ClearExtraInventory();
                }
            }

            return rv;
        }

        public static bool ExpendResources(Dictionary<int, int> requiredItems, Item[,] inventory)
        {
            bool rv = false;
            if (requiredItems == null)
            {
                return false;
            }

            if (InventoryManager.HasSufficientItems(requiredItems, inventory))
            {
                rv = true;

                if (inventory != null)
                {
                    InitExtraInventory(inventory);
                }

                foreach (KeyValuePair<int, int> kvp in requiredItems)
                {
                    InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value, inventory);
                }

                if (inventory != null)
                {
                    ClearExtraInventory();
                }
            }

            return rv;
        }

        public static Item GetCurrentItem()
        {
            return PlayerInventory[GameManager.HUDItemRow, GameManager.HUDItemCol];
        }

        public static Item GetItemFromLocation(int row, int column, bool PlayerInventory = true)
        {
            return GetInventory(PlayerInventory)?[row, column];
        }

        /// <summary>
        /// Runs through the given list and checks that the Inventory has the required items.
        /// </summary>
        /// <param name="requiredItems">The list of required items and the numbers of each item</param>
        /// <returns></returns>
        public static bool HasSufficientItems(Dictionary<int, int> requiredItems, Container c = null)
        {
            bool rv = true;
            foreach (KeyValuePair<int, int> kvp in requiredItems)
            {
                if (c == null)
                {
                    if (!HasItemInPlayerInventory(kvp.Key, kvp.Value))
                    {
                        rv = false;
                        break;
                    }
                }
                else
                {
                    if (!HasItemInInventory(kvp.Key, kvp.Value, c.Inventory))
                    {
                        rv = false;
                        break;
                    }
                }
            }

            return rv;
        }

        public static bool HasSufficientItems(Dictionary<int, int> requiredItems, Item[,] inventory)
        {
            bool rv = true;
            foreach (KeyValuePair<int, int> kvp in requiredItems)
            {
                if (!HasItemInInventory(kvp.Key, kvp.Value, inventory))
                {
                    rv = false;
                    break;
                }
            }

            return rv;
        }

        public static void CleanupInventoryDisplay()
        {
            if (GameManager.HeldItem != null && ExtraInventory != null)
            {
                AddToInventory(GameManager.HeldItem);
                GameManager.DropItem();
            }
            if (GameManager.CurrentNPC != null)
            {
                GUIManager.OpenTextWindow(GameManager.CurrentNPC.GetDialogEntry("Goodbye"));
            }

            if (GameManager.CurrentWorldObject != null && GameManager.CurrentWorldObject is Decor decorObj)
            {
                if(decorObj.SetDisplayEntity(ExtraInventory[0, 0], false))
                {
                    if (decorObj.Archive)
                    {
                        TownManager.AddToArchive(ExtraInventory[0, 0].ID);
                    }
                }
            }

            ExtraHoldSingular = false;
            LockedInventory = false;
            ValidIDs = null;
            CurrentInventoryDisplay = DisplayTypeEnum.None;
            GameManager.SetSelectedWorldObject(null);
        }
    }
}
