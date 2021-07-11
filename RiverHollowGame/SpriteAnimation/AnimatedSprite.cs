using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using RiverHollow.Game_Managers;
using RiverHollow.Utilities;

using static RiverHollow.Game_Managers.GameManager;

namespace RiverHollow.SpriteAnimations
{
    public class AnimatedSprite
    {
        #region Properties
        public float LayerDepth => Position.Y + CurrentFrameAnimation.FrameHeight + (Position.X / 100);

        Texture2D _texture;                         // The texture that holds the images for this sprite
        Color _color = Color.White;              // If set to anything other than Color.White, will colorize the sprite with that color.

        // Dictionary holding all of the FrameAnimation objects
        Dictionary<string, FrameAnimation> _diFrameAnimations = new Dictionary<string, FrameAnimation>();

        string _sCurrAnim = string.Empty;   // Which FrameAnimation from the dictionary above is playing

        // Calculated center of the sprite
        public Vector2 Center => new Vector2(Position.X + Width / 2, Position.Y + Height / 2);
        public int Width { get; private set; } = -1;
        public int Height { get; private set; } = -1;

        int _iScale = 1;
        public int FrameCutoff { get; set; }

        /// Vector2 representing the position of the sprite's upper left
        /// corner pixel.
        public Vector2 Position { get; set; } = Vector2.Zero;

        //When false, this
        public bool Drawing { get; set; } = true;

        public bool Show = true;

        float _fLayerDepthMod;

        public void SetColor(Color c)
        {
            _color = c;
        }

        public void SetDepthMod(float val)
        {
            _fLayerDepthMod = val;
        }

        float _fRotationAngle = 0;
        Vector2 _vRotationOrigin = Vector2.Zero;

        public bool PlayedOnce => CurrentFrameAnimation.PlayCount > 0;

        /// The FrameAnimation object of the currently playing animation
        public FrameAnimation CurrentFrameAnimation
        {
            get
            {
                if (_sCurrAnim != string.Empty && _diFrameAnimations.ContainsKey(_sCurrAnim))
                    return _diFrameAnimations[_sCurrAnim];
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
            get { return _sCurrAnim; }
        }

        #endregion

        public AnimatedSprite(string Texture, bool combatSprite = false)
        {
            _texture = DataManager.GetTexture(Texture);
        }

        public AnimatedSprite(AnimatedSprite sprite)
        {
            _texture = sprite._texture;
            FrameCutoff = sprite.FrameCutoff;
            _color = sprite._color;
            _sCurrAnim = sprite._sCurrAnim;
            _iScale = sprite._iScale;

            Drawing = sprite.Drawing;

            _diFrameAnimations = new Dictionary<string, FrameAnimation>();
            foreach (KeyValuePair<string, FrameAnimation> kvp in sprite._diFrameAnimations){
                _diFrameAnimations[kvp.Key] = new FrameAnimation(kvp.Value);
            }

            Width = sprite.Width;
            Height = sprite.Height;
        }

        public void AddAnimation(AnimationEnum verb, int startX, int startY, int Width, int Height, int Frames = 1, float FrameLength = 1f, bool pingPong = false, bool playsOnce = false) { AddAnimation(Util.GetEnumString(verb), startX, startY, Width, Height, Frames, FrameLength, pingPong, playsOnce); }
        public void AddAnimation(VerbEnum verb, DirectionEnum dir, int startX, int startY, int Width, int Height, int Frames = 1, float FrameLength = 1f, bool pingPong = false, bool playsOnce = false) { AddAnimation(Util.GetActorString(verb, dir), startX, startY, Width, Height, Frames, FrameLength, pingPong, playsOnce); }
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

        public void RemoveAnimation(AnimationEnum verb) { RemoveAnimation(Util.GetEnumString(verb)); }
        public void RemoveAnimation(VerbEnum verb, DirectionEnum dir) { RemoveAnimation(Util.GetActorString(verb, dir)); }
        private void RemoveAnimation(string animationName) { _diFrameAnimations.Remove(animationName); }

        public void PlayAnimation(AnimationEnum verb) { PlayAnimation(Util.GetEnumString(verb)); }
        public void PlayAnimation(VerbEnum verb, DirectionEnum dir) { PlayAnimation(Util.GetActorString(verb, dir)); }
        public void PlayAnimation(string verb, DirectionEnum dir) { PlayAnimation(Util.GetActorString(verb, dir)); }
        public void PlayAnimation(string animate)
        {
            if (_diFrameAnimations.ContainsKey(animate))
            {
                Drawing = true;
                _sCurrAnim = animate;
                Reset();   
            }
        }

        public bool IsCurrentAnimation(AnimationEnum verb) { return IsCurrentAnimation(Util.GetEnumString(verb)); }
        public bool IsCurrentAnimation(VerbEnum verb, DirectionEnum dir) { return IsCurrentAnimation(Util.GetActorString(verb, dir)); }
        public bool IsCurrentAnimation(string verb, DirectionEnum dir) { return IsCurrentAnimation(Util.GetActorString(verb, dir)); }
        private bool IsCurrentAnimation(string animate) { return CurrentAnimation.Equals(animate); }

        /// <summary>
        /// After ensuring that we have the frame we think we're on call Reset on it
        /// so that all of the timer data and counting data has been reset to 0.
        /// </summary>
        public void Reset()
        {
            if (!string.IsNullOrEmpty(_sCurrAnim))
            {
                _diFrameAnimations[_sCurrAnim].FullReset();
            }
        }

        public void SetNextAnimation(string first, string next)
        {
            _diFrameAnimations[first].SetNextAnimation(next);
        }

        public void SetScale(int x)
        {
            Width = Width / _iScale;
            Height = Height / _iScale;

            Width = Width * x;
            Height = Height * x;

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

        public void MoveBy(float x, float y)
        {
            Position = new Vector2(Position.X + x, Position.Y + y);
        }

        public void Update(GameTime gTime)
        {
            // Don't do anything if the sprite is not animating
            if (Drawing)
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
                    if(CurrentFrameAnimation.PlayCount == 1 && CurrentFrameAnimation.PlayOnce)
                    {
                        Drawing  = false;
                    }
                    if(!String.IsNullOrEmpty(CurrentFrameAnimation.NextAnimation))
                    {
                        PlayAnimation(CurrentFrameAnimation.NextAnimation);
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, bool useLayerDepth = true, float visibility = 1.0f, float forcedLayerDepth = -1)
        {
            if (Drawing)
            {
                if (useLayerDepth)
                {
                    float layerDepth = forcedLayerDepth < 0 ? LayerDepth : forcedLayerDepth;
                    Draw(spriteBatch, layerDepth, visibility);
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

                    spriteBatch.Draw(_texture, new Rectangle((int)this.Position.X, drawAtY, this.Width, this.Height - newFrameCutoff), drawThis, _color * visibility);
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, float layerDepth, float visibility = 1.0f)
        {
            if (Drawing)
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
            spriteBatch.Draw(_texture, rotationalRect, CurrentFrameAnimation.FrameRectangle, _color * visibility, _fRotationAngle, _vRotationOrigin, SpriteEffects.None, layerDepth);
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

        /// <summary>
        /// Call this to confirm whether the Current Animation is the one we're looking for
        /// and whether or not the animation is currently animating.
        /// </summary>
        /// <param name="val">The AnimationVerb to guard for</param>
        /// <returns></returns>
        public bool AnimationFinished(AnimationEnum val)
        {
            bool rv = false;

            if(IsCurrentAnimation(val) && !Drawing && PlayedOnce)
            {
                rv = true;
            }

            return rv;
        }
        public bool AnimationVerbFinished(VerbEnum verb, DirectionEnum dir)
        {
            bool rv = false;

            if (IsCurrentAnimation(Util.GetActorString(verb, dir)) && !Drawing)
            {
                rv = true;
            }

            return rv;
        }

        public bool ContainsAnimation(AnimationEnum val)
        {
            return _diFrameAnimations.ContainsKey(Util.GetEnumString(val));
        }
    }
}
