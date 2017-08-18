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
        static GameCalendar _gameCalendar;

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
            _gameCalendar = GameCalendar.GetInstance();
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

        public void Save()
        {
            
        }

        public void Load()
        {
            //if (File.Exists("SaveGame.xml"))
            //{
            //    XmlSerializer deserializer = new XmlSerializer(typeof(PlayerManager));
            //    TextReader textReader = new StreamReader("SaveGame.xml");
            //    instance = (PlayerManager)deserializer.Deserialize(textReader);
            //    textReader.Close();
            //}
        }
    }
}
