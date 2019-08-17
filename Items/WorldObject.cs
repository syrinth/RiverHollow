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
        public enum ObjectType { Building, ClassChanger, Machine, Container, Door, Earth, Floor, WorldObject, Destructible, Plant, Forageable, Wall};
        public ObjectType Type;

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
            set { _vMapPosition = value; }
        }

        protected Rectangle _rSource;
        public Rectangle SourceRectangle => _rSource;

        public Rectangle ClickBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _iWidth, _iHeight);                  //ClickBox is always hard set
        public virtual Rectangle CollisionBox => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _iWidth, _iHeight);      //Can be overriden to only be the base

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

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

        public WorldObject(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height) : this()
        {
            Type = ObjectType.WorldObject;
            _id = id;
            _vMapPosition = pos;
            _iWidth = width;
            _iHeight = height;
            _texture = tex;
            _wallObject = false;

            _rSource = sourceRectangle;
        }

        public virtual void Update(GameTime theGameTime) {}

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_vMapPosition.X, (int)_vMapPosition.Y, _iWidth, _iHeight), _rSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, _vMapPosition.Y + _iHeight + (_vMapPosition.X / 100));
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

        public void RemoveSelfFromTiles()
        {
            foreach (RHTile t in Tiles)
            {
                t.RemoveWorldObject();
                t.RemoveShadowObject();
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
        AnimatedSprite _sprite;
        public int ForageItem => _kvpDrop.Key;

        public Forageable(int id, Dictionary<string, string> stringData, Vector2 pos)
        {
            Type = ObjectType.Forageable;
            _id = id;

             _wallObject = false;

            int x = 0;
            int y = 0;

            string[] texIndices = stringData["Image"].Split('-');
            x = int.Parse(texIndices[0]);
            y = int.Parse(texIndices[1]);

            _iWidth = int.Parse(stringData["Width"]);
            _iHeight = int.Parse(stringData["Height"]);

            if (stringData.ContainsKey("Item")) { ReadItemDrops(stringData["Item"]); }
            if (stringData.ContainsKey("Passable")) { _bImpassable = false; }

            _rSource = new Rectangle(x, y, _iWidth, _iHeight);

            _sprite = new AnimatedSprite(@"Textures\worldObjects");
            _sprite.AddAnimation(WorldObjAnimEnum.Idle, _iWidth, _iHeight, 1, 1f, x, y);

            if (stringData.ContainsKey("Shake"))
            {
                string[] frameSplit = stringData["Shake"].Split('-');
                _sprite.AddAnimation(WorldObjAnimEnum.Shake, _iWidth, _iHeight, int.Parse(frameSplit[0]), float.Parse(frameSplit[1]), x, y);
            }

            SetCoordinates(pos);
        }

        public override void SetCoordinatesByGrid(Vector2 position)
        {
            base.SetCoordinatesByGrid(position);
            _sprite.Position = _vMapPosition;
        }

        public override void Update(GameTime theGameTime)
        {
            _sprite.Update(theGameTime);

            if(_sprite.CurrentAnimation == "Shake" && _sprite.GetPlayCount() >= 1) {
                _sprite.SetCurrentAnimation(WorldObjAnimEnum.Idle);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }

        public void Shake()
        {
            _sprite.SetCurrentAnimation(WorldObjAnimEnum.Shake);
        }
    }

    public class Destructible : WorldObject
    {
        protected int _hp;
        public int HP => _hp;

        protected bool _bBreakable;
        public bool Breakable => _bBreakable;

        protected bool _bChoppable;
        public bool Choppable => _bChoppable;

        protected int _lvltoDmg;
        public int LvlToDmg => _lvltoDmg;

        public Destructible(int id, Dictionary<string, string> stringData, Vector2 pos)
        {
            Type = ObjectType.Destructible;
            _id = id;
            _vMapPosition = pos;

            _wallObject = false;

            int x = 0;
            int y = 0;
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            string[] texIndices = stringData["Image"].Split('-');
            x = int.Parse(texIndices[0]);
            y = int.Parse(texIndices[1]);

            _iWidth = int.Parse(stringData["Width"]);
            _iHeight = int.Parse(stringData["Height"]);
            if (stringData.ContainsKey("Item")) { ReadItemDrops(stringData["Item"]); }

            if (stringData.ContainsKey("Chop")) { _bChoppable = true; }
            if (stringData.ContainsKey("Break")) { _bBreakable = true; }
            if (stringData.ContainsKey("Hp")) { _hp = int.Parse(stringData["Hp"]); }
            if (stringData.ContainsKey("ReqLvl")) { _lvltoDmg = int.Parse(stringData["ReqLvl"]); }

            _rSource = new Rectangle(x, y, _iWidth, _iHeight);
        }

        public Destructible(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height, bool breakIt, bool chopIt, int lvl, int hp) : base(id, pos, sourceRectangle, tex, width, height)
        {
            Type = ObjectType.Destructible;

            _hp = hp;
            _bBreakable = breakIt;
            _bChoppable = chopIt;
            _lvltoDmg = lvl;
        }

        public bool DealDamage(int dmg)
        {
            bool rv = false;
            _hp -= dmg;
            rv = _hp <= 0;

            return rv;
        }
    }

    public class Tree : Destructible
    {
        public override Rectangle CollisionBox { get => new Rectangle((int)MapPosition.X + TileSize, (int)MapPosition.Y + TileSize * 4, TileSize, TileSize); }

        public Tree(int id, Dictionary<string, string> stringData, Vector2 pos) : base(id, stringData, pos)
        {
            Type = ObjectType.Destructible;
            if (stringData.ContainsKey("Texture")) { _texture = GameContentManager.GetTexture(stringData["Texture"]); }
            _bChoppable = true;
        }
    }

    public class Door : WorldObject
    {
        public enum EnumDoorType { Mob, Key, Season};
        public EnumDoorType DoorType;
        bool _bVisible;

        private Door(Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height) : base(-1, pos, sourceRectangle, tex, width, height)
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
            public MobDoor(Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height) : base(pos, sourceRectangle, tex, width, height)
            {
                DoorType = EnumDoorType.Mob;
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
            public KeyDoor(Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height) : base(pos, sourceRectangle, tex, width, height)
            {
                DoorType = EnumDoorType.Key;
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
            public SeasonDoor(Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height) : base(pos, sourceRectangle, tex, width, height)
            {
                DoorType = EnumDoorType.Season;
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

        public Staircase(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height) : base(id, pos, sourceRectangle, tex, width, height)
        {
            Type = ObjectType.WorldObject;
            _wallObject = true;
        }

        public void SetExit(string map)
        {
            _toMap = map;
        }
    }

    public class Floor : WorldObject
    {
        protected Vector2 _vSourcePos;

        public Floor()
        {
            Type = ObjectType.Floor;
            _texture = GameContentManager.GetTexture(@"Textures\texFlooring");

            _iWidth = TileSize; ;
            _iHeight = TileSize;
        }

        public Floor(int id)
        {
            _id = id;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_vMapPosition.X, (int)_vMapPosition.Y, _iWidth, _iHeight), _rSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
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
            bool _bWatered;

            public void Watered(bool value) {
                _bWatered = value;
                _rSource = _bWatered ? new Rectangle(TileSize, 0, TileSize, TileSize): new Rectangle(0, 0, TileSize, TileSize);
            }
            public bool Watered() { return _bWatered; }
            public Earth()
            {
                _id = 0;
                Type = ObjectType.Earth;
                Watered(false);
            }
        }   
    }

    public class WorldItem : WorldObject
    {
        protected string _sMapName;                                 //Used to play sounds on that map
        protected Vector2 _vSourcePos;
        protected AnimatedSprite _sprite;
        public override Vector2 MapPosition
        {
            set
            {
                Vector2 norm = Util.SnapToGrid(value);
                _vMapPosition = norm;
                HeldItemPos = norm;
                if (_sprite != null)
                {
                    _sprite.Position = _vMapPosition;
                }
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
        public void SetMapName(string val) { _sMapName = val; }

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
            public virtual void Update(GameTime gameTime) { }
            public override void Draw(SpriteBatch spriteBatch)
            {
                _sprite.Draw(spriteBatch, true);
            }

            public void ProcessClick()
            {
                int currID = PlayerManager.Combat.CharacterClass.ID;
                int toSet = (currID < ObjectManager.GetClassCount() - 1) ? (PlayerManager.Combat.CharacterClass.ID + 1) : 1;
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
            public override void Update(GameTime gameTime) {
                if (_itemBubble != null)
                {
                    _itemBubble.Update(gameTime);
                }

                //Processing Handling
                if (CurrentlyProcessing != null)
                {
                    _sprite.Update(gameTime);
                    _dProcessedTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (_dProcessedTime >= CurrentlyProcessing.ProcessingTime)
                    {
                        SetHeldItem(CurrentlyProcessing.Output);
                    }
                }
                else if (_iCurrentlyMaking != -1)       //Crafting Handling
                {
                    _sprite.Update(gameTime);
                    _dProcessedTime += gameTime.ElapsedGameTime.TotalSeconds;
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
                SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition);
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

                public ItemBubble(Item it, Machine myMachine)
                {
                    _item = it;

                    _texture = GameContentManager.GetTexture(@"Textures\Dialog");
                    _iWidth = TileSize * 2;
                    _iHeight = TileSize * 2;
                    _rSource = new Rectangle(16, 80, _iWidth, _iHeight);

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
                    spriteBatch.Draw(_texture, new Rectangle((int)_vMapPosition.X, (int)_vMapPosition.Y, _iWidth, _iHeight), _rSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, 99990);
                    if (_item != null)
                    {
                        _item.Draw(spriteBatch, new Rectangle((int)(_vMapPosition.X + 8), (int)(_vMapPosition.Y + 8), TileSize, TileSize), true);
                    }
                }

                public override void Update(GameTime gameTime)
                {
                    _dTimer += gameTime.ElapsedGameTime.TotalSeconds;
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

                ReadSourcePos(stringData["Image"]);
                _iRows = int.Parse(stringData["Rows"]);
                _iColumns = int.Parse(stringData["Cols"]);

                LoadContent();

                _inventory = new Item[_iRows, _iColumns];
            }
            public void LoadContent()
            {
                _texture = GameContentManager.GetTexture(@"Textures\worldObjects");
                _rSource = new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, _iWidth, _iHeight);
            }

            internal ContainerData SaveData()
            {
                ContainerData containerData = new ContainerData
                {
                    containerID = this.ID,
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
                for (int i = 0; i < InventoryManager.maxItemRows; i++)
                {
                    for (int j = 0; j < InventoryManager.maxItemColumns; j++)
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

            public override void Draw(SpriteBatch spriteBatch)
            {
                float layerDepth = (_iCurrentState == 0) ? 1 : _vMapPosition.Y + _iHeight +  (_vMapPosition.X / 100);
                spriteBatch.Draw(_texture, new Rectangle((int)_vMapPosition.X, (int)_vMapPosition.Y, _iWidth, _iHeight), _rSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layerDepth);
            }
            public void LoadContent()
            {
                _texture = GameContentManager.GetTexture(@"Textures\worldObjects");
                _rSource = new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, _iWidth, _iHeight);
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
                        _rSource.X += _iWidth;
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

                _rSource.X += _iWidth * _iCurrentState;
            }
        }

        /// <summary>
        /// Wall object that can adjust themselves based off of other, adjacent walls
        /// </summary>
        public class Wall : WorldItem
        {
            public Wall(int id, Dictionary<string, string> stringData)
            {
                _id = id;
                Type = ObjectType.Wall;
                ReadSourcePos(stringData["Image"]);

                _iWidth = int.Parse(stringData["Width"]);
                _iHeight = int.Parse(stringData["Height"]);

                _rSource = new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, _iWidth, _iHeight);

                _texture = GameContentManager.GetTexture(@"Textures\worldObjects");
            }

            /// <summary>
            /// Adjusts the source rectangle for the Wall compared to nearby wall segments
            /// 
            /// First thing to do is to determine how many walls are adjacent to the wall and where, we then
            /// create a string in NSEW order to determine which segment we need to get.
            /// 
            /// Then compare the string and determine which piece it corresponds to.
            /// 
            /// Finally, run this method again on each of the adjacent walls to update their appearance
            /// </summary>
            /// <param name="adjustAdjacent">Whether or not to call this method against the adjacent tiles</param>
            public void AdjustWall(bool adjustAdjacent = true)
            {
                List<RHTile> liAdjacentTiles = new List<RHTile>();
                RHTile startTile = Tiles[0];

                string sAdjacent = string.Empty;

                //Create the adjacent tiles string
                MakeAdjustments("N", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sMapName].GetTile(new Point((int)(startTile.X), (int)(startTile.Y - 1))));
                MakeAdjustments("S", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sMapName].GetTile(new Point((int)(startTile.X), (int)(startTile.Y + 1))));
                MakeAdjustments("E", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sMapName].GetTile(new Point((int)(startTile.X + 1), (int)(startTile.Y))));
                MakeAdjustments("W", ref sAdjacent, ref liAdjacentTiles, MapManager.Maps[_sMapName].GetTile(new Point((int)(startTile.X - 1), (int)(startTile.Y))));

                //Gross switch statement against the adjacency string to determine
                //which world object to use.
                switch (sAdjacent)
                {
                    case "E":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 13, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "S":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 14, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "W":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 12, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "N":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 15, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "NS":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "EW":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 2, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "SW":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 3, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "NW":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 4, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "NE":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 5, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "SE":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 6, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "NSE":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 7, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "NSW":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 8, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "NEW":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 9, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "SEW":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 10, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    case "NSEW":
                        _rSource = Util.FloatRectangle(_vSourcePos.X + TileSize * 11, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                    default:
                        _rSource = Util.FloatRectangle(_vSourcePos.X, _vSourcePos.Y, _iWidth, _iHeight);
                        break;
                }

                //Call this methodo n the adjacent tiles. Do not recurse.
                if (adjustAdjacent)
                {
                    foreach (RHTile t in liAdjacentTiles)
                    {
                        Wall w = ((Wall)t.GetWorldObject());
                        w.AdjustWall(false);
                    }
                }
            }

            /// <summary>
            /// If the given tile passes the Wall test, add the ifValdid
            /// string to the adjacency string then add to the list
            /// </summary>
            /// <param name="ifValid">Which directional value to add on a pass</param>
            /// <param name="adjacentStr">Ref to the adjacency string</param>
            /// <param name="adjacentList">ref to the adjacency list</param>
            /// <param name="tile">The tile to test</param>
            private void MakeAdjustments(string ifValid, ref string adjacentStr, ref List<RHTile> adjacentList, RHTile tile)
            {
                if (WallTest(tile))
                {
                    adjacentStr += ifValid;
                    adjacentList.Add(tile);
                }
            }

            /// <summary>
            /// Check to see that the tile exists, has an object and that object is a wall
            /// </summary>
            /// <param name="tile">Tile to test against</param>
            /// <returns>True if the tile exists and contains a Wall</returns>
            private bool WallTest(RHTile tile)
            {
                bool rv = false;

                if (tile != null)
                {
                    WorldObject obj = tile.GetWorldObject(false);
                    if (obj != null && obj.Type == ObjectType.Wall)
                    {
                        rv = true;
                    }
                }

                return rv;
            }
        }
    }
}
