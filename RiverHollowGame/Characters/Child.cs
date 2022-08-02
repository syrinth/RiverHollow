using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Child : TalkingActor
    {
        int _iCurrentGrowth = 0;
        private List<int> _liGrowthPeriods;
        List<AnimationData> _liData;

        private ChildStageEnum _eCurrentStage = ChildStageEnum.Newborn;

        private int _iGatherZoneID;

        const double MOVE_COUNTDOWN = 2.5;
        public int ID { get; } = -1;
        private bool _bFollow = false;
        private bool _bIdleCooldown = false;

        private double _dCountdown = 0;

        public Child(int id, Dictionary<string, string> stringData) : base(id)
        {
            ID = id;
            _eActorType = WorldActorTypeEnum.Child;
            _bCanWander = true;

            _liGrowthPeriods = new List<int>() { 4, 10 };

            _sPortrait = Util.GetPortraitLocation(DataManager.PORTRAIT_FOLDER, "Adventurer", stringData["Key"]);

            _liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref _liData, stringData, VerbEnum.Walk);
            Util.AddToAnimationsList(ref _liData, stringData, VerbEnum.Idle);
            Util.AddToAnimationsList(ref _liData, stringData, VerbEnum.Alert);
            LoadSpriteAnimations(ref _sprBody, _liData, DataManager.NPC_FOLDER + "NPC_" + ID + "_" + (int)_eCurrentStage);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (_bBumpedIntoSomething)
            {
                _bBumpedIntoSomething = false;
                MoveToLocation = Vector2.Zero;
                ChangeState(NPCStateEnum.Idle);
            }

            if (_bFollow && !PlayerManager.PlayerInRange(CollisionBox.Center, Constants.TILE_SIZE * 8) && _eCurrentState != NPCStateEnum.TrackPlayer)
            {
                if (!_sprBody.IsCurrentAnimation(VerbEnum.Alert, Facing))
                {
                    ChangeState(NPCStateEnum.Alert);
                }
            }

            ProcessStateEnum(gTime, true);
        }

        public void Rollover()
        {
            if (_eCurrentStage != ChildStageEnum.Toddler)
            {
                _iCurrentGrowth++;

                if (_iCurrentGrowth == _liGrowthPeriods[(int)_eCurrentStage])
                {
                    _iCurrentGrowth = 0;
                    _eCurrentStage = _eCurrentStage + 1;

                    LoadSpriteAnimations(ref _sprBody, _liData, DataManager.NPC_FOLDER + "NPC_" + ID + "_" + (int)_eCurrentStage);
                }
            }
        }

        public override void ProcessRightButtonClick()
        {
            TextEntry text = DataManager.GetGameTextEntry(_bFollow ? "PetUnfollow" : "PetFollow");
            text.FormatText(Name());
            GUIManager.OpenTextWindow(text, this, true);
        }

        public void SpawnInHome()
        {
            WorldObject obj = Util.GetRandomItem(PlayerManager.GetTownObjectsByID(_iGatherZoneID));
            if (obj == null)
            {
                if (CurrentMap == null) { MapManager.TownMap.AddCharacterImmediately(this); }
                else { MapManager.TownMap.AddActor(this); }
                Position = Util.GetRandomItem(MapManager.TownMap.FindFreeTiles()).Position;
            }
            else
            {
                List<RHTile> validTiles = new List<RHTile>();
                Point objLocation = obj.CollisionBox.Location;
                foreach (Vector2 v in Util.GetAllPointsInArea(objLocation.X - (3 * Constants.TILE_SIZE), objLocation.Y - (3 * Constants.TILE_SIZE), Constants.TILE_SIZE * 7, Constants.TILE_SIZE * 7, Constants.TILE_SIZE))
                {
                    RHTile t = obj.CurrentMap.GetTileByPixelPosition(v);
                    if (t != null && t.Passable() && (t.WorldObject == null || t.WorldObject.Walkable)) { validTiles.Add(t); }
                }

                obj.CurrentMap.AddActor(this);
                Position = Util.GetRandomItem(validTiles).Position;

                ChangeState(NPCStateEnum.Wander);
            }
        }

        public void SpawnNearPlayer()
        {
            if (CurrentMap == null) { MapManager.CurrentMap.AddCharacterImmediately(this); }
            else { MapManager.CurrentMap.AddActor(this); }

            List<RHTile> validTiles = new List<RHTile>();
            Point playerLocation = PlayerManager.PlayerActor.CollisionBox.Location;
            foreach (Vector2 v in Util.GetAllPointsInArea(playerLocation.X - (3 * Constants.TILE_SIZE), playerLocation.Y - (3 * Constants.TILE_SIZE), Constants.TILE_SIZE * 7, Constants.TILE_SIZE * 7, Constants.TILE_SIZE))
            {
                RHTile t = MapManager.CurrentMap.GetTileByPixelPosition(v);
                if (t != null && t.Passable() && (t.WorldObject == null || t.WorldObject.Walkable)) { validTiles.Add(t); }
            }

            Position = Util.GetRandomItem(validTiles).Position;

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
