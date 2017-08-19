using Adventure.GUIObjects;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Adventure.Game_Managers.GUIObjects
{
    public class GUIButton : GUIObject
    {
        private Texture2D _textureHover;
        public bool IsMouseHovering = false;
        public GUIButton(int centreX, int centreY, string texture, string hover)
        {
            _texture = GameContentManager.GetInstance().GetTexture(texture);
            _textureHover = GameContentManager.GetInstance().GetTexture(hover);

            _rect = new Rectangle(centreX-_texture.Width/2, centreY-_texture.Height/2, _texture.Width, _texture.Height);
        }

        public GUIButton(int x, int y, int width, int height, string texture, string hover)
        {
            _texture = GameContentManager.GetInstance().GetTexture(texture);
            _textureHover = GameContentManager.GetInstance().GetTexture(hover);

            _rect = new Rectangle(x, y, width, height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _rect, (IsMouseHovering) ? Color.LightGray : Color.White);
        }
    }
}
