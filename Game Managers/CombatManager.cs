﻿using Microsoft.Xna.Framework;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.WorldObjects;
using RiverHollow.Misc;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using System;
using static RiverHollow.GUIObjects.GUIObject;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

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

        public enum PhaseEnum { Charging, NewTurn, EnemyTurn, SelectSkill, ChooseTarget, DisplayAttack, PerformAction, EndCombat }
        public static PhaseEnum CurrentPhase;
        public static ChosenAction SelectedAction;
        private static CombatTile _targetTile;

        public static double Delay;
        public static string Text;

        #region Turn Sequence
        static List<CombatCharacter> _liQueuedCharacters;
        static List<CombatCharacter> _liChargingCharacters;
        #endregion

        #region CombatGrid
        public static CombatTile SelectedTile;

        static readonly int MAX_COL = 8;
        static readonly int MAX_ROW = 3;
        static readonly int ALLY_FRONT = 3;
        static readonly int ENEMY_FRONT = 4;

        static CombatTile[,] _combatMap;
        #endregion

        public static void NewBattle(Mob m)
        {
            ActiveCharacter = null;
            SelectedAction = null;
            SelectedTile = null;

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

            _liQueuedCharacters = new List<CombatCharacter>();
            _liChargingCharacters = new List<CombatCharacter>();
            _liChargingCharacters.AddRange(_listParty);
            _liChargingCharacters.AddRange(_listMonsters);

            //Characters with higher Spd go first
            _liChargingCharacters.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));

            RHRandom random = new RHRandom();
            foreach(CombatCharacter c in _liChargingCharacters)
            {
                c.CurrentCharge += random.Next(0, 50);
            }

            GoToCombat();
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
            if (ActiveCharacter == null)
            {
                if (_liQueuedCharacters.Count == 0)
                {
                    CombatTick();
                }
                else
                {
                    GetActiveCharacter();
                }
            }

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

        internal static bool CanCancel()
        {
            return CurrentPhase == PhaseEnum.ChooseTarget || CurrentPhase == PhaseEnum.SelectSkill;
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
            //MAR
            ProcessActionChoice((CombatAction)CharacterManager.GetActionByIndex(1));//ActiveCharacter.AbilityList[r.Next(0, ActiveCharacter.AbilityList.Count - 1)]);
            if (!SelectedAction.SelfOnly())
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

        public static void ProcessItemChoice(CombatItem it)
        {
            CurrentPhase = PhaseEnum.ChooseTarget;
            SelectedAction = new ChosenAction(it);
        }

        //Gives the total battle XP to every member of the party, remove the mob from the gameand drop items
        public static void EndBattle()
        {
            if (PartyUp())
            {
                foreach (CombatAdventurer a in _listParty) {
                    a.CurrentCharge = 0;
                    a.AddXP(_xpValue);
                }
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
                _liChargingCharacters.Remove(c);                    //Remove the killed member from the turn order 
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

        #region Turn Handling
        private static void CombatTick()
        {
            List<CombatCharacter> toQueue = new List<CombatCharacter>();
            foreach(CombatCharacter c in _liChargingCharacters)
            {
                if (!c.KnockedOut())
                {
                    c.CurrentCharge += c.StatSpd;
                    if (c.CurrentCharge >= 100)
                    {
                        c.CurrentCharge = 100;
                        toQueue.Add(c);
                    }
                }
            }

            foreach(CombatCharacter c in toQueue)
            {
                _liQueuedCharacters.Add(c);
                _liChargingCharacters.Remove(c);
            }
        }
        private static void GetActiveCharacter()
        {
            ActiveCharacter = _liQueuedCharacters[0];
            _liQueuedCharacters.RemoveAt(0);
            _liChargingCharacters.Add(ActiveCharacter);
            _liChargingCharacters.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));

            ActiveCharacter.CurrentCharge = 0;
            ActiveCharacter.TickBuffs();
            if (ActiveCharacter.Poisoned())
            {
                ActiveCharacter.Location.AssignDamage(ActiveCharacter.DecreaseHealth(Math.Max(1, (int)(ActiveCharacter.MaxHP / 20))));
            }
            SetPhaseForTurn();
        }
        public static void EndTurn()
        {
            if (!EndCombatCheck())
            {
                if (CurrentPhase != PhaseEnum.EndCombat)
                {
                    SelectedAction = null;
                    ActiveCharacter = null;
                }
            }
        }
        #endregion
        public static bool PhaseSelectSkill() { return CurrentPhase == PhaseEnum.SelectSkill; }
        public static bool PhaseChooseTarget() { return CurrentPhase == PhaseEnum.ChooseTarget; }

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
                if (c != null) { _character.Tile = this; }
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
        public class ChosenAction
        {
            private CombatItem _chosenItem;
            private CombatAction _chosenAction;

            List<CombatTile> _liLegalTiles;
            public List<CombatTile> LegalTiles => _liLegalTiles;
            CombatCharacter _user;

            string _name;
            public string Name => _name;

            bool _bDrawItem;

            public ChosenAction(CombatItem it)
            {
                _user = ActiveCharacter;
                _chosenItem = it;
                _name = _chosenItem.Name;
                _liLegalTiles = new List<CombatTile>();

                //Only the adjacent tiles are legal
                if (TargetsAlly())
                {
                    _liLegalTiles.Add(_user.Tile);

                    List<CombatTile> adj = GetAdjacent(_user.Tile);
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
                _user = ActiveCharacter;
                _chosenAction = ca;
                _name = _chosenAction.Name;

                _chosenAction.SkillUser = ActiveCharacter;
                _chosenAction.UserStartPosition = ActiveCharacter.Position;

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
                    _liLegalTiles.Add(_user.Tile);
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
                    CombatCharacter c = CombatManager.ActiveCharacter;
                    Point p = c.Position.ToPoint();
                    p.X += c.Width / 2 - 16;
                    _chosenItem.Draw(spritebatch, new Rectangle(p, new Point(32, 32)));
                }
                if (_chosenAction != null)
                {
                    _chosenAction.Sprite.Draw(spritebatch, false);
                }
            }

            public void PerformAction(GameTime gameTime)
            {
                if (_chosenAction != null) { _chosenAction.HandlePhase(gameTime); }
                else if (_chosenItem != null)
                {
                    bool finished = false;
                    CombatCharacter c = CombatManager.ActiveCharacter;
                    if (!c.IsCurrentAnimation("Cast"))
                    {
                        c.PlayAnimation("Cast");
                        _bDrawItem = true;
                    }
                    else if (c.AnimationPlayedXTimes(3))
                    {
                        c.PlayAnimation("Walk");
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
                        _targetTile.GUITile.Heal(val);
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
                    _chosenAction.AnimationSetup(SelectedTile);
                    CombatManager.Text = SelectedAction.Name;
                }
                else if (_chosenItem != null)
                {
                    CombatManager.Text = SelectedAction.Name;
                }
                CombatManager.CurrentPhase = PhaseEnum.DisplayAttack;
                CombatManager.ClearSelectedTile();
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
            public bool AreaOfEffect()
            {
                bool rv = false;
                if (_chosenAction != null) { rv = _chosenAction.AreaOfEffect == AreaEffectEnum.Area; }
                else if (_chosenItem != null) { rv = false; }

                return rv;
            }
        }
    }
}
 