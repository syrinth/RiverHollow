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

namespace RiverHollow.Items
{
    public class WorldObject
    {
        #region Properties
        public enum ObjectType { Crafter, Container, WorldObject, Destructible, Processor};
        public ObjectType Type;

        public List<RHTile> Tiles;

        protected bool _wallObject;
        public bool WallObject { get => _wallObject; }

        protected Vector2 _position;
        public Vector2 Position { get => _position; }

        protected Rectangle _sourceRectangle;
        public Rectangle SourceRectangle { get => _sourceRectangle;  }

        public virtual Rectangle CollisionBox {  get => new Rectangle((int) Position.X, (int) Position.Y, _width, _height); }

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
            _position = pos;
            _width = width;
            _height = height;
            _texture = tex;
            _wallObject = false;

            _sourceRectangle = sourceRectangle;
            Tiles = new List<RHTile>();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, new Rectangle((int)_position.X, (int)_position.Y, _width, _height), _sourceRectangle, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, _position.Y + _height + (_position.X / 100));
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
            _position = position;
        }

        public void ClearTiles()
        {
            foreach (RHTile t in Tiles)
            {
                t.Clear();
            }
        }

        public bool IsDestructible() { return Type == ObjectType.Destructible; }
        public bool IsWorldObject() { return Type == ObjectType.WorldObject; }
        public bool IsCrafter() { return Type == ObjectType.Crafter; }
        public bool IsProcessor() { return Type == ObjectType.Processor; }
        public bool IsMachine() { return IsProcessor() || IsCrafter(); }
    }

    public class Destructible : WorldObject
    {
        protected int _hp;
        public int HP { get => _hp; }

        protected bool _breakable;
        public bool Breakable { get => _breakable; }

        protected bool _choppable;
        public bool Choppable { get => _choppable; }

        protected int _lvltoDmg;
        public int LvlToDmg { get => _lvltoDmg; }

        public Destructible(int id, string[] stringData, Vector2 pos)
        {
            Type = ObjectType.Destructible;
            _id = id;
            _position = pos;

            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            int i = 2;
            //_name = stringData[i++];
            string[] texIndices = stringData[i++].Split(' ');
            int x = int.Parse(texIndices[0]);
            int y = int.Parse(texIndices[1]);
            _width = int.Parse(texIndices[2]);
            _height = int.Parse(texIndices[3]);
            _sourceRectangle = new Rectangle(0 + 32 * x, 0 + 32 * y, _width, _height);
            _choppable = bool.Parse(stringData[i++]);
            _breakable = bool.Parse(stringData[i++]);
            _hp = int.Parse(stringData[i++]);
            _lvltoDmg = int.Parse(stringData[i++]);

            _wallObject = false;
            Tiles = new List<RHTile>();
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
        public override Rectangle CollisionBox { get => new Rectangle((int)Position.X + RHMap.TileSize, (int)Position.Y + RHMap.TileSize * 3, RHMap.TileSize, RHMap.TileSize); }

        public Tree(int id, Vector2 pos, Rectangle sourceRectangle, Texture2D tex, int width, int height, bool breakIt, bool chopIt, int lvl, int hp) : base(id, pos, sourceRectangle, tex, width, height, breakIt, chopIt, lvl, hp)
        {
            Type = ObjectType.Destructible;
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

    public class WorldItem : WorldObject
    {
        protected AnimatedSprite _sprite;
        protected Vector2 _vMapPosition;
        public Vector2 MapPosition
        {
            get { return _vMapPosition; }
            set
            {
                _vMapPosition = value;
                DrawPosition = value;
            }
        }
        public Vector2 DrawPosition
        {
            get { return _sprite.Position; }
            set { _sprite.Position = new Vector2(_sprite.Width > 32 ? value.X - (_sprite.Width - 32) / 2 : value.X, (_sprite.Height > 32) ? value.Y - (_sprite.Height - 32) : value.Y); }
        }

        public class Machine : WorldItem
        {
            protected Vector2 _vSourcePos;
            protected string _sMapName;

            protected Item _heldItem;
            protected double _dProcessedTime;
            public double ProcessedTime => _dProcessedTime;

            public Machine()
            {
                _width = 32;
                _height = 64;
            }
            public void LoadContent()
            {
                _texture = GameContentManager.GetTexture(@"Textures\texMachines");
                _sprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\texMachines"));
                _sprite.AddAnimation("Idle", (int)_vSourcePos.X, (int)_vSourcePos.Y, 32, 64, 1, 0.3f);
                _sprite.AddAnimation("Working", (int)_vSourcePos.X + 32, (int)_vSourcePos.Y, 32, 64, 2, 0.3f);
                _sprite.SetCurrentAnimation("Idle");
                _sprite.IsAnimating = true;
            }
            public virtual void Update(GameTime gameTime) { }
            public override void Draw(SpriteBatch spriteBatch)
            {
                _sprite.Draw(spriteBatch, true);
                if (_heldItem != null)
                {
                    _heldItem.Draw(spriteBatch, new Rectangle(DrawPosition.ToPoint(), new Point(32, 32)), true);
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
            public void SetMapName(string val) { _sMapName = val; }

            public virtual int GetProcessingItemId() { return -1; }

            public virtual MachineData SaveData() { return new MachineData(); }
            public virtual void LoadData(GameManager.MachineData mac) { }
        }

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

                string[] srcPos = stringData[1].Split(' ');
                _vSourcePos = new Vector2(0 + 32 * int.Parse(srcPos[0]), 0 + 32 * int.Parse(srcPos[1]));

                string[] processStr = stringData[2].Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string s in processStr)
                {
                    string[] pieces = s.Split(' ');
                    _diProcessing.Add(int.Parse(pieces[0]), new ProcessRecipe(pieces));
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
                        _sprite.SetCurrentAnimation("Idle");
                    }
                }
            }

            public override bool Processing() { return _currentlyProcessing != null; }
            public override void ProcessClick()
            {
                Item heldItem = GraphicCursor.HeldItem;
                if (heldItem != null)
                {
                    if (_diProcessing.ContainsKey(heldItem.ItemID))
                    {
                        ProcessRecipe p = _diProcessing[heldItem.ItemID];
                        if (heldItem.Number >= p.InputNum)
                        {
                            heldItem.Remove(p.InputNum);
                            _currentlyProcessing = p;
                            _sprite.SetCurrentAnimation("Working");
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

                if (_currentlyProcessing != null) { _sprite.SetCurrentAnimation("Working"); }
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

                    //[x y z] means 1 x => y in z seconds
                    if (data.Length == 3)
                    {
                        _iReqInput = 1;
                        _iOutput = int.Parse(data[1]);
                        _iProcessingTime = int.Parse(data[2]);
                    }
                    else if (data.Length == 4)            //[w x y z] means x w => y in z seconds
                    {
                        _iReqInput = int.Parse(data[1]);
                        _iOutput = int.Parse(data[2]);
                        _iProcessingTime = int.Parse(data[3]);
                    }
                }
            }
        }

        public class Crafter : Machine
        {
            Dictionary<int, Recipe> _diCrafting;
            public Dictionary<int, Recipe> CraftList => _diCrafting;
            Recipe _currentlyMaking;

            public Crafter(int id, string[] stringData) : base()
            {
                _id = id;
                Type = ObjectType.Crafter;
                _diCrafting = new Dictionary<int, Recipe>();
                _dProcessedTime = -1;
                _heldItem = null;

                string[] srcPos = stringData[1].Split(' ');
                _vSourcePos = new Vector2(0 + 32 * int.Parse(srcPos[0]), 0 + 32 * int.Parse(srcPos[1]));

                string[] processStr = stringData[2].Split(' ');
                foreach (string s in processStr)
                {
                    _diCrafting.Add(int.Parse(s), ObjectManager.DictCrafting[int.Parse(s)]);
                }

                LoadContent();
            }

            public override void Update(GameTime gameTime)
            {
                if (_currentlyMaking != null)
                {
                    _sprite.Update(gameTime);
                    _dProcessedTime += gameTime.ElapsedGameTime.TotalSeconds;
                    if (_dProcessedTime >= _currentlyMaking.ProcessingTime)
                    {
                        SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition);
                        _heldItem = ObjectManager.GetItem(_currentlyMaking.Output);
                        _dProcessedTime = -1;
                        _currentlyMaking = null;
                        _sprite.SetCurrentAnimation("Idle");
                    }
                }
            }

            public override bool Processing() { return _currentlyMaking != null; }
            public override void ProcessClick()
            {
                GUIManager.SetScreen(new ItemCreationScreen(this));
            }

            public void MakeChosenItem(int itemID)
            {
                _currentlyMaking = _diCrafting[itemID];
                _sprite.SetCurrentAnimation("Working");
            }

            public override MachineData SaveData()
            {
                MachineData m = new MachineData
                {
                    ID = this.ID,
                    x = (int)this.MapPosition.X,
                    y = (int)this.MapPosition.Y,
                    processedTime = this.ProcessedTime,
                    currentItemID = (this._currentlyMaking == null) ? -1 : this._currentlyMaking.Output,
                    heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID
                };

                return m;
            }
            public override void LoadData(MachineData mac)
            {
                _id = mac.ID;
                MapPosition = new Vector2(mac.x, mac.y);
                _dProcessedTime = mac.processedTime;
                _currentlyMaking = (mac.currentItemID == -1) ? null : _diCrafting[mac.currentItemID];
                _heldItem = ObjectManager.GetItem(mac.heldItemID);

                if (_currentlyMaking != null) { _sprite.SetCurrentAnimation("Working"); }
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

                    //[x y z] means 1 x => y in z seconds
                    if (data.Length == 3)
                    {
                        _iReqInput = 1;
                        _iOutput = int.Parse(data[1]);
                        _iProcessingTime = int.Parse(data[2]);
                    }
                    else if (data.Length == 4)            //[w x y z] means x w => y in z seconds
                    {
                        _iReqInput = int.Parse(data[1]);
                        _iOutput = int.Parse(data[2]);
                        _iProcessingTime = int.Parse(data[3]);
                    }
                }
            }
        }
    }
}
