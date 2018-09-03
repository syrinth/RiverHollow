﻿using RiverHollow.GUIObjects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.Game_Managers.GUIObjects
{
    public class GUIImage : GUIObject
    {
        protected Color _color = Color.White;
        public GUIImage(Rectangle sourceRect, int width, int height, Texture2D texture)
        {
            _texture = texture;
            Width = width;
            Height = height;
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
            _sourceRect = sourceRect;
        }

        public GUIImage(Rectangle sourceRect, int width, int height, string texture)
        {
            _texture = GameContentManager.GetTexture(texture);
            Width = width;
            Height = height;
            _drawRect = new Rectangle((int)Position().X, (int)Position().Y, Width, Height);
            _sourceRect = sourceRect;
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
            spriteBatch.Draw(_texture, _drawRect, _sourceRect, _color * Alpha);
        }

        public void Draw(SpriteBatch spriteBatch, float alpha)
        {
            spriteBatch.Draw(_texture, _drawRect, _sourceRect, Color.Black * alpha);
        }

        public void SetScale(float x)
        {
            SetScale((int)x);
        }
        public virtual void SetScale(int x)
        {
            Width = Width / _iScale;
            Height = Height / _iScale;

            Width = Width * x;
            Height = Height * x;

            _iScale = x;
        }

        public void SetColor(Color c)
        {
            _color = c;
        }
    }
}
