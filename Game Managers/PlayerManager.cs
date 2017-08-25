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
        private static string _currentMap;
        public static string CurrentMap { get => _currentMap; set => _currentMap = value; }

        private static Player _player;
        public static Player Player { get => _player; }

        private static List<Building> _buildings;
        public static List<Building> Buildings { get => _buildings; }

        public static void NewPlayer()
        {
            _buildings = new List<Building>();
            _player = new Player();
            _player.AddItemToFirstAvailableInventory(ObjectManager.ItemIDs.Sword);
            _player.AddItemToFirstAvailableInventory(ObjectManager.ItemIDs.PickAxe);
        }

        public static void Update(GameTime gameTime)
        {
            _player.Update(gameTime);
        }

        public static void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _player.Draw(gameTime, spriteBatch);
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
        }

        public struct BuildingData
        {
             [XmlArray(ElementName = "Workers")]
             public List<WorkerData> Workers;

            [XmlElement(ElementName = "positionX")]
            public int positionX;

            [XmlElement(ElementName = "positionY")]
            public int positionY;

            [XmlElement(ElementName = "BuildingID")]
            public ObjectManager.BuildingID buildingID;

            [XmlElement(ElementName = "ID")]
            public int id;
        }
        public struct WorkerData
        {
            [XmlElement(ElementName = "WorkerID")]
            public ObjectManager.WorkerID workerID;
        }
        public struct ItemData
        {
            [XmlElement(ElementName = "ItemID")]
            public ObjectManager.ItemIDs itemID;

            [XmlElement(ElementName = "Numbers")]
            public int num;
        }
        public struct MapData
        {
            [XmlElement(ElementName = "MapName")]
            public string mapName;

            [XmlArray(ElementName = "WorldObjects")]
            public List<WorldObjectData> worldObjects;
        }
        public struct WorldObjectData
        {
            [XmlElement(ElementName = "WorldObjectID")]
            public ObjectManager.ObjectIDs worldObjectID;

            [XmlElement(ElementName = "X")]
            public int x;

            [XmlElement(ElementName = "Y")]
            public int y;
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
                    buildingData.Workers.Add(workerData);
                }
                // Add the level data.
                data.Buildings.Add(buildingData);
            }

            data.Items = new List<ItemData>();
            foreach (InventoryItem i  in Player.Inventory)
            {
                ItemData itemData = new ItemData();
                if (i != null)
                {
                    itemData.itemID = i.ItemID;
                    itemData.num = i.Number;
                }
                data.Items.Add(itemData);
            }

            data.MapData = new List<MapData>();
            foreach (TileMap tileMap in MapManager.Maps.Values)
            {
                MapData m = new MapData();
                m.mapName = tileMap.Name;
                m.worldObjects = new List<WorldObjectData>();

                foreach (WorldObject w in tileMap.WorldObjects)
                {
                    WorldObjectData d = new WorldObjectData();
                    d.worldObjectID = w.ID;
                    d.x = (int)w.Position.X;
                    d.y = (int)w.Position.Y;
                    m.worldObjects.Add(d);
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

            CurrentMap = data.currentMap;
            foreach(BuildingData b in data.Buildings)
            {
                Building newBuilding = ObjectManager.GetBuilding(b.buildingID);
                newBuilding.AddBuildingDetails(b);
                AddBuilding(newBuilding);
                MapManager.CurrentMap.AddBuilding(newBuilding);
            }
            for (int i = 0; i < Player.maxItemRows; i++)
            {
                for (int j = 0; j < Player.maxItemColumns; j++)
                {
                    ItemData item = data.Items[i* Player.maxItemRows +j];
                    InventoryItem newItem = ObjectManager.GetItem(item.itemID, item.num);
                    _player.AddItemToInventorySpot(newItem, i, j);
                }
            }

            foreach (MapData b in data.MapData)
            {
                TileMap tm = MapManager.Maps[b.mapName];
                foreach(WorldObjectData w in b.worldObjects)
                { 
                    tm.AddWorldObject(ObjectManager.GetWorldObject(w.worldObjectID, new Vector2(w.x,w.y)));
                }
            }
        }
        #endregion
    }
}
