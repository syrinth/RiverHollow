using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using Microsoft.Xna.Framework;
using RiverHollow.Map_Handling;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace RiverHollow.WorldObjects
{

    public class Hazard : WorldObject
    {
        readonly HazardTypeEnum _eHazardType;

        bool _bDrawOver;
        public int Damage { get; }
        public bool Active { get; private set; }
        RHTimer _timer;

        DirectionEnum _eMoveDir = DirectionEnum.None;

        public Hazard(int id, Dictionary<string, string> stringData) : base(id)
        {
            _eObjectType = ObjectTypeEnum.Hazard;
            _eHazardType = Util.ParseEnum<HazardTypeEnum>(stringData["Subtype"]);

            LoadDictionaryData(stringData);
            Damage = int.Parse(stringData["Damage"]);
            Util.AssignValue(ref _bDrawOver, "DrawOver", stringData);
            _bWalkable = true;
            _sprite.SetLayerDepthMod(_bDrawOver ? 1 : -999);

            Activate(_eHazardType == HazardTypeEnum.Passive);

            if(_eHazardType == HazardTypeEnum.Timed)
            {
                _timer = new RHTimer(float.Parse(stringData["Timer"]));
            }

            if (stringData.ContainsKey("Move"))
            {
                _eMoveDir = Util.ParseEnum<DirectionEnum>(stringData["Move"]);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            if (_eHazardType == HazardTypeEnum.Timed)
            {
                _timer.TickDown(gTime);
                if (_timer.Finished())
                {
                    _timer.Reset();
                    Activate(!Active);
                }
            }
            if(_eMoveDir != DirectionEnum.None)
            {
                RHTile currentTile = CurrentMap.GetTileByPixelPosition(CollisionBox.Center);
                RHTile nextTile = currentTile.GetTileByDirection(_eMoveDir);
                float deltaX = Math.Abs((nextTile.Center - CollisionBox.Center.ToVector2()).X);
                float deltaY = Math.Abs((nextTile.Center - CollisionBox.Center.ToVector2()).Y);

                if (!nextTile.Passable() && deltaX < Constants.TILE_SIZE + 2 && deltaY < Constants.TILE_SIZE + 2)
                {
                    _eMoveDir = Util.GetOppositeDirection(_eMoveDir);
                }
                else
                {
                    switch (_eMoveDir)
                    {
                        case DirectionEnum.Down:
                            _sprite.Position += new Vector2(0, 2);
                            _vMapPosition += new Vector2(0, 2);
                            break;
                        case DirectionEnum.Left:
                            _sprite.Position += new Vector2(-2, 0);
                            _vMapPosition += new Vector2(-2, 0);
                            break;
                        case DirectionEnum.Up:
                            _sprite.Position += new Vector2(0, -2);
                            _vMapPosition += new Vector2(0, -2);
                            break;
                        case DirectionEnum.Right:
                            _sprite.Position += new Vector2(2, 0);
                            _vMapPosition += new Vector2(2, 0);
                            break;
                    }
                }
            }

            if (Active && CollisionBox.Intersects(PlayerManager.PlayerActor.CollisionBox))
            {
                PlayerManager.HazardHarmParty(Damage, CollisionBox.Center);
            }
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            base.LoadSprite(stringData, textureName);

            if (_eHazardType != HazardTypeEnum.Passive)
            {
                _sprite.AddAnimation(AnimationEnum.Action1, _pImagePos.X + Constants.TILE_SIZE, _pImagePos.Y, _uSize);
            }
        }

        public override bool PlaceOnMap(Vector2 pos, RHMap map)
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
                    _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
                    break;
                case HazardTypeEnum.Timed:
                case HazardTypeEnum.Triggered:
                    _sprite.PlayAnimation(value ? AnimationEnum.Action1 : AnimationEnum.ObjectIdle);
                    break;
            }
        }
    }
}
