using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using RiverHollow.SpriteAnimations;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Misc;
using System.Collections.Generic;
using RiverHollow.Actors;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Game_Managers.GUIObjects;

namespace RiverHollow.WorldObjects
{
    public class Item
    {
        public enum ItemEnum { Resource, Class, Equipment, Tool, Container, Food, Map, Consumable, StaticItem, Marriage, Clothes  };

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

        protected Texture2D _texTexture;
        public Texture2D Texture => _texTexture;

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

        //What items and in what numebrs are required to make this item
        protected Dictionary<int, int> _diReqToMake;

        //Wahat this item refines into and how  many are required
        protected KeyValuePair<int, int> _kvpRefinesInto;
        public KeyValuePair<int, int> RefinesInto => _kvpRefinesInto;
        #endregion
        public Item() { }

        public Item(int id, Dictionary<string, string> stringData, int num)
        {
            ImportBasics(stringData, id, num);

            _bStacks = true;
            _texTexture = GameContentManager.GetTexture(GameContentManager.ITEM_FOLDER + "Resources");
        }

        protected void ImportBasics(Dictionary<string, string> stringData, int id, int num)
        {
            _iNum = num;
            _iItemID = id;
            _diReqToMake = new Dictionary<int, int>();

            GameContentManager.GetItemText(_iItemID, ref _sName, ref _sDescription);

            //Item Type
            _eItemType = Util.ParseEnum<ItemEnum>(stringData["Type"]);

            //SellPrice
            _iSellPrice = int.Parse(stringData["Sell"]);

            //Image information
            string[] texIndices = stringData["Image"].Split('-');
            _vSourcePos = new Vector2(int.Parse(texIndices[0]), int.Parse(texIndices[1]));

            if (stringData.ContainsKey("ReqItems"))
            {
                //Split by " " for each item set required
                string[] split = stringData["ReqItems"].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach(string s in split)
                {
                    string[] splitData = s.Split('-');
                    _diReqToMake[int.Parse(splitData[0])] = int.Parse(splitData[1]);
                }
            }

            if (stringData.ContainsKey("RefinesInto"))
            {
                string[] splitData = stringData["RefinesInto"].Split('-');
                _kvpRefinesInto = new KeyValuePair<int, int>(int.Parse(splitData[0]), int.Parse(splitData[1]));
            }
        }
        //Copy Constructor
        public Item(Item item)
        {
            _iItemID = item.ItemID;
            _eItemType = item.ItemType;
            _vSourcePos = item._vSourcePos;
            _sName = item.Name;
            _texTexture = item.Texture;
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
                spriteBatch.Draw(_texTexture, new Rectangle((int)_vPosition.X, (int)_vPosition.Y, (int)_dWidth, (int)_dHeight), SourceRectangle, _c, 0, Vector2.Zero, SpriteEffects.None, (float)(Position.Y + _dHeight + (Position.X / 100)));
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBox, bool LayerDepth = false)
        {
            if (LayerDepth)
            {
                spriteBatch.Draw(_texTexture, drawBox, new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, (int)_dWidth, (int)_dHeight), _c, 0, Vector2.Zero, SpriteEffects.None, 99999);
            }
            else
            {
                spriteBatch.Draw(_texTexture, drawBox, new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, (int)_dWidth, (int)_dHeight), _c);
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
        /// <param name="number"></param>
        public void Add(int number, bool playerInventory = true)
        {
            if (_iNum + number <= 999)
            {
                _iNum += number;
            }
            else
            {
                int leftOver = _iNum + number - 999;
                _iNum = 999;

                InventoryManager.AddToInventory(_iItemID, leftOver, playerInventory);
            }
        }
        public virtual bool Remove(int x)
        {
            bool rv = false;
            if (x <= _iNum)
            {
                rv = true;
                _iNum -= x;
                if (_iNum == 0)
                {
                    if(GameManager.HeldItem == this) { GameManager.DropItem(); }
                    InventoryManager.RemoveItemFromInventory(this);
                }
            }
            return rv;
        }

        public Vector2 RandomVelocityVector()
        {
            return new Vector2(Util.GetRandomFloat(-1, 1, 3), Util.GetRandomFloat(-5, 2, 3));
        }

        public List<KeyValuePair<int, int>> GetIngredients()
        {
            List<KeyValuePair<int, int>> ret = new List<KeyValuePair<int, int>>();
            foreach(int key in _diReqToMake.Keys)
            {
                ret.Add(new KeyValuePair<int, int>(key, _diReqToMake[key]));
            }

            return ret;
        }

        public virtual void UseItem(string action) { }

        public virtual void ApplyUniqueData(string str) { }
        public virtual string GetUniqueData() { return string.Empty; }

        public bool IsTool() { return _eItemType == ItemEnum.Tool; }
        public bool IsConsumable() { return _eItemType == ItemEnum.Consumable; }
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
        private WeaponEnum _eWeaponType;
        public WeaponEnum WeaponType => _eWeaponType;
        private ArmorEnum _eArmorType;
        public ArmorEnum ArmorType => _eArmorType;
        private ArmorSlotEnum _eArmorSlot;
        public ArmorSlotEnum ArmorSlot => _eArmorSlot;

        int _iTier;

        private int _iAttack;
        public int Attack => _iAttack; 

        private int _iStr;
        public int Str => _iStr;
        private int _iDef;
        public int Def => _iDef;
        private int _iVit;
        public int Vit => _iVit;
        private int _iMag;
        public int Mag => _iMag;
        private int _iRes;
        public int Res => _iRes;
        private int _iSpd;
        public int Spd => _iSpd;

        public Equipment(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            //EType
            EquipType = Util.ParseEnum<EquipmentEnum>(stringData["EType"]);

            if (EquipType.Equals(EquipmentEnum.Armor)) { _texTexture = GameContentManager.GetTexture(@"Textures\Items\armor"); }
            else if (EquipType.Equals(EquipmentEnum.Weapon)) { _texTexture = GameContentManager.GetTexture(@"Textures\Items\weapons"); }
            else if (EquipType.Equals(EquipmentEnum.Accessory)) { _texTexture = GameContentManager.GetTexture(@"Textures\Items\Accessories"); }

            //ESub
            if (stringData.ContainsKey("ESub"))
            {
                if (EquipType == EquipmentEnum.Armor) { _eArmorType = Util.ParseEnum<ArmorEnum>(stringData["ESub"]); }
                else if (EquipType == EquipmentEnum.Weapon) { _eWeaponType = Util.ParseEnum<WeaponEnum>(stringData["ESub"]); }
            }

            if (EquipType == EquipmentEnum.Armor)
            {
                //Armor Slot
                _eArmorSlot = Util.ParseEnum<ArmorSlotEnum>(stringData["ASlot"]);
            }

            //Stats
            _iTier = int.Parse(stringData["Tier"]);
            if (stringData.ContainsKey(Util.GetEnumString(StatEnum.Str))) { _iStr = GetItemTierData(_iTier, stringData[Util.GetEnumString(StatEnum.Str)]); }
            if (stringData.ContainsKey(Util.GetEnumString(StatEnum.Def))) { _iDef = GetItemTierData(_iTier, stringData[Util.GetEnumString(StatEnum.Def)]); }
            if (stringData.ContainsKey(Util.GetEnumString(StatEnum.Vit))) { _iVit = GetItemTierData(_iTier, stringData[Util.GetEnumString(StatEnum.Vit)]); }
            if (stringData.ContainsKey(Util.GetEnumString(StatEnum.Mag))) { _iMag = GetItemTierData(_iTier, stringData[Util.GetEnumString(StatEnum.Mag)]); }
            if (stringData.ContainsKey(Util.GetEnumString(StatEnum.Res))) { _iRes = GetItemTierData(_iTier, stringData[Util.GetEnumString(StatEnum.Res)]); }
            if (stringData.ContainsKey(Util.GetEnumString(StatEnum.Spd))) { _iSpd = GetItemTierData(_iTier, stringData[Util.GetEnumString(StatEnum.Spd)]); }
            if (stringData.ContainsKey(Util.GetEnumString(StatEnum.Atk))) { _iAttack = GetItemTierData(_iTier, stringData[Util.GetEnumString(StatEnum.Atk)], false); }
        }


        private int GetItemTierData(int tier, string modifier, bool isStat = true)
        {
            int DivideBy = isStat ? 4 : 1; //If it's not a stat,it's localize on oneitem, don't divide.
            double rv = 0;

            if (modifier.Equals("Minor"))
            {
                rv = tier * (double)6 / DivideBy;
            }
            else if (modifier.Equals("Moderate"))
            {
                rv = tier * (double)8 / DivideBy;
            }
            else if (modifier.Equals("Major"))
            {
                rv = tier * (double)10 / DivideBy;
            }

            if (rv % 2 > 0) { rv++; }

            return (int)rv;
        }

        /// <summary>
        /// Appends the stats of the equipment to the item description
        /// </summary>
        /// <returns></returns>
        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            if (Attack > 0) { rv += " Attack: +" + _iAttack + " "; }
            if (Str > 0) { rv += " Str: +" + _iStr + " "; }
            if (Def > 0) { rv += " Def: +" + _iDef + " "; }
            if (Mag > 0) { rv += " Mag: +" + _iMag + " "; }
            if (Res > 0) { rv += " Res: +" + _iRes + " "; }
            if (Spd > 0) { rv += " Spd: +" + _iSpd + " "; }
            rv = rv.Trim();

            return rv;
        }

        internal int GetStat(StatEnum stat)
        {
            int rv = 0;
            switch (stat)
            {
                case StatEnum.Atk:
                    rv += this.Attack;
                    break;
                case StatEnum.Def:
                    rv += this.Def;
                    break;
                case StatEnum.Mag:
                    rv += this.Mag;
                    break;
                case StatEnum.Res:
                    rv += this.Res;
                    break;
                case StatEnum.Spd:
                    rv += this.Spd;
                    break;
                case StatEnum.Str:
                    rv += this.Str;
                    break;
            }

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

        public Clothes(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            _texTexture = GameContentManager.GetTexture(@"Textures\items");

            _bStacks = false;
            int row = 0;

            _clothesType = Util.ParseEnum<ClothesEnum>(stringData["CType"]);
            row = int.Parse(stringData["Row"]);

            int startX = 0;
            int startY = TileSize * row * 2;
            _mainSprite = new AnimatedSprite(@"Textures\texClothes");
            _mainSprite.AddAnimation(WActorWalkAnim.WalkDown, TileSize, TileSize * 2, 3, 0.2f, startX, startY, true);
            _mainSprite.AddAnimation(WActorBaseAnim.IdleDown, TileSize, TileSize * 2, 1, 0.2f, startX + TileSize, startY, true);
            _mainSprite.AddAnimation(WActorWalkAnim.WalkRight, TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 3, startY, true);
            _mainSprite.AddAnimation(WActorBaseAnim.IdleRight, TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 4, startY, true);
            _mainSprite.AddAnimation(WActorWalkAnim.WalkUp, TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 6, startY, true);
            _mainSprite.AddAnimation(WActorBaseAnim.IdleUp, TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 7, startY, true);
            _mainSprite.AddAnimation(WActorWalkAnim.WalkLeft, TileSize, TileSize * 2, 3, 0.2f, startX + TileSize * 9, startY, true);
            _mainSprite.AddAnimation(WActorBaseAnim.IdleLeft, TileSize, TileSize * 2, 1, 0.2f, startX + TileSize * 10, startY, true);
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
        public enum ToolEnum { Pick, Axe, Shovel, WateringCan, Harp };
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

        public Tool(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);

            ToolType = Util.ParseEnum<ToolEnum>(stringData["ToolType"]);
            if (stringData.ContainsKey("Dmg")) { _dmgValue = int.Parse(stringData["Dmg"]); }
            _staminaCost = int.Parse(stringData["Stam"]);

            _texTexture = GameContentManager.GetTexture(@"Textures\Items\tools");

            _iColTexSize = 128;
            _iRowTexSize = TileSize;

            _sprite = new AnimatedSprite(@"Textures\Items\tools");
            _sprite.AddAnimation(ToolAnimEnum.Down, (int)_vSourcePos.X + TileSize, (int)_vSourcePos.Y, TileSize, TileSize * 2, 3, TOOL_ANIM_SPEED);

            _sprite.SetCurrentAnimation(ToolAnimEnum.Down);
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

        public Food(int id, Dictionary<string, string> stringData, int num)
        {
            _texTexture = GameContentManager.GetTexture(GameContentManager.ITEM_FOLDER + "Food");

            ImportBasics(stringData, id, num);

            if (stringData.ContainsKey("Stam")) { _stam = int.Parse(stringData["Stam"]); }
            if (stringData.ContainsKey("Hp")) { _health = int.Parse(stringData["Hp"]); }

            _bStacks = true;
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

        public override void UseItem(string action)
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
        public AdventureMap(int id, Dictionary<string, string> stringData, int num)
        {
            RHRandom r = new RHRandom();
            ImportBasics(stringData, id, num);
            _difficulty = r.Next(4, 5);

            _bStacks = false;
            _texTexture = GameContentManager.GetTexture(@"Textures\items");
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
        public MarriageItem(int id, Dictionary<string, string> stringData)
        {
            ImportBasics(stringData, id, 1);
            _eItemType = ItemEnum.Marriage;
            _iNum = 1;
            _bStacks = false;
            _texTexture = GameContentManager.GetTexture(@"Textures\items");
        }
    }

    public class Consumable : Item
    {
        private ConditionEnum _targetsCondition;
        public ConditionEnum Condition => _targetsCondition;
        private int _iHealth;
        public int Health => _iHealth;
        private int _iMana;
        public int Mana => _iMana;
        private StatusEffect _statusEffect;
        private int _iStatusDuration;

        public bool Helpful;

        public Consumable(int id, Dictionary<string, string> stringData, int num)
        {
            ImportBasics(stringData, id, num);

            Helpful = stringData["CombatType"].Equals("Helpful");
            if (stringData.ContainsKey("Status")){ _targetsCondition = Util.ParseEnum<ConditionEnum>(stringData["Status"]); }
            if (stringData.ContainsKey("Hp")) { _iHealth = int.Parse(stringData["Hp"]); }
            if (stringData.ContainsKey("Mana")) { _iMana = int.Parse(stringData["Mana"]); }
            if (stringData.ContainsKey("StatusEffect")) {
                string[] strBuffer = stringData["StatusEffect"].Split('-');
                _statusEffect = ObjectManager.GetStatusEffectByIndex(int.Parse(strBuffer[0]));
                _iStatusDuration = int.Parse(strBuffer[1]);
            }

            _bStacks = true;
            _texTexture = GameContentManager.GetTexture(GameContentManager.ITEM_FOLDER + "Consumables");
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            if (_targetsCondition > 0) { rv += "Fixes: " + _targetsCondition.ToString() + " "; }
            if (_iHealth > 0) { rv += "Health: +" + _iHealth + " "; }
            if (_iMana > 0) { rv += "Mana: +" + _iMana + " "; }
            rv = rv.Trim();

            return rv;
        }

        public override void UseItem(string action)
        {
            if (Helpful)
            {
                CombatActor target = PlayerManager.GetParty()[int.Parse(action)];

                if (_iHealth > 0) { target.IncreaseHealth(_iHealth); }
                if (_iMana > 0) { target.IncreaseMana(_iMana); }
                if(_statusEffect != null) { target.AddStatusEffect(_statusEffect); }

                Remove(1);
            }
        }
    }

    public class StaticItem : Item
    {
        WorldItem worldObj;

        public StaticItem() { }
        public StaticItem(int id, Dictionary<string, string> stringData, int num = 1)
        {
            ImportBasics(stringData, id, num);
            _texTexture = GameContentManager.GetTexture(GameContentManager.ITEM_FOLDER + "StaticObjects");

            _bStacks = stringData.ContainsKey("Stacks");

            worldObj = (WorldItem)ObjectManager.GetWorldObject(id);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public WorldItem GetWorldItem()
        {
            return worldObj;
        }

        public void SetWorldObjectCoords(Vector2 vec)
        {
            worldObj.SetCoordinatesByGrid(vec);
        }

        /// <summary>
        /// Overrides the base Item remove method because StaticItem items have the
        /// WorldItem objects within them for drawing purposes. If we don't make a new one
        /// then any items that stack, like Walls, have all of their items attached
        /// to onlt the one worldObj
        /// </summary>
        /// <param name="x">The number to remove</param>
        /// <returns>True if item was successfully removed</returns>
        public override bool Remove(int x)
        {
            bool rv = base.Remove(x);

            //Create a new worldObj for any instances of the item that remains
            worldObj = (WorldItem)ObjectManager.GetWorldObject(_iItemID);

            return rv;
        }
    }

    public class ClassItem : Item
    {
        private int _iClassID;

        public ClassItem(int id, Dictionary<string, string> stringData, int num)
        {
            ImportBasics(stringData, id, num);

            _bStacks = false;
            _texTexture = GameContentManager.GetTexture(@"Textures\items");
        }

        public void SetClassChange(int i)
        {
            _iClassID = i;

            string n = ObjectManager.GetClassByIndex(_iClassID).Name;
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

        public override void UseItem(string action)
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
