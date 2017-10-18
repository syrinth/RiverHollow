using RiverHollow.Characters;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIObjects;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    public static class CombatManager
    {
        private static Mob _mob;
        public static Mob CurrentMob { get => _mob; }
        private static List<CombatCharacter> _listMonsters;
        public static List<CombatCharacter> Monsters { get => _listMonsters; }
        private static List<CombatCharacter> _listParty;
        public static List<CombatCharacter> Party { get => _listParty; }
        public static List<CombatCharacter> _turnOrder;

        public enum Phase { EnemyTurn, SelectSkill, Targetting, Waiting, EndCombat }
        public static Phase CurrentPhase;

        public static int _currentTurnIndex;

        public static Ability _skill;

        public static int Delay;
        public static string Text;

        public static void NewBattle(Mob m)
        {
            Delay = 0;
            _mob = m;
            _listMonsters = _mob.Monsters;
            _listParty = PlayerManager.GetParty();
            _turnOrder = new List<CombatCharacter>();
            _turnOrder.Add(_listParty[0]);
            _turnOrder.AddRange(_listMonsters);

            _currentTurnIndex = 0;
            SetPhaseForTurn();
            RiverHollow.ChangeGameState(RiverHollow.GameState.Combat);
        }

        public static void NextTurn()
        {
            if (CurrentPhase != Phase.EndCombat)
            {
                if (_currentTurnIndex < _turnOrder.Count-1)
                {
                    _currentTurnIndex++;
                }
                else
                {
                    _currentTurnIndex = 0;
                }
                SetPhaseForTurn();
            }
        }

        private static void SetPhaseForTurn()
        {
            if (_listMonsters.Contains(_turnOrder[_currentTurnIndex]))
            {
                CurrentPhase = Phase.EnemyTurn;
            }
            else if (_listParty.Contains(_turnOrder[_currentTurnIndex]))
            {
                CurrentPhase = Phase.SelectSkill;
            }
            else if (Delay > 0)
            {
                CurrentPhase = Phase.Waiting;
            }
        }

        public static void TakeTurn(out int dmg)
        {
            CombatCharacter c = _turnOrder[_currentTurnIndex];
            UsingSkill(CharacterManager.GetAbilityByIndex(1));
            UseSkillOnTarget(_listParty[0], out dmg);
        }

        public static void UsingSkill(Ability a)
        {
            _skill = a;
            CurrentPhase = Phase.Targetting;
        }

        public static void UseSkillOnTarget(CombatCharacter target, out int dmg)
        {
            dmg = _skill.Dmg;
            target.DecreaseHealth(dmg);
            Text = _skill.Name;
            Delay = 40;
            CurrentPhase = Phase.Waiting;
        }

        public static void Update()
        {
            if(Delay > 0) {
                Delay--;
                if(Delay == 0)
                {
                    if(string.IsNullOrEmpty(Text)) { Text = string.Empty; }
                    NextTurn();
                }
            }
        }

        public static void EndBattle()
        {
            CurrentPhase = Phase.EndCombat;
            MapManager.RemoveMob(_mob);
            MapManager.DropWorldItems(DropManager.DropItemsFromMob(_mob.ID), _mob.CollisionBox.Center.ToVector2());
            _mob = null;
            RiverHollow.ChangeGameState(RiverHollow.GameState.WorldMap);
        }

        public static void Kill(CombatCharacter c)
        {
            if (_listMonsters.Contains((c)))
            {
                _listMonsters.Remove(c);
            }
            _turnOrder.Remove(c);
            if(_listMonsters.Count == 0)
            {
                EndBattle();
            }
        }
    }
}
