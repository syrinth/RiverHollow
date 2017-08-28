using Adventure.Game_Managers;
using Adventure.SpriteAnimations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.Items
{
    public class Tool : InventoryItem
    {
        protected int _staminaCost;
        public int StaminaCost { get => _staminaCost; }
        protected float _breakValue;
        public float BreakValue { get => _breakValue; }
        protected float _chopValue;
        public float ChopValue { get => _chopValue; }
        protected AnimatedSprite _sprite;
        public AnimatedSprite ToolAnimation { get => _sprite; }

        public Tool(int id, string[] itemValue)
        {
            if (itemValue.Length == 8)
            {
                _num = 1;

                int i = 1;
                _itemType = ItemType.Resource;
                _name = itemValue[i++];
                _description = itemValue[i++];
                _textureIndex = int.Parse(itemValue[i++]);
                _itemID = id; //(ObjectManager.ItemIDs)Enum.Parse(typeof(ObjectManager.ItemIDs), itemValue[i++]);
                i++; //holding out for enum
                _breakValue = float.Parse(itemValue[i++]);
                _chopValue = float.Parse(itemValue[i++]);
                _staminaCost = int.Parse(itemValue[i++]);

                _texture = GameContentManager.GetTexture(@"Textures\tools");

                CalculateSourcePos();
            }
        }

        public Tool(int ID, Vector2 sourcePos, Texture2D texture, string name, string description, float breakVal, float chopVal, int stam, List<KeyValuePair<ObjectManager.ItemIDs, int>> reagents) : base(ID, sourcePos, texture, name, description, 1, false, reagents)
        {
            _sourcePos = sourcePos;
            _breakValue = breakVal;
            _chopValue = chopVal;
            _sprite = new AnimatedSprite(texture);
            _staminaCost = stam;

            _sprite.AddAnimation("Left", (int)_sourcePos.X + 32, (int)_sourcePos.Y, 32, 32, 3, 0.1f);

            _sprite.CurrentAnimation = "Left";
            _sprite.IsAnimating = true;
            _sprite.playsOnce = true;
        }

        public void Update(GameTime gameTime)
        {
            _sprite.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch, Rectangle drawBox)
        {
            spriteBatch.Draw(_texture, drawBox, new Rectangle((int)_sourcePos.X, (int)_sourcePos.Y, 32, 32), Color.White);
        }
    }
}
