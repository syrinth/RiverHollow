﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Characters
{
    public class Child : TalkingActor
    {
        int _iCurrentGrowth = 0;
        private List<int> _liGrowthPeriods;
        List<AnimationData> _liData;

        public enum ChildStageEnum { Newborn, Infant, Toddler };
        private ChildStageEnum _eCurrentStage = ChildStageEnum.Newborn;

        public enum PetStateEnum { Alert, Idle, Leash, Wander };
        private PetStateEnum _eCurrentState = PetStateEnum.Wander;

        private int _iGatherZoneID;

        const double MOVE_COUNTDOWN = 2.5;
        public int ID { get; } = -1;
        private bool _bFollow = false;
        private bool _bIdleCooldown = false;

        private double _dCountdown = 0;

        public Child(int id, Dictionary<string, string> stringData) : base()
        {
            ID = id;
            _eActorType = ActorEnum.Child;

            _liGrowthPeriods = new List<int>() { 4, 10 };

            _sPortrait = Util.GetPortraitLocation(_sPortraitFolder, "Adventurer", id.ToString("00"));
            DataManager.GetTextData("NPC", ID, ref _sName, "Name");

            _liData = new List<AnimationData>();
            AddToAnimationsList(ref _liData, stringData, VerbEnum.Walk);
            AddToAnimationsList(ref _liData, stringData, VerbEnum.Idle);
            AddToAnimationsList(ref _liData, stringData, VerbEnum.Action1);
            LoadSpriteAnimations(ref _sprBody, _liData, _sCreatureFolder + "NPC_" + ID + "_" + (int)_eCurrentStage);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (_bBumpedIntoSomething)
            {
                _bBumpedIntoSomething = false;
                SetMoveObj(Vector2.Zero);
            }

            if (_bFollow && !PlayerManager.PlayerInRange(CollisionBox.Center, TILE_SIZE * 8) && _eCurrentState != PetStateEnum.Leash)
            {
                if (!_sprBody.IsCurrentAnimation(VerbEnum.Action1, Facing))
                {
                    ChangeState(PetStateEnum.Alert);
                }
            }

            switch (_eCurrentState)
            {
                case PetStateEnum.Alert:
                    Alert();
                    break;
                case PetStateEnum.Idle:
                    Idle(gTime);
                    break;
                case PetStateEnum.Wander:
                    Wander(gTime);
                    break;
                case PetStateEnum.Leash:
                    Leash();
                    break;
            }
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

                    LoadSpriteAnimations(ref _sprBody, _liData, _sCreatureFolder + "NPC_" + ID + "_" + (int)_eCurrentStage);
                }
            }
        }

        public override void ProcessRightButtonClick()
        {
            TextEntry text = DataManager.GetGameTextEntry(_bFollow ? "PetUnfollow" : "PetFollow");
            text.FormatText(_sName);
            GUIManager.OpenTextWindow(text, this, true);
        }

        private void Alert()
        {
            if (_sprBody.PlayedOnce)
            {
                ChangeState(PetStateEnum.Leash);
            }
        }

        private void Idle(GameTime gTime)
        {
            if (_dCountdown < 10 + RHRandom.Instance().Next(5)) { _dCountdown += gTime.ElapsedGameTime.TotalSeconds; }
            else
            {
                if (RHRandom.Instance().RollPercent(50))
                {
                    _dCountdown = 0;
                    _bIdleCooldown = true;
                    ChangeState(PetStateEnum.Wander);
                }
            }
        }

        private void Leash()
        {
            Vector2 delta = Position - PlayerManager.PlayerActor.Position;
            HandleMove(Position - delta);

            if (PlayerManager.PlayerInRange(CollisionBox.Center, TILE_SIZE))
            {
                ChangeState(PetStateEnum.Wander);
            }
        }

        private void Wander(GameTime gTime)
        {
            if (_dCountdown < MOVE_COUNTDOWN + (RHRandom.Instance().Next(4) * 0.25)) { _dCountdown += gTime.ElapsedGameTime.TotalSeconds; }
            else if (MoveToLocation == Vector2.Zero)
            {
                _dCountdown = 0;

                if (!_bIdleCooldown && RHRandom.Instance().RollPercent(20))
                {
                    ChangeState(PetStateEnum.Idle);
                    return;
                }

                _bIdleCooldown = false;

                Vector2 moveTo = new Vector2(RHRandom.Instance().Next(8, 32), RHRandom.Instance().Next(8, 32));
                if (RHRandom.Instance().Next(1, 2) == 1) { moveTo.X *= -1; }
                if (RHRandom.Instance().Next(1, 2) == 1) { moveTo.Y *= -1; }

                SetMoveObj(Position + moveTo);
            }

            if (MoveToLocation != Vector2.Zero)
            {
                HandleMove(_vMoveTo);
            }
        }

        public void ChangeState(PetStateEnum state)
        {
            _eCurrentState = state;
            switch (state)
            {
                case PetStateEnum.Alert:
                    PlayAnimation(VerbEnum.Action1, Facing);
                    break;
                case PetStateEnum.Idle:
                    PlayAnimation(VerbEnum.Idle, Facing);
                    break;
                case PetStateEnum.Leash:
                    ChangeMovement(NORMAL_SPEED);
                    break;
                case PetStateEnum.Wander:
                    ChangeMovement(NPC_WALK_SPEED);
                    break;
            }
        }

        private void ChangeMovement(float speed)
        {
            SpdMult = speed;
            PlayAnimation(VerbEnum.Walk, Facing);
            SetMoveObj(Vector2.Zero);
            _dCountdown = 0;
        }

        public void SetFollow(bool value)
        {
            _bFollow = value;
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
                foreach (Vector2 v in Util.GetAllPointsInArea(objLocation.X - (3 * TILE_SIZE), objLocation.Y - (3 * TILE_SIZE), TILE_SIZE * 7, TILE_SIZE * 7, TILE_SIZE))
                {
                    RHTile t = obj.CurrentMap.GetTileByPixelPosition(v);
                    if (t != null && t.Passable() && (t.WorldObject == null || t.WorldObject.Walkable)) { validTiles.Add(t); }
                }

                obj.CurrentMap.AddActor(this);
                Position = Util.GetRandomItem(validTiles).Position;

                ChangeState(PetStateEnum.Wander);
            }
        }

        public void SpawnNearPlayer()
        {
            if (CurrentMap == null) { MapManager.CurrentMap.AddCharacterImmediately(this); }
            else { MapManager.CurrentMap.AddActor(this); }

            List<RHTile> validTiles = new List<RHTile>();
            Point playerLocation = PlayerManager.PlayerActor.CollisionBox.Location;
            foreach (Vector2 v in Util.GetAllPointsInArea(playerLocation.X - (3 * TILE_SIZE), playerLocation.Y - (3 * TILE_SIZE), TILE_SIZE * 7, TILE_SIZE * 7, TILE_SIZE))
            {
                RHTile t = MapManager.CurrentMap.GetTileByPixelPosition(v);
                if (t != null && t.Passable() && (t.WorldObject == null || t.WorldObject.Walkable)) { validTiles.Add(t); }
            }

            Position = Util.GetRandomItem(validTiles).Position;

            ChangeState(PetStateEnum.Wander);
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
