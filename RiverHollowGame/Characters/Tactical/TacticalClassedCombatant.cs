//using Microsoft.Xna.Framework;
//using RiverHollow.Actors.CombatStuff;
//using RiverHollow.CombatStuff;
//using RiverHollow.Game_Managers;
//using RiverHollow.Items;
//using System.Collections.Generic;
//using static RiverHollow.Game_Managers.GameManager;
//using static RiverHollow.Game_Managers.SaveManager;

//namespace RiverHollow.Characters
//{
//    public abstract class ClassedCombatant : TacticalCombatActor
//    {
//        #region Properties
//        public static List<int> LevelRange = new List<int> { 0, 20, 80, 160, 320, 640, 1280, 2560, 5120, 10240 };

//        protected CharacterClass _class;
//        public CharacterClass CharacterClass => _class;
//        private int _classLevel;
//        public int ClassLevel => _classLevel;

//        private Vector2 _vStartPosition;
//        public Vector2 StartPosition => _vStartPosition;

//        private int _iXP;
//        public int XP => _iXP;

//        public bool Protected;

//        public List<GearSlot> _liGearSlots;
//        public GearSlot Weapon;
//        public GearSlot Armor;
//        public GearSlot Head;
//        public GearSlot Wrist;
//        public GearSlot Accessory1;
//        public GearSlot Accessory2;

//        public override int Damage => GetGreatDamage();
//        public override int StatStrength => 10 + _iBuffStrength + GetGearStat(AttributeEnum.Strength);
//        public override int StatDefense => 10 + _iBuffDefense + GetGearStat(AttributeEnum.Defense) + (Protected ? 10 : 0);
//        //public override int StatMaxHealth => 10 + (_classLevel * _class.Attribute) + GetGearStat(AttributeEnum.MaxHealth);
//        public override int StatMagic => 10 + _iBuffMagic + GetGearStat(AttributeEnum.Magic);
//        public override int StatResistance => 10 + _iBuffResistance + GetGearStat(AttributeEnum.Resistance);
//       // public override int StatSpd => 10 + _class.StatSpd + _iBuffSpeed + GetGearStat(AttributeEnum.Speed);

//        public int TempStatStr => 10 + _iBuffStrength + GetTempGearStat(AttributeEnum.Strength);
//        public int TempStatDef => 10 + _iBuffDefense + GetTempGearStat(AttributeEnum.Defense) + (Protected ? 10 : 0);
//       // public int TempStatVit => 10 + (_classLevel * _class.StatMaxHealth) + GetTempGearStat(AttributeEnum.MaxHealth);
//        public int TempStatMag => 10 + _iBuffMagic + GetTempGearStat(AttributeEnum.Magic);
//        public int TempStatRes => 10 + _iBuffResistance + GetTempGearStat(AttributeEnum.Resistance);
//        //public int TempStatSpd => 10 + _class.StatSpd + _iBuffSpeed + GetTempGearStat(AttributeEnum.Speed);

//        public override List<TacticalMenuAction> TacticalAbilityList => _class.TacticalActionList;
//        public List<LiteMenuAction> LiteAbilityList => _class.LiteActionList;

//        public int GetGreatDamage()
//        {
//            int rv = 0;

//            rv += Weapon.GetStat(AttributeEnum.Damage);
//            rv += base.Damage;

//            return rv;
//        }
//        public int GetGearStat(AttributeEnum stat)
//        {
//            int rv = 0;
//            if (_liGearSlots != null)
//            {
//                foreach (GearSlot g in _liGearSlots)
//                {
//                    rv += g.GetStat(stat);
//                }
//            }

//            return rv;
//        }
//        public int GetTempGearStat(AttributeEnum stat)
//        {
//            int rv = 0;

//            foreach (GearSlot g in _liGearSlots)
//            {
//                rv += g.GetTempStat(stat);
//            }

//            return rv;
//        }
//        #endregion

//        public ClassedCombatant() : base()
//        {
//            _classLevel = 1;

//            _liGearSlots = new List<GearSlot>();
//            Weapon = new GearSlot(EquipmentEnum.Weapon);
//            Armor = new GearSlot(EquipmentEnum.Armor);
//            Head = new GearSlot(EquipmentEnum.Head);
//            Wrist = new GearSlot(EquipmentEnum.Wrist);
//            Accessory1 = new GearSlot(EquipmentEnum.Accessory);
//            Accessory2 = new GearSlot(EquipmentEnum.Accessory);

//            _liGearSlots.Add(Weapon);
//            _liGearSlots.Add(Armor);
//            _liGearSlots.Add(Head);
//            _liGearSlots.Add(Wrist);
//            _liGearSlots.Add(Accessory1);
//            _liGearSlots.Add(Accessory2);
//        }

//        public virtual void SetClass(CharacterClass x)
//        {
//            _class = x;
//            _iCurrentHP = MaxHP;
//            _iCurrentMP = MaxMP;
//        }

//        /// <summary>
//        /// Assigns the starting gear to the Actor as long as the slots are empty.
//        /// Should never be called when they can be euipped, checks are for safety.
//        /// </summary>
//        public void AssignStartingGear()
//        {
//            if (Weapon.IsEmpty()) { Weapon.SetGear((Equipment)DataManager.GetItem(_class.WeaponID)); }
//            if (Armor.IsEmpty()) { Armor.SetGear((Equipment)DataManager.GetItem(_class.ArmorID)); }
//            if (Head.IsEmpty()) { Head.SetGear((Equipment)DataManager.GetItem(_class.HeadID)); }
//            if (Wrist.IsEmpty()) { Wrist.SetGear((Equipment)DataManager.GetItem(_class.WristID)); }
//        }

//        public void AddXP(int x)
//        {
//            _iXP += x;
//            if (_iXP >= LevelRange[_classLevel])
//            {
//                _classLevel++;
//            }
//        }

//        public void GetXP(ref double curr, ref double max)
//        {
//            curr = _iXP;
//            max = ClassedCombatant.LevelRange[this.ClassLevel];
//        }

//        #region StartPosition
//        public void IncreaseStartPos()
//        {
//            if (_vStartPosition.Y < 2)
//            {
//                _vStartPosition.Y++;
//            }
//            else
//            {
//                _vStartPosition = new Vector2(_vStartPosition.X++, 0);
//            }
//        }

//        public void SetStartPosition(Vector2 pos)
//        {
//            _vStartPosition = pos;
//        }
//        #endregion

//        /// <summary>
//        /// Retrieves te list of skills the character has based off of their class
//        /// that is also valid based off of their current level.
//        /// </summary>
//        /// <returns></returns>
//        public override List<TacticalCombatAction> GetCurrentSpecials()
//        {
//            List<TacticalCombatAction> rvList = new List<TacticalCombatAction>();

//            rvList.AddRange(_class._liSpecialTacticalActionsList.FindAll(action => action.ReqLevel <= this.ClassLevel));

//            return rvList;
//        }

//        public ClassedCharData SaveClassedCharData()
//        {
//            ClassedCharData advData = new ClassedCharData
//            {
//                armor = Item.SaveData(Armor.GetItem()),
//                weapon = Item.SaveData(Weapon.GetItem()),
//                level = _classLevel,
//                xp = _iXP
//            };

//            return advData;
//        }
//        public void LoadClassedCharData(ClassedCharData data)
//        {
//            Armor.SetGear((Equipment)DataManager.GetItem(data.armor.itemID, data.armor.num));
//            Weapon.SetGear((Equipment)DataManager.GetItem(data.weapon.itemID, data.weapon.num));
//            _classLevel = data.level;
//            _iXP = data.xp;
//        }

//        /// <summary>
//        /// Structure that represents the slot for the character.
//        /// Holds both the actual item and a temp item to compare against.
//        /// </summary>
//        public class GearSlot
//        {
//            EquipmentEnum _enumType;
//            Equipment _eGear;
//            Equipment _eTempGear;
//            public GearSlot(EquipmentEnum type)
//            {
//                _enumType = type;
//            }

//            public void SetGear(Equipment e) { _eGear = e; }
//            public void SetTemp(Equipment e) { _eTempGear = e; }

//            public int GetStat(AttributeEnum stat)
//            {
//                int rv = 0;

//                if (_eGear != null)
//                {
//                    rv += _eGear.Attribute(stat);
//                }

//                return rv;
//            }
//            public int GetTempStat(AttributeEnum stat)
//            {
//                int rv = 0;

//                if (_eTempGear != null)
//                {
//                    rv += _eTempGear.Attribute(stat);
//                }
//                else if (_eGear != null)
//                {
//                    rv += _eGear.Attribute(stat);
//                }

//                return rv;
//            }
//            public Equipment GetItem() { return _eGear; }
//            public bool IsEmpty() { return _eGear == null; }
//        }
//    }
//}
