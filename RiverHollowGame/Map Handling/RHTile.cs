using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.MainObjects;
using RiverHollow.GUIComponents.Screens.HUDWindows;
using RiverHollow.Items;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.Map_Handling
{
    public class RHTile
    {
        public string MapName { get; }
        public int X { get; }
        public int Y { get; }
        public Point Position => new Point(X * Constants.TILE_SIZE, Y * Constants.TILE_SIZE);
        public Rectangle CollisionBox => new Rectangle(Position, Constants.TileSize);
        public Point Center => CollisionBox.Center;

        TravelPoint _travelPoint;

        Dictionary<TiledMapTileLayer, Dictionary<string, string>> _diProps;

        public Wallpaper _objWallpaper;
        public WorldObject WorldObject { get; private set; }
        public WorldObject ShadowObject { get; private set; }
        public WorldObject Flooring { get; private set; }
        public bool IsRoad { get; private set; }

        public bool IsWaterTile => ContainsProperty("Water", out string value) && value.Equals("true");

        public bool IsTilled => Flooring != null && Flooring.GetBoolByIDKey("Earth");
        public bool HasBeenWatered => GetEarth() != null && GetEarth().HasBeenWatered;

        bool _bArea = false;
        bool _bSelected = false;
        bool _bLegalTile = false;
        public bool IsWallpaperWall { get; private set; } = false;

        public RHTile(int x, int y, string mapName)
        {
            X = x;
            Y = y;

            MapName = mapName;
            _diProps = new Dictionary<TiledMapTileLayer, Dictionary<string, string>>();
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _objWallpaper?.Draw(spriteBatch);
        }

        public bool ProcessRightClick()
        {
            bool rv = false;
            var obj = GetWorldObject()?.Pickup;

            DirectionEnum facing = DirectionEnum.None;
            var travelPoint = GetTravelPoint();
            if (travelPoint != null)
            {
                if (!MapManager.ChangingMaps() && PlayerManager.InRangeOfPlayer(travelPoint.CollisionBox, ref facing))
                {
                    PlayerManager.PlayerActor.SetFacing(DirectionEnum.Up);
                    MapManager.ChangeMaps(PlayerManager.PlayerActor, MapName, _travelPoint);
                    SoundManager.PlayEffect(SoundEffectEnum.Door);

                    return true;
                }
            }
            else
            {
                bool playerAdjacent = PlayerManager.InRangeOfPlayer(CollisionBox, ref facing);
                bool inRangeOfObject = obj != null && PlayerManager.InRangeOfPlayer(obj.CollisionBox, ref facing);
                if (inRangeOfObject)
                {
                    rv = obj.ProcessRightClick();
                }

                if (!rv && (playerAdjacent || inRangeOfObject) && !Passable())
                {
                    PlayerManager.TryGrab(this, facing);
                }
            }

            return rv;
        }

        public void SetWallTrue()
        {
            IsWallpaperWall = true;
        }

        public bool SetFloorObject(WorldObject obj)
        {
            bool rv = false;
            if (Flooring == null)
            {
                rv = true;
                Flooring = obj;
            }

            return rv;
        }

        public void SetProperties(RHMap map)
        {
            foreach (KeyValuePair<string, List<TiledMapTileLayer>> kvp in map.Layers)
            {
                foreach (TiledMapTileLayer l in kvp.Value)
                {
                    if (l.TryGetTile(X, Y, out TiledMapTile? tile) && tile != null)
                    {
                        if (((TiledMapTile)tile).GlobalIdentifier != 0)
                        {
                            _diProps.Add(l, map.GetTileProperties((TiledMapTile)tile));
                        }
                    }
                }
            }
            IsRoad = ContainsProperty("Road", out string value) && value.Equals("true");
        }

        /// <summary>
        /// Retrieves the WorldObject on the Tile. If the parameter is false,
        /// it will not return the shadow object
        /// </summary>
        /// <param name="AlsoCheckShadow">Whether or not to return a shadow object</param>
        /// <returns>The relevant associated WorldObject</returns>
        public WorldObject GetWorldObject(bool AlsoCheckShadow = true)
        {
            WorldObject obj = null;

            //Only return the Shadow object if there is no actual WorldObject
            if (WorldObject != null) { obj = WorldObject; }
            else if (AlsoCheckShadow) { obj = ShadowObject; }

            return obj;
        }

        private WorldObject ShadowStructure()
        {
            return (ShadowObject != null && ShadowObject.CompareType(ObjectTypeEnum.Buildable)) ? ShadowObject : null;
        }
        public bool HasObject()
        {
            return WorldObject != null || ShadowStructure() != null || Flooring != null;
        }
        public WorldObject RetrieveObjectFromLayer(bool getEditable)
        {
            WorldObject rv = null;
            if (getEditable)
            {
                if (WorldObject != null && WorldObject.PlayerCanEdit()) { rv = WorldObject.Pickup; }
                else if (Flooring != null) { rv = Flooring; }
                else if (ShadowStructure() != null && ShadowStructure().PlayerCanEdit()) { rv = ShadowStructure(); }
            }
            else
            {
                rv = WorldObject ?? ShadowStructure() ?? Flooring;
            }

            return rv;
        }

        public void RemoveWorldObject()
        {
            WorldObject = null;
        }
        public void RemoveShadowObject()
        {
            ShadowObject = null;
        }
        public void RemoveFlooring()
        {
            Flooring = null;
        }
        public bool SetObject(WorldObject o)
        {
            bool rv = false;
            if (o.FlooringObject())
            {
                rv = SetFloorObject(o);
            }
            else if ((!o.WallObject() && Passable()) || (o.WallObject() && IsWallpaperWall))
            {
                WorldObject = o;
                rv = true;
            }
            return rv;
        }
        public void SetShadowObject(WorldObject o)
        {
            ShadowObject = o;
        }
        public WorldObject GetFloorObject()
        {
            WorldObject f = null;

            if (Flooring != null) { f = Flooring; }

            return f;
        }

        public void SetWallpaper(Wallpaper obj) { _objWallpaper = obj; }

        public void SetTravelPoint(TravelPoint obj) { _travelPoint = obj; }
        public TravelPoint GetTravelPoint()
        {
            return _travelPoint;
        }

        public bool Contains(Actor n)
        {
            return CollisionBox.Contains(n.CollisionCenter);
        }
        public bool ContainsProperty(string property, out string value)
        {
            bool rv = false;
            value = string.Empty;
            foreach (TiledMapTileLayer l in _diProps.Keys)
            {
                rv = ContainsProperty(l, property, out value);
                if (rv) { break; }
            }

            return rv;
        }
        public bool ContainsProperty(TiledMapTileLayer l, string property, out string value)
        {
            bool rv = false;
            value = string.Empty;
            if (_diProps.ContainsKey(l) && _diProps[l].ContainsKey(property))
            {
                value = _diProps[l][property];
                rv = true;
            }

            return rv;
        }

        /// <summary>
        /// Attempt to damage the object assuming it is destructible and can be affected by the used tool
        /// </summary>
        /// <param name="toolUsed">The tool being used</param>
        /// <returns>True if the tool was able to deal damage</returns>
        public bool DamageObject(Tool toolUsed)
        {
            bool rv = false;
            if (WorldObject != null && WorldObject.IsDestructible())
            {
                rv = true;
                ((Destructible)WorldObject).DealDamage(toolUsed);
            }

            return rv;
        }

        public void Rollover()
        {
            GetEarth()?.Rollover();
        }


        public Earth GetEarth()
        {
            Earth rv = null;

            if (IsTilled)
            {
                return Flooring as Earth;
            }

            return rv;
        }

        public void WaterTile()
        {
            if (IsTilled)
            {
                GetEarth().SetWatered(true);
            }
        }

        #region TileTraversal
        private RHMap CurrentMap()
        {
            return MapManager.Maps[MapName];
        }

        public List<RHTile> GetWalkableNeighbours(bool getDiagonal = false)
        {
            List<RHTile> rvList = new List<RHTile>();
            foreach (RHTile tile in GetAdjacentTiles(getDiagonal))
            {
                if (tile != null && tile.CanWalkThrough())
                {
                    rvList.Add(tile);
                }
            }

            return rvList;
        }

        /// <summary>
        /// Returns a list of all RHTiles adjacent to this tile
        /// </summary>
        /// <returns></returns>
        public List<RHTile> GetAdjacentTiles(bool getDiagonal = false)
        {
            List<RHTile> adj = new List<RHTile>();

            //Have to null check
            RHTile temp = GetTileByDirection(DirectionEnum.Up);
            if (temp != null) { adj.Add(temp); }
            temp = GetTileByDirection(DirectionEnum.Down);
            if (temp != null) { adj.Add(temp); }
            temp = GetTileByDirection(DirectionEnum.Left);
            if (temp != null) { adj.Add(temp); }
            temp = GetTileByDirection(DirectionEnum.Right);
            if (temp != null) { adj.Add(temp); }

            if (getDiagonal)
            {
                temp = GetTileByDirection(DirectionEnum.Down)?.GetTileByDirection(DirectionEnum.Left);
                if (temp != null) { adj.Add(temp); }
                temp = GetTileByDirection(DirectionEnum.Down)?.GetTileByDirection(DirectionEnum.Right);
                if (temp != null) { adj.Add(temp); }
                temp = GetTileByDirection(DirectionEnum.Up)?.GetTileByDirection(DirectionEnum.Left);
                if (temp != null) { adj.Add(temp); }
                temp = GetTileByDirection(DirectionEnum.Up)?.GetTileByDirection(DirectionEnum.Right);
                if (temp != null) { adj.Add(temp); }
            }

            return adj;
        }

        /// <summary>
        /// Returns the tile on the TileMap in the given direction from the MapTile
        /// </summary>
        /// <param name="t">The direction to look in</param>
        /// <returns>The Tile if it exists, or null</returns>
        public RHTile GetTileByDirection(DirectionEnum t)
        {
            RHTile rvTile = null;
            switch (t)
            {
                case DirectionEnum.Down:
                    if (this.Y < CurrentMap().MapHeightTiles - 1)
                    {
                        rvTile = CurrentMap().GetTileByGridCoords(this.X, this.Y + 1);
                    }
                    break;
                case DirectionEnum.Left:
                    if (this.X > 0)
                    {
                        rvTile = CurrentMap().GetTileByGridCoords(this.X - 1, this.Y);
                    }
                    break;
                case DirectionEnum.Up:
                    if (this.Y > 0)
                    {
                        rvTile = CurrentMap().GetTileByGridCoords(this.X, this.Y - 1);
                    }
                    break;
                case DirectionEnum.Right:
                    if (this.X < CurrentMap().MapWidthTiles - 1)
                    {
                        rvTile = CurrentMap().GetTileByGridCoords(this.X + 1, this.Y);
                    }
                    break;
            }


            return rvTile;
        }

        /// <summary>
        /// Returns whether the RHTile itself can be walked through or not. This relates
        /// to the core status of the tile, and has nothing to do with objects or actors on it.
        /// </summary>
        /// <returns>True if the tile is not itself locked down</returns>
        public bool Passable()
        {
            bool rv = TileIsPassable() && (WorldObject == null || WorldObject.Walkable);

            return rv;
        }

        public bool TileIsPassable()
        {
            bool rv = true;
            foreach (TiledMapTileLayer l in _diProps.Keys)
            {
                if (l.IsVisible && !l.Name.Contains("Upper") && ContainsProperty(l, "Impassable", out string val) && val.Equals("true"))
                {
                    rv = false;
                    break;
                }
            }

            return rv;
        }

        public bool CanPlaceOnTabletop(WorldObject objToPlace)
        {
            bool rv = false;

            if (WorldObject != null && objToPlace.BuildableType(BuildableEnum.Decor) && WorldObject.BuildableType(BuildableEnum.Decor))
            {
                Decor decorObj = (Decor)WorldObject;
                Decor decorToPlace = (Decor)objToPlace;
                if (decorObj.CanDisplay && decorToPlace.CanBeDisplayed)
                {
                    rv = ((Decor)WorldObject).CanDisplay;
                }
            }

            return rv;
        }

        /// <summary>
        /// This method defines which RHTiles are allowed to be the target of abilities and spells.
        /// You cannot use skills on an RHTile that is impassable, or occupied by an object.
        /// </summary>
        /// <returns>Returns True if the Tile is a legal tile to target</returns>
        public bool CanTargetTile()
        {
            return Passable() && (WorldObject == null || WorldObject.Walkable);
        }

        /// <summary>
        /// Determines if the RHTile can be walked through. 
        /// </summary>
        /// <returns>True if the RHTile can be walked through. Does not mean the RHTile
        /// can be assigned to if it returns true.</returns>
        public bool CanWalkThrough()
        {
            return CanTargetTile();
        }
        #endregion

        public RHTileData SaveData()
        {
            RHTileData data = new RHTileData
            {
                x = X,
                y = Y,
                wallpaperData = _objWallpaper == null ? -1 : _objWallpaper.ID
            };

            return data;
        }
    }
}
