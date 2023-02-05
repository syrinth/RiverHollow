using System;
using Microsoft.Xna.Framework;

using static RiverHollow.Game_Managers.GameManager;
namespace RiverHollow.SpriteAnimations
{
    public class FrameAnimation :ICloneable
    {
        // The first frame of the Animation.  We will calculate other
        // frames on the fly based on this frame.
        private Rectangle rectInitialFrame;

        // The next animation to play
        public string NextAnimation { get; private set; }
        public float FrameTimer { get; private set; } = 0.0f;

        bool _bPingPong;
        bool _bBackTracking;

        public int FrameCount { get; set; } = 1;

        // Number of frames in the Animation
        public bool PlayOnce { get; set; } = false;

        // Amount of time (in seconds) to display each frame
        public float FrameLength { get; set; } = 0.2f;

        // The frame currently being displayed. 
        public int CurrentFrame { get; private set; } = 0;

        // The number of times this animation has been played
        public int PlayCount { get; private set; } = 0;

        // The rectangle associated with the current animation frame.
        public Rectangle FrameRectangle
        {
            get
            {
                return new Rectangle(
                    rectInitialFrame.X + (rectInitialFrame.Width * CurrentFrame),
                    rectInitialFrame.Y, rectInitialFrame.Width, rectInitialFrame.Height);
            }
        }
        public int FrameWidth
        {
            get { return rectInitialFrame.Width; }
        }
        public int FrameHeight
        {
            get { return rectInitialFrame.Height; }
        }

        public FrameAnimation(Rectangle FirstFrame, int Frames, bool pingPong, bool playsOnce)
        {
            rectInitialFrame = FirstFrame;
            FrameCount = Frames;
            _bPingPong = pingPong;
            PlayOnce = playsOnce;
        }

        public FrameAnimation(int X, int Y, int Width, int Height, int Frames, bool pingPong, bool playsOnce)
        {
            rectInitialFrame = new Rectangle(X, Y, Width, Height);
            FrameCount = Frames;
            _bPingPong = pingPong;
            PlayOnce = playsOnce;
        }

        public FrameAnimation(int X, int Y, int Width, int Height, int Frames, float FrameLength, bool pingPong, bool playsOnce)
        {
            rectInitialFrame = new Rectangle(X, Y, Width, Height);
            FrameCount = Frames;
            this.FrameLength = FrameLength;
            _bPingPong = pingPong;
            PlayOnce = playsOnce;
        }

        public FrameAnimation(FrameAnimation initial)
        {
            rectInitialFrame = initial.rectInitialFrame;
            FrameCount = initial.FrameCount;
            this.FrameLength = initial.FrameLength;
            _bPingPong = initial._bPingPong;
            PlayOnce = initial.PlayOnce;
            NextAnimation = initial.NextAnimation;

            CurrentFrame = initial.CurrentFrame;
            FrameTimer = initial.FrameTimer;
        }

        public void Update(GameTime gTime)
        {
            if (FrameCount > 1)
            {
                FrameTimer += (float)gTime.ElapsedGameTime.TotalSeconds;

                if (FrameTimer > FrameLength)
                {
                    FrameTimer = 0.0f;

                    if (_bPingPong)
                    {
                        if (!_bBackTracking && CurrentFrame + 1 <= FrameCount)
                        {
                            CurrentFrame++;
                        }
                        else if (_bBackTracking && CurrentFrame - 1 >= 0)
                        {
                            CurrentFrame--;
                        }

                        if (CurrentFrame == 0)
                        {
                            _bBackTracking = false;
                            PlayCount = (int)MathHelper.Min(PlayCount + 1, int.MaxValue);
                        }
                        else if (CurrentFrame == (FrameCount - 1))
                        {
                            _bBackTracking = true;
                        }
                    }
                    else
                    {
                        if (CurrentFrame + 1 < FrameCount) { CurrentFrame++; }
                        else
                        {
                            PlayCount = (int)MathHelper.Min(PlayCount + 1, int.MaxValue);
                            CurrentFrame = 0;
                        }
                    }
                }
            }
            else if (FrameCount == 1)
            {
                FrameTimer += (float)gTime.ElapsedGameTime.TotalSeconds;

                if (FrameTimer > FrameLength)
                {
                    FrameTimer = 0.0f;
                    PlayCount = (int)MathHelper.Min(PlayCount + 1, int.MaxValue);
                }
            }
        }

        public void SetNextAnimation(string animation)
        {
            NextAnimation = animation;
        }

        /// <summary>
        /// Resets the variables that track where the FrameAnimation is to
        /// their base state so that it starts at the very beginning.
        /// </summary>
        public void FullReset()
        {
            _bBackTracking = false;
            FrameTimer = 0;
            CurrentFrame = 0;
            ResetPlayCount();
        }

        /// <summary>
        /// Sets the current numberof plays to 0
        /// </summary>
        public void ResetPlayCount()
        {
            PlayCount = 0;
        }

        public void SetFrameStartLocation(Point startPosition)
        {
            rectInitialFrame = new Rectangle(startPosition.X, startPosition.Y, FrameWidth, FrameHeight);
        }

        object ICloneable.Clone()
        {
                return new FrameAnimation(this.rectInitialFrame.X, this.rectInitialFrame.Y,
                                          this.rectInitialFrame.Width, this.rectInitialFrame.Height,
                                          this.FrameCount, this.FrameLength, this._bPingPong, this.PlayOnce);
        }
    }
}
