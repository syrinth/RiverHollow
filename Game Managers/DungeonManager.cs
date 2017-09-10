
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Adventure.Game_Managers
{
    static class DungeonManager
    {
        private static int _numRooms = 5;

        private static List<TileMap> _maps = new List<TileMap>();
        public static List<TileMap> Maps { get => _maps; }
        //MapName/<Direction, ToMap>
        private static Dictionary<string, KeyValuePair<string,string>> _backwardsMapKey = new Dictionary<string, KeyValuePair<string, string>>();
        private static Dictionary<string, string> _forwardsMapKey = new Dictionary<string, string>();
        private static int _currentIndex = 0;
        public static int CurrentIndex { get => _currentIndex; }
        public static Rectangle Entrance;

        public static void LoadNewDungeon(AdventureMap map)
        {
            _maps = new List<TileMap>();
            _backwardsMapKey = new Dictionary<string, KeyValuePair<string, string>>();
            _forwardsMapKey = new Dictionary<string, string>();
            int numRooms = map.Difficulty;
            Random r = new Random();
            string dungeonPrefix = @"Dungeons\Room";
            _maps.Add(MapManager.Maps[dungeonPrefix + 1]);// r.Next(1, _numRooms+1);
            while (numRooms > 0)
            {
                int val = -1;
                do
                {
                    val = r.Next(1, _numRooms + 1);
                } while (_maps.Contains(MapManager.Maps[dungeonPrefix + val]));

                _maps.Add(MapManager.Maps[dungeonPrefix + val]);
                numRooms--;
            }

            List<string> directions = new List<string> { "North", "South", "East", "West" };
            int lastDir = r.Next(0, 4);
            for (int i = 0; i < _maps.Count; i++)
            {
                TileMap m = _maps[i];
                
                int dirToUnlock = ReverseDirections(lastDir);

                //Need to reverse the value since we need to open the "east" door, when you come from the "West"

                int dirNextRoom = -1;
                do
                {
                    dirNextRoom = r.Next(0, 4);
                } while (dirNextRoom == dirToUnlock);

                m.LayerVisible(directions[dirToUnlock], false);
                m.LayerVisible(directions[dirNextRoom], false);

                string directionKey = string.Empty;
                foreach (var kvp in m.ExitDictionary)
                {
                    if (kvp.Value.Equals(directions[dirToUnlock]))
                    {
                        directionKey = kvp.Value;
                        break;
                    }
                }

                _backwardsMapKey.Add(m.Name, new KeyValuePair<string, string>(directionKey, (i == 0) ? "Map1" : _maps[i-1].Name));

                if (i == 0)
                {
                    foreach (var kvp in m.EntranceDictionary)
                    {
                        if (kvp.Key.Equals(directions[lastDir]))
                        {
                            Entrance = kvp.Value;
                            break;
                        }
                    }
                }
                else if (i < _maps.Count - 1)
                {
                ////    _forwardsMapKey.Add(rectBackwards, _maps[i + 1].Name);
                }
                else
                {
                    m.LayerVisible(directions[dirNextRoom], true);
                }
                lastDir = dirNextRoom;
            }
        }

        public static int ReverseDirections(int direction)
        {
            if (direction == 0) return 1;
            else if (direction == 1) return 0;
            else if (direction == 2) return 3;
            else return 2;
        }

        public static void ClearDungeon()
        {
            _maps.Clear();
        }

        public static TileMap RoomChange(string direction)
        {
            if (_backwardsMapKey[_maps[_currentIndex].Name].Key.Equals(direction))
            {
                if (_currentIndex > 0)
                {
                    return MapManager.Maps[_backwardsMapKey[_maps[_currentIndex--].Name].Value];
                }
                else
                {
                    return MapManager.Maps[@"Map1"];
                }
            }
            return _maps[++_currentIndex];
        }
    }
}
