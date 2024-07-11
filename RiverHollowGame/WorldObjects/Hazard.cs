using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Map_Handling;
using System;

using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{

    public class Hazard : WorldObject
    {
        readonly HazardTypeEnum _eHazardType;

        public int Damage => GetIntByIDKey("Damage");
        public bool Active { get; private set; }
        RHTimer _timer;

        DirectionEnum _eMoveDir = DirectionEnum.None;

        public Hazard(int id) : base(id)
        {
            _bWalkable = true;
            _eObjectType = ObjectTypeEnum.Hazard;
            _eHazardType = GetEnumByIDKey<HazardTypeEnum>("Subtype");

            Sprite.SetLayerDepthMod(GetBoolByIDKey("DrawOver") ? 1 : -999);

            Activate(_eHazardType == HazardTypeEnum.Passive);

            if(_eHazardType == HazardTypeEnum.Timed)
            {
                _timer = new RHTimer(GetFloatByIDKey("Timer"));
            }

            if (GetBoolByIDKey("Move"))
            {
                _eMoveDir = GetEnumByIDKey<DirectionEnum>("Move");
            }
        }

        public override void Update(GameTime gTime)
        {
            if (_eHazardType == HazardTypeEnum.Timed)
            {
                if (_timer.TickDown(gTime, true))
                {
                    Activate(!Active);
                }
            }
            if(_eMoveDir != DirectionEnum.None)
            {
                RHTile currentTile = CurrentMap.GetTileByPixelPosition(BaseCenter);
                RHTile nextTile = currentTile.GetTileByDirection(_eMoveDir);
                float deltaX = Math.Abs((nextTile.Center - BaseCenter).X);
                float deltaY = Math.Abs((nextTile.Center - BaseCenter).Y);

                if (!nextTile.Passable() && deltaX < Constants.TILE_SIZE + 2 && deltaY < Constants.TILE_SIZE + 2)
                {
                    _eMoveDir = Util.GetOppositeDirection(_eMoveDir);
                }
                else
                {
                    Point vec = Util.GetPointFromDirection(_eMoveDir);
                    Sprite.Position += Util.MultiplyPoint(vec, 2);
                    MapPosition += Util.MultiplyPoint(vec, 2);
                }
            }

            if (Active)
            {
                if (BaseRectangle.Intersects(PlayerManager.PlayerActor.CollisionBox))
                {
                    PlayerManager.PlayerActor.DealDamage(Damage, BaseRectangle);
                }
            }
        }

        protected override void LoadSprite()
        {
            base.LoadSprite();

            if (_eHazardType != HazardTypeEnum.Passive)
            {
                Sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + Constants.TILE_SIZE, _pImagePos.Y, _pSize);
            }
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = base.PlaceOnMap(pos, map);

            if(_eMoveDir != DirectionEnum.None)
            {
                RemoveSelfFromTiles();
            }

            return rv;
        }

        private void Activate(bool value)
        {
            Active = value;
            switch (_eHazardType)
            {
                case HazardTypeEnum.Passive:
                    Sprite.PlayAnimation(AnimationEnum.ObjectIdle);
                    break;
                case HazardTypeEnum.Timed:
                case HazardTypeEnum.Triggered:
                    Sprite.PlayAnimation(value ? AnimationEnum.Action1 : AnimationEnum.ObjectIdle);
                    break;
            }
        }
    }
}
