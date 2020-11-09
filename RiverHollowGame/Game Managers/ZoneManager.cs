using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    public static class ZoneManager
    {
        private static ZoneInfo _ziForest;
        private static ZoneInfo _ziMountain;
        private static ZoneInfo _ziField;
        private static ZoneInfo _ziSwamp;
        private static ZoneInfo _ziTown;

        public static void Initialize()
        {
            _ziForest = new ZoneInfo(GameManager.ZoneEnum.Forest);
            _ziMountain = new ZoneInfo(GameManager.ZoneEnum.Mountain);
            _ziField = new ZoneInfo(GameManager.ZoneEnum.Field);
            _ziSwamp = new ZoneInfo(GameManager.ZoneEnum.Swamp);
            _ziTown = new ZoneInfo(GameManager.ZoneEnum.Town);
        }

        public static void AddMap(GameManager.ZoneEnum zone, string mapName)
        {
            switch (zone)
            {
                case GameManager.ZoneEnum.Field:
                    _ziField.AddMap(mapName);
                    break;
                case GameManager.ZoneEnum.Forest:
                    _ziForest.AddMap(mapName);
                    break;
                case GameManager.ZoneEnum.Mountain:
                    _ziMountain.AddMap(mapName);
                    break;
                case GameManager.ZoneEnum.Swamp:
                    _ziSwamp.AddMap(mapName);
                    break;
                case GameManager.ZoneEnum.Town:
                    _ziTown.AddMap(mapName);
                    break;
            }
        }
    }

    public class ZoneInfo
    {
        int _iZoneLevel;
        public int ZoneLevel => _iZoneLevel;
        GameManager.ZoneEnum _eZone;
        List<string> _liMaps;

        public ZoneInfo(GameManager.ZoneEnum zone)
        {
            _iZoneLevel = 1;
            _eZone = zone;
            _liMaps = new List<string>();
        }

        public void AddMap(string mapName)
        {
            _liMaps.Add(mapName);
        }
    }
}
