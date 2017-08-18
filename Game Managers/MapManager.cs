using Adventure.Characters.NPCs;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Game_Managers
{
    public class MapManager
    {
        static MapManager instance;

        private Dictionary<string, TileMap> _tileMaps;
        private TileMap _currentMap;
        public TileMap CurrentMap { get => _currentMap; }

        private MapManager()
        {
            _tileMaps = new Dictionary<string, TileMap>();
        }

        public static MapManager GetInstance()
        {
            if (instance == null)
            {
                instance = new MapManager();
            }
            return instance;
        }

        public void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            AddMap(@"Maps\Map1", Content, GraphicsDevice);
            AddMap(@"Maps\Map2", Content, GraphicsDevice);
            AddMap(@"Maps\ArcaneTower", Content, GraphicsDevice);

            _currentMap = _tileMaps[@"Map1"];
        }

        public void AddMap(string mapToAdd, ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            TileMap newMap = new TileMap();
            newMap.LoadContent(Content, GraphicsDevice, mapToAdd);
            _tileMaps.Add(newMap.Name, newMap);
        }

        public void ChangeMaps(string newMapStr)
        {
            Rectangle rectEntrance = Rectangle.Empty;
            TileMap newMap = _tileMaps[newMapStr];
            //if (newMap.EntranceDictionary.Keys
            foreach (string s in _tileMaps[newMapStr].EntranceDictionary.Keys)
            {
                if (s.Equals(_currentMap.Name))
                {
                    rectEntrance = _tileMaps[newMapStr].EntranceDictionary[s];
                }
            }
            _currentMap = _tileMaps[newMapStr];

            PlayerManager.GetInstance().CurrentMap = _currentMap.Name;
            PlayerManager.GetInstance().Player.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
        }
        public void EnterBuilding(string newMapStr, string ID, List<Worker> workers)
        {
            Rectangle rectEntrance = Rectangle.Empty;
            TileMap newMap = _tileMaps[newMapStr];
            newMap.Name = ID;
            //if (newMap.EntranceDictionary.Keys
            foreach (string s in _tileMaps[newMapStr].EntranceDictionary.Keys)
            {
                if (s.Equals(_currentMap.Name))
                {
                    rectEntrance = _tileMaps[newMapStr].EntranceDictionary[s];
                }
            }
            _currentMap = _tileMaps[newMapStr];
            _currentMap.ClearWorkers();
            _currentMap.AddWorkersToMap(workers);

            PlayerManager.GetInstance().CurrentMap = _currentMap.Name;
            PlayerManager.GetInstance().Player.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
        }

        public void BackToPlayer()
        {
            _currentMap = _tileMaps[PlayerManager.GetInstance().CurrentMap];
        }

        public void ViewMap(string newMap)
        {
            _currentMap = _tileMaps[newMap];
        }

        public void Update(GameTime gametime)
        {
            _currentMap.Update(gametime);
        }

        public void Draw(SpriteBatch spritebatch)
        {
            _currentMap.Draw(spritebatch);
        }

        public bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            rv = _currentMap.ProcessRightButtonClick(mouseLocation);

            return rv;
        }

        public bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            rv = _currentMap.ProcessLeftButtonClick(mouseLocation);

            return rv;
        }
        public bool ProcessHover(Point mouseLocation)
        {
            bool rv = false;

            rv = _currentMap.ProcessHover(mouseLocation);

            return rv;
        }
    }
}
