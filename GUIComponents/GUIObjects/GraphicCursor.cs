using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Items;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RiverHollow.Game_Managers;

namespace RiverHollow.GUIObjects
{
    public static class GraphicCursor
    {
        private static float Scale = GameManager.Scale;
        public enum CursorType { Normal, Talk, Gift};
        public static CursorType _currentType;
        public static MouseState LastMouseState = new MouseState();
        private static Item _heldItem;
        public static Item HeldItem { get => _heldItem; }
        private static WorkerBuilding _heldBuilding;
        public static WorkerBuilding HeldBuilding { get => _heldBuilding; }

        private static int _workerID = -1;
        public static int WorkerToPlace { get => _workerID; }

        private static Vector2 _position;
        public static Vector2 Position { get => _position; set => _position = value; }

        private static Texture2D _texture;

        public static void LoadContent()
        {
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _currentType = CursorType.Normal;
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

        public static bool PickUpBuilding(WorkerBuilding bldg)
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
            Rectangle source = Rectangle.Empty;
            Texture2D drawIt = _texture;
            switch (_currentType)
            {
                case CursorType.Normal:
                    source = new Rectangle(288, 192, 32, 32);
                    break;
                case CursorType.Talk:
                    source = new Rectangle(288, 160, 32, 32);
                    break;
                case CursorType.Gift:
                    source = new Rectangle(288, 224, 32, 32);
                    break;
            }
            Rectangle drawRectangle = new Rectangle((int)Position.X, (int)Position.Y, 32, 32);
            
            spriteBatch.Draw(drawIt, drawRectangle, source, Color.White);
            if (HeldItem != null)
            {
                _heldItem.Draw(spriteBatch, new Rectangle((int)Position.X+16, (int)Position.Y+16, 32, 32));
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
