using Microsoft.Xna.Framework;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.WorldObjects;
using RiverHollow.Misc;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using System;
using static RiverHollow.GUIObjects.GUIObject;
using Microsoft.Xna.Framework.Input;

namespace RiverHollow.Game_Managers
{
    public static class CombatManager
    {
        public static int CombatScale = 5;
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

        public enum PhaseEnum { NewTurn, EnemyTurn, SelectSkill, ChooseSkillTarget, ChooseItemTarget, DisplayAttack, PerformAction, EndCombat }
        public static PhaseEnum CurrentPhase;

        public static int TurnIndex;

        public static CombatAction ChosenSkill;
        public static CombatItem ChosenItem;
        private static CombatTile _targetTile;

        public static double Delay;
        public static string Text;

        #region CombatGrid
        public enum TargetEnum { None, Ally, Enemy };
        public static TargetEnum TargetType;

        public static CombatTile SelectedTile;

        static readonly int MAX_COL = 8;
        static readonly int MAX_ROW = 3;
        static readonly int ALLY_FRONT = 3;
        static readonly int ENEMY_FRONT = 4;

        static CombatTile[,] _combatMap;
        #endregion

        public static void NewBattle(Mob m)
        {
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

            GoToCombat();
            SetPhaseForTurn();
            PlayerManager.DecreaseStamina(1);
        }

        public static void ConfigureAllies(ref GUICmbtTile[,] allyArray)
        {
            int cols = MAX_COL / 2;
            if (_combatMap != null)
            {
                allyArray = new GUICmbtTile[MAX_ROW, cols];
                for (int row = 0; row < MAX_ROW; row++)
                {
                    for (int col = 0; col < cols; col++)
                    {
                        allyArray[row, col] = new GUICmbtTile(_combatMap[row, col]);
                        if (row == 0 && col == 0) { allyArray[row, col].AnchorToScreen(SideEnum.Left, 100); }
                        else if (col == 0) { allyArray[row, col].AnchorAndAlignToObject(allyArray[row - 1, col], SideEnum.Bottom, SideEnum.Left); }
                        else { allyArray[row, col].AnchorAndAlignToObject(allyArray[row, col - 1], SideEnum.Right, SideEnum.Bottom); }
                    }
                }
            }

            bool oneAdded = false;
            //Get the Players' party and assign each of them a battle position
            List<CombatCharacter> party = CombatManager.Party;
            for (int i = 0; i < party.Count; i++)
            {
                if (party[i] != null)
                {
                    _combatMap[oneAdded ? 2 : 0, i].SetCombatant(party[i]);
                    oneAdded = true;
                }
            }
        }
        public static void ConfigureEnemies(ref GUICmbtTile[,] enemyArray)
        {
            int cols = MAX_COL / 2;
            if (_combatMap != null)
            {
                enemyArray = new GUICmbtTile[MAX_ROW, cols];
                for (int row = 0; row < MAX_ROW; row++)
                {
                    for (int col = cols - 1; col >= 0; col--)
                    {
                        enemyArray[row, col] = new GUICmbtTile(_combatMap[row, col + 4]);
                        if (row == 0 && col == cols - 1) { enemyArray[row, col].AnchorToScreen(SideEnum.Right, 100); }
                        else if (col == cols - 1) { enemyArray[row, col].AnchorAndAlignToObject(enemyArray[row - 1, col], SideEnum.Bottom, SideEnum.Right); }
                        else { enemyArray[row, col].AnchorAndAlignToObject(enemyArray[row, col + 1], SideEnum.Left, SideEnum.Bottom); }
                    }
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
            foreach(CombatTile ct in _combatMap)
            {
                if(ct.TargetType == TargetEnum.Enemy && !_listMonsters.Contains(ct.Character))
                {
                    ct.SetCombatant(null);
                }
            }
        }

        //If we have not gone to the EndTurn phase, increment the turn loop as appropriate
        //If we loop back to 0, reduce stamina by the desired amount.
        public static void NextTurn()
        {
            if(!EndCombatCheck())
            {
                if (CurrentPhase != PhaseEnum.EndCombat)
                {
                    ChosenSkill = null;
                    ChosenItem = null;
                    if (TurnIndex + 1 < TurnOrder.Count) { TurnIndex++; }
                    else
                    {
                        TurnIndex = 0;
                        PlayerManager.DecreaseStamina(stamDrain);
                        GameCalendar.IncrementMinutes();
                    }

                    
                    ActiveCharacter = TurnOrder[TurnIndex];
                    if (ActiveCharacter.KnockedOut()) { NextTurn(); }
                    else
                    {
                        ActiveCharacter.TickBuffs();
                        if (ActiveCharacter.Poisoned()) {
                            ActiveCharacter.Location.AssignDamage(ActiveCharacter.DecreaseHealth(Math.Max(1, (int)(ActiveCharacter.MaxHP/20))));
                        }
                        SetPhaseForTurn();
                    }
                }
            }
        }

        internal static bool CanCancel()
        {
            return CurrentPhase == PhaseEnum.ChooseItemTarget || CurrentPhase == PhaseEnum.ChooseSkillTarget || CurrentPhase == PhaseEnum.SelectSkill;
        }

        private static bool EndCombatCheck()
        {
            bool rv = false;
            if(!PartyUp() || _listMonsters.Count == 0)
            {
                EndBattle();
                rv = true;
            }

            return rv;
        }

        private static void SetPhaseForTurn()
        {
            if (_listMonsters.Contains(ActiveCharacter)) {
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
            ProcessActionChoice((CombatAction)CharacterManager.GetActionByIndex(1), false);//ActiveCharacter.AbilityList[r.Next(0, ActiveCharacter.AbilityList.Count - 1)]);
            if (!ChosenSkill.Target.Equals("Self"))
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
        }

        public static void ProcessActionChoice(CombatAction a, bool chooseTarget = true)
        {
            ChosenSkill = a;
            ChosenSkill.SkillUser = ActiveCharacter;
            ChosenSkill.UserStartPosition = ActiveCharacter.Position;

            if (!ChosenSkill.Target.Equals("Self"))
            {
                if (chooseTarget) {
                    TargetType = ChosenSkill.Target.Equals("Enemy") ? TargetEnum.Enemy : TargetEnum.Ally;
                    CurrentPhase = PhaseEnum.ChooseSkillTarget;
                }  //Skips this phase for enemies. They don't "choose" targets
            }
            else
            {
                CurrentPhase = PhaseEnum.DisplayAttack;
                Text = ChosenSkill.Name;
            }
        }

        public static void ProcessItemChoice(CombatItem it)
        {
            CurrentPhase = PhaseEnum.ChooseItemTarget;
            ChosenItem = it;

            TargetType = ChosenItem.Helpful ? TargetEnum.Ally : TargetEnum.Enemy;
        }

        //Assign target to the skill as well as the skill user
        public static void SetSkillTarget()
        {
            ActiveCharacter.CurrentMP -= ChosenSkill.MPCost;          //Checked before Processing
            _targetTile = SelectedTile;
            ChosenSkill.AnimationSetup(SelectedTile);
            Text = ChosenSkill.Name;
            CurrentPhase = PhaseEnum.DisplayAttack;
            ClearSelectedTile();
        }

        public static void SetItemTarget()
        {
            _targetTile = SelectedTile;
            Text = ChosenItem.Name;
            CurrentPhase = PhaseEnum.DisplayAttack;
            ClearSelectedTile();
        }
        internal static void UseItem()
        {
            if (ChosenItem.Condition != ConditionEnum.None)
            {
                _targetTile.Character.ChangeConditionStatus(ChosenItem.Condition, !ChosenItem.Helpful);
            }
            int val = _targetTile.Character.IncreaseHealth(ChosenItem.Health);
            if (val > 0)
            {
                _targetTile.GUITile.Heal(val);
            }
            InventoryManager.RemoveItemFromInventory(ChosenItem);

            NextTurn();
        }

        //Gives the total battle XP to every member of the party, remove the mob from the gameand drop items
        public static void EndBattle()
        {
            if (PartyUp())
            {
                foreach (CombatAdventurer a in _listParty) { a.AddXP(_xpValue); }
                MapManager.DropItemsOnMap(DropManager.DropItemsFromMob(_mob.ID), _mob.CollisionBox.Center.ToVector2());
            }
            MapManager.RemoveMob(_mob);
            _mob = null;
            GoToWorldMap();
        }

        public static void Kill(CombatCharacter c)
        {
            if (_listMonsters.Contains((c)))
            {
                _listMonsters.Remove(c);
                TurnOrder.Remove(c);                        //Remove the killed member from the turn order 
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

        #region SelectionHandling
        public static void TestHoverTile(CombatTile tile)
        {
            if (tile.Occupied() && tile.Col == FindEnemyFrontLine())
            {
                tile.Select(true);
            }
        }
        public static void HandleKeyboardTargetting()
        {
            bool melee = !ChosenSkill.IsSpell();
            if (SelectedTile == null)
            {
                _combatMap[0, ENEMY_FRONT].Select(true);
                if (!SelectedTile.Occupied())
                {
                    FindNextTarget();
                }
            }

            //CombatTile temp = null;
            if (InputManager.CheckPressedKey(Keys.A))
            {
                FindLastTarget();
                //temp = GetLeft(SelectedTile);
            }
            else if (InputManager.CheckPressedKey(Keys.D))
            {
                FindNextTarget();
                //temp = GetRight(SelectedTile);
            }
            else if (InputManager.CheckPressedKey(Keys.W))
            {
                FindLastTarget();
                //temp = GetTop(SelectedTile);
            }
            else if (InputManager.CheckPressedKey(Keys.S))
            {
                FindNextTarget();
                
                //temp = GetBottom(SelectedTile);
            }

            //If we're targetting enemies, only move to enemy tiles
            //if (temp != null && temp.TargetType == TargetType) { temp.Select(true); }

            if (InputManager.CheckPressedKey(Keys.Enter))
            {
                if (true /*temp.TargetType == TargetEnum.Enemy*/) { CombatManager.SetSkillTarget(); }
                else { CombatManager.SetItemTarget(); }
            }
        }
        public static void FindNextTarget()
        {
            if (TargetType == TargetEnum.Enemy)
            {
                int col = FindEnemyFrontLine();

                for (int row = SelectedTile.Row; row < MAX_ROW; row++)
                {
                    if (FindFirstHelper(_combatMap[row, col]))
                    {
                        goto FindFirstExit;
                    }
                }
            }
       
            FindFirstExit:

            return;
        }
        public static void FindLastTarget()
        {
            int col = FindEnemyFrontLine();

            for (int row = SelectedTile.Row; row >= 0; row--)
            {
                if (FindFirstHelper(_combatMap[row, col]))
                {
                    goto FindFirstExit;
                }
            }
            FindFirstExit:

            return;
        }
        private static bool FindFirstHelper(CombatTile tile)
        {
            bool rv = false;
            if (tile != SelectedTile && tile.Occupied())
            {
                tile.Select(true);
                rv = true;
            }

            return rv;
        }

        private static CombatTile GetTop(CombatTile t)
        {
            CombatTile rv = null;
            if (t.Row > 0)
            {
                rv =  _combatMap[t.Row - 1, t.Col];
            }

            return rv;
        }
        private static CombatTile GetBottom(CombatTile t)
        {
            CombatTile rv = null;
            if (t.Row < MAX_ROW - 1)
            {
                rv = _combatMap[t.Row + 1, t.Col];
            }

            return rv;
        }
        private static CombatTile GetLeft(CombatTile t)
        {
            CombatTile rv = null;
            if (t.Col > 0)
            {
                rv = _combatMap[t.Row, t.Col - 1];
            }

            return rv;
        }
        private static CombatTile GetRight(CombatTile t)
        {
            CombatTile rv = null;
            if (t.Col < MAX_COL - 1)
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

        #endregion

        public static bool PhaseSelectSkill() { return CurrentPhase == PhaseEnum.SelectSkill; }
        public static bool PhaseChooseSkillTarget() { return CurrentPhase == PhaseEnum.ChooseSkillTarget; }
        public static bool PhaseChooseItemTarget() { return CurrentPhase == PhaseEnum.ChooseItemTarget; }

        public class CombatTile
        {
            TargetEnum _tileType;
            public TargetEnum TargetType => _tileType;

            int _iHeal;
            public int  HealAmount => _iHeal;
            int _iRow;
            public int Row => _iRow;
            int _iCol;
            public int Col => _iCol;

            bool _bSelected;
            public bool Selected => _bSelected;

            CombatCharacter _character;
            public CombatCharacter Character => _character;
            GUICmbtTile _gTile;
            public GUICmbtTile GUITile => _gTile;

            public CombatTile(int row, int col, TargetEnum tileType)
            {
                _iRow = row;
                _iCol = col;
                _tileType = tileType;
            }

            public void SetCombatant(CombatCharacter c)
            {
                _character = c;
                _gTile.SyncGUIObjects(c != null);
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
        }
    }
}
