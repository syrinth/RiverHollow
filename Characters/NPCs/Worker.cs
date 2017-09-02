using Adventure.Game_Managers;
using Adventure.GUIObjects;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ItemIDs = Adventure.Game_Managers.ObjectManager.ItemIDs;
namespace Adventure.Characters.NPCs
{
    public abstract class Worker : NPC
    {
        public abstract ObjectManager.WorkerID WorkerID { get; }
        public abstract string WorkerType { get; }
        protected int _dailyFoodReq;
        protected int _currFood;
        protected int _dailyItemID;
        protected Item _heldItem;
        protected int _mood;
        public int Mood { get => _mood; }

        public Worker()
        {
            _currFood = 0;
            _dailyFoodReq = 1;
            _dailyItemID = -1;
            _heldItem = null;
            _mood = 0;
        }

        public override void Talk()
        {
            GraphicCursor.talk = false;
            _mood += 1;

            Random r = new Random();
            GUIManager.LoadScreen(GUIManager.Screens.Text, this, Name + ": " + GameContentManager.GetDialogue(WorkerType +r.Next(1,3)));
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

        public void MakeDailyItem()
        {
            if (_heldItem == null) {
                _heldItem = ObjectManager.GetItem(_dailyItemID);
            }
        }

        public bool MouseInside(Point mousePosition)
        {
            bool rv = false;

            Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            if (rect.Contains(mousePosition))
            {
                rv = true;
            }

            return rv;
        }

        public string GetName()
        {
            return _name;
        }
    }
}
