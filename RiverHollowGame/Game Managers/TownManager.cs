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
        public static Building PetCafe { get; private set; }
        public static Building TownHall { get; private set; }
        public static Structure Market { get; private set; }
        public static Mailbox TownMailbox { get; private set; }
        #endregion

        private static Dictionary<int, Upgrade> _diGlobalUpgrades;
        public static Dictionary<int, Villager> Villagers { get; private set; }
        public static Dictionary<int, Merchant> DIMerchants { get; private set; }
        public static Dictionary<int, ValueTuple<bool, int>> DITravelerInfo { get; private set; }
        public static Dictionary<int, ItemDataState> DIArchive { get; private set; }

        private static Dictionary<LetterTemplateEnum, List<int>> _diAllLetters;
        private static List<Letter> _liMailbox;

        public static Merchant Merchant { get; private set; }
        public static List<Animal> TownAnimals { get; set; }
        public static List<Traveler> Travelers { get; set; }

        public static Item[,] Pantry { get; private set; }

        #region Trackers
        public static int TotalDefeatedMobs;
        public static Dictionary<int, int> DIMobInfo { get; private set; }

        public static int PlantsGrown { get; private set; }
        public static int ValueGoodsSold { get; private set; }
        private static Dictionary<ResourceTypeEnum, int> _diGoodsSold;
        #endregion

        public static void Initialize()
        {
            PlantsGrown = 0;
            ValueGoodsSold = 0;
            _diGoodsSold = new Dictionary<ResourceTypeEnum, int>();
            foreach (ResourceTypeEnum e in Enum.GetValues(typeof(ResourceTypeEnum)))
            {
                _diGoodsSold[e] = 0;
            }

            TownAnimals = new List<Animal>();
            Travelers = new List<Traveler>();

            _liMailbox = new List<Letter>();
            _diAllLetters = new Dictionary<LetterTemplateEnum, List<int>>
            {
                [LetterTemplateEnum.Sent] = new List<int>(),
                [LetterTemplateEnum.Unsent] = new List<int>(),
                [LetterTemplateEnum.Repeatable] = new List<int>()
            };

            foreach (var kvp in DataManager.LetterData)
            {
                var letter = new Letter(kvp.Key);
                var target = letter.Repeatable ? LetterTemplateEnum.Repeatable : LetterTemplateEnum.Unsent;
                _diAllLetters[target].Add(kvp.Key);
            }

            DIMerchants = new Dictionary<int, Merchant>();
            Villagers = new Dictionary<int, Villager>();
            foreach (KeyValuePair<int, Dictionary<string, string>> npcData in DataManager.ActorData)
            {
                Dictionary<string, string> diData = DataManager.ActorData[npcData.Key];
                switch (diData["Type"])
                {
                    case "Merchant":
                        DIMerchants.Add(npcData.Key, new Merchant(npcData.Key, diData));
                        break;

                    case "Villager":
                        Villagers.Add(npcData.Key, new Villager(npcData.Key, diData));
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

            DIArchive = new Dictionary<int, ItemDataState>();
            foreach (var id in DataManager.ItemKeys)
            {
                ItemTypeEnum type = DataManager.GetEnumByIDKey<ItemTypeEnum>(id, "Type", DataType.Item);
                DIArchive[id] = new ItemDataState();
            }

            TotalDefeatedMobs = 0;
            DIMobInfo = new Dictionary<int, int>();
            foreach (KeyValuePair<int, Dictionary<string, string>> kvp in DataManager.ActorData)
            {
                if (kvp.Value["Type"] == Util.GetEnumString(ActorTypeEnum.Mob))
                {
                    DIMobInfo[kvp.Key] = 0;
                }
            }

            _diGlobalUpgrades = DataManager.GetGlobalUpgrades();
        }

        public static void NewGame()
        {

        }

        public static void SetTownName(string x)
        {
            TownName = x;
        }

        public static void Rollover()
        {
            TaskManager.TaskLog.ForEach(x => x.AttemptProgress());

            if (GameCalendar.DayOfWeek == 0)
            {
                _bTravelersCame = false;
            }

            foreach (Villager v in Villagers.Values)
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

                var merchantTables = new Dictionary<ClassTypeEnum, List<KeyValuePair<Item, Building>>>();
                GatherMerch(ref merchantTables);

                foreach (var traveler in Travelers)
                {
                    traveler.PurchaseItem(merchantTables);
                }

                InventoryManager.InitExtraInventory(Pantry);

                List<Food> sortedFood = Util.MultiArrayToList(Pantry).FindAll(x => x.CompareType(ItemTypeEnum.Food)).ConvertAll(x => (Food)x);
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

            foreach(var upgrade in _diGlobalUpgrades.Values)
            {
                if(upgrade.Status == UpgradeStatusEnum.InProgress)
                {
                    upgrade.TriggerUpgrade();
                }
            }
        }
        private static void GatherMerch(ref Dictionary<ClassTypeEnum, List<KeyValuePair<Item, Building>>> townMerchData)
        {
            foreach (ClassTypeEnum e in Enum.GetValues(typeof(ClassTypeEnum)))
            {
                townMerchData[e] = new List<KeyValuePair<Item, Building>>();
            }

            var allBuildings = MapManager.TownMap.GetObjectsByType<Building>();
            foreach (var building in allBuildings)
            {
                foreach (var i in building.Merchandise)
                {
                    if (i is Merchandise merchItem)
                    {
                        var itemData = new KeyValuePair<Item, Building>(i, building);
                        townMerchData[merchItem.ClassType].Add(itemData);
                    }
                }
            }
        }

        public static void AddAnimal(Animal npc)
        {
            TownAnimals.Add(npc);
            npc.MoveToSpawn();
        }

        public static IReadOnlyDictionary<int, Upgrade> GetAllUpgrades()
        {
            return _diGlobalUpgrades;
        }
        public static Upgrade GetGlobalUpgrade(int upgradeID)
        {
            return _diGlobalUpgrades[upgradeID];
        }
        public static void UnlockUpgrade(int upgradeID)
        {
            if (_diGlobalUpgrades[upgradeID].Status == UpgradeStatusEnum.Locked)
            {
                _diGlobalUpgrades[upgradeID].ChangeStatus(UpgradeStatusEnum.Unlocked);
                GUIManager.OpenTextWindow(string.Format("Upgrade_{0}", upgradeID));

                GUIManager.NewInfoAlertIcon(Constants.STR_ALERT_UPGRADE);
            }
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
            for (int i = 0; i < Travelers.Count; i++)
            {
                Travelers[i].CurrentMap.RemoveCharacterImmediately(Travelers[i]);
            }
            Travelers.Clear();

            //Find all Travelers
            var travelerList = new List<Traveler>();
            foreach (int value in DITravelerInfo.Keys)
            {
                Traveler npc = DataManager.CreateActor<Traveler>(value);
                if (npc.Validate()) 
                {
                    travelerList.Add(npc);
                }
            }

            int successChance = Constants.BASE_TRAVELER_RATE + BuildingTravelerChance() + _iTravelerBonus;
            do {
                //Guaranteed at least one set of Travelers/week
                if ((GameCalendar.DayOfWeek == DayEnum.Sunday && !_bTravelersCame) ||  RHRandom.RollPercent(successChance))
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
            int chainSuccess = successChance;
            do
            {
                if (RHRandom.RollPercent(chainSuccess))
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
                        if (RHRandom.RollPercent(80)) { options = groupList; }
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
            AddTraveler(DataManager.CreateActor<Traveler>(id));
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
                        map = MapManager.Maps[Inn.InnerMapName];
                    }
                    break;
                case 1:
                    map = MapManager.Maps[Inn.InnerMapName];
                    break;
                case 2:
                    if (npc.BuildingID() != -1 && TownObjectBuilt(npc.BuildingID()))
                    {
                        map = MapManager.Maps[GetBuildingByID(npc.BuildingID()).InnerMapName];
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

        public static void SetPantry(Item[,] arr)
        {
            Pantry = arr;
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
                DIArchive[id].SetCodex();
            }
        }
        public static void AddToArchive(int id)
        {
            if (DataManager.ItemKeys.Contains(id))
            {
                DIArchive[id].SetArchive();
            }
        }

        public static bool AtArchive()
        {
            return GameManager.CurrentWorldObject != null && GameManager.CurrentWorldObject.GetBoolByIDKey("Archive");
        }
        public static bool CanArchiveItem(Item it)
        {
            return it != null && DIArchive.ContainsKey(it.ID) && !DIArchive[it.ID].Archived;
        }

        private static bool SkipType(int id)
        {
            var type = DataManager.GetEnumByIDKey<ItemTypeEnum>(id, "Type", DataType.Item);

            return type == ItemTypeEnum.Tool;
        }

        public static int GetArchiveTotal()
        {
            int rv = 0;

            foreach(var data in DIArchive)
            {

                if (!SkipType(data.Key) && data.Value.Archived)
                {
                    rv++;
                }
            }

            return rv;
        }

        public static int GetPopulation()
        {
            return Villagers.Count(x => x.Value.LivesInTown);
        }
        public static int GetTownScore()
        {
            float totalScore = 0;
            float buildingScore = 0;
            float infrastuctureScore = 0;
            float decorationScore = 0;
            float miscScore = 0;

            var TownObjects = MapManager.TownMap.GetObjects();
            foreach (var key in TownObjects.Keys)
            {
                var list = TownObjects[key];
                foreach(var obj in list)
                {
                    var objScore = obj.GetTownScore();
                    if (obj.BuildableType(BuildableEnum.Building))
                    {
                        buildingScore += objScore;
                    }
                    else if (obj.BuildableType(BuildableEnum.Wall) || obj.BuildableType(BuildableEnum.Floor))
                    {
                        infrastuctureScore += objScore;
                    }
                    else if (obj.GetBoolByIDKey("Decoration"))
                    {
                        decorationScore += objScore;
                    }
                    else
                    {
                        miscScore += objScore;
                    }
                }
            }

            //totalScore += buildingScore;
            totalScore += Math.Min(infrastuctureScore, buildingScore * 0.4f);
            totalScore += Math.Min(decorationScore, buildingScore * 0.2f);
            totalScore += miscScore;

            return (int)totalScore;
        }
        public static void TrackDefeatedMob(Mob m)
        {
            DIMobInfo[m.ID] += 1;
            TotalDefeatedMobs++;
        }
        public static void IncrementPlantsGrown()
        {
            PlantsGrown++;
        }
        public static void AddToSoldGoods(int itemID, int profit, int num = 1)
        {
            ValueGoodsSold += profit;

            var item = DataManager.GetItem(itemID);
            var itemEnum = item.GetItemGroup();
            _diGoodsSold[itemEnum] = _diGoodsSold[itemEnum] + num;
        }
        public static bool CheckSoldGoods(ResourceTypeEnum e, int val)
        {
            return _diGoodsSold[e] >= val;
        }

        public static void TownManagerCheck(RHMap map, WorldObject obj)
        {
            if (map == MapManager.TownMap)
            {
                if (obj.GetBoolByIDKey("Inn")) { Inn = obj as Building; }
                else if (obj.GetBoolByIDKey("Home")) { Home = obj as Building; }
                else if (obj.GetBoolByIDKey("Pet_Cafe")) { PetCafe = obj as Building; }
                else if (obj.GetBoolByIDKey("TownHall")) { TownHall = obj as Building; }
                else if (obj.GetBoolByIDKey("Market")) { Market = obj as Structure; }
                else if (obj.GetBoolByIDKey("Mailbox")) { TownMailbox = obj as Mailbox; }
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
        public static Building GetCurrentBuilding()
        {
            Building rv = null;
            if (MapManager.CurrentMap.BuildingID != -1)
            {
                int objID = MapManager.CurrentMap.BuildingID;
                if (TownObjectBuilt(objID) && DataManager.GetEnumByIDKey<BuildableEnum>(objID, "Subtype", DataType.WorldObject) == BuildableEnum.Building)
                {
                    rv = (Building)MapManager.TownMap.GetObjectsByID(objID)[0];
                }
            }

            return rv;
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
        public static List<Letter> GetAllLetters()
        {
            return _liMailbox;
        }

        private static void RolloverMailbox()
        {
            var allLetters = new List<int>(_diAllLetters[LetterTemplateEnum.Unsent]);

            foreach (var letterID in allLetters)
            {
                var letter = new Letter(letterID);
                if (letter.Text.Validate())
                {
                    _diAllLetters[LetterTemplateEnum.Unsent].Remove(letterID);
                    _diAllLetters[LetterTemplateEnum.Sent].Add(letterID);

                    _liMailbox.Insert(0, letter);
                }
            }

            if (RHRandom.RollPercent(Constants.MAIL_PERCENT))
            {
                var inTown = new List<Villager>();
                foreach (var item in Villagers.Values)
                {
                    if (item.LivesInTown)
                    {
                        inTown.Add(item);
                    }
                }

                var chosenVillager = Util.GetRandomItem(inTown);

                var letters = new List<Letter>();
                foreach (var letterID in _diAllLetters[LetterTemplateEnum.Repeatable])
                {
                    var letter = new Letter(letterID);
                    if(letter.NPCID == chosenVillager.ID && letter.Text.Validate())
                    {
                        letters.Add(letter);
                    }
                }

                if (letters.Count > 0)
                {
                    var letter = new Letter(Util.GetRandomItem(letters));
                    _liMailbox.Insert(0, letter);
                }
            }
        }
        public static bool MailboxHasUnreadLetters()
        {
            bool rv = false;

            foreach (var letter in _liMailbox)
            {
                if (!letter.LetterRead)
                {
                    rv = true;
                    break;
                }
            }

            return rv;
        }
        public static void ReadLetter(Letter l)
        {
            foreach(var letter in _liMailbox)
            {
                if(letter.Equals(l))
                {
                    letter.ReadLetter();
                    break;
                }
            }
        }

        public static void DeleteLetter(Letter l)
        {
            _liMailbox.Remove(l);
        }
        #endregion

        public static TownData SaveData()
        {
            TownData data = new TownData
            {
                townName = TownName,
                plantsGrown = PlantsGrown,
                goodsSoldValue = ValueGoodsSold,
                TownAnimals = new List<int>(),
                Travelers = new List<int>(),
                VillagerData = new List<VillagerData>(),
                MerchantData = new List<MerchantData>(),
                TravelerData = new List<TravelerData>(),
                CodexEntries = new List<CodexEntryData>(),
                MobInfo = new List<ValueTuple<int, int>>(),
                GlobalUpgrades = new List<ValueTuple<int, int>>(),
                GoodsSold = new List<ValueTuple<int, int>>(),
                Mailbox = new List<LetterData>(),
                MailboxSent = new List<int>(),
                MerchantID = Merchant != null ? Merchant.ID : -1
            };

            Travelers.ForEach(x => data.Travelers.Add(x.ID));
            TownAnimals.ForEach(x => data.TownAnimals.Add(x.ID));

            //Mailbox Data
            foreach(var letter in _liMailbox)
            {
                data.Mailbox.Add(letter.Save());
            }

            foreach (var letterID in _diAllLetters[LetterTemplateEnum.Sent])
            {
                data.MailboxSent.Add(letterID);
            }

            //Actors Data
            foreach (Villager npc in Villagers.Values)
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

            foreach (var kvp in DIArchive)
            {
                if (kvp.Value.Codexed)
                {
                    CodexEntryData travelerData = new CodexEntryData()
                    {
                        id = kvp.Key,
                        found = kvp.Value.Codexed,
                        archived = kvp.Value.Archived
                    };
                    data.CodexEntries.Add(travelerData);
                }
            }

            foreach (var kvp in DIMobInfo)
            {
                data.MobInfo.Add(new ValueTuple<int, int>(kvp.Key, kvp.Value));
            }

            foreach (var kvp in _diGlobalUpgrades)
            {
                data.GlobalUpgrades.Add(new ValueTuple<int, int>(kvp.Key, (int)kvp.Value.Status));
            }

            foreach (ResourceTypeEnum e in _diGoodsSold.Keys)
            {
                data.GoodsSold.Add(new ValueTuple<int, int>((int)e, _diGoodsSold[e]));
            }

            data.travelersCame = _bTravelersCame;

            return data;
        }
        public static void LoadData(TownData saveData)
        {
            SetTownName(saveData.townName);

            foreach (int id in saveData.TownAnimals)
            {
                Animal m = DataManager.CreateActor<Animal>(id);
                AddAnimal(m);
            }

            PlantsGrown = saveData.plantsGrown;
            ValueGoodsSold = saveData.goodsSoldValue;

            foreach (var tpl in saveData.GoodsSold)
            {
                _diGoodsSold[(ResourceTypeEnum)tpl.Item1] = tpl.Item2;
            }

            saveData.Travelers.ForEach(x => AddTraveler(x));
            var unsentMessages = new List<int>(_diAllLetters[LetterTemplateEnum.Unsent]);
            foreach (var letterID in unsentMessages)
            {
                if (saveData.MailboxSent.Contains(letterID))
                {
                    _diAllLetters[LetterTemplateEnum.Unsent].Remove(letterID);
                    _diAllLetters[LetterTemplateEnum.Sent].Add(letterID);
                }
            }

            foreach(var letterData in saveData.Mailbox)
            {
                var letter = new Letter(letterData.LetterID);
                letter.LoadData(letterData);
                _liMailbox.Add(letter);
            }

            foreach (VillagerData data in saveData.VillagerData)
            {
                Villager npc = Villagers[data.npcID];
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
                    if(data.found)
                    {
                        DIArchive[data.id].SetCodex();
                    }

                    if (data.archived)
                    {
                        DIArchive[data.id].SetArchive();
                    }
                }
            }

            foreach (var tpl in saveData.MobInfo)
            {
                DIMobInfo[tpl.Item1] = tpl.Item2;
                TotalDefeatedMobs += tpl.Item2;
            }

            foreach (var tpl in saveData.GlobalUpgrades)
            {
                GetGlobalUpgrade(tpl.Item1).ChangeStatus((UpgradeStatusEnum)tpl.Item2);
            }

            _bTravelersCame = saveData.travelersCame;

            if (saveData.MerchantID != -1)
            {
                Merchant = DIMerchants[saveData.MerchantID];
                Merchant.MoveToSpawn();
            }

            var allBuildings = MapManager.TownMap.GetObjectsByType<Building>();
            foreach(var b in allBuildings)
            {
                b.InnerMap.AssignMerchandise();
            }
        }
    }

    public class ItemDataState
    {
        public bool Codexed { get; private set; }
        public bool Archived { get; private set; }

        public ItemDataState()
        {
            Codexed = false;
            Archived = false;
        }

        public void SetCodex()
        {
            Codexed = true;
        }

        public void SetArchive()
        {
            Archived = true;
        }
    }
}
