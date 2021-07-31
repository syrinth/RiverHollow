using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.Items;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.Items.Buildable;
using System.Linq;

namespace RiverHollow.Game_Managers
{
    public static class PlayerManager
    {
        #region Properties
        public static bool Busy { get; private set; }
        public static List<Task> TaskLog { get; private set; }

        public static double MaxStamina = 100;
        public static double Stamina = MaxStamina;
        public static int _iBuildingID = -1;
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

        public static int HitPoints => World.CurrentHP;
        public static int MaxHitPoints  => World.MaxHP;

        private static Dictionary<int, Building> _diBuildings;
        public static Dictionary<int, Building>.ValueCollection BuildingList => _diBuildings.Values;
        public static Building PlayerHome => _diBuildings[0];

        public static bool ReadyToSleep = false;

        private static List<Pet> _liPets;
        private static List<Mount> _liMounts;
        private static List<ClassedCombatant> _liParty;
        private static Dictionary<int, int> _diStorage;

        public static string Name;
        public static string TownName;

        public static int TotalMoneyEarned { get; private set; } = 0;
        public static int Money { get; private set; } = 0;

        private static int _iMonsterEnergy = 0;
        public static int MonsterEnergy => _iMonsterEnergy;

        private static int _iMonsterEnergyQueue;

        public static bool AllowMovement = true;

        private static Villager _npcSpouse;
        #endregion

        #region Data Collections
        public static Dictionary<int, BuildInfo> DIBuildInfo;
        private static Dictionary<int, List<WorldObject>> _diTownObjects;
        #endregion

        public static void Initialize()
        {
            _liPets = new List<Pet>();
            _liMounts = new List<Mount>();

            _diStorage = new Dictionary<int, int>();
            _diTownObjects = new Dictionary<int, List<WorldObject>>();
            _diTools = new Dictionary<ToolEnum, Tool>();

            TaskLog = new List<Task>();
            World = new PlayerCharacter();
            _liParty = new List<ClassedCombatant> { World };

            _diBuildings = new Dictionary<int, Building>();
            DIBuildInfo = DataManager.GetBuildInfoList();
        }

        public static void NewPlayer()
        {
            World.Position = new Vector2(200, 200);

            CurrentMap = MapManager.CurrentMap.Name;
            World.Position = Util.GetMapPositionOfTile(MapManager.SpawnTile);

            AddTesting();
        }

        public static void AddTesting()
        {
            Dictionary<string, string> diTesting = DataManager.Config[0];
            string[] splitItemValues = Util.FindParams(diTesting["ItemID"]);
            foreach (string s in splitItemValues)
            {
                string[] splitString = s.Split('-');
                InventoryManager.AddToInventory(int.Parse(splitString[0]), (splitString.Length > 1 ? int.Parse(splitString[1]) : 1), true, true);
            }
        }

        public static void SetPath(List<RHTile> list)
        {
            PlayerManager.AllowMovement = false;
            ReadyToSleep = true;
            World.SetPath(list);
        }

        public static void Update(GameTime gTime)
        {
            Vector2 moveDir = Vector2.Zero;

            AddByIncrement(ref _iMonsterEnergyQueue, ref _iMonsterEnergy);

            if (AllowMovement)
            {
                KeyboardState ks = Keyboard.GetState();
                if (!InputManager.MovementKeyDown())
                {
                    World.DetermineFacing(moveDir);
                }

                if (ks.IsKeyDown(Keys.W)) { moveDir += new Vector2(0, -World.BuffedSpeed); }
                else if (ks.IsKeyDown(Keys.S)) { moveDir += new Vector2(0, World.BuffedSpeed); }

                if (ks.IsKeyDown(Keys.A)) { moveDir += new Vector2(-World.BuffedSpeed, 0); }
                else if (ks.IsKeyDown(Keys.D)) { moveDir += new Vector2(World.BuffedSpeed, 0); }

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
            else { UpdateTool(gTime); }

            World.Update(gTime);
        }

        public static void AddByIncrement(ref int queue, ref int addTo)
        {
            if (queue > 0)
            {
                int cap = 5;
                int toGive = 0;

                if (queue <= cap) { toGive = _iMonsterEnergyQueue; }
                else { toGive = cap; }

                queue -= toGive;
                addTo += toGive;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_currentMap == MapManager.CurrentMap.Name)
            {
                World.Draw(spriteBatch, true);
                ToolInUse?.Draw(spriteBatch);
            }
        }

        public static List<ClassedCombatant> GetParty()
        {
            return _liParty;
        }
        public static void AddToParty(ClassedCombatant c)
        {
            foreach (ClassedCombatant oldChar in _liParty)
            {
                if (oldChar.StartPosition.Equals(c.StartPosition))
                {
                    c.IncreaseStartPos();
                }
            }

            if (!_liParty.Contains(c))
            {
                _liParty.Add(c);
            }
        }
        public static void RemoveFromParty(ClassedCombatant c)
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
            //foreach(Building b in Buildings)
            //{
            //    rv += b.Workers.Count;
            //}

            return rv;
        }

        /// <summary>
        /// Iterates through every building, searching for the adventurer 
        /// with the matching PersonalID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static Adventurer GetWorkerByPersonalID(int id)
        {
            Adventurer adv = null;

            //foreach (Building b in Buildings)
            //{
            //    foreach(Adventurer w in b.Workers)
            //    {
            //        if(w.PersonalID == id)
            //        {
            //            adv = w;
            //            break;
            //        }
            //    }
            //}

            return adv;
        }

        /// <summary>
        /// Adds a Task to the Task Log.
        /// 
        /// First we guard against adding any Task that has been Finished. It should never
        /// happen, but just to be sure.
        /// 
        /// Upon adding a Task to the Task Log, we should see if the Task is complete/nearly complete.
        /// 
        /// Some Tasks may involve the defeat of a Task specific monster. If such a monster exists, spawn it now.
        /// </summary>
        /// <param name="t">The Task to add</param>
        public static void AddToTaskLog(Task t)
        {
            if (!t.Finished)
            {
                foreach (Item i in InventoryManager.PlayerInventory) { if (i != null) { t.AttemptProgress(i); } }
                foreach(BuildInfo bi in PlayerManager.DIBuildInfo.Values) {
                    if (bi.Built) {
                        t.AttemptBuildingProgress(bi.ID);
                    }
                }

                t.SpawnTaskMobs();
                TaskLog.Add(t);

                GUIManager.NewTaskIcon();
            }
        }
        public static void AdvanceTaskProgress(Building b)
        {
            foreach (Task q in TaskLog)
            {
                if (q.AttemptBuildingProgress(b.ID))
                {
                    break;
                }
            }
        }
        public static void AdvanceTaskProgress(Monster m)
        {
            foreach (Task q in TaskLog)
            {
                if (q.AttemptProgress(m))
                {
                    break;
                }
            }
        }
        public static void AdvanceTaskProgress(Item i)
        {
            foreach(Task q in TaskLog)
            {
                if (q.AttemptProgress(i))
                {
                    break;
                }
            }
        }
        public static void RemoveTaskProgress(Item i)
        {
            foreach (Task q in TaskLog)
            {
                if (q.RemoveProgress(i))
                {
                    break;
                }
            }
        }

        #region Town Helpers
        public static int GetTownScore() {
            int rv = 0;

            return rv;
        }

        public static Dictionary<int, int> GetStorageItems()
        {
            Dictionary<int, int> rvDictionary = new Dictionary<int, int>();

            foreach(KeyValuePair<int, int> kvp in _diStorage)
            {
                rvDictionary[kvp.Key] = kvp.Value;
            }

            return rvDictionary;
        }
        public static void AddToStorage(int itemID, int num = 1)
        {
            if (_diStorage.ContainsKey(itemID)) { _diStorage[itemID] += num; }
            else { _diStorage[itemID] = num; }
        }
        public static bool HasInStorage(int itemID) { return _diStorage.ContainsKey(itemID) && _diStorage[itemID] > 0; }
        public static void RemoveFromStorage(int itemID)
        {
            if (_diStorage.ContainsKey(itemID)) {
                _diStorage[itemID]--;

                if (_diStorage[itemID] == 0)
                {
                    _diStorage.Remove(itemID);
                }
            }
        }

        public static void AddToTownObjects(WorldObject obj) {

            if (!_diTownObjects.ContainsKey(obj.ID)) { _diTownObjects[obj.ID] = new List<WorldObject>(); }
            if (!_diTownObjects[obj.ID].Contains(obj))
            {
                _diTownObjects[obj.ID].Add(obj);
            }
        }
        public static void RemoveTownObjects(WorldObject obj)
        {
            if (!_diTownObjects[obj.ID].Contains(obj))
            {
                _diTownObjects[obj.ID].Add(obj);
            }
        }
        public static int GetNumberTownObjects(int objID)
        {
            int rv = 0;

            if (_diTownObjects.ContainsKey(objID))
            {
                rv = _diTownObjects[objID].Count;
            }
            return rv;
        }
        public static List<WorldObject> GetTownObjectsByID(int objID)
        {
            List<WorldObject> rv = new List<WorldObject>();

            if (_diTownObjects.ContainsKey(objID))
            {
                rv = _diTownObjects[objID];
            }
            return rv;
        }
        public static IReadOnlyDictionary<int, List<WorldObject>> GetTownObejcts() { return _diTownObjects; }

        public static void AddBuilding(Building b)
        {
            if (!_diBuildings.ContainsKey(b.ID))
            {
                _diBuildings.Add(b.ID, b);
            }
            PlayerManager.DIBuildInfo[b.ID].Built = true;

            AdvanceTaskProgress(b);
        }
        public static void RemoveBuilding(Building b)
        {
           // Buildings.Remove(b);
        }
        public static Building GetBuildingByID(int id)
        {
            return _diBuildings[id];
        }
        public static bool IsBuilt(int id) { return _diBuildings.ContainsKey(id); }
        public static int GetNewBuildingID()
        {
            return _diBuildings.Count +1 ;
        }
        #endregion

        #region PlayerInRange
        public static bool PlayerInRange(Rectangle rect)
        {
            int hypotenuse = (int)Math.Sqrt(TILE_SIZE * TILE_SIZE + TILE_SIZE * TILE_SIZE);
            return PlayerInRange(rect, hypotenuse);
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
            int hypotenuse = (int)Math.Sqrt(TILE_SIZE*TILE_SIZE + TILE_SIZE*TILE_SIZE);
            return PlayerInRange(centre, hypotenuse);
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
            int c = (int)Math.Sqrt(a * a + b * b);

            rv = c > minRange && c <= maxRange;

            return rv;
        }
        #endregion

        public static bool ExpendResources(Dictionary<int, int> requiredItems)
        {
            bool rv = false;
            if (InventoryManager.HasSufficientItems(requiredItems))
            {
                rv = true;
                foreach (KeyValuePair<int, int> kvp in requiredItems)
                {
                    InventoryManager.RemoveItemsFromInventory(kvp.Key, kvp.Value);
                }
            }

            return rv;
        }

        public static void TakeMoney(int x)
        {
            Money -= x;
        }
        public static void AddMoney(int x)
        {
            Money += x;
            TotalMoneyEarned += x;
        }
        public static void SetMoney(int x)
        {
            Money = x;
        }
        public static void SetName(string x)
        {
            Name = x;
            World.SetName(x);
        }
        public static void SetTownName(string x)
        {
            TownName = x;
        }
        public static void SetClass(int x)
        {
            CharacterClass combatClass = DataManager.GetClassByIndex(x);
            World.SetClass(combatClass);
        }

        public static bool DecreaseStamina(double x)
        {
            bool rv = false;
            if (Stamina >= x)
            {
                Stamina -= x;
                rv = true;
            }
            return rv;
        }

        public static void IncreaseStamina(double x)
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
                _liParty.Add(World);
            }

            foreach (Building b in PlayerManager._diBuildings.Values)
            {
                b.Rollover();
            }

            foreach(Pet p in _liPets)
            {
                if (World.ActivePet == p) { p.SpawnNearPlayer(); }
                else { p.SpawnInHome(); }
            }

            PlayerMailbox.Rollover();
            GameManager.VillagersInTheInn = 0;
        }

        public static void GetStamina(ref double curr, ref double max)
        {
            curr = Stamina;
            max = MaxStamina;
        }

        public static PlayerData SaveData()
        {
            PlayerData data = new PlayerData()
            {
                name = PlayerManager.Name,
                money = PlayerManager.Money,
                totalMoneyEarned = PlayerManager.TotalMoneyEarned,
                bodyTypeIndex = PlayerManager.World.BodyType,
                hairColor = PlayerManager.World.HairColor,
                hairIndex = PlayerManager.World.HairIndex,
                hat = Item.SaveData(World.Hat),
                chest = Item.SaveData(World.Body),
                adventurerData = World.SaveClassedCharData(),
                currentClass = World.CharacterClass.ID,
                Items = new List<ItemData>(),
                Storage = new List<StorageData>(),
                liPets = new List<int>(),
                liMounts = new List<int>()
            };

            // Initialize the new data values.
            foreach (Item i in InventoryManager.PlayerInventory)
            {
                ItemData itemData = Item.SaveData(i);
                data.Items.Add(itemData);
            }

            foreach (KeyValuePair<int, int> kvp in PlayerManager.GetStorageItems())
            {
                StorageData storageData = new StorageData
                {
                    objID = kvp.Key,
                    number = kvp.Value
                };
                data.Storage.Add(storageData);
            }

            data.activePet = PlayerManager.World.ActivePet == null ? -1 : PlayerManager.World.ActivePet.ID;
            foreach (Pet p in _liPets)
            {
                data.liPets.Add(p.ID);
            }

            foreach (Mount m in _liMounts)
            {
                data.liMounts.Add(m.ID);
            }

            return data;
        }

        public static void LoadData(PlayerData data)
        {
            //We've already loaded in the player position
            Vector2 pos = World.Position;

            SetName(data.name);
            SetMoney(data.money);
            TotalMoneyEarned = data.totalMoneyEarned;

            World.SetHairColor(data.hairColor);
            World.SetHairType(data.hairIndex);

            SetClass(data.currentClass);
            World.LoadClassedCharData(data.adventurerData);

            World.SetClothes((Clothes)DataManager.GetItem(data.hat.itemID));
            World.SetClothes((Clothes)DataManager.GetItem(data.chest.itemID));

            World.SetBodyType(data.bodyTypeIndex);
            World.Position = pos;

            for (int i = 0; i < InventoryManager.maxItemRows; i++)
            {
                for (int j = 0; j < InventoryManager.maxItemColumns; j++)
                {
                    int index = i * InventoryManager.maxItemColumns + j;
                    ItemData item = data.Items[index];
                    Item newItem = DataManager.GetItem(item.itemID, item.num);
 
                    if (newItem != null) { newItem.ApplyUniqueData(item.strData); }
                    InventoryManager.AddItemToInventorySpot(newItem, i, j);
                }
            }

            foreach(StorageData storageData in data.Storage)
            {
                PlayerManager.AddToStorage(storageData.objID, storageData.number);
            }

            foreach (int i in data.liPets)
            {
                Pet p = (Pet)DataManager.GetNPCByIndex(i);
                if (p.ID == data.activePet) {
                    p.SpawnNearPlayer();
                    World.SetPet(p);
                }
                else { p.SpawnInHome(); }
                AddPet(p);
            }

            foreach (int i in data.liMounts)
            {
                Mount m = (Mount)DataManager.GetNPCByIndex(i);
                AddMount(m);
            }
        }

        /// <summary>
        /// Adds the indicated amount of energy to the Player
        /// </summary>
        public static void AddMonsterEnergyToQueue(int i)
        {
            _iMonsterEnergyQueue += i;
        }

        /// <summary>
        /// Removes the indicated amount of energy from the Player if they have it
        /// </summary>
        public static bool RemoveMonsterEnergy(int i)
        {
            bool rv = false;
            if (_iMonsterEnergy >= i)
            {
                rv = true;
                _iMonsterEnergy += i;
            }
            return rv;
        }

        #region Tool Management
        public static Tool ToolInUse;
        private static Dictionary<ToolEnum, Tool> _diTools;

        private static void UpdateTool(GameTime gTime)
        {
            if (ToolInUse != null)
            {
                ToolInUse.Update(gTime);

                RHTile target = MapManager.CurrentMap.TargetTile;

                if (target != null && ToolInUse.ToolAnimation.AnimationVerbFinished(VerbEnum.UseTool, PlayerManager.World.Facing))
                {
                    if (PlayerManager.ToolIsAxe() || PlayerManager.ToolIsPick() || PlayerManager.ToolIsLantern())
                    {
                        target.DamageObject(PlayerManager.ToolInUse);
                    }
                    else if (PlayerManager.ToolIsScythe())
                    {
                        target.DamageObject(PlayerManager.ToolInUse);

                        if(PlayerManager.World.Facing == DirectionEnum.Left || PlayerManager.World.Facing == DirectionEnum.Right)
                        {
                            target.GetTileByDirection(DirectionEnum.Up).DamageObject(PlayerManager.ToolInUse);
                            target.GetTileByDirection(DirectionEnum.Down).DamageObject(PlayerManager.ToolInUse);
                        }
                        else
                        {
                            target.GetTileByDirection(DirectionEnum.Left).DamageObject(PlayerManager.ToolInUse);
                            target.GetTileByDirection(DirectionEnum.Right).DamageObject(PlayerManager.ToolInUse);
                        }
                    }
                    else if (PlayerManager.ToolIsWateringCan() && target.Flooring != null)
                    {
                        //target.Water(true);
                    }

                    target = null;
                    PlayerManager.FinishedWithTool();
                    World.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
                }
            }
        }

        /// <summary>
        /// Given a Tool, compare is against the current tools, if it's better than the
        /// old tool of it's type, replace the tool.
        /// 
        /// This should always be the case, since there should only ever be one instance
        /// of each tool level in the game, but safety.
        /// </summary>
        /// <param name="newTool">The prospective new tool</param>
        public static void AddTool(Tool newTool)
        {
            if(CompareTools(newTool))
            {
                _diTools[newTool.ToolType] = newTool;
            }
        }

        /// <summary>
        /// Performs a comparison between the new tool and the original tool.
        /// 
        /// Returns True when there is no tool in that slot, or the newTools level is higher than the original's
        /// </summary>
        /// <param name="newTool">The prospective new tool</param>
        /// <returns>True if the newTool is better or there is no original tool.</returns>
        private static bool CompareTools(Tool newTool)
        {
            return (RetrieveTool(newTool.ToolType) == null) || newTool.ToolLevel > RetrieveTool(newTool.ToolType).ToolLevel;
        }

        /// <summary>
        /// Given a ToolEnum type, determine which managed Tool to return
        /// </summary>
        /// <param name="toolType">The type of tool to retrieve</param>
        /// <returns>The managed Tool held by the PlayerManager</returns>
        public static Tool RetrieveTool(ToolEnum toolType)
        {
            Tool rv = null;

            if (_diTools.ContainsKey(toolType)) {
                rv = _diTools[toolType];
            }

            return rv;
        }

        public static void SetTool(Tool t)
        {
            if (t != null && ToolInUse == null)
            {
                ToolInUse = t;
                ToolInUse.Position = new Vector2(World.Position.X - TILE_SIZE, World.Position.Y - (TILE_SIZE * 2));
                if (ToolInUse != null && !Busy)
                {
                    if (DecreaseStamina(ToolInUse.StaminaCost))
                    {
                        PlayerManager.World.DetermineFacing(MapManager.CurrentMap.GetTileByPixelPosition(GUICursor.GetWorldMousePosition()));
                        Busy = true;
                        AllowMovement = false;
                        PlayerManager.World.PlayAnimationVerb(VerbEnum.UseTool);
                        ToolInUse.ToolAnimation.PlayAnimation(VerbEnum.UseTool, World.Facing);
                    }
                    else
                    {
                        ToolInUse = null;
                    }
                }
            }
        }

        /// <summary>
        /// Called when we're finished with the Tool.
        /// </summary>
        public static void FinishedWithTool()
        {
            PlayerManager.ToolInUse = null;
            Busy = false;
            AllowMovement = true;
        }

        public static bool ToolIsAxe() { return ToolInUse.ToolType == ToolEnum.Axe; }
        public static bool ToolIsPick() { return ToolInUse.ToolType == ToolEnum.Pick; }
        public static bool ToolIsLantern() { return ToolInUse.ToolType == ToolEnum.Lantern; }
        public static bool ToolIsScythe() { return ToolInUse.ToolType == ToolEnum.Scythe; }
        public static bool ToolIsShovel() { return ToolInUse.ToolType == ToolEnum.Shovel; }
        public static bool ToolIsWateringCan() { return ToolInUse.ToolType == ToolEnum.WateringCan; }

        public static ToolData SaveToolData()
        {
            ToolData d = new ToolData()
            {
                pickID = _diTools[ToolEnum.Pick].ItemID,
                axeID = _diTools[ToolEnum.Axe].ItemID,
                scytheID = _diTools[ToolEnum.Scythe].ItemID,
            };

            return d;
        }
        public static void LoadToolData(ToolData d)
        {
            _diTools[ToolEnum.Pick] = (Tool)DataManager.GetItem(d.pickID);
            _diTools[ToolEnum.Axe] = (Tool)DataManager.GetItem(d.axeID);
            _diTools[ToolEnum.Scythe] = (Tool)DataManager.GetItem(d.scytheID);
        }

        #endregion

        public static void AddPet(Pet actor) { _liPets.Add(actor); }
        public static void AddMount(Mount actor) { _liMounts.Add(actor); }
        public static void SpawnMounts()
        {
            foreach(Mount actor in _liMounts)
            {
                actor.SpawnInHome();
            }
        }

        public static Mailbox PlayerMailbox;
    }
}
