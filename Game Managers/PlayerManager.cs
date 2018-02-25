using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.Characters;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Characters.CombatStuff;
using RiverHollow.Misc;
using RiverHollow.GUIObjects;

namespace RiverHollow.Game_Managers
{
    public static class PlayerManager
    {
        #region Properties
        private static bool _busy;
        public static Tool UseTool;
        private static RHTile _targetTile = null;

        private static List<Quest> _questLog;
        public static List<Quest> QuestLog { get => _questLog; }
        
        public static int Stamina;
        public static int MaxStamina;
        public static string _inBuilding = string.Empty;
        private static List<int> _canMake;
        public static List<int> CanMake { get => _canMake; }
        private static string _currentMap;
        public static string CurrentMap { get => _currentMap; set => _currentMap = value; }

        public static WorldCharacter World;
        private static Tool _pick;
        private static Tool _axe;

        public static CombatAdventurer Combat;
        public static int HitPoints { get => Combat.CurrentHP; }
        public static int MaxHitPoints { get => Combat.MaxHP; }

        private static List<WorkerBuilding> _buildings;
        public static List<WorkerBuilding> Buildings { get => _buildings; }

        private static List<CombatAdventurer> _party;

        public static MerchantChest _merchantChest;
        private static string _name = "Syrinth";
        public static string Name { get => _name; }

        private static int _money = 2000;
        public static int Money { get => _money; }
        #endregion

        public static void Initialize()
        {
            _party = new List<CombatAdventurer>();
            _questLog = new List<Quest>();
            World = new WorldCharacter();
            Combat = new CombatAdventurer(_name);
            _party.Add(Combat);
            _buildings = new List<WorkerBuilding>();
            _canMake = new List<int>();
        }
        public static void InitPlayer()
        {
            Initialize();
            Combat.LoadContent(@"Textures\Wizard"); //ToDo: position doesn't matter here
            World.LoadContent(@"Textures\Eggplant", 32, 64, 4, 0.2f);

            SetPlayerDefaults();
        }
        public static void NewPlayer()
        {
            Initialize();
            World.LoadContent(@"Textures\Eggplant", 32, 64, 4, 0.2f);
            World.Position = new Vector2(200, 200);
            Combat.LoadContent(@"Textures\Wizard"); //ToDo: position doesn't matter here
            _canMake.Add(6);
            InventoryManager.AddNewItemToInventory(5);
            InventoryManager.AddNewItemToInventory(3);
            InventoryManager.AddNewItemToInventory(4);
            InventoryManager.AddNewItemToInventory(6);
            InventoryManager.AddNewItemToInventory(7);
            InventoryManager.AddNewItemToInventory(8);
            InventoryManager.AddNewItemToInventory(101);
            InventoryManager.AddNewItemToInventory(200);
            InventoryManager.AddNewItemToInventory(80, 10);
            InventoryManager.AddNewItemToInventory(201);

            SetPlayerDefaults();
        }

        public static void SetPlayerDefaults()
        {
            AddToQuestLog(new Quest("Gathering Wood", Quest.QuestGoalType.Fetch, "Getwood, dumbass", new NPC(), 1, null, ObjectManager.GetItem(2)));
            AddToQuestLog(new Quest("Gathering Wood", Quest.QuestGoalType.Fetch, "Getwood, dumbass", new NPC(), 2, null, ObjectManager.GetItem(2)));
            AddToQuestLog(new Quest("Gathering Wood", Quest.QuestGoalType.Fetch, "Getwood, dumbass", new NPC(), 3, null, ObjectManager.GetItem(2)));
            AddToQuestLog(new Quest("Gathering Wood", Quest.QuestGoalType.Fetch, "Getwood, dumbass", new NPC(), 4, null, ObjectManager.GetItem(2)));
            AddToQuestLog(new Quest("Gathering Wood", Quest.QuestGoalType.Fetch, "Getwood, dumbass", new NPC(), 5, null, ObjectManager.GetItem(2)));
            AddToQuestLog(new Quest("Gathering Wood", Quest.QuestGoalType.Fetch, "Getwood, dumbass", new NPC(), 6, null, ObjectManager.GetItem(2)));

            CurrentMap = MapManager.CurrentMap.Name;
            World.Position = Utilities.Normalize(MapManager.Maps[CurrentMap].GetCharacterSpawn("PlayerSpawn"));
            MaxStamina = 50;
            Stamina = MaxStamina;
            Combat.SetClass(CharacterManager.GetClassByIndex(1));
        }

        public static void Update(GameTime gameTime)
        {
            if (GameManager.InCombat()) { UpdateCombat(gameTime); }
            else { UpdateWorld(gameTime); }
        }
        public static void UpdateWorld(GameTime gameTime)
        {
            Vector2 moveVector = Vector2.Zero;
            Vector2 moveDir = Vector2.Zero;
            string animation = "";

            if (UseTool != null)
            {
                UseTool.ToolAnimation.Position = World.Position;
                UseTool.Update(gameTime);
                bool finished = !UseTool.ToolAnimation.IsAnimating;

                if (_targetTile != null && _targetTile.Object != null && finished) 
                {
                    _targetTile.DamageObject(UseTool.DmgValue);
                    _targetTile = null;
                    UseTool = null;
                    _busy = false;
                }
            }
            else
            {
                KeyboardState ks = Keyboard.GetState();
                if (ks.IsKeyDown(Keys.W))
                {
                    World.Facing = WorldCharacter.Direction.North;
                    moveDir += new Vector2(0, -World.Speed);
                    //animation = "Float";
                    moveVector += new Vector2(0, -World.Speed);
                }
                else if (ks.IsKeyDown(Keys.S))
                {
                    World.Facing = WorldCharacter.Direction.South;
                    moveDir += new Vector2(0, World.Speed);
                    //animation = "Float";
                    moveVector += new Vector2(0, World.Speed);
                }

                if (ks.IsKeyDown(Keys.A))
                {
                    World.Facing = WorldCharacter.Direction.East;
                    moveDir += new Vector2(-World.Speed, 0);
                    //animation = "Float";
                    moveVector += new Vector2(-World.Speed, 0);
                }
                else if (ks.IsKeyDown(Keys.D))
                {
                    World.Facing = WorldCharacter.Direction.West;
                    moveDir += new Vector2(World.Speed, 0);
                    //animation = "Float";
                    moveVector += new Vector2(World.Speed, 0);
                }
                
                if (moveDir.Length() != 0)
                {
                    Rectangle testRectX = new Rectangle((int)World.CollisionBox.X + (int)moveDir.X, (int)World.CollisionBox.Y, World.CollisionBox.Width, World.CollisionBox.Height);
                    Rectangle testRectY = new Rectangle((int)World.CollisionBox.X, (int)World.CollisionBox.Y + (int)moveDir.Y, World.CollisionBox.Width, World.CollisionBox.Height);

                    if(MapManager.CurrentMap.CheckForCollisions(World, testRectX, testRectY, ref moveDir))
                    {
                        World.MoveBy((int)moveDir.X, (int)moveDir.Y);
                    }

                    if (World.Sprite.CurrentAnimation != animation)
                    {
                        World.Sprite.CurrentAnimation = animation;
                    }
                }
                else
                {
                    //World.Sprite.CurrentAnimation = "Float" + World.Sprite.CurrentAnimation.Substring(4);
                }
            }
            World.Update(gameTime);
        }
        public static void UpdateCombat(GameTime gameTime)
        {
            
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (GameManager.InCombat()) { DrawCombat(spriteBatch); }
            else { DrawWorld(spriteBatch); }
            _merchantChest.Draw(spriteBatch);
        }
        public static void DrawWorld(SpriteBatch spriteBatch)
        {
            if (_currentMap == MapManager.CurrentMap.Name) {
                World.Draw(spriteBatch, true);
                if (UseTool != null) { UseTool.ToolAnimation.Draw(spriteBatch); }
            }
        }
        public static void DrawCombat(SpriteBatch spriteBatch)
        {
        }

        public static List<CombatAdventurer> GetParty()
        {
            return _party;
        }
        public static void AddToParty(CombatAdventurer c)
        {
            _party.Add(c);
        }
        public static void RemoveFromParty(CombatAdventurer c)
        {
            _party.Remove(c);
        }

        //Random quests should not generate a quest with the same goal as a pre-existing quest
        public static void AddToQuestLog(Quest q)
        {
            foreach(Item i in InventoryManager.PlayerInventory)
            {
                if (i != null) { q.AttemptProgress(i); }
            }
            _questLog.Add(q);
        }
        public static void AdvanceQuestProgress(Object o)
        {
            foreach(Quest q in _questLog)
            {
                if (q.AttemptProgress(o))
                {
                    break;
                }
            }
        }
        public static void RemoveQuestProgress(Item i)
        {
            foreach (Quest q in _questLog)
            {
                if (q.RemoveProgress(i))
                {
                    break;
                }
            }
        }

        public static bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            if (PlayerManager.PlayerInRange(mouseLocation))
            {
                _targetTile = MapManager.RetrieveTile(mouseLocation);
                if (_targetTile.Object != null && _targetTile.Object.IsDestructible())
                {

                    Destructible d = (Destructible)_targetTile.Object;

                    if (d != null && UseTool == null)
                    {
                        if (d.Breakable) { UseTool = _pick; }
                        else if (d.Choppable) { UseTool = _axe; }

                        if (UseTool != null && !_busy)
                        {
                            _busy = true;
                            if (DecreaseStamina(UseTool.StaminaCost))
                            {
                                UseTool.ToolAnimation.IsAnimating = true;
                            }
                            else
                            {
                                UseTool = null;
                            }
                        }
                        rv = true;
                    }
                }
            }

            if(GraphicCursor.HeldItem != null)
            {
                if (GraphicCursor.HeldItem.IsContainer())
                {
                    MapManager.PlaceWorldItem((Container)GraphicCursor.HeldItem, mouseLocation.ToVector2());
                    GraphicCursor.DropItem();
                }

                if (GraphicCursor.HeldItem.IsProcessor())
                {
                    MapManager.PlaceWorldItem((Processor)GraphicCursor.HeldItem, mouseLocation.ToVector2());
                    GraphicCursor.DropItem();
                }
            }

            //if (InventoryManager.CurrentItem != null)
            //{
            //    if (InventoryManager.CurrentItem.Type == Item.ItemType.Container)
            //    {
            //        MapManager.PlaceWorldItem((Container)InventoryManager.CurrentItem, mouseLocation.ToVector2());
            //        InventoryManager.RemoveItemFromInventory(InventoryManager.CurrentItem);
            //    }
            //    else if (InventoryManager.CurrentItem.Type == Item.ItemType.Food)
            //    {
            //        Food f = ((Food)InventoryManager.CurrentItem);
            //        GUIManager.AddTextSelection(f, string.Format("Really eat the {0}? [Yes:Eat|No:DoNothing]", f.Name));
            //    }
            //}

            return rv;
        }

        public static void AddBuilding(WorkerBuilding b)
        {
            _buildings.Add(b);
        }
        public static int GetNewBuildingID()
        {
            return _buildings.Count +1;
        }

        public static bool PlayerInRange(Point centre)
        {
            return PlayerInRange(centre, RHMap.TileSize * 2);
        }
        public static bool PlayerInRange(Vector2 centre, int range)
        {
            return PlayerInRange(centre.ToPoint(), range);
        }
        public static bool PlayerInRange(Vector2 centre, int minRange, int maxRange)
        {
            return PlayerInRange(centre.ToPoint(), minRange, maxRange);
        }
        public static bool PlayerInRange(Point centre, int range)
        {
            bool rv = false;

            Rectangle playerRect = World.GetRectangle();
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            int c = (int)Math.Sqrt(a * a + b * b);

            rv = c <= range;

            return rv;
        }
        public static bool PlayerInRange(Point centre, int minRange, int maxRange)
        {
            bool rv = false;

            Rectangle playerRect = World.GetRectangle();
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            int c = (int)Math.Sqrt(a*a + b*b);

            rv = c > minRange && c <= maxRange;

            return rv;
        }

        public static void TakeMoney(int x)
        {
            _money -= x;
        }
        public static void AddMoney(int x)
        {
            _money += x;
        }
        public static void SetMoney(int x)
        {
            _money = x;
        }

        public static bool DecreaseStamina(int x)
        {
            bool rv = false;
            if (Stamina >= x)
            {
                Stamina -= x;
                rv = true;
            }
            return rv;
        }

        public static void IncreaseStamina(int x)
        {
            if (Stamina + x <= MaxStamina)
            {
                Stamina += x;
            }
            else
            {
                Stamina = MaxStamina;
            }
        }

        public static void Rollover()
        {
            _party.Clear();
            _party.Add(Combat);
        }

        public static void CompareTools(Tool t)
        {
            if (t != null)
            {
                if (t.ToolType == Tool.TypeOfTool.Axe)
                {
                    if (t == _axe) { _axe = InventoryManager.FindTool(Tool.TypeOfTool.Axe); }
                    else
                    {
                        if (_axe == null) { _axe = t; }
                        if (_axe.DmgValue < t.DmgValue) { _axe = t; }
                    }
                }
                else if (t.ToolType == Tool.TypeOfTool.Pick)
                {
                    if (t == _pick) { _pick = InventoryManager.FindTool(Tool.TypeOfTool.Axe); }
                    else
                    {
                        if(_pick == null) { _pick = t; }
                        else if (_pick.DmgValue < t.DmgValue) { _pick = t; }
                    }
                }
            }
        }
    }
}
