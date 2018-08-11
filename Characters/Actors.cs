using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIObjects;
using System;
using RiverHollow.WorldObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;
using RiverHollow.Misc;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Tile_Engine;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Game_Managers.GUIObjects.Screens;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.ObjectManager;
using static RiverHollow.Game_Managers.GUIObjects.ManagementScreen;
namespace RiverHollow.Actors
{
    public class Actor
    {
        protected string _sTexture;
        public enum ActorEnum { Actor, CombatAdventurer, CombatActor, Mob, Monster, NPC, Spirit, WorldAdventurer, WorldCharacter};
        protected ActorEnum _actorType = ActorEnum.Actor;
        public ActorEnum ActorType => _actorType;

        protected string _sName;
        public virtual string Name { get => _sName; }

        protected AnimatedSprite _bodySprite;
        public AnimatedSprite BodySprite { get => _bodySprite; }

        public virtual Vector2 Position
        {
            get { return new Vector2(_bodySprite.Position.X, _bodySprite.Position.Y); }
            set { _bodySprite.Position = value; }
        }
        public virtual Vector2 Center { get => _bodySprite.Center; }

        public Rectangle GetRectangle()
        {
            return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
        }

        protected int _width;
        public int Width { get => _width; }
        protected int _height;
        public int Height { get => _height; }
        public int SpriteWidth { get => _bodySprite.Width; }
        public int SpriteHeight { get => _bodySprite.Height; }

        protected bool _bCanTalk = false;
        public bool CanTalk => _bCanTalk;

        public Actor() { }

        public virtual void LoadContent(string textureToLoad, int frameWidth, int frameHeight, int numFrames, float frameSpeed,int startX = 0, int startY = 0)
        {
            _sTexture = textureToLoad;
            _bodySprite = new AnimatedSprite(_sTexture);
            _bodySprite.AddAnimation("Walk", frameWidth, frameHeight, numFrames, frameSpeed, startX, startY);
            _bodySprite.AddAnimation("Attack", frameWidth, frameHeight, numFrames, frameSpeed, startX, startY);
            _bodySprite.SetCurrentAnimation("Walk");

            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
        }

        public virtual void Update(GameTime theGameTime)
        {
            _bodySprite.Update(theGameTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            _bodySprite.Draw(spriteBatch, useLayerDepth);
        }

        public virtual void SetName(string text)
        {
            _sName = text;
        }

        public virtual void PlayAnimation(string animation)
        {
            _bodySprite.SetCurrentAnimation(animation);
        }

        public bool Contains(Point x) { return _bodySprite.BoundingBox.Contains(x); }
        public bool AnimationFinished() { return _bodySprite.PlayedOnce && _bodySprite.IsAnimating; }
        public bool IsCurrentAnimation(string val) { return _bodySprite.CurrentAnimation.Equals(val); }
        public bool IsAnimating() { return _bodySprite.IsAnimating; }
        public bool AnimationPlayedXTimes(int x) { return _bodySprite.GetPlayCount() >= x; }

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
        public Texture2D Texture { get => _bodySprite.Texture; }
        public Point CharCenter => GetRectangle().Center;
        public override Vector2 Position
        {
            get { return new Vector2(_bodySprite.Position.X, _bodySprite.Position.Y + _bodySprite.Height - TileSize); } //MAR this is fucked up
            set { _bodySprite.Position = new Vector2(value.X, value.Y - _bodySprite.Height + TileSize); }
        }

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
            AddDefaultAnimations(ref _bodySprite, 0, 0);

            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
        }

        public void AddDefaultAnimations(ref AnimatedSprite sprite, int startX, int startY, bool pingpong = false)
        {
            AddDefaultAnimations(ref sprite, _sTexture, startX, startY, pingpong);
        }
        public void AddDefaultAnimations(ref AnimatedSprite sprite, string texture, int startX, int startY, bool pingpong = false)
        {
            sprite = new AnimatedSprite(texture, pingpong);
            sprite.AddAnimation("WalkDown", TileSize, TileSize * 2, 3, 0.2f, startX, startY);
            sprite.AddAnimation("IdleDown", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize, startY);
            sprite.AddAnimation("WalkUp", TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 3, startY);
            sprite.AddAnimation("IdleUp", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 4, startY);
            sprite.AddAnimation("WalkLeft", TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 6, startY);
            sprite.AddAnimation("IdleLeft", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 7, startY);
            sprite.AddAnimation("WalkRight", TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 9, startY);
            sprite.AddAnimation("IdleRight", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 10, startY);
            sprite.SetCurrentAnimation("IdleDown");
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
            if (d == DirectionEnum.Up) { _bodySprite.CurrentAnimation = "Walk North"; }
            else if (d == DirectionEnum.Down) { _bodySprite.CurrentAnimation = "Walk South"; }
            else if (d == DirectionEnum.Right) { _bodySprite.CurrentAnimation = "Walk East"; }
            else if (d == DirectionEnum.Left) { _bodySprite.CurrentAnimation = "Walk West"; }
        }

        public void DetermineFacing(Vector2 direction)
        {
            string animation = string.Empty;

            if (Math.Abs((int)direction.X) > Math.Abs((int)direction.Y))
            {
                if (direction.X > 0)
                {
                    Facing = DirectionEnum.Right;
                    animation = "WalkRight";
                }
                else if (direction.X < 0)
                {
                    Facing = DirectionEnum.Left;
                    animation = "WalkLeft";
                }
            }
            else
            {
                if (direction.Y > 0)
                {
                    Facing = DirectionEnum.Down;
                    animation = "WalkDown";
                }
                else if (direction.Y < 0)
                {
                    Facing = DirectionEnum.Up;
                    animation = "WalkUp";
                }
            }

            if (direction.Length() == 0)
            {
                Idle();
            }

            if (_bodySprite.CurrentAnimation != animation)
            {
                PlayAnimation(animation);
            }
        }

        public virtual void Idle()
        {
            switch (Facing)
            {
                case DirectionEnum.Down:
                    PlayAnimation("IdleDown");
                    break;
                case DirectionEnum.Up:
                    PlayAnimation("IdleUp");
                    break;
                case DirectionEnum.Left:
                    PlayAnimation("IdleLeft");
                    break;
                case DirectionEnum.Right:
                    PlayAnimation("IdleRight");
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
                Position += new Vector2(direction.X, direction.Y);
                rv = true;
            }

            string animation = string.Empty;
            DetermineFacing(direction);

            return rv;
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

        protected Texture2D _portrait;
        public Texture2D Portrait { get => _portrait; }

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
            GUIManager.SetScreen(new TextScreen(this, text));
        }
        public void Talk(string dialogTag)
        {
            string text = string.Empty;
            if (_dialogueDictionary.ContainsKey(dialogTag))
            {
                text = _dialogueDictionary[dialogTag];
            }
            text = Util.ProcessText(text, _sName);
            GUIManager.SetScreen(new TextScreen(this, text));
        }
        public void DrawPortrait(SpriteBatch spriteBatch, Vector2 dest)
        {
            if (_portrait != null)
            {
                spriteBatch.Draw(_portrait, new Vector2(dest.X, dest.Y - PortraitRectangle.Height), PortraitRectangle, Color.White);
            }
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
    }
    public class Villager : TalkingActor
    {
        protected int _index;
        public int ID { get => _index; }
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
        List<KeyValuePair<string, List<RHTile>>> _todaysPathing = null;                             //List of Times with the associated pathing
        protected List<RHTile> _currentPath;                                                        //List of Tiles to currently be traversing
        protected int _scheduleIndex;

        protected double _dEtherealCD;
        protected bool _bIgnoreCollisions;

        public Villager() { }
        public Villager(Villager n)
        {
            _actorType = ActorEnum.NPC;
            _index = n.ID;
            _sName = n.Name;
            _dialogueDictionary = n._dialogueDictionary;
            _portrait = n.Portrait;
            _portraitRect = n._portraitRect;

            LoadContent();
        }

        public Villager(int index, string[] data)
        {
            _collection = new Dictionary<int, bool>();
            _completeSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();
            _scheduleIndex = 0;

            LoadContent();
            ImportBasics(index, data);

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

        protected int ImportBasics(int id, string[] stringData)
        {
            _index = id;
            _sName = GameContentManager.GetGameText("NPC" + _index);
            _portrait = GameContentManager.GetTexture(@"Textures\portraits");
            _dialogueDictionary = GameContentManager.LoadDialogue(@"Data\Dialogue\NPC" + _index);

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
                    _portraitRect = new Rectangle(0, int.Parse(tagType[1]) * 192, PortraitWidth, PortraitHeight);
                    totalCount++;
                }
                else if (tagType[0].Equals("HomeMap"))
                {
                    CurrentMapName = tagType[1];
                    _homeMap = CurrentMapName;
                    Position = Util.SnapToGrid(MapManager.Maps[CurrentMapName].GetCharacterSpawn("NPC" + _index));

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

            Dictionary<string, string> schedule = CharacterManager.GetSchedule("NPC" + _index);
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

        private void HandleMove(Vector2 target)
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

        public virtual void RollOver()
        {
            MapManager.Maps[CurrentMapName].RemoveCharacter(this);
            RHMap map = MapManager.Maps[_homeMap];
            string Spawn = "NPC" + _index;

            Position = Util.SnapToGrid(map.GetCharacterSpawn(Spawn));
            map.AddCharacter(this);

            CalculatePathing();
        }
        public void CalculatePathing()
        {
            string currDay = GameCalendar.GetDayOfWeek();
            string currSeason = GameCalendar.GetSeason();
            string currWeather = GameCalendar.GetWeather();
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

        public void LoadContent()
        {
            if (_index != 8) { _sTexture = @"Textures\NPC1"; }
            else { _sTexture = @"Textures\NPC8"; }

            _bodySprite = new AnimatedSprite(_sTexture);

            int startX = 0;
            int startY = 0;
            _bodySprite.AddAnimation("IdleDown", startX + TileSize, startY, TileSize, TileSize * 2, 1, 0.2f);
            _bodySprite.AddAnimation("WalkDown", startX, startY, TileSize, TileSize * 2, 3, 0.2f);

            startX += TileSize * 3;
            _bodySprite.AddAnimation("IdleUp", startX + TileSize, startY, TileSize, TileSize * 2, 1, 0.2f);
            _bodySprite.AddAnimation("WalkUp", startX, startY, TileSize, TileSize * 2, 3, 0.2f);

            startX += TileSize * 3;
            _bodySprite.AddAnimation("IdleLeft", startX + TileSize, startY, TileSize, TileSize * 2, 1, 0.2f);
            _bodySprite.AddAnimation("WalkLeft", startX, startY, TileSize, TileSize * 2, 3, 0.2f);

            startX += TileSize * 3;
            _bodySprite.AddAnimation("IdleRight", startX + TileSize, startY, TileSize, TileSize * 2, 1, 0.2f);
            _bodySprite.AddAnimation("WalkRight", startX, startY, TileSize, TileSize * 2, 3, 0.2f);

            _bodySprite.SetCurrentAnimation("IdleDown");
            _bodySprite.IsAnimating = true;
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
                        MapManager.Maps["mapHouseNPC" + _index].AddCollectionItem(item.ItemID, _index, index);
                    }
                    else
                    {
                        text = GetDialogEntry("Gift");
                        FriendshipPoints += 1000;
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    GUIManager.SetScreen(new TextScreen(this, text));
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

            LoadContent();

            _index = index;
            int i = ImportBasics(index, stringData);
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
            GUIManager.SetScreen(new TextScreen(this, text));
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
                GUIManager.SetScreen(null);
                GameManager.Scry(true);
                GameManager.MoveBuilding();
                Camera.UnsetObserver();
                MapManager.ViewMap(MapManager.HomeMap);
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("UpgradeBuilding"))
            {
                ManagementScreen m = new ManagementScreen(ActionTypeEnum.Upgrade);
                GUIManager.SetScreen(m);
                GameManager.ClearGMObjects();
            }
            else if (entry.Equals("Destroy"))
            {
                GUIManager.SetScreen(null);
                GameManager.Scry(true);
                GameManager.DestroyBuilding();
                Camera.UnsetObserver();
                MapManager.ViewMap(MapManager.HomeMap);
                GameManager.ClearGMObjects();
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

            LoadContent();

            _index = index;
            int i = ImportBasics(index, stringData);
            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("Class"))
                {
                    _combat = new CombatAdventurer(this);
                    _combat.SetClass(CharacterManager.GetClassByIndex(int.Parse(tagType[1])));
                    _combat.LoadContent(@"Textures\" + _combat.CharacterClass.Name);
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
            string Spawn = Married ? "Spouse" : "NPC" + _index;

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
                        item.Number++;
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
                        MapManager.Maps["mapHouseNPC" + _index].AddCollectionItem(item.ItemID, _index, index);
                    }
                    else
                    {
                        text = GetDialogEntry("Gift");
                        FriendshipPoints += 10;
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    GUIManager.SetScreen(new TextScreen(this, text));
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
        private WorkerBuilding _building;
        public WorkerBuilding Building => _building;
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

        Dictionary<int, Recipe> _diCrafting;
        public Dictionary<int, Recipe> CraftList => _diCrafting;
        Recipe _currentlyMaking;
        public Recipe CurrentlyMaking => _currentlyMaking;
        #endregion

        public WorldAdventurer(string[] stringData, int id)
        {
            _iAdventurerID = id;
            _actorType = ActorEnum.WorldAdventurer;
            ImportBasics(stringData, id);
            SetCombat();

            _sAdventurerType = Combat.CharacterClass.Name;
            _sTexture = @"Textures\" + _sAdventurerType;
            _portraitRect = new Rectangle(0, 105, 80, 96);
            _portrait = GameContentManager.GetTexture(_sTexture);

            LoadContent();
            _iCurrFood = 0;
            _heldItem = null;
            _iMood = 0;
            DrawIt = true;
            Adventuring = false;
        }

        public void LoadContent()
        {
            _bodySprite = new AnimatedSprite(_sTexture, true);
            _bodySprite.AddAnimation("Idle", TileSize, 0, TileSize, TileSize * 2, 1, 0.3f);
            _bodySprite.AddAnimation("WalkDown", 0, 0, TileSize, TileSize * 2, 3, 0.3f);
            _bodySprite.SetCurrentAnimation("Idle");
        }

        protected void ImportBasics(string[] stringData, int id)
        {
            _diCrafting = new Dictionary<int, Recipe>();

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
                        _diCrafting.Add(int.Parse(recipe), ObjectManager.DictCrafting[int.Parse(recipe)]);
                    }
                }
            }
        }

        protected void SetCombat()
        {
            _combat = new CombatAdventurer(this);
            _combat.SetClass(CharacterManager.GetClassByIndex(_iAdventurerID));
            _combat.LoadContent(@"Textures\" + _combat.CharacterClass.Name);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_currentlyMaking != null)
            {
                _bodySprite.Update(gameTime);
                _dProcessedTime += gameTime.ElapsedGameTime.TotalSeconds;
                int modifiedTime = (int)(_currentlyMaking.ProcessingTime * (0.5 + 0.5 * ((100 - Mood) / 100)));   //Workers work faster the happier they are.
                if (_dProcessedTime >= modifiedTime)        //NPCs
                {
                    //SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition);
                    _heldItem = ObjectManager.GetItem(_currentlyMaking.Output);
                    _combat.AddXP(_currentlyMaking.XP);
                    _dProcessedTime = -1;
                    _currentlyMaking = null;
                    _bodySprite.SetCurrentAnimation("Idle");
                }
            }
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
            else if (entry.Equals("Craft"))
            {
                GUIManager.SetScreen(new CraftingScreen(this));
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
            _currentlyMaking = _diCrafting[itemID];
            _bodySprite.SetCurrentAnimation("Working");
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

        public void SetBuilding(WorkerBuilding b)
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
                currentItemID = (this._currentlyMaking == null) ? -1 : this._currentlyMaking.Output,
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
            _currentlyMaking = (data.currentItemID == -1) ? null : _diCrafting[data.currentItemID];
            _heldItem = ObjectManager.GetItem(data.heldItemID);
            Adventuring = data.adventuring;

            SetCombat();
            Combat.LoadData(data.advData);

            if (_currentlyMaking != null) { _bodySprite.SetCurrentAnimation("Working"); }
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
        AnimatedSprite _spriteArms;
        public AnimatedSprite ArmSprite => _spriteArms;
        AnimatedSprite _spriteHair;
        public AnimatedSprite HairSprite => _spriteHair;

        Color _cHairColor = Color.White;
        public Color HairColor => _cHairColor;

        int _iHairIndex = 0;
        public int HairIndex => _iHairIndex;

        public override Vector2 Position
        {
            get { return new Vector2(_bodySprite.Position.X, _bodySprite.Position.Y + _bodySprite.Height - TileSize); }
            set
            {
                _bodySprite.Position = new Vector2(value.X, value.Y - _bodySprite.Height + TileSize);
                _spriteEyes.Position = _bodySprite.Position;
                _spriteArms.Position = _bodySprite.Position;
                _spriteHair.Position = _bodySprite.Position;

                if (_chest != null) { _chest.SetSpritePosition(_bodySprite.Position); }
                if (Hat != null) { _hat.SetSpritePosition(_bodySprite.Position); }
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
        }

        public override void Update(GameTime theGameTime)
        {
            _bodySprite.Update(theGameTime);
            _spriteEyes.Update(theGameTime);
            _spriteArms.Update(theGameTime);
            _spriteHair.Update(theGameTime);

            if (_chest != null) { _chest.Sprite.Update(theGameTime); }
            if (Hat != null) { Hat.Sprite.Update(theGameTime); }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            _bodySprite.Draw(spriteBatch, useLayerDepth);
            _spriteEyes.Draw(spriteBatch, useLayerDepth);
            _spriteArms.Draw(spriteBatch, useLayerDepth);
            _spriteHair.Draw(spriteBatch, useLayerDepth);

            if (_chest != null) { _chest.Sprite.Draw(spriteBatch, useLayerDepth); }
            if (Hat != null) { Hat.Sprite.Draw(spriteBatch, useLayerDepth); }
        }

        public override void LoadContent(string textureToLoad)
        {
            Color bodyColor = Color.White;

            AddDefaultAnimations(ref _bodySprite, textureToLoad, 0, 0, true);
            _bodySprite.SetColor(bodyColor);

            AddDefaultAnimations(ref _spriteEyes, textureToLoad, 0, TileSize * 2, true);
            _spriteEyes.SetDepthMod(0.001f);

            AddDefaultAnimations(ref _spriteArms, textureToLoad, 0, TileSize * 4, true);
            _spriteArms.SetDepthMod(0.002f);
            _spriteArms.SetColor(bodyColor);

            AddDefaultAnimations(ref _spriteHair, @"Textures\texPlayerHair", 0, _iHairIndex * TileSize * 2, true);
            _spriteHair.SetColor(_cHairColor);
            _spriteHair.SetDepthMod(0.003f);

            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
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
            AddDefaultAnimations(ref _spriteHair, @"Textures\texPlayerHair", 0, _iHairIndex * TileSize * 2, true);
            _spriteHair.SetColor(_cHairColor);
            _spriteHair.SetDepthMod(0.003f);
        }

        public override void Idle()
        {
            string animation = string.Empty;
            switch (Facing)
            {
                case DirectionEnum.Down:
                    animation = "IdleDown";
                    break;
                case DirectionEnum.Up:
                    animation = "IdleUp";
                    break;
                case DirectionEnum.Left:
                    animation = "IdleLeft";
                    break;
                case DirectionEnum.Right:
                    animation = "IdleRight";
                    break;
            }

            PlayAnimation(animation);
        }

        public void MoveBy(int x, int y)
        {
            _bodySprite.MoveBy(x, y);
            _spriteEyes.MoveBy(x, y);
            _spriteArms.MoveBy(x, y);
            _spriteHair.MoveBy(x, y);
            if (_chest != null) { _chest.Sprite.MoveBy(x, y); }
            if (Hat != null) { Hat.Sprite.MoveBy(x, y); }
        }

        public override void PlayAnimation(string anim)
        {
            _bodySprite.SetCurrentAnimation(anim);
            _spriteEyes.SetCurrentAnimation(anim);
            _spriteArms.SetCurrentAnimation(anim);
            _spriteHair.SetCurrentAnimation(anim);

            if (_chest != null) { _chest.Sprite.SetCurrentAnimation(anim); }
            if (Hat != null) { Hat.Sprite.SetCurrentAnimation(anim); }
        }

        public void SetScale(int scale = 1)
        {
            _bodySprite.SetScale(scale);
            _spriteEyes.SetScale(scale);
            _spriteArms.SetScale(scale);
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

                c.Sprite.Position = _bodySprite.Position;
                c.Sprite.CurrentAnimation = _bodySprite.CurrentAnimation;
                c.Sprite.SetDepthMod(0.004f);
            }
        }

        public void RemoveClothes(Clothes c)
        {
            if (c != null)
            {
                if (c.IsShirt()) { _chest = null; }
                else if (c.IsHat())
                {
                    _spriteHair.FrameCutoff = 0;
                    _hat = null;
                }
            }
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

            LoadContent(@"Textures\NPCs\Spirit_" + _sType);
        }

        public override void LoadContent(string textureToLoad)
        {
            _bodySprite = new AnimatedSprite(textureToLoad);
            _bodySprite.AddAnimation("Idle", 16, 18, 2, 0.6f, 0, 0);
            _bodySprite.SetCurrentAnimation("Idle");

            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
        }

        public override void Update(GameTime theGameTime)
        {
            if (_bActive)
            {
                base.Update(theGameTime);
                if (!Triggered)
                {
                    int max = TileSize * 13;
                    int dist = 0;
                    if (PlayerManager.CurrentMap == CurrentMapName && PlayerManager.PlayerInRangeGetDist(_bodySprite.Center.ToPoint(), max, ref dist))
                    {
                        float fMax = max;
                        float fDist = dist;
                        float percentage = (Math.Abs(dist - fMax)) / fMax;
                        percentage = Math.Max(percentage, MIN_VISIBILITY);
                        _fVisibility = 0.4f * percentage;
                    }
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (_bActive)
            {
                _bodySprite.Draw(spriteBatch, useLayerDepth, _fVisibility);
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
                GUIManager.SetScreen(new TextScreen(this, _sText));
            }
            return rv;
        }
    }
    public class Mob : WorldActor
    {
        #region Properties
        public override Vector2 Position
        {
            get { return new Vector2(_bodySprite.Position.X, _bodySprite.Position.Y + _bodySprite.Height - TileSize); } //MAR this is fucked up
            set {
                _bodySprite.Position = new Vector2(value.X, value.Y - _bodySprite.Height + TileSize);
                _sprAlert.Position = _bodySprite.Position - new Vector2(0, TileSize);
            }
        }

        protected int _id;
        public int ID { get => _id; }
        protected double _dIdleFor;
        protected int _iLeash = 7;

        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;
        protected List<CombatActor> _monsters;
        public List<CombatActor> Monsters { get => _monsters; }

        int _iMaxRange = TileSize * 10;
        bool _bAlert;
        bool _bLockedOn;
        bool _bLeashed;
        const double MAX_ALERT = 1;
        double _dAlertTimer;
        AnimatedSprite _sprAlert;

        FieldOfVision _FoV;
        Vector2 _vLeashPoint;
        float _fLeashRange = TileSize * 10;
        
        #endregion

        public Mob(int id, string[] stringData)
        {
            _actorType = ActorEnum.Mob;
            _monsters = new List<CombatActor>();
            ImportBasics(stringData, id);
            _textureName = @"Textures\Monsters\Goblin Scout";
            LoadContent();
        }

        public void LoadContent()
        {
            _bodySprite = new AnimatedSprite(_textureName);
            _bodySprite.AddAnimation("IdleDown", TileSize, TileSize, 1, 0.3f, 0, 0);
            _bodySprite.AddAnimation("WalkDown", TileSize, TileSize, 2, 0.3f, 116, 0);
            _bodySprite.AddAnimation("IdleUp", TileSize, TileSize, 1, 0.3f, 48, 0);
            _bodySprite.AddAnimation("WalkUp", TileSize, TileSize, 2, 0.3f, 64, 0);
            _bodySprite.AddAnimation("IdleLeft", TileSize, TileSize, 1, 0.3f, 96, 0);
            _bodySprite.AddAnimation("WalkLeft", TileSize, TileSize, 2, 0.3f, 112, 0);
            _bodySprite.AddAnimation("IdleRight", TileSize, TileSize, 1, 0.3f, 144, 0);
            _bodySprite.AddAnimation("WalkRight", TileSize, TileSize, 2, 0.3f, 160, 0);
            _bodySprite.SetCurrentAnimation("WalkDown");
            Facing = DirectionEnum.Down;

            _width = _bodySprite.Width;
            _height = _bodySprite.Height;

            _sprAlert = new AnimatedSprite(@"Textures\Dialog", true);
            _sprAlert.AddAnimation("Gah", 64, 64, 16, 16, 3, 0.2f);
            _sprAlert.SetCurrentAnimation("Gah");
            _sprAlert.Position = (Position - new Vector2(0, TileSize));
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            for (int i = 0; i < stringData.Length; i++)
            {
                int mID = int.Parse(stringData[i]);
                if (mID > 0)
                {
                    _monsters.Add(CharacterManager.GetMonsterByIndex(mID));
                }
            }

            foreach (CombatActor m in _monsters)
            {
                List<CombatActor> match = _monsters.FindAll(x => ((Monster)x).ID == ((Monster)m).ID);
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

        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            base.LoadContent(_textureName, textureWidth, textureHeight, numFrames, frameSpeed);
        }

        public void NewFoV()
        {
            _FoV = new FieldOfVision(this, _iMaxRange);
        }

        public override void Update(GameTime theGameTime)
        {
            if (_bAlert && !_bLockedOn)
            {
                _dAlertTimer += theGameTime.ElapsedGameTime.TotalSeconds;
                _sprAlert.Update(theGameTime);
            }
            UpdateMovement(theGameTime);
            base.Update(theGameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, bool userLayerDepth = false)
        {
            base.Draw(spriteBatch, userLayerDepth);
            if(_bAlert) { _sprAlert.Draw(spriteBatch, userLayerDepth); }
        }

        private void UpdateMovement(GameTime theGameTime)
        {
            Vector2 direction = Vector2.Zero;

            if (!_bLeashed && _FoV.Contains(PlayerManager.World))
            {
                if (!_bAlert)
                {
                    _bAlert = true;
                    _dAlertTimer = 0;
                }

                if (_dAlertTimer >= MAX_ALERT && _vLeashPoint == Vector2.Zero)
                {
                    _bLockedOn = true;
                    _vLeashPoint = Position;
                }
            }
            else
            {
                if (_bAlert)
                {
                    _bAlert = false;
                    _dAlertTimer = 0;
                }
                IdleMovement(theGameTime);
            }

            if (_bLockedOn)
            {
                if (Math.Abs(_vLeashPoint.X - Position.X) <= _fLeashRange && Math.Abs(_vLeashPoint.Y - Position.Y) <= _fLeashRange)
                {
                    _moveTo = Vector2.Zero;
                    Vector2 targetPos = PlayerManager.World.Position;

                    float deltaX = Math.Abs(targetPos.X - this.Position.X);
                    float deltaY = Math.Abs(targetPos.Y - this.Position.Y);

                    Util.GetMoveSpeed(Position, targetPos, Speed, ref direction);
                    CheckMapForCollisionsAndMove(direction);
                    NewFoV();
                }
                else
                {
                    _bLeashed = true;
                    _bLockedOn = false;
                    _moveTo = _vLeashPoint;
                }
            }

            if (CollisionBox.Intersects(PlayerManager.World.CollisionBox))
            {
                CombatManager.NewBattle(this);
            }
        }

        private void IdleMovement(GameTime theGameTime)
        {
            if (_moveTo == Vector2.Zero && _dIdleFor <= 0)
            {
                int howFar = 2;
                RHRandom r = new RHRandom();
                int decision = r.Next(1, 5);
                if (decision == 1) { _moveTo = new Vector2(Position.X - r.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 2) { _moveTo = new Vector2(Position.X + r.Next(1, howFar) * TileSize, Position.Y); }
                else if (decision == 3) { _moveTo = new Vector2(Position.X, Position.Y - r.Next(1, howFar) * TileSize); }
                else if (decision == 4) { _moveTo = new Vector2(Position.X, Position.Y + r.Next(1, howFar) * TileSize); }
                else
                {
                    DetermineFacing(Vector2.Zero);
                    _dIdleFor = 10;
                }
            }
            else if (_moveTo != Vector2.Zero)
            {
                Vector2 direction = Vector2.Zero;
                float deltaX = Math.Abs(_moveTo.X - this.Position.X);
                float deltaY = Math.Abs(_moveTo.Y - this.Position.Y);

                Util.GetMoveSpeed(Position, _moveTo, Speed, ref direction);
                if (CheckMapForCollisionsAndMove(direction))
                {
                    NewFoV();
                }
                else
                {
                    _moveTo = Vector2.Zero;
                }

                if (Position.X == _moveTo.X && Position.Y == _moveTo.Y)
                {
                    if (_bLeashed && _moveTo == _vLeashPoint)
                    {
                        _vLeashPoint = Vector2.Zero;
                        _bLeashed = false;
                    }

                    _moveTo = Vector2.Zero;
                    DetermineFacing(_moveTo);
                }
            }
            else
            {
                _dIdleFor -= theGameTime.ElapsedGameTime.TotalSeconds;
            }
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
                _iMaxRange = maxRange;
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
        const int MAX_STAT = 99;
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
        public int MaxHP { get => StatVit * 3; }

        protected int _currentMP;
        public int CurrentMP
        {
            get { return _currentMP; }
            set { _currentMP = value; }
        }
        public int MaxMP { get => StatMag * 3; }

        public int CurrentCharge;
        public CombatManager.CombatTile Tile;
        public GUICmbtTile Location => Tile.GUITile;

        public virtual int Attack => (int)(StatStr * 0.5);

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

        protected List<CombatAction> _liSpells;
        public virtual List<CombatAction> SpellList { get => _liSpells; }

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
            _liSpells = new List<CombatAction>();
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

        //ONLY USE THIS FOR CALCULATING TURNS
        public CombatActor(CombatActor c)
        {
            _sName = c.Name;
            CurrentCharge = c.CurrentCharge;
            _diConditions = c._diConditions;
            _statStr = c.StatStr;
            _statDef = c.StatDef;
            _statVit = c.StatVit;
            _statMag = c.StatMag;
            _statRes = c.StatRes;
            _statSpd = c.StatSpd;

            _vStartPos = new Vector2(0, 0);
        }

        public void LoadContent(string texture)
        {
            _bodySprite = new AnimatedSprite(texture);
            int xCrawl = 0;
            int frameWidth = 24;
            int frameHeight = 32;
            _bodySprite.AddAnimation("Walk", frameWidth, frameHeight, 2, 0.5f, (xCrawl * frameWidth), 32);
            xCrawl += 2;
            _bodySprite.AddAnimation("Cast", frameWidth, frameHeight, 2, 0.2f, (xCrawl * frameWidth), 32);
            xCrawl += 2;
            _bodySprite.AddAnimation("Hurt", frameWidth, frameHeight, 1, 0.5f, (xCrawl * frameWidth), 32, "Walk");
            xCrawl += 1;
            _bodySprite.AddAnimation("Attack", frameWidth, frameHeight, 1, 0.3f, (xCrawl * frameWidth), 32);
            xCrawl += 1;
            _bodySprite.AddAnimation("Critical", frameWidth, frameHeight, 2, 0.5f, (xCrawl * frameWidth), 32);
            xCrawl += 2;
            _bodySprite.AddAnimation("KO", frameWidth, frameHeight, 1, 0.5f, (xCrawl * frameWidth), 32);

            _bodySprite.SetCurrentAnimation("Walk");
            _bodySprite.SetScale(CombatManager.CombatScale);
            _width = _bodySprite.Width;
            _height = _bodySprite.Height;
        }

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
            if (CurrentHP > 0)
            {
                if ((float)CurrentHP / (float)MaxHP <= 0.25 && (IsCurrentAnimation("Walk") || IsCurrentAnimation("KO")))
                {
                    PlayAnimation("Critical");
                }
                else if ((float)CurrentHP / (float)MaxHP > 0.25 && IsCurrentAnimation("Critical"))
                {
                    PlayAnimation("Walk");
                }
            }
            else
            {
                if (IsCurrentAnimation("Walk"))
                {
                    PlayAnimation("KO");
                }
            }

            if (_linkedSummon != null)
            {
                _linkedSummon.Update(theGameTime);
            }
        }

        public int ProcessAttack(CombatActor attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            int base_attack = attacker.Attack;

            double power = Math.Pow(((double)base_attack - (double)StatDef), 2) + attacker.StatStr;
            double dMult = Math.Min(2, Math.Max(0.01, power));
            int dmg = (int)Math.Max(1, base_attack * dMult);

            dmg += ApplyResistances(dmg, element);
            return DecreaseHealth(potency);
        }
        public int ProcessSpell(CombatActor attacker, int potency, ElementEnum element = ElementEnum.None)
        {
            int base_damage = (attacker.StatMag - StatRes / 2) * potency;
            int bonus = 0;

            base_damage += ApplyResistances(base_damage, element);
            return DecreaseHealth(base_damage);
        }
        public int ApplyResistances(int dmg, ElementEnum element = ElementEnum.None)
        {
            int modifiedDmg = 0;
            if (element != ElementEnum.None)
            {
                if (MapManager.CurrentMap.IsOutside && GameCalendar.IsRaining())
                {
                    if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (int)(dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Fire)) { modifiedDmg += (int)(dmg * 0.8) - dmg; }
                }
                else if (MapManager.CurrentMap.IsOutside && GameCalendar.IsSnowing())
                {
                    if (element.Equals(ElementEnum.Ice)) { modifiedDmg += (int)(dmg * 1.2) - dmg; }
                    else if (element.Equals(ElementEnum.Lightning)) { modifiedDmg += (int)(dmg * 0.8) - dmg; }
                }

                if (_linkedSummon != null && _diElementalAlignment[element].Equals(ElementAlignment.Neutral))
                {
                    if (_linkedSummon.Element.Equals(element))
                    {
                        modifiedDmg += (int)(dmg * 0.8) - dmg;
                    }
                }

                if (_diElementalAlignment[element].Equals(ElementAlignment.Resists))
                {
                    modifiedDmg += (int)(dmg * 0.8) - dmg;
                }
                else if (_diElementalAlignment[element].Equals(ElementAlignment.Vulnerable))
                {
                    modifiedDmg += (int)(dmg * 1.2) - dmg;
                }
            }

            return modifiedDmg;
        }

        public virtual GUISprite GetSprite()
        {
            return Tile.GUITile.CharacterSprite;
        }

        public virtual int DecreaseHealth(int value)
        {
            _currentHP -= (_currentHP - value >= 0) ? value : _currentHP;
            PlayAnimation("Hurt");
            if (_currentHP == 0)
            {
                _diConditions[ConditionEnum.KO] = true;
                CombatManager.Kill(this);
                UnlinkSummon();
                Tile.GUITile.LinkSummon(null);
            }

            return value;
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

        public Equipment Weapon;
        public Equipment TempWeapon;
        public Equipment Armor;
        public Equipment TempArmor;

        public override int Attack => GetGearAtk();
        public override int StatStr { get => 10 + (_classLevel * _class.StatStr) + _buffStr + GetGearStr(); }
        public override int StatDef { get => 10 + (_classLevel * _class.StatDef) + _buffDef + GetGearDef() + (Protected ? 10 : 0); }
        public override int StatVit { get => 10 + (_classLevel * _class.StatVit) + GetGearVit(); }
        public override int StatMag { get => 10 + (_classLevel * _class.StatMag) + _buffMag + GetGearMag(); }
        public override int StatRes { get => 10 + (_classLevel * _class.StatRes) + _buffRes + GetGearRes(); }
        public override int StatSpd { get => 10 + (_classLevel * _class.StatSpd) + +_buffSpd + GetGearSpd(); }

        public override List<MenuAction> AbilityList { get => _class.AbilityList; }
        public override List<CombatAction> SpellList { get => _class._spellList; }

        public int GetGearAtk()
        {
            int rv = 0;

            if (TempWeapon != null) { rv += TempWeapon.Attack; }
            else if (Weapon != null) { rv += Weapon.Attack; }
            else if (Weapon == null) { rv += base.Attack; }
            if (TempArmor != null) { rv += TempArmor.Attack; }
            else if (Armor != null) { rv += Armor.Attack; }

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
        }

        public CombatAdventurer() : base()
        {
            _actorType = ActorEnum.CombatAdventurer;
            _classLevel = 1;
        }

        public void SetClass(CharacterClass x)
        {
            _class = x;
            _currentHP = MaxHP;
            _currentMP = MaxMP;
        }

        public void AddXP(int x)
        {
            _xp += x;
            if (_xp >= LevelRange[_classLevel])
            {
                _classLevel++;
            }
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
        int _iLvl;
        int _xp;
        public int XP { get => _xp; }
        protected string _textureName;
        protected Vector2 _moveTo = Vector2.Zero;

        #endregion

        public Monster(int id, string[] stringData)
        {
            _actorType = ActorEnum.Monster;
            ImportBasics(stringData, id);
            LoadContent(_textureName, 100, 100, 2, 0.2f);
        }

        protected void ImportBasics(string[] stringData, int id)
        {
            _id = id;
            _sName = GameContentManager.GetGameText("Monster " + _id);

            foreach (string s in stringData)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Texture"))
                {
                    _textureName = @"Textures\" + tagType[1];
                }
                else if (tagType[0].Equals("Lvl"))
                {
                    _iLvl = int.Parse(tagType[1]);
                    _xp = _iLvl * 10;
                    _statStr = 2 * _iLvl + 10;
                    _statDef = 2 * _iLvl + 10;
                    _statVit = (3 * _iLvl) + 80;
                    _statMag = 2 * _iLvl + 10;
                    _statRes = 2 * _iLvl + 10;
                    _statSpd = 10;
                }
                else if (tagType[0].Equals("Trait"))
                {
                    HandleTrait(GameContentManager.GetMonsterTraitData(tagType[1]));
                }
                else if (tagType[0].Equals("Resist"))
                {
                    string[] elemSplit = tagType[1].Split('-');
                    foreach (string elem in elemSplit)
                    {
                        _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Resists;
                    }
                }
                else if (tagType[0].Equals("Vuln"))
                {
                    string[] elemSplit = tagType[1].Split('-');
                    foreach (string elem in elemSplit)
                    {
                        _diElementalAlignment[Util.ParseEnum<ElementEnum>(elem)] = ElementAlignment.Vulnerable;
                    }
                }
            }

            _currentHP = MaxHP;
            _currentMP = MaxMP;
        }

        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            base.LoadContent(_textureName, textureWidth, textureHeight, numFrames, frameSpeed);
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
            _bodySprite = new AnimatedSprite(@"Textures\Eye");
            _bodySprite.AddAnimation("Walk", 0, 0, 16, 16, 2, 0.9f);
            _bodySprite.AddAnimation("Attack", 32, 0, 16, 16, 4, 0.1f);
            _bodySprite.AddAnimation("Cast", 32, 0, 16, 16, 4, 0.1f);
            _bodySprite.SetCurrentAnimation("Idle");
            _bodySprite.SetScale(5);
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

        public override int DecreaseHealth(int value)
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
