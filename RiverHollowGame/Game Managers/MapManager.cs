﻿using System.IO;
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
using RiverHollow.GUIComponents.Screens;
using System.Linq;

namespace RiverHollow.Game_Managers
{
    public static class MapManager
    {
        public static RHMap TownMap => Maps[Constants.TOWN_MAP_NAME];
        public static RHMap InnMap => Maps[Constants.INN_MAP_NAME];
        public static RHMap HomeMap => Maps[Constants.PLAYER_HOME_NAME];
        const string _sMapFolder = @"Content\Maps";
        const string _sDungeonMapFolder = @"Content\Maps\Dungeons";
        public static Vector2 SpawnTile { get; private set; }
        public static Dictionary<string, RHMap> Maps { get; private set; }

        static NewMapInfo _newMapInfo;
        public static RHMap CurrentMap { get; set; }
        public static RHTimer MapChangeTimer { get; private set; }
        
        public static void LoadContent(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            Maps = new Dictionary<string, RHMap>();

            foreach (string dir in Directory.GetDirectories(_sMapFolder))
            {
                if (!dir.Contains("Textures"))
                {
                    foreach (string s in Directory.GetFiles(dir)) { AddMap(s, Content, GraphicsDevice); }
                }
            }

            CurrentMap = Maps[Constants.PLAYER_HOME_NAME];
            MapChangeTimer = new RHTimer(Constants.PLAYER_GRACE_PERIOD, true);
        }
        public static void LoadObjects()
        {
            foreach(RHMap m in Maps.Values)
            {
                m.LoadMapObjects(false);
            }

            foreach (var v in TownManager.Villagers)
            {
                if (DataManager.GetBoolByIDKey(v.Key, "Town", DataType.Actor))
                {
                    v.Value.SendToTown(true);
                }
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

        public static void ChangeMaps(Actor actor, string currMap, TravelPoint travelPoint)
        {
            //Get the entry rectangle on the new map
            TravelPoint entryPoint;
            actor.ClearMovementBuffer();

            //Handling for if the WorldActor is the player character
            if (actor == PlayerManager.PlayerActor)
            {
                RHMap linkedMap = Maps[travelPoint.LinkedMap];
                linkedMap.SpawnMapEntities();

                if (travelPoint.WorldMap)
                {
                    GUIManager.SetScreen(new WorldMapScreen(travelPoint));
                }
                else
                {
                    //If the travel point has no linked map yet and is supposed to generate a level,
                    //send a message off to the DungeonManager to initialize it
                    if (travelPoint.DungeonInfoID > -1 && string.IsNullOrEmpty(travelPoint.LinkedMap))
                    {
                        DungeonManager.InitializeProceduralDungeon(MapManager.CurrentMap.DungeonName, MapManager.CurrentMap.Name, travelPoint);
                    }

                    entryPoint = linkedMap.GetTravelPoint(string.Format("{0}{1}", currMap, travelPoint.MapConnector));

                    Point newPos;
                    if (travelPoint.IsDoor)
                    {
                        newPos = entryPoint.GetMovedCenter();
                        PlayerManager.PlayerActor.DetermineAnimationState(Util.GetPointFromDirection(DirectionEnum.Up));
                    }
                    else if (travelPoint.NoMove)
                    {
                        newPos = actor.CollisionBoxLocation;
                    }
                    else
                    {
                        newPos = entryPoint.FindLinkedPointPosition(travelPoint, actor);
                    }

                    PlayerManager.PlayerActor.ActivePet?.ChangeState(NPCStateEnum.Alert);
                    FadeToNewMap(linkedMap, newPos, entryPoint.EntranceDir, travelPoint.TargetBuilding);

                    PlayerManager.ReleaseTile();
                }
            }
            else if (!actor.Wandering)
            {
                RHMap linkedMap = Maps[travelPoint.LinkedMap];

                entryPoint = linkedMap.GetTravelPoint(currMap);

                Maps[currMap].RemoveActor(actor);
                linkedMap.AddActor(actor);
                RHTile newTile = linkedMap.GetTileByGridCoords(Util.GetGridCoords(entryPoint.GetMovedCenter()));
                actor.NewMapPosition = newTile.Position;
                actor.SetFacing(entryPoint.EntranceDir);

                actor.ClearTileForMapChange(newTile.Position);
            }
        }

        /// <summary>
        /// Begins a fadeout so we can move to the next map and sets the info the map manager needs
        /// so that we know whichmap to move to once the fade is done.
        /// </summary>
        /// <param name="newMap">Map to move to</param>
        /// <param name="playerPos">The position of the player</param>
        public static void FadeToNewMap(RHMap newMap, Point playerPos, DirectionEnum facing, Building b = null)
        {
            if (newMap.Name != CurrentMap.MapAbove && newMap.Name != CurrentMap.MapBelow)
            {
                FadeOut();
            }

            PlayerManager.PlayerActor.DetermineAnimationState(Point.Zero);
            _newMapInfo = new NewMapInfo(newMap, playerPos, facing, b);
        }

        public static void FadeOut()
        {
            GUIManager.BeginFadeOut();
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

        public static void PopulateMaps(bool gameStart)
        {
            foreach (RHMap map in Maps.Values)
            {
                map.PopulateMap(gameStart);
            }
        }

        public static void Update(GameTime gTime)
        {
            if (GUIManager.FullFade)
            {
                if (!_newMapInfo.Equals(default(NewMapInfo)))
                {
                    if (PlayerManager.PlayerActor.CurrentHP == 0) { PlayerManager.PlayerActor.IncreaseHealth(1); }
                    if (PlayerManager.PlayerActor.ActivePet != null) { CurrentMap.RemoveActor(PlayerManager.PlayerActor.ActivePet); }
                    if (PlayerManager.PlayerActor.ActiveMount != null) { CurrentMap.RemoveActor(PlayerManager.PlayerActor.ActiveMount); }

                    CurrentMap.LeaveMap();
                    TaskManager.AssignDelayedTasks();
                    CurrentMap = _newMapInfo.NextMap;

                    SoundManager.ChangeMap();
                    PlayerManager.CurrentMap = _newMapInfo.NextMap.Name;
                    PlayerManager.PlayerActor.SetPosition(_newMapInfo.PlayerPosition);
                    PlayerManager.PlayerActor.SetFacing(_newMapInfo.Facing);
                    PlayerManager.PlayerActor.DetermineAnimationState();

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
                    CurrentMap.EnterMap();

                    _newMapInfo = default;

                    PlayerManager.PlayerActor.ActivePet?.SpawnNearPlayer();

                    if (PlayerManager.PlayerActor.ActiveMount != null)
                    {
                        if (CurrentMap.IsDungeon)
                        {
                            PlayerManager.PlayerActor.Dismount();
                            PlayerManager.PlayerActor.SetPosition(_newMapInfo.PlayerPosition);
                        }
                        else { PlayerManager.PlayerActor.ActiveMount.SyncToPlayer(); }
                    }

                    if (CurrentMap.GetMapProperties().ContainsKey("PetCafe"))
                    {
                        Pet activePet = PlayerManager.PlayerActor.ActivePet;
                        foreach (var p in PlayerManager.Pets)
                        {
                            if(activePet == null || p.ID != activePet.ID)
                            {
                                p.SetPosition(CurrentMap.GetRandomPosition(CurrentMap.GetCharacterObject("Destination")));
                                CurrentMap.AddActor(p);
                            }
                        }
                    }

                    MapChangeTimer.Reset();
                }
                else
                {
                    CurrentMap.SendVillagersToTown();
                }
            }
            else
            {
                MapChangeTimer?.TickDown(gTime);
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
            return CurrentMap.ProcessLeftButtonClick(mouseLocation);
        }
        public static bool ProcessRightButtonClick(Point mouseLocation)
        {
            return CurrentMap.ProcessRightButtonClick(mouseLocation);
        }
        public static bool ProcessHover(Point mouseLocation)
        {
            return CurrentMap.ProcessHover(mouseLocation);
        }
        public static RHTile RetrieveTile(int x, int y)
        {
            return CurrentMap.GetTileByGridCoords(x, y);
        }
        public static void RemoveWorldObject(WorldObject o, bool immediately = false)
        {
            CurrentMap.RemoveWorldObject(o, immediately);
        }

        public static void RemoveActor(Actor c)
        {
            CurrentMap.RemoveActor(c);
        }

        public static void DropItemOnMap(Item item, Point position, bool flyingPop = true)
        {
            CurrentMap.SpawnItemOnMap(item, position, flyingPop);
        }
        public static void DropItemsOnMap(List<Item> items, Point position, bool flyingPop = true)
        {
            CurrentMap.SpawnItemsOnMap(items, position, flyingPop);
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
        public static void QueryWorldMapPathing(ref Dictionary<string, MapPathInfo> travelMap, TravelPoint travelpoint)
        {
            RHMap tempMap = CurrentMap;

            //Assign currentmap travel time to 0
            MapPathInfo pathInfo = new MapPathInfo(tempMap.Name, tempMap.Name, 0)
            {
                Unlocked = tempMap.Visited
            };

            travelMap[tempMap.Name] = pathInfo;
            Recursive(tempMap, ref travelMap, travelpoint);
        }

        private static void Recursive(RHMap tempMap, ref Dictionary<string, MapPathInfo> travelMap, TravelPoint travelpoint)
        {
            foreach (KeyValuePair<string, int> kvp in tempMap.WorldMapNode.MapConnections)
            {
                if (!travelMap.ContainsKey(kvp.Key))
                {
                    int cost = (CurrentMap != tempMap || travelpoint.LinkedMap != kvp.Key) ? tempMap.WorldMapNode.Cost : 0;
                    MapPathInfo pathInfo = new MapPathInfo(kvp.Key, tempMap.Name, cost + travelMap[tempMap.Name].Time + tempMap.WorldMapNode.MapConnections[kvp.Key])
                    {
                        Unlocked = Maps[kvp.Key].Visited
                    };
                    travelMap[kvp.Key] = pathInfo;
                    Recursive(Maps[kvp.Key], ref travelMap, travelpoint);
                }
            }
        }
    }
}
