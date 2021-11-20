using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.TacticalCombatManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.TravelManager;

namespace RiverHollow.Characters
{
    public class TacticalMonster : TacticalCombatActor
    {
        #region Properties
        #region Traits
        enum TraitEnum { Coward };
        bool _bCoward;
        #endregion

        public MonsterSpawn SpawnPoint;
        int _id;
        public int ID => _id;
        int _iRating;
        int _iXP;
        public int XP => _iXP;
        protected Vector2 _moveTo = Vector2.Zero;
        List<int> _liLootIDs;

        public override int Attack => 20 + (_iRating * 10);

        public override int MaxHP => (int)((((Math.Pow(_iRating, 2)) * 10) + 20) * Math.Pow(Math.Max(1, (double)_iRating / 14), 2));

        protected double _dIdleFor;

        bool _bJump;
        Vector2 _vJumpTo;

        int _iMoveFailures = 0;

        List<SpawnConditionEnum> _liSpawnConditions;
        List<TacticalCombatAction> _liCombatActions;

        #region Turn Logic
        RHTile _selectedTile;
        TacticalCombatAction _chosenAction;
        enum TurnStepsEnum { Move, Act, EndTurn };
        List<TurnStepsEnum> _liTurnSteps;

        Thread _thrPathing;
        TravelMap _travelMap;
        List<RHTile> _liFoundPath;
        #endregion

        #endregion

        public TacticalMonster(int id, Dictionary<string, string> data)
        {
            SpdMult = NORMAL_SPEED;
            _liLootIDs = new List<int>();
            _liCombatActions = new List<TacticalCombatAction>();
            _liSpawnConditions = new List<SpawnConditionEnum>();

            _eActorType = ActorEnum.Monster;
            ImportBasics(data, id);
        }

        protected void ImportBasics(Dictionary<string, string> data, int id)
        {
            _id = id;
            DataManager.GetTextData("Monster", _id, ref _sName, "Name");

            _iRating = int.Parse(data["Lvl"]);
            _iXP = _iRating * 10;
            _iStrength = 1 + _iRating;
            _iDefense = 8 + (_iRating * 3);
            _iVitality = 2 * _iRating + 10;
            _iMagic = 0; // 2 * _iRating + 2;
            _iResistance = 2 * _iRating + 10;
            _iSpeed = 10;

            Util.AssignValue(ref _iMoveSpeed, "MoveSpeed", data);
            Util.AssignValue(ref _iBodyWidth, "Width", data);
            Util.AssignValue(ref _iBodyHeight, "Height", data);

            if (data.ContainsKey("Size")) {
                _iSize = int.Parse(data["Size"]);
                _arrTiles = new RHTile[_iSize, _iSize];
            }
            string[] split;
            if (data.ContainsKey("Condition"))
            {
                split = data["Condition"].Split('-');
                for (int i = 0; i < split.Length; i++)
                {
                    _liSpawnConditions.Add(Util.ParseEnum<SpawnConditionEnum>(split[i]));
                }
            }

            if (data.ContainsKey("Loot"))
            {
                split = data["Loot"].Split('|');
                for (int i = 0; i < split.Length; i++)
                {
                    _liLootIDs.Add(int.Parse(split[i]));
                }
            }

            _bJump = false; // data.ContainsKey("Jump"); Turning off jump logic for now

            foreach (string ability in data["Ability"].Split('|'))
            {
                _liCombatActions.Add((TacticalCombatAction)DataManager.GetTacticalActionByIndex(int.Parse(ability)));
            }

            if (data.ContainsKey("Trait"))
            {
                HandleTrait(data["Trait"]);
            }

            if (data.ContainsKey("Resist"))
            {
                foreach (string elem in data["Resist"].Split('-'))
                {
                    _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Resists;
                }
            }

            if (data.ContainsKey("Vuln"))
            {
                foreach (string elem in data["Vuln"].Split('-'))
                {
                    _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Vulnerable;
                }
            }

            _iCurrentHP = MaxHP;
            _iCurrentMP = MaxMP;

            LoadSpriteAnimations(ref _sprBody, LoadWorldAndCombatAnimations(data), DataManager.FOLDER_MONSTERS + data["Texture"]);
        }

        /// <summary>
        /// Override the Talk method so the player cannot talk to monsters
        /// </summary>
        /// <param name="facePlayer"></param>
        public override void Talk(bool facePlayer = true) { }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);

            if (_liTurnSteps?.Count > 0)
            {
                if (_liTurnSteps[0] == TurnStepsEnum.Move)
                {
                    if (TacticalCombatManager.CurrentTurnInfo.HasMoved)
                    {
                        _liTurnSteps.RemoveAt(0);
                    }
                    else if (!CombatPhaseCheck(CmbtPhaseEnum.Moving))
                    {
                        TacticalCombatManager.ChangePhase(TacticalCombatManager.CmbtPhaseEnum.Moving);
                        SetPath(_liFoundPath);
                    }
                }

                if (_liTurnSteps[0] == TurnStepsEnum.Act)
                {
                    if (TacticalCombatManager.CurrentTurnInfo.HasActed)
                    {
                        _liTurnSteps.RemoveAt(0);
                    }
                    else if (!CombatPhaseCheck(CmbtPhaseEnum.PerformAction))
                    {
                        DetermineFacing(_chosenAction.TileTargetList[0]);
                        TacticalCombatManager.SelectedAction = _chosenAction;
                        TacticalCombatManager.ActiveCharacter.CurrentMP -= _chosenAction.MPCost;          //Checked before Processing
                        TacticalCombatManager.ChangePhase(CmbtPhaseEnum.PerformAction);
                    }
                }
                if (_liTurnSteps[0] == TurnStepsEnum.EndTurn)
                {
                    TacticalCombatManager.EndTurn();
                }
            }

            ///When the Monster has finished playing the KO animation, let the CombatManager know so it can do any final actions
            if (IsCurrentAnimation(AnimationEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                TacticalCombatManager.MonsterKOAnimFinished(this);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
        }

        private void HandleTrait(string traitData)
        {
            string[] traits = Util.FindTags(traitData);
            foreach (string s in traits)
            {
                TraitEnum trait = Util.ParseEnum<TraitEnum>(s);

                switch (trait)
                {
                    case TraitEnum.Coward:
                        _bCoward = true;
                        break;
                    default:
                        //DataManager.GetMonsterTraitData(data["Trait"])
                        //string[] tagType = s.Split(':');
                        //if (tagType[0].Equals(Util.GetEnumString(StatEnum.Str)))
                        //{
                        //    ApplyTrait(ref _iStrength, tagType[1]);
                        //}
                        //else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Def)))
                        //{
                        //    ApplyTrait(ref _iDefense, tagType[1]);
                        //}
                        //else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Vit)))
                        //{
                        //    ApplyTrait(ref _iVitality, tagType[1]);
                        //}
                        //else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Mag)))
                        //{
                        //    ApplyTrait(ref _iMagic, tagType[1]);
                        //}
                        //else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Res)))
                        //{
                        //    ApplyTrait(ref _iResistance, tagType[1]);
                        //}
                        //else if (tagType[0].Equals(Util.GetEnumString(StatEnum.Spd)))
                            break;
                }
            }
        }

        //private void ApplyTrait(ref int value, string data)
        //{
        //    if (data.Equals("+"))
        //    {
        //        value = (int)(value * 1.1);
        //    }
        //    else if (data.Equals("-"))
        //    {
        //        value = (int)(value * 0.9);
        //    }
        //}

        private void UpdateMovement(GameTime gTime)
        {
            bool move = true;
            Vector2 direction = Vector2.Zero;

            GetIdleMovementTarget(gTime);

            if (_bJump)
            {
                if (_vMoveTo != Vector2.Zero && BodySprite.CurrentAnimation.StartsWith("Idle"))
                {
                    move = false;

                    PlayAnimationVerb(VerbEnum.Ground);
                }
                else if (BodySprite.CurrentAnimation.StartsWith("Ground") && BodySprite.CurrentFrameAnimation.PlayCount < 1)
                {
                    move = false;
                }
                else if (!BodySprite.CurrentAnimation.StartsWith("Idle") && BodySprite.CurrentFrameAnimation.PlayCount >= 1)
                {
                    bool jumping = BodySprite.CurrentAnimation.StartsWith("Air");
                    PlayAnimationVerb(jumping ? VerbEnum.Ground : VerbEnum.Air);
                    _vJumpTo = Vector2.Zero;
                }
            }

            if (move && _vMoveTo != Vector2.Zero)
            {
                float deltaX = Math.Abs(_vMoveTo.X - this.Position.X);
                float deltaY = Math.Abs(_vMoveTo.Y - this.Position.Y);

                Util.GetMoveSpeed(Position, _vMoveTo, BuffedSpeed, ref direction);
                DetermineFacing(direction);
                if (!CheckMapForCollisionsAndMove(direction, false))
                {
                    _iMoveFailures++;
                }

                //We have finished moving
                if (Position.X == _vMoveTo.X && Position.Y == _vMoveTo.Y)
                {
                    _vMoveTo = Vector2.Zero;
                    PlayAnimationVerb(TacticalCombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
                }
            }
        }

        private void GetIdleMovementTarget(GameTime gTime)
        {
            if ((_vMoveTo == Vector2.Zero && _dIdleFor <= 0 && !BodySprite.CurrentAnimation.StartsWith("Air")) || _iMoveFailures == 5)
            {
                _iMoveFailures = 0;
                int howFar = 2;
                bool skip = false;
                RHRandom rand = RHRandom.Instance();
                int decision = rand.Next(1, 5);
                if (decision == 1) { _vMoveTo = new Vector2(Position.X - rand.Next(1, howFar) * TILE_SIZE, Position.Y); }
                else if (decision == 2) { _vMoveTo = new Vector2(Position.X + rand.Next(1, howFar) * TILE_SIZE, Position.Y); }
                else if (decision == 3) { _vMoveTo = new Vector2(Position.X, Position.Y - rand.Next(1, howFar) * TILE_SIZE); }
                else if (decision == 4) { _vMoveTo = new Vector2(Position.X, Position.Y + rand.Next(1, howFar) * TILE_SIZE); }
                else
                {
                    _vMoveTo = Vector2.Zero;
                    _dIdleFor = 4;
                    PlayAnimationVerb(TacticalCombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
                    skip = true;
                }

                if (!skip)
                {
                    DetermineFacing(new Vector2(_vMoveTo.X - Position.X, _vMoveTo.Y - Position.Y));
                }
            }
            else if (_vMoveTo == Vector2.Zero)
            {
                _dIdleFor -= gTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public bool CheckValidConditions(SpawnConditionEnum s)
        {
            bool rv = true;

            if (_liSpawnConditions.Contains(s))
            {
                foreach (SpawnConditionEnum e in _liSpawnConditions)
                {
                    if (e.Equals(SpawnConditionEnum.Night) && !GameCalendar.IsNight())
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Spring))
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Summer))
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Winter))
                    {
                        rv = false;
                    }
                    else if (CompareSpawnSeason(e, SpawnConditionEnum.Fall))
                    {
                        rv = false;
                    }

                    if (!rv) { break; }
                }
            }

            return rv;
        }

        private bool CompareSpawnSeason(SpawnConditionEnum check, SpawnConditionEnum season)
        {
            return check.Equals(season) && !Util.ParseEnum<SpawnConditionEnum>(GameCalendar.GetSeason()).Equals(season);
        }

        public int GetRandomLootItem()
        {
            return _liLootIDs[RHRandom.Instance().Next(0, _liLootIDs.Count-1)];
        } 

        public override void KO()
        {
            base.KO();
            SpawnPoint.ClearSpawn();
            TacticalCombatManager.GiveXP(this);
            GameManager.SlainMonsters.Add(new GUISprite(BodySprite));
        }

        public override List<TacticalCombatAction> GetCurrentSpecials()
        {
            return _liCombatActions;
        }

        public void TakeTurn()
        {
            TravelManager.SetParams(_iSize, this, _iMoveSpeed);

            if (_liTurnSteps == null) {
                _thrPathing = new Thread(PlanTurn);
                _thrPathing.Start();
            }
        }

        private void PlanTurn()
        {
            //Acquire the TravelMap for the Monster
            _travelMap = TravelManager.FindRangeOfAction(this, _iMoveSpeed, true);

            //First, determine which action we would prefer to use
            foreach (TacticalCombatAction testAction in this.GetCurrentSpecials())
            {
                _liFoundPath = null;
                _liTurnSteps = new List<TurnStepsEnum>();

                _chosenAction = testAction;
                _chosenAction.AssignUser(this);

                //Determine if the characters we want to act on are on our TravelMap or not.
                List<TacticalCombatActor> activeTargets = new List<TacticalCombatActor>();
                List<TacticalCombatActor> distantTargets = new List<TacticalCombatActor>();

                bool skipThisSkill = true;
                foreach (TacticalCombatActor act in (_chosenAction.Target == TargetEnum.Enemy) ? TacticalCombatManager.Party : TacticalCombatManager.Monsters)
                {
                    if (ShouldTryToUseOnActor(act))
                    {
                        skipThisSkill = false;
                        DetermineRangeToActor(act, ref activeTargets, ref distantTargets);
                        //if (act.LinkedSummon != null) { DetermineRangeToActor(act.LinkedSummon, ref activeTargets, ref distantTargets); }
                    }
                    else { continue; }
                }
                //The skill is not usable, go to the next one
                if (skipThisSkill) { continue; }

                PathInfo pathingInfo = null;
                //First we only care about actors within the travelMap
                if (activeTargets.Count > 0)
                {
                    FindShortestPathToActor(activeTargets, ref pathingInfo);
                }

                //If no shortestParth exists, either because there are no targets in the travelMap or there is 
                //no valid path to them, because you cannot finish on an RHTile, iterate over the distant CombatActors.
                if (pathingInfo == null)
                {
                    FindShortestPathToActor(distantTargets, ref pathingInfo);
                }

                if(pathingInfo != null)
                {
                    _liFoundPath = pathingInfo.ActualPath;
                }

                if (_liFoundPath?.Count > 0)
                {
                    _liTurnSteps.Add(TurnStepsEnum.Move);

                    RHTile temp = this.BaseTile;
                    this.SetBaseTile(_liFoundPath[_liFoundPath.Count - 1], false);
                    if (InRangeForSkill(_selectedTile, _chosenAction.Range))
                    {
                        _liTurnSteps.Add(TurnStepsEnum.Act);
                        _chosenAction.AssignTargetTile(_selectedTile);
                    }
                    this.SetBaseTile(temp, false);
                }
                else
                {
                    if (InRangeForSkill(_selectedTile, _chosenAction.Range))
                    {
                        _liTurnSteps.Add(TurnStepsEnum.Act);
                        _chosenAction.AssignTargetTile(_selectedTile);

                        if (_bCoward)
                        {
                            bool run = false;
                            List<RHTile> partyTiles = new List<RHTile>();
                            foreach (TacticalCombatActor act in TacticalCombatManager.Party.FindAll(x => !x.KnockedOut())) {
                                foreach (RHTile t in GetTileList())
                                {
                                    if (Util.GetRHTileDelta(t, act.BaseTile) <= 3)
                                    {
                                        run = true;
                                    }
                                }
                                partyTiles.Add(act.BaseTile);
                            }
                            if (run)
                            {
                                _liFoundPath = TravelManager.FindPathAway(BaseTile, _travelMap, partyTiles);
                                if(_liTurnSteps.Count > 0) { _liTurnSteps.Add(TurnStepsEnum.Move); }
                            }
                        }
                    }
                }

                _liTurnSteps.Add(TurnStepsEnum.EndTurn);

                //After everything is done, clear the TravelManager data
                TravelManager.ClearParams();

                //If we cannot take an action this turn, try to find an action that we can work on
                if (_liTurnSteps.Contains(TurnStepsEnum.Act))
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Given an actor, determines whether or not we are capable of moving to them in this turn
        /// and adds them to the appropriate list
        /// </summary>
        /// <param name="act">The CombatActor to compare against</param>
        /// <param name="activeTargets">List of targets we can reach</param>
        /// <param name="distantTargets">List of targets we cannot reach</param>
        private void DetermineRangeToActor(TacticalCombatActor act, ref List<TacticalCombatActor> activeTargets, ref List<TacticalCombatActor> distantTargets)
        {
            bool found = false;
            foreach (RHTile adjTile in act.GetAdjacentTiles())
            {
                if (adjTile.Character == this || _travelMap.CanEndTurnHere(adjTile))
                {
                    found = true;
                    break;
                }
            }

            if (found) { activeTargets.Add(act); }
            else { distantTargets.Add(act); }
        }

        /// <summary>
        /// Determines whether the chosen action can or should be used on the targetted actor
        /// </summary>
        /// <param name="actor"></param>
        /// <returns></returns>
        private bool ShouldTryToUseOnActor(TacticalCombatActor actor)
        {
            bool rv = true;

            if (actor.KnockedOut()) { return false; }
            if (_chosenAction.Heal && actor.CurrentHP > (actor.MaxHP * 0.5)) { return false; }

            return rv;
        }

        public override void EndTurn()
        {
            _liTurnSteps = null;
        }

        /// <summary>
        /// Given a list of CombatActors, loops through the list, checking to determine if
        /// the path found by the helper method is shorter than the current shortest path.
        /// 
        /// The goal is to find the shortest path to a valid tile out of a list of actors.
        /// </summary>
        /// <param name="possibleTargets">List of targets to loop through</param>
        /// <param name="preferredAction">The action we want to use, pass to the helper</param>
        /// <param name="shortestPath">A reference to a list of RHTiles which will be the shortest path.</param>
        private void FindShortestPathToActor(List<TacticalCombatActor> possibleTargets, ref PathInfo shortestPath)
        {
            foreach (TacticalCombatActor act in possibleTargets)
            {
                PathInfo testInfo = ChooseTargetTile(act);
                if (shortestPath == null || (testInfo != null && (testInfo.Count() < shortestPath.Count() || ChooseBetweenEquals(testInfo.Count(), shortestPath.Count(), 50))))
                {
                    _selectedTile = act.BaseTile;
                    shortestPath = testInfo;
                    if (shortestPath.Count() == 0) { break; }
                }
            }
        }

        /// <summary>
        /// Given a CombatActor and a CombatAction, find the shortest path to the closest RHTile
        /// that is needed to use the given skill on them.
        /// </summary>
        /// <param name="targetActor"></param>
        /// <param name="preferredAction"></param>
        /// <returns></returns>
        private PathInfo ChooseTargetTile(TacticalCombatActor targetActor)
        {
            PathInfo pathInfo = new PathInfo();
            int skillRange = _chosenAction.Range;

            if (skillRange == 1)
            {
                foreach (RHTile tile in FindAdjacentTiles(targetActor))
                {
                    if (TravelManager.TestTileForSize(tile, true))
                    {
                        if (ShortestPathComparison(FindPath(tile), ref pathInfo))
                        {
                            pathInfo.Clean();
                            break;
                        }
                    }
                }
            }
            else
            {
                //Only need to loop due to ranged skills that target allies such as heals or buffs
                foreach (RHTile tile in targetActor.GetTileList())
                {
                    //If we are in range of the mob, don't bother trying to move
                    if (!InRangeForSkill(tile, skillRange))
                    {
                        ShortestPathComparison(FindPath(tile), ref pathInfo);

                        if (pathInfo.ActualPath != null)
                        {
                            int backTrack = 1;
                            //We are moving, but now we want to make sure we will only move as far as we need to in order to use the skill.
                            RHTile lastTile = pathInfo.ActualPath[pathInfo.ActualPath.Count - 1];
                            while (Util.GetRHTileDelta(lastTile, tile) < skillRange)
                            {
                                pathInfo = TravelManager.FindPathViaTravelMap(lastTile, _travelMap);
                                lastTile = pathInfo.ActualPath[pathInfo.ActualPath.Count - backTrack];
                                if (backTrack < pathInfo.ActualPath.Count) { backTrack++; }
                                if (lastTile.HasCombatant()) { break; }
                            }
                        }
                        else
                        {
                            pathInfo.Clean();
                            break;
                        }
                    }
                    else
                    {
                        pathInfo.Clean();
                        break;
                    }
                }
            }

            return pathInfo;
        }

        /// <summary>
        /// Determines whether the indicated tile is within range of the monster.
        /// This takes into account that the monster's entire tile list is used
        /// as an origin point for the skill range.
        /// </summary>
        /// <param name="targetTile">The Tile to check the delta of</param>
        /// <param name="skillRange">Range of the skill</param>
        /// <returns>True if there is at least one RHTile within the monster in rangeo f the target</returns>
        private bool InRangeForSkill(RHTile targetTile, int skillRange)
        {
            bool rv = false;
            foreach (RHTile t in this.GetTileList())
            {
                if (Util.GetRHTileDelta(t, targetTile) <= skillRange)
                {
                    rv = true;
                }
            }

            return rv;
        }

        private bool ShortestPathComparison(PathInfo testInfo, ref PathInfo pathingInfo)
        {
            bool rv = false;
            if (testInfo != null && (pathingInfo.WholePath == null || testInfo.Count() < pathingInfo.Count() || ChooseBetweenEquals(testInfo.Count(), pathingInfo.Count(), 50)))
            {
                pathingInfo = testInfo;
                if(pathingInfo.Count() == 0) {rv = true; }
            }
            return rv;
        }

        private void FindNearbyTravelMapTile(ref List<RHTile> path, int removeAt, int removeCount)
        {
            RHTile targetTile = path[path.Count - 1];   //The end tile of the complete shortest path
            path.RemoveRange(removeAt, removeCount);    //Remove all of the extra tiles beyond the range of the skill
            RHTile endTile = path[path.Count - 1];

            if (_travelMap.ContainsKey(endTile) && !_travelMap[endTile].InRange)
            {
                List<RHTile> shortestPath = new List<RHTile>();

                foreach (KeyValuePair<RHTile, TravelData> kvp in _travelMap)
                {
                    Vector2 start = kvp.Key.Position;
                    List<RHTile> testPath = TravelManager.FindPathToLocation(ref start, targetTile.Position);

                    if(shortestPath == null || testPath.Count < shortestPath.Count)
                    {
                        shortestPath = testPath;
                    }
                }

                foreach(RHTile t in shortestPath)
                {
                    if (_travelMap.ContainsKey(t)) {
                        path = _travelMap.Backtrack(t);
                        break;
                    }
                    else { continue; }
                }
            }
        }

        /// <summary>
        /// If the given paths are equal, there is a chance that we will switch to the new one instead
        /// of sticking to the one we already have.
        /// </summary>
        /// <param name="testCount">Length of the path to test</param>
        /// <param name="shortestCount">Length of the current shortest path</param>
        /// <param name="percent">The percentage chance to switch</param>
        /// <returns>True if we should switch</returns>
        private bool ChooseBetweenEquals(int testCount, int shortestCount, int percent)
        {
            return testCount == shortestCount && RHRandom.Instance().Next(0, 100) < percent;
        }

        private PathInfo FindPath(RHTile tile)
        {
            PathInfo pathingInfo = new PathInfo();

            if (_travelMap.ContainsKey(tile)) {
                List<RHTile> temp = _travelMap.Backtrack(tile);
                pathingInfo.WholePath = temp;
                pathingInfo.ActualPath = temp;
            }
            else { pathingInfo = TravelManager.FindPathViaTravelMap(tile, _travelMap); }

            return pathingInfo;
        }

        /// <summary>
        /// Given a CombatActor, determine which RHTiles are considered adjacent
        /// as far as the size of this Monster is concerned. Does not take into account 
        /// the actual availability of the RHTiles in question.
        /// </summary>
        /// <param name="target">The CombatActor we are trying to cozy up to.</param>
        /// <returns>A List of RHTiles that the Monster's BaseTile could be in to be adjacent</returns>
        private List<RHTile> FindAdjacentTiles(TacticalCombatActor target)
        {
            List<RHTile> rvList = new List<RHTile>();

            FindAdjacentTilesHelper(target.BaseTile, DirectionEnum.Up, DirectionEnum.Left, true, ref rvList);
            FindAdjacentTilesHelper(target.BaseTile, DirectionEnum.Left, DirectionEnum.Up, true, ref rvList);
            FindAdjacentTilesHelper(target.BaseTile, DirectionEnum.Down, DirectionEnum.Left, false, ref rvList);
            FindAdjacentTilesHelper(target.BaseTile, DirectionEnum.Right, DirectionEnum.Up, false, ref rvList);

            return rvList;
        }

        /// <summary>
        /// From the given RHTile, we determine which tiles around it are the "Adjacent" tiles
        /// when taking the Size of the Monster into account. This does not work properly if the
        /// target itself also takes up multiple tiles.
        /// </summary>
        /// <param name="actorTile">The RHTile to plan around</param>
        /// <param name="distanceDir">Direction of the initial movement</param>
        /// <param name="moveDir">Direction along which to slide the mob where it will be adjacent</param>
        /// <param name="getDistance">Whether we need to make room for the mob by adding more gaps</param>
        /// <param name="tileList">The TileList to fill out</param>
        private void FindAdjacentTilesHelper(RHTile actorTile, DirectionEnum distanceDir, DirectionEnum moveDir, bool getDistance, ref List<RHTile> tileList)
        {
            if (actorTile != null)
            {
                RHTile targetTile = actorTile;
                for (int i = 0; i < (getDistance ? _iSize : 1); i++)
                {
                    targetTile = targetTile.GetTileByDirection(distanceDir);
                }
                if (TravelManager.TestTileForSize(targetTile, true))
                {
                    tileList.Add(targetTile);
                }

                for (int i = 0; i < _iSize - 1; i++)
                {
                    targetTile = targetTile.GetTileByDirection(moveDir);
                    if (TravelManager.TestTileForSize(targetTile, true))
                    {
                        tileList.Add(targetTile);
                    }
                }
            }
        }
    }
}
