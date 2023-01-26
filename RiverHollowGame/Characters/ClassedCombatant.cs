using Microsoft.Xna.Framework;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters.Lite
{
    public class ClassedCombatant : CombatActor
    {
        #region Properties

        string _sName;
        public static List<int> LevelRange = new List<int> { 0, 40, 80, 160, 320, 640, 1280, 2560, 5120, 10240 };

        protected Job _class;
        public Job CharacterClass { get => _class; }
        public int ClassLevel { get; private set; }
        public int CurrentXP { get; private set; }
        public AttributeEnum KeyAttribute => _class.KeyAttribute;

        protected Dictionary<GearTypeEnum, Equipment> _diGear;
        protected Dictionary<GearTypeEnum, Equipment> _diGearComparison;

        public override List<CombatAction> Actions { get => _class.Actions; }

        #endregion
        public ClassedCombatant()
        {
            _eActorType = CombatActorTypeEnum.PartyMember;
            ClassLevel = 1;

            _diGear = new Dictionary<GearTypeEnum, Equipment>();
            foreach (GearTypeEnum e in Enum.GetValues(typeof(GearTypeEnum))) { _diGear[e] = null; }
            _diGear.Remove(GearTypeEnum.None);

            _diGearComparison = new Dictionary<GearTypeEnum, Equipment>();
            foreach (GearTypeEnum e in Enum.GetValues(typeof(GearTypeEnum))) { _diGearComparison[e] = null; }
            _diGearComparison.Remove(GearTypeEnum.None);

            _diAttributes = new Dictionary<AttributeEnum, int>();
            foreach (AttributeEnum e in Enum.GetValues(typeof(AttributeEnum)))
            {
                if (e == AttributeEnum.Damage) { _diAttributes[e] = 5; }
                else { _diAttributes[e] = 10; }
            }
        }

        public override GUIImage GetIcon()
        {
            if (this == PlayerManager.PlayerCombatant) { return new GUIImage(new Rectangle(0, 0, 18, 18), ScaleIt(18), ScaleIt(18), DataManager.GetTexture(DataManager.COMBAT_PORTRAITS + "Player_Icon")); }
            else { return new GUIImage(new Rectangle(0, 0, 18, 18), ScaleIt(18), ScaleIt(18), DataManager.GetTexture(DataManager.COMBAT_PORTRAITS + "V_" + _class.ID.ToString("00"))); }
        }
        public virtual void SetClass(Job x)
        {
            _class = x;
            if (x.ID != -1)
            {
                LoadSpriteAnimations(ref _sprBody, Util.LoadCombatAnimations(x.ClassStringData), DataManager.FOLDER_PARTY + "Class_" + _class.ID.ToString("00"));
            }
            
            //Each class has a defined speed attribute
            _diAttributes[AttributeEnum.Speed] = x.SpeedAttribute;
        }

        public void AddXP(int x)
        {
            CurrentXP += x;
            if (CurrentXP >= LevelRange[ClassLevel])
            {
                ClassLevel++;
            }
        }

        public int NextLevelAt()
        {
            return LevelRange[ClassLevel];
        }

        public void GetXP(ref double curr, ref double max)
        {
            curr = CurrentXP;
            max = ClassedCombatant.LevelRange[this.ClassLevel];
        }

        public override int Attribute(AttributeEnum e)
        {
            if (e == AttributeEnum.Vitality)
            {
                return _diAttributes[e] + GearAttribute(e) + _diEffectedAttributes[e].Value;
            }
            else
            {
                return Math.Min(99, _diAttributes[e] + GearAttribute(e)) + _diEffectedAttributes[e].Value;
            }
        }

        public int AttributeTemp(AttributeEnum e)
        {
            return _diAttributes[e] + GearAttrComparison(e);
        }

        public int GearAttribute(AttributeEnum e)
        {
            int rv = 0;

            foreach (Equipment g in _diGear.Values)
            {
                if (g != null)
                {
                    rv += g.Attribute(e);
                }
            }

            return rv;
        }

        public int GearAttrComparison(AttributeEnum attr)
        {
            int rv = 0;

            foreach (GearTypeEnum e in Enum.GetValues(typeof(GearTypeEnum)))
            {
                if(e == GearTypeEnum.None) { continue; }

                if (_diGearComparison[e] != null)
                {
                    rv += _diGearComparison[e].Attribute(attr);
                }
                else if (_diGear[e] != null)
                {
                    rv += _diGear[e].Attribute(attr);
                }
            }

            return rv;
        }

        public void Unequip(GearTypeEnum e) {
            int initialHP = MaxHP;
            _diGear[e] = null;

            if (initialHP > MaxHP) { CurrentHP = MaxHP; }
        }
        public void Equip(Equipment e) {
            if(e == null) { return; }

            int initialHP = MaxHP;
            _diGear[e.GearType] = e;

            if (initialHP > MaxHP) { CurrentHP = MaxHP; }
        }
        public Equipment GetEquipment(GearTypeEnum e) { return _diGear[e]; }

        public void EquipComparator(Equipment e) { _diGearComparison[e.GearType] = e; }
        public Equipment GetEquipmentCompare(GearTypeEnum e) { return _diGearComparison[e]; }

        public void ClearEquipmentCompare()
        {
            foreach (GearTypeEnum e in Enum.GetValues(typeof(GearTypeEnum)))
            {
                _diGearComparison[e] = null;
            }
        }

        public void SetName(string str)
        {
            _sName = str;
        }
        public override string Name()
        {
            return _sName;
        }

        /// <summary>
        /// Assigns the starting gear to the Actor as long as the slots are empty.
        /// Should never be called when they can be euipped, checks are for safety.
        /// </summary>
        public void AssignStartingGear()
        {
            _diGear[GearTypeEnum.Weapon] = (Equipment)DataManager.GetItem(_class.WeaponID);
            _diGear[GearTypeEnum.Chest] = (Equipment)DataManager.GetItem(_class.ArmorID);
            _diGear[GearTypeEnum.Head] = (Equipment)DataManager.GetItem(_class.HeadID);
            _diGear[GearTypeEnum.Accessory] = (Equipment)DataManager.GetItem(_class.AccessoryID);

            CurrentHP = MaxHP;
        }

        public ClassedCharData SaveClassedCharData()
        {
            ClassedCharData advData = new ClassedCharData
            {
                weapon = Item.SaveData(_diGear[GearTypeEnum.Weapon]),
                armor = Item.SaveData(_diGear[GearTypeEnum.Chest]),
                head = Item.SaveData(_diGear[GearTypeEnum.Head]),
                accessory = Item.SaveData(_diGear[GearTypeEnum.Accessory]),
                level = ClassLevel,
                xp = CurrentXP
            };

            return advData;
        }
        public void LoadClassedCharData(ClassedCharData data)
        {
            Equip((Equipment)DataManager.GetItem(data.weapon.itemID));
            Equip((Equipment)DataManager.GetItem(data.armor.itemID));
            Equip((Equipment)DataManager.GetItem(data.head.itemID));
            Equip((Equipment)DataManager.GetItem(data.accessory.itemID));

            ClassLevel = data.level;
            CurrentXP = data.xp;
        }
    }
}
