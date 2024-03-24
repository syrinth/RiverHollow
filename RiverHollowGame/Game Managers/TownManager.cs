﻿using RiverHollow.Buildings;
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
        public static Building TownHall { get; private set; }
        public static Structure Market { get; private set; }
        public static Mailbox TownMailbox { get; private set; }
        #endregion

        private static Dictionary<int, Upgrade> _diGlobalUpgrades;
        public static Dictionary<int, Villager> DIVillagers { get; private set; }
        public static Dictionary<int, Merchant> DIMerchants { get; private set; }
        public static Dictionary<int, ValueTuple<bool, int>> DITravelerInfo { get; private set; }
        public static Dictionary<int, ValueTuple<bool, bool>> DIArchive { get; private set; }

        private static Dictionary<MailboxEnum, List<string>> _diMailbox;

        public static Merchant Merchant { get; private set; }
        public static List<Animal> TownAnimals { get; set; }
        public static List<Traveler> Travelers { get; set; }

        public static Container Pantry { get; private set; }

        #region Trackers
        public static int TotalDefeatedMobs;
        public static Dictionary<int, int> DIMobInfo { get; private set; }

        public static int PlantsGrown { get; private set; }
        public static int ValueGoodsSold { get; private set; }
        private static Dictionary<ItemGroupEnum, int> _diGoodsSold;
        #endregion

        public static void Initialize()
        {
            PlantsGrown = 0;
            ValueGoodsSold = 0;
            _diGoodsSold = new Dictionary<ItemGroupEnum, int>();
            foreach (ItemGroupEnum e in Enum.GetValues(typeof(ItemGroupEnum)))
            {
                _diGoodsSold[e] = 0;
            }

            TownAnimals = new List<Animal>();
            Travelers = new List<Traveler>();
            _diMailbox = new Dictionary<MailboxEnum, List<string>>
            {
                [MailboxEnum.Waiting] = new List<string>(),
                [MailboxEnum.Unsent] = new List<string>(),
                [MailboxEnum.Processed] = new List<string>()
            };
            foreach (var key in DataManager.GetMailboxData())
            {
                _diMailbox[MailboxEnum.Unsent].Add(key);
            }

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

            //DataManager.GetMailboxMessage();
            if (GameCalendar.DayOfWeek == 0)
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
                foreach(var traveler in Travelers)
                {
                    traveler.PurchaseItem();
                }

                InventoryManager.InitExtraInventory(Pantry.Inventory);

                List<Food> sortedFood = Util.MultiArrayToList(Pantry.Inventory).FindAll(x => x.CompareType(ItemEnum.Food)).ConvertAll(x => (Food)x);
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
                Traveler npc = DataManager.CreateTraveler(value);
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

        private static bool SkipType(int id)
        {
            var type = DataManager.GetEnumByIDKey<ItemEnum>(id, "Type", DataType.Item);

            return type == ItemEnum.Tool;
        }

        public static int GetArchiveTotal()
        {
            int rv = 0;

            foreach(var data in DIArchive)
            {

                if (!SkipType(data.Key) && data.Value.Item2)
                {
                    rv++;
                }
            }

            return rv;
        }

        public static int GetPopulation()
        {
            return DIVillagers.Count(x => x.Value.LivesInTown);
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

            totalScore += buildingScore;
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
        public static bool CheckSoldGoods(ItemGroupEnum e, int val)
        {
            return _diGoodsSold[e] >= val;
        }

        public static void TownManagerCheck(RHMap map, WorldObject obj)
        {
            if (map == MapManager.TownMap)
            {
                if (obj.GetBoolByIDKey("Inn")) { Inn = obj as Building; }
                else if (obj.GetBoolByIDKey("Home")) { Home = obj as Building; }
                else if (obj.GetBoolByIDKey("TownHall")) { TownHall = obj as Building; }
                else if (obj.GetBoolByIDKey("Market")) { Market = obj as Structure; }
                else if (obj.GetBoolByIDKey("Mailbox")) { TownMailbox = obj as Mailbox; }
            }
            else if (map == MapManager.InnMap)
            {
                if (obj.GetBoolByIDKey("Pantry"))
                {
                    Pantry = obj as Container;
                }
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
                Util.AddToListDictionary(ref _diMailbox, Enums.MailboxEnum.Processed, str);
            }

            return entry;
        }

        public static bool MailboxHasMessages()
        {
            return _diMailbox[MailboxEnum.Waiting].Count > 0;
        }

        public static bool MailboxMessageRead(int id)
        {
            return _diMailbox[MailboxEnum.Processed].Contains(id.ToString());
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
                MailboxSent = new List<string>(),
                MailboxUnsent = new List<string>(),
                MailboxWaiting = new List<string>(),
                MerchantID = Merchant != null ? Merchant.ID : -1
            };

            Travelers.ForEach(x => data.Travelers.Add(x.ID));
            TownAnimals.ForEach(x => data.TownAnimals.Add(x.ID));

            data.MailboxUnsent = _diMailbox[MailboxEnum.Unsent];
            data.MailboxSent = _diMailbox[MailboxEnum.Processed];
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

            foreach (var kvp in DIMobInfo)
            {
                data.MobInfo.Add(new ValueTuple<int, int>(kvp.Key, kvp.Value));
            }

            foreach (var kvp in _diGlobalUpgrades)
            {
                data.GlobalUpgrades.Add(new ValueTuple<int, int>(kvp.Key, (int)kvp.Value.Status));
            }

            foreach (ItemGroupEnum e in _diGoodsSold.Keys)
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
                Animal m = DataManager.CreateAnimal(id);
                AddAnimal(m);
            }

            PlantsGrown = saveData.plantsGrown;
            ValueGoodsSold = saveData.goodsSoldValue;

            foreach (var tpl in saveData.GoodsSold)
            {
                _diGoodsSold[(ItemGroupEnum)tpl.Item1] = tpl.Item2;
            }

            saveData.Travelers.ForEach(x => AddTraveler(x));
            var unsentMessages = new List<string>(_diMailbox[MailboxEnum.Unsent]);
            foreach(var messageID in unsentMessages)
            {
                if (saveData.MailboxSent.Contains(messageID))
                {
                    _diMailbox[MailboxEnum.Unsent].Remove(messageID);
                    _diMailbox[MailboxEnum.Processed].Add(messageID);
                }
                else if (saveData.MailboxWaiting.Contains(messageID))
                {
                    _diMailbox[MailboxEnum.Unsent].Remove(messageID);
                    _diMailbox[MailboxEnum.Waiting].Add(messageID);
                }
            }
            RolloverMailbox();

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
        }
    }
}
