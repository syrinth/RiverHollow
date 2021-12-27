using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Items
{
    public class Equipment : Item
    {
        public EquipmentEnum EquipType;
        public WeaponEnum WeaponType { get; }
        public ArmorEnum ArmorType { get; }
        public ArmorSlotEnum ArmorSlot { get; }

        int _iTier;
        protected Dictionary<AttributeEnum, int> _diAttributes;

        public Equipment(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            //EType
            EquipType = Util.ParseEnum<EquipmentEnum>(stringData["Subtype"]);

            if (EquipType.Equals(EquipmentEnum.Armor)) { _texTexture = DataManager.GetTexture(@"Textures\Items\armor"); }
            else if (EquipType.Equals(EquipmentEnum.Weapon)) { _texTexture = DataManager.GetTexture(@"Textures\Items\weapons"); }
            else if (EquipType.Equals(EquipmentEnum.Accessory)) { _texTexture = DataManager.GetTexture(@"Textures\Items\Accessories"); }

            //ESub
            if (stringData.ContainsKey("ESub"))
            {
                if (EquipType == EquipmentEnum.Armor) { ArmorType = Util.ParseEnum<ArmorEnum>(stringData["ESub"]); }
                else if (EquipType == EquipmentEnum.Weapon) { WeaponType = Util.ParseEnum<WeaponEnum>(stringData["ESub"]); }
            }

            if (EquipType == EquipmentEnum.Armor)
            {
                //Armor Slot
                ArmorSlot = Util.ParseEnum<ArmorSlotEnum>(stringData["ASlot"]);
            }

            //Attributes
            _iTier = int.Parse(stringData["Tier"]);

            _diAttributes = new Dictionary<AttributeEnum, int>();
            foreach (AttributeEnum e in Enum.GetValues(typeof(AttributeEnum)))
            {
                if (stringData.ContainsKey(Util.GetEnumString(e))) { _diAttributes[e] = GetItemTierData(_iTier, stringData[Util.GetEnumString(e)]); }
                else { _diAttributes[e] = 0; }
            }
        }


        private int GetItemTierData(int tier, string modifier, bool isStat = true)
        {
            int DivideBy = isStat ? 4 : 1; //If it's not a stat,it's localize on oneitem, don't divide.
            double rv = 0;

            if (modifier.Equals("Minor"))
            {
                rv = tier * (double)6 / DivideBy;
            }
            else if (modifier.Equals("Moderate"))
            {
                rv = tier * (double)8 / DivideBy;
            }
            else if (modifier.Equals("Major"))
            {
                rv = tier * (double)10 / DivideBy;
            }

            if (rv % 2 > 0) { rv++; }

            return (int)rv;
        }

        /// <summary>
        /// Appends the stats of the equipment to the item description
        /// </summary>
        /// <returns></returns>
        public override string GetDescription()
        {
            string rv = base.GetDescription();
            //rv += System.Environment.NewLine;
            //if (Damage > 0) { rv += " Attack: +" + Damage + " "; }
            //if (Strength > 0) { rv += " Str: +" + Strength + " "; }
            //if (Defense > 0) { rv += " Def: +" + Defense + " "; }
            //if (Magic > 0) { rv += " Mag: +" + Magic + " "; }
            //if (Resistance > 0) { rv += " Res: +" + Resistance + " "; }
            //if (Speed > 0) { rv += " Spd: +" + Speed + " "; }
            //rv = rv.Trim();

            return rv;
        }

        public int Attribute(AttributeEnum e)
        {
            int rv = 0;
            if (_diAttributes.ContainsKey(e))
            {
                rv = _diAttributes[e];
            }
            return rv;
        }
    }
}
