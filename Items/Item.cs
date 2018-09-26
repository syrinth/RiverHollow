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
        protected ItemEnum _eItemType;
        public ItemEnum ItemType => _eItemType;
        protected int _iItemID;
        public int ItemID => _iItemID;
        protected Color _c = Color.White;
        public Color ItemColor => _c;

        protected double _dWidth = 16;
        protected double _dHeight = 16;
        protected string _sName;
        public string Name => _sName;

        protected Texture2D _texture;
        public Texture2D Texture => _texture;

        protected Vector2 _vSourcePos;

        protected Vector2 _vPosition;
        public virtual Vector2 Position { get => _vPosition; set => _vPosition = value; }

        public virtual Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, TileSize, TileSize); }
        public Rectangle SourceRectangle { get => new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, (int)_dWidth, (int)_dHeight); }

        protected bool _bOnMap;
        public bool OnTheMap { get => _bOnMap; set => _bOnMap = value; }

        protected bool _bAutoPickup = true;
        public bool AutoPickup { get => _bAutoPickup; set => _bAutoPickup = value; }
        protected bool _bManualPickup = false;
        public bool ManualPickup { get => _bManualPickup; set => _bManualPickup = value; }

        protected string _sDescription;

        protected int _iColTexSize = TileSize;
        protected int _iRowTexSize = TileSize;
        private Parabola _movement;
        protected bool _bStacks;
        public bool DoesItStack => _bStacks;

        protected int _iNum;
        public int Number { get => _iNum; }

        protected int _iSellPrice;
        public int SellPrice => _iSellPrice;
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

            _iItemID = id;
            GameContentManager.GetIemText(_iItemID, ref _sName, ref _sDescription);

            int i = 0;
            int totalCount = 0;
            for(; i< stringData.Length; i++)
            {
                string[] tagType = stringData[i].Split(':');
                if (tagType[0].Equals("Type"))
                {
                    _eItemType = Util.ParseEnum<ItemEnum>(tagType[1]);
                    totalCount++;
                }
                else if (tagType[0].Equals("Image"))
                {
                    string[] texIndices = tagType[1].Split('-');
                    _vSourcePos = new Vector2(0 + TileSize * int.Parse(texIndices[0]), 0 + TileSize * int.Parse(texIndices[1]));
                    totalCount++;
                }
                else if (tagType[0].Equals("Sell"))
                {
                    _iSellPrice = int.Parse(tagType[1]);
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
            _iItemID = item.ItemID;
            _eItemType = item.ItemType;
            _vSourcePos = item._vSourcePos;
            _sName = item.Name;
            _texture = item.Texture;
            _sDescription = item._sDescription;
            _iNum = item.Number;
            _bStacks = item.DoesItStack;
        }

        public void Update()
        {
            if (_movement != null)
            {
                if (!_movement.Finished)
                {
                    _vPosition = _movement.MoveTo();
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
            if (_bOnMap)
            {
                spriteBatch.Draw(_texture, new Rectangle((int)_vPosition.X, (int)_vPosition.Y, (int)_dWidth, (int)_dHeight), SourceRectangle, _c, 0, Vector2.Zero, SpriteEffects.None, (float)(Position.Y + _dHeight + (Position.X / 100)));
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBox, bool LayerDepth = false)
        {
            if (LayerDepth)
            {
                spriteBatch.Draw(_texture, drawBox, new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, (int)_dWidth, (int)_dHeight), _c, 0, Vector2.Zero, SpriteEffects.None, 99999);
            }
            else
            {
                spriteBatch.Draw(_texture, drawBox, new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, (int)_dWidth, (int)_dHeight), _c);
            }
        }

        public virtual string GetDescription()
        {
            return _sDescription;
        }

        public void Pop(Vector2 pos)
        {
            _vPosition = pos;
            _bOnMap = true;
            _movement = new Parabola(_vPosition, RandomVelocityVector(), Util.GetRandomFloat(-TileSize*2, TileSize*2, 3));
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

        /// <summary>
        /// Increments the number of the item by the given value.
        /// If it would exceed the max stack, make a new stack if possible.
        /// </summary>
        /// <param name="x"></param>
        public void Add(int x)
        {
            if (_iNum + x <= 999)
            {
                _iNum += x;
            }
            else
            {
                int leftOver = _iNum + x - 999;
                _iNum = 999;

                InventoryManager.AddNewItemToInventory(_iItemID, leftOver);
            }
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
            return new Vector2(Util.GetRandomFloat(-1, 1, 3), Util.GetRandomFloat(-5, 2, 3));
        }

        

        public virtual void UseItem() { }

        public virtual void ApplyUniqueData(string str) { }
        public virtual string GetUniqueData() { return string.Empty; }

        public bool IsTool() { return _eItemType == ItemEnum.Tool; }
        public bool IsCombatItem() { return _eItemType == ItemEnum.Combat; }
        public bool IsEquipment() { return _eItemType == ItemEnum.Equipment; }
        public bool IsFood() { return _eItemType == ItemEnum.Food; }
        public bool IsClassItem() { return _eItemType == ItemEnum.Class; }
        public bool IsContainer() { return _eItemType == ItemEnum.Container; }
        public bool IsStaticItem() { return _eItemType == ItemEnum.StaticItem; }
        public bool IsMarriage() { return _eItemType == ItemEnum.Marriage; }
        public bool IsMap() { return _eItemType == ItemEnum.Map; }
        public bool IsClothes() { return _eItemType == ItemEnum.Clothes; }

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
            Vector2 _vStart;
            Vector2 _vVel;
            public Vector2 Velocity { get => _vVel; }
            private Vector2 _vInitialVel;
            private float _fFinalY;
            private bool _bFinished = false;
            public bool Finished { get => _bFinished; }

            bool _bDownCurve;
            public Parabola(Vector2 pos, Vector2 velocity, float Y)
            {
                _vStart = pos;
                _vVel = velocity;
                _vInitialVel = _vVel;
                _fFinalY = _vStart.Y + Y;
            }

            public void Update()
            {
                _vVel.Y += 0.2f;

                if(_vVel.Y >= 0) {
                    _bDownCurve = true;
                }

                if (_bDownCurve && _vStart.Y >= _fFinalY)
                {
                    //Reset velocity for bouncing
                    _vVel.Y = _vInitialVel.Y/1.5f;
                    _vInitialVel = _vVel;
                    _bDownCurve = false;

                    if (_vVel.Y >= -1f)         //Only bounce a few times
                    {
                        _bFinished = true;
                    }
                }
            }

            public Vector2 MoveTo()
            {
                return _vStart += _vVel;
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
            _mainSprite = new AnimatedSprite(@"Textures\texClothes", true);
            _mainSprite.AddAnimation(WActorAnimEnum.WalkDown, TileSize, TileSize * 2, 3, 0.2f, startX, startY);
            _mainSprite.AddAnimation(WActorAnimEnum.IdleDown, TileSize, TileSize * 2, 1, 0.2f, startX + TileSize, startY);
            _mainSprite.AddAnimation(WActorAnimEnum.WalkUp, TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 3, startY);
            _mainSprite.AddAnimation(WActorAnimEnum.IdleUp, TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 4, startY);
            _mainSprite.AddAnimation(WActorAnimEnum.WalkLeft, TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 6, startY);
            _mainSprite.AddAnimation(WActorAnimEnum.IdleLeft, TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 7, startY);
            _mainSprite.AddAnimation(WActorAnimEnum.WalkRight, TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 9, startY);
            _mainSprite.AddAnimation(WActorAnimEnum.IdleRight, TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 10, startY);
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
                _vPosition = value;
                _sprite.Position = _vPosition;
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

            _iColTexSize = 128;
            _iRowTexSize = TileSize;

            _sprite = new AnimatedSprite(@"Textures\tools");
            _sprite.AddAnimation(ToolAnimEnum.Left, (int)_vSourcePos.X + TileSize, (int)_vSourcePos.Y, TileSize, TileSize, 2, 0.3f);

            _sprite.SetCurrentAnimation(ToolAnimEnum.Left);
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
            RHRandom r = new RHRandom();
            int i = ImportBasics(stringData, id, num);
            _difficulty = r.Next(4, 5);

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
            _eItemType = ItemEnum.Marriage;
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

            string n = ActorManager.GetClassByIndex(_iClassID).Name;
            _sName += n;
            _sDescription += n;

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
