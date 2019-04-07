using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Buildings;
using RiverHollow.Game_Managers;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.GUIObjects.ManagementScreen;
using static RiverHollow.Game_Managers.ObjectManager;
using static RiverHollow.GUIObjects.GUIObject;
using static RiverHollow.WorldObjects.Clothes;

namespace RiverHollow.Actors
{
    public class Actor
    {
        protected const int HUMAN_HEIGHT = (TileSize * 2) + 2;

        protected static string _sVillagerFolder = GameContentManager.ACTOR_FOLDER + @"Villagers\";
        protected static string _sMonsterFolder = GameContentManager.ACTOR_FOLDER + @"Monsters\";
        protected static string _sMobFolder = GameContentManager.ACTOR_FOLDER + @"Mobs\";
        protected static string _sAdventurerFolder = GameContentManager.ACTOR_FOLDER + @"Adventurers\";
        protected static string _sNPsCFolder = GameContentManager.ACTOR_FOLDER + @"NPCs\";

        protected string _sTexture;
        public enum ActorEnum { Actor, CombatAdventurer, CombatActor, Mob, Monster, NPC, Spirit, WorldAdventurer, WorldCharacter};
        protected ActorEnum _actorType = ActorEnum.Actor;
        public ActorEnum ActorType => _actorType;

        protected string _sName;
        public virtual string Name { get => _sName; }

        protected AnimatedSprite _spriteBody;
        public AnimatedSprite BodySprite { get => _spriteBody; }

        public virtual Vector2 Position
        {
            get { return new Vector2(_spriteBody.Position.X, _spriteBody.Position.Y); }
            set { _spriteBody.Position = value; }
        }
        public virtual Vector2 Center { get => _spriteBody.Center; }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        protected int _width;
        public int Width { get => _width; }
        protected int _height;
        public int Height { get => _height; }
        public int SpriteWidth { get => _spriteBody.Width; }
        public int SpriteHeight { get => _spriteBody.Height; }

        protected bool _bCanTalk = false;
        public bool CanTalk => _bCanTalk;

        public Actor() { }

        public virtual void Update(GameTime theGameTime)
        {
            _spriteBody.Update(theGameTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            //_spriteBody.Draw(spriteBatch, useLayerDepth);
        }

        public virtual void SetName(string text)
        {
            _sName = text;
        }

        public virtual void PlayAnimation<TEnum>(TEnum animation)
        {
            _spriteBody.SetCurrentAnimation(animation);
        }
        public virtual void PlayAnimation(string animation)
        {
            _spriteBody.SetCurrentAnimation(animation);
        }

        public bool Contains(Point x) { return _spriteBody.BoundingBox.Contains(x); }
        public bool AnimationFinished() { return _spriteBody.PlayedOnce && _spriteBody.IsAnimating; }
        public bool IsCurrentAnimation<TEnum>(TEnum val) { return _spriteBody.CurrentAnimation.Equals(Util.GetEnumString(val)); }
        public bool IsAnimating() { return _spriteBody.IsAnimating; }
        public bool AnimationPlayedXTimes(int x) { return _spriteBody.GetPlayCount() >= x; }

        public bool IsCombatAdventurer() { return _actorType == ActorEnum.CombatAdventurer; }
        public bool IsMob() { return _actorType == ActorEnum.Mob; }
        public bool IsMonster() { return _actorType == ActorEnum.Monster; }
        public bool IsNPC() { return _actorType == ActorEnum.NPC; }
        public bool IsWorldAdventurer() { return _actorType == ActorEnum.WorldAdventurer; }
        public bool IsWorldCharacter() { return _actorType == ActorEnum.WorldCharacter; }
        public bool IsSpirit() { return _actorType == ActorEnum.Spirit; }
    }

    #region WorldActors
    public class WorldActor : Actor
    {
        #region Properties
        protected Vector2 _vMoveTo;
        public Vector2 MoveToLocation { get => _vMoveTo; }
        public string CurrentMapName;
        public Vector2 NewMapPosition;
        public enum DirectionEnum { Up, Down, Right, Left };
        public DirectionEnum Facing = DirectionEnum.Down;
        public Texture2D Texture { get => _spriteBody.Texture; }
        public Point CharCenter => GetRectangle().Center;

        public override Vector2 Position
        {
            get { return new Vector2(_spriteBody.Position.X, _spriteBody.Position.Y + _spriteBody.Height - TileSize); } //MAR this is fucked up
            set { _spriteBody.Position = new Vector2(value.X, value.Y - _spriteBody.Height + TileSize); }
        }

        public bool FollowingPath => _currentPath.Count > 0;
        protected List<RHTile> _currentPath;
        protected double _dEtherealCD;
        protected bool _bIgnoreCollisions;

        protected double _dCooldown = 0;

        public Rectangle CollisionBox { get => new Rectangle((int)Position.X + (Width / 4), (int)Position.Y, Width / 2, TileSize); }

        protected bool _bActive = true;
        public bool Active => _bActive;

        public int Speed = 2;

        #endregion

        public WorldActor() : base()
        {
            _actorType = ActorEnum.WorldCharacter;
            _width = TileSize;
            _height = TileSize;
        }

        public virtual void LoadContent(string textureToLoad)
        {
            _sTexture = textureToLoad;
            AddDefaultAnimations(ref _spriteBody, HUMAN_HEIGHT, 0, 0);

            _width = _spriteBody.Width;
            _height = _spriteBody.Height;
        }

        public void AddDefaultAnimations(ref AnimatedSprite sprite, int height, int startX, int startY, bool pingpong = true)
        {
            AddDefaultAnimations(ref sprite, height, _sTexture, startX, startY, pingpong);
        }
        public virtual void AddDefaultAnimations(ref AnimatedSprite sprite, int height, string texture, int startX, int startY, bool pingpong = false)
        {
            sprite = new AnimatedSprite(texture, pingpong);
            sprite.AddAnimation(WActorWalkAnim.WalkDown, TileSize, height, 3, 0.2f, startX, startY);
            sprite.AddAnimation(WActorBaseAnim.IdleDown, TileSize, height, 1, 0.2f, startX + TileSize, startY);
            sprite.AddAnimation(WActorWalkAnim.WalkRight, TileSize, height, 3, 0.2f, startX + TileSize * 3, startY);
            sprite.AddAnimation(WActorBaseAnim.IdleRight, TileSize, height, 1, 0.2f, startX + TileSize * 4, startY);
            sprite.AddAnimation(WActorWalkAnim.WalkUp, TileSize, height, 3, 0.2f, startX + TileSize * 6, startY);
            sprite.AddAnimation(WActorBaseAnim.IdleUp, TileSize, height, 1, 0.2f, startX + TileSize * 7, startY);
            sprite.AddAnimation(WActorWalkAnim.WalkLeft, TileSize, height, 3, 0.2f, startX + TileSize * 9, startY);
            sprite.AddAnimation(WActorBaseAnim.IdleLeft, TileSize, height, 1, 0.2f, startX + TileSize * 10, startY);

            sprite.SetCurrentAnimation(WActorBaseAnim.IdleDown);
        }

        public virtual bool CollisionContains(Point mouse)
        {
            return CollisionBox.Contains(mouse);
        }

        public virtual bool CollisionIntersects(Rectangle rect)
        {
            return CollisionBox.Intersects(rect);
        }

        public void SetWalkingDir(DirectionEnum d)
        {
            Facing = d;
            if (d == DirectionEnum.Up) { _spriteBody.SetCurrentAnimation(WActorWalkAnim.WalkUp); }
            else if (d == DirectionEnum.Down) { _spriteBody.SetCurrentAnimation(WActorWalkAnim.WalkDown); }
            else if (d == DirectionEnum.Right) { _spriteBody.SetCurrentAnimation(WActorWalkAnim.WalkRight); }
            else if (d == DirectionEnum.Left) { _spriteBody.SetCurrentAnimation(WActorWalkAnim.WalkLeft); }
        }

        public virtual void DetermineFacing(Vector2 direction)
        {
            WActorWalkAnim animation = WActorWalkAnim.WalkDown;

            if (direction.Length() == 0)
            {
                Idle();
            }
            else {
                if (Math.Abs((int)direction.X) > Math.Abs((int)direction.Y))
                {
                    if (direction.X > 0)
                    {
                        Facing = DirectionEnum.Right;
                        animation = WActorWalkAnim.WalkRight;
                    }
                    else if (direction.X < 0)
                    {
                        Facing = DirectionEnum.Left;
                        animation = WActorWalkAnim.WalkLeft;
                    }
                }
                else
                {
                    if (direction.Y > 0)
                    {
                        Facing = DirectionEnum.Down;
                        animation = WActorWalkAnim.WalkDown;
                    }
                    else if (direction.Y < 0)
                    {
                        Facing = DirectionEnum.Up;
                        animation = WActorWalkAnim.WalkUp;
                    }
                }

                if (_spriteBody.CurrentAnimation != Util.GetEnumString(animation))
                {
                    PlayAnimation(animation);
                }

                List<RHTile> cornerTiles = new List<RHTile>();
                cornerTiles.Add(MapManager.CurrentMap.RetrieveTileFromGridPosition(Util.GetGridCoords(new Vector2(CollisionBox.Left, CollisionBox.Top)).ToPoint()));
                cornerTiles.Add(MapManager.CurrentMap.RetrieveTileFromGridPosition(Util.GetGridCoords(new Vector2(CollisionBox.Right, CollisionBox.Top)).ToPoint()));
                cornerTiles.Add(MapManager.CurrentMap.RetrieveTileFromGridPosition(Util.GetGridCoords(new Vector2(CollisionBox.Left, CollisionBox.Bottom)).ToPoint()));
                cornerTiles.Add(MapManager.CurrentMap.RetrieveTileFromGridPosition(Util.GetGridCoords(new Vector2(CollisionBox.Right, CollisionBox.Bottom)).ToPoint()));
                foreach (RHTile tile in cornerTiles)
                {
                    if (tile != null && tile.WorldObject != null && tile.WorldObject.IsForageable())
                    {
                        Forageable f = (Forageable)tile.WorldObject;
                        f.Shake();
                    }
                }
            }
        }

        public virtual void Idle()
        {
            switch (Facing)
            {
                case DirectionEnum.Down:
                    PlayAnimation(WActorBaseAnim.IdleDown);
                    break;
                case DirectionEnum.Up:
                    PlayAnimation(WActorBaseAnim.IdleUp);
                    break;
                case DirectionEnum.Left:
                    PlayAnimation(WActorBaseAnim.IdleLeft);
                    break;
                case DirectionEnum.Right:
                    PlayAnimation(WActorBaseAnim.IdleRight);
                    break;
            }
        }

        protected bool CheckMapForCollisionsAndMove(Vector2 direction, bool ignoreCollisions = false)
        {
            bool rv = false;
            //Create the X and Y rectangles to test for collisions
            Rectangle testRectX = new Rectangle((int)(Position.X + direction.X), (int)Position.Y, Width, Height);
            Rectangle testRectY = new Rectangle((int)Position.X, (int)(Position.Y + direction.Y), Width, Height);

            //If the CheckForCollisions gave the all clear, move the sprite.
            if (MapManager.Maps[CurrentMapName].CheckForCollisions(this, testRectX, testRectY, ref direction, ignoreCollisions) && direction != Vector2.Zero)
            {
                DetermineFacing(direction);
                Position += new Vector2(direction.X, direction.Y);
                rv = true;
            }

            return rv;
        }

        protected void HandleMove(Vector2 target)
        {
            Vector2 direction = Vector2.Zero;
            float deltaX = Math.Abs(target.X - this.Position.X);
            float deltaY = Math.Abs(target.Y - this.Position.Y);

            Util.GetMoveSpeed(Position, target, Speed, ref direction);
            if (!CheckMapForCollisionsAndMove(direction, _bIgnoreCollisions))
            {
                if (_dEtherealCD == 0) { _dEtherealCD = 5; }
            }
        }

        public void SetMoveObj(Vector2 vec) { _vMoveTo = vec; }
    }
    public class WorldCombatant : WorldActor
    {
        protected CombatAdventurer _combat;
        public CombatAdventurer Combat => _combat;
    }
    public class TalkingActor : WorldCombatant
    {
        protected const int PortraitWidth = 160;
        protected const int PortraitHeight = 192;

        protected string _sPortrait;
        public string Portrait { get => _sPortrait; }

        protected Rectangle _portraitRect;
        public Rectangle PortraitRectangle { get => _portraitRect; }

        protected Dictionary<string, string> _dialogueDictionary;

        public TalkingActor() : base()
        {
            _bCanTalk = true;
        }

        public virtual string GetOpeningText()
        {
            return "I have nothing to say.";
        }
        public virtual void Talk()
        {
            string text = GetOpeningText();

            text = Util.ProcessText(text, _sName);
            GUIManager.OpenTextWindow(text, this);
        }
        public void Talk(string dialogTag)
        {
            string text = string.Empty;
            if (_dialogueDictionary.ContainsKey(dialogTag))
            {
                text = _dialogueDictionary[dialogTag];
            }
            text = Util.ProcessText(text, _sName);
            GUIManager.OpenTextWindow(text, this);
        }

        public virtual string GetSelectionText()
        {
            RHRandom r = new RHRandom();
            string text = _dialogueDictionary["Selection"];
            Util.ProcessText(text, _sName);

            string[] textFromData = Util.FindTags(text);
            string[] options = textFromData[1].Split('|');

            List<string> liCommands = RemoveEntries(options);

            //If there's only two entires left, Talk and Never Mind, then go straight to Talk
            string rv = string.Empty;
            if (liCommands.Count == 2)
            {
                rv = GetDialogEntry("Talk");
            }
            else
            {
                rv = textFromData[0] + "[";   //Puts back the pre selection text
                foreach (string s in liCommands)
                {
                    rv += s + "|";
                }
                rv = rv.Remove(rv.Length - 1);
                rv += "]";
            }

            return rv;
        }
        public virtual List<string> RemoveEntries(string[] options)
        {
            List<string> _liCommands = new List<string>();
            _liCommands.AddRange(options);
            return _liCommands;
        }

        public string GetText()
        {
            RHRandom r = new RHRandom();
            string text = _dialogueDictionary[r.Next(1, 2).ToString()];
            return Util.ProcessText(text, _sName);
        }
        public virtual string GetDialogEntry(string entry)
        {
            return "Get What Dialog Entry?";
        }
        public virtual string HandleDialogSelectionEntry(string entry)
        {
            return "What?";
        }

        public bool HandleTextInteraction(string chosenAction)
        {
            bool rv = true;
            string nextText = GameManager.gmNPC.GetDialogEntry(chosenAction);

            if (chosenAction.StartsWith("Quest"))
            {
                rv = true;
                Quest q = GameManager.DIQuests[int.Parse(chosenAction.Remove(0, "Quest".Length))];
                PlayerManager.AddToQuestLog(q);
                GUIManager.SetWindowText(GameManager.gmNPC.GetDialogEntry("Quest" + q.QuestID));
            }
            else if (chosenAction.StartsWith("Donate"))
            {
                ((Villager)GameManager.gmNPC).FriendshipPoints += 40;
                GUIManager.SetWindowText(nextText);
            }
            else if (chosenAction.StartsWith("NoDonate"))
            {
                ((Villager)GameManager.gmNPC).FriendshipPoints -= 1000;
                GUIManager.SetWindowText(nextText);
            }
            else if (!string.IsNullOrEmpty(nextText))
            {
                GUIManager.SetWindowText(nextText);
            }
            else if (chosenAction.StartsWith("Cancel"))
            {
                rv = false;
            }
            else
            {
                rv = false;
            }

            return rv;
        }
    }
    public class Villager : TalkingActor
    {
        protected int _iIndex;
        public int ID { get => _iIndex; }
        protected string _homeMap;
        public enum NPCTypeEnum { Eligible, Villager, Shopkeeper, Ranger, Worker }
        protected NPCTypeEnum _npcType;
        public NPCTypeEnum NPCType { get => _npcType; }
        public static List<int> FriendRange = new List<int> { 0, 10, 40, 100, 200, 600, 800, 1200, 1600, 2000 };
        public int FriendshipPoints = 1900;

        protected Dictionary<int, bool> _collection;
        public Dictionary<int, bool> Collection { get => _collection; }
        public bool Introduced;
        public bool CanGiveGift = true;

        protected Dictionary<string, List<KeyValuePair<string, string>>> _completeSchedule;         //Every day with a list of KVP Time/GoToLocations
        List<KeyValuePair<string, List<RHTile>>> _todaysPathing = null;                             //List of Times with the associated pathing                                                     //List of Tiles to currently be traversing
        protected int _scheduleIndex;

        public Villager() { }
        //For Cutscenes
        public Villager(Villager n)
        {
            _actorType = ActorEnum.NPC;
            _iIndex = n.ID;
            _sName = n.Name;
            _dialogueDictionary = n._dialogueDictionary;
            //_sPortrait = n.Portrait;
            //_portraitRect = n._portraitRect;
            _portraitRect = new Rectangle(0, 0, 48, 60);
            _sPortrait = _sAdventurerFolder + "WizardPortrait";

            LoadContent(_sVillagerFolder + "NPC" + _iIndex);
        }

        public Villager(int index, string[] data)
        {
            _collection = new Dictionary<int, bool>();
            _completeSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();
            _scheduleIndex = 0;
            _iIndex = index;

            LoadContent(_sVillagerFolder + "NPC" + _iIndex);
            ImportBasics(data);

            MapManager.Maps[CurrentMapName].AddCharacter(this);
        }

        public override string GetOpeningText()
        {
            string rv = string.Empty;
            if (!Introduced)
            {
                rv = _dialogueDictionary["Introduction"];
                Introduced = true;
            }
            else
            {
                if (!CheckQuestLogs(ref rv))
                {
                    rv = GetSelectionText();
                }
            }
            return rv;
        }
        public override string GetDialogEntry(string entry)
        {
            if (entry.Equals("Talk"))
            {
                return GetText();
            }
            else if (entry.Equals("Nothing"))
            {
                return string.Empty;
            }
            else if (entry.Equals("GiveGift"))
            {
                GUIManager.SetScreen(new InventoryScreen(this));
            }
            else if (entry.Equals("Party"))
            {
                if (IsEligible())
                {
                    EligibleNPC e = (EligibleNPC)this;
                    if (e.Married || e.CanJoinParty)
                    {
                        e.JoinParty();
                        return _dialogueDictionary["JoinPartyYes"];
                    }
                    else
                    {
                        return _dialogueDictionary["JoinPartyNo"];
                    }
                }
            }

            return _dialogueDictionary.ContainsKey(entry) ? Util.ProcessText(_dialogueDictionary[entry], _sName) : string.Empty;
        }
        public override List<string> RemoveEntries(string[] options)
        {
            List<string> _liCommands = new List<string>();
            for (int i = 0; i < options.Length; i++)
            {
                bool removeIt = false;
                string s = options[i];

                if (s.Contains("%"))
                {
                    //Special checks are in the format %type:val% so, |%Friend:50%Join Party:Party| or |%Quest:1%Business:Quest1|
                    string[] specialParse = s.Split(new[] { '%' }, StringSplitOptions.RemoveEmptyEntries);
                    string[] specialVal = specialParse[0].Split(':');

                    int.TryParse(specialVal[1], out int val);

                    if (specialVal[0].Equals("Friend"))
                    {
                        removeIt = FriendshipPoints < val;
                    }
                    else if (specialVal[0].Equals("Quest"))
                    {
                        Quest newQuest = GameManager.DIQuests[val];
                        removeIt = PlayerManager.QuestLog.Contains(newQuest) || newQuest.ReadyForHandIn || newQuest.Finished || !newQuest.CanBeGiven();
                    }

                    s = s.Remove(s.IndexOf(specialParse[0]) - 1, specialParse[0].Length + 2);
                }
                else
                {
                    if (!CanGiveGift && s.Contains("GiveGift"))
                    {
                        removeIt = true;
                    }
                }

                if (!removeIt)
                {
                    _liCommands.Add(s);
                }
            }

            return _liCommands;
        }

        protected int ImportBasics(string[] stringData)
        {
            _sPortrait = _sAdventurerFolder + "WizardPortrait";
            _sName = GameContentManager.GetGameText("NPC" + _iIndex);
            _sPortrait = _sAdventurerFolder + "WizardPortrait";
            _dialogueDictionary = GameContentManager.LoadDialogue(@"Data\Dialogue\NPC" + _iIndex);

            int i = 0;
            int totalCount = 0;
            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("Type"))
                {
                    _npcType = Util.ParseEnum<NPCTypeEnum>(tagType[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("PortRow"))
                {
                    _portraitRect = new Rectangle(0, 0, 48, 60);
                    totalCount++;
                }
                else if (tagType[0].Equals("HomeMap"))
                {
                    CurrentMapName = tagType[1];
                    _homeMap = CurrentMapName;
                    Position = Util.SnapToGrid(MapManager.Maps[CurrentMapName].GetCharacterSpawn("NPC" + _iIndex));

                    totalCount++;
                }
                else if (tagType[0].Equals("Collection"))
                {
                    string[] vectorSplit = tagType[1].Split('-');
                    foreach (string s in vectorSplit)
                    {
                        _collection.Add(int.Parse(s), false);
                    }
                    totalCount++;
                }

                if (totalCount == 4)
                {
                    break;
                }
            }

            Dictionary<string, string> schedule = ObjectManager.GetSchedule("NPC" + _iIndex);
            if (schedule != null)
            {
                foreach (KeyValuePair<string, string> kvp in schedule)
                {
                    List<KeyValuePair<string, string>> temp = new List<KeyValuePair<string, string>>();
                    string[] group = kvp.Value.Split('/');
                    foreach (string s in group)
                    {
                        string[] data = s.Split('|');
                        temp.Add(new KeyValuePair<string, string>(data[0], data[1]));
                    }
                    _completeSchedule.Add(kvp.Key, temp);
                }
            }

            _currentPath = new List<RHTile>();
            CalculatePathing();
            return i;
        }
        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);

            if (_dEtherealCD != 0)
            {
                _dEtherealCD -= theGameTime.ElapsedGameTime.TotalSeconds;
                if (_dEtherealCD <= 0)
                {
                    if (!_bIgnoreCollisions)
                    {
                        _dEtherealCD = 5;
                        _bIgnoreCollisions = true;
                    }
                    else
                    {
                        _dEtherealCD = 0;
                        _bIgnoreCollisions = false;
                    }
                }
            }
            if (_vMoveTo != Vector2.Zero)
            {
                HandleMove(_vMoveTo);
            }
            else if (_todaysPathing != null)
            {
                string currTime = GameCalendar.GetTime();
                //_scheduleIndex keeps track of which pathing route we're currently following.
                //Running late code to be implemented later
                if (_scheduleIndex < _todaysPathing.Count && ((_todaysPathing[_scheduleIndex].Key == currTime)))// || RunningLate(movementList[_scheduleIndex].Key, currTime)))
                {
                    _currentPath = _todaysPathing[_scheduleIndex++].Value;
                }

                if (_currentPath.Count > 0)
                {
                    Vector2 targetPos = _currentPath[0].Position;
                    if (Position == targetPos)
                    {
                        _currentPath.RemoveAt(0);
                        if (_currentPath.Count == 0)
                        {
                            DetermineFacing(Vector2.Zero);
                        }
                    }
                    else
                    {
                        HandleMove(targetPos);
                    }
                }
            }
        }

        public virtual void RollOver()
        {
            MapManager.Maps[CurrentMapName].RemoveCharacter(this);
            RHMap map = MapManager.Maps[_homeMap];
            string Spawn = "NPC" + _iIndex;

            Position = Util.SnapToGrid(map.GetCharacterSpawn(Spawn));
            map.AddCharacter(this);

            CalculatePathing();
        }
        public void CalculatePathing()
        {
            string currDay = GameCalendar.GetDayOfWeek();
            string currSeason = GameCalendar.GetSeason();
            string currWeather = GameCalendar.GetWeatherString();
            if (_completeSchedule != null && _completeSchedule.Count > 0)
            {
                string searchVal = currSeason + currDay + currWeather;
                List<KeyValuePair<string, string>> listPathingForDay = null;

                //Search to see if there exists any pathing instructions for the day.
                //If so, set the value of listPathingForDay to the list of times/locations
                if (_completeSchedule.ContainsKey(currSeason + currDay + currWeather))
                {
                    listPathingForDay = _completeSchedule[currSeason + currDay + currWeather];
                }
                else if (_completeSchedule.ContainsKey(currSeason + currDay))
                {
                    listPathingForDay = _completeSchedule[currSeason + currDay];
                }
                else if (_completeSchedule.ContainsKey(currDay))
                {
                    listPathingForDay = _completeSchedule[currDay];
                }

                //If there is pathing instructions for the day, proceed
                //Key = Time, Value = goto Location
                if (listPathingForDay != null)
                {
                    List<KeyValuePair<string, List<RHTile>>> lTimetoTilePath = new List<KeyValuePair<string, List<RHTile>>>();
                    Vector2 start = Position;
                    string mapName = CurrentMapName;
                    foreach (KeyValuePair<string, string> kvp in listPathingForDay)
                    {
                        List<RHTile> timePath;

                        //If the map we're currently on has the target location, pathfind to it.
                        //Otherwise,we need to pathfind to the map that does first.
                        if (MapManager.Maps[CurrentMapName].DictionaryCharacterLayer.ContainsKey(kvp.Value))
                        {
                            timePath = TravelManager.FindPathToLocation(ref start, MapManager.Maps[CurrentMapName].DictionaryCharacterLayer[kvp.Value], CurrentMapName);
                        }
                        else
                        {
                            timePath = TravelManager.FindPathToOtherMap(kvp.Value, ref mapName, ref start);
                        }
                        lTimetoTilePath.Add(new KeyValuePair<string, List<RHTile>>(kvp.Key, timePath));
                        TravelManager.ClearPathingTracks();
                    }

                    _todaysPathing = lTimetoTilePath;
                }
            }
        }

        //When we change maps, we need to empty out all tiles we're moving to on the map we left
        public void ClearTileForMapChange()
        {
            while (_currentPath[0].MapName == CurrentMapName)
            {
                _currentPath.RemoveAt(0);
            }
        }

        public bool RunningLate(string timeToGo, string currTime)
        {
            bool rv = false;
            string[] toGoSplit = timeToGo.Split(':');
            string[] curSplit = currTime.Split(':');

            int intTime = 0;
            int intCurrent = 0;
            if (toGoSplit.Length > 1 && curSplit.Length > 1)
            {
                if (int.TryParse(toGoSplit[0], out intTime) && int.TryParse(curSplit[0], out intCurrent) && intTime < intCurrent)
                {
                    rv = true;
                }
                else if (intTime == intCurrent && int.TryParse(toGoSplit[1], out intTime) && int.TryParse(curSplit[1], out intCurrent) && intTime < intCurrent)
                {
                    rv = true;
                }
            }

            return rv;
        }

        private bool CheckQuestLogs(ref string text)
        {
            bool rv = false;

            foreach (Quest q in PlayerManager.QuestLog)
            {
                if (q.ReadyForHandIn && q.QuestGiver == this)
                {
                    q.FinishQuest(ref text);

                    rv = true;
                    break;
                }
            }

            return rv;
        }

        public virtual void Gift(Item item)
        {
            if (item != null)
            {
                string text = string.Empty;
                item.Remove(1);
                if (item.IsMap() && NPCType == Villager.NPCTypeEnum.Ranger)
                {
                    text = GetDialogEntry("Adventure");
                    DungeonManager.LoadNewDungeon((AdventureMap)item);
                }
                else
                {
                    CanGiveGift = false;
                    if (_collection.ContainsKey(item.ItemID))
                    {
                        FriendshipPoints += _collection[item.ItemID] ? 50 : 20;
                        text = GetDialogEntry("Collection");
                        int index = 1;
                        foreach (int items in _collection.Keys)
                        {
                            if (_collection[items])
                            {
                                index++;
                            }
                        }

                        _collection[item.ItemID] = true;
                        MapManager.Maps["mapHouseNPC" + _iIndex].AddCollectionItem(item.ItemID, _iIndex, index);
                    }
                    else
                    {
                        text = GetDialogEntry("Gift");
                        FriendshipPoints += 1000;
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    GUIManager.OpenTextWindow(text, this, false);
                }
            }
        }

        public int GetFriendshipLevel()
        {
            int rv = 0;
            for (int i = 0; i < FriendRange.Count; i++)
            {
                if (FriendshipPoints >= FriendRange[i])
                {
                    rv = i;
                }
            }

            return rv;
        }

        public bool IsEligible() { return _npcType == NPCTypeEnum.Eligible; }

        public NPCData SaveData()
        {
            NPCData npcData = new NPCData()
            {
                npcID = ID,
                introduced = Introduced,
                friendship = FriendshipPoints,
                collection = new List<CollectionData>()
            };
            foreach (KeyValuePair<int, bool> kvp in Collection)
            {
                CollectionData c = new CollectionData
                {
                    itemID = kvp.Key,
                    given = kvp.Value
                };
                npcData.collection.Add(c);
            }

            return npcData;
        }
        public void LoadData(NPCData data)
        {
            Introduced = data.introduced;
            FriendshipPoints = data.friendship;
            int index = 1;
            foreach (CollectionData c in data.collection)
            {
                if (c.given)
                {
                    Collection[c.itemID] = c.given;
                    MapManager.Maps["HouseNPC" + data.npcID].AddCollectionItem(c.itemID, data.npcID, index++);
                }
            }
        }
    }
    public class ShopKeeper : Villager
    {
        protected List<Merchandise> _liMerchandise;
        public List<Merchandise> Buildings { get => _liMerchandise; }

        public ShopKeeper(int index, string[] stringData)
        {
            _currentPath = new List<RHTile>();
            _liMerchandise = new List<Merchandise>();
            _collection = new Dictionary<int, bool>();
            _completeSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();

            _iIndex = index;
            LoadContent(_sVillagerFolder + "NPC" + _iIndex);

            int i = ImportBasics(stringData);
            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("ShopData"))
                {
                    foreach (KeyValuePair<int, string> kvp in GameContentManager.GetMerchandise(tagType[1]))
                    {
                        _liMerchandise.Add(new Merchandise(kvp.Value));
                    }
                }
            }

            MapManager.Maps[CurrentMapName].AddCharacter(this);
        }

        public void Talk(bool IsOpen = false)
        {
            GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Talk;
            string text = string.Empty;
            if (!Introduced)
            {
                text = _dialogueDictionary["Introduction"];
                Introduced = true;
            }
            else
            {
                if (IsOpen)
                {
                    text = _dialogueDictionary["ShopOpen"];
                }
                else
                {
                    text = GetText();
                }
            }
            text = Util.ProcessText(text, _sName);
            GUIManager.OpenTextWindow(text, this);
        }

        public override string GetDialogEntry(string entry)
        {
            string rv = string.Empty;
            List<Merchandise> _liMerchandise = new List<Merchandise>();
            if (entry.Equals("BuyBuildings"))
            {
                foreach (Merchandise m in this._liMerchandise)
                {
                    if ((m.MerchType == Merchandise.ItemType.Building || m.MerchType == Merchandise.ItemType.Upgrade) && m.Activated()) { _liMerchandise.Add(m); }
                }

                GUIManager.SetScreen(new PurchaseBuildingsScreen(_liMerchandise));
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("BuyWorkers"))
            {
                foreach (Merchandise m in this._liMerchandise)
                {
                    if (m.MerchType == Merchandise.ItemType.Worker && m.Activated()) { _liMerchandise.Add(m); }
                }
                GUIManager.SetScreen(new PurchaseWorkersScreen(_liMerchandise));
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("BuyItems"))
            {
                foreach (Merchandise m in this._liMerchandise)
                {
                    if (m.MerchType == Merchandise.ItemType.Item && m.Activated()) { _liMerchandise.Add(m); }
                }
                GUIManager.SetScreen(new PurchaseItemsScreen(_liMerchandise));
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("SellWorkers"))
            {
                ManagementScreen s = new ManagementScreen();
                s.Sell();
                GUIManager.SetScreen(s);
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("Move"))
            {
                RiverHollow.HomeMapPlacement();
                GameManager.ClearGMObjects();
                GameManager.MoveBuilding();
            }
            else if (entry.Equals("UpgradeBuilding"))
            {
                ManagementScreen m = new ManagementScreen(ActionTypeEnum.Upgrade);
                GUIManager.SetScreen(m);
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("Destroy"))
            {
                RiverHollow.HomeMapPlacement();
                GameManager.ClearGMObjects();
                GameManager.DestroyBuilding();
            }
            else
            {
                rv = base.GetDialogEntry(entry);
            }

            return rv;
        }

        public class Merchandise
        {
            string _sUniqueData;
            public string UniqueData => _sUniqueData;
            public enum ItemType { Building, Worker, Item, Upgrade }
            public ItemType MerchType;
            int _merchID = -1;
            public int MerchID { get => _merchID; }
            string _description;
            int _moneyCost;
            public int MoneyCost { get => _moneyCost; }
            int _iQuestReq = -1;

            List<KeyValuePair<int, int>> _items; //item, then num required
            public List<KeyValuePair<int, int>> RequiredItems { get => _items; }

            public Merchandise(string data)
            {
                _items = new List<KeyValuePair<int, int>>();
                string[] dataValues = data.Split('/');

                int i = 0;
                if (dataValues[0] == "Building")
                {
                    MerchType = ItemType.Building;
                    i = 1;
                    _merchID = int.Parse(dataValues[i++]);
                    _description = dataValues[i++];
                    _moneyCost = int.Parse(dataValues[i++]);

                    string[] reqItems = dataValues[i++].Split(':');
                    foreach (string str in reqItems)
                    {
                        string[] itemsSplit = str.Split(' ');
                        _items.Add(new KeyValuePair<int, int>(int.Parse(itemsSplit[0]), int.Parse(itemsSplit[1])));
                    }
                }
                else if (dataValues[0] == "Worker")
                {
                    MerchType = ItemType.Worker;
                    i = 1;
                    _merchID = int.Parse(dataValues[i++]);
                    _description = dataValues[i++];
                    _moneyCost = int.Parse(dataValues[i++]);
                }
                else if (dataValues[0] == "Item")
                {
                    MerchType = ItemType.Item;
                    i = 1;
                    string[] itemData = dataValues[i++].Split('-');
                    _merchID = int.Parse(itemData[0]);
                    if (itemData.Length > 1) { _sUniqueData = itemData[1]; }
                    _moneyCost = int.Parse(dataValues[i++]);
                    if (dataValues.Length >= i + 1)
                    {
                        _iQuestReq = int.Parse(dataValues[i++]);
                    }
                }
                else if (dataValues[0] == "Upgrade")
                {
                    MerchType = ItemType.Upgrade;
                    i = 1;
                    _merchID = int.Parse(dataValues[i++]);
                    _description = dataValues[i++];
                    _moneyCost = int.Parse(dataValues[i++]);

                    string[] reqItems = dataValues[i++].Split(':');
                    foreach (string str in reqItems)
                    {
                        string[] itemsSplit = str.Split(' ');
                        _items.Add(new KeyValuePair<int, int>(int.Parse(itemsSplit[0]), int.Parse(itemsSplit[1])));
                    }
                }
            }

            public bool Activated()
            {
                bool rv = false;
                rv = _iQuestReq == -1 || GameManager.DIQuests[_iQuestReq].Finished;
                return rv;
            }
        }
    }
    public class EligibleNPC : Villager
    {
        public bool Married;
        bool _bCanJoinParty = true;
        public bool CanJoinParty => _bCanJoinParty;

        public EligibleNPC(int index, string[] stringData)
        {
            _currentPath = new List<RHTile>();
            _collection = new Dictionary<int, bool>();
            _completeSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();

            _iIndex = index;
            LoadContent(_sVillagerFolder + "NPC" + _iIndex);
            
            int i = ImportBasics(stringData);
            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("Class"))
                {
                    _combat = new CombatAdventurer(this);
                    _combat.SetClass(ObjectManager.GetClassByIndex(int.Parse(tagType[1])));
                    _combat.LoadContent(_sAdventurerFolder + _combat.CharacterClass.Name);
                }
            }

            MapManager.Maps[CurrentMapName].AddCharacter(this);
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bActive)
            {
                base.Draw(spriteBatch, useLayerDepth);
            }
        }
        public override void Update(GameTime theGameTime)
        {
            if (_bActive && !Married)   //Just for now
            {
                base.Update(theGameTime);
            }
        }

        public override void RollOver()
        {
            MapManager.Maps[CurrentMapName].RemoveCharacter(this);
            RHMap map = MapManager.Maps[Married ? "mapManor" : _homeMap];
            string Spawn = Married ? "Spouse" : "NPC" + _iIndex;

            Position = Util.SnapToGrid(map.GetCharacterSpawn(Spawn));
            map.AddCharacter(this);

            _bActive = true;
            PlayerManager.RemoveFromParty(_combat);

            //Reset on Monday
            if (GameCalendar.DayOfWeek == 0)
            {
                _bCanJoinParty = true;
                CanGiveGift = true;
            }

            CalculatePathing();
        }

        public override void Gift(Item item)
        {
            if (item != null)
            {
                string text = string.Empty;
                item.Remove(1);
                if (item.IsMap() && NPCType == Villager.NPCTypeEnum.Ranger)
                {
                    text = GetDialogEntry("Adventure");
                    DungeonManager.LoadNewDungeon((AdventureMap)item);
                }
                else if (item.IsMarriage())
                {
                    if (FriendshipPoints > 200)
                    {
                        Married = true;
                        text = GetDialogEntry("MarriageYes");
                    }
                    else
                    {
                        item.Add(1);
                        InventoryManager.AddItemToInventory(item);
                        text = GetDialogEntry("MarriageNo");
                    }
                }
                else
                {
                    CanGiveGift = false;
                    if (_collection.ContainsKey(item.ItemID))
                    {
                        FriendshipPoints += _collection[item.ItemID] ? 50 : 20;
                        text = GetDialogEntry("Collection");
                        int index = 1;
                        foreach (int items in _collection.Keys)
                        {
                            if (_collection[items])
                            {
                                index++;
                            }
                        }

                        _collection[item.ItemID] = true;
                        MapManager.Maps["mapHouseNPC" + _iIndex].AddCollectionItem(item.ItemID, _iIndex, index);
                    }
                    else
                    {
                        text = GetDialogEntry("Gift");
                        FriendshipPoints += 10;
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    GUIManager.OpenTextWindow(text, this, false);
                }
            }
        }

        public void JoinParty()
        {
            _bActive = false;
            _bCanJoinParty = false;
            PlayerManager.AddToParty(((EligibleNPC)this).Combat);
        }

        public new EligibleNPCData SaveData()
        {
            EligibleNPCData npcData = new EligibleNPCData()
            {
                npcData = base.SaveData(),
                married = Married,
                canJoinParty = _bCanJoinParty,
                canGiveGift = CanGiveGift,
                adventurerData = Combat.SaveData()
            };

            return npcData;
        }
        public void LoadData(EligibleNPCData data)
        {
            Introduced = data.npcData.introduced;
            FriendshipPoints = data.npcData.friendship;
            Married = data.married;
            _bCanJoinParty = data.canJoinParty;
            CanGiveGift = data.canGiveGift;
            Combat.LoadData(data.adventurerData);

            int index = 1;
            foreach (CollectionData c in data.npcData.collection)
            {
                if (c.given)
                {
                    Collection[c.itemID] = c.given;
                    MapManager.Maps["HouseNPC" + data.npcData.npcID].AddCollectionItem(c.itemID, data.npcData.npcID, index++);
                }
            }

            if (Married)
            {
                MapManager.Maps[_homeMap].RemoveCharacter(this);
                MapManager.Maps["mapManor"].AddCharacter(this);
                Position = MapManager.Maps["mapManor"].GetCharacterSpawn("Spouse");
            }
        }
    }
    public class WorldAdventurer : TalkingActor
    {
        #region Properties
        private WorkerTypeEnum _workerType;
        public WorkerTypeEnum WorkerType => _workerType;
        protected int _iAdventurerID;
        public int AdventurerID { get => _iAdventurerID; }
        protected string _sAdventurerType;
        private Building _building;
        public Building Building => _building;
        protected int _iDailyFoodReq;
        protected int _iCurrFood;
        protected int _iDailyItemID;
        protected Item _heldItem;
        protected int _iMood;
        public bool DrawIt;
        public bool Adventuring;
        public int Mood { get => _iMood; }

        protected double _dProcessedTime;
        public double ProcessedTime => _dProcessedTime;

        Dictionary<int, int> _diCrafting;
        public Dictionary<int, int> CraftList => _diCrafting;
        int _iCurrentlyMaking;
        public int CurrentlyMaking => _iCurrentlyMaking;
        #endregion

        public WorldAdventurer(string[] stringData, int id)
        {
            _iAdventurerID = id;
            _actorType = ActorEnum.WorldAdventurer;
            ImportBasics(stringData, id);
            SetCombat();

            _sAdventurerType = Combat.CharacterClass.Name;
            _sTexture = _sAdventurerFolder + "WorldAdventurers";
            LoadContent(_iAdventurerID);

            _portraitRect = new Rectangle(0, 0, 48, 60);
            _sPortrait = _sAdventurerFolder + "WizardPortrait";

            _iCurrFood = 0;
            _heldItem = null;
            _iMood = 0;
            DrawIt = true;
            Adventuring = false;

            _sName = _sAdventurerType.Substring(0, 1);
            Combat.SetName(_sName);
        }

        public void LoadContent(int ID)
        {
            AddDefaultAnimations(ref _spriteBody, HUMAN_HEIGHT, 0, (ID - 1) * TileSize * 3);

            _width = _spriteBody.Width;
            _height = _spriteBody.Height;
        }

        protected void ImportBasics(string[] stringData, int id)
        {
            _diCrafting = new Dictionary<int, int>();

            _iAdventurerID = id;

            foreach (string s in stringData)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Type"))
                {
                    _workerType = Util.ParseEnum<WorkerTypeEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Item"))
                {
                    _iDailyItemID = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Food"))
                {
                    _iDailyFoodReq = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Crafts"))
                {
                    string[] crafting = tagType[1].Split(' ');
                    foreach (string recipe in crafting)
                    {
                        _diCrafting.Add(int.Parse(recipe), int.Parse(recipe));
                    }
                }
            }
        }

        protected void SetCombat()
        {
            _combat = new CombatAdventurer(this);
            _combat.SetClass(ObjectManager.GetClassByIndex(_iAdventurerID));
            _combat.LoadContent(_sAdventurerFolder + _combat.CharacterClass.Name);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (DrawIt)
            {
                base.Draw(spriteBatch, useLayerDepth);
                if (_heldItem != null)
                {
                    _heldItem.Draw(spriteBatch, new Rectangle(Position.ToPoint(), new Point(32, 32)), true);
                }
            }
        }

        public override bool CollisionContains(Point mouse)
        {
            bool rv = false;
            if (DrawIt)
            {
                rv = base.CollisionContains(mouse);
            }
            return rv;
        }
        public override bool CollisionIntersects(Rectangle rect)
        {
            bool rv = false;
            if (DrawIt)
            {
                rv = base.CollisionIntersects(rect);
            }
            return rv;
        }

        public override string GetOpeningText()
        {
            return Name + ": " + GameContentManager.GetGameText("AdventurerTree");
        }

        public override string GetSelectionText()
        {
            RHRandom r = new RHRandom();
            string text = _dialogueDictionary[r.Next(1, 2).ToString()];
            return Util.ProcessText(text, _sName);
        }

        public override string GetDialogEntry(string entry)
        {
            string rv = string.Empty;
            if (entry.Equals("Talk"))
            {
                GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Normal;
                _iMood += 1;

                RHRandom r = new RHRandom();
                rv = GameContentManager.GetGameText(_sAdventurerType + r.Next(1, 2));
            }
            else if (entry.Equals("Party"))
            {
                DrawIt = false;
                Adventuring = true;
                PlayerManager.AddToParty(_combat);
                rv = "Of course!";
            }
            return rv;
        }

        public void ProcessChosenItem(int itemID)
        {
            _iCurrentlyMaking = _diCrafting[itemID];
            _spriteBody.SetCurrentAnimation(WActorBaseAnim.MakeItem);
        }

        public int TakeItem()
        {
            int giveItem = -1;
            if (_heldItem != null)
            {
                giveItem = _heldItem.ItemID;
                _heldItem = null;
            }
            return giveItem;
        }

        public int WhatAreYouHolding()
        {
            if (_heldItem != null)
            {
                return _heldItem.ItemID;
            }
            return -1;
        }

        public void SetBuilding(Building b)
        {
            _building = b;
        }

        public bool Rollover()
        {
            bool rv = !Adventuring;
            if (GameManager.AutoDisband)
            {
                DrawIt = true;
                Adventuring = false;
            }
            _combat.CurrentHP = _combat.MaxHP;
            return rv;
        }

        public void MakeDailyItem()
        {
            if (_iDailyItemID != -1)
            {
                InventoryManager.AddNewItemToInventory(_iDailyItemID, _building.BuildingChest);
            }
        }

        public string GetName()
        {
            return _sName;
        }

        public override void SetName(string name)
        {
            _sName = name;
            _combat.SetName(name);
        }

        public WorkerData SaveData()
        {
            WorkerData workerData = new WorkerData
            {
                workerID = this.AdventurerID,
                advData = Combat.SaveData(),
                mood = this.Mood,
                name = this.Name,
                processedTime = this.ProcessedTime,
                currentItemID = (this._iCurrentlyMaking == null) ? -1 : this._iCurrentlyMaking,
                heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID,
                adventuring = Adventuring
            };

            return workerData;
        }
        public void LoadData(WorkerData data)
        {
            _iAdventurerID = data.workerID;
            _iMood = data.mood;
            _sName = data.name;
            _dProcessedTime = data.processedTime;
            _iCurrentlyMaking = data.currentItemID;
            _heldItem = ObjectManager.GetItem(data.heldItemID);
            Adventuring = data.adventuring;

            SetCombat();
            Combat.LoadData(data.advData);

            if (_iCurrentlyMaking != null) { _spriteBody.SetCurrentAnimation(WActorBaseAnim.MakeItem); }
            if (Adventuring)
            {
                DrawIt = false;
                PlayerManager.AddToParty(Combat);
            }
        }
    }
    public class PlayerCharacter : WorldCombatant
    {
        AnimatedSprite _spriteEyes;
        public AnimatedSprite EyeSprite => _spriteEyes;
        AnimatedSprite _spriteHair;
        public AnimatedSprite HairSprite => _spriteHair;

        Color _cHairColor = Color.White;
        public Color HairColor => _cHairColor;

        int _iHairIndex = 0;
        public int HairIndex => _iHairIndex;

        public Vector2 BodyPosition => _spriteBody.Position;
        public override Vector2 Position
        {
            get { return new Vector2(_spriteBody.Position.X, _spriteBody.Position.Y + _spriteBody.Height - TileSize); }
            set
            {
                _spriteBody.Position = new Vector2(value.X, value.Y - _spriteBody.Height + TileSize);
                _spriteEyes.Position = _spriteBody.Position;
                _spriteHair.Position = _spriteBody.Position;

                if (_chest != null) { _chest.SetSpritePosition(_spriteBody.Position); }
                if (Hat != null) { _hat.SetSpritePosition(_spriteBody.Position); }
            }
        }

        Clothes _hat;
        public Clothes Hat => _hat;
        Clothes _chest;
        public Clothes Shirt => _chest;
        Clothes Back;
        Clothes Hands;
        Clothes Legs;
        Clothes Feet;

        public PlayerCharacter() : base()
        {
            _sName = PlayerManager.Name;
            _width = TileSize;
            _height = TileSize;

            _cHairColor = Color.Red;
            Speed = 2;

            _currentPath = new List<RHTile>();
        }

        public override void AddDefaultAnimations(ref AnimatedSprite sprite, int height, string texture, int startX, int startY, bool pingpong = false)
        {
            base.AddDefaultAnimations(ref sprite, height, texture, startX, startY, pingpong);
            sprite.AddAnimation(ToolAnimEnum.Down, TileSize, height, 3, TOOL_ANIM_SPEED, startX + TileSize * 12, startY);
        }

        public override void Update(GameTime theGameTime)
        {
            if (_dCooldown > 0)
            {
                _dCooldown -= theGameTime.ElapsedGameTime.TotalSeconds;
                if (_currentPath.Count == 0 && _dCooldown <= 0 && PlayerManager.ReadyToSleep)
                {
                    GUIManager.SetScreen(new DayEndScreen());
                    _dCooldown = 0;
                }
            }

            if (_currentPath.Count > 0)
            {
                if (_vMoveTo != Vector2.Zero)
                {
                    HandleMove(_vMoveTo);
                }
                else
                {
                    Vector2 targetPos = _currentPath[0].Position;
                    if (Position == targetPos)
                    {
                        _currentPath.RemoveAt(0);
                        if (_currentPath.Count == 0)
                        {
                            if (PlayerManager.ReadyToSleep)
                            {
                                if (_dCooldown == 0)
                                {
                                    Facing = DirectionEnum.Left;
                                    PlayAnimation(WActorBaseAnim.IdleLeft);
                                    _dCooldown = 3;
                                }
                            }
                            else
                            {
                                DetermineFacing(Vector2.Zero);
                            }
                        }
                    }
                    else
                    {
                        HandleMove(targetPos);
                    }
                }
            }

            _spriteBody.Update(theGameTime);
            _spriteEyes.Update(theGameTime);
            _spriteHair.Update(theGameTime);

            if (_chest != null) { _chest.Sprite.Update(theGameTime); }
            if (Hat != null) { Hat.Sprite.Update(theGameTime); }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            _spriteBody.Draw(spriteBatch, useLayerDepth);

            float bodyDepth = _spriteBody.Position.Y + _spriteBody.CurrentFrameAnimation.FrameHeight + (Position.X / 100);
            _spriteEyes.Draw(spriteBatch, useLayerDepth, 1.0f, bodyDepth);
            _spriteHair.Draw(spriteBatch, useLayerDepth, 1.0f, bodyDepth);

            if (_chest != null) { _chest.Sprite.Draw(spriteBatch, useLayerDepth, 1.0f, bodyDepth + 0.01f); }
            if (Hat != null) { Hat.Sprite.Draw(spriteBatch, useLayerDepth); }
        }

        public override void LoadContent(string textureToLoad)
        {
            Color bodyColor = Color.White;

            AddDefaultAnimations(ref _spriteBody, HUMAN_HEIGHT, textureToLoad, 0, 0, true);
            _spriteBody.SetColor(bodyColor);

            AddDefaultAnimations(ref _spriteEyes, TileSize, textureToLoad, 0, TileSize * 3, true);
            _spriteEyes.SetDepthMod(0.001f);

            AddDefaultAnimations(ref _spriteHair, TileSize * 2, @"Textures\texPlayerHair", 0, _iHairIndex * TileSize * 2, true);
            _spriteHair.SetColor(_cHairColor);
            _spriteHair.SetDepthMod(0.003f);

            _width = _spriteBody.Width;
            _height = _spriteBody.Height;
        }

        public void SetHairColor(Color c)
        {
            _cHairColor = c;
            SetColor(_spriteHair, c);
        }

        public void SetColor(AnimatedSprite sprite, Color c)
        {
            sprite.SetColor(c);
        }

        public void SetHairType(int index)
        {
            _iHairIndex = index;
            AddDefaultAnimations(ref _spriteHair, TileSize * 2, @"Textures\texPlayerHair", 0, _iHairIndex * TileSize * 2, true);
            _spriteHair.SetColor(_cHairColor);
            _spriteHair.SetDepthMod(0.003f);
        }

        public void MoveBy(int x, int y)
        {
            _spriteBody.MoveBy(x, y);
            _spriteEyes.MoveBy(x, y);
            _spriteHair.MoveBy(x, y);
            if (_chest != null) { _chest.Sprite.MoveBy(x, y); }
            if (Hat != null) { Hat.Sprite.MoveBy(x, y); }
        }

        public override void PlayAnimation<TEnum>(TEnum anim)
        {
            _spriteBody.SetCurrentAnimation(anim);
            _spriteEyes.SetCurrentAnimation(anim);
            _spriteHair.SetCurrentAnimation(anim);

            if (_chest != null) { _chest.Sprite.SetCurrentAnimation(anim); }
            if (Hat != null) { Hat.Sprite.SetCurrentAnimation(anim); }
        }

        public void SetScale(int scale = 1)
        {
            _spriteBody.SetScale(scale);
            _spriteEyes.SetScale(scale);
            _spriteHair.SetScale(scale);

            if (_chest != null) { _chest.Sprite.SetScale(scale); }
            if (_hat != null) { _hat.Sprite.SetScale(scale); }
        }

        public void SetClothes(Clothes c)
        {
            if (c != null)
            {
                if (c.IsShirt()) { _chest = c; }
                else if (c.IsHat())
                {
                    _spriteHair.FrameCutoff = 9;
                    _hat = c;
                }

                //MAR AWKWARD
                c.Sprite.Position = _spriteBody.Position;
                c.Sprite.SetCurrentAnimation(_spriteBody.CurrentAnimation);
                c.Sprite.SetDepthMod(0.004f);
            }
        }

        public void RemoveClothes(ClothesEnum c)
        {
            if (c.Equals(ClothesEnum.Chest)) { _chest = null; }
            else if (c.Equals(ClothesEnum.Hat))
            {
                _spriteHair.FrameCutoff = 0;
                _hat = null;
            }
        }

        public void SetPath(List<RHTile> list)
        {
            _currentPath = list;
        }
    }

    public class Spirit : TalkingActor
    {
        const float MIN_VISIBILITY = 0.05f;
        float _fVisibility;
        string _sType;
        string _sCondition;
        string _sText;

        public bool Triggered;

        public Spirit(string name, string type, string condition, string text) : base()
        {
            _actorType = ActorEnum.Spirit;
            _fVisibility = MIN_VISIBILITY;

            _sName = name;
            _sType = type;
            _sText = text;
            _sCondition = condition;
            _bActive = false;

            LoadContent(_sNPsCFolder + "Spirit_" + _sType);
        }

        public override void LoadContent(string textureToLoad)
        {
            _spriteBody = new AnimatedSprite(textureToLoad);
            _spriteBody.AddAnimation(WActorBaseAnim.IdleDown, 16, 18, 2, 0.6f, 0, 0);
            _spriteBody.SetCurrentAnimation(WActorBaseAnim.IdleDown);

            _width = _spriteBody.Width;
            _height = _spriteBody.Height;
        }

        public override void Update(GameTime theGameTime)
        {
            //if (_bActive)
            //{
            //    base.Update(theGameTime);
            //    if (!Triggered)
            //    {
            //        int max = TileSize * 13;
            //        int dist = 0;
            //        if (PlayerManager.CurrentMap == CurrentMapName && PlayerManager.PlayerInRangeGetDist(_spriteBody.Center.ToPoint(), max, ref dist))
            //        {
            //            float fMax = max;
            //            float fDist = dist;
            //            float percentage = (Math.Abs(dist - fMax)) / fMax;
            //            percentage = Math.Max(percentage, MIN_VISIBILITY);
            //            _fVisibility = 0.4f * percentage;
            //        }
            //    }
            //}
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bActive)
            {
                _spriteBody.Draw(spriteBatch, useLayerDepth, _fVisibility);
            }
        }

        public void CheckCondition()
        {
            bool active = false;
            string[] splitCondition = _sCondition.Split('/');
            foreach (string s in splitCondition)
            {
                if (s.Equals("Raining"))
                {
                    active = GameCalendar.IsRaining();
                }
                else if (s.Contains("day"))
                {
                    active = s.Equals(GameCalendar.GetDayOfWeek());
                }
                else if (s.Equals("Night"))
                {
                    active = GameCalendar.IsNight();
                }

                if (!active) { break; }
            }

            _bActive = active;
            Triggered = false;
        }
        public override string GetOpeningText()
        {
            string rv = string.Empty;
            if (_bActive)
            {
                Triggered = true;
                _fVisibility = 1.0f;

                string[] loot = GameContentManager.DiSpiritLoot[_sType].Split('/');
                RHRandom r = new RHRandom();
                int arrayID = r.Next(0, loot.Length - 1);
                InventoryManager.AddNewItemToInventory(int.Parse(loot[arrayID]));

                _sText = Util.ProcessText(_sText.Replace("*", "*" + loot[arrayID] + "*"));
                GUIManager.OpenTextWindow(_sText, this);
            }
            return rv;
        }
    }
    public class Mob : WorldActor
    {
        #region Properties
        public override Vector2 Position
        {
            get { return new Vector2(_spriteBody.Position.X, _spriteBody.Position.Y + _spriteBody.Height - TileSize); } //MAR this is fucked up
            set {
                _spriteBody.Position = new Vector2(value.X, value.Y - _spriteBody.Height + TileSize);
                _sprAlert.Position = _spriteBody.Position - new Vector2(0, TileSize);
            }
        }

        protected int _id;
        public int ID { get => _id; }
        protected double _dIdleFor;
        protected int _iLeash = 7;

        protected Vector2 _vMoveTo = Vector2.Zero;
        protected List<CombatActor> _liMonsters;
        public List<CombatActor> Monsters { get => _liMonsters; }

        double _dStun;
        int _iMaxRange = TileSize * 10;
        bool _bAlert;
        bool _bLockedOn;
        bool _bLeashed;
        bool _bJump;
        Vector2 _vJumpTo;
        const double MAX_ALERT = 1;
        double _dAlertTimer;
        AnimatedSprite _sprAlert;

        FieldOfVision _FoV;
        Vector2 _vLeashPoint;
        float _fLeashRange = TileSize * 30;
        int _iMoveFailures = 0;

        List<SpawnConditionEnum> _liSpawnConditions;
        
        #endregion

        public Mob(int id, string[] stringData)
        {
            _liSpawnConditions = new List<SpawnConditionEnum>();
            _actorType = ActorEnum.Mob;
            _liMonsters = new List<CombatActor>();
            ImportBasics(stringData, id);
            _sTexture = _sMobFolder + "FangedFur";
            LoadContent();
        }

        public void LoadContent()
        {
            _spriteBody = new AnimatedSprite(_sTexture);

            if (!_bJump)
            {
                _spriteBody.AddAnimation(WActorBaseAnim.IdleDown, TileSize, TileSize * 2, 1, 0.2f, 0, 0);
                _spriteBody.AddAnimation(WActorWalkAnim.WalkDown, TileSize, TileSize * 2, 4, 0.2f, 0, 0);
                _spriteBody.AddAnimation(WActorBaseAnim.IdleUp, TileSize, TileSize * 2, 1, 0.2f, 64, 0);
                _spriteBody.AddAnimation(WActorWalkAnim.WalkUp, TileSize, TileSize * 2, 4, 0.2f, 64, 0);
                _spriteBody.AddAnimation(WActorBaseAnim.IdleLeft, TileSize, TileSize * 2, 1, 0.2f, 128, 0);
                _spriteBody.AddAnimation(WActorWalkAnim.WalkLeft, TileSize, TileSize * 2, 4, 0.2f, 128, 0);
                _spriteBody.AddAnimation(WActorBaseAnim.IdleRight, TileSize, TileSize * 2, 1, 0.2f, 192, 0);
                _spriteBody.AddAnimation(WActorWalkAnim.WalkRight, TileSize, TileSize * 2, 4, 0.2f, 192, 0);
                _spriteBody.SetCurrentAnimation(WActorWalkAnim.WalkDown);
            }
            else
            {
                _spriteBody.AddAnimation(WActorBaseAnim.IdleDown, TileSize, TileSize * 2, 2, 0.2f, 0, 0);
                _spriteBody.AddAnimation(WActorJumpAnim.GroundDown, TileSize, TileSize * 2, 2, 0.2f, 0, 0);
                _spriteBody.AddAnimation(WActorJumpAnim.AirDown, TileSize, TileSize * 2, 2, 0.2f, 32, 0);

                _spriteBody.AddAnimation(WActorBaseAnim.IdleUp, TileSize, TileSize * 2, 2, 0.2f, 64, 0);
                _spriteBody.AddAnimation(WActorJumpAnim.GroundUp, TileSize, TileSize * 2, 2, 0.2f, 64, 0);
                _spriteBody.AddAnimation(WActorJumpAnim.AirUp, TileSize, TileSize * 2, 2, 0.2f, 96, 0);

                _spriteBody.AddAnimation(WActorBaseAnim.IdleLeft, TileSize, TileSize * 2, 2, 0.2f, 128, 0);
                _spriteBody.AddAnimation(WActorJumpAnim.GroundLeft, TileSize, TileSize * 2, 2, 0.2f, 128, 0);
                _spriteBody.AddAnimation(WActorJumpAnim.AirLeft, TileSize, TileSize * 2, 2, 0.2f, 160, 0);

                _spriteBody.AddAnimation(WActorBaseAnim.IdleRight, TileSize, TileSize * 2, 2, 0.2f, 192, 0);
                _spriteBody.AddAnimation(WActorJumpAnim.GroundRight, TileSize, TileSize * 2, 2, 0.2f, 192, 0);
                _spriteBody.AddAnimation(WActorJumpAnim.AirRight, TileSize, TileSize * 2, 2, 0.2f, 224, 0);
                _spriteBody.SetCurrentAnimation(WActorBaseAnim.IdleDown);
            }
            Facing = DirectionEnum.Down;

            _width = _spriteBody.Width;
            _height = _spriteBody.Height;

            _sprAlert = new AnimatedSprite(@"Textures\Dialog", true);
            _sprAlert.AddAnimation(GenAnimEnum.Play, 64, 64, 16, 16, 3, 0.2f);
            _sprAlert.SetCurrentAnimation(GenAnimEnum.Play);
            _sprAlert.Position = (Position - new Vector2(0, TileSize));
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            for (int i = 0; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("Monster"))
                {
                    int mID = int.Parse(tagType[1]);
                    _liMonsters.Add(ObjectManager.GetMonsterByIndex(mID));
                }
                else if (tagType[0].Equals("Condition"))
                {
                    _liSpawnConditions.Add(Util.ParseEnum<SpawnConditionEnum>(tagType[1]));
                }
                else if (tagType[0].Equals("Jump"))
                {
                    _bJump = true;
                }
            }

            foreach (CombatActor m in _liMonsters)
            {
                List<CombatActor> match = _liMonsters.FindAll(x => ((Monster)x).ID == ((Monster)m).ID);
                if (match.Count > 1)
                {
                    for (int i = 0; i < match.Count; i++)
                    {
                        match[i].SetUnique(Util.NumToString(i + 1, true));
                    }
                }
            }
            _id = id;
            return 0;
        }

        public void NewFoV()
        {
            _FoV = new FieldOfVision(this, _iMaxRange);
        }

        public override void Update(GameTime theGameTime)
        {
            //Check if the mob is still stunned
            if (_dStun > 0)
            {
                _dStun -= theGameTime.ElapsedGameTime.TotalSeconds;
                if(_dStun < 0) { _dStun = 0; }
            }
            else
            {
                UpdateAlertTimer(theGameTime);
                UpdateMovement(theGameTime);
            }

            base.Update(theGameTime);
        }

        private void UpdateAlertTimer(GameTime theGameTime)
        {
            //If mob is on alert, but not locked on to
            //the player, increment the timer.
            if (_bAlert && !_bLockedOn)
            {
                _dAlertTimer += theGameTime.ElapsedGameTime.TotalSeconds;
                _sprAlert.Update(theGameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool userLayerDepth = false)
        {
            base.Draw(spriteBatch, userLayerDepth);
            if(_bAlert) { _sprAlert.Draw(spriteBatch, userLayerDepth); }
        }

        private void UpdateMovement(GameTime theGameTime)
        {
            bool move = true;
            Vector2 direction = Vector2.Zero;

            //Handle Leashing and Idle Movement targetting
            HandlePassivity(theGameTime);

            if (_bLockedOn)
            {
                if (Math.Abs(_vLeashPoint.X - Position.X) <= _fLeashRange && Math.Abs(_vLeashPoint.Y - Position.Y) <= _fLeashRange)
                {
                    _vMoveTo = Vector2.Zero;

                    if (_bJump)
                    {
                        if (_vJumpTo == Vector2.Zero)
                        {
                            _vJumpTo = PlayerManager.World.Position;
                        }
                        _vMoveTo = _vJumpTo;
                    }
                    else
                    {
                        _vMoveTo = PlayerManager.World.Position;
                    }
                }
                else if(!BodySprite.CurrentAnimation.StartsWith("Air"))
                {
                    _bLeashed = true;
                    _bLockedOn = false;
                    _vMoveTo = _vLeashPoint;
                    DetermineFacing(new Vector2(_vMoveTo.X - Position.X, _vMoveTo.Y - Position.Y));
                }
            }

            if (_bJump)
            {
                if (_vMoveTo != Vector2.Zero && BodySprite.CurrentAnimation.StartsWith("Idle"))
                {
                    move = false;

                    string animation = "";
                    switch (Facing)
                    {
                        case DirectionEnum.Down:
                            animation = Util.GetEnumString(WActorJumpAnim.GroundDown);
                            break;
                        case DirectionEnum.Up:
                            animation = Util.GetEnumString(WActorJumpAnim.GroundUp);
                            break;
                        case DirectionEnum.Left:
                            animation = Util.GetEnumString(WActorJumpAnim.GroundLeft);
                            break;
                        case DirectionEnum.Right:
                            animation = Util.GetEnumString(WActorJumpAnim.GroundRight);
                            break;
                    }

                    PlayAnimation(animation);
                }
                else if (BodySprite.CurrentAnimation.StartsWith("Ground") && BodySprite.CurrentFrameAnimation.PlayCount < 1)
                {
                    move = false;
                }
                else if (!BodySprite.CurrentAnimation.StartsWith("Idle") && BodySprite.CurrentFrameAnimation.PlayCount >= 1)
                {
                    bool jumping = BodySprite.CurrentAnimation.StartsWith("Air");
                    string animation = "";
                    switch (Facing)
                    {
                        case DirectionEnum.Down:
                            animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundDown) : Util.GetEnumString(WActorJumpAnim.AirDown);
                            break;
                        case DirectionEnum.Up:
                            animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundUp) : Util.GetEnumString(WActorJumpAnim.AirUp); ;
                            break;
                        case DirectionEnum.Left:
                            animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundLeft) : Util.GetEnumString(WActorJumpAnim.AirLeft); ;
                            break;
                        case DirectionEnum.Right:
                            animation = jumping ? Util.GetEnumString(WActorJumpAnim.GroundRight) : Util.GetEnumString(WActorJumpAnim.AirRight); ;
                            break;
                    }
                    _vJumpTo = Vector2.Zero;
                    PlayAnimation(animation);
                }
            }

            if (move && _vMoveTo != Vector2.Zero)
            {
                float deltaX = Math.Abs(_vMoveTo.X - this.Position.X);
                float deltaY = Math.Abs(_vMoveTo.Y - this.Position.Y);

                Util.GetMoveSpeed(Position, _vMoveTo, Speed, ref direction);
                DetermineFacing(direction);
                if(!CheckMapForCollisionsAndMove(direction, false))
                {
                    _iMoveFailures++;
                }
                NewFoV();

                //We have finished moving
                if (Position.X == _vMoveTo.X && Position.Y == _vMoveTo.Y)
                {
                    //If we were peashing back to our start point unset it.
                    if (_bLeashed && _vMoveTo == _vLeashPoint)
                    {
                        _vLeashPoint = Vector2.Zero;
                        _bLeashed = false;
                    }

                    _vMoveTo = Vector2.Zero;
                    Idle();
                }
            }

            if (CollisionBox.Intersects(PlayerManager.World.CollisionBox))
            {
                _bAlert = false;
                _bLockedOn = false;
                CombatManager.NewBattle(this);
            }
        }

        //If mob is not returning to leash point and player in in range
        private void HandlePassivity(GameTime theGameTime)
        {
            if (!_bLeashed && !_bLockedOn && _FoV.Contains(PlayerManager.World))
            {
                //If alert if not on, set alert
                if (!_bAlert)
                {
                    _bAlert = true;
                    _dAlertTimer = 0;
                }

                if (_dAlertTimer >= MAX_ALERT)
                {
                    if (_vLeashPoint == Vector2.Zero) { _vLeashPoint = Position; }
                    _bLockedOn = true;
                    _bAlert = false;
                }
            }
            else
            {
                if (_bAlert)
                {
                    _bAlert = false;
                    _dAlertTimer = 0;
                }
                GetIdleMovementTarget(theGameTime);
            }
        }
        private void GetIdleMovementTarget(GameTime theGameTime)
        {
            if ((_vMoveTo == Vector2.Zero && _dIdleFor <= 0 && !BodySprite.CurrentAnimation.StartsWith("Air")) || _iMoveFailures == 5)
            {
                _iMoveFailures = 0;
                int howFar = 2;
                bool skip = false;
                RHRandom r = new RHRandom();
                int decision = r.Next(1, 5);
                if (decision == 1) { _vMoveTo = new Vector2(Position.X - r.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 2) { _vMoveTo = new Vector2(Position.X + r.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 3) { _vMoveTo = new Vector2(Position.X, Position.Y - r.Next(1, howFar) * TileSize); }
                else if (decision == 4) { _vMoveTo = new Vector2(Position.X, Position.Y + r.Next(1, howFar) * TileSize); }
                else
                {
                    _vMoveTo = Vector2.Zero;
                    _dIdleFor = 4;
                    Idle();
                    skip = true;
                }

                if (!skip)
                {
                    DetermineFacing(new Vector2(_vMoveTo.X - Position.X, _vMoveTo.Y - Position.Y));
                }
            }
            else if (_vMoveTo == Vector2.Zero)
            {
                _dIdleFor -= theGameTime.ElapsedGameTime.TotalSeconds;
            }
        }

        public override void DetermineFacing(Vector2 direction)
        {
            string animation = string.Empty;

            if (direction.Length() == 0)
            {
                Idle();
            }
            else
            {
                if (!_bJump || (_bJump && !BodySprite.CurrentAnimation.StartsWith("Air")))
                {
                    if (Math.Abs((int)direction.X) > Math.Abs((int)direction.Y))
                    {
                        if (direction.X > 0)
                        {
                            Facing = DirectionEnum.Right;
                            animation = _bJump ? Util.GetEnumString(WActorJumpAnim.GroundRight) : Util.GetEnumString(WActorWalkAnim.WalkRight);
                        }
                        else if (direction.X < 0)
                        {
                            Facing = DirectionEnum.Left;
                            animation = _bJump ? Util.GetEnumString(WActorJumpAnim.GroundLeft) : Util.GetEnumString(WActorWalkAnim.WalkLeft);
                        }
                    }
                    else
                    {
                        if (direction.Y > 0)
                        {
                            Facing = DirectionEnum.Down;
                            animation = _bJump ? Util.GetEnumString(WActorJumpAnim.GroundDown) : Util.GetEnumString(WActorWalkAnim.WalkDown);
                        }
                        else if (direction.Y < 0)
                        {
                            Facing = DirectionEnum.Up;
                            animation = _bJump ? Util.GetEnumString(WActorJumpAnim.GroundUp) : Util.GetEnumString(WActorWalkAnim.WalkUp);
                        }
                    }

                    PlayAnimation(animation);
                }
            }
        }

        public bool CheckValidConditions(SpawnConditionEnum s)
        {
            bool rv = true;

            if (_liSpawnConditions.Contains(s))
            {
                foreach(SpawnConditionEnum e in _liSpawnConditions)
                {
                    if(e.Equals(SpawnConditionEnum.Night) && !GameCalendar.IsNight())
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

        public void Stun()
        {
            _dStun = 5.0f;
        }

        private class FieldOfVision
        {
            int _iMaxRange;
            Vector2 _vFirst;            //The LeftMost of the TopMost
            Vector2 _vSecond;           //The RightMost of the BottomMost
            DirectionEnum _eDir;

            public FieldOfVision(Mob theMob, int maxRange)
            {
                int sideRange = TileSize * 2;
                _iMaxRange = (int)(maxRange * (1 - (0.1 * PlayerManager.GetRoguesInParty())));
                _eDir = theMob.Facing;
                if (_eDir == DirectionEnum.Up || _eDir == DirectionEnum.Down)
                {
                    _vFirst = theMob.Center - new Vector2(sideRange, 0);
                    _vSecond = theMob.Center + new Vector2(sideRange, 0);
                }
                else
                {
                    _vFirst = theMob.Center - new Vector2(0, sideRange);
                    _vSecond = theMob.Center + new Vector2(0, sideRange);
                }
            }

            public void MoveBy(Vector2 v)
            {
                _vFirst += v;
                _vSecond += v;
            }

            public bool Contains(WorldActor actor)
            {
                bool rv = false;
                Vector2 center = actor.CollisionBox.Center.ToVector2();

                Vector2 firstFoV = _vFirst;
                Vector2 secondFoV = _vSecond;
                //Make sure the actor could be in range
                if (_eDir == DirectionEnum.Up && Util.InBetween(center.Y, firstFoV.Y - _iMaxRange, firstFoV.Y))
                {
                    float yMod = Math.Abs(center.Y - firstFoV.Y);
                    firstFoV += new Vector2(-yMod, -yMod);
                    secondFoV += new Vector2(yMod, -yMod);

                    rv =  Util.InBetween(center.X, firstFoV.X, secondFoV.X) && Util.InBetween(center.Y, firstFoV.Y, _vFirst.Y);
                }
                else if (_eDir == DirectionEnum.Down && Util.InBetween(center.Y, firstFoV.Y, firstFoV.Y + _iMaxRange))
                {
                    float yMod = Math.Abs(center.Y - firstFoV.Y);
                    firstFoV += new Vector2(-yMod, yMod);
                    secondFoV += new Vector2(yMod, yMod);

                    rv = Util.InBetween(center.X, firstFoV.X, secondFoV.X) && Util.InBetween(center.Y, _vFirst.Y, firstFoV.Y);
                }
                else if(_eDir == DirectionEnum.Left && Util.InBetween(center.X, firstFoV.X - _iMaxRange, firstFoV.X))
                {
                    float xMod = Math.Abs(center.X - firstFoV.X);
                    firstFoV += new Vector2(-xMod, -xMod);
                    secondFoV += new Vector2(-xMod, xMod);

                    rv = Util.InBetween(center.Y, firstFoV.Y, secondFoV.Y) && Util.InBetween(center.X, firstFoV.X, _vFirst.X);
                }
                else if (_eDir == DirectionEnum.Right && Util.InBetween(center.X, firstFoV.X, firstFoV.X + _iMaxRange))
                {
                    float xMod = Math.Abs(center.X - firstFoV.X);
                    firstFoV += new Vector2(xMod, -xMod);
                    secondFoV += new Vector2(xMod, xMod);

                    rv = Util.InBetween(center.Y, firstFoV.Y, secondFoV.Y) && Util.InBetween(center.X, _vFirst.X, firstFoV.X);
                }

                return rv;
            }
        }
    }
    #endregion

    #region CombatActors
    public class CombatActor : Actor
    {
        #region Properties
        protected const int MAX_STAT = 99;
        protected string _sUnique;

        public override string Name => String.IsNullOrEmpty(_sUnique) ? _sName : _sName + " " + _sUnique;

        private Vector2 _vStartPos;
        public Vector2 StartPos => _vStartPos;

        protected int _currentHP;
        public int CurrentHP
        {
            get { return _currentHP; }
            set { _currentHP = value; }
        }
        public virtual int MaxHP => 20 + (int)Math.Pow(((double)StatVit / 3), 1.98);

        protected int _currentMP;
        public int CurrentMP
        {
            get { return _currentMP; }
            set { _currentMP = value; }
        }
        public virtual int MaxMP => StatMag * 3; 

        public int CurrentCharge;
        public int DummyCharge;
        public CombatManager.CombatTile Tile;
        public GUICmbtTile Location => Tile.GUITile;

        public virtual int Attack => 9;

        protected int _statStr;
        public virtual int StatStr { get => _statStr + _buffStr; }
        protected int _statDef;
        public virtual int StatDef { get => _statDef + _buffDef; }
        protected int _statVit;
        public virtual int StatVit { get => _statVit + _buffVit; }
        protected int _statMag;
        public virtual int StatMag { get => _statMag + _buffMag; }
        protected int _statRes;
        public virtual int StatRes { get => _statRes + _buffRes; }
        protected int _statSpd;
        public virtual int StatSpd { get => _statSpd + _buffSpd; }

        protected int _buffStr;
        protected int _buffDef;
        protected int _buffVit;
        protected int _buffMag;
        protected int _buffRes;
        protected int _buffSpd;

        public int Evasion => (int)(40 / (1 + 10 * (Math.Pow(Math.E, (-0.05 * StatSpd)))));
        public int ResistStatus => (int)(50 / (1 + 10 * (Math.Pow(Math.E, (-0.05 * StatRes)))));

        protected List<MenuAction> _liActions;
        public virtual List<MenuAction> AbilityList { get => _liActions; }

        protected List<CombatAction> _liSpecialActions;
        public virtual List<CombatAction> SpecialActions { get => _liSpecialActions; }

        protected List<Buff> _liBuffs;
        public List<Buff> LiBuffs { get => _liBuffs; }

        protected Dictionary<ConditionEnum, bool> _diConditions;
        public Dictionary<ConditionEnum, bool> DiConditions => _diConditions;

        private ElementEnum _elementAttackEnum;
        protected Dictionary<ElementEnum, ElementAlignment> _diElementalAlignment;
        public Dictionary<ElementEnum, ElementAlignment> DiElementalAlignment => _diElementalAlignment;

        private Summon _linkedSummon;
        public Summon LinkedSummon => _linkedSummon;

        public bool Counter;
        public bool GoToCounter;
        #endregion

        public CombatActor() : base()
        {
            _actorType = ActorEnum.CombatActor;
            _liSpecialActions = new List<CombatAction>();
            _liActions = new List<MenuAction>();
            _liBuffs = new List<Buff>();
            _diConditions = new Dictionary<ConditionEnum, bool>
            {
                [ConditionEnum.KO] = false,
                [ConditionEnum.Poisoned] = false,
                [ConditionEnum.Silenced] = false
            };

            _diElementalAlignment = new Dictionary<ElementEnum, ElementAlignment>
            {
                [ElementEnum.Fire] = ElementAlignment.Neutral,
                [ElementEnum.Ice] = ElementAlignment.Neutral,
                [ElementEnum.Lightning] = ElementAlignment.Neutral
            };
        }

        public virtual void LoadContent(string texture)
        {
            _sTexture = texture;

            _spriteBody = new AnimatedSprite(texture.Replace(" ",""));
            int xCrawl = 0;
            int frameWidth = 24;
            int frameHeight = 32;
            _spriteBody.AddAnimation(CActorAnimEnum.Idle, frameWidth, frameHeight, 2, 0.5f, (xCrawl * frameWidth), 0);
            xCrawl += 2;
            _spriteBody.AddAnimation(CActorAnimEnum.Cast, frameWidth, frameHeight, 2, 0.4f, (xCrawl * frameWidth), 0);
            xCrawl += 2;
            _spriteBody.AddAnimation(CActorAnimEnum.Hurt, frameWidth, frameHeight, 1, 0.5f, (xCrawl * frameWidth), 0);
            xCrawl += 1;
            _spriteBody.AddAnimation(CActorAnimEnum.Attack, frameWidth, frameHeight, 1, 0.3f, (xCrawl * frameWidth), 0);
            xCrawl += 1;
            _spriteBody.AddAnimation(CActorAnimEnum.Critical, frameWidth, frameHeight, 2, 0.9f, (xCrawl * frameWidth), 0);
            xCrawl += 2;
            _spriteBody.AddAnimation(CActorAnimEnum.KO, frameWidth, frameHeight, 1, 0.5f, (xCrawl * frameWidth), 0);

            _spriteBody.SetCurrentAnimation(CActorAnimEnum.Idle);
            _spriteBody.SetScale(CombatManager.CombatScale);
            _width = _spriteBody.Width;
            _height = _spriteBody.Height;
        }

        public override void Update(GameTime theGameTime)
        {
            //Finished being hit, determine action
            if (IsCurrentAnimation(CActorAnimEnum.Hurt) && BodySprite.GetPlayCount() == 1)
            {
                if (CurrentHP == 0) { PlayAnimation(CActorAnimEnum.KO); }
                else if (IsCritical()) { PlayAnimation(CActorAnimEnum.Critical); }
                else { PlayAnimation(CActorAnimEnum.Idle); }
            }

            if (!_diConditions[ConditionEnum.KO] && IsCurrentAnimation(CActorAnimEnum.KO))
            {
                if (IsCritical()) { PlayAnimation(CActorAnimEnum.Critical); }
                else { PlayAnimation(CActorAnimEnum.Idle); }
            }

            if (IsCurrentAnimation(CActorAnimEnum.Critical) && !IsCritical())
            {
                PlayAnimation(CActorAnimEnum.Idle);
            }

            if (_linkedSummon != null)
            {
                _linkedSummon.Update(theGameTime);
            }
        }

        public int ProcessAttack(CombatActor attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            double compression = 0.8;
            double potencyMod = potency / 100;   //100 potency is considered an average attack
            double base_attack = attacker.Attack;  //Attack stat is either weapon damage or mod on monster str
            double StrMult = Math.Round(1 + ((double)attacker.StatStr / 4 * attacker.StatStr / MAX_STAT), 2);

            double dmg = ( Math.Max(1, base_attack - StatDef) * compression * StrMult);

            dmg += ApplyResistances(dmg, element);
            return DecreaseHealth(dmg);
        }
        public int ProcessSpell(CombatActor attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            double maxDmg = (1 + potency) * 3;
            double divisor = 1 + (30 * Math.Pow(Math.E, -0.12 * (attacker.StatMag - StatRes) * Math.Round((double)attacker.StatMag / MAX_STAT, 2)));

            double damage = Math.Round(maxDmg / divisor);
            damage += ApplyResistances(damage, element);

            return DecreaseHealth(damage);
        }
        public double ApplyResistances(double dmg, ElementEnum element = ElementEnum.None)
        {
            double modifiedDmg = 0;
            if (element != ElementEnum.None)
            {
                if (MapManager.CurrentMap.IsOutside && GameCalendar.IsRaining())
                {
                    if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Fire)) { modifiedDmg += (dmg * 0.8) - dmg; }
                }
                else if (MapManager.CurrentMap.IsOutside && GameCalendar.IsSnowing())
                {
                    if (element.Equals(ElementEnum.Ice)) { modifiedDmg += (dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (dmg * 0.8) - dmg; }
                }

                if (_linkedSummon != null && _diElementalAlignment[element].Equals(ElementAlignment.Neutral))
                {
                    if (_linkedSummon.Element.Equals(element))
                    {
                        modifiedDmg += (dmg * 0.8) - dmg;
                    }
                }

                if (_diElementalAlignment[element].Equals(ElementAlignment.Resists))
                {
                    modifiedDmg += (dmg * 0.8) - dmg;
                }
                else if (_diElementalAlignment[element].Equals(ElementAlignment.Vulnerable))
                {
                    modifiedDmg += (dmg * 1.2) - dmg;
                }
            }

            return modifiedDmg;
        }

        public virtual GUISprite GetSprite()
        {
            return Tile.GUITile.CharacterSprite;
        }

        public virtual int DecreaseHealth(double value)
        {
            int iValue = (int)Math.Round(value);
            _currentHP -= (_currentHP - iValue >= 0) ? iValue : _currentHP;
            PlayAnimation(CActorAnimEnum.Hurt);
            if (_currentHP == 0)
            {
                _diConditions[ConditionEnum.KO] = true;
                UnlinkSummon();
                Tile.GUITile.LinkSummon(null);
            }

            return iValue;
        }

        public int IncreaseHealth(int x)
        {
            int amountHealed = 0;
            if (!KnockedOut())
            {
                amountHealed = x;
                if (_currentHP + x <= MaxHP)
                {
                    _currentHP += x;
                }
                else
                {
                    amountHealed = MaxHP - _currentHP;
                    _currentHP = MaxHP;
                }
            }

            return amountHealed;
        }

        public bool IsCritical()
        {
            return (float)CurrentHP / (float)MaxHP <= 0.25;
        }

        public void IncreaseMana(int x)
        {
            if (_currentMP + x <= MaxMP)
            {
                _currentMP += x;
            }
            else
            {
                _currentMP = MaxMP;
            }
        }

        public void TickBuffs()
        {
            List<Buff> toRemove = new List<Buff>();
            foreach (Buff b in _liBuffs)
            {
                if (--b.Duration == 0)
                {
                    toRemove.Add(b);
                    foreach (KeyValuePair<string, int> kvp in b.StatMods)
                    {
                        switch (kvp.Key)
                        {
                            case "Str":
                                _buffStr -= kvp.Value;
                                break;
                            case "Def":
                                _buffDef -= kvp.Value;
                                break;
                            case "Vit":
                                _buffVit -= kvp.Value;
                                break;
                            case "Mag":
                                _buffMag -= kvp.Value;
                                break;
                            case "Res":
                                _buffRes -= kvp.Value;
                                break;
                            case "Spd":
                                _buffSpd -= kvp.Value;
                                break;
                        }
                    }
                }
                else
                {
                    if (b.DoT)
                    {
                        this.Tile.GUITile.AssignEffect(ProcessSpell(b.Caster, b.Potency), true);
                    }
                }
            }

            foreach (Buff b in toRemove)
            {
                _liBuffs.Remove(b);
            }
            toRemove.Clear();
        }

        public void AddBuff(Buff b)
        {
            Buff find = _liBuffs.Find(buff => buff.Name == b.Name);
            if (find == null) { _liBuffs.Add(b); }
            else { find.Duration += b.Duration; }

            foreach (KeyValuePair<string, int> kvp in b.StatMods)
            {
                switch (kvp.Key)
                {
                    case "Str":
                        _buffStr -= kvp.Value;
                        break;
                    case "Def":
                        _buffDef -= kvp.Value;
                        break;
                    case "Vit":
                        _buffVit -= kvp.Value;
                        break;
                    case "Mag":
                        _buffMag -= kvp.Value;
                        break;
                    case "Res":
                        _buffRes -= kvp.Value;
                        break;
                    case "Spd":
                        _buffSpd -= kvp.Value;
                        break;
                }
            }
        }

        public void LinkSummon(Summon s)
        {
            _linkedSummon = s;
            _linkedSummon.Position = Position - new Vector2(100, 100);
            s.Tile = Tile;
        }

        public void UnlinkSummon()
        {
            _linkedSummon = null;
        }

        public virtual ElementEnum GetAttackElement()
        {
            ElementEnum e = _elementAttackEnum;

            if (LinkedSummon != null && e.Equals(ElementEnum.None))
            {
                e = LinkedSummon.Element;
            }
            return e;
        }

        public bool CanCast(int x)
        {
            return x <= CurrentMP;
        }

        public void SetUnique(string u)
        {
            _sUnique = u;
        }

        public bool KnockedOut()
        {
            return _diConditions[ConditionEnum.KO];
        }

        public bool Poisoned()
        {
            return _diConditions[ConditionEnum.Poisoned];
        }

        public bool Silenced()
        {
            return _diConditions[ConditionEnum.Silenced];
        }

        public void ChangeConditionStatus(ConditionEnum c, bool setTo)
        {
            _diConditions[c] = setTo;
        }

        public void ClearConditions()
        {
            foreach (ConditionEnum condition in Enum.GetValues(typeof(ConditionEnum)))
            {
                ChangeConditionStatus(condition, false);
            }
        }

        public void IncreaseStartPos()
        {
            if (_vStartPos.Y < CombatManager.MAX_ROW)
            {
                _vStartPos.Y++;
            }
            else
            {
                _vStartPos = new Vector2(_vStartPos.X++, 0);
            }
        }
        public void SetStartPosition(Vector2 startPos)
        {
            _vStartPos = startPos;
        }

        public virtual bool IsSummon() { return false; }
    }

    public class CombatAdventurer : CombatActor
    {
        #region Properties
        public static List<int> LevelRange = new List<int> { 0, 10, 40, 100, 200, 600, 800, 1200, 1600, 2000 };
        private WorldCombatant _world;
        public WorldCombatant World => _world;

        protected CharacterClass _class;
        public CharacterClass CharacterClass { get => _class; }
        private int _classLevel;
        public int ClassLevel { get => _classLevel; }

        private int _xp;
        public int XP { get => _xp; }

        public bool Protected;

        public AnimatedSprite SpriteWeapon;

        public Equipment Weapon;
        public Equipment TempWeapon;
        public Equipment Armor;
        public Equipment TempArmor;
        public Equipment Head;
        public Equipment TempHead;
        public Equipment Wrist;
        public Equipment TempWrist;

        public override int Attack => GetGearAtk();
        public override int StatStr => 10 + _buffStr + GetGearStr();
        public override int StatDef => 10 + _buffDef + GetGearDef() + (Protected ? 10 : 0);
        public override int StatVit => 10 + (_classLevel * _class.StatVit) + GetGearVit();
        public override int StatMag => 10 +  _buffMag + GetGearMag();
        public override int StatRes => 10 + _buffRes + GetGearRes();
        public override int StatSpd => 10 + _class.StatSpd +_buffSpd + GetGearSpd();

        public override List<MenuAction> AbilityList { get => _class.ActionList; }
        public override List<CombatAction> SpecialActions { get => _class._liSpecialActionsList; }

        public int GetGearAtk()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Attack; }
            else if (Weapon != null) { rv += Weapon.Attack; }

            rv += base.Attack;

            return rv;
        }
        public int GetGearStr()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Str; }
            else if (Weapon != null) { rv += Weapon.Str; }
            if (TempArmor != null) { rv += TempArmor.Str; }
            else if (Armor != null) { rv += Armor.Str; }

            return rv;
        }
        public int GetGearDef()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Def; }
            else if (Weapon != null) { rv += Weapon.Def; }
            if (TempArmor != null) { rv += TempArmor.Def; }
            else if (Armor != null) { rv += Armor.Def; }

            return rv;
        }
        public int GetGearVit()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Vit; }
            else if (Weapon != null) { rv += Weapon.Vit; }
            if (TempArmor != null) { rv += TempArmor.Vit; }
            else if (Armor != null) { rv += Armor.Vit; }

            return rv;
        }
        public int GetGearMag()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Mag; }
            else if (Weapon != null) { rv += Weapon.Mag; }
            if (TempArmor != null) { rv += TempArmor.Mag; }
            else if (Armor != null) { rv += Armor.Mag; }

            return rv;
        }
        public int GetGearRes()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Res; }
            else if (Weapon != null) { rv += Weapon.Res; }
            if (TempArmor != null) { rv += TempArmor.Res; }
            else if (Armor != null) { rv += Armor.Res; }

            return rv;
        }
        public int GetGearSpd()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Spd; }
            else if (Weapon != null) { rv += Weapon.Spd; }
            if (TempArmor != null) { rv += TempArmor.Spd; }
            else if (Armor != null) { rv += Armor.Spd; }

            return rv;
        }

        #endregion
        public CombatAdventurer(WorldCombatant w) : this()
        {
            _sName = w.Name;
            _world = w;

            SpriteWeapon = new AnimatedSprite(@"Textures\Staves");
            SpriteWeapon.AddAnimation(CActorAnimEnum.Idle, 32, 32, 2, 0.4f, 0, 0);
            SpriteWeapon.SetScale(CombatManager.CombatScale);
        }

        public CombatAdventurer() : base()
        {
            _actorType = ActorEnum.CombatAdventurer;
            _classLevel = 1;
        }

        public override void LoadContent(string texture)
        {
            base.LoadContent(texture);

            _sTexture = texture;

            _spriteBody = new AnimatedSprite(texture.Replace(" ", ""));
            int xCrawl = 0;
            int frameWidth = 32;
            int frameHeight = 32;
            _spriteBody.AddAnimation(CActorAnimEnum.Idle, frameWidth, frameHeight, _class.IdleFrames, _class.IdleFramesLength, (xCrawl * frameWidth), 0);
            xCrawl += _class.IdleFrames;
            _spriteBody.AddAnimation(CActorAnimEnum.Cast, frameWidth, frameHeight, _class.CastFrames, _class.CastFramesLength, (xCrawl * frameWidth), 0);
            xCrawl += _class.CastFrames;
            _spriteBody.AddAnimation(CActorAnimEnum.Hurt, frameWidth, frameHeight, _class.HitFrames, _class.HitFramesLength, (xCrawl * frameWidth), 0);
            xCrawl += _class.HitFrames;
            _spriteBody.AddAnimation(CActorAnimEnum.Attack, frameWidth, frameHeight, _class.AttackFrames, _class.AttackFramesLength, (xCrawl * frameWidth), 0);
            xCrawl += _class.AttackFrames;
            _spriteBody.AddAnimation(CActorAnimEnum.Critical, frameWidth, frameHeight, _class.CriticalFrames, _class.CriticalFramesLength, (xCrawl * frameWidth), 0);
            xCrawl += _class.CriticalFrames;
            _spriteBody.AddAnimation(CActorAnimEnum.KO, frameWidth, frameHeight, _class.KOFrames, _class.KOFramesLength, (xCrawl * frameWidth), 0);
            xCrawl += _class.KOFrames;
            _spriteBody.AddAnimation(CActorAnimEnum.Win, frameWidth, frameHeight, _class.WinFrames, _class.WinFramesLength, (xCrawl * frameWidth), 0);

            _spriteBody.SetCurrentAnimation(CActorAnimEnum.Idle);
            _spriteBody.SetScale(CombatManager.CombatScale);
            _width = _spriteBody.Width;
            _height = _spriteBody.Height;
        }

        public void SetClass(CharacterClass x)
        {
            _class = x;
            _currentHP = MaxHP/2;
            _currentMP = MaxMP;

            Weapon = (Equipment)GetItem(_class.WeaponID);
            Armor = (Equipment)GetItem(_class.ArmorID);
            Head = (Equipment)GetItem(_class.HeadID);
            Wrist = (Equipment)GetItem(_class.WristID);
        }

        public void AddXP(int x)
        {
            _xp += x;
            if (_xp >= LevelRange[_classLevel])
            {
                _classLevel++;
            }
        }

        public override void PlayAnimation(string animation)
        {
            base.PlayAnimation(animation);
            SpriteWeapon.SetCurrentAnimation(animation);
        }

        public AdventurerData SaveData()
        {
            AdventurerData advData = new AdventurerData
            {
                armor = Item.SaveData(Armor),
                weapon = Item.SaveData(Weapon),
                level = _classLevel,
                xp = _xp
            };

            return advData;
        }
        public void LoadData(AdventurerData data)
        {
            Armor = (Equipment)ObjectManager.GetItem(data.armor.itemID, data.armor.num);
            Weapon = (Equipment)ObjectManager.GetItem(data.weapon.itemID, data.weapon.num);
            _classLevel = data.level;
            _xp = data.xp;
        }
    }

    public class Monster : CombatActor
    {
        #region Properties
        int _id;
        public int ID { get => _id; }
        int _iRating;
        int _xp;
        public int XP { get => _xp; }
        protected Vector2 _moveTo = Vector2.Zero;

        public override int Attack => 20 + (_iRating * 10);

        public override int MaxHP => (int)((((Math.Pow(_iRating, 2))* 10) + 20) * Math.Pow(Math.Max(1, (double)_iRating / 14), 2));

        #endregion

        public Monster(int id, string[] stringData)
        {
            _actorType = ActorEnum.Monster;
            ImportBasics(stringData, id);
        }

        protected void ImportBasics(string[] stringData, int id)
        {
            _id = id;
            _sName = GameContentManager.GetGameText("Monster " + _id);

            string texture = string.Empty;
            float[] idle = new float[2] { 2, 0.5f };
            float[] attack = new float[2] { 2, 0.2f };
            float[] hurt = new float[2] { 1, 0.5f };

            foreach (string s in stringData)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Texture"))
                {
                    texture = tagType[1];
                }
                else if (tagType[0].Equals("Lvl"))
                {
                    _iRating = int.Parse(tagType[1]);
                    _xp = _iRating * 10;
                    _statStr = 1 + _iRating;
                    _statDef = 8 + (_iRating * 3);
                    _statVit = 2 * _iRating + 10;
                    _statMag = 2 * _iRating + 2;
                    _statRes = 2 * _iRating + 10;
                    _statSpd = 10;
                }
                else if (tagType[0].Equals("Ability"))
                {
                    string[] split = tagType[1].Split('-');
                    foreach (string ability in split)
                    {
                        AbilityList.Add(ObjectManager.GetActionByIndex(int.Parse(ability)));
                    }
                }
                else if (tagType[0].Equals("Trait"))
                {
                    HandleTrait(GameContentManager.GetMonsterTraitData(tagType[1]));
                }
                else if (tagType[0].Equals("Resist"))
                {
                    string[] split = tagType[1].Split('-');
                    foreach (string elem in split)
                    {
                        _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Resists;
                    }
                }
                else if (tagType[0].Equals("Vuln"))
                {
                    string[] split = tagType[1].Split('-');
                    foreach (string elem in split)
                    {
                        _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Vulnerable;
                    }
                }
                else if (tagType[0].Equals("Idle"))
                {
                    string[] split = tagType[1].Split('-');
                    idle[0] = float.Parse(split[0]);
                    idle[1] = float.Parse(split[1]);
                }
                else if (tagType[0].Equals("Attack"))
                {
                    string[] split = tagType[1].Split('-');
                    attack[0] = float.Parse(split[0]);
                    attack[1] = float.Parse(split[1]);
                }
                else if (tagType[0].Equals("Hurt"))
                {
                    string[] split = tagType[1].Split('-');
                    hurt[0] = float.Parse(split[0]);
                    hurt[1] = float.Parse(split[1]);
                }
            }

            LoadContent(_sMonsterFolder + texture, idle, attack, hurt);

            _currentHP = MaxHP;
            _currentMP = MaxMP;
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
            if(BodySprite.CurrentAnimation == Util.GetEnumString(CActorAnimEnum.KO) && BodySprite.CurrentFrameAnimation.PlayCount == 1)
            {
                CombatManager.Kill(this);
            }
        }

        private void HandleTrait(string traitData)
        {
            string[] traits = Util.FindTags(traitData);
            foreach (string s in traits)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Str"))
                {
                    ApplyTrait(ref _statStr, tagType[1]);
                }
                else if (tagType[0].Equals("Def"))
                {
                    ApplyTrait(ref _statDef, tagType[1]);
                }
                else if (tagType[0].Equals("Vit"))
                {
                    ApplyTrait(ref _statVit, tagType[1]);
                }
                else if (tagType[0].Equals("Mag"))
                {
                    ApplyTrait(ref _statMag, tagType[1]);
                }
                else if (tagType[0].Equals("Res"))
                {
                    ApplyTrait(ref _statRes, tagType[1]);
                }
                else if (tagType[0].Equals("Spd"))
                {
                    ApplyTrait(ref _statSpd, tagType[1]);
                }
            }
        }

        private void ApplyTrait(ref int value, string data)
        {
            if (data.Equals("+"))
            {
                value = (int)(value * 1.1);
            }
            else if (data.Equals("-"))
            {
                value = (int)(value * 0.9);
            }
        }

        public void LoadContent(string texture, float[] idle, float[] attack, float[] hurt)
        {
            _sTexture = texture;

            _spriteBody = new AnimatedSprite(texture.Replace(" ", ""));
            int yCrawl = 0;
            int frameWidth = 24;
            int frameHeight = 32;

            _spriteBody.AddAnimation(CActorAnimEnum.Idle, frameWidth, frameHeight, (int)idle[0], idle[1], 0, (yCrawl++ * frameHeight));
            _spriteBody.AddAnimation(CActorAnimEnum.Attack, frameWidth, frameHeight, (int)attack[0], attack[1], 0, (yCrawl++ * frameHeight));
            _spriteBody.AddAnimation(CActorAnimEnum.Hurt, frameWidth, frameHeight, (int)hurt[0], hurt[1], 0, (yCrawl++ * frameHeight));
            _spriteBody.AddAnimation(CActorAnimEnum.Cast, frameWidth, frameHeight, 1, 0.5f, 0, (yCrawl++ * frameHeight));
            _spriteBody.AddAnimation(CActorAnimEnum.KO, frameWidth, frameHeight, 3, 0.3f, 0, (yCrawl++ * frameHeight));

            _spriteBody.SetCurrentAnimation(CActorAnimEnum.Idle);
            _spriteBody.SetScale(CombatManager.CombatScale);
            _width = _spriteBody.Width;
            _height = _spriteBody.Height;
        }
    }

    public class Summon : CombatActor
    {
        ElementEnum _element = ElementEnum.None;
        public ElementEnum Element => _element;

        int _iMagStat;

        public bool Acted;
        public bool Swapped;
        bool _bTwinCast;
        public bool TwinCast => _bTwinCast;
        bool _bAggressive;
        public bool Aggressive => _bAggressive;
        bool _bDefensive;
        public bool Defensive => _bDefensive;

        public CombatActor linkedChar;

        public Summon()
        {
            _spriteBody = new AnimatedSprite(@"Textures\Eye");
            _spriteBody.AddAnimation(CActorAnimEnum.Idle, 0, 0, 16, 16, 2, 0.9f);
            _spriteBody.AddAnimation(CActorAnimEnum.Attack, 32, 0, 16, 16, 4, 0.1f);
            _spriteBody.AddAnimation(CActorAnimEnum.Cast, 32, 0, 16, 16, 4, 0.1f);
            _spriteBody.SetCurrentAnimation(CActorAnimEnum.Idle);
            _spriteBody.SetScale(5);
        }

        public Summon Clone()
        {
            Summon copy = new Summon();
            copy.SetStats(_iMagStat);
            if (TwinCast) { copy.SetTwincast(); }
            if (Aggressive) { copy.SetAggressive(); }
            if (Counter) { copy.Counter = Counter; }
            if (Defensive) { copy.SetDefensive(); }

            copy._element = _element;
            copy.Tile = Tile;

            return copy;
        }

        public void SetStats(int magStat)
        {
            _iMagStat = magStat;
            _statStr = 2 * magStat + 10;
            _statDef = 2 * magStat + 10;
            _statVit = (3 * magStat) + 80;
            _statMag = 2 * magStat + 10;
            _statRes = 2 * magStat + 10;
            _statSpd = 10;

            CurrentHP = MaxHP;
        }

        public override int DecreaseHealth(double value)
        {
            int rv = base.DecreaseHealth(value);

            if (CurrentHP == 0)
            {
                linkedChar.UnlinkSummon();
            }

            return rv;
        }
        public override GUISprite GetSprite()
        {
            return Tile.GUITile.SummonSprite;
        }

        public void SetTwincast() { _bTwinCast = true; }
        public void SetAggressive() { _bAggressive = true; }
        public void SetDefensive() { _bDefensive = true; }
        public void SetElement(ElementEnum el) { _element = el; }

        public override bool IsSummon() { return true; }
    }
    #endregion
}
