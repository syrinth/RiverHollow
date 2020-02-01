﻿using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Misc;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Actors.CombatStuff
{
    public class StatusEffect
    {
        public CombatActor Caster;
        private bool _bDoT;
        public bool DoT => _bDoT;
        private bool _bHoT;
        public bool HoT => _bHoT;
        public bool _bSong;
        public bool Song => _bSong;
        private int _id;
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
            _id = id;
            DataManager.GetStatusEffectText(id, ref _sName, ref _sDescription);

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
    }
}
