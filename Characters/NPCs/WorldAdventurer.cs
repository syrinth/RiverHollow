﻿using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using RiverHollow.Items;
using Microsoft.Xna.Framework;
using RiverHollow.Misc;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.SpriteAnimations;

namespace RiverHollow.Characters.NPCs
{
    //Representation of an Adventurer in the world. WorldAdventurers also produce items at rollover
    public class WorldAdventurer : NPC
    {
        #region Properties
        protected int _id;
        public int ID { get => _id; }
        protected string _adventurerType;
        private WorkerBuilding _building;
        protected int _dailyFoodReq;
        protected int _currFood;
        protected int _dailyItemID;
        protected Item _heldItem;
        protected int _mood;
        public bool DrawIt;
        public bool Busy;
        public int Mood { get => _mood; }
        protected string _texture;

        private CombatAdventurer _c;
        #endregion

        public WorldAdventurer(string[] stringData, int id)
        {
            ImportBasics(stringData, id);
            _texture = @"Textures\" + _adventurerType;
            LoadContent(_texture);
            _currFood = 0;
            _heldItem = null;
            _mood = 0;
            DrawIt = true;
            Busy = false;

            SetCombat();
        }

        public WorldAdventurer(string[] stringData, int id, string name, int mood) : this(stringData, id)
        {
            _name = name;
            _mood = mood;
            SetCombat();
        }

        public void LoadContent(string texture)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(texture));
           // int xCrawl = 0;
            _sprite.AddAnimation("Idle", 16, 32, 1, 0.5f, 1 /*+ (xCrawl * 16)*/, 0);
            _sprite.SetCurrentAnimation("Idle");
            _sprite.SetScale(2);
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            _id = id;
            int i = 0;
            _adventurerType = stringData[i++];
            _dailyItemID = int.Parse(stringData[i++]);
            _dailyFoodReq = int.Parse(stringData[i++]);
            int portraitNum = int.Parse(stringData[i++]);
            _portraitRect = new Rectangle(0, portraitNum*192, 160, 192);
            _portrait = GameContentManager.GetTexture(@"Textures\portraits");

            return i;
        }

        protected void SetCombat()
        {
            _c = new CombatAdventurer(this);
            _c.SetClass(CharacterManager.GetClassByIndex(1));
            _c.LoadContent(@"Textures\Wizard");
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (DrawIt)
            {
                base.Draw(spriteBatch, useLayerDepth);
            }
        }

        public override bool Contains(Point mouse)
        {
            bool rv = false;
            if (DrawIt) {
                rv = base.Contains(mouse);
            }
            return rv;
        }
        public override void Talk()
        {
            GUIManager.LoadTextScreen(this, Name + ": " + GameContentManager.GetDialogue("AdventurerTree"));
        }

        public override string GetSelectionText()
        {
            RHRandom r = new RHRandom();
            string text = _dialogueDictionary[r.Next(1, 2).ToString()];
            return ProcessText(text);
        }

        public override string GetDialogEntry(string entry)
        {
            string rv = string.Empty;
            if (entry.Equals("Talk"))
            {
                GraphicCursor._currentType = GraphicCursor.CursorType.Normal;
                _mood += 1;

                RHRandom r = new RHRandom();
                rv = GameContentManager.GetDialogue(_adventurerType + r.Next(1, 2));
            }
            else if (entry.Equals("Party"))
            {
                DrawIt = false;
                Busy = true;
                PlayerManager.AddToParty(_c);
                rv = "Of course!";
            }
            return rv;
        }

        public int TakeItem()
        {
            int giveItem = -1;
            if (_heldItem != null){
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

        public void SetMood(int val)
        {
            _mood = val;
        }
        public void SetBuilding(WorkerBuilding b)
        {
            _building = b;
        }

        public bool Rollover()
        {
            bool rv = !Busy;
            DrawIt = true;
            Busy = false;
            _c.CurrentHP = _c.MaxHP;
            return rv;
        }

        public void MakeDailyItem()
        {
            if (_dailyItemID != -1)
            {
                InventoryManager.AddNewItemToInventory(_dailyItemID, _building.BuildingChest);
            }
        }

        public string GetName()
        {
            return _name;
        }
    }
}
