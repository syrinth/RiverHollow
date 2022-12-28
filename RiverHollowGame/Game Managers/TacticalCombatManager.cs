//using Microsoft.Xna.Framework;
//using Microsoft.Xna.Framework.Graphics;
//using Microsoft.Xna.Framework.Input;
//using RiverHollow.Characters;
//using RiverHollow.CombatStuff;
//using RiverHollow.GUIComponents.GUIObjects;
//using RiverHollow.GUIComponents.Screens;
//using RiverHollow.WorldObjects;
//using RiverHollow.Tile_Engine;
//using RiverHollow.Utilities;
//using System;
//using System.Collections.Generic;
//using static RiverHollow.Characters.Actor;
//using static RiverHollow.Game_Managers.GameManager;
//using static RiverHollow.Game_Managers.TravelManager;
//using RiverHollow.Misc;
//using RiverHollow.Items;

//namespace RiverHollow.Game_Managers
//{
//    public static class TacticalCombatManager
//    {
//        private const int MOVE_CHARGE = 40;
//        private const int ATTACK_CHARGE = 60;
//        private const double EXP_MULTIPLIER_BONUS = 0.3;
//        public const int BASIC_ATTACK = 300;

//        private static RHMap BattleMap => MapManager.CurrentMap;
//        private static List<Item> _liDroppedItems;
//        public static List<RHTile> LegalTiles { get; private set; }
//        public static List<RHTile> AreaTiles { get; private set; }
//        public static List<RHTile> TimedHazardTiles { get; private set; }

//        public static TacticalCombatActor ActiveCharacter;
//        public static List<TacticalCombatActor> Monsters { get; private set; }
//        public static List<TacticalCombatActor> Party { get; private set; }
//        public static List<TacticalSummon> Summons { get; private set; }

//        private static TacticalCombatScreen _scrCombat;
//        public enum CmbtPhaseEnum { Setup, Charging, Upkeep, MainSelection, ChooseMoveTarget, Moving, ChooseAction, ChooseActionTarget, PerformAction, Victory, DisplayDefeat }//NewTurn, EnemyTurn, SelectSkill, ChooseTarget, Defeat, DisplayAttack, DisplayVictory, Lost, PerformAction, EndCombat }
//        public static CmbtPhaseEnum CurrentPhase;
//        public static TacticalCombatAction SelectedAction;

//        public static RHTile SelectedTile;          //The tile currently selected by the targetter

//        public static double Delay;
//        public static bool InCombat { get; private set; } = false;

//        private static int _iXPMultiplier = 0;

//        public static TurnInfo CurrentTurnInfo;

//        #region Turn Sequence
//        static List<TacticalCombatActor> _liQueuedCharacters;
//        static List<TacticalCombatActor> _liChargingCharacters;
//        #endregion

//        /// <summary>
//        /// Determine whether the current turn is over because the
//        /// ActiveCharacter has both moved and acted.
//        /// </summary>
//        /// <returns>True is the turn is being forced to end</returns>
//        public static bool CheckForForcedEndOfTurn()
//        {
//            bool rv = false;
//            if(CurrentTurnInfo.HasActed && CurrentTurnInfo.HasMoved || (ActiveCharacter.IsSummon() && CurrentTurnInfo.HasActed))
//            {
//                rv = true;
//                EndTurn();
//            }

//            return rv;
//        }

//        public static void NewBattle(string oldMap)
//        {
//            ChangePhase(CmbtPhaseEnum.Setup);

//            ActiveCharacter = null;
//            SelectedAction = null;
//            SelectedTile = null;

//            Delay = 0;

//            Summons = new List<TacticalSummon>();

//            _liDroppedItems = new List<Item>();
//            LegalTiles = new List<RHTile>();
//            AreaTiles = new List<RHTile>();
//            Party = new List<TacticalCombatActor>();
//            Party.AddRange(PlayerManager.GetTacticalParty());

//            Monsters = new List<TacticalCombatActor>();
//            Monsters.AddRange(BattleMap.Monsters);

//            _liQueuedCharacters = new List<TacticalCombatActor>();
//            _liChargingCharacters = new List<TacticalCombatActor>();
//            _liChargingCharacters.AddRange(Party);
//            _liChargingCharacters.AddRange(Monsters);

//            //Characters with higher Spd go first
//            _liChargingCharacters.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));

//            TimedHazardTiles = MapManager.CurrentMap.CheckForCombatHazards(CombatHazard.HazardTypeEnum.Timed);

//            foreach(TacticalCombatActor c in _liChargingCharacters)
//            {
//                c.CurrentCharge = RHRandom.Instance().Next(0, 50);
//            }

//            foreach (TacticalCombatActor c in Monsters)
//            {
//                c.SetBaseTile(BattleMap.GetTileByPixelPosition(c.Position), true);
//            }

//            BattleStartInfo bInfo = BattleMap.DictionaryBattleStarts[oldMap];
//            foreach (ClassedCombatant c in Party)
//            {
//                if (c != PlayerManager.PlayerActor)
//                {
//                    MapManager.Maps[c.CurrentMapName].RemoveActor(c);
//                    c.Activate(true);
//                    c.CurrentMapName = BattleMap.Name;
//                    BattleMap.AddActor(c);
//                    c.SpdMult = NORMAL_SPEED;
//                }

//                Vector2 startpos = c.StartPosition;
//                c.SetBaseTile(bInfo.CombatTiles[(int)startpos.X, (int)startpos.Y], true);
//                c.Facing = PlayerManager.PlayerActor.Facing;
//                c.GoToIdle();
//            }


//            _scrCombat = new TacticalCombatScreen();
//            GUIManager.SetScreen(_scrCombat);

//            InCombat = true;
//            PlayerManager.AllowMovement = false;
//            PlayerManager.PlayerActor.PlayAnimationVerb(TacticalCombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);

//            PlayerManager.PlayerActor.SetMoveObj(Util.SnapToGrid(PlayerManager.PlayerActor.BaseTile.Center));

//            GUICursor.ResetCursor();
//        }

//        public static void Update(GameTime gTime)
//        {
//            switch (CurrentPhase)
//            {
//                //This phase starts combat and ensures that the Actors are locked to their tile.
//                case CmbtPhaseEnum.Setup:
//                    if (PlayerManager.PlayerActor.Position == PlayerManager.PlayerActor.BaseTile.Position)
//                    {
//                        PlayerManager.PlayerActor.PlayAnimationVerb(TacticalCombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
//                        ChangePhase(CmbtPhaseEnum.Charging);
//                    }
//                    break;

//                //For when there is no character so we must charge until someone hits 100
//                case CmbtPhaseEnum.Charging:
//                    //If there is no ActiveCharacter, we need to get the next one
//                    if (ActiveCharacter == null)
//                    {
//                        //If no characters are queued for their next turn,
//                        //we need to tick the combat
//                        if (_liQueuedCharacters.Count == 0)
//                        {
//                            CombatTick(ref _liChargingCharacters, ref _liQueuedCharacters);
//                        }
//                        else
//                        {
//                            //If there is at least one character, retrieve it
//                            if (GetActiveCharacter())
//                            {
//                                Camera.SetObserver(ActiveCharacter, true);
//                            }
//                        }
//                    }
//                    break;

//                //Phase for when a new ActiveCharacter is set but before they can do anything
//                case CmbtPhaseEnum.Upkeep:
//                    //If the ActiveCharacter has a LinkedSummon, have the summon perform its action first
//                    TacticalSummon activeSummon = ActiveCharacter.LinkedSummon;
//                    if (activeSummon != null && !activeSummon.Acted)
//                    {
//                        activeSummon.Acted = true;
//                        ActiveCharacter = activeSummon;
//                        //Have the summon perform its turn action and then reset the linked character to the ActiveCharacter
//                        activeSummon.TakeTurn();
//                    }
//                    else {
//                        CurrentTurnInfo = new TurnInfo();

//                        if (!Camera.IsMoving()) { GoToMainSelection(); }

//                        ActiveCharacter.TickStatusEffects();
//                        if (ActiveCharacter.Poisoned())
//                        {
//                            ActiveCharacter.ModifyHealth(Math.Max(1, (int)(ActiveCharacter.MaxHP / 20)), true);
//                        }
//                    }
//                    break;

//                case TacticalCombatManager.CmbtPhaseEnum.ChooseActionTarget:
//                    if (ActiveCharacter.Paused) { return; }
//                    HandleTargetting();
//                    break;

//                case TacticalCombatManager.CmbtPhaseEnum.ChooseMoveTarget:
//                    if (ActiveCharacter.Paused) { return; }
//                    HandleTargetting();
//                    break;

//                //Use this phase for when the ActiveCharacter is currently moving
//                case CmbtPhaseEnum.Moving:

//                    if (!ActiveCharacter.FollowingPath)
//                    {
//                        RHTile newTile = BattleMap.GetTileByPixelPosition(ActiveCharacter.Position);
//                        ActiveCharacter.SetBaseTile(newTile, false);

//                        if (ActiveCharacter.IsActorType(ActorEnum.Villager) || ActiveCharacter == PlayerManager.PlayerActor)
//                        {
//                            List<Item> tileItems = _liDroppedItems.FindAll(item => newTile.Rect.Contains(item.Position));

//                            foreach (Item tileItem in tileItems)
//                            {
//                                if (tileItem != null && InventoryManager.HasSpaceInInventory(tileItem.ID, tileItem.Number))
//                                {
//                                    BattleMap.AddItemToPlayerInventory(tileItem);
//                                }
//                            }
//                        }
//                        CurrentTurnInfo.Moved();

//                        if (!CheckForForcedEndOfTurn())
//                        {
//                            if (!ActiveCharacter.IsActorType(ActorEnum.Monster))
//                            {
//                                GoToMainSelection();
//                            }
//                        }
//                    }
//                    break;

//                //Use this phase to use the SelectedAction
//                case CmbtPhaseEnum.PerformAction:
//                    if (SelectedAction != null)
//                    {
//                        SelectedAction.Update(gTime);
//                    }

//                    break;

//                case CmbtPhaseEnum.Victory:
//                    if (Monsters?.Count == 0 && !_scrCombat.AreThereFloatingText())
//                    {
//                        EndCombatVictory();
//                    }
//                    break;
//            }
//        }

//        #region Phase Handling
//        /// <summary>
//        /// Controlled for changing phases.
//        /// 
//        /// If we are on the player's turn and the active characteris moving or performing their action
//        /// we want to unpause the game. Otherwise, the game should be paused.
//        /// </summary>
//        /// <param name="newPhase"></param>
//        public static void ChangePhase(CmbtPhaseEnum newPhase)
//        {
//            if(newPhase == CmbtPhaseEnum.ChooseActionTarget || newPhase == CmbtPhaseEnum.ChooseMoveTarget)
//            {
//                _scrCombat.HideMainSelection();
//            }
//            if (newPhase == CmbtPhaseEnum.PerformAction || newPhase == CmbtPhaseEnum.Moving)
//            {
//                if (ActiveCharacter != null && !ActiveCharacter.IsActorType(ActorEnum.Monster) && GameManager.IsPaused())
//                {
//                    GameManager.Unpause();
//                }
//            }
//            else if (!GameManager.IsPaused())
//            {
//                GameManager.Pause();
//            }

//            CurrentPhase = newPhase;
//        }

//        /// <summary>
//        /// Returns true is we are on MainSelection or ChooseAction
//        /// </summary>
//        /// <returns></returns>
//        public static bool AreWeSelectingAnAction()
//        {
//            return CombatPhaseCheck(CmbtPhaseEnum.ChooseAction) || CombatPhaseCheck(CmbtPhaseEnum.MainSelection);
//        }

//        /// <summary>
//        /// Checks the CurrentPhase against the given CombatPhase
//        /// </summary>
//        /// <param name="test">The CombatPhaseEnum to test against</param>
//        /// <returns></returns>
//        public static bool CombatPhaseCheck(CmbtPhaseEnum test)
//        {
//            return CurrentPhase == test;
//        }

//        #endregion

//        public static void DrawUpperCombatLayer(SpriteBatch spriteBatch)
//        {
//            if (InCombat)
//            {
//                _scrCombat.DrawUpperCombatLayer(spriteBatch);
//            }
//        }

//        private static void HandleTargetting()
//        {
//            HandleMouseTargetting();
//            HandleKeyboardTargetting();

//            //Cancel out of selections made if escape is hit
//            if (InputManager.CheckPressedKey(Keys.Escape))
//            {
//                _scrCombat.CancelAction();

//                ClearSelectedTile();
//                SelectedAction = null;
//                if (CurrentPhase == CmbtPhaseEnum.ChooseAction || CurrentPhase == CmbtPhaseEnum.ChooseMoveTarget) {
//                    ChangePhase(CmbtPhaseEnum.MainSelection);
//                    _scrCombat.OpenMainSelection();
//                }
//                else if(CurrentPhase == CmbtPhaseEnum.ChooseActionTarget)
//                {
//                    ChangePhase(CmbtPhaseEnum.ChooseAction);
//                }
//            }
//        }

//        #region CombatScreen Controls
//        /// <summary>
//        /// Switches the phase to MainSelection and tells the CombatScree
//        /// to open a MainSelectionWindow if one is not yet up.
//        /// </summary>
//        public static void GoToMainSelection()
//        {
//            ChangePhase(CmbtPhaseEnum.MainSelection);
//            if (ActiveCharacter.IsActorType(ActorEnum.Monster))
//            {
//                ((TacticalMonster)ActiveCharacter).TakeTurn();
//            }
//            else
//            {
//                _scrCombat.OpenMainSelection();
//            }
//        }
//        #endregion

//        internal static bool CanCancel()
//        {
//            return CurrentPhase == CmbtPhaseEnum.ChooseAction || CurrentPhase == CmbtPhaseEnum.ChooseActionTarget || CurrentPhase == CmbtPhaseEnum.ChooseMoveTarget;
//        }

//        #region End Of Comabt
//        /// <summary>
//        /// Called at the End of a Turn to see if the combat has been won or lost.
//        /// </summary>
//        /// <returns>True is combat is ending</returns>
//        public static bool CheckForEndOfCombat()
//        {
//            bool rv = false;

//            //Lambda expressions to find all characters still standing
//            bool partyUp = Party.FindAll(actor => actor.CurrentHP > 0 ).Count > 0;
//            bool monstersUp = Monsters.FindAll(actor => actor.CurrentHP > 0).Count > 0; ;

//            //If there is at least one party member up and no monsters, go to Victory
//            if (partyUp && !monstersUp)
//            {
//                rv = true;
//                ChangePhase(CmbtPhaseEnum.Victory);
//                foreach (ClassedCombatant a in Party)
//                {
//                    a.CurrentCharge = 0;
//                    a.PlayAnimation(CombatAnimationEnum.Win);
//                }

//            }
//            else if (!partyUp)
//            {
//                rv = true;
//                ChangePhase(CmbtPhaseEnum.DisplayDefeat);
//            }

//            return rv;
//        }

//        /// <summary>
//        /// Called when the Battle ends in Victory.
//        /// We need to re-enable movement as it's locked down in combat
//        /// </summary>
//        public static void EndCombatVictory()
//        {
//            InCombat = false;
//            PlayerManager.AllowMovement = true;
//            foreach (Item it in _liDroppedItems) { it.AutoPickup = true; }

//            MapManager.CurrentMap.CleanupSummons();

//            foreach (ClassedCombatant c in Party.FindAll(x => x != PlayerManager.PlayerActor))
//            {
//                MapManager.CurrentMap.RemoveActor(c);
//                c.Activate(false);
//                c.SpdMult = NPC_WALK_SPEED;
//                BattleMap.RemoveActor(c);
//            }

//            PlayerManager.PlayerActor.PlayAnimation(VerbEnum.Idle, PlayerManager.PlayerActor.Facing);
//            Camera.SetObserver(PlayerManager.PlayerActor);
//            GameManager.ActivateTriggers(MapManager.CurrentMap.Name + MOB_OPEN);

//            GoToHUDScreen();

//            PlayerManager.PlayerActor.ActivePet?.SpawnNearPlayer();
//        }

//        /// <summary>
//        /// Called when the Battle ends in escape.
//        /// We need to re-enable movement as it's locked down in combat
//        /// </summary>
//        public static void EndCombatEscape()
//        {
//            InCombat = false;
//            MapManager.CurrentMap.CleanupSummons();
//            Camera.SetObserver(PlayerManager.PlayerActor);
//            PlayerManager.AllowMovement = true;
//            PlayerManager.PlayerActor.ClearPath();
//            GoToHUDScreen();

//            PlayerManager.PlayerActor.ActivePet?.SpawnNearPlayer();
//        }

//        /// <summary>
//        /// Called when the Battle ends in escape.
//        /// We need to re-enable movement as it's locked down in combat
//        /// </summary>
//        public static void EndCombatDefeat()
//        {
//            InCombat = false;
//            MapManager.CurrentMap.CleanupSummons();
//            Camera.SetObserver(PlayerManager.PlayerActor);
//            //PlayerManager.AllowMovement = true;
//            PlayerManager.PlayerActor.ClearPath();

//            //Give the player back 1 hp
//            PlayerManager.PlayerActor.ClearConditions();
//            PlayerManager.PlayerActor.ModifyHealth(1, false);
//            GoToHUDScreen();

//            MapManager.FadeToNewMap(MapManager.TownMap, MapManager.TownMap.GetCharacterSpawn("PlayerSpawn"));
//        }
//        #endregion

//        public static void ProcessActionChoice(TacticalCombatAction a)
//        {
//            a.AssignUser(ActiveCharacter);
//            SelectedAction = a;
//            ChangePhase(CmbtPhaseEnum.ChooseActionTarget);
//            FindAndHighlightLegalTiles();

//            //if (!SelectedAction.SelfOnly())
//            //{
//            //    if (!ActiveCharacter.IsActorType(ActorEnum.Monster)) {
//            //        ChangePhase(PhaseEnum.ChooseTarget);
//            //    }  //Skips this phase for enemies. They don't "choose" targets
//            //}
//            //else
//            //{
//            //    ChangePhase(PhaseEnum.DisplayAttack);
//            //    Text = SelectedAction.Name;
//            //}
//        }

//        /// <summary>
//        /// Called to give the party experience points for killing a monster
//        /// </summary>
//        /// <param name="m">The monster that has been defeated</param>
//        public static void GiveXP(TacticalMonster m)
//        {
//            //Calculates the total XP based off of the XP multiplier and then add the floating text
//            GameManager.TotalExperience = (int)(m.XP * (1 + (double)(EXP_MULTIPLIER_BONUS * _iXPMultiplier++)));
//        }

//        /// <summary>
//        /// Perform any actions required of the CombatManager on a KO'd Actor.
//        /// </summary>
//        /// <param name="c">The KO'd Actor</param>
//        public static void RemoveKnockedOutCharacter(TacticalCombatActor c)
//        {
//            //If the Actor was a Monster, remove it from the list
//            if (Monsters.Contains((c)))
//            {
//                Monsters.Remove(c);
//                c.ClearTiles();
//            }

//            //Remove the Actor from the turn order 
//            _liChargingCharacters.Remove(c);                    
//            _liQueuedCharacters.Remove(c);
//        }

//        /// <summary>
//        /// Perform any actions that need to happen once the monster
//        /// has finished playing it's death animation
//        /// </summary>
//        /// <param name="c">The defeated Monster</param>
//        public static void MonsterKOAnimFinished(TacticalMonster m)
//        {
//            PlayerManager.AddMonsterEnergyToQueue(100);
//            MapManager.RemoveActor(m);

//            Item droppedItem = DropManager.DropMonsterLoot(m);
//            if (TacticalCombatManager.InCombat) { _liDroppedItems.Add(droppedItem); }
//            else { droppedItem.AutoPickup = true; }
//        }

//        #region Enemy AI

//        //For now, when the enemy takes their turn, have them select a random party member
//        //When enemies get healing/defensive skills, they'll have their own logic to process
//        public static void EnemyTakeTurn()
//        {
//            RHTile targetTile = null;

//            bool gottaMove = true;

//            //Step one determine if we are in range of a target
//            foreach (TacticalCombatAction c in ActiveCharacter.GetCurrentSpecials())
//            {
//                if (c.Range == 1)
//                {
//                    foreach (RHTile t in ActiveCharacter.BaseTile.GetAdjacentTiles())
//                    {
//                        if (t.HasCombatant() && !t.Character.IsActorType(ActorEnum.Monster))
//                        {
//                            gottaMove = false;
//                            SelectedAction = c;
//                            targetTile = t;
//                            break;
//                        }
//                    }
//                }
//            }

//            //If we have found a target to attack, but we need to move to get into range of them
//            //Then find the shortest path, and move as close as you can.
//            if (!CurrentTurnInfo.HasMoved && gottaMove && !ActiveCharacter.FollowingPath)
//            {
//                if (targetTile == null)
//                {
//                    double closestDistance = 0;
//                    Vector2 start = ActiveCharacter.BaseTile.Center;
//                    Vector2 closest = Vector2.Zero;

//                    foreach (TacticalCombatActor actor in Party)
//                    {
//                        Vector2 target = actor.BaseTile.Center;

//                        int deltaX = (int)Math.Abs(start.X - target.X);
//                        int deltaY = (int)Math.Abs(start.Y - target.Y);

//                        double distance = Math.Sqrt(deltaX ^ 2 + deltaY ^ 2);
//                        if (distance < closestDistance || closestDistance == 0)
//                        {
//                            closest = target;
//                            closestDistance = distance;
//                        }
//                    }

//                    SelectedTile = BattleMap.GetTileByPixelPosition(closest);

//                    //Need to unset the Combatant from the tile the monster is moving to so that
//                    //we can pathfind to it
//                    TacticalCombatActor act = SelectedTile.Character;
//                    act.ClearTiles();

//                    //Determine the pathfinding for the Monster
//                    SetMoveTarget();

//                    //Reset the CombatActor's RHTile
//                    BattleMap.GetTileByPixelPosition(closest).SetCombatant(act);
//                }
//            }

//            //If we have not yet acted, we need to move, and we are not following a path
//            if (!CurrentTurnInfo.HasActed && !gottaMove && !ActiveCharacter.FollowingPath)
//            {
//                foreach (RHTile t in ActiveCharacter.BaseTile.GetAdjacentTiles())
//                {
//                    if (t.HasCombatant() && !t.Character.IsActorType(ActorEnum.Monster))
//                    {
//                        SelectedTile = t;
//                        SelectedAction = ActiveCharacter.GetCurrentSpecials()[0];
//                        SelectedAction.UseSkillOnTarget();
//                    }
//                }
//            }
//        }

//        /// <summary>
//        /// Have the enemy iterate over it's skills and determine if
//        /// there are any targets in range.
//        /// </summary>
//        private static void EnemyCheckforTargets()
//        {
//            foreach (TacticalMenuAction action in ActiveCharacter.TacticalAbilityList) {

//            }
//        }
//        #endregion

//        #region Tile Handling
//        /// <summary>
//        /// Highlight all RHTiles in the LegalTiles list.
//        /// </summary>
//        public static void FindAndHighlightLegalTiles()
//        {
//            bool isMoving = CombatPhaseCheck(CmbtPhaseEnum.ChooseMoveTarget);
//            int distance = 0;

//            if (!isMoving) { distance = SelectedAction.Range; }
//            else
//            {
//                distance = ActiveCharacter.MovementSpeed;
//            }

//            TravelManager.SetParams(ActiveCharacter.Size, ActiveCharacter);
//            LegalTiles.Add(ActiveCharacter.BaseTile);
//            foreach (KeyValuePair<RHTile, TravelData> kvp in TravelManager.FindRangeOfAction(ActiveCharacter, distance, isMoving))
//            {
//                if (kvp.Value.InRange)
//                {
//                    LegalTiles.Add(kvp.Key);
//                }

//                //Summons must be placed on empty tiles
//                if(SelectedAction != null && SelectedAction.IsSummonSpell() && kvp.Key.Character != null)
//                {
//                    LegalTiles.Remove(kvp.Key);
//                }
//            } 

//            foreach (RHTile t in LegalTiles)
//            {
//                t.LegalTile(true);
//            }
//            TravelManager.ClearParams();
//        }

//        /// <summary>
//        /// Sets the given RHTile as the SelectedTile and unsets the
//        /// previous SelectedTile if it exists.
//        /// </summary>
//        /// <param name="tile">New tile to select</param>
//        public static void SelectTile(RHTile tile)
//        {
//            tile.Select(true);

//            if (SelectedTile != tile)
//            {
//                if (SelectedTile != null) { SelectedTile.Select(false); }
//                SelectedTile = tile;
//            }

//            if (CurrentPhase == CmbtPhaseEnum.ChooseActionTarget)
//            {
//                ClearAreaTiles();
//                AreaTiles = SelectedAction.DetermineTargetTiles(tile);
//                foreach (RHTile t in AreaTiles)
//                {
//                    t.AreaTile(true);
//                }
//            }
//        }

//        /// <summary>
//        /// Clears the Legal, Selected, and Area tile lists and unsets the flags
//        /// from the given RHTiles
//        /// </summary>
//        public static void ClearAllTiles()
//        {
//            ClearLegalTiles();
//            ClearSelectedTile();
//            ClearAreaTiles();
//        }

//        /// <summary>
//        /// Set each Legal Tile to be illegal and then Clear the list
//        /// </summary>
//        public static void ClearLegalTiles()
//        {
//            foreach (RHTile t in LegalTiles)
//            {
//                t.LegalTile(false);
//            }
//            LegalTiles.Clear();
//        }

//        /// <summary>
//        /// Unsets the CurrentTile
//        /// </summary>
//        public static void ClearSelectedTile()
//        {
//            if (SelectedTile != null) { SelectedTile.Select(false); }
//            SelectedTile = null;
//        }

//        /// <summary>
//        /// Unsets all Area Tiles and then clears the list
//        /// </summary>
//        public static void ClearAreaTiles()
//        {
//            foreach (RHTile t in AreaTiles)
//            {
//                t.AreaTile(false);
//            }
//            AreaTiles.Clear();
//        }

//        /// <summary>
//        /// Compares a Character against the ActiveCharacter to see if
//        /// they are on the same team.
//        /// </summary>
//        /// <param name="actor"></param>
//        /// <returns></returns>
//        public static bool OnSameTeam(TacticalCombatActor actor)
//        {
//            return (!ActiveCharacter.IsActorType(ActorEnum.Monster) && !actor.IsActorType(ActorEnum.Monster)) || (ActiveCharacter.IsActorType(ActorEnum.Monster) && actor.IsActorType(ActorEnum.Monster));
//        }

//        public static void CheckTileForActiveHazard(TacticalCombatActor c)
//        {
//            List<CombatHazard> activatedHazards = new List<CombatHazard>();
//            foreach(RHTile tile in c.GetTileList())
//            {
//                CombatHazard currHazard = tile.HazardObject;
//                if (currHazard != null)
//                {
//                    if (!activatedHazards.Contains(currHazard) && currHazard.Active)
//                    {
//                        activatedHazards.Add(currHazard);
//                        c.ModifyHealth(currHazard.Damage, true);
//                    }
//                }
//            } 
//        }

//        public static void CheckTileForActiveHazard(TacticalCombatActor c, RHTile t)
//        {
//            CombatHazard currHazard = t.HazardObject;
//            if (currHazard != null)
//            {
//                if (currHazard.Active)
//                {
//                    c.ModifyHealth(currHazard.Damage, true);
//                }
//            }
//        }
//        #endregion

//        #region SelectionHandling   
//        /// <summary>
//        /// Determines whether or not the given tile has been selected while hovering over it
//        /// </summary>
//        /// <param name="tile">The tile to test against</param>
//        //public static void TestHoverTile(RHTile tile)
//        //{
//        //    if (SelectedAction.LegalTiles.Contains(tile) && (tile.HasCombatant() || SelectedAction.AreaOfEffect()))
//        //    {
//        //        tile.Select(true);
//        //    }
//        //}
//        public static void HandleKeyboardTargetting()
//        {
//            RHTile temp = SelectedTile;
//            if (temp == null)
//            {
//                temp = ActiveCharacter.BaseTile;
//            }

//            if (InputManager.CheckPressedKey(Keys.A))
//            {
//                temp = temp.GetTileByDirection(DirectionEnum.Left);
//            }
//            else if (InputManager.CheckPressedKey(Keys.D))
//            {
//                temp = temp.GetTileByDirection(DirectionEnum.Right);
//            }
//            else if (InputManager.CheckPressedKey(Keys.W))
//            {
//               temp = temp.GetTileByDirection(DirectionEnum.Up);
//            }
//            else if (InputManager.CheckPressedKey(Keys.S))
//            {
//               temp = temp.GetTileByDirection(DirectionEnum.Down);
//            }

//            if (temp != null && TacticalCombatManager.LegalTiles.Contains(temp) && temp != ActiveCharacter.BaseTile)
//            {
//                SelectTile(temp);
//            }

//            if (InputManager.CheckPressedKey(Keys.Enter))
//            {
//                if (CurrentPhase == CmbtPhaseEnum.ChooseMoveTarget)
//                {
//                    TacticalCombatManager.SetMoveTarget();
//                }
//                else if (CurrentPhase == CmbtPhaseEnum.ChooseActionTarget)
//                {
//                    TacticalCombatManager.SelectedAction.UseSkillOnTarget();
//                }
//            }
//        }
//        public static void HandleMouseTargetting()
//        {
//            Vector2 mouseCursor = GUICursor.GetWorldMousePosition();
//            RHTile tile = BattleMap.GetTileByPixelPosition(mouseCursor);
//            if (tile != null && LegalTiles.Contains(tile))
//            {
//                SelectTile(tile);
//            }
//        }

//        /// <summary>
//        /// If there is a SelectedTile, officially set the targeted tile
//        /// and change the turn phase to Moving.
//        /// 
//        /// Then perform pathfinding to the tile, and prepare to move
//        /// </summary>
//        public static void SetMoveTarget()
//        {
//            if (SelectedTile != null)
//            {
//                ChangePhase(CmbtPhaseEnum.Moving);
//                ActiveCharacter.ClearTiles();
//                Vector2 start = ActiveCharacter.Position;

//                List<RHTile> tilePath = TravelManager.FindPathToLocation(ref start, SelectedTile.Center);

//                if (tilePath != null)
//                {
//                    ActiveCharacter.SetPath(tilePath);
//                    ClearToPerformAction();
//                }
//            }
//        }

//        /// <summary>
//        /// Closes any open selection windows and clears
//        /// the legal and selected tiles
//        /// </summary>
//        public static void ClearToPerformAction()
//        {
//            _scrCombat.CloseMainSelection();
//            ClearAllTiles();
//        }
//        #endregion

//        #region Turn Handling
//        private static void CombatTick(ref List<TacticalCombatActor> charging, ref List<TacticalCombatActor> queued, bool dummy = false)
//        {
//            List<TacticalCombatActor> toQueue = new List<TacticalCombatActor>();

//            foreach(RHTile t in TimedHazardTiles)
//            {
//                if (t.HazardObject.Charge())
//                {

//                }
//            }

//            foreach(TacticalCombatActor c in charging)
//            {
//                //If Actor is not knocked out, increment the charge, capping to 100
//                if (!c.KnockedOut() || c.CurrentHP > 0)
//                {
//                    //Do not charge monsters that are not in engagement range of a player
//                    bool actorIsMonster = c.IsActorType(ActorEnum.Monster);

//                    if (dummy) { HandleChargeTick(ref c.DummyCharge, ref toQueue, c); }
//                    else { HandleChargeTick(ref c.CurrentCharge, ref toQueue, c); }
//                }
//            }

//            foreach(TacticalCombatActor c in toQueue)
//            {
//                queued.Add(c);
//                charging.Remove(c);
//            }
//        }
//        private static void HandleChargeTick(ref int charge, ref List<TacticalCombatActor> toQueue, TacticalCombatActor c)
//        {
//            charge += c.StatSpd;
//            if (charge >= 100)
//            {
//                charge = 100;
//                toQueue.Add(c);
//            }
//        }

//        public static List<TacticalCombatActor> CalculateTurnOrder(int maxShown)
//        {
//            List<TacticalCombatActor> rv = new List<TacticalCombatActor>();
//            List<TacticalCombatActor> queuedCopy = new List<TacticalCombatActor>();
//            List<TacticalCombatActor> chargingCopy = new List<TacticalCombatActor>();

//            LoadList(ref queuedCopy, _liQueuedCharacters);
//            LoadList(ref chargingCopy, _liChargingCharacters);

//            //If there is an Active Character, Add them to the Turn Order list and blank their DummyCharge
//            if (ActiveCharacter != null)
//            {
//                rv.Add(ActiveCharacter);
//                TacticalCombatActor c = chargingCopy.Find(x => x.Name == ActiveCharacter.Name);
//                c.DummyCharge -= (SelectedAction == null) ? c.DummyCharge : ATTACK_CHARGE;
//            }

//            //If there are any queued Actors, add them to the charging list
//            foreach (TacticalCombatActor c in queuedCopy)
//            {
//                if (rv.Count < maxShown)
//                {
//                    rv.Add(c);
//                    c.DummyCharge = 0;
//                    chargingCopy.Add(c);
//                }
//                else { break; }
//            }
//            queuedCopy.Clear();                                                 //Clear the queue

//            //Cap out entries at maxShown
//            while (rv.Count < maxShown)
//            {
//                chargingCopy.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));        //Sort the charging Actors by their speed
//                CombatTick(ref chargingCopy, ref queuedCopy, true);                 //Tick

//                //For all entries in the queue add them to the TurnOrder List,
//                //set the Charge to 0, and add to the queue, sorting by Spd
//                foreach (TacticalCombatActor c in queuedCopy)
//                {
//                    if (rv.Count < maxShown)
//                    {
//                        rv.Add(c);
//                        c.DummyCharge = 0;
//                        chargingCopy.Add(c);
//                    }
//                    else { break; }
//                }
//                queuedCopy.Clear();
//            }

//            return rv;
//        }

//        private static void LoadList(ref List<TacticalCombatActor> toFill, List<TacticalCombatActor> takeFrom)
//        {
//            foreach (TacticalCombatActor c in takeFrom)
//            {
//                TacticalCombatActor actor = c;
//                actor.DummyCharge = c.CurrentCharge;
//                toFill.Add(actor);
//            }
//        }

//        /// <summary>
//        /// We need to get the first CombatActor from the list of queued characters.
//        /// 
//        /// Do not retrieve any queued characters who now have 0 HP.
//        /// If the first character has 0 HP, call this method again.
//        /// </summary>
//        private static bool GetActiveCharacter()
//        {
//            if (_liQueuedCharacters[0].CurrentHP > 0) {
//                ActiveCharacter = _liQueuedCharacters[0];
//                _liQueuedCharacters.RemoveAt(0);

//                //Re-add the character to the charge list and sort by speed
//                _liChargingCharacters.Add(ActiveCharacter);
//                _liChargingCharacters.Sort((x, y) => x.StatSpd.CompareTo(y.StatSpd));

//                //Go into Upkeep phase
//                ChangePhase(CmbtPhaseEnum.Upkeep);
//            }
//            else
//            {
//                //If the character has 0 HP, remove them from the queue
//                //and try again if anyone else is queued.
//                _liQueuedCharacters.RemoveAt(0);
//                if (_liQueuedCharacters.Count > 0)
//                {
//                    GetActiveCharacter();
//                }
//            }

//            return ActiveCharacter != null;
//        }

//        public static void EndTurn()
//        {
//            _scrCombat.CloseMainSelection();

//            //If the character is a Summon, set the ActiveCharacter back to the linked character
//            if (ActiveCharacter.ActorType == ActorEnum.Summon)
//            {
//                ActiveCharacter = ((TacticalSummon)ActiveCharacter).linkedChar;
//                //Go into Upkeep phase
//                ChangePhase(CmbtPhaseEnum.Upkeep);
//            }
//            else
//            {
//                //Only reset the summons acted flag after the summoner has gone
//                if (ActiveCharacter.LinkedSummon != null) {
//                    ActiveCharacter.LinkedSummon.Acted = false;
//                }
//                TurnOver();
//            }
//        }

//        private static void TurnOver()
//        {
//            SelectedAction?.ClearTargets();
//            ActiveCharacter.EndTurn();

//            SelectedAction = null;
//            ActiveCharacter = null;
//            ChangePhase(CmbtPhaseEnum.Charging);
//        }
//        #endregion

//        #region FloatingText Handling
//        /// <summary>
//        /// Adds the FloatingText object to the removal queue
//        /// </summary>
//        public static void RemoveFloatingText(FloatingText fText)
//        {
//            _scrCombat.RemoveFloatingText(fText);
//        }

//        /// <summary>
//        /// Adds the FloatingText object to the queue to add
//        /// </summary>
//        public static void AddFloatingText(FloatingText fText)
//        {
//            _scrCombat?.AddFloatingText(fText);
//        }
//        #endregion

//        /// <summary>
//        /// Used to store info about the ActiveCharacters turn.
//        /// 
//        /// For now, only used to store whether thet havemoved or acted.
//        /// </summary>
//        public struct TurnInfo
//        {
//            public bool HasMoved { get; private set; }
//            public bool HasActed { get; private set; }

//            public void Moved()
//            {
//                HasMoved = true;
//                ActiveCharacter.CurrentCharge -= MOVE_CHARGE;
//            }

//            public void Acted()
//            {
//                HasActed = true;
//                ActiveCharacter.CurrentCharge -= ATTACK_CHARGE;
//            }
//        }
//    }
//}
 
