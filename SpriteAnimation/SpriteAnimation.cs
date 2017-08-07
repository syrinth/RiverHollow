using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Adventure.Tile_Engine;

namespace Adventure.SpriteAnimations
{
    public class SpriteAnimation
    {
        #region properties
        Texture2D _texture;                         // The texture that holds the images for this sprite
        bool _animating = true;                     // True if animations are being played
        Color colorTint = Color.White;              // If set to anything other than Color.White, will colorize the sprite with that color.

        // Screen Position of the Sprite
        Vector2 _Position = new Vector2(0, 0);
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

        int _speed;

        public Vector2 DrawOffset { get; set; }
        public float DrawDepth { get; set; }

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

        public SpriteAnimation(Texture2D Texture)
        {
            _texture = Texture;
            DrawOffset = Vector2.Zero;
            DrawDepth = 0.0f;
            _speed = 10;
        }

        public void LoadContent()
        {
            this.AddAnimation("WalkEast", 0, 48 * 0, 48, 48, 8, 0.1f);
            this.AddAnimation("WalkNorth", 0, 48 * 1, 48, 48, 8, 0.1f);
            this.AddAnimation("WalkNorthEast", 0, 48 * 2, 48, 48, 8, 0.1f);
            this.AddAnimation("WalkNorthWest", 0, 48 * 3, 48, 48, 8, 0.1f);
            this.AddAnimation("WalkSouth", 0, 48 * 4, 48, 48, 8, 0.1f);
            this.AddAnimation("WalkSouthEast", 0, 48 * 5, 48, 48, 8, 0.1f);
            this.AddAnimation("WalkSouthWest", 0, 48 * 6, 48, 48, 8, 0.1f);
            this.AddAnimation("WalkWest", 0, 48 * 7, 48, 48, 8, 0.1f);

            this.AddAnimation("IdleEast", 0, 48 * 0, 48, 48, 1, 0.2f);
            this.AddAnimation("IdleNorth", 0, 48 * 1, 48, 48, 1, 0.2f);
            this.AddAnimation("IdleNorthEast", 0, 48 * 2, 48, 48, 1, 0.2f);
            this.AddAnimation("IdleNorthWest", 0, 48 * 3, 48, 48, 1, 0.2f);
            this.AddAnimation("IdleSouth", 0, 48 * 4, 48, 48, 1, 0.2f);
            this.AddAnimation("IdleSouthEast", 0, 48 * 5, 48, 48, 1, 0.2f);
            this.AddAnimation("IdleSouthWest", 0, 48 * 6, 48, 48, 1, 0.2f);
            this.AddAnimation("IdleWest", 0, 48 * 7, 48, 48, 1, 0.2f);

            this.Position = new Vector2(200, 200);
            this.DrawOffset = new Vector2(-48, -48);
            this.CurrentAnimation = "WalkEast";
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

        public void Update(GameTime gameTime, TileMap gameMap)
        {
            Vector2 moveVector = Vector2.Zero;
            Vector2 moveDir = Vector2.Zero;
            string animation = "";

            KeyboardState ks = Keyboard.GetState();

            if (ks.IsKeyDown(Keys.W))
            {
                moveDir += new Vector2(0, -_speed);
                animation = "WalkNorth";
                moveVector += new Vector2(0, -_speed);
            }
            else if (ks.IsKeyDown(Keys.S))
            {
                moveDir += new Vector2(0, _speed);
                animation = "WalkSouth";
                moveVector += new Vector2(0, _speed);
            }

            if (ks.IsKeyDown(Keys.A))
            {
                moveDir += new Vector2(-_speed, 0);
                animation = "WalkWest";
                moveVector += new Vector2(-_speed, 0);
            }
            else if (ks.IsKeyDown(Keys.D))
            {
                moveDir += new Vector2(_speed, 0);
                animation = "WalkEast";
                moveVector += new Vector2(_speed, 0);
            }

            if (moveDir.Length() != 0)
            {
                Rectangle testRect = new Rectangle((int)_Position.X + (int)moveDir.X, (int)_Position.Y + (int)moveDir.Y, _width, _height);
                bool moveX = true;
                bool moveY = true;

                if (!gameMap.CheckXMovement(testRect) || !gameMap.CheckRightMovement(testRect))
                {
                    moveX = false;  
                }
                if (!gameMap.CheckUpMovement(testRect) || !gameMap.CheckDownMovement(testRect))
                {
                    moveY = false;
                }

                this.MoveBy(moveX ? (int)moveDir.X : 0,  moveY ? (int)moveDir.Y : 0);

                if (this.CurrentAnimation != animation)
                {
                    this.CurrentAnimation = animation;
                }
            }
            else
            {
                this.CurrentAnimation = "Idle" + this.CurrentAnimation.Substring(4);
            }



            this.Position = new Vector2(this.Position.X, this.Position.Y);

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

                // Check to see if there is a "followup" animation named for this animation
                if (!String.IsNullOrEmpty(CurrentFrameAnimation.NextAnimation))
                {
                    // If there is, see if the currently playing animation has
                    // completed a full animation loop
                    if (CurrentFrameAnimation.PlayCount > 0)
                    {
                        // If it has, set up the next animation
                        CurrentAnimation = CurrentFrameAnimation.NextAnimation;
                    }
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch, int XOffset, int YOffset)
        {
            if (_animating) {
                spriteBatch.Draw(_texture, new Rectangle((int)this.Position.X, (int)this.Position.Y, this.Width, this.Height), CurrentFrameAnimation.FrameRectangle, Color.White, 0, new Vector2(0,0), SpriteEffects.None, 0);
            }     
        }
    }
}
