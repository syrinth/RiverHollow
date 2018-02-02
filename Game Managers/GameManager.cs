using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    public static class GameManager
    {
        public static float Scale = 1f;
        public static Dictionary<string, Upgrade> DiUpgrades;

        public static void LoadContent(ContentManager Content)
        {
            DiUpgrades = new Dictionary<string, Upgrade>();
            foreach (KeyValuePair<string, string> kvp in GameContentManager.DiUpgrades)
            {
                DiUpgrades.Add(kvp.Key, new Upgrade(kvp.Key, kvp.Value));
            }
        }

        #region States
        private static bool _scrying;
        private enum State { Paused, Running, Information, Input }
        private static State _gameState;

        private enum Map { None, WorldMap, Combat }
        private static Map _mapState;

        public static void ReadInput() { _gameState = State.Input; }
        public static bool TakingInput() { return _gameState == State.Input; }

        public static void Pause() { _gameState = State.Paused; }
        public static bool IsPaused() { return _gameState == State.Paused; }

        public static void Unpause() { _gameState = State.Running; }
        public static bool IsRunning() { return _gameState == State.Running; }

        public static void Scry(bool val) {
            _gameState = State.Running;
            _scrying = val;
        }
        public static bool Scrying() { return _scrying; }

        public static bool InCombat() { return _mapState == Map.Combat; }
        public static void GoToCombat() { _mapState = Map.Combat; }
        public static bool OnMap() { return _mapState == Map.WorldMap; }
        public static void GoToWorldMap() { _mapState = Map.WorldMap; }
        public static bool Informational() { return _mapState == Map.None; }
        public static void GoToInformation() {
            _mapState = Map.None;
            _gameState = State.Paused;
        }

        public static void BackToMain()
        {
            GUIManager.SetScreen(GUIManager.Screens.HUD);
            _gameState = State.Running;
            _mapState = Map.WorldMap;
        }
        #endregion
    }

    public class Upgrade
    {
        string _id;
        public string ID { get => _id; }
        string _name;
        public string Name { get => _name; }
        string _description;
        public string Description { get => _description; }
        int _moneyCost;
        public int MoneyCost { get => _moneyCost; }
        List<KeyValuePair<int, int>> _liRequiredItems;
        public List<KeyValuePair<int, int>> LiRquiredItems { get => _liRequiredItems; }
        public bool Enabled;

        public Upgrade(string id, string strData)
        {
            _id = id;
            string[] split = strData.Split('/');
            _name = split[0];
            _description = split[1];
            _moneyCost = int.Parse(split[2]);
            string[] itemSplit = split[3].Split(' ');

            _liRequiredItems = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < itemSplit.Length; i++)
            {
                string[] entrySplit = itemSplit[i].Split(':');
                _liRequiredItems.Add(new KeyValuePair<int, int>(int.Parse(entrySplit[0]), int.Parse(entrySplit[1])));
            }
        }
    }
}

//internal static class GameStateManager {    }
