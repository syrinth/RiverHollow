using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters;
using RiverHollow.Characters.Lite;
using RiverHollow.CombatStuff;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;

namespace RiverHollow.Game_Managers
{
    public static class LiteCombatManager
    {
        public const int BASIC_ATTACK = 300;
        public static int CombatScale = 5;
        private static LiteMob _mob;
        public static LiteMob CurrentMob { get => _mob; }
        public static LiteCombatActor ActiveCharacter;
        private static List<LiteCombatActor> _liMonsters;
        public static List<LiteCombatActor> Monsters { get => _liMonsters; }
        private static List<LiteCombatActor> _listParty;
        public static List<LiteCombatActor> Party { get => _listParty; }

        public enum PhaseEnum { Charging, Upkeep, NewTurn, EnemyTurn, SelectSkill, ChooseTarget, Defeat, DisplayAttack, DisplayVictory, Lost, PerformAction, EndCombat }
        public static PhaseEnum CurrentPhase;
        public static ChosenAction SelectedAction;
        private static LiteCombatTile _targetTile;

        public static double Delay;
        public static string Text;

        #region Turn Sequence
        static List<LiteCombatActor> _liQueuedCharacters;
        static List<LiteCombatActor> _liChargingCharacters;
        #endregion

        #region CombatGrid
        public static LiteCombatTile SelectedTile;

        public static readonly int MAX_COL = 8;
        public static readonly int MAX_ROW = 3;
        public static readonly int ALLY_FRONT = 3;
        public static readonly int ENEMY_FRONT = 4;

        static LiteCombatTile[,] _combatMap;
        #endregion

        public static void NewBattle(LiteMob m)
        {
            ActiveCharacter = null;
            SelectedAction = null;
            SelectedTile = null;

            CurrentPhase = PhaseEnum.Charging;
            _combatMap = new LiteCombatTile[MAX_ROW, MAX_COL];
            for (int row = 0; row < MAX_ROW; row++)
            {
                for (int col = 0; col < MAX_COL; col++)
                {
                    _combatMap[row, col] = new LiteCombatTile(row, col, col < ENEMY_FRONT ? TargetEnum.Ally : TargetEnum.Enemy);
                }
            }

            Delay = 0;
            _mob = m;
            _liMonsters = _mob.Monsters;

            _listParty = new List<LiteCombatActor>();
            _listParty.AddRange(PlayerManager.GetLiteParty());

            _liQueuedCharacters = new List<LiteCombatActor>();
            _liChargingCharacters = new List<LiteCombatActor>();
            _liChargingCharacters.AddRange(_listParty);
            _liChargingCharacters.AddRange(_liMonsters);

            //Characters with higher Spd go first
            _liChargingCharacters.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));

            foreach (LiteCombatActor c in _liChargingCharacters)
            {
                c.CurrentCharge = RHRandom.Instance().Next(0, 50);
            }

            GoToCombatScreen();
            PlayerManager.DecreaseStamina(3);       //Decrease Stamina once
        }

        public static LiteCombatTile GetMapTile(int row, int col)
        {
            return _combatMap[row, col];
        }
        public static void AssignPositions(ref GUICombatTile[,] allyArray)
        {
            //Get the Players' party and assign each of them a battle position
            List<LiteCombatActor> party = LiteCombatManager.Party;
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
                    col = random.Next(ENEMY_FRONT, MAX_COL - 1);
                } while (_combatMap[row, col].Occupied());

                _combatMap[row, col].SetCombatant(CurrentMob.Monsters[i]);
            }
        }

        public static void Update(GameTime gameTime)
        {
            if (ActiveCharacter == null)
            {
                if (_liQueuedCharacters.Count == 0)
                {
                    CombatTick(ref _liChargingCharacters, ref _liQueuedCharacters);
                }
                else
                {
                    GetActiveCharacter();
                }
            }

            foreach (LiteCombatTile ct in _combatMap)
            {
                if (ct.Occupied())
                {
                    if (ct.TargetType == TargetEnum.Enemy && !_liMonsters.Contains(ct.Character))
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
                        activeSummon.PlayAnimation(LiteCombatActionEnum.Cast);
                    }
                    else if (activeSummon.BodySprite.GetPlayCount() >= 1)
                    {
                        activeSummon.PlayAnimation(LiteCombatActionEnum.Idle);
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
                foreach (LitePartyMember a in _listParty)
                {
                    a.AddXP(xpDrain);
                }
            }
        }
        internal static bool CanCancel()
        {
            return CurrentPhase == PhaseEnum.ChooseTarget || CurrentPhase == PhaseEnum.SelectSkill;
        }

        private static bool EndCombatCheck()
        {
            bool rv = false;

            bool monstersDown = true;
            foreach (LiteCombatActor m in _liMonsters)
            {
                if (m.CurrentHP != 0)
                {
                    monstersDown = false;
                    break;
                }
            }

            if (PartyUp() && monstersDown)
            {
                rv = true;
                CurrentPhase = PhaseEnum.DisplayVictory;
                InventoryManager.InitMobInventory(1, 5);    //Just temp values
                foreach (LitePartyMember a in _listParty)
                {
                    int levl = a.ClassLevel;
                    a.CurrentCharge = 0;
                    //a.AddXP(EarnedXP);
                    a.Tile.PlayAnimation(LiteCombatActionEnum.Victory);

                    //if (levl != a.ClassLevel)
                    //{
                    //    LiLevels.Add(a.Name + " Level Up!");
                    //}
                }
                //MapManager.DropItemsOnMap(DropManager.DropItemsFromMob(_mob.ID), _mob.CollisionBox.Center.ToVector2());
            }
            else if (!PartyUp())
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
            MapManager.RemoveActor(_mob);
            _mob = null;
            GoToHUDScreen();
        }

        public static void EndCombatEscape()
        {
            GUIManager.BeginFadeOut();
            _mob.Stun();
            GoToHUDScreen();
        }

        public static void SetPhaseForTurn()
        {
            if (_liMonsters.Contains(ActiveCharacter))
            {
                CurrentPhase = PhaseEnum.EnemyTurn;
                EnemyTakeTurn();
            }
            else if (_listParty.Contains(ActiveCharacter)) { CurrentPhase = PhaseEnum.NewTurn; }
        }

        //For now, when the enemy takes their turn, have them select a random party member
        //When enemies get healing/defensive skills, they'll have their own logic to process
        public static void EnemyTakeTurn()
        {
            LiteCombatAction action = null;
            bool canCast = false;

            RHRandom r = RHRandom.Instance();
            do
            {
                action = (LiteCombatAction)ActiveCharacter.AbilityList[r.Next(0, ActiveCharacter.AbilityList.Count - 1)];
                if (action.MPCost <= ActiveCharacter.CurrentMP)
                {
                    canCast = true;
                }
            } while (!canCast);

            ProcessActionChoice(action);
            if (!SelectedAction.SelfOnly())
            {
                if (action.Range == RangeEnum.Melee)
                {
                    List<LiteCombatTile> playerTiles = new List<LiteCombatTile>();
                    int col = FindPlayerFrontLine();

                    for (int row = 0; row < MAX_ROW; row++)
                    {
                        LiteCombatTile tile = _combatMap[row, col];
                        if (tile.Occupied() && tile.Character.CurrentHP > 0)
                        {
                            playerTiles.Add(tile);
                        }
                    }

                    SelectedTile = playerTiles[r.Next(0, playerTiles.Count - 1)];
                }
                else
                {
                    LiteCombatActor adv = _listParty[r.Next(0, _listParty.Count - 1)];
                    SelectedTile = adv.Tile;
                }
            }

            ActiveCharacter.CurrentMP -= action.MPCost;
        }

        internal static List<LiteCombatActor> CalculateTurnOrder(int maxShown)
        {
            List<LiteCombatActor> rv = new List<LiteCombatActor>();
            List<LiteCombatActor> queuedCopy = new List<LiteCombatActor>();
            List<LiteCombatActor> chargingCopy = new List<LiteCombatActor>();

            LoadList(ref queuedCopy, _liQueuedCharacters);
            LoadList(ref chargingCopy, _liChargingCharacters);

            //If there is an Active Character, Add them to the Turn Order list and blank their DummyCharge
            if (ActiveCharacter != null)
            {
                rv.Add(ActiveCharacter);
                LiteCombatActor c = chargingCopy.Find(x => x.Name == ActiveCharacter.Name);
                c.DummyCharge -= (SelectedAction == null) ? c.DummyCharge : SelectedAction.ChargeCost();
            }

            //If there are any queued Actors, add them to the charging list
            foreach (LiteCombatActor c in queuedCopy)
            {
                if (rv.Count < maxShown)
                {
                    rv.Add(c);
                    c.DummyCharge = 0;
                    chargingCopy.Add(c);
                }
                else { break; }
            }
            queuedCopy.Clear();                                                 //Clear the queue

            //Cap out entries at maxShown
            while (rv.Count < maxShown)
            {
                chargingCopy.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));        //Sort the charging Actors by their speed
                CombatTick(ref chargingCopy, ref queuedCopy, true);                       //Tick

                //For all entries in the queue add them to the TurnOrder List,
                //set the Charge to 0, and add to the queue, sorting by Spd
                foreach (LiteCombatActor c in queuedCopy)
                {
                    if (rv.Count < maxShown)
                    {
                        rv.Add(c);
                        c.DummyCharge = 0;
                        chargingCopy.Add(c);
                    }
                    else { break; }
                }
                queuedCopy.Clear();
            }

            return rv;
        }

        private static void LoadList(ref List<LiteCombatActor> toFill, List<LiteCombatActor> takeFrom)
        {
            foreach (LiteCombatActor c in takeFrom)
            {
                LiteCombatActor actor = c;
                actor.DummyCharge = c.CurrentCharge;
                toFill.Add(actor);
            }
        }

        public static void ProcessActionChoice(LiteCombatAction a)
        {
            SelectedAction = new ChosenAction(a);

            if (!SelectedAction.SelfOnly())
            {
                if (!ActiveCharacter.IsActorType(ActorEnum.Monster))
                {
                    CurrentPhase = PhaseEnum.ChooseTarget;
                }  //Skips this phase for enemies. They don't "choose" targets
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

        public static void Kill(LiteCombatActor c)
        {
            if (_liMonsters.Contains((c)))
            {
                _liMonsters.Remove(c);
                _liChargingCharacters.Remove(c);                    //Remove the killed member from the turn order 
                _liQueuedCharacters.Remove(c);
            }
        }

        public static bool PartyUp()
        {
            bool stillOne = false;
            foreach (LiteCombatActor character in _listParty)
            {
                if (character.CurrentHP > 0) { stillOne = true; }
            }

            return stillOne;
        }

        #region SelectionHandling

        /// <summary>
        /// Determines whether or not the given tile has been selected while hovering over it
        /// </summary>
        /// <param name="tile">The tile to test against</param>
        public static void TestHoverTile(LiteCombatTile tile)
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

            LiteCombatTile temp = null;
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
                LiteCombatManager.SelectedAction.SetSkillTarget();
            }
        }
        public static void FindNextTarget()
        {
            for (int i = 0; i < SelectedAction.LegalTiles.Count; i++)
            {
                LiteCombatTile t = SelectedAction.LegalTiles[i];

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
                LiteCombatTile t = SelectedAction.LegalTiles[i];
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

        public static List<LiteCombatTile> GetAdjacent(LiteCombatTile t)
        {
            List<LiteCombatTile> adj = new List<LiteCombatTile>();

            //Have to null check
            LiteCombatTile temp = GetTop(t);
            if (temp != null) { adj.Add(temp); }
            temp = GetBottom(t);
            if (temp != null) { adj.Add(temp); }
            temp = GetLeft(t);
            if (temp != null) { adj.Add(temp); }
            temp = GetRight(t);
            if (temp != null) { adj.Add(temp); }

            return adj;
        }
        public static LiteCombatTile GetTop(LiteCombatTile t)
        {
            LiteCombatTile rv = null;
            if (t.Row > 0)
            {
                rv = _combatMap[t.Row - 1, t.Col];
            }

            return rv;
        }

        public static LiteCombatTile GetBottom(LiteCombatTile t)
        {
            LiteCombatTile rv = null;
            if (t.Row < MAX_ROW - 1)
            {
                rv = _combatMap[t.Row + 1, t.Col];
            }

            return rv;
        }
        /// <summary>
        /// Retrieves the Tile to the Left of the indicated tile as long as it does
        /// not pass over the divide between player and enemy
        /// </summary>
        public static LiteCombatTile GetLeft(LiteCombatTile t)
        {
            LiteCombatTile rv = null;
            if (t.Col > 0 && (t.Col < ENEMY_FRONT || t.Col - 1 >= ENEMY_FRONT))
            {
                rv = _combatMap[t.Row, t.Col - 1];
            }

            return rv;
        }

        /// <summary>
        /// Retrieves the Tile to the Right of the indicated tile as long as it does
        /// not pass over the divide between player and enemy
        /// </summary>
        public static LiteCombatTile GetRight(LiteCombatTile t)
        {
            LiteCombatTile rv = null;
            if (t.Col < MAX_COL - 1 && (t.Col >= ENEMY_FRONT || t.Col + 1 < ENEMY_FRONT))
            {
                rv = _combatMap[t.Row, t.Col + 1];
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
            for (int col = ENEMY_FRONT; col < MAX_COL; col++)
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
                    if (_combatMap[row, col].Occupied() && !_combatMap[row, col].Character.DiConditions[ConditionEnum.KO])
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
        private static void CombatTick(ref List<LiteCombatActor> charging, ref List<LiteCombatActor> queued, bool dummy = false)
        {
            List<LiteCombatActor> toQueue = new List<LiteCombatActor>();
            foreach (LiteCombatActor c in charging)
            {
                //If Actor is not knocked out, increment the charge, capping to 100
                if (!c.KnockedOut() || c.CurrentHP > 0)
                {
                    if (dummy) { HandleChargeTick(ref c.DummyCharge, ref toQueue, c); }
                    else { HandleChargeTick(ref c.CurrentCharge, ref toQueue, c); }
                }
            }

            foreach (LiteCombatActor c in toQueue)
            {
                queued.Add(c);
                charging.Remove(c);
            }
        }
        private static void HandleChargeTick(ref int charge, ref List<LiteCombatActor> toQueue, LiteCombatActor c)
        {
            charge += c.StatSpd;
            if (charge >= 100)
            {
                charge = 100;
                toQueue.Add(c);
            }
        }
        private static void GetActiveCharacter()
        {
            if (_liQueuedCharacters[0].CurrentHP > 0)
            {
                ActiveCharacter = _liQueuedCharacters[0];
                _liQueuedCharacters.RemoveAt(0);
                _liChargingCharacters.Add(ActiveCharacter);
                _liChargingCharacters.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));

                ActiveCharacter.TickStatusEffects();
                if (ActiveCharacter.Poisoned())
                {
                    ActiveCharacter.Location.AssignEffect(ActiveCharacter.DecreaseHealth(Math.Max(1, (int)(ActiveCharacter.MaxHP / 20))), true);
                }

                CurrentPhase = PhaseEnum.Upkeep;    //We have a charcter, but go into Upkeep phase first
            }
        }
        public static void EndTurn()
        {
            ActiveCharacter.CurrentCharge -= SelectedAction.ChargeCost();

            LiteSummon activeSummon = ActiveCharacter.LinkedSummon;
            //If there is no linked summon, or it is a summon, end the turn normally.

            if (!EndCombatCheck())
            {
                if (activeSummon != null)
                {
                    if (activeSummon.Aggressive && SelectedAction.IsMelee())
                    {
                        List<LiteCombatTile> targets = SelectedAction.GetTargetTiles();
                        ActiveCharacter = activeSummon;
                        SelectedAction = new ChosenAction((LiteCombatAction)DataManager.GetLiteActionByIndex(LiteCombatManager.BASIC_ATTACK));
                        SelectedAction.SetUser(ActiveCharacter);
                        SelectedAction.SetTargetTiles(targets);
                    }
                    else if (activeSummon.TwinCast && SelectedAction.Compare(ActionEnum.Spell) && !SelectedAction.IsSummonSpell() && SelectedAction.CanTwinCast())
                    {
                        ActiveCharacter = activeSummon;
                        SelectedAction.SetUser(activeSummon);
                    }
                    else
                    {
                        TurnOver();
                    }
                }
                else
                {
                    TurnOver();
                }
            }
        }

        private static void TurnOver()
        {
            SelectedAction.Clear();

            if (CurrentPhase != PhaseEnum.EndCombat)
            {
                SelectedAction = null;
                ActiveCharacter = null;
            }
        }
        #endregion
        public static bool PhaseSelectSkill() { return CurrentPhase == PhaseEnum.SelectSkill; }
        public static bool PhaseChooseTarget() { return CurrentPhase == PhaseEnum.ChooseTarget && !SelectedAction.TargetsEach(); }

        public class ChosenAction
        {
            private Consumable _chosenItem;
            private LiteCombatAction _chosenAction;

            List<LiteCombatTile> _liLegalTiles;
            public List<LiteCombatTile> LegalTiles => _liLegalTiles;
            public LiteCombatActor User;

            string _name;
            public string Name => _name;

            bool _bDrawItem;

            public ChosenAction(Consumable it)
            {
                User = ActiveCharacter;
                _chosenItem = it;
                _name = _chosenItem.Name;
                _liLegalTiles = new List<LiteCombatTile>();

                //Only the adjacent tiles are legal
                if (TargetsAlly())
                {
                    _liLegalTiles.Add(User.Tile);

                    List<LiteCombatTile> adj = GetAdjacent(User.Tile);
                    foreach (LiteCombatTile t in adj)
                    {
                        if (t.TargetType == TargetEnum.Ally) { _liLegalTiles.Add(t); }
                    }
                }
                else if (TargetsEnemy())
                {
                    EnemyFrontLineLegal();
                }
            }
            public ChosenAction(LiteCombatAction ca)
            {
                User = ActiveCharacter;
                _chosenAction = ca;
                _name = _chosenAction.Name;

                _chosenAction.SkillUser = ActiveCharacter;

                _liLegalTiles = new List<LiteCombatTile>();
                if (IsMelee())
                {
                    EnemyFrontLineLegal();
                }
                else if (IsRanged())
                {
                    int col = -1;
                    int maxCol = MAX_COL;
                    if (TargetsEnemy()) { col = ENEMY_FRONT; }
                    else
                    {
                        col = 0;
                        maxCol = ENEMY_FRONT;
                    }

                    for (; col < maxCol; col++)
                    {
                        for (int row = 0; row < MAX_ROW; row++)
                        {
                            _liLegalTiles.Add(_combatMap[row, col]);
                        }
                    }
                }
                else if (SelfOnly())
                {
                    _liLegalTiles.Add(User.Tile);
                }
                else if (Columns())
                {
                    int startCol = ActiveCharacter.Tile.Col;
                    int endCol = ActiveCharacter.Tile.Col;

                    if (ActiveCharacter.Tile.Col > 0) { startCol = ActiveCharacter.Tile.Col - 1; }
                    if (ActiveCharacter.Tile.Col < ALLY_FRONT) { endCol = ActiveCharacter.Tile.Col + 1; }

                    for (int row = 0; row < MAX_ROW; row++)
                    {
                        for (int col = startCol; col <= endCol; col++)
                        {
                            if (!_combatMap[row, col].Occupied())
                            {
                                _liLegalTiles.Add(_combatMap[row, col]);
                            }
                        }
                    }
                }
                else if (Adjacent())
                {
                    int col = ActiveCharacter.Tile.Col;
                    int row = ActiveCharacter.Tile.Row;

                    if (row - 1 >= 0) { _liLegalTiles.Add(_combatMap[row - 1, col]); }
                    if (row + 1 < MAX_ROW) { _liLegalTiles.Add(_combatMap[row + 1, col]); }

                    if (col - 1 >= 0) { _liLegalTiles.Add(_combatMap[row, col - 1]); }
                    if (col + 1 < ALLY_FRONT) { _liLegalTiles.Add(_combatMap[row, col + 1]); }
                }
            }

            private void EnemyFrontLineLegal()
            {
                int col = FindEnemyFrontLine();
                for (int row = 0; row < MAX_ROW; row++)
                {
                    _liLegalTiles.Add(_combatMap[row, col]);
                }
            }

            public void Draw(SpriteBatch spritebatch)
            {
                if (_bDrawItem && _chosenItem != null)     //We want to draw the item above the character's head
                {
                    int size = TILE_SIZE * LiteCombatManager.CombatScale;
                    GUIImage gItem = new GUIImage(_chosenItem.SourceRectangle, size, size, _chosenItem.Texture);
                    LiteCombatActor c = LiteCombatManager.ActiveCharacter;

                    gItem.AnchorAndAlignToObject(c.GetSprite(), SideEnum.Top, SideEnum.CenterX);
                    gItem.Draw(spritebatch);
                }
                if (_chosenAction != null && _chosenAction.Sprite != null)
                {
                    _chosenAction.Sprite.Draw(spritebatch);
                }
            }

            public void PerformAction(GameTime gameTime)
            {
                if (_chosenAction != null) { _chosenAction.HandlePhase(gameTime); }
                else if (_chosenItem != null)
                {
                    bool finished = false;
                    LiteCombatActor c = LiteCombatManager.ActiveCharacter;
                    if (!c.IsCurrentAnimation(LiteCombatActionEnum.Cast))
                    {
                        c.Tile.PlayAnimation(LiteCombatActionEnum.Cast);
                        _bDrawItem = true;
                    }
                    else if (c.AnimationPlayedXTimes(3))
                    {
                        c.Tile.PlayAnimation(LiteCombatActionEnum.Idle);
                        _bDrawItem = false;
                        finished = true;
                    }

                    if (finished) { UseItem(); }
                }
            }

            public void UseItem()
            {
                if (_chosenItem != null)
                {
                    if (_chosenItem.Condition != ConditionEnum.None)
                    {
                        _targetTile.Character.ChangeConditionStatus(_chosenItem.Condition, !_chosenItem.Helpful);
                    }
                    int val = _targetTile.Character.IncreaseHealth(_chosenItem.Health);
                    if (val > 0)
                    {
                        _targetTile.GUITile.AssignEffect(val, false);
                    }
                    _chosenItem.Remove(1);
                }

                EndTurn();
            }

            public void SetSkillTarget()
            {
                _targetTile = SelectedTile;
                if (_chosenAction != null)
                {
                    LiteCombatManager.ActiveCharacter.CurrentMP -= _chosenAction.MPCost;          //Checked before Processing
                    _chosenAction.AnimationSetup();
                    LiteCombatManager.Text = SelectedAction.Name;
                }
                else if (_chosenItem != null)
                {
                    LiteCombatManager.Text = SelectedAction.Name;
                }
                LiteCombatManager.CurrentPhase = PhaseEnum.DisplayAttack;
                LiteCombatManager.ClearSelectedTile();
            }

            public int ChargeCost()
            {
                int rv = 100;

                if (_chosenAction != null) { rv = _chosenAction.ChargeCost; }

                return rv;
            }

            /// <summary>
            /// Retrieves the tiles that will be effected by this skill based off the area type
            /// </summary>
            /// <returns>A complete list of tiles that will be hit</returns>
            public List<LiteCombatTile> GetEffectedTiles()
            {
                List<LiteCombatTile> cbtTile = new List<LiteCombatTile>();
                if (_chosenItem != null)
                {
                    cbtTile.Add(SelectedTile);
                }
                else
                {
                    LiteCombatActor actor = _chosenAction.SkillUser;
                    if (SelectedTile != null)
                    {
                        cbtTile.Add(SelectedTile);
                        if (_chosenAction.AreaType != AreaTypeEnum.Single)
                        {
                            //Describes which side of the Battlefield we are targetting
                            bool monsterSide = (actor.IsActorType(ActorEnum.PartyMember) && TargetsEnemy()) || (actor.IsActorType(ActorEnum.Monster) && TargetsAlly());
                            bool partySide = (actor.IsActorType(ActorEnum.Monster) && TargetsEnemy()) || (actor.IsActorType(ActorEnum.PartyMember) && TargetsAlly());

                            //All we need to do here is select all of the tiles containing the appropriate characters
                            if (_chosenAction.AreaType == AreaTypeEnum.All)
                            {
                                if (monsterSide)
                                {
                                    foreach (LiteMonster m in _liMonsters)
                                    {
                                        if (!cbtTile.Contains(m.Tile)) { cbtTile.Add(m.Tile); }
                                    }
                                }
                                else
                                {
                                    foreach (LiteCombatActor adv in _listParty)
                                    {
                                        if (!cbtTile.Contains(adv.Tile)) { cbtTile.Add(adv.Tile); }
                                    }
                                }
                            }
                            else
                            {
                                //The coordinates of the selected tile
                                int targetRow = SelectedTile.Row;
                                int targetCol = SelectedTile.Col;

                                //Determines how far to the side the skill can go, based on whether it grows left or right
                                int minCol = monsterSide ? ENEMY_FRONT : 0;
                                int maxCol = monsterSide ? MAX_COL : ENEMY_FRONT;
                                if (_chosenAction.AreaType == AreaTypeEnum.Cross)
                                {
                                    if (targetRow - 1 >= 0) { cbtTile.Add(_combatMap[targetRow - 1, targetCol]); }
                                    if (targetRow + 1 < MAX_ROW) { cbtTile.Add(_combatMap[targetRow + 1, targetCol]); }

                                    if (targetCol - 1 >= minCol) { cbtTile.Add(_combatMap[targetRow, targetCol - 1]); }
                                    if (targetCol + 1 < maxCol) { cbtTile.Add(_combatMap[targetRow, targetCol + 1]); }
                                }
                                else if (_chosenAction.AreaType == AreaTypeEnum.Rectangle)
                                {
                                    RHSize dimensions = _chosenAction.Dimensions;
                                    if (monsterSide)
                                    {
                                        for (int rows = targetRow; rows < MAX_ROW && rows < targetRow + dimensions.Width; rows++)
                                        {
                                            for (int cols = targetCol; cols < maxCol && cols < targetCol + dimensions.Height; cols++)
                                            {
                                                LiteCombatTile t = _combatMap[rows, cols];
                                                if (!cbtTile.Contains(t))
                                                {
                                                    cbtTile.Add(t);
                                                }
                                            }
                                        }
                                    }
                                    else if (partySide)
                                    {
                                        for (int rows = targetRow; rows < MAX_ROW && rows < targetRow + dimensions.Width; rows++)
                                        {
                                            for (int cols = targetCol; cols >= minCol && cols > targetCol - dimensions.Height; cols--)
                                            {
                                                LiteCombatTile t = _combatMap[rows, cols];
                                                if (!cbtTile.Contains(t))
                                                {
                                                    cbtTile.Add(t);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return cbtTile;
            }

            public void SetUser(LiteCombatActor c)
            {
                if (_chosenAction != null)
                {
                    _chosenAction.SkillUser = c;
                }
            }

            public bool CompareTargetType(TargetEnum t) { return t == _chosenAction.Target; }
            public bool TargetsAlly()
            {
                bool rv = false;

                if (_chosenAction != null) { rv = _chosenAction.IsHelpful(); }
                else if (_chosenItem != null) { rv = _chosenItem.Helpful; }

                return rv;
            }
            public bool TargetsEnemy()
            {
                bool rv = false;

                if (_chosenAction != null) { rv = !_chosenAction.IsHelpful(); }
                else if (_chosenItem != null) { rv = !_chosenItem.Helpful; }

                return rv;
            }
            public bool Compare(ActionEnum e) { return _chosenAction != null && _chosenAction.Compare(e); }
            public bool IsSummonSpell() { return _chosenAction != null && _chosenAction.IsSummonSpell(); }
            public bool SelfOnly() { return _chosenAction.Range == RangeEnum.Self; }
            public bool IsMelee() { return _chosenAction.Range == RangeEnum.Melee; }
            public bool IsRanged() { return _chosenAction.Range == RangeEnum.Ranged; }
            public bool SingleTarget()
            {
                bool rv = false;
                if (_chosenAction != null) { rv = _chosenAction.AreaType == AreaTypeEnum.Single; }
                else if (_chosenItem != null) { rv = true; }

                return rv;
            }
            public bool CanTwinCast()
            {
                bool rv = false;
                if (_chosenAction != null)
                {
                    rv = _chosenAction.Potency > 0;
                }
                return rv;
            }

            /// <summary>
            /// Used to determine whether or not the skill is used over an
            /// area or needs to have a specifiedsingle target.
            /// </summary>
            /// <returns>True if can be spread over an area</returns>
            public bool TargetsEach()
            {
                bool rv = false;

                if (_chosenAction != null) { rv = _chosenAction.TargetsEach(); }

                return rv;
            }
            public bool AreaOfEffect()
            {
                bool rv = false;

                if (_chosenAction != null) { rv = _chosenAction.AreaType != AreaTypeEnum.Single; }
                else if (_chosenItem != null) { rv = false; }

                return rv;
            }
            public bool Columns()
            {
                bool rv = false;
                if (_chosenAction != null) { rv = _chosenAction.Range == RangeEnum.Column; }
                else if (_chosenItem != null) { rv = false; }

                return rv;
            }
            public bool Adjacent()
            {
                bool rv = false;
                if (_chosenAction != null) { rv = _chosenAction.Range == RangeEnum.Adjacent; }
                else if (_chosenItem != null) { rv = false; }

                return rv;
            }

            public void Clear()
            {
                if (_chosenAction != null) { _chosenAction.TileTargetList.Clear(); }
                else if (_chosenItem != null) { _targetTile = null; }
            }
            public List<LiteCombatTile> GetTargetTiles()
            {
                if (_chosenAction != null)
                {
                    return _chosenAction.TileTargetList;
                }
                else
                {
                    return null;
                }
            }
            public void SetTargetTiles(List<LiteCombatTile> li)
            {
                if (_chosenAction != null)
                {
                    _chosenAction.AnimationSetup();
                    _chosenAction.TileTargetList = li;
                }
            }
        }
    }
}
