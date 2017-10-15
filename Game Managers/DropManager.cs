using RiverHollow.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    itemList.Add(ObjectManager.GetItem(0, 1));
                    break;
                case 1:
                    itemList.Add(ObjectManager.GetItem(0, 1));
                    itemList.Add(ObjectManager.GetItem(0, 1));
                    itemList.Add(ObjectManager.GetItem(0, 1));
                    itemList.Add(ObjectManager.GetItem(0, 1));
                    itemList.Add(ObjectManager.GetItem(0, 1));
                    break;
                case 2:
                    itemList.Add(ObjectManager.GetItem(2, 1));
                    break;
            }

            return itemList;
        }
    }
}
