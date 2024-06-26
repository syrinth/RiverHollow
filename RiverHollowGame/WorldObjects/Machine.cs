﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;

using static RiverHollow.Utilities.Enums;
using Microsoft.Xna.Framework.Graphics;
using System.Linq;
using System;
using RiverHollow.Buildings;
using static RiverHollow.Game_Managers.SaveManager;
using System.Collections.Specialized;

namespace RiverHollow.WorldObjects
{
    public class Machine : WorldObject
    {
        public Recipe CraftingSlot { get; private set; }

        public bool Producer => GetBoolByIDKey("Producer");

        public Machine(int id) : base(id) { }

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

        public override bool ProcessLeftClick() { return ClickProcess(); }
        public override bool ProcessRightClick() { return ClickProcess(); }

        private bool ClickProcess()
        {
            GUIManager.OpenMainObject(new HUDRecipeBook(this));

            return true;
        }

        /// <summary>
        /// Override method for Rollover. Determine what items we can make
        /// </summary>
        public override void Rollover()
        {
            if (GetObjectBuilding() is Building b)
            {
                if (Producer)
                {
                    ProduceItem(b);
                }
                else
                {
                    CraftItem(b);
                }
            }
        }

        private void CraftItem(Building b)
        {
            int craftsLeft = b.GetDailyCraftingLimit();
            var craftingList = GetCurrentCraftingList();
            var validItems = WhatCanWeCraft(craftingList);

            var merchandiseStock = GetMerchandiseStock();
            InventoryManager.InitExtraInventory(merchandiseStock);

            bool emptySlotFound = InventoryManager.HasSpaceInInventory(9999999, 1, false);
            while (craftsLeft > 0)
            {
                if (validItems.Count > 0)
                {
                    var tempItems = validItems;
                    bool success = false;
                    //Determine if there are any items missing in stock
                    var missingItems = tempItems.Where(x => !InventoryManager.HasItemInInventory(x.ID, 1, merchandiseStock)).ToList();

                    //We found at least one missing item and an empty slot so make it
                    if (missingItems.Count > 0 && emptySlotFound)
                    {
                        var item = missingItems[0];
                        if (TryCraftAtTarget(item, merchandiseStock, craftingList, ref validItems))
                        {
                            success = true;

                        }
                    }
                    else  //Unable to make a new item, make whichever item we have the fewest of
                    {
                        List<Tuple<int, int>> numberList = new List<Tuple<int, int>>();
                        foreach (var i in merchandiseStock)
                        {
                            if (i != null && i.ID > -1)
                            {
                                numberList.Add(new Tuple<int, int>(i.ID, i.Number));
                            }
                        }

                        numberList = numberList.OrderBy(x => x.Item2).ToList();

                        var item = DataManager.GetItem(numberList[0].Item1);
                        if (TryCraftAtTarget(item, GetMerchandiseStock(), craftingList, ref validItems))
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

        private void ProduceItem(Building b)
        {
            int craftsLeft = b.GetDailyCraftingLimit();
            var merchandiseStock = GetMerchandiseStock();
            InventoryManager.InitExtraInventory(merchandiseStock);

            var productionDictionary = GetProductionDictionary();
            while (craftsLeft > 0)
            {
                var rolledItem = Util.RollOnRarityTable(productionDictionary);
                if (InventoryManager.HasSpaceInInventory(rolledItem, 1, false))
                {
                    var item = DataManager.CraftItem(rolledItem);
                    InventoryManager.AddToInventory(item, false, true);
                    TaskManager.AttemptProgressCraft(item);
                    TownManager.AddToCodex(item.ID);
                    craftsLeft--;
                }
                else { break; }
            }

            InventoryManager.ClearExtraInventory();
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

        private void CheckAddToShopInventory(ref Dictionary<int, Tuple<int, List<Container>>> shopInventory, int id)
        {
            if (!shopInventory.ContainsKey(id))
            {
                shopInventory[id] = new Tuple<int, List<Container>>(0, new List<Container>());
            }
        }

        private bool TryCraftAtTarget(Item chosenItem, Item[,] targetInventory, List<int> craftingList, ref List<Item> validItems)
        {
            bool rv = false;

            if (InventoryManager.ExpendResources(chosenItem.GetRequiredItems(), GetStash()))
            {
                rv = true;

                var targetItem = DataManager.CraftItem(chosenItem.ID);
                InventoryManager.InitExtraInventory(targetInventory);
                InventoryManager.AddToInventory(targetItem, false, true);
                InventoryManager.ClearExtraInventory();

                TaskManager.AttemptProgressCraft(targetItem);

                TownManager.AddToCodex(chosenItem.ID);
                validItems = WhatCanWeCraft(craftingList);
            }

            return rv;
        }

        public bool HasSufficientItems(Item targetItem)
        {
            bool rv = false;
            if (GetCurrentCraftingList().Contains(targetItem.ID))
            {
                if (InventoryManager.HasSufficientItems(targetItem.GetRequiredItems(), GetStash()))
                {
                    rv = true;
                }
            }

            return rv;
        }

        public Building GetObjectBuilding()
        {
            Building rv = null;
            if (CurrentMap.BuildingID != -1)
            {
                int objID = CurrentMap.BuildingID;
                if (TownManager.TownObjectBuilt(objID) && DataManager.GetEnumByIDKey<BuildableEnum>(objID, "Subtype", DataType.WorldObject) == BuildableEnum.Building)
                {
                    rv = (Building)MapManager.TownMap.GetObjectsByID(objID)[0];
                }
            }

            return rv;
        }
        public Item[,] GetStash()
        {
            Item[,] rv = null;

            if(GetObjectBuilding() is Building store)
            {
                rv = store.Stash;
            }

            return rv;
        }

        public Item[,] GetMerchandiseStock()
        {
            Item[,] rv = null;

            if (GetObjectBuilding() is Building store)
            {
                rv = store.Merchandise;
            }

            return rv;
        }

        public bool MakingSomething()
        {
            return CraftingSlot.ID >= 0;
        }

        public List<Tuple<int, bool>> GetFullCraftingList()
        {
            var craftingList = new List<Tuple<int, bool>>();
            string makes = GetStringByIDKey("Makes");
            if (!string.IsNullOrEmpty(makes))
            {
                //Read in what items the machine can make
                string[] split = Util.FindParams(makes);
                for (int i = 0; i < split.Length; i++)
                {
                    string[] formula = Util.FindArguments(split[i]);
                    bool canCraft = formula.Length == 1 || (int.Parse(formula[1]) <= TownManager.GetBuildingByID(CurrentMap.BuildingID).GetFormulaLevel());

                    craftingList.Add(new Tuple<int, bool>(int.Parse(formula[0]), canCraft));
                }
            }

            return craftingList;
        }

        public List<int> GetCurrentCraftingList()
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

        public Dictionary<RarityEnum, List<int>> GetProductionDictionary()
        {
            var rv = new Dictionary<RarityEnum, List<int>>();

            string makes = GetStringByIDKey("Makes");
            if (!string.IsNullOrEmpty(makes))
            {
                //Read in what items the machine can make
                string[] split = Util.FindParams(makes);
                foreach (string s in split)
                {
                    int resourceID = -1;
                    RarityEnum rarity = RarityEnum.C;
                    Util.GetRarity(s, ref resourceID, ref rarity);

                    Util.AddToListDictionary(ref rv, rarity, resourceID);
                }
            }

            return rv;
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
