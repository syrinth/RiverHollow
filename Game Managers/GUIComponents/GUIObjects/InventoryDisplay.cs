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
using Adventure.Game_Managers;

namespace Adventure.Screens
{
    public class InventoryDisplay : GUIObject
    {
        private KeyValuePair<Rectangle, InventoryItem>[] _displayList;
        private SpriteFont _displayFont;

        public InventoryDisplay()
        {
            _texture = _gcManager.GetTexture(@"Textures\MiniInventory");
            _displayFont = _gcManager.GetFont(@"Fonts\DisplayFont");
            Position = new Vector2((AdventureGame.ScreenWidth/ 2) - (_texture.Width/2), 16);
            _rect = new Rectangle((int)Position.X, (int)Position.Y, _texture.Width, _texture.Height);

            _displayList = new KeyValuePair<Rectangle, InventoryItem>[Player.maxItemRow];
            Rectangle displayBox = new Rectangle((int)Position.X + 3, (int)Position.Y + 3, 32, 32);
            for (int i=0; i< Player.maxItemRow; i++)
            {

                _displayList[i] = new KeyValuePair<Rectangle, InventoryItem>(displayBox, null);
                displayBox.X += 34;
            }

            _visible = true;
        }

        public bool ProcessLeftButtonClick(Point mouse)
        {
            bool rv = false;

            if (GraphicCursor.HeldItem != null && GiveItem(GraphicCursor.HeldItem))
            {
                GraphicCursor.DropItem();
                rv = true;
            }
            else
            {
               rv =  GraphicCursor.GrabItem(TakeItem(mouse));
            }
            return rv;
        }

        public InventoryItem TakeItem(Point mouse)
        {
            InventoryItem rv = null;

            for (int i = 0; i < Player.maxItemRow; i++)
            {
                if (_displayList[i].Key.Contains(mouse) && _displayList[i].Value != null)
                {
                    rv = new InventoryItem(_displayList[i].Value);
                    _playerManager.Player.RemoveItemFromInventory(i);
                }
            }

            return rv;
        }

        public bool GiveItem(InventoryItem item)
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
                            rv = _playerManager.Player.AddItemToInventorySpot(item, i);
                        }
                    }
                }
            }

            return rv;
        }

        public override void Update(GameTime gameTime)
        {
            if (_visible)
            {
                for (int i = 0; i < Player.maxItemRow; i++)
                {
                    _displayList[i] = new KeyValuePair<Rectangle, InventoryItem>(_displayList[i].Key, _playerManager.Player.Inventory[i]);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (_visible)
            {
                spriteBatch.Draw(_texture, _rect, Color.White);

                Rectangle itemBox = new Rectangle((int)Position.X + 3, (int)Position.Y + 3, 32, 32);
                for (int i = 0; i < Player.maxItemRow; i++)
                {
                    if (_displayList[i].Value != null)
                    {
                        _displayList[i].Value.Draw(spriteBatch, itemBox);
                        spriteBatch.DrawString(_displayFont, _displayList[i].Value.Number.ToString(), new Vector2(itemBox.X + 22, itemBox.Y + 22), Color.White);
                    }
                    itemBox.X += 34;
                }
            }
        }
    }
}
