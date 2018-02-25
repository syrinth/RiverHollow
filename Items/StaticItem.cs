using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using static RiverHollow.Game_Managers.ObjectManager;

namespace RiverHollow.Items
{
    public class StaticItem : Item
    {
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
        protected Item _heldItem;
        protected double _processedTime;

        public virtual void Update(GameTime gameTime) { }
        public bool ProcessingFinished() { return _heldItem != null; }
        public void TakeFinishedItem()
        {
            InventoryManager.AddItemToInventory(_heldItem);
            _heldItem = null;
        }
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

            _pickup = false;
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");
        }

        public override void Update(GameTime gameTime)
        {
            if (_currentlyProcessing != null)
            {
                _processedTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (_processedTime >= _currentlyProcessing.ProcessingTime)
                {
                    SoundManager.PlayEffect("126426__cabeeno-rossley__timer-ends-time-up", 0.9f);
                    _heldItem = ObjectManager.GetItem(_currentlyProcessing.Output);
                    _processedTime = -1;
                    _currentlyProcessing = null;
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
                }
            }
        }
        
        public bool Processing() { return _currentlyProcessing != null; }

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
        ProcessRecipe _currentlyProcessing;

        public Crafter(int id, string[] stringData)
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

            _pickup = false;
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");
        }

        public override void Update(GameTime gameTime)
        {
            if (_currentlyProcessing != null)
            {
                _processedTime += gameTime.ElapsedGameTime.TotalSeconds;
                if (_processedTime >= _currentlyProcessing.ProcessingTime)
                {
                    SoundManager.PlayEffect("126426__cabeeno-rossley__timer-ends-time-up", 0.9f);
                    _heldItem = ObjectManager.GetItem(_currentlyProcessing.Output);
                    _processedTime = -1;
                    _currentlyProcessing = null;
                }
            }
        }

        public void ProcessHeldItem(Item heldItem)
        {
            //if (_diProcessing.ContainsKey(heldItem.ItemID))
            //{
            //    ProcessRecipe p = _diProcessing[heldItem.ItemID];
            //    if (heldItem.Number >= p.InputNum)
            //    {
            //        heldItem.Remove(p.InputNum);
            //        _currentlyProcessing = p;
            //    }
            //}
        }

        public bool Processing() { return _currentlyProcessing != null; }

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
