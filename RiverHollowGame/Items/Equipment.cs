using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Equipment : Item
    {
        const int HEAVY_ATTRIBUTE = 4;
        const int LIGHT_ATTRIBUTE = 3;
        const int WEAPON_DAMAGE_ATTRIBUTE = 5;

        public GearTypeEnum GearType;
        public WeaponEnum WeaponType { get; }
        public ArmorTypeEnum ArmorType { get; }

        int _iTier;
        protected Dictionary<AttributeEnum, int> _diAttributes;

        public Equipment(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            //EType
            GearType = Util.ParseEnum<GearTypeEnum>(stringData["Subtype"]);

            switch (GearType)
            {
                case GearTypeEnum.Weapon:
                    _texTexture = DataManager.GetTexture(@"Textures\Items\Weapons");
                    break;
                default:
                    _texTexture = DataManager.GetTexture(@"Textures\Items\Gear");
                    break;
            }

            //ESub
            if (stringData.ContainsKey("ESub"))
            {
                if (GearType == GearTypeEnum.Weapon) { WeaponType = Util.ParseEnum<WeaponEnum>(stringData["ESub"]); }
                else { ArmorType = Util.ParseEnum<ArmorTypeEnum>(stringData["ESub"]); }
            }

            //Attributes
            _iTier = int.Parse(stringData["Tier"]);

            _diAttributes = new Dictionary<AttributeEnum, int>();
            foreach (AttributeEnum e in Enum.GetValues(typeof(AttributeEnum)))
            {
                if (stringData.ContainsKey(Util.GetEnumString(e))) { _diAttributes[e] = GetItemTierData(e, Util.ParseEnum<AttributeBonusEnum>(stringData[Util.GetEnumString(e)])); }
                else { _diAttributes[e] = 0; }
            }
        }

        //body 4 3 3
        private int GetItemTierData(AttributeEnum attribute, AttributeBonusEnum modifier)
        {
            int rv = 0;
            int subtractBy = 0;
            if(modifier == AttributeBonusEnum.Minor) { subtractBy = 2; }
            else if (modifier == AttributeBonusEnum.Moderate) { subtractBy = 1; }

            switch (GearType)
            {
                case GearTypeEnum.Chest:
                    rv = HEAVY_ATTRIBUTE;
                    break;
                case GearTypeEnum.Accessory:
                case GearTypeEnum.Head:
                    rv = LIGHT_ATTRIBUTE;
                    break;
                case GearTypeEnum.Weapon:
                    if (attribute == AttributeEnum.Damage) { rv = WEAPON_DAMAGE_ATTRIBUTE; }
                    else { rv = LIGHT_ATTRIBUTE; }
                    break;
            }
            return _iTier * (rv - subtractBy) * (attribute == AttributeEnum.Vitality ? 9 : 1);
        }

        /// <summary>
        /// Appends the stats of the equipment to the item description
        /// </summary>
        /// <returns></returns>
        public override string Description()
        {
            string rv = base.Description();
            //rv += System.Environment.NewLine;
            //if (Damage > 0) { rv += " Attack: +" + Damage + " "; }
            //if (Strength > 0) { rv += " Str: +" + Strength + " "; }
            //if (Defence > 0) { rv += " Def: +" + Defence + " "; }
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
