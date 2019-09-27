﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Actors;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
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

        private static List<RHTile> _liLegalTiles;
        public static List<RHTile> LegalTiles => _liLegalTiles;

        private static List<RHTile> _liAreaTiles;
        public static List<RHTile> AreaTiles => _liAreaTiles;

        public static CombatActor ActiveCharacter;
        private static List<CombatActor> _liMonsters;
        public static List<CombatActor> Monsters  => _liMonsters;
        private static List<CombatActor> _listParty;
        public static List<CombatActor> Party => _listParty;

        private static CombatScreen _scrCombat;
        public enum PhaseEnum { Setup, Charging, Upkeep, MainSelection, ChooseMoveTarget, Moving, ChooseAction, ChooseActionTarget, PerformAction, DisplayVictory, DisplayDefeat }//NewTurn, EnemyTurn, SelectSkill, ChooseTarget, Defeat, DisplayAttack, DisplayVictory, Lost, PerformAction, EndCombat }
        public static PhaseEnum CurrentPhase;
        public static ChosenAction SelectedAction;

        public static RHTile SelectedTile;          //The tile currently selected by the targetter
        private static RHTile _tTarget;             //The tile that has been confirmed as the target

        public static double Delay;
        public static string Text;

        private static bool _bInCombat = false;
        public static bool InCombat => _bInCombat;

        private static TurnInfo _turnInfo;

        #region Turn Sequence
        static List<CombatActor> _liQueuedCharacters;
        static List<CombatActor> _liChargingCharacters;
        #endregion

        public static void NewBattle()
        {
            CurrentPhase = PhaseEnum.Setup;

            ActiveCharacter = null;
            SelectedAction = null;
            SelectedTile = null;

            Delay = 0;

            _liLegalTiles = new List<RHTile>();
            _liAreaTiles = new List<RHTile>();
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
                c.Tile.SetCombatant(c);
            }

            PlayerManager.World.Tile = MapManager.CurrentMap.GetTileOffGrid(PlayerManager.World.CollisionBox.Center);
            PlayerManager.World.Tile.SetCombatant(PlayerManager.World);

            List<RHTile> tileList = new List<RHTile>();
            RecursivelyGrowRange(PlayerManager.World.Tile, tileList, 0, 2);
            foreach (CombatActor c in Party)
            {
                if(c != PlayerManager.World)
                {
                    MapManager.Maps[c.CurrentMapName].RemoveCharacter(c);
                    c.Activate(true);
                    c.Tile = tileList[random.Next(tileList.Count)];
                    c.Tile.SetCombatant(c);
                    c.CurrentMapName = MapManager.CurrentMap.Name;
                    MapManager.CurrentMap.AddCharacter(c);
                    c.Position = c.Tile.Position;
                }   
            }

            _scrCombat = new CombatScreen();
            GUIManager.SetScreen(_scrCombat);

            _bInCombat = true;
            PlayerManager.AllowMovement = false;
            PlayerManager.World.Idle();

            PlayerManager.World.SetMoveObj(Util.SnapToGrid(PlayerManager.World.Tile.Center));
        }

        public static void Update(GameTime gTime)
        {
            switch (CurrentPhase)
            {
                //This phase starts combat and ensures that the Actors are locked to their tile.
                case PhaseEnum.Setup:
                    if(PlayerManager.World.Position == PlayerManager.World.Tile.Position)
                    {
                        PlayerManager.World.Idle();
                        CurrentPhase = PhaseEnum.Charging;
                    }
                    break;

                //For when there is no character so we must charge until someone hits 100
                case PhaseEnum.Charging:
                    //If there is no ActiveCharacter, we need to get the next one
                    if (ActiveCharacter == null)
                    {
                        //If no characters are queued for their next turn,
                        //we need to tick the combat
                        if (_liQueuedCharacters.Count == 0)
                        {
                            CombatTick(ref _liChargingCharacters, ref _liQueuedCharacters);
                        }
                        else
                        {
                            //If there is at least one character, retrieve it
                            GetActiveCharacter();
                            if (ActiveCharacter.IsAdventurer())
                            {
                                Camera.SetObserver(ActiveCharacter);
                            }
                        }
                    }
                    break;

                //Phase for when a new ActiveCharacter is set but before they can do anything
                case PhaseEnum.Upkeep:
                    _turnInfo = new TurnInfo();
                    ActiveCharacter.TickStatusEffects();
                    if (ActiveCharacter.Poisoned())
                    {
                        ActiveCharacter.ModifyHealth(Math.Max(1, (int)(ActiveCharacter.MaxHP / 20)), true);
                    }

                    Summon activeSummon = ActiveCharacter.LinkedSummon;
                    if (activeSummon == null || !activeSummon.Regen)
                    {
                        GoToMainSelection();
                    }
                    else if (activeSummon != null && activeSummon.Regen && activeSummon.BodySprite.CurrentAnimation != "Cast")
                    {
                        activeSummon.PlayAnimation(CActorAnimEnum.Cast);
                    }
                    else if (activeSummon.BodySprite.GetPlayCount() >= 1)
                    {
                        activeSummon.PlayAnimation(CActorAnimEnum.Idle);
                        ActiveCharacter.ModifyHealth(30, false);
                        GoToMainSelection();
                    }
                    break;

                case CombatManager.PhaseEnum.ChooseActionTarget:
                    HandleTargetting();
                    break;

                case CombatManager.PhaseEnum.ChooseMoveTarget:
                    HandleTargetting();
                    break;

                //Use this phase for when the ActiveCharacter is currently moving
                case PhaseEnum.Moving:
                    if (!ActiveCharacter.FollowingPath)
                    {
                        ActiveCharacter.Tile.SetCombatant(null);
                        ActiveCharacter.Tile = MapManager.CurrentMap.GetTileOffGrid(ActiveCharacter.CollisionBox.Center);
                        ActiveCharacter.Tile.SetCombatant(ActiveCharacter);
                        //GoToMainSelection();
                        _turnInfo.HasMoved = true;

                        EndTurn();
                        //if (ActiveCharacter.IsMonster()) { }
                    }
                    break;

                //Use this phase to use the SelectedAction
                case PhaseEnum.PerformAction:
                    if (SelectedAction != null)
                    {
                        SelectedAction.Update(gTime);
                    }

                    break;

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
            }
        }

        private static void HandleTargetting()
        {
            HandleMouseTargetting();
            HandleKeyboardTargetting();

            //Cancel out of selections made if escape is hit
            if (InputManager.CheckPressedKey(Keys.Escape))
            {
                _scrCombat.CancelAction();

                ClearSelectedTile();
                SelectedAction = null;
                if (CurrentPhase == PhaseEnum.ChooseAction || CurrentPhase == PhaseEnum.ChooseMoveTarget) {
                    CurrentPhase = PhaseEnum.MainSelection;
                    _scrCombat.OpenMainSelection();
                }
                else if(CurrentPhase == PhaseEnum.ChooseActionTarget)
                {
                    CurrentPhase = PhaseEnum.ChooseAction;
                }
            }
        }

        #region CombatScreen Controls
        /// <summary>
        /// Switches the phase to MainSelection and tells the CombatScree
        /// to open a MainSelectionWindow if one is not yet up.
        /// </summary>
        public static void GoToMainSelection()
        {
            if (ActiveCharacter.IsMonster())
            {
                EnemyTakeTurn();
            }
            else
            {
                CurrentPhase = PhaseEnum.MainSelection;
                _scrCombat.OpenMainSelection();
            }
        }
        #endregion

        private static void GiveXP()
        {
            //int toGive = 0;
            //int total = 0;
            //CurrentMob.GetXP(ref toGive, ref total);

            //int xpDrain = 5;
            //if (toGive > 0)
            //{
            //    CurrentMob.DrainXP(xpDrain);
            //    foreach (ClassedCombatant a in _listParty)
            //    {
            //        a.AddXP(xpDrain);
            //    }
            //}
        }
        internal static bool CanCancel()
        {
            return CurrentPhase == PhaseEnum.ChooseAction || CurrentPhase == PhaseEnum.ChooseActionTarget || CurrentPhase == PhaseEnum.ChooseMoveTarget;
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
               // CurrentPhase = PhaseEnum.Defeat;
            }

            return rv;
        }

        public static void EndCombatVictory()
        {
            GUIManager.BeginFadeOut();
            //MapManager.DropItemsOnMap(DropManager.DropItemsFromMob(_mob.ID), _mob.CollisionBox.Center.ToVector2());
           // MapManager.RemoveMob(_mob);
            GoToWorldMap();
        }

        public static void EndCombatEscape()
        {
            GUIManager.BeginFadeOut();
            //_mob.Stun();
            GoToWorldMap();
        }

        public static void ProcessActionChoice(CombatAction a)
        {
            SelectedAction = new ChosenAction(a);

            if (SelectedAction.Name.Equals("Move"))
            {
                CurrentPhase = PhaseEnum.ChooseMoveTarget;
                FindAndHighlightLegalTiles();
            }
            else
            {
                CurrentPhase = PhaseEnum.ChooseActionTarget;
                FindAndHighlightLegalTiles();
            }
 
            //if (!SelectedAction.SelfOnly())
            //{
            //    if (!ActiveCharacter.IsMonster()) {
            //        CurrentPhase = PhaseEnum.ChooseTarget;
            //    }  //Skips this phase for enemies. They don't "choose" targets
            //}
            //else
            //{
            //    CurrentPhase = PhaseEnum.DisplayAttack;
            //    Text = SelectedAction.Name;
            //}
        }

        public static void ProcessItemChoice(Consumable it)
        {
            CurrentPhase = PhaseEnum.ChooseActionTarget;
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

        #region Enemy AI

        //For now, when the enemy takes their turn, have them select a random party member
        //When enemies get healing/defensive skills, they'll have their own logic to process
        public static void EnemyTakeTurn()
        {
            bool moveToPlayer = false;
            RHTile targetTile = null;
            RHRandom r = new RHRandom();
            CombatAction action = null;
            bool gottaMove = true;

            //Step one determine if we are in range of a target
            foreach (CombatAction c in ActiveCharacter.AbilityList)
            {
                if (c.Range == 1)
                {
                    foreach (RHTile t in ActiveCharacter.Tile.GetAdjacent())
                    {
                        if (t.HasCombatant() && t.Character.IsAdventurer())
                        {
                            gottaMove = false;
                            action = c;
                            targetTile = t;
                            break;
                        }
                    }
                    if (gottaMove)
                    {
                        moveToPlayer = true;
                    }
                }
            }

            //If we have found a target to attack, but we need to move to get into range of them
            //Then find the shortest path, and move as close as you can.
            if (!_turnInfo.HasMoved && gottaMove && !ActiveCharacter.FollowingPath)
            {
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

                    SelectedTile = MapManager.CurrentMap.GetTileOffGrid(closest);

                    //Need to unset the Combatant from the tile the monster is moving to so that
                    //we can pathfind to it
                    CombatActor act = SelectedTile.Character;
                    SelectedTile.SetCombatant(null);

                    //Determine the pathfinding for the Monster
                    SetMoveTarget();

                    //Reset the CombatActor's RHTile
                    MapManager.CurrentMap.GetTileOffGrid(closest).SetCombatant(act);
                }
            }

            //If we have not yet acted, we need to move, and we are not following a path
            if (!_turnInfo.HasActed && !gottaMove && !ActiveCharacter.FollowingPath)
            {
                foreach (RHTile t in ActiveCharacter.Tile.GetAdjacent())
                {
                    if (t.HasCombatant() && t.Character.IsAdventurer())
                    {
                        SelectedTile = t;
                        SelectedAction = new ChosenAction((CombatAction)ActiveCharacter.AbilityList[0]);
                        SelectedAction.AssignTarget();
                    }
                }
            }
        }

        /// <summary>
        /// Have the enemy iterate over it's skills and determine if
        /// there are any targets in range.
        /// </summary>
        private static void EnemyCheckforTargets()
        {
            foreach (MenuAction action in ActiveCharacter.AbilityList) {

            }
        }
        #endregion

        #region Tile Handling
        /// <summary>
        /// Highlight all RHTiles in the LegalTiles list.
        /// </summary>
        public static void FindAndHighlightLegalTiles()
        {
            int distance = (CurrentPhase == PhaseEnum.ChooseMoveTarget ? 5 : SelectedAction.Range);
            RecursivelyGrowRange(ActiveCharacter.Tile, _liLegalTiles, 0, distance);

            foreach (RHTile t in _liLegalTiles)
            {
                t.LegalTile(true);
            }
        }

        /// <summary>
        /// Call this method to determine the range of the skill by taking adjacent RHTiles from the start tile
        /// and iterating a specified number of times in "growth rings"
        /// </summary>
        /// <param name="startTile">The tile to grab adjacent tiles of</param>
        /// <param name="depth">How many "growth rings" we are in</param>
        /// <param name="tileList">The list of RHTiles to add to</param>
        /// <param name="maxDepth">The maximium depth to go for</param>
        private static void RecursivelyGrowRange(RHTile startTile, List<RHTile> tileList, int depth, int maxDepth)
        {
            //If we haven't exceeded the maxDepth, discover the adhacent tiles
            if (depth < maxDepth)
            {
                depth++;
                foreach (RHTile t in startTile.GetAdjacent())
                {
                    //The tile is legal if we are choosing a target for an action; or if we are moving, it is passable, and there is no character
                    //If so, add it to the LegalTiles list and then recursively grow.
                    if ((t.Passable() && t.CanPathThroughInCombat() && (CurrentPhase == PhaseEnum.ChooseMoveTarget || CurrentPhase == PhaseEnum.Setup)) || CurrentPhase == PhaseEnum.ChooseActionTarget)
                    {
                        //Can never target walls or otherwise blocked tiles.
                        if (!tileList.Contains(t) && t.Passable()) { tileList.Add(t); }
                        RecursivelyGrowRange(t, tileList, depth, maxDepth);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the given RHTile as the SelectedTile and unsets the
        /// previous SelectedTile if it exists.
        /// </summary>
        /// <param name="tile">New tile to select</param>
        public static void SelectTile(RHTile tile)
        {
            tile.Select(true);

            if (SelectedTile != tile)
            {
                if (SelectedTile != null) { SelectedTile.Select(false); }
                SelectedTile = tile;
            }

            if (CurrentPhase == PhaseEnum.ChooseActionTarget)
            {
                ClearAreaTiles();

                RecursivelyGrowRange(SelectedTile, _liAreaTiles, 0, SelectedAction.AreaOfEffect());
                foreach (RHTile t in _liAreaTiles)
                {
                    t.AreaTile(true);
                }
            }
        }

        /// <summary>
        /// Clears the Legal, Selected, and Area tile lists and unsets the flags
        /// from the given RHTiles
        /// </summary>
        public static void ClearAllTiles()
        {
            ClearLegalTiles();
            ClearSelectedTile();
            ClearAreaTiles();
        }

        /// <summary>
        /// Set each Legal Tile to be illegal and then Clear the list
        /// </summary>
        public static void ClearLegalTiles()
        {
            foreach (RHTile t in _liLegalTiles)
            {
                t.LegalTile(false);
            }
            _liLegalTiles.Clear();
        }

        /// <summary>
        /// Unsets the SelectedTile
        /// </summary>
        public static void ClearSelectedTile()
        {
            if (SelectedTile != null) { SelectedTile.Select(false); }
            SelectedTile = null;
        }

        /// <summary>
        /// Unsets all Area Tiles and then clears the list
        /// </summary>
        public static void ClearAreaTiles()
        {
            foreach (RHTile t in AreaTiles)
            {
                t.AreaTile(false);
            }
            _liAreaTiles.Clear();
        }
        #endregion

        #region SelectionHandling   
        /// <summary>
        /// Determines whether or not the given tile has been selected while hovering over it
        /// </summary>
        /// <param name="tile">The tile to test against</param>
        //public static void TestHoverTile(RHTile tile)
        //{
        //    if (SelectedAction.LegalTiles.Contains(tile) && (tile.HasCombatant() || SelectedAction.AreaOfEffect()))
        //    {
        //        tile.Select(true);
        //    }
        //}
        public static void HandleKeyboardTargetting()
        {
            RHTile temp = SelectedTile;
            if (temp == null)
            {
                temp = ActiveCharacter.Tile;
            }

            if (InputManager.CheckPressedKey(Keys.A))
            {
                temp = temp.GetTileByDirection(DirectionEnum.Left);
            }
            else if (InputManager.CheckPressedKey(Keys.D))
            {
                temp = temp.GetTileByDirection(DirectionEnum.Right);
            }
            else if (InputManager.CheckPressedKey(Keys.W))
            {
               temp = temp.GetTileByDirection(DirectionEnum.Up);
            }
            else if (InputManager.CheckPressedKey(Keys.S))
            {
               temp = temp.GetTileByDirection(DirectionEnum.Down);
            }

            if (temp != null && CombatManager.LegalTiles.Contains(temp) && temp != ActiveCharacter.Tile)
            {
                SelectTile(temp);
            }

            if (InputManager.CheckPressedKey(Keys.Enter))
            {
                if (CurrentPhase == PhaseEnum.ChooseMoveTarget)
                {
                    CombatManager.SetMoveTarget();
                }
                else if (CurrentPhase == PhaseEnum.ChooseActionTarget)
                {
                    CombatManager.SelectedAction.AssignTarget();
                }
            }
        }
        public static void HandleMouseTargetting()
        {
            Vector2 mouseCursor = GraphicCursor.GetWorldMousePosition();
            RHTile tile = MapManager.CurrentMap.GetTileOffGrid(mouseCursor);
            if (tile != null && _liLegalTiles.Contains(tile))
            {
                SelectTile(tile);
            }
        }

        /// <summary>
        /// If there is a SelectedTile, officially set the targeted tile
        /// and change the turn phase to Moving.
        /// 
        /// Then perform pathfinding to the tile, and prepare to move
        /// </summary>
        public static void SetMoveTarget()
        {
            if (SelectedTile != null)
            {
                _tTarget = SelectedTile;
                CurrentPhase = PhaseEnum.Moving;
                Vector2 start = ActiveCharacter.Position;

                List<RHTile> tilePath = TravelManager.FindPathToLocation(ref start, _tTarget.Center, MapManager.CurrentMap.Name);

                //If a Monster is going to move, we need to either prune it down
                //or remove the last tile so they don't actually try to step into the player's tile
                if (ActiveCharacter.IsMonster())
                {
                    if (tilePath.Count > 5) { tilePath.RemoveRange(5, tilePath.Count - 5); }
                    else { tilePath.RemoveAt(tilePath.Count - 1); }
                }

                ActiveCharacter.SetPath(tilePath);
                TravelManager.ClearPathingTracks(); //Clean up after our pathfinding
                ClearToPerformAction();
            }
        }

        /// <summary>
        /// Closes any open selection windows and clears
        /// the legal and selected tiles
        /// </summary>
        private static void ClearToPerformAction()
        {
            _scrCombat.CloseMainSelection();
            ClearAllTiles();
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

        public static List<CombatActor> CalculateTurnOrder(int maxShown)
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

        /// <summary>
        /// We need to get the first CombatActor from the list of queued characters.
        /// 
        /// Do not retrieve any queued characters who now have 0 HP.
        /// If the first character has 0 HP, call this method again.
        /// </summary>
        private static void GetActiveCharacter()
        {
            if (_liQueuedCharacters[0].CurrentHP > 0) {
                ActiveCharacter = _liQueuedCharacters[0];
                _liQueuedCharacters.RemoveAt(0);

                //Re-add the character to the charge list and sort by speed
                _liChargingCharacters.Add(ActiveCharacter);
                _liChargingCharacters.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));

                //Go into Upkeep phase
                CurrentPhase = PhaseEnum.Upkeep;
            }
            else
            {
                //If the character has 0 HP, remove them from the queue
                //and try again if anyone else is queued.
                _liQueuedCharacters.RemoveAt(0);
                if (_liQueuedCharacters.Count > 0)
                {
                    GetActiveCharacter();
                }
            }
        }

        public static void EndTurn()
        {
            if (SelectedAction != null)
            {
                ActiveCharacter.CurrentCharge -= SelectedAction.ChargeCost();
            }
            else
            {
                ActiveCharacter.CurrentCharge = 0;
            }

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
            if (SelectedAction != null)
            {
                SelectedAction.Clear();
            }

            //if (CurrentPhase != PhaseEnum.EndCombat)
            //{
                SelectedAction = null;
                ActiveCharacter = null;
                CurrentPhase = PhaseEnum.Charging;
            //}
        }
        #endregion

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
            public int Range => (_chosenItem != null ? 1 : _chosenAction.Range);    //Items only have 1 tile of range
            private Consumable _chosenItem;
            private CombatAction _chosenAction;
            public AnimatedSprite Sprite => _chosenAction.Sprite;

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
                //else if (Columns())
                //{
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
                //}
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

            public void Update(GameTime gTime)
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
                        _tTarget.Character.ChangeConditionStatus(_chosenItem.Condition, !_chosenItem.Helpful);
                    }
                    _tTarget.Character.ModifyHealth(_chosenItem.Health, false);
                    _chosenItem.Remove(1);
                }

                EndTurn();
            }

            /// <summary>
            /// Call to officially lock the SelectedTile as the _targetTile
            /// Clear out the tiles and close the window
            /// </summary>
            public void AssignTarget()
            {
                _tTarget = SelectedTile;
                if (_chosenAction != null)
                {
                    ActiveCharacter.CurrentMP -= _chosenAction.MPCost;          //Checked before Processing
                    _chosenAction.AssignTiles();
                    Text = SelectedAction.Name;
                }
                else if (_chosenItem != null)
                {
                    Text = SelectedAction.Name;
                }
                CombatManager.CurrentPhase = PhaseEnum.PerformAction;

                ClearToPerformAction();
            }

            public int ChargeCost()
            {
                int rv = 100;

                if(_chosenAction != null) { rv = _chosenAction.ChargeCost; }

                return rv;
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
            public bool SelfOnly() { return _chosenAction.Range == 0; }
            public bool IsMelee() { return _chosenAction.Range == 1; }
            public bool IsRanged() { return _chosenAction.Range > 1; }
            public bool SingleTarget()
            {
                bool rv = false;
                if (_chosenAction != null) { rv = _chosenAction.AreaOfEffect > 0; }
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

            /// <summary>
            /// Returns the area of effect of the chosen action.
            /// Items have a range of 1
            /// </summary>
            /// <returns></returns>
            public int AreaOfEffect()
            {
                int rv = 1;

                if (_chosenAction != null) {
                    rv = _chosenAction.AreaOfEffect;
                }

                return rv;
            }

            public void Clear()
            {
                if(_chosenAction != null) { _chosenAction.TileTargetList.Clear(); }
                else if (_chosenItem != null) { _tTarget = null; }
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
                    _chosenAction.AssignTiles();
                    _chosenAction.TileTargetList = li;
                }
            }
        }

        /// <summary>
        /// Used to store info about the ActiveCharacters turn.
        /// 
        /// For now, only used to store whether thet havemoved or acted.
        /// </summary>
        public struct TurnInfo
        {
            public bool HasMoved;
            public bool HasActed;
        }
    }
}
 