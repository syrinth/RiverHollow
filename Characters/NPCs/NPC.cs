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
namespace RiverHollow.Characters
{
    public class NPC : WorldCharacter
    {
        #region Pathfinding
        const int slowCost = 100;
        Dictionary<string, List<RHTile>> _dictMapPathing;
        Dictionary<RHTile, RHTile> cameFrom = new Dictionary<RHTile, RHTile>();
        Dictionary<RHTile, double> costSoFar = new Dictionary<RHTile, double>();
        #endregion

        protected int _index;
        public int ID { get => _index; }
        protected string _homeMap;
        public enum NPCTypeEnum { Villager, Shopkeeper, Ranger, Worker }
        protected NPCTypeEnum _npcType;
        public NPCTypeEnum NPCType { get => _npcType; }
        public int Friendship = 0;

        protected Dictionary<int, bool> _collection;
        public Dictionary<int, bool> Collection { get => _collection; }
        public bool Introduced;
        public bool Married;
        public bool GiftGiven;

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
            _index = index;
            _collection = new Dictionary<int, bool>();
            _completeSchedule = new Dictionary<string, List<KeyValuePair<string, string>>>();
            _dialogueDictionary = GameContentManager.LoadDialogue(@"Data\Dialogue\NPC" + _index);
            _portrait = GameContentManager.GetTexture(@"Textures\portraits");
            _scheduleIndex = 0;

            LoadContent();
            ImportBasics(data);

            MapManager.Maps[CurrentMapName].AddCharacter(this);
        }

        protected int ImportBasics(string[] stringData)
        {
            int i = 0;
            _npcType = Util.ParseEnum<NPCTypeEnum>(stringData[i++]);
            _sName = stringData[i++];
            _portraitRect = new Rectangle(0, int.Parse(stringData[i++]) * 192, PortraitWidth, PortraitHeight);
            CurrentMapName = stringData[i++];
            _homeMap = CurrentMapName;
            Position = Util.SnapToGrid(MapManager.Maps[CurrentMapName].GetCharacterSpawn("NPC" + _index));

            string[] vectorSplit = stringData[i++].Split(' ');
            foreach (string s in vectorSplit) {
                _collection.Add(int.Parse(s), false);
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

            if (!Married)   //Just for now
            {
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
        }

        private void HandleMove(Vector2 target)
        {
            Vector2 direction = Vector2.Zero;
            float deltaX = Math.Abs(target.X - this.Position.X);
            float deltaY = Math.Abs(target.Y - this.Position.Y);

            Util.GetMoveSpeed(Position, target, Speed, ref direction);
            CheckMapForCollisionsAndMove(direction);
        }
        
        public void RollOver()
        {
            GiftGiven = false;
            MapManager.Maps[CurrentMapName].RemoveCharacter(this);
            RHMap map = MapManager.Maps[Married ? "mapManor" : _homeMap];
            string Spawn = Married ? "Spouse" : "NPC" + _index;

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

                        if (MapManager.Maps[CurrentMapName].DictionaryCharacterLayer.ContainsKey(kvp.Value))
                        {
                            timePath = FindPathToLocation(ref start, MapManager.Maps[CurrentMapName].DictionaryCharacterLayer[kvp.Value], CurrentMapName);
                        }
                        else
                        {
                            timePath = FindPathToOtherMap(kvp.Value, ref mapName, ref start);
                        }
                        lTimetoTilePath.Add(new KeyValuePair<string, List<RHTile>>(kvp.Key, timePath));
                        ClearPathingTracks();
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
            if (!Introduced) {
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
                if(q.ReadyForHandIn && q.QuestGiver == this)
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
            _bodySprite.AddAnimation("IdleDown", 0, 0, TileSize, TileSize*2, 1, 0.2f);
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
            for(int i = 0; i < options.Length; i++) { 
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
                        removeIt = Friendship < val;
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
                    if (GiftGiven && s.Contains("GiveGift"))
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
                //DrawIt = false;
                //Busy = true;
                //PlayerManager.AddToParty(_c);
                //rv = "Of course!";
                return "I'd love to, but I can't";
            }
            else if (entry.Equals("Nothing"))
            {
                return string.Empty;
            }
            else
            {
                return _dialogueDictionary.ContainsKey(entry) ? Util.ProcessText(_dialogueDictionary[entry], _sName) : string.Empty;
            }
        }

        public void Gift(Item item)
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
                    if (Friendship > 200)
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
                    GiftGiven = true;
                    if (_collection.ContainsKey(item.ItemID))
                    {
                        Friendship += _collection[item.ItemID] ? 50 : 20;
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
                        Friendship += 10;
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    GUIManager.SetScreen(new TextScreen(this, text));
                }
            }
        }

        public NPCData SaveData()
        {
            NPCData npcData = new NPCData()
            {
                npcID = ID,
                introduced = Introduced,
                married = Married,
                friendship = Friendship,
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
            Friendship = data.friendship;
            Married = data.married;
            int index = 1;
            foreach (CollectionData c in data.collection)
            {
                if (c.given)
                {
                    Collection[c.itemID] = c.given;
                    MapManager.Maps["HouseNPC" + data.npcID].AddCollectionItem(c.itemID, data.npcID, index++);
                }
            }

            if (Married)
            {
                MapManager.Maps[_homeMap].RemoveCharacter(this);
                MapManager.Maps["mapManor"].AddCharacter(this);
                Position = MapManager.Maps["mapManor"].GetCharacterSpawn("Spouse");
            }
        }

        #region Pathfinding
        public class PriorityQueue<T>
        {
            // I'm using an unsorted array for this example, but ideally this
            // would be a binary heap. There's an open issue for adding a binary
            // heap to the standard C# library: https://github.com/dotnet/corefx/issues/574
            //
            // Until then, find a binary heap class:
            // * https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
            // * http://visualstudiomagazine.com/articles/2012/11/01/priority-queues-with-c.aspx
            // * http://xfleury.github.io/graphsearch.html
            // * http://stackoverflow.com/questions/102398/priority-queue-in-net

            private List<Tuple<T, double>> elements = new List<Tuple<T, double>>();

            public int Count
            {
                get { return elements.Count; }
            }

            public void Enqueue(T item, double priority)
            {
                elements.Add(Tuple.Create(item, priority));
            }

            public T Dequeue()
            {
                int bestIndex = 0;

                for (int i = 0; i < elements.Count; i++)
                {
                    if (elements[i].Item2 < elements[bestIndex].Item2)
                    {
                        bestIndex = i;
                    }
                }

                T bestItem = elements[bestIndex].Item1;
                elements.RemoveAt(bestIndex);
                return bestItem;
            }
        }

        private List<RHTile> FindPathToOtherMap(string findKey, ref string mapName, ref Vector2 newStart)
        {
            List<RHTile> _completeTilePath = new List<RHTile>();
            _dictMapPathing = new Dictionary<string, List<RHTile>>();
            Vector2 start = newStart;
            Dictionary<string, string> mapCameFrom = new Dictionary<string, string>();
            Dictionary<string, double> mapCostSoFar = new Dictionary<string, double>();

            var frontier = new PriorityQueue<string>();
            frontier.Enqueue(mapName, 0);

            mapCameFrom[mapName] = mapName;
            mapCostSoFar[mapName] = 0;
            while (frontier.Count > 0)
            {
                var testMap = frontier.Dequeue();
                string mapSplit = testMap.Split(':')[0];
                string fromMap = mapCameFrom[testMap];
                if (fromMap != testMap)
                {
                    start = MapManager.Maps[mapSplit].DictionaryEntrance[fromMap].Location.ToVector2();
                }
                if (MapManager.Maps[mapSplit].DictionaryCharacterLayer.ContainsKey(findKey))
                {
                    mapName = testMap;
                    newStart = MapManager.Maps[mapSplit].DictionaryCharacterLayer[findKey];
                    List<RHTile> pathToExit = FindPathToLocation(ref start, MapManager.Maps[mapSplit].DictionaryCharacterLayer[findKey], testMap);
                    fromMap = mapCameFrom[testMap];
                    List<List<RHTile>> _totalPath = new List<List<RHTile>>();
                    _totalPath.Add(pathToExit);
                    while (fromMap != testMap)
                    {
                        _totalPath.Add(_dictMapPathing[fromMap + ":" + testMap]);
                        testMap = fromMap;
                        fromMap = mapCameFrom[testMap];
                    }
                    _totalPath.Reverse();

                    foreach (List<RHTile> l in _totalPath)
                    {
                        _completeTilePath.AddRange(l);
                    }

                    break;
                }

                foreach (KeyValuePair<Rectangle, string> exit in MapManager.Maps[mapSplit].DictionaryExit)
                {
                    List<RHTile> pathToExit = FindPathToLocation(ref start, exit.Key.Location.ToVector2(), testMap);
                    if (pathToExit != null)
                    {
                        double newCost = mapCostSoFar[testMap] + pathToExit.Count;
                        if (!mapCostSoFar.ContainsKey(exit.Value))
                        {
                            mapCostSoFar[exit.Value] = newCost + pathToExit.Count;
                            frontier.Enqueue(exit.Value, newCost);
                            string[] split = null;
                            if (exit.Value.Contains(":")) {
                                split = exit.Value.Split(':');
                            }
                            mapCameFrom[exit.Value] = (split == null) ? mapSplit : mapSplit + ":" + split[1];
                            _dictMapPathing[testMap + ":" + exit.Value] = pathToExit; // This needd another key for the appropriate exit
                        }
                    }

                    ClearPathingTracks();
                }
            }

            return _completeTilePath;
        }
        private List<RHTile> FindPathToLocation(ref Vector2 start, Vector2 target, string mapName)
        {
            List<RHTile> returnList = null;
            RHMap map = MapManager.Maps[mapName.Split(':')[0]];
            RHTile startTile = map.RetrieveTile(start.ToPoint());
            RHTile goalNode = map.RetrieveTile(target.ToPoint());
            var frontier = new PriorityQueue<RHTile>();
            frontier.Enqueue(startTile, 0);

            cameFrom[startTile] = startTile;
            costSoFar[startTile] = 0;

            while(frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                if (current.Equals(goalNode))
                {
                    returnList = BackTrack(current);
                    start = current.Position;
                    break;
                }

                foreach (var next in current.GetWalkableNeighbours())
                {
                    double newCost = costSoFar[current] + GetMovementCost(next);

                    if(!costSoFar.ContainsKey(next) || newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        double priority = newCost + Heuristic(next, goalNode);
                        frontier.Enqueue(next, priority);
                        cameFrom[next] = current;
                    }
                }
            }
            return returnList;
        }
        private List<RHTile> BackTrack(RHTile current)
        {
            List<RHTile> list = new List<RHTile>();
            while (current != cameFrom[current])
            {
                list.Add(current);
                current = cameFrom[current];
            }

            list.Reverse();

            return list;
        }
        private double Heuristic(RHTile a, RHTile b)
        {
            return (Math.Abs(a.Position.X - b.Position.X) + Math.Abs(a.Position.Y - b.Position.Y)) * (a.IsRoad ? 1 : slowCost);
        }
       
        //Returns how much it costs to enter the next square
        private int GetMovementCost(RHTile target)
        {
            return target.IsRoad ? 1 : slowCost;
        }
        private void ClearPathingTracks()
        {
            cameFrom.Clear();
            costSoFar.Clear();
        }
    }
    #endregion
}
