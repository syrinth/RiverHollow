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
        private WorldAdventurer _worldAdv;
        public WorldAdventurer WorldAdv => _worldAdv;
        private EligibleNPC _eligibleNPC;
        public EligibleNPC EligibleNPC => _eligibleNPC;

        protected CharacterClass _class;
        public CharacterClass CharacterClass { get => _class; }
        private int _classLevel;
        public int ClassLevel { get => _classLevel; }

        private int _xp;
        public int XP { get => _xp; }

        public bool Protected;

        public Equipment Weapon;
        public Equipment TempWeapon;
        public Equipment Armor;
        public Equipment TempArmor;

        public override int Attack => GetGearAtk();
        public override int StatStr { get =>  10 + (_classLevel * _class.StatStr) + _buffStr + GetGearStr(); }
        public override int StatDef { get => 10 + (_classLevel * _class.StatDef) + _buffDef +  GetGearDef() + (Protected ? 10 : 0); }
        public override int StatVit { get => 10 + (_classLevel * _class.StatVit) + GetGearVit(); }
        public override int StatMag { get => 10 + (_classLevel * _class.StatMag) + _buffMag + GetGearMag(); }
        public override int StatRes { get => 10 + (_classLevel * _class.StatRes) + _buffRes + GetGearRes(); }
        public override int StatSpd { get => 10 + (_classLevel * _class.StatSpd) + +_buffSpd + GetGearSpd(); }

        public override List<MenuAction> AbilityList { get => _class.AbilityList; }
        public override List<CombatAction> SpellList { get => _class._spellList; }

        public int GetGearAtk()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Attack; }
            else if (Weapon != null) { rv += Weapon.Attack; }
            else if (Weapon == null) { rv += base.Attack; }
            if (TempArmor != null) { rv += TempArmor.Attack; }
            else if (Armor != null) { rv += Armor.Attack; }

            return rv;
        }
        public int GetGearStr()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Str; }
            else if (Weapon != null) { rv += Weapon.Str; }
            if (TempArmor != null) { rv += TempArmor.Str; }
            else if (Armor != null) { rv += Armor.Str; }

            return rv;
        }
        public int GetGearDef()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Def; }
            else if (Weapon != null) { rv += Weapon.Def; }
            if (TempArmor != null) { rv += TempArmor.Def; }
            else if (Armor != null) { rv += Armor.Def; }

            return rv;
        }
        public int GetGearVit()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Vit; }
            else if (Weapon != null) { rv += Weapon.Vit; }
            if (TempArmor != null) { rv += TempArmor.Vit; }
            else if (Armor != null) { rv += Armor.Vit; }

            return rv;
        }
        public int GetGearMag()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Mag; }
            else if (Weapon != null) { rv += Weapon.Mag; }
            if (TempArmor != null) { rv += TempArmor.Mag; }
            else if (Armor != null) { rv += Armor.Mag; }

            return rv;
        }
        public int GetGearRes()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Res; }
            else if (Weapon != null) { rv += Weapon.Res; }
            if (TempArmor != null) { rv += TempArmor.Res; }
            else if (Armor != null) { rv += Armor.Res; }

            return rv;
        }
        public int GetGearSpd()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Spd; }
            else if (Weapon != null) { rv += Weapon.Spd; }
            if (TempArmor != null) { rv += TempArmor.Spd; }
            else if (Armor != null) { rv += Armor.Spd; }

            return rv;
        }

        #endregion
        public CombatAdventurer(WorldAdventurer w) : this()
        {
            _sName = w.Name;
            _worldAdv = w;
        }

        public CombatAdventurer(EligibleNPC c) : this()
        {
            _sName = c.Name;
            _eligibleNPC= c;
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
