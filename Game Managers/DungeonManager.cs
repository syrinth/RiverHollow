using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers
{
    public static class DungeonManager
    {
        private static Dungeon CurrentDungeon => (_liDungeons.ContainsKey(MapManager.CurrentMap.DungeonName) ? _liDungeons[MapManager.CurrentMap.DungeonName] : null);
        private static Dictionary<string, Dungeon> _liDungeons;

        public static void Instantiate()
        {
            _liDungeons = new Dictionary<string, Dungeon>();
        }

        public static void AddMapToDungeon(string dungeonName, string mapName) {
            if (!_liDungeons.ContainsKey(dungeonName))
            {
                _liDungeons[dungeonName] = new Dungeon(dungeonName);
            }

            _liDungeons[dungeonName].AddMap(mapName);
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

        public static void ActivateTrigger(string triggerName)
        {
            ActivateTrigger(MapManager.CurrentMap.DungeonName, triggerName);
        }

        public static void ActivateTrigger(string dungeonName, string triggerName)
        {
            _liDungeons[dungeonName]?.ActivateTrigger(triggerName);
        }
    }

    public class Dungeon
    {
        public int NumKeys { get; private set; }
        public string Name { get; private set; }

        private List<string> _liMapNames;
        public Dungeon(string name)
        {
            _liMapNames = new List<string>();
            Name = name;
        }

        public void AddMap(string name)
        {
            _liMapNames.Add(name);
        }

        public void AddKey() { NumKeys++; }
        public void UseKey() { NumKeys--; }

        public void ActivateTrigger(string triggerName)
        {
            foreach(string mapName in _liMapNames)
            {
                MapManager.Maps[mapName].ActivateTrigger(triggerName);
            }
        }

    }
}
