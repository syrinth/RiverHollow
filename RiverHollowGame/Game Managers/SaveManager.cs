using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Map_Handling;
using RiverHollow.Misc;
using RiverHollow.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using static RiverHollow.Misc.RHTask;

namespace RiverHollow.Game_Managers
{
    public class SaveManager
    {
        static string INFO_FILE_NAME = "SaveInfo";
        public static string RIVER_HOLLOW_SAVES = "Save Games";
        static long _iSaveID = -1;

        private static bool _bSaving = false;
        static Thread _thrSave;

        #region Save/Load
        #region structs
        public struct SaveInfoData
        {
            [XmlElement(ElementName = "SaveFile")]
            public string saveFile;

            [XmlElement(ElementName = "TimeStamp")]
            public DateTime timeStamp;

            [XmlElement(ElementName = "SaveID")]
            public long saveID;

            [XmlElement(ElementName = "Player")]
            public PlayerData playerData;

            [XmlElement(ElementName = "Calendar")]
            public CalendarData Calendar;
        }
        public struct SaveData
        {
            /// <summary>
            /// The Level data object.
            /// </summary>
            /// 
            [XmlElement(ElementName = "SaveID")]
            public long saveID;

            [XmlElement(ElementName = "Player")]
            public PlayerData playerData;

            [XmlElement(ElementName = "Options")]
            public OptionsData optionData;

            [XmlElement(ElementName = "Calendar")]
            public CalendarData Calendar;

            [XmlElement(ElementName = "Environment")]
            public EnvironmentData Environment;

            [XmlElement(ElementName = "Tools")]
            public ToolData Tools;

            [XmlArray(ElementName = "Maps")]
            public List<MapData> MapData;

            [XmlArray(ElementName = "TaskInfo")]
            public List<TaskData> TaskInfo;

            [XmlArray(ElementName = "CurrentMissions")]
            public List<MissionData> CurrentMissions;

            [XmlArray(ElementName = "AvailableMissions")]
            public List<MissionData> AvailableMissions;

            [XmlArray(ElementName = "VillagerData")]
            public List<VillagerData> VillagerData;

            [XmlArray(ElementName = "MerchantData")]
            public List<MerchantData> MerchantData;

            [XmlArray(ElementName = "MerchantQueue")]
            public List<int> MerchantQueue;

            [XmlArray(ElementName = "ShopIData")]
            public List<ShopData> ShopData;

            [XmlArray(ElementName = "CutsceneData")]
            public List<CutsceneData> CSData;

            [XmlElement(ElementName = "Mailbox")]
            public MailboxData TheMailbox;

        }
        public struct OptionsData
        {
            [XmlElement(ElementName = "HideMiniInventory")]
            public bool hideMiniInventory;

            [XmlElement(ElementName = "MusicVolume")]
            public float musicVolume;

            [XmlElement(ElementName = "EffectVolume")]
            public float effectVolume;
        }
        public struct PlayerData
        {
            [XmlElement(ElementName = "Name")]
            public string name;

            [XmlElement(ElementName = "Money")]
            public int money;

            [XmlElement(ElementName = "TotalMoneyEarned")]
            public int totalMoneyEarned;

            [XmlElement(ElementName = "CurrentClass")]
            public int currentClass;

            [XmlElement(ElementName = "BodyType")]
            public int bodyTypeIndex;

            [XmlElement(ElementName = "HairColor")]
            public Color hairColor;

            [XmlElement(ElementName = "HairIndex")]
            public int hairIndex;

            [XmlElement(ElementName = "Hat")]
            public ItemData hat;

            [XmlElement(ElementName = "Chest")]
            public ItemData chest;

            [XmlArray(ElementName = "Items")]
            public List<ItemData> Items;

            [XmlArray(ElementName = "Storage")]
            public List<StorageData> Storage;

            [XmlElement(ElementName = "ActivePet")]
            public int activePet;

            [XmlElement(ElementName = "WeddingCountdown")]
            public int weddingCountdown;

            [XmlElement(ElementName = "BabyCountdown")]
            public int babyCountdown;

            [XmlElement(ElementName = "UniqueItemsBought")]
            public string UniqueItemsBought;

            [XmlArray(ElementName = "Pets")]
            public List<int> liPets;

            [XmlArray(ElementName = "Mounts")]
            public List<int> MountList;

            [XmlArray(ElementName = "Children")]
            public List<ChildData> ChildList;

            [XmlArray(ElementName = "TownAnimals")]
            public List<int> TownAnimals;

            [XmlElement(ElementName = "AdventurerData")]
            public ClassedCharData adventurerData;

            [XmlArray(ElementName = "CraftingDictionary")]
            public List<int> CraftingList;
        }
        public struct StorageData
        {
            [XmlElement(ElementName = "ObjectID")]
            public int objID;

            [XmlElement(ElementName = "Number")]
            public int number;
        }
        public struct CalendarData
        {
            [XmlElement(ElementName = "dayOfMonth")]
            public int dayOfMonth;

            [XmlElement(ElementName = "dayOfWeek")]
            public int dayOfWeek;

            [XmlElement(ElementName = "currSeason")]
            public int currSeason;

        }
        public struct EnvironmentData
        {
            [XmlElement(ElementName = "currWeather")]
            public int currWeather;

            [XmlElement(ElementName = "currSeasonPrecipDays")]
            public int currSeasonPrecipDays;
        }
        public struct BuildInfoData
        {
            [XmlElement(ElementName = "InfoID")]
            public int id;

            [XmlElement(ElementName = "Built")]
            public bool built;

            [XmlElement(ElementName = "Unlocked")]
            public bool unlocked;
        }
        public struct MailboxData
        {
            [XmlArray(ElementName = "MailboxMessages")]
            public List<string> MailboxMessages;
        }
        public struct WorkerData
        {
            [XmlElement(ElementName = "WorkerID")]
            public int workerID;

            [XmlElement(ElementName = "PersonalID")]
            public int PersonalID;

            [XmlElement(ElementName = "Name")]
            public string name;

            [XmlElement(ElementName = "AdventurerData")]
            public ClassedCharData advData;

            [XmlElement(ElementName = "Mood")]
            public int mood;

            [XmlElement(ElementName = "ProcessedTime")]
            public double processedTime;

            [XmlElement(ElementName = "ProcessingItemID")]
            public int currentItemID;

            [XmlElement(ElementName = "HeldItemID")]
            public int heldItemID;

            [XmlElement(ElementName = "State")]
            public int state;
        }
        public struct ClassedCharData
        {
            [XmlElement(ElementName = "XP")]
            public int xp;

            [XmlElement(ElementName = "Level")]
            public int level;

            [XmlElement(ElementName = "Weapon")]
            public ItemData weapon;

            [XmlElement(ElementName = "Armor")]
            public ItemData armor;

            [XmlElement(ElementName = "Head")]
            public ItemData head;

            [XmlElement(ElementName = "Accessory")]
            public ItemData accessory;
        }
        public struct ItemData
        {
            [XmlElement(ElementName = "ItemID")]
            public int itemID;

            [XmlElement(ElementName = "Numbers")]
            public int num;

            [XmlElement(ElementName = "Data")]
            public string strData;
        }
        public struct MapData
        {
            [XmlElement(ElementName = "MapName")]
            public string mapName;

            [XmlArray(ElementName = "WorldObjects")]
            public List<WorldObjectData> worldObjects;
        }
        public struct VillagerData
        {
            [XmlElement(ElementName = "NPCID")]
            public int npcID;

            [XmlElement(ElementName = "SpawnStatus")]
            public int spawnStatus;

            [XmlElement(ElementName = "NextArrival")]
            public int nextArrival;

            [XmlElement(ElementName = "Friendship")]
            public int friendshipPoints;

            [XmlArray(ElementName = "Collection")]
            public List<bool> collection;

            [XmlElement(ElementName = "Relationship")]
            public int relationShipStatus;

            [XmlElement(ElementName = "CanJoin")]
            public bool canJoinParty;

            [XmlElement(ElementName = "CanGiveGift")]
            public bool canGiveGift;

            [XmlElement(ElementName = "AdventurerData")]
            public ClassedCharData classedData;

            [XmlArray(ElementName = "SpokenKeys")]
            public List<string> spokenKeys;
        }
        public struct ChildData
        {
            [XmlElement(ElementName = "childID")]
            public int childID;

            [XmlElement(ElementName = "stageEnum")]
            public int stageEnum;

            [XmlElement(ElementName = "growthTime")]
            public int growthTime;
        }
        public struct MerchantData
        {
            [XmlElement(ElementName = "NPCID")]
            public int npcID;

            [XmlElement(ElementName = "Introduced")]
            public bool introduced;

            [XmlElement(ElementName = "NextArrival")]
            public int timeToNextArrival;

            [XmlElement(ElementName = "ArrivedOnce")]
            public bool arrivedOnce;

            [XmlArray(ElementName = "SpokenKeys")]
            public List<string> spokenKeys;

            [XmlElement(ElementName = "Relationship")]
            public int relationShipStatus;

            [XmlElement(ElementName = "Requests")]
            public string requestString;
        }
        public struct CollectionData
        {
            [XmlElement(ElementName = "ItemID")]
            public int itemID;

            [XmlElement(ElementName = "Given")]
            public bool given;
        }
        public struct RHTileData
        {
            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;

            [XmlElement(ElementName = "WorldObject")]
            public WorldObjectData worldObject;
        }
        public struct WorldObjectData
        {
            [XmlElement(ElementName = "WorldObjectID")]
            public int ID;

            [XmlElement(ElementName = "X")]
            public int X;

            [XmlElement(ElementName = "Y")]
            public int Y;

            [XmlElement(ElementName = "TypeData")]
            public string stringData;
        }
        public struct ShopData
        {
            [XmlElement(ElementName = "ShopID")]
            public int shopID;

            [XmlElement(ElementName = "LockedItems")]
            public string merchUnlockedString;

            [XmlElement(ElementName = "Randomized")]
            public string randomized;
        }
        public struct ToolData
        {
            [XmlElement(ElementName = "PickID")]
            public int pickID;

            [XmlElement(ElementName = "AxeID")]
            public int axeID;

            [XmlElement(ElementName = "ScytheID")]
            public int scytheID;

            [XmlElement(ElementName = "WateringCanID")]
            public int wateringCanID; 

            [XmlElement(ElementName = "BackpackID")]
            public int backpackID;

            [XmlElement(ElementName = "LanternID")]
            public int lanternID;
        }
        public struct MissionData
        {
            [XmlElement(ElementName = "Name")]
            public string Name;

            [XmlElement(ElementName = "DaysToComplete")]
            public int DaysToComplete;

            [XmlElement(ElementName = "DaysFinished")]
            public int DaysFinished;

            [XmlElement(ElementName = "TotalDaysToExpire")]
            public int TotalDaysToExpire;

            [XmlElement(ElementName = "DaysExpired")]
            public int DaysExpired;

            [XmlElement(ElementName = "Money")]
            public int Money;

            [XmlElement(ElementName = "ReqLevel")]
            public int ReqLevel;

            [XmlElement(ElementName = "PartySize")]
            public int PartySize;

            [XmlElement(ElementName = "ReqClassID")]
            public int ReqClassID;

            [XmlArray(ElementName = "Items")]
            public List<ItemData> Items;

            [XmlArray(ElementName = "Adventurers")]
            public List<int> ListAdventurerIDs;
        }
        public struct CutsceneData
        {
            [XmlElement(ElementName = "Played")]
            public bool Played;
        }
        #endregion

        public static void StartSaveThread()
        {
            _thrSave = new Thread(SaveAtEndOfDay);
            _thrSave.Start();
        }

        private static void SaveAtEndOfDay()
        {
            _bSaving = true;
            GameCalendar.NextDay();
            RiverHollow.Rollover();
            PlayerManager.AddMoney(PlayerManager.CalculateIncome());
            SaveManager.Save();
            PlayerManager.Stamina = PlayerManager.MaxStamina;
            foreach (CombatActor actor in PlayerManager.GetParty())
            {
                actor?.IncreaseHealth(actor.MaxHP);
            }

            _bSaving = false;
        }

        public static bool Saving()
        {
            return _bSaving;
        }

        public static long GetSaveID()
        {
            if (_iSaveID == -1)
            {
                _iSaveID = long.Parse(string.Format("{0}{1}{2}{3}", DateTime.Now.Year, DateTime.Now.Day, DateTime.Now.Second, DateTime.Now.Millisecond));
            }

            return _iSaveID;
        }

        public static void Save()
        {
            SaveData data = new SaveData
            {
                saveID = GetSaveID(),
                Calendar = GameCalendar.SaveCalendar(),
                Environment = EnvironmentManager.SaveEnvironment(),
                Tools = PlayerManager.SaveToolData(),
                MapData = new List<MapData>(),
                TaskInfo = new List<TaskData>(),
                CurrentMissions = new List<MissionData>(),
                AvailableMissions = new List<MissionData>(),
                VillagerData = new List<VillagerData>(),
                MerchantData = new List<MerchantData>(),
                MerchantQueue = new List<int>(),
                ShopData = new List<ShopData>(),
                CSData = new List<CutsceneData>(),
                TheMailbox = PlayerManager.PlayerMailbox.SaveData(),
                optionData = SaveOptions(),
                playerData = PlayerManager.SaveData()
            };

            foreach (RHMap tileMap in MapManager.Maps.Values)
            {
                data.MapData.Add(tileMap.SaveData());
            }

            data.TaskInfo = TaskManager.SaveTaskData();

            //foreach (Mission m in MissionManager.AvailableMissions)
            //{
            //    data.AvailableMissions.Add(m.SaveData());
            //}

            //foreach (Mission m in MissionManager.CurrentMissions)
            //{
            //    data.CurrentMissions.Add(m.SaveData());
            //}

            foreach (Villager n in DataManager.DIVillagers.Values)
            {
                data.VillagerData.Add(n.SaveData());
            }

            foreach (Merchant m in DataManager.DIMerchants.Values)
            {
                data.MerchantData.Add(m.SaveData());
            }

            foreach(Merchant m in GameManager.MerchantQueue)
            {
                data.MerchantQueue.Add(m.ID);
            }

            foreach (Shop s in GameManager.DIShops.Values)
            {
                data.ShopData.Add(s.SaveData());
            }

            data.CSData = CutsceneManager.SaveCutscenes();

            // Convert the object to XML data and put it in the stream.
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            var sb = new StringBuilder();
            using (var sr = new StringWriter(sb))
            {
                // Seriaize the data.
                serializer.Serialize(sr, data);
            }

            string saveName = String.Format("{0}_{1}", PlayerManager.Name, _iSaveID);
            string saveFolder = String.Format(@"{0}\{1}", RIVER_HOLLOW_SAVES, saveName);

            if (!Directory.Exists(RIVER_HOLLOW_SAVES)) { Directory.CreateDirectory(RIVER_HOLLOW_SAVES); }
            if (!Directory.Exists(saveFolder)) { Directory.CreateDirectory(saveFolder); }
            File.WriteAllText(String.Format(@"{0}\{1}", saveFolder, saveName), sb.ToString());

            //Smaller subset of information to display on the loading screen
            SaveInfoData infoData = new SaveInfoData()
            {
                saveFile = String.Format(@"{0}\{1}", saveFolder, saveName),
                timeStamp = DateTime.Now,
                saveID = data.saveID,
                Calendar = data.Calendar,
                playerData = data.playerData
            };

            // Convert the object to XML data and put it in the stream.
            serializer = new XmlSerializer(typeof(SaveInfoData));
            sb = new StringBuilder();
            using (var sr = new StringWriter(sb))
            {
                // Seriaize the data.
                serializer.Serialize(sr, infoData);
            }

            File.WriteAllText(String.Format(@"{0}\{1}", saveFolder, INFO_FILE_NAME), sb.ToString());
        }

        public static OptionsData SaveOptions()
        {
            OptionsData data = new OptionsData
            {
                hideMiniInventory = GameManager.HideMiniInventory,
                musicVolume = SoundManager.MusicVolume,
                effectVolume = SoundManager.EffectVolume,
            };
            return data;
        }

        public static void LoadOptions(OptionsData data)
        {
            GameManager.HideMiniInventory = data.hideMiniInventory;
            SoundManager.SetEffectVolume(data.effectVolume);
            SoundManager.SetMusicVolume(data.musicVolume);
        }

        public static List<SaveInfoData> LoadFiles()
        {
            List<SaveInfoData> games = new List<SaveInfoData>();

            if (Directory.Exists(Environment.CurrentDirectory + "\\" + RIVER_HOLLOW_SAVES)) {
                foreach (string dir in Directory.GetDirectories(Environment.CurrentDirectory + "\\" + RIVER_HOLLOW_SAVES))
                {
                    string pathToSaveInfo = String.Format(@"{0}\{1}", dir, INFO_FILE_NAME);
                    if (File.Exists(pathToSaveInfo))
                    {
                        games.Add(LoadSaveInfoData(pathToSaveInfo));
                    }
                }
            }

            return games;
        }

        public static SaveInfoData LoadSaveInfoData(string path)
        {
            SaveInfoData data;

            string xml = path;
            XmlSerializer serializer = new XmlSerializer(typeof(SaveInfoData));

            using (var sr = new StreamReader(xml))
            {
                data = (SaveInfoData)serializer.Deserialize(sr);
            }

            return data;
        }

        public static SaveData LoadData(string fileName)
        {
            SaveData data;
            string xml = fileName;
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));

            using (var sr = new StreamReader(xml))
            {
                data = (SaveData)serializer.Deserialize(sr);
            }

            return data;
        }

        public static void Load(string filePath)
        {
            SaveData dataToLoad = LoadData(filePath);

            _iSaveID = dataToLoad.saveID;

            LoadOptions(dataToLoad.optionData);
            TaskManager.LoadTaskData(dataToLoad.TaskInfo);
            GameCalendar.LoadCalendar(dataToLoad.Calendar);
            EnvironmentManager.LoadEnvironment(dataToLoad.Environment);
            PlayerManager.Initialize();

            foreach (MapData mapData in dataToLoad.MapData)
            {
                RHMap map = MapManager.Maps[mapData.mapName];
                map.LoadData(mapData);
            }

            PlayerManager.LoadData(dataToLoad.playerData);

            PlayerManager.MoveToSpawn();
            PlayerManager.LoadToolData(dataToLoad.Tools);
            //Needs to be here because the Mailbox is a worldobject
            PlayerManager.PlayerMailbox.LoadData(dataToLoad.TheMailbox);

            foreach (VillagerData n in dataToLoad.VillagerData)
            {
                Villager target = DataManager.DIVillagers[n.npcID];
                target.LoadData(n);
            }

            for (int i = 0; i < dataToLoad.ShopData.Count; i++)
            {
                Shop s = GameManager.DIShops[i];
                s.LoadData(dataToLoad.ShopData[i]);

            }

            foreach (MerchantData n in dataToLoad.MerchantData)
            {
                Merchant target = DataManager.DIMerchants[n.npcID];
                target.LoadData(n);
            }

            for (int i = 0; i < dataToLoad.MerchantQueue.Count; i++)
            {
                GameManager.MerchantQueue.Add(DataManager.DIMerchants[dataToLoad.MerchantQueue[i]]);
            }

            GameManager.MoveMerchants();

            CutsceneManager.LoadCutscenes(dataToLoad.CSData);

            //After we've loaded everything, spawn the mounts in the Stables
            PlayerManager.SpawnMounts();
            
        }
        #endregion

        #region XMl Save methods
        private static StreamWriter PrepareXMLFile(string fileName, string assetType)
        {
            StreamWriter dataFile = new StreamWriter(fileName);
            dataFile.WriteLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            dataFile.WriteLine("<XnaContent xmlns:Generic=\"System.Collections.Generic\">");
            dataFile.WriteLine("  <Asset Type=\"" + assetType + "\">"); //Dictionary[int, string]
            return dataFile;
        }

        private static void CloseStreamWriter(ref StreamWriter dataFile)
        {
            dataFile.WriteLine("  </Asset>");
            dataFile.WriteLine("</XnaContent>");
            dataFile.Close();
        }

        public static void SaveXMLData(List<SpawnXMLData> dataList, string fileName)
        {
            StreamWriter dataFile = PrepareXMLFile(fileName, "Dictionary[int, string]");

            foreach (SpawnXMLData data in dataList)
            {
                WriteXMLEntry(dataFile, string.Format("      <Key>{0}</Key>", data.ID), string.Format("      <Value>{0}</Value>", data.GetTagsString()));
            }

            CloseStreamWriter(ref dataFile);
        }

        private static void WriteXMLEntry(StreamWriter dataFile, string key, string value)
        {
            dataFile.WriteLine("    <Item>");
            dataFile.WriteLine(key);
            dataFile.WriteLine(value);
            dataFile.WriteLine("    </Item>");
        }
        #endregion
    }
}
