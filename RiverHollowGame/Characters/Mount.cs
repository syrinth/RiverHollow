﻿using Microsoft.Xna.Framework;
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
        private int StableID => DataManager.GetIntByIDKey(ID, "BuildingID", DataType.Actor);
        public Mount(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            Size = Util.ParsePoint(stringData["Size"]);

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
            Point mod = new Point((playerSprite.Width - BodySprite.Width) / 2, BodySprite.Height - 8);
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
