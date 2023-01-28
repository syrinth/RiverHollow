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
    public class Producer : TalkingActor
    {
        public int ItemID => DataManager.GetIntByIDKey(ID, "ItemID", DataType.NPC);

        public int HouseID => DataManager.GetIntByIDKey(ID, "ObjectID", DataType.NPC);

        public Producer(int id, Dictionary<string, string> stringData) : base(id)
        {
            _fBaseSpeed = Constants.NPC_WALK_SPEED;
            _eActorType = WorldActorTypeEnum.Producer;
            _bCanWander = true;

            RHSize size = DataManager.GetSizeByIDKey(ID, "Size", DataType.NPC);
            _iBodyWidth = size.Width;
            _iBodyHeight = size.Height;

            List<AnimationData> liData = new List<AnimationData>();
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Walk);
            Util.AddToAnimationsList(ref liData, stringData, VerbEnum.Idle);
            LoadSpriteAnimations(ref _sprBody, liData, SpriteName());
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (_bBumpedIntoSomething)
            {
                _bBumpedIntoSomething = false;
                ChangeState(NPCStateEnum.Idle);
                SetMoveTo(Vector2.Zero);
            }

            ProcessStateEnum(gTime, true);
        }

        public override void RollOver()
        {
            MoveToSpawn();
            if (PlayerManager.TownObjectBuilt(HouseID))
            {
                RHMap map = MapManager.Maps[PlayerManager.GetBuildingByID(HouseID).BuildingMapName];
                if(!new WrappedItem(ItemID).PlaceOnMap(map.GetRandomPosition(), map))
                {
                    int i = 0;
                }
            }
        }

        public void MoveToSpawn()
        {
            CurrentMap?.RemoveActor(this);
            RHMap map = MapManager.TownMap;
            if (PlayerManager.TownObjectBuilt(HouseID))
            {
                map = MapManager.Maps[PlayerManager.GetBuildingByID(HouseID).BuildingMapName];
            }

            map.AddActor(this);
            Position = map.GetRandomPosition();
        }
    }
}
