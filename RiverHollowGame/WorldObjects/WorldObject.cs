using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;
using static RiverHollow.Game_Managers.SaveManager;
using RiverHollow.GUIComponents.Screens;
using MonoGame.Extended.BitmapFonts;
using MonoGame.Extended;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.GUIComponents.GUIObjects;
using static RiverHollow.GUIComponents.GUIObjects.GUIObject;
using RiverHollow.GUIComponents;

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

        public bool PlayerCanEdit => !ShopItem && CompareType(ObjectTypeEnum.Buildable);
        public bool ShopItem { get; protected set; } = false;

        protected bool _bWalkable = false;
        protected bool _bSelected = false;

        public bool Walkable => _bWalkable;

        protected ObjectPlacementEnum _ePlacement;
        protected bool _bDrawUnder = false;

        protected Point _pImagePos;
        public Point PickupOffset { get; private set; }

        public Point MapPosition { get; protected set; }

        protected Point _pSize;
        public int Width => _pSize.X * Constants.TILE_SIZE;
        public int Height => _pSize.Y * Constants.TILE_SIZE;

        public int BaseWidth => _rBase.Width;
        public int BaseHeight => _rBase.Height;

        protected List<LightInfo> _liLights;
        public IList<LightInfo> Lights => _liLights.AsReadOnly();

        protected Rectangle _rBase = new Rectangle(0, 0, 1, 1);
        protected Point _pSpriteOffset = new Point(0,0);

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
            return GetTextData("Name");
        }
        public virtual string Description()
        {
            return Name() + System.Environment.NewLine + GetTextData("Description");
        }

        public bool Reset { get; protected set; } = false;
        public bool Movable { get; private set; }
        public bool MoveOnce { get; private set; }
        public DirectionEnum ShoveDirection { get; private set; } = DirectionEnum.None;
        public DirectionEnum PullDirection { get; private set; } = DirectionEnum.None;
        private bool _bHasMoved = false;

        virtual public WorldObject Pickup => this;

        public WorldObject(int id)
        {
            Tiles = new List<RHTile>();

            ID = id;

            string[] split = GetStringParamsByIDKey("Image");
            if (split.Length == 1)
            {
                string[] splitVal = Util.FindArguments(split[0]);
                _pImagePos = new Point(int.Parse(splitVal[0]), int.Parse(splitVal[1]));
                _pImagePos = Util.MultiplyPoint(_pImagePos, Constants.TILE_SIZE);
            }

            _eObjectType = GetEnumByIDKey<ObjectTypeEnum>("Type");

            _pSize = GetPointByIDKey("Size", new Point(1, 1));
            _rBase = GetRectangleByIDKey("Base", new Rectangle(0, 0, 1, 1));

            _ePlacement = GetEnumByIDKey<ObjectPlacementEnum>("Placement");

            if (GetBoolByIDKey("LightID"))
            {
                _liLights = new List<LightInfo>();

                foreach (string s in Util.FindParams(GetStringByIDKey("LightID")))
                {
                    split = Util.FindArguments(s);

                    LightInfo info;
                    info.LightObject = DataManager.GetLight(int.Parse(split[0]));
                    info.Offset = new Point(int.Parse(split[1]), int.Parse(split[2]));

                    SyncLightPositions();
                    _liLights.Add(info);
                }
            }

            if (GetBoolByIDKey("Movable"))
            {
                Movable = true;
            }

            if (GetBoolByIDKey("Walkable"))
            {
                _bWalkable = true;
            }

            LoadSprite();
        }

        public WorldObject(int id, Dictionary<string,string> args) : this(id)
        {
            if (args != null)
            {
                if (args.ContainsKey("Reset"))
                {
                    Reset = true;
                }
                if (args.ContainsKey("Movable"))
                {
                    Movable = true;
                }
                if (args.ContainsKey("MoveOnce"))
                {
                    MoveOnce = true;
                }
                if (args.ContainsKey("ShoveDirection"))
                {
                    ShoveDirection = Util.ParseEnum<DirectionEnum>(args["ShoveDirection"]);
                }
                if (args.ContainsKey("PullDirection"))
                {
                    PullDirection = Util.ParseEnum<DirectionEnum>(args["PullDirection"]);
                }
            }
        }

        protected virtual void LoadSprite()
        {
            if (GetBoolByIDKey("Texture"))
            {
                LoadSprite(DataManager.FOLDER_WORLDOBJECTS + GetStringByIDKey("Texture"));
            }
            else
            {
                LoadSprite(DataManager.FILE_WORLDOBJECTS);
            }
        }
        protected virtual void LoadSprite(string texture)
        {
            Sprite = new AnimatedSprite(texture)
            {
                UseXPosition = Walkable
            };

            if (GetBoolByIDKey("Idle"))
            {
                string[] idleSplit = Util.FindArguments(GetStringByIDKey("Idle"));
                Sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _pSize, int.Parse(idleSplit[0]), float.Parse(idleSplit[1]));
            }
            else
            {
                Sprite.AddAnimation(AnimationEnum.ObjectIdle, _pImagePos.X, _pImagePos.Y, _pSize);
            }

            if (GetBoolByIDKey("SpriteOffset"))
            {
                Sprite.TrimBy(GetIntByIDKey("SpriteOffset", 0));
            }

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
            Sprite.SetColor(_bSelected ? Color.Green : Color.White);

            if (_bDrawUnder) { Sprite.Draw(spriteBatch, 1); }
            else {
                float alpha = 1f;

                var spriteRectangle = new Rectangle(Sprite.Position.X, Sprite.Position.Y, Sprite.Width, Sprite.Height);
                if (!GetBoolByIDKey("NoAlpha") && ((BaseHeight + 1) * Constants.TILE_SIZE < Height) && spriteRectangle.Contains(PlayerManager.PlayerActor.CollisionCenter) && PlayerManager.PlayerActor.CollisionBox.Bottom <= CollisionBox.Top)
                {
                    alpha = 0.9f;
                }
                Sprite.Draw(spriteBatch, true, alpha);
            }
            if (Constants.DRAW_COLLISION)
            {
                spriteBatch.Draw(DataManager.GetTexture(DataManager.HUD_COMPONENTS), CollisionBox, GUIUtils.BLACK_BOX, Color.White * 0.5f, 0f, Vector2.Zero, SpriteEffects.None, Sprite.LayerDepth - 1);
            }

            if (ShopItem && !GameManager.GamePaused() && !CutsceneManager.Playing && Sprite.SpriteRectangle.Contains(GUICursor.GetWorldMousePosition()))
            {
                int value = GetIntByIDKey("Value");

                BitmapFont font = DataManager.GetBitMapFont(@"Fonts\FontBattle");
                Size2 size = font.MeasureString(value.ToString());
                int delta = (int)size.Width - Sprite.SpriteRectangle.Width;
                spriteBatch.DrawString(font, value.ToString(), Sprite.Position.ToVector2() + new Vector2(-delta / 2, -8), Color.White, Constants.MAX_LAYER_DEPTH);

                if (!GUIManager.IsHoverWindowOpen())
                {
                    var win = new GUIItemDescriptionWindow(this, Point.Zero);
                    win.AnchorToScreen(SideEnum.BottomRight);
                    GUIManager.OpenHoverObject(win, Sprite.SpriteRectangle, false);
                }
            }
        }
        public void DrawItem(SpriteBatch spriteBatch, MapItem i)
        {
            i.Draw(spriteBatch, Sprite.LayerDepth + 1);
        }

        public virtual bool ProcessLeftClick() { return false; }
        public virtual bool ProcessRightClick()
        {
            bool rv = false;

            if (GetBoolByIDKey("OpenStock") && GameManager.CurrentBuilding != null)
            {
                rv = true;
                GUIManager.OpenMainObject(new HUDInventoryDisplay(TownManager.Inventory, DisplayTypeEnum.Inventory));
            }

            if (GetBoolByIDKey("Bed"))
            {
                rv = true;
                GUIManager.OpenTextWindow("Selection_Bed");
            }

            return rv;
        }

        public virtual void SelectObject(bool val)
        {
            _bSelected = val;
        }

        public virtual bool HasTileInRange()
        {
            bool rv = false;

            foreach (var tile in Tiles)
            {
                if (PlayerManager.InRangeOfPlayer(tile.CollisionBox))
                {
                    rv = true;
                    break;
                }
            }

            return rv;
        }

        public virtual void Rollover()
        {
            if (Reset)
            {
                CurrentMap.RemoveWorldObject(this);
            }
        }

        public virtual bool IntersectsWith(Rectangle r)
        {
            return CollisionBox.Intersects(r);
        }

        public virtual bool WideOnTop()
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
            if (rv)
            {
                map.AddLights(GetLights());
                SyncLightPositions();
            }
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
                Sprite.Position = position - _pSpriteOffset;
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

        public void AddToInventory()
        {
            if (CompareType(ObjectTypeEnum.Buildable))
            {

                Item displayItem = DataManager.GetItem((Buildable)this);
                if (InventoryManager.HasSpaceInInventory(displayItem.ID, 1))
                {
                    InventoryManager.AddToInventory(displayItem);
                }
            }
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

        public bool BuildableType(BuildableEnum t)
        {
            return CompareType(ObjectTypeEnum.Buildable) && GetEnumByIDKey<BuildableEnum>("Subtype") == t;
        }

        public bool WallObject() { return _ePlacement == ObjectPlacementEnum.Wall; }
        public bool FlooringObject() { return _ePlacement == ObjectPlacementEnum.Floor; }

        public void SetShopItem()
        {
            ShopItem = true;
        }

        public virtual bool CanPickUp() { return false; }

        public void InitiateMove(Vector2 newMovement)
        {
            DirectionEnum moveDir = Util.GetDirection(newMovement);
            bool canMove = Movable && (!MoveOnce || (MoveOnce && !_bHasMoved));
            bool goingBackwards = moveDir == Util.GetOppositeDirection(PlayerManager.PlayerActor.Facing);
            bool shoveMatches = !goingBackwards && ShoveDirection == PlayerManager.PlayerActor.Facing;
            bool pullMatches = goingBackwards && PullDirection == Util.GetOppositeDirection(PlayerManager.PlayerActor.Facing);

            if (ShoveDirection == DirectionEnum.None || shoveMatches || pullMatches)
            {
                if (canMove && (newMovement != Vector2.Zero && moveDir == PlayerManager.PlayerActor.Facing || goingBackwards))
                {
                    //Confirm the way is clear for the player
                    var facingRect = PlayerManager.GetAdjacencyRectangle(PlayerManager.PlayerActor.Facing);
                    var tiles = MapManager.CurrentMap.GetTilesFromRectangleExcludeEdgePoints(facingRect);

                    bool abort = false;
                    foreach(var t in tiles)
                    {
                        if(t.WorldObject != null && t.WorldObject != this && !t.WorldObject.Walkable)
                        {
                            abort = true;
                        }
                    }

                    if (!abort)
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
        }
        public void MoveObject(Point direction)
        {
            MapPosition += direction;
            Sprite.Position += direction;
        }

        #region Lookup Handlers
        public string GetTextData(string key)
        {
            return DataManager.GetTextData(ID, key, DataType.WorldObject);
        }
        public bool GetBoolByIDKey(string key)
        {
            return DataManager.GetBoolByIDKey(ID, key, DataType.WorldObject);
        }
        public int GetIntByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetIntByIDKey(ID, key, DataType.WorldObject, defaultValue);
        }
        public Dictionary<int, int> GetIntDictionaryByIDKey(string key)
        {
            return DataManager.IntDictionaryFromLookup(ID, key, DataType.WorldObject);
        }
        public float GetFloatByIDKey(string key, int defaultValue = -1)
        {
            return DataManager.GetFloatByIDKey(ID, key, DataType.WorldObject, defaultValue);
        }
        public string GetStringByIDKey(string key, string defaultValue = "")
        {
            return DataManager.GetStringByIDKey(ID, key, DataType.WorldObject, defaultValue);
        }
        public string[] GetStringArgsByIDKey(string key, string defaultValue = "")
        {
            return DataManager.GetStringArgsByIDKey(ID, key, DataType.WorldObject, defaultValue);
        }
        public string[] GetStringParamsByIDKey(string key, string defaultValue = "")
        {
            return DataManager.GetStringParamsByIDKey(ID, key, DataType.WorldObject, defaultValue);
        }

        public TEnum GetEnumByIDKey<TEnum>(string key) where TEnum : struct
        {
            return DataManager.GetEnumByIDKey<TEnum>(ID, key, DataType.WorldObject);
        }
        protected Point GetPointByIDKey(string key, Point defaultValue = default)
        {
            return DataManager.GetPointByIDKey(ID, key, DataType.WorldObject, defaultValue);
        }
        protected Rectangle GetRectangleByIDKey(string key, Rectangle defaultValue = default)
        {
            return DataManager.GetRectangleByIDKey(ID, key, DataType.WorldObject, defaultValue);
        }
        #endregion

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
