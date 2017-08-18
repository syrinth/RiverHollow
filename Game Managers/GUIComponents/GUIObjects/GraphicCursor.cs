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
        public static MouseState LastMouseState = new MouseState();
        private static InventoryItem _heldItem;
        public static InventoryItem HeldItem { get => _heldItem; }
        private static Building _heldBuilding;
        public static Building HeldBuilding { get => _heldBuilding; }

        private static ItemManager.WorkerID _workerID = ItemManager.WorkerID.Nothing;
        public static ItemManager.WorkerID WorkerToPlace { get => _workerID; }

        private static Vector2 _position;
        public static Vector2 Position { get => _position; set => _position = value; }
        //public static SpriteFont _font;

        private static Texture2D _texture;

        public static void LoadContent()
        {
            _texture = GameContentManager.GetInstance().GetTexture(@"Textures\cursor");
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        //    _font = GameContentManager.GetInstance().GetFont(@"Fonts\Font");
        }

        public static bool GrabItem(InventoryItem item)
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

        public static bool PickUpWorker(ItemManager.WorkerID id)
        {
            bool rv = false;
            if (id != ItemManager.WorkerID.Nothing)
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
            Texture2D drawIt = _texture;
            Rectangle drawRectangle = new Rectangle((int)Position.X, (int)Position.Y, drawIt.Width, drawIt.Height);
            if (AdventureGame.BuildingMode)
            {
                if (HeldBuilding != null)
                {
                    drawRectangle.X = ((int)((Position.X) / 32)) * 32;
                    drawRectangle.Y = ((int)((Position.Y) / 32)) * 32;
                    drawIt = _heldBuilding.Texture;
                    drawRectangle.Width = drawIt.Width;
                    drawRectangle.Height = drawIt.Height;

                    _heldBuilding.SetCoordinates(new Vector2(drawRectangle.X, drawRectangle.Y));
                    //spriteBatch.DrawString(_font, String.Format("{0}, {1}", drawRectangle.X, drawRectangle.Y), Position += new Vector2(300, 300), Color.Black);
                }
            }
            else
            {
                if (HeldItem != null) { drawIt = _heldItem.Texture; }
            }
            
            spriteBatch.Draw(drawIt, drawRectangle, Color.White);
        }
    }
}
