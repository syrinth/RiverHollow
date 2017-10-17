
using RiverHollow.Game_Managers;
using System.Collections.Generic;

namespace RiverHollow.Characters.CombatStuff
{
    public class CharacterClass
    {
        private string _name;
        public string Name { get => _name; }
        private string _description;
        public string Description { get => _description; }
        public List<Ability> AbilityList;
        public CharacterClass(int id, string[] stringData)
        {
            AbilityList = new List<Ability>();
            ImportBasics(id, stringData);
        }

        protected int ImportBasics(int id, string[] stringData)
        {
            int i = 0;
            _name = stringData[i++];
            _description = stringData[i++];
            string[] split = stringData[i++].Split(' ');
            foreach(string s in split)
            {
                AbilityList.Add(CharacterManager.GetAbilityByIndex(int.Parse(s)));
            }

            return i;
        }
    }
}
