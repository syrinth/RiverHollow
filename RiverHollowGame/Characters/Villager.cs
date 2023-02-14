using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using RiverHollow.WorldObjects;

namespace RiverHollow.Characters
{
    public class Villager : TravellingNPC
    {
        public string StartMap => DataManager.GetStringByIDKey(ID, "StartMap", DataType.NPC);
        public int HouseID => DataManager.GetIntByIDKey(ID, "HouseID", DataType.NPC, 1);
        public bool Marriable => DataManager.GetBoolByIDKey(ID, "CanMarry", DataType.NPC);
        public bool CanBecomePregnant => DataManager.GetBoolByIDKey(ID, "CanBecomePregnant", DataType.NPC);

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

        protected SpawnStateEnum _eSpawnStatus = SpawnStateEnum.OffMap;
        public bool LivesInTown => _eSpawnStatus == SpawnStateEnum.HasHome;
        public bool SpawnOnTheMap => _eSpawnStatus != SpawnStateEnum.OffMap;

        List<Request> _liHousingRequests;

        int _iNextTimeKeyID = 0;

        //The Data containing the path they are currently on
        PathData _currentPathData;

        //The complete schedule with all pathing combinations
        protected Dictionary<string, List<Dictionary<string, string>>> _diCompleteSchedule;
        private string NextScheduledTime => _diCompleteSchedule[_sScheduleKey][_iNextTimeKeyID]["TimeKey"];

        string _sScheduleKey;

        public Villager(int index, Dictionary<string, string> stringData) : base(index, stringData)
        {
            _liHousingRequests = new List<Request>();
            _diCollection = new Dictionary<int, bool>();
            _diItemMoods = new Dictionary<int, MoodEnum>();
            _diRequiredObjectIDs = new Dictionary<int, int>();
            _diCompleteSchedule = new Dictionary<string, List<Dictionary<string, string>>>();

            if (!string.IsNullOrEmpty(StartMap))
            {
                _eSpawnStatus = SpawnStateEnum.NonTownMap;
            }


            if (stringData.ContainsKey("Collection"))
            {
                string[] vectorSplit = Util.FindParams(stringData["Collection"]);
                foreach (string s in vectorSplit)
                {
                    _diCollection.Add(int.Parse(s), false);
                }
            }

            Dictionary<string, List<string>> schedule = DataManager.GetSchedule("NPC_" + stringData["Key"]);
            if (schedule != null)
            {
                foreach (KeyValuePair<string, List<string>> kvp in schedule)
                {
                    List<Dictionary<string, string>> pathingData = new List<Dictionary<string, string>>();
                    foreach (string s in kvp.Value)
                    {
                        Dictionary<string, string> taggedDictionary = DataManager.TaggedStringToDictionary(s);
                        string timeKey = taggedDictionary["Hour"] + ":" + taggedDictionary["Minute"];
                        taggedDictionary["TimeKey"] = timeKey;
                        pathingData.Add(taggedDictionary);
                    }
                    _diCompleteSchedule.Add(kvp.Key, pathingData);
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
        }

        public override void Update(GameTime gTime)
        {
            //Only follow schedules ATM if they are active and not married
            if (OnTheMap && !Married)
            {
                //Only start to find a path if we are not currently on one.
                if (_pathingThread == null && _liTilePath.Count == 0 && _diCompleteSchedule != null && _sScheduleKey != null && _diCompleteSchedule[_sScheduleKey].Count > _iNextTimeKeyID && NextScheduledTime == GameCalendar.GetTime())
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
                if (stillMoving && _liTilePath.Count == 0)
                {
                    string direction = _currentPathData.Direction;
                    string animation = _currentPathData.Animation;
                    if (!string.IsNullOrEmpty(direction))
                    {
                        Facing = Util.ParseEnum<DirectionEnum>(direction);
                        PlayAnimation(VerbEnum.Idle, Facing);
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
            }
        }

        public override void RollOver()
        {
            GiftedToday = false;
            if (GameCalendar.DayOfWeek == 0)
            {
                WeeklyGiftGiven = false;
            }

            bool startedDayInTown = _eSpawnStatus == SpawnStateEnum.WaitAtInn || _eSpawnStatus == SpawnStateEnum.VisitInn || _eSpawnStatus == SpawnStateEnum.HasHome;
            if (TownRequirementsMet())
            {
                HandleTravelTiming();
            }

            switch (_eSpawnStatus)
            {
                case SpawnStateEnum.OffMap:
                    CurrentMap?.RemoveActor(this);
                    CurrentMapName = string.Empty;
                    if (ArrivalPeriod > 0)
                    {
                        _iNextArrival = ArrivalPeriod;
                    }
            
                    break;
                case SpawnStateEnum.SendingToInn:
                    SendToTown();
                    goto default;
                case SpawnStateEnum.VisitInn:
                case SpawnStateEnum.WaitAtInn:
                    if (!startedDayInTown)
                    {
                        SpawnPets();
                    }
                    goto default;
                case SpawnStateEnum.HasHome:
                default:
                    ClearPath();
                    break;
            }

            VillagerMapHandling();
        }

        /// <summary>
        /// Returns the initial text for when the Actor is first talked to.
        /// </summary>
        /// <returns>The text string to display</returns>
        public override TextEntry GetOpeningText()
        {
            TextEntry rv = null;

            foreach (RHTask q in TaskManager.TaskLog)
            {
                q.AttemptProgress(this);
            }

            if (!Introduced)
            {
                rv = GetDialogEntry("Introduction");
                RelationshipState = RelationShipStatusEnum.Friends;
                _bHasTalked = true;
            }
            else if (!CheckTaskLog(ref rv))
            {
                if (_assignedTask != null) {
                    _assignedTask.TaskIsTalking();
                    rv = _diDialogue[_assignedTask.StartTaskDialogue];
                }
                else if (CurrentMap.TheShop != null && CurrentMap.TheShop.ShopkeeperID == ID) { rv = _diDialogue["ShopOpen"]; }
                else if (!_bHasTalked) { rv = GetDailyDialogue(); }
                else
                {
                    rv = _diDialogue["Selection"];
                }
            }

            return rv;
        }

        #region Travel Methods
        public void QueueSendToTown()
        {
            _eSpawnStatus = SpawnStateEnum.SendingToInn;
        }
        public void SendToTown()
        {
            if (_eSpawnStatus == SpawnStateEnum.SendingToInn)
            {
                _eSpawnStatus = SpawnStateEnum.WaitAtInn;
                MoveToSpawn();
            }
        }
        public override bool HandleTravelTiming()
        {
            bool rv = false;

            if (_eSpawnStatus == SpawnStateEnum.OffMap || (_eSpawnStatus == SpawnStateEnum.VisitInn && ArrivalPeriod > 0 ))
            {
                rv = base.HandleTravelTiming();
                _eSpawnStatus =  rv ? SpawnStateEnum.VisitInn : SpawnStateEnum.OffMap;
            }

            return rv;
        }

        public void VillagerMapHandling()
        {
            TryMoveIn();

            if (SpawnOnTheMap)
            {
                MoveToSpawn();
                DetermineValidSchedule();
            }
        }
        /// <summary>
        /// Quick call to see if the NPC's home is built.
        /// </summary>
        public void TryMoveIn()
        {
            if (TownRequirementsMet() && HouseID != -1 && TownManager.TownObjectBuilt(HouseID))
            {
                TaskManager.AdvanceTaskProgress();
                _eSpawnStatus = SpawnStateEnum.HasHome;
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
                return PlayerManager.PlayerHome.BuildingMapName;
            }
            else
            {
                switch (_eSpawnStatus)
                {
                    case SpawnStateEnum.VisitInn:
                    case SpawnStateEnum.WaitAtInn:
                        return "mapInn";
                    case SpawnStateEnum.HasHome:
                        return TownManager.GetBuildingByID(HouseID)?.BuildingMapName;
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
            OnTheMap = true;

            string mapName = GetSpawnMapName();

            if (!string.IsNullOrEmpty(mapName))
            {
                CurrentMap?.RemoveCharacterImmediately(this);
                CurrentMapName = mapName;
                RHMap map = MapManager.Maps[mapName];

                string strSpawn = string.Empty;
                if (Married)
                {
                    strSpawn = "Spouse";
                }
                else
                {
                    switch (_eSpawnStatus)
                    {
                        case SpawnStateEnum.VisitInn:
                        case SpawnStateEnum.WaitAtInn:
                            Point position = map.DictionaryCharacterLayer["InnFloor"];
                            List<RHTile> tiles = map.GetTilesFromRectangleExcludeEdgePoints(new Rectangle((int)position.X, (int)position.Y, 8 * Constants.TILE_SIZE, 6 * Constants.TILE_SIZE));
                            do
                            {
                                RHTile tile = tiles[RHRandom.Instance().Next(tiles.Count)];
                                if (tile.TileIsPassable() && !map.TileContainsActor(tile))
                                {
                                    Position = tile.Position;
                                    break;
                                }
                                else { tiles.Remove(tile); }

                            } while (tiles.Count > 0);
                            break;
                        case SpawnStateEnum.HasHome:
                        case SpawnStateEnum.NonTownMap:
                            Position = Util.SnapToGrid(map.GetCharacterSpawn("NPC_" + ID.ToString()));
                            break;

                    }
                }

                
                map.AddCharacterImmediately(this);
            }
        }
        #endregion

        #region Pathing Handlers
        /// <summary>
        /// This method determines which schedule key to follow as well as which time key
        /// to wait for for the next pathing calculation.
        /// </summary>
        public void DetermineValidSchedule()
        {
            if ((LivesInTown  || !string.IsNullOrEmpty(StartMap)) && _diCompleteSchedule != null)
            {
                //Iterate through the schedule keys
                foreach (string scheduleKey in _diCompleteSchedule.Keys)
                {
                    bool valid = true;
                    string[] args = Util.FindParams(scheduleKey);

                    //For each schedule argument, tryto find a failure. Any failure
                    //breaks out of the argument check and invalidates the scheduleKey
                    foreach (string arg in args)
                    {
                        string[] split = arg.Split(':');
                        if (split[0].Equals("Built"))
                        {
                            if (!TownManager.TownObjectBuilt(int.Parse(split[1])))
                            {
                                valid = false;
                                break;
                            }
                        }
                    }

                    if (valid)
                    {
                        _sScheduleKey = scheduleKey;

                        //We now need to compare the current time against each of the schedule time keys
                        //We're looking for the next key to act on. 
                        foreach (Dictionary<string, string> timeKey in _diCompleteSchedule[_sScheduleKey])
                        {
                            string currentTime = GameCalendar.GetTime();
                            TimeSpan currTimeSpan = TimeSpan.ParseExact(currentTime, "h\\:mm", CultureInfo.CurrentCulture, TimeSpanStyles.None);
                            TimeSpan keyTime = TimeSpan.ParseExact(timeKey["TimeKey"], "h\\:mm", CultureInfo.CurrentCulture, TimeSpanStyles.None);
                            if (TimeSpan.Compare(currTimeSpan, keyTime) <= 0)
                            {
                                _iNextTimeKeyID = _diCompleteSchedule[_sScheduleKey].IndexOf(timeKey);
                                break;
                            }
                        }

                        break;
                    }
                }
            }
        }

        /// <summary>
        /// IF the Villager is in town and is currently on a path, request a new
        /// pathing calculation.
        /// </summary>
        public void RecalculatePath()
        {
            if (LivesInTown && _currentPathData != null)
            {
                TravelManager.RequestPathing(this);
            }
        }

        /// <summary>
        /// This method constructs the PathData and calls out to the TravelManager to
        /// get the shortest path to the appropriate target
        /// </summary>
        protected override void CalculatePath()
        {
            int timeKeyIndex = _currentPathData == null ? _iNextTimeKeyID : _currentPathData.TimeKeyIndex;
            Dictionary<string, string> pathingData = _diCompleteSchedule[_sScheduleKey][timeKeyIndex];

            RHTile nextTile = _liTilePath.Count > 0 ? _liTilePath[0] : null;
            Point startPosition = nextTile != null ? nextTile.Position : Position;
            List<RHTile> timePath = TravelManager.FindRouteToLocation(pathingData["Location"], CurrentMapName, startPosition, Name());

            string direction = string.Empty;
            string animation = string.Empty;

            Util.AssignValue(ref direction, "Dir", pathingData);
            Util.AssignValue(ref animation, "Anim", pathingData);
            _currentPathData = new PathData(timePath, direction, animation, timeKeyIndex, pathingData.ContainsKey("Wander"));

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

            //Set the next TimeKey to watch out for
            _iNextTimeKeyID = timeKeyIndex + 1;
            TravelManager.FinishThreading(ref _pathingThread);
        }
        #endregion

        protected bool CheckTaskLog(ref TextEntry taskEntry)
        {
            bool rv = false;

            foreach (RHTask t in TaskManager.TaskLog)
            {
                if (t.ReadyForHandIn && t.GoalNPC == this)
                {
                    string taskCompleteKey = string.Empty;
                    t.TurnInTask();

                    taskEntry = _diDialogue[t.EndTaskDialogue];

                    ModifyTaskGoalValue(-1);
                    rv = true;
                    break;
                }
            }

            return rv;
        }

        public override TextEntry Gift(Item item)
        {
            TextEntry rv = null;

            bool giftGiven = true;
            if (item != null)
            {
                if (_diCollection.ContainsKey(item.ID) && !_diCollection[item.ID])
                {
                    FriendshipPoints += 50;
                    rv = GetDialogEntry("Collection");
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
                                WeeklyGiftGiven = true;
                                FriendshipPoints += 20;
                                rv = GetDialogEntry("Gift_Happy");
                                break;
                            case MoodEnum.Pleased:
                                WeeklyGiftGiven = true;
                                FriendshipPoints += 10;
                                rv = GetDialogEntry("Gift_Pleased");
                                break;
                            case MoodEnum.Neutral:
                                FriendshipPoints += 5;
                                rv = GetDialogEntry("Gift_Neutral");
                                break;
                            case MoodEnum.Sad:
                                FriendshipPoints -= 2;
                                rv = GetDialogEntry("Gift_Sad");
                                break;
                            case MoodEnum.Miserable:
                                FriendshipPoints -= 10;
                                rv = GetDialogEntry("Gift_Miserable");
                                break;
                        }
                    }
                }
            }

            return rv;
        }

        public MoodEnum ItemOpinion(Item it)
        {
            if (_diItemMoods.ContainsKey(it.ID))
            {
                return _diItemMoods[it.ID];
            }

            if (it.IsItemGroup(ItemGroupEnum.Gem))
            {
                return MoodEnum.Happy;
            }
            if (it.IsItemGroup(ItemGroupEnum.Magic))
            {
                return MoodEnum.Pleased;
            }
            if (it.IsItemGroup(ItemGroupEnum.Flower))
            {
                return MoodEnum.Pleased;
            }
            if (it.IsItemGroup(ItemGroupEnum.Ore))
            {
                return MoodEnum.Sad;
            }
            if (it.IsItemGroup(ItemGroupEnum.None))
            {
                return MoodEnum.Sad;
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
            string petInfo = DataManager.GetStringByIDKey(ID, "PetID", DataType.NPC);
            int petHome = DataManager.GetIntByIDKey(ID, "PetHomeID", DataType.NPC);
            if (!string.IsNullOrEmpty(petInfo) && !TownManager.TownObjectBuilt(petHome))
            {
                int[] split = Util.FindIntArguments(petInfo);
                int num = split.Length > 1 ? split[1] : 1;

                for (int i = 0; i < num; i++)
                {
                    TownManager.AddAnimal(DataManager.CreateAnimal(split[0]));
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
                spawnStatus = (int)_eSpawnStatus,
                nextArrival = _iNextArrival,
                friendshipPoints = FriendshipPoints,
                collection = new List<bool>(_diCollection.Values),
                relationShipStatus = (int)RelationshipState,
                weeklyGiftGiven = WeeklyGiftGiven,
                spokenKeys = _liSpokenKeys,
            };

            return npcData;
        }
        public void LoadData(VillagerData data)
        {
            _eSpawnStatus = (SpawnStateEnum)data.spawnStatus;
            _iNextArrival = data.nextArrival;
            FriendshipPoints = data.friendshipPoints;
            WeeklyGiftGiven = data.weeklyGiftGiven;
            RelationshipState = (RelationShipStatusEnum)data.relationShipStatus;
            if (RelationshipState == RelationShipStatusEnum.Engaged || RelationshipState == RelationShipStatusEnum.Married)
            {
                PlayerManager.Spouse = this;
            }

            if (_iNextArrival <= 0 || !string.IsNullOrEmpty(StartMap))
            {
                DetermineValidSchedule();
            }

            foreach (string s in data.spokenKeys)
            {
                _diDialogue[s].Spoken(this);
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
        private class PathData
        {
            public List<RHTile> Path { get; }
            public string Direction { get; }
            public string Animation { get; }
            public int TimeKeyIndex { get; }
            public bool Wander { get; }

            public PathData(List<RHTile> path, string direction, string animation, int timeKeyIndex, bool wander = false)
            {
                Path = path;
                Direction = direction;
                Animation = animation;
                TimeKeyIndex = timeKeyIndex;
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
                List<RHTile> houseTiles = houseMap.GetTilesFromRectangleExcludeEdgePoints(TownManager.GetTownObjectsByID(houseID)[0].CollisionBox);

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
                    List<RHTile> objTiles = houseMap.GetTilesFromRectangleExcludeEdgePoints(obj.CollisionBox);

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
