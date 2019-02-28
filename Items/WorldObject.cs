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

namespace RiverHollow.WorldObjects
{
    public class WorldObject
    {
        #region Properties
        public static int Rock = 0;
        public static int BigRock = 1;
        public static int Tree = 2;
        public enum ObjectType { Building, ClassChanger, Crafter, Container, Door, Earth, Floor, WorldObject, Destructible, Processor, Plant};
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
        public Rectangle SourceRectangle { get => _rSource;  }

        public virtual Rectangle CollisionBox {  get => new Rectangle((int)MapPosition.X, (int)MapPosition.Y, _width, _height); }

        protected Texture2D _texture;
        public Texture2D Texture { get => _texture; }

        protected int _width;
        protected int _height;

        protected int _id;
        public int ID { get => _id; }
        #endregion

        protected WorldObject() { }

        public WorldObject(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height)
        {
            Type = ObjectType.WorldObject;
            _id = id;
            _vMapPosition = pos;
            _width = width;
            _height = height;
            _texture = tex;
            _wallObject = false;

            _rSource = sourceRectangle;
            Tiles = new List<RHTile>();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_vMapPosition.X, (int)_vMapPosition.Y, _width, _height), _rSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, _vMapPosition.Y + _height + (_vMapPosition.X / 100));
        }

        public virtual bool IntersectsWith(Rectangle r)
        {
            return CollisionBox.Intersects(r);
        }

        public virtual bool Contains(Point m)
        {
            return CollisionBox.Contains(m);
        }

        public virtual void SetCoordinates(Vector2 position)
        {
            _vMapPosition = position;
        }

        public void RemoveSelfFromTiles()
        {
            foreach (RHTile t in Tiles)
            {
                t.RemoveWorldObject();
            }
        }

        public bool IsContainer() { return Type == ObjectType.Container; }
        public bool IsCrafter() { return Type == ObjectType.Crafter; }
        public bool IsDestructible() { return Type == ObjectType.Destructible; }
        public bool IsMachine() { return IsProcessor() || IsCrafter(); }
        public bool IsProcessor() { return Type == ObjectType.Processor; }
        public bool IsPlant() { return Type == ObjectType.Plant; }
        public bool IsWorldObject() { return Type == ObjectType.WorldObject; }
        public bool IsGround() { return Type == ObjectType.Floor; }
        public bool IsEarth() { return Type == ObjectType.Earth; }
        public bool IsDoor() { return Type == ObjectType.Door; }
        public bool IsClassChanger() { return Type == ObjectType.ClassChanger; }
    }

    public class Destructible : WorldObject
    {
        protected int _hp;
        public int HP => _hp;

        protected bool _breakable;
        public bool Breakable => _breakable;

        protected bool _choppable;
        public bool Choppable => _choppable;

        protected int _lvltoDmg;
        public int LvlToDmg => _lvltoDmg;

        public Destructible(int id, string[] stringData, Vector2 pos)
        {
            Type = ObjectType.Destructible;
            _id = id;
            _vMapPosition = pos;

            _wallObject = false;
            Tiles = new List<RHTile>();

            int x = 0;
            int y = 0;
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            foreach (string s in stringData)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Image"))
                {
                    string[] texIndices = tagType[1].Split('-');
                    x = int.Parse(texIndices[0]);
                    y = int.Parse(texIndices[1]);
                }
                else if (tagType[0].Equals("Width"))
                {
                    _width = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Height"))
                {
                    _height = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Chop"))
                {
                    _choppable = true;
                }
                else if (tagType[0].Equals("Break"))
                {
                    _breakable = true;
                }
                else if (tagType[0].Equals("Hp"))
                {
                    _hp = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("ReqLvl"))
                {
                    _lvltoDmg = int.Parse(tagType[1]);
                }
            }

            _rSource = new Rectangle(0 + TileSize * x, 0 + TileSize * y, _width, _height);
        }

        public Destructible(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height, bool breakIt, bool chopIt, int lvl, int hp) : base(id, pos, sourceRectangle, tex, width, height)
        {
            Type = ObjectType.Destructible;

            _hp = hp;
            _breakable = breakIt;
            _choppable = chopIt;
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

        public Tree(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height, bool breakIt, bool chopIt, int lvl, int hp) : base(id, pos, sourceRectangle, tex, width, height, breakIt, chopIt, lvl, hp)
        {
            Type = ObjectType.Destructible;
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
                GUIManager.SetScreen(new TextScreen(GameContentManager.GetGameText("MobDoor"), false));
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
                GUIManager.SetScreen(new TextScreen(this, GameContentManager.GetGameText("KeyDoor")));
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
                GUIManager.SetScreen(new TextScreen(GameContentManager.GetGameText("SpringDoor"), false));
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

            _width = TileSize; ;
            _height = TileSize;
        }

        public Floor(int id)
        {
            _id = id;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_vMapPosition.X, (int)_vMapPosition.Y, _width, _height), _rSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, 0);
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
                    _sprite.Position = new Vector2(_sprite.Width > TileSize ? value.X - (_sprite.Width - TileSize) / 2 : value.X, (_sprite.Height > TileSize) ? value.Y - (_sprite.Height - TileSize) : value.Y);
                }
            }
        }

        protected void ReadSourcePos(string str)
        {
            string[] strPos = str.Split('-');
            _vSourcePos = new Vector2(0 + TileSize * int.Parse(strPos[0]), 0 + TileSize * int.Parse(strPos[1]));
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

                _width = TileSize;
                _height = TileSize * 2;
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
                int toSet = (currID < ActorManager.GetClassCount() - 1) ? (PlayerManager.Combat.CharacterClass.ID + 1) : 1;
                PlayerManager.SetClass(toSet);
            }

            public MachineData SaveData() { return new MachineData(); }
            public virtual void LoadData(GameManager.MachineData mac) { }
        }

        public class Machine : WorldItem
        {
            protected Item _heldItem;
            protected double _dProcessedTime;
            public double ProcessedTime => _dProcessedTime;

            public Machine()
            {
                _width = TileSize;
                _height = TileSize*2;
            }
            public void LoadContent()
            {
                _sprite = new AnimatedSprite(@"Textures\texMachines");
                _sprite.AddAnimation(MachineAnimEnum.Idle, (int)_vSourcePos.X, (int)_vSourcePos.Y, TileSize, TileSize * 2, 1, 0.3f);
                _sprite.AddAnimation(MachineAnimEnum.Working, (int)_vSourcePos.X + TileSize, (int)_vSourcePos.Y, TileSize, TileSize * 2, 2, 0.3f);
                _sprite.SetCurrentAnimation(MachineAnimEnum.Idle);
                _sprite.IsAnimating = true;
            }
            public virtual void Update(GameTime gameTime) { }
            public override void Draw(SpriteBatch spriteBatch)
            {
                _sprite.Draw(spriteBatch, true);
                if (_heldItem != null)
                {
                    _heldItem.Draw(spriteBatch, new Rectangle(HeldItemPos.ToPoint(), new Point(TileSize, TileSize)), true);
                }
            }

            public virtual bool Processing() { return false; }
            public virtual void ProcessClick() { }
            public bool ProcessingFinished() { return _heldItem != null; }
            public void TakeFinishedItem()
            {
                InventoryManager.AddItemToInventory(_heldItem);
                _heldItem = null;
            }

            public virtual int GetProcessingItemId() { return -1; }

            public virtual MachineData SaveData() { return new MachineData(); }
            public virtual void LoadData(GameManager.MachineData mac) { }

            public class Processor : Machine
            {
                Dictionary<int, ProcessRecipe> _diProcessing;
                ProcessRecipe _currentlyProcessing;

                public Processor(int id, string[] stringData)
                {
                    _id = id;
                    Type = ObjectType.Processor;
                    _diProcessing = new Dictionary<int, ProcessRecipe>();
                    _dProcessedTime = -1;
                    _heldItem = null;

                    foreach (string s in stringData)
                    {
                        string[] tagType = s.Split(':');
                        if (tagType[0].Equals("Image"))
                        {
                            ReadSourcePos(tagType[1]);
                        }
                        else if (tagType[0].Equals("Makes"))
                        {
                            string[] recipeStr = Util.FindTags(tagType[1]);
                            foreach (string recipe in recipeStr)
                            {
                                string[] pieces = recipe.Split('-');
                                _diProcessing.Add(int.Parse(pieces[0]), new ProcessRecipe(pieces));
                            }
                        }
                    }

                    LoadContent();
                }

                public override void Update(GameTime gameTime)
                {
                    if (_currentlyProcessing != null)
                    {
                        _sprite.Update(gameTime);
                        _dProcessedTime += gameTime.ElapsedGameTime.TotalSeconds;
                        if (_dProcessedTime >= _currentlyProcessing.ProcessingTime)
                        {
                            SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition);
                            _heldItem = ObjectManager.GetItem(_currentlyProcessing.Output);
                            _dProcessedTime = -1;
                            _currentlyProcessing = null;
                            _sprite.SetCurrentAnimation(MachineAnimEnum.Idle);
                        }
                    }
                }

                public override bool Processing() { return _currentlyProcessing != null; }
                public override void ProcessClick()
                {
                    Item itemToProcess = InventoryManager.GetCurrentItem();
                    if (itemToProcess != null)
                    {
                        int ItemToMake = -1;
                        foreach (ProcessRecipe pr in _diProcessing.Values)
                        {
                            ItemToMake = pr.Output;
                            List<KeyValuePair<int, int>> requirements = ObjectManager.GetItem(pr.Output).GetIngredients();
                            foreach (KeyValuePair<int, int> kvp in requirements)
                            {
                                if (kvp.Key == itemToProcess.ItemID)
                                {
                                    ItemToMake = pr.Output;
                                    break;
                                }
                            }

                            if (ItemToMake != -1) { break; }
                        }

                        if (ItemToMake != -1)
                        {
                            ProcessRecipe p = _diProcessing[ItemToMake];
                            if (itemToProcess.Number >= p.InputNum)
                            {
                                itemToProcess.Remove(p.InputNum);
                                _currentlyProcessing = p;
                                _sprite.SetCurrentAnimation(MachineAnimEnum.Working);
                            }
                        }
                    }
                }

                public override MachineData SaveData()
                {
                    MachineData m = new MachineData
                    {
                        ID = this.ID,
                        x = (int)this.MapPosition.X,
                        y = (int)this.MapPosition.Y,
                        processedTime = this.ProcessedTime,
                        currentItemID = (this._currentlyProcessing == null) ? -1 : this._currentlyProcessing.Input,
                        heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID
                    };

                    return m;
                }
                public override void LoadData(GameManager.MachineData mac)
                {
                    _id = mac.ID;
                    MapPosition = new Vector2(mac.x, mac.y);
                    _dProcessedTime = mac.processedTime;
                    _currentlyProcessing = (mac.currentItemID == -1) ? null : _diProcessing[mac.currentItemID];
                    _heldItem = ObjectManager.GetItem(mac.heldItemID);

                    if (_currentlyProcessing != null) { _sprite.SetCurrentAnimation(MachineAnimEnum.Working); }
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
                        _iOutput = int.Parse(data[0]);

                        Item it = ObjectManager.GetItem(_iOutput);
                        List<KeyValuePair<int, int>> liKVPs = it.GetIngredients();
                        if (liKVPs.Count == 1)
                        {
                            _iProcessingTime = int.Parse(data[1]);
                            _iInput = liKVPs[0].Key;
                            _iReqInput = liKVPs[0].Value;
                        }
                        else
                        {
                            int i = 0;
                            i++;
                        }
                    }
                }
            }

            public class Crafter : Machine
            {
                Dictionary<int, int> _diCrafting;
                public Dictionary<int, int> CraftList => _diCrafting;
                int _iCurrentlyMaking = -1;

                public Crafter(int id, string[] stringData) : base()
                {
                    _id = id;
                    Type = ObjectType.Crafter;
                    _diCrafting = new Dictionary<int, int>();
                    _dProcessedTime = -1;
                    _heldItem = null;

                    foreach (string s in stringData)
                    {
                        string[] tagType = s.Split(':');
                        if (tagType[0].Equals("Image"))
                        {
                            ReadSourcePos(tagType[1]);
                        }
                        else if (tagType[0].Equals("Makes"))
                        {
                            string[] processStr = tagType[1].Split('-');
                            _diCrafting.Add(int.Parse(processStr[0]), int.Parse(processStr[1]));
                        }
                    }         

                    LoadContent();
                }

                public override void Update(GameTime gameTime)
                {
                    if (_iCurrentlyMaking != -1)
                    {
                        _sprite.Update(gameTime);
                        _dProcessedTime += gameTime.ElapsedGameTime.TotalSeconds;
                        if (_dProcessedTime >= _diCrafting[_iCurrentlyMaking])
                        {
                            SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition);
                            _heldItem = ObjectManager.GetItem(_iCurrentlyMaking);
                            _dProcessedTime = -1;
                            _iCurrentlyMaking = -1;
                            _sprite.SetCurrentAnimation(MachineAnimEnum.Idle);
                        }
                    }
                }

                public override bool Processing() { return _iCurrentlyMaking != -1; }
                public override void ProcessClick()
                {
                    GUIManager.SetScreen(new CraftingScreen(this));
                }

                public void MakeChosenItem(int itemID)
                {
                    _iCurrentlyMaking = _diCrafting[itemID];
                    _sprite.SetCurrentAnimation(MachineAnimEnum.Working);
                }

                public override MachineData SaveData()
                {
                    MachineData m = new MachineData
                    {
                        ID = this.ID,
                        x = (int)this.MapPosition.X,
                        y = (int)this.MapPosition.Y,
                        processedTime = this.ProcessedTime,
                        currentItemID = this._iCurrentlyMaking,
                        heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID
                    };

                    return m;
                }
                public override void LoadData(MachineData mac)
                {
                    _id = mac.ID;
                    MapPosition = new Vector2(mac.x, mac.y);
                    _dProcessedTime = mac.processedTime;
                    _iCurrentlyMaking = mac.currentItemID;
                    _heldItem = ObjectManager.GetItem(mac.heldItemID);

                    if (_iCurrentlyMaking != -1) { _sprite.SetCurrentAnimation(MachineAnimEnum.Working); }
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

            public Container(int id, string[] stringData)
            {
                _id = id;
                Type = ObjectType.Container;

                _width = TileSize;
                _height = TileSize;

                foreach (string s in stringData)
                {
                    string[] tagType = s.Split(':');
                    if (tagType[0].Equals("Image"))
                    {
                        ReadSourcePos(tagType[1]);
                    }
                    else if (tagType[0].Equals("Rows"))
                    {
                        _iRows = int.Parse(tagType[1]);
                    }
                    else if (tagType[0].Equals("Cols"))
                    {
                        _iColumns = int.Parse(tagType[1]);
                    }
                }

                LoadContent();

                _inventory = new Item[InventoryManager.maxItemRows, InventoryManager.maxItemColumns];
            }
            public void LoadContent()
            {
                _texture = GameContentManager.GetTexture(@"Textures\worldObjects");
                _rSource = new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, _width, _height);
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

            public Plant(int id, string[] stringData)
            {
                _id = id;
                Type = ObjectType.Plant;
                _blocking = false;

                _iCurrentState = 0;
                _diTransitionTimes = new Dictionary<int, int>();

                _width = TileSize;
                _height = TileSize;

                foreach (string s in stringData)
                {
                    string[] tagType = s.Split(':');
                    if (tagType[0].Equals("Image"))
                    {
                        ReadSourcePos(tagType[1]);
                    }
                    else if (tagType[0].Equals("Item"))
                    {
                        _iResourceID = int.Parse(tagType[1]);
                    }
                    else if (tagType[0].Equals("TrNum"))
                    {
                        _iMaxStates = int.Parse(tagType[1]);
                    }
                    else if (tagType[0].Equals("TrTime"))
                    {
                        string[] dayStr = tagType[1].Split('-');
                        for (int j = 0; j < _iMaxStates - 1; j++)
                        {
                            _diTransitionTimes.Add(j, int.Parse(dayStr[j]));
                        }

                        _iDaysLeft = _diTransitionTimes[0];
                    }
                }

                LoadContent();
            }

            public override void Draw(SpriteBatch spriteBatch)
            {
                float layerDepth = (_iCurrentState == 0) ? 1 : _vMapPosition.Y + (_vMapPosition.X / 100);
                spriteBatch.Draw(_texture, new Rectangle((int)_vMapPosition.X, (int)_vMapPosition.Y, _width, _height), _rSource, Color.White, 0, Vector2.Zero, SpriteEffects.None, layerDepth);
            }
            public void LoadContent()
            {
                _texture = GameContentManager.GetTexture(@"Textures\worldObjects");
                _rSource = new Rectangle((int)_vSourcePos.X, (int)_vSourcePos.Y, _width, _height);
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
                        _rSource.X += _width;
                        if (_diTransitionTimes.ContainsKey(_iCurrentState))
                        {
                            _iDaysLeft = _diTransitionTimes[_iCurrentState];
                        }
                    }
                }
            }
            public bool FinishedGrowing() { return _iCurrentState == _iMaxStates-1; }
            public Item Harvest()
            {
                Item it = null;
                if (FinishedGrowing())
                {
                    it = ObjectManager.GetItem(_iResourceID);
                    it.Pop(MapPosition);
                }
                return it;
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

                _rSource.X += _width * _iCurrentState;
            }
        }
    }
}
