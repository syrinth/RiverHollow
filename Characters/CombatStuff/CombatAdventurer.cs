using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using System.Collections.Generic;

namespace RiverHollow.Characters.CombatStuff
{
    public class CombatAdventurer : CombatCharacter
    {
        #region Properties
        public static List<int> LevelRange = new List<int> { 0, 10, 40, 100, 200, 600, 800, 1200, 1600, 2000};
        private WorldAdventurer _world;
        public WorldAdventurer World { get => _world; }
        protected CharacterClass _class;
        public CharacterClass CharacterClass { get => _class; }
        private int _classLevel;
        public int ClassLevel { get => _classLevel; }

        private int _xp;
        public int XP { get => _xp; }

        public string Name { get => (_world == null) ? PlayerManager.Name : _world.Name; }

        private Equipment _weapon;
        public Equipment Weapon { get => _weapon; }
        private Equipment _armor;
        public Equipment Armor { get => _armor; }

        public override int StatDmg { get =>  10 + (_classLevel * _class.StatDmg) + (_weapon == null ? 0 : _weapon.Dmg) + (_armor == null ? 0 : _armor.Dmg); }
        public override int StatDef { get => 10 + (_classLevel * _class.StatDef) + (_weapon == null ? 0 : _weapon.Def) + (_armor == null ? 0 : _armor.Def); }
        public override int StatHP { get => 10 + (_classLevel * _class.StatHP) + (_weapon == null ? 0 : _weapon.HP) + (_armor == null ? 0 : _armor.HP); }
        public override int StatMagic { get => 10 + (_classLevel * _class.StatMagic) + (_weapon == null ? 0 : _weapon.Mag) + (_armor == null ? 0 : _armor.Mag); }
        public override int StatSpd { get => 10 + (_classLevel * _class.StatSpd) + (_weapon == null ? 0 : _weapon.Spd) + (_armor == null ? 0 : _armor.Spd); }

        #endregion
        public CombatAdventurer(WorldAdventurer w) : base()
        {
            _world = w;
            _weapon = (Equipment)ObjectManager.GetItem(9);
            _armor = (Equipment)ObjectManager.GetItem(10);
            _classLevel = 1;
        }

        public CombatAdventurer() : base()
        {
            _weapon = (Equipment)ObjectManager.GetItem(9);
            _armor = (Equipment)ObjectManager.GetItem(10);
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

        public void AddXP(int x)
        {
            _xp += x;
            if(_xp >= LevelRange[_classLevel])
            {
                _classLevel++;
            }
        }
    }
}
