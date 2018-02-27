using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.ObjectManager;

namespace RiverHollow.Items
{
    public class StaticItem : Item
    {
        protected AnimatedSprite _sprite;
        protected Vector2 _vMapPosition;
        public Vector2 MapPosition
        {
            get { return _vMapPosition; }
            set {
                _vMapPosition = value;
                DrawPosition = value;
            }
        }
        public Vector2 DrawPosition
        {
            get { return _sprite.Position; }
            set { _sprite.Position = new Vector2(_sprite.Width > 32 ? value.X - (_sprite.Width-32)/2 : value.X, (_sprite.Height > 32) ? value.Y - (_sprite.Height - 32) : value.Y); }
        }

        public StaticItem()
        {
        }
    }

    public class Container : StaticItem
    {
        private Item[,] _inventory;
        public Item[,] Inventory { get => _inventory; }

        private int _rows;
        public int Rows { get => _rows; }

        private int _columns;
        public int Columns { get => _columns; }

        public Container(int id, string[] strData)
        {
            int i = ImportBasics(strData, id, 1);
            _rows = int.Parse(strData[i++]);
            _columns = int.Parse(strData[i++]);
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            _pickup = false;
            _inventory = new Item[InventoryManager.maxItemRows, InventoryManager.maxItemColumns];
        }

        public override string GetDescription()
        {
            string rv = base.GetDescription();
            rv += System.Environment.NewLine;
            rv += "Holds " + Rows * Columns + " items";

            return rv;
        }
    }

    public class Machine : StaticItem
    {
        protected string _sMapName;
        
        protected Item _heldItem;
        protected double _processedTime;
        public double ProcessedTime => _processedTime;

        public void LoadContent()
        {
            _iWidth = 32;
            _iHeight = 64;
            _texture = GameContentManager.GetTexture(@"Textures\texMachines");
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(@"Textures\texMachines"));
            _sprite.AddAnimation("Idle", (int)_sourcePos.X, (int)_sourcePos.Y, 32, 64, 1, 0.3f);
            _sprite.AddAnimation("Working", (int)_sourcePos.X + 32, (int)_sourcePos.Y, 32, 64, 2, 0.3f);
            _sprite.SetCurrentAnimation("Idle");
            _sprite.IsAnimating = true;
        }
        public virtual void Update(GameTime gameTime) { }
        public override void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch, true);
        }

        public bool ProcessingFinished() { return _heldItem != null; }
        public void TakeFinishedItem()
        {
            InventoryManager.AddItemToInventory(_heldItem);
            _heldItem = null;
        }
        public void SetMapName(string val) { _sMapName = val; }

        public virtual int GetProcessingItemId() { return -1; }

        public virtual MachineData SaveData() { return new MachineData(); }
        public virtual void LoadData(GameManager.MachineData mac) {  }
    }

    public class Processor : Machine
    {
        Dictionary<int, ProcessRecipe> _diProcessing;
        ProcessRecipe _currentlyProcessing;

        public Processor(int id, string[] stringData)
        {
            _diProcessing = new Dictionary<int, ProcessRecipe>();
            _processedTime = -1;
            _heldItem = null;
            int i = ImportBasics(stringData, id, 1);

            string[] processStr = stringData[i++].Split(new[] { '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in processStr)
            {
                string[] pieces = s.Split(' ');
                _diProcessing.Add(int.Parse(pieces[0]), new ProcessRecipe(pieces));
            }

            LoadContent();

            _pickup = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (_currentlyProcessing != null)
            {
                _sprite.Update(gameTime);
                _processedTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (_processedTime >= _currentlyProcessing.ProcessingTime)
                {
                    SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition);
                    _heldItem = ObjectManager.GetItem(_currentlyProcessing.Output);
                    _processedTime = -1;
                    _currentlyProcessing = null;
                    _sprite.SetCurrentAnimation("Idle");
                }
            }
        }

        public void ProcessHeldItem(Item heldItem)
        {
            if (_diProcessing.ContainsKey(heldItem.ItemID))
            {
                ProcessRecipe p = _diProcessing[heldItem.ItemID];
                if (heldItem.Number >= p.InputNum) {
                    heldItem.Remove(p.InputNum);
                    _currentlyProcessing = p;
                    _sprite.SetCurrentAnimation("Working");
                }
            }
        }
        
        public bool Processing() { return _currentlyProcessing != null; }

        public override MachineData SaveData()
        {
            MachineData m = new MachineData
            {
                staticItemID = this.ItemID,
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
            _itemID = mac.staticItemID;
            MapPosition = new Vector2(mac.x, mac.y);
            _processedTime = mac.processedTime;
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
            _diCrafting = new Dictionary<int, Recipe>();
            _processedTime = -1;
            _heldItem = null;

            int i = ImportBasics(stringData, id, 1);

            string[] processStr = stringData[i++].Split(' ');
            foreach (string s in processStr)
            {
                _diCrafting.Add(int.Parse(s), ObjectManager.DictCrafting[int.Parse(s)]);
            }

            LoadContent();

            _pickup = false;
        }

        public override void Update(GameTime gameTime)
        {
            if (_currentlyMaking != null)
            {
                _sprite.Update(gameTime);
                _processedTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (_processedTime >= _currentlyMaking.ProcessingTime)
                {
                    SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition);
                    _heldItem = ObjectManager.GetItem(_currentlyMaking.Output);
                    _processedTime = -1;
                    _currentlyMaking = null;
                    _sprite.SetCurrentAnimation("Idle");
                }
            }
        }

        public void ProcessChosenItem(int itemID)
        {
            _currentlyMaking = _diCrafting[itemID];
            _sprite.SetCurrentAnimation("Working");
        }

        public bool Processing() { return _currentlyMaking != null; }

        public override MachineData SaveData()
        {
            MachineData m = new MachineData
            {
                staticItemID = this.ItemID,
                x = (int)this.MapPosition.X,
                y = (int)this.MapPosition.Y,
                processedTime = this.ProcessedTime,
                currentItemID = (this._currentlyMaking == null) ? -1 : this._currentlyMaking.Output,
                heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID
            };

            return m;
        }
        public override void LoadData(GameManager.MachineData mac)
        {
            _itemID = mac.staticItemID;
            MapPosition = new Vector2(mac.x, mac.y);
            _processedTime = mac.processedTime;
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
