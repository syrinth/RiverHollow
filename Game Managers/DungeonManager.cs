
using Adventure.Items;
using System;
using System.Collections.Generic;

namespace Adventure.Game_Managers
{
    static class DungeonManager
    {
        private static int _numRooms = 5;

        private static List<string> _maps = new List<string>();
        public static List<string> Maps { get => _maps; }
        private static int _currentIndex = 0;
        public static int CurrentIndex { get => _currentIndex; }

        public static void LoadNewDungeon(AdventureMap map)
        {
            _maps = new List<string>();
            int numRooms = map.Difficulty;
            Random r = new Random();
            string dungeonPrefix = @"Dungeons\Room";
            _maps.Add(dungeonPrefix + 1);// r.Next(1, _numRooms+1);
            while (numRooms > 0)
            {
                int val = -1;
                do
                {
                    val = r.Next(1, _numRooms + 1);
                } while (_maps.Contains(dungeonPrefix + val));

                _maps.Add(dungeonPrefix + val);
                numRooms--;
            }
        }

        public static void ClearDungeon()
        {
            _maps.Clear();
        }

        public static string NextRoom(string lastRoom)
        {
            return _maps[++_currentIndex];
        }
    }
}
