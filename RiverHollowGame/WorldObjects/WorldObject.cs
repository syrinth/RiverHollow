using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.GameManager;
using static RiverHollow.Utilities.Enums;
using System;
using static RiverHollow.Game_Managers.SaveManager;
using RiverHollow.GUIComponents.Screens;
using Newtonsoft.Json.Linq;
using System.Windows.Input;

namespace RiverHollow.WorldObjects
{
    public class WorldObject
    {
        #region Properties
        protected ObjectTypeEnum _eObjectType;
        public ObjectTypeEnum Type => _eObjectType;
        public AnimatedSprite Sprite { get; protected set; }

        public List<RHTile> Tiles;

        protected bool AssignedToTiles => Tiles.Count > 0;

        public string MapName { get; protected set; } = string.Empty;
        public RHMap CurrentMap => MapManager.Maps.ContainsKey(MapName) ? MapManager.Maps[MapName] : null;

        private bool _bMovable = false;
        private DirectionEnum _eShoveDirection = DirectionEnum.None;
        private DirectionEnum _ePullDirection = DirectionEnum.None;
        private bool _bMoveOnce = false;
        private bool _bHasMoved = false;

        protected bool _bWalkable = false;
        public bool Walkable => _bWalkable;
        protected ObjectPlacementEnum _ePlacement;

        protected KeyValuePair<int, int> _kvpDrop; //itemID, # of items dropped

        protected bool _bDrawUnder = false;

        protected Point _pImagePos;
        public Point PickupOffset { get; private set; }

        public Point MapPosition { get; protected set; }

        protected Point _pSize = new Point(1, 1);
        public int Width => _pSize.X * Constants.TILE_SIZE;
        public int Height => _pSize.Y * Constants.TILE_SIZE;

        public int BaseWidth => _rBase.Width;
        public int BaseHeight => _rBase.Height;

        protected List<LightInfo> _liLights;
        public IList<LightInfo> Lights => _liLights.AsReadOnly();

        protected Rectangle _rBase = new Rectangle(0, 0, 1, 1);

        //The ClickBox is always the Sprite itself
        public Rectangle ClickBox => new Rectangle(MapPosition, _pSize);

        //Base is always described in # of Tiles so we must multiply by the Constants.TILE_SIZE
        public Point CollisionPosition => CollisionBox.Location;
        public Rectangle CollisionBox => new Rectangle(MapPosition.X + (_rBase.X * Constants.TILE_SIZE), MapPosition.Y + (_rBase.Y * Constants.TILE_SIZE), (_rBase.Width * Constants.TILE_SIZE), (_rBase.Height * Constants.TILE_SIZE));
        public Point CollisionCenter => CollisionBox.Center;

        public int ID { get; protected set; }
        #endregion

        public virtual string Name()
        {
            return DataManager.GetTextData(ID, "Name", DataType.WorldObject);
        }

        protected WorldObject(int id)
        {
            Tiles = new List<RHTile>();

            ID = id;
        }

        public WorldObject(int id, Dictionary<string, string> stringData) : this(id)
        {
            LoadDictionaryData(stringData);
        }

        protected virtual void LoadDictionaryData(Dictionary<string, string> stringData, bool loadSprite = true)
        {
            string[] split = Util.FindParams(stringData["Image"]);
            if (split.Length == 1)
            {
                string[] splitVal = split[0].Split('-');
                _pImagePos = new Point(int.Parse(splitVal[0]), int.Parse(splitVal[1]));
            }

            Util.AssignValue(ref _pSize, "Size", stringData);

            Point baseOffset = Point.Zero;
            Util.AssignValue(ref baseOffset, "BaseOffset", stringData);

            Point baseSize = new Point(1, 1);
            Util.AssignValue(ref baseSize, "Base", stringData);

            _rBase = new Rectangle(baseOffset, baseSize);

            Util.AssignValue(ref _eObjectType, "Type", stringData);
            Util.AssignValue(ref _ePlacement, "Placement", stringData);

            Util.AssignValue(ref _bMovable, "Movable", stringData);
            Util.AssignValue(ref _bMoveOnce, "MoveOnce", stringData);
            Util.AssignValue(ref _eShoveDirection, "ShoveDirection", stringData);

            if (stringData.ContainsKey("LightID"))
            {
                _liLights = new List<LightInfo>();

                foreach (string s in Util.FindParams(stringData["LightID"]))
                {
                    split = s.Split('-');

                    LightInfo info;
                    info.LightObject = DataManager.GetLight(int.Parse(split[0]));
                    info.Offset = new Point(int.Parse(split[1]), int.Parse(split[2]));

                    SyncLightPositions();
                    _liLights.Add(info);
                }
            }

            if (loadSprite)
            {
                if (stringData.ContainsKey("Texture")) { LoadSprite(stringData, stringData["Texture"]); }
                else { LoadSprite(stringData); }
            }
        }

        protected virtual void LoadSprite(Dictionary<string, string> stringData, string textureName = DataManager.FILE_WORLDOBJECTS)
        {
            Sprite = new AnimatedSprite(textureName);
            if (stringData.ContainsKey("Idle"))
            {
                string[] idleSplit = stringData["Idle"].Split('-');
                Sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _pSize, int.Parse(idleSplit[0]), float.Parse(idleSplit[1]));
            }
            else
            {
                Sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _pSize);
            }

            //MAR
            //if (stringData.ContainsKey("Gathered"))
            //{
            //    string[] gatherSplit = stringData["Gathered"].Split('-');
            //    _sprite.AddAnimation(WorldObjAnimEnum.Gathered, startX, startY, _iWidth, _iHeight, int.Parse(gatherSplit[0]), float.Parse(gatherSplit[1]));
            //}
            SetSpritePos(MapPosition);
        }

        public virtual void Update(GameTime gTime) {
            Sprite.Update(gTime);
            SyncLightPositions();
            if (_liLights != null)
            {
                foreach (LightInfo info in _liLights)
                {
                    info.LightObject.Update(gTime);
                }
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (_bDrawUnder) { Sprite.Draw(spriteBatch, 1); }
            else {
                float alpha = 1f;
                if(((BaseHeight + 1) * Constants.TILE_SIZE < Height) && new Rectangle(Sprite.Position.X, Sprite.Position.Y, Sprite.Width, Sprite.Height).Contains(PlayerManager.PlayerActor.CollisionCenter))
                {
                    alpha = 0.7f;
                }
                Sprite.Draw(spriteBatch, true, alpha);
            }
            if (Constants.DRAW_COLLISION)
            {
                spriteBatch.Draw(DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE), CollisionBox, new Rectangle(160, 128, 2, 2), Color.White * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, Sprite.LayerDepth - 1);
            }
        }
        public void DrawItem(SpriteBatch spriteBatch, MapItem i)
        {
            i.Draw(spriteBatch, Sprite.LayerDepth + 1);
        }

        public virtual bool ProcessLeftClick() { return false; }
        public virtual bool ProcessRightClick() {
            bool rv = false;
            if (DataManager.GetBoolByIDKey(ID, "OpenStock", DataType.WorldObject) && GameManager.CurrentBuilding != null){
                rv = true;
                GUIManager.OpenMainObject(new HUDInventoryDisplay(TownManager.Inventory, DisplayTypeEnum.Inventory));
            }
            return rv;
        }

        public virtual void Rollover() { }

        public virtual bool IntersectsWith(Rectangle r)
        {
            return CollisionBox.Intersects(r);
        }

        public bool WideOnTop()
        {
            return BaseWidth < Width / Constants.TILE_SIZE;
        }

        public virtual bool Contains(Point m)
        {
            return CollisionBox.Contains(m);
        }

        public virtual bool PlaceOnMap(RHMap map, bool ignoreActors = false)
        {
            bool rv = PlaceOnMap(this.MapPosition, map, ignoreActors);
            map.AddLights(GetLights());
            SyncLightPositions();
            return rv;
        }

        public virtual bool PlaceOnMap(Point pos, RHMap map, bool ignoreActors = false)
        {
            pos = new Point(pos.X - (_rBase.X * Constants.TILE_SIZE), pos.Y - (_rBase.Y * Constants.TILE_SIZE));
            SnapPositionToGrid(pos);
            bool rv = map.PlaceWorldObject(this, ignoreActors);
            if (rv)
            {
                MapName = map.Name;
            }

            return rv;
        }

        protected void SetSpritePos(Point position)
        {
            if (Sprite != null)
            {
                Sprite.Position = position;
            }
        }

        public virtual void SnapPositionToGrid(Point position)
        {
            MapPosition = Util.SnapToGrid(position);
            SetSpritePos(MapPosition);
        }

        /// <summary>
        /// If the given RHTile is not present in the list of Tiles, add it
        /// </summary>
        /// <param name="t">The Tile to add to the list of known RHTiles</param>
        public void AddTile(RHTile t)
        {
            Util.AddUniquelyToList(ref Tiles, t);
        }

        /// <summary>
        /// Removes the object from the Tiles this Object sits upon
        /// then clears the Tile list that belongs to the WorldObject
        /// </summary>
        public virtual void RemoveSelfFromTiles()
        {
            foreach (RHTile t in Tiles)
            {
                if (t.Flooring == this) { t.RemoveFlooring(); }
                if (t.WorldObject == this) { t.RemoveWorldObject(); }
                if (t.ShadowObject == this) { t.RemoveShadowObject(); }
            }

            Tiles.Clear();
        }

        /// <summary>
        /// Sets the offset of the mouse based on the 
        /// </summary>
        /// <param name="mousePosition">The current mousePosition</param>
        public void SetPickupOffset(Vector2 mousePosition)
        {
            int xOffset = (Width > Constants.TILE_SIZE) ? (int)(mousePosition.X - Sprite.Position.X) : 0;
            int yOffset = (Height > Constants.TILE_SIZE) ? (int)(mousePosition.Y - Sprite.Position.Y) : 0;

            xOffset = (xOffset / Constants.TILE_SIZE) * Constants.TILE_SIZE;
            yOffset = (yOffset / Constants.TILE_SIZE) * Constants.TILE_SIZE;
            PickupOffset = new Point(xOffset, yOffset);
            
        }

        /// <summary>
        /// Sets the default PickupOffset if the Width of height
        /// is greater than a single RHTile
        /// </summary>
        public void SetPickupOffset()
        {
            int xOffset = (_rBase.Width > 1) ? (_rBase.Width - 1) / 2 : 0;
            int yOffset = (_rBase.Height > 1) ? (_rBase.Height -1) / 2 : 0;
            PickupOffset = new Point((_rBase.X + xOffset) * Constants.TILE_SIZE, (_rBase.Y + yOffset) * Constants.TILE_SIZE);
            PickupOffset = Util.MultiplyPoint(Util.DividePoint(PickupOffset, Constants.TILE_SIZE),  Constants.TILE_SIZE);
        }

        public List<Item> GetDroppedItems()
        {
            List<Item> itemList = new List<Item>();
            for (int i = 0; i < _kvpDrop.Value; i++)
            {
                itemList.Add(DataManager.GetItem(_kvpDrop.Key, 1));
            }

            return itemList;
        }

        public virtual List<Light> GetLights()
        {
            List<Light> lights = null;
            if (_liLights != null)
            {
                lights = new List<Light>();
                foreach (LightInfo info in _liLights)
                {
                    lights.Add(info.LightObject);
                }
            }

            return lights;
        }
        public virtual void SyncLightPositions()
        {
            if (_liLights != null)
            {
                foreach (LightInfo info in _liLights)
                {
                    info.LightObject.Position = new Point(MapPosition.X - info.LightObject.Width / 2, MapPosition.Y - info.LightObject.Height / 2);
                    info.LightObject.Position += info.Offset;
                }
            }
        }

        public bool CompareType(ObjectTypeEnum t) { return Type == t; }
        public bool IsDestructible() { return CompareType(ObjectTypeEnum.Destructible) || CompareType(ObjectTypeEnum.Plant); }
        public bool IsBuildable()
        {
            bool rv = false;
            switch (_eObjectType)
            {
                case ObjectTypeEnum.Beehive:
                case ObjectTypeEnum.Building:
                case ObjectTypeEnum.Buildable:
                case ObjectTypeEnum.Container:
                case ObjectTypeEnum.Decor:
                case ObjectTypeEnum.Floor:
                case ObjectTypeEnum.Garden:
                case ObjectTypeEnum.Mailbox:
                case ObjectTypeEnum.Structure:
                case ObjectTypeEnum.Wall:
                    rv = true;
                    break;
            }

            return rv;
        }

        public bool WallObject() { return _ePlacement == ObjectPlacementEnum.Wall; }
        public bool FlooringObject() { return _ePlacement == ObjectPlacementEnum.Floor; }

        public virtual bool CanPickUp() { return false; }

        public void InitiateMove(Vector2 newMovement)
        {
            DirectionEnum moveDir = Util.GetDirection(newMovement);
            bool canMove = _bMovable && (!_bMoveOnce || (_bMoveOnce && !_bHasMoved));
            bool goingBackwards = moveDir == Util.GetOppositeDirection(PlayerManager.PlayerActor.Facing);
            bool shoveMatches = !goingBackwards && _eShoveDirection == PlayerManager.PlayerActor.Facing;
            bool pullMatches = goingBackwards && _ePullDirection == Util.GetOppositeDirection(PlayerManager.PlayerActor.Facing);

            if (_eShoveDirection == DirectionEnum.None || shoveMatches || pullMatches)
            {
                if (canMove && (newMovement != Vector2.Zero && moveDir == PlayerManager.PlayerActor.Facing || goingBackwards))
                {
                    RHTile nextObjectTile = CurrentMap.GetTileByPixelPosition(CollisionPosition);
                    RHTile checkTile = nextObjectTile;
                    do
                    {
                        checkTile = checkTile.GetTileByDirection(moveDir);
                    } while (checkTile.WorldObject == this);
                    RHTile nextPlayerTile = MapManager.CurrentMap.GetTileByPixelPosition(PlayerManager.PlayerActor.CollisionCenter).GetTileByDirection(moveDir);

                    if (checkTile == null) { return; }
                    else if (goingBackwards && nextPlayerTile == null) { return; }

                    if ((!goingBackwards && checkTile.Passable()) || (goingBackwards && (nextPlayerTile.WorldObject == this || nextPlayerTile.Passable())))
                    {
                        _bHasMoved = true;
                        PlayerManager.HandleGrabMovement(nextObjectTile.GetTileByDirection(moveDir), nextPlayerTile);
                    }
                }
            }
        }
        public void MoveObject(Point direction)
        {
            MapPosition += direction;
            Sprite.Position += direction;
        }

        #region Save Handlers
        public virtual WorldObjectData SaveData()
        {
            WorldObjectData data = new WorldObjectData
            {
                ID = ID,
                X = CollisionBox.X,
                Y = CollisionBox.Y
            };

            return data;
        }
        public virtual void LoadData(WorldObjectData data)
        {
            SnapPositionToGrid(new Point(data.X, data.Y));
        }
        #endregion
    }
}
