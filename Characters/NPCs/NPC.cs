﻿using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.Misc;
using RiverHollow.Items;

namespace RiverHollow.Characters
{
    public class NPC : WorldCharacter
    {
        public enum NPCType { Villager, Shopkeeper, Ranger, Worker }
        protected NPCType _npcType;
        public NPCType Type { get => _npcType; }
        public int Friendship;

        protected Dictionary<int, bool> _collection;
        protected bool _introduced;

        protected Dictionary<string,Dictionary<string,string>> _schedule;
        protected string _moveTo;

        private const int PortraitWidth = 160;
        private const int PortraitHeight = 192;
        protected Texture2D _portrait;
        public Texture2D Portrait { get => _portrait; }
        protected Rectangle _portraitRect;
        public Rectangle PortraitRectangle { get => _portraitRect; }

        protected string _currentMap;
        public string CurrentMap { get => _currentMap; }

        protected Dictionary<string, string> _dialogueDictionary;

        public NPC()
        {
        }

        public NPC(int index, string[] data)
        {
            _collection = new Dictionary<int, bool>();
            _schedule = new Dictionary<string, Dictionary<string, string>>();
            _dialogueDictionary = GameContentManager.LoadDialogue(@"Data\Dialogue\NPC" + index);
            _portrait = GameContentManager.GetTexture(@"Textures\portraits");

            LoadContent();
            ImportBasics(data, index);

            MapManager.Maps[_currentMap].AddCharacter(this);
        }

        protected int ImportBasics(string[] stringData, int index)
        {
            int i = 0;
            _npcType = (NPCType)Enum.Parse(typeof(NPCType), stringData[i++]);
            _name = stringData[i++];
            _portraitRect = new Rectangle(0, int.Parse(stringData[i++]) * 192, PortraitWidth, PortraitHeight);
            _currentMap = stringData[i++];
            Position = MapManager.Maps[_currentMap].GetNPCSpawn("NPC" + index);
            string[] vectorSplit = stringData[i++].Split(' ');
            foreach (string s in vectorSplit) {
                _collection.Add(int.Parse(s), false);
            }

            Dictionary<string, string> schedule = CharacterManager.GetSchedule("NPC" + index);
            if (schedule != null)
            {
                foreach (KeyValuePair<string, string> kvp in schedule)
                {
                    Dictionary<string, string> temp = new Dictionary<string, string>();
                    string[] group = kvp.Value.Split('/');
                    foreach (string s in group)
                    {
                        string[] data = s.Split('|');
                        temp.Add(data[0], data[1]);
                    }
                    _schedule.Add(kvp.Key, temp);
                }
            }
            return i;
        } 

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
                Dictionary<string, string> dict = null;

                if (_schedule.ContainsKey(currSeason + currDay + currWeather)) { dict = _schedule[currSeason + currDay + currWeather]; }
                else if (_schedule.ContainsKey(currSeason + currDay)) { dict = _schedule[currSeason + currDay]; }
                else if (_schedule.ContainsKey(currDay)) { dict = _schedule[currDay]; }

                if (dict.ContainsKey(currTime))
                {
                    _moveTo = dict[currTime];
                }
            }

            if(!string.IsNullOrEmpty(_moveTo))
            {
                Vector2 pos = MapManager.Maps[_currentMap].DictionaryPathing[_moveTo];
                if (pos == Position) {
                    if (_schedule[currDay].ContainsKey(_moveTo))
                    {
                        _moveTo = _schedule[currDay][_moveTo];
                    }
                    else
                    {
                        _moveTo = string.Empty;
                    }
                }
                else {

                    Vector2 direction = Vector2.Zero;
                    float deltaX = Math.Abs(pos.X - this.Position.X);
                    float deltaY = Math.Abs(pos.Y - this.Position.Y);

                    Utilities.GetMoveSpeed(Position, pos, Speed, ref direction);
                    CheckMapForCollisionsAndMove(direction);
                }
            }
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
            GUIManager.LoadScreen(GUIManager.Screens.Text, this, text);
        }

        public void LoadContent()
        {
            base.LoadContent(@"Textures\NPC", 32, 64, 4, 0.2f);
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
                return "Gift me baby one more time.";
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
                    _collection[item.ItemID] = true;
                }
                else
                {
                    text = GetDialogEntry("Gift");
                    Friendship += 10;
                }
            }

            if (!string.IsNullOrEmpty(text))
            {
                GUIManager.LoadScreen(GUIManager.Screens.Text, this, text);
            }
        }
    }
}
