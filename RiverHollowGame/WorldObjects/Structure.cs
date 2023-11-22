using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Structure : Buildable
    {
        readonly protected List<SubObjectInfo> _liSubObjectInfo;
        public IList<SubObjectInfo> ObjectInfo => _liSubObjectInfo.AsReadOnly();
        public Point SpecialCoords => GetPointByIDKey("SpecialCoords");

        public Structure(int id) : base(id)
        {
            _liSubObjectInfo = new List<SubObjectInfo>();

            if (GetBoolByIDKey("SubObjects"))
            {
                foreach (string s in GetStringParamsByIDKey("SubObjects"))
                {
                    string[] split = Util.FindArguments(s);
                    _liSubObjectInfo.Add(new SubObjectInfo() { ObjectID = int.Parse(split[0]), Position = new Point(int.Parse(split[1]), int.Parse(split[2])) });
                }
            }

            _bWalkable = true;
            _bDrawUnder = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (GameManager.HeldObject == this || !GetBoolByIDKey("HideBase"))
            {
                base.Draw(spriteBatch);
            }
        }

        private RHTile GetSubObjectTile(SubObjectInfo info)
        {
            return MapManager.Maps[MapName].GetTileByPixelPosition(new Point(CollisionBox.X + info.Position.X, CollisionBox.Y + info.Position.Y));
        }

        public override void SelectObject(bool val, bool selectParent = true)
        {
            Selected = val;

            foreach (SubObjectInfo info in _liSubObjectInfo)
            {
                var obj = GetSubObjectTile(info)?.WorldObject;
                if(obj != null && obj.Selected != val)
                {
                    GetSubObjectTile(info)?.WorldObject?.SelectObject(val, false);
                }
            }
        }

        public override bool ProcessLeftClick() { return true; }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = false;
            if (base.PlaceOnMap(pos, map))
            {
                rv = true;
                if (GetBoolByIDKey("Market"))
                {
                    foreach (Merchant npc in TownManager.DIMerchants.Values)
                    {
                        if (npc.OnTheMap)
                        {
                            npc.MoveToSpawn();
                        }
                    }
                }

                foreach (SubObjectInfo info in _liSubObjectInfo)
                {
                    WorldObject obj = new SubObject(this, info.ObjectID);
                    RHTile targetTile = GetSubObjectTile(info);
                    RHTile temp;
                    for (int x = 0; x < obj.CollisionBox.Width / 16; x++)
                    {
                        for (int y = 0; y < obj.CollisionBox.Height / 16; y++)
                        {
                            temp = CurrentMap.GetTileByGridCoords(targetTile.X + x, targetTile.Y + y);
                            temp.RemoveWorldObject();
                        }
                    }
                    obj.PlaceOnMap(targetTile.Position, MapManager.Maps[MapName]);
                }
            }

            return rv;
        }

        public override void RemoveSelfFromTiles()
        {
            foreach (SubObjectInfo info in _liSubObjectInfo)
            {
                RHTile targetTile = GetSubObjectTile(info);
                if (targetTile.WorldObject != null)
                {
                    targetTile.WorldObject.Sprite.Show = false;
                    MapManager.Maps[MapName].RemoveWorldObject(targetTile.WorldObject);
                }

                if (DataManager.GetBoolByIDKey(ID, "Market", DataType.WorldObject))
                {
                    foreach (Merchant m in TownManager.DIMerchants.Values)
                    {
                        if (m.OnTheMap)
                        {
                            m.SetPosition(new Point(-99, 99));
                        }
                    }
                }
            }
            base.RemoveSelfFromTiles();
        }

        public override bool HasTileInRange()
        {
            bool rv = base.HasTileInRange();
            if (!rv)
            {
                foreach (SubObjectInfo info in _liSubObjectInfo)
                {
                    RHTile targetTile = GetSubObjectTile(info);
                    if (targetTile.WorldObject != null)
                    {
                        foreach (var tile in targetTile.WorldObject.Tiles)
                        {
                            if (PlayerManager.InRangeOfPlayer(tile.CollisionBox))
                            {
                                rv = true;
                                break;
                            }
                        }
                    }
                }
            }

            return rv;
        }

        public struct SubObjectInfo
        {
            public int ObjectID;
            public Point Position;
        }
    }

    public class SubObject : Buildable
    {
        readonly Structure _mainObj;

        public override WorldObject Pickup => _mainObj;
        public SubObject(Structure mainobj, int id) : base(id)
        {
            _mainObj = mainobj;
            _eObjectType = ObjectTypeEnum.Buildable;
        }

        public override bool ProcessRightClick()
        {
            return _mainObj.ProcessRightClick();
        }

        public override void SelectObject(bool val, bool selectParent = true)
        {
            Selected = val;
            if (selectParent)
            {
                Pickup.SelectObject(val);
            }
        }

        public override bool HasTileInRange()
        {
            return _mainObj.HasTileInRange();
        }
    }
}
