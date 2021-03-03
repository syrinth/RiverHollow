using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;

namespace RiverHollow.Game_Managers
{
    public static class TravelManager
    {
        private static StreamWriter _swWriter;
        private static int _iSize = 1;
        private static int _iMaxPath = -1;
        private static WorldActor _actTraveller;

        public static bool IsDebugEnabled()
        {
            bool rv=false;
            if (DataManager.Config[8]["DebugLogs"].Equals("Enabled"))
            {
                rv=true;
            }
            return rv;
        }


        public static void NewTravelLog(string name)
        {
            if (IsDebugEnabled())
            {
                _swWriter = new StreamWriter(DataManager.Config[8]["InstallDrive"] + @":" + DataManager.Config[8]["DebugPath"] + name + " - TravelManager.txt");
            }
        }

        public static void CloseTravelLog()
        {
            if (IsDebugEnabled())
            {
                _swWriter.Close();
            }
        }

        /// <summary>
        /// Wrapper for writing to the logs. We don't care if it fails.
        /// </summary>
        /// <param name="text"></param>
        public static void WriteToTravelLog(string text)
        {
            try
            {
                if (IsDebugEnabled())
                {
                    _swWriter.WriteLine(text);
                }
            }
            catch (Exception)
            {
            }
        }

        //public static void FindLocation(string currMap, string location)
        //{
        //    RHMap map = MapManager.Maps[currMap];
        //    Dictionary<string, string> mapCameFrom = new Dictionary<string, string>();
        //    Dictionary<string, double> mapCostSoFar = new Dictionary<string, double>();

        //    var frontier = new PriorityQueue<string>();
        //    frontier.Enqueue(currMap, 0);
        //    mapCameFrom[currMap] = currMap;
        //    mapCostSoFar[currMap] = 0;

        //    while (frontier.Count > 0)
        //    {
        //        //Take the Node with the lowest distance
        //        string testMap = frontier.Dequeue();

        //        //Iterate over each map linked to the node
        //        foreach(string linkedMap in _diNodes[testMap].GetMaps())
        //        {
        //            double newCost = mapCostSoFar[testMap] + _diNodes[testMap].GetRouteLength(mapCameFrom[testMap]);
        //            //if (!mapCostSoFar.ContainsKey(exit.Value))
        //            //{
        //            //    mapCostSoFar[exit.Value] = newCost + pathToExit.Count;
        //            //    frontier.Enqueue(exit.Value, newCost);
        //            //    string[] split = null;
        //            //    if (exit.Value.Contains(":"))
        //            //    {
        //            //        split = exit.Value.Split(':');
        //            //    }
        //            //    mapCameFrom[exit.Value] = (split == null) ? testMap : testMap + ":" + split[1];
        //            //    _dictMapPathing[testMapStr + ":" + exit.Value] = pathToExit; // This needd another key for the appropriate exit
        //            //}
        //        }
        //    }
        //}

        #region Pathfinding
        const int DEFAULT_COST = 100;
        static Dictionary<string, List<RHTile>> _diMapPathing;
        #endregion

        #region Pathfinding
        public class PriorityQueue<T>
        {
            // I'm using an unsorted array for this example, but ideally this
            // would be a binary heap. There's an open issue for adding a binary
            // heap to the standard C# library: https://github.com/dotnet/corefx/issues/574
            //
            // Until then, find a binary heap class:
            // * https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
            // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
            // * http://xfleury.github.io/graphsearch.html
            // * http://stackoverflow.com/questions/102398/priority-queue-in-net

            private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

            public int Count
            {
                get { return elements.Count; }
            }

            public void Enqueue(T item, double priority)
            {
                elements.Add(Tuple.Create(item, priority));
            }

            public T Dequeue()
            {
                int bestIndex = 0;

                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Item2 < elements[bestIndex].Item2)
                    {
                        bestIndex = i;
                    }
                }

                T bestItem = elements[bestIndex].Item1;
                elements.RemoveAt(bestIndex);
                return bestItem;
            }

            public List<T> DequeueAllLowest()
            {
                List<T> rvList = new List<T>();
                int bestIndex = 0;

                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Item2 < elements[bestIndex].Item2)
                    {
                        bestIndex = i;
                    }
                }

                Tuple<T, double> bestItem = elements[bestIndex];
                rvList.Add(bestItem.Item1);
                elements.RemoveAt(bestIndex);

                foreach (Tuple<T, double> t in elements)
                {
                    if(t != bestItem && t.Item2 == bestItem.Item2)
                    {
                        rvList.Add(t.Item1);
                    }
                }
                return rvList;
            }
        }

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
        public static List<RHTile> FindPathToOtherMap(string findKey, ref string mapName, ref Vector2 newStart)
        {
            List<RHTile> _liCompletePath = new List<RHTile>();            //The path from start to finish, between maps
            _diMapPathing = new Dictionary<string, List<RHTile>>();         //Dictionary of all pathing

            Vector2 start = newStart;                                       //Set the start to the given location
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
                    newStart = MapManager.Maps[testMap].DictionaryCharacterLayer[findKey];

                    List<RHTile> pathToExit = FindPathToLocation(ref start, MapManager.Maps[testMap].DictionaryCharacterLayer[findKey], testMapStr);
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
                        WriteToTravelLog("");
                        WriteToTravelLog("[" + l[0].X + ", " + l[0].Y + "] => [" + l[l.Count()-1].X + ", " + l[l.Count() - 1].Y + "]");
                        _liCompletePath.AddRange(l);
                    }

                    //We found it, so break the fuck out of the loop
                    break;
                }

                //Iterate over the exits in the map we're testing and pathfind to them from the starting location
                foreach (KeyValuePair<string, TravelPoint> exit in theTestMap.DictionaryTravelPoints)
                {
                    TravelPoint exitPoint = exit.Value;
                    Vector2 pathToVector = exitPoint.GetCenterTilePosition();
                    if (exitPoint.IsDoor)
                    {
                        Vector2 gridCoords = Util.GetGridCoords(exitPoint.GetCenterTilePosition());
                        RHTile doorTile = theTestMap.GetTileByGridCoords(gridCoords);

                        //If the exit points to a door, then path to the RHTile below the door because the door, itself is impassable
                        pathToVector = doorTile.GetTileByDirection(GameManager.DirectionEnum.Down).Center;
                    }
                    //Find the shortest path to the exit in question. We copy the start vector into a new one
                    //so that our start point doesn't get overridden. We do not care about the location of the last
                    //tile in the previous pathfinding instance for this operation.
                    Vector2 startVector = new Vector2(start.X, start.Y);  
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
                            mapCostSoFar[exit.Key] = newCost;         //Set the map cost to the new cost to arrive
                            frontier.Enqueue(exit.Key, newCost);      //Queue the map with the new cost to arrive there

                            //Setting the backtrack path for the exit map
                            mapCameFrom[exit.Key] = testMap;
                            _diMapPathing[testMapStr + ":" + exit.Value.LinkedMap] = pathToExit; // This needs another key for the appropriate exit
                        }
                    }
                }
            }

            return _liCompletePath;
        }

        //Pathfinds from one point to another on a given map
        public static List<RHTile> FindPathToLocationClean(ref Vector2 start, Vector2 target, string mapName)
        {
            List<RHTile> rvList = FindPathToLocation(ref start, target, mapName);
            return rvList;
        }
        public static List<RHTile> FindPathToLocation(ref Vector2 start, Vector2 target, string mapName = null, bool addDoor = false, bool meander = false)
        {
            WriteToTravelLog(System.Environment.NewLine + "+++ " + mapName + " -- [" + (int)start.X/16 + ", " + (int)start.Y / 16 + "] == > [ " + (int)target.X / 16 + ", " + (int)target.Y / 16 + " ] +++");
            
            List<RHTile> returnList = null;
            RHMap map = MapManager.Maps[(mapName ?? MapManager.CurrentMap.Name).Split(':')[0]];     //Returns mapName if it isn't null, else uses the CurrentMap
            RHTile startTile = map.GetTileByPixelPosition(start.ToPoint());
            RHTile goalNode = map.GetTileByPixelPosition(target.ToPoint());
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
                        endTile = current.GetTileByDirection(GameManager.DirectionEnum.Up);
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
                    WriteToTravelLog("---- " + returnList.Count() + " ----");

                    break;
                }

                //Iterate over every tile in the accessible neighbours and, with it as the
                //prospective new BaseTile, confirm that neighbouring tiles are all valid
                foreach (var next in current.GetWalkableNeighbours())
                {
                    bool nextTileIsLast = CombatManager.InCombat && _iMaxPath != -1 && travelMap[current].CostSoFar == _iMaxPath - 1;
                    if (TestTileForSize(next, nextTileIsLast))
                    {
                        double newCost = travelMap[current].CostSoFar + GetMovementCost(next);
                        if (meander)
                        {
                            //If meander is turned on, implement a small distortion in the priority to avoid a perfect shortest path
                            if (RHRandom.Instance.Next(1, 100) > 60)
                            {
                                newCost += RHRandom.Instance.Next(1, 4000);
                            }
                        }
                        if (!travelMap.ContainsKey(next) || newCost < travelMap[next].CostSoFar)
                        {
                            if (IsTileInLine(current, travelMap[current].CameFrom, next)) { newCost--; }

                            double priority = newCost + HeuristicToTile(next, goalNode);
                            
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

            if (current.GetTileByDirection(GameManager.DirectionEnum.Up) == last && current.GetTileByDirection(GameManager.DirectionEnum.Down) == next) { rv = true; }
            if (current.GetTileByDirection(GameManager.DirectionEnum.Left) == last && current.GetTileByDirection(GameManager.DirectionEnum.Right) == next) { rv = true; }
            if (current.GetTileByDirection(GameManager.DirectionEnum.Down) == last && current.GetTileByDirection(GameManager.DirectionEnum.Up) == next) { rv = true; }
            if (current.GetTileByDirection(GameManager.DirectionEnum.Right) == last && current.GetTileByDirection(GameManager.DirectionEnum.Left) == next) { rv = true; }

            return rv;
        }

        /// <summary>
        /// Grows out from the given CombatActor, retrieving a list of RHTiles that are within range of the given desired action
        /// </summary>
        /// <param name="actor">The Actor performing the action</param>
        /// <param name="range">The range of the skill</param>
        /// <param name="movementParams">Whether or not the skill is movement</param>
        /// <returns></returns>
        public static TravelMap FindRangeOfAction(CombatActor actor, int range, bool movementParams)
        {
            TravelMap travelMap = new TravelMap(actor.BaseTile);
            var frontier = new PriorityQueue<RHTile>();
       
            travelMap.Store(actor.BaseTile, actor.BaseTile, 0);

            // Try to Queue all adjacent RHTiles unless they contain the actor
            foreach (RHTile t in actor.BaseTile.GetAdjacentTiles())
            { 
                if (t.Character != actor)
                {
                    QueueForRange(t, actor.BaseTile, 1, ref frontier, ref travelMap, movementParams);
                }
            }

            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                //If the current RHTile has not exceeded the max range, move to Enqueue the adjacent RHTiles
                if (travelMap[current].CostSoFar < range)
                {
                    foreach (var next in current.GetAdjacentTiles())
                    {
                        double newCost = travelMap[current].CostSoFar + 1;

                        if (!travelMap.ContainsKey(next))
                        {
                            QueueForRange(next, current, newCost, ref frontier, ref travelMap, movementParams);
                        }
                    }
                }
            }

            return travelMap;
        }

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
        private static void QueueForRange(RHTile testTile, RHTile lastTile, double newCost, ref PriorityQueue<RHTile> frontier, ref TravelMap travelMap, bool testForMovement)
        {
            //If we are not testing for movement, then do queue the RHTile, otherwise we need to both be able
            //to target the tile, move through the tile for size and walk through it for allies
            if (!testForMovement || (testTile.CanTargetTile() && TestTileForSize(testTile, false) && testTile.CanWalkThroughInCombat()))
            {
                frontier.Enqueue(testTile, newCost);
                travelMap.Store(testTile, lastTile, newCost);

                //Do not highlight tiles that cannot be targeted and if we are testing
                //for movement, we need to be able to land on the tile with our size in mind
                if (testTile.CanTargetTile() && (!testForMovement || TestTileForSize(testTile, true)))
                {
                    travelMap[testTile].InRange = true;
                }
            }
        }

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
                    if(testForEnding && lastTile.Character != null && lastTile.Character != _actTraveller) {
                        return false;
                    }

                    lastTile = lastTile?.GetTileByDirection(GameManager.DirectionEnum.Right);
                }

                //Reset to the first Tile in the current row and go down one
                lastTile = rowTile.GetTileByDirection(GameManager.DirectionEnum.Down);
            }

            return true;
        }

        private static double HeuristicToTile(RHTile a, RHTile b)
        {
            int total = 0;
            int distance = (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));

            //Do not perform any wall buffer checks if we are in combat
            List<RHTile> futureTiles = a.GetWalkableNeighbours();
            int wallBuffer = CombatManager.InCombat ? 0 : ((futureTiles.Count < 4) ? 10 : 0);

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
                int wallBuffer = CombatManager.InCombat ? 0 : ((futureTiles.Count < 4) ? 10 : 0);

                int multiplier = GetMovementCost(a);

                total = (distance + wallBuffer) * multiplier;
            }
            return -total;
        }

        //Returns how much it costs to enter the next square
        private static int GetMovementCost(RHTile target)
        {
            return target.IsRoad ? 1 : GetDefaultCost();
        }

        /// <summary>
        /// This gets the default set cost of movement for an Actor.
        /// However, in combat, we don't want to complicate things, so if we are
        /// in combat and the actr is on the same map as the Player, the value is 1.
        /// </summary>
        /// <returns>The default cost for movement</returns>
        private static int GetDefaultCost()
        {
            if (CombatManager.InCombat && _actTraveller?.CurrentMapName == MapManager.CurrentMap.Name)
            {
                return 1;
            }
            else
            {
                return DEFAULT_COST;
            }
        }

        public static void SetParams(int size, WorldActor act, int maxPath = -1)
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
