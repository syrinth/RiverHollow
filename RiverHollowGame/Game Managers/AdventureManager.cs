using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Game_Managers
{
    internal static class AdventureManager
    {
        private static List<int> _liUnlockedAdventures;
        private static List<KeyValuePair<int, int>> _liAssignedAdventures;

        public static void Initialize(){
            _liUnlockedAdventures = new List<int> { 0 };
            _liAssignedAdventures = new List<KeyValuePair<int, int>>();
        }

        public static List<Adventure> GetUnlockedAdventures()
        {
            var rv = new List<Adventure>();

            foreach(int i in _liUnlockedAdventures)
            {
                rv.Add(new Adventure(i));
            }

            return rv;
        }

        public static void UnlockAdventure(int id)
        {
            _liUnlockedAdventures.Add(id);
        }

        #region Save Handlers
        public static AdventureManagerData SaveData()
        {
            AdventureManagerData data = new AdventureManagerData
            {
                AssignedAdventures = new List<string>(),
                UnlockedAdventures = _liUnlockedAdventures
            };

            foreach (var kvp in _liAssignedAdventures)
            {
                data.AssignedAdventures.Add(string.Format("{0}-{1}", kvp.Key, kvp.Value));
            }

            return data;
        }
        public static void LoadData(AdventureManagerData data)
        {
            _liAssignedAdventures = new List<KeyValuePair<int, int>>();

            foreach (var str in data.AssignedAdventures)
            {
                var split = Util.FindIntArguments(str);
                _liAssignedAdventures.Add(new KeyValuePair<int, int>(split[0], split[1]));
            }

            _liUnlockedAdventures = data.UnlockedAdventures;
        }
        #endregion
    }
}
