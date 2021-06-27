using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using RiverHollow.Characters;
using RiverHollow.Buildings;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using RiverHollow.Items;
using RiverHollow.Utilities;

using static RiverHollow.Misc.Task;

namespace RiverHollow.Game_Managers
{
    public class SaveManager
    {
        static string INFO_FILE_NAME = "SaveInfo";
        public static string RIVER_HOLLOW_SAVES = "Save Games";
        static long _iSaveID = -1;

        #region Save/Load
        #region structs
        public struct SaveInfoData
        {
            [XmlElement(ElementName = "SaveFile")]
            public string saveFile;

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

            [XmlElement(ElementName = "CurrentMap")]
            public string currentMap;

            [XmlElement(ElementName = "Calendar")]
            public CalendarData Calendar;

            [XmlElement(ElementName = "Environment")]
            public EnvironmentData Environment;

            [XmlElement(ElementName = "Tools")]
            public ToolData Tools;

            [XmlArray(ElementName = "Buildings")]
            public List<BuildingData> Buildings;

            [XmlArray(ElementName = "Maps")]
            public List<MapData> MapData;

            [XmlArray(ElementName = "PlotTasks")]
            public List<TaskData> PlotTaskData;

            [XmlArray(ElementName = "TaskLog")]
            public List<TaskData> TaskLogData;

            [XmlArray(ElementName = "CurrentMissions")]
            public List<MissionData> CurrentMissions;

            [XmlArray(ElementName = "AvailableMissions")]
            public List<MissionData> AvailableMissions;

            [XmlArray(ElementName = "VillagerData")]
            public List<VillagerData> VillagerData;

            [XmlArray(ElementName = "MerchantData")]
            public List<MerchantData> MerchantData;

            [XmlArray(ElementName = "BuildingInfoData")]
            public List<BuildInfoData> BuildingInfoData;

            [XmlArray(ElementName = "ShopIData")]
            public List<ShopData> ShopData;

            [XmlElement(ElementName = "Mailbox")]
            public MailboxData TheMailbox;

        }
        public struct OptionsData
        {
            [XmlElement(ElementName = "AutoDisband")]
            public bool autoDisband;

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

            [XmlElement(ElementName = "AdventurerData")]
            public ClassedCharData adventurerData;
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
        public struct BuildingData
        {
            [XmlElement(ElementName = "name")]
            public string sName;

            [XmlElement(ElementName = "positionX")]
            public int iPosX;

            [XmlElement(ElementName = "positionY")]
            public int iPosY;

            [XmlElement(ElementName = "BuildingID")]
            public int iBuildingID;

            [XmlElement(ElementName = "PersonalID")]
            public int iPersonalID;

            [XmlElement(ElementName = "BldgLvl")]
            public int iBldgLevel;

            [XmlElement(ElementName = "UpgradeTime")]
            public int iUpgradeTimer;
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

            [XmlElement(ElementName = "Armor")]
            public ItemData armor;

            [XmlElement(ElementName = "Weapon")]
            public ItemData weapon;
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

            [XmlArray(ElementName = "Containers")]
            public List<ContainerData> containers;

            [XmlArray(ElementName = "Machines")]
            public List<MachineData> machines;

            [XmlArray(ElementName = "Plants")]
            public List<PlantData> plants;

            [XmlArray(ElementName = "Gardens")]
            public List<GardenData> gardens;

            [XmlArray(ElementName = "WarpPoints")]
            public List<WarpPointData> warpPoints;

        }
        public struct VillagerData
        {
            [XmlElement(ElementName = "NPCID")]
            public int npcID;

            [XmlElement(ElementName = "Introduced")]
            public bool introduced;

            [XmlElement(ElementName = "Arrived")]
            public bool arrived;

            [XmlElement(ElementName = "ArrivalDelay")]
            public int arrivalDelay;

            [XmlElement(ElementName = "Friendship")]
            public int friendship;

            [XmlArray(ElementName = "Collection")]
            public List<bool> collection;

            [XmlElement(ElementName = "Married")]
            public bool married;

            [XmlElement(ElementName = "CanJoin")]
            public bool canJoinParty;

            [XmlElement(ElementName = "CanGiveGift")]
            public bool canGiveGift;

            [XmlElement(ElementName = "AdventurerData")]
            public ClassedCharData classedData;

            [XmlArray(ElementName = "SpokenKeys")]
            public List<string> spokenKeys;
        }
        public struct MerchantData
        {
            [XmlElement(ElementName = "NPCID")]
            public int npcID;

            [XmlElement(ElementName = "Introduced")]
            public bool introduced;

            [XmlElement(ElementName = "ArrivalDelay")]
            public int arrivalDelay;

            [XmlArray(ElementName = "SpokenKeys")]
            public List<string> spokenKeys;
        }
        public struct CollectionData
        {
            [XmlElement(ElementName = "ItemID")]
            public int itemID;

            [XmlElement(ElementName = "Given")]
            public bool given;
        }
        public struct WorldObjectData
        {
            [XmlElement(ElementName = "WorldObjectID")]
            public int worldObjectID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;
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
        public struct PlantData
        {
            [XmlElement(ElementName = "PlantID")]
            public int ID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;

            [XmlElement(ElementName = "currentState")]
            public int currentState;

            [XmlElement(ElementName = "daysLeft")]
            public int daysLeft;
        }
        public struct ContainerData
        {
            [XmlElement(ElementName = "ContainerID")]
            public int containerID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;

            [XmlElement(ElementName = "Rows")]
            public int rows;

            [XmlElement(ElementName = "Columns")]
            public int cols;

            [XmlArray(ElementName = "Items")]
            public List<ItemData> Items;
        }
        public struct MachineData
        {
            [XmlElement(ElementName = "MachineID")]
            public int ID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;

            [XmlElement(ElementName = "ProcessedTime")]
            public double processedTime;

            [XmlElement(ElementName = "ProcessingItemID")]
            public int currentItemID;

            [XmlElement(ElementName = "HeldItemID")]
            public int heldItemID;
        }
        public struct WarpPointData
        {
            [XmlElement(ElementName = "WarpPointID")]
            public int ID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;

            [XmlElement(ElementName = "Active")]
            public bool active;
        }
        public struct FloorData
        {
            [XmlElement(ElementName = "FloorID")]
            public int ID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;
        }
        public struct GardenData
        {
            [XmlElement(ElementName = "GardenID")]
            public int ID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;

            [XmlElement(ElementName = "Plantdata")]
            public PlantData plantData;
        }
        public struct ShopData
        {
            [XmlElement(ElementName = "ShopID")]
            public int shopID;

            [XmlElement(ElementName = "LockedItems")]
            public string merchUnlockedString;
        }
        public struct ToolData
        {
            [XmlElement(ElementName = "PickID")]
            public int pickID;

            [XmlElement(ElementName = "AxeID")]
            public int axeID;

            [XmlElement(ElementName = "ScytheID")]
            public int scytheID;
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
        #endregion

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
            SaveData data = new SaveData()
            {
                saveID = GetSaveID(),
                currentMap = PlayerManager.CurrentMap,
                Calendar = GameCalendar.SaveCalendar(),
                Environment = EnvironmentManager.SaveEnvironment(),
                Tools = PlayerManager.SaveToolData(),
                Buildings = new List<BuildingData>(),
                MapData = new List<MapData>(),
                PlotTaskData = new List<TaskData>(),
                TaskLogData = new List<TaskData>(),
                CurrentMissions = new List<MissionData>(),
                AvailableMissions = new List<MissionData>(),
                VillagerData = new List<VillagerData>(),
                MerchantData = new List<MerchantData>(),
                BuildingInfoData = new List<BuildInfoData>(),
                ShopData = new List<ShopData>(),
                TheMailbox = PlayerManager.PlayerMailbox.SaveData(),
                optionData = SaveOptions()
            };

            PlayerData playerData = PlayerManager.SaveData();

            // Initialize the new data values.
            foreach (Item i in InventoryManager.PlayerInventory)
            {
                ItemData itemData = Item.SaveData(i);
                playerData.Items.Add(itemData);
            }

            data.playerData = playerData;

            foreach (Building b in PlayerManager.BuildingList)
            {
                data.Buildings.Add(b.SaveData());
            }

            foreach (RHMap tileMap in MapManager.Maps.Values)
            {
                data.MapData.Add(tileMap.SaveData());
            }

            foreach (Task q in GameManager.DITasks.Values)
            {
                data.PlotTaskData.Add(q.SaveData());
            }

            foreach (Task q in PlayerManager.TaskLog)
            {
                data.TaskLogData.Add(q.SaveData());
            }

            foreach (Mission m in MissionManager.AvailableMissions)
            {
                data.AvailableMissions.Add(m.SaveData());
            }

            foreach (Mission m in MissionManager.CurrentMissions)
            {
                data.CurrentMissions.Add(m.SaveData());
            }

            foreach (Villager n in DataManager.DIVillagers.Values)
            {
                data.VillagerData.Add(n.SaveData());
            }

            foreach (Merchant m in DataManager.DIMerchants.Values)
            {
                data.MerchantData.Add(m.SaveData());
            }

            foreach (BuildInfo b in GameManager.DIBuildInfo.Values)
            {
                data.BuildingInfoData.Add(b.SaveData());
            }

            foreach(KeyValuePair<int, List<Merchandise>> kvp in GameManager.DIShops)
            {
                string value = string.Empty;
                foreach(Merchandise m in kvp.Value)
                {
                    value += m.Unlocked;

                    if(m != kvp.Value[kvp.Value.Count - 1]) { value += "-"; }
                }

                ShopData sData = new ShopData
                {
                    shopID = kvp.Key,
                    merchUnlockedString = value
                };
                data.ShopData.Add(sData);
            }

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
                autoDisband = GameManager.AutoDisband,
                hideMiniInventory = GameManager.HideMiniInventory,
                musicVolume = SoundManager.MusicVolume,
                effectVolume = SoundManager.EffectVolume,
            };
            return data;
        }

        public static void LoadOptions(OptionsData data)
        {
            GameManager.AutoDisband = data.autoDisband;
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
            MapManager.CurrentMap = MapManager.Maps[dataToLoad.currentMap];
            PlayerManager.Initialize();
            PlayerManager.LoadData(dataToLoad.playerData);
            PlayerManager.CurrentMap = MapManager.Maps[dataToLoad.currentMap].Name;
            PlayerManager.World.Position = Util.SnapToGrid(MapManager.Maps[PlayerManager.CurrentMap].GetCharacterSpawn("PlayerSpawn"));
            PlayerManager.World.DetermineFacing(new Vector2(0, 1));
            LoadOptions(dataToLoad.optionData);
            GameCalendar.LoadCalendar(dataToLoad.Calendar);
            EnvironmentManager.LoadEnvironment(dataToLoad.Environment);
            PlayerManager.LoadToolData(dataToLoad.Tools);
            foreach (BuildingData b in dataToLoad.Buildings)
            {
                Building newBuilding = DataManager.GetBuilding(b.iBuildingID);
                newBuilding.LoadData(b);
                newBuilding.PlaceOnMap(newBuilding.MapPosition, MapManager.Maps[MapManager.HomeMap]);
            }

            foreach (MapData mapData in dataToLoad.MapData)
            {
                RHMap map = MapManager.Maps[mapData.mapName];
                map.LoadData(mapData);
            }

            //Needs to be here because the Mailbox is a worldobject
            PlayerManager.PlayerMailbox.LoadData(dataToLoad.TheMailbox);

            foreach (TaskData q in dataToLoad.PlotTaskData)
            {
                Task plotTask = GameManager.DITasks[q.taskID];
                plotTask.LoadData(q);
            }
            foreach (TaskData q in dataToLoad.TaskLogData)
            {
                Task newTask = new Task();

                //We've already loaded plotQuests, no need to redo it
                if (q.taskID == -1) { newTask.LoadData(q); }
                else { newTask = GameManager.DITasks[q.taskID]; }

                PlayerManager.AddToTaskLog(newTask);
            }
            foreach (MissionData m in dataToLoad.CurrentMissions)
            {
                Mission newMission = new Mission();
                newMission.LoadData(m);
                MissionManager.CurrentMissions.Add(newMission);
            }
            foreach (MissionData m in dataToLoad.AvailableMissions)
            {
                Mission newMission = new Mission();
                newMission.LoadData(m);
                MissionManager.AvailableMissions.Add(newMission);
            }
            foreach (VillagerData n in dataToLoad.VillagerData)
            {
                Villager target = DataManager.DIVillagers[n.npcID];
                target.LoadData(n);
            }

            foreach (MerchantData n in dataToLoad.MerchantData)
            {
                Merchant target = DataManager.DIMerchants[n.npcID];
                target.LoadData(n);
            }

            foreach (BuildInfoData n in dataToLoad.BuildingInfoData)
            {
                GameManager.DIBuildInfo[n.id].Built = n.built;
                if (n.unlocked) { GameManager.DIBuildInfo[n.id].Unlock(); }
            }

            foreach(ShopData s in dataToLoad.ShopData)
            {
                string[] split = s.merchUnlockedString.Split('-');
                for (int i = 0; i < GameManager.DIShops[s.shopID].Count; i++)
                {
                    if (i < split.Length && split[i].Equals("True"))
                    {
                        GameManager.DIShops[s.shopID][i].Unlock();
                    }
                }
            }

            //foreach (BuildInfo b in GameManager.DIBuildInfo.Values)
            //{
            //    data.BuildingInfoData.Add(b.SaveData());
            //}

            //string value = string.Empty;
            //foreach (Merchandise m in kvp.Value)
            //{
            //    value += m.Unlocked;

            //    if (m != kvp.Value[kvp.Value.Count - 1]) { value += "-"; }
            //}

            //ShopData sData = new ShopData
            //{
            //    shopID = kvp.Key,
            //    merchStatusString = value
            //};
            //data.ShopData.Add(sData);
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
