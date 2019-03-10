using Microsoft.Xna.Framework.Content;
using RiverHollow.WorldObjects;
using RiverHollow.Misc;
using System.Collections.Generic;

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
            }

            return itemList;
        }

        private static void AddItems(ref List<Item> itemList, int id, int num)
        {
            for (int i = 0; i < num; i++)
            {
                itemList.Add(ObjectManager.GetItem(id, 10));
            }
        }

        public static List<Item> DropItemsFromMob(int id)
        {
            List<Item> itemList = new List<Item>();
            RHRandom r = new RHRandom();
            string[] drops = Util.FindTags(_diMobDrops[id]);
            foreach(string s in drops)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Item"))
                {
                    string[] info = tagType[1].Split('-');
                    int chance = r.Next(1, 100);
                    if (chance <= int.Parse(info[0]))
                    {
                        itemList.Add(ObjectManager.GetItem(int.Parse(info[1])));
                    }
                }
            }

            return itemList;
        }
    }
}
