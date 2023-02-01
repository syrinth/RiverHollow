using Microsoft.Xna.Framework;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using System.Linq;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Game_Managers
{
    internal static class TownManager
    {
        public static string TownName { get; private set; }

        private static Dictionary<int, List<WorldObject>> _diTownObjects;
        private static Dictionary<int, int> _diStorage;
        public static Dictionary<int, Villager> DIVillagers { get; private set; }
        public static Dictionary<int, Merchant> DIMerchants { get; private set; }
        public static List<Merchant> MerchantQueue;
        public static List<Animal> TownAnimals { get; set; }
        public static List<TalkingActor> Visitors { get; set; }

        public static void Initialize()
        {
            _diTownObjects = new Dictionary<int, List<WorldObject>>();
            _diStorage = new Dictionary<int, int>();
            TownAnimals = new List<Animal>();
            Visitors = new List<TalkingActor>();
            MerchantQueue = new List<Merchant>();

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

            HandleTravelers();
        }

        private static void HandleTravelers()
        {
            for (int i = 0; i < Visitors.Count; i++)
            {
                MapManager.TownMap.RemoveCharacterImmediately(Visitors[i]);
            }
            Visitors.Clear();

            if (RHRandom.Instance().RollPercent(Constants.BASE_TRAVELER_RATE))
            {
                var newTraveler = new List<int>();
                foreach (KeyValuePair<int, Dictionary<string, string>> kvp in DataManager.NPCData)
                {
                    if (kvp.Value["Type"] == Util.GetEnumString(WorldActorTypeEnum.Traveler))
                    {
                        newTraveler.Add(kvp.Key);
                    }
                }
                int chosenID = RHRandom.Instance().Next(0, newTraveler.Count - 1);
                AddTraveler(newTraveler[chosenID]);
            }
        }

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

        public static void AddTraveler(int id)
        {
            TalkingActor npc = (TalkingActor)DataManager.CreateNPCByIndex(id);
            Visitors.Add(npc);
            npc.Position = MapManager.TownMap.GetRandomPosition();
            MapManager.TownMap.AddActor(npc);
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

        public static int CalculateIncome()
        {
            int rv = 0;

            foreach (Villager n in DIVillagers.Values)
            {
                if (n.LivesInTown)
                {
                    rv += n.Income;
                }
            }

            return rv;
        }
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

            foreach (WorldActor npc in Visitors)
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

            MoveMerchants();
        }
    }
}
