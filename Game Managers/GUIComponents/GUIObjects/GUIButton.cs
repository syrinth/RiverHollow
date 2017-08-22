using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIObjects
{
    public class GUIButton : GUIObject
    {
        public bool IsMouseHovering = false;
        public GUIButton(int centreX, int centreY, string texture)
        {
            _texture = GameContentManager.GetTexture(texture);

            _rect = new Rectangle(centreX-_texture.Width/2, centreY-_texture.Height/2, _texture.Width, _texture.Height);
        }

        public GUIButton(int x, int y, int width, int height, string texture, string hover)
        {
            _texture = GameContentManager.GetTexture(texture);

            _rect = new Rectangle(x, y, width, height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _rect, (IsMouseHovering) ? Color.LightGray : Color.White);
        }
    }
}
