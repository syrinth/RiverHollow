using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;
using RiverHollow.Items;
using RiverHollow.Utilities;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public static class GUICursor
    {
        public enum CursorTypeEnum { Normal, Talk, Gift, Door, Pickup, Interact};
        private static CursorTypeEnum _eCursorType;
        private static Rectangle _rCollisionRectangle;
        public static int WorkerToPlace { get; private set; } = -1;
        public static Point Position { get; set; }

        private static Texture2D _texture;
        private static Rectangle _rSource;

        private static GUIItem _guiItem;

        public static float Alpha = 1f;

        public static void LoadContent()
        {
            _texture = DataManager.GetTexture(DataManager.HUD_COMPONENTS);
            Position = new Point(Mouse.GetState().X, Mouse.GetState().Y);
            ResetCursor();
        }

        public static void Update()
        {
            Position = new Point(Mouse.GetState().X, Mouse.GetState().Y);
            _guiItem?.Position(new Point(Position.X + 16, Position.Y + 16));
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawRectangle = new Rectangle(Position.X, Position.Y, Constants.TILE_SIZE * 2, Constants.TILE_SIZE * 2);

            if (_eCursorType == CursorTypeEnum.Normal) { Alpha = 1; }
            else { Alpha = PlayerManager.InRangeOfPlayer(_rCollisionRectangle) ? 1 : 0.5f; }

            spriteBatch.Draw(_texture, drawRectangle, _rSource, Color.White * Alpha);
            _guiItem?.Draw(spriteBatch);
        }

        public static void SetGUIItem(Item heldItem)
        {
            if (heldItem == null) { _guiItem = null; }
            else {
                _guiItem = new GUIItem(heldItem);
                _guiItem.DrawShadow(false);
                _guiItem.Position(new Point(Position.X + 16, Position.Y + 16));
                GUIManager.CurrentScreen.RemoveControl(_guiItem);
            }
        }

        public static void ResetCursor()
        {
            SetCursor(CursorTypeEnum.Normal, Rectangle.Empty);
        }
        public static void SetCursor(CursorTypeEnum cursorType, Rectangle collisionRect)
        {
            if (GameManager.GamePaused())
            {
                cursorType = CursorTypeEnum.Normal;
            }

            _eCursorType = cursorType;
            _rCollisionRectangle = collisionRect;
            switch (_eCursorType)
            {
                case CursorTypeEnum.Normal:
                    _rSource = GUIUtils.CURSOR_POINT;
                    break;
                case CursorTypeEnum.Talk:
                    _rSource = GUIUtils.CURSOR_TALK;
                    break;
                case CursorTypeEnum.Door:
                    _rSource = GUIUtils.CURSOR_DOOR;
                    break;
                case CursorTypeEnum.Interact:
                    _rSource = GUIUtils.CURSOR_INTERACT;
                    break;
                case CursorTypeEnum.Pickup:
                    _rSource = GUIUtils.CURSOR_PICKUP;
                    break;
            }
        }

        public static Point GetWorldMousePosition()
        {
            return Camera.GetWorldPosition(Position);
        }

        public static bool PickUpWorker(int id)
        {
            bool rv = false;
            if (id > -1)
            {
                WorkerToPlace = id;
                rv = true;
            }

            return rv;
        }

        public static void UpdateTownBuildObject(GameTime gTime)
        {
            if (GameManager.HeldObject != null)
            {
                UpdateTownObjectLocation();
                HeldObject?.Update(gTime);
            }
        }

        public static void UpdateTownObjectLocation()
        {
            GameManager.HeldObject?.SnapPositionToGrid(GetWorldMousePosition() - GameManager.HeldObject.PickupOffset);
        }

        public static void DrawTownBuildObject(SpriteBatch spriteBatch)
        {
            if (GameManager.HeldObject != null)
            {
                HeldObject?.Draw(spriteBatch);
            }
        }

        /// <summary>
        /// This method draws the image of the item that we are holding onto the 
        /// World Map. However, we do not draw the image if we are Scrying.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public static void DrawPotentialWorldObject(SpriteBatch spriteBatch)
        {
            if (!Scrying())
            {
                //WorldObject construct = GameManager.ConstructionObject;
                //if (construct != null)
                //{
                //    construct.Draw(spriteBatch);
                //}
            }
        }
    }
}
