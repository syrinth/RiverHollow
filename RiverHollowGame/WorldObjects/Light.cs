using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RiverHollow.Game_Managers;
using RiverHollow.SpriteAnimations;
using RiverHollow.Utilities;
using System.Collections.Generic;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.WorldObjects
{
    public class Light
    {
        AnimatedSprite _sprite;
        public Vector2 Position
        {
            get { return _sprite.Position; }
            set { _sprite.Position = value; }
        }

        private Vector2 _vecDimensions;

        public int Width => (int)_vecDimensions.X;
        public int Height => (int)_vecDimensions.Y;

        public Light(int id, Dictionary<string, string> stringData)
        {
            string lightTex = string.Empty;
            Util.AssignValue(ref lightTex, "Texture", stringData);

            _sprite = new AnimatedSprite(DataManager.FOLDER_ENVIRONMENT + lightTex);

            Vector2 animDescriptor = new Vector2(1, 1);
            Util.AssignValue(ref animDescriptor, "Idle", stringData);
            Util.AssignValue(ref _vecDimensions, "Dimensions", stringData);
            _sprite.AddAnimation(AnimationEnum.ObjectIdle, 0, 0, Width, Height, (int)animDescriptor.X, animDescriptor.Y, true);
        }

        public void Update(GameTime gTime)
        {
            _sprite.Update(gTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            _sprite.Draw(spriteBatch);
        }
    }
}
