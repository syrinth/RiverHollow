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
using System;

namespace RiverHollow.Characters.NPCs
{
    //Representation of an Adventurer in the world. WorldAdventurers also produce items at rollover
    public class WorldAdventurer : NPC
    {
        #region Properties
        private WorkerTypeEnum _workerType;
        public WorkerTypeEnum WorkerType => _workerType;
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
            SetCombat();

            _sAdventurerType = Combat.CharacterClass.Name;
            _sTexture = @"Textures\" + _sAdventurerType;
            _portraitRect = new Rectangle(0, 105, 80, 96);
            _portrait = GameContentManager.GetTexture(_sTexture);

            LoadContent(_sTexture);
            _iCurrFood = 0;
            _heldItem = null;
            _iMood = 0;
            DrawIt = true;
            Adventuring = false;
        }

        public  new void LoadContent(string texture)
        {
            _bodySprite = new AnimatedSprite(GameContentManager.GetTexture(texture), true);
            _bodySprite.AddAnimation("Idle", TileSize, 0, TileSize, TileSize * 2, 1, 0.3f);
            _bodySprite.AddAnimation("WalkDown", 0, 0, TileSize, TileSize * 2, 3, 0.3f);
            _bodySprite.SetCurrentAnimation("Idle");
        }

        protected void ImportBasics(string[] stringData, int id)
        {
            _diCrafting = new Dictionary<int, Recipe>();

            _iAdventurerID = id;

            foreach (string s in stringData)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Type"))
                {
                    _workerType = Util.ParseEnum<WorkerTypeEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Item"))
                {
                    _iDailyItemID = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Food"))
                {
                    _iDailyFoodReq = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("Crafts"))
                {
                    string[] crafting = tagType[1].Split(' ');
                    foreach (string recipe in crafting)
                    {
                        _diCrafting.Add(int.Parse(recipe), ObjectManager.DictCrafting[int.Parse(recipe)]);
                    }
                }
            }
        }

        protected void SetCombat()
        {
            _c = new CombatAdventurer(this);
            _c.SetClass(CharacterManager.GetClassByIndex(_iAdventurerID));
            _c.LoadContent(@"Textures\" + _c.CharacterClass.Name);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            if (_currentlyMaking != null)
            {
                _bodySprite.Update(gameTime);
                _dProcessedTime += gameTime.ElapsedGameTime.TotalSeconds;
                int modifiedTime = (int)(_currentlyMaking.ProcessingTime * (0.5 + 0.5*((100 - Mood) / 100)));   //Workers work faster the happier they are.
                if (_dProcessedTime >= modifiedTime)        //NPCs
                {
                    //SoundManager.PlayEffectAtLoc("126426__cabeeno-rossley__timer-ends-time-up", _sMapName, MapPosition);
                    _heldItem = ObjectManager.GetItem(_currentlyMaking.Output);
                    _c.AddXP(_currentlyMaking.XP);
                    _dProcessedTime = -1;
                    _currentlyMaking = null;
                    _bodySprite.SetCurrentAnimation("Idle");
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
            GUIManager.SetScreen(new TextScreen(this, Name + ": " + GameContentManager.GetGameText("AdventurerTree")));
        }

        public override string GetSelectionText()
        {
            RHRandom r = new RHRandom();
            string text = _dialogueDictionary[r.Next(1, 2).ToString()];
            return Util.ProcessText(text, _sName);
        }

        public override string GetDialogEntry(string entry)
        {
            string rv = string.Empty;
            if (entry.Equals("Talk"))
            {
                GraphicCursor._CursorType = GraphicCursor.EnumCursorType.Normal;
                _iMood += 1;

                RHRandom r = new RHRandom();
                rv = GameContentManager.GetGameText(_sAdventurerType + r.Next(1, 2));
            }
            else if (entry.Equals("Craft"))
            {
                GUIManager.SetScreen(new CraftingScreen(this));
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
            _bodySprite.SetCurrentAnimation("Working");
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
            if (GameManager.AutoDisband)
            {
                DrawIt = true;
                Adventuring = false;
            }
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

        public new WorkerData SaveData()
        {
            WorkerData workerData = new WorkerData
            {
                workerID = this.AdventurerID,
                advData = Combat.SaveData(),
                mood = this.Mood,
                name = this.Name,
                processedTime = this.ProcessedTime,
                currentItemID = (this._currentlyMaking == null) ? -1 : this._currentlyMaking.Output,
                heldItemID = (this._heldItem == null) ? -1 : this._heldItem.ItemID,
                adventuring = Adventuring
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
            Adventuring = data.adventuring;

            SetCombat();
            Combat.LoadData(data.advData);

            if (_currentlyMaking != null) { _bodySprite.SetCurrentAnimation("Working"); }
            if (Adventuring)
            {
                DrawIt = false;
                PlayerManager.AddToParty(Combat);
            }
        }
    }
}
