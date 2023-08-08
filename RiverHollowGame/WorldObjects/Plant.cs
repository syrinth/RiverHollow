using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Plant : Destructible
    {
        #region consts
        const float MAX_ROTATION = 0.06f;
        const float ROTATION_MOD = 0.004f;
        const float MAX_BOUNCE = 3;
        #endregion

        bool _bShaken = false;
        bool _bNudge = false;
        bool _bShaking = false;
        DirectionEnum dir = DirectionEnum.Right;
        float _fCurrentRotation = 0f;
        int _iBounceCount = 0;

        public int CurrentState { get; private set; } = 0;
        int _iDaysToNextState = -1;

        public int MaxStates => int.Parse(Util.FindParams(GetStringByIDKey("States", "1"))[0]);

        public int HoneyID => GetIntByIDKey("HoneyID");
        public bool NeedsWatering => !GetBoolByIDKey("NoWater");

        readonly bool _bPopItem;

        public Plant(int id) : base(id)
        {
            _bPopItem = false;

            SetGrowthInfo();
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
                if (GetBoolByIDKey("Texture")) { Sprite = new AnimatedSprite(DataManager.FOLDER_WORLDOBJECTS + GetStringByIDKey("Texture")); }
                else { Sprite = new AnimatedSprite(DataManager.FILE_PLANTS); }

                var images = GetStringParamsByIDKey("Image");
                for (int i = 0; i < MaxStates; i++)
                {
                    var size = Util.ParsePoint(GetStringParamsByIDKey("Size")[i]);
                    var point = Util.ParsePoint(images[i]);
                    point = Util.MultiplyPoint(point, Constants.TILE_SIZE);
                    Sprite.AddAnimation(i.ToString(), point.X, point.Y, size);
                }

                if (GetBoolByIDKey("SpriteOffset"))
                {
                    Sprite.TrimBy(GetIntByIDKey("SpriteOffset", 0));
                }

                if (GetBoolByIDKey("FakeHeight"))
                {
                    Sprite.FakeHeight = GetIntByIDKey("FakeHeight", 0);
                }

                SetSpritePos(MapPosition);
            }
        }

        public override void Update(GameTime gTime)
        {
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
                if (!_bShaken && FinishedGrowing() && HP > 0 && GetBoolByIDKey("SeedID") && RHRandom.Instance().RollPercent(30))
                {
                    rv = true;
                    MapManager.DropItemOnMap(DataManager.GetItem(GetIntByIDKey("SeedID")), CollisionBox.Location);
                }

                if (!Walkable)
                {
                    _bNudge = true;
                    _bShaken = true;
                }
            }

            return rv;
        }

        /// <summary>
        /// On rollover, increase the plant's growth cycle if it has been watered.
        /// </summary>
        public override void Rollover()
        {
            _bShaken = false;

            if (!NeedsWatering ||Tiles[0].Watered)
            {

                if (_iDaysToNextState > 0) //Decrement the number of days until the next phase
                {
                    _iDaysToNextState--;
                    if (_iDaysToNextState == 0 && !FinishedGrowing())
                    {
                        SetState(++CurrentState);
                    }
                }

                if (FinishedGrowing() && GetBoolByIDKey("Spread"))
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
        }

        public void InitiatePlantShake()
        {
            if (!_bShaking && !_bDrawUnder && _bWalkable)
            {
                _bShaking = true;
            }
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
            if (val < MaxStates) { CurrentState = val; }
            else { CurrentState = MaxStates - 1; }

            Sprite.PlayAnimation(CurrentState.ToString());

            SetGrowthInfo();
        }
        private void SetGrowthInfo()
        {
            if (MaxStates > 1)
            {
                if (GetBoolByIDKey("Hp")) { HP = int.Parse(GetStringParamsByIDKey("Hp")[CurrentState]); }
                else { HP = 1; }

                _bWalkable = GetStringParamsByIDKey("Walkable", "0")[CurrentState] == "T";
                _bDrawUnder = GetStringParamsByIDKey("DrawUnder", "0")[CurrentState] == "T";
                _pSize = Util.ParsePoint(GetStringParamsByIDKey("Size")[CurrentState]);
                _rBase = Util.ParseRectangle(GetStringParamsByIDKey("Base")[CurrentState]);
                if (GetBoolByIDKey("BaseOffset"))
                {
                    _pSpriteOffset = Util.ParsePoint(GetStringParamsByIDKey("BaseOffset")[CurrentState]);
                }

                if (CurrentState < MaxStates - 1) { _iDaysToNextState = int.Parse(GetStringParamsByIDKey("Time", "F")[CurrentState]); }
                else { _iDaysToNextState = - 1; }

                if (Tiles != null && Tiles.Count > 0)
                {
                    var tile = Tiles[0];
                    RemoveSelfFromTiles();
                    PlaceOnMap(tile.Position, CurrentMap);
                }

                Sprite.SetRotationOrigin(new Vector2(_pSize.X * Constants.TILE_SIZE / 2, (_pSize.Y * Constants.TILE_SIZE) - 1));    //Subtract one to keep it in the bounds of the rectangle
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
                return GetStringParamsByIDKey("ItemID")[CurrentState];
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
            if (NeededTool == ToolEnum.None)
            {
                if (FinishedGrowing())
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
                }
            }
        }

        /// <summary>
        /// Check if the plant has finished growing or not.
        /// </summary>
        /// <returns>True if it's on the last phase</returns>
        public bool FinishedGrowing() { return CurrentState == MaxStates - 1; }

        public override bool CanPickUp()
        {
            return FinishedGrowing() && NeededTool == ToolEnum.None;
        }

        public override bool WideOnTop()
        {
            if(_pSpriteOffset != Point.Zero)
            {
                return false;
            }
            else if (MaxStates > 1)
            {
                int maxState = MaxStates - 1;
                Point offset = Point.Zero;

                if (GetBoolByIDKey("BaseOffset"))
                {
                    offset = Util.ParsePoint(GetStringParamsByIDKey("BaseOffset")[maxState]);
                }

                if (offset == Point.Zero)
                {
                    var param = GetStringParamsByIDKey("Size");
                    var endSize = Util.ParsePoint(param[maxState]);
                    var endBase = Util.ParseRectangle(GetStringParamsByIDKey("Base")[maxState]);

                    return endBase.Size.X < endSize.X;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return base.WideOnTop();
            }
        }

        public override WorldObjectData SaveData()
        {
            WorldObjectData data = base.SaveData();
            data.stringData += string.Format("/{0}/{1}", CurrentState, _iDaysToNextState);

            return data;
        }

        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            string[] strData = Util.FindParams(data.stringData);
            CurrentState = int.Parse(strData[1]);
            _iDaysToNextState = int.Parse(strData[2]);

            SetState(CurrentState);
        }
    }
}
