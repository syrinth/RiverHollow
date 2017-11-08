using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using RiverHollow.Items;
using Microsoft.Xna.Framework;
using RiverHollow.Misc;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.CombatStuff;

namespace RiverHollow.Characters.NPCs
{
    public class WorldAdventurer : NPC
    {
        #region Properties
        protected int _id;
        public int ID { get => _id; }
        protected string _adventurerType;
        private Building _building;
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
            LoadContent(_texture, 32, 64, 1, 1);
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
            _c.LoadContent(@"Textures\WizardCombat", 100, 100, 2, 0.7f);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (DrawIt)
            {
                base.Draw(spriteBatch);
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
            GUIManager.LoadScreen(GUIManager.Screens.Text, this, Name + ": " + GameContentManager.GetDialogue("AdventurerTree"));
        }

        public override string GetText()
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
            else if (entry.Equals("PartyAdd"))
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

        public void SetName(string text)
        {
            _name = text;
        }

        public void SetMood(int val)
        {
            _mood = val;
        }
        public void SetBuilding(Building b)
        {
            _building = b;
        }

        public bool Rollover()
        {
            bool rv = Busy;
            DrawIt = true;
            Busy = false;
            _c.CurrentHP = _c.MaxHP;
            return rv;
        }

        public void MakeDailyItem()
        {
            _building.BuildingChest.AddItemToFirstAvailableInventorySpot(_dailyItemID);
        }

        public string GetName()
        {
            return _name;
        }
    }
}
