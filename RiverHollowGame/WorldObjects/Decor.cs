using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.GUIComponents.GUIObjects;
using RiverHollow.GUIComponents.Screens;
using RiverHollow.Items;
using RiverHollow.SpriteAnimations;
using RiverHollow.Map_Handling;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Game_Managers.SaveManager;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Decor : Buildable
    {
        protected enum RotationalEnum { None, FourWay, TwoWay };
        protected RotationalEnum RotationType => GetEnumByIDKey<RotationalEnum>("Rotation");

        public DirectionEnum Facing { get; private set; } = DirectionEnum.Down;

        protected Point _pDisplayOffset;
        protected Point _pRotatedDisplayOffset;

        protected Point _pRotationOffset;
        protected Point _pRotationSize;

        public bool CanDisplay => GetBoolByIDKey("Display");
        public bool CanBeDisplayed => GetBoolByIDKey("CanBeDisplayed");

        private Item _itemDisplay;
        private Decor _objDisplay;
        private readonly List<Item> _liMerchIDs;

        public bool HasDisplay => _objDisplay != null || _itemDisplay != null;
        public bool Archive => GetBoolByIDKey("Archive");

        public Decor(int id, Dictionary<string, string> args) : base(id)
        {
            _pRotationOffset = GetPointByIDKey("RotationBaseOffset");
            _pRotationSize = GetPointByIDKey("RotationSize");

            _pDisplayOffset = GetPointByIDKey("DisplayOffset");
            _pRotatedDisplayOffset = GetPointByIDKey("RotatedDisplayOffset");

            if (CanDisplay)
            {
                if (args != null && args.ContainsKey("ItemID"))
                {
                    string[] holdSplit = Util.FindParams(args["ItemID"]);
                    var itemStr = Util.FindArguments(holdSplit[0]);

                    int ID = int.Parse(itemStr[0]);
                    int number = itemStr.Length > 1 ? int.Parse(itemStr[1]) : 1;

                    _itemDisplay = DataManager.GetItem(ID, number);
                    _itemDisplay.DrawShadow(true);
                }
            }

            if (GetBoolByIDKey("ShopTable"))
            {
                _liMerchIDs = new List<Item>();
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_objDisplay != null)
            {
                _objDisplay.Sprite.SetColor(Selected ? Color.Green : Color.White);
                _objDisplay.Sprite.Draw(spriteBatch, Sprite.LayerDepth + 1);
            }
            else if (_itemDisplay != null)
            {
                //Because Items don't exist directly on the map, we only need to tell it where to draw itself here
                _itemDisplay.SetColor(Selected ? Color.Green : Color.White);
                _itemDisplay.Draw(spriteBatch, new Rectangle(MapPosition.X + _pDisplayOffset.X, MapPosition.Y + _pDisplayOffset.Y, Constants.TILE_SIZE, Constants.TILE_SIZE), Sprite.LayerDepth + 1);
            }

            if (_liMerchIDs != null && _liMerchIDs.Count > 0 && GetBoolByIDKey("ShopTable"))
            {
                var strData = GetStringParamsByIDKey("ShopTable");
                var point = Util.ParsePoint(strData[0]);
                var offset = int.Parse(strData[1]);
                var spots = int.Parse(strData[2]);
                for (int i = 0; i < spots && i < _liMerchIDs.Count; i++)
                {
                    _liMerchIDs[i]?.DrawShadow(true);
                    _liMerchIDs[i]?.Draw(spriteBatch, new Rectangle(MapPosition.X + point.X, MapPosition.Y + point.Y, Constants.TILE_SIZE, Constants.TILE_SIZE), Sprite.LayerDepth + 1);

                    point.X += offset;
                }
            }
        }

        /// <summary>
        /// Handler for when a Decor object hasbeen right-clicked
        /// </summary>
        public override bool ProcessRightClick()
        {
            bool rv = false;

            //Currently, only display Decor objects can be interacted with.
            if (CanDisplay)
            {
                rv = true;
                if (!Archive || _itemDisplay == null)
                {
                    GameManager.SetSelectedWorldObject(this);

                    var display = new Item[1, 1];
                    display[0, 0] = _itemDisplay;

                    InventoryManager.ExtraHoldSingular = true;
                    GUIManager.OpenMainObject(new HUDInventoryDisplay(display, DisplayTypeEnum.Inventory));
                }
            }

            return rv;
        }

        /// <summary>
        /// When snapping, we need to call SyncDisplayObject to make sure it
        /// matches the new position
        /// </summary>
        /// <param name="position">The position to snap to.</param>
        public override void SnapPositionToGrid(Point position)
        {
            base.SnapPositionToGrid(position);
            SyncDisplayObject();
        }

        /// <summary>
        /// This override handles the situation where we are attempting to place a decor object
        /// on top of another one.
        /// 
        /// If we are not attempting to make a valid display placement, call the vase PlaceOnMap method
        /// and then sync any display object we may have.
        /// </summary>
        /// <param name="map">The map to place the object on</param>
        /// <returns></returns>
        public override bool PlaceOnMap(RHMap map, bool ignoreActors = false)
        {
            RHTile tile = map.GetTileByPixelPosition(MapPosition);

            bool rv;
            if (tile != null && tile.CanPlaceOnTabletop(this))
            {
                rv = ((Decor)tile.WorldObject).SetDisplayObject(this);
            }
            else
            {
                rv = base.PlaceOnMap(map);
                if (rv)
                {
                    SyncDisplayObject();
                }
            }

            return rv;
        }

        /// <summary>
        /// Assuming the object is capable of rotation, this method does the math
        /// required to change the sprite and base tiles accordingly.
        /// </summary>
        public void Rotate(bool forward = true)
        {
            if (RotationType != RotationalEnum.None)
            {
                //We don't need to do any swaps if the object has the same base and height
                if (_rBase.Width != _rBase.Height)
                {
                    (_pRotatedDisplayOffset, _pDisplayOffset) = (_pDisplayOffset, _pRotatedDisplayOffset);
                    Util.SwitchValues(ref _rBase.Width, ref _rBase.Height);
                    Util.SwitchValues(ref _pSize, ref _pRotationSize);
                    Util.SwitchValues(ref _rBase.X, ref _pRotationOffset.X);
                    Util.SwitchValues(ref _rBase.Y, ref _pRotationOffset.Y);
                }

                Rectangle spriteFrameRectangle = Sprite.CurrentFrameAnimation.FrameRectangle;

                Point crawl = new Point(spriteFrameRectangle.Width, 0);
                if (Facing == DirectionEnum.Up)
                {
                    crawl = new Point(_rBase.Width * Constants.TILE_SIZE * -1, 0);
                }

                Point newImage = spriteFrameRectangle.Location + crawl;

                //Direction handling for the different rotation types
                if (RotationType == RotationalEnum.FourWay)
                {
                    switch (Facing)
                    {
                        case DirectionEnum.Down:
                            if (forward) { Facing = DirectionEnum.Right; }
                            else { Facing = DirectionEnum.Left; }
                            break;
                        case DirectionEnum.Right:
                            if (forward) { Facing = DirectionEnum.Up; }
                            else
                            {
                                newImage = _pImagePos;
                                Facing = DirectionEnum.Down;
                            }
                            break;
                        case DirectionEnum.Up:
                            if (forward) { Facing = DirectionEnum.Left; }
                            else { Facing = DirectionEnum.Right; }
                            break;
                        case DirectionEnum.Left:
                            if (forward)
                            {
                                newImage = _pImagePos;
                                Facing = DirectionEnum.Down;
                            }
                            else { Facing = DirectionEnum.Up; }
                            break;
                    }
                }
                else if (RotationType == RotationalEnum.TwoWay)
                {
                    switch (Facing)
                    {
                        case DirectionEnum.Down:
                            Facing = DirectionEnum.Right;
                            break;
                        case DirectionEnum.Right:
                            newImage = _pImagePos;
                            Facing = DirectionEnum.Down;
                            break;
                    }
                }

                //Updates the sprite info
                Sprite = new AnimatedSprite(DataManager.FILE_DECOR);
                Sprite.AddAnimation(AnimationEnum.ObjectIdle, newImage.X, newImage.Y, _pSize);

                if (RotationType == RotationalEnum.FourWay && Facing == DirectionEnum.Left)
                {
                    Sprite.CurrentFrameAnimation.Flip = true;
                }

                SetSpritePos(MapPosition);

                //Sets the pickup offset to the center of the object.
                SetPickupOffset();

                //Important to not have a flicker before the game asserts where the obeject's new location is
                GUICursor.UpdateTownObjectLocation();
            }
        }

        /// <summary>
        /// Helper methods for loading data. Just keeps rotating in sequence
        /// until we get to the appropriate Facing direction.
        /// </summary>
        /// <param name="dir"></param>
        public void RotateToDirection(DirectionEnum dir) { RotateToDirection((int)dir); }
        private void RotateToDirection(int dir)
        {
            for (int i = 1; i < dir; i++)
            {
                Rotate();
            }
        }

        /// <summary>
        /// Handler for removing, and not swapping out, the display entity.
        /// If we're in PutAway mode, destroy the display object. Otherwise, send
        /// the entity to storage
        /// </summary>
        public void RemoveDisplayEntity()
        {
            if (_objDisplay != null)
            {
                _objDisplay.AddToInventory();
                _objDisplay = null;
            }
            else if (_itemDisplay != null)
            {
                if (InventoryManager.HasSpaceInInventory(_itemDisplay.ID, 1))
                {
                    InventoryManager.AddToInventory(_itemDisplay);
                    _itemDisplay = null;
                }
            }
        }

        /// <summary>
        /// Sets the display Decor object, swaps out any pre-existing display entity
        /// for the given Decor.
        /// </summary>
        /// <param name="obj">The Decor object to display</param>
        public bool SetDisplayObject(Decor obj)
        {
            bool rv = false;
            if (StoreDisplayEntity())
            {
                rv = true;
                _objDisplay = obj;
                SyncDisplayObject();
            }

            return rv;
        }

        /// <summary>
        /// Sets the display Item, swaps out any pre-existing display entity
        /// for the given item.
        /// </summary>
        /// <param name="it">The Item object to display</param>
        public bool SetDisplayEntity(Item it, bool viaBuildMode = true)
        {
            bool rv = false;

            if (!Archive || (it != null && !TownManager.DIArchive[it.ID].Archived))
            {
                if (StoreDisplayEntity(viaBuildMode))
                {
                    if (it != null)
                    {
                        rv = true;

                        if (it.ID > Constants.BUILDABLE_ID_OFFSET)
                        {
                            _objDisplay = (Decor)DataManager.CreateWorldObjectByID(it.ID);
                            SyncDisplayObject();
                        }
                        else
                        {
                            _itemDisplay = DataManager.GetItem(it.ID);
                            _itemDisplay.DrawShadow(true);
                        }

                        if (viaBuildMode)
                        {
                            InventoryManager.RemoveItemsFromInventory(it.ID, 1);
                        }
                    }
                }
            }

            return rv;
        }

        /// <summary>
        /// This method sends any display entity to the appropriate storage 
        /// and blanks out its reference on the Displaying Decor.
        /// 
        /// Only store entity if there is space in storage
        /// </summary>
        /// <returns>True as long as there is space in storage</returns>
        public bool StoreDisplayEntity(bool viaGUIInventory = true)
        {
            bool rv = true;
            Item displayItem = null;

            if (_itemDisplay != null) { displayItem = _itemDisplay; }
            else if (_objDisplay != null) { displayItem = DataManager.GetItem(_objDisplay); }

            if (displayItem != null)
            {
                if (!viaGUIInventory || InventoryManager.HasSpaceInInventory(displayItem.ID, 1))
                {
                    if (viaGUIInventory)
                    {
                        InventoryManager.AddToInventory(displayItem);
                    }
                    _itemDisplay = null;
                    _objDisplay = null;
                }
                else
                {
                    rv = false;
                }
            }

            return rv;
        }

        /// <summary>
        /// This method ensures that the DisplayObject's location is always synced up relative to the Decor object it's placed on.
        /// </summary>
        private void SyncDisplayObject()
        {
            if (_objDisplay != null)
            {
                _objDisplay.SnapPositionToGrid(new Point(MapPosition.X, MapPosition.Y - (_objDisplay.Sprite.Height - Constants.TILE_SIZE)));
                _objDisplay.MapPosition += _pDisplayOffset;
                _objDisplay.SetSpritePos(_objDisplay.MapPosition);
            }
        }

        public int MaxMerchandise()
        {
            int rv = 0;
            if (GetBoolByIDKey("ShopTable"))
            {
                var strData = GetStringParamsByIDKey("ShopTable");
                rv = int.Parse(strData[2]);
            }
            return rv;
        }

        public bool MerchandiseSpaceLeft()
        {
            bool rv = false;
            if (GetBoolByIDKey("ShopTable"))
            {
                var strData = GetStringParamsByIDKey("ShopTable");
                rv = _liMerchIDs.Count + 1< int.Parse(strData[2]);
            }
            return rv;
        }
        public void AddMerchandiseItem(Item i)
        {
            if (i != null)
            {
                _liMerchIDs.Add(i);
            }
        }

        public void ClearMerchandise()
        {
            _liMerchIDs.Clear();
        }

        public override WorldObjectData SaveData()
        {
            //Need to save null items
            WorldObjectData data = base.SaveData();
            string objDisplayStr = _objDisplay == null ? "" : _objDisplay.ID.ToString();
            string itemDisplayStr = _itemDisplay == null ? "" : _itemDisplay.ID.ToString();
            data.stringData = string.Format("{0}/{1}/{2}", (int)Facing, objDisplayStr, itemDisplayStr);

            return data;
        }

        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            string[] strData = Util.FindParams(data.stringData);
            if (strData.Length > 0)
            {
                RotateToDirection(Util.ParseEnum<DirectionEnum>(strData[0]));

                if (!strData[1].Equals(""))
                {
                    int objDisplayID = int.Parse(strData[1]);
                    SetDisplayObject((Decor)DataManager.CreateWorldObjectByID(objDisplayID));
                }
                else if (!strData[2].Equals(""))
                {
                    int itemDisplayID = int.Parse(strData[2]);
                    SetDisplayEntity(DataManager.GetItem(itemDisplayID));
                }
            }
        }
    }
}
