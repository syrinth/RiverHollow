using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.DataManager;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;


namespace RiverHollow.Items
{
    public class WorldObject
    {
        #region Properties
        protected ObjectTypeEnum _eObjectType;
        public ObjectTypeEnum Type => _eObjectType;

        protected AnimatedSprite _sprite;
        public AnimatedSprite Sprite => _sprite;

        public List<RHTile> Tiles;

        protected string MapName => Tiles[0].MapName;
        protected bool _bImpassable = true;
        public bool Blocking => _bImpassable;
        protected bool _wallObject;
        public bool WallObject => _wallObject;

        protected KeyValuePair<int, int> _kvpDrop; //itemID, # of items dropped

        protected Point _pImagePos;

        protected Vector2 _vMapPosition;
        public virtual Vector2 MapPosition => _vMapPosition;

        public Rectangle ClickBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _iWidth, _iHeight);                  //ClickBox is always hard set
        public virtual Rectangle CollisionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _iWidth, _iHeight);      //Can be overriden to only be the base

        protected int _iWidth = TileSize;
        public int Width => _iWidth;
        protected int _iHeight = TileSize;
        public int Height => _iHeight;

        protected int _iBaseWidth = TileSize;
        public virtual int BaseWidth => _iBaseWidth;
        protected int _iBaseHeight = TileSize;
        public virtual int BaseHeight => _iBaseHeight;

        protected int _iID;
        public int ID { get => _iID; }

        protected string _sName;
        public string Name { get => _sName; }

        #endregion

        protected WorldObject() {
            Tiles = new List<RHTile>();
        }

        public WorldObject(int id, Vector2 pos) : this()
        {
            _iID = id;
            _wallObject = false;

            SnapPositionToGrid(pos);
            DataManager.GetTextData("WorldObject", _iID, ref _sName, "Name");
        }

        protected virtual void LoadDictionaryData(Dictionary<string, string> stringData, bool loadSprite = true)
        {
            string[] strPos = stringData["Image"].Split('-');
            _pImagePos = new Point(int.Parse(strPos[0]), int.Parse(strPos[1]));

            Util.AssignValue(ref _iWidth, "Width", stringData);
            Util.AssignValue(ref _iHeight, "Height", stringData);
            if (stringData.ContainsKey("Type")) { _eObjectType = Util.ParseEnum<ObjectTypeEnum>(stringData["Type"]); }

            if (loadSprite)
            {
                if (stringData.ContainsKey("Texture")) { LoadSprite(stringData, stringData["Texture"]); }
                else { LoadSprite(stringData); }
            }
        }

        protected virtual void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            _sprite = new AnimatedSprite(textureName);
            if (stringData.ContainsKey("Idle"))
            {
                string[] idleSplit = stringData["Idle"].Split('-');
                _sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _iWidth, _iHeight, int.Parse(idleSplit[0]), float.Parse(idleSplit[1]));
            }
            else
            {
                _sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _iWidth, _iHeight);
            }

            //MAR
            //if (stringData.ContainsKey("Gathered"))
            //{
            //    string[] gatherSplit = stringData["Gathered"].Split('-');
            //    _sprite.AddAnimation(WorldObjAnimEnum.Gathered, startX, startY, _iWidth, _iHeight, int.Parse(gatherSplit[0]), float.Parse(gatherSplit[1]));
            //}
            SetSpritePos(_vMapPosition);
        }

        public virtual void Update(GameTime gTime) {
            _sprite.Update(gTime);
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }

        public virtual bool IntersectsWith(Rectangle r)
        {
            return CollisionBox.Intersects(r);
        }

        public virtual bool Contains(Point m)
        {
            return CollisionBox.Contains(m);
        }

        protected void SetSpritePos(Vector2 position)
        {
            if (_sprite != null)
            {
                _sprite.Position = position;
            }
        }
        public virtual void SnapPositionToGrid(Vector2 position)
        {
            _vMapPosition = Util.SnapToGrid(position);
            SetSpritePos(_vMapPosition);
        }
        public void SetCoordinates(Vector2 position)
        {
            _vMapPosition = position;
            SetSpritePos(_vMapPosition);
        }

        /// <summary>
        /// If the given RHTile is not present in the list of Tiles, add it
        /// </summary>
        /// <param name="t">The Tile to add to the list of known RHTiles</param>
        public void AddTile(RHTile t)
        {
            if (!Tiles.Contains(t))
            {
                Tiles.Add(t);
            }
        }

        /// <summary>
        /// Removes the object from the Tiles this Object sits upon
        /// </summary>
        public void RemoveSelfFromTiles()
        {
            foreach (RHTile t in Tiles)
            {
                if (t.Flooring == this) { t.RemoveFlooring(); }
                if (t.WorldObject == this) { t.RemoveWorldObject(); }
                if (t.ShadowObject == this) { t.RemoveShadowObject(); }
            }
        }

        public void ReadItemDrops(string itemDrop)
        {
            string[] split = itemDrop.Split('-');
            int id = int.Parse(split[0]);
            int num = 1;

            if (split.Length == 2) { num = int.Parse(split[1]); }
            _kvpDrop = new KeyValuePair<int, int>(id, num);
        }

        public List<Item> GetDroppedItems()
        {
            List<Item> itemList = new List<Item>();
            for (int i = 0; i < _kvpDrop.Value; i++)
            {
                itemList.Add(DataManager.GetItem(_kvpDrop.Key, 1));
            }

            return itemList;
        }

        public bool CompareType(ObjectTypeEnum t) { return Type == t; }
        public virtual bool CanPickUp() { return false; }
    }

    public class Destructible : WorldObject
    {
        protected int _iHP;
        public int HP => _iHP;

        protected ToolEnum _eToolType;
        public ToolEnum WhichTool => _eToolType;

        protected int _lvltoDmg;
        public int LvlToDmg => _lvltoDmg;

        public Destructible(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos)
        {
            LoadDictionaryData(stringData);

            _wallObject = false;

            if (stringData.ContainsKey("ItemID")) { ReadItemDrops(stringData["ItemID"]); }

            if (stringData.ContainsKey("Tool")) { _eToolType = Util.ParseEnum<ToolEnum>(stringData["Tool"]);}
            if (stringData.ContainsKey("Hp")) { _iHP = int.Parse(stringData["Hp"]); }
            if (stringData.ContainsKey("ReqLvl")) { _lvltoDmg = int.Parse(stringData["ReqLvl"]); }

            if (stringData.ContainsKey("DestructionAnim"))
            {
                string[] splitString = stringData["DestructionAnim"].Split('-');
                _sprite.AddAnimation(AnimationEnum.KO, int.Parse(splitString[0]), int.Parse(splitString[1]), TileSize, TileSize, int.Parse(splitString[2]), float.Parse(splitString[3]), false, true);
            }
        }
        public override void Update(GameTime gTime)
        {
            base.Update(gTime);
            if(_sprite.Position != _vMapPosition) { _sprite.Position = _vMapPosition; }
            if (_iHP <= 0 && (!_sprite.ContainsAnimation(AnimationEnum.KO) || _sprite.AnimationFinished(AnimationEnum.KO)))
            {
                MapManager.Maps[Tiles[0].MapName].RemoveWorldObject(this);
            }
        }

        public virtual bool DealDamage(int dmg)
        {
            bool rv = false;
            if (_iHP > 0)
            {
                _iHP -= dmg;
                rv = _iHP <= 0;

                if (rv)
                {
                    _bImpassable = false;
                    _sprite.PlayAnimation(AnimationEnum.KO);
                }
            }

            //Nudge the Object in the direction of the 'attack'
            int xMod = 0, yMod = 0;
            if (PlayerManager.World.Facing == DirectionEnum.Left) { xMod = -1; }
            else if (PlayerManager.World.Facing == DirectionEnum.Right) { xMod = 1; }

            if (PlayerManager.World.Facing == DirectionEnum.Up) { yMod = -1; }
            else if (PlayerManager.World.Facing == DirectionEnum.Down) { yMod = 1; }
            
            _sprite.Position = new Vector2(_sprite.Position.X + xMod, _sprite.Position.Y + yMod);

            return rv;
        }
    }

    public class Tree : Destructible
    {
        public override Rectangle CollisionBox { get => new Rectangle((int)MapPosition.X + TileSize, (int)MapPosition.Y + TileSize * 4, TileSize, TileSize); }

        public Tree(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, stringData, pos)
        {
            LoadDictionaryData(stringData);

            _eToolType = ToolEnum.Axe;
        }
    }

    public class EchoNode : Destructible
    {
        public EchoNode(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, stringData, pos)
        {
            LoadDictionaryData(stringData);
            _eToolType = ToolEnum.Lantern;

            string[] gatheredSplit = stringData["Gathered"].Split('-');
            //_sprite.AddAnimation(WorldObjAnimEnum.Gathered, startX + (int.Parse(idleSplit[0]) * TileSize), startY, TileSize, TileSize, int.Parse(gatheredSplit[0]), float.Parse(gatheredSplit[1]));

            //_sprite.SetCurrentAnimation(WorldObjAnimEnum.Idle);
        }

        public override bool DealDamage(int dmg)
        {
            bool rv = false;
            rv = base.DealDamage(dmg);

            if (rv)
            {
                //_sprite.SetCurrentAnimation(WorldObjAnimEnum.Gathered);
            }
            return rv;
        }
    }

    public class CombatHazard : WorldObject
    {
        public enum HazardTypeEnum { Passive, Timed, Triggered };
        readonly HazardTypeEnum _eHazardType;

        int _iInit;
        bool _bDrawOver;
        public int Damage { get; }
        public bool Active { get; private set; }

        public CombatHazard(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos)
        {
            LoadDictionaryData(stringData);
            _eHazardType = Util.ParseEnum<HazardTypeEnum>(stringData["Subtype"]);
            Damage = int.Parse(stringData["Damage"]);
            Util.AssignValue(ref _bDrawOver, "DrawOver", stringData);
            _sprite.SetDepthMod(_bDrawOver ? 1 : -999);

            _iInit = 0;
            if (_eHazardType == HazardTypeEnum.Passive) { Active = true; }
            else { Activate(false); }
        }

        protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            base.LoadSprite(stringData, textureName);
            _sprite.AddAnimation(AnimationEnum.ObjectAction1, _pImagePos.X + TileSize, _pImagePos.Y, _iWidth, _iHeight);
        }

        public bool Charge() {
            bool rv = false;

            _iInit += 3;

            if(_iInit >= 100)
            {
                _iInit = 0;
                Activate(!Active);
            }

            return rv;
        }

        private void Activate(bool value)
        {
            Active = value;
            _sprite.PlayAnimation(value ? AnimationEnum.ObjectAction1 : AnimationEnum.ObjectIdle);
        }

        public bool SubtypeMatch(HazardTypeEnum cmp)
        {
            return _eHazardType == cmp;
        }
    }

    //public class Staircase : WorldObject
    //{
    //    protected string _toMap;
    //    public string ToMap { get => _toMap; }

    //    public Staircase(int id, Vector2 pos, int width, int height) : base(id, pos, width, height)
    //    {
    //        _eObjectType = ObjectTypeEnum.WorldObject;
    //        _wallObject = true;
    //        _sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
    //        _sprite.AddAnimation(AnimationEnum.ObjectIdle, 96, 0, TileSize, TileSize);
    //    }

    //    public void SetExit(string map)
    //    {
    //        _toMap = map;
    //    }
    //}

    public class Gatherable : WorldObject
    {
        int _iItemID;

        public Gatherable(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos)
        {
            Util.AssignValue(ref _iItemID, "ItemID", stringData);
            LoadDictionaryData(stringData);
        }

        public void Gather()
        {
            InventoryManager.AddToInventory(DataManager.GetItem(_iItemID));
            MapManager.RemoveWorldObject(this);
            RemoveSelfFromTiles();
        }

        public override bool CanPickUp()
        {
            return true;
        }
    }

    /// <summary>
    /// WorldItems represent WorldObjects that are created by placing down items
    /// </summary>
    public abstract class WorldItem : WorldObject
    {
        protected int _iBaseItemID;
        public int BaseItemID => _iBaseItemID;

        protected string _sMapName;                                 //Used to play sounds on that map
        public Vector2 HeldItemPos
        {
            get { return _vMapPosition; }
            set
            {
                _vMapPosition = value;
                if (_sprite != null)
                {
                    _sprite.Position = _vMapPosition;// new Vector2(_sprite.Width > TileSize ? value.X - (_sprite.Width - TileSize) / 2 : value.X, (_sprite.Height > TileSize) ? value.Y - (_sprite.Height - TileSize) : value.Y);
                }
            }
        }

        public override Rectangle CollisionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y + (_iHeight - BaseHeight), BaseWidth, BaseHeight);

        protected WorldItem() : base() {}
        protected WorldItem(int id, Vector2 pos) : base(id, pos) {}

        public override void SnapPositionToGrid(Vector2 position)
        {
            base.SnapPositionToGrid(position);
            HeldItemPos = _vMapPosition;
        }
        public void SetMapName(string val) { _sMapName = val; }

        public class ClassChanger : WorldItem
        {
            public ClassChanger(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos)
            {
                LoadDictionaryData(stringData);
                _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
                _sprite.Drawing = true;
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                _sprite.Draw(spriteBatch, true);
            }

            public void ProcessClick()
            {
                int currID = PlayerManager.World.CharacterClass.ID;
                int toSet = (currID < DataManager.GetClassCount() - 1) ? (PlayerManager.World.CharacterClass.ID + 1) : 1;
                PlayerManager.SetClass(toSet);
            }

            public MachineData SaveData() { return new MachineData(); }
            public virtual void LoadData(MachineData mac) { }
        }

        public abstract class Machine : WorldItem
        {
            private MachineTypeEnum _eMachineType;

            protected Dictionary<int, int> _diReqToMake;
            public Dictionary<int, int> RequiredToMake => _diReqToMake;

            readonly string _sEffectWorking;

            protected double _dProcessedTime = 0;
            int _iCurrentlyMaking = -1;

            protected int _iWorkingFrames = 2;
            protected float _fFrameSpeed = 0.3f;
            protected ItemBubble _itemBubble;
            protected Item _heldItem;

            public Machine(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos - new Vector2(0, TileSize))
            {
                _heldItem = null;

                Util.AssignValue(ref _iBaseItemID, "ItemID", stringData);

                if (stringData.ContainsKey("Work"))
                {
                    string[] split = stringData["Work"].Split('-');
                    _iWorkingFrames = int.Parse(split[0]);
                    _fFrameSpeed = float.Parse(split[1]);
                }

                if (stringData.ContainsKey("Base"))
                {
                    string[] baseStr = stringData["Base"].Split('-');
                    _iBaseWidth = int.Parse(baseStr[0]) * TileSize;
                    _iBaseHeight = int.Parse(baseStr[1]) * TileSize;
                }

                _diReqToMake = new Dictionary<int, int>();
                if (stringData.ContainsKey("ReqItems"))
                {
                    //Split by "|" for each item set required
                    string[] split = Util.FindParams(stringData["ReqItems"]);
                    foreach (string s in split)
                    {
                        string[] splitData = s.Split('-');
                        _diReqToMake[int.Parse(splitData[0])] = int.Parse(splitData[1]);
                    }
                }

                LoadDictionaryData(stringData);
                Util.AssignValue(ref _sEffectWorking, "WorkEffect", stringData);
            }

            protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = "Textures\\texMachines")
            {
                _sprite = new AnimatedSprite(@"Textures\texMachines");
                _sprite.AddAnimation(AnimationEnum.ObjectIdle, (int)_pImagePos.X, (int)_pImagePos.Y, _iWidth, _iHeight, 1, 0.3f, false);
                _sprite.AddAnimation(AnimationEnum.PlayAnimation, (int)_pImagePos.X + _iWidth, (int)_pImagePos.Y, _iWidth, _iHeight, _iWorkingFrames, _fFrameSpeed, false);
                _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
                _sprite.Drawing = true;

                SetSpritePos(_vMapPosition);
            }

            public override void Update(GameTime gTime)
            {
                _itemBubble?.Update(gTime);
            }
            public override void Draw(SpriteBatch spriteBatch)
            {
                _sprite.Draw(spriteBatch);
                _itemBubble?.Draw(spriteBatch);
            }

            public virtual bool StartAutoWork() { return false; }

            public virtual void MakeChosenItem(int itemID)
            {
                _iCurrentlyMaking = itemID;
                _sprite.PlayAnimation(AnimationEnum.PlayAnimation);
            }
            public virtual void SetHeldItem(int itemID)
            {
                SoundManager.StopEffect(this);
                SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition, this);
                _heldItem = DataManager.GetItem(itemID);
                _dProcessedTime = 0;
                _iCurrentlyMaking = -1;
                _sprite.PlayAnimation(AnimationEnum.ObjectIdle);

                _itemBubble = new ItemBubble(_heldItem, this);
            }
            public void TakeFinishedItem()
            {
                InventoryManager.AddToInventory(_heldItem);
                _heldItem = null;
                _itemBubble = null;
            }

            public virtual void Rollover() { }

            public bool MakingSomething() { return _iCurrentlyMaking != -1; }
            public bool HasItem() { return _heldItem != null; }

            public bool IsProcessor() { return _eMachineType == MachineTypeEnum.Processer; }
            public bool IsCraftingMachine() { return _eMachineType == MachineTypeEnum.CraftingMachine; }

            //public MachineData SaveData()
            //{
            //    MachineData m = new MachineData
            //    {
            //        ID = this.ID,
            //        x = (int)this.MapPosition.X,
            //        y = (int)this.MapPosition.Y,
            //        processedTime = this.ProcessedTime,
            //        currentItemID = (this.CurrentlyProcessing == null) ? _iCurrentlyMaking : this.CurrentlyProcessing.Input,
            //        heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID
            //    };

            //    return m;
            //}
            //public void LoadData(MachineData mac)
            //{
            //    _iID = mac.ID;
            //    SnapPositionToGrid(new Vector2(mac.x, mac.y));
            //    _dProcessedTime = mac.processedTime;
            //    _iCurrentlyMaking = mac.currentItemID;
            //    _heldItem = DataManager.GetItem(mac.heldItemID);

            //    if (CurrentlyProcessing != null) { _sprite.PlayAnimation(AnimationEnum.ObjectIdle); }
            //}

            public class Processor : Machine
            {
                //Processor variables
                Dictionary<int, ProcessRecipe> _diProcessing;
                ProcessRecipe CurrentlyProcessing => (_diProcessing.ContainsKey(_iCurrentlyMaking) ? _diProcessing[_iCurrentlyMaking] : null);

                public Processor(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, stringData, pos)
                {
                    _eMachineType = MachineTypeEnum.Processer;
                    _diProcessing = new Dictionary<int, ProcessRecipe>();

                    //Read in what items the machine processes
                    string[] processes = Util.FindParams(stringData["Processes"]);
                    foreach (string recipe in processes)
                    {
                        string[] pieces = recipe.Split('-');
                        _diProcessing.Add(int.Parse(pieces[0]), new ProcessRecipe(pieces));
                    }
                }

                public override void Update(GameTime gTime)
                {
                    base.Update(gTime);
                    if (CurrentlyProcessing != null)
                    {
                        SoundManager.PlayEffectAtLoc(_sEffectWorking, _sMapName, MapPosition, this);
                        _sprite.Update(gTime);
                        _dProcessedTime += gTime.ElapsedGameTime.TotalSeconds;
                        if (_dProcessedTime >= CurrentlyProcessing.ProcessingTime)
                        {
                            SetHeldItem(CurrentlyProcessing.Output);
                        }
                    }
                }

                public override bool StartAutoWork()
                {
                    bool rv = false;

                    Item itemToProcess = InventoryManager.GetCurrentItem();
                    if (itemToProcess != null)
                    {
                        if (_diProcessing.ContainsKey(itemToProcess.ItemID))
                        {
                            ProcessRecipe pr = _diProcessing[itemToProcess.ItemID];
                            if (itemToProcess.Number >= pr.InputNum)
                            {
                                rv = true;
                                itemToProcess.Remove(pr.InputNum);
                                _iCurrentlyMaking = pr.Input;
                                _sprite.PlayAnimation(AnimationEnum.PlayAnimation);
                            }
                        }
                    }

                    return rv;
                }

                public override void Rollover() { }
            }

            public class CraftingMachine : Machine
            {
                public int AutomatedItem { get; } = -1;
                public Dictionary<int, int> CraftingDictionary { get; }
                private bool _bWorking = false;

                public CraftingMachine(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, stringData, pos)
                {
                    _eMachineType = MachineTypeEnum.CraftingMachine;
                    CraftingDictionary = new Dictionary<int, int>();

                    if (stringData.ContainsKey("Makes"))
                    {
                        //Read in what items the machine can make
                        string[] processes = Util.FindParams(stringData["Makes"]);
                        foreach (string recipe in processes)
                        {
                            //Each entry is in written like ID-NumDays
                            string[] pieces = recipe.Split('-');
                            CraftingDictionary.Add(int.Parse(pieces[0]), int.Parse(pieces[1]));
                        }
                    }
                }

                public override void Update(GameTime gTime)
                {
                    base.Update(gTime);
                    if (_iCurrentlyMaking != -1)       //Crafting Handling
                    {
                        SoundManager.PlayEffectAtLoc(_sEffectWorking, _sMapName, MapPosition, this);
                        _sprite.Update(gTime);
                    }
                }

                public override bool StartAutoWork()
                {
                    bool rv = true;

                    if (AutomatedItem == -1)
                    {
                        GUIManager.OpenMainObject(new HUDCraftingDisplay(this));
                    }

                    return rv;
                }

                public override void MakeChosenItem(int itemID)
                {
                    base.MakeChosenItem(itemID);
                    _bWorking = true;
                }
                public override void SetHeldItem(int itemID)
                {
                    base.SetHeldItem(itemID);
                    _bWorking = false;
                }

                public void SetToWork()
                {
                    if (!_bWorking)
                    {
                        _bWorking = true;
                        PlayerManager.DecreaseStamina(2);
                        _sprite.PlayAnimation(AnimationEnum.PlayAnimation);
                    }
                }

                public override void Rollover()
                {
                    if (_bWorking)
                    {
                        _dProcessedTime++;
                        if (_dProcessedTime >= CraftingDictionary[_iCurrentlyMaking])
                        {
                            SetHeldItem(_iCurrentlyMaking);
                        }

                        _bWorking = false;
                        _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
                    }
                }
            }

            private class ProcessRecipe
            {
                public int Input { get; private set; }
                public int InputNum { get; private set; }
                public int Output { get; private set; }
                public int ProcessingTime { get; private set; }

                public ProcessRecipe(string[] data)
                {
                    Input = int.Parse(data[0]);
                    ProcessingTime = int.Parse(data[1]);

                    Item processedItem = DataManager.GetItem(Input);
                    Output = processedItem.RefinesInto.Key;
                    InputNum = processedItem.RefinesInto.Value;
                }
            }

            public class ItemBubble : WorldObject
            {
                private const int MAX_POSITIONS = 3;
                private const double TICK_TIMER = 0.3;
                private Item _item;
                private double _dTimer;
                private Vector2[] _vArrPositions;
                private int _iCurrentPosition;
                private bool _bDec;

                public ItemBubble(Item it, Machine myMachine)
                {
                    _item = it;

                    _iWidth = TileSize * 2;
                    _iHeight = TileSize * 2;
                    _sprite = new AnimatedSprite(DataManager.DIALOGUE_TEXTURE);
                    _sprite.AddAnimation(AnimationEnum.ObjectIdle, 16, 80, _iWidth, _iHeight);

                    _bDec = false;
                    _dTimer = 0;
                    _iCurrentPosition = 0;

                    SetCoordinates(myMachine._vMapPosition + new Vector2((myMachine.Width / 2) - (_iWidth / 2), -_iHeight));

                    _vArrPositions = new Vector2[3];
                    _vArrPositions[0] = new Vector2(_vMapPosition.X, _vMapPosition.Y + 1);
                    _vArrPositions[1] = new Vector2(_vMapPosition.X, _vMapPosition.Y);
                    _vArrPositions[2] = new Vector2(_vMapPosition.X, _vMapPosition.Y - 1);
                }

                public override void Draw(SpriteBatch spriteBatch)
                {
                    _sprite.Draw(spriteBatch, 99998);
                    if (_item != null)
                    {
                        _item.Draw(spriteBatch, new Rectangle((int)(_vMapPosition.X + 8), (int)(_vMapPosition.Y + 8), TileSize, TileSize), true);
                    }
                }

                public override void Update(GameTime gTime)
                {
                    _dTimer += gTime.ElapsedGameTime.TotalSeconds;
                    if(_dTimer >= TICK_TIMER)
                    {
                        _dTimer = 0;

                        if(_iCurrentPosition == MAX_POSITIONS - 1)
                        {
                            _bDec = true;
                            _iCurrentPosition--;
                        }
                        else if (_iCurrentPosition == 0) {
                            _bDec = false;
                            _iCurrentPosition++;
                        }
                        else
                        {
                            if (_bDec) { _iCurrentPosition--; }
                            else { _iCurrentPosition++; }
                        }

                        _vMapPosition = _vArrPositions[_iCurrentPosition];
                        _sprite.Position = _vMapPosition;
                    }
                }
            }
        }

        public class Container : WorldItem
        {
            int _iRows;
            public int Rows { get => _iRows; }
            int _iColumns;
            public int Columns { get => _iColumns; }

            Item[,] _inventory;
            public Item[,] Inventory { get => _inventory; }

            public Container(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos)
            {
                LoadDictionaryData(stringData);

                _iRows = int.Parse(stringData["Rows"]);
                _iColumns = int.Parse(stringData["Cols"]);

                _inventory = new Item[_iRows, _iColumns];
            }

            internal ContainerData SaveData()
            {
                ContainerData containerData = new ContainerData
                {
                    containerID = this.ID,
                    rows = _iRows,
                    cols = _iColumns,
                    x = (int)this.MapPosition.X,
                    y = (int)this.MapPosition.Y
                };

                containerData.Items = new List<ItemData>();
                foreach (Item i in (this.Inventory))
                {
                    ItemData itemData = Item.SaveData(i);
                    containerData.Items.Add(itemData);
                }
                return containerData;
            }
            internal void LoadData(ContainerData data)
            {
                SnapPositionToGrid(new Vector2(data.x, data.y));
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        ItemData item = data.Items[i * InventoryManager.maxItemRows + j];
                        Item newItem = GetItem(item.itemID, item.num);
                        if (newItem != null) { newItem.ApplyUniqueData(item.strData); }

                        InventoryManager.InitContainerInventory(this.Inventory);
                        InventoryManager.AddItemToInventorySpot(newItem, i, j, false);
                        InventoryManager.ClearExtraInventory();
                    }
                }
            }
        }

        public class Plant : WorldItem
        {
            #region consts
            const float MAX_ROTATION = 0.15f;
            const float ROTATION_MOD = 0.02f;
            const float MAX_BOUNCE = 3;
            #endregion

            public override Rectangle CollisionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y + _iHeight - TileSize, TileSize, TileSize);

            bool _bShaking = false;
            DirectionEnum dir = DirectionEnum.Right;
            float _fCurrentRotation = 0f;
            int _iBounceCount = 0;

            readonly bool _bPopItem;
            int _iCurrentState;
            readonly int _iMaxStates;
            readonly int _iResourceID;
            int _iDaysLeft;
            Dictionary<int, int> _diTransitionTimes;

            public Plant(int id, Dictionary<string, string> stringData) : base(id, Vector2.Zero)
            {
                _diTransitionTimes = new Dictionary<int, int>();

                LoadDictionaryData(stringData);
                _bImpassable = false;

                _iCurrentState = 0;

                Util.AssignValue(ref _iResourceID, "Item", stringData);
                Util.AssignValue(ref _iMaxStates, "TrNum", stringData); //Number of growth phases

                _bPopItem = false;

                //The amount of time for each phase
                string[] dayStr = stringData["TrTime"].Split('-');
                for (int j = 0; j < _iMaxStates - 1; j++)
                {
                    _diTransitionTimes.Add(j, int.Parse(dayStr[j]));
                }
                _iDaysLeft = _diTransitionTimes[0];

                _sprite.SetRotationOrigin(new Vector2(_iWidth / 2, _iHeight - 1));    //Subtract one to keep it in the bounds of the rectangle
            }

            protected override void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
            {
                _sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
                _sprite.AddAnimation(0.ToString(), (int)_pImagePos.X, (int)_pImagePos.Y, _iWidth, _iHeight);
                for (int j = 1; j < _diTransitionTimes.Count + 1; j++){
                    _sprite.AddAnimation(j.ToString(), (int)_pImagePos.X + (TileSize * j), (int)_pImagePos.Y, _iWidth, _iHeight);
                }
            }

            public override void Update(GameTime gTime)
            {
                //If the object is shaking, we need to determine what step it's in
                if (_bShaking)
                {
                    if (dir == DirectionEnum.Right) { _fCurrentRotation += ROTATION_MOD;}
                    else if (dir == DirectionEnum.Left) { _fCurrentRotation -= ROTATION_MOD; }

                    _sprite.SetRotationAngle(_fCurrentRotation);

                    //If we've reached the end of our bounce, increment the bounce count
                    //and set us to just below the trigger value for the statement we just hit.
                    if (_iBounceCount == MAX_BOUNCE && _fCurrentRotation >= - ROTATION_MOD && _fCurrentRotation <= ROTATION_MOD)
                    {
                        _bShaking = false;
                        _iBounceCount = 0;
                    }
                    else if (_fCurrentRotation >= MAX_ROTATION)
                    {
                        dir = DirectionEnum.Left;
                        _iBounceCount++;
                        _fCurrentRotation = MAX_ROTATION - ROTATION_MOD;
                    }
                    else if (_fCurrentRotation <= -MAX_ROTATION)
                    {
                        dir = DirectionEnum.Right;
                        _iBounceCount++;
                        _fCurrentRotation = -MAX_ROTATION + ROTATION_MOD;
                    }
                }
                _sprite.Update(gTime);
            }

            /// <summary>
            /// Tell the object to shake
            /// </summary>
            public void Shake()
            {
                if (!_bShaking)
                {
                    if (PlayerManager.World.CollisionBox.Center.X > CollisionBox.Center.X) { dir = DirectionEnum.Left; }
                    else if (PlayerManager.World.CollisionBox.Center.X < CollisionBox.Center.X) { dir = DirectionEnum.Right; }
                    _bShaking = true;
                }
            }

            /// <summary>
            /// On rollover, increase the plant's growth cycle if it has been watered.
            /// </summary>
            public void Rollover()
            {
                if (Tiles[0].IsWatered()) {
                    if (_iDaysLeft > 0) //Decrement the number of days until the next phase
                    {
                        _iDaysLeft--;
                    }
                    else if(!FinishedGrowing()) //If it hasn't finished growing, and there'sno days left, go to the next phase
                    {
                        _iCurrentState++;
                        _sprite.PlayAnimation(_iCurrentState.ToString());
                        if (_diTransitionTimes.ContainsKey(_iCurrentState))
                        {
                            _iDaysLeft = _diTransitionTimes[_iCurrentState];
                        }
                    }
                }
            }
            /// <summary>
            /// Check if the plant has finished growing or not.
            /// </summary>
            /// <returns>True if it's on the last phase</returns>
            public bool FinishedGrowing() { return _iCurrentState == _iMaxStates-1; }

            /// <summary>
            /// Call to tell the plant that it is being Harvested, and follow any logic
            /// that needs to happen for this to occur.
            /// 
            /// Can only Harvest plants that are finished growing.
            /// </summary>
            public void Harvest()
            {
                Item it = null;
                if (FinishedGrowing())
                {
                    it = DataManager.GetItem(_iResourceID);
                    if (_bPopItem)
                    {
                        it.Pop(MapPosition);
                    }
                    else
                    {
                        InventoryManager.AddToInventory(it);
                    }

                    MapManager.RemoveWorldObject(this);
                    RemoveSelfFromTiles();
                }
            }

            public void FinishGrowth()
            {
                _iCurrentState = _iMaxStates - 1;
                //_rSource.X += _iWidth * _iCurrentState;
            }

            public override bool CanPickUp()
            {
                return FinishedGrowing();
            }

            internal PlantData SaveData()
            {
                PlantData plantData = new PlantData
                {
                    ID = _iID,
                    x = (int)MapPosition.X,
                    y = (int)this.MapPosition.Y,
                    currentState = _iCurrentState,
                    daysLeft = _iDaysLeft
                };

                return plantData;
            }

            internal void LoadData(PlantData data)
            {
                SnapPositionToGrid(new Vector2(data.x, data.y));
                _iCurrentState = data.currentState;
                _iDaysLeft = data.daysLeft;

                _sprite.PlayAnimation(_iCurrentState.ToString());
            }
        }

        public class AdjustableObject : WorldItem
        {
            //This is used for subtypes that have different sprites.
            //Like the Earth which has a watered and unwatered Sprite
            protected virtual AnimatedSprite Target => _sprite;

            public AdjustableObject() : base(){}
            public AdjustableObject(int id, Vector2 pos) : base(id, pos) { }

            /// <summary>
            /// Loads in the different sprite versions required for an AdjustableObject
            /// so that they can be easily played and referenced in the future.
            /// </summary>
            /// <param name="sprite">The AnimatedSprite to load the animations into</param>
            /// <param name="vStart">The source position for this texture series</param>
            protected void LoadAdjustableSprite(ref AnimatedSprite spr, string textureName = DataManager.FILE_FLOORING)
            {
                spr = new AnimatedSprite(textureName);
                spr.AddAnimation("None", (int)_pImagePos.X, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("NS", (int)_pImagePos.X + TileSize, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("EW", (int)_pImagePos.X + TileSize * 2, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("SW", (int)_pImagePos.X + TileSize * 3, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("NW", (int)_pImagePos.X + TileSize * 4, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("NE", (int)_pImagePos.X + TileSize * 5, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("SE", (int)_pImagePos.X + TileSize * 6, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("NSE", (int)_pImagePos.X + TileSize * 7, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("NSW", (int)_pImagePos.X + TileSize * 8, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("NEW", (int)_pImagePos.X + TileSize * 9, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("SEW", (int)_pImagePos.X + TileSize * 10, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("NSEW", (int)_pImagePos.X + TileSize * 11, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("W", (int)_pImagePos.X + TileSize * 12, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("E", (int)_pImagePos.X + TileSize * 13, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("S", (int)_pImagePos.X + TileSize * 14, (int)_pImagePos.Y, _iWidth, _iHeight);
                spr.AddAnimation("N", (int)_pImagePos.X + TileSize * 15, (int)_pImagePos.Y, _iWidth, _iHeight);
            }

            /// <summary>
            /// Adjusts the source rectangle for the AdjustableObject compared to nearby AdjustableObjects
            /// 
            /// First thing to do is to determine how many AdjustableObjects are adjacent to the AdjustableObject and where, we then
            /// create a string in NSEW order to determine which segment we need to get.
            /// 
            /// Then compare the string and determine which piece it corresponds to.
            /// 
            /// Finally, run this method again on each of the adjacent AdjustableObjects to update their appearance
            /// </summary>
            /// <param name="adjustAdjacent">Whether or not to call this method against the adjacent tiles</param>
            public void AdjustObject(bool adjustAdjacent = true)
            {
                List<RHTile> liAdjacentTiles = new List<RHTile>();
                RHTile startTile = Tiles[0];

                string sAdjacent = string.Empty;

                //Create the adjacent tiles string
                MakeAdjustments("N", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sMapName].GetTileByGridCoords(new Point((int)(startTile.X), (int)(startTile.Y - 1))));
                MakeAdjustments("S", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sMapName].GetTileByGridCoords(new Point((int)(startTile.X), (int)(startTile.Y + 1))));
                MakeAdjustments("E", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sMapName].GetTileByGridCoords(new Point((int)(startTile.X + 1), (int)(startTile.Y))));
                MakeAdjustments("W", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sMapName].GetTileByGridCoords(new Point((int)(startTile.X - 1), (int)(startTile.Y))));

                Target.PlayAnimation(string.IsNullOrEmpty(sAdjacent) ? "None" : sAdjacent);

                //Find all matching objects in the adjacent tiles and call
                //this method without recursion on them.
                if (adjustAdjacent)
                {
                    foreach (RHTile t in liAdjacentTiles)
                    {
                        AdjustableObject obj = null;
                        if(MatchingObjectTest(t, ref obj))
                        {
                            obj.AdjustObject(false);
                        }
                    }
                }
            }

            /// <summary>
            /// If the given tile passes the AdjustableObject test, add the if Valdid
            /// string to the adjacency string then add to the list
            /// </summary>
            /// <param name="ifValid">Which directional value to add on a pass</param>
            /// <param name="adjacentStr">Ref to the adjacency string</param>
            /// <param name="adjacentList">ref to the adjacency list</param>
            /// <param name="tile">The tile to test</param>
            protected void MakeAdjustments(string ifValid, ref string adjacentStr, ref List<RHTile> adjacentList, RHTile tile)
            {
                if (MatchingObjectTest(tile))
                {
                    adjacentStr += ifValid;
                    adjacentList.Add(tile);
                }
            }

            /// <summary>
            /// Helper for the MatchingObjectTest method for when we don't care about the object
            /// </summary>
            /// <param name="tile">Tile to test against</param>
            /// <returns>True if the tile exists and contains a matching AdjustableObject</returns>
            protected virtual bool MatchingObjectTest(RHTile tile) {
                AdjustableObject obj = null;
                return MatchingObjectTest(tile, ref obj);
            }

            /// <summary>
            /// Check to see that the tile exists, has an AdjustableObject and that AdjustableObject matches the initial type
            /// </summary>
            /// <param name="tile">Tile to test against</param>
            /// <param name="obj">Reference to any AdjustableObject that may be found</param>
            /// <returns>True if the tile exists and contains a matching AdjustableObject</returns>
            protected virtual bool MatchingObjectTest(RHTile tile, ref AdjustableObject obj) { return false; }
        }

        public class Floor : AdjustableObject
        {
            public Floor() : base() { }

            /// <summary>
            /// Base Constructor to hard define the Height and Width
            /// </summary>
            public Floor(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos)
            {
                LoadDictionaryData(stringData, false);
                LoadAdjustableSprite(ref _sprite, DataManager.FILE_FLOORING);

                _eObjectType = ObjectTypeEnum.Floor;
            }

            /// <summary>
            /// Overriding because weneed to set the Depth to 0 for drawing since
            /// this is a floor object and needs to beon the bottom.
            /// </summary>
            public override void Draw(SpriteBatch spriteBatch)
            {
                Target.Draw(spriteBatch, 0);
            }

            /// <summary>
            /// Check to see that the tile exists, has an AdjustableObject and that AdjustableObject matches the initial type
            /// </summary>
            /// <param name="tile">Tile to test against</param>
            /// <returns>True if the tile exists and contains a Wall</returns>
            protected override bool MatchingObjectTest(RHTile tile, ref AdjustableObject obj)
            {
                bool rv = false;

                if (tile != null)
                {
                    obj = tile.GetFloorObject();
                    if (obj != null && obj.Type == Type)
                    {
                        rv = true;
                    }
                }

                return rv;
            }

            internal FloorData SaveData()
            {
                FloorData floorData = new FloorData
                {
                    ID = _iID,
                    x = (int)MapPosition.X,
                    y = (int)MapPosition.Y
                };

                return floorData;
            }
            internal void LoadData(FloorData data)
            {
                _iID = data.ID;
                SnapPositionToGrid(new Vector2(data.x, data.y));
            }

            public class Earth : Floor
            {
                protected override AnimatedSprite Target => _bWatered ? _sprWatered : _sprite;

                AnimatedSprite _sprWatered;
                bool _bWatered;

                public Earth()
                {
                    _iID = 0;
                    _eObjectType = ObjectTypeEnum.Earth;
                    _pImagePos = Point.Zero;

                    LoadAdjustableSprite(ref _sprite);
                    _pImagePos.Y += TileSize;

                    LoadAdjustableSprite(ref _sprWatered);
                    _pImagePos.Y -= TileSize;

                    Watered(false);
                }

                public override void SnapPositionToGrid(Vector2 position)
                {
                    base.SnapPositionToGrid(position);
                    _sprWatered.Position = position;
                }

                public void Watered(bool value)
                {
                    _bWatered = value;
                    _sprWatered.PlayAnimation(_sprite.CurrentAnimation.ToString());
                }
                public bool Watered() { return _bWatered; }
            }
        }

        /// <summary>
        /// Wall object that can adjust themselves based off of other, adjacent walls
        /// </summary>
        public class Wall : AdjustableObject
        {
            public Wall(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos)
            {
                LoadDictionaryData(stringData, false);
                LoadAdjustableSprite(ref _sprite, DataManager.FILE_WORLDOBJECTS);
            }

            /// <summary>
            /// Check to see that the tile exists, has an AdjustableObject and that AdjustableObject matches the initial type
            /// </summary>
            /// <param name="tile">Tile to test against</param>
            /// <returns>True if the tile exists and contains a Wall</returns>
            protected override bool MatchingObjectTest(RHTile tile, ref AdjustableObject obj)
            {
                bool rv = false;

                if (tile != null)
                {
                    WorldObject wObj = tile.GetWorldObject(false);
                    if (wObj != null && wObj.Type == Type)
                    {
                        obj = (AdjustableObject)wObj;
                        rv = true;
                    }
                }

                return rv;
            }
        }

        public class Light : WorldItem
        {
            public Light(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos)
            {
                LoadDictionaryData(stringData);

                string[] idleSplit = stringData["Idle"].Split('-');
                _sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _iWidth, _iHeight, int.Parse(idleSplit[0]), float.Parse(idleSplit[1]));
                _sprite.PlayAnimation(AnimationEnum.ObjectIdle);
                _sprite.Drawing = true;
            }
        }
    }

    public abstract class TriggerObject : WorldObject
    {
        #region constants
        const string MATCH_TRIGGER = "MatchTrigger";
        const string TRIGGER_NUMBER = "TriggerNumber";
        const string ITEM_KEY_ID = "ItemKeyID";
        const string OUT_TRIGGER = "OutTrigger";

        #endregion

        enum DungeonObjectType { Trigger, Door };
        readonly DungeonObjectType _eSubType;
        readonly string _sOutTrigger;   //What trigger response is sent
        protected string _sMatchTrigger; //What, if anything, the object responds to
        protected int _iTriggerNumber = 1;
        protected int _iTriggersLeft = 1;
        bool _bVisible = true;
        readonly int _iItemKeyID = -1;
        bool _bHasBeenTriggered = false;

        protected TriggerObject(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, pos)
        {
            LoadDictionaryData(stringData);
            _eSubType = Util.ParseEnum<DungeonObjectType>(stringData["Subtype"]);

            Util.AssignValue(ref _sOutTrigger, OUT_TRIGGER, stringData);
            Util.AssignValue(ref _sMatchTrigger, MATCH_TRIGGER, stringData);
            Util.AssignValue(ref _iTriggerNumber, TRIGGER_NUMBER, stringData);
            Util.AssignValue(ref _iItemKeyID, ITEM_KEY_ID, stringData);

            _iTriggersLeft = _iTriggerNumber;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_bVisible)
            {
                base.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// This method is called when the player interacts with the object.
        /// </summary>
        public virtual void Interact() { }

        /// <summary>
        /// This method is called when something attempts to trigger it
        /// </summary>
        /// <param name="name">The name of the trigger</param>
        public virtual void AttemptToTrigger(string name) {}

        /// <summary>
        /// Call to see if the object will be triggered by the sent trigger
        /// </summary>
        /// <param name="triggerName">The trigger name to match against the response trigger</param>
        /// <returns>True if theo object can trigger</returns>
        private bool CanTrigger(string triggerName)
        {
            bool rv = false;
            if(triggerName == _sMatchTrigger)
            {
                rv = CanTrigger();
            }
             
            return rv;
        }

        /// <summary>
        /// Call to see if the object can trigger. Only valid if the object hasn't been triggered
        /// and the object hasno more remaining trigger numbers to wait on
        /// </summary>
        /// <returns>True if the object can trigger</returns>
        private bool CanTrigger()
        {
            bool rv = false;
            if (!_bHasBeenTriggered && UpdateTriggerNumber())
            {
                rv = true;
            }

            return rv;
        }

        /// <summary>
        /// This method is called to trigger the object and make it send its trigger
        /// </summary>
        public virtual void FireTrigger() { }

        /// <summary>
        /// Call this to reset the DungeonObject to its original state.
        /// </summary>
        public virtual void Reset() { }

        /// <summary>
        /// Given an item type, check it against the key for the DungeonObject
        /// </summary>
        /// <param name="item">The Item to check against</param>
        /// <returns>True if the item is the key</returns>
        public bool CheckForKey(Item item)
        {
            bool rv = false;
            if (_iItemKeyID == item.ItemID)
            {
                rv = true;
                item.Remove(1);
            }

            return rv;
        }

        /// <summary>
        /// This checks whether or not the object should trigger
        /// </summary>
        /// <returns>Returns true if the object can trigger</returns>
        private bool UpdateTriggerNumber()
        {
            if (_iTriggersLeft > 0)
            {
                _iTriggersLeft--;
            }

            return _iTriggersLeft == 0;
        }

        public class Trigger : TriggerObject
        {
            string _sSoundEffect;
            Item _item;
            public Trigger(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, stringData, pos)
            {
                _item = DataManager.GetItem(_iItemKeyID);

                Util.AssignValue(ref _sSoundEffect, "SoundEffect", stringData);

                if (_iItemKeyID == -1)
                {
                    _sprite.AddAnimation(AnimationEnum.ObjectAction1, _pImagePos.X + Width, _pImagePos.Y, _iWidth, _iHeight);
                }
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                base.Draw(spriteBatch);
                if(_item != null)
                {
                    float visibility = _bHasBeenTriggered ? 1f : 0.25f;
                    _item.Draw(spriteBatch, new Rectangle((int)(_vMapPosition.X), (int)(_vMapPosition.Y - 6), TileSize, TileSize), true, _sprite.LayerDepth + 1, visibility);
                }
            }

            /// <summary>
            /// Called when the player interacts with the object.
            /// 
            /// If it's already triggered, do nothing.
            /// </summary>
            public override void Interact()
            {
                if (!_bHasBeenTriggered)
                {
                    //If there's an itemKeyID, display appropriate text
                    if (_iItemKeyID != -1)
                    {
                        GUIManager.OpenMainObject(new HUDInventoryDisplay());
                    }
                    else {
                        SoundManager.PlayEffectAtLoc(_sSoundEffect, MapName, MapPosition, this);
                        FireTrigger();
                    }
                }
            }

            public override void AttemptToTrigger(string name)
            {
                if (CanTrigger(name))
                {
                    FireTrigger();
                }
            }

            public override void FireTrigger()
            {
                if(CanTrigger())
                {
                    _bHasBeenTriggered = true;
                    _sprite.PlayAnimation(AnimationEnum.ObjectAction1);
                    GameManager.ActivateTriggers(_sOutTrigger);
                }
            }
        }

        public class Door : TriggerObject
        {
            public override Rectangle CollisionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y + (_iHeight - BaseHeight), BaseWidth, BaseHeight);

            readonly bool _bKeyDoor;
            public Door(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, stringData, pos)
            {
                if (stringData.ContainsKey("Base"))
                {
                    string[] split = stringData["Base"].Split('-');
                    _iBaseWidth = TileSize * int.Parse(split[0]);
                    _iBaseHeight = TileSize * int.Parse(split[1]);
                }

                if (stringData.ContainsKey("KeyDoor"))
                {
                    _bKeyDoor = true;
                    _sMatchTrigger = GameManager.KEY_OPEN;
                }
            }

            /// <summary>
            /// When a door is triggered, it becomes passable and invisible.
            /// </summary>
            /// <param name="name"></param>
            public override void AttemptToTrigger(string name)
            {
                if (CanTrigger(name))
                {
                    if (!string.IsNullOrEmpty(_sOutTrigger))
                    {
                        GameManager.ActivateTriggers(_sOutTrigger);
                    }
                    _bHasBeenTriggered = true;
                    _bImpassable = false;
                    _bVisible = false;
                }
            }

            /// <summary>
            /// When triggered, makes doors impassable again
            /// </summary>
            public override void Reset()
            {
                _bImpassable = true;
                _bVisible = true;
                _iTriggersLeft = _iTriggerNumber;
            }

            /// <summary>
            /// Handles the response from whent he player attempts to Interact with the Door object.
            /// Primarily just handles the output for the doors and the type of triggers required to use it.
            /// </summary>
            public override void Interact()
            {
                if (_bKeyDoor)
                {
                    if (DungeonManager.DungeonKeys() > 0)
                    {
                        DungeonManager.UseDungeonKey();
                        AttemptToTrigger(KEY_OPEN);
                    }
                    else
                    {
                        GUIManager.OpenTextWindow(DataManager.GetGameText("Key_Door"));
                    }
                }
                else if (_iItemKeyID != -1)
                {
                    GUIManager.OpenMainObject(new HUDInventoryDisplay());
                }
                else
                {
                    GUIManager.OpenTextWindow(DataManager.GetGameText("Trigger_Door"));
                }
            }
        }
    }
}
