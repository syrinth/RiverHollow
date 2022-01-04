//using Microsoft.Xna.Framework;
//using RiverHollow.CombatStuff;
//using RiverHollow.Game_Managers;
//using RiverHollow.Utilities;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static RiverHollow.Game_Managers.GameManager;

//namespace RiverHollow.Characters
//{
//    public class TacticalSummon : TacticalCombatActor
//    {
//        public override Vector2 Position
//        {
//            get
//            {
//                return new Vector2(_sprBody.Position.X + TILE_SIZE, _sprBody.Position.Y + _sprBody.Height - (TILE_SIZE * (_iSize + 1)));
//            }
//            set
//            {
//                _sprBody.Position = new Vector2(value.X - TILE_SIZE, value.Y - _sprBody.Height + (TILE_SIZE * (_iSize + 1)));
//            }
//        }

//        ElementEnum _eElementType = ElementEnum.None;
//        public ElementEnum Element => _eElementType;

//        public override int Damage => _iMagStat;
//        int _iMagStat;

//        public bool Acted;

//        public TacticalCombatActor linkedChar;
//        private TacticalCombatAction _action;

//        public TacticalSummon(int id, Dictionary<string, string> stringData)
//        {
//            _eActorType = ActorEnum.Summon;

//            Util.AssignValue(ref _eElementType, "Element", stringData);

//            _action = DataManager.GetTacticalActionByIndex(int.Parse(stringData["Ability"]));

//            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(stringData), DataManager.FOLDER_SUMMONS + stringData["Texture"]);
//        }

//        public void SetStats(int magStat)
//        {
//            _iMagStat = magStat;
//            _iStrength = magStat;
//            _iDefense = magStat;
//            _iMaxHealth = magStat;
//            _iMagic = magStat;
//            _iResistance = magStat;
//            _iSpeed = 10;

//            CurrentHP = MaxHP;
//        }

//        public override void Update(GameTime gTime)
//        {
//            base.Update(gTime);

//            ///When the Monster has finished playing the KO animation, let the CombatManager know so it can do any final actions
//            if (IsCurrentAnimation(CombatAnimationEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
//            {
//                MapManager.RemoveActor(this);
//            }
//        }

//        public override void KO()
//        {
//            base.KO();
//            ClearTiles();
//            _diConditions[ConditionEnum.KO] = true;
//        }
//        public void TakeTurn()
//        {
//            _action.AssignUser(this);
//            _action.AssignTargetTile(BaseTile);
//            TacticalCombatManager.SelectedAction = _action;
//            TacticalCombatManager.ChangePhase(TacticalCombatManager.CmbtPhaseEnum.PerformAction);
//        }

//        /// <summary>
//        /// Local override for the Summon. Unlinks the Summon if it dies.
//        /// </summary>
//        /// <param name="value">Damage dealt</param>
//        /// <param name="bHarmful">Whether the modifier is harmful or helpful</param>
//        public override void ModifyHealth(double value, bool bHarmful)
//        {
//            base.ModifyHealth(value, bHarmful);

//            if (CurrentHP == 0)
//            {
//                linkedChar.UnlinkSummon();
//            }
//        }

//        public override bool IsSummon() { return true; }
//    }
//}
