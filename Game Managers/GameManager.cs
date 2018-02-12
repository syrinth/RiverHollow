using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using RiverHollow.Characters;
using RiverHollow.Characters.NPCs;
using RiverHollow.Items;
using RiverHollow.Tile_Engine;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace RiverHollow.Game_Managers
{
    public static class GameManager
    {
        public static float Scale = 2f;
        public static Dictionary<string, Upgrade> DiUpgrades;

        public static void LoadContent(ContentManager Content)
        {
            DiUpgrades = new Dictionary<string, Upgrade>();
            foreach (KeyValuePair<string, string> kvp in GameContentManager.DiUpgrades)
            {
                DiUpgrades.Add(kvp.Key, new Upgrade(kvp.Key, kvp.Value));
            }
        }

        #region States
        private static bool _scrying;
        private enum State { Paused, Running, Information, Input }
        private static State _gameState;

        private enum Map { None, WorldMap, Combat }
        private static Map _mapState;

        public static void ReadInput() { _gameState = State.Input; }
        public static bool TakingInput() { return _gameState == State.Input; }

        public static void Pause() { _gameState = State.Paused; }
        public static bool IsPaused() { return _gameState == State.Paused; }

        public static void Unpause() { _gameState = State.Running; }
        public static bool IsRunning() { return _gameState == State.Running; }

        public static void Scry(bool val) {
            _gameState = State.Running;
            _scrying = val;
        }
        public static bool Scrying() { return _scrying; }

        public static bool InCombat() { return _mapState == Map.Combat; }
        public static void GoToCombat() {
            _mapState = Map.Combat;
            GUIManager.SetScreen(GUIManager.Screens.Combat);
        }
        public static bool OnMap() { return _mapState == Map.WorldMap; }
        public static void GoToWorldMap() {
            _mapState = Map.WorldMap;
            GUIManager.SetScreen(GUIManager.Screens.HUD);
        }
        public static bool Informational() { return _mapState == Map.None; }
        public static void GoToInformation() {
            _mapState = Map.None;
            _gameState = State.Paused;
        }

        public static void BackToMain()
        {
            GUIManager.SetScreen(GUIManager.Screens.HUD);
            _gameState = State.Running;
            _mapState = Map.WorldMap;
        }
        #endregion

        #region Save/Load
        public struct SaveData
        {
            /// <summary>
            /// The Level data object.
            /// </summary>
            /// 
            [XmlElement(ElementName = "CurrentMap")]
            public string currentMap;

            [XmlElement(ElementName = "Money")]
            public int money;

            [XmlElement(ElementName = "Calendar")]
            public CalendarData Calendar;

            [XmlArray(ElementName = "Items")]
            public List<ItemData> Items;

            [XmlArray(ElementName = "Buildings")]
            public List<BuildingData> Buildings;

            [XmlArray(ElementName = "Maps")]
            public List<MapData> MapData;

            [XmlArray(ElementName = "Upgrades")]
            public List<UpgradeData> UpgradeData;

            [XmlArray(ElementName = "NPCData")]
            public List<NPCData> NPCData;
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
        }
        public struct BuildingData
        {
            [XmlArray(ElementName = "Workers")]
            public List<WorkerData> Workers;

            [XmlArray(ElementName = "StaticItems")]
            public List<StaticItemData> staticItems;

            [XmlElement(ElementName = "positionX")]
            public int positionX;

            [XmlElement(ElementName = "positionY")]
            public int positionY;

            [XmlElement(ElementName = "BuildingID")]
            public int buildingID;

            [XmlElement(ElementName = "PersonalID")]
            public int id;

            [XmlElement(ElementName = "BuildingChest")]
            public StaticItemData buildingChest;

            [XmlElement(ElementName = "Pantry")]
            public StaticItemData pantry;
        }
        public struct WorkerData
        {
            [XmlElement(ElementName = "WorkerID")]
            public int workerID;

            [XmlElement(ElementName = "Name")]
            public string name;

            [XmlElement(ElementName = "Mood")]
            public int mood;
        }
        public struct ItemData
        {
            [XmlElement(ElementName = "ItemID")]
            public int itemID;

            [XmlElement(ElementName = "Numbers")]
            public int num;
        }
        public struct MapData
        {
            [XmlElement(ElementName = "MapName")]
            public string mapName;

            [XmlArray(ElementName = "WorldObjects")]
            public List<WorldObjectData> worldObjects;

            [XmlArray(ElementName = "StaticItems")]
            public List<StaticItemData> staticItems;
        }
        public struct UpgradeData
        {
            [XmlElement(ElementName = "UpgradeID")]
            public string upgradeID;

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

            [XmlElement(ElementName = "Collection")]
            public List<CollectionData> collection;
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
        public struct StaticItemData
        {
            [XmlElement(ElementName = "StaticItemID")]
            public int staticItemID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;

            [XmlArray(ElementName = "Items")]
            public List<ItemData> Items;
        }

        public static string Save()
        {
            SaveData data = new SaveData()
            {
                // Create a list to store the data already saved.
                currentMap = PlayerManager.CurrentMap,
                money = PlayerManager.Money,
                Calendar = new CalendarData
                {
                    dayOfWeek = GameCalendar.DayOfWeek,
                    dayOfMonth = GameCalendar.CurrentDay,
                    currSeason = GameCalendar.CurrentSeason,
                    currWeather = GameCalendar.CurrentWeather
                },
                Items = new List<ItemData>(),
                Buildings = new List<BuildingData>(),
                MapData = new List<MapData>(),
                UpgradeData = new List<UpgradeData>(),
                NPCData = new List<NPCData>()
            };

            // Initialize the new data values.
            foreach (Item i in InventoryManager.PlayerInventory)
            {
                ItemData itemData = new ItemData();
                if (i != null)
                {
                    itemData.itemID = i.ItemID;
                    itemData.num = i.Number;
                }
                else
                {
                    itemData.itemID = -1;
                }
                data.Items.Add(itemData);
            }

            foreach (WorkerBuilding b in PlayerManager.Buildings)
            {
                BuildingData buildingData = new BuildingData();
                buildingData.buildingID = b.ID;
                buildingData.positionX = (int)b.Position.X;
                buildingData.positionY = (int)b.Position.Y;
                buildingData.id = b.PersonalID;

                buildingData.Workers = new List<WorkerData>();

                foreach (WorldAdventurer w in b.Workers)
                {
                    WorkerData workerData = new WorkerData();
                    workerData.workerID = w.ID;
                    workerData.mood = w.Mood;
                    workerData.name = w.Name;
                    buildingData.Workers.Add(workerData);
                }

                buildingData.pantry = SaveStaticItemData(b.Pantry);
                buildingData.buildingChest = SaveStaticItemData(b.BuildingChest);

                buildingData.staticItems = new List<StaticItemData>();
                foreach (StaticItem item in b.StaticItems)
                {
                    buildingData.staticItems.Add(SaveStaticItemData(item));
                }

                data.Buildings.Add(buildingData);
            }

            foreach (RHMap tileMap in MapManager.Maps.Values)
            {
                MapData m = new MapData();
                m.mapName = tileMap.Name;
                m.worldObjects = new List<WorldObjectData>();
                m.staticItems = new List<StaticItemData>();

                if (!tileMap.IsBuilding)
                {
                    foreach (WorldObject w in tileMap.WorldObjects)
                    {
                        WorldObjectData d = new WorldObjectData();
                        d.worldObjectID = w.ID;
                        d.x = (int)w.Position.X;
                        d.y = (int)w.Position.Y;
                        m.worldObjects.Add(d);
                    }
                    foreach (StaticItem item in tileMap.StaticItems)
                    {
                        m.staticItems.Add(SaveStaticItemData(item));
                    }
                }

                data.MapData.Add(m);
            }

            foreach (Upgrade u in GameManager.DiUpgrades.Values)
            {
                UpgradeData upgData = new UpgradeData();
                upgData.upgradeID = u.ID;
                upgData.enabled = u.Enabled;
                data.UpgradeData.Add(upgData);
            }

            foreach (NPC n in CharacterManager.DiNPC.Values)
            {
                NPCData npcData = new NPCData()
                {
                    npcID = n.ID,
                    introduced = n.Introduced,
                    friendship = n.Friendship,
                    collection = new List<CollectionData> ()
                };
                foreach (KeyValuePair<int, bool> kvp in n.Collection){
                    CollectionData c = new CollectionData
                    {
                        itemID = kvp.Key,
                        given = kvp.Value
                    };
                    npcData.collection.Add(c);
                }
                data.NPCData.Add(npcData);
            }

            // Convert the object to XML data and put it in the stream.
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            var sb = new StringBuilder();
            using (var sr = new StringWriter(sb))
            {
                // Seriaize the data.
                serializer.Serialize(sr, data);
            }

            File.WriteAllText("SaveGame.xml", sb.ToString());
            return sb.ToString();
        }
        public static StaticItemData SaveStaticItemData(StaticItem item)
        {
            StaticItemData d = new StaticItemData();
            d.staticItemID = item.ItemID;
            d.x = (int)item.Position.X;
            d.y = (int)item.Position.Y;

            if (item.GetType().Equals(typeof(Container)))
            {
                d.Items = new List<ItemData>();
                foreach (Item i in ((Container)item).Inventory)
                {
                    ItemData itemData = new ItemData();
                    if (i != null)
                    {
                        itemData.itemID = i.ItemID;
                        itemData.num = i.Number;
                    }
                    else
                    {
                        itemData.itemID = -1;
                    }
                    d.Items.Add(itemData);
                }
            }
            return d;
        }

        public static void Load()
        {
            string xml = "SaveGame.xml";
            string _byteOrderMarkUtf16 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (xml.StartsWith(_byteOrderMarkUtf16))
            {
                xml = xml.Remove(0, _byteOrderMarkUtf16.Length);
            }
            XmlSerializer serializer = new XmlSerializer(typeof(SaveData));
            SaveData data;
            using (var sr = new StreamReader(xml))
            {
                data = (SaveData)serializer.Deserialize(sr);
            }
            MapManager.CurrentMap = MapManager.Maps[data.currentMap];
            PlayerManager.InitPlayer();
            PlayerManager.SetMoney(data.money);
            GameCalendar.LoadCalendar(data.Calendar); 
            foreach (BuildingData b in data.Buildings)
            {
                WorkerBuilding newBuilding = ObjectManager.GetBuilding(b.buildingID);
                newBuilding.AddBuildingDetails(b);
                MapManager.Maps["NearWilds"].AddBuilding(newBuilding);

                newBuilding.Pantry = LoadStaticItemData(b.pantry);
                newBuilding.BuildingChest = LoadStaticItemData(b.buildingChest);

                foreach (StaticItemData s in b.staticItems)
                {
                    newBuilding.StaticItems.Add(LoadStaticItemData(s));
                }
            }
            for (int i = 0; i < InventoryManager.maxItemRows; i++)
            {
                for (int j = 0; j < InventoryManager.maxItemColumns; j++)
                {
                    int index = i * InventoryManager.maxItemColumns + j;
                    ItemData item = data.Items[index];
                    Item newItem = ObjectManager.GetItem(item.itemID, item.num);
                    InventoryManager.AddItemToInventorySpot(newItem, i, j);
                }
            }
            foreach (MapData m in data.MapData)
            {
                RHMap tm = MapManager.Maps[m.mapName];
                foreach (WorldObjectData w in m.worldObjects)
                {
                    tm.AddWorldObject(ObjectManager.GetWorldObject(w.worldObjectID, new Vector2(w.x, w.y)), true);
                }
                foreach (StaticItemData s in m.staticItems)
                {
                    tm.PlaceStaticItem(LoadStaticItemData(s), new Vector2(s.x, s.y));
                }
            }
            foreach (UpgradeData u in data.UpgradeData)
            {
                GameManager.DiUpgrades[u.upgradeID].Enabled = u.enabled;
            }
            foreach (NPCData n in data.NPCData)
            {
                NPC target = CharacterManager.DiNPC[n.npcID];
                target.Introduced = n.introduced;
                target.Friendship = n.friendship;
                int index = 1;
                foreach(CollectionData c in n.collection)
                {
                    if (c.given)
                    {
                        target.Collection[c.itemID] = c.given;
                        MapManager.Maps["HouseNPC" + n.npcID].AddCollectionItem(c.itemID, index++);
                    }
                }
            }
        }

        public static Container LoadStaticItemData(StaticItemData data)
        {
            Container c = (Container)ObjectManager.GetItem(data.staticItemID);

            for (int i = 0; i < InventoryManager.maxItemRows; i++)
            {
                for (int j = 0; j < InventoryManager.maxItemColumns; j++)
                {
                    ItemData item = data.Items[i * InventoryManager.maxItemRows + j];
                    Item newItem = ObjectManager.GetItem(item.itemID, item.num);
                    InventoryManager.AddItemToInventorySpot(newItem, i, j, c);
                    c.Position = new Vector2(data.x, data.y);
                }
            }
            return c;
        }
        #endregion
    }

    public class Upgrade
    {
        string _id;
        public string ID { get => _id; }
        string _name;
        public string Name { get => _name; }
        string _description;
        public string Description { get => _description; }
        int _moneyCost;
        public int MoneyCost { get => _moneyCost; }
        List<KeyValuePair<int, int>> _liRequiredItems;
        public List<KeyValuePair<int, int>> LiRquiredItems { get => _liRequiredItems; }
        public bool Enabled;

        public Upgrade(string id, string strData)
        {
            _id = id;
            string[] split = strData.Split('/');
            _name = split[0];
            _description = split[1];
            _moneyCost = int.Parse(split[2]);
            string[] itemSplit = split[3].Split(' ');

            _liRequiredItems = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < itemSplit.Length; i++)
            {
                string[] entrySplit = itemSplit[i].Split(':');
                _liRequiredItems.Add(new KeyValuePair<int, int>(int.Parse(entrySplit[0]), int.Parse(entrySplit[1])));
            }
        }
    }
}

//internal static class GameStateManager {    }
