using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Tile_Engine;
using RiverHollow.Utilities;

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

        public const string HomeMap = "mapTown";
        const string _sMapFolder = @"Content\Maps";
        const string _sDungeonMapFolder = @"Content\Maps\Dungeons";
        public static string SpawnMap { get; private set; }
        public static Vector2 SpawnTile { get; private set; }
        public static Dictionary<string, RHMap> Maps { get; private set; }

        static NewMapInfo _newMapInfo;
        public static RHMap CurrentMap { get; set; }
        
        public static void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            Maps = new Dictionary<string, RHMap>();

            foreach(string dir in Directory.GetDirectories(_sMapFolder))
            {
                if (!dir.Contains("Textures"))
                {
                    foreach (string s in Directory.GetFiles(dir)) { AddMap(s, Content, GraphicsDevice); }
                }
            }

            string[] defaultInfo = DataManager.Config[7]["SpawnMap"].Split('-');
            SetSpawnMap(defaultInfo[0], int.Parse(defaultInfo[1]), int.Parse(defaultInfo[2]));
        }
        public static void LoadObjects()
        {
            foreach(RHMap m in Maps.Values)
            {
                m.LoadMapObjects();
            }
        }

        /// <summary>
        /// Sets the map that the player character will spawn on at" the start of thegame
        /// </summary>
        /// <param name="map">The name of the map</param>
        public static void SetSpawnMap(string map, int x, int y)
        {
            SpawnMap = map;
            CurrentMap = Maps[MapManager.SpawnMap];
            SpawnTile = new Vector2(x, y);
        }

        public static void AddMap(string mapToAdd, ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            RHMap newMap = new RHMap();

            string name = string.Empty;
            Util.ParseContentFileRetName(ref mapToAdd, ref name);
            if (name.IndexOf("map") == 0)                       //Ensures that we're loading a map
            {
                if (!Maps.ContainsKey(name))
                {
                    newMap.LoadContent(Content, GraphicsDevice, mapToAdd, name);
                    Maps.Add(name, newMap);
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
                if (CombatManager.InCombat)
                {
                    CombatManager.EndCombatEscape();
                }

                //IF the travel point has no linked map yet and is supposed to generate a level,
                //send a message off to the DungeonManager to initialize it
                if (travelPoint.GenerateZoneMap > 0 && string.IsNullOrEmpty(travelPoint.LinkedMap))
                {
                    DungeonManager.InitializeDungeon(MapManager.CurrentMap.DungeonName, travelPoint.GenerateZoneMap);
                }

                entryPoint = Maps[travelPoint.LinkedMap].DictionaryTravelPoints[currMap];

                Vector2 newPos = Vector2.Zero;
                if (travelPoint.IsDoor)
                {
                    newPos = entryPoint.GetMovedCenter();
                    PlayerManager.World.DetermineFacing(new Vector2(0, -1));
                }
                else
                {
                    newPos = entryPoint.FindLinkedPointPosition(travelPoint.Center, c);
                }

                FadeToNewMap(Maps[travelPoint.LinkedMap], newPos);
            }
            else
            {
                entryPoint = Maps[travelPoint.LinkedMap].DictionaryTravelPoints[currMap];

                c.ClearTileForMapChange();

                Maps[currMap].RemoveCharacter(c);
                Maps[travelPoint.LinkedMap].AddCharacter(c);
                RHTile newTile = Maps[travelPoint.LinkedMap].GetTileByGridCoords(Util.GetGridCoords(entryPoint.GetMovedCenter()));
                c.NewMapPosition = newTile.Position;
            }
        }

        /// <summary>
        /// Begins a fadeout so we can move to the next map and sets the info the map manager needs
        /// so that we know whichmap to move to once the fade is done.
        /// </summary>
        /// <param name="newMap">Map to move to</param>
        /// <param name="playerPos">The position of the player</param>
        public static void FadeToNewMap(RHMap newMap, Vector2 playerPos, Building b = null)
        {
            GUIManager.BeginFadeOut();

            PlayerManager.World.SetMovementState(ActorMovementStateEnum.Idle);
            PlayerManager.World.PlayAnimationVerb(CombatManager.InCombat ? VerbEnum.Walk : VerbEnum.Idle);
            _newMapInfo = new NewMapInfo(newMap, playerPos, b);
        }

        public static bool ChangingMaps()
        {
            return !_newMapInfo.Equals(default(NewMapInfo));
        }

        public static void EnterBuilding(TravelPoint doorLoc, Building b)
        {
            TravelPoint tPoint = null;

            foreach (string s in Maps[b.MapName].DictionaryTravelPoints.Keys)
            {
                if (s.Equals(PlayerManager.CurrentMap))
                {
                    tPoint = Maps[b.MapName].DictionaryTravelPoints[s];
                }
            }

            FadeToNewMap(Maps[b.MapName], tPoint.GetMovedCenter(), b);
        }

        public static void BackToPlayer()
        {
            CurrentMap = Maps[PlayerManager.CurrentMap];
        }

        public static void ViewMap(string newMap)
        {
            CurrentMap = Maps[newMap];
        }

        public static void PopulateMaps(bool loaded)
        {
            foreach(RHMap map in Maps.Values)
            {
                map.PopulateMap(loaded);
            }
            int mapWidth = Maps[MapManager.HomeMap].MapWidthTiles;
            int mapHeight = Maps[MapManager.HomeMap].MapHeightTiles;
            RHRandom rand = RHRandom.Instance();

            if (!loaded)
            {
                int rockID = int.Parse(DataManager.Config[1]["ObjectID"]);
                int bigRockID = int.Parse(DataManager.Config[2]["ObjectID"]);
                int treeID = int.Parse(DataManager.Config[3]["ObjectID"]);
                int grassID = int.Parse(DataManager.Config[11]["ObjectID"]);
                int stumpID = int.Parse(DataManager.Config[12]["ObjectID"]);
                int clayID = int.Parse(DataManager.Config[13]["ObjectID"]);

                List <RHTile> possibleTiles = Maps[MapManager.HomeMap].TileList;
                possibleTiles.RemoveAll(x => !x.Passable() || x.Flooring != null);

                PopulateHomeMapHelper(ref possibleTiles, bigRockID, 10);
                PopulateHomeMapHelper(ref possibleTiles, rockID, 49);
                PopulateHomeMapHelper(ref possibleTiles, clayID, 50);
                PopulateHomeMapHelper(ref possibleTiles, treeID, 99);
                PopulateHomeMapHelper(ref possibleTiles, stumpID, 10);
                PopulateHomeMapHelper(ref possibleTiles, grassID, 1000);
            }
        }

        private static void PopulateHomeMapHelper(ref List<RHTile> possibleTiles, int ID, int numToPlace)
        {
            RHRandom rand = RHRandom.Instance();
            for (int i = 0; i < numToPlace; i++)
            {
                RHTile targetTile = possibleTiles[rand.Next(0, possibleTiles.Count - 1)];
                WorldObject obj = DataManager.GetWorldObjectByID(ID);
                obj.PlaceOnMap(targetTile.Position, MapManager.Maps[HomeMap]);
                if (obj.CompareType(ObjectTypeEnum.Plant))
                {
                    ((Plant)obj).FinishGrowth();
                }

                possibleTiles.Remove(targetTile);
            }
        }

        public static void Update(GameTime gTime)
        {
            if(!_newMapInfo.Equals(default(NewMapInfo)) && GUIManager.FadingIn)
            {
                string oldMap = CurrentMap.Name;
                CurrentMap = _newMapInfo.NextMap;
                if (_newMapInfo.EnteredBuilding != null)
                {
                    CurrentMap.LoadBuilding(_newMapInfo.EnteredBuilding);
                }

                SoundManager.ChangeMap();
                CurrentMap.LeaveMap();
                PlayerManager.CurrentMap = _newMapInfo.NextMap.Name;
                PlayerManager.World.Position = _newMapInfo.PlayerPosition;
                CurrentMap.EnterMap();
                _newMapInfo = default;

                //Enter combat upon entering a map with living monsters
                if (CurrentMap.Monsters.Count > 0)
                {
                    CombatManager.NewBattle(oldMap);
                }
            }

            foreach(RHMap map in Maps.Values)
            {
                map.Update(gTime);
            }
        }

        public static void DrawBase(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawBase(spriteBatch);
            GUICursor.DrawTownBuildObject(spriteBatch);
            GUICursor.DrawPotentialWorldObject(spriteBatch);
        }

        public static void DrawLights(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawLights(spriteBatch);
        }
        public static void DrawUpper(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawUpper(spriteBatch);
        }

        public static bool ProcessLeftButtonClick(Point mouseLocation)
        {
            bool rv = false;

            rv = CurrentMap.ProcessLeftButtonClick(mouseLocation);

            return rv;
        }
        public static bool ProcessRightButtonClick(Point mouseLocation)
        {
            bool rv = false;

            rv = CurrentMap.ProcessRightButtonClick(mouseLocation);

            return rv;
        }
        public static bool ProcessHover(Point mouseLocation)
        {
            bool rv = false;

            rv = CurrentMap.ProcessHover(mouseLocation);

            return rv;
        }
        public static RHTile RetrieveTile(int x, int y)
        {
            return CurrentMap.GetTileByGridCoords(x, y);
        }
        public static void RemoveWorldObject(WorldObject o)
        {
            CurrentMap.RemoveWorldObject(o);
        }

        public static void RemoveCharacter(WorldActor c)
        {
            CurrentMap.RemoveCharacter(c);
        }
        public static void RemoveSummon(Summon s)
        {
            CurrentMap.RemoveSummon(s);
        }
        public static void CleanupSummons()
        {
            CurrentMap.CleanupSummons();
        }
        public static void RemoveMonster(Monster m)
        {
            CurrentMap.RemoveMonster(m);
        }
        public static void DropItemsOnMap(List<Item> items, Vector2 position, bool flyingPop = true)
        {
            CurrentMap.DropItemsOnMap(items, position, flyingPop);
        }

        public static void Rollover()
        {
            foreach(RHMap map in Maps.Values)
            {
                map.Rollover();
            }
        }

        public static void CheckSpirits()
        {
            foreach (RHMap map in Maps.Values)
            {
                map.CheckSpirits();
            }
        }
    }
}
