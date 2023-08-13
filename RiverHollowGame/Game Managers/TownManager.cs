using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Items;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
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

        #region Key Buildings
        public static Building Inn { get; private set; }
        public static Building Home { get; private set; }
        public static Structure Market { get; private set; }
        public static Mailbox TownMailbox { get; private set; }
        #endregion

        public static Dictionary<int, Villager> DIVillagers { get; private set; }
        public static Dictionary<int, Merchant> DIMerchants { get; private set; }
        public static Dictionary<int, ValueTuple<bool, int>> DITravelerInfo { get; private set; }
        public static Dictionary<int, ValueTuple<bool, bool>> DIArchive { get; private set; }

        private static Dictionary<MailboxEnum, List<string>> _diMailbox;

        public static Merchant Merchant { get; private set; }
        public static List<Animal> TownAnimals { get; set; }
        public static List<Traveler> Travelers { get; set; }

        public static Item[,] Inventory { get; private set; }

        public static void Initialize()
        {
            TownAnimals = new List<Animal>();
            Travelers = new List<Traveler>();
            _diMailbox = new Dictionary<MailboxEnum, List<string>>
            {
                [MailboxEnum.Waiting] = new List<string>(),
                [MailboxEnum.Unsent] = new List<string>(),
                [MailboxEnum.Sent] = new List<string>()
            };
            foreach (var key in DataManager.GetMailboxData())
            {
                _diMailbox[MailboxEnum.Unsent].Add(key);
            }

            Inventory = new Item[Constants.KITCHEN_STOCK_ROW, Constants.KITCHEN_STOCK_COLUMN];

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
            //DataManager.GetMailboxMessage();
            if(GameCalendar.DayOfWeek == 0)
            {
                _bTravelersCame = false;
            }

            foreach (Villager v in DIVillagers.Values)
            {
                v.RollOver();
            }

            if (Market != null)
            {
                Merchant?.Cleanup();
                Merchant = null;

                foreach (Merchant m in DIMerchants.Values)
                {
                    if (Merchant == null)
                    {
                        m.RollOver();
                    }
                    else { break; }
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

                foreach (Traveler t in Travelers.FindAll(x => !x.HasEaten()))
                {
                    TryToEat(sortedFood, t, t.FavoriteFood());
                }
                foreach (Traveler t in Travelers.FindAll(x => !x.HasEaten()))
                {
                    TryToEatNeutral(sortedFood, t);
                }
                foreach (Traveler t in Travelers.FindAll(x => !x.HasEaten()))
                {
                    TryToEat(sortedFood, t, t.DislikedFood());
                }

                InventoryManager.ClearExtraInventory();

                Travelers.ForEach(npc => Income += npc.CalculateIncome());
            }

            SpawnTravelers();

            RolloverMailbox();
        }

        public static void AddToKitchen(Item i)
        {
            bool closeAfter = InventoryManager.ExtraInventory != Inventory;

            InventoryManager.InitExtraInventory(Inventory);
            InventoryManager.AddToInventory(i, false);
            AddToArchive(i.ID);
            if (closeAfter)
            {
                InventoryManager.ClearExtraInventory();
            }
        }

        public static bool CheckKitchenSpace(Item i)
        {
            bool closeAfter = InventoryManager.ExtraInventory != Inventory;

            InventoryManager.InitExtraInventory(Inventory);
            bool rv = InventoryManager.HasSpaceInInventory(i.ID, i.Number, false);
            if (closeAfter)
            {
                InventoryManager.ClearExtraInventory();
            }

            return rv;
        }
        public static void AddAnimal(Animal npc)
        {
            TownAnimals.Add(npc);
            npc.MoveToSpawn();
        }

        #region Traveler Code
        public static void IncreaseTravelerBonus()
        {
            _iTravelerBonus += Constants.BUILDING_TRAVELER_BOOST;
        }
        private static int BuildingTravelerChance()
        {
            int rv = 0;
            foreach (var kvp in MapManager.TownMap.GetObjects())
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
                if (npc.Validate()) 
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

        private static void TryToEat(List<Food> sortedFood, Traveler t, FoodTypeEnum e)
        {
            var eatMe = sortedFood.Find(f => f.FoodType == e);
            if (eatMe != null)
            {
                t.TryEat(eatMe);
            }
        }
        private static void TryToEatNeutral(List<Food> sortedFood, Traveler t)
        {
            var eatMe = sortedFood.Find(f => t.NeutralFood(f.FoodType));
            if (eatMe != null)
            {
                t.TryEat(eatMe);
            }
        }
        #endregion

        #region Merchant Code
        public static void SetMerchant(Merchant m)
        {
            Merchant = m;
        }
        #endregion

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

        public static void TownManagerCheck(RHMap map, WorldObject obj)
        {
            if (map == MapManager.TownMap)
            {
                if (obj.GetBoolByIDKey("Inn")) { Inn = (Building)obj; }
                if (obj.GetBoolByIDKey("Home")) { Home = (Building)obj; }
                if (obj.GetBoolByIDKey("Market")) { Market = (Structure)obj; }
                if (obj.GetBoolByIDKey("Mailbox")) { TownMailbox = (Mailbox)obj; }
            }
        }
        public static int GetNumberTownObjects(int objID)
        {
            return MapManager.TownMap.GetNumberObjects(objID, true);
        }
        public static bool TownObjectBuilt(int objID)
        {
            return GetNumberTownObjects(objID) > 0;
        }
        public static List<WorldObject> GetTownObjectsByID(int objID)
        {
            return MapManager.TownMap.GetObjectsByID(objID);
        }
        public static Building GetBuildingByID(int objID)
        {
            Building rv = null;
            if (TownObjectBuilt(objID) && DataManager.GetEnumByIDKey<BuildableEnum>(objID, "Subtype", DataType.WorldObject) == BuildableEnum.Building)
            {
                rv = (Building)MapManager.TownMap.GetObjectsByID(objID)[0];
            }

            return rv;
        }
        public static IReadOnlyDictionary<int, List<WorldObject>> GetTownObjects() { return MapManager.TownMap.GetObjects(); }
        #endregion

        #region Mailbox
        private static void RolloverMailbox()
        {
            var allLetters = new List<string>(_diMailbox[MailboxEnum.Unsent]);
            foreach (var letterID in allLetters)
            {
                TextEntry entry = DataManager.GetMailboxLetter(letterID);
                if (entry.Validate())
                {
                    _diMailbox[MailboxEnum.Unsent].Remove(letterID);
                    _diMailbox[MailboxEnum.Waiting].Add(letterID);
                }
            }
        }

        //Only use this for messages that can recur.
        public static void MailboxSendMessage(string letterID)
        {
            _diMailbox[MailboxEnum.Waiting].Add(letterID);
        }

        public static TextEntry MailboxTakeMessage()
        {
            TextEntry entry = null;
            if (_diMailbox[MailboxEnum.Waiting].Count > 0)
            {
                string str = _diMailbox[MailboxEnum.Waiting][0];
                entry = DataManager.GetMailboxLetter(str);

                _diMailbox[MailboxEnum.Waiting].Remove(str);
                Util.AddToListDictionary(ref _diMailbox, Enums.MailboxEnum.Sent, str);
            }

            return entry;
        }

        public static bool MailboxHasMessages()
        {
            return _diMailbox[MailboxEnum.Waiting].Count > 0;
        }
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
                CodexEntries = new List<CodexEntryData>(),
                Inventory = new List<ItemData>(),
                MailboxSent = new List<string>(),
                MailboxUnsent = new List<string>(),
                MailboxWaiting = new List<string>(),
                MerchantID = Merchant != null ? Merchant.ID : -1
            };

            Travelers.ForEach(x => data.Travelers.Add(x.ID));
            TownAnimals.ForEach(x => data.TownAnimals.Add(x.ID));

            data.MailboxUnsent = _diMailbox[MailboxEnum.Unsent];
            data.MailboxSent = _diMailbox[MailboxEnum.Sent];
            data.MailboxWaiting = _diMailbox[MailboxEnum.Waiting];

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

            saveData.Travelers.ForEach(x => AddTraveler(x));
            _diMailbox[MailboxEnum.Unsent] = saveData.MailboxUnsent;
            _diMailbox[MailboxEnum.Sent] = saveData.MailboxSent;
            _diMailbox[MailboxEnum.Waiting] = saveData.MailboxWaiting;

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
            }

            InventoryManager.InitExtraInventory(Inventory);
            for (int i = 0; i < Constants.KITCHEN_STOCK_ROW; i++)
            {
                for (int j = 0; j < Constants.KITCHEN_STOCK_COLUMN; j++)
                {
                    int index = i * Constants.KITCHEN_STOCK_COLUMN + j;
                    ItemData item = saveData.Inventory[index];

                    Item newItem = DataManager.GetItem(item.itemID, item.num);
                    newItem?.ApplyUniqueData(item.strData);
                    InventoryManager.AddItemToInventorySpot(newItem, i, j, false);
                }
            }
            InventoryManager.ClearExtraInventory();

            _bTravelersCame = saveData.travelersCame;

            if (saveData.MerchantID != -1)
            {
                Merchant = DIMerchants[saveData.MerchantID];
                Merchant.MoveToSpawn();
            }
        }
    }
}
