using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Characters
{
    public class TalkingActor : Actor
    {
        protected ActorFaceEnum _eFaceEnum;
        protected List<ActorFaceEnum> _liActorFaceQueue;
        protected string _sPortrait;
        public string Portrait => _sPortrait;

        protected Dictionary<string, TextEntry> _diDialogue;

        public static List<int> FriendRange = new List<int> { 0, 2000, 4000, 6000, 8000, 10000, 12000 };
        public int FriendshipPoints;

        protected bool _bHasTalked;
        protected int _iTaskGoals = 0;
        protected RHTask _assignedTask;

        protected List<string> _liSpokenKeys;
        protected List<string> _liHeldItems;

        //The Data containing the path they are currently on
        protected RHTimer _timer;
        protected PathData _currentPathData;
        protected List<ScheduleData> _liSchedule;
        public bool HasSchedule => _liSchedule?.Count > 0;
        public ScheduleData CurrentSchedule { get; private set; }

        public TalkingActor() : base()
        {
            _liActorFaceQueue = new List<ActorFaceEnum>();
            _liSpokenKeys = new List<string>();
            _liHeldItems = new List<string>();
        }
        public TalkingActor(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            FriendshipPoints = 0;
            _liSchedule = new List<ScheduleData>();
            _liActorFaceQueue = new List<ActorFaceEnum>();
            _liSpokenKeys = new List<string>();
            _liHeldItems = new List<string>();

            _diDialogue = DataManager.GetNPCDialogue(stringData["Key"]);

            List<AnimationData> liAnimationData = Util.LoadWorldAnimations(stringData);
            BodySprite = LoadSpriteAnimations(liAnimationData, SpriteName());
            PlayAnimationVerb(VerbEnum.Idle);
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            base.Draw(spriteBatch, useLayerDepth);
            if (OnTheMap && DisplayIcons())
            {
                Rectangle pos = new Rectangle(GetHoverPointLocation(), new Point(Constants.TILE_SIZE, Constants.TILE_SIZE));

                if(HasHeldItems())
                {
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.HUD_COMPONENTS), pos, GUIUtils.HELD_ITEM, Color.White, 0, Vector2.Zero, SpriteEffects.None, Constants.MAX_LAYER_DEPTH);
                }
                else if (_iTaskGoals > 0)
                {
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.HUD_COMPONENTS), pos, GUIUtils.QUEST_TURNIN, Color.White, 0, Vector2.Zero, SpriteEffects.None, Constants.MAX_LAYER_DEPTH);
                }
                else if (_emoji == null & _assignedTask?.TaskState == TaskStateEnum.Assigned)
                {
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.HUD_COMPONENTS), pos, GUIUtils.QUEST_NEW, Color.White, 0, Vector2.Zero, SpriteEffects.None, Constants.MAX_LAYER_DEPTH);
                }
            }
        }

        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            _timer?.TickDown(gTime);

            CheckBumpedIntoSomething();

            if (_bFollow)
            {
                switch (_eCurrentState)
                {
                    case NPCStateEnum.TrackPlayer:
                        if(IsActorType(ActorTypeEnum.Pet) && PlayerManager.PlayerInRange(CollisionCenter, Constants.FOLLOW_ARRIVED))
                        {
                            ChangeState(NPCStateEnum.Idle);
                        }
                        break;
                    default:
                        if (!BodySprite.IsCurrentAnimation(VerbEnum.Action1, Facing) && !PlayerManager.PlayerInRange(CollisionCenter, Constants.FOLLOW_ALERT_THRESHOLD))
                        {
                            ChangeState(NPCStateEnum.Alert);
                        }
                        break;

                }
            }

            if (!FollowingPath && _currentPathData != null)
            {
                Wandering = _currentPathData.Wander;
                _currentPathData = null;
            }

            ProcessStateEnum(gTime, true);
        }

        public virtual bool DisplayIcons() { return true; }
        public bool HasAssignedTask()
        {
            return _assignedTask != null && _assignedTask.TaskState == TaskStateEnum.Assigned;
        }
        public bool AssignTask(RHTask task)
        {
            bool rv = false;

            if (_assignedTask == null)
            {
                rv = true;
                _assignedTask = task;
            }

            return rv;
        }
        
        public RHTask GetAssignedTask()
        {
            return _assignedTask;
        }

        public void ModifyTaskGoalValue(int val)
        {
            _iTaskGoals += val;
        }

        public virtual void Gift(Item item) { }
        public virtual TextEntry JoinParty() { return null; }

        public virtual void StopTalking() {
            GameManager.SetCurrentNPC(null);
            ResetActorFace();

            if(_assignedTask?.TaskState == TaskStateEnum.Talking)
            {
                _assignedTask.AddTaskToLog(false);
                _assignedTask = null;
            }

            TaskManager.HandleQueuedTask();
        }

        public override void ProcessRightButtonClick()
        {
            StartConversation();
        }

        /// <summary>
        ///  Retrieves any opening text, processes it, then opens a text window
        /// </summary>
        /// <param name="facePlayer">Whether the NPC should face the player. Mainly used to avoid messing up a cutscene</param>
        public virtual void StartConversation(bool facePlayer = true)
        {
            FacePlayer(true);
            GUIManager.OpenTextWindow(GetOpeningText(), this, true, true);
        }

        /// <summary>
        /// Used when already talking to an NPC, gets the next dialog tag in the conversation
        /// and opens a new window for it.
        /// </summary>
        /// <param name="dialogTag">The dialog tag to talk with</param>
        public void ContinueConversation(string dialogTag)
        {
            TextEntry text = GetDialogEntry(dialogTag);
            GUIManager.OpenTextWindow(text, this, true, true);
        }

        public void TalkCutscene(TextEntry cutsceneEntry)
        {
            GUIManager.OpenTextWindow(cutsceneEntry, this, true, true);
        }

        /// <summary>
        /// Stand-in Virtual method to be overrriden. Should never get called.
        /// </summary>
        public virtual TextEntry GetOpeningText() { return new TextEntry(); }

        /// <summary>
        /// Base method to get a line of dialog from the dialog dictionary.
        /// 
        /// Mostly used for the "Talk" parameter or if the TalkingActor has no other options.
        /// </summary>
        /// <returns>The dialog string for the entry.</returns>
        public TextEntry GetDailyDialogue()
        {
            TextEntry rv = new TextEntry();

            _bHasTalked = true;
            PriorityQueue<TextEntry> keyPool = new PriorityQueue<TextEntry>();
            foreach (TextEntry entry in _diDialogue.Values)
            {
                if (entry.Validate(this))
                {
                    keyPool.Enqueue(entry, entry.Priority);
                }
            }

            if (keyPool.Count > 0)
            {
                List<TextEntry> possibles = keyPool.DequeueAllLowest();
                rv = possibles[RHRandom.Instance().Next(0, possibles.Count - 1)];
            }

            return rv;
        }

        /// <summary>
        /// Retrieves the specified entry from the _diDictionaryand calls Util.ProcessTexton it.
        /// </summary>
        /// <param name="entry">The key of the entry to get from the Dictionary</param>
        /// <returns>The processed string text for the entry </returns>
        public virtual TextEntry GetDialogEntry(string entry)
        {
            TextEntry rv = new TextEntry();

            if (_diDialogue.ContainsKey(entry))
            {
                rv = _diDialogue[entry];
            }

            return rv;
        }

        /// <summary>
        /// Determines the level of Friendship based off of how many Friendship points the Actor has.
        /// </summary>
        /// <returns></returns>
        public int GetFriendshipLevel()
        {
            int rv = 0;
            for (int i = 0; i < FriendRange.Count; i++)
            {
                if (FriendshipPoints >= FriendRange[i]) { rv = i; }
                else { break; }
            }

            return rv;
        }

        /// <summary>
        /// Creates the queue of faces the TalkingActor will progress through as they talk.
        /// </summary>
        /// <param name="value">A '-' delimited list of facial expressions corresponding to ActoEnumFaces</param>
        public void QueueActorFace(string value)
        {
            foreach (string s in Util.FindArguments(value))
            {
                _liActorFaceQueue.Add(Util.ParseEnum<ActorFaceEnum>(s));
            }
        }

        /// <summary>
        /// Removes the first entry from list of queued faces and sets it as the current face.
        /// Calling this again on an empty list resets the face to default
        /// </summary>
        public void DeQueueActorFace()
        {
            if (_liActorFaceQueue.Count == 0) { ResetActorFace(); }
            else
            {
                _eFaceEnum = _liActorFaceQueue[0];
                _liActorFaceQueue.RemoveAt(0);
            }
        }

        /// <summary>
        /// Clears the FaceQueue and resets the face to Default
        /// </summary>
        public void ResetActorFace()
        {
            _liActorFaceQueue.Clear();
            _eFaceEnum = ActorFaceEnum.Default;
        }

        public Rectangle GetPortraitRectangle()
        {
            int startX = 0;
            int width = 48;
            startX += (int)_eFaceEnum * width;
            return new Rectangle(startX, 0, width, 60);
        }

        public void AddSpokenKey(string key)
        {
            _liSpokenKeys.Add(key);
        }

        #region HeldItemHandling
        public bool HasHeldItems()
        {
            return _liHeldItems.Count > 0;
        }
        public void AssignItemToNPC(int id, int number)
        {
            _liHeldItems.Add(string.Format("{0}-{1}", id, number));
        }
        public bool PlayerCanHoldItems()
        {
            bool rv = _liHeldItems.Count > 0;

            for (int i = 0; i < _liHeldItems.Count; i++)
            {
                var split = Util.FindIntArguments(_liHeldItems[i]);
                if (!InventoryManager.HasSpaceInInventory(split[0], split[1]))
                {
                    rv = false;
                    break;
                }
            }

            return rv;
        }
        public void GiveItemsToPlayer()
        {
            int lastFound = -1;
            for (int i = 0; i < _liHeldItems.Count; i++)
            {
                lastFound = i;
                var split = Util.FindIntArguments(_liHeldItems[i]);
                if (InventoryManager.HasSpaceInInventory(split[0], split[1]))
                {
                    InventoryManager.AddToInventory(split[0], split[1]);
                }
                else
                {
                    break;
                }
            }

            if(lastFound > -1)
            {
                _liHeldItems.RemoveRange(0, lastFound + 1);
            }
        }

        public void CheckInventoryAlert()
        {
            //One alert per set of items
            if (HasHeldItems())
            {
                GUIManager.NewWarningAlertIcon(Constants.STR_ALERT_INVENTORY);
            }
        }
        #endregion

        #region Emojis And Traits
        protected bool EmojiActionAboutToEnd(int percentRate)
        {
            return _liSchedule.Count > 0 && Util.MinutesLeft(GameCalendar.GetTime(), _liSchedule[0].Time, 1) && RHRandom.RollPercent(percentRate);
        }

        protected bool PeriodicEmojiReady(GameTime gTime)
        {
            return _rhEmojiTimer.TickDown(gTime) && _emoji == null;
        }

        public bool HasTrait(ActorTraitsEnum e)
        {
            bool rv = false;

            var traits = GetStringParamsByIDKey("Traits").ToList();
            if (traits.Contains(Util.GetEnumString(e)))
            {
                rv = true;
            }

            return rv;
        }

        public int TraitValue(ActorTraitsEnum e)
        {
            if (HasTrait(e))
            {
                switch (e)
                {
                    case ActorTraitsEnum.Chatty:
                        return Constants.TRAIT_CHATTY_BONUS;
                    case ActorTraitsEnum.Musical:
                        return Constants.TRAIT_MUSICAL_BONUS;
                }
            }

            return 0;
        }
        #endregion

        #region Scheduling
        public void RequeueCurrentAction()
        {
            _liTilePath.Clear();
            _liSchedule.Insert(0, CurrentSchedule);
            TravelManager.RequestPathing(this);
        }

        public virtual void CreateDailySchedule()
        {
            _liSchedule.Clear();
        }

        protected void AddScheduleAction(NPCActionState actionState)
        {
            //ToDo: Need to be able to parse multiple actions in one day. For example:  Inn:09-30/14-30/Monday-11-00/Monday-18-00
            var timeKey = GetDefaultTime(actionState);
            var actionKey = Util.GetEnumString(actionState);
            var todayList = new List<ScheduleData>();
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

        /// <summary>
        /// This method constructs the PathData and calls out to the TravelManager to
        /// get the shortest path to the appropriate target
        /// </summary>
        protected override void GetPathToNextAction()
        {
            Wandering = false;

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

            SetCurrentSchedule(_liSchedule[0]);
            _liSchedule.RemoveAt(0);
        }

        protected virtual bool ProcessActionStateData(out Point targetPosition, out string targetMapName, out DirectionEnum dir)
        {
            dir = DirectionEnum.Down;
            targetPosition = Point.Zero;
            targetMapName = string.Empty;

            return false;
        }

        protected bool ProcessActionStateDataHandler(int id, string locationName, out Point targetPosition, out string targetMapName)
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

        protected bool ProcessActionStateDataHandler(WorldObject obj, out Point targetPosition, out string targetMapName)
        {
            bool rv = false;

            targetMapName = MapManager.TownMap.Name;

            if (FriendCheck(targetMapName, out targetPosition))
            {
                rv = true;
            }
            else
            {
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

        protected virtual bool FriendCheck(string targetMapName, out Point targetPosition)
        {
            targetPosition = Point.Zero;
            return false;
        }
        private string GetDefaultTime(NPCActionState actionState)
        {
            string rv = string.Empty;

            switch (actionState)
            {
                case NPCActionState.Inn:
                    rv = Constants.VILLAGER_INN_DEFAULT;
                    break;
                case NPCActionState.Market:
                    rv = Constants.VILLAGER_MARKET_DEFAULT;
                    break;
                case NPCActionState.Craft:
                    rv = Constants.VILLAGER_CRAFT_DEFAULT;
                    break;
                case NPCActionState.OpenShop:
                    rv = Constants.VILLAGER_SHOP_DEFAULT;
                    break;
                case NPCActionState.VisitFriend:
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
        protected void AddScheduleData(TimeSpan timeKey, NPCActionState e, ref List<ScheduleData> list, string data = "")
        {
            list.Add(new ScheduleData(timeKey.ToString(), e, data));
        }
        protected void AddScheduleData(string timeKey, NPCActionState e, ref List<ScheduleData> list, string data = "")
        {
            list.Add(new ScheduleData(timeKey, e, data));
        }
        protected void CreateScheduleData(string parameter, NPCActionState actionState, ref List<ScheduleData> list)
        {
            var str = Util.FindArguments(parameter);
            var timeStr = str.Length == 1 ? str[0] : string.Format("{0}:{1}", str[0], str[1]);
            if (TimeSpan.TryParse(timeStr, out TimeSpan timeSpan))
            {
                CreateScheduleData(timeSpan, actionState, ref list);
            }
        }
        protected void CreateScheduleData(TimeSpan timeSpan, NPCActionState actionState, ref List<ScheduleData> list)
        {
            var timeMod = GetTimeModifier();
            timeSpan = timeSpan.Add(timeMod);
            var timeKey = timeSpan.ToString();
            AddScheduleData(timeKey, actionState, ref list);
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

        protected void SetCurrentSchedule(ScheduleData e)
        {
            CurrentSchedule = e;
        }

        protected void AddWalkableTile(ref List<RHTile> tiles, RHTile tile, DirectionEnum dir)
        {
            if (tile.GetTileByDirection(dir).Passable())
            {
                tiles.Add(tile.GetTileByDirection(dir));
            }
        }

        protected void RemoveOccupiedTiles(ref List<RHTile> tiles)
        {
            foreach (var v in TownManager.Villagers.Values)
            {
                tiles.Remove(v.GetOccupantTile());
            }
        }
        #endregion

        /// <summary>
        /// Object representing the actions that need to be taken that are tied to a current path
        /// This includes the path being taken, the direction to face at the end, and the
        /// animation that may need to be played.
        /// </summary>
        protected class PathData
        {
            public List<RHTile> Path { get; }
            public DirectionEnum Direction { get; }
            public string Animation { get; }
            public bool Wander { get; private set; }

            public PathData(List<RHTile> path, DirectionEnum direction, string animation)
            {
                Path = path;
                Direction = direction;
                Animation = animation;
            }

            public void SetWander(bool val)
            {
                Wander = val;
            }
        }
    }
}
