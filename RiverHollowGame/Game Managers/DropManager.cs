using System.Collections.Generic;
using RiverHollow.Characters;
using RiverHollow.WorldObjects;
using RiverHollow.Utilities;

namespace RiverHollow.Game_Managers
{
    public static class DropManager
    {
        public static List<Item> DropItemsFromWorldObject(int id)
        {
            List<Item> itemList = new List<Item>();

            switch (id)
            {
                case 0:
                    AddItems(ref itemList, 0, 1);
                    break;
                case 1:
                    AddItems(ref itemList, 0, 5);
                    break;
                case 2:
                    AddItems(ref itemList, 2, 7);
                    break;
                case 5:
                    AddItems(ref itemList, 80, 2);
                    break;
                case 8:
                    AddItems(ref itemList, 11, 8);
                    break;
                case 10:
                    AddItems(ref itemList, 90, 1);
                    break;
                case 11:
                    AddItems(ref itemList, 91, 1);
                    break;
            }

            return itemList;
        }

        private static void AddItems(ref List<Item> itemList, int id, int num)
        {
            for (int i = 0; i < num; i++)
            {
                itemList.Add(DataManager.GetItem(id, num));
            }
        }

        /// <summary>
        /// Randomly gets the appropriate item from the the Monster to drop
        /// </summary>
        /// <param name="m">The Monster that is dropping the Item</param>
        /// <returns>The Item that was dropped</returns>
        public static Item DropMonsterLoot(TacticalMonster m)
        {
            Item droppedItem = DataManager.GetItem(m.GetRandomLootItem());
            //Just for testing atm
            List<Item> it = new List<Item> { droppedItem };

            it[0].AutoPickup = false;

            MapManager.DropItemsOnMap(it, Util.SnapToGrid(m.CollisionBox.Center.ToVector2()), false);

            return droppedItem;
        }
    }
}
