using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    internal static class TownManager
    {
        public static string TownName { get; private set; }

        private static bool _bTravelersCame = false;
        private static int _iTravelerBonus = 0;

        public static int Income { get; private set; }

        public static Building Inn { get; private set; }
        public static Building Home { get; private set; }
        public static Structure Market { get; private set; }

        private static Dictionary<int, List<WorldObject>> _diTownObjects;
        public static Dictionary<int, Villager> DIVillagers { get; private set; }
        public static Dictionary<int, Merchant> DIMerchants { get; private set; }
        public static Dictionary<int, ValueTuple<bool, int>> DITravelerInfo { get; private set; }
        public static Dictionary<int, ValueTuple<bool, bool>> DIArchive { get; private set; }
        public static List<Merchant> MerchantQueue;
        public static List<Animal> TownAnimals { get; set; }
        public static List<Traveler> Travelers { get; set; }

        public static Item[,] Inventory { get; private set; }

        public static void Initialize()
        {
            _diTownObjects = new Dictionary<int, List<WorldObject>>();
            TownAnimals = new List<Animal>();
            Travelers = new List<Traveler>();
            MerchantQueue = new List<Merchant>();

            Inventory = new Item[Constants.KITCHEN_STOCK_SIZE, Constants.KITCHEN_STOCK_SIZE];

            DIMerchants = new Dictionary<int, Merchant>();
            DIVillagers = new Dictionary<int, Villager>();
            foreach (KeyValuePair<int, Dictionary<string, string>> npcData in DataManager.ActorData)
            {
                Dictionary<string, string> diData = DataManager.ActorData[npcData.Key];
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

            DITravelerInfo = new Dictionary<int, ValueTuple<bool, int>>();
            foreach(KeyValuePair<int, Dictionary<string, string>> kvp in DataManager.ActorData)
            {
                if (kvp.Value["Type"] == Util.GetEnumString(ActorTypeEnum.Traveler))
                {
                    DITravelerInfo[kvp.Key] = new ValueTuple<bool, int>(false, 0);
                }
            }

            DIArchive = new Dictionary<int, ValueTuple<bool, bool>>();
            foreach (var id in DataManager.ItemKeys)
            {
                ItemEnum type = DataManager.GetEnumByIDKey<ItemEnum>(id, "Type", DataType.Item);
                DIArchive[id] = new ValueTuple<bool, bool>(false, false);
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
                if (TownManager.Market != null)
                {
                    m.RollOver();
                }
            }

            foreach (Actor npc in TownAnimals)
            {
                npc.RollOver();
            }

            Income = 0;
            if (Travelers.Count > 0)
            {
                InventoryManager.InitExtraInventory(Inventory);

                List<Food> sortedFood = Util.MultiArrayToList(Inventory).FindAll(x => x.CompareType(ItemEnum.Food)).ConvertAll(x => (Food)x);
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

        public static bool CheckKitchenSpace(Item i)
        {
            bool rv = false;
            InventoryManager.InitExtraInventory(Inventory);
            rv = InventoryManager.HasSpaceInInventory(i.ID, i.Number, false);
            InventoryManager.ClearExtraInventory();

            return rv;
        }

        #region Traveler Code
        public static void IncreaseTravelerBonus()
        {
            _iTravelerBonus += 50;
        }
        private static int BuildingTravelerChance()
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
            //No Market, no Travelers
            if (TownManager.Market == null)
            {
                return;
            }

            for (int i = 0; i < Travelers.Count; i++)
            {
                Travelers[i].CurrentMap.RemoveCharacterImmediately(Travelers[i]);
            }
            Travelers.Clear();

            //Find all Travelers
            var travelerList = new List<Traveler>();
            foreach (int value in DITravelerInfo.Keys)
            {
                Traveler npc = DataManager.CreateTraveler(value);
                if (npc.Validate() || (!npc.Rare() && RHRandom.Instance().RollPercent(10)))
                {
                    travelerList.Add(npc);
                }
            }

            int successChance = Constants.BASE_TRAVELER_RATE + BuildingTravelerChance() + _iTravelerBonus;
            do {
                //Guaranteed at least one set of Travelers/week
                if ((GameCalendar.DayOfWeek == DayEnum.Sunday && !_bTravelersCame) ||  RHRandom.Instance().RollPercent(successChance))
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

            _iTravelerBonus = 0;
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
            npc.SetPosition(map.GetRandomPosition());
            map.AddActor(npc);

            ValueTuple<bool, int> tuple = DITravelerInfo[npc.ID];
            tuple.Item2 += 1;
            DITravelerInfo[npc.ID] = new ValueTuple<bool, int>(DITravelerInfo[npc.ID].Item1, DITravelerInfo[npc.ID].Item2 + 1);
        }
        #endregion

        public static void MoveMerchants()
        {
            if (MerchantQueue.Count > 0)
            {
                Merchant chosenMerchant = MerchantQueue[0];
                chosenMerchant.MoveToSpawn();
                MerchantQueue.Remove(chosenMerchant);
            }
        }

        public static void AddAnimal(Animal npc)
        {
            TownAnimals.Add(npc);
            npc.MoveToSpawn();
        }


        #region Town Helpers
        public static void AddToCodex(int id)
        {
            if (DataManager.ItemKeys.Contains(id))
            {
                DIArchive[id] = new ValueTuple<bool, bool>(true, false);
            }
        }
        public static void AddToArchive(int id)
        {
            if (DataManager.ItemKeys.Contains(id))
            {
                DIArchive[id] = new ValueTuple<bool, bool>(true, true);
            }
        }

        public static bool AtArchive()
        {
            return GameManager.CurrentWorldObject != null && GameManager.CurrentWorldObject.GetBoolByIDKey("Archive");
        }
        public static bool CanArchiveItem(Item it)
        {
            return it != null && DIArchive.ContainsKey(it.ID) && !DIArchive[it.ID].Item2;
        }

        public static int GetPopulation()
        {
            return DIVillagers.Count(x => x.Value.LivesInTown);
        }
        public static int GetTownScore()
        {
            int rv = 0;

            return rv;
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
                TownAnimals = new List<int>(),
                Travelers = new List<int>(),
                VillagerData = new List<VillagerData>(),
                MerchantData = new List<MerchantData>(),
                TravelerData = new List<TravelerData>(),
                MerchantQueue = new List<int>(),
                CodexEntries = new List<CodexEntryData>(),
                Inventory = new List<ItemData>()
            };

            foreach (Actor npc in TownAnimals)
            {
                data.TownAnimals.Add(npc.ID);
            }

            foreach (Actor npc in Travelers)
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

            foreach (var kvp in DITravelerInfo)
            {
                TravelerData travelerData = new TravelerData()
                {
                    npcID = kvp.Key,
                    introduced = kvp.Value.Item1,
                    numVisits = kvp.Value.Item2
                };
                data.TravelerData.Add(travelerData);
            }

            foreach (Merchant npc in MerchantQueue)
            {
                data.MerchantQueue.Add(npc.ID);
            }

            foreach (Item i in Inventory)
            {
                data.Inventory.Add(Item.SaveData(i));
            }

            foreach (var kvp in DIArchive)
            {
                if (kvp.Value.Item1)
                {
                    CodexEntryData travelerData = new CodexEntryData()
                    {
                        id = kvp.Key,
                        found = kvp.Value.Item1,
                        archived = kvp.Value.Item2
                    };
                    data.CodexEntries.Add(travelerData);
                }
            }

            data.travelersCame = _bTravelersCame;

            return data;
        }
        public static void LoadData(TownData saveData)
        {
            SetTownName(saveData.townName);

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

            foreach (TravelerData data in saveData.TravelerData)
            {
                DITravelerInfo[data.npcID] = new ValueTuple<bool, int>(data.introduced, data.numVisits);
            }

            for (int i = 0; i < saveData.MerchantQueue.Count; i++)
            {
                MerchantQueue.Add(TownManager.DIMerchants[saveData.MerchantQueue[i]]);
            }

            foreach (TravelerData data in saveData.TravelerData)
            {
                DITravelerInfo[data.npcID] = new ValueTuple<bool, int>(data.introduced, data.numVisits);
            }

            foreach (CodexEntryData data in saveData.CodexEntries)
            {
                if (data.id < 8000)
                {
                    DIArchive[data.id] = new ValueTuple<bool, bool>(data.found, data.archived);
                }
                else
                {
                    int i = 0;
                }
            }

            InventoryManager.InitExtraInventory(Inventory);
            for (int i = 0; i < Constants.KITCHEN_STOCK_SIZE; i++)
            {
                for (int j = 0; j < Constants.KITCHEN_STOCK_SIZE; j++)
                {
                    int index = i * Constants.KITCHEN_STOCK_SIZE + j;
                    ItemData item = saveData.Inventory[index];

                    Item newItem = DataManager.GetItem(item.itemID, item.num);
                    newItem?.ApplyUniqueData(item.strData);
                    InventoryManager.AddItemToInventorySpot(newItem, i, j, false);
                }
            }
            InventoryManager.ClearExtraInventory();

            _bTravelersCame = saveData.travelersCame;

            MoveMerchants();
        }
    }
}
