using Adventure.Characters.NPCs;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Adventure.Game_Managers
{
    public static class PlayerManager
    {
        public static string _inBuilding = string.Empty;
        private static List<int> _canMake;
        public static List<int> CanMake { get => _canMake; }
        private static string _currentMap;
        public static string CurrentMap { get => _currentMap; set => _currentMap = value; }

        private static Player _player;
        public static Player Player { get => _player; }

        private static List<Building> _buildings;
        public static List<Building> Buildings { get => _buildings; }

        public static MerchantChest _merchantChest;

        public static void InitPlayer()
        {
            _player = new Player();
            _buildings = new List<Building>();
            _player = new Player();
            _canMake = new List<int>();
        }

        public static void NewPlayer()
        {          
            _buildings = new List<Building>();
            _player = new Player();
            _canMake = new List<int>();
            _canMake.Add(6);
            _player.AddItemToFirstAvailableInventorySpot(5);
            _player.AddItemToFirstAvailableInventorySpot(3);
            _player.AddItemToFirstAvailableInventorySpot(4);
            _player.AddItemToFirstAvailableInventorySpot(6);
            _player.AddItemToFirstAvailableInventorySpot(7);
        }

        public static void Update(GameTime gameTime)
        {
            _player.Update(gameTime);
        }

        public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _player.Draw(gameTime, spriteBatch);
            _merchantChest.Draw(spriteBatch);
        }

        public static bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            rv = _player.ProcessLeftButtonClick(mouseLocation);

            return rv;
        }

        public static void AddBuilding(Building b)
        {
            _buildings.Add(b);
        }

        public static int GetNewBuildingID()
        {
            return _buildings.Count +1;
        }

        #region Save/Load
        public struct SaveData
        {
            /// <summary>
            /// The Level data object.
            /// </summary>
            /// 
            [XmlElement(ElementName = "CurrentMap")]
            public string currentMap;

            [XmlArray(ElementName = "Buildings")]
            public List<BuildingData> Buildings;

            [XmlArray(ElementName = "Items")]
            public List<ItemData> Items;

            [XmlArray(ElementName = "Maps")]
            public List<MapData> MapData;

            //[XmlArray(ElementName = "NPCData")]
            //public List<NPCData> NPCData;
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
            public ObjectManager.BuildingID buildingID;

            [XmlElement(ElementName = "ID")]
            public int id;

            [XmlElement(ElementName = "BuildingChest")]
            public StaticItemData buildingChest;

            [XmlElement(ElementName = "Pantry")]
            public StaticItemData pantry;
        }
        public struct WorkerData
        {
            [XmlElement(ElementName = "WorkerID")]
            public ObjectManager.WorkerID workerID;

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
        //public struct NPCData
        //{
        //    [XmlElement(ElementName = "Name")]
        //    public string name;

        //    [XmlElement(ElementName = "Introduced")]
        //    public bool introduced;

        //}
        public struct WorldObjectData
        {
            [XmlElement(ElementName = "WorldObjectID")]
            public ObjectManager.ObjectIDs worldObjectID;

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
            SaveData data = new SaveData();
            // Create a list to store the data already saved.
            data.currentMap = CurrentMap;
            data.Buildings = new List<BuildingData>();

            // Initialize the new data values.
            foreach (Building b in Buildings)
            {
                BuildingData buildingData = new BuildingData();
                buildingData.buildingID = b.BuildingID;
                buildingData.positionX = (int)b.Position.X;
                buildingData.positionY = (int)b.Position.Y;
                buildingData.id = b.ID;

                buildingData.Workers = new List<WorkerData>();

                foreach (Worker w in b.Workers)
                {
                    WorkerData workerData = new WorkerData();
                    workerData.workerID = w.WorkerID;
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

            data.Items = new List<ItemData>();
            foreach (Item i  in Player.Inventory)
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

            data.MapData = new List<MapData>();
            foreach (TileMap tileMap in MapManager.Maps.Values)
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

            InitPlayer();
            CurrentMap = data.currentMap;
            foreach (BuildingData b in data.Buildings)
            {
                Building newBuilding = ObjectManager.GetBuilding(b.buildingID);
                newBuilding.AddBuildingDetails(b);
                MapManager.CurrentMap.AddBuilding(newBuilding);

                newBuilding.Pantry = LoadStaticItemData(b.pantry);
                newBuilding.BuildingChest = LoadStaticItemData(b.buildingChest);

                foreach (StaticItemData s in b.staticItems)
                {
                    newBuilding.StaticItems.Add(LoadStaticItemData(s));
                }
            }
            for (int i = 0; i < Player.maxItemRows; i++)
            {
                for (int j = 0; j < Player.maxItemColumns; j++)
                {
                    int index = i * Player.maxItemColumns + j;
                    ItemData item = data.Items[index];
                    Item newItem = ObjectManager.GetItem(item.itemID, item.num);
                    _player.AddItemToInventorySpot(newItem, i, j);
                }
            }

            foreach (MapData m in data.MapData)
            {
                TileMap tm = MapManager.Maps[m.mapName];
                foreach(WorldObjectData w in m.worldObjects)
                { 
                    tm.AddWorldObject(ObjectManager.GetWorldObject(w.worldObjectID, new Vector2(w.x,w.y)));
                }
                foreach (StaticItemData s in m.staticItems)
                {
                    tm.PlaceStaticItem(LoadStaticItemData(s), new Vector2(s.x, s.y));
                }
            }
        }

        public static Container LoadStaticItemData(StaticItemData data)
        {
            Container c = (Container)ObjectManager.GetItem(data.staticItemID);

            for (int i = 0; i < Player.maxItemRows; i++)
            {
                for (int j = 0; j < Player.maxItemColumns; j++)
                {
                    ItemData item = data.Items[i * Player.maxItemRows + j];
                    Item newItem = ObjectManager.GetItem(item.itemID, item.num);
                    c.AddItemToInventorySpot(newItem, i, j);
                    c.Position = new Vector2(data.x, data.y);
                }
            }
            return c;
        }
        #endregion
    }
}
