using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using RiverHollow.SpriteAnimations;
using RiverHollow.GUIObjects;

namespace RiverHollow.Items
{
    public class Item
    {
        public enum ItemEnum { Resource, Equipment, Tool, Container, Food, Map, Combat, Processor, Crafter };

        #region properties
        protected ItemEnum _itemType;
        public ItemEnum ItemType { get => _itemType; }
        protected int _itemID;
        public int ItemID { get => _itemID; }

        protected double _dWidth = 32;
        protected double _dHeight = 32;
        protected string _name;
        public string Name { get => _name; }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected Vector2 _sourcePos;

        protected Vector2 _position;
        public Vector2 Position { get => _position; set => _position = value; }

        public virtual Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, 32, 32); }
        public Rectangle SourceRectangle { get => new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, (int)_dWidth, (int)_dHeight); }

        protected bool _onTheMap;
        public bool OnTheMap { get => _onTheMap; set => _onTheMap = value; }

        protected bool _pickup = true;
        public bool Pickup { get => _pickup; set => _pickup = value; }

        protected string _description;

        protected int _columnTextureSize = 32;
        protected int _rowTextureSize = 32;
        private Parabola _movement;
        protected bool _doesItStack;
        public bool DoesItStack { get => _doesItStack; }

        protected int _num;
        public int Number { get => _num; set => _num = value; }

        protected int _sellPrice;
        public int SellPrice { get => _sellPrice; }
        #endregion
        public Item() { }

        public Item(int id, string[] itemValue, int num)
        {
            ImportBasics(itemValue, id, num);

            _doesItStack = true;
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }

        protected int ImportBasics(string[] stringData, int id, int num)
        {
            _num = num;

            int i = 0;
            _itemType = (ItemEnum)Enum.Parse(typeof(ItemEnum), stringData[i++]);
            _name = stringData[i++];
            _description = stringData[i++];
            string[] texIndices = stringData[i++].Split(' ');
            _sourcePos = new Vector2(0 + 32 * int.Parse(texIndices[0]), 0 + 32 * int.Parse(texIndices[1]));
            _sellPrice = int.Parse(stringData[i++]);

            _itemID = id;//(ObjectManager.ItemIDs)Enum.Parse(typeof(ObjectManager.ItemIDs), itemValue[i++]);

            return i;
        }
        //Copy Constructor
        public Item(Item item)
        {
            _itemID = item.ItemID;
            _sourcePos = item._sourcePos;
            _name = item.Name;
            _texture = item.Texture;
            _description = item._description;
            _num = item.Number;
            _doesItStack = item.DoesItStack;
        }

        public void Update()
        {
            if (_movement != null)
            {
                if (!_movement.Finished)
                {
                    _position = _movement.MoveTo();
                    _movement.Update();
                }
                else
                {
                    _movement = null;
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_onTheMap)
            {
                spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, (int)_dWidth, (int)_dHeight), SourceRectangle, Color.White);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            double drawHeight = _dHeight;
            int tempX = drawBox.X;
            int tempWidth = drawBox.Width;

            if (_dHeight != _dWidth)
            {
                int drawX = (_dHeight != _dWidth) ? (int)drawWidth / 2 : drawBox.X;
                drawBox.X += drawX;
                drawBox.Width = (int)drawWidth;
            }
            
            spriteBatch.Draw(_texture, drawBox, new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, (int)_dWidth, (int)_dHeight), Color.White);

            drawBox.X = tempX;
            drawBox.Width = tempWidth;
        }

        public virtual string GetDescription()
        {
            return _description;
        }

        public void Pop(Vector2 pos)
        {

            _position = pos;
            _onTheMap = true;
            _movement = new Parabola(_position, RandomVelocityVector(), RandNumber(8, 32, 0, 0));
        }

        public bool FinishedMoving()
        {
            bool rv = true;

            if (_movement != null && !_movement.Finished)
            {
                rv = false;
            }
            return rv;
        }

        public bool Remove(int x)
        {
            bool rv = false;
            if (x <= _num)
            {
                rv = true;
                _num -= x;
                if (_num == 0)
                {
                    if(GraphicCursor.HeldItem == this) { GraphicCursor.DropItem(); }
                    InventoryManager.RemoveItemFromInventory(this);
                }
            }
            return rv;
        }

        public Vector2 RandomVelocityVector()
        {
            float xVal = RandNumber(-3, 3, -1, 1);
            float divider = RandNumber(3, 5, 0, 0);
            return new Vector2(xVal/divider, RandNumber(-5, -2, 0, 0));
        }

        public int RandNumber(int minValue, int maxValue, int minExclude, int maxExclude)
        {
            Thread.Sleep(1);
            int rv = 0;
            int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
            Random r = new Random(seed);
            bool found = false;
            while (!found)
            {
                rv = r.Next(minValue, maxValue);
                if (rv < minExclude || rv > maxExclude)
                {
                    found = true;
                }
            }

            return rv;
        }

        public bool IsTool() { return _itemType == ItemEnum.Tool; }
        public bool IsCombatItem() { return _itemType == ItemEnum.Combat; }
        public bool IsEquipment() { return _itemType == ItemEnum.Equipment; }
        public bool IsFood() { return _itemType == ItemEnum.Food; }
        public bool IsContainer() { return _itemType == ItemEnum.Container; }

        public bool IsCrafter() { return _itemType == ItemEnum.Crafter; }
        public bool IsProcessor() { return _itemType == ItemEnum.Processor; }
        public bool IsMachine() { return IsProcessor() || IsCrafter(); }

        private class Parabola
        {
            private Vector2 _pos;
            private Vector2 _vel;
            public Vector2 Velocity { get => _vel; }
            private Vector2 _initialVelocity;
            private int _finalY;
            private bool _finished = false;
            public bool Finished { get => _finished; }
            public Parabola(Vector2 pos, Vector2 velocity, int Y)
            {
                _pos = pos;
                _vel = velocity;
                _initialVelocity = _vel;
                _finalY = (int)_pos.Y + Y;
            }

            public void Update()
            {
                _vel.Y += 0.2f;
                if (_pos.Y >= _finalY)
                {
                    _vel.Y = _initialVelocity.Y/1.5f;
                    _initialVelocity = _vel;
                    if (_vel.Y >= -1f)
                    {
                        _finished = true;
                    }
                }
            }

            public Vector2 MoveTo()
            {
                return _pos += _vel;
            }
        }
    }

    public class Equipment : Item
    {
        public enum EquipmentEnum { Armor, Weapon };
        public EquipmentEnum EquipType;
        private int _dmg;
        public int Dmg { get => _dmg; }
        private int _def;
        public int Def { get => _def; }
        private int _spd;
        public int Spd { get => _spd; }
        private int _mag;
        public int Mag { get => _mag; }
        private int _hp;
        public int HP { get => _hp; }

        public Equipment(int id, string[] stringData)
        {
            int i = ImportBasics(stringData, id, 1);
            EquipType = (EquipmentEnum)Enum.Parse(typeof(EquipmentEnum), stringData[i++]);
            _dmg = int.Parse(stringData[i++]);
            _def = int.Parse(stringData[i++]);
            _spd = int.Parse(stringData[i++]);
            _mag = int.Parse(stringData[i++]);
            _hp = int.Parse(stringData[i++]);
            _texture = GameContentManager.GetTexture(@"Textures\weapons");
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            if (Dmg > 0) { rv += "Dmg: +" + _dmg + " "; }
            if (Def > 0) { rv += "Def: +" + _def + " "; }
            if (Spd > 0) { rv += "Spd: +" + _spd + " "; }
            if (Mag > 0) { rv += "Mag: +" + _mag + " "; }
            if (HP > 0) { rv += "HP: +" + _hp + " "; }
            rv = rv.Trim();

            return rv;
        }
    }

    public class Tool : Item
    {
        public enum ToolEnum { Pick, Axe };
        public ToolEnum ToolType;
        protected int _staminaCost;
        public int StaminaCost { get => _staminaCost; }
        protected int _dmgValue;
        public int DmgValue { get => _dmgValue; }

        protected AnimatedSprite _sprite;
        public AnimatedSprite ToolAnimation { get => _sprite; }

        public Tool(int id, string[] stringData)
        {
            int i = ImportBasics(stringData, id, 1);
            ToolType = (ToolEnum)Enum.Parse(typeof(ToolEnum), stringData[i++]);
            _dmgValue = int.Parse(stringData[i++]);
            _staminaCost = int.Parse(stringData[i++]);
            _texture = GameContentManager.GetTexture(@"Textures\tools");

            _columnTextureSize = 128;
            _rowTextureSize = 32;

            _sprite = new AnimatedSprite(_texture);
            _sprite.AddAnimation("Left", (int)_sourcePos.X + 32, (int)_sourcePos.Y, 32, 32, 3, 0.1f);

            _sprite.CurrentAnimation = "Left";
            _sprite.IsAnimating = true;
            _sprite.PlaysOnce = true;
        }

        public void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            spriteBatch.Draw(_texture, drawBox, new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, 32, 32), Color.White);
        }
    }

    public class Food : Item
    {
        private int _stam;
        public int Stamina { get => _stam; }
        private int _health;
        public int Health { get => _health; }

        public Food(int id, string[] itemValue, int num)
        {
            int i = ImportBasics(itemValue, id, num);
            _stam = int.Parse(itemValue[i++]);
            _health = int.Parse(itemValue[i++]);

            _doesItStack = true;
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            if (Health > 0) { rv += "Health: +" + _health + " "; }
            if (Stamina > 0) { rv += "Stamina: +" + _stam + " "; }
            rv = rv.Trim();

            return rv;
        }
    }

    public class AdventureMap : Item
    {
        private int _difficulty;
        public int Difficulty { get => _difficulty; }
        public AdventureMap(int id, string[] itemValue, int num)
        {
            int i = ImportBasics(itemValue, id, num);
            _difficulty = RandNumber(4, 5, 0, 0);

            _doesItStack = false;
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            rv += "Difficulty: " + _difficulty;

            return rv;
        }
    }

    public class CombatItem : Item
    {
        private int _stam;
        public int Stamina { get => _stam; }
        private int _health;
        public int Health { get => _health; }
        private int _mana;
        public int Mana { get => _mana; }

        public CombatItem(int id, string[] itemValue, int num)
        {
            int i = ImportBasics(itemValue, id, num);
            _stam = int.Parse(itemValue[i++]);
            _health = int.Parse(itemValue[i++]);
            _mana = int.Parse(itemValue[i++]);

            _doesItStack = true;
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            if (_health > 0) { rv += "Health: +" + _health + " "; }
            if (_stam > 0) { rv += "Stamina: +" + _stam + " "; }
            if (_mana > 0) { rv += "Mana: +" + _mana + " "; }
            rv = rv.Trim();

            return rv;
        }
    }
}
