using Microsoft.Xna.Framework;
using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.GameManager;

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

        public static void InitializeDungeon()
        {
            _liDungeons["Cave"].InitializeDungeon();
        }
    }

    public class Dungeon
    {
        const int DUNGEON_SIZE = 6;
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

        public void InitializeDungeon()
        {
            RHMap lastMap = MapManager.Maps["mapCave_Entrance"];
            DirectionEnum lastDir = DirectionEnum.Down;

            List<string> copy = new List<string>(_liMapNames.FindAll(x => x.StartsWith("mapCave__")));
            for (int i= 0; i < DUNGEON_SIZE; i++){
                int randomInt = RHRandom.Instance().Next(copy.Count - 1); 
                RHMap nextMap = MapManager.Maps[copy[randomInt]];

                //Remove the map from consideration
                copy.RemoveAt(randomInt);

                //Get the opposite direction of the last map
                DirectionEnum oppDir = Util.GetOpposite(lastDir);

                //Find the TravelPoints on each map that touch each other
                TravelPoint lastMapTravelPt = lastMap.DictionaryTravelPoints[Util.GetEnumString(lastDir)];
                TravelPoint nextMapTravelPt = nextMap.DictionaryTravelPoints[Util.GetEnumString(oppDir)];

                lastMapTravelPt.AssignLinkedMap(nextMap.Name);
                nextMapTravelPt.AssignLinkedMap(lastMap.Name);

                //Remove the initial link between the Direction and the Travelpoint
                //Assign the appropriate map key
                lastMap.DictionaryTravelPoints.Remove(Util.GetEnumString(lastDir));
                lastMap.DictionaryTravelPoints[lastMapTravelPt.LinkedMap] = lastMapTravelPt;

                nextMap.DictionaryTravelPoints.Remove(Util.GetEnumString(oppDir));
                nextMap.DictionaryTravelPoints[nextMapTravelPt.LinkedMap] = nextMapTravelPt;

                lastMap = nextMap;

                List<DirectionEnum> dirList = Enum.GetValues(typeof(DirectionEnum)).Cast<DirectionEnum>().ToList();
                dirList.Remove(oppDir);

                lastDir = dirList[RHRandom.Instance().Next(dirList.Count - 1)];
            }
        }

        public void AddKey() { NumKeys++; }
        public void UseKey() { NumKeys--; }
    }
}
