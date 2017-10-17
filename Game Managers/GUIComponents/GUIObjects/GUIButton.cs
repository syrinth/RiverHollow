using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIButton : GUIObject
    {
        public bool IsMouseHovering = false;
        public bool _enabled;

        public GUIButton(Vector2 position, Rectangle sourceRect, int width, int height, string texture)
        {
            _texture = GameContentManager.GetTexture(texture);
            _width = width;
            _height = height;
            Position = position - new Vector2(width/2, height/2);
            _enabled = true;

            _sourceRect = sourceRect;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRect, (IsMouseHovering) ? Color.LightGray : Color.White);
        }
    }
}
