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

        private int _statStr;
        public int StatStr => _statStr;
        private int _statDef;
        public int StatDef => _statDef;
        private int _statVit;
        public int StatVit => _statVit;
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

        private string _sName;
        public string Name { get => _sName; }
        private string _sDescription;
        public string Description { get => _sDescription; }
        public List<LiteMenuAction> LiteActionList;
        public List<LiteCombatAction> _liSpecialLiteActionsList;
        public List<TacticalMenuAction> TacticalActionList;
        public List<TacticalCombatAction> _liSpecialTacticalActionsList;
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
            LiteActionList = new List<LiteMenuAction>();
            _liSpecialLiteActionsList = new List<LiteCombatAction>();
            TacticalActionList = new List<TacticalMenuAction>();
            _liSpecialTacticalActionsList = new List<TacticalCombatAction>();

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

            if (stringData.ContainsKey("Ability"))
            {
                string[] split = stringData["Ability"].Split('|');
                foreach (string ability in split)
                {
                    TacticalCombatAction ac = (TacticalCombatAction)DataManager.GetTacticalActionByIndex(int.Parse(ability));
                    TacticalActionList.Add(ac);

                    LiteCombatAction lite = (LiteCombatAction)DataManager.GetLiteActionByIndex(int.Parse(ability));
                    LiteActionList.Add(lite);
                }
            }

            if (stringData.ContainsKey("Spell"))
            {
                string[] spellSplit = stringData["Spell"].Split('|');
                foreach (string spell in spellSplit)
                {
                    TacticalCombatAction ac = (TacticalCombatAction)DataManager.GetTacticalActionByIndex(int.Parse(spell));
                    _liSpecialTacticalActionsList.Add(ac);

                    LiteCombatAction lite = (LiteCombatAction)DataManager.GetLiteActionByIndex(int.Parse(spell));
                    _liSpecialLiteActionsList.Add(lite);
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


            //Adds Special, Use Item, Move, and End Turn
            if (RiverHollow.COMBAT_STYLE == CombatStyleEnum.Lite)
            {
                LiteActionList.Add(new LiteMenuAction(2, ActionEnum.MenuSpell, new Vector2(1, 0)));
                LiteActionList.Add(new LiteMenuAction(1, ActionEnum.MenuItem, new Vector2(2, 0)));
                LiteActionList.Add(DataManager.GetLiteActionByIndex(0));
                LiteActionList.Add(new LiteMenuAction(3, ActionEnum.EndTurn, new Vector2(4, 0)));
            }
            else
            {
                TacticalActionList.Add(new TacticalMenuAction(2, ActionEnum.MenuSpell, new Vector2(1, 0)));
                TacticalActionList.Add(new TacticalMenuAction(1, ActionEnum.MenuItem, new Vector2(2, 0)));
                TacticalActionList.Add(new TacticalMenuAction(0, ActionEnum.Move, new Vector2(3, 0)));
                TacticalActionList.Add(new TacticalMenuAction(3, ActionEnum.EndTurn, new Vector2(4, 0)));
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
    }
}
