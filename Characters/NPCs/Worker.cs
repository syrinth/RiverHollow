using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using RiverHollow.Items;
using Microsoft.Xna.Framework;
using RiverHollow.Misc;

namespace RiverHollow.Characters.NPCs
{
    public class Worker : NPC
    {
        protected int _id;
        public int ID { get => _id; }
        protected string _workerType;
        private Building _building;
        protected int _dailyFoodReq;
        protected int _currFood;
        protected int _dailyItemID;
        protected Item _heldItem;
        protected int _mood;
        public int Mood { get => _mood; }
        protected string _texture;

        public Worker(string[] stringData, int id)
        {
            ImportBasics(stringData, id);
            _texture = @"Textures\" + _workerType;
            LoadContent(_texture, 32, 64, 1, 1);
            _currFood = 0;
            _heldItem = null;
            _mood = 0;
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            _id = id;
            int i = 0;
            _workerType = stringData[i++];
            _dailyItemID = int.Parse(stringData[i++]);
            _dailyFoodReq = int.Parse(stringData[i++]);
            int portraitNum = int.Parse(stringData[i++]);
            _portraitRect = new Rectangle(0, portraitNum*192, 160, 192);
            _portrait = GameContentManager.GetTexture(@"Textures\portraits");

            return i;
        }

        public override void Talk()
        {
            GraphicCursor._currentType = GraphicCursor.CursorType.Normal;
            _mood += 1;

            RHRandom r = new RHRandom();
            GUIManager.LoadScreen(GUIManager.Screens.Text, this, Name + ": " + GameContentManager.GetDialogue(_workerType + r.Next(1,2)));
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
