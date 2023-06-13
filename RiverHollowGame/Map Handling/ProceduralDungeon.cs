using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Map_Handling
{
    public class ProceduralDungeon : Dungeon
    {
        const int ENCOUNTER_RATE = 7;
        const int MAP_DIMENSIONS = 7;
        const int MIN_DUNGEON = 7;
        const int MAX_DUNGEON = 12;
        const int DUNGEON_SIZE = 6;

        int _iCurrentLevelSize = 0;
        private List<RoomInfo> _liMapInfo;
        public ProceduralDungeon(string name) : base(name)
        {
            _liMapInfo = new List<RoomInfo>();
        }

        public void InitializeDungeon(string currentMap, TravelPoint pt)
        {
            _iCurrentLevelSize = 0;

            _diDungeonInfo = DataManager.GetDungeonInfo(pt.DungeonInfoID);

            //Generate a new DungeonMap
            RoomInfo[,] arrDungeonMap = new RoomInfo[MAP_DIMENSIONS, MAP_DIMENSIONS];

            //Note the given information
            RHMap startingMap = MapManager.Maps[currentMap];
            RHMap targetMap = MapManager.Maps[pt.GoToMap];

            //Randomly choose a map piece as our start
            List<RoomInfo> modularMaps = new List<RoomInfo>(_liMapInfo.FindAll(x => x.Map.Modular));

            //By default, coords are 0,0 so no changes necessary
            RoomInfo firstRoom = RetrieveRandomRoomInfo(ref modularMaps);

            //Assign the first room to the center of the map
            firstRoom.SetCoordinates(new Point(3, 3));
            arrDungeonMap[3, 3] = firstRoom;

            List<RoomInfo> mapsInUse = new List<RoomInfo> { firstRoom };

            //Kick off the recursive method
            GrowMapFromRoom(firstRoom, 65, ref modularMaps, ref mapsInUse, ref arrDungeonMap);

            //startingMap.DictionaryTravelPoints.Values.ToList().Find(x => x.Modular).Dir; 

            //Connect the start and end rooms to the generated dungeon map
            ConnectTerminalMap(startingMap, Util.GetOppositeDirection(pt.EntranceDir), mapsInUse, arrDungeonMap);
            ConnectTerminalMap(targetMap, pt.EntranceDir, mapsInUse, arrDungeonMap);

            //For each map, assign the information needed to spawn resources, and monsters
            //then handle the entrances, and spawn themap entities.
            foreach (RoomInfo rmInfo in mapsInUse)
            {

                //foreach (MonsterSpawn spawn in rmInfo.Map.MonsterSpawnPoints)
                //{
                //    spawn.AssignMonsterIDs("Spawn-All", _diDungeonInfo["MonsterID"]);
                //}

                HandleEntrances(rmInfo.Map);
            }
        }

        public override void AddMap(RHMap map)
        {
            RoomInfo rmInfo = new RoomInfo(map);

            _liMapInfo.Add(rmInfo);
            string recallPoint = string.Empty;
            Util.AssignValue(ref recallPoint, "RecallPoint", map.Map.Properties);
            if (!string.IsNullOrEmpty(recallPoint))
            {
                string[] split = recallPoint.Split(',');
                _pRecallPoint = new Point(int.Parse(split[0]), int.Parse(split[1]));
                _sEntranceMapName = map.Name;
            }
        }

        /// <summary>
        /// Procedural function used to grow the Dungeon Map. Using a do/while loop to
        /// proof it against the possibility of having a map with too few rooms
        /// </summary>
        /// <param name="rmInfo">The roomInfo to run this iteration on</param>
        /// <param name="chancePerEntrance">The chance to propogate an entrance</param>
        /// <param name="modularMaps">The list of available map pieces</param>
        /// <param name="mapsInUse">The list of all maps currently being used</param>
        /// <param name="dungeonMap">Array representing the Dungeon Map</param>
        private void GrowMapFromRoom(RoomInfo rmInfo, int chancePerEntrance, ref List<RoomInfo> modularMaps, ref List<RoomInfo> mapsInUse, ref RoomInfo[,] dungeonMap)
        {
            List<RoomInfo> roomQueue = new List<RoomInfo>();

            do
            {
                List<TravelPoint> pointsToConnect = new List<TravelPoint>();

                Point roomCoordinates = rmInfo.Coordinates;
                //Because we are modifying the Dictionary of TravelPoints, we need to first determine
                //which TravelPoints will be used and act on that list separately.
                foreach (TravelPoint pt in rmInfo.Map.DictionaryTravelPoints.Values)
                {
                    //Do a string empty check to ensure that we don't overwrite a connected map
                    if (RHRandom.Instance().Next(1, 100) <= chancePerEntrance && string.IsNullOrEmpty(pt.LinkedMap))
                    {
                        Point newCoords = roomCoordinates + Util.GetPointFromDirection(pt.EntranceDir);
                        if (AreCoordinatesValid(newCoords))
                        {
                            pointsToConnect.Add(pt);
                        }
                    }
                }

                foreach (TravelPoint pt in pointsToConnect)
                {
                    RoomInfo room;

                    //Update the coordinates of the new room, relative to the coordinates of the room it is now attached to.
                    //This will overwrite if a room gets attached to multiple times, but they should all be in sync
                    Point newCoords = roomCoordinates + Util.GetPointFromDirection(pt.EntranceDir);
                    if (dungeonMap[(int)newCoords.X, (int)newCoords.Y] != null) { room = dungeonMap[(int)newCoords.X, (int)newCoords.Y]; }
                    else
                    {
                        room = RetrieveRandomRoomInfo(ref modularMaps);
                        room.SetCoordinates(newCoords);
                        mapsInUse.Add(room);
                        dungeonMap[(int)newCoords.X, (int)newCoords.Y] = room;
                        _iCurrentLevelSize++;
                    }

                    ConnectMaps(rmInfo.Map, room.Map, pt.EntranceDir);

                    if (_iCurrentLevelSize == MAX_DUNGEON) { return; }
                    else { roomQueue.Add(room); }
                }

                if (roomQueue.Count > 0)
                {
                    rmInfo = roomQueue[0];
                    roomQueue.RemoveAt(0);
                }
                else
                {
                    //If we've run out of rooms in the queue pick a random room out of the list of used rooms and try again
                    rmInfo = mapsInUse[RHRandom.Instance().Next(mapsInUse.Count)];
                }

            } while (_iCurrentLevelSize < MAX_DUNGEON);
        }

        /// <summary>
        /// Grab random maps in the list of maps and search through their TravelPoints
        /// for a point that has the matching DirectionEnum that we're looking for.
        /// 
        /// Ensure that the TravelPoint we need to use is not linked, and that the point
        /// that would correspond to a Dungeon Room is not occupied.
        /// 
        /// Connect the maps, if we have found a valid TravelPoint
        /// 
        /// If we don't find a valid TravelPoint on the chosen map, remove it from the list
        /// and loop, trying again.
        /// </summary>
        /// <param name="terminalMap">The static map to connect</param>
        /// <param name="dirToConnectTo">The DirectionEnum to search for on other maps</param>
        /// <param name="mapsInUse">List of maps that have been added</param>
        /// <param name="arrDungeonMap">The dungeonMap itself</param>
        private void ConnectTerminalMap(RHMap terminalMap, DirectionEnum dirToConnectTo, List<RoomInfo> mapsInUse, RoomInfo[,] arrDungeonMap)
        {
            //Copy the list of maps in use so we can safely remove entries
            List<RoomInfo> usedMapCopy = new List<RoomInfo>(mapsInUse);
            bool terminalMapAdded = false;
            do
            {
                RoomInfo rmInfo = usedMapCopy[RHRandom.Instance().Next(usedMapCopy.Count)];
                TravelPoint point = rmInfo.Map.DictionaryTravelPoints.Values.ToList().Find(x => x.EntranceDir == dirToConnectTo && x.MapLink == string.Empty);

                //Ensure that the corresponding DungeonRoom location isn't occupied
                Point targetCoords = rmInfo.Coordinates + Util.GetPointFromDirection(Util.GetOppositeDirection(dirToConnectTo));
                if (point != null && AreCoordinatesValid(targetCoords) && arrDungeonMap[(int)targetCoords.X, (int)targetCoords.Y] != null)
                {
                    ConnectMaps(terminalMap, rmInfo.Map, Util.GetOppositeDirection(dirToConnectTo));
                    terminalMapAdded = true;
                }

                //Remove the map so we won't look at it again
                usedMapCopy.Remove(rmInfo);
            } while (!terminalMapAdded);

        }

        /// <summary>
        /// Ensures that the given coordinates are within the legal
        /// bounds of the Dungeon Map
        /// </summary>
        /// <param name="coordinates">The Dungeon coordinates of the room</param>
        /// <returns>True if the coordinate numbers are within range</returns>
        private bool AreCoordinatesValid(Point coordinates)
        {
            return coordinates.X > 0 && coordinates.X < DUNGEON_SIZE && coordinates.Y > 0 && coordinates.Y < DUNGEON_SIZE;
        }

        /// <summary>
        /// This method handles the dictionary links between two RHMap objects, given a direction of connection.
        /// The given direction must be that of the TravelPoint on Map1 that describes the direction of entry from
        /// Map2
        /// </summary>
        /// <param name="map1">The map we "start" on</param>
        /// <param name="map2">The map we "end up" on</param>
        /// <param name="movementDir">The movementDir of the travelPoint connecting Map1 to Map2</param>
        private void ConnectMaps(RHMap map1, RHMap map2, DirectionEnum movementDir)
        {
            //Ensures that there is a map2 and that the two maps are not already connected
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
            map1.DictionaryTravelPoints[positionPoint.MapLink] = positionPoint;

            map2.DictionaryTravelPoints.Remove(Util.GetEnumString(oppDir));
            map2.DictionaryTravelPoints[linkedPoint.MapLink] = linkedPoint;

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

        /// <summary>
        /// Creates all of the entrance objects on the indicatedm ap
        /// </summary>
        /// <param name="map"></param>
        private void HandleEntrances(RHMap map)
        {
            string[] strsplit = Util.FindParams(_diDungeonInfo["EntranceID"]);

            foreach (TiledMapObject obj in map.GetMapObjectsByName("BlockObject"))
            {
                Rectangle blocker = Util.RectFromTiledMapObject(obj);
                DirectionEnum dir = Util.ParseEnum<DirectionEnum>(obj.Properties["Dir"]);
                Point pos;
                switch (dir)
                {
                    case DirectionEnum.Up:
                        pos = new Point(blocker.X + blocker.Width, blocker.Y);
                        CreateEntranceObject(obj, map, int.Parse(strsplit[0]), int.Parse(strsplit[1]), int.Parse(strsplit[2]), blocker.Location, pos, blocker.Location);
                        break;
                    case DirectionEnum.Down:
                        pos = blocker.Location + new Point(0, Constants.TILE_SIZE);
                        CreateEntranceObject(obj, map, int.Parse(strsplit[3]), int.Parse(strsplit[4]), int.Parse(strsplit[5]), pos, new Point(pos.X + blocker.Width, pos.Y), pos);
                        break;
                    case DirectionEnum.Left:
                        CreateEntranceObject(obj, map, int.Parse(strsplit[6]), int.Parse(strsplit[7]), int.Parse(strsplit[8]), blocker.Location, blocker.Location + new Point(0, blocker.Height), blocker.Location);
                        break;
                    case DirectionEnum.Right:
                        CreateEntranceObject(obj, map, int.Parse(strsplit[9]), int.Parse(strsplit[10]), int.Parse(strsplit[11]), blocker.Location, blocker.Location + new Point(0, blocker.Height), blocker.Location);
                        break;
                }
            }
        }

        /// <summary>
        /// Creates the appropriate WorldObject(s) at the location of the given blocker/entrywayobject
        /// </summary>
        /// <param name="entranceObject">The TiledMapObject for the blocker object</param>
        /// <param name="map">The map we're on</param>
        /// <param name="closedID">The ID of the WorldObject to close this entry</param>
        /// <param name="openID1">The ID of the leftmost/top entryway WorldObject</param>
        /// <param name="openID2">The ID of the righttmost/bottom entryway WorldObject</param>
        /// <param name="firstObj">The location to create the first open WorldObject</param>
        /// <param name="secondObj">The location to create the second open WorldObject</param>
        /// <param name="closedObject">The location to create the closed WorldObject</param>
        private void CreateEntranceObject(TiledMapObject entranceObject, RHMap map, int closedID, int openID1, int openID2, Point firstObj, Point secondObj, Point closedObject)
        {
            if (bool.Parse(entranceObject.Properties["Open"]))
            {
                WorldObject obj = DataManager.CreateWorldObjectByID(openID1);
                obj.PlaceOnMap(firstObj, map);

                obj = DataManager.CreateWorldObjectByID(openID2);
                obj.PlaceOnMap(secondObj, map);
            }
            else
            {
                WorldObject obj = DataManager.CreateWorldObjectByID(closedID);
                obj.PlaceOnMap(closedObject, map);
            }
        }

        /// <summary>
        /// Grab a random RoomInfo object from the given List and then remove it from the List
        /// </summary>
        /// <param name="mapPieces">A reference to the list of possible RoomInfo objects</param>
        /// <returns>A randomly selected RoomInfo</returns>
        private RoomInfo RetrieveRandomRoomInfo(ref List<RoomInfo> mapPieces)
        {
            int randomInt = RHRandom.Instance().Next(mapPieces.Count - 1);
            RoomInfo rv = mapPieces[randomInt];

            mapPieces.RemoveAt(randomInt);
            return rv;
        }

        public override void ResetDungeon()
        {
            foreach (RoomInfo map in _liMapInfo)
            {
                RHMap m = map.Map;
                Dictionary<string, TravelPoint> travlCpy = new Dictionary<string, TravelPoint>(m.DictionaryTravelPoints);
                foreach (KeyValuePair<string, TravelPoint> kvp in travlCpy)
                {
                    if (kvp.Value.Modular)
                    {
                        TravelPoint pt = m.DictionaryTravelPoints[kvp.Key];
                        pt.Reset();

                        m.DictionaryTravelPoints.Remove(kvp.Key);
                        m.DictionaryTravelPoints[Util.GetEnumString(pt.EntranceDir)] = pt;
                    }
                }

                foreach (TiledMapObject blocker in m.GetMapObjectsByName("BlockObject"))
                {
                    blocker.Properties["Open"] = "false";
                }

                m.ClearMapEntities();
            }
        }

        private class RoomInfo
        {
            public readonly RHMap Map;
            public Point Coordinates = Point.Zero;

            public bool Modular => Map.Modular;

            public RoomInfo(RHMap map)
            {
                Map = map;
            }

            public void SetCoordinates(Point coords)
            {
                Coordinates = coords;
            }
        }
    }
}
