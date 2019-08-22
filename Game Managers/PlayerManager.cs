using RiverHollow.WorldObjects;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using RiverHollow.Actors;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Actors.CombatStuff;
using RiverHollow.Misc;
using RiverHollow.GUIObjects;
using static RiverHollow.WorldObjects.WorldItem;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Buildings;

namespace RiverHollow.Game_Managers
{
    public static class PlayerManager
    {
        #region Properties
        private static bool _bBusy;
        public static bool Busy => _bBusy;
        public static Tool UseTool;

        private static List<Quest> _questLog;
        public static List<Quest> QuestLog { get => _questLog; }
        
        public static int Stamina = 50;
        public static int MaxStamina = 50;
        public static string _sBuildingID = string.Empty;
        private static List<int> _canMake;
        public static List<int> CanMake { get => _canMake; }
        private static string _currentMap;
        public static string CurrentMap
        {
            get { return _currentMap; }
            set {
                _currentMap = value;
                World.CurrentMapName = _currentMap;
            }
        }

        public static PlayerCharacter World;
        private static Tool _pick;
        private static Tool _axe;
        private static Tool _shovel;
        private static Tool _wateringCan;

        public static CombatAdventurer Combat;
        public static int HitPoints { get => Combat.CurrentHP; }
        public static int MaxHitPoints { get => Combat.MaxHP; }

        private static List<Building> _liBuildings;
        public static List<Building> Buildings { get => _liBuildings; }

        public static bool ReadyToSleep = false;

        private static List<CombatAdventurer> _liParty;

        public static MerchantChest _merchantChest;
        public static string Name;
        public static string ManorName;

        private static int _money = 2000;
        public static int Money { get => _money; }

        public static bool AllowMovement = true;

        private static EligibleNPC _marriedTo;
        #endregion

        public static void Initialize()
        {
            _liParty = new List<CombatAdventurer>();
            _questLog = new List<Quest>();
            World = new PlayerCharacter();
            Combat = new PlayerCombat(World);
            _liParty.Add(Combat);
            _liBuildings = new List<Building>();
            _canMake = new List<int>();

            World.LoadContent(@"Textures\texPlayer");
        }

        public static void NewPlayer()
        {
            World.Position = new Vector2(200, 200);
            _canMake.Add(190);

            CurrentMap = MapManager.CurrentMap.Name;
            World.Position = Util.SnapToGrid(MapManager.Maps[CurrentMap].GetCharacterSpawn("PlayerSpawn"));

            AddTesting();
        }

        public static void AddTesting()
        {
            InventoryManager.AddToInventory(0, 990);
            InventoryManager.AddToInventory(2, 990);
            InventoryManager.AddToInventory(240, 99);
            InventoryManager.AddToInventory(20);
            InventoryManager.AddToInventory(21);
            InventoryManager.AddToInventory(22);
            InventoryManager.AddToInventory(24);
            InventoryManager.AddToInventory(629);


            AddToQuestLog(new Quest("Gathering Wood", Quest.QuestType.Fetch, "Getwood, dumbass", 1, null, ObjectManager.GetItem(2)));
        }

        public static void SetPath(List<RHTile> list)
        {
            PlayerManager.AllowMovement = false;
            ReadyToSleep = true;
            World.SetPath(list);
        }

        public static void Update(GameTime gameTime)
        {
            if (GameManager.InCombat()) { UpdateCombat(gameTime); }
            else { UpdateWorld(gameTime); }
        }

        public static void UpdateWorld(GameTime gameTime)
        {
            Vector2 moveDir = Vector2.Zero;

            if (AllowMovement)
            {
                if(UseTool == null)
                {
                    KeyboardState ks = Keyboard.GetState();
                    if (ks.IsKeyDown(Keys.W))
                    {
                        moveDir += new Vector2(0, -World.Speed);
                    }
                    else if (ks.IsKeyDown(Keys.S))
                    {
                        moveDir += new Vector2(0, World.Speed);
                    }

                    if (ks.IsKeyDown(Keys.A))
                    {
                        moveDir += new Vector2(-World.Speed, 0);
                    }
                    else if (ks.IsKeyDown(Keys.D))
                    {
                        moveDir += new Vector2(World.Speed, 0);
                    }

                    World.DetermineFacing(moveDir);

                    if (moveDir.Length() != 0)
                    {
                        Rectangle testRectX = new Rectangle((int)World.CollisionBox.X + (int)moveDir.X, (int)World.CollisionBox.Y, World.CollisionBox.Width, World.CollisionBox.Height);
                        Rectangle testRectY = new Rectangle((int)World.CollisionBox.X, (int)World.CollisionBox.Y + (int)moveDir.Y, World.CollisionBox.Width, World.CollisionBox.Height);

                        if (MapManager.CurrentMap.CheckForCollisions(World, testRectX, testRectY, ref moveDir))
                        {
                            //Might be technically correct but FEELS wrong
                            //moveDir.Normalize();
                            //moveDir *= World.Speed;
                            World.MoveBy((int)moveDir.X, (int)moveDir.Y);
                        }
                    }
                }
                else
                {
                    UseTool.Update(gameTime);

                    bool finished = !World.BodySprite.CurrentAnimation.StartsWith("Tool");

                    RHTile target = MapManager.CurrentMap.TargetTile;
                    //UseTool
                    if (target != null && finished)
                    {
                        if (target.WorldObject != null && (PlayerManager.ToolIsAxe() || PlayerManager.ToolIsPick()))
                        {
                            target.DamageObject(PlayerManager.UseTool.DmgValue);
                        }
                        else if (PlayerManager.ToolIsShovel())
                        {
                            target.Dig();
                            MapManager.CurrentMap.ModTiles.Add(target);
                        }
                        else if (PlayerManager.ToolIsWateringCan())
                        {
                            target.Water(true);
                        }

                        target = null;
                        PlayerManager.UnsetTool();
                        World.PlayAnimation(WActorBaseAnim.IdleDown);
                    }
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
        }
        public static void DrawWorld(SpriteBatch spriteBatch)
        {
            if (_currentMap == MapManager.CurrentMap.Name) {
                World.Draw(spriteBatch, true);
                if (UseTool != null) {
                    UseTool.Draw(spriteBatch);
                }
            }
        }
        public static void DrawCombat(SpriteBatch spriteBatch)
        {
        }

        public static List<CombatAdventurer> GetParty()
        {
            return _liParty;
        }
        public static void AddToParty(CombatAdventurer c)
        {
            foreach(CombatActor oldChar in _liParty)
            {
                if (oldChar.StartPos.Equals(c.StartPos))
                {
                    c.IncreaseStartPos();
                }
            }

            if (!_liParty.Contains(c))
            {
                _liParty.Add(c);
            }
        }
        public static void RemoveFromParty(CombatAdventurer c)
        {
            if (_liParty.Contains(c))
            {
                _liParty.Remove(c);
            }
        }

        /// <summary>
        /// Iterates through all buildings owned by the Player, and 
        /// adds all of the workers to the total number.
        /// </summary>
        /// <returns>The total number of workers</returns>
        public static int GetTotalWorkers()
        {
            int rv = 0;
            foreach(Building b in _liBuildings)
            {
                rv += b.Workers.Count;
            }

            return rv;
        }

        /// <summary>
        /// Iterates through every building, searching for the adventurer 
        /// with the matching PersonalID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static WorldAdventurer GetWorkerByPersonalID(int id)
        {
            WorldAdventurer adv = null;

            foreach (Building b in _liBuildings)
            {
                foreach(WorldAdventurer w in b.Workers)
                {
                    if(w.PersonalID == id)
                    {
                        adv = w;
                        break;
                    }
                }
            }

            return adv;
        }
        public static int GetDuelistsInParty()
        {
            return GetClassInParty(5);
        }
        public static int GetKnightsInParty()
        {
            return GetClassInParty(6);
        }
        public static int GetRoguesInParty()
        {
            return GetClassInParty(7);
        }
        public static int GetBardsInParty()
        {
            return GetClassInParty(8);
        }
        public static int GetClassInParty(int classID)
        {
            int rv = 0;
            foreach (CombatAdventurer c in _liParty)
            {
                if (c.CharacterClass.ID == classID)
                {
                    rv++;
                }
            }

            return rv;
        }

        //Random quests should not generate a quest with the same goal as a pre-existing quest
        public static void AddToQuestLog(Quest q)
        {
            foreach(Item i in InventoryManager.PlayerInventory)
            {
                if (i != null) { q.AttemptProgress(i); }
            }
            q.SpawnMobs();
            _questLog.Add(q);
        }
        public static void AdvanceQuestProgress(Monster m)
        {
            foreach (Quest q in _questLog)
            {
                if (q.AttemptProgress(m))
                {
                    break;
                }
            }
        }
        public static void AdvanceQuestProgress(Item i)
        {
            foreach(Quest q in _questLog)
            {
                if (q.AttemptProgress(i))
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

        public static bool SetTool(Tool.ToolEnum toolType, Point mouse)
        {
            bool rv = false;
            PlayerManager.World.Idle();

            Tool t = null;
            switch (toolType)
            {
                case Tool.ToolEnum.Axe:
                    t = _axe;
                    break;
                case Tool.ToolEnum.Pick:
                    t = _pick;
                    break;
                case Tool.ToolEnum.Shovel:
                    t = _shovel;
                    break;
                case Tool.ToolEnum.WateringCan:
                    t = _wateringCan;
                    break;
            }
            if (t != null && UseTool == null)
            {
                rv = true;
                UseTool = t;
                UseTool.Position = World.BodyPosition;
                if (UseTool != null && !_bBusy)
                {
                    _bBusy = true;
                    if (DecreaseStamina(UseTool.StaminaCost))
                    {
                        UseTool.ToolAnimation.IsAnimating = true;
                        PlayerManager.World.PlayAnimation(WActorBaseAnim.ToolDown);
                    }
                    else
                    {
                        UseTool = null;
                    }
                }
            }

            return rv;
        }
        public static void UnsetTool()
        {
            PlayerManager.UseTool = null;
            _bBusy = false;
        }

        public static void AddBuilding(Building b)
        {
            _liBuildings.Add(b);
        }
        public static void RemoveBuilding(Building b)
        {
            _liBuildings.Remove(b);
        }
        public static int GetNewBuildingID()
        {
            return _liBuildings.Count +1 ;
        }

        public static bool PlayerInRange(Rectangle rect)
        {
            return PlayerInRange(rect, TileSize);
        }
        public static bool PlayerInRange(Rectangle rect, int range)
        {
            bool rv = false;
            if (PlayerInRange(new Vector2(rect.Center.X, rect.Top), range))
            {
                rv = true;
            }
            else if (PlayerInRange(new Vector2(rect.Center.X, rect.Bottom), range))
            {
                rv = true;
            }
            else if (PlayerInRange(new Vector2(rect.Left, rect.Center.Y), range))
            {
                rv = true;
            }
            else if (PlayerInRange(new Vector2(rect.Right, rect.Center.Y), range))
            {
                rv = true;
            }

            return rv;
        }

        public static bool PlayerInRange(Point centre)
        {
            return PlayerInRange(centre, TileSize*2);
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

            Rectangle playerRect = World.CollisionBox;
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            int c = (int)Math.Sqrt(a * a + b * b);

            rv = c <= range;

            return rv;
        }

        public static bool PlayerInRangeGetDist(Point centre, int range, ref int distance)
        {
            bool rv = false;

            Rectangle playerRect = World.GetRectangle();
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            distance = (int)Math.Sqrt(a * a + b * b);

            rv = distance <= range;

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
        public static void SetName(string x)
        {
            Name = x;
            Combat.SetName(x);
        }
        public static void SetManorName(string x)
        {
            ManorName = x;
        }
        public static void SetClass(int x)
        {
            CharacterClass combatClass = ObjectManager.GetClassByIndex(x);
            Combat.SetClass(combatClass);
            Combat.LoadContent(GameContentManager.PLAYER_FOLDER + "Player" + combatClass.Name);
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
            if (GameManager.AutoDisband)
            {
                _liParty.Clear();
                _liParty.Add(Combat);
            }
        }

        public static void CompareTools(Tool t)
        {
            if (t != null)
            {
                if (t.ToolType == Tool.ToolEnum.Axe)
                {
                    if (t == _axe) { _axe = InventoryManager.FindTool(Tool.ToolEnum.Axe); }
                    else
                    {
                        if (_axe == null) { _axe = t; }
                        if (_axe.DmgValue < t.DmgValue) { _axe = t; }
                    }
                }
                else if (t.ToolType == Tool.ToolEnum.Pick)
                {
                    if (t == _pick) { _pick = InventoryManager.FindTool(Tool.ToolEnum.Axe); }
                    else
                    {
                        if(_pick == null) { _pick = t; }
                        else if (_pick.DmgValue < t.DmgValue) { _pick = t; }
                    }
                }
                else if (t.ToolType == Tool.ToolEnum.Shovel)
                {
                    if (t == _shovel) { _shovel = InventoryManager.FindTool(Tool.ToolEnum.Shovel); }
                    else
                    {
                        if (_shovel == null) { _shovel = t; }
                        //else if (_pick.DmgValue < t.DmgValue) { _pick = t; }
                    }
                }
                else if (t.ToolType == Tool.ToolEnum.WateringCan)
                {
                    if (t == _wateringCan) { _wateringCan = InventoryManager.FindTool(Tool.ToolEnum.WateringCan); }
                    else
                    {
                        if (_wateringCan == null) { _wateringCan = t; }
                        //else if (_pick.DmgValue < t.DmgValue) { _pick = t; }
                    }
                }
            }
        }

        public static void GetStamina(ref int curr, ref int max)
        {
            curr = Stamina;
            max = MaxStamina;
        }

        public static PlayerData SaveData()
        {
            PlayerData d = new PlayerData()
            {
                name = PlayerManager.Name,
                money = PlayerManager.Money,
                hairColor = PlayerManager.World.HairColor,
                hairIndex = PlayerManager.World.HairIndex,
                hat = Item.SaveData(World.Hat),
                chest = Item.SaveData(World.Shirt),
                adventurerData = Combat.SaveData(),
                currentClass = PlayerManager.Combat.CharacterClass.ID,
                Items = new List<ItemData>()
            };

            return d;
        }

        public static void LoadData(PlayerData data)
        {
            SetName(data.name);
            SetMoney(data.money);
            World.SetHairColor(data.hairColor);
            World.SetHairType(data.hairIndex);

            SetClass(data.currentClass);
            Combat.LoadData(data.adventurerData);

            World.SetClothes((Clothes)ObjectManager.GetItem(data.hat.itemID));
            World.SetClothes((Clothes)ObjectManager.GetItem(data.chest.itemID));

            for (int i = 0; i < InventoryManager.maxItemRows; i++)
            {
                for (int j = 0; j < InventoryManager.maxItemColumns; j++)
                {
                    int index = i * InventoryManager.maxItemColumns + j;
                    ItemData item = data.Items[index];
                    Item newItem = ObjectManager.GetItem(item.itemID, item.num);
 
                    if (newItem != null) { newItem.ApplyUniqueData(item.strData); }
                    InventoryManager.AddItemToInventorySpot(newItem, i, j);
                }
            }
        }

        public static bool ToolIsAxe() { return UseTool == _axe; }
        public static bool ToolIsPick() { return UseTool == _pick; }
        public static bool ToolIsShovel() { return UseTool == _shovel; }
        public static bool ToolIsWateringCan() { return UseTool == _wateringCan; }
    }
}
