using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Actors;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.Game_Managers
{
    public static class CombatManager
    {
        public static int CombatScale = 5;
        private static Mob _mob;
        public static Mob CurrentMob { get => _mob; }
        public static CombatActor ActiveCharacter;
        private static List<CombatActor> _liMonsters;
        public static List<CombatActor> Monsters { get => _liMonsters; }
        private static List<CombatActor> _listParty;
        public static List<CombatActor> Party { get => _listParty; }
        
        public enum PhaseEnum { Charging, Upkeep, NewTurn, EnemyTurn, SelectSkill, ChooseTarget, Defeat, DisplayAttack, DisplayVictory, Lost, PerformAction, EndCombat }
        public static PhaseEnum CurrentPhase;
        public static ChosenAction SelectedAction;
        private static CombatTile _targetTile;

        public static double Delay;
        public static string Text;

        #region Turn Sequence
        static List<CombatActor> _liQueuedCharacters;
        static List<CombatActor> _liChargingCharacters;
        #endregion

        #region CombatGrid
        public static CombatTile SelectedTile;

        public static readonly int MAX_COL = 8;
        public static readonly int MAX_ROW = 3;
        public static readonly int ALLY_FRONT = 3;
        public static readonly int ENEMY_FRONT = 4;

        static CombatTile[,] _combatMap;
        #endregion

        public static void NewBattle(Mob m)
        {
            ActiveCharacter = null;
            SelectedAction = null;
            SelectedTile = null;

            CurrentPhase = PhaseEnum.Charging;
            _combatMap = new CombatTile[MAX_ROW, MAX_COL];
            for(int row = 0; row < MAX_ROW; row++)
            {
                for (int col = 0; col < MAX_COL; col++)
                {
                    _combatMap[row, col] = new CombatTile(row, col, col < ENEMY_FRONT ? TargetEnum.Ally : TargetEnum.Enemy);
                }
            }

            Delay = 0;
            _mob = m;
            _liMonsters = _mob.Monsters;

            _listParty = new List<CombatActor>();
            _listParty.AddRange(PlayerManager.GetParty());

            _liQueuedCharacters = new List<CombatActor>();
            _liChargingCharacters = new List<CombatActor>();
            _liChargingCharacters.AddRange(_listParty);
            _liChargingCharacters.AddRange(_liMonsters);

            //Characters with higher Spd go first
            _liChargingCharacters.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));

            RHRandom random = new RHRandom();
            foreach(CombatActor c in _liChargingCharacters)
            {
                c.CurrentCharge = random.Next(0, 50);
            }

            GoToCombat();
            PlayerManager.DecreaseStamina(3);       //Decrease Stamina once
        }

        public static CombatTile GetMapTile(int row, int col)
        {
            return _combatMap[row, col];
        }
        public static void AssignPositions(ref GUICmbtTile[,] allyArray)
        {
            //Get the Players' party and assign each of them a battle position
            List<CombatActor> party = CombatManager.Party;
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] != null)
                {
                    Vector2 vec = party[i].StartPos;
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
                    RHRandom random = new RHRandom();
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

            foreach(CombatTile ct in _combatMap)
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
                    }
                    else
                    {
                        Delay -= gameTime.ElapsedGameTime.TotalSeconds;
                    }
                    break;

                case PhaseEnum.Upkeep:
                    Summon activeSummon = ActiveCharacter.LinkedSummon;
                    if (activeSummon == null || !activeSummon.Regen)
                    {
                        SetPhaseForTurn();
                    }
                    else if (activeSummon != null && activeSummon.Regen && activeSummon.BodySprite.CurrentAnimation != "Cast")
                    {
                        activeSummon.PlayAnimation(CActorAnimEnum.Cast);
                    }
                    else if(activeSummon.BodySprite.GetPlayCount() >= 1)
                    {
                        activeSummon.PlayAnimation(CActorAnimEnum.Idle);
                        ActiveCharacter.IncreaseHealth(30);
                        ActiveCharacter.Tile.GUITile.AssignEffect(30, false);
                        SetPhaseForTurn();
                    }
                    
                    break;

            }
        }

        private static void GiveXP()
        {
            int toGive = 0;
            int total = 0;
            CurrentMob.GetXP(ref toGive, ref total);

            int xpDrain = 5;
            if (toGive > 0)
            {
                CurrentMob.DrainXP(xpDrain);
                foreach (CombatAdventurer a in _listParty)
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
            foreach (CombatActor m in _liMonsters)
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
                foreach (CombatAdventurer a in _listParty)
                {
                    int levl = a.ClassLevel;
                    a.CurrentCharge = 0;
                    //a.AddXP(EarnedXP);
                    a.PlayAnimation(CActorAnimEnum.Win);

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
            GUIManager.FadeOut();
            //MapManager.DropItemsOnMap(DropManager.DropItemsFromMob(_mob.ID), _mob.CollisionBox.Center.ToVector2());
            MapManager.RemoveMob(_mob);
            _mob = null;
            GoToWorldMap();
        }

        public static void EndCombatEscape()
        {
            GUIManager.FadeOut();
            _mob.Stun();
            GoToWorldMap();
        }

        public static void SetPhaseForTurn()
        {
            if (_liMonsters.Contains(ActiveCharacter)) {
                CurrentPhase = PhaseEnum.EnemyTurn;
                EnemyTakeTurn();
            }
            else if (_listParty.Contains(ActiveCharacter)) { CurrentPhase = PhaseEnum.NewTurn; }
        }

        //For now, when the enemy takes their turn, have them select a random party member
        //When enemies get healing/defensive skills, they'll have their own logic to process
        public static void EnemyTakeTurn()
        {
            RHRandom r = new RHRandom();
            CombatAction action = null;
            bool canCast = false;
            do
            {
                action = (CombatAction)ActiveCharacter.AbilityList[r.Next(0, ActiveCharacter.AbilityList.Count - 1)];
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
                    CombatActor adv = _listParty[r.Next(0, _listParty.Count - 1)];
                    SelectedTile = adv.Tile;
                }
            }

            ActiveCharacter.CurrentMP -= action.MPCost;
        }

        internal static List<CombatActor> CalculateTurnOrder(int maxShown)
        {
            List<CombatActor> rv = new List<CombatActor>();
            List<CombatActor> queuedCopy = new List<CombatActor>();
            List<CombatActor> chargingCopy = new List<CombatActor>();

            LoadList(ref queuedCopy, _liQueuedCharacters);
            LoadList(ref chargingCopy, _liChargingCharacters);

            //If there is an Active Character, Add them to the Turn Order list and blank their DummyCharge
            if (ActiveCharacter != null)
            {
                rv.Add(ActiveCharacter);
                CombatActor c = chargingCopy.Find(x => x.Name == ActiveCharacter.Name);
                c.DummyCharge -= (SelectedAction == null) ? c.DummyCharge : SelectedAction.ChargeCost();
            }

            //If there are any queued Actors, add them to the charging list
            foreach (CombatActor c in queuedCopy)
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
                foreach (CombatActor c in queuedCopy)
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

        private static void LoadList(ref List<CombatActor> toFill, List<CombatActor> takeFrom)
        {
            foreach (CombatActor c in takeFrom)
            {
                CombatActor actor = c;
                actor.DummyCharge = c.CurrentCharge;
                toFill.Add(actor);
            }
        }

        public static void ProcessActionChoice(CombatAction a)
        {
            SelectedAction = new ChosenAction(a);

            if (!SelectedAction.SelfOnly())
            {
                if (!ActiveCharacter.IsMonster()) {
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

        public static void Kill(CombatActor c)
        {
            if (_liMonsters.Contains((c)))
            {
                c.BodySprite.IsAnimating = false;
                _liMonsters.Remove(c);
                _liChargingCharacters.Remove(c);                    //Remove the killed member from the turn order 
                _liQueuedCharacters.Remove(c);
            }
        }

        public static bool PartyUp()
        {
            bool stillOne = false;
            foreach (CombatActor character in _listParty)
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
        public static void TestHoverTile(CombatTile tile)
        {
            if (SelectedAction.LegalTiles.Contains(tile) && (tile.Occupied() || SelectedAction.AreaOfEffect()))
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

            if (temp != null && SelectedAction.CompareTargetType(temp.TargetType)) {
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

        private static List<CombatTile> GetAdjacent(CombatTile t) {
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
                rv =  _combatMap[t.Row - 1, t.Col];
            }

            return rv;
        }

        public static CombatTile GetBottom(CombatTile t)
        {
            CombatTile rv = null;
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
        public static CombatTile GetLeft(CombatTile t)
        {
            CombatTile rv = null;
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
        public static CombatTile GetRight(CombatTile t)
        {
            CombatTile rv = null;
            if (t.Col < MAX_COL - 1 && (t.Col >= ENEMY_FRONT || t.Col + 1 < ENEMY_FRONT))
            {
                rv = _combatMap[t.Row, t.Col + 1];
            }

            return rv;
        }

        public static void ClearSelectedTile()
        {
            if(SelectedTile != null) { SelectedTile.Select(false); }
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
        private static void CombatTick(ref List<CombatActor> charging, ref List<CombatActor> queued, bool dummy = false)
        {
            List<CombatActor> toQueue = new List<CombatActor>();
            foreach(CombatActor c in charging)
            {
                //If Actor is not knocked out, increment the charge, capping to 100
                if (!c.KnockedOut() || c.CurrentHP > 0)
                {
                    if (dummy) { HandleChargeTick(ref c.DummyCharge, ref toQueue, c); }
                    else { HandleChargeTick(ref c.CurrentCharge, ref toQueue, c); }
                }
            }

            foreach(CombatActor c in toQueue)
            {
                queued.Add(c);
                charging.Remove(c);
            }
        }
        private static void HandleChargeTick(ref int charge, ref List<CombatActor> toQueue, CombatActor c)
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
            if (_liQueuedCharacters[0].CurrentHP > 0) {
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
            else
            {
                int i = 0;
            }
        }
        public static void EndTurn()
        {
            ActiveCharacter.CurrentCharge -= SelectedAction.ChargeCost();

            Summon actSummon = ActiveCharacter.LinkedSummon;
            //If there is no linked summon, or it is a summon, end the turn normally.

            if (!EndCombatCheck())
            {
                if (actSummon != null)
                {
                    if (actSummon.Aggressive && SelectedAction.IsMelee())
                    {
                        List<CombatTile> targets = SelectedAction.GetTargetTiles();
                        ActiveCharacter = actSummon;
                        SelectedAction = new ChosenAction((CombatAction)ObjectManager.GetActionByIndex(1));
                        SelectedAction.SetTargetTiles(targets);
                    }
                    else if (actSummon.TwinCast && SelectedAction.IsSpell() && !SelectedAction.IsSummonSpell())
                    {
                        ActiveCharacter = actSummon;
                        SelectedAction.SetUser(actSummon);
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

        public class CombatTile
        {
            TargetEnum _tileType;
            public TargetEnum TargetType => _tileType;

            int _iRow;
            public int Row => _iRow;
            int _iCol;
            public int Col => _iCol;

            bool _bSelected;
            public bool Selected => _bSelected;

            CombatActor _character;
            public CombatActor Character => _character;
            GUICmbtTile _gTile;
            public GUICmbtTile GUITile => _gTile;

            public CombatTile(int row, int col, TargetEnum tileType)
            {
                _iRow = row;
                _iCol = col;
                _tileType = tileType;
            }

            public void SetCombatant(CombatActor c, bool moveCharNow = true)
            {
                _character = c;
                if (_character != null)
                {
                    if (_character.Tile != null)
                    {
                        _character.Tile.SetCombatant(null);
                    }
                    if(_character.Tile != null) {
                        foreach (CombatTile tile in GetAdjacent(_character.Tile))
                        {
                            CheckForProtected(tile);
                        }
                    }
                    _character.Tile = this;
                    CheckForProtected(this);
                }

                _gTile.SyncGUIObjects(_character != null);
                if (_character != null)
                {
                    foreach (KeyValuePair<ConditionEnum, bool> kvp in _character.DiConditions)
                    {
                        if (kvp.Value)
                        {
                            GUITile.ChangeCondition(kvp.Key, TargetEnum.Enemy);
                        }
                    }
                }
            }
            public void SetSummon(Summon s)
            {
                _character.LinkSummon(s);
                _gTile.LinkSummon(s);
            }

            private void CheckForProtected(CombatTile t)
            {
                bool found = false;
                List<CombatTile> adjacent = GetAdjacent(t);
                foreach (CombatTile tile in adjacent)
                {
                    if (tile.Occupied() && this.TargetType == tile.TargetType)
                    {
                        if (tile.Character != this.Character && tile.Character.IsCombatAdventurer() && this.Character.IsCombatAdventurer())
                        {
                            found = true;
                            CombatAdventurer adv = (CombatAdventurer)tile.Character;
                            adv.Protected = true;
                            adv = (CombatAdventurer)this.Character;
                            adv.Protected = true;
                        }
                    }
                }

                if (!found && this.Character.IsCombatAdventurer())
                {
                    CombatAdventurer adv = (CombatAdventurer)this.Character;
                    adv.Protected = false;
                }
            }

            public void AssignGUITile(GUICmbtTile c)
            {
                _gTile = c;
            }

            public bool Occupied()
            {
                return _character != null;
            }

            public void Select(bool val)
            {
                _bSelected = val;

                if (_bSelected && SelectedTile != this)
                {
                    if (SelectedTile != null) { SelectedTile.Select(false); }
                    SelectedTile = this;
                }
            }

            public void PlayAnimation<TEnum>(TEnum animation)
            {
                _gTile.PlayAnimation(animation);
            }
        }
        public class ChosenAction
        {
            private Consumable _chosenItem;
            private CombatAction _chosenAction;

            List<CombatTile> _liLegalTiles;
            public List<CombatTile> LegalTiles => _liLegalTiles;
            public CombatActor User;

            string _name;
            public string Name => _name;

            bool _bDrawItem;

            public ChosenAction(Consumable it)
            {
                User = ActiveCharacter;
                _chosenItem = it;
                _name = _chosenItem.Name;
                _liLegalTiles = new List<CombatTile>();

                //Only the adjacent tiles are legal
                if (TargetsAlly())
                {
                    _liLegalTiles.Add(User.Tile);

                    List<CombatTile> adj = GetAdjacent(User.Tile);
                    foreach (CombatTile t in adj)
                    {
                        if (t.TargetType == TargetEnum.Ally) { _liLegalTiles.Add(t); }
                    }
                }
                else if (TargetsEnemy())
                {
                    EnemyFrontLineLegal();
                }
            }
            public ChosenAction(CombatAction ca)
            {
                User = ActiveCharacter;
                _chosenAction = ca;
                _name = _chosenAction.Name;

                _chosenAction.SkillUser = ActiveCharacter;

                _liLegalTiles = new List<CombatTile>();
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

                    if(ActiveCharacter.Tile.Col > 0) { startCol = ActiveCharacter.Tile.Col - 1; }
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
                    int size = TileSize * CombatManager.CombatScale;
                    GUIImage gItem = new GUIImage(_chosenItem.SourceRectangle, size, size, _chosenItem.Texture);
                    CombatActor c = CombatManager.ActiveCharacter;

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
                    CombatActor c = CombatManager.ActiveCharacter;
                    if (!c.IsCurrentAnimation(CActorAnimEnum.Cast))
                    {
                        c.PlayAnimation(CActorAnimEnum.Cast);
                        _bDrawItem = true;
                    }
                    else if (c.AnimationPlayedXTimes(3))
                    {
                        c.PlayAnimation(CActorAnimEnum.Idle);
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
                    CombatManager.ActiveCharacter.CurrentMP -= _chosenAction.MPCost;          //Checked before Processing
                    _chosenAction.AnimationSetup();
                    CombatManager.Text = SelectedAction.Name;
                }
                else if (_chosenItem != null)
                {
                    CombatManager.Text = SelectedAction.Name;
                }
                CombatManager.CurrentPhase = PhaseEnum.DisplayAttack;
                CombatManager.ClearSelectedTile();
            }

            public int ChargeCost()
            {
                int rv = 100;

                if(_chosenAction != null) { rv = _chosenAction.ChargeCost; }

                return rv;
            }

            /// <summary>
            /// Retrieves the tiles that will be effected by this skill based off the area type
            /// </summary>
            /// <returns>A complete list of tiles that will be hit</returns>
            public List<CombatTile> GetEffectedTiles(){
                List<CombatTile> cbtTile = new List<CombatTile>();
                if (_chosenItem != null) {
                    cbtTile.Add(SelectedTile);
                }
                else
                {
                    CombatActor actor = _chosenAction.SkillUser;
                    if (SelectedTile != null)
                    {
                        cbtTile.Add(SelectedTile);
                        if (_chosenAction.AreaOfEffect != AreaEffectEnum.Single)
                        {
                            //Describes which side of the Battlefield we are targetting
                            bool monsterSide = (actor.IsCombatAdventurer() && TargetsEnemy()) || (actor.IsMonster() && TargetsAlly());
                            bool partySide = (actor.IsMonster() && TargetsEnemy()) || (actor.IsCombatAdventurer() && TargetsAlly());

                            //All we need to do here is select all of the tiles containing the appropriate characters
                            if (_chosenAction.AreaOfEffect == AreaEffectEnum.Each)
                            {
                                if (monsterSide)
                                {
                                    foreach(Monster m in _liMonsters)
                                    {
                                        if (!cbtTile.Contains(m.Tile)) { cbtTile.Add(m.Tile); }
                                    }
                                }
                                else
                                {
                                    foreach (CombatActor adv in _listParty)
                                    {
                                        if (!cbtTile.Contains(adv.Tile)) { cbtTile.Add(adv.Tile); }
                                    }
                                }
                            }
                            else {
                                //The coordinates of the selected tile
                                int targetRow = SelectedTile.Row;
                                int targetCol = SelectedTile.Col;

                                //Determines how far to the side the skill can go, based on whether it grows left or right
                                int minCol = monsterSide ? ENEMY_FRONT : 0;
                                int maxCol = monsterSide ? MAX_COL : ENEMY_FRONT;
                                if (_chosenAction.AreaOfEffect == AreaEffectEnum.Cross)
                                {
                                    if (targetRow - 1 >= 0) { cbtTile.Add(_combatMap[targetRow - 1, targetCol]); }
                                    if (targetRow + 1 < MAX_ROW) { cbtTile.Add(_combatMap[targetRow + 1, targetCol]); }

                                    if (targetCol - 1 >= minCol) { cbtTile.Add(_combatMap[targetRow, targetCol - 1]); }
                                    if (targetCol + 1 < maxCol) { cbtTile.Add(_combatMap[targetRow, targetCol + 1]); }
                                }
                                else if (_chosenAction.AreaOfEffect == AreaEffectEnum.Rectangle)
                                {
                                    KeyValuePair<int, int> dimensions = _chosenAction.Dimensions;
                                    if (monsterSide)
                                    {
                                        for (int rows = targetRow; rows < MAX_ROW && rows < targetRow + dimensions.Key; rows++)
                                        {
                                            for (int cols = targetCol; cols < maxCol && cols < targetCol + dimensions.Value; cols++)
                                            {
                                                CombatTile t = _combatMap[rows, cols];
                                                if (!cbtTile.Contains(t))
                                                {
                                                    cbtTile.Add(t);
                                                }
                                            }
                                        }
                                    }
                                    else if (partySide)
                                    {
                                        for (int rows = targetRow; rows < MAX_ROW && rows < targetRow + dimensions.Key; rows++)
                                        {
                                            for (int cols = targetCol; cols >= minCol && cols > targetCol - dimensions.Value; cols--)
                                            {
                                                CombatTile t = _combatMap[rows, cols];
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

            public void SetUser(CombatActor c) {
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
            public bool IsSpell() { return _chosenAction != null && _chosenAction.IsSpell(); }
            public bool IsSummonSpell() { return _chosenAction != null && _chosenAction.IsSummonSpell(); }
            public bool SelfOnly() { return _chosenAction.Range == RangeEnum.Self; }
            public bool IsMelee() { return _chosenAction.Range == RangeEnum.Melee; }
            public bool IsRanged() { return _chosenAction.Range == RangeEnum.Ranged; }
            public bool SingleTarget()
            {
                bool rv = false;
                if (_chosenAction != null) { rv = _chosenAction.AreaOfEffect == AreaEffectEnum.Single; }
                else if (_chosenItem != null) { rv = true; }

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

                if(_chosenAction != null) { rv = _chosenAction.TargetsEach(); }

                return rv;
            }
            public bool AreaOfEffect()
            {
                bool rv = false;

                if (_chosenAction != null) { rv = _chosenAction.AreaOfEffect != AreaEffectEnum.Single; }
                else if (_chosenItem != null) { rv = false; }

                return rv;
            }
            public bool Columns() {
                bool rv = false;
                if (_chosenAction != null) { rv = _chosenAction.Range == RangeEnum.Column; }
                else if (_chosenItem != null) { rv = false; }

                return rv;
            }

            public void Clear()
            {
                if(_chosenAction != null) { _chosenAction.TileTargetList.Clear(); }
                else if (_chosenItem != null) { _targetTile = null; }
            }
            public List<CombatTile> GetTargetTiles()
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
            public void SetTargetTiles(List<CombatTile> li)
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
 