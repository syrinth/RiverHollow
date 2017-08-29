﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Adventure.Tile_Engine;

namespace Adventure.SpriteAnimations
{
    public class AnimatedSprite
    {
        #region properties
        Texture2D _texture;                         // The texture that holds the images for this sprite
        bool _animating = true;                     // True if animations are being played
        Color colorTint = Color.White;              // If set to anything other than Color.White, will colorize the sprite with that color.

        // Screen Position of the Sprite
        private Vector2 _Position = new Vector2(0, 0);
        Vector2 _LastPosition = new Vector2(0, 0);

        // Dictionary holding all of the FrameAnimation objects
        // associated with this sprite.
        Dictionary<string, FrameAnimation> _frameanimations = new Dictionary<string, FrameAnimation>();

        // Which FrameAnimation from the dictionary above is playing
        string _currAnimation = null;

        // Calculated center of the sprite
        Vector2 v2Center;

        // Calculated width and height of the sprite
        int _width;
        int _height;

        ///
        /// Vector2 representing the position of the sprite's upper left
        /// corner pixel.
        ///
        public Vector2 Position
        {
            get { return _Position; }
            set
            {
                _LastPosition = _Position;
                _Position = value;
            }
        }

        ///
        /// The X position of the sprite's upper left corner pixel.
        ///
        public int X
        {
            get { return (int)_Position.X; }
            set
            {
                _LastPosition.X = _Position.X;
                _Position.X = value;
            }
        }

        ///
        /// The Y position of the sprite's upper left corner pixel.
        ///
        public int Y
        {
            get { return (int)_Position.Y; }
            set
            {
                _LastPosition.Y = _Position.Y;
                _Position.Y = value;
            }
        }

        ///
        /// Width (in pixels) of the sprite animation frames
        ///
        public int Width
        {
            get { return _width; }
        }

        ///
        /// Height (in pixels) of the sprite animation frames
        ///
        public int Height
        {
            get { return _height; }
        }

        ///
        /// Screen coordinates of the bounding box surrounding this sprite
        ///
        public Rectangle BoundingBox
        {
            get { return new Rectangle(X, Y, _width, _height); }
        }

        ///
        /// The texture associated with this sprite.  All FrameAnimations will be
        /// relative to this texture.
        ///
        public Texture2D Texture
        {
            get { return _texture; }
        }

        ///
        /// Color value to tint the sprite with when drawing.  Color.White
        /// (the default) indicates no tinting.
        ///
        public Color Tint
        {
            get { return colorTint; }
            set { colorTint = value; }
        }

        ///
        /// True if the sprite is (or should be) playing animation frames.  If this value is set
        /// to false, the sprite will not be drawn (a sprite needs at least 1 single frame animation
        /// in order to be displayed.
        ///
        public bool IsAnimating
        {
            get { return _animating; }
            set { _animating = value; }
        }

        public bool playsOnce = false;
        ///
        /// The FrameAnimation object of the currently playing animation
        ///
        public FrameAnimation CurrentFrameAnimation
        {
            get
            {
                if (!string.IsNullOrEmpty(_currAnimation))
                    return _frameanimations[_currAnimation];
                else
                    return null;
            }
        }

        ///
        /// The string name of the currently playing animaton.  Setting the animation
        /// resets the CurrentFrame and PlayCount properties to zero.
        ///
        public string CurrentAnimation
        {
            get { return _currAnimation; }
            set
            {
                if (_frameanimations.ContainsKey(value))
                {
                    _currAnimation = value;
                    _frameanimations[_currAnimation].CurrentFrame = 0;
                    _frameanimations[_currAnimation].PlayCount = 0;
                }
            }
        }

#endregion

        public AnimatedSprite(Texture2D Texture)
        {
            _texture = Texture;
        }

        //TODO: Remove this method, classes should do it manually, not in this level
        public void LoadContent(int textureWidth, int textureHeight, int numFrames, float frameSpeed)
        {
            this.AddAnimation("Float", 0, 0, textureWidth, textureHeight, numFrames, frameSpeed);

            this.CurrentAnimation = "Float";
            this.IsAnimating = true;
        }

        public void AddAnimation(string Name, int X, int Y, int Width, int Height, int Frames, float FrameLength)
        {
            _frameanimations.Add(Name, new FrameAnimation(X, Y, Width, Height, Frames, FrameLength));
            _width = Width;
            _height = Height;
            v2Center = new Vector2(_width / 2, _height / 2);
        }

        public void AddAnimation(string Name, int X, int Y, int Width, int Height, int Frames, float FrameLength, string NextAnimation)
        {
            _frameanimations.Add(Name, new FrameAnimation(X, Y, Width, Height, Frames, FrameLength, NextAnimation));
            _width = Width;
            _height = Height;
            v2Center = new Vector2(_width / 2, _height / 2);
        }

        public FrameAnimation GetAnimationByName(string Name)
        {
            if (_frameanimations.ContainsKey(Name))
            {
                return _frameanimations[Name];
            }
            else
            {
                return null;
            }
        }

        public void MoveBy(int x, int y)
        {
            _LastPosition = _Position;
            _Position.X += x;
            _Position.Y += y;
        }

        public void Update(GameTime gameTime)
        {
            // Don't do anything if the sprite is not animating
            if (_animating)
            {
                // If there is not a currently active animation
                if (CurrentFrameAnimation == null)
                {
                    // Make sure we have an animation associated with this sprite
                    if (_frameanimations.Count > 0)
                    {
                        // Set the active animation to the first animation
                        // associated with this sprite
                        string[] sKeys = new string[_frameanimations.Count];
                        _frameanimations.Keys.CopyTo(sKeys, 0);
                        CurrentAnimation = sKeys[0];
                    }
                    else
                    {
                        return;
                    }
                }

                // Run the Animation's update method
                CurrentFrameAnimation.Update(gameTime);

                if (playsOnce && CurrentFrameAnimation.PlayCount > 0)
                {
                    IsAnimating = false;
                    CurrentFrameAnimation.PlayCount = 0;
                }

                // Check to see if there is a "followup" animation named for this animation
                //if (!String.IsNullOrEmpty(CurrentFrameAnimation.NextAnimation))
                //{
                // If there is, see if the currently playing animation has
                // completed a full animation loop
                //if (CurrentFrameAnimation.PlayCount > 0)
                //{
                //    // If it has, set up the next animation
                //    CurrentAnimation = CurrentFrameAnimation.NextAnimation;
                //}
                //}
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_animating) {
                spriteBatch.Draw(_texture, new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height), CurrentFrameAnimation.FrameRectangle, Color.White, 0, new Vector2(0,0), SpriteEffects.None, Position.Y + Texture.Height+(Position.X/100));
            }     
        }
    }
}
