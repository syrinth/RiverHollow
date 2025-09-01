using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Animal : TalkingActor
    {
        public int ItemID => GetIntByIDKey("ItemID");

        public int HouseID => GetIntByIDKey("ObjectID");

        public Animal(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _fBaseSpeed = Constants.NPC_WALK_SPEED;
            _fWanderSpeed = Constants.NPC_WALK_SPEED;

            Wandering = true;
            _eCollisionState = ActorCollisionState.Slow;

            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Walk);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            BodySprite = LoadSpriteAnimations(liData, SpriteName());
        }

        public override void RollOver()
        {
            MoveToSpawn();
            if (TownManager.TownObjectBuilt(HouseID))
            {
                RHMap map = MapManager.Maps[TownManager.GetBuildingByID(HouseID).InnerMapName];
                if(!new WrappedItem(ItemID).PlaceOnMap(map.GetRandomPoint(), map))
                {
                    LogManager.WriteToLog("Could not place item ID {0} on map {1}", ItemID, map);
                }
            }
        }

        public void MoveToSpawn()
        {
            CurrentMap?.RemoveActor(this);
            RHMap map = MapManager.TownMap;
            if (TownManager.TownObjectBuilt(HouseID))
            {
                map = MapManager.Maps[TownManager.GetBuildingByID(HouseID).InnerMapName];
            }

            map.AddActor(this);
            SetPosition(map.GetRandomPoint());
        }
    }
}
