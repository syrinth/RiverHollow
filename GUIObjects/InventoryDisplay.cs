using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Adventure.Items;
using Microsoft.Xna.Framework.Input;

namespace Adventure.Screens
{
    public class InventoryDisplay : GUIObject
    {
        private static InventoryDisplay instance;
        private KeyValuePair<Rectangle, InventoryItem>[] _displayList;
        private SpriteFont _displayFont;
        private Rectangle _rect;

        private InventoryDisplay(ContentManager Content, int screenWidth)
        {
            _texture = Content.Load<Texture2D>(@"Textures\MiniInventory");
            _displayFont = Content.Load<SpriteFont>(@"Fonts\DisplayFont");
            Position = new Vector2((screenWidth/2) - (_texture.Width/2), 16);

            _displayList = new KeyValuePair<Rectangle, InventoryItem>[Player.maxItemRow];
            Rectangle displayBox = new Rectangle((int)Position.X + 3, (int)Position.Y + 3, 32, 32);
            for (int i=0; i< Player.maxItemRow; i++)
            {

                _displayList[i] = new KeyValuePair<Rectangle, InventoryItem>(displayBox, null);
                displayBox.X += 34;
            }
            _rect = new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);
        }

        public static InventoryDisplay GetInstance(ContentManager Content, int screenWidth)
        {
            if (instance == null)
            {
                instance = new InventoryDisplay(Content, screenWidth);
            }
            return instance;
        }

        public InventoryItem TakeItem(Player p)
        {
            InventoryItem rv = null;
            Vector2 mouse = new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);
            if (_rect.Contains(mouse))
            {
                for (int i = 0; i < Player.maxItemRow; i++)
                {
                    if (_displayList[i].Key.Contains(mouse) && _displayList[i].Value != null)
                    {
                        rv = new InventoryItem(_displayList[i].Value);
                        p.RemoveItemFromInventory(i);
                    }
                }
            }

            return rv;
        }

        public bool GiveItem(Player p, InventoryItem item)
        {
            bool rv = false;
            if (item != null)
            {
                Vector2 mouse = new Vector2(Mouse.GetState().Position.X, Mouse.GetState().Position.Y);
                if (_rect.Contains(mouse))
                {
                    for (int i = 0; i < Player.maxItemRow; i++)
                    {
                        if (_displayList[i].Key.Contains(mouse) && _displayList[i].Value == null)
                        {
                            rv = p.AddItemToInventorySpot(item, i);
                        }
                    }
                }
            }

            return rv;
        }

        public void Update(GameTime gameTime, Player p)
        {
            for (int i = 0; i < Player.maxItemRow; i++)
            {
                _displayList[i] = new KeyValuePair<Rectangle, InventoryItem>(_displayList[i].Key, p.Inventory[i]);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _rect, Color.White);

            Rectangle itemBox = new Rectangle((int)Position.X + 3, (int)Position.Y + 3, 32, 32);
            for (int i = 0; i < Player.maxItemRow; i++)
            {
                if (_displayList[i].Value != null)
                {
                    _displayList[i].Value.Draw(spriteBatch, itemBox);
                    spriteBatch.DrawString(_displayFont, _displayList[i].Value.Number.ToString(), new Vector2(itemBox.X+22, itemBox.Y+22), Color.White);
                }
                itemBox.X += 34;
            }
        }
    }
}
