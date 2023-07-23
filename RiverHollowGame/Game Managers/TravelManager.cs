using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    public static class TravelManager
    {
        private static StreamWriter _swWriter;
        private static int _iSize = 1;
        private static int _iMaxPath = -1;
        private static Actor _actTraveller;

        #region Threading
        static Thread _thread;
        static List<Actor> _liPathingRequest;

        /// <summary>
        /// Nulls the threads
        /// </summary>
        /// <param name="pathingThread">Reference to the villager's thread</param>
        public static void FinishThreading(ref Thread pathingThread)
        {
            _thread = null;
            pathingThread = null;
        }

        /// <summary>
        /// Call to register a pathing request with the TravelManager
        /// </summary>
        /// <param name="actor">The actor to register to pathfind</param>
        public static void RequestPathing(Actor actor)
        {
            if(_liPathingRequest == null) { _liPathingRequest = new List<Actor>(); }
            if (!_liPathingRequest.Contains(actor)) { _liPathingRequest.Add(actor); }
        }

        /// <summary>
        /// Assuming there are requests to process, and there is not currently
        /// a thread being worked on, start the villager's CalculatePathThreaded method
        /// </summary>
        public static void DequeuePathingRequest()
        {
            if (_liPathingRequest != null && _liPathingRequest.Count > 0 && _thread == null)
            {
                _thread = _liPathingRequest[0].CalculatePathThreaded();
                _liPathingRequest.RemoveAt(0);
            }
        }
        #endregion

        #region Travel Log
        public static bool IsDebugEnabled()
        {
            bool rv=false;
            if (DataManager.Config[8]["DebugLogs"].Equals("Enabled"))
            {
                rv = true;
            }
            return rv;
        }

        public static void NewTravelLog(string logName)
        {
            if (IsDebugEnabled())
            {
                string fileName = DataManager.Config[8]["InstallDrive"] + @":" + DataManager.Config[8]["DebugPath"] + logName + " - TravelManager.txt";
                _swWriter = new StreamWriter(fileName);
            }
        }

        public static void CloseTravelLog()
        {
            if (IsDebugEnabled())
            {
                _swWriter?.Close();
            }
        }

        /// <summary>
        /// Wrapper for writing to the logs. We don't care if it fails.
        /// </summary>
        /// <param name="text"></param>
        public static void WriteToTravelLog(string text)
        {
            if (IsDebugEnabled())
            {
                _swWriter?.WriteLine(text);
            }
        }
        #endregion

        #region Pathfinding Parameters
        const int DEFAULT_COST = 100;
        static Dictionary<string, List<RHTile>> _diMapPathing;
        #endregion

        #region Pathfinding


        /// <summary>
        /// Call this method to find the path from the current location, to a location on a different map.
        /// 
        /// When we are finished, set the newStart variable to the end location so subsequent pathfinding calls
        /// know to start there.
        /// </summary>
        /// <param name="findKey">The location to try to reach</param>
        /// <param name="mapName">The map the actor is currently on</param>
        /// <param name="newStart">Reference to the start point. Changes for subsequent pathfinding calulations</param>
        /// <returns></returns>
        public static List<RHTile> FindRouteToLocation(string findKey, string mapName, Point newStart, string logName = "")
        {
            if (!string.IsNullOrEmpty(logName)) { TravelManager.NewTravelLog(logName); }

            List<RHTile> _liCompletePath = new List<RHTile>();            //The path from start to finish, between maps
            _diMapPathing = new Dictionary<string, List<RHTile>>();         //Dictionary of all pathing

            Point start = newStart;                                       //Set the start to the given location
            Dictionary<string, string> mapCameFrom = new Dictionary<string, string>();      
            Dictionary<string, double> mapCostSoFar = new Dictionary<string, double>();     //Records the cost to travel to maps that we've discovered

            WriteToTravelLog("====================== " + mapName + " => " + findKey + " ======================");

            //Initialize pathfinding. Add the start node to the map frontier
            //The map frontier is a list of maps discovered and the cost to arrive at them
            var frontier = new PriorityQueue<string>();
            frontier.Enqueue(mapName, 0);
            mapCameFrom[mapName] = mapName;
            mapCostSoFar[mapName] = 0;

            //As long as there are still maps to explore, we need to keep checking
            //When the loop resets, we are checking a 'new' map.
            while (frontier.Count > 0)
            {
                var testMapStr = frontier.Dequeue();           //The map with the shortest path
                string testMap = testMapStr.Split(':')[0];     //The map we're currently testing. Split is for multiple entrances from same map
                string fromMap = mapCameFrom[testMapStr];      //The map we came from

                RHMap theTestMap = MapManager.Maps[testMap];

                //If the from map isn't the testing map, set the start point at the entrance from the from map
                if (fromMap != testMapStr)
                {
                    //Find the location of the new endpoint on the target map
                    TravelPoint linkedPoint = theTestMap.DictionaryTravelPoints[fromMap];
                    start = linkedPoint.GetMovedCenter();
                }

                //If the testMap contains the key that we're looking for then we need to pathfind from the entrance to the key
                if (theTestMap.DictionaryCharacterLayer.ContainsKey(findKey))
                {
                    //Set the initial values for the map pathfinding
                    //To make this work with the reversal later on, start
                    //at the key, and then walk back to the entrance to the map.
                    mapName = testMapStr;
                    newStart = MapManager.Maps[testMap].DictionaryCharacterLayer[findKey].Location;

                    List<RHTile> pathToExit = FindPathToLocation(ref start, MapManager.Maps[testMap].DictionaryCharacterLayer[findKey].Location, testMapStr);
                    fromMap = mapCameFrom[testMapStr];          //Do the backtracking

                    List<List<RHTile>> liTotalPath = new List<List<RHTile>> { pathToExit };  //The pathfor this segment

                    //Now we backtrack from the map, and add each path to the totalPath
                    while (fromMap != testMapStr)
                    {
                        liTotalPath.Add(_diMapPathing[fromMap + ":" + testMapStr]);
                        testMapStr = fromMap;
                        fromMap = mapCameFrom[testMapStr];
                    }

                    //Since everything has been added in reverse order, we need to reverse it.
                    //Last tile, second last tile, ... first tile
                    liTotalPath.Reverse();

                    //TravelManager Log
                    foreach (List<RHTile> l in liTotalPath)
                    {
                        if (l.Count > 0)
                        {
                            WriteToTravelLog("");
                            WriteToTravelLog("[" + l[0].X + ", " + l[0].Y + "] => [" + l[l.Count() - 1].X + ", " + l[l.Count() - 1].Y + "]");
                            _liCompletePath.AddRange(l);
                        }
                    }

                    //We found it, so break the fuck out of the loop
                    break;
                }

                //Iterate over the exits in the map we're testing and pathfind to them from the starting location
                foreach (KeyValuePair<string, TravelPoint> exit in theTestMap.DictionaryTravelPoints)
                {
                    if (exit.Value.Modular) { continue; }
                    TravelPoint exitPoint = exit.Value;
                    Point pathToVector = exitPoint.GetCenterTilePosition();
                    if (exitPoint.IsDoor)
                    {
                        Point gridCoords = Util.GetGridCoords(exitPoint.GetCenterTilePosition());
                        RHTile doorTile = theTestMap.GetTileByGridCoords(gridCoords);

                        //If the exit points to a door, then path to the RHTile below the door because the door, itself is impassable
                        pathToVector = doorTile.GetTileByDirection(DirectionEnum.Down).Center;
                    }

                    //Find the shortest path to the exit in question. We copy the start vector into a new one
                    //so that our start point doesn't get overridden. We do not care about the location of the last
                    //tile in the previous pathfinding instance for this operation.
                    Point startVector = new Point(start.X, start.Y);
                    List<RHTile> pathToExit = FindPathToLocation(ref startVector, pathToVector, testMapStr, exitPoint.IsDoor);
                    if (pathToExit != null)
                    {
                        //Determine what the new cost of traveling to the testmap is, by appending the
                        //length of the found path, to the current cost to travel to the test map and,
                        //if the map isn't in the dictionary, or the newCost to arrive there is less than
                        //the old cost, we need to change the value to the new shortest path.
                        double newCost = mapCostSoFar[testMapStr] + pathToExit.Count;
                        if (!mapCostSoFar.ContainsKey(exit.Key) || newCost < mapCostSoFar[exit.Key])
                        {
                            if (!exit.Key.Contains(":"))
                            {
                                mapCostSoFar[exit.Key] = newCost;         //Set the map cost to the new cost to arrive
                                frontier.Enqueue(exit.Key, newCost);      //Queue the map with the new cost to arrive there

                                //Setting the backtrack path for the exit map
                                mapCameFrom[exit.Key] = testMap;
                                _diMapPathing[testMapStr + ":" + exit.Value.LinkedMap] = pathToExit; // This needs another key for the appropriate exit
                                WriteToTravelLog("---" + testMapStr + ":" + exit.Value.LinkedMap + "---");
                            }
                            else
                            {
                                int i = 0;
                            }
                        }
                    }
                }
            }

            CloseTravelLog();

            return _liCompletePath;
        }

        //Pathfinds from one point to another on a given map
        public static List<RHTile> FindPathToLocationClean(ref Point start, Point target, string mapName)
        {
            List<RHTile> rvList = FindPathToLocation(ref start, target, mapName);
            return rvList;
        }
        public static List<RHTile> FindPathToLocation(ref Point start, Point target, string mapName = null, bool addDoor = false, bool avoidWalls = true, bool meander = false)
        {
            WriteToTravelLog(System.Environment.NewLine + "+++ " + mapName + " -- [" + start.X/16 + ", " + start.Y / 16 + "] == > [ " + target.X / 16 + ", " + target.Y / 16 + " ] +++");
            
            List<RHTile> returnList = null;
            RHMap map = MapManager.Maps[(mapName ?? MapManager.CurrentMap.Name).Split(':')[0]];     //Returns mapName if it isn't null, else uses the CurrentMap
            RHTile startTile = map.GetTileByPixelPosition(start);
            RHTile goalNode = map.GetTileByPixelPosition(target);
            var travelMap = new TravelMap(startTile);
            var frontier = new PriorityQueue<RHTile>();

            frontier.Enqueue(startTile, 0);
            travelMap.Store(startTile, startTile, 0);
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goalNode))
                {
                    RHTile endTile = current;
                    //Check to see if we stop just before a door
                    if (addDoor)
                    {
                        endTile = current.GetTileByDirection(DirectionEnum.Up);
                        //Cost doesn't matter at this point so just make it 0
                        travelMap.Store(endTile, current, travelMap[current].CostSoFar + GetMovementCost(endTile));
                    }
                    returnList = travelMap.Backtrack(endTile);
                    start = endTile.Position;

                    string print = string.Empty;
                    for (int i = 0; i < returnList.Count(); i++)
                    {
                        print += "[" + returnList[i].X + ", " + returnList[i].Y + "]";

                        if (i != 0 && i % 10 == 0)
                        {
                            WriteToTravelLog(print);
                            print = string.Empty;
                        }
                        else if(i == returnList.Count() - 1)
                        {
                            WriteToTravelLog(print);
                        }
                    }

                    break;
                }

                //Iterate over every tile in the accessible neighbours and, with it as the
                //prospective new BaseTile, confirm that neighbouring tiles are all valid
                foreach (var next in current.GetWalkableNeighbours())
                {
                    if (TestTileForSize(next, false))
                    {
                        double newCost = travelMap[current].CostSoFar + GetMovementCost(next);
                        if (meander)
                        {
                            //If meander is turned on, implement a small distortion in the priority to avoid a perfect shortest path
                            if (RHRandom.Instance().Next(1, 100) > 60)
                            {
                                newCost += RHRandom.Instance().Next(1, 4000);
                            }
                        }
                        if (!travelMap.ContainsKey(next) || newCost < travelMap[next].CostSoFar)
                        {
                            if (IsTileInLine(current, travelMap[current].CameFrom, next)) { newCost--; }

                            double priority = newCost + HeuristicToTile(next, goalNode, avoidWalls);
                            
                            frontier.Enqueue(next, priority);
                            travelMap.Store(next, current, newCost);
                        }
                    }
                }
            }

            return returnList;
        }

        private static bool IsTileInLine(RHTile current, RHTile last, RHTile next)
        {
            bool rv = false;

            if (current.GetTileByDirection(DirectionEnum.Up) == last && current.GetTileByDirection(DirectionEnum.Down) == next) { rv = true; }
            if (current.GetTileByDirection(DirectionEnum.Left) == last && current.GetTileByDirection(DirectionEnum.Right) == next) { rv = true; }
            if (current.GetTileByDirection(DirectionEnum.Down) == last && current.GetTileByDirection(DirectionEnum.Up) == next) { rv = true; }
            if (current.GetTileByDirection(DirectionEnum.Right) == last && current.GetTileByDirection(DirectionEnum.Left) == next) { rv = true; }

            return rv;
        }

        ///// <summary>
        ///// Grows out from the given CombatActor, retrieving a list of RHTiles that are within range of the given desired action
        ///// </summary>
        ///// <param name="actor">The Actor performing the action</param>
        ///// <param name="range">The range of the skill</param>
        ///// <param name="movementParams">Whether or not the skill is movement</param>
        ///// <returns></returns>
        //public static TravelMap FindRangeOfAction(TacticalCombatActor actor, int range, bool movementParams)
        //{
        //    TravelMap travelMap = new TravelMap(actor.BaseTile);
        //    var frontier = new PriorityQueue<RHTile>();
       
        //    travelMap.Store(actor.BaseTile, actor.BaseTile, 0);

        //    // Try to Queue all adjacent RHTiles unless they contain the actor
        //    foreach (RHTile t in actor.BaseTile.GetAdjacentTiles())
        //    { 
        //        if (t.Character != actor)
        //        {
        //            QueueForRange(t, actor.BaseTile, 1, ref frontier, ref travelMap, movementParams);
        //        }
        //    }

        //    while (frontier.Count > 0)
        //    {
        //        var current = frontier.Dequeue();

        //        //If the current RHTile has not exceeded the max range, move to Enqueue the adjacent RHTiles
        //        if (travelMap[current].CostSoFar < range)
        //        {
        //            foreach (var next in current.GetAdjacentTiles())
        //            {
        //                double newCost = travelMap[current].CostSoFar + 1;

        //                if (!travelMap.ContainsKey(next))
        //                {
        //                    QueueForRange(next, current, newCost, ref frontier, ref travelMap, movementParams);
        //                }
        //            }
        //        }
        //    }

        //    return travelMap;
        //}

        #region TravelMap Pathing
        /// <summary>
        /// Calls FindPath To TravelMap and then appends the path
        /// to the found tile from within the TravelMap
        /// </summary>
        /// <param name="startTile">The RHTile to start from</param>
        /// <param name="map">The TravelMap to crawl towards</param>
        /// <returns>A class containing both the whole path and the path to move on</returns>
        public static PathInfo FindPathViaTravelMap(RHTile startTile, TravelMap map)
        {
            PathInfo info = new PathInfo();
            List<RHTile> pathToTravelMap = FindPathToTravelMap(startTile, map);
            if (pathToTravelMap.Count > 0)
            {
                RHTile firstInMap = pathToTravelMap[pathToTravelMap.Count - 1];
                info.AssignPaths(firstInMap, pathToTravelMap, map.Backtrack(firstInMap));
            }

            return info;
        }

        /// <summary>
        /// Given a start location and a previous calculated TravelMap, find the shortest
        /// path to a tile that is within the range of the TravelMap.
        /// </summary>
        /// <param name="startTile">The RHTile to start from</param>
        /// <param name="map">The TravelMap to crawl towards</param>
        /// <returns>The shortest path to the first RHTile in the TravelMap</returns>
        public static List<RHTile> FindPathToTravelMap(RHTile startTile, TravelMap map)
        {
            List<RHTile> rvList = new List<RHTile>();

            RHTile goalNode = map.BaseTile;
            var frontier = new PriorityQueue<RHTile>();
            var travelMap = new TravelMap(startTile);

            frontier.Enqueue(startTile, 0);
            travelMap.Store(startTile, startTile, 0);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                //We keep looping until we find the first RHTile that is in the given TravelMap.
                //Then we return that tile's path
                if (current != startTile && map.CanEndTurnHere(current))
                {
                    rvList = map.Backtrack(current);
                    break;
                }

                //Iterate over every tile in the accessible neighbours and, with it as the
                //prospective new BaseTile, confirm that neighbouring tiles are all valid
                foreach (var next in current.GetWalkableNeighbours())
                {
                    if (TestTileForSize(next))
                    {
                        double newCost = travelMap[current].CostSoFar + GetMovementCost(next);

                        if (!travelMap.ContainsKey(next) || newCost < travelMap[next].CostSoFar)
                        {
                            double priority = newCost + HeuristicToTile(next, goalNode);
                            //Discourages use of occupied tiles but does not stop them from being used
                            if (!TestTileForSize(next, true)) {
                                priority += GetMovementCost(next) * 2;
                            }
                            frontier.Enqueue(next, priority);
                            travelMap.Store(next, current, newCost);
                        }
                    }
                }
            }

            return rvList;
        }

        public static List<RHTile> FindPathAway(RHTile startTile, TravelMap map, List<RHTile> runFromList)
        {
            List<RHTile> rvList = new List<RHTile>();

            RHTile goalNode = map.BaseTile;
            var frontier = new PriorityQueue<RHTile>();
            var fleeMap = new TravelMap(startTile);

            frontier.Enqueue(startTile, 0);
            fleeMap.Store(startTile, startTile, 0);

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                //Keep looking until we find a valid tile that we can end in that is not in the map
                if (current != startTile && TestTileForSize(current, true) && !map.CanEndTurnHere(current))
                {
                    rvList = fleeMap.Backtrack(current);

                    //If the TravelMap does not contain the key but it is in the rvList remove it.
                    //It can be in the rvList by being on the map but not InRange.
                    if (!map.CanEndTurnHere(current) && rvList.Contains(current)) {
                        rvList.Remove(current);
                    }
                    break;
                }

                //Iterate over every tile in the accessible neighbours and, with it as the
                //prospective new BaseTile, confirm that neighbouring tiles are all valid
                foreach (var next in current.GetWalkableNeighbours())
                {
                    if (TestTileForSize(next))
                    {
                        double newCost = fleeMap[current].CostSoFar + GetMovementCost(next);

                        if (!fleeMap.ContainsKey(next) || newCost < fleeMap[next].CostSoFar)
                        {
                            double priority = newCost + HeuristicFromPoints(next, runFromList);
                            //Discourages use of occupied tiles but does not stop them from being used
                            if (!TestTileForSize(next, true))
                            {
                                priority += GetMovementCost(next) * 2;  
                            }
                            frontier.Enqueue(next, priority);
                            fleeMap.Store(next, current, newCost);
                        }
                    }
                }
            }

            return rvList;
        }

        #endregion

        /// <summary>
        /// Helper method to Queue up RHTiles for use in FindRangeOfAction.
        /// </summary>
        //private static void QueueForRange(RHTile testTile, RHTile lastTile, double newCost, ref PriorityQueue<RHTile> frontier, ref TravelMap travelMap, bool testForMovement)
        //{
        //    //If we are not testing for movement, then do queue the RHTile, otherwise we need to both be able
        //    //to target the tile, move through the tile for size and walk through it for allies
        //    if (!testForMovement || (testTile.CanTargetTile() && TestTileForSize(testTile, false) && testTile.CanWalkThroughInCombat()))
        //    {
        //        frontier.Enqueue(testTile, newCost);
        //        travelMap.Store(testTile, lastTile, newCost);

        //        //Do not highlight tiles that cannot be targeted and if we are testing
        //        //for movement, we need to be able to land on the tile with our size in mind
        //        if (testTile.CanTargetTile() && (!testForMovement || TestTileForSize(testTile, true)))
        //        {
        //            travelMap[testTile].InRange = true;
        //        }
        //    }
        //}

        /// <summary>
        /// This method tests the tiles to the left and down of the given tile, since that will be the theoretical
        /// BaseTile, to ensure that the actor can fit there.
        /// </summary>
        /// <param name="nextTile">The tile to be evaluated as a new BaseTile</param>
        /// <returns>True if the actor will fit</returns>
        public static bool TestTileForSize(RHTile nextTile, bool testForEnding = false)
        {
            RHTile lastTile = nextTile;
            for (int i = 0; i < _iSize; i++)
            {
                RHTile rowTile = lastTile;
                for (int j = 0; j < _iSize; j++)
                {
                    //Ensure that the tile exists and it is open
                    if (lastTile == null || !lastTile.CanTargetTile())
                    {
                        return false;
                    }

                    //If we are testing for ending on this tile, we need to check to ensure that the tile does not
                    //contain any other CombatActor that is not the traveller
                    if (testForEnding)
                    {
                        return false;
                    }

                    lastTile = lastTile?.GetTileByDirection(DirectionEnum.Right);
                }

                //Reset to the first Tile in the current row and go down one
                lastTile = rowTile.GetTileByDirection(DirectionEnum.Down);
            }

            return true;
        }

        private static double HeuristicToTile(RHTile a, RHTile b, bool avoidWalls = true)
        {
            int total = 0;
            int distance = (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));

            //Do not perform any wall buffer checks if we are in combat
            int wallBuffer = avoidWalls ? 10 : 0;

            int multiplier = GetMovementCost(a);

            total = (distance + wallBuffer) * multiplier;
            return total;
        }

        private static double HeuristicFromPoints(RHTile a, List<RHTile> fleeFrom)
        {
            int total = 0;
            foreach (RHTile t in fleeFrom)
            {
                int distance = (Math.Abs(a.X - t.X) + Math.Abs(a.Y - t.Y));

                //Do not perform any wall buffer checks if we are in combat
                List<RHTile> futureTiles = a.GetWalkableNeighbours();
                int wallBuffer = (futureTiles.Count < 4) ? 10 : 0;

                int multiplier = GetMovementCost(a);

                total = (distance + wallBuffer) * multiplier;
            }
            return -total;
        }

        //Returns how much it costs to enter the next square
        private static int GetMovementCost(RHTile target)
        {
            return (target.IsRoad || target.Flooring != null) ? 1 : DEFAULT_COST;
        }

        public static void SetParams(int size, Actor act, int maxPath = -1)
        {
            _iSize = size;
            _iMaxPath = maxPath;
            _actTraveller = act;
        }

        public static void ClearParams()
        {
            _iSize = 1;
            _actTraveller = null;
        }

        public class TravelMap : Dictionary<RHTile, TravelData>
        {
            private RHTile _baseTile;
            public RHTile BaseTile => _baseTile;

            public TravelMap(RHTile startTile) : base()
            {
                _baseTile = startTile;
            }

            public List<RHTile> Backtrack(RHTile endTile)
            {
                RHTile current = endTile;
                List<RHTile> rvList = new List<RHTile>();

                while (current != this[current].CameFrom)
                {
                    RHTile lastTile = rvList.Count > 0 ? rvList[rvList.Count - 1] : null;

                    if(lastTile != null && ((lastTile.X == current.X && current.X == this[current].CameFrom.X) ||
                        (lastTile.Y == current.Y && current.Y == this[current].CameFrom.Y)))
                    {
                        current = this[current].CameFrom;
                        continue;
                    }

                    rvList.Add(current);
                    current = this[current].CameFrom;
                }

                rvList.Reverse();

                return rvList;
            }

            public void Store(RHTile key, RHTile from, double cost)
            {
                this[key] = new TravelData
                {
                    CameFrom = from,
                    CostSoFar = cost
                };
            }

            public bool CanEndTurnHere(RHTile tile)
            {
                return this.ContainsKey(tile) && this[tile].InRange;
            }
        }

        public class TravelData
        {
            public RHTile CameFrom;
            public double CostSoFar;
            public bool InRange;
        }

        public class PathInfo
        {
            public static bool IsNull(PathInfo info) { return info == null || info.WholePath == null; }
            public List<RHTile> WholePath;
            public List<RHTile> ActualPath;

            public void Clean()
            {
                WholePath = new List<RHTile>();
                ActualPath = new List<RHTile>();
            }

            public void AssignPaths(RHTile foundTile, List<RHTile> pathToMap, List<RHTile> mapPath)
            {
                WholePath = new List<RHTile>();
                ActualPath = mapPath;                    //Gets the backtrace from the travelmap to the found tile

                WholePath.AddRange(mapPath);                //Add the TravelMap path to the whole path     
                WholePath.Remove(foundTile);            //Remove the current tile to avoid dupes
                pathToMap.Reverse();                        //Reverse the path to the map so that it goes from the map to the target
                WholePath.AddRange(pathToMap);              //Add the path to the map to the whole path
            }

            public int Count()
            {
                if (WholePath == null) { return 0; }
                else
                {
                    return WholePath.Count;
                }
            }
        }
        #endregion
    }
}
