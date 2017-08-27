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
                    itemList.Add(ObjectManager.GetItem(ObjectManager.ItemIDs.Stone));
                    break;
                case ObjectManager.ObjectIDs.Tree:
                    itemList.Add(ObjectManager.GetItem(ObjectManager.ItemIDs.Wood));
                    break;
            }

            return itemList;
        }
    }
}
