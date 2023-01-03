using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Item
    {
        #region properties
        protected ItemEnum _eItemType;
        public ItemEnum ItemType => _eItemType;
        public MerchType MerchType => DataManager.GetEnumByIDKey<MerchType>(ID, "MerchType", DataType.Item);
        public int ID { get; } = -1;
        protected Color _c = Color.White;
        public Color ItemColor => _c;

        protected double _dWidth = 16;
        protected double _dHeight = 16;

        protected Texture2D _texTexture;
        public Texture2D Texture => _texTexture;

        protected Vector2 _vSourcePos;

        protected Vector2 _vPosition;
        public virtual Vector2 Position { get => _vPosition; set => _vPosition = value; }

        public virtual Rectangle CollisionBox { get => new Rectangle((int)Position.X, (int)Position.Y, Constants.TILE_SIZE, Constants.TILE_SIZE); }
        public Rectangle SourceRectangle { get => new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, (int)_dWidth, (int)_dHeight); }

        protected bool _bOnMap;
        public bool OnTheMap { get => _bOnMap; set => _bOnMap = value; }

        protected bool _bAutoPickup = true;
        public bool AutoPickup { get => _bAutoPickup; set => _bAutoPickup = value; }
        protected bool _bManualPickup = false;
        public bool ManualPickup { get => _bManualPickup; set => _bManualPickup = value; }

        protected int _iColTexSize = Constants.TILE_SIZE;
        protected int _iRowTexSize = Constants.TILE_SIZE;
        private Parabola _movement;

        protected int _iNum;
        public int Number { get => _iNum; }

        public int Value => DataManager.GetIntByIDKey(ID, "Value", DataType.Item);
        public int TotalValue => Value * _iNum;
        public int SellPrice => Value / 2;

        //What items and in what numbers are required to make this item
        protected Dictionary<int, int> _diReqToMake;

        //Wahat this item refines into and how  many are required
        protected KeyValuePair<int, int> _kvpRefinesInto;
        public KeyValuePair<int, int> RefinesInto => _kvpRefinesInto;
        #endregion
        public Item() { }

        public Item(int id, Dictionary<string, string> stringData, int num)
        {
            _iNum = num;
            ID = id;

            //Item Type
            _eItemType = Util.ParseEnum<ItemEnum>(stringData["Type"]);

            //Image information
            string[] texIndices = stringData["Image"].Split('-');
            _vSourcePos = new Vector2(int.Parse(texIndices[0]), int.Parse(texIndices[1]));
            _vSourcePos *= Constants.TILE_SIZE;

            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);

            if (stringData.ContainsKey("RefinesInto"))
            {
                string[] splitData = stringData["RefinesInto"].Split('-');
                _kvpRefinesInto = new KeyValuePair<int, int>(int.Parse(splitData[0]), int.Parse(splitData[1]));
            }

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Resources");
        }

        //Copy Constructor
        public Item(Item item)
        {
            ID = item.ID;
            _eItemType = item.ItemType;
            _vSourcePos = item._vSourcePos;
            _texTexture = item.Texture;
            _iNum = item.Number;
        }

        public virtual void Update(GameTime gTime)
        {
            if (_movement != null)
            {
                if (!_movement.Finished)
                {
                    _vPosition = _movement.MoveTo();
                    _movement.Update(gTime);
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

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBox, bool LayerDepth = false, float forcedLayerDepth = Constants.MAX_LAYER_DEPTH, float alpha = 1f)
        {
            if (LayerDepth)
            {
                spriteBatch.Draw(_texTexture, drawBox, new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, (int)_dWidth, (int)_dHeight), _c * alpha, 0, Vector2.Zero, SpriteEffects.None, forcedLayerDepth);
            }
            else
            {
                spriteBatch.Draw(_texTexture, drawBox, new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, (int)_dWidth, (int)_dHeight), _c * alpha);
            }
        }

        public virtual string Name()
        {
            return DataManager.GetTextData("Item", ID, "Name");
        }
        public virtual string Description()
        {
            return Name() + System.Environment.NewLine + DataManager.GetTextData("Item", ID, "Description");
        }

        public void Pop(Vector2 pos)
        {
            _vPosition = pos;
            _movement = new Parabola(_vPosition);
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

                InventoryManager.AddToInventory(ID, leftOver, playerInventory);
            }
        }
        public virtual bool Remove(int x, bool playerInventory = true)
        {
            bool rv = false;
            if (x <= _iNum)
            {
                rv = true;
                _iNum -= x;
                if (_iNum == 0)
                {
                    if(GameManager.HeldItem == this) { GameManager.DropItem(); }
                    InventoryManager.RemoveItemFromInventory(this, playerInventory);
                    TaskManager.RemoveTaskProgress(this);
                }
            }
            return rv;
        }

        public Dictionary<int, int> GetRequiredItems()
        {
            return _diReqToMake;
        }

        public virtual bool AddToInventoryTrigger() { return false; }

        protected void ConfirmItemUse(TextEntry entry)
        {
            if (entry != null)
            {
                GUIManager.OpenTextWindow(entry, false);
            }
        }
        public virtual bool ItemBeingUsed() { return false; }
        public virtual bool HasUse() { return false; }
        public virtual void UseItem(TextEntryVerbEnum action) { }

        public virtual void ApplyUniqueData(string str) { }
        public virtual string GetUniqueData() { return string.Empty; }

        public void SetColor(Color c) { _c = c; }

        public bool Stacks()
        {
            switch (_eItemType)
            {
                case ItemEnum.Blueprint:
                case ItemEnum.Clothing:
                case ItemEnum.Equipment:
                case ItemEnum.NPCToken:
                case ItemEnum.Special:
                case ItemEnum.Tool:
                    return false;
                default:
                    return true;
            }
        }
        public bool Giftable()
        {
            bool rv = false;

            rv = !(CompareType(ItemEnum.Tool) || CompareType(ItemEnum.Special));

            return rv;
        }
        public bool IsUnique()
        {
            bool rv = false;
            switch (_eItemType)
            {
                case ItemEnum.Blueprint:
                case ItemEnum.NPCToken:
                case ItemEnum.Tool:
                    rv = true;
                    break;
            }
            return rv;
        }
        public bool CompareType(ItemEnum type) { return _eItemType == type; }

        public static string SaveItemToString(Item i)
        {
            string rv = string.Empty;
            ItemData data = SaveData(i);

            string strData = string.Empty;
            strData += data.itemID + "-";
            strData += data.num + "-";
            strData += data.strData;
            rv += strData;

            return rv;
        }
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
                itemID = ID,
                num = Number,
                strData = GetUniqueData()
            };

            return itemData;
        }

        private class Parabola
        {
            readonly double _dCurveDegree = -0.1;
            readonly double _dCurveWidth = 2.8;

            Vector2 _vPosition;
            Vector2 _vStartPosition;
            public bool Finished { get; private set; } = false;

            RHTimer _timer;
            double _dPauseTimer = 0.1;
            float _fXOffset = 0;
            readonly bool _bGoLeft = false;
            readonly bool _bBounce = true;

            Parabola subBounce;
            public Parabola(Vector2 pos)
            {
                RHRandom r = RHRandom.Instance();

                float widthChange = r.Next(0, 5);
                widthChange *= 0.1f;
                if (r.Next(0, 1) == 1) { widthChange *= -1; }
                _dCurveWidth += widthChange;
                if (r.Next(0, 1) == 1) {
                    _bGoLeft = true;
                    _dCurveWidth *= -1;
                }

                double timerChange = r.Next(0, 9);
                timerChange *= 0.01;
                if (r.Next(0, 1) == 1) { timerChange *= -1; }
                _timer = new RHTimer(Constants.ITEM_BOUNCE_SPEED + timerChange);

                _vStartPosition = pos;
                _vPosition = pos;
            }

            public Parabola(Vector2 pos, double curveDegree, double curveWidth, bool goingLeft)
            {
                _timer = new RHTimer(Constants.ITEM_BOUNCE_SPEED);
                _bBounce = false;
                _vStartPosition = pos;
                _vPosition = pos;
                _dCurveDegree = curveDegree;
                _dCurveWidth = curveWidth;
                _bGoLeft = goingLeft;
            }

            public void Update(GameTime gTime)
            {
                _timer.TickDown(gTime);
                if (_timer.Finished())
                {
                    if (_bBounce || (_bBounce && subBounce != null))
                    {
                        if (subBounce == null)
                        {
                            subBounce = new Parabola(_vPosition, _dCurveDegree + 0.04, _dCurveWidth + (_bGoLeft ? 1.5 : -1.5), _bGoLeft);
                        }
                        else if (!subBounce.Finished)
                        {
                            subBounce.Update(gTime);
                        }
                        else { Finished = true; }
                    }
                    else
                    {
                        if (_dPauseTimer > 0)
                        {
                            _dPauseTimer -= gTime.ElapsedGameTime.TotalSeconds;
                        }
                        else
                        {
                            Finished = true;
                        }
                    }
                }
                else
                {
                    _fXOffset += _bGoLeft ? -2f : 2f;
                    float yOffset = (float)((_dCurveDegree * (_fXOffset * _fXOffset)) + (_dCurveWidth * _fXOffset));
                    _vPosition.X = _vStartPosition.X + _fXOffset;
                    _vPosition.Y = _vStartPosition.Y - yOffset;
                }
            }

            public Vector2 MoveTo()
            {
                if (subBounce != null) { return subBounce.MoveTo(); }
                else { return _vPosition; }
            }
        }
    }

    public class MonsterFood : Item
    {
        int _iSpawnNum;
        public int SpawnNumber => _iSpawnNum;
        int _iSpawnID;
        public int SpawnID => _iSpawnID;

        public MonsterFood(int id, Dictionary<string, string> stringData, int num) : base(id, stringData, num)
        {
            string[] splitData = stringData["Spawn"].Split('-');
            _iSpawnNum = int.Parse(splitData[0]);
            _iSpawnID = int.Parse(splitData[1]);

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Consumables");
        }

        public override bool ItemBeingUsed()
        {
            GameManager.SetSelectedItem(this);
            ConfirmItemUse(DataManager.GetGameTextEntry("MonsterFood_False"));

            return true;
        }

        public override void UseItem(TextEntryVerbEnum action)
        {
            if (action.Equals("UseItem"))
            {
                MapManager.CurrentMap.PrimeMonsterSpawns(this);
                MapManager.CurrentMap.DropItemOnMap(this, PlayerManager.PlayerActor.Position);
                _bAutoPickup = false;
                _bManualPickup = false;
                Remove(1);
                ClearGMObjects();
            }
        }
    }
}
