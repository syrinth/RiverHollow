using Microsoft.Xna.Framework;
using RiverHollow.Tile_Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers
{
    public static class TravelManager
    {
        private static Dictionary<string, Node> _diNodes;

        public static void Calculate()
        {
            _diNodes = new Dictionary<string, Node>();

            foreach (RHMap map in MapManager.Maps.Values)
            {
                if (!map.WorkerBuilding && !map.IsDungeon)
                {
                    _diNodes.Add(map.Name, new Node(map));
                }
            }
        }

        public static List<RHTile> GetRoute(string currMap, string fromMap, string toMap)
        {
            return _diNodes[currMap].GetRoute(fromMap, toMap);
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
        const int slowCost = 100;
        static Dictionary<string, List<RHTile>> _dictMapPathing;
        static Dictionary<RHTile, RHTile> cameFrom = new Dictionary<RHTile, RHTile>();
        static Dictionary<RHTile, double> costSoFar = new Dictionary<RHTile, double>();
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
        }

        public static List<RHTile> FindPathToOtherMap(string findKey, ref string mapName, ref Vector2 newStart)
        {
            List<RHTile> _completeTilePath = new List<RHTile>();
            _dictMapPathing = new Dictionary<string, List<RHTile>>();
            Vector2 start = newStart;
            Dictionary<string, string> mapCameFrom = new Dictionary<string, string>();
            Dictionary<string, double> mapCostSoFar = new Dictionary<string, double>();

            //Initialize pathfinding. Add the start node to the frontier
            var frontier = new PriorityQueue<string>();
            frontier.Enqueue(mapName, 0);
            mapCameFrom[mapName] = mapName;
            mapCostSoFar[mapName] = 0;

            //As long as there are still nodes to explore, we need to keep checking
            while (frontier.Count > 0)
            {
                var testMapStr = frontier.Dequeue();           //The map with the shortest path
                string testMap = testMapStr.Split(':')[0];    //The map we're currently testing. Split is for multiple entrances from same map
                string fromMap = mapCameFrom[testMapStr];      //The map we came from

                //If the from map isn't the testing map, set the start point at the entrance from the from map
                if (fromMap != testMapStr)
                {
                    start = MapManager.Maps[testMap].DictionaryEntrance[fromMap].Location.ToVector2();
                }

                //If the testMap does contain the key that we're looking for then we need to pathfind from the entrance to the key
                if (MapManager.Maps[testMap].DictionaryCharacterLayer.ContainsKey(findKey))
                {
                    mapName = testMapStr;
                    newStart = MapManager.Maps[testMap].DictionaryCharacterLayer[findKey];
                    List<RHTile> pathToExit = FindPathToLocation(ref start, MapManager.Maps[testMap].DictionaryCharacterLayer[findKey], testMapStr);
                    fromMap = mapCameFrom[testMapStr];
                    List<List<RHTile>> _totalPath = new List<List<RHTile>>();
                    _totalPath.Add(pathToExit);
                    while (fromMap != testMapStr)
                    {
                        _totalPath.Add(_dictMapPathing[fromMap + ":" + testMapStr]);
                        testMapStr = fromMap;
                        fromMap = mapCameFrom[testMapStr];
                    }
                    _totalPath.Reverse();

                    foreach (List<RHTile> l in _totalPath)
                    {
                        _completeTilePath.AddRange(l);
                    }

                    //We found it, so break the fuck out of the loop
                    break;
                }

                //Iterate over the exits in the map we're testing and pathfind to them.
                foreach (KeyValuePair<Rectangle, string> exit in MapManager.Maps[testMap].DictionaryExit)
                {
                    //Find the shortest patht o the exit in question
                    
                    List<RHTile> pathToExit = FindPathToLocation(ref start, exit.Key.Location.ToVector2(), testMapStr);
                    if (pathToExit != null)
                    {
                        double newCost = mapCostSoFar[testMapStr] + pathToExit.Count;
                        if (!mapCostSoFar.ContainsKey(exit.Value))
                        {
                            mapCostSoFar[exit.Value] = newCost + pathToExit.Count;
                            frontier.Enqueue(exit.Value, newCost);
                            string[] split = null;
                            if (exit.Value.Contains(":"))
                            {
                                split = exit.Value.Split(':');
                            }
                            mapCameFrom[exit.Value] = (split == null) ? testMap : testMap + ":" + split[1];
                            _dictMapPathing[testMapStr + ":" + exit.Value] = pathToExit; // This needd another key for the appropriate exit
                        }
                    }

                    ClearPathingTracks();
                }
            }

            return _completeTilePath;
        }

        //Pathfinds from one point to another on a given map
        public static List<RHTile> FindPathToLocation(ref Vector2 start, Vector2 target, string mapName)
        {
            List<RHTile> returnList = null;
            RHMap map = MapManager.Maps[mapName.Split(':')[0]];
            RHTile startTile = map.RetrieveTile(start.ToPoint());
            RHTile goalNode = map.RetrieveTile(target.ToPoint());
            var frontier = new PriorityQueue<RHTile>();
            frontier.Enqueue(startTile, 0);

            cameFrom[startTile] = startTile;
            costSoFar[startTile] = 0;
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                if (current.Equals(goalNode))
                {
                    returnList = BackTrack(current);
                    start = current.Position;
                    break;
                }

                foreach (var next in current.GetWalkableNeighbours())
                {
                    double newCost = costSoFar[current] + GetMovementCost(next);

                    if (!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = newCost + Heuristic(next, goalNode);

                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
            return returnList;
        }
        private static List<RHTile> BackTrack(RHTile current)
        {
            List<RHTile> list = new List<RHTile>();
            if (current.MapName.Equals("mapForest"))
            {
                int kj = 2;
            }
            while (current != cameFrom[current])
            {
                list.Add(current);
                current = cameFrom[current];
            }

            list.Reverse();
            //foreach (RHTile t in list)
            //{
            //    using (StreamWriter file = new System.IO.StreamWriter(current.MapName + "_" + current.X +"-" +current.Y + ".txt"))
            //    {
            //        file.WriteLine(String.Format("[{0},{1}]{2}", t.X, t.Y, System.Environment.NewLine));
            //    }
            //}

            return list;
        }
        private static double Heuristic(RHTile a, RHTile b)
        {
            int total = 0;
            int distance = (Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));

            List<RHTile> futureTiles = a.GetWalkableNeighbours();
            int wallBuffer = (futureTiles.Count < 4) ? 10 : 0;

            int multiplier = (a.IsRoad ? 1 : slowCost);

            total = (distance + wallBuffer) * multiplier;
            return total;
        }

        //Returns how much it costs to enter the next square
        private static int GetMovementCost(RHTile target)
        {
            return target.IsRoad ? 1 : slowCost;
        }
        public static void ClearPathingTracks()
        {
            cameFrom.Clear();
            costSoFar.Clear();
        }
        #endregion

        /*
         * A Node is explicitly for pathfinding between maps and their connections.
         * Nodes do not store connections between map Objects and Exits
         */
        private class Node
        {
            string _sName;
            RHMap _map;
            Dictionary<string, Dictionary<string, List<RHTile>>> _diRoutes;         //Key-Where you start, Val Dict Key -Where you go, Tiles?

            public Node(RHMap map)
            {
                _sName = map.Name;
                _map = map;
                _diRoutes = new Dictionary<string, Dictionary<string, List<RHTile>>>();

                foreach (KeyValuePair<string, Rectangle> kvpEntrance in map.DictionaryEntrance)
                {
                    _diRoutes[kvpEntrance.Key] = new Dictionary<string, List<RHTile>>();
                    foreach (KeyValuePair<Rectangle, string> kvpExit in map.DictionaryExit)
                    {
                        if (kvpEntrance.Key != kvpExit.Value)
                        {
                            Vector2 start = kvpEntrance.Value.Center.ToVector2();
                            _diRoutes[kvpEntrance.Key][kvpExit.Value] = TravelManager.FindPathToLocation(ref start, kvpExit.Key.Center.ToVector2(), _map.Name);
                            ClearPathingTracks();
                        }
                    }
                }
            }

            public List<RHTile> GetRoute(string fromMap, string toMap)
            {
                List<RHTile> rv = new List<RHTile>();
                if (_diRoutes[fromMap].ContainsKey(toMap))
                {
                    rv = _diRoutes[fromMap][toMap];
                }
                return rv;
            }

            public List<string> GetMaps()
            {
                return _diRoutes.Keys.ToList<string>();
            }

            public double GetRouteLength(string fromMap, string toMap)
            {
                return _diRoutes[fromMap][toMap].Count;
            }
        }
    }
}
