using Adventure.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers
{
    public static class DropManager
    {
        public static List<Item> DropItemsFromWorldObject(ObjectManager.ObjectIDs id)
        {
            List<Item> itemList = new List<Item>();

            switch (id)
            {
                case ObjectManager.ObjectIDs.Rock:
                    itemList.Add(ObjectManager.GetItem(1, 1));
                    break;
                case ObjectManager.ObjectIDs.Tree:
                    itemList.Add(ObjectManager.GetItem(3, 1));
                    break;
            }

            return itemList;
        }
    }
}
