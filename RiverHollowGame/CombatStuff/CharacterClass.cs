using System;
using System.Collections.Generic;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.CombatStuff
{
    public class CharacterClass
    {
        public int ID { get; } = -1;

        Dictionary<AttributeEnum, int> _diAttributes;

        private string _sName;
        public string Name => _sName;
        private string _sDescription;
        public string Description => _sDescription;
        public List<CombatAction> Actions;

        WeaponEnum _weaponType;
        public WeaponEnum WeaponType => _weaponType;
        ArmorEnum _armorType;
        public ArmorEnum ArmorType => _armorType;

        public int WeaponID;
        public int ArmorID;
        public int HeadID;
        public int WristID;

        public Dictionary<string, string> ClassStringData => DataManager.GetClassDataByID(ID);

        public CharacterClass() { }
        public CharacterClass(int id, Dictionary<string, string> stringData)
        {
            ID = id;

            Actions = new List<CombatAction>();
            ImportBasics(stringData);
        }

        protected void ImportBasics(Dictionary<string, string> stringData)
        {
            DataManager.GetTextData("Class", ID, ref _sName, "Name");
            DataManager.GetTextData("Class", ID, ref _sDescription, "Description");

            Util.AssignValue(ref _weaponType, "Weapon", stringData);
            Util.AssignValue(ref _armorType, "Armor", stringData);

            WeaponID = int.Parse(stringData["DWeap"]);
            ArmorID = int.Parse(stringData["DArmor"]);
            HeadID = int.Parse(stringData["DHead"]);
            WristID = int.Parse(stringData["DWrist"]);

            if (stringData.ContainsKey("Actions"))
            {
                string[] split = stringData["Actions"].Split('|');
                foreach (string ability in split)
                {
                    Actions.Add(DataManager.GetCombatActionByIndex(int.Parse(ability)));
                }
                Actions.Add(DataManager.GetCombatActionByIndex(int.Parse(DataManager.Config[18]["UseItem"])));
                Actions.Add(DataManager.GetCombatActionByIndex(int.Parse(DataManager.Config[18]["MoveAction"])));
            }

            _diAttributes = new Dictionary<AttributeEnum, int>();
            foreach (AttributeEnum e in Enum.GetValues(typeof(AttributeEnum)))
            {
                if (stringData.ContainsKey(Util.GetEnumString(e))) { _diAttributes[e] = int.Parse(stringData[Util.GetEnumString(e)]); }
                else { _diAttributes[e] = 0; }
            }
        }

        public int Attribute(AttributeEnum e) {
            int rv = 0;
            if (_diAttributes.ContainsKey(e))
            {
                rv = _diAttributes[e];
            }
            return rv;
        }
    }
}
