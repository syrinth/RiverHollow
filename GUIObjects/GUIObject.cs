using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Adventure.GUIObjects
{
    public abstract class GUIObject
    {
        private int _height;
        public int Height { get => _height; set => _height = value; }
        private int _width;
        public int Width { get => _width; set => _width = value; }

        private Vector2 _position;
        public Vector2 Position { get => _position; set => _position = value; }

        protected Texture2D _texture;

        public abstract void Draw(SpriteBatch spriteBatch);
    }
}
