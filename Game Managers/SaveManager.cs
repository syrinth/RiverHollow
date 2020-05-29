﻿using Microsoft.Xna.Framework;
using RiverHollow.Actors;
using RiverHollow.Buildings;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using static RiverHollow.Misc.Quest;

namespace RiverHollow.Game_Managers
{
    public class SaveManager
    {
        static long _iSaveID = -1;

        #region Save/Load
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

            [XmlArray(ElementName = "Buildings")]
            public List<BuildingData> Buildings;

            [XmlArray(ElementName = "Maps")]
            public List<MapData> MapData;

            [XmlArray(ElementName = "Upgrades")]
            public List<UpgradeData> UpgradeData;

            [XmlArray(ElementName = "PlotQuests")]
            public List<QuestData> PlotQuestData;

            [XmlArray(ElementName = "QuestLog")]
            public List<QuestData> QuestLogData;

            [XmlArray(ElementName = "CurrentMissions")]
            public List<MissionData> CurrentMissions;

            [XmlArray(ElementName = "AvailableMissions")]
            public List<MissionData> AvailableMissions;

            [XmlArray(ElementName = "NPCData")]
            public List<NPCData> NPCData;

            [XmlArray(ElementName = "EligibleData")]
            public List<EligibleNPCData> EligibleData;
        }
        public struct OptionsData
        {
            [XmlElement(ElementName = "AutoDisband")]
            public bool autoDisband;

            [XmlElement(ElementName = "HideMiniInventory")]
            public bool hideMiniInventory;
        }
        public struct PlayerData
        {
            [XmlElement(ElementName = "Name")]
            public string name;

            [XmlElement(ElementName = "Money")]
            public int money;

            [XmlElement(ElementName = "CurrentClass")]
            public int currentClass;

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

            [XmlElement(ElementName = "currWeather")]
            public int currWeather;

            [XmlElement(ElementName = "currSeasonPrecipDays")]
            public int currSeasonPrecipDays;
        }
        public struct BuildingData
        {
            [XmlArray(ElementName = "Workers")]
            public List<WorkerData> Workers;

            [XmlArray(ElementName = "Containers")]
            public List<ContainerData> containers;

            [XmlArray(ElementName = "Machines")]
            public List<MachineData> machines;

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

            [XmlElement(ElementName = "BuildingChest")]
            public ContainerData buildingChest;

            [XmlElement(ElementName = "Pantry")]
            public ContainerData pantry;
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

            [XmlArray(ElementName = "Floors")]
            public List<FloorData> floors;

            [XmlArray(ElementName = "Earth")]
            public List<FloorData> earth;
        }
        public struct UpgradeData
        {
            [XmlElement(ElementName = "UpgradeID")]
            public int upgradeID;

            [XmlElement(ElementName = "Enabled")]
            public bool enabled;
        }
        public struct NPCData
        {
            [XmlElement(ElementName = "NPCID")]
            public int npcID;

            [XmlElement(ElementName = "Introduced")]
            public bool introduced;

            [XmlElement(ElementName = "Friendship")]
            public int friendship;

            [XmlArray(ElementName = "Collection")]
            public List<CollectionData> collection;
        }

        public struct EligibleNPCData
        {
            [XmlElement(ElementName = "NPCData")]
            public NPCData npcData;

            [XmlElement(ElementName = "Married")]
            public bool married;

            [XmlElement(ElementName = "CanJoin")]
            public bool canJoinParty;

            [XmlElement(ElementName = "CanGiveGift")]
            public bool canGiveGift;

            [XmlElement(ElementName = "AdventurerData")]
            public ClassedCharData classedData;
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
        public struct FloorData
        {
            [XmlElement(ElementName = "FloorID")]
            public int ID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;
        }
        public struct EarthData
        {
            [XmlElement(ElementName = "Watered")]
            public bool watered;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;
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

        public static long GetSaveID()
        {
            if (_iSaveID == -1)
            {
                _iSaveID = long.Parse(string.Format("{0}{1}{2}{3}", DateTime.Now.Year, DateTime.Now.Day, DateTime.Now.Second, DateTime.Now.Millisecond));
            }

            return _iSaveID;
        }

        public static string Save()
        {
            SaveData data = new SaveData()
            {
                saveID = GetSaveID(),
                currentMap = PlayerManager.CurrentMap,
                Calendar = GameCalendar.SaveCalendar(),
                Buildings = new List<BuildingData>(),
                MapData = new List<MapData>(),
                UpgradeData = new List<UpgradeData>(),
                PlotQuestData = new List<QuestData>(),
                QuestLogData = new List<QuestData>(),
                CurrentMissions = new List<MissionData>(),
                AvailableMissions = new List<MissionData>(),
                NPCData = new List<NPCData>(),
                EligibleData = new List<EligibleNPCData>(),
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

            foreach (Building b in PlayerManager.Buildings)
            {
                data.Buildings.Add(b.SaveData());
            }

            foreach (RHMap tileMap in MapManager.Maps.Values)
            {
                data.MapData.Add(tileMap.SaveData());
            }

            foreach (Upgrade u in GameManager.DiUpgrades.Values)
            {
                UpgradeData upgData = new UpgradeData
                {
                    upgradeID = u.ID,
                    enabled = u.Enabled
                };
                data.UpgradeData.Add(upgData);
            }

            foreach (Quest q in GameManager.DiQuests.Values)
            {
                data.PlotQuestData.Add(q.SaveData());
            }

            foreach (Quest q in PlayerManager.QuestLog)
            {
                data.QuestLogData.Add(q.SaveData());
            }

            foreach (Mission m in MissionManager.AvailableMissions)
            {
                data.AvailableMissions.Add(m.SaveData());
            }

            foreach (Mission m in MissionManager.CurrentMissions)
            {
                data.CurrentMissions.Add(m.SaveData());
            }

            foreach (Villager n in DataManager.DiNPC.Values)
            {
                if (n.IsEligible()) { data.EligibleData.Add(((EligibleNPC)n).SaveData()); }
                else { data.NPCData.Add(n.SaveData()); }
            }

            // Convert the object to XML data and put it in the stream.
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            var sb = new StringBuilder();
            using (var sr = new StringWriter(sb))
            {
                // Seriaize the data.
                serializer.Serialize(sr, data);
            }

            File.WriteAllText(PlayerManager.Name + _iSaveID + ".rh", sb.ToString());
            return sb.ToString();
        }

        public static OptionsData SaveOptions()
        {
            OptionsData data = new OptionsData
            {
                autoDisband = GameManager.AutoDisband,
                hideMiniInventory = GameManager.HideMiniInventory
            };
            return data;
        }

        public static void LoadOptions(OptionsData data)
        {
            GameManager.AutoDisband = data.autoDisband;
            GameManager.HideMiniInventory = data.hideMiniInventory;
        }

        public static List<SaveData> LoadFiles()
        {
            List<SaveData> games = new List<SaveData>();

            foreach (string s in Directory.GetFiles(System.Environment.CurrentDirectory, "*.rh"))
            {
                games.Add(LoadData(s));
            }

            return games;
        }

        public static SaveData LoadData(string fileName)
        {
            SaveData data;
            string xml = fileName;
            string _byteOrderMarkUtf16 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (xml.StartsWith(_byteOrderMarkUtf16))
            {
                xml = xml.Remove(0, _byteOrderMarkUtf16.Length);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));

            using (var sr = new StreamReader(xml))
            {
                data = (SaveData)serializer.Deserialize(sr);
            }

            return data;
        }

        public static void Load(SaveData data)
        {
            _iSaveID = data.saveID;
            MapManager.CurrentMap = MapManager.Maps[data.currentMap];
            PlayerManager.Initialize();
            PlayerManager.LoadData(data.playerData);
            PlayerManager.CurrentMap = MapManager.Maps[data.currentMap].Name;
            PlayerManager.World.Position = Util.SnapToGrid(MapManager.Maps[PlayerManager.CurrentMap].GetCharacterSpawn("PlayerSpawn"));
            PlayerManager.World.DetermineFacing(new Vector2(0, 1));
            LoadOptions(data.optionData);
            GameCalendar.LoadCalendar(data.Calendar);
            foreach (BuildingData b in data.Buildings)
            {
                Building newBuilding = DataManager.GetBuilding(b.iBuildingID);
                newBuilding.LoadData(b);
                bool place = b.iBuildingID == 0 || b.iBldgLevel != 0;
                MapManager.Maps[MapManager.HomeMap].AddBuilding(newBuilding, place, place);
            }

            foreach (MapData mapData in data.MapData)
            {
                RHMap map = MapManager.Maps[mapData.mapName];
                map.LoadData(mapData);
            }
            foreach (UpgradeData u in data.UpgradeData)
            {
                GameManager.DiUpgrades[u.upgradeID].Enabled = u.enabled;
            }
            foreach (QuestData q in data.PlotQuestData)
            {
                Quest plotQuest = GameManager.DiQuests[q.questID];
                plotQuest.LoadData(q);
            }
            foreach (QuestData q in data.QuestLogData)
            {
                Quest newQuest = new Quest();
                newQuest.LoadData(q);
                PlayerManager.AddToQuestLog(newQuest);
            }
            foreach (MissionData m in data.CurrentMissions)
            {
                Mission newMission = new Mission();
                newMission.LoadData(m);
                MissionManager.CurrentMissions.Add(newMission);
            }
            foreach (MissionData m in data.AvailableMissions)
            {
                Mission newMission = new Mission();
                newMission.LoadData(m);
                MissionManager.AvailableMissions.Add(newMission);
            }
            foreach (NPCData n in data.NPCData)
            {
                Villager target = DataManager.DiNPC[n.npcID];
                target.LoadData(n);
            }

            foreach (EligibleNPCData n in data.EligibleData)
            {
                EligibleNPC target = (EligibleNPC)DataManager.DiNPC[n.npcData.npcID];
                target.LoadData(n);
            }
        }
        #endregion
    }
}
