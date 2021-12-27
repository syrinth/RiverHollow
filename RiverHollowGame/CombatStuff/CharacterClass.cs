using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.CombatStuff
{
    public class CharacterClass
    {
        int _iID;
        public int ID => _iID;

        Dictionary<AttributeEnum, int> _diAttributes;

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

        private string _sName;
        public string Name { get => _sName; }
        private string _sDescription;
        public string Description { get => _sDescription; }
        public List<CombatAction> Actions;

        WeaponEnum _weaponType;
        public WeaponEnum WeaponType => _weaponType;
        ArmorEnum _armorType;
        public ArmorEnum ArmorType => _armorType;

        public int WeaponID;
        public int ArmorID;
        public int HeadID;
        public int WristID;

        public CharacterClass()
        {
            Actions = new List<CombatAction>();

            _iID = -1;
        }

        public CharacterClass(int id, Dictionary<string, string> stringData) : this()
        {
            ImportBasics(id, stringData);
        }

        protected void ImportBasics(int id, Dictionary<string, string> stringData)
        {
            _iID = id;
            DataManager.GetTextData("Class", _iID, ref _sName, "Name");
            DataManager.GetTextData("Class", _iID, ref _sDescription, "Description");

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
            }

            //Doesn't seem to be given anymore?
            //_statStr = int.Parse(stringData["Dmg"]);
            //_statDef = int.Parse(stringData[Util.GetEnumString(StatEnum.Def)]);
            //_statVit = int.Parse(stringData["Hp"]);
            //_statMagic = int.Parse(stringData[Util.GetEnumString(StatEnum.Mag)]);
            //_statSpd = int.Parse(stringData[Util.GetEnumString(StatEnum.Spd)]);

            SetClassAnimation(stringData, "Idle", ref _iIdleFrames, ref _fIdleFrameLength);
            SetClassAnimation(stringData, "Cast", ref _iCastFrames, ref _fCastFrameLength);
            SetClassAnimation(stringData, "Hit", ref _iHitFrames, ref _fHitFrameLength);
            SetClassAnimation(stringData, "Attack", ref _iAttackFrames, ref _fAttackFrameLength);
            SetClassAnimation(stringData, "Crit", ref _iCriticalFrames, ref _fCriticalFrameLength);
            SetClassAnimation(stringData, "KO", ref _iKOFrames, ref _fKOFrameLength);
            SetClassAnimation(stringData, "Win", ref _iWinFrames, ref _fWinFrameLength);

            _diAttributes = new Dictionary<AttributeEnum, int>();
            foreach (AttributeEnum e in Enum.GetValues(typeof(AttributeEnum)))
            {
                if (stringData.ContainsKey(Util.GetEnumString(e))) { _diAttributes[e] = int.Parse(stringData[Util.GetEnumString(e)]); }
                else { _diAttributes[e] = 0; }
            }
        }

        private void SetClassAnimation(Dictionary<string, string> stringData, string key, ref int frames, ref float frameLength)
        {
            if (stringData.ContainsKey(key))
            {
                string[] frameSplit = stringData[key].Split('-');
                frames = int.Parse(frameSplit[0]);
                frameLength = float.Parse(frameSplit[1]);
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
