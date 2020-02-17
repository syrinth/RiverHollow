using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RiverHollow.Actors;
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
using static RiverHollow.Misc.Quest;
using static RiverHollow.Actors.ShopKeeper;
using RiverHollow.Game_Managers.GUIComponents.GUIObjects;
using RiverHollow.Buildings;

namespace RiverHollow.Game_Managers
{
    public static class GameManager
    {
        public enum ZoneEnum { Forest, Mountain, Field, Swamp, Town };
        public enum DirectionEnum { Up, Down, Right, Left };
        public enum CardinalDirectionsEnum { North, NorthEast, East, SouthEast, South,  SouthWest, West, NorthWest};
        public enum VerbEnum { Walk, Idle, Hurt, Critical, Ground, Air, UseTool, Attack, Cast, MakeItem };
        public enum AnimationEnum { None, Spawn, KO, Win, PlayAnimation, Rain, Snow, ObjectIdle };

        public enum ToolAnimEnum { Down, Up, Left, Right }
        public enum WorldObjAnimEnum { Idle, Working, Shake, Gathered };
        public enum PlantEnum { Seeds, Seedling, Adult, Ripe };

        public enum StatEnum { Atk, Str, Def, Mag, Res, Spd, Vit, Crit, Evade};
        public enum PotencyBonusEnum { None, Conditions, Summons };
        public enum EquipmentEnum { Armor, Weapon, Accessory, Head, Wrist};
        public enum PlayerColorEnum { None, Eyes, Hair, Skin };
        public enum ActionEnum { Action, Menu, Spell };
        public enum SkillTagsEnum { Bonus, Harm, Heal, Push, Pull, Remove, Retreat, Step, Status, Summon};
        public enum TargetEnum { Enemy, Ally};
        public enum AreaTypeEnum { Single, Cross, Ring, Line };
        public enum MenuEnum { Action, Special, UseItem };
        public enum ElementEnum { None, Fire, Ice, Lightning };
        public enum AttackTypeEnum { Physical, Magical };
        public enum ElementAlignment { Neutral, Vulnerable, Resists };
        public enum ConditionEnum { None, KO, Poisoned, Silenced };
        public enum WorkerTypeEnum { None, Magic, Martial };
        public enum WeaponEnum { None, Spear, Shield, Rapier, Bow, Wand, Knife, Orb, Staff };
        public enum ArmorEnum { None, Cloth, Light, Heavy };
        public enum ArmorSlotEnum { None, Head, Armor, Wrist };
        public enum SpawnConditionEnum { Spring, Summer, Winter, Fall, Precipitation, Night, Forest, Mountain, Swamp, Plains };
        public enum ToolEnum { Pick, Axe, Shovel, WateringCan, Harp, Lantern };
        public static float Scale = 4f;
        public const int TileSize = 16;
        public static int ScaledTileSize => (int)(TileSize * Scale);
        public static int MaxBldgLevel = 3;
        public static Dictionary<int, Upgrade> DiUpgrades;
        public static Dictionary<int, Quest> DiQuests;

        public static Merchandise gmMerchandise;
        public static TalkingActor CurrentNPC;
        public static Item gmActiveItem;
        public static Spirit gmSpirit;
        public static KeyDoor CurrentDoor;

        static long _iSaveID = -1;
        public static int MAX_NAME_LEN = 10;

        public static bool AutoDisband;
        public static bool HideMiniInventory = true;

        public const float TOOL_ANIM_SPEED = 0.15f;

        public static int HUDItemRow;
        public static int HUDItemCol;

        public static void LoadContent(ContentManager Content)
        {
            DiUpgrades = new Dictionary<int, Upgrade>();
            foreach (KeyValuePair<int, string> kvp in DataManager.DiUpgrades)
            {
                DiUpgrades.Add(kvp.Key, new Upgrade(kvp.Key, kvp.Value));
            }
        }

        public static void LoadQuests(ContentManager Content)
        {
            DiQuests = new Dictionary<int, Quest>();
            foreach (KeyValuePair<int, string> kvp in DataManager.DiQuests)
            {
                DiQuests.Add(kvp.Key, new Quest(kvp.Value, kvp.Key));
            }
        }

        public static void ProcessTextInteraction(string selectedAction)
        {
            if (selectedAction.Equals("SleepNow"))
            {
                Vector2 pos = PlayerManager.World.CollisionBox.Center.ToVector2();
                PlayerManager.SetPath(TravelManager.FindPathToLocation(ref pos, MapManager.CurrentMap.DictionaryCharacterLayer["PlayerSpawn"], MapManager.CurrentMap.Name));
                TravelManager.Clear();
                GameManager.BackToMain();
            }
            else if (selectedAction.Equals("OpenDoor"))
            {
                GUIManager.OpenMainObject(new HUDInventoryDisplay());
            }
            else if (selectedAction.Contains("SellContract") && GameManager.CurrentNPC != null)
            {
                if (GameManager.CurrentNPC.IsAdventurer())
                {
                    ((Adventurer)GameManager.CurrentNPC).Building.RemoveWorker((Adventurer)GameManager.CurrentNPC);
                    PlayerManager.AddMoney(1000);
                    GameManager.BackToMain();
                }
            }
        }

        public static void ClearGMObjects()
        {
            CurrentNPC = null;
            CurrentDoor = null;
            gmActiveItem = null;
            gmSpirit = null;
        }

        public static OptionsData SaveOptions()
        {
            OptionsData data = new OptionsData
            {
                autoDisband = AutoDisband,
                hideMiniInventory = HideMiniInventory
            };
            return data;
        }

        public static void LoadOptions(OptionsData data)
        {
            AutoDisband = data.autoDisband;
            HideMiniInventory = data.hideMiniInventory;
        }

        #region Held Objects
        static Item _heldItem;
        public static Item HeldItem { get => _heldItem; }
        static Building _heldBuilding;
        public static Building HeldBuilding { get => _heldBuilding; }

        /// <summary>
        /// Grabs a building to be placed and/or moved.
        /// </summary>
        /// <returns>True if the building exists</returns>
        public static bool PickUpBuilding(Building bldg)
        {
            bool rv = false;
            if (bldg != null)
            {
                _heldBuilding = bldg;
                rv = true;
            }

            return rv;
        }
        public static void DropBuilding()
        {
            _heldBuilding = null;
        }

        /// <summary>
        /// Grabs an item to be moved around inventory
        /// </summary>
        /// <returns>True if the item exists</returns>
        public static bool GrabItem(Item item)
        {
            bool rv = false;
            if (item != null)
            {
                _heldItem = item;
                rv = true;
            }

            return rv;
        }
        public static void DropItem()
        {
            _heldItem = null;
        }
        #endregion

        #region States
        private static bool _scrying;
        private enum StateEnum { Paused, Running, Information }
        private static StateEnum _state;
        private enum InputEnum { None, Input }
        private static InputEnum _inputState;

        private enum MapEnum { None, WorldMap }
        private static MapEnum _mapState;

        private enum EnumBuildType { None, Construct, Destroy, Move }
        private static EnumBuildType _buildType;

        public static void ReadInput() { _inputState = InputEnum.Input; }
        public static void DontReadInput() { _inputState = InputEnum.None; }
        public static bool TakingInput() { return _inputState == InputEnum.Input; }

        public static void Pause() {
            _state = StateEnum.Paused;
        }
        public static bool IsPaused() { return _state == StateEnum.Paused; }

        public static void Unpause() { _state = StateEnum.Running; }
        public static bool IsRunning() { return _state == StateEnum.Running; }

        public static void Scry(bool val) {
            _state = StateEnum.Running;
            _scrying = val;
        }
        public static bool Scrying() { return _scrying; }

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
        public static void LeaveBuildMode() { _buildType = EnumBuildType.None; }

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
                optionData = GameManager.SaveOptions()
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

            foreach (Upgrade u in DiUpgrades.Values)
            {
                UpgradeData upgData = new UpgradeData
                {
                    upgradeID = u.ID,
                    enabled = u.Enabled
                };
                data.UpgradeData.Add(upgData);
            }

            foreach (Quest q in DiQuests.Values)
            {
                data.PlotQuestData.Add(q.SaveData());
            }

            foreach (Quest q in PlayerManager.QuestLog)
            {
                data.QuestLogData.Add(q.SaveData());
            }

            foreach(Mission m in MissionManager.AvailableMissions)
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
                DiUpgrades[u.upgradeID].Enabled = u.enabled;
            }
            foreach (QuestData q in data.PlotQuestData)
            {
                Quest plotQuest = DiQuests[q.questID];
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

            DataManager.GetUpgradeText(_id, ref _name, ref _description);

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
