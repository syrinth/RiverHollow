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
        private int StableID => GetIntByIDKey("BuildingID");
        public Mount(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Walk);
            BodySprite = LoadSpriteAnimations(liData, SpriteName());
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
            Point mod = new Point((playerSprite.Width - Width) / 2, Height - 8);
            SetPosition(playerSprite.Position + mod);
        }

        public void SpawnInHome()
        {
            RHMap stableMap = MapManager.Maps[TownManager.GetBuildingByID(StableID)?.InnerMapName];
            stableMap.AddActor(this);
            SetPosition(Util.GetRandomItem(stableMap.FindFreeTiles()).Position);
        }

        public bool CanEnterBuilding(string mapName)
        {
            bool rv = false;

            RHMap stableMap = MapManager.Maps[TownManager.GetBuildingByID(StableID)?.InnerMapName];
            if (mapName.Equals(stableMap.Name))
            {
                rv = true;
            }
            return rv;
        }

        public bool StableBuilt() { return TownManager.TownObjectBuilt(StableID); }
    }
}
