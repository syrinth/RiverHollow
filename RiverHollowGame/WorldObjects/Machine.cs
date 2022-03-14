using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Machine : WorldObject
    {
        readonly string _sEffectWorking = "";

        protected int _iContainingBuildingID = -1;

        protected double _dProcessedTime = 0;
        int _iCurrentlyMaking = -1;

        protected int _iWorkingFrames = 2;
        protected float _fFrameSpeed = 0.3f;

        public Dictionary<int, int> CraftingDictionary { get; }
        private bool _bWorking = false;

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
            if (_iCurrentlyMaking != -1)       //Crafting Handling
            {
                _sprite.Update(gTime);

                _dProcessedTime += gTime.ElapsedGameTime.TotalSeconds;
                // CheckFinishedCrafting();
            }
        }

        public override void ProcessLeftClick() { ClickProcess(); }
        public override void ProcessRightClick() { ClickProcess(); }

        private void ClickProcess()
        {
            if (!MakingSomething())
            {
                GUIManager.OpenMainObject(new HUDCraftingDisplay(this));
            }
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
            if (InventoryManager.HasSpaceInInventory(itemToCraft.ItemID, 1) && PlayerManager.ExpendResources(itemToCraft.GetRequiredItems()))
            {
                double mod = 0;
                if (_iContainingBuildingID != -1)
                {
                    mod = 0.1 * (PlayerManager.GetBuildingByID(_iContainingBuildingID).Level - 1);
                }

                PlayerManager.DecreaseStamina(1 - mod);

                //_iCurrentlyMaking = itemToCraft.ItemID;
                //_sprite.PlayAnimation(CombatAnimationEnum.PlayAnimation);

                InventoryManager.AddToInventory(itemToCraft.ItemID);
                if (!string.IsNullOrEmpty(_sEffectWorking))
                {
                    SoundManager.PlayEffect(_sEffectWorking);
                }
            }
        }

        /// <summary>
        /// OVerride method for Rollover. Shouldn't matter since item crafting should take no time
        /// but for future proofing we'll have this here.
        /// </summary>
        public override void Rollover()
        {
            if (_bWorking)
            {
                _dProcessedTime += GameCalendar.GetMinutesToNextMorning();
                //CheckFinishedCrafting();

                _bWorking = false;
            }
        }

        public bool MakingSomething() { return _iCurrentlyMaking != -1; }

        public override bool PlaceOnMap(Vector2 pos, RHMap map)
        {
            bool rv = false;
            if (base.PlaceOnMap(pos, map))
            {
                rv = true;
                GameManager.AddMachine(this, Name());

                if (map.BuildingID != -1)
                {
                    _iContainingBuildingID = map.BuildingID;
                }
            }

            return rv;
        }

        public MachineData SaveData()
        {
            MachineData m = new MachineData
            {
                ID = this.ID,
                x = (int)this.MapPosition.X,
                y = (int)this.MapPosition.Y,
                processedTime = this._dProcessedTime,
                currentItemID = _iCurrentlyMaking
            };

            return m;
        }
        public void LoadData(MachineData mac)
        {
            _iID = mac.ID;
            SnapPositionToGrid(new Vector2(mac.x, mac.y));
            _dProcessedTime = mac.processedTime;
            _iCurrentlyMaking = mac.currentItemID;

            // if (CurrentlyProcessing != null) { _sprite.PlayAnimation(CombatAnimationEnum.ObjectIdle); }
        }
    }
}
