using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Child : TalkingActor
    {
        int _iCurrentGrowth = 0;
        private readonly List<int> _liGrowthPeriods;
        readonly List<AnimationData> _liData;

        private ChildStageEnum _eCurrentStage = ChildStageEnum.Newborn;

        private readonly int _iGatherZoneID;

        const double MOVE_COUNTDOWN = 2.5;
        private readonly bool _bIdleCooldown = false;

        private readonly double _dCountdown = 0;

        public Child(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            Wandering = true;
            _eCollisionState = ActorCollisionState.Slow;

            _liGrowthPeriods = new List<int>() { 4, 10 };

            _sPortrait = Util.GetPortraitLocation(DataManager.PORTRAIT_FOLDER, "Adventurer", stringData["Key"]);

            _liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref _liData, stringData, VerbEnum.Walk);
            Util.AddToAnimationsList(ref _liData, stringData, VerbEnum.Idle);
            Util.AddToAnimationsList(ref _liData, stringData, AnimationEnum.Alert);
            BodySprite = LoadSpriteAnimations(_liData, SpriteName() + "_" + (int)_eCurrentStage);
        }

        //public override void Update(GameTime gTime)
        //{
        //    base.Update(gTime);

        //    if (_bBumpedIntoSomething)
        //    {
        //        _bBumpedIntoSomething = false;
        //        ChangeState(NPCStateEnum.Idle);
        //        SetMoveTo(Vector2.Zero);
        //    }

        //    if (_bFollow && !PlayerManager.PlayerInRange(CollisionCenter, Constants.TILE_SIZE * 8) && _eCurrentState != NPCStateEnum.TrackPlayer)
        //    {
        //        if (!_sprBody.IsCurrentAnimation(VerbEnum.Alert, Facing))
        //        {
        //            ChangeState(NPCStateEnum.Alert);
        //        }
        //    }

        //    ProcessStateEnum(gTime, true);
        //}

        public void Rollover()
        {
            if (_eCurrentStage != ChildStageEnum.Toddler)
            {
                _iCurrentGrowth++;

                if (_iCurrentGrowth == _liGrowthPeriods[(int)_eCurrentStage])
                {
                    _iCurrentGrowth = 0;
                    _eCurrentStage = _eCurrentStage + 1;

                    BodySprite = LoadSpriteAnimations(_liData, SpriteName() + "_" + (int)_eCurrentStage);
                }
            }
        }

        public override void ProcessRightButtonClick()
        {
            GUIManager.OpenTextWindow(_bFollow ? "PetUnfollow" : "PetFollow", Name, this, true);
        }

        public void SpawnInHome()
        {
            WorldObject obj = Util.GetRandomItem(TownManager.GetTownObjectsByID(_iGatherZoneID));
            if (obj == null)
            {
                if (CurrentMap == null) { MapManager.TownMap.AddCharacterImmediately(this); }
                else { MapManager.TownMap.AddActor(this); }
                SetPosition(Util.GetRandomItem(MapManager.TownMap.FindFreeTiles()).Position);
            }
            else
            {
                List<RHTile> validTiles = new List<RHTile>();
                Point objLocation = obj.BaseRectangle.Location;
                foreach (Point p in Util.GetAllPointsInArea(objLocation.X - (3 * Constants.TILE_SIZE), objLocation.Y - (3 * Constants.TILE_SIZE), Constants.TILE_SIZE * 7, Constants.TILE_SIZE * 7, Constants.TILE_SIZE))
                {
                    RHTile t = obj.CurrentMap.GetTileByPixelPosition(p);
                    if (t != null && t.Passable() && (t.WorldObject == null || t.WorldObject.Walkable)) { validTiles.Add(t); }
                }

                obj.CurrentMap.AddActor(this);
                SetPosition(Util.GetRandomItem(validTiles).Position);

                ChangeState(NPCStateEnum.Wander);
            }
        }

        public void SpawnNearPlayer()
        {
            if (CurrentMap == null) { MapManager.CurrentMap.AddCharacterImmediately(this); }
            else { MapManager.CurrentMap.AddActor(this); }

            List<RHTile> validTiles = new List<RHTile>();
            Point playerLocation = PlayerManager.PlayerActor.CollisionBox.Location;
            foreach (Point p in Util.GetAllPointsInArea(playerLocation.X - (3 * Constants.TILE_SIZE), playerLocation.Y - (3 * Constants.TILE_SIZE), Constants.TILE_SIZE * 7, Constants.TILE_SIZE * 7, Constants.TILE_SIZE))
            {
                RHTile t = MapManager.CurrentMap.GetTileByPixelPosition(p);
                if (t != null && t.Passable() && (t.WorldObject == null || t.WorldObject.Walkable)) { validTiles.Add(t); }
            }

            SetPosition(Util.GetRandomItem(validTiles).Position);

            ChangeState(NPCStateEnum.Wander);
        }

        public ChildData SaveData()
        {
            ChildData data = new ChildData()
            {
                childID = ID,
                stageEnum = (int)_eCurrentStage,
                growthTime = _iCurrentGrowth
            };

            return data;
        }

        public void LoadData(ChildData data)
        {
            _eCurrentStage = (ChildStageEnum)data.stageEnum;
            _iCurrentGrowth = data.growthTime;
        }
    }
}
