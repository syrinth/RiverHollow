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
        protected int _index;
        public enum NPCType { Villager, Shopkeeper, Ranger, Worker }
        protected NPCType _npcType;
        public NPCType Type { get => _npcType; }
        public int Friendship;

        protected Dictionary<int, bool> _collection;
        protected bool _introduced;

        protected Dictionary<string, List<KeyValuePair<string, string>>> _schedule;
        protected string _moveTo;
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
            _schedule = new Dictionary<string, List<KeyValuePair<string, string>>>();
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
                    _schedule.Add(kvp.Key, temp);
                }
            }
            return i;
        }
        bool pathingRun = false;
        List<RHTile> moveToTile;
        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);

            string currDay = GameCalendar.GetDay();
            string currSeason = GameCalendar.GetSeason();
            string currWeather = GameCalendar.GetWeather();
            string currTime = GameCalendar.GetTime();
            if (_schedule != null && _schedule.Count > 0)
            {
                string searchVal = currSeason + currDay + currWeather;
                List<KeyValuePair<string, string>> movementList = null;

                if (_schedule.ContainsKey(currSeason + currDay + currWeather))
                {
                    movementList = _schedule[currSeason + currDay + currWeather];
                }
                else if (_schedule.ContainsKey(currSeason + currDay))
                {
                    movementList = _schedule[currSeason + currDay];
                }
                else if (_schedule.ContainsKey(currDay))
                {
                    movementList = _schedule[currDay];
                }

                if (movementList != null)
                {
                    if (_scheduleIndex < movementList.Count && ((movementList[_scheduleIndex].Key == currTime) || RunningLate(movementList[_scheduleIndex].Key, currTime)))
                    {
                        _moveTo = movementList[_scheduleIndex].Value;
                    }
                    else if (_scheduleIndex < movementList.Count && movementList[_scheduleIndex].Key == CurrentMapName)
                    {
                        _moveTo = movementList[_scheduleIndex].Value;
                    }
                }
            }

            if(!string.IsNullOrEmpty(_moveTo))
            {
                Vector2 pos = Vector2.Zero;
                if (MapManager.Maps[CurrentMapName].DictionaryCharacterLayer.ContainsKey(_moveTo)) {
                    pos = MapManager.Maps[CurrentMapName].DictionaryCharacterLayer[_moveTo];

                    if (!pathingRun)
                    {
                        pathingRun = true;
                        moveToTile = RunPathing(Position, pos);
                    }
                }
                else if (MapManager.Maps[CurrentMapName].DictionaryExit.ContainsValue(_moveTo)) {
                    foreach (KeyValuePair<Rectangle, string> kvp in MapManager.Maps[CurrentMapName].DictionaryExit) {
                        if (kvp.Value == _moveTo)
                        {
                            pos = Utilities.Normalize(kvp.Key.Center.ToVector2());

                            if (!pathingRun)
                            {
                                pathingRun = true;
                                moveToTile = RunPathing(Position, pos);
                            }
                        }
                    }
                }

                if (moveToTile.Count > 0)
                {
                    pos = moveToTile[0].Position;
                    if (Position == pos)
                    {
                        moveToTile.RemoveAt(0);
                    }
                    else
                    {
                        Vector2 direction = Vector2.Zero;
                        float deltaX = Math.Abs(pos.X - this.Position.X);
                        float deltaY = Math.Abs(pos.Y - this.Position.Y);

                        Utilities.GetMoveSpeed(Position, pos, Speed, ref direction);
                        CheckMapForCollisionsAndMove(direction);
                    }
                }

                //if (pos == Position) {
                //    if (_scheduleIndex < _schedule[currDay].Count - 1 && _schedule[currDay][_scheduleIndex + 1].Key == _moveTo)
                //    {
                //        _moveTo = _schedule[currDay][++_scheduleIndex].Value;
                //    }
                //    else
                //    {
                //        _scheduleIndex++;
                //        _moveTo = string.Empty;
                //    }
                //}
                //else {

                //    Vector2 direction = Vector2.Zero;
                //    float deltaX = Math.Abs(pos.X - this.Position.X);
                //    float deltaY = Math.Abs(pos.Y - this.Position.Y);

                //    Utilities.GetMoveSpeed(Position, pos, Speed, ref direction);
                //    CheckMapForCollisionsAndMove(direction);
                //}
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
            GraphicCursor._currentType = GraphicCursor.CursorType.Normal;
            string text = string.Empty;
            if (!_introduced) {
                text = _dialogueDictionary["Introduction"];
                _introduced = true;
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

        //private class MapNode
        //{
        //    private RHTile _tile;
        //    public Vector2 Pos { get => _tile.Position; }
        //}

        Dictionary<RHTile, RHTile> cameFrom = new Dictionary<RHTile, RHTile>();
        Dictionary<RHTile, double> costSoFar = new Dictionary<RHTile, double>();

        private double Heuristic(RHTile a, RHTile b)
        {
            return (Math.Abs(a.Position.X - b.Position.X) + Math.Abs(b.Position.Y - b.Position.Y))*0.5;
        }

        private List<RHTile> RunPathing(Vector2 start, Vector2 target)
        {
            List<RHTile> returnList = null;
            RHMap map = MapManager.Maps[CurrentMapName];
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

        //Returns how much it costs to enter the next square
        private int GetMovementCost(RHTile target)
        {
            return target.ContainsProperty("Road", out string val) && val.Equals("true") ? 1 : 20;
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
    }
}
