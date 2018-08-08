using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Threading;
using RiverHollow.SpriteAnimations;
using RiverHollow.GUIObjects;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Misc;

namespace RiverHollow.WorldObjects
{
    public class Item
    {
        public enum ItemEnum { Resource, Class, Equipment, Tool, Container, Food, Map, Combat, StaticItem, Marriage, Clothes  };

        #region properties
        protected ItemEnum _itemType;
        public ItemEnum ItemType => _itemType;
        protected int _itemID;
        public int ItemID => _itemID;
        protected Color _c = Color.White;
        public Color ItemColor => _c;

        protected double _dWidth = 16;
        protected double _dHeight = 16;
        protected string _name;
        public string Name => _name;

        protected Texture2D _texture;
        public Texture2D Texture => _texture;

        protected Vector2 _sourcePos;

        protected Vector2 _position;
        public virtual Vector2 Position { get => _position; set => _position = value; }

        public virtual Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, TileSize, TileSize); }
        public Rectangle SourceRectangle { get => new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, (int)_dWidth, (int)_dHeight); }

        protected bool _onTheMap;
        public bool OnTheMap { get => _onTheMap; set => _onTheMap = value; }

        protected bool _pickup = true;
        public bool Pickup { get => _pickup; set => _pickup = value; }

        protected string _description;

        protected int _columnTextureSize = TileSize;
        protected int _rowTextureSize = TileSize;
        private Parabola _movement;
        protected bool _bStacks;
        public bool DoesItStack => _bStacks;

        protected int _iNum;
        public int Number { get => _iNum; set => _iNum = value; }

        protected int _sellPrice;
        public int SellPrice => _sellPrice;
        #endregion
        public Item() { }

        public Item(int id, string[] stringData, int num)
        {
            ImportBasics(stringData, id, num);

            _bStacks = true;
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }

        protected int ImportBasics(string[] stringData, int id, int num)
        {
            _iNum = num;

            _itemID = id;
            GameContentManager.GetIemText(_itemID, ref _name, ref _description);

            int i = 0;
            int totalCount = 0;
            for(; i< stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("Type"))
                {
                    _itemType = Util.ParseEnum<ItemEnum>(tagType[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Image"))
                {
                    string[] texIndices = tagType[1].Split('-');
                    _sourcePos = new Vector2(0 + TileSize * int.Parse(texIndices[0]), 0 + TileSize * int.Parse(texIndices[1]));
                    totalCount++;
                }
                else if (tagType[0].Equals("Sell"))
                {
                    _sellPrice = int.Parse(tagType[1]);
                    totalCount++;
                }

                if(totalCount == 3)
                {
                    break;
                }
            }

            return i;
        }
        //Copy Constructor
        public Item(Item item)
        {
            _itemID = item.ItemID;
            _itemType = item.ItemType;
            _sourcePos = item._sourcePos;
            _name = item.Name;
            _texture = item.Texture;
            _description = item._description;
            _iNum = item.Number;
            _bStacks = item.DoesItStack;
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
                spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, (int)_dWidth, (int)_dHeight), SourceRectangle, _c);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBox, bool LayerDepth = false)
        {
            double drawHeight = _dHeight;
            int tempX = drawBox.X;
            int tempWidth = drawBox.Width;

            if (_dHeight != _dWidth)
            {
                double drawWidth = (_dHeight != _dWidth) ? drawBox.Width * (_dWidth / _dHeight) : drawBox.Width;
                int drawX = (_dHeight != _dWidth) ? (int)drawWidth / 2 : drawBox.X;
                drawBox.X += drawX;
                drawBox.Width = (int)drawWidth;
            }

            if (LayerDepth)
            {
                spriteBatch.Draw(_texture, drawBox, new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, (int)_dWidth, (int)_dHeight), _c, 0, Vector2.Zero, SpriteEffects.None, 99999);
            }
            else
            {
                spriteBatch.Draw(_texture, drawBox, new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, (int)_dWidth, (int)_dHeight), _c);
            }

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
            _movement = new Parabola(_position, RandomVelocityVector(), RandNumber(8, TileSize, 0, 0));
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
            if (x <= _iNum)
            {
                rv = true;
                _iNum -= x;
                if (_iNum == 0)
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

        public virtual void UseItem() { }

        public virtual void ApplyUniqueData(string str) { }
        public virtual string GetUniqueData() { return string.Empty; }

        public bool IsTool() { return _itemType == ItemEnum.Tool; }
        public bool IsCombatItem() { return _itemType == ItemEnum.Combat; }
        public bool IsEquipment() { return _itemType == ItemEnum.Equipment; }
        public bool IsFood() { return _itemType == ItemEnum.Food; }
        public bool IsClassItem() { return _itemType == ItemEnum.Class; }
        public bool IsContainer() { return _itemType == ItemEnum.Container; }
        public bool IsStaticItem() { return _itemType == ItemEnum.StaticItem; }
        public bool IsMarriage() { return _itemType == ItemEnum.Marriage; }
        public bool IsMap() { return _itemType == ItemEnum.Map; }
        public bool IsClothes() { return _itemType == ItemEnum.Clothes; }

        public static ItemData SaveData(Item i)
        {
            if (i == null)
            {
                return new ItemData
                {
                    itemID = -1,
                    num = 0,
                    strData = ""
                }; 
            }
            else { return i.SaveData(); }
        }
        public ItemData SaveData()
        {
            ItemData itemData = new ItemData
            {
                itemID = ItemID,
                num = Number,
                strData = GetUniqueData()
            };

            return itemData;
        }

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
        public EquipmentEnum EquipType;
        private WeaponEnum _weaponType;
        public WeaponEnum WeaponType => _weaponType;
        private ArmorEnum _armorType;
        public ArmorEnum ArmorType => _armorType;

        int _iTier;

        private int _attack;
        public int Attack => _attack; 

        private int _str;
        public int Str => _str;
        private int _def;
        public int Def => _def;
        private int _vit;
        public int Vit => _vit;
        private int _mag;
        public int Mag => _mag;
        private int _res;
        public int Res => _res;
        private int _spd;
        public int Spd => _spd;

        public Equipment(int id, string[] stringData)
        {
            int i = ImportBasics(stringData, id, 1);

            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("EType"))
                {
                    EquipType = Util.ParseEnum<EquipmentEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("ESub"))
                {
                    if (EquipType == EquipmentEnum.Armor) { _armorType = Util.ParseEnum<ArmorEnum>(tagType[1]); }
                    else if (EquipType == EquipmentEnum.Weapon) { _weaponType = Util.ParseEnum<WeaponEnum>(tagType[1]); }
                }
                else if (tagType[0].Equals("Tier"))
                {
                    _iTier = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Atk"))
                {
                    _attack = GameContentManager.GetItemTierData(_iTier, tagType[1], false);
                }
                else if (tagType[0].Equals("Str"))
                {
                    _str = GameContentManager.GetItemTierData(_iTier, tagType[1]);
                }
                else if (tagType[0].Equals("Def"))
                {
                    _def = GameContentManager.GetItemTierData(_iTier, tagType[1]);
                }
                else if (tagType[0].Equals("Vit"))
                {
                    _vit = GameContentManager.GetItemTierData(_iTier, tagType[1]);
                }
                else if (tagType[0].Equals("Mag"))
                {
                    _mag = GameContentManager.GetItemTierData(_iTier, tagType[1]);
                }
                else if (tagType[0].Equals("Res"))
                {
                    _res = GameContentManager.GetItemTierData(_iTier, tagType[1]);
                }
                else if (tagType[0].Equals("Spd"))
                {
                    _spd = GameContentManager.GetItemTierData(_iTier, tagType[1]);
                }
            }

            _texture = GameContentManager.GetTexture(@"Textures\weapons");
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            if (Attack > 0) { rv += " Attack: +" + _attack + " "; }
            if (Str > 0) { rv += " Str: +" + _str + " "; }
            if (Def > 0) { rv += " Def: +" + _def + " "; }
            if (Vit > 0) { rv += " Vit: +" + _vit + " "; }
            if (Mag > 0) { rv += " Mag: +" + _mag + " "; }
            if (Res > 0) { rv += " REs: +" + _res + " "; }
            if (Spd > 0) { rv += " Spd: +" + _spd + " "; }
            rv = rv.Trim();

            return rv;
        }
    }

    public class Clothes : Item
    {
        public enum ClothesEnum { None, Chest, Pants, Hat};
        ClothesEnum _clothesType;
        public ClothesEnum ClothesType => _clothesType;

        private AnimatedSprite _mainSprite;
        public AnimatedSprite Sprite => _mainSprite;

        public Clothes(int id, string[] stringData)
        {
            int i = ImportBasics(stringData, id, 1);

            _texture = GameContentManager.GetTexture(@"Textures\items");

            _bStacks = false;
            int row = 0;

            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("CType"))
                {
                    _clothesType = Util.ParseEnum<ClothesEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Row"))
                {
                    row = int.Parse(tagType[1]);
                }
            }

            int startX = 0;
            int startY = TileSize * row * 2;
            _mainSprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\texClothes"), true);
            _mainSprite.AddAnimation("WalkDown", TileSize, TileSize * 2, 3, 0.2f, startX, startY);
            _mainSprite.AddAnimation("IdleDown", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize, startY);
            _mainSprite.AddAnimation("WalkUp", TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 3, startY);
            _mainSprite.AddAnimation("IdleUp", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 4, startY);
            _mainSprite.AddAnimation("WalkLeft", TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 6, startY);
            _mainSprite.AddAnimation("IdleLeft", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 7, startY);
            _mainSprite.AddAnimation("WalkRight", TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 9, startY);
            _mainSprite.AddAnimation("IdleRight", TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 10, startY);
        }

        public void SetSpritePosition(Vector2 Position)
        {
            _mainSprite.Position = Position;
        }

        public bool IsShirt() { return _clothesType == ClothesEnum.Chest; }
        public bool IsPants() { return _clothesType == ClothesEnum.Pants; }
        public bool IsHat() { return _clothesType == ClothesEnum.Hat; }
    }

    public class Tool : Item
    {
        public enum ToolEnum { Pick, Axe, Shovel, WateringCan };
        public ToolEnum ToolType;
        protected int _staminaCost;
        public int StaminaCost { get => _staminaCost; }
        protected int _dmgValue;
        public int DmgValue { get => _dmgValue; }

        protected AnimatedSprite _sprite;
        public AnimatedSprite ToolAnimation { get => _sprite; }

        public override Vector2 Position {
            set
            {
                _position = value;
                _sprite.Position = _position;
            }
        }

        public Tool(int id, string[] stringData)
        {
            int i = ImportBasics(stringData, id, 1);

            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("ToolType"))
                {
                    ToolType = Util.ParseEnum<ToolEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Dmg"))
                {
                    _dmgValue = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Stam"))
                {
                    _staminaCost = int.Parse(tagType[1]);
                }         
            }
            _texture = GameContentManager.GetTexture(@"Textures\tools");

            _columnTextureSize = 128;
            _rowTextureSize = TileSize;

            _sprite = new AnimatedSprite(_texture);
            _sprite.AddAnimation("Left", (int)_sourcePos.X + TileSize, (int)_sourcePos.Y, TileSize, TileSize, 2, 0.3f);

            _sprite.CurrentAnimation = "Left";
            _sprite.IsAnimating = true;
            _sprite.PlaysOnce = true;
        }

        public void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, 99999);
        }
    }

    public class Food : Item
    {
        private int _stam;
        public int Stamina { get => _stam; }
        private int _health;
        public int Health { get => _health; }

        public Food(int id, string[] stringData, int num)
        {
            int i = ImportBasics(stringData, id, num);

            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("Stam"))
                {
                    _stam = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Hp"))
                {
                    _health = int.Parse(tagType[1]);
                }
            }

            _bStacks = true;
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

        public override void UseItem()
        {
            if (Number > 0)
            {
                Remove(1);
                PlayerManager.IncreaseStamina(Stamina);
                PlayerManager.Combat.IncreaseHealth(Health);
            }
            BackToMain();
        }
    }

    public class AdventureMap : Item
    {
        private int _difficulty;
        public int Difficulty { get => _difficulty; }
        public AdventureMap(int id, string[] stringData, int num)
        {
            int i = ImportBasics(stringData, id, num);
            _difficulty = RandNumber(4, 5, 0, 0);

            _bStacks = false;
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

    public class MarriageItem : Item
    {
        public MarriageItem(int id, string[] stringData)
        {
            ImportBasics(stringData, id, 1);
            _itemType = ItemEnum.Marriage;
            _iNum = 1;
            _bStacks = false;
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }
    }

    public class CombatItem : Item
    {
        private ConditionEnum _targetsCondition;
        public ConditionEnum Condition => _targetsCondition;
        private int _iStam;
        public int Stamina => _iStam;
        private int _iHealth;
        public int Health => _iHealth;
        private int _iMana;
        public int Mana => _iMana;

        public bool Helpful;

        public CombatItem(int id, string[] stringData, int num)
        {
            int i = ImportBasics(stringData, id, num);

            for (; i < stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("CombatType"))
                {
                    Helpful = tagType[1].Equals("Helpful");
                }
                else if (tagType[0].Equals("Status"))
                {
                    _targetsCondition = Util.ParseEnum<ConditionEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Stam"))
                {
                    _iStam = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Hp"))
                {
                    _iHealth = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Mana"))
                {
                    _iMana = int.Parse(tagType[1]);
                }
            }

            _bStacks = true;
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            if (_targetsCondition > 0) { rv += "Fixes: " + _targetsCondition.ToString() + " "; }
            if (_iHealth > 0) { rv += "Health: +" + _iHealth + " "; }
            if (_iStam > 0) { rv += "Stamina: +" + _iStam + " "; }
            if (_iMana > 0) { rv += "Mana: +" + _iMana + " "; }
            rv = rv.Trim();

            return rv;
        }
    }

    public class ClassItem : Item
    {
        private int _iClassID;

        public ClassItem(int id, string[] stringData, int num)
        {
            int i = ImportBasics(stringData, id, num);

            _bStacks = false;
            _texture = GameContentManager.GetTexture(@"Textures\items");
        }

        public void SetClassChange(int i)
        {
            _iClassID = i;

            string n = CharacterManager.GetClassByIndex(_iClassID).Name;
            _name += n;
            _description += n;

            switch (_iClassID)
            {
                case 1:
                    _c = Color.Cyan;
                    break;
                case 2:
                    _c = Color.LightGray;
                    break;
                case 3:
                    _c = Color.Blue;
                    break;
                case 4:
                    _c = Color.Yellow;
                    break;
            }
        }

        public override void UseItem()
        {
            if (Number > 0)
            {
                Remove(1);
                PlayerManager.SetClass(_iClassID);
            }
            BackToMain();
        }

        public override void ApplyUniqueData(string str)
        {
            SetClassChange(int.Parse(str));
        }

        public override string GetUniqueData()
        {
            return _iClassID.ToString();
        }
    }
}
