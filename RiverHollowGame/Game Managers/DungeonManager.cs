using Microsoft.Xna.Framework;
using MonoGame.Extended.Tiled;
using RiverHollow.WorldObjects;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    public static class DungeonManager
    {
        public static Dungeon CurrentDungeon => (_diDungeons.ContainsKey(MapManager.CurrentMap.DungeonName) ? _diDungeons[MapManager.CurrentMap.DungeonName] : null);
        private static Dictionary<string, Dungeon> _diDungeons;

        public static void Instantiate()
        {
            _diDungeons = new Dictionary<string, Dungeon>();
        }

        public static void ResetDungeons()
        {
            foreach(Dungeon d in _diDungeons.Values)
            {
                d.ResetDungeon();
            }
        }

        public static void GoToEntrance()
        {
            CurrentDungeon?.GoToEntrance();
            GUIManager.CloseMainObject();
        }

        public static void AddMapToDungeon(string dungeonName, bool procedural, RHMap map) {
            if (!_diDungeons.ContainsKey(dungeonName))
            {
               // if (procedural) { _diDungeons[dungeonName] = new ProceduralDungeon(dungeonName); }
                //else { _diDungeons[dungeonName] = new Dungeon(dungeonName); }
            }

            _diDungeons[dungeonName].AddMap(map);
        }
        public static void AddTriggerObject(string dungeonName, TriggerObject obj)
        {
            _diDungeons[dungeonName].AddTriggerObject(obj);
        }
        public static void AddWarpPoint(WarpPoint obj, string dungeon)
        {
            _diDungeons[dungeon].AddWarpPoint(obj);
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
            if (!string.IsNullOrEmpty(MapManager.CurrentMap.DungeonName))
            {
                _diDungeons[dungeonName]?.ActivateTrigger(triggerName);
            }
        }

        public static void InitializeProceduralDungeon(string dungeonName, string currentMap, TravelPoint pt)
        {
           // ((ProceduralDungeon)_diDungeons[dungeonName]).InitializeDungeon(currentMap, pt);
        }
    }
}
