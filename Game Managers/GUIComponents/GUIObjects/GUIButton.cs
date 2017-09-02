using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIObjects
{
    public class GUIButton : GUIObject
    {
        public bool IsMouseHovering = false;

        {
            _texture = GameContentManager.GetTexture(texture);

        }

        public override void Draw(SpriteBatch spriteBatch)
        {
        }
    }
}
