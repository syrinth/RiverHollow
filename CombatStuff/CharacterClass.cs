
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Actors.CombatStuff
{
    public class CharacterClass
    {
        int _iID;
        public int ID => _iID;

        private int _statStr;
        public int StatStr => _statStr;
        private int _statDef;
        public int StatDef  => _statDef;
        private int _statVit;
        public int StatVit  => _statVit;
        private int _statMagic;
        public int StatMag => _statMagic;
        private int _statRes;
        public int StatRes => _statRes;
        private int _statSpd;
        public int StatSpd => _statSpd;

        private string _name;
        public string Name { get => _name; }
        private string _description;
        public string Description { get => _description; }
        public List<MenuAction> AbilityList;
        public List<CombatAction> _spellList;
        WeaponEnum _weaponType;
        public WeaponEnum WeaponType=> _weaponType;
        ArmorEnum _armorType;
        public ArmorEnum ArmorType => _armorType;

        public CharacterClass(int id, string[] stringData)
        {
            AbilityList = new List<MenuAction>();
            _spellList = new List<CombatAction>();
            ImportBasics(id, stringData);
        }

        protected void ImportBasics(int id, string[] stringData)
        {
            _iID = id;

            GameContentManager.GetClassText(_iID, ref  _name, ref _description);

            foreach (string s in stringData)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Weapon"))
                {
                    _weaponType = Util.ParseEnum<WeaponEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Armor"))
                {
                    _armorType = Util.ParseEnum<ArmorEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Dmg"))
                {
                    _statStr = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Def"))
                {
                    _statDef = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Hp"))
                {
                    _statVit = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Mag"))
                {
                    _statMagic = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Spd"))
                {
                    _statSpd = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Ability"))
                {
                    string[] abilitySplit = tagType[1].Split('-');
                    foreach (string ability in abilitySplit)
                    {
                        AbilityList.Add(ActorManager.GetActionByIndex(int.Parse(ability)));
                    }
                }
                else if (tagType[0].Equals("Spell"))
                {
                    string[] spellSplit = tagType[1].Split('-');
                    foreach (string spell in spellSplit)
                    {
                        CombatAction ac = (CombatAction)ActorManager.GetActionByIndex(int.Parse(spell));
                        _spellList.Add(ac);
                    }
                }
            }
        }
    }
}
