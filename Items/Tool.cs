using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RiverHollow.Items
{
    public class Tool : Item
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
            int i = ImportBasics(itemValue, id, 1);

            _breakValue = float.Parse(itemValue[i++]);
            _chopValue = float.Parse(itemValue[i++]);
            _staminaCost = int.Parse(itemValue[i++]);
            _texture = GameContentManager.GetTexture(@"Textures\tools");

            _columnTextureSize = 128;
            _rowTextureSize = 32;

            CalculateSourcePos();

            _sprite = new AnimatedSprite(_texture);
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
