﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.CombatStuff;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using RiverHollow.Utilities;
using System.Linq;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.GUIComponents.GUIObjects.Combat.Lite;

namespace RiverHollow.Game_Managers
{
    public static class CombatManager
    {
        public static Mob CurrentMob { get; private set; }
        public static List<CombatActor> Monsters { get; private set; }
        public static List<CombatActor> Party { get; private set; }

        public static bool PlayerTurn => Party.Contains(ActiveCharacter);

        public enum PhaseEnum { Upkeep, NewTurn, EnemyTurn, ChooseAction, ChooseTarget, Defeat, DisplayAttack, DisplayVictory, PerformAction, EndCombat }
        public static PhaseEnum CurrentPhase;
        public static ChosenAction SelectedAction;
        public static CombatTile TargetTile { get; private set; }

        public static double Delay;
        public static string Text;

        #region Turn Order Info
        public static CombatActor ActiveCharacter => _liCurrentRound[0];
        public static CombatActor StartingActor => _liNextRound[0].Key;

        static List<CombatActor> _liCurrentRound;
        public static IList<CombatActor> CurrentRoundOrder => _liCurrentRound.AsReadOnly();
        static List<KeyValuePair<CombatActor, int>> _liNextRound;
        #endregion

        #region CombatGrid
        public static CombatTile SelectedTile;

        public static readonly int MAX_COLUMN = 8;
        public static readonly int MAX_ROW = 3;
        public static readonly int ALLY_FRONT = 3;
        public static readonly int ENEMY_FRONT = 4;

        static CombatTile[,] _combatMap;
        #endregion

        public static void NewBattle(Mob m)
        {
            SelectedAction = null;
            SelectedTile = null;

            _combatMap = new CombatTile[MAX_ROW, MAX_COLUMN];
            for (int row = 0; row < MAX_ROW; row++)
            {
                for (int col = 0; col < MAX_COLUMN; col++)
                {
                    _combatMap[row, col] = new CombatTile(row, col, col < ENEMY_FRONT ? TargetEnum.Ally : TargetEnum.Enemy);
                }
            }

            Delay = 0;
            CurrentMob = m;
            Monsters = CurrentMob.Monsters;

            Party = new List<CombatActor>();
            Party.AddRange(PlayerManager.GetParty());

            _liCurrentRound = new List<CombatActor>();
            _liNextRound = new List<KeyValuePair<CombatActor, int>>();

            RollForNextRound();
            GoToNextRound();

            GoToCombatScreen();
            PlayerManager.DecreaseStamina(3);       //Decrease Stamina once
        }

        private static void RollForNextRound()
        {
            List<CombatActor> actors = new List<CombatActor>();
            actors.AddRange(Monsters);
            actors.AddRange(Party);

            _liNextRound.Clear();
            for (int i = 0; i < actors.Count; i++)
            {
                int initiative = RHRandom.Instance().Next(0, 10) + actors[i].Attribute(AttributeEnum.Speed);
                _liNextRound.Add(new KeyValuePair<CombatActor, int>(actors[i], initiative));
            }

            _liNextRound = _liNextRound.OrderByDescending(x => x.Value).ThenByDescending(x => x.Key.Attribute(AttributeEnum.Speed)).ToList();
        }

        public static void GoToNextRound()
        {
            CurrentPhase = PhaseEnum.Upkeep;

            for (int i = 0; i < _liNextRound.Count; i++)
            {
                _liCurrentRound.Add(_liNextRound[i].Key);
            }

            RollForNextRound();
        }

        public static CombatTile GetMapTile(int row, int col)
        {
            return _combatMap[row, col];
        }
        public static void AssignPositions(ref GUICombatTile[,] allyArray)
        {
            //Get the Players' party and assign each of them a battle position
            List<CombatActor> party = CombatManager.Party;
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] != null)
                {
                    Vector2 vec = party[i].StartPosition;
                    _combatMap[(int)vec.Y, (int)vec.X].SetCombatant(party[i]);
                }
            }
            //Get the Enemies and assign each of them a battle position
            for (int i = 0; i < CurrentMob.Monsters.Count; i++)
            {
                int row = -1;
                int col = -1;
                do
                {
                    RHRandom random = RHRandom.Instance();
                    row = random.Next(0, MAX_ROW - 1);
                    col = random.Next(ENEMY_FRONT, MAX_COLUMN - 1);
                } while (_combatMap[row, col].Occupied());

                _combatMap[row, col].SetCombatant(CurrentMob.Monsters[i]);
            }
        }

        public static void Update(GameTime gameTime)
        {
            foreach (CombatTile ct in _combatMap)
            {
                if (ct.Occupied())
                {
                    if (ct.TargetType == TargetEnum.Enemy && !Monsters.Contains(ct.Character))
                    {
                        ct.SetCombatant(null);
                    }
                    else
                    {
                        ct.Character.Update(gameTime);
                    }
                }
            }

            switch (CurrentPhase)
            {
                case PhaseEnum.DisplayVictory:
                    if (Delay <= 0)
                    {
                        Delay = 0.05f;
                        GiveXP();
                        CurrentMob.Defeat();
                        MapManager.CurrentMap.RemoveActor(CurrentMob);
                    }
                    else
                    {
                        Delay -= gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    break;

                case PhaseEnum.Upkeep:
                    LiteSummon activeSummon = ActiveCharacter.LinkedSummon;
                    if (activeSummon == null || !activeSummon.Regen)
                    {
                        SetPhaseForTurn();
                    }
                    else if (activeSummon != null && activeSummon.Regen && activeSummon.BodySprite.CurrentAnimation != "Cast")
                    {
                        activeSummon.PlayAnimation(CombatActionEnum.Cast);
                    }
                    else if (activeSummon.BodySprite.GetPlayCount() >= 1)
                    {
                        activeSummon.PlayAnimation(CombatActionEnum.Idle);
                        ActiveCharacter.IncreaseHealth(30);
                        ActiveCharacter.Tile.GUITile.AssignEffect(30, false);
                        SetPhaseForTurn();
                    }

                    break;

            }
        }

        private static void GiveXP()
        {
            double toGive = 0;
            double total = 0;
            CurrentMob.GetXP(ref toGive, ref total);

            int xpDrain = 5;
            if (toGive > 0)
            {
                CurrentMob.DrainXP(xpDrain);
                foreach (ClassedCombatant a in Party)
                {
                    a.AddXP(xpDrain);
                }
            }
        }
        internal static bool CanCancel()
        {
            return CurrentPhase == PhaseEnum.ChooseTarget || CurrentPhase == PhaseEnum.ChooseAction;
        }

        private static bool EndCombatCheck()
        {
            bool rv = false;

            bool monstersDown = true;
            foreach (CombatActor m in Monsters)
            {
                if (m.CurrentHP != 0)
                {
                    monstersDown = false;
                    break;
                }
            }

            if (IsPartyUp() && monstersDown)
            {
                rv = true;
                CurrentPhase = PhaseEnum.DisplayVictory;
                InventoryManager.InitMobInventory(1, 5);    //Just temp values
                foreach (ClassedCombatant a in Party)
                {
                    int levl = a.ClassLevel;
                    //a.AddXP(EarnedXP);
                    a.Tile.PlayAnimation(CombatActionEnum.Victory);

                    //if (levl != a.ClassLevel)
                    //{
                    //    LiLevels.Add(a.Name + " Level Up!");
                    //}
                }
                //MapManager.DropItemsOnMap(DropManager.DropItemsFromMob(_mob.ID), _mob.CollisionBox.Center.ToVector2());
            }
            else if (!IsPartyUp())
            {
                rv = true;
                CurrentPhase = PhaseEnum.Defeat;
            }

            return rv;
        }

        public static void EndCombatVictory()
        {
            GUIManager.BeginFadeOut();
            //MapManager.DropItemsOnMap(DropManager.DropItemsFromMob(_mob.ID), _mob.CollisionBox.Center.ToVector2());
            MapManager.RemoveActor(CurrentMob);
            CurrentMob = null;
            GoToHUDScreen();
        }

        public static void EndCombatEscape()
        {
            GUIManager.BeginFadeOut();
            CurrentMob.Stun();
            GoToHUDScreen();
        }

        public static void SetPhaseForTurn()
        {
            if (Monsters.Contains(ActiveCharacter))
            {
                CurrentPhase = PhaseEnum.EnemyTurn;
                EnemyTakeTurn();
            }
            else if (Party.Contains(ActiveCharacter)) { CurrentPhase = PhaseEnum.NewTurn; }
        }

        //For now, when the enemy takes their turn, have them select a random party member
        //When enemies get healing/defensive skills, they'll have their own logic to process
        public static void EnemyTakeTurn()
        {
            CombatAction action = null;

            RHRandom r = RHRandom.Instance();
            action = (CombatAction)ActiveCharacter.Actions[r.Next(0, ActiveCharacter.Actions.Count - 1)];

            ProcessActionChoice(action);
            if (!SelectedAction.SelfOnly())
            {
                if (action.Range == RangeEnum.Melee)
                {
                    List<CombatTile> playerTiles = new List<CombatTile>();
                    int col = FindPlayerFrontLine();

                    for (int row = 0; row < MAX_ROW; row++)
                    {
                        CombatTile tile = _combatMap[row, col];
                        if (tile.Occupied() && tile.Character.CurrentHP > 0)
                        {
                            playerTiles.Add(tile);
                        }
                    }

                    SelectedTile = playerTiles[r.Next(0, playerTiles.Count - 1)];
                }
                else
                {
                    CombatActor adv = Party[r.Next(0, Party.Count - 1)];
                    SelectedTile = adv.Tile;
                }
            }
        }

        private static void LoadList(ref List<CombatActor> toFill, List<CombatActor> takeFrom)
        {
            foreach (CombatActor c in takeFrom)
            {
                CombatActor actor = c;
                toFill.Add(actor);
            }
        }

        public static void ProcessActionChoice(CombatAction a)
        {
            SelectedAction = new ChosenAction(a);

            if (!SelectedAction.SelfOnly())
            {
                //Skips this phase for enemies. They don't "choose" targets
                if (PlayerTurn)
                {
                    CurrentPhase = PhaseEnum.ChooseTarget;
                }  
            }
            else
            {
                CurrentPhase = PhaseEnum.DisplayAttack;
                Text = SelectedAction.Name;
            }
        }

        public static void ProcessItemChoice(Consumable it)
        {
            CurrentPhase = PhaseEnum.ChooseTarget;
            SelectedAction = new ChosenAction(it);
        }

        public static void Kill(CombatActor c)
        {
            if (Monsters.Contains((c)))
            {
                Monsters.Remove(c);
                _liCurrentRound.Remove(c);                    //Remove the killed member from the turn order 
                _liNextRound.Remove(_liNextRound.Find(x => x.Key == c));
            }
        }

        public static bool IsPartyUp()
        {
            bool stillOne = false;
            foreach (CombatActor character in Party)
            {
                if (character.CurrentHP > 0) { stillOne = true; }
            }

            return stillOne;
        }

        public static void SetChosenItem(Consumable i)
        {
            SelectedAction = new ChosenAction(i);
            CurrentPhase = PhaseEnum.ChooseTarget;
        }

        public static void SetChosenAction(int value)
        {
            if(value < ActiveCharacter.Actions.Count && ActiveCharacter.Actions[value] != null){
                SelectedAction = new ChosenAction(ActiveCharacter.Actions[value]);
                CurrentPhase = PhaseEnum.ChooseTarget;
            }
        }

        public static void SetTargetTile(CombatTile tile)
        {
            TargetTile = tile;
        }

        public static CombatTile GetTileFromMap(int row, int column)
        {
            return _combatMap[row, column];
        }

        #region SelectionHandling

        /// <summary>
        /// Determines whether or not the given tile has been selected while hovering over it
        /// </summary>
        /// <param name="tile">The tile to test against</param>
        public static void TestHoverTile(CombatTile tile)
        {
            if (SelectedAction.LegalTiles.Contains(tile) && (tile.Occupied() || SelectedAction.AreaOfEffect() || (SelectedAction.Compare(ActionEnum.Move) && !tile.Occupied())))
            {
                tile.Select(true);
            }
        }
        public static void HandleKeyboardTargetting()
        {
            if (SelectedTile == null)
            {
                SelectedAction.LegalTiles[0].Select(true);

                if (!SelectedTile.Occupied())
                {
                    FindNextTarget();
                }
            }

            CombatTile temp = null;
            if (InputManager.CheckPressedKey(Keys.A))
            {
                if (SelectedAction.IsMelee()) { FindLastTarget(); }
                else { temp = GetLeft(SelectedTile); }
            }
            else if (InputManager.CheckPressedKey(Keys.D))
            {
                if (SelectedAction.IsMelee()) { FindNextTarget(); }
                else { temp = GetRight(SelectedTile); }
            }
            else if (InputManager.CheckPressedKey(Keys.W))
            {
                if (SelectedAction.IsMelee()) { FindLastTarget(); }
                else { temp = GetTop(SelectedTile); }
            }
            else if (InputManager.CheckPressedKey(Keys.S))
            {
                if (SelectedAction.IsMelee()) { FindNextTarget(); }
                else { temp = GetBottom(SelectedTile); }
            }

            if (temp != null && SelectedAction.CompareTargetType(temp.TargetType))
            {
                temp.Select(true);
            }

            if (InputManager.CheckPressedKey(Keys.Enter))
            {
                CombatManager.SelectedAction.SetSkillTarget();
            }
        }
        public static void FindNextTarget()
        {
            for (int i = 0; i < SelectedAction.LegalTiles.Count; i++)
            {
                CombatTile t = SelectedAction.LegalTiles[i];

                if (t == SelectedTile)
                {
                    while (i < SelectedAction.LegalTiles.Count - 1)
                    {
                        t = SelectedAction.LegalTiles[i + 1];
                        if (t.Occupied())
                        {
                            t.Select(true);
                            goto Exit;
                        }
                        i++;
                    }

                    break;
                }
            }
            Exit:

            return;
        }
        public static void FindLastTarget()
        {
            for (int i = 0; i < SelectedAction.LegalTiles.Count; i++)
            {
                CombatTile t = SelectedAction.LegalTiles[i];
                if (t == SelectedTile)
                {
                    while (i > 0)
                    {
                        t = SelectedAction.LegalTiles[i - 1];
                        if (t.Occupied())
                        {
                            t.Select(true);
                            goto Exit;
                        }
                        i--;
                    }

                    break;
                }
            }
            Exit:

            return;
        }

        public static List<CombatTile> GetAdjacent(CombatTile t)
        {
            List<CombatTile> adj = new List<CombatTile>();

            //Have to null check
            CombatTile temp = GetTop(t);
            if (temp != null) { adj.Add(temp); }
            temp = GetBottom(t);
            if (temp != null) { adj.Add(temp); }
            temp = GetLeft(t);
            if (temp != null) { adj.Add(temp); }
            temp = GetRight(t);
            if (temp != null) { adj.Add(temp); }

            return adj;
        }
        public static CombatTile GetTop(CombatTile t)
        {
            CombatTile rv = null;
            if (t.Row > 0)
            {
                rv = _combatMap[t.Row - 1, t.Column];
            }

            return rv;
        }

        public static CombatTile GetBottom(CombatTile t)
        {
            CombatTile rv = null;
            if (t.Row < MAX_ROW - 1)
            {
                rv = _combatMap[t.Row + 1, t.Column];
            }

            return rv;
        }
        /// <summary>
        /// Retrieves the Tile to the Left of the indicated tile as long as it does
        /// not pass over the divide between player and enemy
        /// </summary>
        public static CombatTile GetLeft(CombatTile t)
        {
            CombatTile rv = null;
            if (t.Column > 0 && (t.Column < ENEMY_FRONT || t.Column - 1 >= ENEMY_FRONT))
            {
                rv = _combatMap[t.Row, t.Column - 1];
            }

            return rv;
        }

        /// <summary>
        /// Retrieves the Tile to the Right of the indicated tile as long as it does
        /// not pass over the divide between player and enemy
        /// </summary>
        public static CombatTile GetRight(CombatTile t)
        {
            CombatTile rv = null;
            if (t.Column < MAX_COLUMN - 1 && (t.Column >= ENEMY_FRONT || t.Column + 1 < ENEMY_FRONT))
            {
                rv = _combatMap[t.Row, t.Column + 1];
            }

            return rv;
        }

        public static void ClearSelectedTile()
        {
            if (SelectedTile != null) { SelectedTile.Select(false); }
            SelectedTile = null;
        }

        public static int FindEnemyFrontLine()
        {
            int rv = 0;

            //Go down each column, looking for a target
            for (int col = ENEMY_FRONT; col < MAX_COLUMN; col++)
            {
                for (int row = 0; row < MAX_ROW; row++)
                {
                    if (_combatMap[row, col].Occupied())
                    {
                        rv = col;
                        goto ExitFrontLine;
                    }
                }
            }
            ExitFrontLine:

            return rv;
        }
        public static int FindPlayerFrontLine()
        {
            int rv = 0;

            //Go down each column, looking for a target
            for (int col = ALLY_FRONT; col >= 0; col--)
            {
                for (int row = 0; row < MAX_ROW; row++)
                {
                    if (_combatMap[row, col].Occupied() && !_combatMap[row, col].Character.KnockedOut)
                    {
                        rv = col;
                        goto ExitFrontLine;
                    }
                }
            }
            ExitFrontLine:

            return rv;
        }

        #endregion

        #region Turn Handling
        public static void EndTurn()
        {
            LiteSummon activeSummon = ActiveCharacter.LinkedSummon;
            //If there is no linked summon, or it is a summon, end the turn normally.

            if (!EndCombatCheck())
            {
                TurnOver();

                //if (activeSummon != null)
                //{
                //    if (activeSummon.Aggressive && SelectedAction.IsMelee())
                //    {
                //        List<LiteCombatTile> targets = SelectedAction.GetTargetTiles();
                //        ActiveCharacter = activeSummon;
                //        SelectedAction = new ChosenAction((LiteCombatAction)DataManager.GetLiteActionByIndex(CombatManager.BASIC_ATTACK));
                //        SelectedAction.SetUser(ActiveCharacter);
                //        SelectedAction.SetTargetTiles(targets);
                //    }
                //    else if (activeSummon.TwinCast && SelectedAction.Compare(ActionEnum.Spell) && !SelectedAction.IsSummonSpell() && SelectedAction.CanTwinCast())
                //    {
                //        ActiveCharacter = activeSummon;
                //        SelectedAction.SetUser(activeSummon);
                //    }
                //    else
                //    {
                //        TurnOver();
                //    }
                //}
                //else
                //{
                //    TurnOver();
                //}
            }
        }

        private static void TurnOver()
        {
            SelectedAction.Clear();

            if (CurrentPhase != PhaseEnum.EndCombat)
            {
                SelectedAction = null;
                _liCurrentRound.RemoveAt(0);
                
                if(_liCurrentRound.Count == 0)
                {
                    GoToNextRound();
                }
            }

            SetPhaseForTurn();
        }
        #endregion

        public static bool PhaseSelectSkill() { return CurrentPhase == PhaseEnum.ChooseAction; }
        public static bool PhaseChooseTarget() { return CurrentPhase == PhaseEnum.ChooseTarget && !SelectedAction.TargetsEach(); }
    }
}
