using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Pet : TalkingActor
    {
        private int GatherZoneID => GetIntByIDKey("ObjectID");

        public Pet(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _fBaseSpeed = 1;
            Wandering = true;
            _eCollisionState = ActorCollisionState.Slow;

            //_sPortrait = Util.GetPortraitLocation(DataManager.PORTRAIT_FOLDER, "Adventurer", stringData["Key"]);

            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Walk);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            Util.AddToAnimationsList(ref liData, stringData, AnimationEnum.Alert, true);
            BodySprite = LoadSpriteAnimations(liData, SpriteName());
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
        }

        public override void ProcessRightButtonClick()
        {
            if (TownManager.PetCafe != null && CurrentMap == TownManager.PetCafe.InnerMap)
            {
                TextEntry text = DataManager.GetGameTextEntry(_bFollow ? "PetUnfollow" : "PetFollow", Name());
                GUIManager.OpenTextWindow(text, this, true);
            }
        }

        public void SpawnNearPlayer()
        {
            if (!CutsceneManager.Playing)
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
        }
    }
}
