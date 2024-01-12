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

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (GetBoolByIDKey("ShopTable"))
            {
                var strData = GetStringParamsByIDKey("ShopTable");
                var point = Util.ParsePoint(strData[0]);
                var offset = int.Parse(strData[1]);
                for (int i = 0; i < Columns; i++)
                {
                    Inventory[0, i]?.Draw(spriteBatch, new Rectangle(MapPosition.X + point.X, MapPosition.Y + point.Y, Constants.TILE_SIZE, Constants.TILE_SIZE), true, Sprite.LayerDepth + 1);

                    point.X += offset;
                }
            }
        }

        public override bool ProcessRightClick()
        {
            DisplayTypeEnum displayType = DisplayTypeEnum.Inventory;
            List<int> validIDs = null;
            if (GetBoolByIDKey("ShopTable"))
            {
                displayType = DisplayTypeEnum.ShopTable;
                var machines = CurrentMap.GetObjectsByType<Machine>();

                if (machines.Count > 0)
                {
                    validIDs = new List<int>();
                    foreach (var obj in machines)
                    {
                        if (obj is Machine m)
                        {
                            validIDs.AddRange(m.GetCraftingList().Select(x => x.Item1));
                        }
                    }
                }
            }

            GUIManager.OpenMainObject(new HUDInventoryDisplay(Inventory, displayType, false, validIDs));
            InventoryManager.SetHoldItem(GetEnumByIDKey<ItemEnum>("ItemType"));
            return true;
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map, ignoreActors);

            bool isStash = GetBoolByIDKey("Stash");
            bool isShopTable = GetBoolByIDKey("ShopTable");
            if (rv && (isStash || isShopTable))
            {
                var machines = CurrentMap.GetObjectsByType<Machine>();

                foreach(var obj in machines)
                {
                    if (isStash)
                    {
                        if (obj is Machine m)
                        {
                            m.SetStash(this);
                        }
                    }
                    else if (isShopTable)
                    {
                        if (obj is Machine m)
                        {
                            m.AddShopTable(this);
                        }
                    }
                }
            }

            if (isShopTable && GetBoolByIDKey("Pantry"))
            {
                TownManager.TownManagerCheck(CurrentMap, this);
            }

            return rv;
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
