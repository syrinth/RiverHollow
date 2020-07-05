﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Actors;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.GUIObjects;
using RiverHollow.Misc;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.WorldObjects;
using System.Collections.Generic;
using System.IO;
using static RiverHollow.Actors.Actor;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Game_Managers
{
    public static class MapManager
    {
        struct NewMapInfo
        {
            public RHMap NextMap;
            public Vector2 PlayerPosition;
            public Building EnteredBuilding;
            public NewMapInfo(RHMap map, Vector2 pos, Building b)
            {
                NextMap = map;
                PlayerPosition = pos;
                EnteredBuilding = b;
            }
        }//[Friends:1-30]
        public const string HomeMap = "mapManorGrounds";
        public const string SpawnMap = "mapForest_08"; //"mapManorGrounds"; //"mapForest_01"; //"mapRiverHollowTown"; //"mapSpringDungeonC"; // "mapRiverHollowTown"; //;
        const string _sMapFolder = @"Content\Maps";
        const string _sDungeonMapFolder = @"Content\Maps\Dungeons";

        static Dictionary<string, RHMap> _tileMaps;
        public static Dictionary<string, RHMap> Maps { get => _tileMaps; }

        static NewMapInfo _newMapInfo;
        static RHMap _currentMap;
        public static RHMap CurrentMap { get => _currentMap; set => _currentMap = value; }

        static List<Weather> _liWeather;

        public static void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            _tileMaps = new Dictionary<string, RHMap>();
            _liWeather = new List<Weather>();
            InitWeather();

            foreach(string dir in Directory.GetDirectories(_sMapFolder))
            {
                if (!dir.Contains("Textures"))
                {
                    foreach (string s in Directory.GetFiles(dir)) { AddMap(s, Content, GraphicsDevice); }
                }
            }

            _currentMap = _tileMaps[MapManager.SpawnMap];
        }
        public static void LoadObjects()
        {
            foreach(RHMap m in Maps.Values)
            {
                m.LoadMapObjects();
            }
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

        public static void ChangeMaps(WorldActor c, string currMap, TravelPoint travelPoint)
        {
            //Get the entry rectangle on the new map
            TravelPoint entryPoint = null;

            //Handling for if the WorldActor is the player character
            if (c == PlayerManager.World)
            {
                //Handling for if the player is currently in a building and is leaving it
                if (PlayerManager._iBuildingID != -1)
                {
                    entryPoint = _tileMaps[travelPoint.LinkedMap].DictionaryTravelPoints[PlayerManager._iBuildingID.ToString()];
                    PlayerManager._iBuildingID = -1;
                }
                else
                {
                    entryPoint = _tileMaps[travelPoint.LinkedMap].DictionaryTravelPoints[currMap];
                }

                FadeToNewMap(_tileMaps[travelPoint.LinkedMap], entryPoint.FindLinkedPointPosition(travelPoint.Center, c));
                SoundManager.PlayEffect("126426__cabeeno-rossley__timer-ends-time-up");
            }
            else
            {
                entryPoint = _tileMaps[travelPoint.LinkedMap].DictionaryTravelPoints[currMap];

                if (c.IsActorType(ActorEnum.NPC) || c.IsActorType(ActorEnum.WorldCharacter))
                {
                    ((Villager)c).ClearTileForMapChange();
                }
                _tileMaps[currMap].RemoveCharacter(c);
                _tileMaps[travelPoint.LinkedMap].AddCharacter(c);
                c.NewMapPosition = entryPoint.FindLinkedPointPosition(travelPoint.Center, c); //This needs to get updated when officially added to the new map
            }
        }

        /// <summary>
        /// Begins a fadeot so we can move to the next map and sets the info the map manager needs
        /// so that we know whichmap to move to once the fade is done.
        /// </summary>
        /// <param name="newMap">Map to move to</param>
        /// <param name="playerPos">The position of the player</param>
        public static void FadeToNewMap(RHMap newMap, Vector2 playerPos, Building b = null)
        {
            GUIManager.BeginFadeOut();
            PlayerManager.World.PlayAnimation(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
            _newMapInfo = new NewMapInfo(newMap, playerPos, b);
        }

        public static void EnterBuilding(TravelPoint doorLoc, Building b)
        {
            TravelPoint tPoint = null;
            PlayerManager._iBuildingID = b.PersonalID;

            foreach (string s in _tileMaps[b.MapName].DictionaryTravelPoints.Keys)
            {
                if (s.Equals(PlayerManager.CurrentMap))
                {
                    tPoint = _tileMaps[b.MapName].DictionaryTravelPoints[s];
                }
            }

            FadeToNewMap(_tileMaps[b.MapName], tPoint.FindLinkedPointPosition(doorLoc.Center, PlayerManager.World), b);
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
                map.PopulateMap(loaded);
            }
            int mapWidth = _tileMaps[MapManager.HomeMap].MapWidthTiles;
            int mapHeight = _tileMaps[MapManager.HomeMap].MapHeightTiles;
            RHRandom rand = RHRandom.Instance;
            //LoadMap1
            if (!loaded)
            {
                int rockID = int.Parse(DataManager.Config[1]["ObjectID"]);
                int bigRockID = int.Parse(DataManager.Config[2]["ObjectID"]);
                int treeID = int.Parse(DataManager.Config[3]["ObjectID"]);

                for (int i = 0; i < 99; i++)
                {
                    _tileMaps[MapManager.HomeMap].PlaceWorldObject(DataManager.GetWorldObject(rockID, new Vector2(rand.Next(1, mapWidth - 1) * TileSize, rand.Next(1, mapHeight - 1) * TileSize)), true);
                }
                for (int i = 0; i < 99; i++)
                {
                    _tileMaps[MapManager.HomeMap].PlaceWorldObject(DataManager.GetWorldObject(treeID, new Vector2(rand.Next(1, mapWidth - 1) * TileSize, rand.Next(1, mapHeight - 1) * TileSize)), true);
                }
                for (int i = 0; i < 10; i++)
                {
                    _tileMaps[MapManager.HomeMap].PlaceWorldObject(DataManager.GetWorldObject(bigRockID, new Vector2(rand.Next(1, mapWidth - 1) * TileSize, rand.Next(1, mapHeight - 1) * TileSize)), true);
                }
            }
        }

        public static void Update(GameTime gTime)
        {
            if(!_newMapInfo.Equals(default(NewMapInfo)) && GUIManager.FadingIn)
            {
                string oldMap = _currentMap.Name;
                _currentMap = _newMapInfo.NextMap;
                if (_newMapInfo.EnteredBuilding != null)
                {
                    _currentMap.LoadBuilding(_newMapInfo.EnteredBuilding);
                }

                PlayerManager.CurrentMap = _newMapInfo.NextMap.Name;
                PlayerManager.World.Position = _newMapInfo.PlayerPosition;
                _currentMap.CheckForTriggeredCutScenes();
                _newMapInfo = default;

                //Enter combat upon entering a map with living monsters
                if (_currentMap.Monsters.Count > 0)
                {
                    CombatManager.NewBattle(oldMap);
                }
            }

            foreach(RHMap map in _tileMaps.Values)
            {
                map.Update(gTime);
            }
            if (_currentMap.IsOutside)
            {
                foreach (Weather s in _liWeather)
                {
                    s.Update(gTime);
                }
            }
        }

        public static void DrawBase(SpriteBatch spriteBatch)
        {
            _currentMap.DrawBase(spriteBatch);
            GUICursor.DrawBuilding(spriteBatch);
            GUICursor.DrawPotentialWorldObject(spriteBatch);
        }

        public static void DrawLights(SpriteBatch spriteBatch)
        {
            _currentMap.DrawLights(spriteBatch);
        }
        public static void DrawUpper(SpriteBatch spriteBatch)
        {
            _currentMap.DrawUpper(spriteBatch);
            if (_currentMap.IsOutside)
            {
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
            return _currentMap.GetTileByGridCoords(x, y);
        }
        public static RHTile RetrieveTile(Point mouseLocation)
        {
            return _currentMap.GetTileByPixelPosition(mouseLocation);
        }
        public static void RemoveWorldObject(WorldObject o)
        {
            _currentMap.RemoveWorldObject(o);
        }

        public static void RemoveCharacter(WorldActor c)
        {
            _currentMap.RemoveCharacter(c);
        }
        public static void RemoveMonster(Monster m)
        {
            _currentMap.RemoveMonster(m);
        }
        public static void DropItemsOnMap(List<Item> items, Vector2 position, bool flyingPop = true)
        {
            _currentMap.DropItemsOnMap(items, position, flyingPop);
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

        public static void SetWeather(AnimationEnum weather)
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
                _sprite.AddAnimation(AnimationEnum.Rain, 0, 0, 160, 160, 2, 0.2f);
                _sprite.AddAnimation(AnimationEnum.Snow, 0, 160, 160, 160, 3, 0.2f);
                _sprite.IsAnimating = false;
            }

            public void SetWeather(AnimationEnum weather)
            {
                _sprite.PlayAnimation(weather);
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

            internal void Update(GameTime gTime)
            {
                _sprite.Update(gTime);
            }
        }
    }
}
