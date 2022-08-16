using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.WorldObjects
{
    public class Structure : Buildable
    {
        List<SubObjectInfo> _liSubObjectInfo;
        public IList<SubObjectInfo> ObjectInfo => _liSubObjectInfo.AsReadOnly();

        readonly Vector2 _vecSpecialCoords = Vector2.Zero;
        public Vector2 SpecialCoords => _vecSpecialCoords;
        public Structure(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _liSubObjectInfo = new List<SubObjectInfo>();

            Util.AssignValue(ref _vecSpecialCoords, "SpecialCoords", stringData);

            if (stringData.ContainsKey("SubObjects"))
            {
                foreach (string s in Util.FindParams(stringData["SubObjects"]))
                {
                    string[] split = s.Split('-');
                    _liSubObjectInfo.Add(new SubObjectInfo() { ObjectID = int.Parse(split[0]), Position = new Vector2(int.Parse(split[1]), int.Parse(split[2])) });
                }
            }

            _bWalkable = true;
            _bDrawUnder = true;
        }

        public override bool PlaceOnMap(Vector2 pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = false;
            if (base.PlaceOnMap(pos, map))
            {
                rv = true;
                if (_iID == int.Parse(DataManager.Config[15]["ObjectID"]))
                {
                    foreach (Merchant m in DataManager.DIMerchants.Values)
                    {
                        if (m.OnTheMap)
                        {
                            m.MoveToSpawn();
                        }
                    }
                }

                foreach (SubObjectInfo info in _liSubObjectInfo)
                {
                    WorldObject obj = DataManager.CreateWorldObjectByID(info.ObjectID);
                    RHTile targetTile = MapManager.Maps[MapName].GetTileByPixelPosition(new Vector2(pos.X + info.Position.X, pos.Y + info.Position.Y));
                    RHTile temp = targetTile;
                    for (int i = 0; i < obj.CollisionBox.Width / 16; i++)
                    {
                        temp.RemoveWorldObject();
                        temp = temp.GetTileByDirection(Enums.DirectionEnum.Right); 
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
                RHTile targetTile = MapManager.Maps[MapName].GetTileByPixelPosition(new Vector2(_vMapPosition.X + info.Position.X, _vMapPosition.Y + info.Position.Y));
                if (targetTile.WorldObject != null)
                {
                    targetTile.WorldObject.Sprite.Drawing = false;
                    MapManager.Maps[MapName].RemoveWorldObject(targetTile.WorldObject);
                }

                if (_iID == int.Parse(DataManager.Config[15]["ObjectID"]))
                {
                    foreach (Merchant m in DataManager.DIMerchants.Values)
                    {
                        if (m.OnTheMap)
                        {
                            m.Position = new Vector2(-99, 99);
                        }
                    }
                }
            }
            base.RemoveSelfFromTiles();
        }

        public struct SubObjectInfo
        {
            public int ObjectID;
            public Vector2 Position;
        }
    }
}
