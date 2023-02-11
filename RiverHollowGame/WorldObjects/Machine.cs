using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using RiverHollow.Buildings;

using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.WorldObjects
{
    public class Machine : WorldObject
    {
        readonly string _sEffectWorking = "";

        public Recipe[] CraftingSlots { get; private set; }
        public bool CraftDaily => DataManager.GetBoolByIDKey(ID, "Daily", DataType.WorldObject);
        public bool Kitchen => DataManager.GetBoolByIDKey(ID, "Kitchen", DataType.WorldObject);

        private bool HoldItem => DataManager.GetBoolByIDKey(ID, "HoldItem", DataType.WorldObject);

        private Point ItemOffset => DataManager.GetPointByIDKey(ID, "ItemOffset", DataType.WorldObject);

        protected int _iWorkingFrames = 2;
        protected float _fFrameSpeed = 0.3f;

        public int Capacity => DataManager.GetIntByIDKey(ID, "Capacity", DataType.WorldObject, 1);
        public List<int> CraftingList { get; }

        public Machine(int id, Dictionary<string, string> stringData) : base(id)
        {
            if (stringData.ContainsKey("WorkAnimation"))
            {
                string[] split = stringData["WorkAnimation"].Split('-');
                _iWorkingFrames = int.Parse(split[0]);
                _fFrameSpeed = float.Parse(split[1]);
            }

            Util.AssignValue(ref _sEffectWorking, "WorkEffect", stringData);

            CraftingList = new List<int>();
            if (stringData.ContainsKey("Makes"))
            {
                //Read in what items the machine can make
                string[] split = Util.FindParams(stringData["Makes"]);
                for (int i = 0; i < split.Length; i++)
                {
                    CraftingList.Add(int.Parse(split[i]));
                }
            }

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
            _sprite = new AnimatedSprite(@"Textures\texMachines");
            _sprite.AddAnimation(AnimationEnum.ObjectIdle, (int)_pImagePos.X, (int)_pImagePos.Y, _pSize, 1, 0.3f, false);
            _sprite.AddAnimation(AnimationEnum.PlayAnimation, (int)_pImagePos.X + _pSize.Y, (int)_pImagePos.Y, _pSize, _iWorkingFrames, _fFrameSpeed, false);
            _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
            _sprite.Drawing = true;

            SetSpritePos(MapPosition);
        }

        public override void Update(GameTime gTime)
        {
            if (MakingSomething())
            {
                _sprite.Update(gTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (HoldingItem())
            {
                Item i = DataManager.CraftItem(CraftingSlots[0].ID);
                i.Draw(spriteBatch, new Rectangle((int)(MapPosition.X - ItemOffset.X), (int)(MapPosition.Y - ItemOffset.Y), Constants.TILE_SIZE, Constants.TILE_SIZE), true, _sprite.LayerDepth + 1);
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
            if (HoldingItem())
            {
                InventoryManager.AddToInventory(DataManager.CraftItem(CraftingSlots[0].ID));
                CraftingSlots[0].ID = -1;
            }
            else
            {
                GUIManager.OpenMainObject(new HUDCraftingDisplay(this));
            }

            return true;
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
            InventoryManager.AddToInventory(DataManager.CraftItem(CraftingSlots[capacityIndex].ID));
            CraftingSlots[capacityIndex].ID = -1;
            CraftingSlots[capacityIndex].CraftTime = 0;
        }

        public bool SufficientStamina()
        {
            return !CraftDaily && PlayerManager.Stamina >= Constants.ACTION_COST / 2;
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
        public void AttemptToCraftChosenItem(Item itemToCraft)
        {
            bool success = false;
            if (CraftDaily)
            {
                for (int i = 0; i < Capacity; i++)
                {
                    if (CraftingSlots[i].ID == -1)
                    {
                        if (PlayerManager.ExpendResources(itemToCraft.GetRequiredItems()))
                        {
                            success = true;
                            CraftingSlots[i].ID = itemToCraft.ID;
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
                PlayerManager.DecreaseStamina(Constants.ACTION_COST / 2);
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
                data.stringData += Util.SaveInt(CraftingSlots[i].ID) + "-" + CraftingSlots[i].CraftTime + "|";
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
                    CraftingSlots[i].CraftTime = int.Parse(split[1]);
                }
            }
        }

        public struct Recipe
        {
            public int ID;
            public int CraftTime;
        }
    }
}
