using System.IO;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Buildings;
using RiverHollow.Characters;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.WorldObjects;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using RiverHollow.Items;
using static RiverHollow.Utilities.Enums;

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

        public static RHMap TownMap => Maps[Constants.TOWN_MAP_NAME];
        const string _sMapFolder = @"Content\Maps";
        const string _sDungeonMapFolder = @"Content\Maps\Dungeons";
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

            CurrentMap = Maps[Constants.PLAYER_HOME_NAME];
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
            if (c == PlayerManager.PlayerActor)
            {
                //If the travel point has no linked map yet and is supposed to generate a level,
                //send a message off to the DungeonManager to initialize it
                if (travelPoint.DungeonInfoID > -1 && string.IsNullOrEmpty(travelPoint.LinkedMap))
                {
                    DungeonManager.InitializeProceduralDungeon(MapManager.CurrentMap.DungeonName, MapManager.CurrentMap.Name, travelPoint);
                }

                entryPoint = Maps[travelPoint.LinkedMap].DictionaryTravelPoints[currMap];

                Vector2 newPos = Vector2.Zero;
                if (travelPoint.IsDoor)
                {
                    newPos = entryPoint.GetMovedCenter();
                    PlayerManager.PlayerActor.DetermineAnimationState(new Vector2(0, -1));
                }
                else if (travelPoint.NoMove)
                {
                    newPos = c.Position;
                }
                else
                {
                    newPos = entryPoint.FindLinkedPointPosition(travelPoint.Center, c);
                }

                Maps[travelPoint.LinkedMap].SpawnMapEntities();
                PlayerManager.PlayerActor.ActivePet?.ChangeState(NPCStateEnum.Alert);
                FadeToNewMap(Maps[travelPoint.LinkedMap], newPos, travelPoint.TargetBuilding);

                PlayerManager.ReleaseTile();
            }
            else
            {
                entryPoint = Maps[travelPoint.LinkedMap].DictionaryTravelPoints[currMap];

                c.ClearTileForMapChange();

                Maps[currMap].RemoveActor(c);
                Maps[travelPoint.LinkedMap].AddActor(c);
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
            if (newMap.Name != CurrentMap.MapAbove && newMap.Name != CurrentMap.MapBelow)
            {
                GUIManager.BeginFadeOut();
            }

            PlayerManager.PlayerActor.DetermineAnimationState(Vector2.Zero);
            _newMapInfo = new NewMapInfo(newMap, playerPos, b);
        }

        public static bool ChangingMaps()
        {
            return !_newMapInfo.Equals(default(NewMapInfo));
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
            int mapWidth = Maps[Constants.TOWN_MAP_NAME].MapWidthTiles;
            int mapHeight = Maps[Constants.TOWN_MAP_NAME].MapHeightTiles;
            RHRandom rand = RHRandom.Instance();

            if (!loaded)
            {
                int rockID = int.Parse(DataManager.Config[1]["ObjectID"]);
                int bigRockID = int.Parse(DataManager.Config[2]["ObjectID"]);
                int treeID = int.Parse(DataManager.Config[3]["ObjectID"]);
                int grassID = int.Parse(DataManager.Config[11]["ObjectID"]);
                int stumpID = int.Parse(DataManager.Config[12]["ObjectID"]);

                List <RHTile> possibleTiles = Maps[Constants.TOWN_MAP_NAME].TileList;
                possibleTiles.RemoveAll(x => !x.Passable() || x.Flooring != null);

                PopulateHomeMapHelper(ref possibleTiles, bigRockID, 5);
                PopulateHomeMapHelper(ref possibleTiles, rockID, 50);
                PopulateHomeMapHelper(ref possibleTiles, treeID, 50);
                PopulateHomeMapHelper(ref possibleTiles, stumpID, 10);
            }
        }

        private static void PopulateHomeMapHelper(ref List<RHTile> possibleTiles, int ID, int numToPlace)
        {
            RHRandom rand = RHRandom.Instance();
            for (int i = 0; i < numToPlace; i++)
            {
                RHTile targetTile = possibleTiles[rand.Next(0, possibleTiles.Count - 1)];
                WorldObject obj = DataManager.CreateWorldObjectByID(ID);
                obj.PlaceOnMap(targetTile.Position, MapManager.Maps[Constants.TOWN_MAP_NAME]);
                if (obj.CompareType(ObjectTypeEnum.Plant))
                {
                    ((Plant)obj).FinishGrowth();
                }

                possibleTiles.Remove(targetTile);
            }
        }

        public static void Update(GameTime gTime)
        {
            if (!_newMapInfo.Equals(default(NewMapInfo)) && (GUIManager.FadingIn || GUIManager.NotFading))
            {
                if (PlayerManager.PlayerActor.ActivePet != null) { CurrentMap.RemoveActor(PlayerManager.PlayerActor.ActivePet); }
                if (PlayerManager.PlayerActor.ActiveMount != null) { CurrentMap.RemoveActor(PlayerManager.PlayerActor.ActiveMount); }

                CurrentMap.ResetMobPositioning();
                CurrentMap.LeaveMap();
                TaskManager.AssignDelayedTasks();
                string oldMap = CurrentMap.Name;
                CurrentMap = _newMapInfo.NextMap;

                SoundManager.ChangeMap();
                PlayerManager.CurrentMap = _newMapInfo.NextMap.Name;
                PlayerManager.PlayerActor.Position = _newMapInfo.PlayerPosition;
                CurrentMap.EnterMap();

                if (_newMapInfo.EnteredBuilding != null)
                {
                    GameManager.CurrentBuilding = _newMapInfo.EnteredBuilding;
                    //CurrentMap.LoadBuilding(_newMapInfo.EnteredBuilding);
                    TaskManager.TaskProgressEnterBuilding(_newMapInfo.EnteredBuilding.ID);
                }
                else
                {
                    GameManager.CurrentBuilding = null;
                }

                _newMapInfo = default;

                PlayerManager.PlayerActor.ActivePet?.SpawnNearPlayer();

                if (PlayerManager.PlayerActor.ActiveMount != null)
                {
                    if (CurrentMap.IsDungeon)
                    {
                        PlayerManager.PlayerActor.Dismount();
                        PlayerManager.PlayerActor.Position = _newMapInfo.PlayerPosition;
                    }
                    else { PlayerManager.PlayerActor.ActiveMount.SyncToPlayer(); }
                }
            }

            foreach(RHMap map in Maps.Values)
            {
                map.Update(gTime);
            }
        }

        public static void DrawBelowBase(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawBelowBase(spriteBatch);
        }
        public static void DrawAboveBase(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawAboveBase(spriteBatch);
        }
        public static void DrawBase(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawBase(spriteBatch);
        }

        public static void DrawBelowGround(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawBelowGround(spriteBatch);
        }
        public static void DrawAboveGround(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawAboveGround(spriteBatch);
        }
        public static void DrawGround(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawGround(spriteBatch);
            GUICursor.DrawTownBuildObject(spriteBatch);
            GUICursor.DrawPotentialWorldObject(spriteBatch);
        }

        public static void DrawBelowUpper(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawBelowUpper(spriteBatch);
        }
        public static void DrawAboveUpper(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawAboveUpper(spriteBatch);
        }
        public static void DrawUpper(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawUpper(spriteBatch);
        }

        public static void DrawLights(SpriteBatch spriteBatch)
        {
            CurrentMap.DrawLights(spriteBatch);
            PlayerManager.DrawLight(spriteBatch);
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

        public static void RemoveActor(WorldActor c)
        {
            CurrentMap.RemoveActor(c);
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
