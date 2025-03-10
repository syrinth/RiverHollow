﻿using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class Villager : TravellingNPC
    {
        public string SpawnID => Constants.MAPOBJ_HOME + ID;
        public string StartMap => GetStringByIDKey("StartMap");
        public int HouseID => GetIntByIDKey("HouseID", 1);
        public bool Marriable => GetBoolByIDKey("CanMarry");
        public bool CanBecomePregnant => GetBoolByIDKey("CanBecomePregnant");

        public int[] FriendIDs => GetIntParamsByIDKey("Friends");

        private Dictionary<int, MoodEnum> _diItemMoods;

        public bool GiftedToday = false;
        public bool WeeklyGiftGiven = false;

        public bool CanGiveGift => !WeeklyGiftGiven && !GiftedToday;

        protected Dictionary<int, bool> _diCollection;

        public bool Pregnant { get; set; }
        public bool CanBeMarried => Marriable;

        public override RelationShipStatusEnum RelationshipState
        {
            set
            {
                base.RelationshipState = value;
                if (RelationshipState == RelationShipStatusEnum.Married) { PlayerManager.Spouse = this; }
                else if (RelationshipState == RelationShipStatusEnum.Engaged)
                {
                    PlayerManager.WeddingCountdown = 3;
                    PlayerManager.Spouse = this;
                }
            }
        }
        public bool Married => RelationshipState == RelationShipStatusEnum.Married;

        public SpawnStateEnum SpawnStatus { get;  protected set; } = SpawnStateEnum.OffMap;
        public bool LivesInTown => SpawnStatus == SpawnStateEnum.HasHome;
        public bool SpawnsOnAMap => SpawnStatus != SpawnStateEnum.OffMap;

        readonly List<Request> _liHousingRequests;

        //The Data containing the path they are currently on
        PathData _currentPathData;
        protected List<KeyValuePair<string, NPCActionState>> _liSchedule;
        public bool HasSchedule => _liSchedule?.Count > 0;
        public NPCActionState CurrentActionState { get; private set; }
        protected override string SpriteName()
        {
            return DataManager.VILLAGER_FOLDER + GetStringByIDKey("Key");
        }

        public Villager(int index, Dictionary<string, string> stringData) : base(index, stringData)
        {
            _liHousingRequests = new List<Request>();
            _liSchedule = new List<KeyValuePair<string, NPCActionState>>();
            _diCollection = new Dictionary<int, bool>();
            _diItemMoods = new Dictionary<int, MoodEnum>();

            if (!string.IsNullOrEmpty(StartMap))
            {
                SpawnStatus = SpawnStateEnum.NonTownMap;
            }

            if (stringData.ContainsKey("Collection"))
            {
                string[] vectorSplit = Util.FindParams(stringData["Collection"]);
                foreach (string s in vectorSplit)
                {
                    _diCollection.Add(int.Parse(s), false);
                }
            }

            if (stringData.ContainsKey("HappyID"))
            {
                var ids = Util.FindIntParams(stringData["HappyID"]);
                ids.ForEach(x => _diItemMoods[x] = MoodEnum.Happy);
            }
            if (stringData.ContainsKey("PleasedID"))
            {
                var ids = Util.FindIntParams(stringData["PleasedID"]);
                ids.ForEach(x => _diItemMoods[x] = MoodEnum.Pleased);
            }
            if (stringData.ContainsKey("NeutralID"))
            {
                var ids = Util.FindIntParams(stringData["NeutralID"]);
                ids.ForEach(x => _diItemMoods[x] = MoodEnum.Neutral);
            }
            if (stringData.ContainsKey("SadID"))
            {
                var ids = Util.FindIntParams(stringData["SadID"]);
                ids.ForEach(x => _diItemMoods[x] = MoodEnum.Sad);
            }
            if (stringData.ContainsKey("MiserableID"))
            {
                var ids = Util.FindIntParams(stringData["MiserableID"]);
                ids.ForEach(x => _diItemMoods[x] = MoodEnum.Miserable);
            }
            if (stringData.ContainsKey("Inactive"))
            {
                Activate(false);
            }
        }

        public override void Update(GameTime gTime)
        {
            //Only follow schedules ATM if they are active and not married
            if (OnTheMap && !Married)
            {
                if (_pathingThread == null && _liTilePath.Count == 0 && _liSchedule.Count > 0 && Util.CompareTimeStrings(_liSchedule[0].Key, GameCalendar.GetTime()))
                {
                    TravelManager.RequestPathing(this);
                }

                //Determine whether or not we are currently moving
                bool stillMoving = _liTilePath.Count > 0;

                //Call up to the base to handle normal Update methods
                //Movement is handled here
                base.Update(gTime);

                //If we ended out movement during the update, process any directional facing
                //And animations that may be requested
                if (stillMoving && _liTilePath.Count == 0 && _currentPathData != null)
                {
                    string animation = _currentPathData.Animation;
                    if (_currentPathData.Direction != DirectionEnum.None)
                    {
                        SetFacing(_currentPathData.Direction);
                        PlayAnimation(VerbEnum.Idle);
                    }

                    if (!string.IsNullOrEmpty(animation))
                    {
                        BodySprite.PlayAnimation(animation);
                    }

                    if (_currentPathData.Wander)
                    {
                        Wandering = true;
                    }

                    _currentPathData = null;
                }

                EmojiChecks(gTime);
            }
        }

        public override void RollOver()
        {
            TravelManager.FinishThreading(ref _pathingThread);

            ClearPath();
            GiftedToday = false;
            if (GameCalendar.DayOfWeek == 0)
            {
                WeeklyGiftGiven = false;
            }

            if (SpawnStatus == SpawnStateEnum.OffMap && CheckValidate())
            {
                SpawnStatus = SpawnStateEnum.WaitAtInn;
                SpawnPets();
            }

            MoveToSpawn();
            CreateDailySchedule();
        }

        public override bool DisplayIcons()
        {
            return SpawnStatus != SpawnStateEnum.SendingToInn;
        }

        public void Introduce()
        {
            if (!Introduced)
            {
                RelationshipState = RelationShipStatusEnum.Friends;
            }
        }

        /// <summary>
        /// Returns the initial text for when the Actor is first talked to.
        /// </summary>
        /// <returns>The text string to display</returns>
        public override TextEntry GetOpeningText()
        {
            TextEntry rv = new TextEntry();

            if (!_bHasTalked)
            {
                FriendshipPoints += Constants.TALK_FRIENDSHIP;
            }

            foreach (RHTask q in TaskManager.TaskLog)
            {
                q.AttemptProgress(this);
            }

            if (!Introduced)
            {
                rv = GetDialogEntry("Introduction");
                Introduce();
                _bHasTalked = true;
            }
            else if (PlayerCanHoldItems())
            {
                rv = DataManager.GetGameTextEntry("Give_Item");
            }
            else if (!CheckTaskLog(ref rv))
            {
                if (_assignedTask != null)
                {
                    _assignedTask.TaskIsTalking();
                    rv = GetDialogEntry(_assignedTask.StartTaskDialogue);
                }
                else if (!_bHasTalked) { rv = GetDailyDialogue(); }
                else
                {
                    rv = GetDialogEntry("Selection");
                }
            }

            return rv;
        }

        #region Travel Methods
        public void ReadySmokeBomb()
        {
            SpawnStatus = SpawnStateEnum.SendingToInn;
            MapManager.FadeOut();
        }
        public void SendToTown(bool force = false)
        {
            if (force || SpawnStatus == SpawnStateEnum.SendingToInn)
            {
                SpawnStatus = SpawnStateEnum.WaitAtInn;
                MoveToSpawn();
            }
        }

        public void SetStartingLocation()
        {
            TryToMoveIn();

            if (SpawnsOnAMap)
            {
                MoveToSpawn();
            }
        }

        //Try to move into Town
        public void TryToMoveIn()
        {
            if (HouseID != -1 && TownManager.TownObjectBuilt(HouseID) && SpawnStatus != SpawnStateEnum.HasHome)
            {
                TaskManager.AdvanceTaskProgress();
                SpawnStatus = SpawnStateEnum.HasHome;
            }
        }

        /// <summary>
        /// Call to return the name of the map the Villager should return to on Rollover.
        /// 
        /// If the NPC is married, they spawn in the house, otherwise if their home is built, they spawn there.
        /// 
        /// Otherwise, they spawn in the Inn.
        /// </summary>
        /// <returns>The string name of the map to put the NPC on</returns>
        protected string GetSpawnMapName()
        {
            string strSpawn = string.Empty;
            if (Married)
            {
                return PlayerManager.PlayerHome.InnerMapName;
            }
            else
            {
                switch (SpawnStatus)
                {
                    case SpawnStateEnum.WaitAtInn:
                        return "mapInn";
                    case SpawnStateEnum.HasHome:
                        return TownManager.GetBuildingByID(HouseID)?.InnerMapName;
                    case SpawnStateEnum.SendingToInn:
                    case SpawnStateEnum.NonTownMap:
                        return StartMap;
                }
            }
            return strSpawn;
        }

        /// <summary>
        /// Removes the NPC from the current map and moves them back
        /// to their home map and back to their spawn point.
        /// </summary>
        public override void MoveToSpawn()
        {
            //OnTheMap = _eSpawnStatus != SpawnStateEnum.OffMap;
 
            string mapName = GetSpawnMapName();

            CurrentMap?.RemoveCharacterImmediately(this);
            CurrentMapName = mapName;

            if (!string.IsNullOrEmpty(mapName))
            {
                RHMap map = MapManager.Maps[mapName];

                if (Married)
                {
                    //ToDo: Fix Marriage spawning for spouse
                }
                else
                {
                    switch (SpawnStatus)
                    {
                        case SpawnStateEnum.WaitAtInn:
                            SetPosition(map.GetRandomPosition(map.GetCharacterObject("Destination")));
                            break;
                        case SpawnStateEnum.HasHome:
                        case SpawnStateEnum.NonTownMap:
                            SetPosition(Util.SnapToGrid(map.GetCharacterSpawn(SpawnID)));
                            break;

                    }
                }

                SetFacing(DirectionEnum.Down);
                PlayAnimation(VerbEnum.Idle);
                map.AddCharacterImmediately(this);
            }
        }
        #endregion

        #region Pathing Handlers
        protected override void CheckBumpedIntoSomething()
        {
            if (_bBumpedIntoSomething && _liTilePath.Count > 0 && _liTilePath[0].MapName == CurrentMapName)
            {
                //Still unclear on why we are hitting objects. Suspect due to when we cut clip off RHTiles and we're doing it sloppily
                RequeueCurrentAction();
            }
        }

        public void RequeueCurrentAction()
        {
            _liTilePath.Clear();
            _liSchedule.Insert(0, new KeyValuePair<string, NPCActionState>(string.Empty, CurrentActionState));
            TravelManager.RequestPathing(this);
        }
        /// <summary>
        /// This method determines which schedule key to follow as well as which time key
        /// to wait for for the next pathing calculation.
        /// </summary>
        public void CreateDailySchedule()
        {
            _liSchedule.Clear();

            if (LivesInTown)
            {
                AddScheduleAction(NPCActionState.Home);
                AddScheduleAction(NPCActionState.Craft);
                AddScheduleAction(NPCActionState.Inn);
                AddScheduleAction(NPCActionState.Shop);
                AddScheduleAction(NPCActionState.Market);
                AddScheduleAction(NPCActionState.Visit);
                AddScheduleAction(NPCActionState.PetCafe);

                _liSchedule = _liSchedule.OrderBy(x => x.Key).ToList();
            }
        }
        protected void AddScheduleAction(NPCActionState actionState)
        {
            //ToDo: Need to be able to parse multiple actions in one day. For example:  Inn:09-30/14-30/Monday-11-00/Monday-18-00
            var timeKey = GetDefaultTime(actionState);
            var actionKey = Util.GetEnumString(actionState);
            var todayList = new List<KeyValuePair<string, NPCActionState>>();
            if (ValidActionInWeather(actionState))
            {
                var strParamsKey = GetBoolByIDKey(actionKey) ? GetStringByIDKey(actionKey) : timeKey;
                var strDataParams = Util.FindParams(strParamsKey);

                if (!strDataParams[0].Equals("Skip") && strDataParams.Length > 0)
                {
                    bool defBehaviour = true;
                    for (int i = 0; i < strDataParams.Length; i++)
                    {
                        var strData = Util.FindArguments(strDataParams[i]);

                        switch (strData.Length)
                        {
                            case 1:
                                CreateScheduleData(strData[0], actionState, ref todayList);
                                break;
                            case 2:
                                if (!strData[1].Equals("Skip"))
                                {
                                    CreateScheduleData(string.Format("{0}-{1}", strData[0], strData[1]), actionState, ref todayList);
                                }
                                else if (Util.ParseEnum<DayEnum>(strData[0]) == GameCalendar.DayOfWeek)
                                {
                                    todayList.Clear();
                                }
                                break;
                            case 3:
                                if (Util.ParseEnum<DayEnum>(strData[0]) == GameCalendar.DayOfWeek)
                                {
                                    if (defBehaviour)
                                    {
                                        defBehaviour = false;
                                        todayList.Clear();
                                    }
                                    CreateScheduleData(string.Format("{0}-{1}", strData[1], strData[2]), actionState, ref todayList);
                                }
                                break;

                        }
                    }
                }
            }

            _liSchedule.AddRange(todayList);
        }
        private string GetDefaultTime(NPCActionState actionState)
        {
            string rv = string.Empty;

            switch (actionState)
            {
                case NPCActionState.Inn:
                    switch (ActorType)
                    {
                        case ActorTypeEnum.Villager:
                            rv = Constants.VILLAGER_INN_DEFAULT;
                            break;
                        case ActorTypeEnum.Traveler:
                            rv = Constants.TRAVELER_INN_DEFAULT;
                            break;
                    }
                    break;
                case NPCActionState.Market:
                    switch (ActorType)
                    {
                        case ActorTypeEnum.Villager:
                            rv = Constants.VILLAGER_MARKET_DEFAULT;
                            break;
                        case ActorTypeEnum.Traveler:
                            rv = Constants.TRAVELER_MARKET_DEFAULT;
                            break;
                    }
                    break;
                case NPCActionState.Craft:
                    rv = Constants.VILLAGER_CRAFT_DEFAULT;
                    break;
                case NPCActionState.Shop:
                    switch (ActorType)
                    {
                        case ActorTypeEnum.Villager:
                            rv = Constants.VILLAGER_SHOP_DEFAULT;
                            break;
                        case ActorTypeEnum.Traveler:
                            rv = Constants.TRAVELER_SHOP_DEFAULT;
                            break;
                    }
                    break;
                case NPCActionState.Visit:
                    rv = Constants.VILLAGER_VISIT_DEFAULT;
                    break;
                case NPCActionState.Home:
                    rv = Constants.VILLAGER_HOME_DEFAULT;
                    break;
                case NPCActionState.PetCafe:
                    rv = Constants.VILLAGER_PETCAFE_DEFAULT;
                    break;
            }

            return rv;
        }
        private bool ValidActionInWeather(NPCActionState actionState)
        {
            bool rv = true;
            if (EnvironmentManager.IsRaining())
            {
                if (actionState == NPCActionState.Market)
                {
                    rv = false;
                }
            }

            return rv;
        }
        private void CreateScheduleData(string parameter, NPCActionState actionState, ref List<KeyValuePair<string, NPCActionState>> list)
        {
            var str = Util.FindArguments(parameter);
            var timeStr = str.Length == 1 ? str[0] : string.Format("{0}:{1}", str[0], str[1]);
            if (TimeSpan.TryParse(timeStr, out TimeSpan timeSpan))
            {
                var timeMod = GetTimeModifier();
                timeSpan = timeSpan.Add(timeMod);
                var timeKey = timeSpan.ToString();
                list.Add(new KeyValuePair<string, NPCActionState>(timeKey, actionState));
            }
        }

        private TimeSpan GetTimeModifier()
        {
            TimeSpan rv = TimeSpan.FromMinutes(RHRandom.Instance().Next(0, Constants.ACTION_DELAY) - Constants.ACTION_DELAY / 2);

            if (HasTrait(ActorTraitsEnum.Prompt))
            {
                rv = default;
            }
            else if (HasTrait(ActorTraitsEnum.Early))
            {
                rv = TimeSpan.FromMinutes(RHRandom.Instance().Next(0, Constants.ACTION_DELAY) - Constants.ACTION_DELAY * 2);
            }
            else if (HasTrait(ActorTraitsEnum.Late))
            {
                rv = TimeSpan.FromMinutes(RHRandom.Instance().Next(0, Constants.ACTION_DELAY));
            }

            return rv;
        }
        private bool ProcessActionStateData(out Point targetPosition, out string targetMapName, out DirectionEnum dir)
        {
            dir = DirectionEnum.Down;
            var currentAction = _liSchedule[0].Value;
            switch (currentAction)
            {
                case NPCActionState.Inn:
                    return ProcessActionStateDataHandler(TownManager.Inn.ID, "Destination", out targetPosition, out targetMapName);
                case NPCActionState.Craft:
                    dir = DirectionEnum.Up;
                    if (ProcessActionStateDataHandler(HouseID, Constants.MAPOBJ_CRAFT, out targetPosition, out targetMapName))
                    {
                        return true;
                    }
                    else { goto case NPCActionState.Home; }
                case NPCActionState.Shop:
                    if (ProcessActionStateDataHandler(HouseID, Constants.MAPOBJ_SHOP, out targetPosition, out targetMapName))
                    {
                        return true;
                    }
                    else { goto case NPCActionState.Home; }
                case NPCActionState.Market:
                    if (TownManager.Merchant != null)
                    {
                        return ProcessActionStateDataHandler(TownManager.Market, out targetPosition, out targetMapName);
                    }
                    else { goto case NPCActionState.Home; }
                case NPCActionState.Visit:
                    var friend = GetRandomFriend(x => true);
                    if (friend != null)
                    {
                        return ProcessActionStateDataHandler(friend.HouseID, "Visitor", out targetPosition, out targetMapName);
                    }
                    else { goto case NPCActionState.Home; }
                case NPCActionState.Home:
                    return ProcessActionStateDataHandler(HouseID, SpawnID, out targetPosition, out targetMapName);
                case NPCActionState.PetCafe:
                    if (TownManager.PetCafe != null)
                    {
                        return ProcessActionStateDataHandler(TownManager.PetCafe.ID, "Destination", out targetPosition, out targetMapName);
                    }
                    break;
            }

            targetPosition = Point.Zero;
            targetMapName = string.Empty;

            return false;
        }
        /// <summary>
        /// This method constructs the PathData and calls out to the TravelManager to
        /// get the shortest path to the appropriate target
        /// </summary>
        protected override void GetPathToNextAction()
        {
            if (ProcessActionStateData(out Point targetPosition, out string targetMapName, out DirectionEnum dir))
            {
                RHTile nextTile = _liTilePath.Count > 0 ? _liTilePath[0] : null;
                Point startPosition = nextTile != null ? nextTile.Position : CollisionBoxLocation;
                List<RHTile> timePath = TravelManager.FindRouteToPositionOnMap(targetPosition, targetMapName, CurrentMapName, startPosition, Name);

                string animation = string.Empty;
                _currentPathData = new PathData(timePath, dir, animation);

                //Keep the next tile in the path in order to keep things consistent
                if (nextTile != null)
                {
                    _currentPathData.Path.Insert(0, nextTile);
                }
                _liTilePath = _currentPathData.Path;

                if (_liTilePath?.Count > 0)
                {
                    SetMoveTo(_liTilePath[0].Position);
                }
            }

            TravelManager.FinishThreading(ref _pathingThread);

            CurrentActionState = _liSchedule[0].Value;
            _liSchedule.RemoveAt(0);
        }
        private bool ProcessActionStateDataHandler(int id, string locationName, out Point targetPosition, out string targetMapName)
        {
            bool rv = false;
            targetPosition = Point.Zero;

            var building = TownManager.GetBuildingByID(id);
            targetMapName = building.InnerMapName;

            if (FriendCheck(targetMapName, out targetPosition))
            {
                rv = true;
            }
            else
            {
                var r = building.InnerMap.GetCharacterObject(locationName);
                if (r != Rectangle.Empty)
                {
                    rv = true;
                    var tiles = MapManager.Maps[targetMapName].GetTilesFromRectangleExcludeEdgePoints(r).Where(x => x.Passable()).ToList();
                    RemoveOccupiedTiles(ref tiles);

                    if (TownManager.Merchant != null)
                    {
                        tiles.Remove(TownManager.Merchant.GetOccupantTile());
                    }

                    if (tiles.Count > 0)
                    {
                        RHTile targetTile = Util.GetRandomItem(tiles);
                        targetPosition = targetTile.Position;
                    }
                }
            }

            return rv;
        }
        private bool ProcessActionStateDataHandler(WorldObject obj, out Point targetPosition, out string targetMapName)
        {
            bool rv = false;

            targetMapName = MapManager.TownMap.Name;

            if (FriendCheck(targetMapName, out targetPosition)){
                rv = true;
            }
            else {
                var targetObjs = MapManager.TownMap.GetObjectsByID(obj.ID);
                var chosenObj = Util.GetRandomItem(targetObjs);

                var tiles = MapManager.TownMap.GetTilesFromRectangleExcludeEdgePoints(chosenObj.BaseRectangle).Where(x => x.Passable()).ToList();
                if (tiles.Count > 0)
                {
                    rv = true;
                    targetPosition = Util.GetRandomItem(tiles).Position;
                }
                else
                {
                    targetPosition = Point.Zero;
                }
            }

            return rv;
        }

        private bool FriendCheck(string targetMapName, out Point targetPosition)
        {
            bool rv = false;
            var currentAction = _liSchedule[0].Value;

            targetPosition = Point.Zero;
            if (HasTrait(ActorTraitsEnum.Anxious) || (RHRandom.RollPercent(Constants.WALK_TO_FRIEND_PERCENT) && _emoji?.Emoji != ActorEmojiEnum.Dots))
            {
                var chosenFriend = GetRandomFriend(x => x.GetOccupantTile() != null && x.GetOccupantTile().MapName.Equals(targetMapName));
                if (chosenFriend != null && chosenFriend.CurrentActionState == currentAction)
                {
                    var tile = chosenFriend.GetOccupantTile();
                    var tiles = new List<RHTile>();

                    AddWalkableTile(ref tiles, tile, DirectionEnum.Left);
                    AddWalkableTile(ref tiles, tile, DirectionEnum.Right);
                    RemoveOccupiedTiles(ref tiles);

                    if (tiles.Count > 0)
                    {
                        rv = true;
                        targetPosition = Util.GetRandomItem(tiles).Position;
                    }
                    else
                    {
                        targetPosition = Point.Zero;
                    }
                }
            }

            return rv;
        }
        private void AddWalkableTile(ref List<RHTile> tiles, RHTile tile, DirectionEnum dir)
        {
            if (tile.GetTileByDirection(dir).Passable())
            {
                tiles.Add(tile.GetTileByDirection(dir));
            }
        }

        private void RemoveOccupiedTiles(ref List<RHTile> tiles)
        {
            foreach (var v in TownManager.Villagers.Values)
            {
                tiles.Remove(v.GetOccupantTile());
            }
        }

        #endregion

        private bool EmojiStateSocial()
        {
            var validStates = new List<NPCActionState>() { NPCActionState.Visit, NPCActionState.Market, NPCActionState.Inn, NPCActionState.PetCafe };
            return validStates.Contains(CurrentActionState);
        }

        public void EmojiChecks(GameTime gTime)
        {
            if (LivesInTown && PeriodicEmojiReady(gTime))
            {
                if (!FollowingPath)
                {
                    if (CurrentActionState == NPCActionState.Craft)
                    {
                        if (RHRandom.RollPercent(Constants.EMOJI_SING_DEFAULT_RATE + TraitValue(ActorTraitsEnum.Musical)))
                        {
                            SetEmoji(ActorEmojiEnum.Sing, true);
                        }
                    }
                    else if (EmojiStateSocial())
                    {
                        //Looking for someone to Chat with
                        var tiles = GetOccupantTile().GetWalkableNeighbours(true);
                        foreach (var actor in CurrentMap.Actors)
                        {
                            if (tiles.Contains(actor.GetOccupantTile()) && !actor.FollowingPath)
                            {
                                if (HasTrait(ActorTraitsEnum.Anxious) && RHRandom.RollPercent(Constants.TRAIT_ANXIOUS_CHANCE))
                                {
                                    RequeueCurrentAction();
                                }
                                else if (HasTrait(ActorTraitsEnum.Recluse) && RHRandom.RollPercent(Constants.TRAIT_RECLUSE_CHANCE))
                                {
                                    SetEmoji(ActorEmojiEnum.Dots);
                                    RequeueCurrentAction();
                                    break;
                                }
                                else if (RHRandom.RollPercent(Constants.EMOJI_CHAT_DEFAULT_RATE + TraitValue(ActorTraitsEnum.Chatty)))
                                {
                                    SetEmoji(ActorEmojiEnum.Talk, true);
                                }
                                break;
                            }
                        }
                    }
                }
                
                if (_emoji == null && HasTrait(ActorTraitsEnum.Musical) && RHRandom.RollPercent(Constants.TRAIT_MUSICAL_BONUS))
                {
                    SetEmoji(ActorEmojiEnum.Sing);
                }

                _rhEmojiTimer.Reset(RHRandom.Instance().Next(3, 5));
            }
            
            //Off Work
            if (CurrentActionState == NPCActionState.Craft)
            {
                if (EmojiActionAboutToEnd(Constants.EMOJI_WORK_FINISHED_DEFAULT_RATE))
                {
                    SetEmoji(ActorEmojiEnum.Happy);
                }
            }

            //Going Home at night
            if (_liSchedule.Count == 1 && _liSchedule[0].Value == NPCActionState.Home)
            {
                if (EmojiActionAboutToEnd(Constants.EMOJI_SLEEPY_DEFAULT_RATE))
                {
                    SetEmoji(ActorEmojiEnum.Sleepy);
                }
            }
        }

        private bool EmojiActionAboutToEnd(int percentRate)
        {
            return _liSchedule.Count > 0 && Util.MinutesLeft(GameCalendar.GetTime(), _liSchedule[0].Key, 1) && RHRandom.RollPercent(percentRate);
        }

        public bool CheckTaskLog(ref TextEntry taskEntry)
        {
            bool rv = false;
             
            foreach (RHTask t in TaskManager.TaskLog)
            {
                if (t.ReadyForHandIn && t.GoalNPC() == this && !t.HasEndBuilding)
                {
                    rv = true;
                    TaskManager.QueuedHandin = t;
                    ModifyTaskGoalValue(-1);
                    taskEntry = GetDialogEntry(t.EndTaskDialogue);

                    break;
                }
            }

            return rv;
        }

        public override void Gift(Item item)
        {
            TextEntry response = null;

            bool giftGiven = true;
            if (item != null)
            {
                if (_diCollection.ContainsKey(item.ID) && !_diCollection[item.ID])
                {
                    FriendshipPoints += Constants.GIFT_COLLECTION;
                    response = GetDialogEntry("Collection");
                    int index = new List<int>(_diCollection.Keys).FindIndex(x => x == item.ID);

                    _diCollection[item.ID] = true;
                    MapManager.Maps[GetSpawnMapName()].AddCollectionItem(item.ID, ID, index);
                }
                else
                {
                    if (giftGiven)
                    {
                        item.Remove(1);
                        GiftedToday = true;

                        MoodEnum mood = ItemOpinion(item);
                        switch (mood)
                        {
                            case MoodEnum.Happy:
                                SetEmoji(ActorEmojiEnum.Heart);
                                WeeklyGiftGiven = true;
                                FriendshipPoints += Constants.GIFT_HAPPY;
                                response = GetDialogEntry("Gift_Happy");
                                break;
                            case MoodEnum.Pleased:
                                SetEmoji(ActorEmojiEnum.Heart);
                                WeeklyGiftGiven = true;
                                FriendshipPoints += Constants.GIFT_PLEASED;
                                response = GetDialogEntry("Gift_Pleased");
                                break;
                            case MoodEnum.Neutral:
                                SetEmoji(ActorEmojiEnum.Heart);
                                FriendshipPoints += Constants.GIFT_NEUTRAL;
                                response = GetDialogEntry("Gift_Neutral");
                                break;
                            case MoodEnum.Sad:
                                FriendshipPoints += Constants.GIFT_SAD;
                                response = GetDialogEntry("Gift_Sad");
                                break;
                            case MoodEnum.Miserable:
                                FriendshipPoints += Constants.GIFT_MISERABLE;
                                response = GetDialogEntry("Gift_Miserable");
                                break;
                        }
                    }
                }
            }

            GUIManager.CloseMainObject();
            GUIManager.OpenTextWindow(response, this);
        }

        public Villager GetRandomFriend(Func<Villager, bool> condition)
        {
            Villager chosenFriend = null;
            var possibleFriends = new List<Villager>();

            foreach (var id in FriendIDs)
            {
                Villager v = TownManager.Villagers[id];
                if (v.LivesInTown)
                {
                    possibleFriends.Add(v);
                }
            }

            possibleFriends = possibleFriends.Where(condition).ToList();
            if (possibleFriends.Count > 0)
            {
                chosenFriend = Util.GetRandomItem(possibleFriends);
            }

            return chosenFriend;
        }

        public MoodEnum ItemOpinion(Item it)
        {
            if (_diItemMoods.ContainsKey(it.ID))
            {
                return _diItemMoods[it.ID];
            }

            if (it.CompareType(ItemTypeEnum.Resource))
            {
                if (it.IsItemGroup(ResourceTypeEnum.Gem))
                {
                    return MoodEnum.Happy;
                }
                if (it.IsItemGroup(ResourceTypeEnum.Flower))
                {
                    return MoodEnum.Pleased;
                }
                if (it.IsItemGroup(ResourceTypeEnum.Ore))
                {
                    return MoodEnum.Sad;
                }
                if (it.IsItemGroup(ResourceTypeEnum.None))
                {
                    return MoodEnum.Sad;
                }
            }

            return MoodEnum.Neutral;
        }

        public MoodEnum GetSatisfaction()
        {
            if(!TownManager.TownObjectBuilt(int.Parse(DataManager.Config[19]["ObjectID"]))){
                return MoodEnum.Neutral;
            }

            int points = 40;
            foreach (Request r in _liHousingRequests)
            {
                points += r.ConditionsMet(HouseID);
            }

            if (points <= 10) { return MoodEnum.Miserable; }
            else if (points >= 10 && points < 40) { return MoodEnum.Sad; }
            else if (points >= 40 && points < 50) { return MoodEnum.Neutral; }
            else if (points >= 50 && points < 70) { return MoodEnum.Pleased; }
            else if (points >= 70 && points < 90) { return MoodEnum.Happy; }
            else { return MoodEnum.Ecstatic; }
        }

        private void SpawnPets()
        {
            string petInfo = GetStringByIDKey("PetID");
            int petHome = GetIntByIDKey("PetHomeID");
            if (!string.IsNullOrEmpty(petInfo) && !TownManager.TownObjectBuilt(petHome))
            {
                int[] split = Util.FindIntArguments(petInfo);
                int num = split.Length > 1 ? split[1] : 1;

                for (int i = 0; i < num; i++)
                {
                    TownManager.AddAnimal(DataManager.CreateActor<Animal>(split[0]));
                }
            }
        }

        public bool AvailableToDate() { return CanBeMarried && GetFriendshipLevel() >= 6 && RelationshipState == RelationShipStatusEnum.Friends; }
        public bool AvailableToMarry() { return CanBeMarried && GetFriendshipLevel() >= 6 && RelationshipState == RelationShipStatusEnum.Dating; }

        public VillagerData SaveData()
        {
            VillagerData npcData = new VillagerData()
            {
                npcID = ID,
                spawnStatus = (int)SpawnStatus,
                friendshipPoints = FriendshipPoints,
                collection = new List<bool>(_diCollection.Values),
                relationShipStatus = (int)RelationshipState,
                weeklyGiftGiven = WeeklyGiftGiven,
                spokenKeys = _liSpokenKeys,
                heldItems = _liHeldItems
            };

            return npcData;
        }
        public void LoadData(VillagerData data)
        {
            SpawnStatus = (SpawnStateEnum)data.spawnStatus;
            FriendshipPoints = data.friendshipPoints;
            WeeklyGiftGiven = data.weeklyGiftGiven;
            RelationshipState = (RelationShipStatusEnum)data.relationShipStatus;
            if (RelationshipState == RelationShipStatusEnum.Engaged || RelationshipState == RelationShipStatusEnum.Married)
            {
                PlayerManager.Spouse = this;
            }

            foreach (string s in data.spokenKeys)
            {
                GetDialogEntry(s).Spoken(this);
            }

            foreach (string s in data.heldItems)
            {
                string[] split = Util.FindArguments(s);
                int id = int.Parse(split[0]);
                int number = split.Length == 1 ? 1 : int.Parse(split[1]);

                AssignItemToNPC(id, number);
            }

            int index = 0;
            List<int> keys = new List<int>(_diCollection.Keys);
            foreach (int key in keys)
            {
                _diCollection[key] = data.collection[index++];
            }
        }

        /// <summary>
        /// Object representing the actionsthat need to be taken that are tied to a current path
        /// This includes the path being taken, the direction to face at the end, and the
        /// animation that mayneed to be played.
        /// </summary>
        protected class PathData
        {
            public List<RHTile> Path { get; }
            public DirectionEnum Direction { get; }
            public string Animation { get; }
            public bool Wander { get; }

            public PathData(List<RHTile> path, DirectionEnum direction, string animation)
            {
                Path = path;
                Direction = direction;
                Animation = animation;
            }
        }
        private class Request
        {
            public VillagerRequestEnum RequestRange { get; }
            public int ObjectID { get; }
            public int Number { get; }
            public bool Completed { get; set; } = false;

            public Request(VillagerRequestEnum e, int objID, int num = 1)
            {
                RequestRange = e;
                ObjectID = objID;
                Number = num;
            }

            public int ConditionsMet(int houseID)
            {
                if (TownManager.GetNumberTownObjects(ObjectID) < Number) { return 0; }

                RHMap houseMap = TownManager.GetTownObjectsByID(houseID)[0].CurrentMap;
                List<RHTile> validTiles = new List<RHTile>();
                List<RHTile> houseTiles = houseMap.GetTilesFromRectangleExcludeEdgePoints(TownManager.GetTownObjectsByID(houseID)[0].BaseRectangle);

                for (int i = 0; i < houseTiles.Count; i++) {
                    foreach (RHTile t in houseMap.GetAllTilesInRange(houseTiles[i], 10))
                    {
                        Util.AddUniquelyToList(ref validTiles, t);
                    }
                }
            
                int numAtCorrectRange = 0;
                for (int i = 0; i < TownManager.GetTownObjectsByID(ObjectID).Count; i++)
                {
                    WorldObject obj = TownManager.GetTownObjectsByID(ObjectID)[i];
                    List<RHTile> objTiles = houseMap.GetTilesFromRectangleExcludeEdgePoints(obj.BaseRectangle);

                    List<RHTile> inRange = objTiles.FindAll(o => validTiles.Contains(o));
                    switch (RequestRange)
                    {
                        case VillagerRequestEnum.Close:
                            if (inRange.Count != 0)
                            {
                                numAtCorrectRange++;
                            }
                            break;
                        case VillagerRequestEnum.Far:
                            if (inRange.Count != 0)
                            {
                                return -10;
                            }
                            break;
                        case VillagerRequestEnum.TownWide:
                            numAtCorrectRange++;
                            break;
                    }
                }

                if (numAtCorrectRange == TownManager.GetNumberTownObjects(ObjectID))
                {
                    return 10;
                }
                else { return 0; }
            }
        }
    }
}
