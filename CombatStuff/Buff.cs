using System.Collections.Generic;

namespace RiverHollow.Characters.CombatStuff
{
    public class Buff
    {
        private int _id;
        private string _name;
        public string Name { get => _name; }
        public int Duration;
        private List<KeyValuePair<string, int>> _stats;
        public List<KeyValuePair<string, int>> StatMods { get => _stats; }
        private int _conditionID;
        private string _description;
        public string Description { get => _description; }

        public Buff(int id, string[] stringData)
        {
            _stats = new List<KeyValuePair<string, int>>();
            ImportBasics(id, stringData);
        }
        protected int ImportBasics(int id, string[] stringData)
        {
            int i = 0;
            _name = stringData[i++];
            _description = stringData[i++];
            string[] split = stringData[i++].Split(' ');

            //This is where we parse for stats effected
            foreach (string s in split)
            {
                string[] statMods = s.Split(':');
                _stats.Add(new KeyValuePair<string, int>(statMods[0], int.Parse(statMods[1])));
            }

            _id = id;

            return i;
        }
    }
}
