using Microsoft.Xna.Framework;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;

namespace RiverHollow.Characters
{
    public abstract class ClassedCombatant : TacticalCombatActor
    {
        #region Properties
        public static List<int> LevelRange = new List<int> { 0, 20, 80, 160, 320, 640, 1280, 2560, 5120, 10240 };

        protected CharacterClass _class;
        public CharacterClass CharacterClass => _class;
        private int _classLevel;
        public int ClassLevel => _classLevel;

        private Vector2 _vStartPosition;
        public Vector2 StartPosition => _vStartPosition;

        private int _iXP;
        public int XP => _iXP;

        public bool Protected;

        public List<GearSlot> _liGearSlots;
        public GearSlot Weapon;
        public GearSlot Armor;
        public GearSlot Head;
        public GearSlot Wrist;
        public GearSlot Accessory1;
        public GearSlot Accessory2;

        public override int Attack => GetGearAtk();
        public override int StatStr => 10 + _iBuffStr + GetGearStat(StatEnum.Str);
        public override int StatDef => 10 + _iBuffDef + GetGearStat(StatEnum.Def) + (Protected ? 10 : 0);
        public override int StatVit => 10 + (_classLevel * _class.StatVit) + GetGearStat(StatEnum.Vit);
        public override int StatMag => 10 + _iBuffMag + GetGearStat(StatEnum.Mag);
        public override int StatRes => 10 + _iBuffRes + GetGearStat(StatEnum.Res);
        public override int StatSpd => 10 + _class.StatSpd + _iBuffSpd + GetGearStat(StatEnum.Spd);

        public int TempStatStr => 10 + _iBuffStr + GetTempGearStat(StatEnum.Str);
        public int TempStatDef => 10 + _iBuffDef + GetTempGearStat(StatEnum.Def) + (Protected ? 10 : 0);
        public int TempStatVit => 10 + (_classLevel * _class.StatVit) + GetTempGearStat(StatEnum.Vit);
        public int TempStatMag => 10 + _iBuffMag + GetTempGearStat(StatEnum.Mag);
        public int TempStatRes => 10 + _iBuffRes + GetTempGearStat(StatEnum.Res);
        public int TempStatSpd => 10 + _class.StatSpd + _iBuffSpd + GetTempGearStat(StatEnum.Spd);

        public override List<TacticalMenuAction> AbilityList => _class.ActionList;

        public int GetGearAtk()
        {
            int rv = 0;

            rv += Weapon.GetStat(StatEnum.Atk);
            rv += base.Attack;

            return rv;
        }
        public int GetGearStat(StatEnum stat)
        {
            int rv = 0;
            if (_liGearSlots != null)
            {
                foreach (GearSlot g in _liGearSlots)
                {
                    rv += g.GetStat(stat);
                }
            }

            return rv;
        }
        public int GetTempGearStat(StatEnum stat)
        {
            int rv = 0;

            foreach (GearSlot g in _liGearSlots)
            {
                rv += g.GetTempStat(stat);
            }

            return rv;
        }
        #endregion

        public ClassedCombatant() : base()
        {
            _classLevel = 1;

            _liGearSlots = new List<GearSlot>();
            Weapon = new GearSlot(EquipmentEnum.Weapon);
            Armor = new GearSlot(EquipmentEnum.Armor);
            Head = new GearSlot(EquipmentEnum.Head);
            Wrist = new GearSlot(EquipmentEnum.Wrist);
            Accessory1 = new GearSlot(EquipmentEnum.Accessory);
            Accessory2 = new GearSlot(EquipmentEnum.Accessory);

            _liGearSlots.Add(Weapon);
            _liGearSlots.Add(Armor);
            _liGearSlots.Add(Head);
            _liGearSlots.Add(Wrist);
            _liGearSlots.Add(Accessory1);
            _liGearSlots.Add(Accessory2);
        }

        public virtual void SetClass(CharacterClass x)
        {
            _class = x;
            _iCurrentHP = MaxHP;
            _iCurrentMP = MaxMP;
        }

        /// <summary>
        /// Assigns the starting gear to the Actor as long as the slots are empty.
        /// Should never be called when they can be euipped, checks are for safety.
        /// </summary>
        public void AssignStartingGear()
        {
            if (Weapon.IsEmpty()) { Weapon.SetGear((Equipment)DataManager.GetItem(_class.WeaponID)); }
            if (Armor.IsEmpty()) { Armor.SetGear((Equipment)DataManager.GetItem(_class.ArmorID)); }
            if (Head.IsEmpty()) { Head.SetGear((Equipment)DataManager.GetItem(_class.HeadID)); }
            if (Wrist.IsEmpty()) { Wrist.SetGear((Equipment)DataManager.GetItem(_class.WristID)); }
        }

        public void AddXP(int x)
        {
            _iXP += x;
            if (_iXP >= LevelRange[_classLevel])
            {
                _classLevel++;
            }
        }

        public void GetXP(ref double curr, ref double max)
        {
            curr = _iXP;
            max = ClassedCombatant.LevelRange[this.ClassLevel];
        }

        #region StartPosition
        public void IncreaseStartPos()
        {
            if (_vStartPosition.Y < 2)
            {
                _vStartPosition.Y++;
            }
            else
            {
                _vStartPosition = new Vector2(_vStartPosition.X++, 0);
            }
        }

        public void SetStartPosition(Vector2 pos)
        {
            _vStartPosition = pos;
        }
        #endregion

        /// <summary>
        /// Retrieves te list of skills the character has based off of their class
        /// that is also valid based off of their current level.
        /// </summary>
        /// <returns></returns>
        public override List<TacticalCombatAction> GetCurrentSpecials()
        {
            List<TacticalCombatAction> rvList = new List<TacticalCombatAction>();

            rvList.AddRange(_class._liSpecialActionsList.FindAll(action => action.ReqLevel <= this.ClassLevel));

            return rvList;
        }

        public ClassedCharData SaveClassedCharData()
        {
            ClassedCharData advData = new ClassedCharData
            {
                armor = Item.SaveData(Armor.GetItem()),
                weapon = Item.SaveData(Weapon.GetItem()),
                level = _classLevel,
                xp = _iXP
            };

            return advData;
        }
        public void LoadClassedCharData(ClassedCharData data)
        {
            Armor.SetGear((Equipment)DataManager.GetItem(data.armor.itemID, data.armor.num));
            Weapon.SetGear((Equipment)DataManager.GetItem(data.weapon.itemID, data.weapon.num));
            _classLevel = data.level;
            _iXP = data.xp;
        }

        /// <summary>
        /// Structure that represents the slot for the character.
        /// Holds both the actual item and a temp item to compare against.
        /// </summary>
        public class GearSlot
        {
            EquipmentEnum _enumType;
            Equipment _eGear;
            Equipment _eTempGear;
            public GearSlot(EquipmentEnum type)
            {
                _enumType = type;
            }

            public void SetGear(Equipment e) { _eGear = e; }
            public void SetTemp(Equipment e) { _eTempGear = e; }

            public int GetStat(StatEnum stat)
            {
                int rv = 0;

                if (_eGear != null)
                {
                    rv += _eGear.GetStat(stat);
                }

                return rv;
            }
            public int GetTempStat(StatEnum stat)
            {
                int rv = 0;

                if (_eTempGear != null)
                {
                    rv += _eTempGear.GetStat(stat);
                }
                else if (_eGear != null)
                {
                    rv += _eGear.GetStat(stat);
                }

                return rv;
            }
            public Equipment GetItem() { return _eGear; }
            public bool IsEmpty() { return _eGear == null; }
        }
    }
}
