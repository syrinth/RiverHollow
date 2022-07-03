using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Pet : TalkingActor
    {
        private int _iGatherZoneID;

        public Pet(int id, Dictionary<string, string> stringData) : base(id)
        {
            _fBaseSpeed = 1;
            _eActorType = WorldActorTypeEnum.Pet;
            _bCanWander = true;

            //_sPortrait = Util.GetPortraitLocation(DataManager.PORTRAIT_FOLDER, "Adventurer", stringData["Key"]);

            Util.AssignValue(ref _iGatherZoneID, "ObjectID", stringData);

            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Walk);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Action1);
            LoadSpriteAnimations(ref _sprBody, liData, DataManager.NPC_FOLDER + "NPC_" + stringData["Key"]);
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (_bBumpedIntoSomething)
            {
                _bBumpedIntoSomething = false;
                SetMoveObj(Vector2.Zero);
                ChangeState(NPCStateEnum.Idle);
            }

            if (_bFollow && !PlayerManager.PlayerInRange(CollisionBox.Center, TILE_SIZE * 8) && _eCurrentState != NPCStateEnum.TrackPlayer)
            {
                if (!_sprBody.IsCurrentAnimation(VerbEnum.Action1, Facing))
                {
                    ChangeState(NPCStateEnum.Alert);
                }
            }

            ProcessStateEnum(gTime, true);
        }

        public override void ProcessRightButtonClick()
        {
            //TextEntry text = DataManager.GetGameTextEntry(_bFollow ? "Unfollow" : "PetFollow");
            //text.FormatText(_sName);
            //GUIManager.OpenTextWindow(text, this, true);
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

                ChangeState(NPCStateEnum.Wander);
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

            ChangeState(NPCStateEnum.Wander);
        }
    }
}
