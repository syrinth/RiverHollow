using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.CombatStuff;
using RiverHollow.WorldObjects;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.SaveManager;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.WorldObjects.Buildable;
using static RiverHollow.Characters.TravellingNPC;
using RiverHollow.Characters.Lite;
using RiverHollow.Items;

namespace RiverHollow.Game_Managers
{
    public static class PlayerManager
    {
        #region Properties
        public static bool Busy { get; private set; }
        public static List<RHTask> TaskLog { get; private set; }

        public static double MaxStamina = 100;
        public static double Stamina = MaxStamina;
        public static int _iBuildingID = -1;
        private static string _currentMap;
        public static string CurrentMap
        {
            get { return _currentMap; }
            set {
                _currentMap = value;
                PlayerActor.CurrentMapName = _currentMap;
            }
        }

        public static ClassedCombatant PlayerCombatant;
        public static PlayerCharacter PlayerActor;

        public static int HitPoints => PlayerCombatant.CurrentHP;
        public static int MaxHitPoints  => PlayerCombatant.MaxHP;

        private static Dictionary<ObjectTypeEnum, List<int>> _diCrafting;

        public static Building PlayerHome => (Building)GetTownObjectsByID(27)[0];

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

        public static int WeddingCountdown { get; set; } = 0;
        public static Villager Spouse { get; set; }
        public static List<Child> Children { get; set; }

        public static int BabyCountdown { get; set; }

        private static List<int> _liUniqueItemsBought;

        private static ExpectingChildEnum _eChildStatus;
        public static ExpectingChildEnum ChildStatus {
            get => _eChildStatus;
            set {                
                _eChildStatus = value;
                BabyCountdown = _eChildStatus == ExpectingChildEnum.None ? 0 : 7;
            }
        }
        #endregion

        #region Data Collections
        private static Dictionary<int, List<WorldObject>> _diTownObjects;
        #endregion

        public static void Initialize()
        {
            _diCrafting = new Dictionary<ObjectTypeEnum, List<int>>
            {
                [ObjectTypeEnum.Building] = new List<int>(),
                [ObjectTypeEnum.Floor] = new List<int>(),
                [ObjectTypeEnum.Structure] = new List<int>(),
                [ObjectTypeEnum.Plant] = new List<int>(),
                [ObjectTypeEnum.Wall] = new List<int>()
            };

            Children = new List<Child>();
            _liPets = new List<Pet>();
            _liMounts = new List<Mount>();

            _diStorage = new Dictionary<int, int>();
            _diTownObjects = new Dictionary<int, List<WorldObject>>();
            _liUniqueItemsBought = new List<int>();
            _diTools = new Dictionary<ToolEnum, Tool>();

            TaskLog = new List<RHTask>();
            PlayerActor = new PlayerCharacter();
            PlayerCombatant = new ClassedCombatant();

            //Sets a default class so we can load and display the character to start
            PlayerCombatant.SetClass(DataManager.GetClassByIndex(1));

            _liParty = new List<ClassedCombatant> { PlayerCombatant };
        }

        public static void NewPlayer()
        {
            CurrentMap = MapManager.CurrentMap.Name;
            PlayerActor.Position = Util.GetMapPositionOfTile(MapManager.SpawnTile);

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
            PlayerActor.SetPath(list);
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
                    PlayerActor.DetermineFacing(moveDir);
                }

                if (ks.IsKeyDown(Keys.W)) { moveDir += new Vector2(0, -PlayerActor.BuffedSpeed); }
                else if (ks.IsKeyDown(Keys.S)) { moveDir += new Vector2(0, PlayerActor.BuffedSpeed); }

                if (ks.IsKeyDown(Keys.A)) { moveDir += new Vector2(-PlayerActor.BuffedSpeed, 0); }
                else if (ks.IsKeyDown(Keys.D)) { moveDir += new Vector2(PlayerActor.BuffedSpeed, 0); }

                PlayerActor.DetermineFacing(moveDir);

                if (moveDir.Length() != 0)
                {
                    Rectangle testRectX = new Rectangle((int)PlayerActor.CollisionBox.X + (int)moveDir.X, (int)PlayerActor.CollisionBox.Y, PlayerActor.CollisionBox.Width, PlayerActor.CollisionBox.Height);
                    Rectangle testRectY = new Rectangle((int)PlayerActor.CollisionBox.X, (int)PlayerActor.CollisionBox.Y + (int)moveDir.Y, PlayerActor.CollisionBox.Width, PlayerActor.CollisionBox.Height);

                    if (MapManager.CurrentMap.CheckForCollisions(PlayerActor, testRectX, testRectY, ref moveDir))
                    {
                        //Might be technically correct but FEELS wrong
                        //moveDir.Normalize();
                        //moveDir *= World.Speed;
                        PlayerActor.MoveBy((int)moveDir.X, (int)moveDir.Y);
                    }
                }

            }
            else { UpdateTool(gTime); }

            PlayerActor.Update(gTime);
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
                PlayerActor.Draw(spriteBatch, true);
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
        public static void AddToTaskLog(RHTask t)
        {
            if (!t.Finished)
            {
                foreach (Item i in InventoryManager.PlayerInventory) { if (i != null) { t.AttemptProgress(i); } }
                foreach (int k in PlayerManager.GetTownObjects().Keys)
                {
                    t.AttemptStructureBuildProgress(k);
                }

                t.SpawnTaskMobs();
                TaskLog.Add(t);

                GUIManager.NewAlertIcon("New Task");
            }
        }
        public static void AdvanceTaskProgress(WorldObject obj)
        {
            foreach (RHTask q in TaskLog)
            {
                if (q.AttemptStructureBuildProgress(obj.ID))
                {
                    break;
                }
            }
        }
        public static void AdvanceTaskProgress(Monster m)
        {
            foreach (RHTask q in TaskLog)
            {
                if (q.AttemptProgress(m))
                {
                    break;
                }
            }
        }
        public static void AdvanceTaskProgress(Item i)
        {
            foreach(RHTask q in TaskLog)
            {
                if (q.AttemptProgress(i))
                {
                    break;
                }
            }
        }
        public static void TaskProgressEnterBuilding(int buildingID)
        {
            if (buildingID != -1)
            {
                foreach (RHTask q in TaskLog)
                {
                    if (q.TaskProgressEnterBuilding(buildingID))
                    {
                        break;
                    }
                }
            }
        }
        public static void RemoveTaskProgress(Item i)
        {
            foreach (RHTask q in TaskLog)
            {
                if (q.RemoveProgress(i))
                {
                    break;
                }
            }
        }
        public static bool HasTaskID(int taskID)
        {
            return TaskLog.Find(t => t.TaskID == taskID) != null;
        }

        #region Crafting Dictionary
        public static List<int> GetCraftingList(ObjectTypeEnum e)
        {
            return _diCrafting[e];
        }
        public static bool  AddToCraftingDictionary(int id, bool displayAlert = true)
        {
            bool rv = false;

            WorldObject obj = DataManager.CreateWorldObjectByID(id);
            ObjectTypeEnum e = obj.Type;
            switch (obj.Type)
            {
                case ObjectTypeEnum.Building:
                    e = ObjectTypeEnum.Building;
                    break;
                case ObjectTypeEnum.Floor:
                    e = ObjectTypeEnum.Floor;
                    break;
                case ObjectTypeEnum.Beehive:
                case ObjectTypeEnum.Buildable:
                case ObjectTypeEnum.Container:
                case ObjectTypeEnum.Garden:
                case ObjectTypeEnum.Structure:
                case ObjectTypeEnum.Wall:
                    e = ObjectTypeEnum.Structure;
                    break;
                case ObjectTypeEnum.Plant:
                    e = ObjectTypeEnum.Plant;
                    break;
                case ObjectTypeEnum.Wallpaper:
                    e = ObjectTypeEnum.Wallpaper;
                    break;
            }

            if (!_diCrafting[e].Contains(id))
            {
                rv = true;
                _diCrafting[e].Add(id);

                if (displayAlert)
                {
                    GUIManager.NewAlertIcon("New Blueprint Unlocked");
                }
            }

            return rv;
        }
        public static void AddToCraftingDictionary(int[] unlocks)
        {
            bool displayAlert = false;
            for (int i = 0; i < unlocks.Length; i++)
            {
                displayAlert = AddToCraftingDictionary(unlocks[i], false);
            }

            if (displayAlert)
            {
                GUIManager.NewAlertIcon("New " + (unlocks.Length > 1 ? "Blueprints" : "Blueprint") + " Unlocked");
            }
        }
        #endregion

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

        public static void AddToTownObjects(WorldObject obj)
        {
            bool buildable = false;
            switch (obj.Type)
            {
                case ObjectTypeEnum.Building:
                case ObjectTypeEnum.Mailbox:
                case ObjectTypeEnum.Structure:
                case ObjectTypeEnum.Floor:
                case ObjectTypeEnum.Wallpaper:
                case ObjectTypeEnum.Beehive:
                case ObjectTypeEnum.Buildable:
                case ObjectTypeEnum.Decor:
                case ObjectTypeEnum.Garden:
                case ObjectTypeEnum.Wall:
                    buildable = true;
                    break;
            }

            if (buildable)
            {
                if (!_diTownObjects.ContainsKey(obj.ID)) { _diTownObjects[obj.ID] = new List<WorldObject>(); }
                if (!_diTownObjects[obj.ID].Contains(obj))
                {
                    _diTownObjects[obj.ID].Add(obj);
                }
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
        public static bool TownObjectBuilt(int objID)
        {
            return GetNumberTownObjects(objID) > 0;
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
        public static Building GetBuildingByID(int objID)
        {
            Building rv = null;
            if (TownObjectBuilt(objID))
            {
                WorldObject obj = GetTownObjectsByID(objID)[0];
                if (obj.CompareType(ObjectTypeEnum.Building))
                {
                    rv = (Building)obj;
                }
            }

            return rv;
        }
        public static IReadOnlyDictionary<int, List<WorldObject>> GetTownObjects() { return _diTownObjects; }
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

            Rectangle playerRect = PlayerActor.CollisionBox;
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            int c = (int)Math.Sqrt(a * a + b * b);

            rv = c <= range;

            return rv;
        }

        public static bool PlayerInRangeGetDist(Point centre, int range, ref int distance)
        {
            bool rv = false;

            Rectangle playerRect = PlayerActor.GetRectangle();
            int a = Math.Abs(playerRect.Center.X - centre.X);
            int b = Math.Abs(playerRect.Center.Y - centre.Y);
            distance = (int)Math.Sqrt(a * a + b * b);

            rv = distance <= range;

            return rv;
        }

        public static bool PlayerInRange(Point centre, int minRange, int maxRange)
        {
            bool rv = false;

            Rectangle playerRect = PlayerActor.GetRectangle();
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
            PlayerActor.SetName(x);
            PlayerCombatant.SetName(x);
        }
        public static void SetTownName(string x)
        {
            TownName = x;
        }
        public static void SetClass(int x)
        {
            PlayerCombatant.SetClass(DataManager.GetClassByIndex(x));
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
                _liParty.Add(PlayerCombatant);
            }

            if(BabyCountdown > 0)
            {
                BabyCountdown--;
                if (BabyCountdown == 0)
                {
                    ChildStatus = ExpectingChildEnum.None;
                    if (PlayerActor.Pregnant) { PlayerActor.Pregnant = false; }
                    else if (Spouse.Pregnant) { Spouse.Pregnant = false; }

                    Child p = DataManager.CreateChild(10);
                    p.SpawnNearPlayer();
                    Children.Add(p);
                }
            }

            if (WeddingCountdown > 0)
            {
                WeddingCountdown--;
                if (WeddingCountdown == 0)
                {
                    Spouse.Relationship = RelationShipStatusEnum.Married;
                }
            }

            foreach (Pet p in _liPets)
            {
                if (PlayerActor.ActivePet == p) { p.SpawnNearPlayer(); }
                else { p.SpawnInHome(); }
            }

            foreach(Child c in Children) { c.Rollover(); }

            PlayerMailbox.Rollover();
            GameManager.VillagersInTheInn = 0;
        }

        public static void GetStamina(ref double curr, ref double max)
        {
            curr = Stamina;
            max = MaxStamina;
        }

        public static void DetermineBabyAcquisition()
        {
            if (PlayerActor.CanBecomePregnant == Spouse.CanBecomePregnant) { PlayerManager.ChildStatus = ExpectingChildEnum.Adoption; }
            else
            {
                PlayerManager.ChildStatus = ExpectingChildEnum.Pregnant;

                if (PlayerActor.CanBecomePregnant) { PlayerActor.Pregnant = true; }
                else if (Spouse.CanBecomePregnant) { Spouse.Pregnant = true; }
            }
        }

        public static void AddToUniqueBoughtItems(int id)
        {
            Util.AddUniquelyToList(ref _liUniqueItemsBought, id);
        }
        public static bool AlreadyBoughtUniqueItem(int id)
        {
            return _liUniqueItemsBought.Contains(id);
        }

        public static PlayerData SaveData()
        {
            PlayerData data = new PlayerData()
            {
                name = PlayerManager.Name,
                money = PlayerManager.Money,
                totalMoneyEarned = PlayerManager.TotalMoneyEarned,
                bodyTypeIndex = PlayerManager.PlayerActor.BodyType,
                hairColor = PlayerManager.PlayerActor.HairColor,
                hairIndex = PlayerManager.PlayerActor.HairIndex,
                hat = Item.SaveData(PlayerActor.Hat),
                chest = Item.SaveData(PlayerActor.Body),
                adventurerData = PlayerCombatant.SaveClassedCharData(),
                currentClass = PlayerCombatant.CharacterClass.ID,
                weddingCountdown = WeddingCountdown,
                babyCountdown = BabyCountdown,
                Items = new List<ItemData>(),
                Storage = new List<StorageData>(),
                liPets = new List<int>(),
                MountList = new List<int>(),
                ChildList = new List<ChildData>(),
                CraftingList = new List<int>()
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

            data.activePet = PlayerManager.PlayerActor.ActivePet == null ? -1 : PlayerManager.PlayerActor.ActivePet.ID;
            foreach (Pet p in _liPets)
            {
                data.liPets.Add(p.ID);
            }

            foreach (Mount m in _liMounts)
            {
                data.MountList.Add(m.ID);
            }

            foreach (Child c in Children)
            {
                data.ChildList.Add(c.SaveData());
            }

            foreach(List<int> craftList in _diCrafting.Values)
            {
                data.CraftingList.AddRange(craftList);
            }

            for (int i = 0; i < _liUniqueItemsBought.Count; i++)
            {
                if (i == 0) { data.UniqueItemsBought = _liUniqueItemsBought[i].ToString(); }
                else { data.UniqueItemsBought += "|" + _liUniqueItemsBought[i].ToString(); }
            }

            return data;
        }

        public static void LoadData(PlayerData saveData)
        {
            //We've already loaded in the player position
            Vector2 pos = PlayerActor.Position;

            SetName(saveData.name);
            SetMoney(saveData.money);
            TotalMoneyEarned = saveData.totalMoneyEarned;

            BabyCountdown = saveData.babyCountdown;
            WeddingCountdown = saveData.weddingCountdown;

            PlayerActor.SetHairColor(saveData.hairColor);
            PlayerActor.SetHairType(saveData.hairIndex);

            SetClass(saveData.currentClass);
            PlayerCombatant.LoadClassedCharData(saveData.adventurerData);

            PlayerActor.SetClothes((Clothing)DataManager.GetItem(saveData.hat.itemID));
            PlayerActor.SetClothes((Clothing)DataManager.GetItem(saveData.chest.itemID));

            PlayerActor.SetBodyType(saveData.bodyTypeIndex);
            PlayerActor.Position = pos;

            for (int i = 0; i < InventoryManager.maxItemRows; i++)
            {
                for (int j = 0; j < InventoryManager.maxItemColumns; j++)
                {
                    int index = i * InventoryManager.maxItemColumns + j;
                    ItemData item = saveData.Items[index];
                    Item newItem = DataManager.GetItem(item.itemID, item.num);
 
                    if (newItem != null) { newItem.ApplyUniqueData(item.strData); }
                    InventoryManager.AddItemToInventorySpot(newItem, i, j);
                }
            }

            foreach(StorageData storageData in saveData.Storage)
            {
                PlayerManager.AddToStorage(storageData.objID, storageData.number);
            }

            foreach (int i in saveData.liPets)
            {
                Pet p = DataManager.CreatePet(i);
                if (p.ID == saveData.activePet) {
                    p.SpawnNearPlayer();
                    PlayerActor.SetPet(p);
                }
                else { p.SpawnInHome(); }
                AddPet(p);
            }

            foreach (int i in saveData.MountList)
            {
                Mount m = DataManager.CreateMount(i);
                AddMount(m);
            }

            foreach (ChildData data in saveData.ChildList)
            {
                Child m = DataManager.CreateChild(data.childID);
                m.SpawnNearPlayer();
                AddChild(m);
            }

            foreach (int i in saveData.CraftingList)
            {
                AddToCraftingDictionary(i, false);
            }

            if (saveData.UniqueItemsBought != null)
            {
                string[] uniqueSplit = Util.FindParams(saveData.UniqueItemsBought);
                for (int i = 0; i < uniqueSplit.Length; i++)
                {
                    _liUniqueItemsBought.Add(int.Parse(uniqueSplit[i]));
                }
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

                if (target != null && ToolInUse.ToolAnimation.AnimationVerbFinished(VerbEnum.UseTool, PlayerManager.PlayerActor.Facing))
                {
                    if (PlayerManager.ToolIsAxe() || PlayerManager.ToolIsPick() || PlayerManager.ToolIsLantern())
                    {
                        target.DamageObject(PlayerManager.ToolInUse);
                    }
                    else if (PlayerManager.ToolIsScythe())
                    {
                        target.DamageObject(PlayerManager.ToolInUse);

                        if(PlayerManager.PlayerActor.Facing == DirectionEnum.Left || PlayerManager.PlayerActor.Facing == DirectionEnum.Right)
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
                    PlayerActor.PlayAnimation(VerbEnum.Idle, DirectionEnum.Down);
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
                ToolInUse.Position = new Vector2(PlayerActor.Position.X - TILE_SIZE, PlayerActor.Position.Y - (TILE_SIZE * 2));
                if (ToolInUse != null && !Busy)
                {
                    if (DecreaseStamina(ToolInUse.StaminaCost))
                    {
                        PlayerManager.PlayerActor.DetermineFacing(MapManager.CurrentMap.GetTileByPixelPosition(GUICursor.GetWorldMousePosition()));
                        Busy = true;
                        AllowMovement = false;
                        PlayerManager.PlayerActor.PlayAnimationVerb(VerbEnum.Idle);
                        ToolInUse.ToolAnimation.PlayAnimation(VerbEnum.UseTool, PlayerActor.Facing);
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

        public static void AddChild(Child actor) { Children.Add(actor); }
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
