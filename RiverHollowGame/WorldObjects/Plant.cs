﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.WorldObjects
{
    public class Plant : Destructible
    {
        #region consts
        const float MAX_ROTATION = 0.15f;
        const float ROTATION_MOD = 0.02f;
        const float MAX_BOUNCE = 3;
        #endregion

        bool _bShaking = false;
        DirectionEnum dir = DirectionEnum.Right;
        float _fCurrentRotation = 0f;
        int _iBounceCount = 0;

        readonly bool _bPopItem;
        int _iCurrentState;
        readonly int _iMaxStates;
        readonly int _iResourceID;
        int _iDaysLeft;
        Dictionary<int, int> _diTransitionTimes;
        int _iSeedID = 0;
        public int SeedID => _iSeedID;

        int _iHoneyID = -1;
        public int HoneyID => _iHoneyID;

        Garden _objGarden;
        SeasonEnum _eSeason;

        public Plant(int id, Dictionary<string, string> stringData) : base(id, stringData, false)
        {
            _diTransitionTimes = new Dictionary<int, int>();

            LoadDictionaryData(stringData);

            _bWalkable = true;

            _iCurrentState = 0;
            _rBase.Y = _pSize.Y - 1;

            Util.AssignValue(ref _eSeason, "Season", stringData);
            _iHoneyID = Util.AssignValue("HoneyID", stringData);
            _iSeedID =  Util.AssignValue("SeedID", stringData);
            _iResourceID = Util.AssignValue("ItemID", stringData);
            _iMaxStates = Util.AssignValue("TrNum", stringData); //Number of growth phases

            _bPopItem = false;

            //The amount of time for each phase
            string[] dayStr = stringData["TrTime"].Split('-');
            for (int j = 0; j < _iMaxStates - 1; j++)
            {
                _diTransitionTimes.Add(j, int.Parse(dayStr[j]));
                Sprite.AddAnimation((j + 1).ToString(), _pImagePos.X + (Constants.TILE_SIZE * (j + 1)), _pImagePos.Y, _pSize);
            }
            _iDaysLeft = _diTransitionTimes[0];

            if (stringData.ContainsKey("DestructionAnim"))
            {
                string[] splitString = stringData["DestructionAnim"].Split('-');
                Sprite.AddAnimation(AnimationEnum.KO, int.Parse(splitString[0]), int.Parse(splitString[1]), _pSize, int.Parse(splitString[2]), float.Parse(splitString[3]), false, true);
            }

            Sprite.SetRotationOrigin(new Vector2(_pSize.X * Constants.TILE_SIZE / 2, (_pSize.Y * Constants.TILE_SIZE) - 1));    //Subtract one to keep it in the bounds of the rectangle
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            Sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
            Sprite.AddAnimation(0.ToString(), (int)_pImagePos.X, (int)_pImagePos.Y, _pSize);
            for (int j = 1; j < _diTransitionTimes.Count + 1; j++)
            {
                Sprite.AddAnimation(j.ToString(), (int)_pImagePos.X + (Constants.TILE_SIZE * j), (int)_pImagePos.Y, _pSize);
            }
        }

        public override void Update(GameTime gTime)
        {
            //If the object is shaking, we need to determine what step it's in
            if (_bShaking)
            {
                if (dir == DirectionEnum.Right) { _fCurrentRotation += ROTATION_MOD; }
                else if (dir == DirectionEnum.Left) { _fCurrentRotation -= ROTATION_MOD; }

                Sprite.SetRotationAngle(_fCurrentRotation);

                //If we've reached the end of our bounce, increment the bounce count
                //and set us to just below the trigger value for the statement we just hit.
                if (_iBounceCount == MAX_BOUNCE && _fCurrentRotation >= -ROTATION_MOD && _fCurrentRotation <= ROTATION_MOD)
                {
                    _bShaking = false;
                    _iBounceCount = 0;
                }
                else if (_fCurrentRotation >= MAX_ROTATION)
                {
                    dir = DirectionEnum.Left;
                    _iBounceCount++;
                    _fCurrentRotation = MAX_ROTATION - ROTATION_MOD;
                }
                else if (_fCurrentRotation <= -MAX_ROTATION)
                {
                    dir = DirectionEnum.Right;
                    _iBounceCount++;
                    _fCurrentRotation = -MAX_ROTATION + ROTATION_MOD;
                }
            }

            base.Update(gTime);
        }

        public override bool ProcessLeftClick()
        {
            bool rv = false;

            if (FinishedGrowing() && _iHP > 0)
            {
                Harvest();
                rv = true;
            }

            return rv;
        }
        //public override void ProcessRightClick() { Harvest(); }

        /// <summary>
        /// Call to tell the plant that it is being Harvested, and follow any logic
        /// that needs to happen for this to occur.
        /// 
        /// Can only Harvest plants that are finished growing.
        /// </summary>
        public void Harvest()
        {
            if (NeededTool != ToolEnum.None) { }// PlayerManager.SetTool(PlayerManager.RetrieveTool(NeededTool)); }
            else
            {
                if (FinishedGrowing() && PlayerManager.LoseEnergy(Constants.ACTION_COST))
                {
                    Item it = DataManager.GetItem(_iResourceID);
                    if (_bPopItem)
                    {
                        MapManager.DropItemOnMap(it, MapPosition);
                    }
                    else
                    {
                        InventoryManager.AddToInventory(it);
                    }

                    MapManager.RemoveWorldObject(this);
                    RemoveSelfFromTiles();
                    _objGarden.SetPlant(null);
                }
            }
        }

        /// <summary>
        /// Tell the object to shake
        /// </summary>
        public void Shake()
        {
            if (!_bShaking)
            {
                if (PlayerManager.PlayerActor.CollisionCenter.X > CollisionCenter.X) { dir = DirectionEnum.Left; }
                else if (PlayerManager.PlayerActor.CollisionCenter.X < CollisionCenter.X) { dir = DirectionEnum.Right; }
                _bShaking = true;
            }
        }

        /// <summary>
        /// On rollover, increase the plant's growth cycle if it has been watered.
        /// </summary>
        public override void Rollover()
        {
            if (_iDaysLeft > 0) //Decrement the number of days until the next phase
            {
                _iDaysLeft--;
            }
            else if (!FinishedGrowing()) //If it hasn't finished growing, and there's no days left, go to the next phase
            {
                _iCurrentState++;
                Sprite.PlayAnimation(_iCurrentState.ToString());
                if (_diTransitionTimes.ContainsKey(_iCurrentState))
                {
                    _iDaysLeft = _diTransitionTimes[_iCurrentState];
                }
            }
            else if (_objGarden == null)
            {
                CurrentMap.AddLights(GetLights());
            }
        }

        public bool InSeason() { return Util.GetEnumString(_eSeason).Equals(GameCalendar.GetCurrentSeason()); }

        /// <summary>
        /// Check if the plant has finished growing or not.
        /// </summary>
        /// <returns>True if it's on the last phase</returns>
        public bool FinishedGrowing() { return _iCurrentState == _iMaxStates - 1; }

        public void FinishGrowth()
        {
            _iCurrentState = _iMaxStates - 1;
            //_rSource.X += _iWidth * _iCurrentState;
        }

        public override bool CanPickUp()
        {
            return FinishedGrowing();
        }

        public void SetGarden(Garden g)
        {
            _objGarden = g;
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData += _iCurrentState + "|";
            data.stringData += _iDaysLeft;

            return data;
        }

        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            string[] strData = Util.FindParams(data.stringData);
            _iCurrentState = int.Parse(strData[0]);
            _iDaysLeft = int.Parse(strData[1]);

            Sprite.PlayAnimation(_iCurrentState.ToString());
        }
    }
}
