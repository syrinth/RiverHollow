using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Game_Managers
{
    public static class DungeonManager
    {
        public static Dungeon CurrentDungeon => (_diDungeons.ContainsKey(MapManager.CurrentMap.DungeonName) ? _diDungeons[MapManager.CurrentMap.DungeonName] : null);
        private static Dictionary<string, Dungeon> _diDungeons;

        public static void Instantiate()
        {
            _diDungeons = new Dictionary<string, Dungeon>();
        }

        public static void ResetDungeons()
        {
            foreach(Dungeon d in _diDungeons.Values)
            {
                d.ResetDungeon();
            }
        }

        public static void GoToEntrance()
        {
            CurrentDungeon?.GoToEntrance();
            GUIManager.CloseMainObject();
        }

        public static void AddMapToDungeon(string dungeonName, RHMap map) {
            if (!_diDungeons.ContainsKey(dungeonName))
            {
                _diDungeons[dungeonName] = new Dungeon(dungeonName);
            }

            _diDungeons[dungeonName].AddMap(map);
        }

        public static void AddWarpPoint(WarpPoint obj, string dungeon)
        {
            _diDungeons[dungeon].AddWarpPoint(obj);
        }
        public static void AddDungeonKey() { CurrentDungeon.AddKey(); }
        public static void UseDungeonKey() { CurrentDungeon.UseKey(); }
        public static int DungeonKeys()
        {
            int rv = 0;

            if (CurrentDungeon != null)
            {
                rv = CurrentDungeon.NumKeys;
            }

            return rv;
        }

        public static void InitializeDungeon(string dungeonName, string currentMap, TravelPoint pt)
        {
            _diDungeons[dungeonName].InitializeDungeon(currentMap, pt);
        }
    }

    public class Dungeon
    {
        const int DUNGEON_SIZE = 6;
        string _sEntranceMapName;
        Vector2 _vRecallPoint;
        public int NumKeys { get; private set; }
        public string Name { get; private set; }
        List<WarpPoint> _liWarpPoints;
        public IList<WarpPoint> WarpPoints { get { return _liWarpPoints.AsReadOnly(); } }

        private List<string> _liMapNames;
        public Dungeon(string name)
        {
            _liWarpPoints = new List<WarpPoint>();
            _liMapNames = new List<string>();
            Name = name;
        }

        public void InitializeDungeon(string currentMap, TravelPoint pt)
        {
            int mapArraySize = 7;

            string mapStart = "map" + Name + "_";
            RHMap[,] dungeonMapArray = new RHMap[mapArraySize, mapArraySize];

            string firstStaticMapName = (pt.Dir == DirectionEnum.Down) ? currentMap : pt.GoToMap;
            string lastStaticMapName = (pt.Dir == DirectionEnum.Down) ? pt.GoToMap : currentMap;

            RHMap firstStaticMap = MapManager.Maps[firstStaticMapName];
            RHMap lastStaticMap = MapManager.Maps[lastStaticMapName];

            TravelPoint fromFirstRandom = GetFirstDirectionalTravelPoint(firstStaticMap.DictionaryTravelPoints);
            TravelPoint fromLastRandom = GetFirstDirectionalTravelPoint(lastStaticMap.DictionaryTravelPoints);

            List<string> mapPieces = new List<string>(_liMapNames.FindAll(x => x.StartsWith("mapCave__")));

            RHMap firstRandomMap = RetrieveMapPiece(ref mapPieces);
            RHMap lastRandomMap = RetrieveMapPiece(ref mapPieces);

            Vector2 startMapPosition = Vector2.Zero;
            Vector2 endMapPosition = Vector2.Zero;
            AssignEndPieceToMap(ref dungeonMapArray, mapArraySize, firstRandomMap, fromFirstRandom, ref startMapPosition);
            AssignEndPieceToMap(ref dungeonMapArray, mapArraySize, lastRandomMap, fromLastRandom, ref endMapPosition);

            //Fill in the map
            Vector2 delta = endMapPosition - startMapPosition;

            int totalDistance = (int)(Math.Abs(delta.X) + Math.Abs(delta.Y));

            Vector2 fillPosition = startMapPosition;
            for (int i = 0; i < totalDistance - 1; i++)
            {
                int coinFlip = RHRandom.Instance().Next(1, 2);

                Vector2 moveBy = Vector2.Zero;
                //if 1, move horizontally
                if (coinFlip == 1)
                {
                    if (delta.X != 0) { moveBy = MoveHorizontal(ref delta); }
                    else { moveBy = MoveVertical(ref delta); }
                }
                else if (coinFlip == 2)
                {
                    if (delta.Y != 0) { moveBy = MoveVertical(ref delta); }
                    else { moveBy = MoveHorizontal(ref delta); }
                }

                fillPosition += moveBy;
                dungeonMapArray[(int)fillPosition.X, (int)fillPosition.Y] = RetrieveMapPiece(ref mapPieces);
            }

            ConnectMaps(firstStaticMap, firstRandomMap, DirectionEnum.Down);
            ConnectMaps(lastRandomMap, lastStaticMap, DirectionEnum.Down);

            foreach (Vector2 vec in Util.GetAllPointsInArea(0, 0, mapArraySize, mapArraySize))
            {
                ConnectToNeighbours(ref dungeonMapArray, mapArraySize, vec, pt.DungeonInfoID);
            }

            foreach (Vector2 vec in Util.GetAllPointsInArea(0, 0, mapArraySize, mapArraySize))
            {
                RHMap map = dungeonMapArray[(int)vec.X, (int)vec.Y];
                if (map != null)
                {
                    CreateBlockers(map);
                    map.SpawnMapEntities();
                }
            }
        }

        public void ResetDungeon()
        {
            foreach (string mapName in _liMapNames)
            {
                RHMap m = MapManager.Maps[mapName];
                Dictionary<string, TravelPoint> travlCpy = new Dictionary<string, TravelPoint>(m.DictionaryTravelPoints);
                foreach (KeyValuePair<string, TravelPoint> kvp in travlCpy)
                {
                    if (kvp.Value.Modular)
                    {
                        TravelPoint pt = m.DictionaryTravelPoints[kvp.Key];
                        pt.Reset();

                        m.DictionaryTravelPoints.Remove(kvp.Key);
                        m.DictionaryTravelPoints[Util.GetEnumString(pt.Dir)] = pt;
                    }
                }

                Dictionary<string, BattleStartInfo> battleCpy = new Dictionary<string, BattleStartInfo>(m.DictionaryBattleStarts);
                foreach (KeyValuePair<string, BattleStartInfo> kvp in battleCpy)
                {
                    if (kvp.Value.Modular)
                    {
                        BattleStartInfo bInfo = m.DictionaryBattleStarts[kvp.Key];
                        bInfo.Reset();

                        m.DictionaryBattleStarts.Remove(kvp.Key);
                        m.DictionaryBattleStarts[Util.GetEnumString(bInfo.Dir)] = bInfo;
                    }
                }

                foreach (TiledMapObject blocker in m.GetMapObjectsByName("BlockObject"))
                {
                    blocker.Properties["Open"] = "false";
                }

                m.ClearMapEntities();
            }
        }

        public void AddWarpPoint(WarpPoint obj)
        {
            _liWarpPoints.Add(obj);
        }

        public void AddMap(RHMap map)
        {
            _liMapNames.Add(map.Name);
            string recallPoint = string.Empty;
            Util.AssignValue(ref recallPoint, "RecallPoint", map.Map.Properties);
            if (!string.IsNullOrEmpty(recallPoint))
            {
                string[] split = recallPoint.Split(',');
                _vRecallPoint = new Vector2(int.Parse(split[0]), int.Parse(split[1]));
                _sEntranceMapName = map.Name;
            }
        }

        public void GoToEntrance()
        {
            MapManager.FadeToNewMap(MapManager.Maps[_sEntranceMapName], _vRecallPoint);
            PlayerManager.World.DetermineFacing(new Vector2(0, 1));
        }

        private void ConnectToNeighbours(ref RHMap[,] dungeonMap, int mapSize, Vector2 mapPosition, int dungeonLevel)
        {
            RHMap targetMap = dungeonMap[(int)mapPosition.X, (int)mapPosition.Y];
            if (targetMap != null)
            {
                if (mapPosition.X + 1 < mapSize) { ConnectMaps(dungeonMap[(int)mapPosition.X, (int)mapPosition.Y], dungeonMap[(int)mapPosition.X + 1, (int)mapPosition.Y], DirectionEnum.Left); }
                if (mapPosition.X - 1 >= 0) { ConnectMaps(dungeonMap[(int)mapPosition.X, (int)mapPosition.Y], dungeonMap[(int)mapPosition.X - 1, (int)mapPosition.Y], DirectionEnum.Right); }
                if (mapPosition.Y + 1 < mapSize) { ConnectMaps(dungeonMap[(int)mapPosition.X, (int)mapPosition.Y], dungeonMap[(int)mapPosition.X, (int)mapPosition.Y + 1], DirectionEnum.Up); }
                if (mapPosition.Y - 1 >= 0) { ConnectMaps(dungeonMap[(int)mapPosition.X, (int)mapPosition.Y], dungeonMap[(int)mapPosition.X, (int)mapPosition.Y - 1], DirectionEnum.Down); }

                //Spawn resources on the targetMap
                Dictionary<string, string> dungeonInfo = DataManager.GetDungeonInfo(dungeonLevel);
                targetMap.AssignResourceSpawns("20-40", dungeonInfo["ObjectID"]);
 
                foreach(MonsterSpawn spawn in targetMap.MonsterSpawnPoints)
                {
                    spawn.AssignMonsterIDs("Spawn-All", dungeonInfo["MonsterID"]);
                }
            }
        }

        private void ConnectMaps(RHMap map1, RHMap map2, DirectionEnum movementDir)
        {
            if (map2 == null || map2.DictionaryTravelPoints.ContainsKey(map1.Name))
            {
                return;
            }

            DirectionEnum oppDir = Util.GetOppositeDirection(movementDir);

            TravelPoint positionPoint = map1.DictionaryTravelPoints[Util.GetEnumString(movementDir)];
            TravelPoint linkedPoint = map2.DictionaryTravelPoints[Util.GetEnumString(oppDir)];

            positionPoint.AssignLinkedMap(map2.Name);
            linkedPoint.AssignLinkedMap(map1.Name);

            //Remove the initial link between the Direction and the Travelpoint
            //Assign the appropriate map key
            map1.DictionaryTravelPoints.Remove(Util.GetEnumString(movementDir));
            map1.DictionaryTravelPoints[positionPoint.LinkedMap] = positionPoint;

            map2.DictionaryTravelPoints.Remove(Util.GetEnumString(oppDir));
            map2.DictionaryTravelPoints[linkedPoint.LinkedMap] = linkedPoint;

            BattleStartInfo bInfo;
            if (map1.DictionaryBattleStarts.Count > 0)
            {
                bInfo = map1.DictionaryBattleStarts[Util.GetEnumString(movementDir)];
                map1.DictionaryBattleStarts.Remove(Util.GetEnumString(movementDir));
                bInfo.AssignLinkedMap(map2.Name);
                map1.DictionaryBattleStarts[map2.Name] = bInfo;
            }

            if (map2.DictionaryBattleStarts.Count > 0)
            {
                bInfo = map2.DictionaryBattleStarts[Util.GetEnumString(oppDir)];
                map2.DictionaryBattleStarts.Remove(Util.GetEnumString(oppDir));
                bInfo.AssignLinkedMap(map1.Name);
                map2.DictionaryBattleStarts[map1.Name] = bInfo;
            }

            //Set the relevant blockerObject Open value to true
            if (map1.GetMapObjectByTagAndValue("Dir", Util.GetEnumString(oppDir)) != null)
            {
                map1.GetMapObjectByTagAndValue("Dir", Util.GetEnumString(oppDir)).Properties["Open"] = "true";
            }

            //Set the relevant blockerObject Open value to true
            if (map2.GetMapObjectByTagAndValue("Dir", Util.GetEnumString(movementDir)) != null)
            {
                map2.GetMapObjectByTagAndValue("Dir", Util.GetEnumString(movementDir)).Properties["Open"] = "true";
            }
        }

        private void CreateBlockers(RHMap map)
        {
            foreach(TiledMapObject blocker in map.GetMapObjectsByName("BlockObject"))
            {
                DirectionEnum dir = Util.ParseEnum<DirectionEnum>(blocker.Properties["Dir"]);
                switch (dir)
                {
                    case DirectionEnum.Up:
                        if (bool.Parse(blocker.Properties["Open"]))
                        {
                            WorldObject obj = DataManager.GetWorldObjectByID(60);
                            obj.PlaceOnMap(blocker.Position, map);

                            obj = DataManager.GetWorldObjectByID(61);
                            obj.PlaceOnMap(new Vector2(blocker.Position.X + blocker.Size.Width, (int)blocker.Position.Y), map);
                        }
                        else
                        {
                            WorldObject obj = DataManager.GetWorldObjectByID(59);
                            obj.PlaceOnMap(blocker.Position, map);
                        }
                        break;
                    case DirectionEnum.Down:
                        if (bool.Parse(blocker.Properties["Open"]))
                        {
                            Vector2 pos = blocker.Position;
                            pos += new Vector2(0, TileSize);
                            WorldObject obj = DataManager.GetWorldObjectByID(71);
                            obj.PlaceOnMap(pos, map);

                            obj = DataManager.GetWorldObjectByID(72);
                            obj.PlaceOnMap(new Vector2(pos.X + blocker.Size.Width, pos.Y), map);
                        }
                        else
                        {
                            Vector2 pos = blocker.Position;
                            pos += new Vector2(0, TileSize);
                            WorldObject obj = DataManager.GetWorldObjectByID(70);
                            obj.PlaceOnMap(pos, map);
                        }
                        break;
                    case DirectionEnum.Left:
                        if (bool.Parse(blocker.Properties["Open"]))
                        {
                            Vector2 pos = blocker.Position;
                            WorldObject obj = DataManager.GetWorldObjectByID(74);
                            obj.PlaceOnMap(blocker.Position, map);

                            obj = DataManager.GetWorldObjectByID(75);
                            obj.PlaceOnMap(new Vector2(blocker.Position.X, blocker.Position.Y + blocker.Size.Height), map);
                        }
                        else
                        {
                            WorldObject obj = DataManager.GetWorldObjectByID(73);
                            obj.PlaceOnMap(blocker.Position, map);
                        }
                        break;
                    case DirectionEnum.Right:
                        if (bool.Parse(blocker.Properties["Open"]))
                        {
                            Vector2 pos = blocker.Position;
                            WorldObject obj = DataManager.GetWorldObjectByID(77);
                            obj.PlaceOnMap(blocker.Position, map);

                            obj = DataManager.GetWorldObjectByID(78);
                            obj.PlaceOnMap(new Vector2(blocker.Position.X, blocker.Position.Y + blocker.Size.Height), map);
                        }
                        else
                        {
                            WorldObject obj = DataManager.GetWorldObjectByID(76);
                            obj.PlaceOnMap(blocker.Position, map);
                        }
                        break;
                }
            }

            //Create Special Objects on the reverse of the direction of the movement
            //Because the movement describes where someone came from to arrive there
            //While the blocker object describes the direction it is blocking
        }

        private Vector2 MoveHorizontal(ref Vector2 delta)
        {
            Vector2 rv = Vector2.Zero;

            if(delta.X < 0) {
                rv.X = -1;
                delta.X += 1;
            }
            else if (delta.X > 0) {
                rv.X = 1;
                delta.X -= 1;
            }

            return rv;
        }
        private Vector2 MoveVertical(ref Vector2 delta)
        {
            Vector2 rv = Vector2.Zero;

            if (delta.Y < 0) {
                rv.Y = -1;
                delta.Y += 1;
            }
            else if (delta.Y > 0) {
                rv.Y = 1;
                delta.Y -= 1;
            }

            return rv;
        }

        private TravelPoint GetFirstDirectionalTravelPoint(Dictionary<string, TravelPoint> travelPoints)
        {
            TravelPoint rv = null;
            foreach (KeyValuePair<string, TravelPoint> kvp in travelPoints)
            {
                if (Util.StringIsEnum<DirectionEnum>(kvp.Key))
                {
                    rv = kvp.Value;
                    break;
                }
            }

            return rv;
        }

        private RHMap RetrieveMapPiece(ref List<string> mapPieces)
        {
            int randomInt = RHRandom.Instance().Next(mapPieces.Count - 1);
            RHMap rv = MapManager.Maps[mapPieces[randomInt]];

            mapPieces.RemoveAt(randomInt);
            return rv;
        }

        private void AssignEndPieceToMap(ref RHMap[,] dungeonMap, int mapSize, RHMap pieceToAssign, TravelPoint pointToConnect, ref Vector2 coords)
        {
            DirectionEnum pointDir = Util.GetOppositeDirection(pointToConnect.Dir);

            switch (pointDir)
            {
                case DirectionEnum.Down:
                    coords.X = RHRandom.Instance().Next(mapSize -1);
                    coords.Y = 0;
                    break;
                case DirectionEnum.Up:
                    coords.X = RHRandom.Instance().Next(mapSize - 1);
                    coords.Y = mapSize - 1;
                    break;
                case DirectionEnum.Left:
                    coords.X = 0;
                    coords.Y = RHRandom.Instance().Next(mapSize - 1);
                    break;
                case DirectionEnum.Right:
                    coords.X = mapSize - 1; ;
                    coords.Y = RHRandom.Instance().Next(mapSize - 1);
                    break;
            }

            dungeonMap[(int)coords.X, (int)coords.Y] = pieceToAssign;
        }

        public void AddKey() { NumKeys++; }
        public void UseKey() { NumKeys--; }
    }
}
