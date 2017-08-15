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
            string[] str = Directory.GetFiles(Content.RootDirectory + @"\Maps", "*", SearchOption.TopDirectoryOnly);
            foreach (string s in str)
            {
                string modPath = s.Replace(@"Content\", "").Replace(".xnb", "");
                if (modPath.Replace(@"Maps\","").Contains("Map"))
                {
                    TileMap newMap = new TileMap();
                    newMap.LoadContent(Content, GraphicsDevice, modPath);
                    _tileMaps.Add(newMap._name, newMap);
                }
            }

            _currentMap = _tileMaps[@"Maps\Map1"];
        }

        public void SetCurrentMap(string newMap)
        {
            newMap = newMap.Insert(0, @"Maps\");
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

        public bool ProcessMapClick(Point mouseLocation)
        {
            bool rv = false;

            rv = _currentMap.ProcessMapClick(mouseLocation);

            return rv;
        }
    }
}
