using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    internal static class TownManager
    {
        public static string TownName { get; private set; }

        private static bool _bTravelersCame = false;

        public static int Income { get; private set; }

        public static Building Inn { get; private set; }
        public static Building Home { get; private set; }
        public static Structure Market { get; private set; }

        private static Dictionary<int, List<WorldObject>> _diTownObjects;
        private static Dictionary<int, int> _diStorage;
        public static Dictionary<int, Villager> DIVillagers { get; private set; }
        public static Dictionary<int, Merchant> DIMerchants { get; private set; }
        public static List<Merchant> MerchantQueue;
        public static List<Animal> TownAnimals { get; set; }
        public static List<Traveler> Travelers { get; set; }

        public static Item[,] Inventory { get; private set; }

        public static void Initialize()
        {
            _diTownObjects = new Dictionary<int, List<WorldObject>>();
            _diStorage = new Dictionary<int, int>();
            TownAnimals = new List<Animal>();
            Travelers = new List<Traveler>();
            MerchantQueue = new List<Merchant>();

            Inventory = new Item[Constants.KITCHEN_STOCK_SIZE, Constants.KITCHEN_STOCK_SIZE];

            DIMerchants = new Dictionary<int, Merchant>();
            DIVillagers = new Dictionary<int, Villager>();
            foreach (KeyValuePair<int, Dictionary<string, string>> npcData in DataManager.NPCData)
            {
                Dictionary<string, string> diData = DataManager.NPCData[npcData.Key];
                switch (diData["Type"])
                {
                    case "Merchant":
                        DIMerchants.Add(npcData.Key, new Merchant(npcData.Key, diData));
                        break;

                    case "Villager":
                        DIVillagers.Add(npcData.Key, new Villager(npcData.Key, diData));
                        break;
                    default:
                        break;
                }

            }
        }

        public static void SetTownName(string x)
        {
            TownName = x;
        }

        public static void Rollover()
        {
            if(GameCalendar.DayOfWeek == 0)
            {
                _bTravelersCame = false;
            }

            foreach (Villager v in DIVillagers.Values)
            {
                v.RollOver();
            }
            foreach (Merchant m in DIMerchants.Values)
            {
                if (GetNumberTownObjects(int.Parse(DataManager.Config[15]["ObjectID"])) > 0)
                {
                    m.RollOver();
                }
            }

            foreach (WorldActor npc in TownAnimals)
            {
                npc.RollOver();
            }

            Income = 0;
            if (Travelers.Count > 0)
            {
                InventoryManager.InitExtraInventory(Inventory);

                List<Food> sortedFood = Util.MultiArrayToList(Inventory).ConvertAll(x => (Food)x);
                sortedFood = sortedFood.OrderBy(x => x.FoodType).ThenByDescending(x => x.Value).ToList();
                foreach (Food f in sortedFood)
                {
                    if (f != null)
                    {
                        var set = Travelers.FindAll(npc => f.FoodType == npc.FavoriteFood());
                        set.ForEach(npc => npc.TryEat(f));

                        set = Travelers.FindAll(npc => npc.NeutralFood(f.FoodType));
                        set.ForEach(npc => npc.TryEat(f));

                        set = Travelers.FindAll(npc => f.FoodType == npc.DislikedFood() || f.FoodType == FoodTypeEnum.Forage);
                        set.ForEach(npc => npc.TryEat(f));
                    }
                }

                InventoryManager.ClearExtraInventory();

                Travelers.ForEach(npc => Income += npc.CalculateIncome());
            }

            SpawnTravelers();
        }

        public static void AddToKitchen(Item i)
        {
            InventoryManager.InitExtraInventory(Inventory);
            InventoryManager.AddToInventory(i, false);
            InventoryManager.ClearExtraInventory();
        }

        #region Traveler Code
        private static int IncreasedTravelerChance()
        {
            int rv = 0;
            foreach (var kvp in _diTownObjects)
            {
                Building b = GetBuildingByID(kvp.Key);
                if (b != null)
                {
                    rv += b.GetTravelerChance();
                }
            }
            return rv;
        }
        private static void SpawnTravelers()
        {
            for (int i = 0; i < Travelers.Count; i++)
            {
                MapManager.TownMap.RemoveCharacterImmediately(Travelers[i]);
            }
            Travelers.Clear();

            //Find all Travelers
            var travelerList = new List<Traveler>();
            foreach (KeyValuePair<int, Dictionary<string, string>> kvp in DataManager.NPCData)
            {
                if (kvp.Value["Type"] == Util.GetEnumString(WorldActorTypeEnum.Traveler))
                {
                    Traveler npc = DataManager.CreateTraveler(kvp.Key);
                    if (npc.Validate() || (!npc.Rare() && RHRandom.Instance().RollPercent(10)))
                    {
                        travelerList.Add(npc);
                    }
                }
            }

            int successChance = Constants.BASE_TRAVELER_RATE + IncreasedTravelerChance();
            do {
                //Guaranteed at least one set of Travelers/week
                if ((GameCalendar.DayOfWeek == 6 && !_bTravelersCame) ||  RHRandom.Instance().RollPercent(successChance))
                {
                    _bTravelersCame = true;

                    if (travelerList.Count == 0)
                    {
                        break;
                    }

                    var options = travelerList;
                    CheckForRareTraveler(ref options);

                    Traveler npc = Util.GetRandomItem(options);
                    if (npc != null)
                    {
                        AddTraveler(npc);
                        travelerList.Remove(npc);

                        MakeGroup(ref travelerList, successChance, npc.Group());
                        successChance /= Constants.GROUP_DIVISOR;
                    }
                    else { break; }
                }
                else { break; }
            } while (successChance > Constants.EXTRA_TRAVELER_THRESHOLD);
        }

        private static void MakeGroup(ref List<Traveler> travelerList, int successChance, TravelerGroupEnum group)
        {
            int chainSuccess = successChance * 2;
            do
            {
                if (RHRandom.Instance().RollPercent(chainSuccess))
                {
                    List<Traveler> options;
                    var noneList = travelerList.FindAll(x => x.Group() == TravelerGroupEnum.None);
                    var groupList = travelerList.FindAll(x => x.Group() == group);

                    Traveler npc = null;
                    if (noneList.Count == 0 || groupList.Count == 0)
                    {
                        options = new List<Traveler>();
                        options.AddRange(groupList);
                        options.AddRange(noneList);
                    }
                    else if (group == TravelerGroupEnum.None) { options = travelerList; }
                    else
                    {
                        if (RHRandom.Instance().RollPercent(80)) { options = groupList; }
                        else { options = noneList; }
                    }

                    CheckForRareTraveler(ref options);

                    npc = Util.GetRandomItem(options);
                    if (npc == null)
                    {
                        break;
                    }

                    AddTraveler(npc);
                    travelerList.Remove(npc);

                    //If we're making a group off of a None group, need to transition to any actual group we roll on
                    if (group == TravelerGroupEnum.None)
                    {
                        group = npc.Group();
                    }

                    chainSuccess /= Constants.MEMBER_DIVISOR;
                }
                else { break; }

            } while (chainSuccess > Constants.EXTRA_TRAVELER_THRESHOLD);
        }
        private static void CheckForRareTraveler(ref List<Traveler> options)
        {
            //Check for a rare traveller
            if (options.Any(x => x.Rare()))
            {
                bool getRare = RHRandom.Instance().Next(1, 10) == 10;
                options = options.FindAll(x => x.Rare() == getRare);
            }
        }

        public static void AddTraveler(int id)
        {
            AddTraveler((Traveler)DataManager.CreateNPCByIndex(id));
        }
        public static void AddTraveler(Traveler npc)
        {
            Travelers.Add(npc);

            RHMap map = MapManager.TownMap;
            int roll = RHRandom.Instance().Next(0, 2);
            switch (roll)
            {
                case 0:
                    if (EnvironmentManager.IsRaining())
                    {
                        map = MapManager.Maps[Inn.BuildingMapName];
                    }
                    break;
                case 1:
                    map = MapManager.Maps[Inn.BuildingMapName];
                    break;
                case 2:
                    if (npc.BuildingID() != -1 && TownObjectBuilt(npc.BuildingID()))
                    {
                        map = MapManager.Maps[GetBuildingByID(npc.BuildingID()).BuildingMapName];
                    }
                    else { goto case 0; }
                    break;

            }
            npc.Position = map.GetRandomPosition();
            map.AddActor(npc);
        }
        #endregion

        public static void MoveMerchants()
        {
            if (MerchantQueue.Count > 0)
            {
                Merchant chosenMerchant = MerchantQueue[0];
                chosenMerchant.ArriveInTown();
                MerchantQueue.Remove(chosenMerchant);
            }
        }

        public static void AddAnimal(Animal npc)
        {
            TownAnimals.Add(npc);
            npc.MoveToSpawn();
        }


        #region Town Helpers
        public static int GetPopulation()
        {
            return DIVillagers.Count(x => x.Value.LivesInTown);
        }
        public static int GetTownScore()
        {
            int rv = 0;

            return rv;
        }

        public static Dictionary<int, int> GetStorageItems()
        {
            Dictionary<int, int> rvDictionary = new Dictionary<int, int>();

            foreach (KeyValuePair<int, int> kvp in _diStorage)
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
            if (_diStorage.ContainsKey(itemID))
            {
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
                case ObjectTypeEnum.Container:
                case ObjectTypeEnum.Decor:
                case ObjectTypeEnum.Garden:
                case ObjectTypeEnum.Wall:
                    buildable = true;
                    break;
            }

            if (DataManager.GetBoolByIDKey(obj.ID, "Inn", DataType.WorldObject)) { Inn = (Building)obj; }
            if (DataManager.GetBoolByIDKey(obj.ID, "Home", DataType.WorldObject)) { Home = (Building)obj; }
            if (DataManager.GetBoolByIDKey(obj.ID, "Market", DataType.WorldObject)) { Market = (Structure)obj; }

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
            if (TownObjectBuilt(objID) && DataManager.GetEnumByIDKey<ObjectTypeEnum>(objID, "Type", DataType.WorldObject) == ObjectTypeEnum.Building)
            {
                rv = (Building)GetTownObjectsByID(objID)[0];
            }

            return rv;
        }
        public static IReadOnlyDictionary<int, List<WorldObject>> GetTownObjects() { return _diTownObjects; }
        #endregion

        public static TownData SaveData()
        {
            TownData data = new TownData
            {
                townName = TownName,
                Storage = new List<StorageData>(),
                TownAnimals = new List<int>(),
                Travelers = new List<int>(),
                VillagerData = new List<VillagerData>(),
                MerchantData = new List<MerchantData>(),
                MerchantQueue = new List<int>()
            };


            foreach (KeyValuePair<int, int> kvp in GetStorageItems())
            {
                StorageData storageData = new StorageData
                {
                    objID = kvp.Key,
                    number = kvp.Value
                };
                data.Storage.Add(storageData);
            }

            foreach (WorldActor npc in TownAnimals)
            {
                data.TownAnimals.Add(npc.ID);
            }

            foreach (WorldActor npc in Travelers)
            {
                data.Travelers.Add(npc.ID);
            }

            foreach (Villager npc in DIVillagers.Values)
            {
                data.VillagerData.Add(npc.SaveData());
            }

            foreach (Merchant npc in DIMerchants.Values)
            {
                data.MerchantData.Add(npc.SaveData());
            }

            foreach (Merchant npc in MerchantQueue)
            {
                data.MerchantQueue.Add(npc.ID);
            }

            data.travelersCame = _bTravelersCame;

            return data;
        }
        public static void LoadData(TownData saveData)
        {
            SetTownName(saveData.townName);

            foreach (StorageData storageData in saveData.Storage)
            {
                AddToStorage(storageData.objID, storageData.number);
            }

            foreach (int id in saveData.TownAnimals)
            {
                Animal m = DataManager.CreateAnimal(id);
                AddAnimal(m);
            }

            foreach (int id in saveData.Travelers)
            {
                AddTraveler(id);
            }

            foreach (VillagerData data in saveData.VillagerData)
            {
                Villager npc = DIVillagers[data.npcID];
                npc.LoadData(data);
                npc.MoveToSpawn();
            }

            foreach (MerchantData data in saveData.MerchantData)
            {
                Merchant npc = DIMerchants[data.npcID];
                npc.LoadData(data);
            }

            for (int i = 0; i < saveData.MerchantQueue.Count; i++)
            {
                MerchantQueue.Add(TownManager.DIMerchants[saveData.MerchantQueue[i]]);
            }

            _bTravelersCame = saveData.travelersCame;

            MoveMerchants();
        }
    }
}
