using Microsoft.Xna.Framework.Graphics;
using RiverHollow.WorldObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;

using static RiverHollow.Game_Managers.GameManager;
using RiverHollow.Buildings;
using RiverHollow.SpriteAnimations;

namespace RiverHollow.GUIObjects
{
    public static class GraphicCursor
    {
        public enum EnumCursorType { Normal, Talk, Gift};
        public static EnumCursorType _CursorType;
        public static MouseState LastMouseState = new MouseState();
        private static Item _heldItem;
        public static Item HeldItem { get => _heldItem; }
        private static Building _heldBuilding;
        public static Building HeldBuilding { get => _heldBuilding; }

        private static int _workerID = -1;
        public static int WorkerToPlace { get => _workerID; }

        private static Vector2 _position;
        public static Vector2 Position { get => _position; set => _position = value; }

        private static Texture2D _texture;

        public static void LoadContent()
        {
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _CursorType = EnumCursorType.Normal;
        }

        public static Vector2 GetTranslatedPosition()
        {
            Vector3 translate = Camera._transform.Translation;
            Vector2 mousePoint = Vector2.Zero;
            mousePoint.X = (int)((_position.X - translate.X) / Scale);
            mousePoint.Y = (int)((_position.Y - translate.Y) / Scale);

            return mousePoint;
        }

        public static bool GrabItem(Item item)
        {
            bool rv = false;
            if(item != null)
            {
                _heldItem = item;
                rv = true;
            }

            return rv;
        }

        public static void DropItem()
        {
            _heldItem = null;
        }

        public static bool PickUpBuilding(Building bldg)
        {
            bool rv = false;
            if (bldg != null)
            {
                _heldBuilding = bldg;
                rv = true;
            }

            return rv;
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

        public static void DropBuilding()
        {
            _heldBuilding = null;
        }

        public static void Update()
        {
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (_heldBuilding == null)
            {
                Rectangle source = Rectangle.Empty;
                Texture2D drawIt = _texture;
                float alpha = 1f;
                switch (_CursorType)
                {
                    case EnumCursorType.Normal:
                        source = new Rectangle(288, 192, 32, 32);
                        break;
                    case EnumCursorType.Talk:
                        source = new Rectangle(288, 160, 32, 32);
                        alpha = (PlayerManager.PlayerInRange(GetTranslatedMouseLocation().ToPoint(), (int)(TileSize * 1.5))) ? 1 : 0.5f;
                        break;
                }
                Rectangle drawRectangle = new Rectangle((int)Position.X, (int)Position.Y, 32, 32);

                spriteBatch.Draw(drawIt, drawRectangle, source, Color.White * alpha);
                if (HeldItem != null)
                {
                    _heldItem.Draw(spriteBatch, new Rectangle((int)Position.X + 16, (int)Position.Y + 16, 32, 32));
                }
            }
        }

        public static void DrawBuilding(SpriteBatch spriteBatch)
        {
            if (GameManager.Scrying() && _heldBuilding != null)
            {
                Vector2 mousePosition = GetTranslatedPosition();
                Texture2D drawIt = _heldBuilding.Texture;
                Rectangle drawRectangle = new Rectangle(((int)((mousePosition.X - drawIt.Width / 2) / TileSize)) * TileSize, ((int)((mousePosition.Y - drawIt.Height / 2) / TileSize)) * TileSize, drawIt.Width, drawIt.Height);
                Rectangle source = new Rectangle(0, 0, drawIt.Width, drawIt.Height);

                _heldBuilding.SetCoordinates(new Vector2(drawRectangle.X, drawRectangle.Y));
                spriteBatch.Draw(drawIt, drawRectangle, null, Color.White, 0, Vector2.Zero, SpriteEffects.None, mousePosition.Y + drawIt.Height);
            }
        }

        public static void DrawPotentialWorldObject(SpriteBatch spriteBatch)
        {
            StaticItem it = InventoryManager.GetCurrentStaticItem();
            if (!Scrying() && it != null)
            {
                WorldItem obj = it.GetWorldItem();
                obj.Draw(spriteBatch);
            }
        }

        public static Vector2 GetTranslatedMouseLocation()
        {
            Vector2 mousePoint = Position;
            Vector3 translate = Camera._transform.Translation;
            mousePoint.X = (int)((mousePoint.X - translate.X) / Scale);
            mousePoint.Y = (int)((mousePoint.Y - translate.Y) / Scale);
            return mousePoint;
        }
    }
}
