using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Container : Buildable
    {
        public int Rows => GetIntByIDKey("Rows");
        public int Columns => GetIntByIDKey("Cols");
        public Item[,] Inventory { get; }

        public Container(int id, Dictionary<string, string> args) : base(id)
        {
            Inventory = new Item[Rows, Columns];

            InventoryManager.InitExtraInventory(Inventory);
            if (args != null && args.ContainsKey("ItemID"))
            {
                string[] holdSplit = Util.FindParams(args["ItemID"]);
                foreach (string s in holdSplit)
                {
                    var itemStr = Util.FindArguments(s);
                    InventoryManager.AddToInventory(int.Parse(itemStr[0]), (itemStr.Length > 1 ? int.Parse(itemStr[1]) : 1), false);
                }
            }
            InventoryManager.ClearExtraInventory();
        }

        public override bool ProcessRightClick()
        {
            DisplayTypeEnum displayType = DisplayTypeEnum.Inventory;
            GameManager.SetSelectedWorldObject(this);
            GUIManager.OpenMainObject(new HUDInventoryDisplay(Inventory, displayType, false));
            InventoryManager.SetHoldItem(GetEnumByIDKey<ItemTypeEnum>("ItemType"));
            return true;
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            foreach (Item i in Inventory)
            {
                data.stringData += string.Format("{0}/", Item.SaveItemToString(i));
            }
            data.stringData = data.stringData.Remove(data.stringData.Length - 1);
            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);

            string[] strData = Util.FindParams(data.stringData);
            if (strData.Length > 0)
            {
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        int index = Util.ListIndexFromMultiArray(i, j, Columns);
                        if (!string.IsNullOrEmpty(strData[index]))
                        {
                            string[] itemData = Util.FindArguments(strData[index]);
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
    }
}
