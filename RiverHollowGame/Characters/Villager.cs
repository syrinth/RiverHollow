﻿using Microsoft.Xna.Framework;
using RiverHollow.Characters.Lite;
using RiverHollow.CombatStuff;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;
using RiverHollow.WorldObjects;

namespace RiverHollow.Characters
{
    public class Villager : TravellingNPC
    {
        protected string _sStartMap;
        protected int _iHouseBuildingID = -1;

        protected Dictionary<int, bool> _diCollection;

        private int _iTaxMultiplier = 1;
        private bool _bCanBecomePregnant = false;
        public bool CanBecomePregnant => _bCanBecomePregnant;
        public bool Pregnant { get; set; }
        public bool Combatant { get; private set; } = false;
        private bool _bCanMarry = false;
        public bool CanBeMarried => _bCanMarry;

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

        protected VillagerSpawnStatus _eSpawnStatus = VillagerSpawnStatus.OffMap;
        public bool LivesInTown => _eSpawnStatus == VillagerSpawnStatus.HasHome;
        public bool SpawnOnTheMap => _eSpawnStatus != VillagerSpawnStatus.OffMap;

        List<Request> _liHousingRequests;

        int _iNextTimeKeyID = 0;

        //The Data containing the path they are currently on
        PathData _currentPathData;

        //The complete schedule with all pathing combinations
        protected Dictionary<string, List<Dictionary<string, string>>> _diCompleteSchedule;
        private string NextScheduledTime => _diCompleteSchedule[_sScheduleKey][_iNextTimeKeyID]["TimeKey"];

        string _sScheduleKey;

        public ClassedCombatant CombatVersion { get; private set; }

        public Villager(int id) : base(id)
        {
            _diCollection = new Dictionary<int, bool>();
        }

        //Copy Construcor for Cutscenes
        public Villager(Villager n) : this(n.ID)
        {
            _eActorType = WorldActorTypeEnum.Villager;
            Combatant = n.Combatant;
            CombatVersion = n.CombatVersion;

            _diDialogue = n._diDialogue;
            _sPortrait = n.Portrait;

            _iBodyWidth = n._sprBody.Width;
            _iBodyHeight = n._sprBody.Height;
            _sprBody = new AnimatedSprite(n.BodySprite);
        }

        public Villager(int index, Dictionary<string, string> stringData, bool loadanimations = true) : this(index)
        {
            _eActorType = WorldActorTypeEnum.Villager;
            _liHousingRequests = new List<Request>();
            _diRequiredObjectIDs = new Dictionary<int, int>();
            _diCompleteSchedule = new Dictionary<string, List<Dictionary<string, string>>>();

            ImportBasics(stringData, loadanimations);
        }

        protected override void ImportBasics(Dictionary<string, string> stringData, bool loadanimations = true)
        {
            base.ImportBasics(stringData, loadanimations);

            Util.AssignValue(ref _sStartMap, "StartMap", stringData);

            Util.AssignValue(ref _iTaxMultiplier, "TaxValue", stringData);

            Util.AssignValue(ref _bCanMarry, "CanMarry", stringData);
            Util.AssignValue(ref _bCanBecomePregnant, "CanBecomePregnant", stringData);

            Util.AssignValue(ref _iHouseBuildingID, "HouseID", stringData);
            Util.AssignValue(ref _iTotalMoneyEarnedReq, "TotalMoneyEarnedReq", stringData);

            if (stringData.ContainsKey("AtInn")) {
                _eSpawnStatus = VillagerSpawnStatus.WaitAtInn;
                GameManager.AddToInnQueue(this);
            }
            else if (stringData.ContainsKey("InTown")) { _eSpawnStatus = VillagerSpawnStatus.HasHome; }
            else if (!string.IsNullOrEmpty(_sStartMap)) { _eSpawnStatus = VillagerSpawnStatus.NonTownMap; }

            CombatVersion = new ClassedCombatant();
            //CombatVersion.SetName(Name);
            if (stringData.ContainsKey("Class"))
            {
                Combatant = true;
                CombatVersion.SetClass(DataManager.GetClassByIndex(int.Parse(stringData["Class"])));
                CombatVersion.AssignStartingGear();
                CombatVersion.SetName(Name());
            }
            else { CombatVersion.SetClass(new CharacterClass()); }

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
        }

        public override void Update(GameTime gTime)
        {
            //Only follow schedules ATM if they are active and not married
            if (_bOnTheMap && !Married)
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
                        _sprBody.PlayAnimation(animation);
                    }

                    if (_currentPathData.Wander)
                    {
                        _bCanWander = true;
                    }

                    _currentPathData = null;
                }
            }
        }

        public override void RollOver()
        {
            if (GameCalendar.DayOfWeek == 0)
            {
                CanGiveGift = true;
            }

            if (TownRequirementsMet())
            {
                HandleTravelTiming();
            }

            switch (_eSpawnStatus)
            {
                case VillagerSpawnStatus.OffMap:
                    CurrentMap?.RemoveActor(this);
                    CurrentMapName = string.Empty;
                    _iNextArrival = _iArrivalPeriod;
                    GameManager.RemoveFromInnQueue(this);
                    break;
                case VillagerSpawnStatus.HasHome:
                    GameManager.RemoveFromInnQueue(this);
                    goto default;
                case VillagerSpawnStatus.VisitInn:
                case VillagerSpawnStatus.WaitAtInn:
                    if (GameManager.GetInnPosition(this) == -1) {
                        if (!GameManager.RoomAtTheInn())
                        {
                            _iNextArrival = 1;
                            _eSpawnStatus = VillagerSpawnStatus.OffMap;
                        }
                        else
                        {
                            GameManager.AddToInnQueue(this);
                        }
                    }
                    goto default;
                default:
                    ClearPath();
                    MoveToSpawn();
                    break;
            }

            JoinPartyCheck();
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
        public void SendToTown()
        {
            _eSpawnStatus = VillagerSpawnStatus.WaitAtInn;
            GameManager.AddToInnQueue(this);
            MoveToSpawn();
        }
        public override bool HandleTravelTiming()
        {
            bool rv = false;

            if (_eSpawnStatus == VillagerSpawnStatus.VisitInn || _eSpawnStatus == VillagerSpawnStatus.OffMap)
            {
                rv = base.HandleTravelTiming();
                _eSpawnStatus =  rv ? VillagerSpawnStatus.VisitInn : VillagerSpawnStatus.OffMap;
            }

            return rv;
        }

        /// <summary>
        /// Quick call to see if the NPC's home is built. Returns false if they have no assigned home.
        /// </summary>
        public bool JustMovedIn()
        {
            switch (_eSpawnStatus)
            {
                case VillagerSpawnStatus.VisitInn:
                case VillagerSpawnStatus.WaitAtInn:
                    if (TownRequirementsMet() && _iHouseBuildingID != -1 && PlayerManager.TownObjectBuilt(_iHouseBuildingID))
                    {
                        _eSpawnStatus = VillagerSpawnStatus.HasHome;
                        return true;
                    }
                    break;
            }

            return false;
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
                return PlayerManager.PlayerHome.MapName;
            }
            else
            {
                switch (_eSpawnStatus)
                {
                    case VillagerSpawnStatus.VisitInn:
                    case VillagerSpawnStatus.WaitAtInn:
                        return "mapInn";
                    case VillagerSpawnStatus.HasHome:
                        return PlayerManager.GetBuildingByID(_iHouseBuildingID)?.MapName;
                    case VillagerSpawnStatus.NonTownMap:
                        return _sStartMap;
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
            _bOnTheMap = true;

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
                        case VillagerSpawnStatus.VisitInn:
                        case VillagerSpawnStatus.WaitAtInn:
                            strSpawn = "NPC_Wait_" + GameManager.GetInnPosition(this);
                            break;
                        case VillagerSpawnStatus.HasHome:
                        case VillagerSpawnStatus.NonTownMap:
                            strSpawn = "NPC_" + ID.ToString();
                            break;

                    }
                }

                Position = Util.SnapToGrid(map.GetCharacterSpawn(strSpawn));
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
            if ((LivesInTown  || !string.IsNullOrEmpty(_sStartMap)) && _diCompleteSchedule != null)
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
                            if (!PlayerManager.TownObjectBuilt(int.Parse(split[1])))
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
            Vector2 startPosition = nextTile != null ? nextTile.Position : Position;
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
                MoveToLocation = _liTilePath[0].Position;
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
                if (_diCollection.ContainsKey(item.ItemID))
                {
                    FriendshipPoints += _diCollection[item.ItemID] ? 50 : 20;
                    rv = GetDialogEntry("Collection");
                    int index = new List<int>(_diCollection.Keys).FindIndex(x => x == item.ItemID);

                    _diCollection[item.ItemID] = true;
                    MapManager.Maps[GetSpawnMapName()].AddCollectionItem(item.ItemID, ID, index);
                }
                else
                {
                    rv = GetDialogEntry("Gift");
                    FriendshipPoints += 1000;
                }
            }

            if (giftGiven)
            {
                item.Remove(1);
                CanGiveGift = false;
            }

            return rv;
        }

        public override TextEntry JoinParty()
        {
            TextEntry rv = null;
            if (Combatant)
            {
                _bOnTheMap = false;
                PlayerManager.AddToParty(CombatVersion);
                rv = GetDialogEntry("JoinPartyYes");
            }
            else
            {
                rv = GetDialogEntry("JoinPartyNo");
            }

            return rv;
        }

        public void JoinPartyCheck()
        {
            if (RelationshipState != RelationShipStatusEnum.None)
            {
                switch (_eSpawnStatus)
                {
                    case VillagerSpawnStatus.HasHome:
                    case VillagerSpawnStatus.VisitInn:
                    case VillagerSpawnStatus.WaitAtInn:
                        JoinParty();
                        break;
                }
            }
        }

        public int GetTaxes()
        {
            return 250 * (int)GetSatisfaction() * _iTaxMultiplier;
        }

        public SatisfactionStateEnum GetSatisfaction()
        {
            if(!PlayerManager.TownObjectBuilt(int.Parse(DataManager.Config[19]["ObjectID"]))){
                return SatisfactionStateEnum.Neutral;
            }

            int points = 40;
            foreach (Request r in _liHousingRequests)
            {
                points += r.ConditionsMet(_iHouseBuildingID);
            }

            if (points <= 10) { return SatisfactionStateEnum.Miserable; }
            else if (points >= 10 && points < 40) { return SatisfactionStateEnum.Sad; }
            else if (points >= 40 && points < 50) { return SatisfactionStateEnum.Neutral; }
            else if (points >= 50 && points < 70) { return SatisfactionStateEnum.Pleased; }
            else if (points >= 70 && points < 90) { return SatisfactionStateEnum.Happy; }
            else { return SatisfactionStateEnum.Ecastatic; }
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
                canGiveGift = CanGiveGift,
                spokenKeys = _liSpokenKeys,
                innPosition = GameManager.GetInnPosition(this),
            };
            
            if (CombatVersion!= null && CombatVersion.CharacterClass != null) { npcData.classedData = CombatVersion.SaveClassedCharData(); }

            return npcData;
        }
        public void LoadData(VillagerData data)
        {
            GameManager.AssignToPosition(this, data.innPosition);
            _eSpawnStatus = (VillagerSpawnStatus)data.spawnStatus;
            _iNextArrival = data.nextArrival;
            FriendshipPoints = data.friendshipPoints;
            CanGiveGift = data.canGiveGift;
            RelationshipState = (RelationShipStatusEnum)data.relationShipStatus;
            if (RelationshipState == RelationShipStatusEnum.Engaged || RelationshipState == RelationShipStatusEnum.Married)
            {
                PlayerManager.Spouse = this;
            }

            if (_iNextArrival <= 0 || !string.IsNullOrEmpty(_sStartMap))
            {
                MoveToSpawn();
                DetermineValidSchedule();
            }

            if (CombatVersion != null && CombatVersion.CharacterClass != null) {
                CombatVersion.LoadClassedCharData(data.classedData);
                JoinPartyCheck();
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
                if (PlayerManager.GetNumberTownObjects(ObjectID) < Number) { return 0; }

                RHMap houseMap = PlayerManager.GetTownObjectsByID(houseID)[0].CurrentMap;
                List<RHTile> validTiles = new List<RHTile>();
                List<RHTile> houseTiles = houseMap.GetTilesFromGridAlignedRectangle(PlayerManager.GetTownObjectsByID(houseID)[0].CollisionBox);

                for (int i = 0; i < houseTiles.Count; i++) {
                    foreach (RHTile t in houseMap.GetAllTilesInRange(houseTiles[i], 10))
                    {
                        Util.AddUniquelyToList(ref validTiles, t);
                    }
                }
            
                int numAtCorrectRange = 0;
                for (int i = 0; i < PlayerManager.GetTownObjectsByID(ObjectID).Count; i++)
                {
                    WorldObject obj = PlayerManager.GetTownObjectsByID(ObjectID)[i];
                    List<RHTile> objTiles = houseMap.GetTilesFromGridAlignedRectangle(obj.CollisionBox);

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

                if (numAtCorrectRange == PlayerManager.GetNumberTownObjects(ObjectID))
                {
                    return 10;
                }
                else { return 0; }
            }
        }
    }
}
