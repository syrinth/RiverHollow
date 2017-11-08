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
        private static int _xpValue;
        private static Mob _mob;
        public static Mob CurrentMob { get => _mob; }
        private static List<CombatCharacter> _listMonsters;
        public static List<CombatCharacter> Monsters { get => _listMonsters; }
        private static List<CombatCharacter> _listParty;
        public static List<CombatCharacter> Party { get => _listParty; }
        public static List<CombatCharacter> TurnOrder;

        public enum Phase { EnemyTurn, SelectSkill, Targetting, DisplayAttack, Animation, EndCombat }
        public static Phase CurrentPhase;

        public static int TurnIndex;

        public static Ability _skill;
        private static BattleLocation _target;
        public static int PlayerTarget;

        public static double Delay;
        public static string Text;

        public static void NewBattle(Mob m)
        {
            Delay = 0;
            _mob = m;
            _listMonsters = _mob.Monsters;
            _xpValue = 0;
            foreach (Monster mon in _listMonsters)
            {
                _xpValue += mon.XP;
            }

            _listParty = new List<CombatCharacter>();
            foreach (CombatAdventurer c in PlayerManager.GetParty()) {
                _listParty.Add(c);
            }

            TurnOrder = new List<CombatCharacter>();
            TurnOrder.AddRange(_listParty);
            TurnOrder.AddRange(_listMonsters);

            RHRandom r = new RHRandom();
            foreach (CombatCharacter c in TurnOrder)
            {
                c.Initiative =  r.Next(1, 20) + (c.StatSpd/2);
            }
            TurnOrder.Sort((x, y) => x.Initiative.CompareTo(y.Initiative));

            TurnIndex = 0;
            SetPhaseForTurn();
            PlayerManager.DecreaseStamina(1);
            RiverHollow.ChangeGameState(RiverHollow.GameState.Combat);
        }

        public static void NextTurn()
        {
            if (CurrentPhase != Phase.EndCombat)
            {
                if (TurnIndex < TurnOrder.Count-1)
                {
                    TurnIndex++;
                }
                else
                {
                    TurnIndex = 0;
                    PlayerManager.DecreaseStamina(1);
                }
                SetPhaseForTurn();
            }
        }

        private static void SetPhaseForTurn()
        {
            if (_listMonsters.Contains(TurnOrder[TurnIndex]))
            {
                CurrentPhase = Phase.EnemyTurn;
            }
            else if (_listParty.Contains(TurnOrder[TurnIndex]))
            {
                CurrentPhase = Phase.SelectSkill;
            }
            else if (Delay > 0)
            {
                CurrentPhase = Phase.DisplayAttack;
            }
        }

        public static void EnemyTakeTurn()
        {
            CombatCharacter c = TurnOrder[TurnIndex];
            UsingSkill(CharacterManager.GetAbilityByIndex(1));
            RHRandom r = new RHRandom();
            PlayerTarget = r.Next(0, _listParty.Count-1);
        }

        public static void UsingSkill(Ability a)
        {
            _skill = a;
            _skill.Sprite.IsAnimating = true;
            CurrentPhase = Phase.Targetting;
        }

        public static void UseSkillOnTarget(BattleLocation target)
        {
            _target = target;
            _skill.SkillUser = TurnOrder[TurnIndex];
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
                            Utilities.GetMoveSpeed(s.Position, _skill.TargetPosition, 20, ref direction);
                            s.Position += direction;
                            Delay += gameTime.ElapsedGameTime.TotalSeconds;
                        }
                    }
                }
            }
        }

        public static void EndBattle()
        {
            foreach(CombatAdventurer a in _listParty)
            {
                a.AddXP(_xpValue);
            }
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
            TurnOrder.Remove(c);
            if(_listMonsters.Count == 0)
            {
                Delay = 1;
                CurrentPhase = Phase.EndCombat;
            }
        }
    }
}
