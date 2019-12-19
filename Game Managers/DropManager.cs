using Microsoft.Xna.Framework.Content;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using RiverHollow.Actors;

namespace RiverHollow.Game_Managers
{
    public static class DropManager
    {
        private static Dictionary<int, string> _diMobDrops;

        public static void LoadContent(ContentManager Content)
        {
            _diMobDrops = Content.Load<Dictionary<int, string>>(@"Data\MobDrops");
        }

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
                itemList.Add(ObjectManager.GetItem(id, num));
            }
        }

        public static void DropItemsFromMonster(Monster m)
        {
            //Just for testing atm
            List<Item> it = new List<Item> { ObjectManager.GetItem(26) };

            it[0].AutoPickup = false;
            m.Tile.SetCombatItem(it[0]);

            MapManager.DropItemsOnMap(it, m.Tile.Position, false);
        }
    }
}
