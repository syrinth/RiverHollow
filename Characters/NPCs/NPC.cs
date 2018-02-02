using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.Misc;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;

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
        public enum NPCType { Villager, Shopkeeper, Ranger, Worker }
        protected NPCType _npcType;
        public NPCType Type { get => _npcType; }
        public int Friendship;

        protected Dictionary<int, bool> _collection;
        public Dictionary<int, bool> Collection { get => _collection; }
        public bool Introduced;

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

        public NPC()
        {
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
            _npcType = (NPCType)Enum.Parse(typeof(NPCType), stringData[i++]);
            _name = stringData[i++];
            _portraitRect = new Rectangle(0, int.Parse(stringData[i++]) * 192, PortraitWidth, PortraitHeight);
            CurrentMapName = stringData[i++];
            Position = Utilities.Normalize(MapManager.Maps[CurrentMapName].GetCharacterSpawn("NPC" + _index));

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

            if (_todaysPathing != null)
            {
                string currTime = GameCalendar.GetTime();
                //_scheduleIndex keeps track of which pathing route we're currently following.
                //Running late code to be implemented later
                if (_scheduleIndex < _todaysPathing.Count && ((_todaysPathing[_scheduleIndex].Key == currTime)))// || RunningLate(movementList[_scheduleIndex].Key, currTime)))
                {
                    _currentPath = _todaysPathing[_scheduleIndex++].Value;
                }
            }

            if (_currentPath.Count > 0)
            {
                Vector2 targetPos = _currentPath[0].Position;
                if (Position == targetPos)
                {
                    _currentPath.RemoveAt(0);
                }
                else
                {
                    Vector2 direction = Vector2.Zero;
                    float deltaX = Math.Abs(targetPos.X - this.Position.X);
                    float deltaY = Math.Abs(targetPos.Y - this.Position.Y);

                    Utilities.GetMoveSpeed(Position, targetPos, Speed, ref direction);
                    CheckMapForCollisionsAndMove(direction);
                }
            }
        }
        
        public void RollOver()
        {
            Position = Utilities.Normalize(MapManager.Maps[CurrentMapName].GetCharacterSpawn("NPC" + _index));
            CalculatePathing();
        }
        public void CalculatePathing()
        {
            string currDay = GameCalendar.GetDay();
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
                            timePath = FindPathToLocation(start, MapManager.Maps[CurrentMapName].DictionaryCharacterLayer[kvp.Value], CurrentMapName);
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
        public void ClearTileForMapChange()
        {
            while (_currentPath[0].MapName == CurrentMapName)
            {
                _currentPath.RemoveAt(0);
            }
        }       //When we change maps, we need to empty out all tiles we're moving to on the map we left

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
                text = GetSelectionText();
            }
            text = ProcessText(text);
            GUIManager.LoadTextScreen(this, text);
        }

        public void LoadContent()
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\NPC1"));
            _sprite.AddAnimation("Stand South", 0, 0, 32, 64, 1, 0.3f);
            _sprite.AddAnimation("Walk South", 0, 0, 32, 64, 4, 0.3f);
            _sprite.AddAnimation("Walk North", 0, 64, 32, 64, 4, 0.3f);
            _sprite.AddAnimation("Walk East", 0, 128, 32, 64, 4, 0.3f);
            _sprite.AddAnimation("Walk West", 0, 192, 32, 64, 4, 0.3f);
            _sprite.SetCurrentAnimation("Stand South");
            _sprite.IsAnimating = true;
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
            return ProcessText(text);
        }

        public virtual string GetText()
        {
            RHRandom r = new RHRandom();
            string text = _dialogueDictionary[r.Next(1, 2).ToString()];
            return ProcessText(text);
        }

        public virtual string GetDialogEntry(string entry)
        {
            if (entry.Equals("Talk"))
            {
                return GetText();
            }
            else if (entry.Equals("GiveGift"))
            {
                GUIManager.LoadScreen(GUIManager.Screens.Inventory, this);
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
                return _dialogueDictionary.ContainsKey(entry) ? ProcessText(_dialogueDictionary[entry]) : string.Empty;
            }
        }

        public string ProcessText(string text)
        {
            string rv = string.Empty;
            string[] sections = text.Split(new[] { '$' }, StringSplitOptions.RemoveEmptyEntries);
            for(int i=0; i< sections.Length; i++)
            {
                if (int.TryParse(sections[i], out int val))
                {
                    if (val == 0) { sections[i] = _name; }
                    else { sections[i] = CharacterManager.GetCharacterNameByIndex(val); }
                }
                else if (sections[i] == "^") { sections[i] = PlayerManager.Name; }

                rv += sections[i];
            }

            return rv;
        }

        public void Gift(Item item)
        {
            if (item != null)
            {
                string text = string.Empty;
                item.Remove(1);
                if (item.Type == Item.ItemType.Map && Type == NPC.NPCType.Ranger)
                {
                    text = GetDialogEntry("Adventure");
                    DungeonManager.LoadNewDungeon((AdventureMap)item);
                }
                else
                {
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
                        MapManager.Maps["HouseNPC" + _index].AddCollectionItem(item.ItemID, index);
                    }
                    else
                    {
                        text = GetDialogEntry("Gift");
                        Friendship += 10;
                    }
                }

                if (!string.IsNullOrEmpty(text))
                {
                    GUIManager.LoadTextScreen(this, text);
                }
            }
        }

        public void SetName(string text)
        {
            _name = text;
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
                string fromMap = mapCameFrom[testMap];
                if (fromMap != testMap)
                {
                    start = MapManager.Maps[testMap].DictionaryEntrance[fromMap].Location.ToVector2();
                }
                if (MapManager.Maps[testMap].DictionaryCharacterLayer.ContainsKey(findKey))
                {
                    mapName = testMap;
                    newStart = MapManager.Maps[testMap].DictionaryCharacterLayer[findKey];
                    List<RHTile> pathToExit = FindPathToLocation(start, MapManager.Maps[testMap].DictionaryCharacterLayer[findKey], testMap);
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

                foreach (KeyValuePair<Rectangle, string> exit in MapManager.Maps[testMap].DictionaryExit)
                {
                    List<RHTile> pathToExit = FindPathToLocation(start, exit.Key.Location.ToVector2(), testMap);
                    double newCost = mapCostSoFar[testMap] + pathToExit.Count;
                    if (!mapCostSoFar.ContainsKey(exit.Value))
                    {
                        mapCostSoFar[exit.Value] = newCost + pathToExit.Count;
                        frontier.Enqueue(exit.Value, newCost);
                        mapCameFrom[exit.Value] = testMap;
                        _dictMapPathing[testMap + ":" + exit.Value] = pathToExit; // This needd another key for the appropriate exit
                    }
                    ClearPathingTracks();
                }
            }

            return _completeTilePath;
        }
        private List<RHTile> FindPathToLocation(Vector2 start, Vector2 target, string mapName)
        {
            List<RHTile> returnList = null;
            RHMap map = MapManager.Maps[mapName];
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
            return (Math.Abs(a.Position.X - b.Position.X) + Math.Abs(b.Position.Y - b.Position.Y)) * (a.IsRoad ? 1 : slowCost);
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
