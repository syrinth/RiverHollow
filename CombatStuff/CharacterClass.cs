
using RiverHollow.Game_Managers;
using System.Collections.Generic;

namespace RiverHollow.Characters.CombatStuff
{
    public class CharacterClass
    {
        int _iID;
        public int ID => _iID;
        private int _statDmg;
        public int StatDmg { get => _statDmg; }
        private int _statDef;
        public int StatDef { get => _statDef; }
        private int _statHP;
        public int StatHP { get => _statHP; }
        private int _statMagic;
        public int StatMagic { get => _statMagic; }
        private int _statSpd;
        public int StatSpd { get => _statSpd; }

        private string _name;
        public string Name { get => _name; }
        private string _description;
        public string Description { get => _description; }
        public List<MenuAction> AbilityList;
        public List<CombatAction> SpellList;

        public CharacterClass(int id, string[] stringData)
        {
            AbilityList = new List<MenuAction>();
            SpellList = new List<CombatAction>();
            ImportBasics(id, stringData);
        }

        protected int ImportBasics(int id, string[] stringData)
        {
            _iID = id;

            int i = 0;
            _name = stringData[i++];
            _description = stringData[i++];
            _statDmg = int.Parse(stringData[i++]);
            _statDef = int.Parse(stringData[i++]);
            _statHP = int.Parse(stringData[i++]);
            _statMagic = int.Parse(stringData[i++]);
            _statSpd = int.Parse(stringData[i++]);
            string[] split = stringData[i++].Split(' ');
            foreach(string s in split)
            {
                AbilityList.Add(CharacterManager.GetActionByIndex(int.Parse(s)));
            }
            split = stringData[i++].Split(' ');
            foreach(string s in split)
            {
                SpellList.Add((CombatAction)CharacterManager.GetActionByIndex(int.Parse(s)));
            }

            return i;
        }
    }
}
