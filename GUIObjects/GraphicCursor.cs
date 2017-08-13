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

namespace Adventure.GUIObjects
{
    public class GraphicCursor : GUIObject
    {
        private static GraphicCursor instance;
        private InventoryItem _heldItem;
        public InventoryItem HeldItem { get => _heldItem; }

        private GraphicCursor(ContentManager Content)
        {
            _texture = Content.Load<Texture2D>(@"Textures\cursor");
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public static GraphicCursor GetInstance(ContentManager Content)
        {
            if (instance == null)
            {
                instance = new GraphicCursor(Content);
            }
            return instance;
        }

        public void GrabItem(InventoryItem item)
        {
            if(item != null)
            {
                _heldItem = item;
            }
        }

        public void DropItem()
        {
            _heldItem = null;
        }

        public void Update()
        {
            Position = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Texture2D drawIt = (_heldItem != null) ? _heldItem.Texture : _texture;
            spriteBatch.Draw(drawIt, new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height), Color.White);
        }
    }
}
