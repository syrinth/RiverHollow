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
        protected RotationalEnum _eRotationType = RotationalEnum.None;

        protected DirectionEnum _eFacingDir = DirectionEnum.Down;
        public DirectionEnum Facing => _eFacingDir;

        protected Point _pDisplayOffset = Point.Zero;
        protected Point _pRotatedDisplayOffset = Point.Zero;

        protected int _iRotationBaseOffsetX;
        protected int _iRotationBaseOffsetY;
        protected Point _pRotationSize;

        private readonly bool _bDisplaysObject = false;
        public bool CanDisplay => _bDisplaysObject;

        private readonly bool _bCanBeDisplayed = false;
        public bool CanBeDisplayed => _bCanBeDisplayed;
        private Item _itemDisplay;
        private Decor _objDisplay;
        public bool HasDisplay => _objDisplay != null || _itemDisplay != null;
        bool Archive => GetBoolByIDKey("Archive");

        public Decor(int id, Dictionary<string, string> stringData) : base(id, stringData)
        {
            _eObjectType = ObjectTypeEnum.Decor;
            Util.AssignValue(ref _eRotationType, "Rotation", stringData);
            Util.AssignValues(ref _iRotationBaseOffsetX, ref _iRotationBaseOffsetY, "RotationBaseOffset", stringData);
            Util.AssignValue(ref _pRotationSize, "RotationSize", stringData);
            Util.AssignValue(ref _bDisplaysObject, "Display", stringData);
            Util.AssignValue(ref _bCanBeDisplayed, "CanBeDisplayed", stringData);
            Util.AssignValue(ref _pDisplayOffset, "DisplayOffset", stringData);
            Util.AssignValue(ref _pRotatedDisplayOffset, "RotatedDisplayOffset", stringData);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            if (_objDisplay != null)
            {
                _objDisplay.Sprite.SetColor(_bSelected ? Color.Green : Color.White);
                _objDisplay.Sprite.Draw(spriteBatch, Sprite.LayerDepth + 1);
            }
            else if (_itemDisplay != null)
            {
                //Because Items don't exist directly on the map, we only need to tell it where to draw itself here
                _itemDisplay.SetColor(_bSelected ? Color.Green : Color.White);
                _itemDisplay.Draw(spriteBatch, new Rectangle(MapPosition.X + _pDisplayOffset.X, MapPosition.Y + _pDisplayOffset.Y, Constants.TILE_SIZE, Constants.TILE_SIZE), true, Sprite.LayerDepth + 1);
            }
        }

        //public override bool ProcessLeftClick()
        //{
        //    bool rv = false;
        //    if (GameManager.CanMoveObject())
        //    {
        //        rv = true;

        //        if(_itemDisplay != null)
        //        {
        //            if (InventoryManager.HasSpaceInInventory(_itemDisplay.ID, 1))
        //            {
        //                InventoryManager.AddToInventory(_itemDisplay);
        //            }
        //        }
        //        else if(_objDisplay != null)
        //        {
        //            GameManager.MovingWorldObject(_objDisplay);
        //            _objDisplay = null;
        //        }
        //        else
        //        {
        //            CurrentMap.RemoveWorldObject(this, true);
        //            GameManager.MovingWorldObject(this);
        //        }
                
        //    }

        //    return rv;
        //}

        /// <summary>
        /// Handler for when a Decor object hasbeen right-clicked
        /// </summary>
        public override bool ProcessRightClick()
        {
            bool rv = false;

            if (ShopItem)
            {
                MapManager.CurrentMap.TheShop?.Interact(MapManager.CurrentMap, GUICursor.Position);
            }
            else
            {
                //Currently, only display Decor objects can be interacted with.
                if (CanDisplay)
                {
                    rv = true;
                    if (Archive || _itemDisplay == null)
                    {
                        GameManager.SetSelectedWorldObject(this);
                        GUIManager.OpenMainObject(new HUDInventoryDisplay());
                    }
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
                SyncDisplayObject();
            }

            return rv;
        }

        /// <summary>
        /// Assuming the object is capable of rotation, this method does the math
        /// required to change the sprite and base tiles accordingly.
        /// </summary>
        public void Rotate()
        {
            if (_eRotationType != RotationalEnum.None)
            {
                //We don't need to do any swaps if the object has the same base and height
                if (_rBase.Width != _rBase.Height)
                {
                    (_pRotatedDisplayOffset, _pDisplayOffset) = (_pDisplayOffset, _pRotatedDisplayOffset);
                    Util.SwitchValues(ref _rBase.Width, ref _rBase.Height);
                    Util.SwitchValues(ref _pSize, ref _pRotationSize);
                    Util.SwitchValues(ref _rBase.X, ref _iRotationBaseOffsetX);
                    Util.SwitchValues(ref _rBase.Y, ref _iRotationBaseOffsetY);
                }

                Rectangle spriteFrameRectangle = Sprite.CurrentFrameAnimation.FrameRectangle;
                Point newImage = spriteFrameRectangle.Location + new Point(spriteFrameRectangle.Width, 0);

                //Direction handling for the different rotation types
                if (_eRotationType == RotationalEnum.FourWay)
                {
                    switch (_eFacingDir)
                    {
                        case DirectionEnum.Down:
                            _eFacingDir = DirectionEnum.Right;
                            break;
                        case DirectionEnum.Right:
                            _eFacingDir = DirectionEnum.Up;
                            break;
                        case DirectionEnum.Up:
                            _eFacingDir = DirectionEnum.Left;
                            break;
                        case DirectionEnum.Left:
                            newImage = _pImagePos;
                            _eFacingDir = DirectionEnum.Down;
                            break;
                    }
                }
                else if (_eRotationType == RotationalEnum.TwoWay)
                {
                    switch (_eFacingDir)
                    {
                        case DirectionEnum.Down:
                            _eFacingDir = DirectionEnum.Right;
                            break;
                        case DirectionEnum.Right:
                            newImage = _pImagePos;
                            _eFacingDir = DirectionEnum.Down;
                            break;
                    }
                }

                //Updates the sprite info
                Sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
                Sprite.AddAnimation(AnimationEnum.ObjectIdle, newImage.X, newImage.Y, _pSize);
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
        public void SetDisplayEntity(Item it)
        {
            if (!Archive || !TownManager.DIArchive[it.ID].Item2)
            {
                if (StoreDisplayEntity())
                {
                    if (it != null)
                    {
                        if (it.ID > Constants.BUILDABLE_ID_OFFSET)
                        {
                            _objDisplay = (Decor)DataManager.CreateWorldObjectByID(it.ID);
                            SyncDisplayObject();
                        }
                        else { _itemDisplay = DataManager.GetItem(it.ID); }

                        InventoryManager.RemoveItemsFromInventory(it.ID, 1);
                        GUIManager.CloseMainObject();

                        if (Archive)
                        {
                            TownManager.AddToArchive(it.ID);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This method sends any display entity to the appropriate storage 
        /// and blanks out its reference on the Displaying Decor.
        /// 
        /// Only store entity if there is space in storage
        /// </summary>
        /// <returns>True as long as there is space in storage</returns>
        public bool StoreDisplayEntity()
        {
            bool rv = true;
            Item displayItem = null;

            if (_itemDisplay != null) { displayItem = _itemDisplay; }
            else if (_objDisplay != null) { displayItem = DataManager.GetItem(_objDisplay); }

            if (displayItem != null)
            {
                if (InventoryManager.HasSpaceInInventory(displayItem.ID, 1))
                {
                    InventoryManager.AddToInventory(displayItem);
                    _itemDisplay = null;
                    _objDisplay = null;
                }
                else { rv = false; }
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

        public override WorldObjectData SaveData()
        {
            //Need to save null items
            WorldObjectData data = base.SaveData();
            string objDisplayStr = _objDisplay == null ? "" : _objDisplay.ID.ToString();
            string itemDisplayStr = _itemDisplay == null ? "" : _itemDisplay.ID.ToString();
            data.stringData = string.Format("{0}/{1}/{2}", (int)_eFacingDir, objDisplayStr, itemDisplayStr);

            return data;
        }

        public override void LoadData(WorldObjectData data)
        {
            base.LoadData(data);
            string[] strData = Util.FindParams(data.stringData);
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
