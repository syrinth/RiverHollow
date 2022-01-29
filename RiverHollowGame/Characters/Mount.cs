using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Mount : BuyableNPC
    {
        public override Rectangle CollisionBox => new Rectangle((int)Position.X, (int)Position.Y, Width, TILE_SIZE);

        int _iStableID = -1;
        public Mount(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _eActorType = WorldActorTypeEnum.Mount;
            DataManager.GetTextData("NPC", ID, ref _sName, "Name");

            Util.AssignValue(ref _iBodyWidth, "Width", stringData);
            Util.AssignValue(ref _iBodyHeight, "Height", stringData);

            Util.AssignValue(ref _iStableID, "BuildingID", stringData);

            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Walk);
            LoadSpriteAnimations(ref _sprBody, liData, DataManager.NPC_FOLDER + "NPC_" + stringData["Key"]);
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
            RHMap stableMap = MapManager.Maps[PlayerManager.GetBuildingByID(_iStableID)?.MapName];
            stableMap.AddActor(this);
            Position = Util.GetRandomItem(stableMap.FindFreeTiles()).Position;
        }

        public bool CanEnterBuilding(string mapName)
        {
            bool rv = false;

            RHMap stableMap = MapManager.Maps[PlayerManager.GetBuildingByID(_iStableID)?.MapName];
            if (mapName.Equals(stableMap.Name))
            {
                rv = true;
            }
            return rv;
        }

        public bool StableBuilt() { return PlayerManager.TownObjectBuilt(_iStableID); }
    }
}
