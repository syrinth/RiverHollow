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
