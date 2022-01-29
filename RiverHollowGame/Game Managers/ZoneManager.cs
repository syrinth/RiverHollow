using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

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
            _ziForest = new ZoneInfo(ZoneEnum.Forest);
            _ziMountain = new ZoneInfo(ZoneEnum.Mountain);
            _ziField = new ZoneInfo(ZoneEnum.Field);
            _ziSwamp = new ZoneInfo(ZoneEnum.Swamp);
            _ziTown = new ZoneInfo(ZoneEnum.Town);
        }

        public static void AddMap(ZoneEnum zone, string mapName)
        {
            switch (zone)
            {
                case ZoneEnum.Field:
                    _ziField.AddMap(mapName);
                    break;
                case ZoneEnum.Forest:
                    _ziForest.AddMap(mapName);
                    break;
                case ZoneEnum.Mountain:
                    _ziMountain.AddMap(mapName);
                    break;
                case ZoneEnum.Swamp:
                    _ziSwamp.AddMap(mapName);
                    break;
                case ZoneEnum.Town:
                    _ziTown.AddMap(mapName);
                    break;
            }
        }
    }

    public class ZoneInfo
    {
        int _iZoneLevel;
        public int ZoneLevel => _iZoneLevel;
        ZoneEnum _eZone;
        List<string> _liMaps;

        public ZoneInfo(ZoneEnum zone)
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
