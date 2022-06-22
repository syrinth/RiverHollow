﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
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
        }

        public override void ProcessRightClick()
        {
            GUIManager.OpenMainObject(new HUDInventoryDisplay(Inventory, DisplayTypeEnum.Inventory));
        }

        internal ContainerData SaveData()
        {
            ContainerData containerData = new ContainerData
            {
                containerID = this.ID,
                rows = Rows,
                cols = Columns,
                x = (int)this.MapPosition.X,
                y = (int)this.MapPosition.Y
            };

            containerData.Items = new List<ItemData>();
            foreach (Item i in (this.Inventory))
            {
                ItemData itemData = Item.SaveData(i);
                containerData.Items.Add(itemData);
            }
            return containerData;
        }
        internal void LoadData(ContainerData data)
        {
            SnapPositionToGrid(new Vector2(data.x, data.y));
            for (int i = 0; i < Rows; i++)
            {
                for (int j = 0; j < Columns; j++)
                {
                    ItemData item = data.Items[i * InventoryManager.maxItemRows + j];
                    Item newItem = DataManager.GetItem(item.itemID, item.num);
                    if (newItem != null) { newItem.ApplyUniqueData(item.strData); }

                    InventoryManager.InitContainerInventory(this.Inventory);
                    InventoryManager.AddItemToInventorySpot(newItem, i, j, false);
                    InventoryManager.ClearExtraInventory();
                }
            }
        }
    }
}
