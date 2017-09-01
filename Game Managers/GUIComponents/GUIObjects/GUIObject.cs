﻿using Adventure.Game_Managers;
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
        protected int _height;
        public int Height { get => _height; set => _height = value; }
        protected int _width;
        public int Width { get => _width; set => _width = value; }

        protected Vector2 _position;
        public Vector2 Position { get => _position; set { _position = value; _rect = new Rectangle((int)_position.X, (int)_position.Y, _width, _height); } }

        protected Rectangle _rect;
        public Rectangle Rectangle { get => _rect; }

        protected Texture2D _texture;

        public virtual bool Contains(Point mouse)
        {
            return Rectangle.Contains(mouse);
        }
        public virtual void Update(GameTime gameTime) { }
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _rect, Color.White);
        }

        public static Vector2 PosFromCenter(Vector2 center, int width, int height)
        {
            return new Vector2(center.X - width / 2, center.Y - height / 2);
        }

        public static Vector2 PosFromCenter(int x, int y, int width, int height)
        {
            return new Vector2(x - width / 2, y - height / 2);
        }
    }
}
