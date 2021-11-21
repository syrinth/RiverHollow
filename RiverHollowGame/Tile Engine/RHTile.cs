using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Tiled;
using RiverHollow.Characters;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.MainObjects;
using RiverHollow.Utilities;
using RiverHollow.WorldObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Game_Managers.TacticalCombatManager;
using static RiverHollow.WorldObjects.Buildable;
using static RiverHollow.WorldObjects.Buildable.AdjustableObject;

namespace RiverHollow.Tile_Engine
{
    public class RHTile
    {
        bool _tileExists;
        public string MapName { get; }
        public int X { get; }
        public int Y { get; }
        public Vector2 Position => new Vector2(X * TILE_SIZE, Y * TILE_SIZE);
        public Vector2 Center => new Vector2(Position.X + TILE_SIZE / 2, Position.Y + TILE_SIZE / 2);
        public Rectangle Rect => Util.FloatRectangle(Position, TILE_SIZE, TILE_SIZE);

        string _sClickAction = string.Empty;
        TravelPoint _travelPoint;
        public TacticalCombatActor Character { get; private set; }

        Dictionary<TiledMapTileLayer, Dictionary<string, string>> _diProps;

        public Wallpaper _objWallpaper;
        public WorldObject WorldObject { get; private set; }
        public WorldObject ShadowObject { get; private set; }
        public CombatHazard HazardObject { get; private set; }
        public Floor Flooring { get; private set; }
        public bool IsRoad { get; private set; }

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
            Rectangle dest = new Rectangle((int)Position.X, (int)Position.Y, TILE_SIZE, TILE_SIZE);

            if (TacticalCombatManager.InCombat)
            {
                //Only draw one of the tile targetting types
                if (this == TacticalCombatManager.ActiveCharacter?.BaseTile && DisplaySelectedTile()) { spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS), dest, new Rectangle(48, 112, 16, 16), Color.White); }
                else if (_bSelected) { spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS), dest, new Rectangle(16, 112, 16, 16), Color.White); }
                else if (_bArea) { spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS), dest, new Rectangle(32, 112, 16, 16), Color.White); }
                else if (_bLegalTile) { spriteBatch.Draw(DataManager.GetTexture(DataManager.FILE_WORLDOBJECTS), dest, new Rectangle(0, 112, 16, 16), Color.White); }
            }

            if (Flooring != null) { Flooring.Draw(spriteBatch); }
            if (WorldObject != null) { WorldObject.Draw(spriteBatch); }
        }
        public void DrawWallpaper(SpriteBatch spriteBatch)
        {
            _objWallpaper?.Draw(spriteBatch);
        }

        public bool ProcessRightClick()
        {
            bool rv = false;

            if (!string.IsNullOrEmpty(_sClickAction))
            {
                if (_sClickAction.Equals("Display_Town")) { GUIManager.OpenMainObject(new TownInfoWindow()); }
            }
            if (GetTravelPoint() != null)
            {
                if (PlayerManager.PlayerInRange(_travelPoint.CollisionBox) && !MapManager.ChangingMaps())
                {
                    // if (obj.BuildingID > 1) { MapManager.EnterBuilding(obj, PlayerManager.Buildings.Find(x => x.PersonalID == obj.BuildingID)); }
                    //else { MapManager.ChangeMaps(PlayerManager.World, this.Name, obj); }
                    MapManager.ChangeMaps(PlayerManager.World, MapName, _travelPoint);
                    SoundManager.PlayEffect("close_door_1");
                    return true;
                }
            }
            else if (GetWorldObject() != null)
            {
                if (PlayerManager.PlayerInRange(Center.ToPoint()))
                {
                    GetWorldObject().ProcessRightClick();
                    rv = true;
                }
            }

            if (ContainsProperty("Save", out string val) && val.Equals("true"))
            {
                GUIManager.OpenTextWindow(DataManager.GetGameTextEntry("Save"));
            }

            return rv;
        }

        private bool DisplaySelectedTile()
        {
            return CombatPhaseCheck(CmbtPhaseEnum.ChooseActionTarget) || CombatPhaseCheck(CmbtPhaseEnum.ChooseMoveTarget) || CombatPhaseCheck(CmbtPhaseEnum.MainSelection);
        }

        public void SetWallTrue()
        {
            IsWallpaperWall = true;
        }

        public bool SetFloor(Floor f)
        {
            bool rv = false;
            if (Flooring == null)
            {
                rv = true;
                Flooring = f;
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
                        if (tile.Value.GlobalIdentifier != 0)
                        {
                            _tileExists = true;
                        }
                        if (((TiledMapTile)tile).GlobalIdentifier != 0)
                        {
                            _diProps.Add(l, map.GetProperties((TiledMapTile)tile));
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
            return (ShadowObject != null && ShadowObject.IsBuildable()) ? ShadowObject : null;
        }
        public bool HasBuildableObject()
        {
            return WorldObject != null || ShadowStructure() != null || Flooring != null;
        }
        public WorldObject RetrieveUppermostStructureObject()
        {
            return ShadowStructure() ?? WorldObject ?? Flooring;
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
            if (o.CompareType(ObjectTypeEnum.Floor))
            {
                rv = SetFloor((Floor)o);
            }
            else if ((!o.WallObject && Passable()) || (o.WallObject && IsWallpaperWall))
            {
                WorldObject = o;
                rv = true;
            }
            return rv;
        }
        public bool SetShadowObject(WorldObject o)
        {
            bool rv = false;
            if ((!o.WallObject && Passable()) || (o.WallObject && IsWallpaperWall))
            {
                ShadowObject = o;
                rv = true;
            }
            return rv;
        }
        public Floor GetFloorObject()
        {
            Floor f = null;

            if (Flooring != null) { f = Flooring; }

            return f;
        }

        public void SetWallpaper(Wallpaper obj) { _objWallpaper = obj; }


        /// <summary>
        /// Sets the Hazard object for the RHTile
        /// </summary>
        /// <param name="h">The Hazard object to set.</param>
        public void SetHazard(CombatHazard h)
        {
            HazardObject = h;
        }

        /// <summary>
        /// Determines whether or not the tile is a valid target for being dug.
        /// 
        /// Currently can be dug if the property is set, and there are no objects sitting on it.
        /// </summary>
        /// <returns></returns>
        public bool CanDig()
        {
            bool rv = false;
            foreach (TiledMapTileLayer l in _diProps.Keys)
            {
                if (l.IsVisible && ContainsProperty(l, "CanDig", out string val) && val.Equals("true") && WorldObject == null && Flooring == null)
                {
                    rv = true;
                }
            }

            return rv;
        }

        public void SetClickAction(string str) { _sClickAction = str; }
        public void SetTravelPoint(TravelPoint obj) { _travelPoint = obj; }
        public TravelPoint GetTravelPoint()
        {
            return _travelPoint;
        }

        public bool Contains(Villager n)
        {
            bool rv = false;

            rv = Rect.Contains(n.CollisionBox.Center);

            return rv;
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
                Destructible d = (Destructible)WorldObject;
                if (d.NeededTool == toolUsed.ToolType)
                {
                    SoundManager.PlayEffectAtLoc(toolUsed.SoundEffect, MapName, Center, toolUsed);
                    d.DealDamage(toolUsed.ToolLevel);
                }
            }

            return rv;
        }

        public void Clear()
        {
            WorldObject = null;
            Flooring = null;
        }

        public void Rollover()
        {
            WorldObject.Rollover();
            Flooring.Rollover();
        }

        /// <summary>
        /// Returns if the tile has a CombatActor assigned to it. 
        /// </summary>
        public bool HasCombatant()
        {
            return Character != null;
        }

        /// <summary>
        /// Assigns a CombatActor to the RHTile
        /// </summary>
        /// <param name="c">The combatant to set to this tile</param>
        public void SetCombatant(TacticalCombatActor c)
        {
            Character = c;
        }

        /// <summary>
        /// Sets the selected value of the RHTile
        /// </summary>
        /// <param name="val">Whether to set or unset the selected value</param>
        public void Select(bool val)
        {
            _bSelected = val;
        }

        /// <summary>
        /// Sets the legal value of the RHTile
        /// </summary>
        /// <param name="val">Whether to set or unset the selected value</param>
        public void LegalTile(bool val)
        {
            _bLegalTile = val;
        }

        /// <summary>
        /// Sets the area value of the RHTile
        /// </summary>
        /// <param name="val">Whether to set or unset the area value</param>
        public void AreaTile(bool val)
        {
            _bArea = val;
        }

        #region TileTraversal
        private RHMap MyMap()
        {
            return MapManager.Maps[MapName];
        }

        public List<RHTile> GetWalkableNeighbours()
        {
            List<RHTile> rvList = new List<RHTile>();
            foreach (RHTile tile in GetAdjacentTiles())
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
        public List<RHTile> GetAdjacentTiles()
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
                    if (this.Y < MyMap().MapHeightTiles - 1)
                    {
                        rvTile = MyMap().GetTileByGridCoords(this.X, this.Y + 1);
                    }
                    break;
                case DirectionEnum.Left:
                    if (this.X > 0)
                    {
                        rvTile = MyMap().GetTileByGridCoords(this.X - 1, this.Y);
                    }
                    break;
                case DirectionEnum.Up:
                    if (this.Y > 0)
                    {
                        rvTile = MyMap().GetTileByGridCoords(this.X, this.Y - 1);
                    }
                    break;
                case DirectionEnum.Right:
                    if (this.X < MyMap().MapWidthTiles - 1)
                    {
                        rvTile = MyMap().GetTileByGridCoords(this.X + 1, this.Y);
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
            bool rv = _tileExists && (WorldObject == null || WorldObject.Walkable);
            if (_tileExists)
            {
                foreach (TiledMapTileLayer l in _diProps.Keys)
                {
                    if (l.IsVisible && !l.Name.Contains("Upper") && ContainsProperty(l, "Impassable", out string val) && val.Equals("true"))
                    {
                        rv = false;
                    }
                }
            }

            return rv;
        }

        public bool CanPlaceOnTabletop(WorldObject objToPlace)
        {
            bool rv = false;

            if (WorldObject != null && WorldObject.CompareType(ObjectTypeEnum.Decor) && WorldObject.CompareType(ObjectTypeEnum.Decor))
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
            //&& GetTravelPoint() == null
            return Passable() && WorldObject == null;
        }

        /// <summary>
        /// Determines if the RHTile can be walked through. 
        /// </summary>
        /// <returns>True if the RHTile can be walked through. Does not mean the RHTile
        /// can be assigned to if it returns true.</returns>
        public bool CanWalkThrough()
        {
            return CanTargetTile() && CanWalkThroughInCombat();
        }

        /// <summary>
        /// For use only during Combat to see if can path through.
        /// Characters can path through tiles occupied by an ally.
        /// </summary>
        /// <returns>True if not in combat, or character is null</returns>
        public bool CanWalkThroughInCombat()
        {
            bool rv = true;
            if (TacticalCombatManager.InCombat)
            {
                rv = Character == null || Character.IsSummon() || TacticalCombatManager.OnSameTeam(Character);
            }
            return rv;
        }
        #endregion
    }
}
