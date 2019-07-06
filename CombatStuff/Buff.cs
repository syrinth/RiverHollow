using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using System.Collections.Generic;

namespace RiverHollow.Actors.CombatStuff
{
    public class StatusEffect
    {
        public CombatActor Caster;
        public bool DoT;
        public bool _bSong;
        public bool Song => _bSong;
        private int _id;
        private string _sName;
        public string Name { get => _sName; }
        public int Potency;
        public int Duration;
        private List<KeyValuePair<string, int>> _liStats;
        public List<KeyValuePair<string, int>> StatMods  => _liStats;
        private int _conditionID;
        private string _sDescription;
        public string Description { get => _sDescription; }

        private bool _bCounter;
        public bool Counter => _bCounter;

        public StatusEffect(int id, Dictionary<string, string> data)
        {
            _id = id;
            GameContentManager.GetStatusEffectText(id, ref _sName, ref _sDescription);

            _liStats = new List<KeyValuePair<string, int>>();
            ImportBasics(id, data);
        }
        protected void ImportBasics(int id, Dictionary<string, string> data)
        {
            //This is where we parse for stats effected
            if (data.ContainsKey("Buff"))
            {
                string[] splitEffects = data["Buff"].Split(' ');
                foreach (string effect in splitEffects)
                {
                    string[] statMods = effect.Split('-');
                    _liStats.Add(new KeyValuePair<string, int>(statMods[0], int.Parse(statMods[1])));
                }
            }

            if (data.ContainsKey("Debuff"))
            {
                string[] splitEffects = data["Debuff"].Split(' ');
                foreach (string effect in splitEffects)
                {
                    string[] statMods = effect.Split('-');
                    _liStats.Add(new KeyValuePair<string, int>(statMods[0], -int.Parse(statMods[1])));
                }
            }

            _bSong = data.ContainsKey("Song");

            _bCounter = data.ContainsKey("Counter");
        }
    }
}
