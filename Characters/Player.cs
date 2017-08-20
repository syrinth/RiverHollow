using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Adventure.SpriteAnimations;
using Adventure.Tile_Engine;
using Adventure.Characters;
using Adventure.Items;
using System;

using ItemIDs = Adventure.Game_Managers.ObjectManager.ItemIDs;
using Adventure.Game_Managers;

namespace Adventure
{
    public class Player : CombatCharacter
    {
        public static int maxItemRow = 11;
        private bool usingTool = false;
        //private Weapon sword;
        private InventoryItem[] _inventory;
        public InventoryItem[] Inventory { get => _inventory; }

        private int _currentInventorySlot = 0;
        private InventoryItem _currentItem;
        public InventoryItem CurrentItem { get => _inventory[_currentInventorySlot]; }

        private WorldObject _targettedObject = null;

        public Player()
        {
            LoadContent();
            Position = new Vector2(200, 200);
            Speed = 5;

            _inventory = new InventoryItem[maxItemRow];
            _currentItem = null;
        }
        public void LoadContent()
        {
            base.LoadContent(@"Textures\Eggplant", 32, 64, 4, 0.2f);
            //sword = new Weapon(this);
        }

        public override void Update(GameTime gameTime)
        {
            Vector2 moveVector = Vector2.Zero;
            Vector2 moveDir = Vector2.Zero;
            string animation = "";

            if (usingTool && _targettedObject != null)
            {
                ((Tool)CurrentItem).ToolAnimation.Position = Position;
                ((Tool)CurrentItem).Update(gameTime);
                usingTool = ((Tool)CurrentItem).ToolAnimation.IsAnimating;
                if (!usingTool)
                {
                    bool destroyed = false;
                    if (_targettedObject.Breakable)
                    {
                        destroyed = _targettedObject.DealDamage(((Tool)CurrentItem).BreakValue);
                    }
                    else if (_targettedObject.Choppable)
                    {
                        destroyed = _targettedObject.DealDamage(((Tool)CurrentItem).ChopValue);
                    }

                    if (destroyed)
                    {
                        _targettedObject = null;
                    }
                    
                }
            }
            else
            {
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

                    if (_mapManager.CurrentMap.CheckLeftMovement(testRectX) && _mapManager.CurrentMap.CheckRightMovement(testRectX))
                    {
                        _sprite.MoveBy((int)moveDir.X, 0);
                    }

                    if (_mapManager.CurrentMap.CheckUpMovement(testRectY) && _mapManager.CurrentMap.CheckDownMovement(testRectY))
                    {
                        _sprite.MoveBy(0, (int)moveDir.Y);
                    }

                    if (_sprite.CurrentAnimation != animation)
                    {
                        _sprite.CurrentAnimation = animation;
                    }

                    this.Position = new Vector2(_sprite.Position.X, _sprite.Position.Y);
                }
                else
                {
                    _sprite.CurrentAnimation = "Float" + _sprite.CurrentAnimation.Substring(4);
                }
            }
            _sprite.Update(gameTime);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (usingTool)
            {
                ((Tool)CurrentItem).ToolAnimation.Draw(spriteBatch);
            }
        }

        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (CurrentItem.GetType().Equals(typeof(Tool)) && MapManager.GetInstance().CurrentMap.PlayerInRange(CollisionBox, mouseLocation))
            {
                usingTool = true;
                ((Tool)CurrentItem).ToolAnimation.IsAnimating = true;
                _targettedObject = MapManager.GetInstance().FindWorldObject(mouseLocation);
                rv = true;
            }

            return rv;
        }

        public bool HasSpaceInInventory(ItemIDs itemID)
        {
            bool rv = false;
            if (itemID != ItemIDs.Nothing)
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

        public void AddItemToFirstAvailableInventory(ItemIDs itemID)
        {
            if (itemID != ItemIDs.Nothing)
            {
                for (int i = 0; i < _inventory.Length; i++)
                {
                    if (_inventory[i] == null)
                    {
                        _inventory[i] = ObjectManager.GetItem(itemID);
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

        public bool AddItemToInventorySpot(InventoryItem item, int i)
        {
            bool rv = false;
            if (_inventory[i] == null)
            {
                _inventory[i] = new InventoryItem(item);
                rv = true;
            }
            else
            {
                if (_inventory[i].ItemID == item.ItemID && _inventory[i].DoesItStack && 999 >= (_inventory[i].Number + item.Number))
                {
                    _inventory[i].Number += item.Number;
                    rv = true;
                }
            }
            return rv;
        }

        public void RemoveItemFromInventory(int index)
        {
            for (int i = 0; i < _inventory.Length; i++)
            {
                if(i == index)
                {
                    _inventory[i] = null;
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
