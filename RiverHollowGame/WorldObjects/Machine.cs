﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using RiverHollow.Buildings;

namespace RiverHollow.WorldObjects
{
    public class Machine : WorldObject
    {
        readonly string _sEffectWorking = "";

        int _iCurrentlyMaking = -1;
        private bool CraftDaily => DataManager.GetBoolByIDKey(ID, "Daily", DataType.WorldObject);

        protected int _iDaysLeft = 0;
        protected int _iWorkingFrames = 2;
        protected float _fFrameSpeed = 0.3f;

        public Dictionary<int, int> CraftingDictionary { get; }

        public bool MakingSomething() { return _iCurrentlyMaking != -1; }

        public Machine(int id, Dictionary<string, string> stringData) : base(id)
        {
            if (stringData.ContainsKey("WorkAnimation"))
            {
                string[] split = stringData["WorkAnimation"].Split('-');
                _iWorkingFrames = int.Parse(split[0]);
                _fFrameSpeed = float.Parse(split[1]);
            }

            Util.AssignValue(ref _sEffectWorking, "WorkEffect", stringData);

            CraftingDictionary = new Dictionary<int, int>();
            if (stringData.ContainsKey("Makes"))
            {
                //Read in what items the machine can make
                string[] processes = Util.FindParams(stringData["Makes"]);
                foreach (string recipe in processes)
                {
                    //Each entry is in written like ID-NumDays
                    string[] pieces = recipe.Split('-');
                    CraftingDictionary.Add(int.Parse(pieces[0]), int.Parse(pieces[1]));
                }
            }

            LoadDictionaryData(stringData);
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = "Textures\\texMachines")
        {
            _sprite = new AnimatedSprite(@"Textures\texMachines");
            _sprite.AddAnimation(AnimationEnum.ObjectIdle, (int)_pImagePos.X, (int)_pImagePos.Y, _uSize, 1, 0.3f, false);
            _sprite.AddAnimation(AnimationEnum.PlayAnimation, (int)_pImagePos.X + _uSize.Width, (int)_pImagePos.Y, _uSize, _iWorkingFrames, _fFrameSpeed, false);
            _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
            _sprite.Drawing = true;

            SetSpritePos(_vMapPosition);
        }

        public override void Update(GameTime gTime)
        {
            if (MakingSomething())       //Crafting Handling
            {
                _sprite.Update(gTime);
            }
        }

        /// <summary>
        /// Override method for Rollover. Shouldn't matter since item crafting should take no time
        /// but for future proofing we'll have this here.
        /// </summary>
        public override void Rollover()
        {
            if (MakingSomething())
            {
                _iDaysLeft--;
                if (_iDaysLeft == 0)
                {
                    if(CurrentMap.BuildingID != -1)
                    {
                        Building b = PlayerManager.GetBuildingByID(CurrentMap.BuildingID);
                        b.AddToStock(DataManager.CraftItem(_iCurrentlyMaking));
                    }

                    _iCurrentlyMaking = -1;
                }
            }
        }

        public override bool ProcessLeftClick() { return ClickProcess(); }
        public override bool ProcessRightClick() { return ClickProcess(); }

        private bool ClickProcess()
        {
            bool rv = false;

            if (!MakingSomething())
            {
                rv = true;
                GUIManager.OpenMainObject(new HUDCraftingDisplay(this));
            }

            return rv;
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
            if (InventoryManager.HasSpaceInInventory(itemToCraft.ID, 1) && PlayerManager.ExpendResources(itemToCraft.GetRequiredItems()))
            {
                if (!CraftDaily)
                {
                    PlayerManager.DecreaseStamina(Constants.ACTION_COST);
                    InventoryManager.AddToInventory(itemToCraft.ID, itemToCraft.Number);
                }
                else
                {
                    _iCurrentlyMaking = itemToCraft.ID;
                    _iDaysLeft = DataManager.GetIntByIDKey(_iCurrentlyMaking, "CraftTime", DataType.Item, 1);
                    GUIManager.CloseMainObject();
                }
                //_sprite.PlayAnimation(CombatAnimationEnum.PlayAnimation);

                
                if (!string.IsNullOrEmpty(_sEffectWorking))
                {
                    SoundManager.PlayEffect(_sEffectWorking);
                }
            }
        }

        public override bool PlaceOnMap(Vector2 pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = false;
            if (base.PlaceOnMap(pos, map))
            {
                rv = true;
                GameManager.AddMachine(this, Name());
            }

            return rv;
        }

        /// <summary>
        /// Not currently used
        /// </summary>
        //private void CheckFinishedCrafting()
        //{
        //    if (_iCurrentlyMaking != -1 && _dProcessedTime >= CraftingDictionary[_iCurrentlyMaking])
        //    {
        //        InventoryManager.AddToInventory(_iCurrentlyMaking);
        //        SoundManager.StopEffect(this);
        //        SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition, this);
        //        _dProcessedTime = 0;
        //        _iCurrentlyMaking = -1;
        //        _sprite.PlayAnimation(CombatAnimationEnum.ObjectIdle);
        //    }
        //}

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData += _iDaysLeft + "|";
            data.stringData += _iCurrentlyMaking;

            return data;
        }
        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            string[] strData = Util.FindParams(data.stringData);

            _iDaysLeft = int.Parse(strData[0]);
            _iCurrentlyMaking = int.Parse(strData[1]);
        }
    }
}
