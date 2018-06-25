using RiverHollow.Characters.NPCs;
using RiverHollow.Game_Managers;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

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

        public Equipment Weapon;
        public Equipment Armor;

        public override int StatDmg { get =>  10 + (_classLevel * _class.StatDmg) + _buffDmg + (Weapon == null ? 0 : Weapon.Dmg) + (Armor == null ? 0 : Armor.Dmg); }
        public override int StatDef { get => 10 + (_classLevel * _class.StatDef) + _buffDef + (Weapon == null ? 0 : Weapon.Def) + (Armor == null ? 0 : Armor.Def); }
        public override int StatHP { get => 10 + (_classLevel * _class.StatHP) + (Weapon == null ? 0 : Weapon.HP) + (Armor == null ? 0 : Armor.HP); }
        public override int StatMagic { get => 10 + (_classLevel * _class.StatMagic) + _buffMagic + (Weapon == null ? 0 : Weapon.Mag) + (Armor == null ? 0 : Armor.Mag); }
        public override int StatSpd { get => 10 + (_classLevel * _class.StatSpd) + +_buffSpd + (Weapon == null ? 0 : Weapon.Spd) + (Armor == null ? 0 : Armor.Spd); }

        public override List<MenuAction> AbilityList { get => _class.AbilityList; }
        public override List<CombatAction> SpellList { get => _class.SpellList; }

        #endregion
        public CombatAdventurer(WorldAdventurer w) : this()
        {
            _sName = w.Name;
            _world = w;
        }

        public CombatAdventurer(string name) : this()
        {
            _sName = name;
        }

        public CombatAdventurer() : base()
        {
            _characterType = CharacterEnum.CombatAdventurer;
            _classLevel = 1;
        }

        public void SetClass(CharacterClass x)
        {
            _class = x;
            _currentHP = MaxHP;
            _currentMP = MaxMP;
        }

        public void AddXP(int x)
        {
            _xp += x;
            if(_xp >= LevelRange[_classLevel])
            {
                _classLevel++;
            }
        }

        public AdventurerData SaveData()
        {
            AdventurerData advData = new AdventurerData
            {
                armor = Item.SaveData(Armor),
                weapon = Item.SaveData(Weapon),
                level = _classLevel,
                xp = _xp
            };

            return advData;
        }
        public void LoadData(AdventurerData data)
        {
            Armor = (Equipment)ObjectManager.GetItem(data.armor.itemID, data.armor.num);
            Weapon = (Equipment)ObjectManager.GetItem(data.weapon.itemID, data.weapon.num);
            _classLevel = data.level;
            _xp = data.xp;
        }
    }
}
