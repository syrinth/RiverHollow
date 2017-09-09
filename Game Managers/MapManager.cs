using Adventure.Characters;
using Adventure.Characters.Monsters;
using Adventure.Characters.NPCs;
using Adventure.GUIObjects;
using Adventure.Items;
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
    public static class MapManager
    {
        private static Dictionary<string, TileMap> _tileMaps;
        public static Dictionary<string, TileMap> Maps { get => _tileMaps; }

        private static TileMap _currentMap;
        public static TileMap CurrentMap { get => _currentMap; }

        public static void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            _tileMaps = new Dictionary<string, TileMap>();
            AddMap(@"Maps\Map1", Content, GraphicsDevice);
            AddMap(@"Maps\Map2", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room1", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room2", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room3", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room4", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room5", Content, GraphicsDevice);
            AddMap(@"Maps\ArcaneTower", Content, GraphicsDevice);

            _currentMap = _tileMaps[@"Map1"];
        }

        public static void AddMap(string mapToAdd, ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            TileMap newMap = new TileMap();
            newMap.LoadContent(Content, GraphicsDevice, mapToAdd);
            _tileMaps.Add(newMap.Name, newMap);
        }

        public static void ChangeMaps(string newMapStr)
        {
            GUIManager.FadeOut();
            Rectangle rectEntrance = Rectangle.Empty;
            TileMap newMap = _tileMaps[newMapStr];

            foreach (string s in _tileMaps[newMapStr].EntranceDictionary.Keys)
            {
                if (!string.IsNullOrEmpty(PlayerManager._inBuilding))
                {
                    rectEntrance = _tileMaps[newMapStr].EntranceDictionary[PlayerManager._inBuilding];
                    PlayerManager._inBuilding = string.Empty;
                }
                else
                {
                    if (s.Equals(_currentMap.Name))
                    {
                        rectEntrance = _tileMaps[newMapStr].EntranceDictionary[s];
                    }
                }
            }
            _currentMap = _tileMaps[newMapStr];

            PlayerManager.CurrentMap = _currentMap.Name;
            PlayerManager.Player.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
        }

        public static void EnterDungeon()
        {
            List<string> directions = new List<string> { "North", "South", "East", "West" };
            Random r = new Random();

            int dir = r.Next(0, 4);
            int dirFrom = -1;

            //Need to reverse the value since we need to open the "east" door, when you come from the "West"
            if (dir == 0) dirFrom = 1;
            else if (dir == 1) dirFrom = 0;
            else if (dir == 2) dirFrom = 3;
            else if (dir == 3) dirFrom = 2;
            int dirTo = -1;
            do
            {
                dirTo = r.Next(0, 4);
            } while (dirTo == dir);

            TileMap newMap = _tileMaps[DungeonManager.Maps[0]];
            newMap.LayerVisible(directions[dirFrom], false);
            newMap.LayerVisible(directions[dirTo], false);

            Rectangle rectEntrance = Rectangle.Empty;
            foreach (string s in newMap.EntranceDictionary.Keys)
            {
                if (s.Equals(directions[dir]))
                {
                    rectEntrance = newMap.EntranceDictionary[s];
                }
            }
            _currentMap = newMap;

            PlayerManager.CurrentMap = _currentMap.Name;
            PlayerManager.Player.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
        }
        public static void ChangeDungeonRoom(string direction)
        {
            Random r = new Random();
            TileMap newMap = _tileMaps[DungeonManager.NextRoom(_currentMap.Name)];
            Rectangle rectEntrance = Rectangle.Empty;
            foreach (string s in newMap.EntranceDictionary.Keys)
            {
                if (s.Equals(direction))
                {
                    rectEntrance = newMap.EntranceDictionary[s];
                }
            }
            _currentMap = newMap;

            PlayerManager.CurrentMap = _currentMap.Name;
            PlayerManager.Player.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
        }

        //string newMapStr, string ID, List<Worker> workers
        public static void EnterBuilding(Building b)
        {
            Rectangle rectEntrance = Rectangle.Empty;
            //TileMap newMap = _tileMaps[b._map];
            PlayerManager._inBuilding = b.ID.ToString();

            foreach (string s in _tileMaps[b._map].EntranceDictionary.Keys)
            {
                if (s.Equals(_currentMap.Name))
                {
                    rectEntrance = _tileMaps[b._map].EntranceDictionary[s];
                }
            }
            _currentMap = _tileMaps[b._map];
            _currentMap.LoadBuilding(b);

            PlayerManager.CurrentMap = _currentMap.Name;
            PlayerManager.Player.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
        }

        public static void BackToPlayer()
        {
            _currentMap = _tileMaps[PlayerManager.CurrentMap];
        }

        public static void ViewMap(string newMap)
        {
            _currentMap = _tileMaps[newMap];
        }

        public static void PopulateMaps(bool loaded)
        {
            int mapWidth = _tileMaps[@"Map1"].MapWidth;
            int mapHeight = _tileMaps[@"Map1"].MapHeight;
            Random r = new Random();
            //LoadMap1
            if (!loaded)
            {
                for (int i = 0; i < 99; i++)
                {
                    _tileMaps[@"Map1"].AddWorldObject(ObjectManager.GetWorldObject(ObjectManager.ObjectIDs.Rock, new Vector2(r.Next(0, mapWidth) * TileMap.TileSize, r.Next(0, mapHeight) * TileMap.TileSize)));
                }
                for (int i = 0; i < 99; i++)
                {
                    _tileMaps[@"Map1"].AddWorldObject(ObjectManager.GetWorldObject(ObjectManager.ObjectIDs.Tree, new Vector2(r.Next(0, mapWidth) * TileMap.TileSize, r.Next(0, mapHeight) * TileMap.TileSize)));
                }
            }
            _tileMaps[@"Map1"].AddCharacter(new Goblin(new Vector2(1340, 1340)));
            MerchantChest m = new MerchantChest();
            PlayerManager._merchantChest = m;
        }

        public static void Update(GameTime gametime)
        {
            _currentMap.Update(gametime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            _currentMap.Draw(spriteBatch);

            if (AdventureGame.State == AdventureGame.GameState.Build)
            {
                if (GraphicCursor.HeldBuilding != null)
                {
                    Texture2D drawIt = GraphicCursor.HeldBuilding.Texture;
                    Rectangle drawRectangle = new Rectangle(((int)((GraphicCursor.Position.X) / 32)) * 32, ((int)((GraphicCursor.Position.Y) / 32)) * 32, drawIt.Width, drawIt.Height);
                    Rectangle source = new Rectangle(0, 0, drawIt.Width, drawIt.Height);

                    GraphicCursor.HeldBuilding.SetCoordinates(new Vector2(drawRectangle.X, drawRectangle.Y));
                    spriteBatch.Draw(drawIt, drawRectangle, source, Color.White);
                }
            }
        }

        public static bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            rv = _currentMap.ProcessLeftButtonClick(mouseLocation);

            return rv;
        }
        public static bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            rv = _currentMap.ProcessRightButtonClick(mouseLocation);

            return rv;
        }
        public static bool ProcessHover(Point mouseLocation)
        {
            bool rv = false;

            rv = _currentMap.ProcessHover(mouseLocation);

            return rv;
        }
        public static WorldObject FindWorldObject(Point mouseLocation)
        {
            return _currentMap.FindWorldObject(mouseLocation);
        }
        public static void RemoveWorldObject(WorldObject o)
        {
            _currentMap.RemoveWorldObject(o);
        }

        public static void RemoveCharacter(Character c)
        {
            _currentMap.RemoveCharacter(c);
        }
        public static void DropWorldItems(List<Item> items, Vector2 position)
        {
            _currentMap.DropWorldItems(items, position);
        }
        public static void PlaceWorldItem(StaticItem staticItem, Vector2 position)
        {
            _currentMap.PlaceStaticItem(staticItem, position);
        }
    }
}
