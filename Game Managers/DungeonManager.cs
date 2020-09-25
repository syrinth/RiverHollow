using Microsoft.Xna.Framework;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    public static class DungeonManager
    {
        public static Dungeon CurrentDungeon => (_liDungeons.ContainsKey(MapManager.CurrentMap.DungeonName) ? _liDungeons[MapManager.CurrentMap.DungeonName] : null);
        private static Dictionary<string, Dungeon> _liDungeons;

        public static void Instantiate()
        {
            _liDungeons = new Dictionary<string, Dungeon>();
        }

        public static void GoToEntrance()
        {
            CurrentDungeon?.GoToEntrance();
            GUIManager.CloseMainObject();
        }

        public static void AddMapToDungeon(string dungeonName, RHMap map) {
            if (!_liDungeons.ContainsKey(dungeonName))
            {
                _liDungeons[dungeonName] = new Dungeon(dungeonName);
            }

            _liDungeons[dungeonName].AddMap(map);
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
        string _sEntranceMapName;
        Vector2 _vRecallPoint;
        public int NumKeys { get; private set; }
        public string Name { get; private set; }

        private List<string> _liMapNames;
        public Dungeon(string name)
        {
            _liMapNames = new List<string>();
            Name = name;
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
