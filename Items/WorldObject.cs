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
        public enum ObjectType { Building, ClassChanger, Machine, Container, Door, Earth, Floor, WorldObject, Destructible, Plant, Forageable};
        public ObjectType Type;

        public List<RHTile> Tiles;

        protected bool _blocking = true;
        public bool Blocking => _blocking;
        protected bool _wallObject;
        public bool WallObject => _wallObject;

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
        public int BaseWidth => _iBaseWidth;
        protected int _iBaseHeight = TileSize;
        public int BaseHeight => _iBaseHeight;

        protected int _id;
        public int ID { get => _id; }
        #endregion

        protected WorldObject() { }

        public WorldObject(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height)
        {
            Type = ObjectType.WorldObject;
            _id = id;
            _vMapPosition = pos;
            _iWidth = width;
            _iHeight = height;
            _texture = tex;
            _wallObject = false;

            _rSource = sourceRectangle;
            Tiles = new List<RHTile>();
        }

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
        int _iDropID;
        public int DropID => _iDropID;

        public Forageable(int id, Dictionary<string, string> stringData, Vector2 pos)
        {
            Type = ObjectType.Forageable;
            _id = id;
            _vMapPosition = pos;

            _wallObject = false;
            Tiles = new List<RHTile>();

            int x = 0;
            int y = 0;
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            string[] texIndices = stringData["Image"].Split('-');
            x = int.Parse(texIndices[0]);
            y = int.Parse(texIndices[1]);

            _iWidth = int.Parse(stringData["Width"]);
            _iHeight = int.Parse(stringData["Height"]);
            _iDropID = int.Parse(stringData["Item"]);

            _rSource = new Rectangle(x, y, _iWidth, _iHeight);
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
            Tiles = new List<RHTile>();

            int x = 0;
            int y = 0;
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            string[] texIndices = stringData["Image"].Split('-');
            x = int.Parse(texIndices[0]);
            y = int.Parse(texIndices[1]);

            _iWidth = int.Parse(stringData["Width"]);
            _iHeight = int.Parse(stringData["Height"]);

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
                    _blocking = false;
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
                    _blocking = false;
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
                _blocking = !unlocked;
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
                _sprite.AddAnimation(MachineAnimEnum.Idle, (int)_vSourcePos.X, (int)_vSourcePos.Y, TileSize, TileSize * 2, 1, 0.3f);
                _sprite.SetCurrentAnimation(MachineAnimEnum.Idle);
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
                _sprite = new AnimatedSprite(@"Textures\texMachines", true);
                _sprite.AddAnimation(MachineAnimEnum.Idle, (int)_vSourcePos.X, (int)_vSourcePos.Y, _iWidth, _iHeight, 1, 0.3f);
                _sprite.AddAnimation(MachineAnimEnum.Working, (int)_vSourcePos.X + _iWidth, (int)_vSourcePos.Y, _iWidth, _iHeight, _iWorkingFrames, _fFrameSpeed);
                _sprite.SetCurrentAnimation(MachineAnimEnum.Idle);
                _sprite.IsAnimating = true;
            }
            public virtual void Update(GameTime gameTime) {
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
                _sprite.SetCurrentAnimation(MachineAnimEnum.Idle);

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
                            _sprite.SetCurrentAnimation(MachineAnimEnum.Working);
                        }
                    }
                }

                if (!Processed) {
                    GUIManager.SetScreen(new CraftingScreen(this));
                }
            }
            public bool HasItem() { return _heldItem != null; }
            public void TakeFinishedItem()
            {
                InventoryManager.AddItemToInventory(_heldItem);
                _heldItem = null;
                _itemBubble = null;
            }

            public void MakeChosenItem(int itemID)
            {
                _iCurrentlyMaking = itemID;
                _sprite.SetCurrentAnimation(MachineAnimEnum.Working);
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

                if (CurrentlyProcessing != null) { _sprite.SetCurrentAnimation(MachineAnimEnum.Working); }
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

                public void Update(GameTime gameTime)
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

                _inventory = new Item[InventoryManager.maxItemRows, InventoryManager.maxItemColumns];
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
                        InventoryManager.AddItemToInventorySpot(newItem, i, j, this);
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
                _blocking = false;

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
                        InventoryManager.AddItemToInventory(it);
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
    }
}
