using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;

using static RiverHollow.Utilities.Enums;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Map_Handling;
using System.Linq;
using System;
using RiverHollow.Buildings;

namespace RiverHollow.WorldObjects
{
    public class Machine : WorldObject
    {
        public Recipe CraftingSlot { get; private set; }
        private bool HoldItem => GetBoolByIDKey("HoldItem");

        private Point ItemOffset => GetPointByIDKey("ItemOffset");

        public Container Stash { get; private set; }
        private readonly List<Container> _liShopTables;

        public Machine(int id) : base(id)
        {
            _liShopTables = new List<Container>();
        }

        protected override void LoadSprite()
        {
            var frames = 2;
            var frameSpeed = 0.3f;

            if (GetBoolByIDKey("WorkAnimation"))
            {
                string[] split = GetStringArgsByIDKey("WorkAnimation");
                frames = int.Parse(split[0]);
                frameSpeed = float.Parse(split[1]);
            }

            Sprite = new AnimatedSprite(DataManager.FILE_MACHINES);
            Sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _pSize, 1, 0.3f, false);
            Sprite.AddAnimation(AnimationEnum.PlayAnimation, _pImagePos.X + _pSize.Y, _pImagePos.Y, _pSize, frames, frameSpeed, false);
            Sprite.PlayAnimation(AnimationEnum.ObjectIdle);
            Sprite.Show = true;

            SetSpritePos(MapPosition);
        }

        public override void Update(GameTime gTime)
        {
            if (MakingSomething())
            {
                Sprite.Update(gTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (HoldingItem())
            {
                Item i = DataManager.CraftItem(CraftingSlot.ID);
                i.Draw(spriteBatch, new Rectangle((int)(MapPosition.X - ItemOffset.X), (int)(MapPosition.Y - ItemOffset.Y), Constants.TILE_SIZE, Constants.TILE_SIZE), true, Sprite.LayerDepth + 1);
            }
            base.Draw(spriteBatch);
        }

        public override bool ProcessLeftClick() { return ClickProcess(); }
        public override bool ProcessRightClick() { return ClickProcess(); }

        private bool ClickProcess()
        {
            if (HoldingItem())
            {
                InventoryManager.AddToInventory(DataManager.CraftItem(CraftingSlot.ID));
                CraftingSlot = new Recipe(-1, -1);
            }
            else
            {
                GUIManager.OpenMainObject(new HUDRecipeBook(this));
            }

            return true;
        }

        /// <summary>
        /// Override method for Rollover. Determine what items we can make
        /// </summary>
        public override void Rollover()
        {
            var mapProperties = CurrentMap.GetMapProperties();
            if (Stash != null && mapProperties.ContainsKey("BuildingID") && int.TryParse(mapProperties["BuildingID"], out int buildingID))
            {
                Building b = TownManager.GetBuildingByID(buildingID);
                if (b != null)
                {

                    int craftsLeft = b.GetDailyCraftingLimit();
                    var craftingList = GetCraftingList();
                    var validItems = WhatCanWeCraft(craftingList);

                    //Create a Dictionary to represent the stock we currently have
                    var shopInventory = CreateShopInventory(craftingList);
                    bool emptySlotFound = shopInventory.ContainsKey(-1);

                    while (craftsLeft > 0)
                    {
                        if (validItems.Count > 0)
                        {
                            var tempItems = validItems;
                            bool success = false;
                            if (_liShopTables.Count > 0)
                            {
                                //Determine if there are any items missing in stock
                                var missingItems = tempItems.Where(x => !shopInventory.ContainsKey(x.ID)).ToList();

                                //We found at least one missing item and an empty slot so make it
                                if (missingItems.Count > 0 && emptySlotFound)
                                {
                                    var item = missingItems[0];
                                    var table = Util.GetRandomItem(shopInventory[-1].Item2);
                                    if (TryCraftAtTarget(item, table, craftingList, ref validItems))
                                    {
                                        success = true;

                                        //Update the ShopInventory with the new item
                                        AddItemToShopInventory(ref shopInventory, item, table);

                                        //Remove one instance of table from the shopInventory. If there are multiple empty slots in the table
                                        //It will be present multiple times.
                                        shopInventory[-1].Item2.Remove(table);
                                        if (shopInventory[-1].Item2.Count == 0)
                                        {
                                            shopInventory.Remove(-1);
                                            emptySlotFound = shopInventory.ContainsKey(-1);
                                        }
                                    }
                                }
                                else  //Unable to make a new item, make whichever item we have the fewest of
                                {
                                    List<Tuple<int, int>> numberList = new List<Tuple<int, int>>();
                                    foreach (var key in shopInventory.Keys)
                                    {
                                        if (key > -1)
                                        {
                                            numberList.Add(new Tuple<int, int>(key, shopInventory[key].Item1));
                                        }
                                    }

                                    numberList = numberList.OrderBy(x => x.Item2).ToList();

                                    var item = DataManager.GetItem(numberList[0].Item1);
                                    var table = Util.GetRandomItem(shopInventory[item.ID].Item2);
                                    if (TryCraftAtTarget(item, table, craftingList, ref validItems))
                                    {
                                        success = true;
                                        AddItemToShopInventory(ref shopInventory, item, table);
                                    }
                                }
                            }
                            else
                            {
                                var craftedItem = DataManager.CraftItem(validItems[0].ID);
                                if (TryCraftAtTarget(craftedItem, Stash, craftingList, ref validItems))
                                {
                                    success = true;
                                }
                            }

                            if (!success) { break; }
                            else { craftsLeft--; }
                        }
                        else { break; }
                    }
                }
            }
        }

        private List<Item> WhatCanWeCraft(List<int> craftingList)
        {
            var validItems = new List<Item>();
            foreach (var craftID in craftingList)
            {
                Item i = DataManager.GetItem(craftID);
                if (HasSufficientItems(i))
                {
                    validItems.Add(i);
                }
            }

            validItems = validItems.OrderByDescending(x => x.Value).ToList();

            return validItems;
        }

        private Dictionary<int, Tuple<int, List<Container>>> CreateShopInventory(List<int> craftingList)
        {
            var shopInventory = new Dictionary<int, Tuple<int, List<Container>>>();
            foreach (var table in _liShopTables)
            {
                foreach (Item i in table.Inventory)
                {
                    if (i == null)
                    {
                        AddEmptyToShopInventory(ref shopInventory, -1, table);
                    }
                    else
                    {
                        AddItemToShopInventory(ref shopInventory, i, table);
                    }
                }
            }
            return shopInventory;
        }

        private void AddEmptyToShopInventory(ref Dictionary<int, Tuple<int, List<Container>>> shopInventory, int id, Container table)
        {
            CheckAddToShopInventory(ref shopInventory, -1);
            shopInventory[-1].Item2.Add(table);
        }

        private void AddItemToShopInventory(ref Dictionary<int, Tuple<int, List<Container>>> shopInventory, Item i, Container table)
        {
            CheckAddToShopInventory(ref shopInventory, i.ID);

            var inventory = shopInventory[i.ID];
            int numberofItems = inventory.Item1;

            inventory.Item2.Add(table);
            numberofItems += i.Number;

            shopInventory[i.ID] = new Tuple<int, List<Container>>(numberofItems, inventory.Item2);
        }

        private void CheckAddToShopInventory(ref Dictionary<int, Tuple<int, List<Container>>> shopInventory, int id)
        {
            if (!shopInventory.ContainsKey(id))
            {
                shopInventory[id] = new Tuple<int, List<Container>>(0, new List<Container>());
            }
        }

        private bool TryCraftAtTarget(Item chosenItem, Container targetContainer, List<int> craftingList, ref List<Item> validItems)
        {
            bool rv = false;

            if (InventoryManager.ExpendResources(chosenItem.GetRequiredItems(), Stash))
            {
                rv = true;

                InventoryManager.InitExtraInventory(targetContainer.Inventory);
                InventoryManager.AddToInventory(DataManager.CraftItem(chosenItem.ID), false, true);
                InventoryManager.ClearExtraInventory();

                validItems = WhatCanWeCraft(craftingList);
            }

            return rv;
        }

        public bool HasSufficientItems(Item targetItem)
        {
            bool rv = false;
            if (GetCraftingList().Contains(targetItem.ID))
            {
                if (InventoryManager.HasSufficientItems(targetItem.GetRequiredItems(), Stash))
                {
                    rv = true;
                }
            }

            return rv;
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map, ignoreActors);

            if (rv)
            {
                var containers = CurrentMap.GetObjectsByType<Container>();
                var stash = containers.Where(x => x.GetBoolByIDKey("Stash")).ToList();

                if (stash != null && stash.Count > 0 && stash[0] is Container c)
                {
                    SetStash(c);
                }

                var shopTables = containers.Where(x => x.GetBoolByIDKey("ShopTable")).ToList();

                if (shopTables != null && shopTables.Count > 0 && shopTables[0] is Container t)
                {
                    AddShopTable(t);
                }
            }

            return rv;
        }

        public void SetStash(Container obj)
        {
            if (Stash == null)
            {
                Stash = obj;
            }
        }

        public void AddShopTable(Container obj)
        {
            _liShopTables?.Add(obj);
        }

        public bool MakingSomething()
        {
            return CraftingSlot.ID >= 0;
        }

        private bool HoldingItem()
        {
            return HoldItem && CraftingSlot.ID != -1 && CraftingSlot.CraftTime == 0;
        }

        public List<int> GetCraftingList()
        {
            var craftingList = new List<int>();
            string makes = GetStringByIDKey("Makes");
            if (!string.IsNullOrEmpty(makes))
            {
                //Read in what items the machine can make
                string[] split = Util.FindParams(makes);
                for (int i = 0; i < split.Length; i++)
                {
                    string[] formula = Util.FindArguments(split[i]);
                    if (formula.Length == 1 || (int.Parse(formula[1]) <= TownManager.GetBuildingByID(CurrentMap.BuildingID).GetFormulaLevel()))
                    {
                        craftingList.Add(int.Parse(formula[0]));
                    }
                }
            }

            return craftingList;
        }

        public struct Recipe
        {
            public int ID;
            public int CraftTime;

            public Recipe(int id, int craftTime)
            {
                ID = id;
                CraftTime = craftTime;
            }
        }
    }
}
