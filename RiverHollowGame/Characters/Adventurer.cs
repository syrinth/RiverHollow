using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Characters
{
    public class Adventurer : ClassedCombatant
    {
        #region Properties
        private enum AdventurerStateEnum { Idle, InParty, OnMission, AddToParty };
        private AdventurerStateEnum _eState;
        public AdventurerTypeEnum WorkerType { get; private set; }
        protected int _iPersonalID;
        public int PersonalID { get => _iPersonalID; }
        protected int _iID;
        public int WorkerID { get => _iID; }
        protected string _sAdventurerType;
        public Building Building { get; private set; }
        protected int _iDailyFoodReq;
        protected int _iCurrFood;
        protected int _iDailyItemID;
        public int DailyItemID => _iDailyItemID;
        protected Item _heldItem;
        protected int _iMood;
        private int _iResting;
        public int Mood { get => _iMood; }
        public Mission CurrentMission { get; private set; }

        public override bool OnTheMap => _eState == AdventurerStateEnum.Idle;
        #endregion

        public Adventurer(Dictionary<string, string> data, int id)
        {
            _iID = id;
            _iPersonalID = PlayerManager.GetTotalWorkers();
            //_eActorType = ActorEnum.Adventurer;
            ImportBasics(data, id);

            SetClass(DataManager.GetClassByIndex(_iID));
            AssignStartingGear();
            _sAdventurerType = CharacterClass.Name;

            _iCurrFood = 0;
            _heldItem = null;
            _iMood = 0;

            _eState = AdventurerStateEnum.Idle;

            _sName = _sAdventurerType.Substring(0, 1);
        }

        protected void ImportBasics(Dictionary<string, string> data, int id)
        {
            _iID = id;

            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Adventurer", id.ToString("00"));
            //_sPortrait = _sPortraitFolder + "WizardPortrait";

            WorkerType = Util.ParseEnum<AdventurerTypeEnum>(data["Type"]);
            _iDailyItemID = int.Parse(data["ItemID"]);
            _iDailyFoodReq = int.Parse(data["Food"]);

            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(data), _sMerchantFolder + "Adventurer_" + _iID);
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_eState == AdventurerStateEnum.Idle || _eState == AdventurerStateEnum.AddToParty || (TacticalCombatManager.InCombat && _eState == AdventurerStateEnum.InParty))
            {
                base.Draw(spriteBatch, useLayerDepth);
                if (_heldItem != null)
                {
                    _heldItem.Draw(spriteBatch, new Rectangle(Position.ToPoint(), new Point(32, 32)), true);
                }
            }
        }

        public override bool CollisionContains(Point mouse)
        {
            bool rv = false;
            if (_eState == AdventurerStateEnum.Idle)
            {
                rv = base.CollisionContains(mouse);
            }
            return rv;
        }
        public override bool CollisionIntersects(Rectangle rect)
        {
            bool rv = false;
            if (_eState == AdventurerStateEnum.Idle)
            {
                rv = base.CollisionIntersects(rect);
            }
            return rv;
        }

        //public override string GetDialogEntry(string entry)
        //{
        //    return Util.ProcessText(DataManager.GetAdventurerDialogue(_iID, entry));
        //}

        public override TextEntry GetOpeningText()
        {
            return new TextEntry();//Name + ": " + DataManager.GetAdventurerDialogue(_iID, "Selection");
        }

        public override void StopTalking()
        {
            if (_eState == AdventurerStateEnum.AddToParty)
            {
                _eState = AdventurerStateEnum.InParty;
                PlayerManager.AddToParty(this);
            }
        }

        public int TakeItem()
        {
            int giveItem = -1;
            if (_heldItem != null)
            {
                giveItem = _heldItem.ItemID;
                _heldItem = null;
            }
            return giveItem;
        }

        public int WhatAreYouHolding()
        {
            if (_heldItem != null)
            {
                return _heldItem.ItemID;
            }
            return -1;
        }

        public void SetBuilding(Building b)
        {
            Building = b;
        }

        /// <summary>
        /// Called on rollover, if the WorldAdventurer is in a rest state, subtract one
        /// from the int. If they are currently on a mission, but the mission has been 
        /// completed by the MissionManager's rollover method, reset the state to idle,
        /// null the mission, and set _iResting to be half of the runtime of the Mission.
        /// </summary>
        /// <returns>True if the WorldAdventurer should make their daily item.</returns>
        public bool Rollover()
        {
            bool rv = false;

            if (_iResting > 0) { _iResting--; }

            switch (_eState)
            {
                case AdventurerStateEnum.Idle:
                    _iCurrentHP = MaxHP;
                    rv = true;
                    break;
                case AdventurerStateEnum.InParty:
                    if (GameManager.AutoDisband)
                    {
                        _eState = AdventurerStateEnum.Idle;
                    }
                    break;
                case AdventurerStateEnum.OnMission:
                    if (CurrentMission.Completed())
                    {
                        _eState = AdventurerStateEnum.Idle;
                        _iResting = CurrentMission.DaysToComplete / 2;
                        CurrentMission = null;
                    }
                    break;
            }

            return rv;
        }

        /// <summary>
        /// Creates the worers daily item in the inventory of the building's container.
        /// Need to set the InventoryManager to look at it, then clear it.
        /// </summary>
        public void MakeDailyItem()
        {
            if (_iDailyItemID != -1)
            {
                InventoryManager.InitContainerInventory(Building.BuildingChest.Inventory);
                InventoryManager.AddToInventory(_iDailyItemID, 1, false);
                InventoryManager.ClearExtraInventory();
            }
        }

        public string GetName()
        {
            return _sName;
        }

        /// <summary>
        /// Assigns the WorldAdventurer to the given mission.
        /// </summary>
        /// <param name="m">The mission they are on</param>
        public void AssignToMission(Mission m)
        {
            CurrentMission = m;
            _eState = AdventurerStateEnum.OnMission;
        }

        /// <summary>
        /// Cancels the indicated mission, returning the adventurer to their
        /// home building. Does not get called unless a mission has been canceled.
        /// </summary>
        public void EndMission()
        {
            _iResting = CurrentMission.DaysToComplete / 2;
            CurrentMission = null;
        }

        /// <summary>
        /// Gets a string representation of the WorldAdventurers current state
        /// </summary>
        public string GetStateText()
        {
            string rv = string.Empty;

            switch (_eState)
            {
                case AdventurerStateEnum.Idle:
                    rv = "Idle";
                    break;
                case AdventurerStateEnum.InParty:
                    rv = "In Party";
                    break;
                case AdventurerStateEnum.OnMission:
                    rv = "On Mission \"" + CurrentMission.Name + "\" days left: " + (CurrentMission.DaysToComplete - CurrentMission.DaysFinished).ToString();
                    break;
            }

            return rv;
        }

        /// <summary>
        /// WorldAdventurers are only available for missions if they're not on
        /// a mission and they are not currently in a resting state.
        /// </summary>
        public bool AvailableForMissions()
        {
            return (_eState != AdventurerStateEnum.OnMission && _iResting == 0);
        }

        public WorkerData SaveAdventurerData()
        {
            WorkerData workerData = new WorkerData
            {
                workerID = this.WorkerID,
                PersonalID = this.PersonalID,
                advData = base.SaveClassedCharData(),
                mood = this.Mood,
                name = this.Name,
                heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID,
                state = (int)_eState
            };

            return workerData;
        }
        public void LoadAdventurerData(WorkerData data)
        {
            _iID = data.workerID;
            _iPersonalID = data.PersonalID;
            _iMood = data.mood;
            _sName = data.name;
            _heldItem = DataManager.GetItem(data.heldItemID);
            _eState = (AdventurerStateEnum)data.state;

            base.LoadClassedCharData(data.advData);

            if (_eState == AdventurerStateEnum.InParty)
            {
                PlayerManager.AddToParty(this);
            }
        }
    }
}
