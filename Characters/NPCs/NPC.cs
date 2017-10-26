﻿using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.Misc;

namespace RiverHollow.Characters
{
    public class NPC : WorldCharacter
    {
        //private List<ObjectManager.ItemIDs> _likedItems;
        //private List<ObjectManager.ItemIDs> _hatedItems;

        public enum NPCType { Villager, Shopkeeper, Ranger, Worker }
        protected NPCType _npcType;
        public NPCType Type { get => _npcType; }
        protected int _friendship;
        public int Friendship { get => _friendship; set => _friendship = value; }

        protected List<Vector2> _schedule;
        protected Vector2 _moveTo;

        private const int PortraitWidth = 160;
        private const int PortraitHeight = 192;
        protected Texture2D _portrait;
        public Texture2D Portrait { get => _portrait; }
        protected Rectangle _portraitRect;
        public Rectangle PortraitRectangle { get => _portraitRect; }

        protected string _name;
        public string Name { get => _name; }

        protected string _currentMap;
        public string CurrentMap { get => _currentMap; }

        protected Dictionary<string, string> _dialogueDictionary;

        public NPC()
        {
        }

        public NPC(int index, string[] data)
        {
            LoadContent();

            LoadBasic(data);

            _dialogueDictionary = GameContentManager.LoadDialogue(@"Data\Dialogue\NPC" + index);
            _portrait = GameContentManager.GetTexture(@"Textures\portraits");

            _schedule = new List<Vector2>
                {
                    new Vector2(Position.X - 100, Position.Y + 100),
                    Position
                };
            _moveTo = _schedule[0];

            MapManager.Maps[_currentMap].AddCharacter(this);
        }

        protected int LoadBasic(string[] data)
        {
            int i = 0;
            _npcType = (NPCType)Enum.Parse(typeof(NPCType), data[i++]);
            _name = data[i++];
            _portraitRect = new Rectangle(0, int.Parse(data[i++]) * 192, PortraitWidth, PortraitHeight);
            _currentMap = data[i++];
            string[] vectorSplit = data[i++].Split(' ');
            Position = new Vector2(int.Parse(vectorSplit[0]), int.Parse(vectorSplit[1]));
            return i;
        } 

        public override void Update(GameTime theGameTime)
        {
            base.Update(theGameTime);
        }

        public virtual void Talk()
        {
            GraphicCursor._currentType = GraphicCursor.CursorType.Normal;
            string text = string.Empty;
            if (CharacterManager._talkedTo.ContainsKey(_name) && CharacterManager._talkedTo[_name] == false) {
                text = _dialogueDictionary["Introduction"];
                CharacterManager._talkedTo[_name] = true;
            }
            else
            {
                text = GetText();
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
            return ProcessText(_dialogueDictionary[entry]);
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
    }
}
