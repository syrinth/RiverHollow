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

        public Equipment Weapon;
        public Equipment Armor;

        public override int StatDmg { get =>  10 + (_classLevel * _class.StatDmg) + (Weapon == null ? 0 : Weapon.Dmg) + (Armor == null ? 0 : Armor.Dmg); }
        public override int StatDef { get => 10 + (_classLevel * _class.StatDef) + (Weapon == null ? 0 : Weapon.Def) + (Armor == null ? 0 : Armor.Def); }
        public override int StatHP { get => 10 + (_classLevel * _class.StatHP) + (Weapon == null ? 0 : Weapon.HP) + (Armor == null ? 0 : Armor.HP); }
        public override int StatMagic { get => 10 + (_classLevel * _class.StatMagic) + (Weapon == null ? 0 : Weapon.Mag) + (Armor == null ? 0 : Armor.Mag); }
        public override int StatSpd { get => 10 + (_classLevel * _class.StatSpd) + (Weapon == null ? 0 : Weapon.Spd) + (Armor == null ? 0 : Armor.Spd); }

        #endregion
        public CombatAdventurer(WorldAdventurer w) : base()
        {
            _world = w;
            Weapon = (Equipment)ObjectManager.GetItem(9);
            Armor = (Equipment)ObjectManager.GetItem(10);
            _classLevel = 1;
        }

        public CombatAdventurer() : base()
        {
            Weapon = (Equipment)ObjectManager.GetItem(9);
            Armor = (Equipment)ObjectManager.GetItem(10);
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
