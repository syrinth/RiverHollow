using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.CombatStuff
{
    public class Job
    {
        public int ID { get; } = -1;

        public List<CombatAction> Actions;

        AttributeEnum _eKeyAttribute;
        public AttributeEnum KeyAttribute => _eKeyAttribute;
        WeaponEnum _weaponType;
        public WeaponEnum WeaponType => _weaponType;
        ArmorTypeEnum _armorType;
        public ArmorTypeEnum ArmorType => _armorType;

        int _iSpeedAttribute;
        public int SpeedAttribute => _iSpeedAttribute;

        public int WeaponID;
        public int ArmorID;
        public int HeadID;
        public int AccessoryID;

        public Dictionary<string, string> ClassStringData => DataManager.GetJobDataByID(ID);

        public Job() { }
        public Job(int id, Dictionary<string, string> stringData)
        {
            ID = id;

            Actions = new List<CombatAction>();
            Util.AssignValue(ref _weaponType, "Weapon", stringData);
            Util.AssignValue(ref _armorType, "Armor", stringData);

            Util.AssignValue(ref _iSpeedAttribute, "Speed", stringData);
            Util.AssignValue(ref _eKeyAttribute, "KeyAttribute", stringData);

            string[] gearsplit = Util.FindParams(stringData["GearID"]);
            WeaponID = int.Parse(gearsplit[0]);
            ArmorID = int.Parse(gearsplit[1]);
            HeadID = int.Parse(gearsplit[2]);
            AccessoryID = int.Parse(gearsplit[3]);

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
        }

        public string Name()
        {
            return DataManager.GetTextData(ID, "Name", DataType.Job);
        }

        public string Description()
        {
            return DataManager.GetTextData(ID, "Description", DataType.Job);
        }
    }
}
