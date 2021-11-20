﻿using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Characters
{
    public class LiteSummon : LiteCombatActor
    {
        ElementEnum _element = ElementEnum.None;
        public ElementEnum Element => _element;

        int _iMagStat;
        List<KeyValuePair<StatEnum, int>> _liBuffedStats;
        public List<KeyValuePair<StatEnum, int>> BuffedStats => _liBuffedStats;

        public bool Acted;
        bool _bTwinCast;
        public bool TwinCast => _bTwinCast;
        bool _bAggressive;
        public bool Aggressive => _bAggressive;
        bool _bRegen;
        public bool Regen => _bRegen;

        public LiteCombatActor linkedChar;

        public LiteSummon(int id, Dictionary<string, string> stringData)
        {
            _liBuffedStats = new List<KeyValuePair<StatEnum, int>>();
            _bGuard = stringData.ContainsKey("Defensive");
            _bAggressive = stringData.ContainsKey("Aggressive");
            _bTwinCast = stringData.ContainsKey("TwinCast");
            _bRegen = stringData.ContainsKey("Regen");
            Counter = stringData.ContainsKey("Counter");

            if (stringData.ContainsKey("Element"))
            {
                _element = Util.ParseEnum<ElementEnum>(stringData["Element"]);
            }

            foreach (StatEnum stat in Enum.GetValues(typeof(StatEnum)))
            {
                if (stringData.ContainsKey(Util.GetEnumString(stat)))
                {
                    _liBuffedStats.Add(new KeyValuePair<StatEnum, int>(stat, 0));
                }
            }

            string[] spawn = stringData["Spawn"].Split('-');
            string[] idle = stringData["Idle"].Split('-');
            string[] cast = stringData["Cast"].Split('-');
            string[] attack = stringData["Attack"].Split('-');

            RHSize frameSize = new RHSize(16, 16);
            int startX = 0;
            int startY = 0;

            _sprBody = new AnimatedSprite(@"Textures\Actors\Summons\" + stringData["Texture"]);
            _sprBody.AddAnimation(LiteCombatActionEnum.Spawn, startX, startY, frameSize, int.Parse(spawn[0]), float.Parse(spawn[1]));
            startX += int.Parse(spawn[0]) * frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Idle, startX, startY, frameSize, int.Parse(idle[0]), float.Parse(idle[1]));
            startX += int.Parse(idle[0]) * frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Cast, startX, startY, frameSize, int.Parse(cast[0]), float.Parse(cast[1]));
            startX += int.Parse(cast[0]) * frameSize.Width;
            _sprBody.AddAnimation(LiteCombatActionEnum.Attack, startX, startY, frameSize, int.Parse(attack[0]), float.Parse(attack[1]));
            _sprBody.SetNextAnimation(LiteCombatActionEnum.Spawn, LiteCombatActionEnum.Idle);
            _sprBody.PlayAnimation(LiteCombatActionEnum.Spawn);
            _sprBody.SetScale(5);
        }

        public void SetStats(int magStat)
        {
            _iMagStat = magStat;
            _iStrength = 2 * magStat + 10;
            _iDefense = 2 * magStat + 10;
            _iVitality = (3 * magStat) + 80;
            _iMagic = 2 * magStat + 10;
            _iResistance = 2 * magStat + 10;
            _iSpeed = 10;

            for (int i = 0; i < _liBuffedStats.Count; i++)
            {
                _liBuffedStats[i] = new KeyValuePair<StatEnum, int>(_liBuffedStats[i].Key, magStat / 2);
            }

            CurrentHP = MaxHP;
        }

        public override int DecreaseHealth(double value)
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
