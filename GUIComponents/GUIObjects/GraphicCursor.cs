using Microsoft.Xna.Framework.Graphics;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.GUIObjects
{
    public static class GraphicCursor
    {
        public enum EnumCursorType { Normal, Talk, Gift, Door};
        public static EnumCursorType _CursorType;
        public static MouseState LastMouseState = new MouseState();

        private static int _workerID = -1;
        public static int WorkerToPlace => _workerID; 

        private static Vector2 _position;
        public static Vector2 Position { get => _position; set => _position = value; }

        private static Texture2D _texture;

        public static float Alpha = 1f;

        public static void LoadContent()
        {
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _CursorType = EnumCursorType.Normal;
        }

        public static Vector2 GetWorldMousePosition()
        {
            Vector3 translate = Camera._transform.Translation;
            Vector2 mousePoint = Vector2.Zero;
            mousePoint.X = (int)((_position.X - translate.X) / Scale);
            mousePoint.Y = (int)((_position.Y - translate.Y) / Scale);

            return mousePoint;
        }

        public static bool PickUpWorker(int id)
        {
            bool rv = false;
            if (id > -1)
            {
                _workerID = id;
                rv = true;
            }

            return rv;
        }

        public static void Update()
        {
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (GameManager.HeldBuilding == null)
            {
                Rectangle source = Rectangle.Empty;
                Texture2D drawIt = _texture;
                if (!CombatManager.InCombat)
                {
                    switch (_CursorType)
                    {
                        case EnumCursorType.Normal:
                            source = new Rectangle(304, 160, 16, 16);
                            break;
                        case EnumCursorType.Talk:
                            source = new Rectangle(288, 160, 16, 16);
                            Alpha = (PlayerManager.PlayerInRange(GetWorldMousePosition().ToPoint(), (int)(TileSize * 1.5))) ? 1 : 0.5f;
                            break;
                        case EnumCursorType.Door:
                            source = new Rectangle(288, 176, 16, 16);
                            break;
                    }
                }
                else
                {
                    source = new Rectangle(304, 160, 16, 16);
                }
                Rectangle drawRectangle = new Rectangle((int)Position.X, (int)Position.Y, TileSize * 2, TileSize * 2);

                spriteBatch.Draw(drawIt, drawRectangle, source, Color.White * Alpha);
                if (HeldItem != null)
                {
                    GameManager.HeldItem.Draw(spriteBatch, new Rectangle((int)Position.X + 16, (int)Position.Y + 16, 32, 32));
                }
            }
        }

        public static void DrawBuilding(SpriteBatch spriteBatch)
        {
            if (GameManager.Scrying() && GameManager.HeldBuilding != null)
            {
                Vector2 mousePosition = GetWorldMousePosition();
                Rectangle drawRectangle = new Rectangle(((int)((mousePosition.X - HeldBuilding.Width / 2) / TileSize)) * TileSize, ((int)((mousePosition.Y - HeldBuilding.Height / 2) / TileSize)) * TileSize, HeldBuilding.Width, HeldBuilding.Height);

                GameManager.HeldBuilding.SetCoordinatesByGrid(new Vector2(drawRectangle.X, drawRectangle.Y));
                HeldBuilding.Sprite.Draw(spriteBatch);
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
                InventoryManager.GetCurrentStaticItem()?.GetWorldItem().Draw(spriteBatch);
            }
        }
    }
}
