using Adventure.Characters.NPCs;
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
    public class PlayerManager
    {
        static PlayerManager instance;

        private string _currentMap;
        public string CurrentMap { get => _currentMap; set => _currentMap = value; }

        private Player _player;
        public Player Player { get => _player; }

        private List<Building> _buildings;
        public List<Building> Buildings { get => _buildings; }

        private PlayerManager()
        {
            _buildings = new List<Building>();
        }

        public static PlayerManager GetInstance()
        {
            if (instance == null)
            {
                instance = new PlayerManager();
            }
            return instance;
        }

        public void NewPlayer()
        {
            _player = new Player();
        }

        public void Update(GameTime gameTime)
        {
            _player.Update(gameTime);
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _player.Draw(gameTime, spriteBatch);
        }

        public void AddBuilding(Building b)
        {
            _buildings.Add(b);
        }

        public int GetNewBuildingID()
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
        }

        public struct BuildingData
        {
            /// <summary>
            /// The Level data object.
            /// </summary>
             [XmlArray(ElementName = "Workers")]
             public List<WorkerData> Workers;

            [XmlElement(ElementName = "positionX")]
            public int positionX;

            [XmlElement(ElementName = "positionY")]
            public int positionY;

            [XmlElement(ElementName = "BuildingID")]
            public ItemManager.BuildingID buildingID;

            [XmlElement(ElementName = "ID")]
            public int id;
        }
        public struct WorkerData
        {
            /// <summary>
            /// The Level data object.
            /// </summary>
            [XmlElement(ElementName = "WorkerID")]
            public ItemManager.WorkerID workerID;
        }

        public string Save()
        {
            SaveData data = new SaveData();
            // Create a list to store the data already saved.
            data.currentMap = CurrentMap;
            data.Buildings = new List<BuildingData>();

            // Initialize the new data values.
            foreach (Building b in PlayerManager.GetInstance().Buildings)
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

        public void Load()
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
                Building newBuilding = ItemManager.GetBuilding(b.buildingID);
                newBuilding.AddBuildingDetails(b);
                AddBuilding(newBuilding);
                MapManager.GetInstance().CurrentMap.AddBuilding(newBuilding);
            }
        }
        #endregion
    }
}
