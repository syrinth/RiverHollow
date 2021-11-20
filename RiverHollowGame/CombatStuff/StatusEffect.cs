using System.Collections.Generic;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.CombatStuff
{
    public class StatusEffect
    {
        public LiteCombatActor LiteCaster;
        public TacticalCombatActor TacticalCaster;
        private bool _bDoT;
        public bool DoT => _bDoT;
        private bool _bHoT;
        public bool HoT => _bHoT;
        public bool _bSong;
        public bool Song => _bSong;
        private int _iID;
        private string _sName;
        public string Name => _sName;
        int _iPotency;
        public int Potency => _iPotency;
        public int Duration;
        private List<KeyValuePair<StatEnum, int>> _liStats;
        public List<KeyValuePair<StatEnum, int>> StatMods  => _liStats;
        private int _conditionID;
        private string _sDescription;
        public string Description { get => _sDescription; }

        private bool _bCounter;
        public bool Counter => _bCounter;

        private bool _bGuard;
        public bool Guard => _bGuard;

        public StatusEffect(int id, Dictionary<string, string> data)
        {
            _iID = id;
            DataManager.GetTextData("StatusEffect", _iID, ref _sName, "Name");
            DataManager.GetTextData("StatusEffect", _iID, ref _sDescription, "Description");

            _liStats = new List<KeyValuePair<StatEnum, int>>();
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
                    _liStats.Add(new KeyValuePair<StatEnum, int>(Util.ParseEnum<StatEnum>(statMods[0]), int.Parse(statMods[1])));
                }
            }

            if (data.ContainsKey("Debuff"))
            {
                string[] splitEffects = data["Debuff"].Split(' ');
                foreach (string effect in splitEffects)
                {
                    string[] statMods = effect.Split('-');
                    _liStats.Add(new KeyValuePair<StatEnum, int>(Util.ParseEnum<StatEnum>(statMods[0]), -int.Parse(statMods[1])));
                }
            }

            if (data.ContainsKey("Potency"))
            {
                _iPotency = int.Parse(data["Potency"]);
            }

            _bHoT = data.ContainsKey("HoT");
            _bDoT = data.ContainsKey("DoT");

            _bSong = data.ContainsKey("Song");

            _bCounter = data.ContainsKey("Counter");
            _bGuard = data.ContainsKey("Guard");
        }

        public void AssignCaster(LiteCombatActor act) { LiteCaster = act; }
        public void AssignCaster(TacticalCombatActor act) { TacticalCaster = act; }
    }
}
