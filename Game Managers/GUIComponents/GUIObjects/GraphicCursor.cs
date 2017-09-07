using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Adventure.Items;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Adventure.Game_Managers;

namespace Adventure.GUIObjects
{
    public static class GraphicCursor
    {
        public enum CursorType { Normal, Talk, Gift};
        public static CursorType _currentType;
        public static MouseState LastMouseState = new MouseState();
        private static Item _heldItem;
        public static Item HeldItem { get => _heldItem; }
        private static Building _heldBuilding;
        public static Building HeldBuilding { get => _heldBuilding; }

        private static ObjectManager.WorkerID _workerID = ObjectManager.WorkerID.Nothing;
        public static ObjectManager.WorkerID WorkerToPlace { get => _workerID; }

        private static Vector2 _position;
        public static Vector2 Position { get => _position; set => _position = value; }

        private static Texture2D _texture;

        public static void LoadContent()
        {
            _texture = GameContentManager.GetTexture(@"Textures\Dialog");
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            _currentType = CursorType.Normal;
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

        public static bool PickUpWorker(ObjectManager.WorkerID id)
        {
            bool rv = false;
            if (id != ObjectManager.WorkerID.Nothing)
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
    }
}
