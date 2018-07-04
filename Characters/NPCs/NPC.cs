using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.Misc;
using RiverHollow.WorldObjects;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Game_Managers.GUIObjects;

using static RiverHollow.Game_Managers.GameManager;
using System.IO;
using RiverHollow.Characters.CombatStuff;

namespace RiverHollow.Characters
{
    public class NPC : WorldCharacter
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

        private const int PortraitWidth = 160;
        private const int PortraitHeight = 192;
        protected Texture2D _portrait;
        public Texture2D Portrait { get => _portrait; }
        protected Rectangle _portraitRect;
        public Rectangle PortraitRectangle { get => _portraitRect; }

        protected Dictionary<string, string> _dialogueDictionary;

        protected double _dEtherealCD;
        protected bool _bIgnoreCollisions;

        public NPC() { }
        public NPC(NPC n)
        {
            _characterType = CharacterEnum.NPC;
            _index = n.ID;
            _sName = n.Name;
            _dialogueDictionary = n._dialogueDictionary;
            _portrait = n.Portrait;
            _portraitRect = n._portraitRect;

            LoadContent();
        }

        public NPC(int index, string[] data)
        {
            _collection = new Dictionary<int, bool>();
            _completeSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();
            _scheduleIndex = 0;

            LoadContent();
            ImportBasics(index, data);

            MapManager.Maps[CurrentMapName].AddCharacter(this);
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

            if(_dEtherealCD != 0)
            {
                _dEtherealCD -= theGameTime.ElapsedGameTime.TotalSeconds;
                if(_dEtherealCD <= 0)
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
                        DetermineFacing(Vector2.Zero);
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
                if(_dEtherealCD == 0) { _dEtherealCD = 5; }
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

        public virtual void Talk()
        {
            string text = string.Empty;
            if (!Introduced)
            {
                text = _dialogueDictionary["Introduction"];
                Introduced = true;
            }
            else
            {
                if (!CheckQuestLogs(ref text))
                {
                    text = GetSelectionText();
                }
            }
            text = Util.ProcessText(text, _sName);
            GUIManager.SetScreen(new TextScreen(this, text));
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

        public virtual void Talk(string dialogTag)
        {
            string text = string.Empty;
            if (_dialogueDictionary.ContainsKey(dialogTag))
            {
                text = _dialogueDictionary[dialogTag];
            }
            text = Util.ProcessText(text, _sName);
            GUIManager.SetScreen(new TextScreen(this, text));
        }

        public void LoadContent()
        {
            if (_index != 8)
            {
                _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\NPC1"));
            }
            else
            {
                _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\NPC8"));
            }
            _bodySprite.AddAnimation("IdleDown", 0, 0, TileSize, TileSize * 2, 1, 0.2f);
            _bodySprite.AddAnimation("WalkDown", 0, 0, TileSize, TileSize * 2, 4, 0.2f);
            _bodySprite.AddAnimation("WalkUp", 0, 32, TileSize, TileSize * 2, 4, 0.2f);
            _bodySprite.AddAnimation("WalkRight", 0, 64, TileSize, TileSize * 2, 4, 0.2f);
            _bodySprite.AddAnimation("WalkLeft", 0, 96, TileSize, TileSize * 2, 4, 0.2f);
            _bodySprite.SetCurrentAnimation("IdleDown");
            _bodySprite.IsAnimating = true;
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
            List<string> liCommands = new List<string>();
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
                    liCommands.Add(s);
                }
            }

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

        public virtual string GetText()
        {
            RHRandom r = new RHRandom();
            string text = _dialogueDictionary[r.Next(1, 2).ToString()];
            return Util.ProcessText(text, _sName);
        }

        public virtual string GetDialogEntry(string entry)
        {
            if (entry.Equals("Talk"))
            {
                return GetText();
            }
            else if (entry.Equals("GiveGift"))
            {
                GUIManager.SetScreen(new InventoryScreen(this));
                return "";
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
            else if (entry.Equals("Nothing"))
            {
                return string.Empty;
            }

            return _dialogueDictionary.ContainsKey(entry) ? Util.ProcessText(_dialogueDictionary[entry], _sName) : string.Empty;
        }

        public virtual void Gift(Item item)
        {
            if (item != null)
            {
                string text = string.Empty;
                item.Remove(1);
                if (item.IsMap() && NPCType == NPC.NPCTypeEnum.Ranger)
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
            for( int i=0; i < FriendRange.Count; i++)
            {
                if(FriendshipPoints >= FriendRange[i])
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

    public class EligibleNPC : NPC
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
                    _combat = new CombatAdventurer(_sName);
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
                if (item.IsMap() && NPCType == NPC.NPCTypeEnum.Ranger)
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
}
