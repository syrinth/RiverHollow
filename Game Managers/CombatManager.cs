using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
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

        public enum Phase { EnemyTurn, SelectSkill, Targetting, DisplayAttack, Animation, EndCombat }
        public static Phase CurrentPhase;

        public static int _currentTurnIndex;

        public static Ability _skill;
        private static Position _target;
        public static int PlayerTarget;

        public static double Delay;
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
                CurrentPhase = Phase.DisplayAttack;
            }
        }

        public static void TakeTurn()
        {
            CombatCharacter c = _turnOrder[_currentTurnIndex];
            UsingSkill(CharacterManager.GetAbilityByIndex(1));
            PlayerTarget = 0;
        }

        public static void UsingSkill(Ability a)
        {
            _skill = a;
            _skill.Sprite.IsAnimating = true;
            CurrentPhase = Phase.Targetting;
        }

        public static void UseSkillOnTarget(Position target)
        {
            _target = target;
            _skill.SkillUser = _turnOrder[_currentTurnIndex];
            _skill.PreEffect(target);
            Text = _skill.Name;
            Delay = 0.5;
            CurrentPhase = Phase.DisplayAttack;
        }

        public static void Update(GameTime gameTime)
        {
            if(Delay > 0) {
                Delay -= gameTime.ElapsedGameTime.TotalSeconds;
                if (Delay <= 0)
                {
                    Delay = 0;
                    if (CurrentPhase == Phase.DisplayAttack)
                    {
                        if (!string.IsNullOrEmpty(Text)) { Text = string.Empty; }
                        CurrentPhase = Phase.Animation;
                        Delay = _skill.GetDelay();
                    }
                    else if (CurrentPhase == CombatManager.Phase.EndCombat)
                    {
                        EndBattle();
                    }
                    else
                    {
                        if (_skill.IsFinished())
                        {
                            _skill.ApplyEffect(_target);
                            _skill = null;
                            NextTurn();
                        }
                    }
                }
                else
                {
                    if (CurrentPhase == CombatManager.Phase.Animation)
                    {
                        if (!_skill.IsFinished() && _skill.TargetPosition != Vector2.Zero)
                        {
                            AnimatedSprite s = _skill.Sprite;
                            Vector2 direction = Vector2.Zero;
                            Utilities.GetMoveSpeed(s.Position, _skill.TargetPosition, 10, ref direction);
                            s.Position += direction;
                            Delay += gameTime.ElapsedGameTime.TotalSeconds;
                        }
                    }
                }
            }
        }

        public static void EndBattle()
        {
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
                Delay = 1;
                CurrentPhase = Phase.EndCombat;
            }
        }
    }
}
