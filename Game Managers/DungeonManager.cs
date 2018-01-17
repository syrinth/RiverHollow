﻿using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using RiverHollow.Misc;

namespace RiverHollow.Game_Managers
{
    static class DungeonManager
    {
        private static int _numRooms = 5;
        public enum EndCondition { TreasureChest, KillAll };
        private static EndCondition _condition = EndCondition.TreasureChest;
        private static Container _endChest;

        private static List<RHMap> _maps = new List<RHMap>();
        public static List<RHMap> Maps { get => _maps; }
        //MapName/<Direction, ToMap>
        private static Dictionary<string, KeyValuePair<string,string>> _backwardsMapKey = new Dictionary<string, KeyValuePair<string, string>>();
        private static Dictionary<string, string> _forwardsMapKey = new Dictionary<string, string>();
        private static int _currentIndex = 0;
        public static int CurrentIndex { get => _currentIndex; }
        public static Rectangle Entrance;

        public static void LoadNewDungeon(AdventureMap map)
        {
            _maps = new List<RHMap>();
            _backwardsMapKey = new Dictionary<string, KeyValuePair<string, string>>();
            _forwardsMapKey = new Dictionary<string, string>();
            int maxRooms = map.Difficulty;
            RHRandom r = new RHRandom();
            string dungeonPrefix = @"Dungeons\Room";
            _maps.Add(MapManager.Maps[dungeonPrefix + 1]);// r.Next(1, _numRooms+1);
            PopulateRoom(r, _maps[0], false);
            int numRoom = 0;
            while (numRoom < maxRooms)
            {
                int val = -1;
                do
                {
                    val = r.Next(1, _numRooms);
                } while (_maps.Contains(MapManager.Maps[dungeonPrefix + val]));

                RHMap newMap = MapManager.Maps[dungeonPrefix + val];
                _maps.Add(MapManager.Maps[dungeonPrefix + val]);
                PopulateRoom(r, newMap, numRoom == maxRooms - 1);
                numRoom++;
            }

            List<string> directions = new List<string> { "North", "South", "East", "West" };
            int lastDir = r.Next(0, 3);
            for (int i = 0; i < _maps.Count; i++)
            {
                RHMap m = _maps[i];
                
                int dirToUnlock = ReverseDirections(lastDir);

                //Need to reverse the value since we need to open the "east" door, when you come from the "West"

                int dirNextRoom = -1;
                do
                {
                    dirNextRoom = r.Next(0, 3);
                } while (dirNextRoom == dirToUnlock);

                m.LayerVisible(directions[dirToUnlock], false);
                m.LayerVisible(directions[dirNextRoom], false);

                string directionKey = string.Empty;
                foreach (var kvp in m.DictionaryExit)
                {
                    if (kvp.Value.Equals(directions[dirToUnlock]))
                    {
                        directionKey = kvp.Value;
                        break;
                    }
                }

                _backwardsMapKey.Add(m.Name, new KeyValuePair<string, string>(directionKey, (i == 0) ? "NearWilds" : _maps[i-1].Name));

                if (i == 0)
                {
                    foreach (var kvp in m.DictionaryEntrance)
                    {
                        if (kvp.Key.Equals(directions[lastDir]))
                        {
                            Entrance = kvp.Value;
                            break;
                        }
                    }
                }
                else if (i >= _maps.Count - 1)
                {
                    m.LayerVisible(directions[dirNextRoom], true);
                }
                lastDir = dirNextRoom;
            }
        }

        public static void PopulateRoom(RHRandom r, RHMap m, bool lastRoom)
        {
            int mapWidth = m.MapWidth;
            int mapHeight = m.MapHeight;
            for (int i = 0; i < 5; i++)
            {
                Vector2 vect = new Vector2(r.Next(1, mapWidth-1) * RHMap.TileSize, r.Next(1, mapHeight-1) * RHMap.TileSize);
                m.AddWorldObject(ObjectManager.GetWorldObject(1, vect), true);
            }
            for (int i = 0; i < 30; i++)
            {
                Vector2 vect = new Vector2(r.Next(1, mapWidth-1) * RHMap.TileSize, r.Next(1, mapHeight-1) * RHMap.TileSize);
                m.AddWorldObject(ObjectManager.GetWorldObject(0, vect), true);
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vect = new Vector2(r.Next(1, mapWidth-1) * RHMap.TileSize, r.Next(1, mapHeight-2) * RHMap.TileSize);
                Mob mob = CharacterManager.GetMobByIndex(1, vect);
                mob.CurrentMapName = m.Name.Replace(@"Maps\", "");
                m.AddMob(mob);
            }
            for (int i = 0; i < 5; i++)
            {
                Vector2 vect = new Vector2(r.Next(1, mapWidth - 1) * RHMap.TileSize, r.Next(1, mapHeight - 2) * RHMap.TileSize);
                Mob mob = CharacterManager.GetMobByIndex(2, vect);
                mob.CurrentMapName = m.Name.Replace(@"Maps\", "");
                m.AddMob(mob);
            }

            if (lastRoom && _condition == EndCondition.TreasureChest)
            {
                Vector2 vect = new Vector2(r.Next(1, mapWidth-1) * RHMap.TileSize, r.Next(1, mapHeight-1) * RHMap.TileSize);
                Container c = (Container)ObjectManager.GetItem(6);
                m.PlaceStaticItem(c, vect);
                _endChest = c;
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

        public static RHMap RoomChange(string direction, bool straightOut = false)
        {
            if (straightOut)
            {
                return MapManager.Maps[@"NearWilds"];
            }
            else
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
            }
            return _maps[++_currentIndex];
        }

        public static bool IsEndChest(Container c)
        {
            return c == _endChest;
        }
    }
}
