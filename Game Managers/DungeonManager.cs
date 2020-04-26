using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Game_Managers
{
    public static class DungeonManager
    {
        private static Dungeon CurrentDungeon => (_liDungeons.ContainsKey(MapManager.CurrentMap.DungeonName) ? _liDungeons[MapManager.CurrentMap.DungeonName]: null);
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

        public static void ActivateTrigger(string dungeonName, string triggerName)
        {
            _liDungeons[dungeonName]?.Trigger(triggerName);
        }
    }

    public class Dungeon
    {
        private int _iKeys;
        public int NumKeys => _iKeys;
        private string _sName;
        public String Name { get { return _sName; } }

        private List<string> _liMapNames;
        public Dungeon(string name)
        {
            _liMapNames = new List<string>();
            _sName = name;
        }

        public void AddMap(string name)
        {
            _liMapNames.Add(name);
        }

        public void AddKey() { _iKeys++; }
        public void UseKey() { _iKeys--; }

        public void Trigger(string triggerName)
        {
            foreach(string mapName in _liMapNames)
            {
                MapManager.Maps[mapName].Trigger(triggerName);
            }
        }

    }
}
