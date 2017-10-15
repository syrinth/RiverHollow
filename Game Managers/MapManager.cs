using Adventure.Characters;
using Adventure.GUIObjects;
using Adventure.Items;
using Adventure.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace Adventure.Game_Managers
{
    public static class MapManager
    {
        private static Dictionary<string, RHMap> _tileMaps;
        public static Dictionary<string, RHMap> Maps { get => _tileMaps; }

        private static RHMap _currentMap;
        public static RHMap CurrentMap { get => _currentMap; }

        public static void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            _tileMaps = new Dictionary<string, RHMap>();
            AddMap(@"Maps\Map1", Content, GraphicsDevice);
            AddMap(@"Maps\Map2", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room1", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room2", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room3", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room4", Content, GraphicsDevice);
            AddMap(@"Maps\Dungeons\Room5", Content, GraphicsDevice);
            AddMap(@"Maps\Arcane Tower", Content, GraphicsDevice);

            _currentMap = _tileMaps[@"Map1"];
        }

        public static void AddMap(string mapToAdd, ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            RHMap newMap = new RHMap();
            newMap.LoadContent(Content, GraphicsDevice, mapToAdd);
            _tileMaps.Add(newMap.Name, newMap);
        }

        public static void ChangeMaps(string newMapStr)
        {
            GUIManager.FadeOut();
            Rectangle rectEntrance = Rectangle.Empty;
            RHMap newMap = _tileMaps[newMapStr];

            if (_currentMap.IsDungeon)
            {
                rectEntrance = _tileMaps[newMapStr].EntranceDictionary["Dungeon"];
            }
            else
            {
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
            }
            _currentMap = _tileMaps[newMapStr];

            PlayerManager.CurrentMap = _currentMap.Name;
            PlayerManager.World.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
        }

        public static void EnterDungeon()
        {
            GUIManager.FadeOut();
            _currentMap = DungeonManager.Maps[0];

            PlayerManager.CurrentMap = _currentMap.Name;
            PlayerManager.World.Position = new Vector2(DungeonManager.Entrance.Left, DungeonManager.Entrance.Top);
        }

        public static void ChangeDungeonRoom(string direction, bool straightOut = false)
        {
            GUIManager.FadeOut();
            RHMap newMap = DungeonManager.RoomChange(direction, straightOut);

            Rectangle rectEntrance = newMap.IsDungeon ? newMap.EntranceDictionary[direction] : newMap.EntranceDictionary["Dungeon"];
            
            _currentMap = newMap;

            PlayerManager.CurrentMap = _currentMap.Name;
            PlayerManager.World.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
        }

        public static void EnterBuilding(Building b)
        {
            Rectangle rectEntrance = Rectangle.Empty;
            PlayerManager._inBuilding = b.ID.ToString();

            foreach (string s in _tileMaps[b._name].EntranceDictionary.Keys)
            {
                if (s.Equals(_currentMap.Name))
                {
                    rectEntrance = _tileMaps[b._name].EntranceDictionary[s];
                }
            }
            _currentMap = _tileMaps[b._name];
            _currentMap.LoadBuilding(b);

            PlayerManager.CurrentMap = _currentMap.Name;
            PlayerManager.World.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
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
                    _tileMaps[@"Map1"].AddWorldObject(ObjectManager.GetWorldObject(0, new Vector2(r.Next(1, mapWidth-1) * RHMap.TileSize, r.Next(1, mapHeight-1) * RHMap.TileSize)), true);
                }
                for (int i = 0; i < 99; i++)
                {
                    _tileMaps[@"Map1"].AddWorldObject(ObjectManager.GetWorldObject(2, new Vector2(r.Next(1, mapWidth-1) * RHMap.TileSize, r.Next(1, mapHeight-1) * RHMap.TileSize)), true);
                }
            }
            _tileMaps[@"Map1"].AddMob(CharacterManager.GetMobByIndex(1, new Vector2(1340, 1340)));
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

            if (RiverHollow.State == RiverHollow.GameState.Build)
            {
                if (GraphicCursor.HeldBuilding != null)
                {
                    Vector2 mousePosition = GraphicCursor.GetTranslatedPosition();
                    Texture2D drawIt = GraphicCursor.HeldBuilding.Texture;
                    Rectangle drawRectangle = new Rectangle(((int)(mousePosition.X / 32)) * 32, ((int)(mousePosition.Y / 32)) * 32, drawIt.Width, drawIt.Height);
                    Rectangle source = new Rectangle(0, 0, drawIt.Width, drawIt.Height);

                    GraphicCursor.HeldBuilding.SetCoordinates(new Vector2(drawRectangle.X, drawRectangle.Y));
                    spriteBatch.Draw(drawIt, drawRectangle, null, Color.White, 0, new Vector2(0, 0), SpriteEffects.None, mousePosition.Y + drawIt.Height);
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
        public static RHTile RetrieveTile(Point mouseLocation)
        {
            return _currentMap.RetrieveTile(mouseLocation);
        }
        public static void RemoveWorldObject(WorldObject o)
        {
            _currentMap.RemoveWorldObject(o);
        }

        public static void RemoveCharacter(WorldCharacter c)
        {
            _currentMap.RemoveCharacter(c);
        }
        public static void RemoveMob(Mob m)
        {
            _currentMap.RemoveMob(m);
        }
        public static void DropWorldItems(List<Item> items, Vector2 position)
        {
            _currentMap.DropWorldItems(items, position);
        }
        public static void PlaceWorldItem(StaticItem staticItem, Vector2 position)
        {
            _currentMap.PlaceStaticItem(staticItem, position, false);
        }
    }
}
