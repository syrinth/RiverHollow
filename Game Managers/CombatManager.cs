using Microsoft.Xna.Framework;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Misc;
using System.Collections.Generic;

namespace RiverHollow.Game_Managers
{
    public static class CombatManager
    {
        private static int _xpValue;
        private static Mob _mob;
        private static int stamDrain = 1;
        public static Mob CurrentMob { get => _mob; }
        public static CombatCharacter ActiveCharacter;
        private static List<CombatCharacter> _listMonsters;
        public static List<CombatCharacter> Monsters { get => _listMonsters; }
        private static List<CombatCharacter> _listParty;
        public static List<CombatCharacter> Party { get => _listParty; }
        public static List<CombatCharacter> TurnOrder;

        public enum Phase { NewTurn, EnemyTurn, SelectSkill, ChooseSkillTarget, ChooseItemTarget, DisplayAttack, PerformAction, EndCombat }
        public static Phase CurrentPhase;

        public static int TurnIndex;

        public static CombatAction ChosenSkill;
        public static CombatItem ChosenItem;
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
            foreach (Monster mon in _listMonsters) { _xpValue += mon.XP; }                                      //Sets the accumulated xp for the battle

            _listParty = new List<CombatCharacter>();
            _listParty.AddRange(PlayerManager.GetParty());

            TurnOrder = new List<CombatCharacter>();
            TurnOrder.AddRange(_listParty);
            TurnOrder.AddRange(_listMonsters);

            RHRandom r = new RHRandom();
            foreach (CombatCharacter c in TurnOrder) { c.Initiative =  r.Next(1, 20) + (c.StatSpd/2); }         //Roll initiative for everyone
            TurnOrder.Sort((x, y) => x.Initiative.CompareTo(y.Initiative));

            TurnIndex = 0;
            ActiveCharacter = TurnOrder[TurnIndex];
            SetPhaseForTurn();
            PlayerManager.DecreaseStamina(1);
            GameManager.GoToCombat();
        }

        public static void Update(GameTime gameTime)
        {
        }

        //If we have not gone to the EndTurn phase, increment the turn loop as appropriate
        //If we loop back to 0, reduce stamina by the desired amount.
        public static void NextTurn()
        {
            if (CurrentPhase != Phase.EndCombat)
            {
                if (TurnIndex < TurnOrder.Count-1) { TurnIndex++; }
                else
                {
                    TurnIndex = 0;
                    PlayerManager.DecreaseStamina(stamDrain);
                    GameCalendar.IncrementMinutes();
                }

                ActiveCharacter = TurnOrder[TurnIndex];
                if (ActiveCharacter.CurrentHP == 0) { NextTurn(); }
                else
                {
                    ActiveCharacter.TickBuffs();
                    SetPhaseForTurn();
                }
            }
        }

        private static void SetPhaseForTurn()
        {
            if (_listMonsters.Contains(ActiveCharacter)) {
                CurrentPhase = Phase.EnemyTurn;
                EnemyTakeTurn();
            }
            else if (_listParty.Contains(ActiveCharacter)) { CurrentPhase = Phase.NewTurn; }
        }

        //For now, when the enemy takes their turn, have them select a random party member
        //When enemies get healing/defensive skills, they'll have their own logic to process
        public static void EnemyTakeTurn()
        {
            RHRandom r = new RHRandom();
            ProcessActionChoice((CombatAction)CharacterManager.GetActionByIndex(1), false);//ActiveCharacter.AbilityList[r.Next(0, ActiveCharacter.AbilityList.Count - 1)]);
            if (!ChosenSkill.Target.Equals("Self"))
            {
                do
                {
                    PlayerTarget = r.Next(0, _listParty.Count - 1);
                } while (_listParty[PlayerTarget].CurrentHP == 0);
            }
        }

        public static void ProcessActionChoice(CombatAction a, bool chooseTarget = true)
        {
            ChosenSkill = a;
            ChosenSkill.SkillUser = ActiveCharacter;

            ActiveCharacter.CurrentMP -= a.MPCost;          //Checked before Processing
            if (!ChosenSkill.Target.Equals("Self"))
            {
                if (chooseTarget) { CurrentPhase = Phase.ChooseSkillTarget; }  //Skips this phase for enemies. They don't "choose" targets
            }
            else
            {
                ChosenSkill.ApplyEffectToSelf();
                ChosenSkill = null;
                NextTurn();
            }
        }

        public static void ProcessItemChoice(CombatItem it)
        {
            CurrentPhase = Phase.ChooseItemTarget;
            ChosenItem = it;
        }

        //Assign target to the skill as well as the skill user
        public static void SetSkillTarget(BattleLocation target)
        {
            _target = target;
            ChosenSkill.UserStartPosition = ActiveCharacter.Position;
            ChosenSkill.AnimationSetup(target);
            Text = ChosenSkill.Name;
            CurrentPhase = Phase.DisplayAttack;
        }

        public static void SetItemTarget(BattleLocation target)
        {
            _target = target;
            Text = ChosenItem.Name;
            CurrentPhase = Phase.DisplayAttack;
        }

        //Gives the total battle XP to every member of the party, remove the mob from the gameand drop items
        public static void EndBattle()
        {
            if (PartyUp())
            {
                foreach (CombatAdventurer a in _listParty) { a.AddXP(_xpValue); }
                MapManager.DropWorldItems(DropManager.DropItemsFromMob(_mob.ID), _mob.CollisionBox.Center.ToVector2());
            }
            MapManager.RemoveMob(_mob);
            _mob = null;
            GameManager.GoToWorldMap();
        }

        public static void Kill(CombatCharacter c)
        {
            if (_listMonsters.Contains((c)))
            {
                _listMonsters.Remove(c);
                TurnOrder.Remove(c);                                            //Remove the killed member from the turn order

                if (_listMonsters.Count == 0)
                {
                    Delay = 1;
                    EndBattle();
                }
            }
            else
            {
                if (!PartyUp()) { EndBattle(); }
            }
        }

        public static bool PartyUp()
        {
            bool stillOne = false;
            foreach (CombatCharacter character in _listParty)
            {
                if (character.CurrentHP > 0) { stillOne = true; }
            }

            return stillOne;
        }

        public static bool PhaseSelectSkill() { return CurrentPhase == Phase.SelectSkill; }
        public static bool PhaseChooseSkillTarget() { return CurrentPhase == Phase.ChooseSkillTarget; }
        public static bool PhaseChooseItemTarget() { return CurrentPhase == Phase.ChooseItemTarget; }

        internal static void UseItem()
        {
            int val = _target.Character.IncreaseHealth(CombatManager.ChosenItem.Health);
            _target.Heal(val);
            InventoryManager.RemoveItemFromInventory(ChosenItem);

            NextTurn();
        }
    }
}
