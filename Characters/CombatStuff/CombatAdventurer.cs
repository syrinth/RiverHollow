
namespace RiverHollow.Characters.CombatStuff
{
    public class CombatAdventurer : CombatCharacter
    {
        #region Properties
        protected CharacterClass _class;
        public CharacterClass CharacterClass { get => _class; }
        private int _classLevel;
        public int ClassLevel { get => _classLevel; }

        public override int StatDmg { get => 10 + (_classLevel * _class.StatDmg); }
        public override int StatDef { get => 10 + (_classLevel * _class.StatDef); }
        public override int StatHP { get => 10 + (_classLevel * _class.StatHP); }
        public override int StatMagic { get => 10 + (_classLevel * _class.StatMagic); }
        public override int StatSpd { get => 10 + (_classLevel * _class.StatSpd); }

        #endregion
        public CombatAdventurer() : base()
        {
            _classLevel = 1;
        }

        public void SetClass(CharacterClass x)
        {
            _class = x;
            _currentHP = MaxHP;

            foreach (Ability a in _class.AbilityList)
            {
                _abilityList.Add(a);
            }
        }
    }
}
