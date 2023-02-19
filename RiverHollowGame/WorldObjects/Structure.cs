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

        readonly Point _vecSpecialCoords = Point.Zero;
        public Point SpecialCoords => _vecSpecialCoords;
        public Structure(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _liSubObjectInfo = new List<SubObjectInfo>();

            Util.AssignValue(ref _vecSpecialCoords, "SpecialCoords", stringData);

            if (stringData.ContainsKey("SubObjects"))
            {
                foreach (string s in Util.FindParams(stringData["SubObjects"]))
                {
                    string[] split = s.Split('-');
                    _liSubObjectInfo.Add(new SubObjectInfo() { ObjectID = int.Parse(split[0]), Position = new Point(int.Parse(split[1]), int.Parse(split[2])) });
                }
            }

            _bWalkable = true;
            _bDrawUnder = true;
        }

        public override bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            bool rv = false;
            if (base.PlaceOnMap(pos, map))
            {
                rv = true;
                if (ID == int.Parse(DataManager.Config[15]["ObjectID"]))
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
                    WorldObject obj = DataManager.CreateWorldObjectByID(info.ObjectID);
                    RHTile targetTile = MapManager.Maps[MapName].GetTileByPixelPosition(new Point(pos.X + info.Position.X, pos.Y + info.Position.Y));
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
                RHTile targetTile = MapManager.Maps[MapName].GetTileByPixelPosition(new Point(MapPosition.X + info.Position.X, MapPosition.Y + info.Position.Y));
                if (targetTile.WorldObject != null)
                {
                    targetTile.WorldObject.Sprite.Show = false;
                    MapManager.Maps[MapName].RemoveWorldObject(targetTile.WorldObject);
                }

                if (ID == int.Parse(DataManager.Config[15]["ObjectID"]))
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

        public struct SubObjectInfo
        {
            public int ObjectID;
            public Point Position;
        }
    }
}
