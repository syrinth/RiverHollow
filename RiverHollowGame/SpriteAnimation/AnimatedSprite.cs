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
        //Combatsprites have buffers that we need to accomodate
        public bool CombatSprite { get; } = false;

        public float LayerDepth => Position.Y + CurrentFrameAnimation.FrameHeight - (CombatSprite ? TileSize : 0) + ((Position.X + (CombatSprite ? TileSize : 0)) / 100);

        Texture2D _texture;                         // The texture that holds the images for this sprite
        bool _bAnimating = true;                     // True if animations are being played
        public bool PlayedOnce = false;
        Color _color = Color.White;              // If set to anything other than Color.White, will colorize the sprite with that color.

        // Screen Position of the Sprite
        private Vector2 _vPosition = Vector2.Zero;

        // Dictionary holding all of the FrameAnimation objects
        Dictionary<string, FrameAnimation> _diFrameAnimations = new Dictionary<string, FrameAnimation>();

        string _sCurrAnim = string.Empty;   // Which FrameAnimation from the dictionary above is playing

        // Calculated center of the sprite
        public Vector2 Center => new Vector2(_vPosition.X + _width / 2, Position.Y + _height / 2);

        // Calculated width and height of the sprite
        int _width;
        public int Width => _width;
        int _height;
        public int Height => _height;

        int _iScale = 1;

        //Used for cutting off the top of hair
        int _iFrameCutoff;          
        public int FrameCutoff
        {
            get { return _iFrameCutoff; }
            set { _iFrameCutoff = value; }
        }

        /// Vector2 representing the position of the sprite's upper left
        /// corner pixel.
        public Vector2 Position
        {
            get { return _vPosition; }
            set {_vPosition = value; }
        }

        public bool IsAnimating
        {
            get { return _bAnimating; }
            set { _bAnimating = value; }
        }

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

        public bool PlaysOnce = false;
        ///
        /// The FrameAnimation object of the currently playing animation
        ///
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
            CombatSprite = combatSprite;
            _texture = DataManager.GetTexture(Texture);
        }

        public AnimatedSprite(AnimatedSprite sprite)
        {
            _texture = sprite._texture;
            CombatSprite = sprite.CombatSprite;
            _diFrameAnimations = sprite._diFrameAnimations;
            _iFrameCutoff = sprite._iFrameCutoff;
            _color = sprite._color;
            _sCurrAnim = sprite._sCurrAnim;
            _iScale = sprite._iScale;

            IsAnimating = sprite.IsAnimating;
            PlayedOnce = sprite.PlayedOnce;

            _width = sprite._width;
            _height = sprite._height;
        }

        public void AddAnimation(AnimationEnum verb, int startX, int startY, int Width, int Height, int Frames = 1, float FrameLength = 1f, bool pingPong = false) { AddAnimation(Util.GetEnumString(verb), startX, startY, Width, Height, Frames, FrameLength, pingPong); }
        public void AddAnimation(VerbEnum verb, DirectionEnum dir, int startX, int startY, int Width, int Height, int Frames = 1, float FrameLength = 1f, bool pingPong = false) { AddAnimation(Util.GetActorString(verb, dir), startX, startY, Width, Height, Frames, FrameLength, pingPong); }
        public void AddAnimation(string animationName, int startX, int startY, int Width, int Height, int Frames = 1, float FrameLength = 1f, bool pingPong = false)
        {
            _diFrameAnimations.Add(animationName, new FrameAnimation(startX, startY, Width, Height, Frames, FrameLength, pingPong));
            _width = Width;
            _height = Height;
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
            if (_sCurrAnim != animate && _diFrameAnimations.ContainsKey(animate))
            {
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
            _width = _width / _iScale;
            _height = _height / _iScale;

            _width = _width * x;
            _height = _height * x;

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
            _vPosition.X += x;
            _vPosition.Y += y; 
        }

        public void Update(GameTime gTime)
        {
            // Don't do anything if the sprite is not animating
            if (_bAnimating)
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
                    if(!String.IsNullOrEmpty(CurrentFrameAnimation.NextAnimation))
                    {
                        PlayAnimation(CurrentFrameAnimation.NextAnimation);
                    }
                    else if (PlaysOnce)
                    {
                        PlayedOnce = true;
                        IsAnimating = false;
                        CurrentFrameAnimation.ResetPlayCount();
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, bool useLayerDepth = true, float visibility = 1.0f, float forcedLayerDepth = -1)
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

        public void Draw(SpriteBatch spriteBatch, float layerDepth, float visibility = 1.0f)
        {
            if (_bAnimating)
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
    }
}
