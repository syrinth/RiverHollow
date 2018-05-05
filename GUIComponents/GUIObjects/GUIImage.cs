﻿using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIImage : GUIObject
    {
        public Rectangle _sourceRectangle;
        public GUIImage(Vector2 position, Rectangle sourceRect, int width, int height, Texture2D texture)
        {
            _texture = texture;
            Position(position);
            Width = width;
            Height = height;
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
            _sourceRectangle = sourceRect;
        }

        public GUIImage(Vector2 position, Rectangle sourceRect, int width, int height, string texture)
        {
            _texture = GameContentManager.GetTexture(texture);
            Position(position);
            Width = width;
            Height = height;
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
            _sourceRectangle = sourceRect;
        }

        public void MoveImageTo(Vector2 pos)
        {
            Position(pos);
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
        }

        public void MoveImageBy(Vector2 pos)
        {
            PositionAdd(pos);
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRectangle, Color.White);
        }

        public void Draw(SpriteBatch spriteBatch, float alpha)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRectangle, Color.Black * alpha);
        }

        public void SetScale(float scale)
        {
            Width = (int)(Width * scale);
            Height = (int)(Height * scale);
        }
    }
}
