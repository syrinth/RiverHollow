using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Items
{
    public class Item
    {
        #region properties
        protected ItemTypeEnum _eItemType;
        public ItemTypeEnum ItemType => _eItemType;
        public int ID { get; } = -1;
        protected Color _c = Color.White;
        public Color ItemColor => _c;

        protected int _iWidth = 16;
        protected int _iHeight = 16;

        protected Texture2D _texTexture;
        public Texture2D Texture => _texTexture;

        protected Point _pSourcePos;
        public Rectangle SourceRectangle => new Rectangle(_pSourcePos.X, _pSourcePos.Y, _iWidth, _iHeight);


        protected int _iNum;
        public int Number { get => _iNum; }

        public virtual int Value => GetIntByIDKey("Value");
        public int TotalSellValue => Value * _iNum;

        //What items and in what numbers are required to make this item
        protected Dictionary<int, int> _diReqToMake;

        private bool _bDrawShadow = false;

        public virtual bool Usable => false;

        #endregion
        public Item(int id)
        {
            ID = id;
        }

        public Item(int id, int num)
        {
            _iNum = num;
            ID = id;

            //Item Type
            _eItemType = GetEnumByIDKey<ItemTypeEnum>("Type");

            //Image information
            string[] texIndices = FindArgumentsByIDKey("Image");
            _pSourcePos = new Point(int.Parse(texIndices[0]) * Constants.TILE_SIZE, int.Parse(texIndices[1]) * Constants.TILE_SIZE);

            if (GetBoolByIDKey("ReqItems"))
            {
                _diReqToMake = new Dictionary<int, int>();

                //Split by "/" for each item set required
                string[] split = FindParamsByIDKey("ReqItems");
                foreach (string s in split)
                {
                    string[] splitData = Util.FindArguments(s);
                    _diReqToMake[int.Parse(splitData[0])] = splitData.Length > 1 ? int.Parse(splitData[1]) : 1;
                }
            }

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Resources");
        }

        //Copy Constructor
        public Item(Item item)
        {
            ID = item.ID;
            _eItemType = item.ItemType;
            _pSourcePos = item._pSourcePos;
            _texTexture = item.Texture;
            _iNum = item.Number;
        }

        public virtual void Update(GameTime gTime) { }

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBox, float forcedLayerDepth = Constants.MAX_LAYER_DEPTH, float alpha = 1f)
        {
            DrawShadow(spriteBatch, drawBox, forcedLayerDepth, alpha);
            spriteBatch.Draw(_texTexture, drawBox, SourceRectangle, _c * alpha, 0, Vector2.Zero, SpriteEffects.None, forcedLayerDepth);
        }

        public void DrawShadow(SpriteBatch spriteBatch, Rectangle drawBox, float forcedLayerDepth = Constants.MAX_LAYER_DEPTH, float alpha = 1f)
        {
            if (_bDrawShadow)
            {
                spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_MISC_SPRITES), drawBox, Constants.ITEM_SHADOW, _c * alpha, 0, Vector2.Zero, SpriteEffects.None, forcedLayerDepth - Constants.SPRITE_LINKED_BELOW_MOD);
            }
        }

        public virtual string Name()
        {
            return DataManager.GetTextData(ID, "Name", DataType.Item);
        }
        public virtual string Description()
        {
            return DataManager.GetTextData(ID, "Description", DataType.Item);
        }

        public List<Tuple<GameIconEnum, int>> GetDetails()
        {
            var rv = new List<Tuple<GameIconEnum, int>>();

            if (GetBoolByIDKey("Hp"))
            {
                rv.Add(new Tuple<GameIconEnum, int>(GameIconEnum.Health, GetIntByIDKey("Hp")));
            }
            if (GetBoolByIDKey("EnergyRecovery"))
            {
                rv.Add(new Tuple<GameIconEnum, int>(GameIconEnum.Energy, GetIntByIDKey("EnergyRecovery")));
            }
            if (this is Seed seedItem)
            {
                int totalTime = 0;

                var timeParam = DataManager.GetStringParamsByIDKey(seedItem.PlantID, "Time", DataType.WorldObject);
                foreach (var t in timeParam)
                {
                    if (int.TryParse(t, out int val))
                    {
                        totalTime += val;
                    }
                    else
                    {
                        break;
                    }
                }

                rv.Add(new Tuple<GameIconEnum, int>(GameIconEnum.Time, totalTime));
            }
            if (this is Tool toolItem)
            {
                rv.Add(new Tuple<GameIconEnum, int>(GameIconEnum.Level, toolItem.ToolLevel));
            }
            if (Value > 0)
            {
                rv.Add(new Tuple<GameIconEnum, int>(GameIconEnum.Coin, Value));
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
            if (_iNum + number <= Constants.MAX_STACK_SIZE)
            {
                _iNum += number;
            }
            else
            {
                int leftOver = _iNum + number - Constants.MAX_STACK_SIZE;
                _iNum = Constants.MAX_STACK_SIZE;

                InventoryManager.AddToInventory(ID, leftOver, playerInventory);
            }
        }
        public bool Remove(int x, bool playerInventory = true)
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
                }
                TaskManager.CheckItemCount();
            }
            return rv;
        }

        public void SetNumber(int value)
        {
            if (value > 0 && value < 999)
            {
                _iNum = value;
            }
        }

        public Dictionary<int, int> GetRequiredItems()
        {
            return _diReqToMake;
        }

        public virtual bool AddToInventoryTrigger()
        {
            TownManager.AddToCodex(ID);
            return false;
        }

        public virtual bool HasUse() { return false; }
        public virtual void UseItem() { }

        public void StrikeAPose()
        {
            var poseEntry = DataManager.GetGameTextEntry("Pose_" + ID, Name());
            if (GUIManager.IsTextWindowOpen())
            {
                GUIManager.QueueTextWindow(poseEntry);
            }
            else
            {
                GUIManager.OpenTextWindow(poseEntry);
            }

            PlayerManager.ObtainedItem = new MapItem(this)
            {
                Position = PlayerManager.PlayerActor.Position + new Point(0, -(Constants.TILE_SIZE - 9))
            };

            PlayerManager.PlayerActor.PlayAnimation(AnimationEnum.Pose);
        }

        public void DrawShadow(bool value)
        {
            _bDrawShadow = value;
        }

        public virtual void ApplyUniqueData(string str) { }
        public virtual string GetUniqueData() { return string.Empty; }

        public void SetColor(Color c) { _c = c; }

        public bool Stacks()
        {
            switch (_eItemType)
            {
                case ItemTypeEnum.Buildable:
                    BuildableEnum type = DataManager.GetEnumByIDKey<BuildableEnum>(ID - Constants.BUILDABLE_ID_OFFSET, "Subtype", DataType.WorldObject);
                    if (type == BuildableEnum.Floor || type == BuildableEnum.Wall) { return true; }
                    else { return false; }
                case ItemTypeEnum.Blueprint:
                    return false;
                case ItemTypeEnum.NPCToken:
                case ItemTypeEnum.Special:
                case ItemTypeEnum.Tool:
                    return false;
                default:
                    return true;
            }
        }
        public bool CanBeDropped()
        {
            switch (_eItemType)
            {
                case ItemTypeEnum.Tool:
                case ItemTypeEnum.Buildable:
                    return false;
                default: return true;
            }
        }
        public bool Giftable()
        {
            return !(CompareType(ItemTypeEnum.Tool) || CompareType(ItemTypeEnum.Special));
        }
        public bool IsUnique()
        {
            bool rv = false;
            switch (_eItemType)
            {
                case ItemTypeEnum.Blueprint:
                case ItemTypeEnum.Special:
                    rv = !GetBoolByIDKey("Increase");
                    break;
                case ItemTypeEnum.NPCToken:
                case ItemTypeEnum.Tool:
                    rv = true;
                    break;
            }
            return rv;
        }
        public ResourceTypeEnum GetItemGroup()
        {
            return GetEnumByIDKey<ResourceTypeEnum>("Subtype");
        }
        public bool IsItemGroup(ResourceTypeEnum e)
        {
            return GetEnumByIDKey<ResourceTypeEnum>("Subtype") == e;
        }
        public bool CompareType(params ItemTypeEnum[] types) { return types.Any(x => _eItemType == x); }

        #region Lookup Handlers
        public bool GetBoolByIDKey(string key)
        {
            return DataManager.GetBoolByIDKey(ID, key, DataType.Item);
        }
        public virtual int GetIntByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetIntByIDKey(ID, key, DataType.Item, defaultValue);
        }
        public virtual float GetFloatByIDKey(string key, float defaultValue = -1)
        {
            return DataManager.GetFloatByIDKey(ID, key, DataType.Item, defaultValue);
        }
        public string GetStringByIDKey(string key)
        {
            return DataManager.GetStringByIDKey(ID, key, DataType.Item);
        }
        public string[] FindArgumentsByIDKey(string key)
        {
            return Util.FindArguments(DataManager.GetStringByIDKey(ID, key, DataType.Item));
        }
        public string[] FindParamsByIDKey(string key)
        {
            return Util.FindParams(DataManager.GetStringByIDKey(ID, key, DataType.Item));
        }
        public virtual TEnum GetEnumByIDKey<TEnum>(string key) where TEnum : struct
        {
            return DataManager.GetEnumByIDKey<TEnum>(ID, key, DataType.Item);
        }
        #endregion

        public static string SaveItemToString(Item i)
        {
            ItemData data = SaveData(i);
            if (data.itemID == -1) { return ""; }
            else { return string.Format("{0}-{1}-{2}", Util.IntToString(data.itemID), data.num, data.strData); }
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
    }

    public class MonsterFood : Item
    {
        int _iSpawnNum;
        public int SpawnNumber => _iSpawnNum;
        int _iSpawnID;
        public int SpawnID => _iSpawnID;

        public MonsterFood(int id, int num) : base(id, num)
        {
            string[] splitData = Util.FindArguments(GetStringByIDKey("Spawn"));
            _iSpawnNum = int.Parse(splitData[0]);
            _iSpawnID = int.Parse(splitData[1]);

            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Consumables");
        }
        public override void UseItem()
        {
            MapManager.CurrentMap.PrimeMonsterSpawns(this);
            MapManager.CurrentMap.SpawnItemOnMap(this, PlayerManager.PlayerActor.CollisionBoxLocation, true);
            Remove(1);
            ClearGMObjects();
        }
    }

    public class Merchandise : Item
    {
        public int Tier => GetIntByIDKey("Tier");
        public ClassTypeEnum ClassType => GetEnumByIDKey<ClassTypeEnum>("Group");
        public MerchandiseTypeEnum MerchType => GetEnumByIDKey<MerchandiseTypeEnum>("Subtype");

        public Merchandise(int id, int num) : base(id, num)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Resources");
        }
    }

    public class Relic : Item
    {
        public Relic(int id, int num) : base (id, num)
        {
            _texTexture = DataManager.GetTexture(DataManager.FOLDER_ITEMS + "Relics");
        }
    }
}
