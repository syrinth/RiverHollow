﻿using RiverHollow.Actors;
using RiverHollow.GUIObjects;
using RiverHollow.WorldObjects;
using RiverHollow.Tile_Engine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using RiverHollow.Misc;
using System.IO;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.SpriteAnimations;
using RiverHollow.Game_Managers.GUIObjects;
using RiverHollow.Buildings;

namespace RiverHollow.Game_Managers
{
    public static class MapManager
    {
        public const string HomeMap = "mapManorGrounds"; //"mapRiverHollowTown"; //"mapManorGrounds"; //"mapForestDungeonZone"; //"mapRiverHollowTown"; //
        const string _sMapFolder = @"Content\Maps";
        const string _sDungeonMapFolder = @"Content\Maps\Dungeons";

        static Dictionary<string, RHMap> _tileMaps;
        public static Dictionary<string, RHMap> Maps { get => _tileMaps; }

        static RHMap _currentMap;
        public static RHMap CurrentMap { get => _currentMap; set => _currentMap = value; }

        static List<Weather> _liWeather;
        static GUIImage _ambientLight;

        public static void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            _tileMaps = new Dictionary<string, RHMap>();
            _liWeather = new List<Weather>();
            InitWeather();

            foreach (string s in Directory.GetFiles(_sMapFolder)) { AddMap(s, Content, GraphicsDevice); }
            foreach (string s in Directory.GetFiles(_sDungeonMapFolder)) { AddMap(s, Content, GraphicsDevice); }

            _currentMap = _tileMaps[MapManager.HomeMap];
            _ambientLight = new GUIImage(new Rectangle(160, 128, TileSize, TileSize), RiverHollow.ScreenWidth, RiverHollow.ScreenHeight, @"Textures\Dialog");
        }

        public static void AddMap(string mapToAdd, ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            RHMap newMap = new RHMap();

            string name = string.Empty;
            Util.ParseContentFileRetName(ref mapToAdd, ref name);
            if (name.IndexOf("map") == 0)                       //Ensures that we're loading a map
            {
                if (!_tileMaps.ContainsKey(name))
                {
                    newMap.LoadContent(Content, GraphicsDevice, mapToAdd, name);
                    _tileMaps.Add(name, newMap);
                }
            }
        }

        public static void ChangeMaps(WorldActor c, string currMap, string newMapStr)
        {
            Rectangle rectEntrance = Rectangle.Empty;
            string[] splitString = newMapStr.Split(':');
            string newMapName = splitString[0];
            string ID = (splitString.Length == 2) ? splitString[1] : "";
            RHMap newMap = _tileMaps[newMapName];

            if (_tileMaps[currMap].IsDungeon)
            {
                rectEntrance = _tileMaps[newMapName].DictionaryEntrance["Dungeon"];
            }
            else
            {
                foreach (string s in _tileMaps[newMapName].DictionaryEntrance.Keys)
                {
                    string[] testString = s.Split(':');
                    string testName = testString[0];
                    string testID = (testString.Length == 2) ? testString[1] : "";

                    if (c == PlayerManager.World && !string.IsNullOrEmpty(PlayerManager._sBuildingID))
                    {
                        rectEntrance = _tileMaps[newMapName].DictionaryEntrance[PlayerManager._sBuildingID];
                        PlayerManager._sBuildingID = string.Empty;
                    }
                    else
                    {
                        if (testName.Equals(_tileMaps[currMap].Name) && testID == ID)
                        {
                            rectEntrance = _tileMaps[newMapName].DictionaryEntrance[s];
                        }
                    }
                }
            }

            if (c == PlayerManager.World)
            {
                GUIManager.FadeOut();
                SoundManager.PlayEffect("126426__cabeeno-rossley__timer-ends-time-up");
                _currentMap = _tileMaps[newMapName];

                PlayerManager.CurrentMap = _currentMap.Name;
                PlayerManager.World.Position = Util.SnapToGrid(new Vector2(rectEntrance.Left, rectEntrance.Top));
                CutsceneManager.CheckForTriggedCutscene();
            }
            else
            {
                if (c.IsNPC() || c.IsWorldCharacter()){
                    ((Villager)c).ClearTileForMapChange();
                }
                _tileMaps[currMap].RemoveCharacter(c);
                _tileMaps[newMapName].AddCharacter(c);
                c.NewMapPosition = Util.SnapToGrid(new Vector2(rectEntrance.Left, rectEntrance.Top)); //This needs to get updated when officially added to the new map
            }
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

            Rectangle rectEntrance = newMap.IsDungeon ? newMap.DictionaryEntrance[direction] : newMap.DictionaryEntrance["Dungeon"];
            
            _currentMap = newMap;

            PlayerManager.CurrentMap = _currentMap.Name;
            PlayerManager.World.Position = new Vector2(rectEntrance.Left, rectEntrance.Top);
        }

        public static void EnterBuilding(Building b)
        {
            Rectangle rectEntrance = Rectangle.Empty;
            PlayerManager._sBuildingID = b.PersonalID.ToString();

            foreach (string s in _tileMaps[b.MapName].DictionaryEntrance.Keys)
            {
                if (s.Equals(_currentMap.Name))
                {
                    rectEntrance = _tileMaps[b.MapName].DictionaryEntrance[s];
                }
            }
            _currentMap = _tileMaps[b.MapName];
            _currentMap.LoadBuilding((Building)b);

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
            foreach(RHMap map in _tileMaps.Values)
            {
                map.PopulateMap();
            }
            int mapWidth = _tileMaps[MapManager.HomeMap].MapWidthTiles;
            int mapHeight = _tileMaps[MapManager.HomeMap].MapHeightTiles;
            RHRandom r = new RHRandom();
            //LoadMap1
            if (!loaded)
            {
                for (int i = 0; i < 99; i++)
                {
                    _tileMaps[MapManager.HomeMap].PlaceWorldObject(ObjectManager.GetWorldObject(WorldItem.Rock, new Vector2(r.Next(1, mapWidth - 1) * TileSize, r.Next(1, mapHeight - 1) * TileSize)), true);
                }
                for (int i = 0; i < 99; i++)
                {
                    _tileMaps[MapManager.HomeMap].PlaceWorldObject(ObjectManager.GetWorldObject(WorldItem.Tree, new Vector2(r.Next(1, mapWidth - 1) * TileSize, r.Next(1, mapHeight - 1) * TileSize)), true);
                }
                for (int i = 0; i < 10; i++)
                {
                    _tileMaps[MapManager.HomeMap].PlaceWorldObject(ObjectManager.GetWorldObject(WorldItem.BigRock, new Vector2(r.Next(1, mapWidth - 1) * TileSize, r.Next(1, mapHeight - 1) * TileSize)), true);
                }
            }

            MerchantChest m = new MerchantChest();
            PlayerManager._merchantChest = m;
        }

        public static void Update(GameTime gametime)
        {
            foreach(RHMap map in _tileMaps.Values)
            {
                map.Update(gametime);
            }
            if (_currentMap.IsOutside)
            {
                foreach (Weather s in _liWeather)
                {
                    s.Update(gametime);
                }
            }
        }

        public static void DrawBase(SpriteBatch spriteBatch)
        {
            _currentMap.DrawBase(spriteBatch);
            GraphicCursor.DrawBuilding(spriteBatch);
            GraphicCursor.DrawPotentialWorldObject(spriteBatch);
        }

        public static void DrawUpper(SpriteBatch spriteBatch)
        {
            _currentMap.DrawUpper(spriteBatch);
            if (_currentMap.IsOutside)
            {
                _ambientLight.Draw(spriteBatch, GameCalendar.GetAmbientLight());
                if (!GameCalendar.IsSunny())
                {
                    foreach (Weather s in _liWeather)
                    {
                        s.Draw(spriteBatch);
                    }
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
        public static RHTile RetrieveTile(int x, int y)
        {
            return _currentMap.GetTile(x, y);
        }
        public static RHTile RetrieveTile(Point mouseLocation)
        {
            return _currentMap.GetTileOffGrid(mouseLocation);
        }
        public static void RemoveWorldObject(WorldObject o)
        {
            _currentMap.RemoveWorldObject(o);
        }

        public static void RemoveCharacter(WorldActor c)
        {
            _currentMap.RemoveCharacter(c);
        }
        public static void RemoveMob(Mob m)
        {
            _currentMap.RemoveMob(m);
        }
        public static void DropItemsOnMap(List<Item> items, Vector2 position)
        {
            _currentMap.DropItemsOnMap(items, position);
        }
        public static void PlaceWorldObject(WorldObject worldObject)
        {
            _currentMap.PlaceWorldObject(worldObject);
        }
        public static bool PlacePlayerObject(WorldObject worldObject)
        {
            return _currentMap.PlacePlayerObject(worldObject);
        }

        public static void InitWeather()
        {
            int cols = 1 + RiverHollow.ScreenWidth / 160;
            int rows = 1 + RiverHollow.ScreenHeight / 160;

            int x = 0;
            int y = 0;
            for(int i = 0; i< cols; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    Weather w = new Weather(x, y);
                    _liWeather.Add(w);
                    y += 160;
                }
                x += 160;
                y = 0;
            }
        }

        public static void SetWeather(WeatherAnimEnum weather)
        {
            foreach(Weather w in _liWeather)
            {
                w.SetWeather(weather);
            }

            if (GameCalendar.IsRaining())
            {
                foreach (RHMap map in _tileMaps.Values)
                {
                    map.WaterTiles();
                }
            }
        }

        public static void Rollover()
        {
            foreach(RHMap map in _tileMaps.Values)
            {
                map.Rollover();
            }
        }

        public static void CheckSpirits()
        {
            foreach (RHMap map in _tileMaps.Values)
            {
                map.CheckSpirits();
            }
        }

        private class Weather
        {
            AnimatedSprite _sprite;
            public Weather(int x, int y)
            {
                _sprite = new AnimatedSprite(@"Textures\texWeather")
                {
                    Position = new Vector2(x, y)
                };
                _sprite.AddAnimation(WeatherAnimEnum.Rain, 0, 0, 160, 160, 2, 0.2f);
                _sprite.AddAnimation(WeatherAnimEnum.Snow, 0, 160, 160, 160, 3, 0.2f);
                _sprite.IsAnimating = false;
            }

            public void SetWeather(WeatherAnimEnum weather)
            {
                _sprite.SetCurrentAnimation(weather);
                _sprite.IsAnimating = true;
            }

            public void StopWeather()
            {
                _sprite.IsAnimating = false;
            }

            internal void Draw(SpriteBatch spriteBatch)
            {
                _sprite.Draw(spriteBatch, false);
            }

            internal void Update(GameTime gametime)
            {
                _sprite.Update(gametime);
            }
        }
    }
}
