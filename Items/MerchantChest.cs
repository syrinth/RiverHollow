
using Adventure.Game_Managers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Adventure.Items
{
    public class MerchantChest : WorldObject
    {
        private List<Item> _toSell;
        public MerchantChest()
        {
            _position = new Vector2(800, 800);
            _width = 64;
            _height = 64;
            _sourceRectangle = new Rectangle(32, 0, 64, 64);
            _collisionBox = new Rectangle((int)_position.X, (int)_position.Y+32, 64, 32);
            _texture = GameContentManager.GetTexture(@"Textures\worldObjects");

            _toSell = new List<Item>();
        }

        public int SellAll()
        {
            int val = 0;
            foreach(Item i in _toSell)
            {
                val += i.SellPrice;
                PlayerManager.Player.AddMoney(i.SellPrice);
            }
            _toSell.Clear();
            return val;
        }

        public void AddItem(Item i)
        {
            _toSell.Add(i);
        }
    }
}
