using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Container : Buildable
    {
        public int Rows { get; }
        public int Columns { get; }
        public Item[,] Inventory { get; }

        public Container(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            Rows = int.Parse(stringData["Rows"]);
            Columns = int.Parse(stringData["Cols"]);

            Inventory = new Item[Rows, Columns];

            InventoryManager.InitExtraInventory(Inventory);
            if (stringData.ContainsKey("ItemID"))
            {
                string[] holdSplit = Util.FindParams(stringData["ItemID"]);
                foreach (string s in holdSplit)
                {
                    InventoryManager.AddToInventory(int.Parse(s), 1, false);
                }
            }
            InventoryManager.ClearExtraInventory();
        }

        public override void ProcessRightClick()
        {
            GUIManager.OpenMainObject(new HUDInventoryDisplay(Inventory, DisplayTypeEnum.Inventory));
        }

        public bool HasItem()
        {
            bool rv = false;
            foreach (Item i in Inventory)
            {
                if(i != null)
                {
                    rv = true;
                }
            }
            return rv;
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();

            foreach (Item i in (this.Inventory))
            {
                ItemData itemData = Item.SaveData(i);
                string strData = string.Empty;
                strData += itemData.itemID + "-";
                strData += itemData.num + "-";
                strData += itemData.strData;
                data.stringData += strData + "|";
            }
            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);

            string[] strData = Util.FindParams(data.stringData);
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    string[] itemData = Util.FindArguments(strData[i * InventoryManager.maxItemRows + j]);
                    Item newItem = DataManager.GetItem(int.Parse(itemData[0]), int.Parse(itemData[1]));
                    if (newItem != null && itemData.Length > 2) { newItem.ApplyUniqueData(itemData[2]); }

                    InventoryManager.InitExtraInventory(this.Inventory);
                    InventoryManager.AddItemToInventorySpot(newItem, i, j, false);
                    InventoryManager.ClearExtraInventory();
                }
            }
        }
    }
}
