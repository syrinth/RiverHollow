using RiverHollow.Actors.CombatStuff;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters.Lite
{
    public class LitePartyMember : LiteCombatActor
    {

        #region Properties
        public override string Name => _world.Name;

        public static List<int> LevelRange = new List<int> { 0, 40, 80, 160, 320, 640, 1280, 2560, 5120, 10240 };
        protected ClassedCombatant _world;
        public ClassedCombatant World => _world;

        protected CharacterClass _class;
        public CharacterClass CharacterClass { get => _class; }
        private int _classLevel;
        public int ClassLevel { get => _classLevel; }

        private int _iXP;
        public int XP { get => _iXP; }

        public bool Protected;

        public List<GearSlot> _liGearSlots;
        public GearSlot Weapon;
        public GearSlot Armor;
        public GearSlot Head;
        public GearSlot Wrist;
        public GearSlot Accessory1;
        public GearSlot Accessory2;

        public override int Attack => GetGearAtk();
        public override int StatStr => 10 + _iBuffStr + GetGearStat(StatEnum.Str);
        public override int StatDef => 10 + _iBuffDef + GetGearStat(StatEnum.Def) + (Protected ? 10 : 0);
        public override int StatVit => 10 + (_classLevel * _class.StatVit) + GetGearStat(StatEnum.Vit);
        public override int StatMag => 10 + _iBuffMag + GetGearStat(StatEnum.Mag);
        public override int StatRes => 10 + _iBuffRes + GetGearStat(StatEnum.Res);
        public override int StatSpd => 10 + _class.StatSpd + _iBuffSpd + GetGearStat(StatEnum.Spd);

        public int TempStatStr => 10 + _iBuffStr + GetTempGearStat(StatEnum.Str);
        public int TempStatDef => 10 + _iBuffDef + GetTempGearStat(StatEnum.Def) + (Protected ? 10 : 0);
        public int TempStatVit => 10 + (_classLevel * _class.StatVit) + GetTempGearStat(StatEnum.Vit);
        public int TempStatMag => 10 + _iBuffMag + GetTempGearStat(StatEnum.Mag);
        public int TempStatRes => 10 + _iBuffRes + GetTempGearStat(StatEnum.Res);
        public int TempStatSpd => 10 + _class.StatSpd + _iBuffSpd + GetTempGearStat(StatEnum.Spd);

        public override List<LiteMenuAction> AbilityList { get => _class.LiteActionList; }
        public override List<LiteCombatAction> SpecialActions { get => _class._liSpecialLiteActionsList; }

        public int GetGearAtk()
        {
            int rv = 0;

            rv += Weapon.GetStat(StatEnum.Atk);
            rv += base.Attack;

            return rv;
        }

        public int GetGearStat(StatEnum stat)
        {
            int rv = 0;

            foreach (GearSlot g in _liGearSlots)
            {
                rv += g.GetStat(stat);
            }

            return rv;
        }

        public int GetTempGearStat(StatEnum stat)
        {
            int rv = 0;

            foreach (GearSlot g in _liGearSlots)
            {
                rv += g.GetTempStat(stat);
            }

            return rv;
        }

        #endregion
        public LitePartyMember() : base()
        {
            _eActorType = ActorEnum.PartyMember;
            _classLevel = 1;

            _liGearSlots = new List<GearSlot>();
            Weapon = new GearSlot(EquipmentEnum.Weapon);
            Armor = new GearSlot(EquipmentEnum.Armor);
            Head = new GearSlot(EquipmentEnum.Head);
            Wrist = new GearSlot(EquipmentEnum.Wrist);
            Accessory1 = new GearSlot(EquipmentEnum.Accessory);
            Accessory2 = new GearSlot(EquipmentEnum.Accessory);

            _liGearSlots.Add(Weapon);
            _liGearSlots.Add(Armor);
            _liGearSlots.Add(Head);
            _liGearSlots.Add(Wrist);
            _liGearSlots.Add(Accessory1);
            _liGearSlots.Add(Accessory2);
        }

        public LitePartyMember(ClassedCombatant w) : this()
        {
            _world = w;
        }

        public override void LoadContent(string texture)
        {
            _sprBody = new AnimatedSprite(texture);
            int xCrawl = 0;
            RHSize frameSize = new RHSize(2, 2);
            _sprBody.AddAnimation(LiteCombatActionEnum.Idle, xCrawl * TILE_SIZE, 0, frameSize, 2, 0.5f);
            xCrawl += 2 * frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Cast, xCrawl * TILE_SIZE, 0, frameSize, 3, 0.4f);
            xCrawl += 3 * frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Hurt, xCrawl * TILE_SIZE, 0, frameSize, 1, 0.5f);
            xCrawl += 1 * frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Attack, xCrawl * TILE_SIZE, 0, frameSize, 1, 0.3f);
            xCrawl += 1 * frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Critical, xCrawl * TILE_SIZE, 0, frameSize, 2, 0.9f);
            xCrawl += 2 * frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.KO, xCrawl * TILE_SIZE, 0, frameSize, 1, 0.5f);
            xCrawl += 1 * frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Victory, xCrawl * TILE_SIZE, 0, frameSize, 2, 0.5f);

            _sprBody.PlayAnimation(LiteCombatActionEnum.Idle);
            _sprBody.SetScale(LiteCombatManager.CombatScale);
            _iBodyWidth = frameSize.Width * LiteCombatManager.CombatScale;
            _iBodyHeight = frameSize.Height * LiteCombatManager.CombatScale;
        }

        public void SetClass(CharacterClass x)
        {
            _class = x;
            _iCurrentHP = MaxHP;
            _iCurrentMP = MaxMP;

            Weapon.SetGear((Equipment)DataManager.GetItem(_class.WeaponID));
            Armor.SetGear((Equipment)DataManager.GetItem(_class.ArmorID));
            Head.SetGear((Equipment)DataManager.GetItem(_class.HeadID));
            Wrist.SetGear((Equipment)DataManager.GetItem(_class.WristID));

            LoadContent(DataManager.FOLDER_PARTY + "Wizard");
        }

        public void AddXP(int x)
        {
            _iXP += x;
            if (_iXP >= LevelRange[_classLevel])
            {
                _classLevel++;
            }
        }

        public void GetXP(ref double curr, ref double max)
        {
            curr = _iXP;
            max = LitePartyMember.LevelRange[this.ClassLevel];
        }

        /// <summary>
        /// Structure that represents the slot for the character.
        /// Holds both the actual item and a temp item to compare against.
        /// </summary>
        public class GearSlot
        {
            EquipmentEnum _enumType;
            Equipment _eGear;
            Equipment _eTempGear;
            public GearSlot(EquipmentEnum type)
            {
                _enumType = type;
            }

            public void SetGear(Equipment e) { _eGear = e; }
            public void SetTemp(Equipment e) { _eTempGear = e; }

            public int GetStat(StatEnum stat)
            {
                int rv = 0;

                if (_eGear != null)
                {
                    rv += _eGear.GetStat(stat);
                }

                return rv;
            }
            public int GetTempStat(StatEnum stat)
            {
                int rv = 0;

                if (_eTempGear != null)
                {
                    rv += _eTempGear.GetStat(stat);
                }
                else if (_eGear != null)
                {
                    rv += _eGear.GetStat(stat);
                }

                return rv;
            }
            public Equipment GetItem() { return _eGear; }
        }
    }
}
