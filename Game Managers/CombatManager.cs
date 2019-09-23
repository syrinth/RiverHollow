using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Actors;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.GUIObjects.GUIObject;

namespace RiverHollow.Game_Managers
{
    public static class CombatManager
    {
        public const int BASIC_ATTACK = 300;
        public static int CombatScale = 5;
        private static Monster _mob;
        public static Monster CurrentMob { get => _mob; }
        public static CombatActor ActiveCharacter;
        private static List<CombatActor> _liMonsters;
        public static List<CombatActor> Monsters  => _liMonsters;
        private static List<CombatActor> _listParty;
        public static List<CombatActor> Party => _listParty;
        
        public enum PhaseEnum { Charging, Upkeep, NewTurn, EnemyTurn, SelectSkill, ChooseTarget, Defeat, DisplayAttack, DisplayVictory, Lost, PerformAction, EndCombat }
        public static PhaseEnum CurrentPhase;
        public static ChosenAction SelectedAction;
        private static RHTile _targetTile;

        public static double Delay;
        public static string Text;

        private static bool _bInCombat = false;
        public static bool InCombat => _bInCombat;

        #region Turn Sequence
        static List<CombatActor> _liQueuedCharacters;
        static List<CombatActor> _liChargingCharacters;
        #endregion

        public static RHTile SelectedTile;

        public static void NewBattle()
        {
            ActiveCharacter = null;
            SelectedAction = null;
            SelectedTile = null;

            CurrentPhase = PhaseEnum.Charging;

            Delay = 0;

            _listParty = new List<CombatActor>();
            _listParty.AddRange(PlayerManager.GetParty());

            _liMonsters = new List<CombatActor>();
            _liMonsters.AddRange(MapManager.CurrentMap.Monsters);

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

            foreach (CombatActor c in _liMonsters)
            {
                c.Tile = MapManager.CurrentMap.GetTileOffGrid(c.CollisionBox.Center);
            }

            foreach (CombatActor c in Party)
            {
                c.Tile = MapManager.CurrentMap.GetTileOffGrid(c.CollisionBox.Center);
            }

            GoToCombat();
            PlayerManager.DecreaseStamina(3);       //Decrease Stamina once
        }

        public static void Update(GameTime gTime)
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
                        Delay -= gTime.ElapsedGameTime.TotalSeconds;
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
                        ActiveCharacter.ModifyHealth(30, false);
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
                foreach (ClassedCombatant a in _listParty)
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
                foreach (ClassedCombatant a in _listParty)
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
            GUIManager.BeginFadeOut();
            //MapManager.DropItemsOnMap(DropManager.DropItemsFromMob(_mob.ID), _mob.CollisionBox.Center.ToVector2());
           // MapManager.RemoveMob(_mob);
            _mob = null;
            GoToWorldMap();
        }

        public static void EndCombatEscape()
        {
            GUIManager.BeginFadeOut();
            //_mob.Stun();
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
            bool moveToPlayer = false;
            RHTile targetTile = null;
            //RHRandom r = new RHRandom();
            //CombatAction action = null;
            ////Step one determine if we are in range of a target
            //foreach(CombatAction c in ActiveCharacter.AbilityList)
            //{
            //    if(c.Range == RangeEnum.Melee)
            //    {
            //        foreach(RHTile t in ActiveCharacter.Tile.GetAdjacent())
            //        {
            //            if(t.HasCombatant() && t.Character.IsAdventurer())
            //            {
            //                moveToPlayer = true;
            //                action = c;
            //                targetTile = t;
            //                break;
            //            }
            //        }
            //    }
            //}

            //Step two if not in range, move towards a target
            if (!ActiveCharacter.FollowingPath) {
                if (targetTile == null)
                {
                    double closestDistance = 0;
                    Vector2 start = ActiveCharacter.Tile.Center;
                    Vector2 closest = Vector2.Zero;

                    foreach (CombatActor actor in Party)
                    {
                        Vector2 target = actor.Tile.Center;

                        int deltaX = (int)Math.Abs(start.X - target.X);
                        int deltaY = (int)Math.Abs(start.Y - target.Y);

                        double distance = Math.Sqrt(deltaX ^ 2 + deltaY ^ 2);
                        if (distance < closestDistance || closestDistance == 0)
                        {
                            closest = target;
                            closestDistance = distance;
                        }
                    }

                    List<RHTile> tilePath = TravelManager.FindPathToLocation(ref start, closest, PlayerManager.CurrentMap);

                    //If we are moving to a Player character, we need to remove the last tile since it actually
                    //contains the character in question.
                    if (moveToPlayer) { tilePath.RemoveAt(tilePath.Count - 1); }
                    ActiveCharacter.SetPath(tilePath);
                }
            }

            ////Step three use action
            //if(action != null && targetTile != null)
            //{
            //    //bool canCast = false;
            //    //do
            //    //{
            //    //    action = (CombatAction)ActiveCharacter.AbilityList[r.Next(0, ActiveCharacter.AbilityList.Count - 1)];
            //    //    if (action.MPCost <= ActiveCharacter.CurrentMP)
            //    //    {
            //    //        canCast = true;
            //    //    }
            //    //} while (!canCast);

            //    ProcessActionChoice(action);
            //    if (!SelectedAction.SelfOnly())
            //    {
            //        if (action.Range == RangeEnum.Melee)
            //        {
            //            //ToDo Fix this
            //            //SelectedTile = 
            //        }
            //        else
            //        {
            //            CombatActor adv = _listParty[r.Next(0, _listParty.Count - 1)];
            //            SelectedTile = adv.Tile;
            //        }
            //    }

            //    ActiveCharacter.CurrentMP -= action.MPCost;
            //} 
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
        public static void TestHoverTile(RHTile tile)
        {
            if (SelectedAction.LegalTiles.Contains(tile) && (tile.HasCombatant() || SelectedAction.AreaOfEffect()))
            {
                tile.Select(true);
            }
        }
        public static void HandleKeyboardTargetting()
        {
            if (SelectedTile == null)
            {
                SelectedAction.LegalTiles[0].Select(true);

                if (!SelectedTile.HasCombatant())
                {
                    FindNextTarget();
                }
            }

            RHTile temp = null;
            if (InputManager.CheckPressedKey(Keys.A))
            {
                if (SelectedAction.IsMelee()) { FindLastTarget(); }
                else { temp = SelectedTile.GetTileByDirection(DirectionEnum.Left); }
            }
            else if (InputManager.CheckPressedKey(Keys.D))
            {
                if (SelectedAction.IsMelee()) { FindNextTarget(); }
                else { temp = SelectedTile.GetTileByDirection(DirectionEnum.Right); }
            }
            else if (InputManager.CheckPressedKey(Keys.W))
            {
                if (SelectedAction.IsMelee()) { FindLastTarget(); }
                else { temp = SelectedTile.GetTileByDirection(DirectionEnum.Up); }
            }
            else if (InputManager.CheckPressedKey(Keys.S))
            {
                if (SelectedAction.IsMelee()) { FindNextTarget(); }
                else { temp = SelectedTile.GetTileByDirection(DirectionEnum.Down); }
            }

            SelectTile(temp, true);

            if (InputManager.CheckPressedKey(Keys.Enter))
            {
                CombatManager.SelectedAction.SetSkillTarget();
            }
        }
        public static void FindNextTarget()
        {
            for (int i = 0; i < SelectedAction.LegalTiles.Count; i++)
            {
                RHTile t = SelectedAction.LegalTiles[i];

                if (t == SelectedTile)
                {
                    while (i < SelectedAction.LegalTiles.Count - 1)
                    {
                        t = SelectedAction.LegalTiles[i + 1];
                        if (t.HasCombatant())
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
                RHTile t = SelectedAction.LegalTiles[i];
                if (t == SelectedTile)
                {
                    while (i > 0)
                    {
                        t = SelectedAction.LegalTiles[i - 1];
                        if (t.HasCombatant())
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

        public static void SelectTile(RHTile tile, bool val)
        {
            tile.Select(val);

            if (val && SelectedTile != tile)
            {
                if (SelectedTile != null) { SelectedTile.Select(false); }
                SelectedTile = tile;
            }
        }

        public static void ClearSelectedTile()
        {
            if(SelectedTile != null) { SelectedTile.Select(false); }
            SelectedTile = null;
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
                    ActiveCharacter.ModifyHealth(Math.Max(1, (int)(ActiveCharacter.MaxHP / 20)), true);
                }

                CurrentPhase = PhaseEnum.Upkeep;    //We have a charcter, but go into Upkeep phase first
            }
        }
        public static void EndTurn()
        {
            ActiveCharacter.CurrentCharge -= SelectedAction.ChargeCost();

            Summon activeSummon = ActiveCharacter.LinkedSummon;
            //If there is no linked summon, or it is a summon, end the turn normally.

            if (!EndCombatCheck())
            {
                if (activeSummon != null)
                {
                    if (activeSummon.Aggressive && SelectedAction.IsMelee())
                    {
                        List<RHTile> targets = SelectedAction.GetTargetTiles();
                        ActiveCharacter = activeSummon;
                        SelectedAction = new ChosenAction((CombatAction)ObjectManager.GetActionByIndex(CombatManager.BASIC_ATTACK));
                        SelectedAction.SetUser(ActiveCharacter);
                        SelectedAction.SetTargetTiles(targets);
                    }
                    else if (activeSummon.TwinCast && SelectedAction.IsSpell() && !SelectedAction.IsSummonSpell() && SelectedAction.CanTwinCast())
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

        //public class CombatTile
        //{
        //    TargetEnum _tileType;
        //    public TargetEnum TargetType => _tileType;

        //    int _iRow;
        //    public int Row => _iRow;
        //    int _iCol;
        //    public int Col => _iCol;

        //    bool _bSelected;
        //    public bool Selected => _bSelected;

        //    CombatActor _character;
        //    public CombatActor Character => _character;
        //    GUICmbtTile _gTile;
        //    public GUICmbtTile GUITile => _gTile;

        //    public CombatTile(int row, int col, TargetEnum tileType)
        //    {
        //        _iRow = row;
        //        _iCol = col;
        //        _tileType = tileType;
        //    }

        //    public void SetCombatant(CombatActor c, bool moveCharNow = true)
        //    {
        //        _character = c;
        //        if (_character != null)
        //        {
        //            if (_character.Tile != null)
        //            {
        //                _character.Tile.SetCombatant(null);
        //            }
        //            if(_character.Tile != null) {
        //                foreach (CombatTile tile in GetAdjacent(_character.Tile))
        //                {
        //                    CheckForProtected(tile);
        //                }
        //            }
        //            _character.Tile = this;
        //            CheckForProtected(this);
        //        }

        //        _gTile.SyncGUIObjects(_character != null);
        //        if (_character != null)
        //        {
        //            foreach (KeyValuePair<ConditionEnum, bool> kvp in _character.DiConditions)
        //            {
        //                if (kvp.Value)
        //                {
        //                    GUITile.ChangeCondition(kvp.Key, TargetEnum.Enemy);
        //                }
        //            }
        //        }
        //    }

        //    private void CheckForProtected(CombatTile t)
        //    {
        //        bool found = false;
        //        List<CombatTile> adjacent = GetAdjacent(t);
        //        //foreach (CombatTile tile in adjacent)
        //        //{
        //        //    if (tile.Occupied() && this.TargetType == tile.TargetType)
        //        //    {
        //        //        if (tile.Character != this.Character && tile.Character.IsCombatAdventurer() && this.Character.IsCombatAdventurer())
        //        //        {
        //        //            found = true;
        //        //            ClassedCombatant adv = (ClassedCombatant)tile.Character;
        //        //            adv.Protected = true;
        //        //            adv = (ClassedCombatant)this.Character;
        //        //            adv.Protected = true;
        //        //        }
        //        //    }
        //        //}

        //        //if (!found && this.Character.IsCombatAdventurer())
        //        //{
        //        //    ClassedCombatant adv = (ClassedCombatant)this.Character;
        //        //    adv.Protected = false;
        //        //}
        //    }

        //    public void AssignGUITile(GUICmbtTile c)
        //    {
        //        _gTile = c;
        //    }

        //    public bool Occupied()
        //    {
        //        return _character != null;
        //    }

        //    public void Select(bool val)
        //    {
        //        _bSelected = val;

        //        if (_bSelected && SelectedTile != this)
        //        {
        //            if (SelectedTile != null) { SelectedTile.Select(false); }
        //            SelectedTile = this;
        //        }
        //    }

        //    public void PlayAnimation<TEnum>(TEnum animation)
        //    {
        //        _gTile.PlayAnimation(animation);
        //    }
        //}
        public class ChosenAction
        {
            private Consumable _chosenItem;
            private CombatAction _chosenAction;

            List<RHTile> _liLegalTiles;
            public List<RHTile> LegalTiles => _liLegalTiles;
            public CombatActor User;

            string _name;
            public string Name => _name;

            bool _bDrawItem;

            public ChosenAction(Consumable it)
            {
                User = ActiveCharacter;
                _chosenItem = it;
                _name = _chosenItem.Name;
                _liLegalTiles = new List<RHTile>();

                //Only the adjacent tiles are legal
                _liLegalTiles = User.Tile.GetAdjacent();
            }

            public ChosenAction(CombatAction ca)
            {
                User = ActiveCharacter;
                _chosenAction = ca;
                _name = _chosenAction.Name;

                _chosenAction.SkillUser = ActiveCharacter;

                _liLegalTiles = new List<RHTile>();
                if (IsMelee())
                {
                    _liLegalTiles = User.Tile.GetAdjacent();
                }
                else if (IsRanged())
                {
                    //int col = -1;
                    //int maxCol = MAX_COL;
                    //if (TargetsEnemy()) { col = ENEMY_FRONT; }
                    //else
                    //{
                    //    col = 0;
                    //    maxCol = ENEMY_FRONT;
                    //}

                    //for (; col < maxCol; col++)
                    //{
                    //    for (int row = 0; row < MAX_ROW; row++)
                    //    {
                    //        //_liLegalTiles.Add(_combatMap[row, col]);
                    //    }
                    //}
                }
                else if (SelfOnly())
                {
                    _liLegalTiles.Add(User.Tile);
                }
                else if (Columns())
                {
                    //int startCol = ActiveCharacter.Tile.X;
                    //int endCol = ActiveCharacter.Tile.X;

                    //if(ActiveCharacter.Tile.X > 0) { startCol = ActiveCharacter.Tile.X - 1; }
                    //if (ActiveCharacter.Tile.Col < ALLY_FRONT) { endCol = ActiveCharacter.Tile.Col + 1; }

                    //for (int row = 0; row < MAX_ROW; row++)
                    //{
                    //    for (int col = startCol; col <= endCol; col++)
                    //    {
                    //        if (!_combatMap[row, col].Occupied())
                    //        {
                    //            _liLegalTiles.Add(_combatMap[row, col]);
                    //        }
                    //    }
                    //}
                }
            }

            public void Draw(SpriteBatch spritebatch)
            {
                if (_bDrawItem && _chosenItem != null)     //We want to draw the item above the character's head
                {
                    int size = TileSize * CombatManager.CombatScale;
                    //GUIImage gItem = new GUIImage(_chosenItem.SourceRectangle, size, size, _chosenItem.Texture);
                    //CombatActor c = CombatManager.ActiveCharacter;

                    //gItem.AnchorAndAlignToObject(c.BodySprite, SideEnum.Top, SideEnum.CenterX);
                    //gItem.Draw(spritebatch);
                }
                if (_chosenAction != null && _chosenAction.Sprite != null)
                {
                    _chosenAction.Sprite.Draw(spritebatch);
                }
            }

            public void PerformAction(GameTime gTime)
            {
                if (_chosenAction != null) { _chosenAction.HandlePhase(gTime); }
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
                    _targetTile.Character.ModifyHealth(_chosenItem.Health, false);
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
            public List<RHTile> GetEffectedTiles(){
                List<RHTile> cbtTile = new List<RHTile>();
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
                            //bool monsterSide = (actor.IsCombatAdventurer() && TargetsEnemy()) || (actor.IsMonster() && TargetsAlly());
                            //bool partySide = (actor.IsMonster() && TargetsEnemy()) || (actor.IsCombatAdventurer() && TargetsAlly());

                            ////All we need to do here is select all of the tiles containing the appropriate characters
                            //if (_chosenAction.AreaOfEffect == AreaEffectEnum.Each)
                            //{
                            //    if (monsterSide)
                            //    {
                            //        foreach(Monster m in _liMonsters)
                            //        {
                            //            if (!cbtTile.Contains(m.Tile)) { cbtTile.Add(m.Tile); }
                            //        }
                            //    }
                            //    else
                            //    {
                            //        foreach (CombatActor adv in _listParty)
                            //        {
                            //            if (!cbtTile.Contains(adv.Tile)) { cbtTile.Add(adv.Tile); }
                            //        }
                            //    }
                            //}
                            //else {
                            //    //The coordinates of the selected tile
                            //    int targetRow = SelectedTile.Row;
                            //    int targetCol = SelectedTile.Col;

                            //    //Determines how far to the side the skill can go, based on whether it grows left or right
                            //    int minCol = monsterSide ? ENEMY_FRONT : 0;
                            //    int maxCol = monsterSide ? MAX_COL : ENEMY_FRONT;
                            //    if (_chosenAction.AreaOfEffect == AreaEffectEnum.Cross)
                            //    {
                            //        if (targetRow - 1 >= 0) { cbtTile.Add(_combatMap[targetRow - 1, targetCol]); }
                            //        if (targetRow + 1 < MAX_ROW) { cbtTile.Add(_combatMap[targetRow + 1, targetCol]); }

                            //        if (targetCol - 1 >= minCol) { cbtTile.Add(_combatMap[targetRow, targetCol - 1]); }
                            //        if (targetCol + 1 < maxCol) { cbtTile.Add(_combatMap[targetRow, targetCol + 1]); }
                            //    }
                            //    else if (_chosenAction.AreaOfEffect == AreaEffectEnum.Rectangle)
                            //    {
                            //        KeyValuePair<int, int> dimensions = _chosenAction.Dimensions;
                            //        if (monsterSide)
                            //        {
                            //            for (int rows = targetRow; rows < MAX_ROW && rows < targetRow + dimensions.Key; rows++)
                            //            {
                            //                for (int cols = targetCol; cols < maxCol && cols < targetCol + dimensions.Value; cols++)
                            //                {
                            //                    CombatTile t = _combatMap[rows, cols];
                            //                    if (!cbtTile.Contains(t))
                            //                    {
                            //                        cbtTile.Add(t);
                            //                    }
                            //                }
                            //            }
                            //        }
                            //        else if (partySide)
                            //        {
                            //            for (int rows = targetRow; rows < MAX_ROW && rows < targetRow + dimensions.Key; rows++)
                            //            {
                            //                for (int cols = targetCol; cols >= minCol && cols > targetCol - dimensions.Value; cols--)
                            //                {
                            //                    CombatTile t = _combatMap[rows, cols];
                            //                    if (!cbtTile.Contains(t))
                            //                    {
                            //                        cbtTile.Add(t);
                            //                    }
                            //                }
                            //            }
                            //        }
                            //    }
                            //}
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
            public bool CanTwinCast()
            {
                bool rv = false;
                if(_chosenAction != null){
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
            public List<RHTile> GetTargetTiles()
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
            public void SetTargetTiles(List<RHTile> li)
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
 