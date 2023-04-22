﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;
using static RiverHollow.Utilities.Enums;

namespace RiverHollow.SpriteAnimations
{
    public class AnimatedSprite
    {
        #region Properties
        AnimatedSprite _sprLinkedSprite;
        public float LayerDepth => _sprLinkedSprite != null ? (_sprLinkedSprite.LayerDepth - 0.1f) : Position.Y + CurrentFrameAnimation.FrameHeight - (Position.X / 100f);

        readonly Texture2D _texture;                         // The texture that holds the images for this sprite
        public Color SpriteColor { get; private set; } = Color.White;
        public float Alpha { get; private set; } = 1;

        // Dictionary holding all of the FrameAnimation objects
        Dictionary<string, FrameAnimation> _diFrameAnimations = new Dictionary<string, FrameAnimation>();

        public Rectangle SpriteRectangle => new Rectangle(Position, new Point(Width, Height));
        public int Width { get; private set; } = -1;
        public int Height { get; private set; } = -1;

        int _iScale = 1;
        public int FrameCutoff { get; set; }

        /// Vector2 representing the position of the sprite's upper left
        /// corner pixel.
        public Point Position { get; set; } = Point.Zero;

        public bool Show { get; set; } = true;
        public bool Finished { get; set; } = false;

        float _fLayerDepthMod;

        float _fRotationAngle = 0;
        Vector2 _vRotationOrigin = Vector2.Zero;

        public int CurrentFrame => CurrentFrameAnimation.CurrentFrame;

        /// The FrameAnimation object of the currently playing animation
        public FrameAnimation CurrentFrameAnimation
        {
            get
            {
                if (CurrentAnimation != string.Empty && _diFrameAnimations.ContainsKey(CurrentAnimation))
                    return _diFrameAnimations[CurrentAnimation];
                else
                    return null;
            }
        }

        ///
        /// The string name of the currently playing animaton.  Setting the animation
        /// resets the CurrentFrame and PlayCount properties to zero.
        ///
        public string CurrentAnimation { get; private set; } = string.Empty;

        #endregion

        public AnimatedSprite(string Texture, bool combatSprite = false)
        {
            _texture = DataManager.GetTexture(Texture);

            if(_texture == null)
            {
                ErrorManager.TrackError();
            }
        }

        public AnimatedSprite(AnimatedSprite sprite)
        {
            _texture = sprite._texture;
            FrameCutoff = sprite.FrameCutoff;
            SpriteColor = sprite.SpriteColor;
            CurrentAnimation = sprite.CurrentAnimation;
            _iScale = sprite._iScale;

            Show = sprite.Show;

            _diFrameAnimations = new Dictionary<string, FrameAnimation>();
            foreach (KeyValuePair<string, FrameAnimation> kvp in sprite._diFrameAnimations){
                _diFrameAnimations[kvp.Key] = new FrameAnimation(kvp.Value);
            }

            Width = sprite.Width;
            Height = sprite.Height;
        }

        #region AddAnimation Helpers
        public void AddAnimation<TEnum>(TEnum verb, int startX, int startY, Point size, int Frames = 1, float FrameLength = 1f, bool pingPong = false, bool playsOnce = false) { AddAnimation(Util.GetEnumString(verb), startX, startY, size.X * Constants.TILE_SIZE, size.Y * Constants.TILE_SIZE, Frames, FrameLength, pingPong, playsOnce); }
        public void AddAnimation<TEnum>(TEnum verb, int startX, int startY, int Width, int Height, int Frames = 1, float FrameLength = 1f, bool pingPong = false, bool playsOnce = false) { AddAnimation(Util.GetEnumString(verb), startX, startY, Width, Height, Frames, FrameLength, pingPong, playsOnce); }
        public void AddAnimation(VerbEnum verb, DirectionEnum dir, int startX, int startY, int Width, int Height, int Frames = 1, float FrameLength = 1f, bool pingPong = false, bool playsOnce = false) { AddAnimation(Util.GetActorString(verb, dir), startX, startY, Width, Height, Frames, FrameLength, pingPong, playsOnce); }
        public void AddAnimation(string animationName, int startX, int startY, Point size, int Frames = 1, float FrameLength = 1f, bool pingPong = false, bool playsOnce = false) { AddAnimation(animationName, startX, startY, size.X * Constants.TILE_SIZE, size.Y * Constants.TILE_SIZE, Frames, FrameLength, pingPong, playsOnce); }
        public void AddAnimation(string animationName, int startX, int startY, int Width, int Height, int Frames = 1, float FrameLength = 1f, bool pingPong = false, bool playsOnce = false)
        {
            _diFrameAnimations.Add(animationName, new FrameAnimation(startX, startY, Width, Height, Frames, FrameLength, pingPong, playsOnce));
            if (this.Width == -1) { this.Width = Width; }
            if (this.Height == -1) { this.Height = Height; }
            if (_diFrameAnimations.Count == 1)
            {
                PlayAnimation(animationName);
            }
        }
        #endregion

        #region RemoveAnimation Helpers
        public void RemoveAnimation(VerbEnum verb, DirectionEnum dir) { RemoveAnimation(Util.GetActorString(verb, dir)); }
        private void RemoveAnimation(string animationName) { _diFrameAnimations.Remove(animationName); }
        #endregion

        #region PlayAnimation Helpers
        public void PlayAnimation<TEnum>(TEnum e) { PlayAnimation(Util.GetEnumString(e)); }
        public void PlayAnimation(VerbEnum verb, DirectionEnum dir) { PlayAnimation(Util.GetActorString(verb, dir)); }
        public void PlayAnimation(string verb, DirectionEnum dir) { PlayAnimation(Util.GetActorString(verb, dir)); }
        public void PlayAnimation(string animate)
        {
            if (_diFrameAnimations.ContainsKey(animate) && (!Show || CurrentAnimation != animate))
            {
                Show = true;
                Finished = false;
                CurrentAnimation = animate;
                Reset();   
            }
        }
        #endregion

        #region IsCurrentAnimation Helpers
        public bool AnimationFinished<TEnum>(TEnum e)
        {
            return IsCurrentAnimation(e) && Finished;
        }
        public bool IsCurrentAnimation<TEnum>(TEnum e) { return IsCurrentAnimation(Util.GetEnumString(e)); }
        public bool IsCurrentAnimation(VerbEnum verb, DirectionEnum dir) { return IsCurrentAnimation(Util.GetActorString(verb, dir)); }
        private bool IsCurrentAnimation(string animate) { return CurrentAnimation.Equals(animate); }
        #endregion

        public void SetColor(Color c)
        {
            SpriteColor = c;
        }

        public void SetAlternate<TEnum>(Point newStart, TEnum frameToShift)
        {
            _diFrameAnimations[Util.GetEnumString(frameToShift)].SetFrameStartLocation(newStart);
        }

        public void SetLayerDepthMod(float val)
        {
            _fLayerDepthMod = val;
        }

        public void SetLinkedSprite(AnimatedSprite sprite)
        {
            _sprLinkedSprite = sprite;
        }

        /// <summary>
        /// After ensuring that we have the frame we think we're on call Reset on it
        /// so that all of the timer data and counting data has been reset to 0.
        /// </summary>
        public void Reset()
        {
            if (!string.IsNullOrEmpty(CurrentAnimation))
            {
                _diFrameAnimations[CurrentAnimation].FullReset();
            }
        }

        public void SetNextAnimation<TEnum>(TEnum first, TEnum next)
        {
            SetNextAnimation(Util.GetEnumString(first), Util.GetEnumString(next));
        }
        public void SetNextAnimation(string first, string next)
        {
            _diFrameAnimations[first].SetNextAnimation(next);
        }

        public void SetScale(int x)
        {
            Width /= _iScale;
            Height /= _iScale;

            Width *= x;
            Height *= x;

            _iScale = x;
        }

        public FrameAnimation GetAnimationByName<TEnum>(TEnum animEnum)
        {
            string anim = Util.GetEnumString(animEnum);
            if (_diFrameAnimations.ContainsKey(anim))
            {
                return _diFrameAnimations[anim];
            }
            else
            {
                return null;
            }
        }

        public void Update(GameTime gTime)
        {
            if (Show && !Finished)
            {
                // If there is not a currently active animation
                if (CurrentFrameAnimation == null)
                {
                    // Make sure we have an animation associated with this sprite
                    if (_diFrameAnimations.Count > 0)
                    {
                        // Set the active animation to the first animation
                        // associated with this sprite
                        string[] sKeys = new string[_diFrameAnimations.Count];
                        _diFrameAnimations.Keys.CopyTo(sKeys, 0);
                        PlayAnimation(sKeys[0]);
                    }
                    else
                    {
                        return;
                    }
                }

                // Run the Animation's update method
                CurrentFrameAnimation.Update(gTime);

                if (CurrentFrameAnimation.PlayCount > 0)
                {
                    if (CurrentFrameAnimation.PlayCount == 1 && CurrentFrameAnimation.PlayOnce)
                    {
                        Finished = true;
                        Reset();
                    }

                    if (!String.IsNullOrEmpty(CurrentFrameAnimation.NextAnimation))
                    {
                        PlayAnimation(CurrentFrameAnimation.NextAnimation);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, bool useLayerDepth = true, float alpha = 1.0f, float forcedLayerDepth = -1)
        {
            if (Show)
            {
                if (useLayerDepth)
                {
                    float layerDepth = forcedLayerDepth < 0 ? LayerDepth : forcedLayerDepth;
                    Draw(spriteBatch, layerDepth, alpha);
                }
                else
                {
                    int newFrameCutoff = (int)(FrameCutoff * _iScale);
                    int drawAtY = (int)this.Position.Y;
                    Rectangle drawThis = CurrentFrameAnimation.FrameRectangle;

                    //This is used for lopping off the top part of a sprite,specifically for hair for hats
                    if (FrameCutoff != 0)
                    {
                        drawAtY += newFrameCutoff;
                        drawThis = new Rectangle(drawThis.X, FrameCutoff, drawThis.Width, drawThis.Height - FrameCutoff);
                    }

                    spriteBatch.Draw(_texture, new Rectangle(Position.X, drawAtY, Width, Height - newFrameCutoff), drawThis, SpriteColor * alpha);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, float layerDepth, float visibility = 1.0f)
        {
            if (Show)
            {
                int drawAtY = (int)this.Position.Y;
                Rectangle drawThis = CurrentFrameAnimation.FrameRectangle;

                //This is used for lopping off the top part of a sprite,specifically for hair for hats
                if (FrameCutoff != 0)
                {
                    drawAtY += FrameCutoff;
                    drawThis = new Rectangle(drawThis.X, FrameCutoff, drawThis.Width, drawThis.Height-FrameCutoff);
                }
                Draw(spriteBatch, new Rectangle((int)this.Position.X, drawAtY, this.Width, this.Height - FrameCutoff), drawThis, visibility, layerDepth + _fLayerDepthMod);
            }
        }

        /// <summary>
        /// Helper function for Drawing an AnimatedSprite
        /// </summary>
        /// <param name="spriteBatch">The spritebatch being used to draw</param>
        /// <param name="destinationRectangle">The Rectangle describing where on the screen to draw</param>
        /// <param name="sourceRectangle">The Rectangle describing where on the image we draw from</param>
        /// <param name="visibility">The opacity of the sprite</param>
        /// <param name="layerDepth">At what depth to draw the image, 0 is at the bottom.</param>
        private void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Rectangle sourceRectangle, float visibility = 1.0f, float layerDepth = 0)
        {
            Rectangle rotationalRect = destinationRectangle;
            rotationalRect.X += (int)_vRotationOrigin.X;
            rotationalRect.Y += (int)_vRotationOrigin.Y;
            spriteBatch.Draw(_texture, rotationalRect, CurrentFrameAnimation.FrameRectangle, SpriteColor * visibility, _fRotationAngle, _vRotationOrigin, SpriteEffects.None, layerDepth);
        }

        /// <summary>
        /// Returns the number of times the AnimatedSprite has fully played
        /// </summary>
        public int GetPlayCount()
        {
            return CurrentFrameAnimation.PlayCount;
        }

        /// <summary>
        /// Sets the angle to rotate the drawnsprite at
        /// </summary>
        /// <param name="angle">The angle, with 0 being straight</param>
        public void SetRotationAngle(float angle)
        {
            _fRotationAngle = angle;
        }

        /// <summary>
        /// Sets the location on the Texture2D to rotate around
        /// </summary>
        /// <param name="center"></param>
        public void SetRotationOrigin(Vector2 center)
        {
            _vRotationOrigin = center;
        }

        public bool ContainsAnimation<TEnum>(TEnum val)
        {
            return _diFrameAnimations.ContainsKey(Util.GetEnumString(val));
        }
    }
}
