using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Map_Handling;

namespace RiverHollow.WorldObjects
{
    public class Machine : WorldObject
    {
        readonly string _sEffectWorking = "";

        public Recipe[] CraftingSlots { get; private set; }
        public bool CraftDaily => GetBoolByIDKey("Daily");
        public bool Kitchen => GetBoolByIDKey("Kitchen");
        private bool HoldItem => GetBoolByIDKey("HoldItem");

        private Point ItemOffset => GetPointByIDKey("ItemOffset");

        protected int _iWorkingFrames = 2;
        protected float _fFrameSpeed = 0.3f;

        public int Capacity => GetIntByIDKey("Capacity", 1);
        public int MaxBatch => GetIntByIDKey("Batch", CraftDaily ? 3 : 1);

        public Machine(int id, Dictionary<string, string> stringData) : base(id)
        {
            if (stringData.ContainsKey("WorkAnimation"))
            {
                string[] split = stringData["WorkAnimation"].Split('-');
                _iWorkingFrames = int.Parse(split[0]);
                _fFrameSpeed = float.Parse(split[1]);
            }

            Util.AssignValue(ref _sEffectWorking, "WorkEffect", stringData);

            LoadDictionaryData(stringData);

            CraftingSlots = new Recipe[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                CraftingSlots[i] = new Recipe
                {
                    ID = -1,
                    CraftTime = 0
                };
            }
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = "Textures\\texMachines")
        {
            Sprite = new AnimatedSprite(@"Textures\texMachines");
            Sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _pSize, 1, 0.3f, false);
            Sprite.AddAnimation(AnimationEnum.PlayAnimation, _pImagePos.X + _pSize.Y, _pImagePos.Y, _pSize, _iWorkingFrames, _fFrameSpeed, false);
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
                Item i = DataManager.CraftItem(CraftingSlots[0].ID);
                i.Draw(spriteBatch, new Rectangle((int)(MapPosition.X - ItemOffset.X), (int)(MapPosition.Y - ItemOffset.Y), Constants.TILE_SIZE, Constants.TILE_SIZE), true, Sprite.LayerDepth + 1);
            }
            base.Draw(spriteBatch);
        }

        /// <summary>
        /// Override method for Rollover. Shouldn't matter since item crafting should take no time
        /// but for future proofing we'll have this here.
        /// </summary>
        public override void Rollover()
        {
            if (MakingSomething())
            {
                for (int i = 0; i < Capacity; i++)
                {
                    CraftingSlots[i].CraftTime -= CraftingSlots[i].CraftTime > 0 ? 1 : 0;
                }
            }
        }

        public override bool ProcessLeftClick() { return ClickProcess(); }
        public override bool ProcessRightClick() { return ClickProcess(); }

        private bool ClickProcess()
        {
            bool rv = false;
            if (Tiles.Find(x => x.PlayerIsAdjacent()) != null)
            {
                rv = true;
                if (HoldingItem())
                {
                    InventoryManager.AddToInventory(DataManager.CraftItem(CraftingSlots[0].ID));
                    CraftingSlots[0].ID = -1;
                }
                else
                {
                    GUIManager.OpenMainObject(new HUDCraftingDisplay(this));
                }
            }

            return rv;
        }

        public bool MakingSomething()
        {
            bool rv = false;
            for (int i = 0; i < Capacity; i++)
            {
                if (CraftingSlots[i].ID != -1)
                {
                    rv = true;
                    break;
                }
            }

            return rv;
        }

        private bool HoldingItem()
        {
            return HoldItem && CraftingSlots[0].ID != -1 && CraftingSlots[0].CraftTime == 0;
        }

        public bool CapacityFull()
        {
            bool rv = true;
            for (int i = 0; i < Capacity; i++)
            {
                if (CraftingSlots[i].ID == -1)
                {
                    rv = false;
                    break;
                }
            }

            return rv;
        }

        public void TakeItem(int capacityIndex)
        {
            InventoryManager.AddToInventory(DataManager.CraftItem(CraftingSlots[capacityIndex].ID, CraftingSlots[capacityIndex].BatchSize));
            CraftingSlots[capacityIndex].ID = -1;
            CraftingSlots[capacityIndex].BatchSize = 1;
            CraftingSlots[capacityIndex].CraftTime = 0;
        }

        public bool SufficientStamina()
        {
            return CraftDaily || PlayerManager.CurrentEnergy >= Constants.ACTION_COST / 2;
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

        /// <summary>
        /// Called by the HUDCraftingMenu to craft the selected item.
        /// 
        /// Ensure that the Player has enough space in their inventory for the item
        /// as well as they have the required items to make it.
        /// 
        /// Perform the Crafting steps and add the item to the inventory.
        /// </summary>
        /// <param name="itemToCraft">The Item object to craft</param>
        public void AttemptToCraftChosenItem(Item itemToCraft, int batchSize)
        {
            bool success = false;
            if (CraftDaily)
            {
                for (int i = 0; i < Capacity; i++)
                {
                    if (CraftingSlots[i].ID == -1)
                    {
                        if (PlayerManager.ExpendResources(itemToCraft.GetRequiredItems(), batchSize))
                        {
                            success = true;
                            CraftingSlots[i].ID = itemToCraft.ID;
                            CraftingSlots[i].BatchSize = batchSize;
                            CraftingSlots[i].CraftTime = DataManager.GetIntByIDKey(CraftingSlots[i].ID, "CraftTime", DataType.Item, 1);
                        }
                        break;
                    }
                }
            }
            else if (InventoryManager.HasSpaceInInventory(itemToCraft.ID, 1)
                && SufficientStamina()
                && PlayerManager.ExpendResources(itemToCraft.GetRequiredItems()))
            {
                success = true;
                PlayerManager.LoseEnergy(Constants.ACTION_COST / 2);
                if (Kitchen && CurrentMap.BuildingID == TownManager.Inn.ID)
                {
                    TownManager.AddToKitchen(DataManager.CraftItem(itemToCraft.ID));
                }
                else
                {
                    InventoryManager.AddToInventory(itemToCraft.ID, itemToCraft.Number);
                }
            }

            if (success && !string.IsNullOrEmpty(_sEffectWorking))
            {
                SoundManager.PlayEffect(_sEffectWorking);
            }
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            for (int i = 0; i < Capacity; i++)
            {
                data.stringData += Util.SaveInt(CraftingSlots[i].ID) + "-" + CraftingSlots[i].BatchSize + "-" + CraftingSlots[i].CraftTime + "|";
            }
            data.stringData =  data.stringData.TrimEnd('|');

            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            if (data.stringData != null)
            {
                string[] strData = Util.FindParams(data.stringData);

                for (int i = 0; i < strData.Length; i++)
                {
                    if (i >= Capacity) { break; }

                    string[] split = Util.FindArguments(strData[i]);
                    CraftingSlots[i].ID = Util.LoadInt(split[0]);
                    CraftingSlots[i].BatchSize = int.Parse(split[1]);
                    if (split.Length > 2)
                    {
                        CraftingSlots[i].CraftTime = int.Parse(split[2]);
                    }
                }
            }
        }

        public struct Recipe
        {
            public int ID;
            public int CraftTime;
            public int BatchSize;
        }
    }
}
