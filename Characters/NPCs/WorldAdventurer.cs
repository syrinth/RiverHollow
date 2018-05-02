using RiverHollow.Game_Managers;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using RiverHollow.Misc;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.SpriteAnimations;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.ObjectManager;
using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Game_Managers.GUIComponents.Screens;
using RiverHollow.Tile_Engine;

namespace RiverHollow.Characters.NPCs
{
    //Representation of an Adventurer in the world. WorldAdventurers also produce items at rollover
    public class WorldAdventurer : NPC
    {
        #region Properties
        protected int _iAdventurerID;
        public int AdventurerID { get => _iAdventurerID; }
        protected string _sAdventurerType;
        private WorkerBuilding _building;
        public WorkerBuilding Building => _building;
        protected int _iDailyFoodReq;
        protected int _iCurrFood;
        protected int _iDailyItemID;
        protected Item _heldItem;
        protected int _iMood;
        public bool DrawIt;
        public bool Adventuring;
        public int Mood { get => _iMood; }
        protected string _sTexture;

        protected double _dProcessedTime;
        public double ProcessedTime => _dProcessedTime;

        Dictionary<int, Recipe> _diCrafting;
        public Dictionary<int, Recipe> CraftList => _diCrafting;
        Recipe _currentlyMaking;
        public Recipe CurrentlyMaking => _currentlyMaking;

        private CombatAdventurer _c;
        public CombatAdventurer Combat => _c;
        #endregion

        public WorldAdventurer(string[] stringData, int id)
        {
            _iAdventurerID = id;
            _characterType = CharacterEnum.WorldAdventurer;
            ImportBasics(stringData, id);
            LoadContent(_sTexture);
            _iCurrFood = 0;
            _heldItem = null;
            _iMood = 0;
            DrawIt = true;
            Adventuring = false;

            SetCombat();
        }

        public  new void LoadContent(string texture)
        {
            _sprite = new AnimatedSprite(GameContentManager.GetTexture(texture));
            _sprite.AddAnimation("Idle", 0, 0, TileSize, TileSize * 2, 1, 0.3f);
            _sprite.AddAnimation("WalkDown", TileSize, 0, TileSize, TileSize * 2, 2, 0.3f);
            _sprite.SetCurrentAnimation("Idle");
        }

        protected int ImportBasics(string[] stringData, int id)
        {
            _diCrafting = new Dictionary<int, Recipe>();

            _iAdventurerID = id;
            int i = 0;
            _sAdventurerType = stringData[i++];
            _sTexture = @"Textures\" + _sAdventurerType;
            _iDailyItemID = int.Parse(stringData[i++]);
            _iDailyFoodReq = int.Parse(stringData[i++]);
            int portraitNum = int.Parse(stringData[i++]);
            _portraitRect = new Rectangle(0, 105, 80, 96);
            _portrait = GameContentManager.GetTexture(_sTexture);
            if (stringData.Length >= i)
            {
                string[] crafting = stringData[i++].Split(' ');
                foreach (string s in crafting)
                {
                    _diCrafting.Add(int.Parse(s), ObjectManager.DictCrafting[int.Parse(s)]);
                }
            }

            return i;
        }

        protected void SetCombat()
        {
            _c = new CombatAdventurer(this);
            _c.SetClass(CharacterManager.GetClassByIndex(1));
            _c.LoadContent(@"Textures\Wizard");
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_currentlyMaking != null)
            {
                _sprite.Update(gameTime);
                _dProcessedTime += gameTime.ElapsedGameTime.TotalSeconds;
                int modifiedTime = (int)(_currentlyMaking.ProcessingTime * (0.5 + 0.5*((100 - Mood) / 100)));   //Workers work faster the happier they are.
                if (_dProcessedTime >= modifiedTime)        //NPCs
                {
                    //SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition);
                    _heldItem = ObjectManager.GetItem(_currentlyMaking.Output);
                    _c.AddXP(_currentlyMaking.XP);
                    _dProcessedTime = -1;
                    _currentlyMaking = null;
                    _sprite.SetCurrentAnimation("Idle");
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, bool useLayerDepth = false)
        {
            if (DrawIt)
            {
                base.Draw(spriteBatch, useLayerDepth);
                if (_heldItem != null)
                {
                    _heldItem.Draw(spriteBatch, new Rectangle(Position.ToPoint(), new Point(32, 32)), true);
                }
            }
        }

        public override bool CollisionContains(Point mouse)
        {
            bool rv = false;
            if (DrawIt) {
                rv = base.CollisionContains(mouse);
            }
            return rv;
        }
        public override bool CollisionIntersects(Rectangle rect)
        {
            bool rv = false;
            if (DrawIt)
            {
                rv = base.CollisionIntersects(rect);
            }
            return rv;
        }

        public override void Talk()
        {
            GUIManager.SetScreen(new TextScreen(this, Name + ": " + GameContentManager.GetDialogue("AdventurerTree")));
        }

        public override string GetSelectionText()
        {
            RHRandom r = new RHRandom();
            string text = _dialogueDictionary[r.Next(1, 2).ToString()];
            return Utilities.ProcessText(text, _sName);
        }

        public override string GetDialogEntry(string entry)
        {
            string rv = string.Empty;
            if (entry.Equals("Talk"))
            {
                GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Normal;
                _iMood += 1;

                RHRandom r = new RHRandom();
                rv = GameContentManager.GetDialogue(_sAdventurerType + r.Next(1, 2));
            }
            else if (entry.Equals("Craft"))
            {
                GUIManager.SetScreen(new ItemCreationScreen(this));
            }
            else if (entry.Equals("Party"))
            {
                DrawIt = false;
                Adventuring = true;
                PlayerManager.AddToParty(_c);
                rv = "Of course!";
            }
            return rv;
        }

        public void ProcessChosenItem(int itemID)
        {
            _currentlyMaking = _diCrafting[itemID];
            _sprite.SetCurrentAnimation("Working");
        }

        public int TakeItem()
        {
            int giveItem = -1;
            if (_heldItem != null){
                giveItem = _heldItem.ItemID;
                _heldItem = null;
            }
            return giveItem;
        }

        public int WhatAreYouHolding()
        {
            if (_heldItem != null)
            {
                return _heldItem.ItemID;
            }
            return -1;
        }

        public void SetBuilding(WorkerBuilding b)
        {
            _building = b;
        }

        public bool Rollover()
        {
            bool rv = !Adventuring;
            DrawIt = true;
            Adventuring = false;
            _c.CurrentHP = _c.MaxHP;
            return rv;
        }

        public void MakeDailyItem()
        {
            if (_iDailyItemID != -1)
            {
                InventoryManager.AddNewItemToInventory(_iDailyItemID, _building.BuildingChest);
            }
        }

        public string GetName()
        {
            return _sName;
        }

        public override void SetName(string name)
        {
            _sName = name;
            _c.SetName(name);
        }

        public WorkerData SaveData()
        {
            WorkerData workerData = new WorkerData
            {
                workerID = this.AdventurerID,
                mood = this.Mood,
                name = this.Name,
                processedTime = this.ProcessedTime,
                currentItemID = (this._currentlyMaking == null) ? -1 : this._currentlyMaking.Output,
                heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID
            };

            return workerData;
        }
        public void LoadData(WorkerData data)
        {
            _iAdventurerID = data.workerID;
            _iMood = data.mood;
            _sName = data.name;
            _dProcessedTime = data.processedTime;
            _currentlyMaking = (data.currentItemID == -1) ? null : _diCrafting[data.currentItemID];
            _heldItem = ObjectManager.GetItem(data.heldItemID);

            SetCombat();
            _c.SetName(_sName);

            if (_currentlyMaking != null) { _sprite.SetCurrentAnimation("Working"); }
        }
    }
}
