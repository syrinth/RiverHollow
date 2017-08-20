using Adventure.Game_Managers;
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
        protected int _dailyFoodReq;
        protected int _currFood;
        protected ObjectManager.ItemIDs _dailyItemID;
        protected InventoryItem _heldItem;
        protected int _mood;

        public Worker()
        {
            _currFood = 0;
            _dailyFoodReq = 1;
            _dailyItemID = ItemIDs.Nothing;
            _heldItem = null;
            _mood = 0;
        }

        public ItemIDs TakeItem()
        {
            ItemIDs giveItem = ItemIDs.Nothing;
            if (_heldItem != null){
                giveItem = _heldItem.GetItemID();
                _heldItem = null;
            }
            return giveItem;
        }

        public ItemIDs WhatAreYouHolding()
        {
            if (_heldItem != null)
            {
                return _heldItem.ItemID;
            }
            return ItemIDs.Nothing;
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
    }
}
