using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Plant : Destructible
    {
        #region consts
        const float MAX_ROTATION = 0.15f;
        const float ROTATION_MOD = 0.02f;
        const float MAX_BOUNCE = 3;
        #endregion

        bool _bShaken = false;
        bool _bNudge = false;
        bool _bShaking = false;
        DirectionEnum dir = DirectionEnum.Right;
        float _fCurrentRotation = 0f;
        int _iBounceCount = 0;

        int _iCurrentState = 0;
        int _iDaysToNextState = -1;

        public int MaxStates => int.Parse(Util.FindParams(GetStringByIDKey("States", "1"))[0]);

        public int HoneyID => GetIntByIDKey("HoneyID");

        readonly bool _bPopItem;

        public Plant(int id) : base(id)
        {
            _bPopItem = false;
            SetGrowthInfo();

            Sprite.SetRotationOrigin(new Vector2(_pSize.X * Constants.TILE_SIZE / 2, (_pSize.Y * Constants.TILE_SIZE) - 1));    //Subtract one to keep it in the bounds of the rectangle
        }

        protected override void LoadSprite()
        {
            var maxStates = MaxStates;
            if (maxStates == 1)
            {
                base.LoadSprite();
            }
            else
            {
                Sprite = new AnimatedSprite(GetStringByIDKey("Texture"));

                var images = GetStringParamsByIDKey("Image");
                for(int i =0; i < MaxStates; i++) { 
                    var size = Util.ParsePoint(GetStringParamsByIDKey("Size")[i]);
                    var point = Util.ParsePoint(images[i]);
                    point = Util.MultiplyPoint(point, Constants.TILE_SIZE);
                    Sprite.AddAnimation(i.ToString(), point.X, point.Y, size);
                }

                SetSpritePos(MapPosition);
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

            if (_bNudge)
            {
                _bNudge = false;
                NudgeObject(true);
            }
        }

        public override bool ProcessLeftClick()
        {
            bool rv = false;
            if (FinishedGrowing() && HP > 0)
            {
                rv = true;
                Harvest();
            }

            return rv;
        }

        public override bool ProcessRightClick()
        {
            bool rv = false;

            if (CanPickUp() && FinishedGrowing() && HP > 0)
            {
                rv = true;
                Harvest();
            }
            else
            {
                if (MaxStates == 1 || _iCurrentState > 0)
                {
                    _bNudge = true;
                }

                if (!_bShaken && FinishedGrowing() && HP > 0 && GetBoolByIDKey("SeedID") && RHRandom.Instance().RollPercent(30))
                {
                    rv = true;
                    MapManager.DropItemOnMap(DataManager.GetItem(GetIntByIDKey("SeedID")), CollisionBox.Location);
                }
                _bShaken = true;
            }

            return rv;
        }

        public void FinishGrowth()
        {
            SetState(MaxStates);
        }
        public void RandomizeState()
        {
            SetState(RHRandom.Instance().Next(0, MaxStates - 1));
        }
        protected void SetState(int val)
        {
            if (val < MaxStates) { _iCurrentState = val; }
            else { _iCurrentState = MaxStates - 1; }

            Sprite.PlayAnimation(_iCurrentState.ToString());

            SetGrowthInfo();
        }
        private void SetGrowthInfo()
        {
            if (MaxStates > 1)
            {
                HP = int.Parse(GetStringParamsByIDKey("Hp")[_iCurrentState]);

                _bWalkable = GetStringParamsByIDKey("Walkable", "0")[_iCurrentState] == "T";
                _bDrawUnder = _bWalkable && GetBoolByIDKey("DrawUnder");
                _iDaysToNextState = int.Parse(GetStringParamsByIDKey("Time", "F")[_iCurrentState]);
                _pSize = Util.ParsePoint(GetStringParamsByIDKey("Size")[_iCurrentState]);
                _rBase = Util.ParseRectangle(GetStringParamsByIDKey("Base")[_iCurrentState]);

                if (Tiles != null && Tiles.Count > 0)
                {
                    var tile = Tiles[0];
                    RemoveSelfFromTiles();
                    PlaceOnMap(tile.Position, CurrentMap);
                }
            }
            else
            {
                _bWalkable = GetBoolByIDKey("Walkable");
            }
        }

        protected override string Loot()
        {
            if (GetBoolByIDKey("ItemID"))
            {
                return GetStringParamsByIDKey("ItemID")[_iCurrentState];
            }
            else { return string.Empty; }
        }

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
                   var items = GetDroppedItems();
                    foreach (var it in items)
                    {
                        if (_bPopItem)
                        {
                            MapManager.DropItemOnMap(it, MapPosition);
                        }
                        else
                        {
                            InventoryManager.AddToInventory(it);
                        }
                    }

                    MapManager.RemoveWorldObject(this);
                    RemoveSelfFromTiles();
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
            _bShaken = false;

            if (_iDaysToNextState > 0) //Decrement the number of days until the next phase
            {
                _iDaysToNextState--;
                if(_iDaysToNextState == 0 && !FinishedGrowing())
                {
                    SetState(++_iCurrentState);
                }
            }

            if(FinishedGrowing() && GetBoolByIDKey("Spread"))
            {
                var spreadParams = Util.FindIntParams(GetStringByIDKey("Spread"));
                if (RHRandom.Instance().RollPercent(spreadParams[0]))
                {
                    var targetTile = Util.GetRandomItem(CurrentMap.GetAllTilesInRange(Tiles[0], spreadParams[1]));
                    if (targetTile != null && targetTile.WorldObject == null && targetTile.Flooring == null)
                    {
                        DataManager.CreateAndPlaceNewWorldObject(ID, targetTile.Position, CurrentMap);
                    }
                }
            }
        }

        /// <summary>
        /// Check if the plant has finished growing or not.
        /// </summary>
        /// <returns>True if it's on the last phase</returns>
        public bool FinishedGrowing() { return _iCurrentState == MaxStates - 1; }

        public override bool CanPickUp()
        {
            return FinishedGrowing() && NeededTool == ToolEnum.None;
        }

        public override bool WideOnTop()
        {
            if (MaxStates > 1)
            {
                var endSize = Util.ParsePoint(GetStringParamsByIDKey("Size")[MaxStates - 1]);
                var endBase = Util.ParseRectangle(GetStringParamsByIDKey("Base")[MaxStates - 1]);

                return endBase.X < endSize.X;
            }
            else
            {
                return base.WideOnTop();
            }
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData += string.Format("/{0}/{1}", _iCurrentState, _iDaysToNextState);

            return data;
        }

        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            string[] strData = Util.FindParams(data.stringData);
            _iCurrentState = int.Parse(strData[1]);
            _iDaysToNextState = int.Parse(strData[2]);

            SetState(_iCurrentState);
        }
    }
}
