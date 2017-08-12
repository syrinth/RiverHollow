using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ItemIDs = Adventure.Items.ItemList.ItemIDs;
namespace Adventure.Characters.NPCs
{
    public class Worker : NPC
    {
        protected int _dailyFoodReq;
        protected int _currFood;
        protected ItemList.ItemIDs _dailyItemID;
        protected InventoryItem _heldItem;
        protected int _mood;

        public Worker()
        {
            _currFood = 0;
            _dailyFoodReq = 1;
            _dailyItemID = ItemIDs.NOTHING;
            _heldItem = null;
            _mood = 0;
        }

        public ItemIDs TakeItem()
        {
            ItemIDs giveItem = ItemIDs.NOTHING;
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
            return ItemIDs.NOTHING;
        }

        public void MakeDailyItem()
        {
            if (_heldItem == null) {
                _heldItem = ItemList.GetItem(_dailyItemID);
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

        public bool PlayerInRange(Rectangle playerRect)
        {
            bool rv = false;

            Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y, Width, Height);
            if (Math.Abs(rect.Center.X - playerRect.Center.X) <= TileMap._tileWidth &&
                Math.Abs(rect.Center.Y - playerRect.Center.Y) <= TileMap._tileWidth)
            {
                rv = true;
            }

            return rv;
        }
    }
}
