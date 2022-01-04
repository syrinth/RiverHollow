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

namespace RiverHollow.Characters.Lite
{
    public class ClassedCombatant : CombatActor
    {
        #region Properties

        public static List<int> LevelRange = new List<int> { 0, 40, 80, 160, 320, 640, 1280, 2560, 5120, 10240 };

        protected CharacterClass _class;
        public CharacterClass CharacterClass { get => _class; }
        public int ClassLevel { get; private set; }
        public int XP { get; private set; }

        protected Dictionary<EquipmentEnum, Equipment> _diGear;
        protected Dictionary<EquipmentEnum, Equipment> _diGearComparison;

        public override List<CombatAction> Actions { get => _class.Actions; }

        #endregion
        public ClassedCombatant() : base()
        {
            _eActorType = ActorEnum.PartyMember;
            ClassLevel = 1;
            _iBodyWidth = 32;
            _iBodyHeight = 32;

            _diGear = new Dictionary<EquipmentEnum, Equipment>();
            foreach (EquipmentEnum e in Enum.GetValues(typeof(EquipmentEnum))) { _diGear[e] = null; }

            _diGearComparison = new Dictionary<EquipmentEnum, Equipment>();
            foreach (EquipmentEnum e in Enum.GetValues(typeof(EquipmentEnum))) { _diGearComparison[e] = null; }
        }

        public override GUIImage GetIcon()
        {
            return new GUIImage(new Rectangle(0, 0, 18, 18), ScaleIt(18), ScaleIt(18), DataManager.GetTexture(_sCombatPortraits + "V_" + (_class.ID +1).ToString("00")));

        }
        public virtual void SetClass(CharacterClass x)
        {
            _class = x;
            CurrentHP = MaxHP;
            if (x.ID != -1)
            {
                LoadSpriteAnimations(ref _sprBody, Util.LoadCombatAnimations(x.ClassStringData), DataManager.FOLDER_PARTY + "Wizard");
            }
        }

        public void AddXP(int x)
        {
            XP += x;
            if (XP >= LevelRange[ClassLevel])
            {
                ClassLevel++;
            }
        }

        public void GetXP(ref double curr, ref double max)
        {
            curr = XP;
            max = ClassedCombatant.LevelRange[this.ClassLevel];
        }

        public override int Attribute(AttributeEnum e)
        {
            if (e == AttributeEnum.Damage)
            {
                return GearAttribute(e);
            }
            else
            {
                return 10 + _diAttributes[e] + _class.Attribute(e) + _diEffectedAttributes[e].Value + GearAttribute(e);
            }
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

        public int TempAttribute(AttributeEnum e)
        {
            if (e == AttributeEnum.Damage)
            {
                return GearAttrComparison(e);
            }
            else
            {
                return 10 + _diAttributes[e] + _class.Attribute(e) + _diEffectedAttributes[e].Value + GearAttrComparison(e);
            }
        }

        public int GearAttrComparison(AttributeEnum attr)
        {
            int rv = 0;

            foreach (EquipmentEnum e in Enum.GetValues(typeof(EquipmentEnum)))
            {
                if (_diGearComparison[e] != null)
                {
                    rv = _diGearComparison[e].Attribute(attr);
                }
                else if (_diGear[e] != null)
                {
                    rv = _diGear[e].Attribute(attr);
                }
            }

            return rv;
        }

        public void Unequip(EquipmentEnum e) { _diGear[e] = null; }
        public void Equip(Equipment e) { _diGear[e.EquipType] = e; }
        public Equipment GetEquipment(EquipmentEnum e) { return _diGear[e]; }

        public void EquipComparator(Equipment e) { _diGearComparison[e.EquipType] = e; }
        public Equipment GetEquipmentCompare(EquipmentEnum e) { return _diGearComparison[e]; }

        public void ClearEquipmentCompare()
        {
            foreach (EquipmentEnum e in Enum.GetValues(typeof(EquipmentEnum)))
            {
                _diGearComparison[e] = null;
            }
        }

        /// <summary>
        /// Assigns the starting gear to the Actor as long as the slots are empty.
        /// Should never be called when they can be euipped, checks are for safety.
        /// </summary>
        public void AssignStartingGear()
        {
            _diGear[EquipmentEnum.Weapon] = (Equipment)DataManager.GetItem(_class.WeaponID);
            _diGear[EquipmentEnum.Armor] = (Equipment)DataManager.GetItem(_class.ArmorID);
            _diGear[EquipmentEnum.Head] = (Equipment)DataManager.GetItem(_class.HeadID);
            _diGear[EquipmentEnum.Wrist] = (Equipment)DataManager.GetItem(_class.WristID);
        }

        public ClassedCharData SaveClassedCharData()
        {
            ClassedCharData advData = new ClassedCharData
            {
                armor = Item.SaveData(_diGear[EquipmentEnum.Armor]),
                weapon = Item.SaveData(_diGear[EquipmentEnum.Weapon]),
                level = ClassLevel,
                xp = XP
            };

            return advData;
        }
        public void LoadClassedCharData(ClassedCharData data)
        {
            _diGear[EquipmentEnum.Armor] = (Equipment)DataManager.GetItem(data.armor.itemID);
            _diGear[EquipmentEnum.Weapon] = (Equipment)DataManager.GetItem(data.weapon.itemID);
            ClassLevel = data.level;
            XP = data.xp;
        }
    }
}
