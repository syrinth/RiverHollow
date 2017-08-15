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
        private static Vector2 _position;
        public static Vector2 Position { get => _position; set => _position = value; }

        private static Texture2D _texture;

        public static void LoadContent()
        {
            _texture = GameContentManager.GetInstance().GetTexture(@"Textures\cursor");
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
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

        public static void Update()
        {
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Texture2D drawIt = (_heldItem != null) ? _heldItem.Texture : _texture;
            spriteBatch.Draw(drawIt, new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height), Color.White);
        }
    }
}
