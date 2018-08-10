using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers.GUIObjects;

using static RiverHollow.Game_Managers.GameManager;
using System;
using RiverHollow.WorldObjects;
using RiverHollow.GUIObjects;
using System.Collections.Generic;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Actors.CombatStuff;
using static RiverHollow.Game_Managers.ObjectManager;
using RiverHollow.Tile_Engine;

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

        public Actor() { }

        public virtual void LoadContent(string textureToLoad, int frameWidth, int frameHeight, int numFrames, float frameSpeed,int startX = 0, int startY = 0)
        {
            _sTexture = textureToLoad;
            _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(_sTexture));
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
        public bool CanTalk() { return IsWorldCharacter() || IsNPC() || IsWorldAdventurer() || IsSpirit(); }
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

        protected GUIImage _headshot;

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
            sprite = new AnimatedSprite(GameContentManager.GetTexture(texture), pingpong);
            sprite.AddAnimation("WalkDown", TileSize, TileSize * 2, 3, 0.2f, startX, startY);
            sprite.AddAnimation("IdleDown", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize, startY);
            _headshot = new GUIImage(Vector2.Zero, new Rectangle(startX + TileSize, startY, TileSize, TileSize), TileSize, TileSize, texture);
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
                _bodySprite.MoveBy((int)direction.X, (int)direction.Y);
                rv = true;
            }

            string animation = string.Empty;
            DetermineFacing(direction);

            return rv;
        }

        public void SetMoveObj(Vector2 vec) { _vMoveTo = vec; }

        public virtual GUIHeadShot GetHeadShot()
        {
            return new GUIHeadShot(_headshot);
        }
    }
    public class TalkingActor : WorldActor
    {
        protected const int PortraitWidth = 160;
        protected const int PortraitHeight = 192;

        protected Texture2D _portrait;
        public Texture2D Portrait { get => _portrait; }

        protected Rectangle _portraitRect;
        public Rectangle PortraitRectangle { get => _portraitRect; }

        protected Dictionary<string, string> _dialogueDictionary;

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

            _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\NPC1"));

            int startX = 0;
            int startY = 0;
            _bodySprite.AddAnimation("IdleDown", startX + TileSize, startY, TileSize, TileSize * 2, 1, 0.2f);
            _bodySprite.AddAnimation("WalkDown", startX, startY, TileSize, TileSize * 2, 3, 0.2f);
            _headshot = new GUIImage(Vector2.Zero, new Rectangle(startX + TileSize, startY, TileSize, TileSize), TileSize, TileSize, _sTexture);

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

    public class EligibleNPC : Villager
    {
        public bool Married;
        bool _bCanJoinParty = true;
        public bool CanJoinParty => _bCanJoinParty;
        private CombatAdventurer _combat;
        public CombatAdventurer Combat => _combat;

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

        private CombatAdventurer _c;
        public CombatAdventurer Combat => _c;
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
            _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(_sTexture), true);
            _bodySprite.AddAnimation("Idle", TileSize, 0, TileSize, TileSize * 2, 1, 0.3f);
            _bodySprite.AddAnimation("WalkDown", 0, 0, TileSize, TileSize * 2, 3, 0.3f);
            _bodySprite.SetCurrentAnimation("Idle");

            _headshot = new GUIImage(Vector2.Zero, new Rectangle(TileSize, 0, TileSize, TileSize), TileSize, TileSize, _sTexture);
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
            _c = new CombatAdventurer(this);
            _c.SetClass(CharacterManager.GetClassByIndex(_iAdventurerID));
            _c.LoadContent(@"Textures\" + _c.CharacterClass.Name);
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
                    _c.AddXP(_currentlyMaking.XP);
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
                PlayerManager.AddToParty(_c);
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
            _c.CurrentHP = _c.MaxHP;
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
            _c.SetName(name);
        }

        public new WorkerData SaveData()
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

    public class PlayerCharacter : WorldActor
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
        public Clothes Chest => _chest;
        Clothes Back;
        Clothes Hands;
        Clothes Legs;
        Clothes Feet;

        public PlayerCharacter() : base()
        {
            _width = TileSize;
            _height = TileSize;

            _cHairColor = Color.Red;

            Speed = 2;
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
            if (c.IsShirt()) { _chest = null; }
            else if (c.IsHat())
            {
                _spriteHair.FrameCutoff = 0;
                _hat = null;
            }
        }

        public override GUIHeadShot GetHeadShot()
        {
            return new GUIHeadShot(new GUICoin());
        }
    }

    public class GUIHeadShot : GUIObject
    {
        GUIImage _hat;
        GUIImage _hair;
        GUIImage _eyes;
        GUIImage _body;
        GUIImage _shirt;

        List<GUIImage> _liImages;

        public GUIHeadShot(GUIImage body)
        {
            _body = body;
            _body.SetScale(Scale);

            _liImages = new List<GUIImage>();
            _liImages.Add(_body);

            Width = _body.Height;
            Height = _body.Height;
        }

        public GUIHeadShot(GUIImage body, GUIImage eyes, GUIImage hair, GUIImage hat, GUIImage shirt)
        {
            _body = body;
            Width = _body.Height;
            Height = _body.Height;

            _eyes = eyes;
            _hair = hair;
            _hat = hat;
            _shirt = shirt;

            _liImages = new List<GUIImage>();
            _liImages.Add(_body);
            if (_eyes != null) { _liImages.Add(_eyes); }
            if (_hair != null) { _liImages.Add(_hair); }
            if (_hat != null) { _liImages.Add(_hat); }
            if (_shirt != null) { _liImages.Add(_shirt); }

            foreach (GUIImage g in _liImages)
            {
                g.SetScale(Scale);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            foreach (GUIImage g in _liImages)
            {
                g.Draw(spriteBatch);
            }
        }

        public override void Position(Vector2 value)
        {
            base.Position(value);
            foreach (GUIImage g in _liImages)
            {
                g.Position(value);
            }
        }
    }
    #endregion
}
