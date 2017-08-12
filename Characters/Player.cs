﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Adventure.SpriteAnimations;
using Adventure.Tile_Engine;
using Adventure.Characters;
using Adventure.Items;
using System;

using ItemIDs = Adventure.Items.ItemList.ItemIDs;
namespace Adventure
{
    public class Player : CombatCharacter
    {
        private Weapon sword;
        private InventoryItem[] _inventory;

        public Player(ContentManager theContentManager)
        {
            LoadContent(theContentManager);
            Position = new Vector2(200, 200);
            Speed = 5;

            _inventory = new InventoryItem[12];
        }
        public void LoadContent(ContentManager theContentManager)
        {
            base.LoadContent(theContentManager, @"Eggplant", 32, 64, 4, 0.2f);
            sword = new Weapon(theContentManager, this);
        }

        public override void Update(GameTime theGameTime, TileMap currMap)
        {
            Vector2 moveVector = Vector2.Zero;
            Vector2 moveDir = Vector2.Zero;
            string animation = "";

            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                sword.Attack(this);
            }
            sword.Update(theGameTime);

            KeyboardState ks = Keyboard.GetState();
            if (ks.IsKeyDown(Keys.W))
            {
                moveDir += new Vector2(0, -_speed);
                animation = "Float";
                moveVector += new Vector2(0, -_speed);
            }
            else if (ks.IsKeyDown(Keys.S))
            {
                moveDir += new Vector2(0, _speed);
                animation = "Float";
                moveVector += new Vector2(0, _speed);
            }

            if (ks.IsKeyDown(Keys.A))
            {
                moveDir += new Vector2(-_speed, 0);
                animation = "Float";
                moveVector += new Vector2(-_speed, 0);
            }
            else if (ks.IsKeyDown(Keys.D))
            {
                moveDir += new Vector2(_speed, 0);
                animation = "Float";
                moveVector += new Vector2(_speed, 0);
            }

            if (moveDir.Length() != 0)
            {
                Rectangle testRectX = new Rectangle((int)Position.X + (int)moveDir.X, (int)Position.Y, Width, Height);
                Rectangle testRectY = new Rectangle((int)Position.X, (int)Position.Y + (int)moveDir.Y, Width, Height);

                if (currMap.CheckLeftMovement(testRectX) && currMap.CheckRightMovement(testRectX))
                {
                    _sprite.MoveBy((int)moveDir.X, 0);
                }

                if (currMap.CheckUpMovement(testRectY) &&currMap.CheckDownMovement(testRectY))
                {
                    _sprite.MoveBy(0, (int)moveDir.Y);
                }

                if (_sprite.CurrentAnimation != animation)
                {
                    _sprite.CurrentAnimation = animation;
                }
            }
            else
            {
                _sprite.CurrentAnimation = "Float" + _sprite.CurrentAnimation.Substring(4);
            }

            this.Position = new Vector2(_sprite.Position.X, _sprite.Position.Y);

            _sprite.Update(theGameTime, currMap);
        }

        public void Draw(GameTime theGameTime, SpriteBatch spriteBatch)
        {
            sword.Draw(theGameTime, spriteBatch);
            base.Draw(spriteBatch);
        }

        public bool HasSpaceInInventory(ItemIDs itemID)
        {
            bool rv = false;
            if (itemID != ItemIDs.NOTHING)
            {
                for (int i = 0; i < _inventory.Length; i++)
                {
                    InventoryItem testItem = _inventory[i];
                    if (testItem == null)
                    {
                        rv = true;
                        break;
                    }
                    else
                    {
                        if (testItem.ItemID == itemID && testItem.Number < 999)
                        {
                            rv = true;
                            break;
                        }
                    }
                }
            }

            return rv;
        }

        public void AddItemToInventory(ItemIDs itemID)
        {
            if (itemID != ItemIDs.NOTHING)
            {
                for (int i = 0; i < _inventory.Length; i++)
                {
                    if (_inventory[i] == null)
                    {
                        _inventory[i] = ItemList.GetItem(itemID);
                        break;
                    }
                    else
                    {
                        if (_inventory[i].ItemID == itemID && _inventory[i].Number < 999)
                        {
                            _inventory[i].Number++;
                            break;
                        }
                    }
                }
            }
        }

        public string[] GetInventoryArray()
        {
            string[] stringArray = new string[_inventory.Length];
            for(int i = 0; i< _inventory.Length; i++)
            {
                if (_inventory[i] != null)
                {
                    stringArray[i] = _inventory[i].Name + ", " + _inventory[i].Number.ToString();
                }
            }

            return stringArray;
        }
    }
}
