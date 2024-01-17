using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents;
using RiverHollow.Misc;
using RiverHollow.Utilities;
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
        protected ItemEnum _eItemType;
        public ItemEnum ItemType => _eItemType;
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

        #endregion
        public Item(int id)
        {
            ID = id;
        }

        public Item(int id, Dictionary<string, string> stringData, int num)
        {
            _iNum = num;
            ID = id;

            //Item Type
            _eItemType = Util.ParseEnum<ItemEnum>(stringData["Type"]);

            //Image information
            string[] texIndices = Util.FindArguments(stringData["Image"]);
            _pSourcePos = new Point(int.Parse(texIndices[0]) * Constants.TILE_SIZE, int.Parse(texIndices[1]) * Constants.TILE_SIZE);

            Util.AssignValue(ref _diReqToMake, "ReqItems", stringData);

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

        public virtual void Draw(SpriteBatch spriteBatch, Rectangle drawBox, bool LayerDepth = false, float forcedLayerDepth = Constants.MAX_LAYER_DEPTH, float alpha = 1f)
        {
            DrawShadow(spriteBatch, drawBox, LayerDepth, forcedLayerDepth, alpha);
            if (LayerDepth)
            {
                spriteBatch.Draw(_texTexture, drawBox, SourceRectangle, _c * alpha, 0, Vector2.Zero, SpriteEffects.None, forcedLayerDepth);
            }
            else
            {
                spriteBatch.Draw(_texTexture, drawBox, SourceRectangle, _c * alpha);
            }
        }

        public void DrawShadow(SpriteBatch spriteBatch, Rectangle drawBox, bool LayerDepth = false, float forcedLayerDepth = Constants.MAX_LAYER_DEPTH, float alpha = 1f)
        {
            if (_bDrawShadow)
            {
                if (LayerDepth)
                {
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_MISC_SPRITES), drawBox, Constants.ITEM_SHADOW, _c * alpha, 0, Vector2.Zero, SpriteEffects.None, forcedLayerDepth);
                }
                else
                {
                    spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_MISC_SPRITES), drawBox, Constants.ITEM_SHADOW, _c * alpha);
                }
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

        public virtual string GetDetails()
        {
            return string.Empty;
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

        protected void ConfirmItemUse(TextEntry entry)
        {
            if (entry != null)
            {
                GUIManager.OpenTextWindow(entry, false);
            }
        }
        public virtual bool ItemBeingUsed() { return false; }
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
                case ItemEnum.Buildable:
                    BuildableEnum type = DataManager.GetEnumByIDKey<BuildableEnum>(ID - Constants.BUILDABLE_ID_OFFSET, "Subtype", DataType.WorldObject);
                    if (type == BuildableEnum.Floor || type == BuildableEnum.Wall) { return true; }
                    else { return false; }
                case ItemEnum.Blueprint:
                case ItemEnum.Clothing:
                    return false;
                case ItemEnum.NPCToken:
                case ItemEnum.Special:
                case ItemEnum.Tool:
                    return false;
                default:
                    return true;
            }
        }
        public bool CanBeDropped()
        {
            switch (_eItemType)
            {
                case ItemEnum.Tool:
                case ItemEnum.Buildable:
                    return false;
                default: return true;
            }
        }
        public bool Giftable()
        {
            return !(CompareType(ItemEnum.Tool) || CompareType(ItemEnum.Special));
        }
        public bool IsUnique()
        {
            bool rv = false;
            switch (_eItemType)
            {
                case ItemEnum.Blueprint:
                case ItemEnum.Special:
                    rv = !GetBoolByIDKey("Increase");
                    break;
                case ItemEnum.NPCToken:
                case ItemEnum.Tool:
                    rv = true;
                    break;
            }
            return rv;
        }
        public bool IsItemGroup(ItemGroupEnum e)
        {
            return GetEnumByIDKey<ItemGroupEnum>("Subtype") == e;
        }
        public bool CompareType(params ItemEnum[] types) { return types.Any(x => _eItemType == x); }

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

        public MonsterFood(int id, Dictionary<string, string> stringData, int num) : base(id, stringData, num)
        {
            string[] splitData = Util.FindArguments(stringData["Spawn"]);
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

        public override void UseItem()
        {
            MapManager.CurrentMap.PrimeMonsterSpawns(this);
            MapManager.CurrentMap.SpawnItemOnMap(this, PlayerManager.PlayerActor.CollisionBoxLocation, true);
            Remove(1);
            ClearGMObjects();
        }
    }
}
