using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class Summon : CombatActor
    {
        ElementEnum _element = ElementEnum.None;
        public ElementEnum Element => _element;

        int _iMagStat;
        List<KeyValuePair<AttributeEnum, int>> _liBuffedStats;
        public List<KeyValuePair<AttributeEnum, int>> BuffedStats => _liBuffedStats;

        public bool Acted;
        bool _bTwinCast;
        public bool TwinCast => _bTwinCast;
        bool _bAggressive;
        public bool Aggressive => _bAggressive;
        bool _bRegen;
        public bool Regen => _bRegen;

        public CombatActor linkedChar;

        public Summon(int id, Dictionary<string, string> data)
        {
            _liBuffedStats = new List<KeyValuePair<AttributeEnum, int>>();
            _bGuard = data.ContainsKey("Defensive");
            _bAggressive = data.ContainsKey("Aggressive");
            _bTwinCast = data.ContainsKey("TwinCast");
            _bRegen = data.ContainsKey("Regen");
            Counter = data.ContainsKey("Counter");

            if (data.ContainsKey("Element"))
            {
                _element = Util.ParseEnum<ElementEnum>(data["Element"]);
            }

            foreach (AttributeEnum stat in Enum.GetValues(typeof(AttributeEnum)))
            {
                if (data.ContainsKey(Util.GetEnumString(stat)))
                {
                    _liBuffedStats.Add(new KeyValuePair<AttributeEnum, int>(stat, 0));
                }
            }

            Util.AssignValue(ref _iBodyWidth, "Width", data);
            Util.AssignValue(ref _iBodyHeight, "Height", data);

            LoadSpriteAnimations(ref _sprBody, Util.LoadCombatAnimations(data), data["Texture"]);

            _sprBody.SetNextAnimation(AnimationEnum.Spawn, AnimationEnum.Idle);
            _sprBody.PlayAnimation(AnimationEnum.Spawn);
            _sprBody.SetScale(5);
        }

        public void SetStats(int magStat)
        {
            _iMagStat = magStat;
            _diAttributes[AttributeEnum.Strength] = 2 * magStat + 10;
            _diAttributes[AttributeEnum.Defense] = 2 * magStat + 10;
            _diAttributes[AttributeEnum.MaxHealth] = (3 * magStat) + 80;
            _diAttributes[AttributeEnum.Magic] = 2 * magStat + 10;
            _diAttributes[AttributeEnum.Resistance] = 2 * magStat + 10;
            _diAttributes[AttributeEnum.Speed] = 10;

            for (int i = 0; i < _liBuffedStats.Count; i++)
            {
                _liBuffedStats[i] = new KeyValuePair<AttributeEnum, int>(_liBuffedStats[i].Key, magStat / 2);
            }

            CurrentHP = MaxHP;
        }

        public override int DecreaseHealth(int value)
        {
            int rv = base.DecreaseHealth(value);

            if (CurrentHP == 0)
            {
                linkedChar.UnlinkSummon();
            }

            return rv;
        }
        public override GUISprite GetSprite()
        {
            return Tile.GUITile.SummonSprite;
        }

        public override bool IsSummon() { return true; }
    }
}
