
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

        private int _iIdleFrames;
        public int IdleFrames => _iIdleFrames;
        private float _fIdleFrameLength;
        public float IdleFramesLength => _fIdleFrameLength;
        private int _iCastFrames;
        public int CastFrames => _iCastFrames;
        private float _fCastFrameLength;
        public float CastFramesLength => _fCastFrameLength;
        private int _iHitFrames;
        public int HitFrames => _iHitFrames;
        private float _fHitFrameLength;
        public float HitFramesLength => _fHitFrameLength;
        private int _iAttackFrames;
        public int AttackFrames => _iAttackFrames;
        private float _fAttackFrameLength;
        public float AttackFramesLength => _fAttackFrameLength;
        private int _iCriticalFrames;
        public int CriticalFrames => _iCriticalFrames;
        private float _fCriticalFrameLength;
        public float CriticalFramesLength => _fCriticalFrameLength;
        private int _iKOFrames;
        public int KOFrames => _iKOFrames;
        private float _fKOFrameLength;
        public float KOFramesLength => _fKOFrameLength;
        private int _iWinFrames;
        public int WinFrames => _iWinFrames;
        private float _fWinFrameLength;
        public float WinFramesLength => _fWinFrameLength;

        private string _name;
        public string Name { get => _name; }
        private string _description;
        public string Description { get => _description; }
        public List<MenuAction> ActionList;
        public List<CombatAction> _liSpecialActionsList;
        WeaponEnum _weaponType;
        public WeaponEnum WeaponType=> _weaponType;
        ArmorEnum _armorType;
        public ArmorEnum ArmorType => _armorType;

        public int WeaponID;
        public int ArmorID;
        public int HeadID;
        public int WristID;

        public CharacterClass(int id, string[] stringData)
        {
            ActionList = new List<MenuAction>();
            _liSpecialActionsList = new List<CombatAction>();
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
                else if (tagType[0].Equals("Idle"))
                {
                    string[] frameSplit = tagType[1].Split('-');
                    _iIdleFrames = int.Parse(frameSplit[0]);
                    _fIdleFrameLength = float.Parse(frameSplit[1]);
                }
                else if (tagType[0].Equals("Cast"))
                {
                    string[] frameSplit = tagType[1].Split('-');
                    _iCastFrames = int.Parse(frameSplit[0]);
                    _fCastFrameLength = float.Parse(frameSplit[1]);
                }
                else if (tagType[0].Equals("Hit"))
                {
                    string[] frameSplit = tagType[1].Split('-');
                    _iHitFrames = int.Parse(frameSplit[0]);
                    _fHitFrameLength = float.Parse(frameSplit[1]);
                }
                else if (tagType[0].Equals("Attack"))
                {
                    string[] frameSplit = tagType[1].Split('-');
                    _iAttackFrames = int.Parse(frameSplit[0]);
                    _fAttackFrameLength = float.Parse(frameSplit[1]);
                }
                else if (tagType[0].Equals("Crit"))
                {
                    string[] frameSplit = tagType[1].Split('-');
                    _iCriticalFrames = int.Parse(frameSplit[0]);
                    _fCriticalFrameLength = float.Parse(frameSplit[1]);
                }
                else if (tagType[0].Equals("KO"))
                {
                    string[] frameSplit = tagType[1].Split('-');
                    _iKOFrames = int.Parse(frameSplit[0]);
                    _fKOFrameLength = float.Parse(frameSplit[1]);
                }
                else if (tagType[0].Equals("Win"))
                {
                    string[] frameSplit = tagType[1].Split('-');
                    _iWinFrames = int.Parse(frameSplit[0]);
                    _fWinFrameLength = float.Parse(frameSplit[1]);
                }
                else if (tagType[0].Equals("Ability"))
                {
                    ActionList.Add(ActorManager.GetActionByIndex(int.Parse(tagType[1])));
                }
                else if (tagType[0].Equals("Spell"))
                {
                    string[] spellSplit = tagType[1].Split('-');
                    foreach (string spell in spellSplit)
                    {
                        CombatAction ac = (CombatAction)ActorManager.GetActionByIndex(int.Parse(spell));
                        _liSpecialActionsList.Add(ac);
                    }
                }
                else if (tagType[0].Equals("DWeap"))
                {
                    WeaponID = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("DArmor"))
                {
                    ArmorID = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("DHead"))
                {
                    HeadID = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("DWrist"))
                {
                    WristID = int.Parse(tagType[1]);
                }
            }

            //Adds Special, Use Item, Move, and Escape to the Actions
            ActionList.Add(ActorManager.GetActionByIndex(3));
            ActionList.Add(ActorManager.GetActionByIndex(2));
            ActionList.Add(ActorManager.GetActionByIndex(0));
            ActionList.Add(ActorManager.GetActionByIndex(1));
        }
    }
}
