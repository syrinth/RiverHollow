
using RiverHollow.Game_Managers;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using RiverHollow.SpriteAnimations;
using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.WorldObjects
{
    public class MerchantChest : WorldObject
    {
        public override Rectangle CollisionBox { get => new Rectangle((int) _vMapPosition.X, (int) _vMapPosition.Y+32, 64, 32);
    }
        private List<Item> _toSell;
        public MerchantChest()
        {
            _vMapPosition = new Vector2(800, 800);
            _iWidth = 64;
            _iHeight = 64;
            _sprite = new AnimatedSprite(DataManager.FILE_WORLDOBJECTS);
            _sprite.AddAnimation(AnimationEnum.ObjectIdle, 32, 0, 64, 64);

            _toSell = new List<Item>();
        }

        public int SellAll()
        {
            int val = 0;
            foreach(Item i in _toSell)
            {
                val += i.SellPrice;
                PlayerManager.AddMoney(i.SellPrice);
            }
            _toSell.Clear();
            return val;
        }

        public void AddItem(Item i)
        {
            if (i != null)
            {
                _toSell.Add(i);
            }
        }
    }
}
