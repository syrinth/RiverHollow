﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.BitmapFonts;
using RiverHollow.Actors;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Characters;
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
        private const double EXP_MULTIPLIER_BONUS = 0.3;
        public const int BASIC_ATTACK = 300;
        public static int CombatScale = 5;

        private static List<Item> _liDroppedItems;
        private static List<RHTile> _liLegalTiles;
        public static List<RHTile> LegalTiles => _liLegalTiles;

        private static List<RHTile> _liAreaTiles;
        public static List<RHTile> AreaTiles => _liAreaTiles;

        public static CombatActor ActiveCharacter;
        private static List<CombatActor> _liMonsters;
        public static List<CombatActor> Monsters  => _liMonsters;
        private static List<CombatActor> _liParty;
        public static List<CombatActor> Party => _liParty;

        private static CombatScreen _scrCombat;
        public enum PhaseEnum { Setup, Charging, Upkeep, MainSelection, ChooseMoveTarget, Moving, ChooseAction, ChooseActionTarget, PerformAction, Victory, DisplayDefeat }//NewTurn, EnemyTurn, SelectSkill, ChooseTarget, Defeat, DisplayAttack, DisplayVictory, Lost, PerformAction, EndCombat }
        public static PhaseEnum CurrentPhase;
        public static ChosenAction SelectedAction;

        public static RHTile SelectedTile;          //The tile currently selected by the targetter
        private static RHTile _tTarget;             //The tile that has been confirmed as the target

        public static double Delay;
        public static string Text;

        private static bool _bInCombat = false;
        public static bool InCombat => _bInCombat;

        private static int _iXPMultiplier = 0;

        public static TurnInfo CurrentTurnInfo;

        #region Turn Sequence
        static List<CombatActor> _liQueuedCharacters;
        static List<CombatActor> _liChargingCharacters;
        #endregion

        public static bool CheckForEndTurn()
        {
            bool rv = false;
            if(CurrentTurnInfo.HasActed && CurrentTurnInfo.HasMoved)
            {
                rv = true;
                EndTurn();
            }

            return rv;
        }

        public static void NewBattle(string oldMap)
        {
            ChangePhase(PhaseEnum.Setup);

            ActiveCharacter = null;
            SelectedAction = null;
            SelectedTile = null;

            Delay = 0;

            _liDroppedItems = new List<Item>();
            _liLegalTiles = new List<RHTile>();
            _liAreaTiles = new List<RHTile>();
            _liParty = new List<CombatActor>();
            _liParty.AddRange(PlayerManager.GetParty());

            _liMonsters = new List<CombatActor>();
            _liMonsters.AddRange(MapManager.CurrentMap.Monsters);

            _liQueuedCharacters = new List<CombatActor>();
            _liChargingCharacters = new List<CombatActor>();
            _liChargingCharacters.AddRange(_liParty);
            _liChargingCharacters.AddRange(_liMonsters);

            //Characters with higher Spd go first
            _liChargingCharacters.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));

            foreach(CombatActor c in _liChargingCharacters)
            {
                c.CurrentCharge = RHRandom.Instance.Next(0, 50);
            }

            foreach (CombatActor c in _liMonsters)
            {
                c.SetBaseTile(MapManager.CurrentMap.GetTileByPixelPosition(c.Position));
            }

            RHTile[,] tiles = MapManager.CurrentMap.DictionaryCombatTiles[oldMap];
            foreach (ClassedCombatant c in Party)
            {
                if (c != PlayerManager.World)
                {
                    MapManager.Maps[c.CurrentMapName].RemoveCharacter(c);
                    c.Activate(true);
                    c.CurrentMapName = MapManager.CurrentMap.Name;
                    MapManager.CurrentMap.AddCharacter(c);
                }

                Vector2 startpos = c.StartPosition;
                c.SetBaseTile(tiles[(int)startpos.X, (int)startpos.Y]);
                c.Position = c.BaseTile.Position;
                c.Facing = PlayerManager.World.Facing;
                c.PlayAnimation(VerbEnum.Walk);
            }


            _scrCombat = new CombatScreen();
            GUIManager.SetScreen(_scrCombat);

            _bInCombat = true;
            PlayerManager.AllowMovement = false;
            PlayerManager.World.PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);

            PlayerManager.World.SetMoveObj(Util.SnapToGrid(PlayerManager.World.BaseTile.Center));
        }

        public static void Update(GameTime gTime)
        {
            switch (CurrentPhase)
            {
                //This phase starts combat and ensures that the Actors are locked to their tile.
                case PhaseEnum.Setup:
                    if(PlayerManager.World.Position == PlayerManager.World.BaseTile.Position)
                    {
                        PlayerManager.World.PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
                        ChangePhase(PhaseEnum.Charging);
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
                            Camera.SetObserver(ActiveCharacter, true);
                        }
                    }
                    break;

                //Phase for when a new ActiveCharacter is set but before they can do anything
                case PhaseEnum.Upkeep:
                    CurrentTurnInfo = new TurnInfo();
                    ActiveCharacter.TickStatusEffects();
                    if (ActiveCharacter.Poisoned())
                    {
                        ActiveCharacter.ModifyHealth(Math.Max(1, (int)(ActiveCharacter.MaxHP / 20)), true);
                    }

                    Summon activeSummon = ActiveCharacter.LinkedSummon;
                    if ((activeSummon == null || !activeSummon.Regen))
                    {
                        if (!Camera.IsMoving())
                        {
                            GoToMainSelection();
                        }
                    }
                    else if (activeSummon != null && activeSummon.Regen && activeSummon.BodySprite.CurrentAnimation != "Cast")
                    {
                        activeSummon.PlayAnimation(VerbEnum.Cast);
                    }
                    else if (activeSummon.BodySprite.GetPlayCount() >= 1)
                    {
                        activeSummon.PlayAnimation(VerbEnum.Idle);
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
                        RHTile newTile = MapManager.CurrentMap.GetTileByPixelPosition(ActiveCharacter.Position);
                        ActiveCharacter.SetBaseTile(newTile);

                        Item tileItem = _liDroppedItems.Find(item => newTile.Rect.Contains(item.Position));

                        if(tileItem != null && InventoryManager.HasSpaceInInventory(tileItem.ItemID, tileItem.Number))
                        {
                            MapManager.CurrentMap.AddItemToPlayerInventory(tileItem);
                        }
                        CurrentTurnInfo.HasMoved = true;

                        if (!CheckForEndTurn())
                        {
                            if (ActiveCharacter.IsMonster())
                            {
                                if (SelectedAction != null)
                                {
                                    CombatManager.ChangePhase(PhaseEnum.PerformAction);
                                }
                                else { EndTurn(); }
                            }
                            else if (ActiveCharacter.IsAdventurer())
                            {
                                GoToMainSelection();
                            }
                        }
                        //if (!ActiveCharacter.IsMonster() || SelectedAction == null)
                        //{
                        //    EndTurn();
                        //}
                        //else
                        //{
                        //    CombatManager.ChangePhase(PhaseEnum.PerformAction);
                        //}
                    }
                    break;

                //Use this phase to use the SelectedAction
                case PhaseEnum.PerformAction:
                    if (SelectedAction != null)
                    {
                        SelectedAction.Update(gTime);
                    }

                    break;

                case PhaseEnum.Victory:
                    if(_liMonsters?.Count == 0 && !_scrCombat.AreThereFloatingText())
                    {
                        _bInCombat = false;
                        EndCombatVictory();
                    }
                    break;
            }
        }
        
        /// <summary>
        /// Controlled for changing phases.
        /// 
        /// If we are on the player's turn and the active characteris moving or performing their action
        /// we want to unpause the game. Otherwise, the game should be paused.
        /// </summary>
        /// <param name="newPhase"></param>
        public static void ChangePhase(PhaseEnum newPhase)
        { 
            if(newPhase == PhaseEnum.PerformAction || newPhase == PhaseEnum.Moving)
            {
                if (ActiveCharacter != null && ActiveCharacter.IsAdventurer())
                {
                    GameManager.Unpause();
                }
            }
            else { GameManager.Pause(); }

            CurrentPhase = newPhase;
        }

        public static void DrawUpperCombatLayer(SpriteBatch spriteBatch)
        {
            if (_bInCombat)
            {
                _scrCombat.DrawUpperCombatLayer(spriteBatch);
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
                    ChangePhase(PhaseEnum.MainSelection);
                    _scrCombat.OpenMainSelection();
                }
                else if(CurrentPhase == PhaseEnum.ChooseActionTarget)
                {
                    ChangePhase(PhaseEnum.ChooseAction);
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
            ChangePhase(PhaseEnum.MainSelection);
            if (ActiveCharacter.IsMonster())
            {
                ((Monster)ActiveCharacter).TakeTurn();
            }
            else
            {
                _scrCombat.OpenMainSelection();
            }
        }
        #endregion

        internal static bool CanCancel()
        {
            return CurrentPhase == PhaseEnum.ChooseAction || CurrentPhase == PhaseEnum.ChooseActionTarget || CurrentPhase == PhaseEnum.ChooseMoveTarget;
        }

        #region End Of Comabt
        /// <summary>
        /// Called at the End of a Turn to see if the combat has been won or lost.
        /// </summary>
        /// <returns>True is combat is ending</returns>
        private static bool EndTurnOfCombatOverCheck()
        {
            bool rv = false;

            //Lambda expressions to find all characters still standing
            bool partyUp = _liParty.FindAll(actor => actor.CurrentHP > 0 ).Count > 0;
            bool monstersUp = _liMonsters.FindAll(actor => actor.CurrentHP > 0).Count > 0; ;

            //If there is at least one party member up and no monsters, go to Victory
            if (partyUp && !monstersUp)
            {
                rv = true;
                ChangePhase(PhaseEnum.Victory);
                foreach (ClassedCombatant a in _liParty)
                {
                    a.CurrentCharge = 0;
                    a.PlayAnimation(AnimationEnum.Win);
                }

            }
            else if (!partyUp)
            {
                rv = true;
                Unpause();
                //ChangePhase(PhaseEnum.Defeat);
            }

            return rv;
        }

        /// <summary>
        /// Called when the Battle ends in Victory.
        /// We need to re-enable movement as it's locked down in combat
        /// </summary>
        public static void EndCombatVictory()
        {
            Unpause();
            PlayerManager.AllowMovement = true;
            foreach (Item it in _liDroppedItems) { it.AutoPickup = true; }

            GoToWorldMap();
        }

        /// <summary>
        /// Called when the Battle ends in escape.
        /// We need to re-enable movement as it's locked down in combat
        /// </summary>
        public static void EndCombatEscape()
        {
            Unpause();
            PlayerManager.AllowMovement = true;
            GoToWorldMap();
        }
        #endregion

        public static void ProcessActionChoice(CombatAction a)
        {
            SelectedAction = new ChosenAction(a);
            ChangePhase(PhaseEnum.ChooseActionTarget);
            FindAndHighlightLegalTiles();

            //if (!SelectedAction.SelfOnly())
            //{
            //    if (!ActiveCharacter.IsMonster()) {
            //        ChangePhase(PhaseEnum.ChooseTarget);
            //    }  //Skips this phase for enemies. They don't "choose" targets
            //}
            //else
            //{
            //    ChangePhase(PhaseEnum.DisplayAttack);
            //    Text = SelectedAction.Name;
            //}
        }

        public static void ProcessItemChoice(Consumable it)
        {
            ChangePhase(PhaseEnum.ChooseActionTarget);
            SelectedAction = new ChosenAction(it);
        }

        public static void AssignMonsterAction(CombatAction a, RHTile targetTile)
        {
            SelectedAction = new ChosenAction(a);
            SelectedTile = targetTile;
            SelectedAction.AssignTarget();
        }

        /// <summary>
        /// Called to give the party experience points for killing a monster
        /// </summary>
        /// <param name="m">The monster that has been defeated</param>
        public static void GiveXP(Monster m)
        {
            //Calculates the total XP based off of the XP multiplier and then add the floating text
            double xpToGive = m.XP * (1 + (double)(EXP_MULTIPLIER_BONUS * _iXPMultiplier++));
            AddFloatingText(new FloatingText(m.Position, m.SpriteWidth, string.Format("{0} XP", xpToGive), Color.Yellow));

            //Give the XP to the party
            foreach (ClassedCombatant c in PlayerManager.GetParty())
            {
                int startLevel = c.ClassLevel;
                c.AddXP((int)xpToGive);

                if(c.ClassLevel > startLevel) {
                    AddFloatingText(new FloatingText(c.Position, c.SpriteWidth, "LEVEL UP", Color.White));
                    c.PlayAnimation(AnimationEnum.Win);

                    CombatAction newAction = c.GetCurrentSpecials().Find(action => action.ReqLevel > startLevel && action.ReqLevel <= c.ClassLevel);
                    if (newAction != null)
                    {
                        _scrCombat.OpenTextWindow(string.Format("Learned new skill {0}", newAction.Name));
                    }
                }
            }
        }

        /// <summary>
        /// Perform any actions required of the CombatManager on a KO'd Actor.
        /// </summary>
        /// <param name="c">The KO'd Actor</param>
        public static void RemoveKnockedOutCharacter(CombatActor c)
        {
            //If the Actor was a Monster, remove it from the list
            if (_liMonsters.Contains((c)))
            {
                _liMonsters.Remove(c);
                c.ClearTiles();
            }

            //Remove the Actor from the turn order 
            _liChargingCharacters.Remove(c);                    
            _liQueuedCharacters.Remove(c);
        }

        /// <summary>
        /// Perform any actions that need to happen once the monster
        /// has finished playing it's death animation
        /// </summary>
        /// <param name="c">The defeated Monster</param>
        public static void MonsterKOAnimFinished(Monster m)
        {
            PlayerManager.AddMonsterEnergyToQueue(100);
            MapManager.RemoveMonster(m);
            _liDroppedItems.Add(DropManager.DropMonsterLoot(m));
        }

        #region Enemy AI

        //For now, when the enemy takes their turn, have them select a random party member
        //When enemies get healing/defensive skills, they'll have their own logic to process
        public static void EnemyTakeTurn()
        {
            RHTile targetTile = null;

            bool gottaMove = true;

            //Step one determine if we are in range of a target
            foreach (CombatAction c in ActiveCharacter.GetCurrentSpecials())
            {
                if (c.Range == 1)
                {
                    foreach (RHTile t in ActiveCharacter.BaseTile.GetAdjacentTiles())
                    {
                        if (t.HasCombatant() && t.Character.IsAdventurer())
                        {
                            gottaMove = false;
                            SelectedAction = new ChosenAction(c);
                            targetTile = t;
                            break;
                        }
                    }
                }
            }

            //If we have found a target to attack, but we need to move to get into range of them
            //Then find the shortest path, and move as close as you can.
            if (!CurrentTurnInfo.HasMoved && gottaMove && !ActiveCharacter.FollowingPath)
            {
                if (targetTile == null)
                {
                    double closestDistance = 0;
                    Vector2 start = ActiveCharacter.BaseTile.Center;
                    Vector2 closest = Vector2.Zero;

                    foreach (CombatActor actor in Party)
                    {
                        Vector2 target = actor.BaseTile.Center;

                        int deltaX = (int)Math.Abs(start.X - target.X);
                        int deltaY = (int)Math.Abs(start.Y - target.Y);

                        double distance = Math.Sqrt(deltaX ^ 2 + deltaY ^ 2);
                        if (distance < closestDistance || closestDistance == 0)
                        {
                            closest = target;
                            closestDistance = distance;
                        }
                    }

                    SelectedTile = MapManager.CurrentMap.GetTileByPixelPosition(closest);

                    //Need to unset the Combatant from the tile the monster is moving to so that
                    //we can pathfind to it
                    CombatActor act = SelectedTile.Character;
                    act.ClearTiles();

                    //Determine the pathfinding for the Monster
                    SetMoveTarget();

                    //Reset the CombatActor's RHTile
                    MapManager.CurrentMap.GetTileByPixelPosition(closest).SetCombatant(act);
                }
            }

            //If we have not yet acted, we need to move, and we are not following a path
            if (!CurrentTurnInfo.HasActed && !gottaMove && !ActiveCharacter.FollowingPath)
            {
                foreach (RHTile t in ActiveCharacter.BaseTile.GetAdjacentTiles())
                {
                    if (t.HasCombatant() && t.Character.IsAdventurer())
                    {
                        SelectedTile = t;
                        SelectedAction = new ChosenAction((CombatAction)ActiveCharacter.GetCurrentSpecials()[0]);
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
            bool isMoving = CurrentPhase == PhaseEnum.ChooseMoveTarget;
            int distance = (isMoving ? 5 : SelectedAction.Range);
            foreach(RHTile tile in TravelManager.FindRangeOfAction(ActiveCharacter, distance, isMoving).Keys)
            {
                _liLegalTiles.Add(tile);
            } 

            foreach (RHTile t in _liLegalTiles)
            {
                t.LegalTile(true);
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
                _liAreaTiles = SelectedAction.DetermineTargetTiles(tile);
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
        /// Unsets the CurrentTile
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

        /// <summary>
        /// Compares a Character against the ActiveCharacter to see if
        /// they are on the same team.
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        public static bool OnSameTeam(CombatActor actor)
        {
            if (actor.IsMonster())
            {
                int i = 0;
                i++;
            }
            return (ActiveCharacter.IsAdventurer() && actor.IsAdventurer()) || (ActiveCharacter.IsMonster() && actor.IsMonster());
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
                temp = ActiveCharacter.BaseTile;
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

            if (temp != null && CombatManager.LegalTiles.Contains(temp) && temp != ActiveCharacter.BaseTile)
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
            RHTile tile = MapManager.CurrentMap.GetTileByPixelPosition(mouseCursor);
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
                ChangePhase(PhaseEnum.Moving);
                ActiveCharacter.ClearTiles();
                Vector2 start = ActiveCharacter.Position;

                List<RHTile> tilePath = TravelManager.FindPathToLocation(ref start, _tTarget.Center);

                if (tilePath != null)
                {
                    ActiveCharacter.SetPath(tilePath);
                    ClearToPerformAction();
                }
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
                CombatTick(ref chargingCopy, ref queuedCopy, true);                 //Tick

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
                ChangePhase(PhaseEnum.Upkeep);
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
            _scrCombat.CloseMainSelection();

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

            if (!EndTurnOfCombatOverCheck())
            {
                if (activeSummon != null)
                {
                    if (activeSummon.Aggressive)// && SelectedAction.IsMelee())
                    {
                        List<RHTile> targets = SelectedAction.GetTargetTiles();
                        ActiveCharacter = activeSummon;
                        SelectedAction = new ChosenAction((CombatAction)DataManager.GetActionByIndex(CombatManager.BASIC_ATTACK));
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
                SelectedAction.ClearTargets();
            }

            //if (CurrentPhase != PhaseEnum.EndCombat)
            //{
            SelectedAction = null;
            ActiveCharacter = null;
            ChangePhase(PhaseEnum.Charging);
            //}
        }
        #endregion

        #region FloatingText Handling
        /// <summary>
        /// Adds the FloatingText object to the removal queue
        /// </summary>
        public static void RemoveFloatingText(FloatingText fText)
        {
            _scrCombat.RemoveFloatingText(fText);
        }

        /// <summary>
        /// Adds the FloatingText object to the queue to add
        /// </summary>
        public static void AddFloatingText(FloatingText fText)
        {
            _scrCombat.AddFloatingText(fText);
        }
        #endregion

        public class ChosenAction
        {
            public TargetEnum Target => (_chosenItem != null ? TargetEnum.Ally : _chosenAction.Target);
            public AreaTypeEnum AreaType => (_chosenItem != null ? AreaTypeEnum.Single : _chosenAction.AreaType);
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
                _liLegalTiles = User.BaseTile.GetAdjacentTiles();
            }

            public ChosenAction(CombatAction ca)
            {
                User = ActiveCharacter;
                _chosenAction = ca;
                _name = _chosenAction.Name;

                _chosenAction.SkillUser = ActiveCharacter;

                _liLegalTiles = new List<RHTile>();
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

                _chosenAction?.Sprite?.Draw(spritebatch);
            }

            public void Update(GameTime gTime)
            {
                if (_chosenAction != null) { _chosenAction.HandlePhase(gTime); }
                else if (_chosenItem != null)
                {
                    bool finished = false;
                    CombatActor c = CombatManager.ActiveCharacter;
                    if (!c.IsDirectionalAnimation(VerbEnum.Cast))
                    {
                        c.PlayAnimation(VerbEnum.Cast);
                        _bDrawItem = true;
                    }
                    else if (c.AnimationPlayedXTimes(3))
                    {
                        c.PlayAnimation(VerbEnum.Walk);
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
                ChangePhase(PhaseEnum.PerformAction);

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
            public bool CanTwinCast()
            {
                bool rv = false;
                if(_chosenAction != null){
                    rv = _chosenAction.Potency > 0;
                }
                return rv;
            }

            #region Targeting
            public void ClearTargets()
            {
                if(_chosenAction != null) { _chosenAction.TileTargetList.Clear(); }
                else if (_chosenItem != null) { _tTarget = null; }
            }

            public List<RHTile> GetTargetTiles()
            {
                return _chosenAction?.TileTargetList;
            }

            public void SetTargetTiles(List<RHTile> li)
            {
                if (_chosenAction != null)
                {
                    _chosenAction.AssignTiles();
                    _chosenAction.TileTargetList = li;
                }
            }

            public List<RHTile> DetermineTargetTiles(RHTile targetedTile)
            {
                List<RHTile> rvList = new List<RHTile>() { targetedTile };

                if(_chosenAction != null)
                {
                    switch (_chosenAction.AreaType)
                    {
                        case AreaTypeEnum.Cross:
                            foreach(RHTile t in targetedTile.GetAdjacentTiles())
                            {
                                if (t.CanTargetTile()) {
                                    rvList.Add(t);
                                }
                            }
                            break;
                        default:
                            break;
                    }
                }

                return rvList;
            }
            #endregion
        }

        /// <summary>
        /// Used to store info about the ActiveCharacters turn.
        /// 
        /// For now, only used to store whether thet havemoved or acted.
        /// </summary>
        public struct TurnInfo
        {
            private bool _bMoved;
            public bool HasMoved { get => _bMoved; set => _bMoved = value; }
            private bool _bActed;
            public bool HasActed { get => _bActed; set => _bActed = value; }
        }
    }
}
 