using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Mount : TalkingActor
    {
        private int StableID => DataManager.GetIntByIDKey(ID, "BuildingID", DataType.NPC);
        public override Rectangle CollisionBox => new Rectangle((int)Position.X, (int)Position.Y, Width, Constants.TILE_SIZE);

        public Mount(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            ActorType = WorldActorTypeEnum.Mount;

            Util.AssignValue(ref _iBodyWidth, "Width", stringData);
            Util.AssignValue(ref _iBodyHeight, "Height", stringData);

            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Walk);
            LoadSpriteAnimations(ref _sprBody, liData, SpriteName());
        }

        public override void ProcessRightButtonClick()
        {
            if (!PlayerManager.PlayerActor.Mounted)
            {
                PlayerManager.PlayerActor.MountUp(this);
            }
        }

        public void SyncToPlayer()
        {
            AnimatedSprite playerSprite = PlayerManager.PlayerActor.BodySprite;
            MapManager.CurrentMap.AddActor(this);
            Vector2 mod = new Vector2((playerSprite.Width - BodySprite.Width) / 2, BodySprite.Height - 8);
            Position = playerSprite.Position + mod;
        }

        public void SpawnInHome()
        {
            RHMap stableMap = MapManager.Maps[TownManager.GetBuildingByID(StableID)?.BuildingMapName];
            stableMap.AddActor(this);
            Position = Util.GetRandomItem(stableMap.FindFreeTiles()).Position;
        }

        public bool CanEnterBuilding(string mapName)
        {
            bool rv = false;

            RHMap stableMap = MapManager.Maps[TownManager.GetBuildingByID(StableID)?.BuildingMapName];
            if (mapName.Equals(stableMap.Name))
            {
                rv = true;
            }
            return rv;
        }

        public bool StableBuilt() { return TownManager.TownObjectBuilt(StableID); }
    }
}
