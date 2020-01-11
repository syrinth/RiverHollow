using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using RiverHollow.Game_Managers;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using static RiverHollow.Game_Managers.ObjectManager;
using RiverHollow.GUIObjects;
using System;
using RiverHollow.SpriteAnimations;
using RiverHollow.Misc;
using RiverHollow.GUIComponents.GUIObjects;

namespace RiverHollow.WorldObjects
{
    public class WorldObject
    {
        #region Properties
        public static int Rock = 0;
        public static int BigRock = 1;
        public static int Tree = 2;
        public enum ObjectType { Building, ClassChanger, Machine, Container, Door, Earth, Floor, WorldObject, Destructible, Plant, Forageable, Wall, Light};
        public ObjectType Type;

        protected AnimatedSprite _sprite;
        public AnimatedSprite Sprite => _sprite;

        public List<RHTile> Tiles;
        public List<RHTile> ShadowTiles;

        protected bool _bImpassable = true;
        public bool Blocking => _bImpassable;
        protected bool _wallObject;
        public bool WallObject => _wallObject;

        protected KeyValuePair<int, int> _kvpDrop; //itemID, # of items dropped

        protected Vector2 _vMapPosition;
        public virtual Vector2 MapPosition
        {
            get { return _vMapPosition; }
            set {
                Vector2 norm = Util.SnapToGrid(value);
                _vMapPosition = norm;
                if (_sprite != null)
                {
                    _sprite.Position = _vMapPosition;
                }
            }
        }

        public Rectangle ClickBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _iWidth, _iHeight);                  //ClickBox is always hard set
        public virtual Rectangle CollisionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _iWidth, _iHeight);      //Can be overriden to only be the base

        protected int _iWidth;
        public int Width => _iWidth;
        protected int _iHeight;
        public int Height => _iHeight;

        protected int _iBaseWidth = TileSize;
        public virtual int BaseWidth => _iBaseWidth;
        protected int _iBaseHeight = TileSize;
        public virtual int BaseHeight => _iBaseHeight;

        protected int _id;
        public int ID { get => _id; }
        #endregion

        protected WorldObject() {
            Tiles = new List<RHTile>();
            ShadowTiles = new List<RHTile>();
        }

        public WorldObject(int id, Vector2 pos, int width, int height) : this()
        {
            Type = ObjectType.WorldObject;
            _id = id;
            _vMapPosition = pos;
            _iWidth = width;
            _iHeight = height;
            _wallObject = false;
        }

        protected virtual void LoadSprite(Dictionary<string, string> stringData, string textureName = GameContentManager.FILE_WORLDOBJECTS)
        {
            string[] texIndices = stringData["Image"].Split('-');
            int startX = int.Parse(texIndices[0]);
            int startY = int.Parse(texIndices[1]);

            _sprite = new AnimatedSprite(textureName);
            if (stringData.ContainsKey("Idle"))
            {
                string[] idleSplit = stringData["Idle"].Split('-');
                _sprite.AddAnimation(WorldObjAnimEnum.Idle, startX, startY, _iWidth, _iHeight, int.Parse(idleSplit[0]), float.Parse(idleSplit[1]));
            }
            else
            {
                _sprite.AddAnimation(WorldObjAnimEnum.Idle, startX, startY, _iWidth, _iHeight);
            }

            if (stringData.ContainsKey("Gathered"))
            {
                string[] gatherSplit = stringData["Gathered"].Split('-');
                _sprite.AddAnimation(WorldObjAnimEnum.Gathered, startX, startY, _iWidth, _iHeight, int.Parse(gatherSplit[0]), float.Parse(gatherSplit[1]));
            }
            _sprite.Position = _vMapPosition;
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

        public virtual void SetCoordinatesByGrid(Vector2 position)
        {
            MapPosition = Util.SnapToGrid(position);
        }
        public virtual void SetCoordinates(Vector2 position)
        {
            MapPosition = position;
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
                itemList.Add(ObjectManager.GetItem(_kvpDrop.Key, _kvpDrop.Value));
            }

            return itemList;
        }

        public bool IsContainer() { return Type == ObjectType.Container; }
        public bool IsDestructible() { return Type == ObjectType.Destructible; }
        public bool IsMachine() { return Type == ObjectType.Machine; }
        public bool IsPlant() { return Type == ObjectType.Plant; }
        public bool IsWorldObject() { return Type == ObjectType.WorldObject; }
        public bool IsGround() { return Type == ObjectType.Floor; }
        public bool IsEarth() { return Type == ObjectType.Earth; }
        public bool IsDoor() { return Type == ObjectType.Door; }
        public bool IsClassChanger() { return Type == ObjectType.ClassChanger; }
        public bool IsBuilding() { return Type == ObjectType.Building; }
        public bool IsForageable() { return Type == ObjectType.Forageable; }
    }

    public class Forageable : WorldObject
    {
        public override Rectangle CollisionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y + _iHeight - TileSize, TileSize, TileSize);
        const float MAX_ROTATION = 0.4f;
        const float ROTATION_MOD = 0.09f;
        bool _bShaking = false;
        float _fCurrentRotation = 0f;
        int _iBounceCount = 0;

        public int ForageItem => _kvpDrop.Key;

        public Forageable(int id, Dictionary<string, string> stringData, Vector2 pos)
        {
            Type = ObjectType.Forageable;
            _id = id;

             _wallObject = false;

            _iWidth = int.Parse(stringData["Width"]);
            _iHeight = int.Parse(stringData["Height"]);

            if (stringData.ContainsKey("Item")) { ReadItemDrops(stringData["Item"]); }
            if (stringData.ContainsKey("Passable")) { _bImpassable = false; }

            SetCoordinates(pos);
            LoadSprite(stringData);

            string[] texIndices = stringData["Image"].Split('-');
            int startX = int.Parse(texIndices[0]);
            int startY = int.Parse(texIndices[1]);
            _sprite.SetRotationOrigin(new Vector2(_iWidth / 2, _iHeight - 1));    //Subtract one to keep it in the bounds of the rectangle
        }

        public override void SetCoordinatesByGrid(Vector2 position)
        {
            base.SetCoordinatesByGrid(position);
            _sprite.Position = _vMapPosition;
        }

        public override void Update(GameTime gTime)
        {
            //If the object is shaking, we need to determine what step it's in
            if (_bShaking)
            {
                //We start out going right
                if (_iBounceCount == 0)
                {
                    _fCurrentRotation += ROTATION_MOD;
                }
                else if (_iBounceCount == 1) //After one bounce, we go left
                {
                    _fCurrentRotation -= ROTATION_MOD;
                }
                else   //After we've bounced twice, stop bouncing completely
                {
                    _bShaking = false;
                    _iBounceCount = 0;
                    _fCurrentRotation = 0;
                }
                _sprite.SetRotationAngle(_fCurrentRotation);

                //If we've reached the end of our bounce, increment the bounce count
                //and set us to just below the trigger value for the statement we just hit.
                if (_fCurrentRotation >= MAX_ROTATION)
                {
                    _fCurrentRotation = MAX_ROTATION - ROTATION_MOD;
                    _iBounceCount++;
                }
                else if (_fCurrentRotation <= -MAX_ROTATION)
                {
                    _fCurrentRotation = MAX_ROTATION  + ROTATION_MOD;
                    _iBounceCount++;
                }
            }
            _sprite.Update(gTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }

        /// <summary>
        /// Tell the object to shake
        /// </summary>
        public void Shake()
        {
            _bShaking = true;
        }
    }

    public class Destructible : WorldObject
    {
        protected int _iHP;
        public int HP => _iHP;

        protected ToolEnum _eToolType;
        public ToolEnum WhichTool => _eToolType;

        protected int _lvltoDmg;
        public int LvlToDmg => _lvltoDmg;

        public Destructible(int id, Dictionary<string, string> stringData, Vector2 pos)
        {
            Type = ObjectType.Destructible;
            _id = id;

            _wallObject = false;

            _iWidth = int.Parse(stringData["Width"]);
            _iHeight = int.Parse(stringData["Height"]);
            if (stringData.ContainsKey("Item")) { ReadItemDrops(stringData["Item"]); }

            if (stringData.ContainsKey("Tool")) { _eToolType = Util.ParseEnum<ToolEnum>(stringData["Tool"]);}
            if (stringData.ContainsKey("Hp")) { _iHP = int.Parse(stringData["Hp"]); }
            if (stringData.ContainsKey("ReqLvl")) { _lvltoDmg = int.Parse(stringData["ReqLvl"]); }

            SetCoordinates(pos);
            LoadSprite(stringData);
        }

        public virtual bool DealDamage(int dmg)
        {
            bool rv = false;
            _iHP -= dmg;
            rv = _iHP <= 0;

            return rv;
        }
    }

    public class Tree : Destructible
    {
        public override Rectangle CollisionBox { get => new Rectangle((int)MapPosition.X + TileSize, (int)MapPosition.Y + TileSize * 4, TileSize, TileSize); }

        public Tree(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, stringData, pos)
        {
            Type = ObjectType.Destructible;
            if (stringData.ContainsKey("Texture")) {
                LoadSprite(stringData, stringData["Texture"]);
            }
            _eToolType = ToolEnum.Axe;
        }
    }

    public class EchoNode : Destructible
    {
        public EchoNode(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, stringData, pos)
        {
            Type = ObjectType.Destructible;
            _eToolType = ToolEnum.Lantern;
            _sprite = new AnimatedSprite(GameContentManager.FILE_WORLDOBJECTS);

            string[] imageSplit = stringData["Image"].Split('-');
            string[] idleSplit = stringData["Idle"].Split('-');
            string[] gatheredSplit = stringData["Gathered"].Split('-');

            int startX = int.Parse(imageSplit[0]);
            int startY = int.Parse(imageSplit[1]);
            _sprite.AddAnimation(WorldObjAnimEnum.Idle, startX, startY, TileSize, TileSize, int.Parse(idleSplit[0]), float.Parse(idleSplit[1]));
            _sprite.AddAnimation(WorldObjAnimEnum.Gathered, startX + (int.Parse(idleSplit[0]) * TileSize), startY, TileSize, TileSize, int.Parse(gatheredSplit[0]), float.Parse(gatheredSplit[1]));

            _sprite.SetCurrentAnimation(WorldObjAnimEnum.Idle);
            _sprite.IsAnimating = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }

        public override void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
        }

        public override bool DealDamage(int dmg)
        {
            bool rv = false;
            rv = base.DealDamage(dmg);

            if (rv)
            {
                _sprite.SetCurrentAnimation(WorldObjAnimEnum.Gathered);
            }
            return rv;
        }
    }

    public class Light : WorldItem
    {
        public Light(int id, Dictionary<string, string> stringData, Vector2 pos)
        {
            Type = ObjectType.Light;
            _sprite = new AnimatedSprite(GameContentManager.FILE_WORLDOBJECTS);

            _iHeight = TileSize;
            _iWidth = TileSize;
            string[] imageSplit = stringData["Image"].Split('-');
            string[] idleSplit = stringData["Idle"].Split('-');

            int startX = int.Parse(imageSplit[0]);
            int startY = int.Parse(imageSplit[1]);
            _sprite.AddAnimation(WorldObjAnimEnum.Idle, startX, startY, TileSize, TileSize, int.Parse(idleSplit[0]), float.Parse(idleSplit[1]));

            _sprite.SetCurrentAnimation(WorldObjAnimEnum.Idle);
            _sprite.IsAnimating = true;
        }
    }

    public class Door : WorldObject
    {
        public enum EnumDoorType { Mob, Key, Season};
        public EnumDoorType DoorType;
        bool _bVisible;

        private Door(Vector2 pos, int width, int height) : base(-1, pos, width, height)
        {
            Type = ObjectType.Door;
            _bVisible = true;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_bVisible)
            {
                base.Draw(spriteBatch);
            }
        }

        public virtual void ReadInscription() { }

        public bool IsMobDoor() { return DoorType == EnumDoorType.Mob; }
        public bool IsKeyDoor() { return DoorType == EnumDoorType.Key; }
        public bool IsSeasonDoor() { return DoorType == EnumDoorType.Season; }

        public class MobDoor : Door
        {
            public MobDoor(Vector2 pos, int width, int height) : base(pos, width, height)
            {
                DoorType = EnumDoorType.Mob;
                _sprite = new AnimatedSprite(GameContentManager.FILE_WORLDOBJECTS);
                _sprite.AddAnimation(WorldObjAnimEnum.Idle, 64, 0, 16, 32);
            }

            public override void ReadInscription() {
                GUIManager.OpenTextWindow(GameContentManager.GetGameText("MobDoor"));
               // GUIManager.SetScreen(new TextScreen(GameContentManager.GetGameText("MobDoor"), false));
            }

            public void Check(int mobCount)
            {
                if (mobCount == 0)
                {
                    _bImpassable = false;
                    _bVisible = false;
                }
            }
        }
        public class KeyDoor : Door
        {
            private int _iKeyID = 0;
            public KeyDoor(Vector2 pos, int width, int height) : base(pos, width, height)
            {
                DoorType = EnumDoorType.Key;
                _sprite = new AnimatedSprite(GameContentManager.FILE_WORLDOBJECTS);
                _sprite.AddAnimation(WorldObjAnimEnum.Idle, 64, 0, 16, 32);
            }

            public void SetKey(int value)
            {
                _iKeyID = value;
            }

            public override void ReadInscription()
            {
                GUIManager.OpenTextWindow(GameContentManager.GetGameText("KeyDoor"));
                //GUIManager.SetScreen(new TextScreen(this, GameContentManager.GetGameText("KeyDoor")));
            }

            public bool Check(Item item)
            {
                bool rv = false;
                if (_iKeyID == item.ItemID)
                {
                    rv = true;
                    item.Remove(1);
                    _bImpassable = false;
                    _bVisible = false;
                }

                return rv;
            }
        }
        public class SeasonDoor : Door
        {
            private string _sSeason = "";
            public SeasonDoor(Vector2 pos, int width, int height) : base(pos, width, height)
            {
                DoorType = EnumDoorType.Season;
                _sprite = new AnimatedSprite(GameContentManager.FILE_WORLDOBJECTS);
                _sprite.AddAnimation(WorldObjAnimEnum.Idle, 64, 0, 16, 32);
            }

            public void SetSeason(string value)
            {
                _sSeason = value;
            }

            public override void ReadInscription()
            {
                GUIManager.OpenTextWindow(GameContentManager.GetGameText("SpringDoor"));
                //GUIManager.SetScreen(new TextScreen(GameContentManager.GetGameText("SpringDoor"), false));
            }

            public bool Check()
            {
                bool rv = false;
                bool unlocked = _sSeason == GameCalendar.GetSeason();

                rv = unlocked;
                _bImpassable = !unlocked;
                _bVisible = !unlocked;

                return rv;
            }
        }
    }

    public class Staircase : WorldObject
    {
        protected string _toMap;
        public string ToMap { get => _toMap; }

        public Staircase(int id, Vector2 pos, int width, int height) : base(id, pos, width, height)
        {
            Type = ObjectType.WorldObject;
            _wallObject = true;
            _sprite = new AnimatedSprite(GameContentManager.FILE_WORLDOBJECTS);
            _sprite.AddAnimation(WorldObjAnimEnum.Idle, 96, 0, TileSize, TileSize);
        }

        public void SetExit(string map)
        {
            _toMap = map;
        }
    }

    public class WorldItem : WorldObject
    {
        protected string _sName;                                 //Used to play sounds on that map
        protected Vector2 _vSourcePos;
        public override Vector2 MapPosition
        {
            set
            {
                base.MapPosition = value;
                HeldItemPos = _vMapPosition;
            }
        }
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

        protected void ReadSourcePos(string str)
        {
            string[] strPos = str.Split('-');
            _vSourcePos = new Vector2(int.Parse(strPos[0]), int.Parse(strPos[1]));
        }
        public void SetMapName(string val) { _sName = val; }

        public class ClassChanger : WorldItem
        {
            public ClassChanger(int id, Vector2 position)
            {
                _id = id;
                Type = ObjectType.ClassChanger;
                LoadContent();

                MapPosition = position;

                _iWidth = TileSize;
                _iHeight = TileSize * 2;
            }
            public void LoadContent()
            {
                _sprite = new AnimatedSprite(@"Textures\texMachines");
                _sprite.AddAnimation(WorldObjAnimEnum.Idle, (int)_vSourcePos.X, (int)_vSourcePos.Y, TileSize, TileSize * 2, 1, 0.3f);
                _sprite.SetCurrentAnimation(WorldObjAnimEnum.Idle);
                _sprite.IsAnimating = true;
            }
            public virtual void Update(GameTime gTime) { }
            public override void Draw(SpriteBatch spriteBatch)
            {
                _sprite.Draw(spriteBatch, true);
            }

            public void ProcessClick()
            {
                int currID = PlayerManager.World.CharacterClass.ID;
                int toSet = (currID < ObjectManager.GetClassCount() - 1) ? (PlayerManager.World.CharacterClass.ID + 1) : 1;
                PlayerManager.SetClass(toSet);
            }

            public MachineData SaveData() { return new MachineData(); }
            public virtual void LoadData(GameManager.MachineData mac) { }
        }

        public class Machine : WorldItem
        {
            //Processor variables
            Dictionary<int, ProcessRecipe> _diProcessing;
            ProcessRecipe CurrentlyProcessing => (_diProcessing.ContainsKey(_iCurrentlyMaking) ? _diProcessing[_iCurrentlyMaking] : null);

            //Crafter variables
            Dictionary<int, int> _diCrafting;
            public Dictionary<int, int> CraftList => _diCrafting;
            int _iCurrentlyMaking = -1;

            protected int _iWorkingFrames = 2;
            protected float _fFrameSpeed = 0.3f;
            protected ItemBubble _itemBubble;
            protected Item _heldItem;
            protected double _dProcessedTime;
            public double ProcessedTime => _dProcessedTime;

            public Machine(int id, Dictionary<string, string> stringData)
            {
                _id = id;
                _heldItem = null;
                _dProcessedTime = -1;
                _diCrafting = new Dictionary<int, int>();
                _diProcessing = new Dictionary<int, ProcessRecipe>();
                Type = ObjectType.Machine;
                ReadSourcePos(stringData["Image"]);

                _iWidth = int.Parse(stringData["Width"]);
                _iHeight = int.Parse(stringData["Height"]);                

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

                //Read in what items the machine processes
                if (stringData.ContainsKey("Processes"))
                {
                    string[] processes = stringData["Processes"].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string recipe in processes)
                    {
                        string[] pieces = recipe.Split('-');
                        _diProcessing.Add(int.Parse(pieces[0]), new ProcessRecipe(pieces));
                    }
                }

                //Read in what items the machine can make
                if (stringData.ContainsKey("Makes"))
                {
                    string[] processes = stringData["Makes"].Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string recipe in processes)
                    {
                        string[] pieces = recipe.Split('-');
                        _diCrafting.Add(int.Parse(pieces[0]), int.Parse(pieces[1]));
                    }
                }

                LoadContent();
            }
            public void LoadContent()
            {
                _sprite = new AnimatedSprite(@"Textures\texMachines");
                _sprite.AddAnimation(WorldObjAnimEnum.Idle, (int)_vSourcePos.X, (int)_vSourcePos.Y, _iWidth, _iHeight, 1, 0.3f, true);
                _sprite.AddAnimation(WorldObjAnimEnum.Working, (int)_vSourcePos.X + _iWidth, (int)_vSourcePos.Y, _iWidth, _iHeight, _iWorkingFrames, _fFrameSpeed, true);
                _sprite.SetCurrentAnimation(WorldObjAnimEnum.Idle);
                _sprite.IsAnimating = true;
            }
            public override void Update(GameTime gTime) {
                if (_itemBubble != null)
                {
                    _itemBubble.Update(gTime);
                }

                //Processing Handling
                if (CurrentlyProcessing != null)
                {
                    _sprite.Update(gTime);
                    _dProcessedTime += gTime.ElapsedGameTime.TotalSeconds;
                    if (_dProcessedTime >= CurrentlyProcessing.ProcessingTime)
                    {
                        SetHeldItem(CurrentlyProcessing.Output);
                    }
                }
                else if (_iCurrentlyMaking != -1)       //Crafting Handling
                {
                    _sprite.Update(gTime);
                    _dProcessedTime += gTime.ElapsedGameTime.TotalSeconds;
                    if (_dProcessedTime >= _diCrafting[_iCurrentlyMaking])
                    {
                        SetHeldItem(_iCurrentlyMaking);
                    }
                }
            }
            public override void Draw(SpriteBatch spriteBatch)
            {
                _sprite.Draw(spriteBatch);
                if(_itemBubble != null)
                {
                    _itemBubble.Draw(spriteBatch);
                }
            }

            public void SetHeldItem(int itemID)
            {
                SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sName, MapPosition);
                _heldItem = ObjectManager.GetItem(itemID);
                _dProcessedTime = -1;
                _iCurrentlyMaking = -1;
                _sprite.SetCurrentAnimation(WorldObjAnimEnum.Idle);

                _itemBubble = new ItemBubble(_heldItem, this);
            }

            public bool Working() { return _iCurrentlyMaking != -1; }
            public void ProcessClick()
            {
                bool Processed = false;
                Item itemToProcess = InventoryManager.GetCurrentItem();
                if (itemToProcess != null)
                {
                    if (_diProcessing.ContainsKey(itemToProcess.ItemID))
                    {
                        ProcessRecipe pr = _diProcessing[itemToProcess.ItemID];
                        if (itemToProcess.Number >= pr.InputNum)
                        {
                            Processed = true;
                            itemToProcess.Remove(pr.InputNum);
                            _iCurrentlyMaking = pr.Input;
                            _sprite.SetCurrentAnimation(WorldObjAnimEnum.Working);
                        }
                    }
                }

                if (!Processed) {
                    GUIManager.OpenMainObject(new HUDCraftingDisplay(this));
                }
            }
            public bool HasItem() { return _heldItem != null; }
            public void TakeFinishedItem()
            {
                InventoryManager.AddToInventory(_heldItem);
                _heldItem = null;
                _itemBubble = null;
            }

            public void MakeChosenItem(int itemID)
            {
                _iCurrentlyMaking = itemID;
                _sprite.SetCurrentAnimation(WorldObjAnimEnum.Working);
            }

            public virtual int GetProcessingItemId() { return -1; }

            public MachineData SaveData()
            {
                MachineData m = new MachineData
                {
                    ID = this.ID,
                    x = (int)this.MapPosition.X,
                    y = (int)this.MapPosition.Y,
                    processedTime = this.ProcessedTime,
                    currentItemID = (this.CurrentlyProcessing == null) ? _iCurrentlyMaking : this.CurrentlyProcessing.Input,
                    heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID
                };

                return m;
            }
            public void LoadData(GameManager.MachineData mac)
            {
                _id = mac.ID;
                MapPosition = new Vector2(mac.x, mac.y);
                _dProcessedTime = mac.processedTime;
                _iCurrentlyMaking = mac.currentItemID;
                _heldItem = ObjectManager.GetItem(mac.heldItemID);

                if (CurrentlyProcessing != null) { _sprite.SetCurrentAnimation(WorldObjAnimEnum.Working); }
            }

            private class ProcessRecipe
            {
                int _iInput;
                public int Input => _iInput;
                int _iReqInput;
                public int InputNum => _iReqInput;
                int _iOutput;
                public int Output => _iOutput;
                int _iProcessingTime;
                public int ProcessingTime => _iProcessingTime;

                public ProcessRecipe(string[] data)
                {
                    _iInput = int.Parse(data[0]);
                    _iProcessingTime = int.Parse(data[1]);

                    Item processedItem = ObjectManager.GetItem(_iInput);
                    _iOutput = processedItem.RefinesInto.Key;
                    _iReqInput = processedItem.RefinesInto.Value;
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

                //ItemBubble overrides because it isn't locked to the grid
                public override Vector2 MapPosition
                {
                    get { return base.MapPosition; }
                    set {
                        _vMapPosition = value;
                        if (_sprite != null)
                        {
                            _sprite.Position = _vMapPosition;
                        }
                    }
                }

                public ItemBubble(Item it, Machine myMachine)
                {
                    _item = it;

                    _iWidth = TileSize * 2;
                    _iHeight = TileSize * 2;
                    _sprite = new AnimatedSprite(@"Textures\Dialog");
                    _sprite.AddAnimation(WorldObjAnimEnum.Idle, 16, 80, _iWidth, _iHeight);

                    _bDec = false;
                    _dTimer = 0;
                    _iCurrentPosition = 0;

                    MapPosition =  myMachine._vMapPosition + new Vector2((myMachine.Width / 2) - (_iWidth / 2), -_iHeight);

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

            public Container(int id, Dictionary<string, string> stringData)
            {
                _id = id;
                Type = ObjectType.Container;

                _iWidth = int.Parse(stringData["Width"]);
                _iHeight = int.Parse(stringData["Height"]);

                _iRows = int.Parse(stringData["Rows"]);
                _iColumns = int.Parse(stringData["Cols"]);

                ReadSourcePos(stringData["Image"]);
                LoadSprite(stringData);

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
                MapPosition = new Vector2(data.x, data.y);
                for (int i = 0; i < Rows; i++)
                {
                    for (int j = 0; j < Columns; j++)
                    {
                        ItemData item = data.Items[i * InventoryManager.maxItemRows + j];
                        Item newItem = GetItem(item.itemID, item.num);
                        if (newItem != null) { newItem.ApplyUniqueData(item.strData); }

                        InventoryManager.InitContainerInventory(this);
                        InventoryManager.AddItemToInventorySpot(newItem, i, j, false);
                        InventoryManager.ClearExtraInventory();
                    }
                }
            }
        }

        public class Plant : WorldItem
        {
            int _iCurrentState;
            int _iMaxStates;
            int _iResourceID;
            int _iDaysLeft;
            Dictionary<int, int> _diTransitionTimes;

            public Plant(int id, Dictionary<string, string> stringData)
            {
                _id = id;
                Type = ObjectType.Plant;
                _bImpassable = false;

                _iCurrentState = 0;
                _diTransitionTimes = new Dictionary<int, int>();

                _iWidth = TileSize;
                _iHeight = TileSize;

                ReadSourcePos(stringData["Image"]);
                _iResourceID = int.Parse(stringData["Item"]);
                _iMaxStates = int.Parse(stringData["TrNum"]);       //Number of growth phases

                //The amount of time for each phase
                string[] dayStr = stringData["TrTime"].Split('-');
                for (int j = 0; j < _iMaxStates - 1; j++)
                {
                    _diTransitionTimes.Add(j, int.Parse(dayStr[j]));
                }
                _iDaysLeft = _diTransitionTimes[0];

                LoadContent();
            }

            public void LoadContent()
            {
                _sprite = new AnimatedSprite(GameContentManager.FILE_WORLDOBJECTS);
                _sprite.AddAnimation(0.ToString(), (int)_vSourcePos.X, (int)_vSourcePos.Y, _iWidth, _iHeight);
                for (int j = 1; j < _diTransitionTimes.Count; j++){
                    _sprite.AddAnimation(j.ToString(), (int)_vSourcePos.X + (TileSize * j), (int)_vSourcePos.Y, _iWidth, _iHeight);
                }
            }

            public void Rollover()
            {
                if (Tiles[0].IsWatered()) {
                    if (_iDaysLeft > 0)
                    {
                        _iDaysLeft--;
                    }
                    else
                    {
                        _iCurrentState++;
                        _sprite.SetCurrentAnimation(_iCurrentState.ToString());
                        if (_diTransitionTimes.ContainsKey(_iCurrentState))
                        {
                            _iDaysLeft = _diTransitionTimes[_iCurrentState];
                        }
                    }
                }
            }
            public bool FinishedGrowing() { return _iCurrentState == _iMaxStates-1; }
            public Item Harvest(bool pop)
            {
                Item it = null;
                if (FinishedGrowing())
                {
                    it = ObjectManager.GetItem(_iResourceID);
                    if (pop)
                    {
                        it.Pop(MapPosition);
                    }
                    else
                    {
                        InventoryManager.AddToInventory(it);
                    }
                }
                return it;
            }

            public void FinishGrowth()
            {
                _iCurrentState = _iMaxStates - 1;
                //_rSource.X += _iWidth * _iCurrentState;
            }

            internal PlantData SaveData()
            {
                PlantData plantData = new PlantData
                {
                    ID = _id,
                    x = (int)MapPosition.X,
                    y = (int)this.MapPosition.Y,
                    currentState = _iCurrentState,
                    daysLeft = _iDaysLeft
                };

                return plantData;
            }

            internal void LoadData(PlantData data)
            {
                MapPosition = new Vector2(data.x, data.y);
                _iCurrentState = data.currentState;
                _iDaysLeft = data.daysLeft;

                _sprite.SetCurrentAnimation(_iCurrentState.ToString());
            }
        }

        public class AdjustableObject : WorldItem
        {
            //This is used for subtypes that have different sprites.
            //Like the Earth which has a watered and unwatered Sprite
            protected virtual AnimatedSprite Target => _sprite;

            /// <summary>
            /// Loads in the different sprite versions required for an AdjustableObject
            /// so that they can be easily played and referenced in the future.
            /// </summary>
            /// <param name="sprite">The AnimatedSprite to load the animations into</param>
            /// <param name="vStart">The source position for this texture series</param>
            protected void LoadSprite(AnimatedSprite sprite, Vector2 vStart)
            {
                sprite.AddAnimation("None", (int)_vSourcePos.X, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("NS", (int)_vSourcePos.X + TileSize, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("EW", (int)_vSourcePos.X + TileSize * 2, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("SW", (int)_vSourcePos.X + TileSize * 3, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("NW", (int)_vSourcePos.X + TileSize * 4, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("NE", (int)_vSourcePos.X + TileSize * 5, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("SE", (int)_vSourcePos.X + TileSize * 6, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("NSE", (int)_vSourcePos.X + TileSize * 7, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("NSW", (int)_vSourcePos.X + TileSize * 8, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("NEW", (int)_vSourcePos.X + TileSize * 9, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("SEW", (int)_vSourcePos.X + TileSize * 10, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("NSEW", (int)_vSourcePos.X + TileSize * 11, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("W", (int)_vSourcePos.X + TileSize * 12, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("E", (int)_vSourcePos.X + TileSize * 13, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("S", (int)_vSourcePos.X + TileSize * 14, (int)_vSourcePos.Y, _iWidth, _iHeight);
                sprite.AddAnimation("N", (int)_vSourcePos.X + TileSize * 15, (int)_vSourcePos.Y, _iWidth, _iHeight);
                
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
                MakeAdjustments("N", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sName].GetTileByGridCoords(new Point((int)(startTile.X), (int)(startTile.Y - 1))));
                MakeAdjustments("S", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sName].GetTileByGridCoords(new Point((int)(startTile.X), (int)(startTile.Y + 1))));
                MakeAdjustments("E", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sName].GetTileByGridCoords(new Point((int)(startTile.X + 1), (int)(startTile.Y))));
                MakeAdjustments("W", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sName].GetTileByGridCoords(new Point((int)(startTile.X - 1), (int)(startTile.Y))));

                Target.SetCurrentAnimation(string.IsNullOrEmpty(sAdjacent) ? "None" : sAdjacent);

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
            /// <summary>
            /// Base Constructor to hard define the Height and Width
            /// </summary>
            protected Floor() {
                _iWidth = TileSize;
                _iHeight = TileSize;
            }

            public Floor(int id, Dictionary<string, string> stringData, Vector2 pos) : this()
            {
                _id = id;
                Type = ObjectType.Floor;
                ReadSourcePos(stringData["Image"]);

                _sprite = new AnimatedSprite(GameContentManager.FILE_FLOORING);
                LoadSprite(_sprite, _vSourcePos);

                MapPosition = pos;
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
                    ID = _id,
                    x = (int)MapPosition.X,
                    y = (int)MapPosition.Y
                };

                return floorData;
            }
            internal void LoadData(FloorData data)
            {
                _id = data.ID;
                MapPosition = new Vector2(data.x, data.y);
            }

            public class Earth : Floor
            {
                protected override AnimatedSprite Target => _bWatered ? _sprWatered : _sprite;
                public override Vector2 MapPosition
                {
                    set
                    {
                        base.MapPosition = value;
                        _sprWatered.Position = _vMapPosition;
                    }
                }

                AnimatedSprite _sprWatered;
                bool _bWatered;

                public Earth()
                {
                    _id = 0;
                    Type = ObjectType.Earth;
                    _vSourcePos = Vector2.Zero;

                    _sprite = new AnimatedSprite(GameContentManager.FILE_FLOORING);
                    LoadSprite(_sprite, _vSourcePos);

                    _sprWatered = new AnimatedSprite(GameContentManager.FILE_FLOORING);
                    LoadSprite(_sprWatered, new Vector2(_vSourcePos.X, _vSourcePos.Y + TileSize));

                    Watered(false);
                }

                public void Watered(bool value)
                {
                    _bWatered = value;
                    _sprWatered.SetCurrentAnimation(_sprite.CurrentAnimation);
                }
                public bool Watered() { return _bWatered; }
            }
        }

        /// <summary>
        /// Wall object that can adjust themselves based off of other, adjacent walls
        /// </summary>
        public class Wall : AdjustableObject
        {
            public Wall(int id, Dictionary<string, string> stringData, Vector2 pos)
            {
                _id = id;
                Type = ObjectType.Wall;
                ReadSourcePos(stringData["Image"]);

                _iWidth = int.Parse(stringData["Width"]);
                _iHeight = int.Parse(stringData["Height"]);

                _sprite = new AnimatedSprite(GameContentManager.FILE_WORLDOBJECTS);
                LoadSprite(_sprite, _vSourcePos);

                MapPosition = pos;
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
    }
}
