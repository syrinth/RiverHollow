using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RiverHollow.Characters;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.WorldObjects;
using RiverHollow.Misc;
using RiverHollow.Tile_Engine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using static RiverHollow.WorldObjects.Door;
using RiverHollow.Characters.NPCs;
using static RiverHollow.Misc.Quest;

namespace RiverHollow.Game_Managers
{
    public static class GameManager
    {
        public enum ActionEnum { Action, Menu, Spell };
        public enum TargetEnum { Enemy, Ally};
        public enum MenuEnum { Action, Cast, UseItem };
        public enum RangeEnum { Self, Melee, Ranged, Column };
        public enum AreaEffectEnum { Single, Area };
        public enum ElementEnum { None, Fire, Ice, Lightning };
        public enum AttackTypeEnum { Physical, Magical };
        public enum ElementAlignment { Neutral, Vulnerable, Resists };
        public enum ConditionEnum { None, KO, Poisoned, Silenced };
        public enum WorkerTypeEnum { None, Magic, Martial };
        public enum WeaponEnum { None, Staff, Sword };
        public enum ArmorEnum { None, Cloth, Heavy };
        public static float Scale = 4f;
        public static int TileSize = 16;
        public static int MaxBldgLevel = 3;
        public static Dictionary<int, Upgrade> DiUpgrades;
        public static Dictionary<int, Quest> DIQuests;

        public static Item gmActiveItem;
        public static NPC gmNPC;
        public static Spirit gmSpirit;
        public static KeyDoor gmDoor;
        public static Vector2 gmCurrentItem;

        static long _iSaveID = -1;
        public static int MAX_NAME_LEN = 10;

        public static void LoadContent(ContentManager Content)
        {
            DiUpgrades = new Dictionary<int, Upgrade>();
            foreach (KeyValuePair<int, string> kvp in GameContentManager.DiUpgrades)
            {
                DiUpgrades.Add(kvp.Key, new Upgrade(kvp.Key, kvp.Value));
            }
        }

        public static void LoadQuests(ContentManager Content)
        {
            DIQuests = new Dictionary<int, Quest>();
            foreach (KeyValuePair<int, string> kvp in GameContentManager.DiQuests)
            {
                DIQuests.Add(kvp.Key, new Quest(kvp.Value, kvp.Key));
            }
        }

        public static void UseItem()
        {
            gmActiveItem.UseItem();
            gmActiveItem = null;
        }

        public static void ClearGMObjects()
        {
            gmNPC = null;
            gmActiveItem = null;
            gmDoor = null;
            gmSpirit = null;
        }

        #region States
        private static bool _scrying;
        private enum StateEnum { Paused, Running, Information }
        private static StateEnum _state;
        private enum InputEnum { None, Input }
        private static InputEnum _inputState;

        private enum MapEnum { None, WorldMap, Combat }
        private static MapEnum _mapState;

        private enum EnumBuildType { None, Construct, Destroy, Move }
        private static EnumBuildType _buildType;

        public static void ReadInput() { _inputState = InputEnum.Input; }
        public static void DontReadInput() { _inputState = InputEnum.None; }
        public static bool TakingInput() { return _inputState == InputEnum.Input; }

        public static void Pause() { _state = StateEnum.Paused; }
        public static bool IsPaused() { return _state == StateEnum.Paused; }

        public static void Unpause() { _state = StateEnum.Running; }
        public static bool IsRunning() { return _state == StateEnum.Running; }

        public static void Scry(bool val) {
            _state = StateEnum.Running;
            _scrying = val;
        }
        public static bool Scrying() { return _scrying; }

        public static bool InCombat() { return _mapState == MapEnum.Combat; }
        public static void GoToCombat() {
            _mapState = MapEnum.Combat;
            GUIManager.SetScreen(new CombatScreen());
        }
        public static bool OnMap() { return _mapState == MapEnum.WorldMap; }
        public static void GoToWorldMap() {
            _mapState = MapEnum.WorldMap;
            GUIManager.SetScreen(new HUDScreen());
        }
        public static bool Informational() { return _mapState == MapEnum.None; }
        public static void GoToInformation() {
            _mapState = MapEnum.None;
            _state = StateEnum.Paused;
        }

        public static bool Constructing() { return _buildType == EnumBuildType.Construct; }
        public static void ConstructBuilding() { _buildType = EnumBuildType.Construct; }
        public static bool MovingBuildings() { return _buildType == EnumBuildType.Move; }
        public static void MoveBuilding() { _buildType = EnumBuildType.Move; }
        public static bool DestroyingBuildings() { return _buildType == EnumBuildType.Destroy; }
        public static void DestroyBuilding() { _buildType = EnumBuildType.Destroy; }
        public static void FinishedBuilding() { _buildType = EnumBuildType.None; }

        public static void BackToMain()
        {
            GUIManager.SetScreen(new HUDScreen());
            _state = StateEnum.Running;
            _mapState = MapEnum.WorldMap;
            ClearGMObjects();
        }
        #endregion

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

            [XmlArray(ElementName = "NPCData")]
            public List<NPCData> NPCData;

            [XmlArray(ElementName = "EligibleData")]
            public List<EligibleNPCData> EligibleData;
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
            public AdventurerData adventurerData;
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
            public string name;

            [XmlElement(ElementName = "positionX")]
            public int positionX;

            [XmlElement(ElementName = "positionY")]
            public int positionY;

            [XmlElement(ElementName = "BuildingID")]
            public int buildingID;

            [XmlElement(ElementName = "PersonalID")]
            public int id;

            [XmlElement(ElementName = "BldgLvl")]
            public int bldgLvl;

            [XmlElement(ElementName = "BuildingChest")]
            public ContainerData buildingChest;

            [XmlElement(ElementName = "Pantry")]
            public ContainerData pantry;
        }
        public struct WorkerData
        {
            [XmlElement(ElementName = "WorkerID")]
            public int workerID;

            [XmlElement(ElementName = "Name")]
            public string name;

            [XmlElement(ElementName = "AdventurerData")]
            public AdventurerData advData;

            [XmlElement(ElementName = "Mood")]
            public int mood;

            [XmlElement(ElementName = "ProcessedTime")]
            public double processedTime;

            [XmlElement(ElementName = "ProcessingItemID")]
            public int currentItemID;

            [XmlElement(ElementName = "HeldItemID")]
            public int heldItemID;
        }
        public struct AdventurerData
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
            public AdventurerData adventurerData;
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
                NPCData = new List<NPCData>(),
                EligibleData = new List<EligibleNPCData>()
            };

            PlayerData playerData = PlayerManager.SaveData();

            // Initialize the new data values.
            foreach (Item i in InventoryManager.PlayerInventory)
            {
                ItemData itemData = Item.SaveData(i);
                playerData.Items.Add(itemData);
            }

            data.playerData = playerData;

            foreach (WorkerBuilding b in PlayerManager.Buildings)
            {
                data.Buildings.Add(b.SaveData());
            }

            foreach (RHMap tileMap in MapManager.Maps.Values)
            {
                data.MapData.Add(tileMap.SaveData());
            }

            foreach (Upgrade u in DiUpgrades.Values)
            {
                UpgradeData upgData = new UpgradeData
                {
                    upgradeID = u.ID,
                    enabled = u.Enabled
                };
                data.UpgradeData.Add(upgData);
            }

            foreach (Quest q in DIQuests.Values)
            {
                data.PlotQuestData.Add(q.SaveData());
            }

            foreach (Quest q in PlayerManager.QuestLog)
            {
                data.QuestLogData.Add(q.SaveData());
            }

            foreach (NPC n in CharacterManager.DiNPC.Values)
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

            File.WriteAllText(PlayerManager.Name+_iSaveID+".rh", sb.ToString());
            return sb.ToString();
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
            GameCalendar.LoadCalendar(data.Calendar); 
            foreach (BuildingData b in data.Buildings)
            {
                WorkerBuilding newBuilding = ObjectManager.GetBuilding(b.buildingID);
                newBuilding.LoadData(b);
                MapManager.Maps[MapManager.HomeMap].AddBuilding(newBuilding, false);
            }

            foreach (MapData mapData in data.MapData)
            {
                RHMap map = MapManager.Maps[mapData.mapName];
                map.LoadData(mapData);
            }
            foreach (UpgradeData u in data.UpgradeData)
            {
                DiUpgrades[u.upgradeID].Enabled = u.enabled;
            }
            foreach (QuestData q in data.PlotQuestData)
            {
                Quest plotQuest = DIQuests[q.questID];
                plotQuest.LoadData(q);
            }
            foreach (QuestData q in data.QuestLogData)
            {
                Quest newQuest = new Quest();
                newQuest.LoadData(q);
                PlayerManager.AddToQuestLog(newQuest);
            }
            foreach (NPCData n in data.NPCData)
            {
                NPC target = CharacterManager.DiNPC[n.npcID];
                target.LoadData(n);
            }

            foreach (EligibleNPCData n in data.EligibleData)
            {
                EligibleNPC target = (EligibleNPC)CharacterManager.DiNPC[n.npcData.npcID];
                target.LoadData(n);
            }
        }
        #endregion
    }

    public class Upgrade
    {
        enum UpgradeTypeEnum { Building }
        UpgradeTypeEnum _type;
        int _id;
        public int ID { get => _id; }
        string _name;
        public string Name { get => _name; }
        string _description;
        public string Description { get => _description; }
        int _iCost;
        public int MoneyCost { get => _iCost; }
        List<KeyValuePair<int, int>> _liRequiredItems;
        public List<KeyValuePair<int, int>> LiRquiredItems { get => _liRequiredItems; }
        public bool Enabled;

        public Upgrade(int id, string strData)
        {
            _id = id;
            _liRequiredItems = new List<KeyValuePair<int, int>>();

            GameContentManager.GetUpgradeText(_id, ref _name, ref _description);

            string[] strSplit = Util.FindTags(strData);
            foreach (string s in strSplit)
            {
                string[] tagType = s.Split(':');
                if (tagType[0].Equals("Type"))
                {
                    _type = Util.ParseEnum<UpgradeTypeEnum>(tagType[1]);
                }
                else if (tagType[0].Equals("Cost"))
                {
                    _iCost = int.Parse(tagType[1]);
                }
                else if (tagType[0].Equals("ItemReq"))
                {
                    string[] itemSplit = tagType[1].Split(':');
                    for (int i = 0; i < itemSplit.Length; i++)
                    {
                        string[] entrySplit = itemSplit[i].Split('-');
                        _liRequiredItems.Add(new KeyValuePair<int, int>(int.Parse(entrySplit[0]), int.Parse(entrySplit[1])));
                    }
                }
            } 
        }
    }
}
