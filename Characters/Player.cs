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
        private bool _usingTool = false;
        public bool UsingTool { get => _usingTool; }
        private bool _usingWeapon = false;
        public bool UsingWeapon { get => _usingWeapon; }

        private InventoryItem[] _inventory;
        public InventoryItem[] Inventory { get => _inventory; }

        private int _currentInventorySlot = 0;
        public int CurrentItemNumber { get => _currentInventorySlot; }
        //private InventoryItem _currentItem;
        public InventoryItem CurrentItem { get => _inventory[_currentInventorySlot]; }

        private WorldObject _targettedObject = null;

        private int _stamina;
        public int Stamina { get => _stamina; }

        private int _maxStamina;
        public int MaxStamina { get => _maxStamina; }

        private int _money;
        public int Money { get => _money; }

        public Player()
        {
            LoadContent();
            Position = new Vector2(200, 200);
            Speed = 5;
            _stamina = 50;
            _maxStamina = 50;

            _inventory = new InventoryItem[maxItemRow];
            //_currentItem = null;
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

            if (_usingTool)
            {
                ((Tool)CurrentItem).ToolAnimation.Position = Position;
                ((Tool)CurrentItem).Update(gameTime);
                _usingTool = ((Tool)CurrentItem).ToolAnimation.IsAnimating;
                if (_targettedObject != null && !_usingTool)
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
            else if (_usingWeapon)
            {
                ((Weapon)CurrentItem).Update(gameTime);
                _usingWeapon = ((Weapon)CurrentItem).StillAttacking;
            }
            else
            {
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Keys.W))
                {
                    _facing = Facing.North;
                    moveDir += new Vector2(0, -_speed);
                    animation = "Float";
                    moveVector += new Vector2(0, -_speed);
                }
                else if (ks.IsKeyDown(Keys.S))
                {
                    _facing = Facing.South;
                    moveDir += new Vector2(0, _speed);
                    animation = "Float";
                    moveVector += new Vector2(0, _speed);
                }

                if (ks.IsKeyDown(Keys.A))
                {
                    _facing = Facing.East;
                    moveDir += new Vector2(-_speed, 0);
                    animation = "Float";
                    moveVector += new Vector2(-_speed, 0);
                }
                else if (ks.IsKeyDown(Keys.D))
                {
                    _facing = Facing.West;
                    moveDir += new Vector2(_speed, 0);
                    animation = "Float";
                    moveVector += new Vector2(_speed, 0);
                }

                ProcessKeyboardInput(ks);

                if (moveDir.Length() != 0)
                {
                    Rectangle testRectX = new Rectangle((int)Position.X + (int)moveDir.X, (int)Position.Y, Width, Height);
                    Rectangle testRectY = new Rectangle((int)Position.X, (int)Position.Y + (int)moveDir.Y, Width, Height);

                    if (MapManager.CurrentMap.CheckLeftMovement(this, testRectX) && MapManager.CurrentMap.CheckRightMovement(this, testRectX))
                    {
                        _sprite.MoveBy((int)moveDir.X, 0);
                    }

                    if (MapManager.CurrentMap.CheckUpMovement(this, testRectY) && MapManager.CurrentMap.CheckDownMovement(this, testRectY))
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
            if (_usingTool)
            {
                ((Tool)CurrentItem).ToolAnimation.Draw(spriteBatch);
            }
            if (_usingWeapon)
            {
                ((Weapon)CurrentItem).Draw(spriteBatch);
            }
        }

        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (CurrentItem != null)
            {
                if (CurrentItem.GetType().Equals(typeof(Tool)) && MapManager.CurrentMap.PlayerInRange(CollisionBox, mouseLocation))
                {
                    if (DecreaseStamina(((Tool)CurrentItem).StaminaCost))
                    {
                        _usingTool = true;
                        ((Tool)CurrentItem).ToolAnimation.IsAnimating = true;
                        _targettedObject = MapManager.FindWorldObject(mouseLocation);
                    }
                    rv = true;
                }
                else if (CurrentItem.GetType().Equals(typeof(Weapon)))
                {
                    if (DecreaseStamina(((Weapon)CurrentItem).StaminaCost))
                    {
                        _usingWeapon = true;
                        ((Weapon)CurrentItem).Attack(_facing);
                    }
                    rv = true;
                }
            }

            return rv;
        }

        public bool ProcessKeyboardInput(KeyboardState ks)
        {
            bool rv = false;

            if (ks.IsKeyDown(Keys.D1)) { _currentInventorySlot = 0; }
            if (ks.IsKeyDown(Keys.D2)) { _currentInventorySlot = 1; }
            if (ks.IsKeyDown(Keys.D3)) { _currentInventorySlot = 2; }
            if (ks.IsKeyDown(Keys.D4)) { _currentInventorySlot = 3; }
            if (ks.IsKeyDown(Keys.D5)) { _currentInventorySlot = 4; }
            if (ks.IsKeyDown(Keys.D6)) { _currentInventorySlot = 5; }
            if (ks.IsKeyDown(Keys.D7)) { _currentInventorySlot = 6; }
            if (ks.IsKeyDown(Keys.D8)) { _currentInventorySlot = 7; }
            if (ks.IsKeyDown(Keys.D9)) { _currentInventorySlot = 8; }
            if (ks.IsKeyDown(Keys.D0)) { _currentInventorySlot = 9; }

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
                if (!IncrementExistingItem(itemID))
                {
                    for (int i = 0; i < _inventory.Length; i++)
                    {

                        if (_inventory[i] == null)
                        {
                            _inventory[i] = ObjectManager.GetItem(itemID);
                            break;
                        }
                    }
                }
            }
        }

        public bool IncrementExistingItem(ObjectManager.ItemIDs itemID)
        {
            bool rv = false;
            for (int i = 0; i < _inventory.Length; i++)
            {
                if (_inventory[i] != null && _inventory[i].ItemID == itemID && _inventory[i].Number < 999) { 
                    _inventory[i].Number++;
                    return true;
                }
            }
            return rv;
        }

        public bool AddItemToInventorySpot(InventoryItem item, int i)
        {
            bool rv = false;
            if (item != null)
            {
                if (_inventory[i] == null)
                {
                    if (item.GetType().Equals(typeof(Weapon)))
                    {
                        _inventory[i] = (Weapon)(item);
                    }
                    else if (item.GetType().Equals(typeof(Tool)))
                    {
                        _inventory[i] = (Tool)(item);
                    }
                    else
                    {
                        _inventory[i] = new InventoryItem(item);
                    }
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
                    break;
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

        public bool DecreaseStamina(int x)
        {
            bool rv = false;
            if (_stamina >= x)
            {
                _stamina -= x;
                rv = true;
            }
            return rv;
        }

        public void IncreaseStamina(int x)
        {
            _stamina += x;
        }

        public void TakeMoney(int x)
        {
            _money -= x;
        }

        public void AddMoney(int x)
        {
            _money += x;
        }
    }
}
