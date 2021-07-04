using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Items;
using RiverHollow.GUIComponents.GUIObjects.GUIWindows;

namespace RiverHollow.GUIComponents.GUIObjects
{
    public static class GUICursor
    {
        public enum CursorTypeEnum { Normal, Talk, Gift, Door, Pickup};
        private static CursorTypeEnum _eCursorType;
        private static Rectangle _rCollisionRectangle;
        public static MouseState LastMouseState = new MouseState();
        public static int WorkerToPlace { get; private set; } = -1;
        public static Vector2 Position { get; set; }

        private static Texture2D _texture;
        private static Rectangle _rSource;

        private static GUIItem _guiItem;

        public static float Alpha = 1f;

        public static void LoadContent()
        {
            _texture = DataManager.GetTexture(DataManager.DIALOGUE_TEXTURE);
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            ResetCursor();
        }

        public static void Update()
        {
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _guiItem?.Position(new Vector2((int)Position.X + 16, (int)Position.Y + 16));
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Rectangle drawRectangle = new Rectangle((int)Position.X, (int)Position.Y, TILE_SIZE * 2, TILE_SIZE * 2);

            if (_eCursorType == CursorTypeEnum.Normal) { Alpha = 1; }
            else { Alpha = (PlayerManager.PlayerInRange(_rCollisionRectangle)) ? 1 : 0.5f; }

            spriteBatch.Draw(_texture, drawRectangle, _rSource, Color.White * Alpha);
            _guiItem?.Draw(spriteBatch);
        }

        public static void SetGUIItem(Item heldItem)
        {
            if (heldItem == null) { _guiItem = null; }
            else {
                _guiItem = new GUIItem(heldItem);
                _guiItem.Position(new Vector2((int)Position.X + 16, (int)Position.Y + 16));
            }
        }

        public static void ResetCursor()
        {
            SetCursor(CursorTypeEnum.Normal, Rectangle.Empty);
        }
        public static void SetCursor(CursorTypeEnum cursorType, Rectangle collisionRect)
        {
            if (CombatManager.InCombat || GameManager.IsPaused())
            {
                cursorType = CursorTypeEnum.Normal;
            }

            _eCursorType = cursorType;
            _rCollisionRectangle = collisionRect;
            switch (_eCursorType)
            {
                case CursorTypeEnum.Normal:
                    _rSource = new Rectangle(304, 160, 16, 16);
                    break;
                case CursorTypeEnum.Talk:
                    _rSource = new Rectangle(288, 160, 16, 16);
                    break;
                case CursorTypeEnum.Door:
                    _rSource = new Rectangle(288, 176, 16, 16);
                    break;
                case CursorTypeEnum.Pickup:
                    _rSource = new Rectangle(304, 176, 16, 16);
                    break;
            }
        }

        public static Vector2 GetWorldMousePosition()
        {
            Vector3 translate = Camera._transform.Translation;
            Vector2 mousePoint = Vector2.Zero;
            mousePoint.X = (int)((Position.X - translate.X) / CurrentScale);
            mousePoint.Y = (int)((Position.Y - translate.Y) / CurrentScale);

            return mousePoint;
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
                GameManager.HeldObject?.SnapPositionToGrid(GetWorldMousePosition() - GameManager.HeldObject.PickupOffset);
                HeldObject?.Update(gTime);
            }
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
        /// World Map. However, we do not draw the image if we are Scrying, or on
        /// a map that is for combat.
        /// </summary>
        /// <param name="spriteBatch"></param>
        public static void DrawPotentialWorldObject(SpriteBatch spriteBatch)
        {
            if (!Scrying() && !MapManager.CurrentMap.IsCombatMap)
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
